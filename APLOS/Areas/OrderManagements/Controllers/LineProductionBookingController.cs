using Aplos.Controllers;
using Aplos.Properties;
using ExcelDataReader;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Model.OrderManagements;
using Library.Service.Helpers;
using Library.Service.OrderManagements;
using Library.Service.Properties;
using Library.ViewModel.OrderManagements;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.OrderManagements.Controllers
{
	public class LineProductionBookingController : BaseController
	{
		#region Constructor
		private readonly ILineEmployeeAssignService _lineEmployeeAssignService;

		public LineProductionBookingController(ILineEmployeeAssignService lineEmployeeAssignService)
		{
			_lineEmployeeAssignService = lineEmployeeAssignService;
		}
		#endregion

		#region -- Pages

		[Authorize]
		public ActionResult Aplos()
		{
			return View();
		}

		#endregion

		#region -- Operations

		[HttpGet, Authorize]
		public ActionResult GetList(string date, string salesOrderName, string line, string shift)
		{
			return Json(_lineEmployeeAssignService.GetForEditPrdBooking(date, salesOrderName, line, shift), JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult Edit(string id, decimal prdQty, IEnumerable<LineProductionOperationBookingViewModel> entities)
		{
			_lineEmployeeAssignService.UpdateGraphLineProduction(id, prdQty, entities);
			return Json(new { Message = AplosMessage.Updated });
		}

		#endregion

		#region Entry from excel

		[Authorize]
		public ActionResult LineProductionExcel()
		{
			return View();
		}

		[HttpGet, Authorize]
		public ActionResult GetListByDate(string date)
		{
			return Json(_lineEmployeeAssignService.GetProductionBookingListByDate(date), JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult PostExcelData(FormCollection form)
		{
			var toDate = Convert.ToDateTime(form["toDate"]);
			var file = Request.Files["file"];
			var extension = string.Empty;
			var modelList = new List<LineProductionOperationBookingViewModel>();

			if (file.IsNull())
				throw new CustomException(ServiceResources.FilePathNotFound);

			extension = Path.GetExtension(file.FileName);
			if (extension.ToLower() != ".xls" && extension.ToLower() != ".xlsx")
				throw new CustomException(Resources.ExcelUploadError);

			//var fileBytes = new byte[file.ContentLength];

			var path = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), "LineBooking" + extension);
			if (System.IO.File.Exists(path))
			{
				System.IO.File.Delete(path);
				file.SaveAs(path);
			}
			else
				file.SaveAs(path);

			using (var stream = System.IO.File.Open(path, FileMode.Open, FileAccess.Read))
			{
				IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);
				var conf = new ExcelDataSetConfiguration
				{
					ConfigureDataTable = _ => new ExcelDataTableConfiguration
					{
						UseHeaderRow = true
					}
				};
				var dataSet = reader.AsDataSet(conf);
				var dataTable = dataSet.Tables[0];

				for (var i = 0; i < dataTable.Rows.Count; i++)
				{
					var row = dataTable.Rows[i];
					var pDate = row["Date"].ToString().Replace(".", "/");
					if (toDate == Convert.ToDateTime(pDate))
					{
						var model = new LineProductionOperationBookingViewModel();
						model.Id = null;
						model.PlantName = row["Plant"].ToString();
						model.ProductionDate = Convert.ToDateTime(row["Date"].ToString());//DateTime.ParseExact(pDate; "dd/MM/yyyy"; null);
						model.Line = row["Line Number"].ToString();
						model.ProductionShift = row["Shift"].ToString();
						model.SalesOrder = row["SO/LI"].ToString();
						model.Fabrication = row["Version"].ToString();
						model.Style = row["Style"].ToString();
						model.ProductionQty = string.IsNullOrEmpty(row["ACT_PRD"].ToString()) == true ? 0 : Convert.ToInt16(row["ACT_PRD"].ToString());
						model.CustomerCode = row["Customer code"].ToString();
						model.CustomerName = row["Customer Name"].ToString();
						model.TotalManPower = string.IsNullOrEmpty(row["Total Man Power."].ToString()) == true ? 0 : Convert.ToDecimal(row["Total Man Power."].ToString());
						model.PlanRunMC = string.IsNullOrEmpty(row["PlanRunM/c"].ToString()) == true ? 0 : Convert.ToDecimal(row["PlanRunM/c"].ToString());
						model.ActualRunMC = string.IsNullOrEmpty(row["ActRunM/c"].ToString()) == true ? 0 : Convert.ToDecimal(row["ActRunM/c"].ToString());
						model.ExtraMC = string.IsNullOrEmpty(row["Extra M/c"].ToString()) == true ? 0 : Convert.ToDecimal(row["Extra M/c"].ToString());
						model.TrimCheckPress = string.IsNullOrEmpty(row["Trim/check/press"].ToString()) == true ? 0 : Convert.ToDecimal(row["Trim/check/press"].ToString());
						model.SewingSMV = string.IsNullOrEmpty(row["Sewing SMV"].ToString()) == true ? 0 : Convert.ToDecimal(row["Sewing SMV"].ToString());
						model.TotalSMV = string.IsNullOrEmpty(row["Total SMV"].ToString()) == true ? 0 : Convert.ToDecimal(row["Total SMV"].ToString());
						model.MCMINAvailable = string.IsNullOrEmpty(row["M/c MIN Avail."].ToString()) == true ? 0 : Convert.ToDecimal(row["M/c MIN Avail."].ToString());
						model.NonMCMINAvailable = string.IsNullOrEmpty(row["Non M/c MIN Avail."].ToString()) == true ? 0 : Convert.ToDecimal(row["Non M/c MIN Avail."].ToString());
						model.TotalMINAvailable = string.IsNullOrEmpty(row["Total MIN Avail."].ToString()) == true ? 0 : Convert.ToDecimal(row["Total MIN Avail."].ToString());
						model.ActualMINWorked = string.IsNullOrEmpty(row["Actual MIN Worked"].ToString()) == true ? 0 : Convert.ToDecimal(row["Actual MIN Worked"].ToString());
						model.MCSAMProd = string.IsNullOrEmpty(row["M/c SAM Prod."].ToString()) == true ? 0 : Convert.ToDecimal(row["M/c SAM Prod."].ToString());
						model.TotalSAMProd = string.IsNullOrEmpty(row["Total SAM Prod."].ToString()) == true ? 0 : Convert.ToDecimal(row["Total SAM Prod."].ToString());
						model.MCEfficiency = string.IsNullOrEmpty(row["M/c Efficiency"].ToString()) == true ? 0 : Convert.ToDecimal(row["M/c Efficiency"].ToString());
						model.OrderQty = string.IsNullOrEmpty(row["Order Qty"].ToString()) == true ? 0 : Convert.ToDecimal(row["Order Qty"].ToString());
						model.TargetQuantity = string.IsNullOrEmpty(row["Target Quantity."].ToString()) == true ? 0 : Convert.ToDecimal(row["Target Quantity."].ToString());
						model.MaterialCode = row["Material Code"].ToString();
						model.MaterialDesc = row["Material Desc."].ToString();

						model.MachineType = row["Machine Type."].ToString();
						model.OperationType = row["Operation Type"].ToString();
						model.OperationName = row["operation name"].ToString();
						model.Target = string.IsNullOrEmpty(row["Target."].ToString()) == true ? 0 : Convert.ToDecimal(row["Target."].ToString());
						model.Rate = string.IsNullOrEmpty(row["RATE"].ToString()) == true ? 0 : Convert.ToDecimal(row["RATE"].ToString());
						modelList.Add(model);
					}
				}
			}
			if (modelList.IsNotNull() && modelList.Count > 0)
			{
				_lineEmployeeAssignService.InsertLineProductionOperation(modelList, toDate);
				if (System.IO.File.Exists(path))
					System.IO.File.Delete(path);
			}
			else
			{
				if (System.IO.File.Exists(path))
					System.IO.File.Delete(path);
				throw new CustomException("No data found to selected date.");
			}

			return Json(new { Message = AplosMessage.Insert });
		}

		[HttpPost,ChaildAction(ParentActionName = "PostExcelData")]
		public ActionResult UpdateNoApplicablePcsRate(string id)
		{
			_lineEmployeeAssignService.UpdateNoApplicablePcsRate(id);
			return Json(new { });
		}

		#endregion
	}
}