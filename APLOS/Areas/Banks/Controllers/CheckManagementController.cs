#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.MaterialManagement.Inventory;
using Library.Model.Banks;
using Library.Model.Enums;
using Library.Security.Core;
using Library.Service.Advances;
using Library.Service.Banks;
using Library.Service.Currencies;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Organizations;
using Library.Service.Vouchers;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Banks.Controllers
{
    public class CheckManagementController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly ICheckLotService _checkLotService;
        private readonly ICheckLotNewService _checkLotNewService;
       // private readonly ICheckLotDetailService _checkLotDetailService;

        private readonly AccountVoucherReportService _accountVoucherReportService;
        private readonly ICompanyParallelCurrencyService _companyParallelCurrencyService;
        private readonly IPlantService _plantService;
        // private readonly IRepositoryAsync<CheckLotDetailHistory> _checkLotDetailHistoryRepository;
        private readonly AccountsBankService _accountsBankService;

        public CheckManagementController(
            ICheckLotService checkLotService, ISqlRepository sqlRepository
            //, ICheckLotDetailService checkLotDetailService
            , AccountVoucherReportService accountVoucherReportService
            , AccountsBankService accountsBankService
            , ICompanyParallelCurrencyService companyParallelCurrencyService
            , IPlantService plantService
            , ICheckLotNewService checkLotNewService
            //, IRepositoryAsync<CheckLotDetailHistory> checkLotDetailHistoryRepository
            )
        {
            _checkLotService = checkLotService;
            //_checkLotDetailService = checkLotDetailService;
            _sqlRepository = sqlRepository;

            _accountsBankService = accountsBankService;


            _accountVoucherReportService = accountVoucherReportService;
            _companyParallelCurrencyService = companyParallelCurrencyService;
            _plantService = plantService;
            _checkLotNewService = checkLotNewService;
            //_checkLotDetailHistoryRepository = checkLotDetailHistoryRepository;
        }

        #endregion Constructor

        #region Aplos

     
        public ActionResult CheckLot()
        {
            return View("~/Areas/Banks/Views/CheckLot.cshtml");
        }

        #endregion Aplos

        #region Operation

        [HttpGet, Authorize]
        public JsonResult GetCbo(string bankMasterId)
        {
            return Json(_checkLotNewService.GetCbo(bankMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetDetailCbo(string checkLotId, bool isSequential)
        {
            return Json(_checkLotNewService.GetCbo(checkLotId, isSequential), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult getExistingdetailcbo(string checkLotId, bool isSequential)
        {
            return Json(_checkLotNewService.GetExistingCbo(checkLotId, isSequential), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetDetailCbo1(string checkLotId, bool isSequential)
        {
            return Json(_checkLotNewService.GetCbo1(checkLotId, isSequential), JsonRequestBehavior.AllowGet);
        }




        //[HttpGet, Authorize]
        //public JsonResult GetCbo1()
        //{
        //    return Json(_checkLotNewService.GetCbo1, JsonRequestBehavior.AllowGet);
        //}


        [HttpGet, Authorize]
        public JsonResult GetLotNumber()
        {
            return Json(_checkLotService.GetPK(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetChequeLotList(GridParameter parameters, string bankMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_checkLotService.GetChequeLotList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, bankMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetChequeLotDetailList(string chequeId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_checkLotService.GetChequeLotDetailList(chequeId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(CheckLot checkLot)
        {
            _checkLotService.InsertGraph(checkLot);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(CheckLot checkLot)
        {
            _checkLotNewService.UpdateCheckLot(checkLot);
            return Json(new { Message = AplosMessage.Insert });
        }

        #endregion Operation




        #region Print non cash check
        [HttpGet]
        public ActionResult PrintCheckBeneficiaryPermision()
        {
           return View();
        }

        // [Authorize]
        public ActionResult PrintNonCashCheck()
        {
            return View("~/Areas/Banks/Views/PrintNonCashCheck.cshtml");
        }

        public ActionResult RePrintNonCashCheck()
        {
            return View("~/Areas/Banks/Views/RePrintNonCashCheck.cshtml");
        }

        public ActionResult RePrintCashCheck()
        {
            return View("~/Areas/Banks/Views/RePrintCashCheck.cshtml");
        }


        public ActionResult CheckVoid()
        {
            return View("~/Areas/Banks/Views/CheckVoid.cshtml");
        }

        public ActionResult CheckManagementReport()
        {
            return View("~/Areas/Banks/Views/CheckManagementReport.cshtml");
        }

        

        #region Print cash check
        [HttpPost,Authorize]
        public JsonResult CashCheckPrintReport(string voucherDetailId, int checkLotDetailId, decimal amount, string bankCurrencyId, string checkDate, string checkTamplate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _checkLotNewService.UpdateGraphAndPrintCashCheck(voucherDetailId, checkLotDetailId, amount, checkDate, identity.Name);
            var ru = new ReportUtility();
            var inWord = ru.InWord(Convert.ToDouble(amount), bankCurrencyId);


            PrintCashCheckReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, voucherDetailId, checkDate, checkTamplate);//, strPathHindi, strPathEnglish, strPathBangla);


            return Json(new { InWord = inWord, Message = AplosMessage.Insert });
        }

        [HttpGet, Authorize]
        public ActionResult GetPrintCashCheckReport(string voucherDetailId, string checkDate,string checkTamplate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                PrintCashCheckReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, voucherDetailId, checkDate, checkTamplate);//, strPathHindi, strPathEnglish, strPathBangla);

            }
            catch (Exception ex)
            {

                //throw ex;
            }
            return View();
        }


        private void PrintCashCheckReport(string companyGroupId, string companyId, string plantId, string voucherDetailId, string checkDate, string checkTamplate)
        {
            try
            {
                var reportUtility = new ReportUtility();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string strPath = "";
                var fileName = "";
                //int lang = "+ languageId + @";


                File = "NonCashcheck.docx";
                //string filepath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);
                string filepath = Path.Combine(ResourcesPathReader.GetCheckPath(), File);
                FileInfo DocFile = new FileInfo(filepath);
                if (DocFile.Exists == false)
                {
                    //DocFile = new FileInfo(System.Web.HttpContext.Current.Server.MapPath(".") + "\\Doc1.docx");
                    throw new CustomException("File Not Found");
                }


                DataTable dtEmp = GetPrintCashData(companyGroupId, companyId, plantId, voucherDetailId);

                dtEmp.Rows[0]["D1"] = Convert.ToDateTime(checkDate).ToString("dd").Substring(0, 1);
                dtEmp.Rows[0]["D2"] = Convert.ToDateTime(checkDate).ToString("dd").Substring(1, 1);
                dtEmp.Rows[0]["M1"] = Convert.ToDateTime(checkDate).ToString("MM").Substring(0, 1);
                dtEmp.Rows[0]["M2"] = Convert.ToDateTime(checkDate).ToString("MM").Substring(1, 1);

                dtEmp.Rows[0]["Y1"] = Convert.ToDateTime(checkDate).ToString("yyyy").Substring(0, 1);
                dtEmp.Rows[0]["Y2"] = Convert.ToDateTime(checkDate).ToString("yyyy").Substring(1, 1);
                dtEmp.Rows[0]["Y3"] = Convert.ToDateTime(checkDate).ToString("yyyy").Substring(2, 1);
                dtEmp.Rows[0]["Y4"] = Convert.ToDateTime(checkDate).ToString("yyyy").Substring(3, 1);

                //A opens input document.
                WordDocument document = new WordDocument(DocFile.FullName);

                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                Dictionary<string, int> replaced = new Dictionary<string, int>();


                string value = "";

                for (int i = 0; i < dtEmp.Columns.Count; i++)
                {
                    string PlaceHolder = "{" + dtEmp.Columns[i].ColumnName + "}";
                    document.Replace(PlaceHolder, dtEmp.Rows[0][i].ToString(), false, false);
                }

                document.Replace("{InWord}", reportUtility.InWord(Convert.ToDouble(dtEmp.Rows[0]["InFigure"].ToString()), dtEmp.Rows[0]["CurrencyId"].ToString()), false, true);
                foreach (string item in replaced.Keys)
                {
                    if (replaced[item] == 0)
                        document.Replace(item, "", false, true);
                }


                string fileNames = string.Empty;

                //Region that is for Pdf.Document
                DocToPDFConverter converter = new DocToPDFConverter();
                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();
                //Closes the instance of document objects
                document.Close();
                string Prefix = "CheckTamplate" + checkTamplate;
                //Saves the PDF file
                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);
                document.Close();

                //For word File
                //fileNames = "NonCashCheck.docx";
                //document.Save(fileNames, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                //document.Close();

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public DataTable GetPrintCashData(string companyGroupId, string companyId, string plantId, string voucherDetailId)
        {
            try
            {

                string sql = @" SELECT V.Id ,VD.Id AS VoucherDetailId
                        -- ,VD.GLGeneralInfoId
                         --,V.VoucherNo, dbo.[INSERT_SPACE_BEFORE_CAPITAL_LETTERS](V.SourceType) AS VoucherType
                        -- ,V.VoucherTypeId, REPLACE(CONVERT(CHAR(11), V.VoucherDate, 106),' ','-') AS VoucherDate
                        -- ,B.UserName Bank 
                         --, BM.AccountTitle BankAccountTitle
                         ,p.UserName Party, BM.AccountNumber AccountName                         
                        , CU.Code AS CurrencyCode
                        , VD.CurrencyId
                       -- , Format(V.PostingDate,'dd-MM-yyyy') AS CheckDate
                        , Format(V.PostingDate,'dd-MM-yyyy') AS PostingDate
                        ,'' AS  D1 
                        ,'' AS  D2
                        ,'' AS   M1
                        ,'' AS   M2

                        ,'' AS   Y1
                        ,'' AS   Y2
                        ,'' AS   Y3
                        ,'' AS   Y4
                        --, D1=SUBSTRING(Format(V.PostingDate,'dd-MM-yyyy'),1,1)
                        --, D1=SUBSTRING(Format(V.PostingDate,'dd-MM-yyyy'),1,1)
                        --SELECT SUBSTRING(Id,PATINDEX('%[0-9]%', Id), LEN(Id)) Col, Id from [dbo].[BOMDetail]
                        --, VD.BankMasterId
                        , CONVERT(DECIMAL(18,2),COALESCE((VD.CrAmount),0)) AS Amount
                        , CONVERT(DECIMAL(18,2),COALESCE((VD.CrAmount),0)) AS InFigure
                        
                            FROM [TRN].[GLTransactionDetail] AS SD
                            INNER JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=SD.Id
                            INNER JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
							INNER JOIN SCS.Currency AS CU ON VD.CurrencyId=CU.Id
                            LEFT JOIN MST.BankMaster BM ON BM.Id=VD.BankMasterId
							LEFT JOIN HKP.Bank B ON B.Id=BM.BankId
                            LEFT JOIN TRN.CheckLotDetailHistory CLH ON CLH.VoucherDetailId=VD.Id
							left join  TRN.VoucherDetail vdd  on vdd.VoucherId=v.Id and vdd.id=(
							select top 1 VD.Id from TRN.VoucherDetail VD  where vd.VoucherId=v.Id and isnull(vd.PartyId,'')<>'')
							left join HKP.Party p on p.Id=vdd.PartyId
                            WHERE  VD.BankMasterId IS NOT NULL
                            AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"' AND V.PostingDate<>'' AND V.Archive=0 --AND V.SourceType<>'OpeningBalance'
                             AND CLH.VoucherDetailId <>'' AND VD.CrAmount>0 and VD.Id='" + voucherDetailId + @"'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
     #endregion print cash check

        [HttpPost, Authorize]
        public JsonResult NonCashCheckPrintReport(string voucherDetailId, int checkLotDetailId, decimal amount, string bankCurrencyId, string checkDate, string checkTamplate,string party, string partyBankId, string partyAccount)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _checkLotNewService.UpdateGraphAndPrint(voucherDetailId, checkLotDetailId, amount, checkDate, identity.Name,party,partyBankId,partyAccount);
            var ru = new ReportUtility();
            var inWord = ru.InWord(Convert.ToDouble(amount), bankCurrencyId);

           
            //PrintNonCashCheckReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, voucherDetailId, checkDate, checkTamplate,party);


            return Json(new { InWord = inWord, Message = AplosMessage.Insert });
        }

        #region Printing of check

        [HttpPost, Authorize]
        public JsonResult CheckVoidPrintReport(string voucherDetailId, int checkLotDetailId, decimal amount, string bankCurrencyId, string checkDate, string checkTamplate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _checkLotNewService.UpdateGraphAndCheckVoidPrint(voucherDetailId, checkLotDetailId, amount, checkDate, identity.Name);
            var ru = new ReportUtility();
            var inWord = ru.InWord(Convert.ToDouble(amount), bankCurrencyId);


            //PrintNonCashCheckReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, voucherDetailId, checkDate, checkTamplate);


            return Json(new { InWord = inWord, Message = AplosMessage.Insert });
        }
        

        private object Find(int checkLotDetailId)
        {
            throw new NotImplementedException();
        }

        private void UpdateGraph(object checkLotDetail)
        {
            throw new NotImplementedException();
        }

        #endregion Printing of check

        #region Printing of check cash

        
        #endregion Printing of check cash

        [HttpGet, Authorize]
        public ActionResult GetPrintNonCashCheckReport(string voucherDetailId, string checkDate, string checkTamplate, string party,string toPay)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                PrintNonCashCheckReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, voucherDetailId, checkDate, checkTamplate, party, toPay);//, strPathHindi, strPathEnglish, strPathBangla);

            }
            catch (Exception ex)
            {

                //throw ex;
            }
            return View();
        }


        private void PrintNonCashCheckReport(string companyGroupId, string companyId, string plantId, string voucherDetailId, string checkDate,string checkTamplate,string party,string toPay)
        {
            try
            {
                var reportUtility = new ReportUtility();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string strPath = "";
                var fileName = "";
                //int lang = "+ languageId + @";


                File = checkTamplate + ".docx";
                string filepath = Path.Combine(ResourcesPathReader.GetCheckPath(), File);
                FileInfo DocFile = new FileInfo(filepath);
                if (DocFile.Exists == false)
                {
                    //DocFile = new FileInfo(System.Web.HttpContext.Current.Server.MapPath(".") + "\\Doc1.docx");
                    throw new CustomException("File Not Found");
                }

     
                DataTable dtEmp = GetPrintNonCashData(companyGroupId, companyId, plantId, voucherDetailId);

                dtEmp.Rows[0]["D1"] = Convert.ToDateTime(checkDate).ToString("dd").Substring(0, 1);
                dtEmp.Rows[0]["D2"] = Convert.ToDateTime(checkDate).ToString("dd").Substring(1, 1);
                dtEmp.Rows[0]["M1"] = Convert.ToDateTime(checkDate).ToString("MM").Substring(0, 1);
                dtEmp.Rows[0]["M2"] = Convert.ToDateTime(checkDate).ToString("MM").Substring(1, 1);

                dtEmp.Rows[0]["Y1"] = Convert.ToDateTime(checkDate).ToString("yyyy").Substring(0, 1);
                dtEmp.Rows[0]["Y2"] = Convert.ToDateTime(checkDate).ToString("yyyy").Substring(1, 1);
                dtEmp.Rows[0]["Y3"] = Convert.ToDateTime(checkDate).ToString("yyyy").Substring(2, 1);
                dtEmp.Rows[0]["Y4"] = Convert.ToDateTime(checkDate).ToString("yyyy").Substring(3, 1);

                dtEmp.Rows[0]["Party"] = party;
                dtEmp.Rows[0]["Type"] = toPay.ToString();
                //A opens input document.
                WordDocument document = new WordDocument(DocFile.FullName);

                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                Dictionary<string, int> replaced = new Dictionary<string, int>();

                string value = "";

                for (int i = 0; i < dtEmp.Columns.Count; i++)
                {
                    string PlaceHolder = "{" + dtEmp.Columns[i].ColumnName + "}";
                    document.Replace(PlaceHolder, dtEmp.Rows[0][i].ToString(), false, false);
                }
                //document.Replace("{InFigure}", reportUtility.NumberFormatDecimalLocal(Convert.ToDouble(dtEmp.Rows[0]["InFigure"].ToString()), false, true);
                document.Replace("{InFigure}", Convert.ToDouble(dtEmp.Rows[0]["InFigure"]).ToString("N", CultureInfo.InvariantCulture), false, true);

                document.Replace("{InWord}", reportUtility.InWord(Convert.ToDouble(dtEmp.Rows[0]["InFigure"].ToString()), dtEmp.Rows[0]["CurrencyId"].ToString()), false, true);
                foreach (string item in replaced.Keys)
                {
                    if (replaced[item] == 0)
                        document.Replace(item, "", false, true);
                }


                string fileNames = string.Empty;

                //Region that is for Pdf.Document
                DocToPDFConverter converter = new DocToPDFConverter();
                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();
                //Closes the instance of document objects
                document.Close();
                //string Prefix = "VoucherDetailId" + voucherDetailId;
                string Prefix = "CheckTamplate" + checkTamplate;
                //Saves the PDF file
                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);
                document.Close();

                //For word File
                //fileNames = "NonCashCheck.docx";
                //document.Save(fileNames, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                //document.Close();

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public DataTable GetPrintNonCashData(string companyGroupId, string companyId, string plantId, string voucherDetailId)
        {
            try
            {

                string sql = @" SELECT V.Id ,VD.Id AS VoucherDetailId
                        -- ,VD.GLGeneralInfoId
                         --,V.VoucherNo, dbo.[INSERT_SPACE_BEFORE_CAPITAL_LETTERS](V.SourceType) AS VoucherType
                        -- ,V.VoucherTypeId, REPLACE(CONVERT(CHAR(11), V.VoucherDate, 106),' ','-') AS VoucherDate
                        -- ,B.UserName Bank 
                         --, BM.AccountTitle BankAccountTitle
                         ,p.UserName Party, BM.AccountNumber AccountName                         
                        , CU.Code AS CurrencyCode,'' Type
                        , VD.CurrencyId
                        --, Format(V.PostingDate,'dd-MM-yyyy') AS CheckDate
                        , Format(V.PostingDate,'dd-MM-yyyy') AS PostingDate
                        ,'' AS  D1 
                        ,'' AS  D2
                        ,'' AS   M1
                        ,'' AS   M2

                        ,'' AS   Y1
                        ,'' AS   Y2
                        ,'' AS   Y3
                        ,'' AS   Y4
                        --, D1=SUBSTRING(Format(V.PostingDate,'dd-MM-yyyy'),1,1)
                        --, D1=SUBSTRING(Format(V.PostingDate,'dd-MM-yyyy'),1,1)
                        --SELECT SUBSTRING(Id,PATINDEX('%[0-9]%', Id), LEN(Id)) Col, Id from [dbo].[BOMDetail]
                        --, VD.BankMasterId
                        , CONVERT(DECIMAL(18,2),COALESCE((VD.CrAmount),0)) AS Amount
                        , CONVERT(DECIMAL(18,2),COALESCE((VD.CrAmount),0)) AS InFigure
                        
                            FROM [TRN].[GLTransactionDetail] AS SD
                            INNER JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=SD.Id
                            INNER JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
							INNER JOIN SCS.Currency AS CU ON VD.CurrencyId=CU.Id
                            LEFT JOIN MST.BankMaster BM ON BM.Id=VD.BankMasterId
							LEFT JOIN HKP.Bank B ON B.Id=BM.BankId
                            LEFT JOIN TRN.CheckLotDetailHistory CLH ON CLH.VoucherDetailId=VD.Id
							left join  TRN.VoucherDetail vdd  on vdd.VoucherId=v.Id and vdd.id=(
							select top 1 VD.Id from TRN.VoucherDetail VD  where vd.VoucherId=v.Id and isnull(vd.PartyId,'')<>'')
							left join HKP.Party p on p.Id=vdd.PartyId
                            WHERE  VD.BankMasterId IS NOT NULL
                            AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"' AND V.PostingDate<>'' AND V.Archive=0 --AND V.SourceType<>'OpeningBalance'
                             AND CLH.VoucherDetailId <>'' AND VD.CrAmount>0 and VD.Id='" + voucherDetailId + @"'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        //Check void print
        [HttpGet, Authorize]
        public ActionResult GetPrintCheckVoidReport(string voucherDetailId, string checkDate, string checkTamplate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                PrintCheckVoidReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, voucherDetailId, checkDate, checkTamplate);//, strPathHindi, strPathEnglish, strPathBangla);

            }
            catch (Exception ex)
            {

                //throw ex;
            }
            return View();
        }

        private void PrintCheckVoidReport(string companyGroupId, string companyId, string plantId, string voucherDetailId, string checkDate, string checkTamplate)
        {
            try
            {
                var reportUtility = new ReportUtility();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string strPath = "";
                var fileName = "";
                //int lang = "+ languageId + @";

                File = checkTamplate + ".docx";
                // File = "NonCashcheck.docx";
                string filepath = Path.Combine(ResourcesPathReader.GetCheckPath(), File);
                FileInfo DocFile = new FileInfo(filepath);
                if (DocFile.Exists == false)
                {
                    //DocFile = new FileInfo(System.Web.HttpContext.Current.Server.MapPath(".") + "\\Doc1.docx");
                    throw new CustomException("File Not Found");
                }


                DataTable dtEmp = GetPrintCheckVoidData(companyGroupId, companyId, plantId, voucherDetailId);

                dtEmp.Rows[0]["D1"] = Convert.ToDateTime(checkDate).ToString("dd").Substring(0, 1);
                dtEmp.Rows[0]["D2"] = Convert.ToDateTime(checkDate).ToString("dd").Substring(1, 1);
                dtEmp.Rows[0]["M1"] = Convert.ToDateTime(checkDate).ToString("MM").Substring(0, 1);
                dtEmp.Rows[0]["M2"] = Convert.ToDateTime(checkDate).ToString("MM").Substring(1, 1);

                dtEmp.Rows[0]["Y1"] = Convert.ToDateTime(checkDate).ToString("yyyy").Substring(0, 1);
                dtEmp.Rows[0]["Y2"] = Convert.ToDateTime(checkDate).ToString("yyyy").Substring(1, 1);
                dtEmp.Rows[0]["Y3"] = Convert.ToDateTime(checkDate).ToString("yyyy").Substring(2, 1);
                dtEmp.Rows[0]["Y4"] = Convert.ToDateTime(checkDate).ToString("yyyy").Substring(3, 1);

                //A opens input document.
                WordDocument document = new WordDocument(DocFile.FullName);

                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                Dictionary<string, int> replaced = new Dictionary<string, int>();


                string value = "";

                for (int i = 0; i < dtEmp.Columns.Count; i++)
                {
                    string PlaceHolder = "{" + dtEmp.Columns[i].ColumnName + "}";
                    document.Replace(PlaceHolder, dtEmp.Rows[0][i].ToString(), false, false);
                }

                document.Replace("{InWord}", reportUtility.InWord(Convert.ToDouble(dtEmp.Rows[0]["InFigure"].ToString()), dtEmp.Rows[0]["CurrencyId"].ToString()), false, true);
                foreach (string item in replaced.Keys)
                {
                    if (replaced[item] == 0)
                        document.Replace(item, "", false, true);
                }


                string fileNames = string.Empty;

                //Region that is for Pdf.Document
                DocToPDFConverter converter = new DocToPDFConverter();
                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();
                //Closes the instance of document objects
                document.Close();
                //string Prefix = "VoucherDetailId" + voucherDetailId;
                string Prefix = "CheckTamplate" + checkTamplate;
                //Saves the PDF file
                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);
                document.Close();

                //For word File
                //fileNames = "NonCashCheck.docx";
                //document.Save(fileNames, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                //document.Close();

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public DataTable GetPrintCheckVoidData(string companyGroupId, string companyId, string plantId, string voucherDetailId)
        {
            try
            {

                string sql = @" SELECT V.Id ,VD.Id AS VoucherDetailId
                        -- ,VD.GLGeneralInfoId
                         --,V.VoucherNo, dbo.[INSERT_SPACE_BEFORE_CAPITAL_LETTERS](V.SourceType) AS VoucherType
                        -- ,V.VoucherTypeId, REPLACE(CONVERT(CHAR(11), V.VoucherDate, 106),' ','-') AS VoucherDate
                        -- ,B.UserName Bank 
                         --, BM.AccountTitle BankAccountTitle
                         ,p.UserName Party, BM.AccountNumber AccountName                         
                        , CU.Code AS CurrencyCode
                        , VD.CurrencyId
                        --, Format(V.PostingDate,'dd-MM-yyyy') AS CheckDate
                        , Format(V.PostingDate,'dd-MM-yyyy') AS PostingDate
                        ,'' AS  D1 
                        ,'' AS  D2
                        ,'' AS   M1
                        ,'' AS   M2

                        ,'' AS   Y1
                        ,'' AS   Y2
                        ,'' AS   Y3
                        ,'' AS   Y4
                        --, D1=SUBSTRING(Format(V.PostingDate,'dd-MM-yyyy'),1,1)
                        --, D1=SUBSTRING(Format(V.PostingDate,'dd-MM-yyyy'),1,1)
                        --SELECT SUBSTRING(Id,PATINDEX('%[0-9]%', Id), LEN(Id)) Col, Id from [dbo].[BOMDetail]
                        --, VD.BankMasterId
                        , CONVERT(DECIMAL(18,2),COALESCE((VD.CrAmount),0)) AS Amount
                        , CONVERT(DECIMAL(18,2),COALESCE((VD.CrAmount),0)) AS InFigure
                        
                            FROM [TRN].[GLTransactionDetail] AS SD
                            INNER JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=SD.Id
                            INNER JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
							INNER JOIN SCS.Currency AS CU ON VD.CurrencyId=CU.Id
                            LEFT JOIN MST.BankMaster BM ON BM.Id=VD.BankMasterId
							LEFT JOIN HKP.Bank B ON B.Id=BM.BankId
                            LEFT JOIN TRN.CheckLotDetailHistory CLH ON CLH.VoucherDetailId=VD.Id
							left join  TRN.VoucherDetail vdd  on vdd.VoucherId=v.Id and vdd.id=(
							select top 1 VD.Id from TRN.VoucherDetail VD  where vd.VoucherId=v.Id and isnull(vd.PartyId,'')<>'')
							left join HKP.Party p on p.Id=vdd.PartyId
                            WHERE  VD.BankMasterId IS NOT NULL
                            AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"' AND V.PostingDate<>'' AND V.Archive=0 --AND V.SourceType<>'OpeningBalance'
                             AND CLH.VoucherDetailId <>'' AND VD.CrAmount>0 and VD.Id='" + voucherDetailId + @"'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        #region Re-Print Cash Check update and Report




        [HttpPost, Authorize]
        public JsonResult CashCheckRePrintReport(string voucherDetailId, int checkLotDetailId, decimal amount, string bankCurrencyId, string checkDate,string checkTamplate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _checkLotNewService.UpdateGraphAndCashChequeRePrint(voucherDetailId, checkLotDetailId, amount, checkDate, identity.Name);
            var ru = new ReportUtility();
            var inWord = ru.InWord(Convert.ToDouble(amount), bankCurrencyId);


            RePrintCashCheckReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, voucherDetailId, checkDate, checkTamplate); //, strPathHindi, strPathEnglish, strPathBangla);


            return Json(new { InWord = inWord, Message = AplosMessage.Insert });
        }

        [HttpGet, Authorize]
        public ActionResult GetRePrintCashCheckReport(string voucherDetailId, string checkDate, string checkTamplate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                RePrintCashCheckReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, voucherDetailId, checkDate, checkTamplate);//, strPathHindi, strPathEnglish, strPathBangla);

            }
            catch (Exception ex)
            {

                //throw ex;
            }
            return View();
        }


        private void RePrintCashCheckReport(string companyGroupId, string companyId, string plantId, string voucherDetailId, string checkDate,string checkTamplate)
        {
            try
            {
                var reportUtility = new ReportUtility();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string strPath = "";
                var fileName = "";
                //int lang = "+ languageId + @";

                File = checkTamplate + ".docx";
                //File = "NonCashcheck.docx";
                //string filepath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);
                string filepath = Path.Combine(ResourcesPathReader.GetCheckPath(), File);
                FileInfo DocFile = new FileInfo(filepath);
                if (DocFile.Exists == false)
                {
                    //DocFile = new FileInfo(System.Web.HttpContext.Current.Server.MapPath(".") + "\\Doc1.docx");
                    throw new CustomException("File Not Found");
                }

                DataTable dtEmp = GetRePrintCashData(companyGroupId, companyId, plantId, voucherDetailId);

                dtEmp.Rows[0]["D1"] = Convert.ToDateTime(checkDate).ToString("dd").Substring(0, 1);
                dtEmp.Rows[0]["D2"] = Convert.ToDateTime(checkDate).ToString("dd").Substring(1, 1);
                dtEmp.Rows[0]["M1"] = Convert.ToDateTime(checkDate).ToString("MM").Substring(0, 1);
                dtEmp.Rows[0]["M2"] = Convert.ToDateTime(checkDate).ToString("MM").Substring(1, 1);

                dtEmp.Rows[0]["Y1"] = Convert.ToDateTime(checkDate).ToString("yyyy").Substring(0, 1);
                dtEmp.Rows[0]["Y2"] = Convert.ToDateTime(checkDate).ToString("yyyy").Substring(1, 1);
                dtEmp.Rows[0]["Y3"] = Convert.ToDateTime(checkDate).ToString("yyyy").Substring(2, 1);
                dtEmp.Rows[0]["Y4"] = Convert.ToDateTime(checkDate).ToString("yyyy").Substring(3, 1);
                //dtEmp.Rows[0]["Party"] = party;
                ////A opens input document.
                WordDocument document = new WordDocument(DocFile.FullName);

                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                Dictionary<string, int> replaced = new Dictionary<string, int>();

                string value = "";

                for (int i = 0; i < dtEmp.Columns.Count; i++)
                {
                    string PlaceHolder = "{" + dtEmp.Columns[i].ColumnName + "}";
                    document.Replace(PlaceHolder, dtEmp.Rows[0][i].ToString(), false, false);
                }


                document.Replace("{InFigure}", Convert.ToDouble(dtEmp.Rows[0]["InFigure"]).ToString("#,##0.00"), false, true);

                document.Replace("{InWord}", reportUtility.InWord(Convert.ToDouble(dtEmp.Rows[0]["InFigure"].ToString()), dtEmp.Rows[0]["CurrencyId"].ToString()), false, true);
                foreach (string item in replaced.Keys)
                {
                    if (replaced[item] == 0)
                        document.Replace(item, "", false, true);
                }


                string fileNames = string.Empty;

                //Region that is for Pdf.Document
                DocToPDFConverter converter = new DocToPDFConverter();
                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();
                //Closes the instance of document objects
                document.Close();
                string Prefix = "CheckTamplate" + checkTamplate;
                //Saves the PDF file
                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);
                document.Close();

                //fileNames = "NonCashCheck.docx";
                //document.Save(fileNames, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                //document.Close();

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public DataTable GetRePrintCashData(string companyGroupId, string companyId, string plantId, string voucherDetailId)
        {
            try
            {

                string sql = @"  SELECT V.Id ,VD.Id AS VoucherDetailId
                        -- ,VD.GLGeneralInfoId
                         --,V.VoucherNo, dbo.[INSERT_SPACE_BEFORE_CAPITAL_LETTERS](V.SourceType) AS VoucherType
                        -- ,V.VoucherTypeId, REPLACE(CONVERT(CHAR(11), V.VoucherDate, 106),' ','-') AS VoucherDate
                        -- ,B.UserName Bank 
                         --, BM.AccountTitle BankAccountTitle
                         ,p.UserName Party, BM.AccountNumber AccountName                         
                        , CU.Code AS CurrencyCode
                        , VD.CurrencyId
                       -- , Format(V.PostingDate,'dd-MM-yyyy') AS CheckDate
                        , Format(V.PostingDate,'dd-MM-yyyy') AS PostingDate
                        ,'' AS  D1 
                        ,'' AS  D2
                        ,'' AS   M1
                        ,'' AS   M2

                        ,'' AS   Y1
                        ,'' AS   Y2
                        ,'' AS   Y3
                        ,'' AS   Y4
                        --, D1=SUBSTRING(Format(V.PostingDate,'dd-MM-yyyy'),1,1)
                        --, D1=SUBSTRING(Format(V.PostingDate,'dd-MM-yyyy'),1,1)
                        --SELECT SUBSTRING(Id,PATINDEX('%[0-9]%', Id), LEN(Id)) Col, Id from [dbo].[BOMDetail]
                        --, VD.BankMasterId
                        , CONVERT(DECIMAL(18,2),COALESCE((VD.CrAmount),0)) AS Amount
                        , CONVERT(DECIMAL(18,2),COALESCE((VD.CrAmount),0)) AS InFigure
                        
                            FROM [TRN].[GLTransactionDetail] AS SD
                            INNER JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=SD.Id
                            INNER JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
							INNER JOIN SCS.Currency AS CU ON VD.CurrencyId=CU.Id
                            LEFT JOIN MST.BankMaster BM ON BM.Id=VD.BankMasterId
							LEFT JOIN HKP.Bank B ON B.Id=BM.BankId
                            LEFT JOIN TRN.CheckLotDetailHistory CLH ON CLH.VoucherDetailId=VD.Id
							left join  TRN.VoucherDetail vdd  on vdd.VoucherId=v.Id and vdd.id=(
							select top 1 VD.Id from TRN.VoucherDetail VD  where vd.VoucherId=v.Id and isnull(vd.PartyId,'')<>'')
							left join HKP.Party p on p.Id=vdd.PartyId
                            WHERE  VD.BankMasterId IS NOT NULL
                            AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"' AND V.PostingDate<>'' AND V.Archive=0 --AND V.SourceType<>'OpeningBalance'
                             AND CLH.VoucherDetailId <>'' AND VD.CrAmount>0 and VD.Id='" + voucherDetailId + @"'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion RePrintCashCheckReport


        #region RePrintNonCashCheck Report

        [HttpPost, Authorize]
        public JsonResult NonCashCheckRePrintReport(string voucherDetailId, int checkLotDetailId, decimal amount, string bankCurrencyId, string checkDate, string checkTamplate, string party, string partyBankId, string partyAccount)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _checkLotNewService.UpdateGraphAndRePrint(voucherDetailId, checkLotDetailId, amount, checkDate, identity.Name,party,partyBankId,partyAccount);
            var ru = new ReportUtility();
            var inWord = ru.InWord(Convert.ToDouble(amount), bankCurrencyId);


           // RePrintNonCashCheckReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, voucherDetailId, checkDate, checkTamplate, party); //, strPathHindi, strPathEnglish, strPathBangla);


            return Json(new { InWord = inWord, Message = AplosMessage.Insert });
        }

        [HttpGet, Authorize]
        public ActionResult GetRePrintNonCashCheckReport(string voucherDetailId, string checkDate, string checkTamplate, string party, string toPay)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                RePrintNonCashCheckReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, voucherDetailId, checkDate, checkTamplate, party,toPay);//, strPathHindi, strPathEnglish, strPathBangla);

            }
            catch (Exception ex)
            {

                //throw ex;
            }
            return View();
        }


        private void RePrintNonCashCheckReport(string companyGroupId, string companyId, string plantId, string voucherDetailId, string checkDate, string checkTamplate, string party,string toPay)
        {
            try
            {
                var reportUtility = new ReportUtility();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility oRU = new ReportUtility();
                string File = "";
                string langID = "";
                string strPath = "";
                var fileName = "";
                //int lang = "+ languageId + @";

                 File = checkTamplate + ".docx";
                //File = "NonCashcheck.docx";
                //string filepath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);
                string filepath = Path.Combine(ResourcesPathReader.GetCheckPath(), File);
                FileInfo DocFile = new FileInfo(filepath);
                if (DocFile.Exists == false)
                {
                    //DocFile = new FileInfo(System.Web.HttpContext.Current.Server.MapPath(".") + "\\Doc1.docx");
                    throw new CustomException("File Not Found");
                }

                DataTable dtEmp = GetRePrintNonCashData(companyGroupId, companyId, plantId, voucherDetailId);

                dtEmp.Rows[0]["D1"] = Convert.ToDateTime(checkDate).ToString("dd").Substring(0, 1);
                dtEmp.Rows[0]["D2"] = Convert.ToDateTime(checkDate).ToString("dd").Substring(1, 1);
                dtEmp.Rows[0]["M1"] = Convert.ToDateTime(checkDate).ToString("MM").Substring(0, 1);
                dtEmp.Rows[0]["M2"] = Convert.ToDateTime(checkDate).ToString("MM").Substring(1, 1);

                dtEmp.Rows[0]["Y1"] = Convert.ToDateTime(checkDate).ToString("yyyy").Substring(0, 1);
                dtEmp.Rows[0]["Y2"] = Convert.ToDateTime(checkDate).ToString("yyyy").Substring(1, 1);
                dtEmp.Rows[0]["Y3"] = Convert.ToDateTime(checkDate).ToString("yyyy").Substring(2, 1);
                dtEmp.Rows[0]["Y4"] = Convert.ToDateTime(checkDate).ToString("yyyy").Substring(3, 1);
                dtEmp.Rows[0]["Party"] = party;
                //A opens input document.
                dtEmp.Rows[0]["Type"] = toPay.ToString();
                WordDocument document = new WordDocument(DocFile.FullName);

                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                Dictionary<string, int> replaced = new Dictionary<string, int>();

                string value = "";

                for (int i = 0; i < dtEmp.Columns.Count; i++)
                {
                    string PlaceHolder = "" + dtEmp.Columns[i].ColumnName + "";
                    document.Replace(PlaceHolder, dtEmp.Rows[0][i].ToString(), false, false);
                }


                document.Replace("{InFigure}", Convert.ToDouble(dtEmp.Rows[0]["InFigure"]).ToString("#,##0.00"), false, true);

                document.Replace("InWord", reportUtility.InWord(Convert.ToDouble(dtEmp.Rows[0]["InFigure"].ToString()), dtEmp.Rows[0]["CurrencyId"].ToString()), false, true);
                foreach (string item in replaced.Keys)
                {
                    if (replaced[item] == 0)
                        document.Replace(item, "", false, true);
                }

                document.Replace("{", "", false, false);
                document.Replace("}", "", false, false);
                string fileNames = string.Empty;

                //Region that is for Pdf.Document
                DocToPDFConverter converter = new DocToPDFConverter();
                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();
                //Closes the instance of document objects
                document.Close();
                string Prefix = "CheckTamplate" + checkTamplate;
                //Saves the PDF file
                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);
                document.Close();

                //fileNames = "NonCashCheck.docx";
                //document.Save(fileNames, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                //document.Close();

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public DataTable GetRePrintNonCashData(string companyGroupId, string companyId, string plantId, string voucherDetailId)
        {
            try
            {

                string sql = @"  SELECT V.Id ,VD.Id AS VoucherDetailId
                        -- ,VD.GLGeneralInfoId
                         --,V.VoucherNo, dbo.[INSERT_SPACE_BEFORE_CAPITAL_LETTERS](V.SourceType) AS VoucherType
                        -- ,V.VoucherTypeId, REPLACE(CONVERT(CHAR(11), V.VoucherDate, 106),' ','-') AS VoucherDate
                        -- ,B.UserName Bank 
                         --, BM.AccountTitle BankAccountTitle
                         ,p.UserName Party, BM.AccountNumber AccountName                         
                        , CU.Code AS CurrencyCode,'' Type
                        , VD.CurrencyId
                        --, Format(V.PostingDate,'dd-MM-yyyy') AS CheckDate
                        , Format(V.PostingDate,'dd-MM-yyyy') AS PostingDate
                        ,'' AS  D1 
                        ,'' AS  D2
                        ,'' AS   M1
                        ,'' AS   M2

                        ,'' AS   Y1
                        ,'' AS   Y2
                        ,'' AS   Y3
                        ,'' AS   Y4
                        --, D1=SUBSTRING(Format(V.PostingDate,'dd-MM-yyyy'),1,1)
                        --, D1=SUBSTRING(Format(V.PostingDate,'dd-MM-yyyy'),1,1)
                        --SELECT SUBSTRING(Id,PATINDEX('%[0-9]%', Id), LEN(Id)) Col, Id from [dbo].[BOMDetail]
                        --, VD.BankMasterId
                        , CONVERT(DECIMAL(18,2),COALESCE((VD.CrAmount),0)) AS Amount
                        , CONVERT(DECIMAL(18,2),COALESCE((VD.CrAmount),0)) AS InFigure
                        
                            FROM [TRN].[GLTransactionDetail] AS SD
                            INNER JOIN [TRN].[VoucherDetail] AS VD ON VD.Id=SD.Id
                            INNER JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
							INNER JOIN SCS.Currency AS CU ON VD.CurrencyId=CU.Id
                            LEFT JOIN MST.BankMaster BM ON BM.Id=VD.BankMasterId
							LEFT JOIN HKP.Bank B ON B.Id=BM.BankId
                            LEFT JOIN TRN.CheckLotDetailHistory CLH ON CLH.VoucherDetailId=VD.Id
							left join  TRN.VoucherDetail vdd  on vdd.VoucherId=v.Id and vdd.id=(
							select top 1 VD.Id from TRN.VoucherDetail VD  where vd.VoucherId=v.Id and isnull(vd.PartyId,'')<>'')
							left join HKP.Party p on p.Id=vdd.PartyId
                            WHERE  VD.BankMasterId IS NOT NULL
                            AND V.CompanyGroupId='" + companyGroupId + @"' AND V.CompanyId='" + companyId + @"' AND V.PlantId='" + plantId + @"' AND V.PostingDate<>'' AND V.Archive=0 --AND V.SourceType<>'OpeningBalance'
                             AND CLH.VoucherDetailId <>'' AND VD.CrAmount>0 and VD.Id='" + voucherDetailId + @"'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion RePrintNonCashCheck Report

       
        [HttpGet, Authorize]
        public ActionResult GetPrintNonCashCheckVoucherReport(ReportFormat reportFormat, string voucherId, string voucherDetailId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
           // var workbook = _bankReportService.GetPaymentByBankReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.BankJournal);
            var workbook = _accountVoucherReportService.GetPrintNonCashCheckVoucherReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, voucherDetailId);

            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return View();
            }
        }


        #region Report operation
        [HttpGet, Authorize]
        public ActionResult GetCheckManagementReport(string checkLotId, string lotNumber)   //bool checkbox  GetOperationReportExcel
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {

                ExcelEngine excelEngine = new ExcelEngine();

                //IWorkbook workbook = IssueReportList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, checkbox);
                IWorkbook workbook = CheckManagementReportList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, checkLotId, lotNumber);

                string strFileName = "CheckReport.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (CustomException ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }
            return null;
        }


        //Check Management Report
        private DataTable GetCheckManagementReportData(string companyGroupId, string companyId, string plantId,string checkLotId, string lotNumber)
        {
           var sql = @"select CLH.Id CheckLotHistoryId,format(CLH.PrintDate,'dd-MMM-yyyy') PrintDate,CLH.PrintBy,format( CLH.CheckDate,'dd-MM-yyyy')CheckDate,CLH.AddedBy,format( CLH.AddedDate,'dd-MM-yyyy')AddedDate,CLH.UpdatedBy,format (CLH.UpdatedDate,'dd-MM-yyyy')UpdatedDate
            ,CLH.CheckStatus,CLH.PrintStatus,CL.Id ,CL.BankMasterId,CL.LotNumber,CL.FromNo,CL.ToNo,CL.IsNonSequential,CL.Active
            --,CL.AddedBy ,CL.AddedDate,CL.UpdatedBy,CL.UpdatedDate 
            ,CL.IsClose,CLD.Id CheckLodDetailId,CLD.CheckNumber,CLD.IsPrint,CLD.IsCancel,CLD.ResonForCash
            --,CLD.AddedBy,CLD.AddedDate,CLD.UpdatedBy,CLD.UpdatedDate
            ,CLD.SequenceNumber,V.Id VoucherId,V.VoucherNo,format( v.VoucherDate,'dd-MM-yyyy')VoucherDate,v.PostingDate,v.PostedDate,vd.Id VoucherDetailId,vd.DrAmount, VD.CrAmount
			,B.UserName Bank,B.CheckTemplate,BM.AccountTitle ,P.UserName Party
            from trn.CheckLotDetailHistory CLH
            left join trn.CheckLotDetail CLD ON CLD.Id = CLH.CheckLotDetailId
            left join trn.CheckLot CL ON CL.Id=CLD.CheckLotId
            left join trn.VoucherDetail VD ON VD.Id=CLH.VoucherDetailId
            left join trn.Voucher V on V.Id=VD.VoucherId
			Left join mst.BankMaster BM ON BM.Id=CL.BankMasterId
			left join HKP.Bank B ON B.Id=BM.BankId
		    --left join hkp.Party p on p.Id=VD.PartyId
		        left join TRN.VoucherDetail vdd on vdd.VoucherId=v.Id and vdd.id=(
                select top 1 VD.Id from TRN.VoucherDetail VD where vd.VoucherId=v.Id and isnull(vd.PartyId,'')<>'')
                left join HKP.Party p on p.Id=vdd.PartyId

            left join org.CompanyGroup CG ON CG.Id= V.CompanyGroupId
              WHERE V.CompanyGroupId = '" + companyGroupId + "' AND CL.Id = '" + checkLotId + "'  AND CL.LotNumber = '" + lotNumber + "'   AND V.Archive = 0";

            return _sqlRepository.GetDataTable(sql);
        }


        private IWorkbook CheckManagementReportList(string companyGroupId, string companyId, string plantId, string checkLotId, string lotNumber)  //, bool checkbox
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            DataTable dtIssueReportList = GetCheckManagementReportData(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, checkLotId, lotNumber);

            if (dtIssueReportList.Rows.Count == 0)
                throw new Exception("No data found");

            worksheet.Name = "CheckManagementReport";


            int COL = 1; int ROW = 4;
            int startCol = COL;

            int colH = 1; //int ROW = 4;
            int strCol = colH;
            worksheet[ROW, colH].Text = "Lot Number#";
            worksheet[ROW, colH+1].Text = dtIssueReportList.Rows[0]["LotNumber"].ToString();
            worksheet[ROW, colH].ColumnWidth = 10;
            worksheet[ROW, colH].CellStyle.Font.Bold = true;
            worksheet[ROW, colH].HorizontalAlignment = ExcelHAlign.HAlignRight;
       

            worksheet[ROW, colH+2].Text = "Bank:";
            worksheet[ROW, colH + 4].Text = dtIssueReportList.Rows[0]["Bank"].ToString();
           // int colBank = colBankValue;
            worksheet[ROW, colH].ColumnWidth = 8;
            worksheet[ROW, colH].CellStyle.Font.Bold = true;
            worksheet[ROW, colH+2].HorizontalAlignment = ExcelHAlign.HAlignRight;
            worksheet[ROW, 4, ROW, 5].Merge();
   

            worksheet[ROW, colH+5].Text = "Account:";
            worksheet[ROW, colH + 7].Text = dtIssueReportList.Rows[0]["AccountTitle"].ToString();
           // int colAccountTitle = colAccountValue;
            worksheet[ROW, colH].ColumnWidth = 8;
            worksheet[ROW, colH].CellStyle.Font.Bold = true;
            worksheet[ROW, colH+5].HorizontalAlignment = ExcelHAlign.HAlignRight;
            worksheet[ROW, 7, ROW, 8].Merge();
       

            //rowH++;
            ROW++;


            //int COL = 1; int ROW = 5;
            //int startCol = COL;



            //ROW++;

            //worksheet[ROW, COL].Text = "SL. No";
            //int colSLNO = COL;
            //worksheet[ROW, COL].ColumnWidth = 5;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //COL++;

            //worksheet[ROW, COL].Text = "Sequence";
            //int colSequence = COL;
            //worksheet[ROW, COL].ColumnWidth = 10;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //COL++;

            //worksheet[ROW, COL].Text = "CheckLotId";
            //int colCheckLotId = COL;
            //worksheet[ROW, COL].ColumnWidth = 10;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            //worksheet[ROW, COL].Text = "LotNumber";
            //int colLotNumber = COL;
            //worksheet[ROW, COL].ColumnWidth = 10;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            //worksheet[ROW, COL].Text = "From No";
            //int colFromNo = COL;
            //worksheet[ROW, COL].ColumnWidth = 5;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            //worksheet[ROW, COL].Text = "To No";
            //int colToNo = COL;
            //worksheet[ROW, COL].ColumnWidth = 5;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;




            //worksheet[ROW, COL].Text = "NonSequential";
            //int colIsNonSequential = COL;
            //worksheet[ROW, COL].ColumnWidth = 8;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;


            //worksheet[ROW, COL].Text = "Active";
            //int colActive = COL;
            //worksheet[ROW, COL].ColumnWidth = 5;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            //worksheet[ROW, COL].Text = "BankMasterId";
            //int colBankMasterId = COL;
            //worksheet[ROW, COL].ColumnWidth = 8;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;


            //worksheet[ROW, COL].Text = "IsClose";
            //int colIsClose = COL;
            //worksheet[ROW, COL].ColumnWidth = 6;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;


            //worksheet[ROW, COL].Text = "CheckLodDetailId";
            //int colCheckLodDetailId = COL;
            //worksheet[ROW, COL].ColumnWidth = 10;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;


            worksheet[ROW, COL].Text = "Check Number";
            int colCheckNumber = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;


            worksheet[ROW, COL].Text = "Check Date";
            int colCheckDate = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;


            worksheet[ROW, COL].Text = "Amount";
            int colCrAmount = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;




            worksheet[ROW, COL].Text = "Party";
            int colParty = COL;
            worksheet[ROW, COL].ColumnWidth = 30;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //// worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;

            //worksheet[ROW, COL].Text = "IsPrint";
            //int colIsPrint = COL;
            //worksheet[ROW, COL].ColumnWidth = 8;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;


            //worksheet[ROW, COL].Text = "IsCancel";
            //int colIsCancel = COL;
            //worksheet[ROW, COL].ColumnWidth = 8;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            //worksheet[ROW, COL].Text = "ResonForCash";
            //int colResonForCash = COL;
            //worksheet[ROW, COL].ColumnWidth = 25;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            //worksheet[ROW, COL].Text = "SequenceNumber";
            //int colSequenceNumber = COL;
            //worksheet[ROW, COL].ColumnWidth = 8;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            //worksheet[ROW, COL].Text = "VoucherId";
            //int colVoucherId = COL;
            //worksheet[ROW, COL].ColumnWidth = 7;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //// worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //COL++;

            worksheet[ROW, COL].Text = "Voucher No";
            int colVoucherNo = COL;
            worksheet[ROW, COL].ColumnWidth = 15;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;

            COL++;

            worksheet[ROW, COL].Text = "Voucher Date";
            int colVoucherDate = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
        
            COL++;

            //worksheet[ROW, COL].Text = "VoucherDetailId";
            //int colVoucherDetailId = COL;
            //worksheet[ROW, COL].ColumnWidth = 10;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //COL++;

            //worksheet[ROW, COL].Text = "DrAmount";
            //int colDrAmount = COL;
            //worksheet[ROW, COL].ColumnWidth = 10;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //COL++;



           // worksheet[ROW, COL].Text = "Bank";
           // int colBank = COL;
           // worksheet[ROW, COL].ColumnWidth = 20;
           // worksheet[ROW, COL].CellStyle.Font.Bold = true;
           //// worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
           // COL++;


           // worksheet[ROW, COL].Text = "AccountTitle";
           // int colAccountTitle = COL;
           // worksheet[ROW, COL].ColumnWidth = 25;
           // worksheet[ROW, COL].CellStyle.Font.Bold = true;
           // //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
           // COL++;
            

            //worksheet[ROW, COL].Text = "CheckTemplate";
            //int colCheckTemplate = COL;
            //worksheet[ROW, COL].ColumnWidth = 8;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            //COL++;

            //worksheet[ROW, COL].Text = "CheckLotHistoryId";
            //int colCheckLotHistoryId = COL;
            //worksheet[ROW, COL].ColumnWidth = 10;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;

            worksheet[ROW, COL].Text = "Print Date";
            int colPrintDate = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            COL++;


            worksheet[ROW, COL].Text = "Print By";
            int colPrintBy = COL;
            worksheet[ROW, COL].ColumnWidth = 8;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;




            worksheet[ROW, COL].Text = "Print Status";
            int colPrintStatus = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            COL++;

            worksheet[ROW, COL].Text = "Check Status";
            int colCheckStatus = COL;
            worksheet[ROW, COL].ColumnWidth = 10;
            worksheet[ROW, COL].CellStyle.Font.Bold = true;
            // worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
           // COL++;


       

            //worksheet[ROW, COL].Text = "Added By";
            //int colAddedBy = COL;
            //worksheet[ROW, COL].ColumnWidth = 10;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            ////worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            //COL++;

            //worksheet[ROW, COL].Text = "Added Date";
            //int colAddedDate = COL;
            //worksheet[ROW, COL].ColumnWidth = 10;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;



            //worksheet[ROW, COL].Text = "UpdatedDate";
            //int colUpdatedDate = COL;
            //worksheet[ROW, COL].ColumnWidth = 15;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //COL++;



            //int colTaskDetail = 0;
            //if (checkbox == true)
            //{
            //    COL++;
            //    colTaskDetail = COL;

            //    worksheet[ROW, COL].Text = "Sub Task";
            //    worksheet[ROW, COL].ColumnWidth = 40;
            //    worksheet[ROW, COL].CellStyle.Font.Bold = true;
            //}
            //COL++;

            //worksheet[ROW, COL].Text = "SubTaskStatus";
            //int colSubTaskStatus  = COL;
            //worksheet[ROW, COL].ColumnWidth = 15;
            //worksheet[ROW, COL].CellStyle.Font.Bold = true;
            ////COL++;

            int endCol = COL;
            worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
            worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
            ///worksheet.Range[ROW, 1, ROW, endCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
            worksheet.Range[ROW, 1, ROW, endCol].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Black;
            //worksheet.Range[ROW, startCol, ROW, COL].CellStyle.Font.Color = ExcelKnownColors.White;
            ROW++;

            for (int i = 0; i < dtIssueReportList.Rows.Count; i++)
            {
               // worksheet[ROW, colSLNO].Number = (i + 1);

               // worksheet[ROW, colCheckLotId].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["Id"].ToString());
               // worksheet[ROW, colCheckLotId].NumberFormat = OTSBD.clsStaticInfo.NumberFormat();
               // worksheet[ROW, colLotNumber].Text = dtIssueReportList.Rows[i]["LotNumber"].ToString();
                //worksheet[ROW, colIsClose].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["IsClose"].ToString());
                //worksheet[ROW, colIsClose].Text = dtIssueReportList.Rows[i]["IsClose"].ToString();
                //worksheet[ROW, colIsClose].NumberFormat = OTSBD.clsStaticInfo.NumberFormat();
                //worksheet[ROW, colFromNo].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["FromNo"].ToString());
                //worksheet[ROW, colFromNo].NumberFormat = OTSBD.clsStaticInfo.NumberFormat();
                //worksheet[ROW, colToNo].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["ToNo"].ToString());
               // worksheet[ROW, colToNo].NumberFormat = OTSBD.clsStaticInfo.NumberFormat();
                //worksheet[ROW, colIsNonSequential].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["IsNonSequential"].ToString());
                //worksheet[ROW, colIsNonSequential].Text = dtIssueReportList.Rows[i]["IsNonSequential"].ToString();
               // worksheet[ROW, colBankMasterId].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["BankMasterId"].ToString());
                // worksheet[ROW, colBankMasterId].NumberFormat = OTSBD.clsStaticInfo.NumberFormat();
                //worksheet[ROW, colActive].Text = dtIssueReportList.Rows[i]["Active"].ToString();
                //worksheet[ROW, colActive].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["Active"].ToString());
                // worksheet[ROW, colActive].NumberFormat = OTSBD.clsStaticInfo.NumberFormat();


                //worksheet[ROW, colCheckLodDetailId].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["CheckLodDetailId"].ToString());
                //worksheet[ROW, colCheckLodDetailId].Text = dtIssueReportList.Rows[i]["CheckLodDetailId"].ToString();
                worksheet[ROW, colCheckNumber].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["CheckNumber"].ToString());
               // worksheet[ROW, colBasicProcessTime].NumberFormat = clsStaticInfo.NumberFormat(2);
                //worksheet[ROW, colIsPrint].Text = dtIssueReportList.Rows[i]["IsPrint"].ToString();
                //worksheet[ROW, colIsCancel].Text = dtIssueReportList.Rows[i]["IsCancel"].ToString();
                //worksheet[ROW, colMachineCode].NumberFormat = clsStaticInfo.NumberFormat(2);
               // worksheet[ROW, colResonForCash].Text = dtIssueReportList.Rows[i]["ResonForCash"].ToString();
               // worksheet[ROW, colSequenceNumber].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["SequenceNumber"].ToString());

                //worksheet[ROW, colVoucherId].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["VoucherId"].ToString());
               // worksheet[ROW, colArticleName].Text = dtIssueReportList.Rows[i]["VoucherId"].ToString();
                worksheet[ROW, colVoucherNo].Text = dtIssueReportList.Rows[i]["VoucherNo"].ToString();
                worksheet[ROW, colVoucherDate].Text = dtIssueReportList.Rows[i]["VoucherDate"].ToString();
                //worksheet[ROW, colVoucherDetailId].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["VoucherDetailId"].ToString());
                //worksheet[ROW, colDrAmount].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["DrAmount"].ToString());
                //worksheet[ROW, colDrAmount].NumberFormat =OTSBD.clsStaticInfo.NumberFormat(2);
                worksheet[ROW, colCrAmount].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["CrAmount"].ToString());
                worksheet[ROW, colCrAmount].NumberFormat =OTSBD.clsStaticInfo.NumberFormat(2);
               // worksheet[ROW, colBank].Text = dtIssueReportList.Rows[i]["Bank"].ToString();
               // worksheet[ROW, colAccountTitle].Text = dtIssueReportList.Rows[i]["AccountTitle"].ToString();
                
                //worksheet[ROW, colCheckTemplate].Text = dtIssueReportList.Rows[i]["CheckTemplate"].ToString();

                
                //worksheet[ROW, colCheckLotHistoryId].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["CheckLotHistoryId"].ToString());
                worksheet[ROW, colPrintDate].Text = dtIssueReportList.Rows[i]["PrintDate"].ToString();
                worksheet[ROW, colPrintBy].Text = dtIssueReportList.Rows[i]["PrintBy"].ToString();
                worksheet[ROW, colCheckDate].Text = dtIssueReportList.Rows[i]["CheckDate"].ToString();
               // worksheet[ROW, colAddedBy].Text = dtIssueReportList.Rows[i]["AddedBy"].ToString();
               // worksheet[ROW, colAddedDate].Text = dtIssueReportList.Rows[i]["AddedDate"].ToString();
                //worksheet[ROW, colUpdatedBy].Text = dtIssueReportList.Rows[i]["UpdatedBy"].ToString();
               // worksheet[ROW, colUpdatedDate].Text = dtIssueReportList.Rows[i]["UpdatedDate"].ToString();
                worksheet[ROW, colCheckStatus].Text = dtIssueReportList.Rows[i]["CheckStatus"].ToString();
                worksheet[ROW, colPrintStatus].Text = dtIssueReportList.Rows[i]["PrintStatus"].ToString();
                worksheet[ROW, colParty].Text = dtIssueReportList.Rows[i]["Party"].ToString();



                //worksheet[ROW, colOperationLength].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["OperationLength"].ToString());
                //worksheet[ROW, colOperationLength].NumberFormat = clsStaticInfo.NumberFormat(2);
                //worksheet[ROW, colIsClose].NumberFormat = OTSBD.clsStaticInfo.NumberFormat();
                //worksheet[ROW, colRemarks].Text = dtIssueReportList.Rows[i]["PrintDate"].ToString();


                //if (checkbox == true)
                //{

                //    worksheet[ROW, colTaskDetail].Text = dtIssueReportList.Rows[i]["TaskDetail"].ToString();

                //}

                // worksheet[ROW, colPurchasePrice].NumberFormat = clsStaticInfo.NumberFormat();
                // worksheet[ROW, colScantionAmount].Number = clsStaticInfo.dbl(dtAllLoanRegisterList.Rows[i]["ScantionAmount"].ToString());
                //worksheet[ROW, colFGComponent].Number = clsStaticInfo.dbl(dtIssueReportList.Rows[i]["FGComponent"].ToString());

                worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                //rowH++;
                ROW++;

            }

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.CellStyle.Font.Size = 8f;


            //dtIssueReportList.Rows[i]["LotNumber"].ToString()
            //"Master Order#" + MasterOrderId
            ReportUtility reportUtility = new ReportUtility();

            reportUtility.PlantHeader(ref worksheet, endCol, "Check Management", identity.PlantId);
            reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
            worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            // worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

            worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
            worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
            worksheet.IsGridLinesVisible = false;

            #region Freeze Panes

            worksheet.IsDisplayZeros = false;
            worksheet.UsedRange["A6"].FreezePanes();
            worksheet.FirstVisibleColumn = 1;
            worksheet.FirstVisibleRow = 6;

            #endregion Freeze Panes



            return workbook;
        }
        #endregion 




        [HttpGet, Authorize]
        public ActionResult GetPrintCheckVoidVoucherReport(ReportFormat reportFormat, string voucherId, string voucherDetailId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            // var workbook = _bankReportService.GetPaymentByBankReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, SourceType.BankJournal);
            var workbook = _accountVoucherReportService.GetPrintCheckVoidVoucherReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, voucherDetailId);

            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return View();
            }
        }

        


        [HttpGet, Authorize]
        public ActionResult GetRePrintNonCashCheckVoucherReport(ReportFormat reportFormat, string voucherId, string voucherDetailId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _accountVoucherReportService.GetRePrintNonCashCheckVoucherReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, voucherDetailId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return View();
            }
        }


        [HttpGet, Authorize]
        public ActionResult GetRePrintCashCheckVoucherReport(ReportFormat reportFormat, string voucherId, string voucherDetailId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _accountVoucherReportService.GetRePrintCashCheckVoucherReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, voucherDetailId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return View();
            }
        }


        //Voucher Report for Cash print
        [HttpGet, Authorize]
        public ActionResult GetPrintCashCheckVoucherReport(ReportFormat reportFormat, string voucherId, string voucherDetailId, string bankMasterId,  string cashMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var workbook = _accountVoucherReportService.GetPrintCashCheckVoucherReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId, voucherDetailId, bankMasterId, cashMasterId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return View();
            }
        }

        public Dictionary<string, object> GetDashboardJournalHeader(string companyGroupId, string companyId, string plantId, string voucherId)
        {
            var cmdText = @"SELECT VT.UserName AS VoucherTypeName, V.VoucherNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                            , REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, V.AddedBy, V.PostedBy, UPPER(V.Narration) AS Narration, CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END AS [Status]
                            , V.CurrencyId, C.Code AS CurrencyCode,EB.BeneficiaryType,EB.EmployeeId
							,Beneficiary=CASE WHEN EB.EmployeeId<>'' THEN 'Employee' WHEN EB.PartyId<>'' THEN 'Party' ELSE NULL end
							,BeneficiaryName=CASE WHEN EB.EmployeeId<>'' THEN EI.EmployeeName WHEN EB.PartyId<>'' THEN P.UserName ELSE NULL end
                            FROM  [TRN].[Voucher] AS V 
                            LEFT JOIN [SCS].[VoucherType] AS VT ON VT.Id=V.VoucherTypeId
							LEFT JOIN [SCS].[Currency] AS C ON C.Id=V.CurrencyId
							LEFT JOIN TRN.ExpenseBooking AS EB ON EB.VoucherId=V.Id
							LEFT JOIN [dbo].[EmployeeInformation] AS EI ON EI.SystemId=EB.EmployeeId
							LEFT JOIN [HKP].[Party] AS P ON P.Id=EB.PartyId
                            WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' ";
            return _sqlRepository.GetData(cmdText);
        }

        public DataTable GetAdvanceJournalData(string companyGroupId, string companyId, string plantId, string voucherId)
        {
            var cmdText = @"SELECT V.Id, GL.Id AS AccountCodeId, GL.AccountCode, VDC.VoucherDetailId, FY.FiscalYearName, FYP.PeriodName, FYP.PeriodNo, V.IsPark, REPLACE(CONVERT(VARCHAR(11), V.PostingDate, 106), ' ', '-') AS PostingDate
                , [Park/Post]=CASE WHEN V.IsPark=1 THEN 'Parked' ELSE 'Posted' END, REPLACE(CONVERT(VARCHAR(11), V.DocDate, 106), ' ', '-') AS DocDate, V.DocRefNo, REPLACE(CONVERT(VARCHAR(11), V.VoucherDate, 106), ' ', '-') AS VoucherDate
                , V.VoucherNo, V.CurrencyId, CU1.Code AS TrnCurrency, V.AddedBy, V.PostedBy, VDC.ParallelCurrencyId, CU.Code AS CurrencyCode, VDC.FromCurrencyId, VDC.ToCurrencyId, VDC.ToCurrencyRate
                , VD.DrAmount+VD.CrAmount AS Value,VD.DrAmount,VD.CrAmount, VDC.DrAmount AS CompanyCurrencyDrAmount, VDC.CrAmount AS CompanyCurrencyCrAmount, [DRCR]=CASE WHEN VDC.DrAmount>0 THEN '1' ELSE '2' END, VD.GLGeneralInfoId, GL.UserName AS GL, GL.AccountCode AS GLGeneralInfoCode
                , REPLACE(CONVERT(VARCHAR(11), VD.DocDate, 106), ' ', '-') AS InvoiceDate, VD.DocRefNo AS InvoiceNo, UPPER(VD.Narration) AS DetailNarration, ENT.UserName AS Entity
                , VD.Id AS BudgetMasterId, BUD.UserName AS BudgetName, ACT.UserName AS Activity, UPPER(V.Narration) AS Narration, P.UserName AS PartyName, PP.UserName AS PartyLocation,VD.PartyType, VD.FAType,VD.FixedAssetMasterId
                ,[ParticularName]=CASE
                WHEN EI.EmployeeName<>'' THEN EI.EmployeeCode+'-'+EI.EmployeeName
                WHEN BM.AccountTitle<>'' THEN BM.AccountTitle
                WHEN P.UserName<>'' THEN P.UserName
                WHEN CM.UserName<>'' THEN CM.UserName
                WHEN FAM.UserName<>'' THEN FAM.UserName
                ELSE '' END
                FROM [TRN].[VoucherDetailCurrency] AS VDC
                INNER JOIN [TRN].[VoucherDetail] AS VD ON VD.Id =VDC.VoucherDetailId
                INNER JOIN [TRN].[Voucher] AS V ON V.Id=VD.VoucherId
                LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=VD.GLGeneralInfoId
                LEFT JOIN [SCS].[Currency] AS CU ON CU.Id=VDC.ParallelCurrencyId
                LEFT JOIN [SCS].[Currency] AS CU1 ON CU1.Id=V.CurrencyId
                LEFT JOIN [SCS].[FiscalYear] AS FY ON FY.Id=V.FiscalYearId
                LEFT JOIN [SCS].[FiscalYearPeriod] AS FYP ON FYP.Id=V.FiscalYearPeriodId
                LEFT JOIN [MST].[BudgetMaster] BMT ON VD.BudgetMasterId=BMT.Id
                LEFT JOIN [HKP].[Budget] BUD ON BUD.Id=BMT.BudgetId
                LEFT JOIN [HKP].[Activity] AS ACT ON ACT.Id = VD.ActivityId
                LEFT JOIN [ORG].[Entity] AS ENT ON ENT.Id = VD.EntityId
                LEFT JOIN [HKP].Party AS P ON P.Id=VD.PartyId
                LEFT JOIN [HKP].PartyPlant AS PP ON PP.Id=VD.PartyPlantId
                LEFT JOIN [DBO].EmployeeInformation AS EI ON EI.SystemId=VD.EmployeeId
                LEFT JOIN [MST].BankMaster AS BM ON BM.Id=VD.BankMasterId
                LEFT JOIN [MST].CashMaster AS CM ON CM.Id=VD.CashMasterId
                LEFT JOIN [MST].[FixedAssetMaster] AS FAM ON FAM.Id=VD.FixedAssetMasterId
                WHERE V.Archive=0 AND V.CompanyGroupId='" + companyGroupId + "' AND V.CompanyId='" + companyId + "' AND V.PlantId='" + plantId + "' AND V.Id='" + voucherId + "' ORDER BY VD.DrAmount DESC";
            return _sqlRepository.GetDataTable(cmdText);
        }

        public IWorkbook GetDashboardJournalVoucherReport(out string reportFileName, string companyGroupId, string companyId, string plantId, string plantName, string voucherId)
        {
            var reportUtility = new ReportUtility();
            var excelEngine = new ExcelEngine();
            var workbook = reportUtility.GetWorkbook(ref excelEngine, 1);
            workbook.Version = ExcelVersion.Excel2016;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Voucher";

            var header = GetDashboardJournalHeader(companyGroupId, companyId, plantId, voucherId);

            reportFileName = Convert.ToDateTime(header["PostingDate"]).ToString("yyMMdd") + " " + header["VoucherNo"];

            //var dsLocal = _voucherService.GetAdvanceJournalData(companyGroupId, companyId, plantId, voucherId);
            var dsLocal = GetAdvanceJournalData(companyGroupId, companyId, plantId, voucherId);

            var transcationCurrency = header["CurrencyId"].ToString();
            _companyParallelCurrencyService.GetParallelCurrency(companyId, out string companyCurrencyId, out string companyCurrencyCode);

            var row = 5;

            var colLast = 1;

            int xlsCol = 1;
            int colGl = 0;
            int colParticulars = 0;
            int colinrDebit = 0;
            int colinrCredit = 0;
            int colusdDebit = 0;
            int colusdCradit = 0;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Voucher No");
            reportUtility.SetText(ref sheet, row, 2, header["VoucherNo"].ToString(), ExcelHAlign.HAlignLeft);
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Voucher Date");
            reportUtility.SetText(ref sheet, row, 5, header["VoucherDate"].ToString(), ExcelHAlign.HAlignLeft);

            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Posting Date");
            reportUtility.SetText(ref sheet, row, 2, header["PostingDate"].ToString(), ExcelHAlign.HAlignLeft);
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "DocDate");
            reportUtility.SetText(ref sheet, row, 5, header["DocDate"].ToString(), ExcelHAlign.HAlignLeft);

            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Status");
            reportUtility.SetText(ref sheet, row, 2, header["Status"].ToString(), ExcelHAlign.HAlignLeft);
            reportUtility.SetMasterHeaderText(ref sheet, row, 4, "Doc Ref");
            reportUtility.SetText(ref sheet, row, 5, header["DocRefNo"].ToString(), ExcelHAlign.HAlignLeft);

            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            row++;

            colLast = companyCurrencyId == transcationCurrency ? 5 : 7;
            reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Narration");
            reportUtility.SetText(ref sheet, row, 2, header["Narration"].ToString(), ExcelHAlign.HAlignLeft);
            sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();

            if (header["BeneficiaryType"].ToString() != null)
            {
                reportUtility.SetMasterHeaderText(ref sheet, row, 1, "Beneficiary - (" + header["Beneficiary"].ToString() + ")");
                reportUtility.SetText(ref sheet, row, 2, header["BeneficiaryName"].ToString(), ExcelHAlign.HAlignLeft);
                sheet[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(3) + row].Merge();
            }
            row++;

            if (companyCurrencyId == transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();
                sheet[row, 4, row, 5].BorderAround(ExcelLineStyle.Thin);
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, 4, header["CurrencyCode"].ToString(), ExcelHAlign.HAlignCenter);
                sheet[row, 4, row, 5].Merge();

                reportUtility.SetHeaderText(ref sheet, row, 6, companyCurrencyCode, ExcelHAlign.HAlignCenter);
                sheet[row, 6, row, 7].Merge();
                sheet[row, 6, row, 7].BorderAround(ExcelLineStyle.Thin);
            }

            row++;

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL"); colGl = xlsCol; xlsCol++;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].BorderAround(ExcelLineStyle.Thin); ;
            sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge(); xlsCol++;
            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "GL", 12, ExcelHAlign.HAlignRight);

            reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Particulars", 12); colParticulars = xlsCol; xlsCol++;

            if (companyCurrencyId != transcationCurrency)
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colinrCredit = xlsCol; xlsCol++;

                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 12, ExcelHAlign.HAlignRight); colusdDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 12, ExcelHAlign.HAlignRight); colusdCradit = xlsCol;
                colLast = xlsCol;
            }
            else
            {
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Debit", 13, ExcelHAlign.HAlignRight); colinrDebit = xlsCol; xlsCol++;
                reportUtility.SetHeaderText(ref sheet, row, xlsCol, "Credit", 13, ExcelHAlign.HAlignRight); colinrCredit = xlsCol;
                colLast = xlsCol;
            }

            if (dsLocal.Rows.Count > 0)
            {
                double totalTranAmount = 0;
                double totalBookCurrencyAmount = 0;
                var xRow = row;
                row++;
                for (int i = 0; i < dsLocal.Rows.Count; i++)
                {
                    var glName = dsLocal.Rows[i]["BudgetName"].ToString();


                    reportUtility.SetText(ref sheet, row, colGl, dsLocal.Rows[i]["GLGeneralInfoCode"] + " - " + glName + " - " + dsLocal.Rows[i]["Activity"]);

                    sheet[reportUtility.GetColumnNameForXls(colGl) + row + ":" + reportUtility.GetColumnNameForXls(2) + row].Merge();


                    reportUtility.SetText(ref sheet, row, colParticulars, dsLocal.Rows[i]["ParticularName"].ToString());

                    if (companyCurrencyId != transcationCurrency)
                    {
                        reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colusdDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colusdCradit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                        totalTranAmount += Convert.ToDouble(dsLocal.Rows[i]["DrAmount"].ToString());
                    }
                    else
                    {
                        reportUtility.SetText(ref sheet, row, colinrDebit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString()));
                        reportUtility.SetText(ref sheet, row, colinrCredit, Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyCrAmount"].ToString()));
                    }
                    totalBookCurrencyAmount += Convert.ToDouble(dsLocal.Rows[i]["CompanyCurrencyDrAmount"].ToString());

                    sheet.Range[row, 1, row, colLast].BorderInside(ExcelLineStyle.Hair);
                    sheet.Range[row, 1, row, colLast].BorderAround(ExcelLineStyle.Hair);
                    row++;

                    glName = string.Empty;

                }


                reportUtility.SetText(ref sheet, row, 3, "Total: ", true);
                var lastRow = row - 1;

                if (companyCurrencyId != transcationCurrency)
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (lastRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (lastRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colusdDebit) + (lastRow) + ")";
                    sheet.Range[row, colusdDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colusdCradit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colusdCradit) + xRow + ":" + reportUtility.GetColumnNameForXls(colusdCradit) + (lastRow) + ")";
                    sheet.Range[row, colusdCradit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colusdCradit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colusdCradit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colusdCradit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colusdCradit].BorderAround(ExcelLineStyle.Hair);
                }
                else
                {
                    sheet.Range[row, colinrDebit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrDebit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrDebit) + (lastRow) + ")";
                    sheet.Range[row, colinrDebit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrDebit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrDebit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrDebit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrDebit].BorderAround(ExcelLineStyle.Hair);

                    sheet.Range[row, colinrCredit].Formula = "=SUM(" + reportUtility.GetColumnNameForXls(colinrCredit) + xRow + ":" + reportUtility.GetColumnNameForXls(colinrCredit) + (lastRow) + ")";
                    sheet.Range[row, colinrCredit].NumberFormat = reportUtility.NumberFormatDecimalTwo();
                    sheet.Range[row, colinrCredit].CellStyle.Font.Bold = true;
                    sheet.Range[row, colinrCredit].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet.Range[row, colinrCredit].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet.Range[row, colinrCredit].BorderAround(ExcelLineStyle.Hair);
                }

                row += 2;
                reportUtility.SetText(ref sheet, row, 1, "In Word:", true);

                if (companyCurrencyId != transcationCurrency && _plantService.Find(plantId).IsShowFCInWord)
                {
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalTranAmount, transcationCurrency);
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                    sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;
                    row++;
                }

                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].Text = reportUtility.InWord(totalBookCurrencyAmount, companyCurrencyId);
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row + ":" + reportUtility.GetColumnNameForXls(colLast) + row].Merge();
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[reportUtility.GetColumnNameForXls(2) + row].CellStyle.Font.Bold = true;

                sheet.UsedRange.AutofitColumns();
                sheet[1, 2].ColumnWidth = 40;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                row += 4;
                reportUtility.SetSignatureText(ref sheet, row - 1, 1, header["AddedBy"].ToString());
                sheet.Range[row, 1].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 1, "Prepared By", true);

                reportUtility.SetSignatureText(ref sheet, row - 1, 3, header["PostedBy"].ToString());
                sheet.Range[row, 3].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 3, "Checked By", true);

                sheet.Range[row, 5].Borders[ExcelBordersIndex.EdgeTop].LineStyle = ExcelLineStyle.Thin;
                reportUtility.SetTextMiddle(ref sheet, row, 5, "Authorized By", true);

                reportUtility.CompanyPlantHeader(ref sheet, colLast, "Voucher Report", companyId, plantName, null); //header["VoucherTypeName"].ToString()
                reportUtility.PageSetup(ref sheet, colLast, ExcelPageOrientation.Portrait);
            }
            else
            {
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.CellStyle.Font.Size = 8;
                reportUtility.CompanyPlantHeader(ref sheet, 5, "Voucher Report", companyId, plantName, null);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Portrait);
            }
            return workbook;
        }

        #endregion Print non cash check

        #region Print cash check

       // [Authorize]
        public ActionResult PrintCashCheck()
        {
            return View("~/Areas/Banks/Views/PrintCashCheck.cshtml");
        }



        #endregion Print cash check

        public ActionResult PostDateCheque()
        {
            return View("~/Areas/Banks/Views/PostDateCheque.cshtml");
        }

        [HttpPost]
        public ActionResult CreatePdc(Dictionary<string, object> Pdc)
        {
            try
            {
                SavePdc(Pdc);
                return Json(new { Message = AplosMessage.Insert }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, ex.Message });
            }
        }


        public void SavePdc(Dictionary<string, object> Pdc)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {

                string sql = "SELECT * FROM TRN.PostDepositCheque WHERE ID='" + Pdc["Id"] + "' ";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                DataView DvMaster = new DataView(dsMaster.Tables[0]);
                DateTime eDate = Convert.ToDateTime(Pdc["PostingDate"]);
                double addedDays = Convert.ToDouble(Pdc["RemainderDays"]);
                DateTime EffectiveDate = eDate.AddDays(addedDays);

                if (DvMaster.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();

                        string pdcID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "PostDepositCheque", out pdcID);

                        dr["Id"] = pdcID;

                        dr["BankMasterId"] = Pdc["BankMasterId"];
                        dr["PartyId"] = Pdc["PartyId"];
                        dr["DocRefNo"] = Pdc["DocRefNo"];
                        dr["DocDate"] = Pdc["DocDate"];
                        dr["PostingDate"] = Pdc["PostingDate"];
                        dr["PaymentDate"] = Pdc["PaymentDate"];
                        dr["BaseDate"] = Pdc["BaseDate"];
                        dr["ChequeNo"] = Pdc["ChequeNo"];
                        dr["CurrencyId"] = Pdc["CurrencyId"];
                        dr["Amount"] = Pdc["Amount"];
                        dr["ResponsiblePersonId"] = Pdc["ResponsiblePersonId"];
                        dr["POId"] = Pdc["POId"];
                        dr["RemainderDays"] = Pdc["RemainderDays"];
                        dr["EffectiveDate"] = EffectiveDate;
                        dr["Days"] = Pdc["Days"];
                        dr["Remarks"] = Pdc["Remarks"];

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;
                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = DvMaster[0].Row;
                        dr.BeginEdit();

                        dr["BankMasterId"] = Pdc["BankMasterId"];
                        dr["PartyId"] = Pdc["PartyId"];
                        dr["DocRefNo"] = Pdc["DocRefNo"];
                        dr["DocDate"] = Pdc["DocDate"];
                        dr["PostingDate"] = Pdc["PostingDate"];
                        dr["PaymentDate"] = Pdc["PaymentDate"];
                        dr["BaseDate"] = Pdc["BaseDate"];
                        dr["ChequeNo"] = Pdc["ChequeNo"];
                        dr["CurrencyId"] = Pdc["CurrencyId"];
                        dr["Amount"] = Pdc["Amount"];
                        dr["ResponsiblePersonId"] = Pdc["ResponsiblePersonId"];
                        dr["POId"] = Pdc["POId"];
                        dr["RemainderDays"] = Pdc["RemainderDays"];
                    dr["EffectiveDate"] = EffectiveDate;
                    dr["Days"] = Pdc["Days"];
                        dr["Remarks"] = Pdc["Remarks"];

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();
                    }
                    DvMaster.RowFilter = null;
                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "delete TRN.PostDepositCheque Where Id='" + id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();

                return Json(new { Message = AplosMessage.Deleted });
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select PDC.Id,PDC.BankMasterId,BM.AccountTitle BankName,PDC.PartyId,P.UserName PartyName,PDC.CurrencyId,C.[Name] Currency
							,format(PDC.PaymentDate,'dd-MMM-yyyy') PaymentDate,PDC.Amount,PDC.DocRefNo,PDC.DocDate,PDC.PostingDate
							,PDC.BaseDate,EI.SystemId ResponsiblePersonId,EI.EmployeeName ResponsiblePerson,EI.EmployeeCode ResponsiblePersonCode
							,PDC.RemainderDays,PDC.[Days],PDC.POId,PDC.ChequeNo,PDC.Remarks
                            from TRN.PostDepositCheque PDC
							left join MST.BankMaster BM on BM.Id=PDC.BankMasterId
							left join HKP.Party P on P.Id=PDC.PartyId
							left join SCS.Currency C on C.Id=PDC.CurrencyId
							left join EmployeeInformation EI on EI.SystemId=PDC.ResponsiblePersonId";

            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetPOList(string VendorId)
        {
            return Json(GetListForPOHold(VendorId), JsonRequestBehavior.AllowGet);
        }

        public IEnumerable<object> GetListForPOHold(string VendorId)
        {
            var Sql = "";
            try
            {
                    Sql = @"SELECT IR.Id POId, REPLACE(CONVERT(CHAR(11), IR.PODate, 106), ' ', '-') AS PODate
                                        ,ISNULL(Pr.UserName ,'') CustomerName,ISNULL(CON.ContractNo,'') ContractNo
                                        ,IR.POType, '' LCNo
                                        ,ISNULL(CON.UDNo,'') UDNo 
                                        FROM[TRN].[PurchaseOrder] AS IR 
                                        LEFT JOIN [dbo].[Contract] CON on CON.Id= IR.ContractId
                                        LEFT JOIN [HKP].[Party] Pr ON Pr.Id =CON.CustomerId
                                            WHERE IR.PlantId='20171' AND (IR.POType='PO' OR IR.POType='POByReq' OR IR.POType='POBOQ')
                                        AND IR.IsClosed= 0  
                                            AND IR.CheckedByStatus= 'Checked' AND IR.AuthorizedByStatus= 'Approved'
	                                        AND IR.PartyId='"+ VendorId + @"'";
            
               
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetPostDateChequeReport(string POId)
        {
            try
            {
                AccountsBankService accountsBankService = new AccountsBankService(_sqlRepository);
                string fileName = "";
                fileName = _accountsBankService.PostDateChequeReport(POId, "Post Date Cheque Report");
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        [HttpGet, Authorize]
        public ActionResult DownloadUsingFullPath(string FullPath, string fileName)
        {
            try
            {
                ExcelEngine excelEngine = new ExcelEngine();
                //string fullPath = HostingEnvironment.MapPath("~/") + FileName;
                IWorkbook workbook = excelEngine.Excel.Workbooks.Open(FullPath);
                try
                {
                    System.IO.File.Delete(FullPath);
                }
                catch (Exception)
                {
                }

                workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
                return null;

            }
            catch (Exception ex)
            {


            }
            return null;
        }
    }
}