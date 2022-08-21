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

namespace Aplos.Areas.Materials.Controllers
{
    public class StockRegisterController : BaseController
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

        public StockRegisterController(
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
       
		public ActionResult StockRegister() 
		{
			return View();
		}

        

        [Authorize, HttpPost]
		public JsonResult GetMaterialLedger(string fromDate,string toDate)
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
			var jsondata= Json(_inventoryReceiveService.GetMaterialLedger(fromDate,toDate), JsonRequestBehavior.AllowGet);
			jsondata.MaxJsonLength = int.MaxValue;
			return jsondata;
		}
        
        [HttpPost, Authorize]
        public ActionResult StockRegisterData(string PlantId, string ToDate, string FromDate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                List<Dictionary<string, object>> NewData = (List<Dictionary<string, object>>)Library.Service.Helpers.DataTableExtensions.DataTableToJson(GetStockRegisterData(identity.PlantId, FromDate, ToDate));
                var jsondata = Json(new { NewData, Message = AplosMessage.Success });
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetStockRegisterData(string PlantId, string FromDate, string ToDate)
        {
            try
            {
                var str = @"SELECT * FROM (SELECT   ROW_NUMBER() OVER(ORDER BY IRD.Id ASC) AS SLNo  
							,IsRegular =case when MM.IsRegular=1 then 'Yes' else 'No' end
							,MT.UserName MaterialType
						,MGM.UserName AS MaterialGroupMasterName
						,IM.MaterialMasterId
						,MM.UserName MaterialMasterName
						,ART.StandardName ArticleName, ISNULL(FCV.UserName,'') AS SKU1
						,ISNULL(SCV.UserName,'') AS SKU2
						,ISNULL(TCV.UserName,'') AS SKU3 
						,IR.Id As GRNNo,IRD.Id As GRNROWId,   REPLACE(CONVERT(CHAR(11), IR.GRNDate, 106),' ','-') AS GRNDate
						,IRD.TransactionQty GRNQty
						,TUoM.UserName AS UOM
						,ROUND(Isnull(IRD.MaterialTranAmount,0),2)  GRNMaterialAmount
						,IRD.BaseQty-IRD.IssueQty BalanceStock
						,DATEDIFF(day, IR.GRNDate,GETDATE()) AS 'StockInDays'
                        ,IsAsset=CASE WHEN IRD.IsAsset=0 then 'No' else 'Yes' END
						,MS.UserName StorageLocation,'' StorageResponsiblePerson
						,GRNType=CASE WHEN IR.EmployeeId <> '' Then 'Employee' else 'Vendor' END
                        ,p.UserName AS PartyName
						,EI.EmployeeName FirstName						   
						,IR.GateEntryNo
						,IR.DocRefNo
						,IR.AddedBy
                        ,CASE  WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  AND IR.AuthorizedByStatus = 'Approved' Then 'Approved'
								WHEN IR.CheckedBy is not null And IR.CheckedByStatus = 'ForChecked' AND IR.AuthorizedBy is null And IR.AuthorizedByStatus is null Then 'To be Checked'										
								WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Checked' AND IR.AuthorizedBy is NOT null  Then 'To be approved'
								WHEN IR.CheckedBy is not null ANd IR.CheckedByStatus = 'Hold' Then 'Checking Hold'
								WHEN IR.CheckedBy is not null AND IR.CheckedByStatus = 'Rejected' Then 'Checking Rejected'
                                WHEN IR.CheckedBy is not null ANd IR.AuthorizedByStatus = 'Hold' Then 'Approving Hold'
								WHEN IR.CheckedBy is not null AND IR.AuthorizedByStatus = 'Rejected' Then 'Approving Rejected'	 
								END GRNCheckStatus
                        ,EI1.EmployeeName CheckedBY
						,EI2.EmployeeName AuthorizedBy
						,pod.InventoryReceiveId PONo,REPLACE(CONVERT(CHAR(11), po.PODate, 106),' ','-') AS PODate,pod.TransactionQty POQty
						,MRM.Id RequsitionNo,REPLACE(CONVERT(CHAR(11), MRM.RequisitionDate, 106),' ','-') AS  RequisitionDate,MRD.TransactionQty RequisitionQty
						,EMRM.EmployeeName RequisitionAddedBy
						,EMRM1.EmployeeName ReqCheckBy
						,EMRM2.EmployeeName ReqApproveBy
					from TRN.InventoryMaterial AS IM
					JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
					LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
					LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
					LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
					LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
					LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
					LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
					LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
					LEFT jOIN [TRN].[InventoryReceiveDetail] AS IRD ON IRD.InventoryMaterialId=IM.Id --and ird.InventoryReceiveId='1987'
					LEFT jOIN [TRN].[InventoryReceive] AS IR ON IR.Id=IRD.InventoryReceiveId
					LEFT JOIN [TRN].[PurchaseOrderDetail] AS PID on PID.Id=IRD.PODetailsId 
					LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IRD.TransactionUoMId=TUoM.Id	
					LEFT JOIN [SCS].[UnitOfMeasurement] AS BUoM ON IRD.BaseUOMId=BUoM.Id
					left JOIN org.Company AS co  ON co.Id=ir.CompanyId
					left JOIN [SCS].[Currency] AS CU ON Co.BaseCurrencyId=CU.Id
					LEFT JOIN [HKP].[MaterialType] AS MT On MGM.MaterialTypeId=MT.Id				
					LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId
					LEFT JOIN HKP.CompanyParty CP ON CP.PartyId=P.Id AND CP.PartyType='Vendor' AND cp.PlantId=IR.PlantId
					LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=CP.PartyAccountGroupId AND PAG.AccountType='Vendor' 
					LEFT JOIN HKP.PartyCategory PC on PC.Id=P.PartyCategoryId
					LEFT JOIN HKP.PartySubCategory PSC on PSC.Id=P.PartySubCategoryId
					LEFT JOIN HKP.PartyGroup PG on PG.Id=P.PartyGroupId
					LEFT JOIN HKP.PartyPlant AS PP ON PP.Id=IR.InvoicingPartyPlantId  
					LEFT JOIN HKP.PartyPlant AS PPD ON PPD.Id=IR.DeliveryPartyPlantId
					LEFT JOIN EmployeeInformation EI ON EI.SystemId=IR.EmployeeId
                    LEFT JOIN hkp.MaterialStorage AS MS ON MS.Id=IR.MaterialStorageId
					LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=IR.CheckedBy
					LEFT JOIN EmployeeInformation EI2 ON EI2.SystemId=IR.AuthorizedBy
                    LEFT JOIN trn.Invoice as I ON I.InventoryReceiveId=IR.Id					
					LEFT JOIN trn.Voucher V on V.Id=I.VoucherId
                    LEFT JOIN trn.EmployeePayable as ep ON ep.InventoryReceiveId=IR.Id					
					LEFT JOIN trn.Voucher V1 on V1.Id=ep.VoucherId
					LEFT JOIN trn.GateEntry  GE ON GE.Id=Ir.GateEntryNo	
					LEFT JOIN trn.PurchaseOrderDetail pod on pod.Id=IRD.PODetailsId
					LEFT JOIN trn.PurchaseOrder po on po.Id=pod.InventoryReceiveId
					LEFT JOIN TRN.MaterialRequsitionDetails MRD ON MRD.Id=pod.RequisitionDetailId
					LEFT JOIN TRN.MaterialRequsitionMaster MRM ON MRM.Id=MRD.MaterialReqqusitionMasterId
					LEFT JOIN EmployeeInformation EMRM ON EMRM.SystemId=MRM.AddedBy
					LEFT JOIN EmployeeInformation EMRM1 ON EMRM1.SystemId=MRM.CheckedBy
					LEFT JOIN EmployeeInformation EMRM2 ON EMRM2.SystemId=MRM.AuthorizedBy
					WHERE  IR.PlantId='" + PlantId + "' AND convert(Date,IR.GRNDate) BETWEEN  '"+ FromDate + "' AND '"+ ToDate + @"'
						--AND IR.GRNType IN('GRNBYPO') 
					AND (IRD.BaseQty-IRD.IssueQty)>0 and MRM.Id<>''
						) x";
                return _sqlRepository.GetDataTable(str);

                //if (isreport)
                //{

                //    var newsql = "select * from(" + str + ") y where y.GRNNo in (" + GRNNo + @")";
                //    return _sqlRepository.GetDataTable(newsql);

                //}
                //else
                //{
                //    str += "";
                //    return _sqlRepository.GetDataTable(str);
                //}


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

			report.SetHeaderText(ref sheet, ROW, COL, "Party Name", 25, ExcelHAlign.HAlignLeft);
			int ColPartyName = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Invoicing Party Plant", 18, ExcelHAlign.HAlignLeft);
			int ColInvoicingPartyPlant = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Delivery Party Plant", 18, ExcelHAlign.HAlignLeft);
			int ColDeliveryPartyPlant = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Party Code", 13, ExcelHAlign.HAlignLeft);
			int ColPartyCode = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Tax ID", 15, ExcelHAlign.HAlignLeft);
			int ColTaxID = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Employee", 18, ExcelHAlign.HAlignLeft);
			int ColEmployee = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "GRNNo", 10, ExcelHAlign.HAlignLeft);
			int ColGRNNo = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "GRN Date", 10, ExcelHAlign.HAlignLeft);
			int ColGRNEntryDate = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Voucher No", 12, ExcelHAlign.HAlignLeft);
			int ColVoucherNo = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Posting Date", 12, ExcelHAlign.HAlignLeft);
			int ColPostingDate = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Doc Ref No", 10, ExcelHAlign.HAlignLeft);
			int ColDocRefNo = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Doc Ref Date", 11, ExcelHAlign.HAlignLeft);
			int ColDocRefDate = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Grn Doc Date Difference", 20, ExcelHAlign.HAlignLeft);
			int ColGrnDocDateDifference = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Gate Entry No", 12, ExcelHAlign.HAlignLeft);
			int ColGateEntryNo = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Gate Name", 13, ExcelHAlign.HAlignLeft);
			int ColGateName = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Base Currency", 12, ExcelHAlign.HAlignLeft);
			int ColBaseCurrency = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Base Amount", 13, ExcelHAlign.HAlignRight);
			//sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
			int ColMaterialTranAmount = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Total Tax Amount", 15, ExcelHAlign.HAlignRight);
			int ColTotalTaxAmount = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Total Base Amount", 16, ExcelHAlign.HAlignRight);
			int ColTotalMaterialBaseAmount = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Payment", 13, ExcelHAlign.HAlignRight);
			int ColPayment = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Balance", 13, ExcelHAlign.HAlignRight);
			int ColBalance = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Party Group", 10, ExcelHAlign.HAlignRight);
			int ColPartyGroup = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Party Category", 13, ExcelHAlign.HAlignRight);
			int ColPartyCategory = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Party SubCategory", 16, ExcelHAlign.HAlignRight);
			int ColPartySubCategory = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Party Type", 10, ExcelHAlign.HAlignRight);
			int ColPartyType = COL;
			COL++;

			report.SetHeaderText(ref sheet, ROW, COL, "Party Account Group", 18, ExcelHAlign.HAlignLeft);
			int ColPartyAccountGroup = COL;

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
				sheet[ROW, ColPartyName].Text = data.Rows[i]["PartyName"].ToString();
				sheet[ROW, ColInvoicingPartyPlant].Text = data.Rows[i]["InvoicingPartyPlant"].ToString();
				sheet[ROW, ColDeliveryPartyPlant].Text = data.Rows[i]["DeliveryPartyPlant"].ToString();
				sheet[ROW, ColPartyCode].Text = data.Rows[i]["PartyCode"].ToString();
				sheet[ROW, ColTaxID].Text = data.Rows[i]["GSTINNo"].ToString();
				sheet[ROW, ColEmployee].Text = data.Rows[i]["Employee"].ToString();
				sheet[ROW, ColGRNNo].Text = data.Rows[i]["GRNNo"].ToString();
				sheet[ROW, ColGRNEntryDate].Text = data.Rows[i]["GRNEntryDate"].ToString();
				sheet[ROW, ColVoucherNo].Text = data.Rows[i]["VoucherNo"].ToString();
				sheet[ROW, ColPostingDate].Text = data.Rows[i]["PostingDate"].ToString();
				sheet[ROW, ColDocRefNo].Text = data.Rows[i]["DocRefNo"].ToString();
				sheet[ROW, ColDocRefDate].Text = data.Rows[i]["DocRefDate"].ToString();
				sheet[ROW, ColGrnDocDateDifference].Text = data.Rows[i]["GrnDocDateDifference"].ToString();
				sheet[ROW, ColGateEntryNo].Text = data.Rows[i]["GateEntryNo"].ToString();
				sheet[ROW, ColGateName].Text = data.Rows[i]["GateName"].ToString();
				sheet[ROW, ColBaseCurrency].Text = data.Rows[i]["CurrencyName"].ToString();
				sheet[ROW, ColMaterialTranAmount].Number = clsStaticInfo.dbl(data.Rows[i]["MaterialTranAmount"].ToString());
				sheet[ROW, ColTotalTaxAmount].Number = clsStaticInfo.dbl(data.Rows[i]["TotalTaxAmount"].ToString());
				sheet[ROW, ColTotalMaterialBaseAmount].Number = clsStaticInfo.dbl(data.Rows[i]["TotalMaterialBaseAmount"].ToString());
				sheet[ROW, ColPayment].Number = clsStaticInfo.dbl(data.Rows[i]["Payment"].ToString());
				sheet[ROW, ColBalance].Number = clsStaticInfo.dbl(data.Rows[i]["Balance"].ToString());
				sheet[ROW, ColPartyGroup].Text = data.Rows[i]["PartyGroup"].ToString();
				sheet[ROW, ColPartyCategory].Text = data.Rows[i]["PartyCategory"].ToString();
				sheet[ROW, ColPartySubCategory].Text = data.Rows[i]["PartySubCategory"].ToString();
				sheet[ROW, ColPartyType].Text = data.Rows[i]["PartyType"].ToString();
				sheet[ROW, ColPartyAccountGroup].Text = data.Rows[i]["PartyAccountGroup"].ToString();


				sheet.Range[ROW, ColPartyName, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
				sheet.Range[ROW, ColPartyName, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

				ROW++;
			}

			//ROW++;

			if (FromDate != "" && ToDate != "")
			{


				report.SetText(ref sheet, ROW, Convert.ToInt32(ColMaterialTranAmount) - 1, "Total");
				sheet.Range[ROW, Convert.ToInt32(ColMaterialTranAmount) - 1].CellStyle.Font.Bold = true;
				//sheet.Range[1, ROW, Convert.ToInt32(ColMaterialTranAmount) - 1, ROW].Merge();
				object sumObject;

				//sumObject = data.Compute("Sum(MaterialTranAmount)", "");
				//sheet.Range[ROW, Convert.ToInt32(ColMaterialTranAmount)].CellStyle.Font.Bold = true;
				//report.SetText(ref sheet, ROW, Convert.ToInt32(ColMaterialTranAmount), Convert.ToDouble(sumObject).ToString("0.##"));
				//sheet.Range[ROW, Convert.ToInt32(ColMaterialTranAmount)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				//sheet.Range[ROW, Convert.ToInt32(ColMaterialTranAmount)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = data.Compute("Sum(TotalMaterialBaseAmount)", "");
				sheet.Range[ROW, Convert.ToInt32(ColTotalMaterialBaseAmount)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet, ROW, Convert.ToInt32(ColTotalMaterialBaseAmount), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet.Range[ROW, Convert.ToInt32(ColTotalMaterialBaseAmount)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet.Range[ROW, Convert.ToInt32(ColTotalMaterialBaseAmount)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = data.Compute("Sum(Payment)", "");
				sheet.Range[ROW, Convert.ToInt32(ColPayment)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet, ROW, Convert.ToInt32(ColPayment), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet.Range[ROW, Convert.ToInt32(ColPayment)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet.Range[ROW, Convert.ToInt32(ColPayment)].VerticalAlignment = ExcelVAlign.VAlignTop;

				sumObject = data.Compute("Sum(Balance)", "");
				sheet.Range[ROW, Convert.ToInt32(ColBalance)].CellStyle.Font.Bold = true;
				report.SetText(ref sheet, ROW, Convert.ToInt32(ColBalance), Convert.ToDouble(sumObject).ToString("0.##"));
				sheet.Range[ROW, Convert.ToInt32(ColBalance)].HorizontalAlignment = ExcelHAlign.HAlignRight;
				sheet.Range[ROW, Convert.ToInt32(ColBalance)].VerticalAlignment = ExcelVAlign.VAlignTop;

			}

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