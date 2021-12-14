#region using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Model.Materials;
using Library.Service.Enums;
using Library.Service.Materials;
using System.Collections.Generic;
using System.Threading;
using System.Web.Mvc;
using Library.Data.Sql;
using System.Web.Script.Serialization;
using System;
using Newtonsoft.Json;
using Library.Data;
using System.IO;
using Library.HumanResource.Attendance.Manual;
using Library.Service.Helpers;
using System.Data;
using Library.OrderManagement.FabricRollClass;

#endregion using

namespace Aplos.Areas.Commercial.Controllers
{
	public class ProformaInvoiceController : BaseController
	{
		#region -- Constructor

		private readonly IFabricRollMasterService _fabricRollMasterService;
		private SqlRepository _sqlRepository = new SqlRepository();
		public ProformaInvoiceController(IFabricRollMasterService fabricRollMasterService)
		{
			_fabricRollMasterService = fabricRollMasterService;
		}

		#endregion -- Constructor

		#region Pages

		[Authorize]
		public ActionResult Aplos()
		{
			return View();
		}

		#endregion Pages

		#region -- Operations

		[HttpGet, Authorize]
		public JsonResult GetList(GridParameter parameters, string paidHours)
		{
			CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_fabricRollMasterService.Query(parameters, identity.CompanyGroupId, paidHours, identity.PlantId), JsonRequestBehavior.AllowGet);
		}

		[HttpGet, Authorize]
		public JsonResult GetFabricIncrementValue()
		{
			return Json(_fabricRollMasterService.InsertOrUpdateGraphIncrement(), JsonRequestBehavior.AllowGet);
		}
		[HttpPost]
		public JsonResult Create(IEnumerable<FabricRollMaster> entities)
		{
			_fabricRollMasterService.InsertOrUpdateGraph(entities);
			return Json(new { Message = AplosMessage.Insert });
		}


		[HttpPost]
		public JsonResult Update(List<Dictionary<string, object>> FabricRollData, string PackingForm)
		{
			_fabricRollMasterService.UpdateFabricRoll(FabricRollData, PackingForm);
			return Json(new { Message = AplosMessage.Updated });
		}



		[HttpPost, Authorize]
		public JsonResult GetRoll(int NoofRolls, Dictionary<string, object> SelectedRow, double Width, string PackingForm)
		{
			_fabricRollMasterService.CreateRoll(NoofRolls, SelectedRow, Width, PackingForm);
			return Json(new { Message = AplosMessage.Insert });
		}

		public ActionResult Delete(string id)
		{
			try
			{
				if (string.IsNullOrEmpty(id))
					throw new Exception("Select entry first");
				ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
				con.BeginTransaction();
				con.executeQuery("delete from TRN.FabricRollMaster where id='" + id + "'");
				con.CommitTransaction();
				return Json(new { Error = false,/* Sequence = GetSequence(),*/ Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
			}
		}
		[HttpGet, Authorize]
		public JsonResult GetGRNList(GridParameter parameters)
		{
			return Json(_fabricRollMasterService.GetGRNList(parameters, BusinessProcessEnum.FabricRollManagement.ToString()), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public JsonResult GetGRNDetailList(GridParameter parameters, string inventoryReceiveId)
		{
			return Json(_fabricRollMasterService.GetGRNDetailList(parameters, inventoryReceiveId, BusinessProcessEnum.FabricRollManagement.ToString()), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public JsonResult GetFABRollList(GridParameter parameters, string inventoryReceiveDetailId)
		{
			return Json(_fabricRollMasterService.GetFABRollList(parameters, inventoryReceiveDetailId), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public JsonResult GetBarCideList(string inventoryReceiveDetailId)
		{
			return Json(_fabricRollMasterService.GetBarCideList(inventoryReceiveDetailId), JsonRequestBehavior.AllowGet);
		}
		#endregion -- Operations




		[HttpPost, Authorize]
		public ActionResult PIList(string column, string value)
		{
			string strkey = "1=1";
			if (string.IsNullOrEmpty(column) == false)
				strkey = column + " like '%" + value + "%'";

			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			string sql = @"select top 100 * from (SELECT PM.Id,PM.PINo,PM.RefNo,FORMAT(PM.PIDate,'dd-MMM-yyyy') PIDate,PM.CurrencyId,PM.BuyerId
,PM.CustomerId,PM.InvoicingByAddress,PM.DeliveryByAddress,PM.RevisionNo
,C.Code Currency,B.UserName Buyer,P.UserName Customer
 FROM PIMaster PM 
LEFT OUTER JOIN SCS.Currency AS c ON C.Id=PM.CurrencyId
LEFT OUTER JOIN hkp.Buyer AS b ON B.Id=PM.BuyerId
LEFT OUTER JOIN HKP.Party AS p ON p.Id=PM.CustomerId
) AS TEMP WHERE " + strkey;

			return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
		}

		[HttpPost, Authorize]
		public ActionResult MaterialList(string inventoryReceiveId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			string sql = @"SELECT 
DISTINCT IRD.Id,IRD.InventoryReceiveId,IRD.TransactionQty,IRD.TransactionUoMId,Isnull(FRM.SplitCount,0)SplitCount
,ISNULL(FRM.TotalDistributeQty,0)TotalDistributeQty,UOM.UserName UOM,BUoM.UserName BaseUoM,IR.Id GRNNo,IR.GRNDate
,P.UserName PartyName,PL.FabRollPrefix,IM.PlantId,IM.MaterialMasterId,IM.ArticleId
,IM.FirstCharacteristicsId SKUId,MM.UserName MaterialMasterName,MMA.StandardName ArticleName
,C.UserName SKU1,C2.UserName SKU2,C3.UserName SKU3,CV.UserName SKUValue,CV2.UserName SKUValue2,CV3.UserName SKUValue3, C.UserName +':'+CV.UserName SKUInfo,CU.Code
,MGM.UserName MaterialGroup
FROM [TRN].[InventoryReceiveDetail] IRD
                                        LEFT JOIN TRN.InventoryReceive IR ON IRD.InventoryReceiveId=IR.Id
                                        LEFT JOIN HKP.Party P ON IR.PartyId=P.Id
                                        LEFT JOIN TRN.InventoryMaterial IM ON IRD.InventoryMaterialId=IM.Id
										--LEFT JOIN ORG.Plant PL ON IM.PlantId= PL.Id
                                        LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
										LEFT JOIN scs.PlantConfig PL ON  PL.PlantId=IM.PlantId
                                        LEFT JOIN SCS.UnitOfMeasurement UOM ON IRD.TransactionUoMId=UOM.Id
                                        LEFT JOIN SCS.UnitOfMeasurement BUoM ON IRD.BaseUOMId=BUoM.Id
                                        LEFT JOIN MST.MaterialMaster MM ON IM.MaterialMasterId=MM.Id

                                        LEFT JOIN MST.MaterialGroupMaster MGM ON MM.MaterialGroupMasterId=MGM.Id
                                        LEFT JOIN MST.MaterialMasterArticle MMA ON IM.ArticleId=MMA.Id

                                        LEFT JOIN HKP.Characteristics C ON IM.FirstCharacteristicsId=C.Id
                                        LEFT JOIN HKP.Characteristics C2 ON IM.SecondCharacteristicsId=C2.Id
                                        LEFT JOIN HKP.Characteristics C3 ON IM.ThirdCharacteristicsId=C3.Id

                                        LEFT JOIN [HKP].[CharacteristicsValue] CV ON IM.FirstCharacteristicsValueId=CV.Id
                                        LEFT JOIN [HKP].[CharacteristicsValue] CV2 ON IM.SecondCharacteristicsValueId=CV2.Id
                                        LEFT JOIN [HKP].[CharacteristicsValue] CV3 ON IM.ThirdCharacteristicsValueId=CV3.Id
                                        LEFT JOIN MST.MaterialMasterBusinessProcess MMBP ON MM.Id=MMBP.MaterialMasterId
                                        LEFT JOIN SCS.BusinessProcess BP ON MMBP.BusinessProcessId=BP.Id
										LEFT JOIN (SELECT COUNT(Id) SplitCount,Sum(VendorQty) TotalDistributeQty
										,InventoryReceiveDetailId FROM TRN.FabricRollMaster 
										GROUP BY InventoryReceiveDetailId) FRM ON IRD.Id=FRM.InventoryReceiveDetailId
WHERE BP.BusinessProcessName='FabricRollManagement' AND IRD.InventoryReceiveId='" + inventoryReceiveId + @"'";

			return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);

		}

		[HttpPost, Authorize]
		public ActionResult FabricRollList(string inventoryReceiveDetailId)
		{

			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			string sql = @"select * from TRN.FabricRollMaster where InventoryReceiveDetailId='" + inventoryReceiveDetailId + @"'";

			return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);

		}
		[HttpGet, Authorize]
		public ActionResult DownloadRollReport(string inventoryReceiveDetailId)
		{
			try
			{
				Library.OrderManagement.FabricRollClass.FabricRollClass RollReport = new Library.OrderManagement.FabricRollClass.FabricRollClass();
				RollReport.DownloadReport(inventoryReceiveDetailId);

				return null;
			}
			catch (Exception ex)
			{
				throw ex;
			}

		}


		#region Upload Roll Data

		[HttpPost]
		public JsonResult CreateRollFile(FormCollection form)
		{
			var pre = form["FabricRollFile"];
			var settings = new JsonSerializerSettings
			{
				NullValueHandling = NullValueHandling.Ignore,
				MissingMemberHandling = MissingMemberHandling.Ignore
			};
			var FabricRollFile = JsonConvert.DeserializeObject<FabricRollFile>(pre, settings);
			var file = Request.Files["file"];
			if (file != null)
			{
				var extension = Path.GetExtension(file.FileName);
				if (extension.ToLower() != ".xls" && extension.ToLower() != ".xlsx")
				{
					throw new CustomException(Resources.ImageUploadError);
				}


				FabricRollClass Clsss = new FabricRollClass();
				//clsManualAttendanceFileUpload p = new clsManualAttendanceFileUpload();
				Clsss.Save(file.FileName, extension, FabricRollFile, out DataSet dsMaster);
				var path = Path.Combine(ResourcesPathReader.GetFabricRollFilePath(), dsMaster.Tables[0].Rows[0]["FileId"].ToString());

				if (System.IO.File.Exists(path))
				{
					System.IO.File.Delete(path);
					file.SaveAs(path);
				}
				else
				{
					file.SaveAs(path);
				}
			}
			return Json(new { Message = AplosMessage.Success });
		}

		[HttpGet, Authorize]
		public ActionResult GetMaster()
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				clsManualAttendanceFileUpload ep = new clsManualAttendanceFileUpload();
				return Json(ep.GetMaster(identity.PlantId), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				return Json(new { Error = true, Message = ex.Message });
			}
		}
		#endregion
		public void SaveFile(out string path)
		{
			path = "";
			try
			{
				var file = Request.Files["file"];
				if (file != null)
				{
					var extension = Path.GetExtension(file.FileName);
					if (extension.ToLower() == ".xlsx" || extension.ToLower() == ".xls")
					{
					}
					else
						throw new CustomException(Resources.ExcelUploadError);
				}
				if (file != null)
				{
					path = Path.Combine(ResourcesPathReader.GetFabricRollData(), file.FileName);
					if (System.IO.File.Exists(path))
					{
						System.IO.File.Delete(path);
						file.SaveAs(path);
					}
					else
					{
						file.SaveAs(path);
					}
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}


	}
}