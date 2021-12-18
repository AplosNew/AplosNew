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
using System.Linq;

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

		[HttpGet, Authorize]
		public ActionResult GetAllData(string PIMasterId,string VersionId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			string sql = @"SELECT PM.Id,PM.PINo,PM.RefNo,FORMAT(PM.PIDate,'dd-MMM-yyyy') PIDate,PM.CurrencyId,PM.BuyerId
							,PM.CustomerId,PM.InvoicingByAddress,PM.DeliveryByAddress,PM.RevisionNo
							,C.Code Currency,B.UserName Buyer,P.UserName Customer
							 FROM PIMaster PM 
							LEFT OUTER JOIN SCS.Currency AS c ON C.Id=PM.CurrencyId
							LEFT OUTER JOIN hkp.Buyer AS b ON B.Id=PM.BuyerId
							LEFT OUTER JOIN HKP.Party AS p ON p.Id=PM.CustomerId
						WHERE PM.Id='"+ PIMasterId + @"'";

			var PIMasterData = _sqlRepository.GetDataCollection(sql, null);

			sql = @"SELECT p.Id, p.PIMasterId, p.PIVersionId, p.Rate, p.Quantity, p.Amount, p.UoMId,NULL AS MaterialGroupUOMList,
							   p.[Description], p.DeliveryDate, p.MaterialGroupMasterId,mgm.UserName AS MaterialGroup       
						  FROM PIMaterial AS p
						  LEFT JOIN mst.MaterialGroupMaster AS mgm ON mgm.Id=p.MaterialGroupMasterId
						WHERE p.PIMasterId='"+ PIMasterId + @"' AND p.PIVersionId='"+ VersionId + @"'";

			var PIMaterial = _sqlRepository.GetDataCollection(sql, null);

			sql = @"SELECT U.MaterialGroupMasterId,UOM.Code,UOM.Id FROM (
					SELECT mgm.Id MaterialGroupMasterId, mgm.BaseUoMId AS UOMId FROM mst.MaterialGroupMaster AS mgm
					UNION ALL
					SELECT m.MaterialGroupMasterId, m.AlternativeUoMId
					  FROM mst.MaterialGroupAlternativeUoM AS M
					) U
					JOIN scs.UnitOfMeasurement AS uom ON uom.Id=U.UOMId
					WHERE U.MaterialGroupMasterId IN (
						SELECT P.MaterialGroupMasterId FROM PIMaterial P WHERE p.PIMasterId='"+ PIMasterId + @"' AND p.PIVersionId='" + VersionId + @"'
					)";
			var UOMList=_sqlRepository.GetDataCollection(sql, null);
            for (int i = 0; i < PIMaterial.Count; i++)
            {
				var U = UOMList.Where(w => w["MaterialGroupMasterId"] == PIMaterial[i]["MaterialGroupMasterId"].ToString()).ToList();
				PIMaterial[i]["MaterialGroupUOMList"] = U;
			}
			sql = @" SELECT * FROM PIVersion AS pv WHERE pv.PIMasterId='" + PIMasterId + @"'";
			var VersisonList= _sqlRepository.GetDataCollection(sql, null);

			return Json(new{PIMaster= PIMasterData,VarsionData= VersisonList,ItemData= PIMaterial }, JsonRequestBehavior.AllowGet);
		}


		[HttpGet, Authorize]
		public ActionResult GetUoMList(string MaterialGroupMasterId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			string sql = @"SELECT U.MaterialGroupMasterId,UOM.Code,UOM.Id FROM (
SELECT mgm.Id MaterialGroupMasterId, mgm.BaseUoMId AS UOMId FROM mst.MaterialGroupMaster AS mgm
UNION ALL
SELECT m.MaterialGroupMasterId, m.AlternativeUoMId
  FROM mst.MaterialGroupAlternativeUoM AS M
) U
JOIN scs.UnitOfMeasurement AS uom ON uom.Id=U.UOMId
WHERE U.MaterialGroupMasterId='"+ MaterialGroupMasterId + @"'";

			var PIMasterData = _sqlRepository.GetDataCollection(sql, null);		

			return Json(new { PIMaster = PIMasterData}, JsonRequestBehavior.AllowGet);
		}



		#region Upload Roll Data
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

	}
}