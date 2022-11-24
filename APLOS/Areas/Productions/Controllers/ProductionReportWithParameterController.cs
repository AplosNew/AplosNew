#region using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Enums;
using Library.Model.Materials;
using Library.Service.Helpers;
using Library.MaterialManagement.Inventory;
using Library.Service.Materials;
using Library.ViewModel.Materials;
using Newtonsoft.Json;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Aplos.MaterialManagement;




#endregion using

namespace Aplos.Areas.Productions.Controllers
{
	public class ProductionReportWithParameterController : BaseController
	{
		#region -- Constructor
		//private readonly IPurchaseOrderService _inventoryReveiveService;
		private readonly IMaterialMasterService _materialMasterService;
		private readonly IInventoryReceiveService _inventoryReceiveService;
		private readonly ISqlRepository _sqlRepository;

		private readonly IMaterialMasterAlternativeUOMService _materialMasterAlternativeUOMService;
		private readonly IMaterialMasterProcessRoutingService _materialMasterProcessRoutingService;
		private readonly IMaterialMasterUsageService _materialMasterUsageService;
		private readonly IMaterialMasterAttributeValueService _materialMasterAttributeValueService;
		private readonly IMaterialAttributeValueService _materialValueService;
		private readonly IMaterialMasterCharacteristicsValueService _materialMasterCharacteristicsValueService;
		private readonly IMaterialMasterProcessSetService _materialMasterProcessService;
		private readonly IMaterialMasterMachineProcessService _assetItemProcessService;
		//private readonly IInventoryReceiveService _inventoryReceiveService;

		public ProductionReportWithParameterController(
			  ISqlRepository sqlRepository,
			  IInventoryReceiveService inventoryReceiveService
			 , IMaterialMasterService materialMasterService
			, IMaterialMasterAlternativeUOMService materialMasterAlternativeUOMService
			, IMaterialMasterProcessRoutingService materialMasterProcessRoutingService
			, IMaterialMasterUsageService materialMasterUsageService
			, IMaterialMasterAttributeValueService materialMasterAttributeValueService
			, IMaterialMasterCharacteristicsValueService materialMasterCharacteristicsValueService
			, IMaterialMasterProcessSetService materialMasterProcessService
			, IMaterialMasterMachineProcessService assetItemProcessService
			, IMaterialAttributeValueService materialValueService

			)
		{

			_sqlRepository = sqlRepository;
			_inventoryReceiveService = inventoryReceiveService;
			_materialMasterService = materialMasterService;
			_materialMasterAlternativeUOMService = materialMasterAlternativeUOMService;
			_materialMasterProcessRoutingService = materialMasterProcessRoutingService;
			_materialMasterUsageService = materialMasterUsageService;
			_materialMasterAttributeValueService = materialMasterAttributeValueService;
			_materialMasterCharacteristicsValueService = materialMasterCharacteristicsValueService;
			_materialMasterProcessService = materialMasterProcessService;
			_assetItemProcessService = assetItemProcessService;
			_materialValueService = materialValueService;


		}

		#endregion -- Constructor

		#region Pages

		public ActionResult Aplos()
		{
			return View();
		}



		[Authorize, HttpPost]
		public JsonResult GetMaterialLedger(string fromDate, string toDate)
		{
			DateTime fDate = DateTime.Parse(fromDate);
			DateTime tDate = DateTime.Parse(toDate);
			if (fromDate == null || fromDate == "")
			{
				throw new CustomException("Select From Date");
			}
			else if (toDate == null || toDate == "")
			{
				throw new CustomException("Select To Date");
			}
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			var jsondata = Json(_inventoryReceiveService.GetMaterialLedger(fromDate, toDate), JsonRequestBehavior.AllowGet);
			jsondata.MaxJsonLength = int.MaxValue;
			return jsondata;
		}

		[HttpPost, Authorize]
		public ActionResult ProductionParameterData(string EntityId, string ProcessId, string ToDate, string FromDate,  string ShiftId)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				List<Dictionary<string, object>> NewData = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(GetProductionParameterData(EntityId, ProcessId,FromDate, ToDate,  ShiftId));
				var jsondata = Json(new { NewData, Message = AplosMessage.Success });
				jsondata.MaxJsonLength = int.MaxValue;
				return jsondata;
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public DataTable GetProductionParameterData(string EntityId, string ProcessId, string FromDate, string ToDate,string ShiftId)
		{
			try
			{
				var str = @"SELECT e2.UserName Entity,P.UserName Process,PSQ.Sequence ProcessSequence,FORMAT(PS.ProductionDate,'dd-MMM-yyyy')ProductionDate
										,CSG.UserName [Shift],WCM.UserName WorkCenterMaster,PS.ProductionOrderId,PS.LotNumber,E.EmployeeName ResponsiblePerson,E.EmployeeName Mentor
										,PS.Remarks,'' MaterialMaster,''Article,''BuyerRefrence,''Productcode,PS.AddedBy,FORMAT(PS.AddedDate,'dd-MMM-yyyy')AddedDate,PS.UpdatedBy,FORMAT(PS.UpdatedDate,'dd-MMM-yyyy')UpdateDate
										FROM [TRN].[ProductionSummary] PS
										LEFT JOIN ORG.Entity AS e2 ON e2.Id = PS.EntityId
										LEFT JOIN HKP.Process P ON P.Id=PS.ProcessId
										LEFT JOIN [dbo].[ProcessAndInventorySequence] PSQ ON PSQ.ProcessId = P.Id
										LEFT JOIN EmployeeInformation E ON E.SystemId=PS.ResponsiblePersonId
										LEFT JOIN EmployeeInformation M ON M.SystemId=PS.MentorId
										LEFT JOIN SCS.WorkCenterMaster WCM ON WCM.Id=PS.WorkCenterMasterId
										LEFT JOIN dbo.ShiftDefination csg ON csg.SystemId=pp.ProductionShiftId
										Where
										PS.EntityId='" + EntityId +@"' and
										
										PS.ProductionDate between '" + FromDate + "' AND '"+ ToDate + "'";
				return _sqlRepository.GetDataTable(str);

			}
			catch (Exception e)
			{
				throw e;
			}
		}


		[HttpPost, Authorize]
		public ActionResult StockRegisterReport(string ToDate, string FromDate, string SlNo)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				string fileName = "";
				fileName = CreateStockRegisterReportSheet(identity.CompanyId, identity.PlantId, FromDate, ToDate, SlNo, "Stock Register Report " + FromDate + " To " + ToDate + "");
				return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

		public string CreateStockRegisterReportSheet(string CompanyId, string PlantId, string FromDate, string ToDate, string SlNo, string SheetName)
		{
			var excelEngine = new ExcelEngine();
			var report = new ReportUtility();
			var workbook = report.GetWorkbook(ref excelEngine, 1);
			workbook.Version = ExcelVersion.Excel2016;

			var data = GetStockRegisterReportData(CompanyId, PlantId, FromDate, ToDate, SlNo, true);

			var sheet = workbook.Worksheets[0];

			#region sheet1
			sheet.Name = "PurchaseRegisterGRNWise";

			int ROW = 7;
			int endCol = 1;
			int COL = 1;

			//sheet.Range[ROW, COL].Text = "From - "+FromDate+" , To - "+ToDate;
			//sheet.Range[ROW, COL].ColumnWidth = 13;
			//sheet.Range[ROW, COL].CellStyle.Font.Size = 12;
			//sheet.Range[ROW, COL].CellStyle.Font.Bold = true;
			//sheet.Range[ROW, COL].VerticalAlignment = ExcelVAlign.VAlignCenter;
			//sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			//ROW += 2;

			#region Grid Headers

			report.SetHeaderText(ref sheet, ROW, COL, "Entity", 25, ExcelHAlign.HAlignLeft);
			int ColEntity = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Process", 18, ExcelHAlign.HAlignLeft);
			int ColProcess = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Process sequence", 18, ExcelHAlign.HAlignLeft);
			int ColProcessSequence = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Date", 13, ExcelHAlign.HAlignLeft);
			int ColDate = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Shift", 15, ExcelHAlign.HAlignLeft);
			int ColShift = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Work Center", 18, ExcelHAlign.HAlignLeft);
			int ColWorkCenter = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Po No.", 10, ExcelHAlign.HAlignLeft);
			int ColPoNo = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Lot No.", 10, ExcelHAlign.HAlignLeft);
			int ColLotNo = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Production Work Station", 12, ExcelHAlign.HAlignLeft);
			int ColProductionWorkStation = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Responsible Person", 12, ExcelHAlign.HAlignLeft);
			int ColResponsiblePerson = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Mentor", 10, ExcelHAlign.HAlignLeft);
			int ColMentor = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Production", 11, ExcelHAlign.HAlignLeft);
			int ColProduction = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Parameter 1", 20, ExcelHAlign.HAlignLeft);
			int ColPeramiter1 = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Parameter 2", 12, ExcelHAlign.HAlignLeft);
			int ColPeramiter2 = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Parameter 3", 13, ExcelHAlign.HAlignLeft);
			int ColPeramiter3 = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Parameter 4", 12, ExcelHAlign.HAlignLeft);
			int ColPeramiter4 = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 13, ExcelHAlign.HAlignRight);
			//sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
			int ColRemarks = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Material Master", 15, ExcelHAlign.HAlignRight);
			int ColMaterialMaster = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Article", 16, ExcelHAlign.HAlignRight);
			int ColArticle = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Buyer Refrence", 13, ExcelHAlign.HAlignRight);
			int ColBuyerRefrence = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Product Code", 13, ExcelHAlign.HAlignRight);
			int ColProductCode = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Add By", 10, ExcelHAlign.HAlignRight);
			int ColAddBy = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Add Date", 13, ExcelHAlign.HAlignRight);
			int ColAddDate = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Updated By", 16, ExcelHAlign.HAlignRight);
			int ColUpdatedBy = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Updated Time", 10, ExcelHAlign.HAlignRight);
			int ColUpdatedTime = COL;
			
			endCol = COL;
			#endregion Headers


			sheet.Range[ROW, 1, ROW, COL].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
			ROW++;
			var startRow = 0;
			var endRow = 0;
			int RowIndex = ROW;
			startRow = ROW;

			for (int i = 0; i < data.Rows.Count; i++)
			{
				sheet[ROW, ColEntity].Text = data.Rows[i]["PartyName"].ToString();
				sheet[ROW, ColProcess].Text = data.Rows[i]["InvoicingPartyPlant"].ToString();
				sheet[ROW, ColProcessSequence].Text = data.Rows[i]["DeliveryPartyPlant"].ToString();
				sheet[ROW, ColDate].Text = data.Rows[i]["PartyCode"].ToString();
				sheet[ROW, ColShift].Text = data.Rows[i]["GSTINNo"].ToString();
				sheet[ROW, ColWorkCenter].Text = data.Rows[i]["Employee"].ToString();
				sheet[ROW, ColPoNo].Text = data.Rows[i]["GRNNo"].ToString();
				sheet[ROW, ColLotNo].Text = data.Rows[i]["GRNEntryDate"].ToString();
				sheet[ROW, ColProductionWorkStation].Text = data.Rows[i]["VoucherNo"].ToString();
				sheet[ROW, ColResponsiblePerson].Text = data.Rows[i]["PostingDate"].ToString();
				sheet[ROW, ColMentor].Text = data.Rows[i]["DocRefNo"].ToString();
				sheet[ROW, ColProduction].Text = data.Rows[i]["DocRefDate"].ToString();
				sheet[ROW, ColPeramiter1].Text = data.Rows[i]["GrnDocDateDifference"].ToString();
				sheet[ROW, ColPeramiter2].Text = data.Rows[i]["GateEntryNo"].ToString();
				sheet[ROW, ColPeramiter3].Text = data.Rows[i]["GateName"].ToString();
				sheet[ROW, ColPeramiter4].Text = data.Rows[i]["CurrencyName"].ToString();
				sheet[ROW, ColRemarks].Text = data.Rows[i]["PartyGroup"].ToString();
				sheet[ROW, ColMaterialMaster].Text = data.Rows[i]["PartyCategory"].ToString();
				sheet[ROW, ColArticle].Text = data.Rows[i]["PartySubCategory"].ToString();
				sheet[ROW, ColBuyerRefrence].Text = data.Rows[i]["PartyType"].ToString();
				sheet[ROW, ColProductCode].Text = data.Rows[i]["PartyAccountGroup"].ToString();
				sheet[ROW, ColAddBy].Text = data.Rows[i]["PartyAccountGroup"].ToString();
				sheet[ROW, ColAddDate].Text = data.Rows[i]["PartyAccountGroup"].ToString();
				sheet[ROW, ColUpdatedBy].Text = data.Rows[i]["PartyAccountGroup"].ToString();
				sheet[ROW, ColUpdatedTime].Text = data.Rows[i]["PartyAccountGroup"].ToString();


				sheet.Range[ROW, ColEntity, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
				sheet.Range[ROW, ColEntity, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

				ROW++;
			}

			//ROW++;

			//if (FromDate != "" && ToDate != "")
			//{


			//	report.SetText(ref sheet, ROW, Convert.ToInt32(ColMaterialTranAmount) - 1, "Total");
			//	sheet.Range[ROW, Convert.ToInt32(ColMaterialTranAmount) - 1].CellStyle.Font.Bold = true;
			//	//sheet.Range[1, ROW, Convert.ToInt32(ColMaterialTranAmount) - 1, ROW].Merge();
			//	object sumObject;

			//	//sumObject = data.Compute("Sum(MaterialTranAmount)", "");
			//	//sheet.Range[ROW, Convert.ToInt32(ColMaterialTranAmount)].CellStyle.Font.Bold = true;
			//	//report.SetText(ref sheet, ROW, Convert.ToInt32(ColMaterialTranAmount), Convert.ToDouble(sumObject).ToString("0.##"));
			//	//sheet.Range[ROW, Convert.ToInt32(ColMaterialTranAmount)].HorizontalAlignment = ExcelHAlign.HAlignRight;
			//	//sheet.Range[ROW, Convert.ToInt32(ColMaterialTranAmount)].VerticalAlignment = ExcelVAlign.VAlignTop;

			//	sumObject = data.Compute("Sum(TotalMaterialBaseAmount)", "");
			//	sheet.Range[ROW, Convert.ToInt32(ColTotalMaterialBaseAmount)].CellStyle.Font.Bold = true;
			//	report.SetText(ref sheet, ROW, Convert.ToInt32(ColTotalMaterialBaseAmount), Convert.ToDouble(sumObject).ToString("0.##"));
			//	sheet.Range[ROW, Convert.ToInt32(ColTotalMaterialBaseAmount)].HorizontalAlignment = ExcelHAlign.HAlignRight;
			//	sheet.Range[ROW, Convert.ToInt32(ColTotalMaterialBaseAmount)].VerticalAlignment = ExcelVAlign.VAlignTop;

			//	sumObject = data.Compute("Sum(Payment)", "");
			//	sheet.Range[ROW, Convert.ToInt32(ColPayment)].CellStyle.Font.Bold = true;
			//	report.SetText(ref sheet, ROW, Convert.ToInt32(ColPayment), Convert.ToDouble(sumObject).ToString("0.##"));
			//	sheet.Range[ROW, Convert.ToInt32(ColPayment)].HorizontalAlignment = ExcelHAlign.HAlignRight;
			//	sheet.Range[ROW, Convert.ToInt32(ColPayment)].VerticalAlignment = ExcelVAlign.VAlignTop;

			//	sumObject = data.Compute("Sum(Balance)", "");
			//	sheet.Range[ROW, Convert.ToInt32(ColBalance)].CellStyle.Font.Bold = true;
			//	report.SetText(ref sheet, ROW, Convert.ToInt32(ColBalance), Convert.ToDouble(sumObject).ToString("0.##"));
			//	sheet.Range[ROW, Convert.ToInt32(ColBalance)].HorizontalAlignment = ExcelHAlign.HAlignRight;
			//	sheet.Range[ROW, Convert.ToInt32(ColBalance)].VerticalAlignment = ExcelVAlign.VAlignTop;

			//}

			endRow = ROW - 1;
			endRow = ROW - 1;

			#endregion sheet



			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			sheet.UsedRange.WrapText = true;
			sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
			sheet.UsedRange.CellStyle.Font.Size = 8;



			ReportUtility reportUtility = new ReportUtility();
			//reportUtility.CompanyHeader(ref sheet, endCol, "Purchase Report Register GRN Wise", identity.CompanyId);
			reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);


			sheet.Name = SheetName;
			sheet.UsedRange.WrapText = true;
			sheet.IsGridLinesVisible = false;
			report.PlantHeader(ref sheet, COL, SheetName, PlantId);
			report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);

			var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
			workbook.Version = ExcelVersion.Excel2016;

			workbook.SaveAs(filePath);
			workbook.Close();
			excelEngine.Dispose();
			return filePath;

		}

		public DataTable GetStockRegisterReportData(string CompanyId, string PlantId, string FromDate, string ToDate, string GRNNo, bool isreport)
		{
			try
			{
				var str = @"SELECT   IR.Id GRNNo,REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNEntryDate,
							IR.GateEntryNo,p.UserName AS PartyName,P.Code PartyCode,isnull(PP.GSTIN,'') GSTINNo
						   ,ROUND(Isnull(IRD.TotalMaterialTranAmount*IR.ToCurrencyRate,0),2) MaterialTranAmount
						   ,ROUND(Isnull(IRD.TotalTaxAmount,0),2)+ROUND(Isnull(IRD.ChargesTaxTranAmount,0),2) TotalTaxAmount
						   ,ROUND(Isnull(IRD.TotalMaterialTranAmount*IR.ToCurrencyRate,0)+Isnull(IRD.TotalTaxAmount,0),2)+ROUND(Isnull(IRD.ChargesTaxTranAmount,0),2) TotalMaterialBaseAmount
						   ,SUM(ROUND(ISNULL(I.WrittenOffAmount*I.CompanyCurrencyRate,0),4)) as Payment
						   ,( ROUND(Isnull(IRD.TotalMaterialTranAmount*IR.ToCurrencyRate,0)+Isnull(IRD.TotalTaxAmount,0)+Isnull(IRD.ChargesTaxTranAmount,0),2))-(SUM(ROUND(ISNULL(I.WrittenOffAmount*I.CompanyCurrencyRate,0),4))) as Balance
						   ,VoucherNo=CASE WHEN IR.EmployeeId <> '' Then V1.VoucherNo else V.VoucherNo END
						   ,PostingDate= CASE WHEN IR.EmployeeId <> '' Then REPLACE(CONVERT(CHAR(11), ep.PostingDate, 106),' ','-')   else REPLACE(CONVERT(CHAR(11), I.PostingDate, 106),' ','-')  END 
						   ,IR.DocRefNo,CU.Code CurrencyName,IR.PartyType
						   ,PG.UserName PartyGroup,PC.UserName PartyCategory,PSC.UserName PartySubCategory,PAG.UserName PartyAccountGroup

							--new add
							,'' DocRefDate,'' GrnDocDateDifference,'' GateName,'' InvoicingPartyPlant,'' DeliveryPartyPlant,'' Employee
					from [TRN].[InventoryReceive] AS IR
					left jOIN (select InventoryReceiveId,Sum(TransactionQty)TransactionQty,Sum(MaterialTranAmount)MaterialTranAmount
						,Sum(TotalMaterialTranAmount)TotalMaterialTranAmount,Sum(TotalMaterialBooksCurrencyAmount)TotalMaterialBooksCurrencyAmount
						,SUM(TotalTaxAmount) TotalTaxAmount,sum(ChargesTaxTranAmount) ChargesTaxTranAmount
						FROM [TRN].[InventoryReceiveDetail]
					group by InventoryReceiveId ) AS IRD ON IR.Id=IRD.InventoryReceiveId 
					left JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
					LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId 
					LEFT JOIN HKP.PartyCategory PC on PC.Id=P.PartyCategoryId
					LEFT JOIN HKP.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
					LEFT JOIN HKP.PartyGroup PG on PG.Id=P.PartyGroupId
					LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Vendor' AND cp.PlantId=IR.PlantId
					LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Vendor'
					LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
					LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
					LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
                    left JOIN trn.Invoice as I ON I.InventoryReceiveId=IR.Id					
					left join trn.Voucher V on V.Id=I.VoucherId
                    left JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
					left join trn.Voucher V1 on V1.Id=ep.VoucherId
						
					where  IR.PlantId='" + PlantId + @"' AND convert(Date,IR.GRNDate) BETWEEN  '" + FromDate + @"' AND '" + ToDate + @"' 
                    AND IR.GRNType IN('GRNBYPO','GRN','EMPGRN')

					group by IR.GRNDate,IR.Id,IR.GateEntryNo,p.UserName,P.Code,PP.GSTIN,IRD.TotalMaterialTranAmount,IRD.TotalMaterialBooksCurrencyAmount,IRD.TotalTaxAmount,IRD.ChargesTaxTranAmount
					,MaterialTranAmount,IR.EmployeeId,IR.EmployeeId,V.VoucherNo,V1.VoucherNo,ep.PostingDate,I.PostingDate,IR.DocRefNo,CU.Code,IR.PartyType,PAG.UserName
					,PC.UserName,PSC.UserName,PG.UserName,IR.ToCurrencyRate";

				if (isreport)
				{

					var newsql = "select * from(" + str + ") y where y.GRNNo in (" + GRNNo + @")";
					return _sqlRepository.GetDataTable(newsql);

				}
				else
				{
					str += "";
					return _sqlRepository.GetDataTable(str);
				}


			}
			catch (Exception e)
			{
				throw e;
			}
		}

		#endregion Pages
	}

}