
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Inventory;
using Library.Model.Parties;
using Library.Model.Products;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.MaterialManagement.Inventory;
using Library.Service.Logs;
using Library.Service.Products;
using Library.MaterialManagement.Reports;
using Library.ViewModel.Inventory;
using Library.ViewModel.Materials;
using Library.ViewModel.OrderManagements;
using Microsoft.Reporting.WebForms;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Aplos.Filters;
using Aplos.Helpers;
using Syncfusion.ExcelToPdfConverter;
using Syncfusion.Pdf;
using Library.MaterialManagement.InventoryManagements;

namespace Aplos.Areas.Products.Controllers
{
	public class GatePassRegisterController : Controller
	{
		#region Constructor
		GatePassService gps = new GatePassService();


		public GatePassRegisterController(
			)
		{
		}

		#endregion Constructor

		#region Aplos

		public ActionResult Aplos()
		{
			return View();
		}

		
		#endregion Aplos


		[HttpGet, Authorize]
		public ActionResult getGatePassRegister()
        {
			return Json(gps.getGatePassRegister(), JsonRequestBehavior.AllowGet);
        }

		[Authorize, HttpGet]
		public ActionResult GatePassReportExcel()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				

				ExcelEngine excelEngine = new ExcelEngine();

				IWorkbook workbook = getAllData();

				string strFileName = "Gate Pass Register.xlsx";
				workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
				workbook.Close();
			}
			catch (Exception ex)
			{
				return Json(ex.Message, JsonRequestBehavior.AllowGet);

			}
			return null;
		}

		[HttpGet, Authorize]
		public ActionResult GatePassReportPdf()

		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{

				ExcelEngine excelEngine = new ExcelEngine();

				IWorkbook workbook = getAllData();

				string strFileName = "Gate Pass Register.pdf";
				ExcelToPdfConverter convert = new ExcelToPdfConverter(workbook);
				PdfDocument pdfDoc = convert.Convert();
				workbook.Close();
				pdfDoc.Save(strFileName, System.Web.HttpContext.Current.Response, HttpReadType.Save);

			}
			catch (Exception ex)
			{
				return Json(ex.Message, JsonRequestBehavior.AllowGet);

			}
			return null;
		}


        private IWorkbook getAllData()
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];
            sheet.Name = "Gate Pass Register Report";



            int ROW = 6;
            int endCol = 1;
            int COL = 1;

            DataTable dt = gps.getGatePassRegisterReport();

            #region Grid Headers

            report.SetHeaderText(ref sheet, ROW, COL, "GatePassDetailId", 13, ExcelHAlign.HAlignCenter);
            int ColGPDId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "GatePassMasterId", 13, ExcelHAlign.HAlignCenter);
            int ColGPMId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "CompanyGroup", 13, ExcelHAlign.HAlignCenter);
            int ColCompanyGroup = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Plant", 13, ExcelHAlign.HAlignCenter);
            int ColPlant = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "GatePassType", 13, ExcelHAlign.HAlignCenter);
            int ColGatePassType = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "GatePassStatus", 13, ExcelHAlign.HAlignCenter);
            int ColGatePassStatus = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "GPMReturnableDate", 13, ExcelHAlign.HAlignCenter);
            int ColRetDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "GatePassEntryDate", 13, ExcelHAlign.HAlignCenter);
            int ColEntryDate = COL;
            COL++; 
            report.SetHeaderText(ref sheet, ROW, COL, "Material", 13, ExcelHAlign.HAlignCenter);
            int ColMaterial = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Article", 13, ExcelHAlign.HAlignCenter);
            int ColArt = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "FirstCharacteristics", 13, ExcelHAlign.HAlignCenter);
            int ColFC = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "FirstCharacteristicsValue", 13, ExcelHAlign.HAlignCenter);
            int ColFCV = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SecondCharacteristics", 13, ExcelHAlign.HAlignCenter);
            int ColSC = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SecondCharacteristicsValue", 13, ExcelHAlign.HAlignCenter);
            int ColSCV = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ThirdCharacteristics", 13, ExcelHAlign.HAlignCenter);
            int ColTC = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ThirdCharacteristicsValue", 13, ExcelHAlign.HAlignCenter);
            int ColTCV = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "MaterialDetail", 13, ExcelHAlign.HAlignCenter);
            int ColmaterialDetail = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "TransactionQty", 13, ExcelHAlign.HAlignCenter);
            int ColTrnQty = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "UOM", 13, ExcelHAlign.HAlignCenter);
            int ColUom = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 13, ExcelHAlign.HAlignCenter);
            int ColRem = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "IsReturnable", 13, ExcelHAlign.HAlignCenter);
            int ColRet = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "GPDReturnableDate", 13, ExcelHAlign.HAlignCenter);
            int ColRtDate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "IsMutilated", 13, ExcelHAlign.HAlignCenter);
            int ColMul = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Rate", 13, ExcelHAlign.HAlignCenter);
            int ColRate = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ChallanNo", 13, ExcelHAlign.HAlignCenter);
            int ColChlNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ChallanNoDetailId", 13, ExcelHAlign.HAlignCenter);
            int ColChlDetail = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "PurposeofGatePass", 13, ExcelHAlign.HAlignCenter);
            int ColPGP = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ConsignmentNo", 13, ExcelHAlign.HAlignCenter);
            int ColConsigNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "GPDDriverName", 13, ExcelHAlign.HAlignCenter);
            int ColDriverN = COL;
            COL++;

           

            report.SetHeaderText(ref sheet, ROW, COL, "FromEmployee", 13, ExcelHAlign.HAlignCenter);
            int ColFrEmp = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Through", 13, ExcelHAlign.HAlignCenter);
            int Colthrough = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "CourierName", 13, ExcelHAlign.HAlignCenter);
            int ColCourierName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "RunnerEmployeeId", 13, ExcelHAlign.HAlignCenter);
            int ColRunner = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ToType", 13, ExcelHAlign.HAlignCenter);
            int ColTType = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ToParty", 13, ExcelHAlign.HAlignCenter);
            int ColTParty = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ToBuyer", 13, ExcelHAlign.HAlignCenter);
            int ColTBuyer = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ToPlant", 13, ExcelHAlign.HAlignCenter);
            int ColTPlant = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ToUnit", 13, ExcelHAlign.HAlignCenter);
            int ColTUnit = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ToDivision", 13, ExcelHAlign.HAlignCenter);
            int ColTDiv = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ToDepartment", 13, ExcelHAlign.HAlignCenter);
            int ColTDep = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "DepartmentEmployee", 13, ExcelHAlign.HAlignCenter);
            int ColDepEmp = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "OtherCompanyName", 13, ExcelHAlign.HAlignCenter);
            int ColOtherCmp = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "MobileNo", 13, ExcelHAlign.HAlignCenter);
            int ColMobNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Address", 13, ExcelHAlign.HAlignCenter);
            int ColAdd = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "GPMRemarks", 13, ExcelHAlign.HAlignCenter);
            int ColRemarks = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "CheckedByEmployee", 13, ExcelHAlign.HAlignCenter);
            int ColChkEmp = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "CheckedByStatus", 13, ExcelHAlign.HAlignCenter);
            int ColChkSt = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "CheckedHoldRejectReason", 13, ExcelHAlign.HAlignCenter);
            int ColChkRej = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ApprovedByEmployee", 13, ExcelHAlign.HAlignCenter);
            int ColAppEmp = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ApprovedHoldRejectReason", 13, ExcelHAlign.HAlignCenter);
            int ColAppRej = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SenderSecurityEmployee", 13, ExcelHAlign.HAlignCenter);
            int ColSecEmp = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "SenderSecurityApprovedStatus", 13, ExcelHAlign.HAlignCenter);
            int ColSecSt = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ReceiverSecurityEmployee", 13, ExcelHAlign.HAlignCenter);
            int ColRecEmp = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ReceiverSecurityApprovedStatus", 13, ExcelHAlign.HAlignCenter);
            int ColRecSt = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "VendorBuyerOtherCompanyReceivedStatus", 13, ExcelHAlign.HAlignCenter);
            int ColVendor = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "GPMChallanNo", 13, ExcelHAlign.HAlignCenter);
            int ColChalanNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "TransportAgentMobileNo", 13, ExcelHAlign.HAlignCenter);
            int ColTrpMob = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "TransportAgentName", 13, ExcelHAlign.HAlignCenter);
            int ColTrpName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "VehicleNo", 13, ExcelHAlign.HAlignCenter);
            int ColVehNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "GateOutStatus", 13, ExcelHAlign.HAlignCenter);
            int ColOutSt = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "GateRegisterType", 13, ExcelHAlign.HAlignCenter);
            int ColRegType = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "ReceivedChallanNo", 13, ExcelHAlign.HAlignCenter);
            int ColRecChlNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "InvoiceNo", 13, ExcelHAlign.HAlignCenter);
            int ColInvNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "PurposeofGatePass", 13, ExcelHAlign.HAlignCenter);
            int ColPurGatePass = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "GPMConsignmentNo", 13, ExcelHAlign.HAlignCenter);
            int ColConsignNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "DriverName", 13, ExcelHAlign.HAlignCenter);
            int ColDriverName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "NoOfPackages", 13, ExcelHAlign.HAlignCenter);
            int ColPackages = COL;
            COL++;


            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < dt.Rows.Count; i++)
            {

                sheet[ROW, ColGPDId].Text = dt.Rows[i]["GatePassDetailId"].ToString();
                sheet[ROW, ColGPMId].Text = dt.Rows[i]["GatePassMasterId"].ToString();
                sheet[ROW, ColCompanyGroup].Text = dt.Rows[i]["CompanyGroup"].ToString();
                sheet[ROW, ColPlant].Text = dt.Rows[i]["Plant"].ToString();
                sheet[ROW, ColGatePassType].Text = dt.Rows[i]["GatePassType"].ToString();
                sheet[ROW, ColGatePassStatus].Text = dt.Rows[i]["GatePassStatus"].ToString();
                sheet[ROW, ColRetDate].Text = dt.Rows[i]["GPMReturnableDate"].ToString();
                sheet[ROW, ColEntryDate].Text = dt.Rows[i]["GatePassEntryDate"].ToString();
                sheet[ROW, ColMaterial].Text = dt.Rows[i]["Material"].ToString();
                sheet[ROW, ColArt].Text = dt.Rows[i]["Article"].ToString();
                sheet[ROW, ColFC].Text = dt.Rows[i]["FirstCharacteristics"].ToString();
                sheet[ROW, ColFCV].Text = dt.Rows[i]["FirstCharacteristicsValue"].ToString();
                sheet[ROW, ColSC].Text = dt.Rows[i]["SecondCharacteristics"].ToString();
                sheet[ROW, ColSCV].Text = dt.Rows[i]["SecondCharacteristicsValue"].ToString();
                sheet[ROW, ColTC].Text = dt.Rows[i]["ThirdCharacteristics"].ToString();
                sheet[ROW, ColTCV].Text = dt.Rows[i]["ThirdCharacteristicsValue"].ToString();
                sheet[ROW, ColmaterialDetail].Text = dt.Rows[i]["MaterialDetail"].ToString();



                sheet[ROW, ColTrnQty].Text = dt.Rows[i]["TransactionQty"].ToString();
                sheet[ROW, ColUom].Text = dt.Rows[i]["UOM"].ToString();
                sheet[ROW, ColRem].Text = dt.Rows[i]["Remarks"].ToString();
                sheet[ROW, ColRet].Text = dt.Rows[i]["IsReturnable"].ToString();
                sheet[ROW, ColRtDate].Text = dt.Rows[i]["GPDReturnableDate"].ToString();
                sheet[ROW, ColMul].Text = dt.Rows[i]["IsMutilated"].ToString();
                sheet[ROW, ColRate].Text = dt.Rows[i]["Rate"].ToString();
                sheet[ROW, ColChlNo].Text = dt.Rows[i]["ChallanNo"].ToString();
                sheet[ROW, ColChlDetail].Text = dt.Rows[i]["ChallanNoDetailId"].ToString();
                sheet[ROW, ColPGP].Text = dt.Rows[i]["PurposeofGatePass"].ToString();

                sheet[ROW, ColConsigNo].Text = dt.Rows[i]["ConsignmentNo"].ToString();
                sheet[ROW, ColDriverN].Text = dt.Rows[i]["GPDDriverName"].ToString();
                
                sheet[ROW, ColFrEmp].Text = dt.Rows[i]["FromEmployee"].ToString();
                sheet[ROW, Colthrough].Text = dt.Rows[i]["Through"].ToString();

                sheet[ROW, ColCourierName].Text = dt.Rows[i]["CourierName"].ToString();
                sheet[ROW, ColRunner].Text = dt.Rows[i]["RunnerEmployeeId"].ToString();
                sheet[ROW, ColTType].Text = dt.Rows[i]["ToType"].ToString();
                sheet[ROW, ColTParty].Text = dt.Rows[i]["ToParty"].ToString();
                sheet[ROW, ColTBuyer].Text = dt.Rows[i]["ToBuyer"].ToString();
                sheet[ROW, ColTPlant].Text = dt.Rows[i]["ToPlant"].ToString();
                sheet[ROW, ColTUnit].Text = dt.Rows[i]["ToUnit"].ToString();
                sheet[ROW, ColTDiv].Text = dt.Rows[i]["ToDivision"].ToString();
                sheet[ROW, ColTDep].Text = dt.Rows[i]["ToDepartment"].ToString();
                sheet[ROW, ColDepEmp].Text = dt.Rows[i]["ToDepartment"].ToString();



                sheet[ROW, ColOtherCmp].Text = dt.Rows[i]["OtherCompanyName"].ToString();
                sheet[ROW, ColMobNo].Text = dt.Rows[i]["MobileNo"].ToString();
                sheet[ROW, ColAdd].Text = dt.Rows[i]["Address"].ToString();
                sheet[ROW, ColRemarks].Text = dt.Rows[i]["GPMRemarks"].ToString();
                sheet[ROW, ColChkEmp].Text = dt.Rows[i]["CheckedByEmployee"].ToString();
                sheet[ROW, ColChkSt].Text = dt.Rows[i]["CheckedByStatus"].ToString();
                sheet[ROW, ColChkRej].Text = dt.Rows[i]["CheckedHoldRejectReason"].ToString();
                sheet[ROW, ColAppEmp].Text = dt.Rows[i]["ApprovedByEmployee"].ToString();
                sheet[ROW, ColAppRej].Text = dt.Rows[i]["ApprovedHoldRejectReason"].ToString();
                sheet[ROW, ColSecEmp].Text = dt.Rows[i]["SenderSecurityEmployee"].ToString();



                sheet[ROW, ColSecSt].Text = dt.Rows[i]["SenderSecurityApprovedStatus"].ToString();
                sheet[ROW, ColRecEmp].Text = dt.Rows[i]["ReceiverSecurityEmployee"].ToString();
                sheet[ROW, ColRecSt].Text = dt.Rows[i]["ReceiverSecurityApprovedStatus"].ToString();
                sheet[ROW, ColVendor].Text = dt.Rows[i]["VendorBuyerOtherCompanyReceivedStatus"].ToString();
                sheet[ROW, ColChalanNo].Text = dt.Rows[i]["GPMChallanNo"].ToString();
                sheet[ROW, ColTrpMob].Text = dt.Rows[i]["TransportAgentMobileNo"].ToString();
                sheet[ROW, ColTrpName].Text = dt.Rows[i]["TransportAgentName"].ToString();
                sheet[ROW, ColVehNo].Text = dt.Rows[i]["VehicleNo"].ToString();
                sheet[ROW, ColOutSt].Text = dt.Rows[i]["GateOutStatus"].ToString();
                sheet[ROW, ColRegType].Text = dt.Rows[i]["GateRegisterType"].ToString();

                sheet[ROW, ColRecChlNo].Text = dt.Rows[i]["ReceivedChallanNo"].ToString();
                sheet[ROW, ColInvNo].Text = dt.Rows[i]["InvoiceNo"].ToString();
                sheet[ROW, ColPurGatePass].Text = dt.Rows[i]["PurposeofGatePass"].ToString();
                sheet[ROW, ColConsignNo].Text = dt.Rows[i]["GPMConsignmentNo"].ToString();
                sheet[ROW, ColDriverName].Text = dt.Rows[i]["DriverName"].ToString();
                sheet[ROW, ColPackages].Text = dt.Rows[i]["NoOfPackages"].ToString();






            sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }

            ROW++;
           
            
            endRow = ROW - 1;
            endRow = ROW - 1;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref sheet, endCol, "Gate Pass Register Report", identity.PlantId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }

        
    }

}


                       

