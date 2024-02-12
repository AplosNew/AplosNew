
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
using Syncfusion.XlsIO;
using System;
using System.Web.Mvc;
namespace Aplos.Areas.Products.Controllers
{
	public class OutPassRegisterController : Controller
	{
		#region Constructor
		private readonly ISqlRepository _sqlRepository;

		public OutPassRegisterController(
			IGateEntryService GateentryTokenService, ISqlRepository R)
		{
			_sqlRepository = R;
		}

		public OutPassRegisterController(
			 ISqlRepository sqlRepository)
		{
			_sqlRepository = sqlRepository;
		}

		#endregion Constructor

		#region 

		public ActionResult Report()
		{
			return View();
		}


		#endregion Aplos

		#region AgainstGatePassEntry
		[HttpPost, Authorize]
		public ActionResult GateAgainstGatePassExl()

		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{

				string fileName = "";
				fileName = GateOutExcelView("GatePassOut");
				return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				return Json(ex.Message, JsonRequestBehavior.AllowGet); ;
			}
		}

		public string GateOutExcelView(string SheetName)
		{
			ExcelEngine excelEngine = null;
			IApplication application = null;
			IWorkbook workbook = null;
			IWorksheet sheet = null;
			var report = new ReportUtility();
			var filePath = "";

			try
			{

				excelEngine = new ExcelEngine();
				application = excelEngine.Excel;
				workbook = application.Workbooks.Create(1);
				workbook.Worksheets[0].Name = "Gate Pass Out";
				sheet = workbook.Worksheets[0];
				DataTable data;
				GetAgainstGetePassEntry(out data);
				int ROW = 6; int COL = 1;

				#region Columns

				report.SetHeaderText(ref sheet, ROW, COL, "RGPNo", 9, ExcelHAlign.HAlignRight);
				int ColRGPNo = COL;
				COL++;

				sheet[ROW, COL].Text = "RGPDate";
				sheet[ROW, COL].ColumnWidth = 10;
				int ColRGPDate = COL;
				COL++;

				sheet[ROW, COL].Text = "Expected Return Date";
				sheet[ROW, COL].ColumnWidth = 10;
				sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
				int ColReturnDate = COL;
				COL++;

				sheet[ROW, COL].Text = "Status";
				sheet[ROW, COL].ColumnWidth = 10;
				sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
				int ColStatus = COL;
				COL++;

				sheet[ROW, COL].Text = "Party";
				sheet[ROW, COL].ColumnWidth = 16;
				sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
				int ColParty = COL;
				COL++;

				sheet[ROW, COL].Text = "City";
				sheet[ROW, COL].ColumnWidth = 16;
				sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
				int ColCity = COL;
				COL++;

				sheet[ROW, COL].Text = "Sender";
				sheet[ROW, COL].ColumnWidth = 16;
				sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
				int ColSender = COL;
				COL++;

				sheet[ROW, COL].Text = "Sender Department";
				sheet[ROW, COL].ColumnWidth = 16;
				sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
				int ColSenderDep = COL;
				COL++;

				sheet[ROW, COL].Text = "Material";
				sheet[ROW, COL].ColumnWidth = 16;
				sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
				int ColItemDesc = COL;
				COL++;

				sheet[ROW, COL].Text = "Article";
				sheet[ROW, COL].ColumnWidth = 16;
				sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
				int ColArticle = COL;
				COL++;

				sheet[ROW, COL].Text = "UOM";
				sheet[ROW, COL].ColumnWidth = 8;
				sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
				int ColUOM = COL;
				COL++;

				sheet[ROW, COL].Text = "OutQty";
				sheet[ROW, COL].ColumnWidth = 10;
				sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
				int ColOutQty = COL;
				COL++;

				sheet[ROW, COL].Text = "Rate";
				sheet[ROW, COL].ColumnWidth = 10;
				sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
				int ColRate = COL;
				COL++;

				sheet[ROW, COL].Text = "Amount";
				sheet[ROW, COL].ColumnWidth = 12;
				sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
				int ColAmount = COL;
				COL++;

				sheet[ROW, COL].Text = "ReceivedQty";
				sheet[ROW, COL].ColumnWidth = 10;
				sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
				int ColInQty = COL;
				COL++;

				sheet[ROW, COL].Text = "Balance";
				sheet[ROW, COL].ColumnWidth = 8;
				sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
				int ColBal = COL;
				COL++;

				

				//sheet[ROW, COL].Text = "Challan No.";
				//sheet[ROW, COL].ColumnWidth = 16;
				//sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
				//int ColChallanNo = COL;
				//COL++;

				//sheet[ROW, COL].Text = "Gate Pass Status";
				//sheet[ROW, COL].ColumnWidth = 16;
				//sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
				//int ColGatePassSts = COL;
				//COL++;

				//sheet[ROW, COL].Text = "Gate Pass Type";
				//sheet[ROW, COL].ColumnWidth = 16;
				//sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
				//int ColGatePassType = COL;
				//COL++;

				sheet[ROW, COL].Text = "No Of Packags";
				sheet[ROW, COL].ColumnWidth = 10;
				sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
				int ColNoOfPackags = COL;
				COL++;

				sheet[ROW, COL].Text = "Late By Days";
                sheet[ROW, COL].ColumnWidth = 10;
                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                int ColLateByDate = COL;
                COL++;

                //            sheet[ROW, COL].Text = "NoOfPackages";
                //sheet[ROW, COL].ColumnWidth = 16;
                //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                //int ColNoOfPackages = COL;
                #endregion Columns

                int endCol = COL;
				sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
				sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
				sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
				sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
				sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
				sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

				ROW++;
				int startRow = ROW;
				double[] arr = new double[3];
				for (int i = 0; i < data.Rows.Count; i++)
				{

					sheet[ROW, ColRGPNo].Number = clsStaticInfo.dbl(data.Rows[i]["Id"].ToString());
					sheet[ROW, ColRGPDate].Text = data.Rows[i]["RGPDate"].ToString();
					sheet[ROW, ColParty].Text = data.Rows[i]["Party"].ToString();
					sheet[ROW, ColCity].Text = data.Rows[i]["City"].ToString();
					sheet[ROW, ColItemDesc].Text = data.Rows[i]["ItemDescription"].ToString();
					sheet[ROW, ColUOM].Text = data.Rows[i]["UOM"].ToString();
					sheet[ROW, ColOutQty].Number = clsStaticInfo.dbl(data.Rows[i]["OutQty"].ToString());
					sheet[ROW, ColRate].Number = clsStaticInfo.dbl(data.Rows[i]["Rate"].ToString());
					sheet[ROW, ColAmount].Number = clsStaticInfo.dbl(data.Rows[i]["Amount"].ToString());
					sheet[ROW, ColInQty].Number = clsStaticInfo.dbl(data.Rows[i]["ReceivedQty"].ToString());
					sheet[ROW, ColBal].Number = clsStaticInfo.dbl(data.Rows[i]["balance"].ToString());
					sheet[ROW, ColReturnDate].Text = data.Rows[i]["ReturnableDate"].ToString();
					//sheet[ROW, ColChallanNo].Text = data.Rows[i]["ChallanNo"].ToString();
					//sheet[ROW, ColGatePassSts].Text = data.Rows[i]["GatePassStatus"].ToString();
					//sheet[ROW, ColGatePassType].Text = data.Rows[i]["GatePassType"].ToString();
					sheet[ROW, ColNoOfPackags].Number = clsStaticInfo.dbl(data.Rows[i]["NoOfPackages"].ToString());
					sheet[ROW, ColStatus].Text = data.Rows[i]["Status"].ToString();
					sheet[ROW, ColArticle].Text = data.Rows[i]["Article"].ToString();
					sheet[ROW, ColLateByDate].Number = clsStaticInfo.dbl(data.Rows[i]["LateByDays"].ToString());
					sheet[ROW, ColSender].Text = data.Rows[i]["Sender"].ToString();
					sheet[ROW, ColSenderDep].Text = data.Rows[i]["Department"].ToString();


					ROW++;
				}


				sheet.UsedRange.WrapText = false;
				sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
				sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
				sheet["A" + startRow.ToString()].FreezePanes();

				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				ReportUtility reportUtility = new ReportUtility();
				reportUtility.PlantHeader(ref sheet, endCol, "Gate Passout", identity.PlantId);
				reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
				sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
				sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
				sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
				sheet.UsedRange.WrapText = false;
				sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
				sheet.IsGridLinesVisible = true;
				sheet.PageSetup.TopMargin = 0.2;
				sheet.PageSetup.BottomMargin = 0.8;
				//sheet.PageSetup.PrintTitleRows = "$1:$6";
				sheet.PageSetup.LeftMargin = 0.2;
				sheet.PageSetup.RightMargin = 0.2;
				sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
				sheet.PageSetup.FitToPagesTall = 0;
				sheet.PageSetup.FitToPagesWide = 1;
				sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
				sheet.PageSetup.CenterHorizontally = true;


				filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
				workbook.SaveAs(filePath);
				workbook.Close();
				excelEngine.Dispose();
				return filePath;

			}
			catch (Exception ex)
			{
				throw ex;
			}

		}

		// Written by Nitesh
		public void GetAgainstGetePassEntry(out DataTable data)
		{
			try
			{
				var sql = @"select gpm.Id,gdp.Id Detail_Row, FORMAT(gpm.GatePassEntryDate,'dd-MMM-yyyy') RGPDate,
P.UserName Party, C.UserName City, MM.UserName ItemDescription,MMA.StandardName Article, UOM.UserName UOM
,ISNULL(gdp.TransactionQty,0) OutQty,isnull(gdp.Rate,0)Rate,Amount=IsNULL(gdp.TransactionQty*gdp.Rate,0)
, Isnull(gpd2.InQty,0) ReceivedQty,balance=isnull(gdp.TransactionQty-isnull(gpd2.InQty,0),0),  am.Address1,gpm.InvoiceNo
,  FORMAT(gpm.ReturnableDate,'dd-MMM-yyyy')ReturnableDate, '' LotDate, gpd2.ChallanNo,gpm.GatePassStatus
,gpm.GatePassType,gpm.NoOfPackages
,[Status]= case when gdp.TransactionQty<gpd2.InQty then 'Received' when gdp.TransactionQty-gpd2.InQty = 0 then 'Received' else 'Pending' end, 
LateByDays = case when isnull(gdp.TransactionQty,0) < isnull(gpd2.InQty,0) then '' when isnull(gdp.TransactionQty-isnull(gpd2.InQty,0),0) = 0 then '' else 
DATEDIFF(Day,FORMAT(gpm.ReturnableDate,'dd-MMM-yyyy'),GETDATE()) end , EI.EmployeeName Sender, D.UserName Department

					from trn.GatePassDetails gdp
					left join MST.MaterialMasterArticle MMA on MMA.Id = gdp.ArticleId
					left join MST.MaterialMaster MM on MM.Id = MMA.MaterialMasterId
					join TRN.GatePassMaster gpm on gpm.Id=gdp.GatePassMasterId
					left join hkp.Party P ON P.Id=gpm.ToPartyCode
					left join MST.AddressMaster am on am.Id=p.AddressMasterId
					left join SCS.City C on C.Id = am.CityId
					left join SCS.UnitOfMeasurement UOM on UOM.Id = gdp.TransactionUoMId
					left join EmployeeInformation EI on EI.SystemId = gpm.FromEmployeeId
					 LEFT JOIN org.Department D ON D.id=EI.DepartmentId
					left join(
								select sum(isnull(cgpd.TransactionQty,0)) InQty,gpmR.ChallanNo,cgpd.ChallanNoDetailId, cgpd.Rate,cgpd.MaterialMasterId,cgpd.ArticleId

								from TRN.GatePassDetails cgpd
								left join TRN.GatePassMaster gpmR ON gpmR.id=cgpd.GatePassMasterId --and gpmR.GatePassType='Return' and gpmR.GatePassStatus='NonReturnable'

								group by gpmR.ChallanNo, cgpd.Rate,cgpd.MaterialMasterId,cgpd.ArticleId,cgpd.ChallanNoDetailId
							) gpd2 on gpd2.ChallanNo=gpm.Id and gdp.MaterialMasterId=gpd2.MaterialMasterId and gpd2.ChallanNoDetailId=gdp.Id
				  where gpm.GatePassType='Send' and gpm.GatePassStatus='Returnable' 

				 
";

				data = _sqlRepository.GetDataTable(sql);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
		#endregion AgainstGatePassEntry

	}


}