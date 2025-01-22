
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
	public class GateentryTokenController : Controller
	{
		#region Constructor


		private readonly IGateEntryService _gateEntryService;
		private readonly IMaterialRequsitionDetailsServiceService _materialRequsitionDetailsServiceService;
		private readonly IMaterialRequsitionMasterServiceService _materialRequsitionMasterServiceService;



		private readonly IPurchaseOrderDetailService _inventoryDetailService;
		private readonly IPOMaterialService _inventoryMaterialService;
		private readonly IPurchaseOrderServiceService _inventoryService;
		private readonly IInventoryReceiveReportService _inventoryReportService;
		private readonly ISqlRepository _sqlRepository;

		//private readonly ISqlRepository _sqlRepository;

		public GateentryTokenController(
			IGateEntryService GateentryTokenService, ISqlRepository R)
		{
			_gateEntryService = GateentryTokenService;
			_sqlRepository = R;
		}


		public GateentryTokenController(
			 IGateEntryService gateEntryService
			, IMaterialRequsitionDetailsServiceService materialRequsitionDetailsServiceService
			, IPurchaseOrderDetailService inventoryDetailService
			, IPOMaterialService inventoryMaterialService
			, IInventoryReceiveReportService inventoryReportService
			, IPurchaseOrderServiceService inventoryService
			, IMaterialRequsitionMasterServiceService materialRequsitionMasterServiceService
			, ISqlRepository sqlRepository)
		{
			_gateEntryService = gateEntryService;
			_inventoryDetailService = inventoryDetailService;
			_inventoryMaterialService = inventoryMaterialService;
			_inventoryService = inventoryService;
			_inventoryReportService = inventoryReportService;
			_sqlRepository = sqlRepository;
			_materialRequsitionDetailsServiceService = materialRequsitionDetailsServiceService;
			_materialRequsitionMasterServiceService = materialRequsitionMasterServiceService;
		}

		#endregion Constructor

		#region Aplos

		public ActionResult Aplos()
		{
			return View();
		}

		public ActionResult GatePass()
		{
			return View();
		}
		public ActionResult InGatePass() 
		{
			return View();
		}
		public ActionResult InGatePassNoGeneration() 
		{
			return View();
		}
		
		[Authorize]
		public ActionResult GatePassChecked()
		{
			return View();
		}

		[Authorize]
		public ActionResult GatePassApproved()
		{
			return View();
		}
		//[Authorize]
		public ActionResult GatePassApprovedBySecurity()
		{
			return View();
		}
		public ActionResult GatePassEmployee()
		{
			return View();
		}
		//GateentryRegister For Report

		public ActionResult GateentryRegister()
		{
			return View();
		}
		public ActionResult InOutGatePass()
		{
			return View();
		}

		[Authorize]
		public ActionResult InOutGatePassCheck()
		{
			return View();
		}


		[Authorize]
		public ActionResult PendingGateoutList()
		{
			return View();
		}

		
		#endregion Aplos

		#region Gate Entry Code Start Here
		[HttpPost]
		public JsonResult Create(GateEntry entity, string PlantWiseGateId)
		{
			//try
			//{


			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			entity.CompanyGroupId = identity.CompanyGroupId;
			entity.CompanyId = identity.CompanyId;
			entity.PlantId = identity.PlantId;
			string time = entity.GateEntryTime.ToString();
			string resTime = " " + entity.GateEntryTime.ToString("hh:mm:ss tt");
			string aa = Convert.ToDateTime(entity.EntryDate).ToString("dd-MMM-yyyy") + resTime;
			string bb = DateTime.Now.ToString("dd-MMM-yyyy hh:mm:ss tt");

			if (Convert.ToDateTime(aa) > DateTime.Now)
			{
				throw new CustomException("Date & time can not grater than current date & time");
			}
			if (PlantWiseGateId == null || PlantWiseGateId == "")
			{
				throw new CustomException("Please select Gate!");

			}
			entity.GateEntryTime = Convert.ToDateTime(aa);
			entity.FlagStatus = "Ok";
			if (entity.GateEntryType == "Vendor")
			{
				entity.GateEntryType = "Vendor";
				entity.EmployeeIdForGateEntry = null;
				//entity.PartyCode = entity.PartyCode;
				if (entity.PartyId == null || entity.PartyId == "")
				{
					throw new CustomException("Please select vendor!");

				}

			}
			else if (entity.GateEntryType == "Employee")
			{
				entity.GateEntryType = "Employee";
				entity.PartyId = null;
				entity.EmployeeIdForGateEntry = entity.EmployeeIdForGateEntry;
				if (entity.EmployeeIdForGateEntry == null || entity.EmployeeIdForGateEntry == "")
				{
					throw new CustomException("Please select employee!");

				}
			}
			_gateEntryService.Insert(entity, PlantWiseGateId);
			return Json(new { entity, Message = AplosMessage.Success + " Gate Entry No <b>" + entity.Id + "</b>" });
			//         }
			//         catch(Exception ex)
			//{
			//             return Json(new { Error=true, Message = ex.ToString() });
			//         }
		}
		[HttpPost, Authorize]
		public JsonResult Edit(GateEntry entity)
		{
			//entity.GateEntryTime = Convert.ToDateTime(entity.EntryDate).ToString("dd-MMM-yyyy ") + Convert.ToDateTime(entity.GateEntryTime).ToString("hh:mm ");
			_gateEntryService.Update(entity);
			return Json(new { Message = AplosMessage.Updated });
		}

		[HttpPost, Authorize]
		public ActionResult Delete(string id)
		{
			if (!string.IsNullOrEmpty(id))
			{
				_gateEntryService.DeleteReq(id);
				return Json(new { Message = AplosMessage.Deleted });
			}
			else
				throw new CustomException(Resources.IdNotFound);
		}
		[HttpPost]
		public ActionResult DeleteGateEntry(string id)
		{
			if (!string.IsNullOrEmpty(id))
			{
				_gateEntryService.DeleteGateEntry(id);
				return Json(new { Message = AplosMessage.Deleted });
			}
			else
				throw new CustomException(Resources.IdNotFound);
		}
		[HttpPost]
		public ActionResult CancelGateEntry(string id)
		{
			if (!string.IsNullOrEmpty(id))
			{
				_gateEntryService.CancelGateEntry(id);
				return Json(new { Message = AplosMessage.Success });
			}
			else
				throw new CustomException(Resources.IdNotFound);
		}
		[Authorize, HttpPost, ChaildAction(ParentActionName = nameof(Delete))]
		public JsonResult ServiceChargesDelete(string serviceId)
		{
			_gateEntryService.Delete(serviceId);
			return Json(new { Message = AplosMessage.Deleted });
		}



		[Authorize, HttpGet]
		public JsonResult GetSupervisorCbo()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_materialRequsitionMasterServiceService.GetSupervisorCbo(), JsonRequestBehavior.AllowGet);
		}
		[Authorize, HttpGet]
		public JsonResult GetEntity()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_materialRequsitionMasterServiceService.GetEntity(), JsonRequestBehavior.AllowGet);
		}
		[Authorize, HttpGet]
		public JsonResult GetEmployee()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_materialRequsitionMasterServiceService.GetEmployee(), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpGet]
		public JsonResult GetAllReqdata(string IsSysAdmin, string UserId, string plantId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			IsSysAdmin = identity.IsSysAdmin.ToString();
			UserId = identity.UserId;
			plantId = identity.PlantId;
			var res = _gateEntryService.GetAllReqdata(IsSysAdmin, UserId, plantId);
			var jsondata = Json(res, JsonRequestBehavior.AllowGet);
			jsondata.MaxJsonLength = int.MaxValue;
			return jsondata;
			//return Json(_gateEntryService.GetAllReqdata(IsSysAdmin, UserId, plantId), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpGet]
		public JsonResult GetGateDetailDataById(string id)
		{
			//_materialRequsitionMasterServiceService
			return Json(_gateEntryService.GetReqMaster(id), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpPost]
		//string InventoryReceiveDetailId, string TransactionQty, string TransactionRate, string TrnAmount,string BaseTaxAmount,string BaseAmount,
		public JsonResult UpdateMaterial(IEnumerable<MaterialRequisitionDetailViewModel> entity, IEnumerable<PurchaseOrderTax> receiveTaxList)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			var ip = identity.IPAddress;
			var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
			var UpdatedBy = identity.Name;
			if (entity != null)
			{
				foreach (var item in entity)
				{
					if (item.TransactionQty == 0)
						throw new CustomException("Enter Qty!");
					if (item.TotalAmount == 0)
						throw new CustomException("Enter Amount!");
					//if (item.TransactionQty == 0)
					//    throw new CustomException("Please Input   Quantity !");
				}
			}
			//entity.CompanyGroupId = identity.CompanyGroupId;
			//entity.CompanyId = identity.CompanyId;
			//entity.PlantId = identity.PlantId;
			_materialRequsitionMasterServiceService.UpdateMaterial(entity, receiveTaxList);
			return Json(new { Message = AplosMessage.Updated });
		}




		[Authorize, HttpGet]

		public JsonResult PlantWiseGateCbo(string IsSysAdmin, string UserId, string plantId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			IsSysAdmin = identity.IsSysAdmin.ToString();
			UserId = identity.UserId;
			plantId = identity.PlantId;
			return Json(_gateEntryService.PlantWiseGateCbo(IsSysAdmin, UserId, plantId), JsonRequestBehavior.AllowGet);
		}

		#region GateEntryReport  Order Report 
		[HttpGet, Authorize]
		public ActionResult GateEntryReport(string GateEntryId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

			_gateEntryService.GateEntryReport(identity.CompanyGroupId, identity.PlantId, GateEntryId);

			return View();

		}
		#endregion


		#endregion




		#region Gate Pass system Code Start Here
		[Authorize, HttpGet]
		public JsonResult GetDepartmentByPlant(string DivisionId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

			try
			{
				var sql = "";

				sql = @"select distinct d.Id Value,d.UserName Text from employeeinformation EI
                            Left join [ORG].[Department] d on d.id=EI.DepartmentId
                            WHERE EI.DivisionId='" + DivisionId + "'";

				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
			// return Json(_gateEntryService.PlantWiseGateCbo(IsSysAdmin, UserId, plantId), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpGet]
		public JsonResult GetPlant(string IsSysAdmin, string UserId, string plantId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			IsSysAdmin = identity.IsSysAdmin.ToString();
			UserId = identity.UserId;
			plantId = identity.PlantId;
			try
			{
				var sql = "";

				//sql = @"select Id Value,UserName Text from org.Plant
				//                        WHERE CompanyId='" + identity.CompanyId + "'";
				sql = @"select Plant.Id Value ,Plant.UserName Text,C.UserName CompanyName  from org.Plant
									LEFT JOIN org.Company C On C.Id=Plant.CompanyId";

				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
			// return Json(_gateEntryService.PlantWiseGateCbo(IsSysAdmin, UserId, plantId), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpGet]
		public JsonResult GetUnit(string plantId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";

				sql = @"select distinct d.Id Value,d.UserName Text from employeeinformation EI
                    Left join [ORG].[Unit] d on d.id=EI.UnitId
                    WHERE EI.PlantId='" + plantId + "'";

				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
			// return Json(_gateEntryService.PlantWiseGateCbo(IsSysAdmin, UserId, plantId), JsonRequestBehavior.AllowGet);
		}
		[Authorize, HttpGet]
		public JsonResult GetDivision(string UnitId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";

				sql = @"select distinct d.Id Value,d.UserName Text from employeeinformation EI
                        Left join [ORG].[Division] d on d.id=EI.UnitId
                        WHERE EI.unitId='" + UnitId + "'";

				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
			// return Json(_gateEntryService.PlantWiseGateCbo(IsSysAdmin, UserId, plantId), JsonRequestBehavior.AllowGet);
		}
		[HttpGet, Authorize]
		public JsonResult EmployeeListByDepartment(GridParameter parameters, string DepartmentId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_gateEntryService.EmployeeListByDepartment(parameters, DepartmentId), JsonRequestBehavior.AllowGet);
		}
		[Authorize, HttpGet]
		public JsonResult GetSenderName(string UnitId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";

				sql = @"select EI.SystemId, EI.EmployeeName,c.UserName Company,p.UserName plantName from employeeinformation EI
							LEFT JOIN ORG.Company C ON C.Id=EI.CompanyId
							LEFT JOIN org.Plant p ON p.Id=EI.PlantId
                        WHERE SystemId='" + identity.EmployeeId + "'";

				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
			// return Json(_gateEntryService.PlantWiseGateCbo(IsSysAdmin, UserId, plantId), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpGet]
		public JsonResult GetBuyer()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";

				sql = @"select id Value,UserName Text from[HKP].[Buyer]";
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
			// return Json(_gateEntryService.PlantWiseGateCbo(IsSysAdmin, UserId, plantId), JsonRequestBehavior.AllowGet);
		}
		[HttpPost]
		public JsonResult CreateGatePass(GatePassMaster entity, string PlantWiseGateId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			entity.CompanyGroupId = identity.CompanyGroupId;
			//entity.CompanyId = identity.CompanyId;
			entity.PlantId = identity.PlantId;
			entity.GatePassStatus = entity.GatePassStatus1;
			entity.CheckedByStatus = "ForChecked";
			//if (entity.GateEntryType == "Vendor")
			//{
			//    entity.GateEntryType = "Vendor";
			//    entity.EmployeeIdForGateEntry = null;
			//    entity.PartyCode = entity.PartyCode;
			//    if (entity.PartyCode == null || entity.PartyCode == "")
			//    {
			//        throw new CustomException("Please select vendor!");

			//    }

			//}
			//else if (entity.GateEntryType == "Employee")
			//{
			//    entity.GateEntryType = "Employee";
			//    entity.PartyCode = null;
			//    entity.EmployeeIdForGateEntry = entity.EmployeeIdForGateEntry;
			//    if (entity.EmployeeIdForGateEntry == null || entity.EmployeeIdForGateEntry == "")
			//    {
			//        throw new CustomException("Please select employee!");

			//    }
			//}


			_gateEntryService.InsertGatePass(entity, PlantWiseGateId);
			return Json(new { entity, Message = AplosMessage.Success + " Gate Pass No <b>" + entity.Id + "</b>" });
		}
		[HttpPost]
		public JsonResult EditGatePass(GatePassMaster entity)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			entity.CompanyGroupId = identity.CompanyGroupId;
			//entity.CompanyId = identity.CompanyId;
			entity.PlantId = identity.PlantId;
			entity.GatePassStatus = entity.GatePassStatus1;
			entity.CheckedByStatus = "ForChecked";
			_gateEntryService.UpdateGatePass(entity);
			return Json(new { Message = AplosMessage.Updated });
		}
		[HttpPost]
		public ActionResult DeleteGatePass(string id)
		{
			if (!string.IsNullOrEmpty(id))
			{
				_gateEntryService.DeleteGatePass(id);
				return Json(new { Message = AplosMessage.Deleted });
			}
			else
				throw new CustomException(Resources.IdNotFound);
		}

		[Authorize, HttpGet]
		public JsonResult GetIndexGridDataList(string ReqStatus,string GateRegisterType)
		{
			try
			{
				var sql = "";
				if (ReqStatus == "1")//Created List
				{
					sql = @"
                        SELECT top(500)   GPM.[Id]
                              ,GPM.[CompanyGroupId]
                              --,GPM.[CompanyId]
                              ,GPM.[PlantId]
                              ,GPM.[GatePassType]
                              ,GPM.[GatePassStatus] GatePassStatus1
                              ,REPLACE(CONVERT(CHAR(11), GPM.[ReturnableDate], 106),' ','-') AS ReturnableDate
                              ,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
                              ,GPM.[FromEmployeeId]
	                          ,EI.EmployeeName SenderName
                              ,GPM.[Through]
                              ,GPM.[CourierName]
                              ,GPM.[RunnerEmployeeId]
	                          ,EI1.EmployeeName RunnerEmployee
                              ,GPM.[ToType]
                              ,GPM.[ToPartyCode]
	                          ,p.UserName Vendor
                              ,GPM.[ToBuyerId]
	                          ,BUYer.UserName BuyerName
                              ,GPM.[ToPlantId]
	                          ,Plant.UserName PlantName
                              ,GPM.[ToUnitId]
	                          ,Unit.UserName UnitName
                              ,GPM.[ToDivisionId]
	                          ,Division.UserName DivisionName
                              ,GPM.[ToDepartment]
	                          ,Department.UserName DepartmentName
                              ,GPM.[DepartmentEmployeeId]
	                          ,EI2.EmployeeName DepartmentEmployee
                              ,GPM.[OtherCompanyName]
                              ,GPM.[PersonName]
                              ,GPM.[MobileNo]
                              ,GPM.[Address]
                              ,GPM.[Remarks]
                              ,GPM.[CheckedBy]
                              ,EI5.EmployeeName CheckedByEmployee                           
                              ,GPM.[CheckedByStatus]
                              ,GPM.[CheckedHoldRejectReason]
                              ,GPM.[ApprovedBy]
                              ,EI6.EmployeeName ApprovedByEmployee      
                              ,GPM.[ApprovedByStatus]
                              ,GPM.[ApprovedHoldRejectReason]
                              ,GPM.[SenderSecurityEmployeeId]
	                           ,EI3.EmployeeName SenderSecurityEmployee
                              ,GPM.[SenderSecurityApprovedStatus]
                              ,GPM.[ReceiverSecurityEmployeeId]
	                          ,EI4.EmployeeName ReceiverSecurityEmployee
                              ,GPM.[ReceiverSecurityApprovedStatus]
                              ,GPM.[VendorBuyerOtherCompanyReceivedStatus]
                              ,GPM.[AddedBy]
                              ,GPM.[AddedDate]
                              ,GPM.[AddedFromIP]
                              ,GPM.[UpdatedBy]
                              ,GPM.[UpdatedDate]
                              ,GPM.[UpdatedFromIP],GPM.ChallanNo,GPM.GateRegisterType,GPM.PurposeofGatePass,GPM.ConsignmentNo,GPM.DriverName,GPM.InvoiceNo,GPM.InvoiceValue,GPM.TransportAgentName,GPM.TransportAgentMobileNo,GPM.TransportAgentMobileNo,GPM.VehicleNo,GPM.NoofPackages
                          FROM [TRN].[GatePassMaster] GPM
                          LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.FromEmployeeId
                          LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.RunnerEmployeeId
                          LEft JOIN hkp.Party P On p.Id=GPM.ToPartyCode
                          LEft JOIN [HKP].[Buyer] BUYer On BUYer.Id=GPM.ToBuyerId
                          LEFT JOIN org.Plant Plant ON Plant.id=GPM.ToPlantId
                          LEFT JOIN org.Unit Unit ON Unit.id=GPM.ToUnitId
                          LEFT JOIN org.Division Division ON Division.id=GPM.ToDivisionId
                          LEFT JOIN org.Department Department ON Department.id=GPM.ToDepartment
                          LEFT JOIN Employeeinformation EI2 on EI2.SystemId= GPM.DepartmentEmployeeId
                          LEFT JOIN Employeeinformation EI3 on EI3.SystemId= GPM.SenderSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI4 on EI4.SystemId= GPM.ReceiverSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI5 on EI5.SystemId= GPM.CheckedBy
                          LEFT JOIN Employeeinformation EI6 on EI6.SystemId= GPM.ApprovedBy
                          Where GPM.CheckedByStatus='ForChecked' AND GPM.GateRegisterType='" + GateRegisterType + @"'   Order By GPM.[GatePassEntryDate] DESC";
				}
				if (ReqStatus == "2")//Checked Hold/Reject
				{
					sql = @"
                         SELECT top(500)  GPM.[Id]
                              ,GPM.[CompanyGroupId]
                              --,GPM.[CompanyId]
                              ,GPM.[PlantId]
                              ,GPM.[GatePassType]
                              ,GPM.[GatePassStatus] GatePassStatus1
                              ,REPLACE(CONVERT(CHAR(11), GPM.[ReturnableDate], 106),' ','-') AS ReturnableDate
                              ,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
                              ,GPM.[FromEmployeeId]
	                          ,EI.EmployeeName SenderName
                              ,GPM.[Through]
                              ,GPM.[CourierName]
                              ,GPM.[RunnerEmployeeId]
	                          ,EI1.EmployeeName RunnerEmployee
                              ,GPM.[ToType]
                              ,GPM.[ToPartyCode]
	                          ,p.UserName Vendor
                              ,GPM.[ToBuyerId]
	                          ,BUYer.UserName BuyerName
                              ,GPM.[ToPlantId]
	                          ,Plant.UserName PlantName
                              ,GPM.[ToUnitId]
	                          ,Unit.UserName UnitName
                              ,GPM.[ToDivisionId]
	                          ,Division.UserName DivisionName
                              ,GPM.[ToDepartment]
	                          ,Department.UserName DepartmentName
                              ,GPM.[DepartmentEmployeeId]
	                          ,EI2.EmployeeName DepartmentEmployee
                              ,GPM.[OtherCompanyName]
                              ,GPM.[PersonName]
                              ,GPM.[MobileNo]
                              ,GPM.[Address]
                              ,GPM.[Remarks]
                              ,GPM.[CheckedBy]
                              ,EI5.EmployeeName CheckedByEmployee                           
                              ,GPM.[CheckedByStatus]
                              ,GPM.[CheckedHoldRejectReason]
                              ,GPM.[ApprovedBy]
                              ,EI6.EmployeeName ApprovedByEmployee      
                              ,GPM.[ApprovedByStatus]
                              ,GPM.[ApprovedHoldRejectReason]
                              ,GPM.[SenderSecurityEmployeeId]
	                           ,EI3.EmployeeName SenderSecurityEmployee
                              ,GPM.[SenderSecurityApprovedStatus]
                              ,GPM.[ReceiverSecurityEmployeeId]
	                          ,EI4.EmployeeName ReceiverSecurityEmployee
                              ,GPM.[ReceiverSecurityApprovedStatus]
                              ,GPM.[VendorBuyerOtherCompanyReceivedStatus]
                              ,GPM.[AddedBy]
                              ,GPM.[AddedDate]
                              ,GPM.[AddedFromIP]
                              ,GPM.[UpdatedBy]
                              ,GPM.[UpdatedDate]
                              ,GPM.[UpdatedFromIP],GPM.ChallanNo,GPM.GateRegisterType,GPM.PurposeofGatePass,GPM.ConsignmentNo,GPM.DriverName,GPM.InvoiceNo,GPM.InvoiceValue,GPM.TransportAgentName,GPM.TransportAgentMobileNo,GPM.TransportAgentMobileNo,GPM.VehicleNo,GPM.NoofPackages
                          FROM [TRN].[GatePassMaster] GPM
                          LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.FromEmployeeId
                          LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.RunnerEmployeeId
                          LEft JOIN hkp.Party P On p.Id=GPM.ToPartyCode
                          LEft JOIN [HKP].[Buyer] BUYer On BUYer.Id=GPM.ToBuyerId
                          LEFT JOIN org.Plant Plant ON Plant.id=GPM.ToPlantId
                          LEFT JOIN org.Unit Unit ON Unit.id=GPM.ToUnitId
                          LEFT JOIN org.Division Division ON Division.id=GPM.ToDivisionId
                          LEFT JOIN org.Department Department ON Department.id=GPM.ToDepartment
                          LEFT JOIN Employeeinformation EI2 on EI2.SystemId= GPM.DepartmentEmployeeId
                          LEFT JOIN Employeeinformation EI3 on EI3.SystemId= GPM.SenderSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI4 on EI4.SystemId= GPM.ReceiverSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI5 on EI5.SystemId= GPM.CheckedBy
                          LEFT JOIN Employeeinformation EI6 on EI6.SystemId= GPM.ApprovedBy
                          Where (GPM.CheckedByStatus='Hold' or GPM.CheckedByStatus='Reject') AND GPM.GateRegisterType='" + GateRegisterType + @"' Order By GPM.[Id] DESC";
				}
				if (ReqStatus == "3")//Checked
				{
					sql = @"
                         SELECT top(500)  GPM.[Id]
                              ,GPM.[CompanyGroupId]
                              --,GPM.[CompanyId]
                              ,GPM.[PlantId]
                              ,GPM.[GatePassType]
                              ,GPM.[GatePassStatus] GatePassStatus1
                              ,REPLACE(CONVERT(CHAR(11), GPM.[ReturnableDate], 106),' ','-') AS ReturnableDate
                              ,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
                              ,GPM.[FromEmployeeId]
	                          ,EI.EmployeeName SenderName
                              ,GPM.[Through]
                              ,GPM.[CourierName]
                              ,GPM.[RunnerEmployeeId]
	                          ,EI1.EmployeeName RunnerEmployee
                              ,GPM.[ToType]
                              ,GPM.[ToPartyCode]
	                          ,p.UserName Vendor
                              ,GPM.[ToBuyerId]
	                          ,BUYer.UserName BuyerName
                              ,GPM.[ToPlantId]
	                          ,Plant.UserName PlantName
                              ,GPM.[ToUnitId]
	                          ,Unit.UserName UnitName
                              ,GPM.[ToDivisionId]
	                          ,Division.UserName DivisionName
                              ,GPM.[ToDepartment]
	                          ,Department.UserName DepartmentName
                              ,GPM.[DepartmentEmployeeId]
	                          ,EI2.EmployeeName DepartmentEmployee
                              ,GPM.[OtherCompanyName]
                              ,GPM.[PersonName]
                              ,GPM.[MobileNo]
                              ,GPM.[Address]
                              ,GPM.[Remarks]
                              ,GPM.[CheckedBy]
                              ,EI5.EmployeeName CheckedByEmployee                           
                              ,GPM.[CheckedByStatus]
                              ,GPM.[CheckedHoldRejectReason]
                              ,GPM.[ApprovedBy]
                              ,EI6.EmployeeName ApprovedByEmployee      
                              ,GPM.[ApprovedByStatus]
                              ,GPM.[ApprovedHoldRejectReason]
                              ,GPM.[SenderSecurityEmployeeId]
	                           ,EI3.EmployeeName SenderSecurityEmployee
                              ,GPM.[SenderSecurityApprovedStatus]
                              ,GPM.[ReceiverSecurityEmployeeId]
	                          ,EI4.EmployeeName ReceiverSecurityEmployee
                              ,GPM.[ReceiverSecurityApprovedStatus]
                              ,GPM.[VendorBuyerOtherCompanyReceivedStatus]
                              ,GPM.[AddedBy]
                              ,GPM.[AddedDate]
                              ,GPM.[AddedFromIP]
                              ,GPM.[UpdatedBy]
                              ,GPM.[UpdatedDate]
                              ,GPM.[UpdatedFromIP],GPM.ChallanNo,GPM.GateRegisterType,GPM.PurposeofGatePass,GPM.ConsignmentNo,GPM.DriverName,GPM.InvoiceNo,GPM.InvoiceValue,GPM.TransportAgentName,GPM.TransportAgentMobileNo,GPM.TransportAgentMobileNo,GPM.VehicleNo,GPM.NoofPackages
                          FROM [TRN].[GatePassMaster] GPM
                          LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.FromEmployeeId
                          LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.RunnerEmployeeId
                          LEft JOIN hkp.Party P On p.Id=GPM.ToPartyCode
                          LEft JOIN [HKP].[Buyer] BUYer On BUYer.Id=GPM.ToBuyerId
                          LEFT JOIN org.Plant Plant ON Plant.id=GPM.ToPlantId
                          LEFT JOIN org.Unit Unit ON Unit.id=GPM.ToUnitId
                          LEFT JOIN org.Division Division ON Division.id=GPM.ToDivisionId
                          LEFT JOIN org.Department Department ON Department.id=GPM.ToDepartment
                          LEFT JOIN Employeeinformation EI2 on EI2.SystemId= GPM.DepartmentEmployeeId
                          LEFT JOIN Employeeinformation EI3 on EI3.SystemId= GPM.SenderSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI4 on EI4.SystemId= GPM.ReceiverSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI5 on EI5.SystemId= GPM.CheckedBy
                          LEFT JOIN Employeeinformation EI6 on EI6.SystemId= GPM.ApprovedBy
                          Where GPM.CheckedByStatus='Checked' AND ApprovedByStatus='For Approval' AND GPM.GateRegisterType='" + GateRegisterType + @"' Order By GPM.[Id] DESC";
				}
				if (ReqStatus == "4")
				{
					sql = @"
                        SELECT  GPM.[Id]
                              ,GPM.[CompanyGroupId]
                              --,GPM.[CompanyId]
                              ,GPM.[PlantId]
                              ,GPM.[GatePassType]
                              ,GPM.[GatePassStatus] GatePassStatus1
                              ,REPLACE(CONVERT(CHAR(11), GPM.[ReturnableDate], 106),' ','-') AS ReturnableDate
                              ,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
                              ,GPM.[FromEmployeeId]
	                          ,EI.EmployeeName SenderName
                              ,GPM.[Through]
                              ,GPM.[CourierName]
                              ,GPM.[RunnerEmployeeId]
	                          ,EI1.EmployeeName RunnerEmployee
                              ,GPM.[ToType]
                              ,GPM.[ToPartyCode]
	                          ,p.UserName Vendor
                              ,GPM.[ToBuyerId]
	                          ,BUYer.UserName BuyerName
                              ,GPM.[ToPlantId]
	                          ,Plant.UserName PlantName
                              ,GPM.[ToUnitId]
	                          ,Unit.UserName UnitName
                              ,GPM.[ToDivisionId]
	                          ,Division.UserName DivisionName
                              ,GPM.[ToDepartment]
	                          ,Department.UserName DepartmentName
                              ,GPM.[DepartmentEmployeeId]
	                          ,EI2.EmployeeName DepartmentEmployee
                              ,GPM.[OtherCompanyName]
                              ,GPM.[PersonName]
                              ,GPM.[MobileNo]
                              ,GPM.[Address]
                              ,GPM.[Remarks]
                              ,GPM.[CheckedBy]
                              ,EI5.EmployeeName CheckedByEmployee                           
                              ,GPM.[CheckedByStatus]
                              ,GPM.[CheckedHoldRejectReason]
                              ,GPM.[ApprovedBy]
                              ,EI6.EmployeeName ApprovedByEmployee      
                              ,GPM.[ApprovedByStatus]
                              ,GPM.[ApprovedHoldRejectReason]
                              ,GPM.[SenderSecurityEmployeeId]
	                           ,EI3.EmployeeName SenderSecurityEmployee
                              ,GPM.[SenderSecurityApprovedStatus]
                              ,GPM.[ReceiverSecurityEmployeeId]
	                          ,EI4.EmployeeName ReceiverSecurityEmployee
                              ,GPM.[ReceiverSecurityApprovedStatus]
                              ,GPM.[VendorBuyerOtherCompanyReceivedStatus]
                              ,GPM.[AddedBy]
                              ,GPM.[AddedDate]
                              ,GPM.[AddedFromIP]
                              ,GPM.[UpdatedBy]
                              ,GPM.[UpdatedDate]
                              ,GPM.[UpdatedFromIP],GPM.ChallanNo,GPM.GateRegisterType,GPM.PurposeofGatePass,GPM.ConsignmentNo,GPM.DriverName,GPM.InvoiceNo,GPM.InvoiceValue,GPM.TransportAgentName,GPM.TransportAgentMobileNo,GPM.TransportAgentMobileNo,GPM.VehicleNo,GPM.NoofPackages
                          FROM [TRN].[GatePassMaster] GPM
                          LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.FromEmployeeId
                          LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.RunnerEmployeeId
                          LEft JOIN hkp.Party P On p.Id=GPM.ToPartyCode
                          LEft JOIN [HKP].[Buyer] BUYer On BUYer.Id=GPM.ToBuyerId
                          LEFT JOIN org.Plant Plant ON Plant.id=GPM.ToPlantId
                          LEFT JOIN org.Unit Unit ON Unit.id=GPM.ToUnitId
                          LEFT JOIN org.Division Division ON Division.id=GPM.ToDivisionId
                          LEFT JOIN org.Department Department ON Department.id=GPM.ToDepartment
                          LEFT JOIN Employeeinformation EI2 on EI2.SystemId= GPM.DepartmentEmployeeId
                          LEFT JOIN Employeeinformation EI3 on EI3.SystemId= GPM.SenderSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI4 on EI4.SystemId= GPM.ReceiverSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI5 on EI5.SystemId= GPM.CheckedBy
                          LEFT JOIN Employeeinformation EI6 on EI6.SystemId= GPM.ApprovedBy
                          Where GPM.CheckedByStatus='Checked' AND (ApprovedByStatus='Hold' or ApprovedByStatus='Reject') AND GPM.GateRegisterType='" + GateRegisterType + @"'  Order By GPM.[Id] DESC";
				}
				if (ReqStatus == "5")
				{
					sql = @"
                        SELECT  GPM.[Id]
                              ,GPM.[CompanyGroupId]
                              --,GPM.[CompanyId]
                              ,GPM.[PlantId]
                              ,GPM.[GatePassType]
                              ,GPM.[GatePassStatus] GatePassStatus1
                              ,REPLACE(CONVERT(CHAR(11), GPM.[ReturnableDate], 106),' ','-') AS ReturnableDate
                              ,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
                              ,GPM.[FromEmployeeId]
	                          ,EI.EmployeeName SenderName
                              ,GPM.[Through]
                              ,GPM.[CourierName]
                              ,GPM.[RunnerEmployeeId]
	                          ,EI1.EmployeeName RunnerEmployee
                              ,GPM.[ToType]
                              ,GPM.[ToPartyCode]
	                          ,p.UserName Vendor
                              ,GPM.[ToBuyerId]
	                          ,BUYer.UserName BuyerName
                              ,GPM.[ToPlantId]
	                          ,Plant.UserName PlantName
                              ,GPM.[ToUnitId]
	                          ,Unit.UserName UnitName
                              ,GPM.[ToDivisionId]
	                          ,Division.UserName DivisionName
                              ,GPM.[ToDepartment]
	                          ,Department.UserName DepartmentName
                              ,GPM.[DepartmentEmployeeId]
	                          ,EI2.EmployeeName DepartmentEmployee
                              ,GPM.[OtherCompanyName]
                              ,GPM.[PersonName]
                              ,GPM.[MobileNo]
                              ,GPM.[Address]
                              ,GPM.[Remarks]
                              ,GPM.[CheckedBy]
                              ,EI5.EmployeeName CheckedByEmployee                           
                              ,GPM.[CheckedByStatus]
                              ,GPM.[CheckedHoldRejectReason]
                              ,GPM.[ApprovedBy]
                              ,EI6.EmployeeName ApprovedByEmployee      
                              ,GPM.[ApprovedByStatus]
                              ,GPM.[ApprovedHoldRejectReason]
                              ,GPM.[SenderSecurityEmployeeId]
	                           ,EI3.EmployeeName SenderSecurityEmployee
                              ,GPM.[SenderSecurityApprovedStatus]
                              ,GPM.[ReceiverSecurityEmployeeId]
	                          ,EI4.EmployeeName ReceiverSecurityEmployee
                              ,GPM.[ReceiverSecurityApprovedStatus]
                              ,GPM.[VendorBuyerOtherCompanyReceivedStatus]
                              ,GPM.[AddedBy]
                              ,GPM.[AddedDate]
                              ,GPM.[AddedFromIP]
                              ,GPM.[UpdatedBy]
                              ,GPM.[UpdatedDate]
                              ,GPM.[UpdatedFromIP],GPM.ChallanNo,GPM.GateRegisterType,GPM.PurposeofGatePass,GPM.ConsignmentNo,GPM.DriverName,GPM.InvoiceNo,GPM.InvoiceValue,GPM.TransportAgentName,GPM.TransportAgentMobileNo,GPM.TransportAgentMobileNo,GPM.VehicleNo,GPM.NoofPackages
                          FROM [TRN].[GatePassMaster] GPM
                          LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.FromEmployeeId
                          LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.RunnerEmployeeId
                          LEft JOIN hkp.Party P On p.Id=GPM.ToPartyCode
                          LEft JOIN [HKP].[Buyer] BUYer On BUYer.Id=GPM.ToBuyerId
                          LEFT JOIN org.Plant Plant ON Plant.id=GPM.ToPlantId
                          LEFT JOIN org.Unit Unit ON Unit.id=GPM.ToUnitId
                          LEFT JOIN org.Division Division ON Division.id=GPM.ToDivisionId
                          LEFT JOIN org.Department Department ON Department.id=GPM.ToDepartment
                          LEFT JOIN Employeeinformation EI2 on EI2.SystemId= GPM.DepartmentEmployeeId
                          LEFT JOIN Employeeinformation EI3 on EI3.SystemId= GPM.SenderSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI4 on EI4.SystemId= GPM.ReceiverSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI5 on EI5.SystemId= GPM.CheckedBy
                          LEFT JOIN Employeeinformation EI6 on EI6.SystemId= GPM.ApprovedBy
                          Where GPM.CheckedByStatus='Checked' AND ApprovedByStatus='Approved'  AND GPM.GateRegisterType='" + GateRegisterType + @"' Order By GPM.[Id] DESC";
				}
				if (ReqStatus == "6")
				{
					sql = @"
                        SELECT  GPM.[Id]
                              ,GPM.[CompanyGroupId]
                              --,GPM.[CompanyId]
                              ,GPM.[PlantId]
                              ,GPM.[GatePassType]
                              ,GPM.[GatePassStatus] GatePassStatus1
                              ,REPLACE(CONVERT(CHAR(11), GPM.[ReturnableDate], 106),' ','-') AS ReturnableDate
                              ,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
                              ,GPM.[FromEmployeeId]
	                          ,EI.EmployeeName SenderName
                              ,GPM.[Through]
                              ,GPM.[CourierName]
                              ,GPM.[RunnerEmployeeId]
	                          ,EI1.EmployeeName RunnerEmployee
                              ,GPM.[ToType]
                              ,GPM.[ToPartyCode]
	                          ,p.UserName Vendor
                              ,GPM.[ToBuyerId]
	                          ,BUYer.UserName BuyerName
                              ,GPM.[ToPlantId]
	                          ,Plant.UserName PlantName
                              ,GPM.[ToUnitId]
	                          ,Unit.UserName UnitName
                              ,GPM.[ToDivisionId]
	                          ,Division.UserName DivisionName
                              ,GPM.[ToDepartment]
	                          ,Department.UserName DepartmentName
                              ,GPM.[DepartmentEmployeeId]
	                          ,EI2.EmployeeName DepartmentEmployee
                              ,GPM.[OtherCompanyName]
                              ,GPM.[PersonName]
                              ,GPM.[MobileNo]
                              ,GPM.[Address]
                              ,GPM.[Remarks]
                              ,GPM.[CheckedBy]
                              ,EI5.EmployeeName CheckedByEmployee                           
                              ,GPM.[CheckedByStatus]
                              ,GPM.[CheckedHoldRejectReason]
                              ,GPM.[ApprovedBy]
                              ,EI6.EmployeeName ApprovedByEmployee      
                              ,GPM.[ApprovedByStatus]
                              ,GPM.[ApprovedHoldRejectReason]
                              ,GPM.[SenderSecurityEmployeeId]
	                           ,EI3.EmployeeName SenderSecurityEmployee
                              ,GPM.[SenderSecurityApprovedStatus]
                              ,GPM.[ReceiverSecurityEmployeeId]
	                          ,EI4.EmployeeName ReceiverSecurityEmployee
                              ,GPM.[ReceiverSecurityApprovedStatus]
                              ,GPM.[VendorBuyerOtherCompanyReceivedStatus]
                              ,GPM.[AddedBy]
                              ,GPM.[AddedDate]
                              ,GPM.[AddedFromIP]
                              ,GPM.[UpdatedBy]
                              ,GPM.[UpdatedDate]
                              ,GPM.[UpdatedFromIP],GPM.ChallanNo,GPM.GateRegisterType,GPM.PurposeofGatePass,GPM.ConsignmentNo,GPM.DriverName,GPM.InvoiceNo,GPM.InvoiceValue,GPM.TransportAgentName,GPM.TransportAgentMobileNo,GPM.TransportAgentMobileNo,GPM.VehicleNo,GPM.NoofPackages
                          FROM [TRN].[GatePassMaster] GPM
                          LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.FromEmployeeId
                          LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.RunnerEmployeeId
                          LEft JOIN hkp.Party P On p.Id=GPM.ToPartyCode
                          LEft JOIN [HKP].[Buyer] BUYer On BUYer.Id=GPM.ToBuyerId
                          LEFT JOIN org.Plant Plant ON Plant.id=GPM.ToPlantId
                          LEFT JOIN org.Unit Unit ON Unit.id=GPM.ToUnitId
                          LEFT JOIN org.Division Division ON Division.id=GPM.ToDivisionId
                          LEFT JOIN org.Department Department ON Department.id=GPM.ToDepartment
                          LEFT JOIN Employeeinformation EI2 on EI2.SystemId= GPM.DepartmentEmployeeId
                          LEFT JOIN Employeeinformation EI3 on EI3.SystemId= GPM.SenderSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI4 on EI4.SystemId= GPM.ReceiverSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI5 on EI5.SystemId= GPM.CheckedBy
                          LEFT JOIN Employeeinformation EI6 on EI6.SystemId= GPM.ApprovedBy
                          Where GPM.CheckedByStatus='Checked' AND ApprovedByStatus='Approved'
						  AND SenderSecurityApprovedStatus='GateOut' and GateOutStatus=1 AND GPM.GateRegisterType='" + GateRegisterType + @"'
						  Order By GPM.[Id] DESC";

				}
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
			// return Json(_gateEntryService.PlantWiseGateCbo(IsSysAdmin, UserId, plantId), JsonRequestBehavior.AllowGet);
		}
		[Authorize, HttpGet]
		public JsonResult GetIndexGridDataListForInEntry(string ReqStatus, string GateRegisterType) 
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";
				if (ReqStatus == "1")//Created List 
				{
					sql = @"
                        SELECT  GPM.[Id]
							,GPM.[CompanyGroupId]
							--,GPM.[CompanyId]
							,GPM.[PlantId]
							,GPM.[GatePassType]
							,GPM.[GatePassStatus] GatePassStatus1
							,REPLACE(CONVERT(CHAR(11), GPM.[ReturnableDate], 106),' ','-') AS ReturnableDate
							,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
							,GPM.[FromEmployeeId]
							,GPM.[Through]
							,GPM.[CourierName]
							,GPM.[RunnerEmployeeId]
							,GPM.[ToType]
							,GPM.[ToPartyCode]
							,GPM.[ToBuyerId]
							,GPM.[ToPlantId]
							,GPM.[ToUnitId]
							,GPM.[ToDivisionId]
							,GPM.[ToDepartment]
							,GPM.[DepartmentEmployeeId]
							,GPM.[OtherCompanyName]
							,GPM.[PersonName]
							,GPM.[MobileNo]
							,GPM.[Address]
							,GPM.[Remarks]
							,GPM.[CheckedBy]
							,GPM.[CheckedByStatus]
							,GPM.[CheckedHoldRejectReason]
							,GPM.[ApprovedBy]
							,GPM.[ApprovedByStatus]
							,GPM.[ApprovedHoldRejectReason]
							,GPM.[SenderSecurityEmployeeId]
							,GPM.[SenderSecurityApprovedStatus]
							,GPM.[ReceiverSecurityEmployeeId]
							,GPM.[ReceiverSecurityApprovedStatus]
							,GPM.[VendorBuyerOtherCompanyReceivedStatus]
							,GPM.[AddedBy]
							,GPM.[AddedDate]
							,GPM.[AddedFromIP]
							,GPM.[UpdatedBy]
							,GPM.[UpdatedDate]
							,GPM.[UpdatedFromIP],GPM.ChallanNo,GPM.GateRegisterType,GPM.InvoiceNo,GPM.InvoiceValue,GPM.ReceivedChallanNO,GPM.PurposeofGatePass,GPM.ConsignmentNo,GPM.DriverName,GPM.InvoiceNo,GPM.InvoiceValue,GPM.InvoiceValue,GPM.TransportAgentName,GPM.TransportAgentMobileNo,GPM.TransportAgentMobileNo,GPM.VehicleNo,GPM.NoofPackages
						FROM [TRN].[GatePassMaster] GPM
                          Where GPM.CheckedByStatus='ForChecked' AND GPM.GateRegisterType='" + GateRegisterType + @"' Order By GPM.[Id] DESC";
				}
				//if (ReqStatus == "2")//Checked Hold/Reject
				//{
				//	sql = @"
    //                    SELECT  GPM.[Id]
    //                          ,GPM.[CompanyGroupId]
    //                          --,GPM.[CompanyId]
    //                          ,GPM.[PlantId]
    //                          ,GPM.[GatePassType]
    //                          ,GPM.[GatePassStatus] GatePassStatus1
    //                          ,REPLACE(CONVERT(CHAR(11), GPM.[ReturnableDate], 106),' ','-') AS ReturnableDate
    //                          ,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
    //                          ,GPM.[FromEmployeeId]
	   //                       ,EI.EmployeeName SenderName
    //                          ,GPM.[Through]
    //                          ,GPM.[CourierName]
    //                          ,GPM.[RunnerEmployeeId]
	   //                       ,EI1.EmployeeName RunnerEmployee
    //                          ,GPM.[ToType]
    //                          ,GPM.[ToPartyCode]
	   //                       ,p.UserName Vendor
    //                          ,GPM.[ToBuyerId]
	   //                       ,BUYer.UserName BuyerName
    //                          ,GPM.[ToPlantId]
	   //                       ,Plant.UserName PlantName
    //                          ,GPM.[ToUnitId]
	   //                       ,Unit.UserName UnitName
    //                          ,GPM.[ToDivisionId]
	   //                       ,Division.UserName DivisionName
    //                          ,GPM.[ToDepartment]
	   //                       ,Department.UserName DepartmentName
    //                          ,GPM.[DepartmentEmployeeId]
	   //                       ,EI2.EmployeeName DepartmentEmployee
    //                          ,GPM.[OtherCompanyName]
    //                          ,GPM.[PersonName]
    //                          ,GPM.[MobileNo]
    //                          ,GPM.[Address]
    //                          ,GPM.[Remarks]
    //                          ,GPM.[CheckedBy]
    //                          ,EI5.EmployeeName CheckedByEmployee                           
    //                          ,GPM.[CheckedByStatus]
    //                          ,GPM.[CheckedHoldRejectReason]
    //                          ,GPM.[ApprovedBy]
    //                          ,EI6.EmployeeName ApprovedByEmployee      
    //                          ,GPM.[ApprovedByStatus]
    //                          ,GPM.[ApprovedHoldRejectReason]
    //                          ,GPM.[SenderSecurityEmployeeId]
	   //                        ,EI3.EmployeeName SenderSecurityEmployee
    //                          ,GPM.[SenderSecurityApprovedStatus]
    //                          ,GPM.[ReceiverSecurityEmployeeId]
	   //                       ,EI4.EmployeeName ReceiverSecurityEmployee
    //                          ,GPM.[ReceiverSecurityApprovedStatus]
    //                          ,GPM.[VendorBuyerOtherCompanyReceivedStatus]
    //                          ,GPM.[AddedBy]
    //                          ,GPM.[AddedDate]
    //                          ,GPM.[AddedFromIP]
    //                          ,GPM.[UpdatedBy]
    //                          ,GPM.[UpdatedDate]
    //                          ,GPM.[UpdatedFromIP],GPM.ChallanNo
    //                      FROM [TRN].[GatePassMaster] GPM
    //                      LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.FromEmployeeId
    //                      LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.RunnerEmployeeId
    //                      LEft JOIN hkp.Party P On p.Id=GPM.ToPartyCode
    //                      LEft JOIN [HKP].[Buyer] BUYer On BUYer.Id=GPM.ToBuyerId
    //                      LEFT JOIN org.Plant Plant ON Plant.id=GPM.ToPlantId
    //                      LEFT JOIN org.Unit Unit ON Unit.id=GPM.ToUnitId
    //                      LEFT JOIN org.Division Division ON Division.id=GPM.ToDivisionId
    //                      LEFT JOIN org.Department Department ON Department.id=GPM.ToDepartment
    //                      LEFT JOIN Employeeinformation EI2 on EI2.SystemId= GPM.DepartmentEmployeeId
    //                      LEFT JOIN Employeeinformation EI3 on EI3.SystemId= GPM.SenderSecurityEmployeeId
    //                      LEFT JOIN Employeeinformation EI4 on EI4.SystemId= GPM.ReceiverSecurityEmployeeId
    //                      LEFT JOIN Employeeinformation EI5 on EI5.SystemId= GPM.CheckedBy
    //                      LEFT JOIN Employeeinformation EI6 on EI6.SystemId= GPM.ApprovedBy
    //                      Where GPM.FromEmployeeId='" + identity.EmployeeId + @"' 
				//		  AND (GPM.CheckedByStatus='Hold' || GPM.CheckedByStatus='Reject') AND GPM.GateRegisterType='" + GateRegisterType + @"' Order By GPM.[Id] DESC";
				//}
				//if (ReqStatus == "3")//Checked
				//{
				//	sql = @"
    //                    SELECT  GPM.[Id]
    //                          ,GPM.[CompanyGroupId]
    //                          --,GPM.[CompanyId]
    //                          ,GPM.[PlantId]
    //                          ,GPM.[GatePassType]
    //                          ,GPM.[GatePassStatus] GatePassStatus1
    //                          ,REPLACE(CONVERT(CHAR(11), GPM.[ReturnableDate], 106),' ','-') AS ReturnableDate
    //                          ,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
    //                          ,GPM.[FromEmployeeId]
	   //                       ,EI.EmployeeName SenderName
    //                          ,GPM.[Through]
    //                          ,GPM.[CourierName]
    //                          ,GPM.[RunnerEmployeeId]
	   //                       ,EI1.EmployeeName RunnerEmployee
    //                          ,GPM.[ToType]
    //                          ,GPM.[ToPartyCode]
	   //                       ,p.UserName Vendor
    //                          ,GPM.[ToBuyerId]
	   //                       ,BUYer.UserName BuyerName
    //                          ,GPM.[ToPlantId]
	   //                       ,Plant.UserName PlantName
    //                          ,GPM.[ToUnitId]
	   //                       ,Unit.UserName UnitName
    //                          ,GPM.[ToDivisionId]
	   //                       ,Division.UserName DivisionName
    //                          ,GPM.[ToDepartment]
	   //                       ,Department.UserName DepartmentName
    //                          ,GPM.[DepartmentEmployeeId]
	   //                       ,EI2.EmployeeName DepartmentEmployee
    //                          ,GPM.[OtherCompanyName]
    //                          ,GPM.[PersonName]
    //                          ,GPM.[MobileNo]
    //                          ,GPM.[Address]
    //                          ,GPM.[Remarks]
    //                          ,GPM.[CheckedBy]
    //                          ,EI5.EmployeeName CheckedByEmployee                           
    //                          ,GPM.[CheckedByStatus]
    //                          ,GPM.[CheckedHoldRejectReason]
    //                          ,GPM.[ApprovedBy]
    //                          ,EI6.EmployeeName ApprovedByEmployee      
    //                          ,GPM.[ApprovedByStatus]
    //                          ,GPM.[ApprovedHoldRejectReason]
    //                          ,GPM.[SenderSecurityEmployeeId]
	   //                        ,EI3.EmployeeName SenderSecurityEmployee
    //                          ,GPM.[SenderSecurityApprovedStatus]
    //                          ,GPM.[ReceiverSecurityEmployeeId]
	   //                       ,EI4.EmployeeName ReceiverSecurityEmployee
    //                          ,GPM.[ReceiverSecurityApprovedStatus]
    //                          ,GPM.[VendorBuyerOtherCompanyReceivedStatus]
    //                          ,GPM.[AddedBy]
    //                          ,GPM.[AddedDate]
    //                          ,GPM.[AddedFromIP]
    //                          ,GPM.[UpdatedBy]
    //                          ,GPM.[UpdatedDate]
    //                          ,GPM.[UpdatedFromIP],GPM.ChallanNo
    //                      FROM [TRN].[GatePassMaster] GPM
    //                      LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.FromEmployeeId
    //                      LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.RunnerEmployeeId
    //                      LEft JOIN hkp.Party P On p.Id=GPM.ToPartyCode
    //                      LEft JOIN [HKP].[Buyer] BUYer On BUYer.Id=GPM.ToBuyerId
    //                      LEFT JOIN org.Plant Plant ON Plant.id=GPM.ToPlantId
    //                      LEFT JOIN org.Unit Unit ON Unit.id=GPM.ToUnitId
    //                      LEFT JOIN org.Division Division ON Division.id=GPM.ToDivisionId
    //                      LEFT JOIN org.Department Department ON Department.id=GPM.ToDepartment
    //                      LEFT JOIN Employeeinformation EI2 on EI2.SystemId= GPM.DepartmentEmployeeId
    //                      LEFT JOIN Employeeinformation EI3 on EI3.SystemId= GPM.SenderSecurityEmployeeId
    //                      LEFT JOIN Employeeinformation EI4 on EI4.SystemId= GPM.ReceiverSecurityEmployeeId
    //                      LEFT JOIN Employeeinformation EI5 on EI5.SystemId= GPM.CheckedBy
    //                      LEFT JOIN Employeeinformation EI6 on EI6.SystemId= GPM.ApprovedBy
    //                      Where GPM.FromEmployeeId='" + identity.EmployeeId + @"' 
				//		  AND GPM.CheckedByStatus='Checked' AND ApprovedByStatus='For Approval' AND GPM.GateRegisterType='" + GateRegisterType + @"' Order By GPM.[Id] DESC";
				//}
				//if (ReqStatus == "4")
				//{
				//	sql = @"
    //                    SELECT  GPM.[Id]
    //                          ,GPM.[CompanyGroupId]
    //                          --,GPM.[CompanyId]
    //                          ,GPM.[PlantId]
    //                          ,GPM.[GatePassType]
    //                          ,GPM.[GatePassStatus] GatePassStatus1
    //                          ,REPLACE(CONVERT(CHAR(11), GPM.[ReturnableDate], 106),' ','-') AS ReturnableDate
    //                          ,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
    //                          ,GPM.[FromEmployeeId]
	   //                       ,EI.EmployeeName SenderName
    //                          ,GPM.[Through]
    //                          ,GPM.[CourierName]
    //                          ,GPM.[RunnerEmployeeId]
	   //                       ,EI1.EmployeeName RunnerEmployee
    //                          ,GPM.[ToType]
    //                          ,GPM.[ToPartyCode]
	   //                       ,p.UserName Vendor
    //                          ,GPM.[ToBuyerId]
	   //                       ,BUYer.UserName BuyerName
    //                          ,GPM.[ToPlantId]
	   //                       ,Plant.UserName PlantName
    //                          ,GPM.[ToUnitId]
	   //                       ,Unit.UserName UnitName
    //                          ,GPM.[ToDivisionId]
	   //                       ,Division.UserName DivisionName
    //                          ,GPM.[ToDepartment]
	   //                       ,Department.UserName DepartmentName
    //                          ,GPM.[DepartmentEmployeeId]
	   //                       ,EI2.EmployeeName DepartmentEmployee
    //                          ,GPM.[OtherCompanyName]
    //                          ,GPM.[PersonName]
    //                          ,GPM.[MobileNo]
    //                          ,GPM.[Address]
    //                          ,GPM.[Remarks]
    //                          ,GPM.[CheckedBy]
    //                          ,EI5.EmployeeName CheckedByEmployee                           
    //                          ,GPM.[CheckedByStatus]
    //                          ,GPM.[CheckedHoldRejectReason]
    //                          ,GPM.[ApprovedBy]
    //                          ,EI6.EmployeeName ApprovedByEmployee      
    //                          ,GPM.[ApprovedByStatus]
    //                          ,GPM.[ApprovedHoldRejectReason]
    //                          ,GPM.[SenderSecurityEmployeeId]
	   //                        ,EI3.EmployeeName SenderSecurityEmployee
    //                          ,GPM.[SenderSecurityApprovedStatus]
    //                          ,GPM.[ReceiverSecurityEmployeeId]
	   //                       ,EI4.EmployeeName ReceiverSecurityEmployee
    //                          ,GPM.[ReceiverSecurityApprovedStatus]
    //                          ,GPM.[VendorBuyerOtherCompanyReceivedStatus]
    //                          ,GPM.[AddedBy]
    //                          ,GPM.[AddedDate]
    //                          ,GPM.[AddedFromIP]
    //                          ,GPM.[UpdatedBy]
    //                          ,GPM.[UpdatedDate]
    //                          ,GPM.[UpdatedFromIP],GPM.ChallanNo
    //                      FROM [TRN].[GatePassMaster] GPM
    //                      LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.FromEmployeeId
    //                      LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.RunnerEmployeeId
    //                      LEft JOIN hkp.Party P On p.Id=GPM.ToPartyCode
    //                      LEft JOIN [HKP].[Buyer] BUYer On BUYer.Id=GPM.ToBuyerId
    //                      LEFT JOIN org.Plant Plant ON Plant.id=GPM.ToPlantId
    //                      LEFT JOIN org.Unit Unit ON Unit.id=GPM.ToUnitId
    //                      LEFT JOIN org.Division Division ON Division.id=GPM.ToDivisionId
    //                      LEFT JOIN org.Department Department ON Department.id=GPM.ToDepartment
    //                      LEFT JOIN Employeeinformation EI2 on EI2.SystemId= GPM.DepartmentEmployeeId
    //                      LEFT JOIN Employeeinformation EI3 on EI3.SystemId= GPM.SenderSecurityEmployeeId
    //                      LEFT JOIN Employeeinformation EI4 on EI4.SystemId= GPM.ReceiverSecurityEmployeeId
    //                      LEFT JOIN Employeeinformation EI5 on EI5.SystemId= GPM.CheckedBy
    //                      LEFT JOIN Employeeinformation EI6 on EI6.SystemId= GPM.ApprovedBy
    //                      Where GPM.FromEmployeeId='" + identity.EmployeeId + @"' 
				//		  AND GPM.CheckedByStatus='Checked' AND (ApprovedByStatus='Hold' || ApprovedByStatus='Reject') AND GPM.GateRegisterType='" + GateRegisterType + @"'  Order By GPM.[Id] DESC";
				//}
				//if (ReqStatus == "5")
				//{
				//	sql = @"
    //                    SELECT  GPM.[Id]
    //                          ,GPM.[CompanyGroupId]
    //                          --,GPM.[CompanyId]
    //                          ,GPM.[PlantId]
    //                          ,GPM.[GatePassType]
    //                          ,GPM.[GatePassStatus] GatePassStatus1
    //                          ,REPLACE(CONVERT(CHAR(11), GPM.[ReturnableDate], 106),' ','-') AS ReturnableDate
    //                          ,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
    //                          ,GPM.[FromEmployeeId]
	   //                       ,EI.EmployeeName SenderName
    //                          ,GPM.[Through]
    //                          ,GPM.[CourierName]
    //                          ,GPM.[RunnerEmployeeId]
	   //                       ,EI1.EmployeeName RunnerEmployee
    //                          ,GPM.[ToType]
    //                          ,GPM.[ToPartyCode]
	   //                       ,p.UserName Vendor
    //                          ,GPM.[ToBuyerId]
	   //                       ,BUYer.UserName BuyerName
    //                          ,GPM.[ToPlantId]
	   //                       ,Plant.UserName PlantName
    //                          ,GPM.[ToUnitId]
	   //                       ,Unit.UserName UnitName
    //                          ,GPM.[ToDivisionId]
	   //                       ,Division.UserName DivisionName
    //                          ,GPM.[ToDepartment]
	   //                       ,Department.UserName DepartmentName
    //                          ,GPM.[DepartmentEmployeeId]
	   //                       ,EI2.EmployeeName DepartmentEmployee
    //                          ,GPM.[OtherCompanyName]
    //                          ,GPM.[PersonName]
    //                          ,GPM.[MobileNo]
    //                          ,GPM.[Address]
    //                          ,GPM.[Remarks]
    //                          ,GPM.[CheckedBy]
    //                          ,EI5.EmployeeName CheckedByEmployee                           
    //                          ,GPM.[CheckedByStatus]
    //                          ,GPM.[CheckedHoldRejectReason]
    //                          ,GPM.[ApprovedBy]
    //                          ,EI6.EmployeeName ApprovedByEmployee      
    //                          ,GPM.[ApprovedByStatus]
    //                          ,GPM.[ApprovedHoldRejectReason]
    //                          ,GPM.[SenderSecurityEmployeeId]
	   //                        ,EI3.EmployeeName SenderSecurityEmployee
    //                          ,GPM.[SenderSecurityApprovedStatus]
    //                          ,GPM.[ReceiverSecurityEmployeeId]
	   //                       ,EI4.EmployeeName ReceiverSecurityEmployee
    //                          ,GPM.[ReceiverSecurityApprovedStatus]
    //                          ,GPM.[VendorBuyerOtherCompanyReceivedStatus]
    //                          ,GPM.[AddedBy]
    //                          ,GPM.[AddedDate]
    //                          ,GPM.[AddedFromIP]
    //                          ,GPM.[UpdatedBy]
    //                          ,GPM.[UpdatedDate]
    //                          ,GPM.[UpdatedFromIP],GPM.ChallanNo
    //                      FROM [TRN].[GatePassMaster] GPM
    //                      LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.FromEmployeeId
    //                      LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.RunnerEmployeeId
    //                      LEft JOIN hkp.Party P On p.Id=GPM.ToPartyCode
    //                      LEft JOIN [HKP].[Buyer] BUYer On BUYer.Id=GPM.ToBuyerId
    //                      LEFT JOIN org.Plant Plant ON Plant.id=GPM.ToPlantId
    //                      LEFT JOIN org.Unit Unit ON Unit.id=GPM.ToUnitId
    //                      LEFT JOIN org.Division Division ON Division.id=GPM.ToDivisionId
    //                      LEFT JOIN org.Department Department ON Department.id=GPM.ToDepartment
    //                      LEFT JOIN Employeeinformation EI2 on EI2.SystemId= GPM.DepartmentEmployeeId
    //                      LEFT JOIN Employeeinformation EI3 on EI3.SystemId= GPM.SenderSecurityEmployeeId
    //                      LEFT JOIN Employeeinformation EI4 on EI4.SystemId= GPM.ReceiverSecurityEmployeeId
    //                      LEFT JOIN Employeeinformation EI5 on EI5.SystemId= GPM.CheckedBy
    //                      LEFT JOIN Employeeinformation EI6 on EI6.SystemId= GPM.ApprovedBy
    //                      Where GPM.FromEmployeeId='" + identity.EmployeeId + @"' 
				//		  AND GPM.CheckedByStatus='Checked' AND ApprovedByStatus='Approved'  AND GPM.GateRegisterType='" + GateRegisterType + @"' Order By GPM.[Id] DESC";
				//}
				//if (ReqStatus == "6")
				//{
				//	sql = @"
    //                    SELECT  GPM.[Id]
    //                          ,GPM.[CompanyGroupId]
    //                          --,GPM.[CompanyId]
    //                          ,GPM.[PlantId]
    //                          ,GPM.[GatePassType]
    //                          ,GPM.[GatePassStatus] GatePassStatus1
    //                          ,REPLACE(CONVERT(CHAR(11), GPM.[ReturnableDate], 106),' ','-') AS ReturnableDate
    //                          ,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
    //                          ,GPM.[FromEmployeeId]
	   //                       ,EI.EmployeeName SenderName
    //                          ,GPM.[Through]
    //                          ,GPM.[CourierName]
    //                          ,GPM.[RunnerEmployeeId]
	   //                       ,EI1.EmployeeName RunnerEmployee
    //                          ,GPM.[ToType]
    //                          ,GPM.[ToPartyCode]
	   //                       ,p.UserName Vendor
    //                          ,GPM.[ToBuyerId]
	   //                       ,BUYer.UserName BuyerName
    //                          ,GPM.[ToPlantId]
	   //                       ,Plant.UserName PlantName
    //                          ,GPM.[ToUnitId]
	   //                       ,Unit.UserName UnitName
    //                          ,GPM.[ToDivisionId]
	   //                       ,Division.UserName DivisionName
    //                          ,GPM.[ToDepartment]
	   //                       ,Department.UserName DepartmentName
    //                          ,GPM.[DepartmentEmployeeId]
	   //                       ,EI2.EmployeeName DepartmentEmployee
    //                          ,GPM.[OtherCompanyName]
    //                          ,GPM.[PersonName]
    //                          ,GPM.[MobileNo]
    //                          ,GPM.[Address]
    //                          ,GPM.[Remarks]
    //                          ,GPM.[CheckedBy]
    //                          ,EI5.EmployeeName CheckedByEmployee                           
    //                          ,GPM.[CheckedByStatus]
    //                          ,GPM.[CheckedHoldRejectReason]
    //                          ,GPM.[ApprovedBy]
    //                          ,EI6.EmployeeName ApprovedByEmployee      
    //                          ,GPM.[ApprovedByStatus]
    //                          ,GPM.[ApprovedHoldRejectReason]
    //                          ,GPM.[SenderSecurityEmployeeId]
	   //                        ,EI3.EmployeeName SenderSecurityEmployee
    //                          ,GPM.[SenderSecurityApprovedStatus]
    //                          ,GPM.[ReceiverSecurityEmployeeId]
	   //                       ,EI4.EmployeeName ReceiverSecurityEmployee
    //                          ,GPM.[ReceiverSecurityApprovedStatus]
    //                          ,GPM.[VendorBuyerOtherCompanyReceivedStatus]
    //                          ,GPM.[AddedBy]
    //                          ,GPM.[AddedDate]
    //                          ,GPM.[AddedFromIP]
    //                          ,GPM.[UpdatedBy]
    //                          ,GPM.[UpdatedDate]
    //                          ,GPM.[UpdatedFromIP],GPM.ChallanNo
    //                      FROM [TRN].[GatePassMaster] GPM
    //                      LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.FromEmployeeId
    //                      LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.RunnerEmployeeId
    //                      LEft JOIN hkp.Party P On p.Id=GPM.ToPartyCode
    //                      LEft JOIN [HKP].[Buyer] BUYer On BUYer.Id=GPM.ToBuyerId
    //                      LEFT JOIN org.Plant Plant ON Plant.id=GPM.ToPlantId
    //                      LEFT JOIN org.Unit Unit ON Unit.id=GPM.ToUnitId
    //                      LEFT JOIN org.Division Division ON Division.id=GPM.ToDivisionId
    //                      LEFT JOIN org.Department Department ON Department.id=GPM.ToDepartment
    //                      LEFT JOIN Employeeinformation EI2 on EI2.SystemId= GPM.DepartmentEmployeeId
    //                      LEFT JOIN Employeeinformation EI3 on EI3.SystemId= GPM.SenderSecurityEmployeeId
    //                      LEFT JOIN Employeeinformation EI4 on EI4.SystemId= GPM.ReceiverSecurityEmployeeId
    //                      LEFT JOIN Employeeinformation EI5 on EI5.SystemId= GPM.CheckedBy
    //                      LEFT JOIN Employeeinformation EI6 on EI6.SystemId= GPM.ApprovedBy
    //                      Where GPM.FromEmployeeId='" + identity.EmployeeId + @"' 
				//		  AND GPM.CheckedByStatus='Checked' AND ApprovedByStatus='Approved'
				//		  AND SenderSecurityApprovedStatus='Approved',GateOutStatus=1 AND GPM.GateRegisterType='" + GateRegisterType + @"'
				//		  Order By GPM.[Id] DESC";

				//}
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
			// return Json(_gateEntryService.PlantWiseGateCbo(IsSysAdmin, UserId, plantId), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpGet]
		public JsonResult GetIndexGridDataListDetails(string Id)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";

				sql = @"
                        SELECT  GPM.[Id]
                              ,GPM.[CompanyGroupId]
                              ,GPM.[CompanyId]
                              ,GPM.[PlantId]
                              ,GPM.[GatePassType]
                              ,GPM.[GatePassStatus]
                              ,GPM.[ReturnableDate]
                              ,GPM.[GatePassEntryDate]
                              ,GPM.[FromEmployeeId]
	                          ,EI.EmployeeName SenderName
                              ,GPM.[Through]
                              ,GPM.[CourierName]
                              ,GPM.[RunnerEmployeeId]
	                          ,EI1.EmployeeName RunnerEmployee
                              ,GPM.[ToType]
                              ,GPM.[ToPartyCode]
	                          ,p.UserName Vendor
                              ,GPM.[ToBuyerId]
	                          ,BUYer.UserName BuyerName
                              ,GPM.[ToPlantId]
	                          ,Plant.UserName PlantName
                              ,GPM.[ToUnitId]
	                          ,Unit.UserName UnitName
                              ,GPM.[ToDivisionId]
	                          ,Division.UserName DivisionName
                              ,GPM.[ToDepartment]
	                          ,Department.UserName DepartmentName
                              ,GPM.[DepartmentEmployeeId]
	                          ,EI2.EmployeeName DepartmentEmployee
                              ,GPM.[OtherCompanyName]
                              ,GPM.[PersonName]
                              ,GPM.[MobileNo]
                              ,GPM.[Address]
                              ,GPM.[Remarks]
                              ,GPM.[CheckedBy]
                              ,GPM.[CheckedByStatus]
                              ,GPM.[CheckedHoldRejectReason]
                              ,GPM.[ApprovedBy]
                              ,GPM.[ApprovedByStatus]
                              ,GPM.[ApprovedHoldRejectReason]
                              ,GPM.[SenderSecurityEmployeeId]
	                           ,EI3.EmployeeName SenderSecurityEmployee
                              ,GPM.[SenderSecurityApprovedStatus]
                              ,GPM.[ReceiverSecurityEmployeeId]
	                          ,EI4.EmployeeName ReceiverSecurityEmployee
                              ,GPM.[ReceiverSecurityApprovedStatus]
                              ,GPM.[VendorBuyerOtherCompanyReceivedStatus]
                              ,GPM.[AddedBy]
                              ,GPM.[AddedDate]
                              ,GPM.[AddedFromIP]
                              ,GPM.[UpdatedBy]
                              ,GPM.[UpdatedDate]
                              ,GPM.[UpdatedFromIP]
                          FROM [TRN].[GatePassMaster] GPM
                          LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.FromEmployeeId
                          LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.RunnerEmployeeId
                          LEft JOIN hkp.Party P On p.code=GPM.ToPartyCode
                          LEft JOIN [HKP].[Buyer] BUYer On BUYer.Id=GPM.ToBuyerId
                          LEFT JOIN org.Plant Plant ON Plant.id=GPM.ToPlantId
                          LEFT JOIN org.Unit Unit ON Unit.id=GPM.ToUnitId
                          LEFT JOIN org.Division Division ON Division.id=GPM.ToDivisionId
                          LEFT JOIN org.Department Department ON Department.id=GPM.ToDepartment
                          LEFT JOIN Employeeinformation EI2 on EI2.SystemId= GPM.DepartmentEmployeeId
                          LEFT JOIN Employeeinformation EI3 on EI3.SystemId= GPM.SenderSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI4 on EI4.SystemId= GPM.ReceiverSecurityEmployeeId
                        ";
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
			// return Json(_gateEntryService.PlantWiseGateCbo(IsSysAdmin, UserId, plantId), JsonRequestBehavior.AllowGet);
		}
		[Authorize, HttpPost]
		public JsonResult DetailCreate(GatePassDetailsViewModel entity, string ChallanNo)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			//entity.CompanyGroupId = identity.CompanyGroupId;
			//if (entity != null)
			//{
			//    if (entity.BudgetType == "Overbudget")
			//    {
			//        if (entity.Reason == null || entity.Reason == "")
			//        {
			//            throw new CustomException("Input Reason!");
			//        }
			//    }
			//    else if (entity.BudgetType == "New")
			//    {
			//        if (entity.MaterialDetail == null || entity.MaterialDetail == "")
			//        {
			//            throw new CustomException("Input Material Detail!");
			//        }
			//        else if (entity.TransactionUoMId == null || entity.TransactionUoMId == "")
			//        {
			//            throw new CustomException("Please select UOM!");
			//        }
			//    }

			//}
			//entity.CompanyId = identity.CompanyId;
			//entity.PlantId = identity.PlantId;
			_gateEntryService.InsertOrUpdateGraph(entity, ChallanNo);
			return Json(new { entity.Id, Message = AplosMessage.Success });
		}
		[Authorize, HttpPost]
		public JsonResult DetailCreateDispatch(IEnumerable<GatePassDetailsViewModel> entity, string ChallanNo,string MasterId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			//entity.CompanyGroupId = identity.CompanyGroupId;
			//if (entity != null)
			//{
			//    if (entity.BudgetType == "Overbudget")
			//    {
			//        if (entity.Reason == null || entity.Reason == "")
			//        {
			//            throw new CustomException("Input Reason!");
			//        }
			//    }
			//    else if (entity.BudgetType == "New")
			//    {
			//        if (entity.MaterialDetail == null || entity.MaterialDetail == "")
			//        {
			//            throw new CustomException("Input Material Detail!");
			//        }
			//        else if (entity.TransactionUoMId == null || entity.TransactionUoMId == "")
			//        {
			//            throw new CustomException("Please select UOM!");
			//        }
			//    }

			//}
			//entity.CompanyId = identity.CompanyId;
			//entity.PlantId = identity.PlantId;
			_gateEntryService.InsertOrUpdateGraphDispatch(entity, ChallanNo, MasterId);
			return Json(new { Message = AplosMessage.Success });
		}
		[Authorize, HttpGet]
		public JsonResult GetInventoryMaterialList(GridParameter parameters, string inveReveiveId,string GatePassNewId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(_gateEntryService.QueryForGatePassDetail(parameters, inveReveiveId, GatePassNewId), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpGet]
		public JsonResult GetAllMaterilaList()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";

				sql = @"SELECT                   IM.Id
                        ,IR.Id AS GatePassMasterId
                        , MGM.UserName AS MaterialGroupMasterName
                        , IM.MaterialMasterId, MM.UserName
                        , IM.ArticleId, ART.StandardName
                        , IM.FirstCharacteristicsId, FC.UserName AS FirstCharacteristics
                        , IM.FirstCharacteristicsValueId, FCV.UserName AS FirstCharacteristicsValue
                        , IM.SecondCharacteristicsId, SC.UserName AS SecondCharacteristics
                        , IM.SecondCharacteristicsValueId, SCV.UserName AS SecondCharacteristicsValue
                        , IM.ThirdCharacteristicsId, TC.UserName AS ThirdCharacteristics
                        , IM.ThirdCharacteristicsValueId, TCV.UserName AS ThirdCharacteristicsValue
                        , ROUND(IM.TransactionQty,2) TransactionQty
                        , IM.TransactionUoMId
                        ,Replace(CONVERT(VARCHAR(11), IM.ReturnableDate, 106), ' ', '-') ReturnableDate
                        , TUoM.UserName AS TransactionUoM                       
                        ,IM.MaterialDetail        
                        ,IM.Remarks
                        ,IsReturnable = CASE WHen IM.IsReturnable=1 Then 'Yes' Else 'No' End
						,IM.ReturnableDate
						,IsMutilated= CASE When IM.IsMutilated=1 Then 'Yes' ELSE 'No' END                      
                        FROM TRN.GatePassDetails AS IM
                        LEFT JOIN MST.MaterialMaster AS MM ON IM.MaterialMasterId=MM.Id
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                        LEFT JOIN MST.MaterialMasterArticle AS ART ON IM.ArticleId=ART.Id
                        LEFT JOIN HKP.Characteristics AS FC ON IM.FirstCharacteristicsId=FC.Id
                        LEFT JOIN HKP.Characteristics AS SC ON IM.SecondCharacteristicsId=SC.Id
                        LEFT JOIN HKP.Characteristics AS TC ON IM.ThirdCharacteristicsId=TC.Id
                        LEFT JOIN HKP.CharacteristicsValue AS FCV ON IM.FirstCharacteristicsValueId=FCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS SCV ON IM.SecondCharacteristicsValueId=SCV.Id
                        LEFT JOIN HKP.CharacteristicsValue AS TCV ON IM.ThirdCharacteristicsValueId=TCV.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON IM.TransactionUoMId=TUoM.Id
                        LEFT JOIN [TRN].[GatePassMaster] AS IR ON IM.GatePassMasterId=IR.Id 
						Where IR.PlantId='" + identity.PlantId + "'";
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
			// return Json(_gateEntryService.PlantWiseGateCbo(IsSysAdmin, UserId, plantId), JsonRequestBehavior.AllowGet);
		}
		[HttpPost, ChaildAction(ParentActionName = nameof(Delete))]
		public JsonResult DeleteGatePassDEtails(string id)
		{
			_gateEntryService.DeleteGatePassDEtails(id);
			return Json(new { Message = AplosMessage.Deleted });
		}

		[Authorize, HttpGet]
		public JsonResult GetChallanNo(string IsSysAdmin, string UserId, string plantId,string InOutStatus)
		{
			
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";
				if (InOutStatus == "In")
				{

					sql = @"SELECT  GPM.[Id] Value
                              ,GPM.[CompanyGroupId]
                              --,GPM.[CompanyId]
                              ,GPM.[PlantId]
                              ,GPM.[GatePassType]
                              ,GPM.[GatePassStatus]
                              ,REPLACE(CONVERT(CHAR(11), GPM.[ReturnableDate], 106),' ','-') AS ReturnableDate
                              ,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
                              ,GPM.[FromEmployeeId]
	                          ,EI.EmployeeName SenderName
                              ,GPM.[Through]
                              ,GPM.[CourierName]
                              ,GPM.[RunnerEmployeeId]
	                          ,EI1.EmployeeName RunnerEmployee
                              ,GPM.[ToType]
                              ,GPM.[ToPartyCode]
	                          ,p.UserName Vendor
                              ,GPM.[ToBuyerId]
	                          ,BUYer.UserName BuyerName
                              ,GPM.[ToPlantId]
	                          ,Plant.UserName PlantName
                              ,GPM.[ToUnitId]
	                          ,Unit.UserName UnitName
                              ,GPM.[ToDivisionId]
	                          ,Division.UserName DivisionName
                              ,GPM.[ToDepartment]
	                          ,Department.UserName DepartmentName
                              ,GPM.[DepartmentEmployeeId]
	                          ,EI2.EmployeeName DepartmentEmployee
                              ,GPM.[OtherCompanyName]
                              ,GPM.[PersonName]
                              ,GPM.[MobileNo]
                              ,GPM.[Address]
                              ,GPM.[Remarks]
                              ,GPM.[CheckedBy]
                              ,EI5.EmployeeName CheckedByEmployee                           
                              ,GPM.[CheckedByStatus]
                              ,GPM.[CheckedHoldRejectReason]
                              ,GPM.[ApprovedBy]
                              ,EI6.EmployeeName ApprovedByEmployee      
                              ,GPM.[ApprovedByStatus]
                              ,GPM.[ApprovedHoldRejectReason]
                              ,GPM.[SenderSecurityEmployeeId]
	                           ,EI3.EmployeeName SenderSecurityEmployee
                              ,GPM.[SenderSecurityApprovedStatus]
                              ,GPM.[ReceiverSecurityEmployeeId]
	                          ,EI4.EmployeeName ReceiverSecurityEmployee
                              ,GPM.[ReceiverSecurityApprovedStatus]
                              ,GPM.[VendorBuyerOtherCompanyReceivedStatus]
                              ,GPM.[AddedBy]
                              ,GPM.[AddedDate]
                              ,GPM.[AddedFromIP]
                              ,GPM.[UpdatedBy]
                              ,GPM.[UpdatedDate]
                              ,GPM.[UpdatedFromIP],GPM.ChallanNo,GPM.PurposeofGatePass,GPM.ConsignmentNo,GPM.DriverName,GPM.NoOfPackages,GPM.InvoiceNo,GPM.InvoiceValue
                          FROM [TRN].[GatePassMaster] GPM
                          LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.FromEmployeeId
                          LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.RunnerEmployeeId
                          LEft JOIN hkp.Party P On p.code=GPM.ToPartyCode
                          LEft JOIN [HKP].[Buyer] BUYer On BUYer.Id=GPM.ToBuyerId
                          LEFT JOIN org.Plant Plant ON Plant.id=GPM.ToPlantId
                          LEFT JOIN org.Unit Unit ON Unit.id=GPM.ToUnitId
                          LEFT JOIN org.Division Division ON Division.id=GPM.ToDivisionId
                          LEFT JOIN org.Department Department ON Department.id=GPM.ToDepartment
                          LEFT JOIN Employeeinformation EI2 on EI2.SystemId= GPM.DepartmentEmployeeId
                          LEFT JOIN Employeeinformation EI3 on EI3.SystemId= GPM.SenderSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI4 on EI4.SystemId= GPM.ReceiverSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI5 on EI5.SystemId= GPM.CheckedBy
                          LEFT JOIN Employeeinformation EI6 on EI6.SystemId= GPM.ApprovedBy
                          WHERE  GPM.[GatePassStatus]='Returnable' AND GPM.GateRegisterType='" + InOutStatus + @"'";//" AND GPM.[CheckedByStatus]= 'Checked' " + "AND GPM.[ApprovedByStatus]= 'Approved'  GPM.DepartmentEmployeeId='" + identity.EmployeeId + "' AND
				}
				else
				{

					sql = @"SELECT  GPM.[Id] Value
                              ,GPM.[CompanyGroupId]
                              --,GPM.[CompanyId]
                              ,GPM.[PlantId]
                              ,GPM.[GatePassType]
                              ,GPM.[GatePassStatus]
                              ,REPLACE(CONVERT(CHAR(11), GPM.[ReturnableDate], 106),' ','-') AS ReturnableDate
                              ,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
                              ,GPM.[FromEmployeeId]
	                          ,EI.EmployeeName SenderName
                              ,GPM.[Through]
                              ,GPM.[CourierName]
                              ,GPM.[RunnerEmployeeId]
	                          ,EI1.EmployeeName RunnerEmployee
                              ,GPM.[ToType]
                              ,GPM.[ToPartyCode]
	                          ,p.UserName Vendor
                              ,GPM.[ToBuyerId]
	                          ,BUYer.UserName BuyerName
                              ,GPM.[ToPlantId]
	                          ,Plant.UserName PlantName
                              ,GPM.[ToUnitId]
	                          ,Unit.UserName UnitName
                              ,GPM.[ToDivisionId]
	                          ,Division.UserName DivisionName
                              ,GPM.[ToDepartment]
	                          ,Department.UserName DepartmentName
                              ,GPM.[DepartmentEmployeeId]
	                          ,EI2.EmployeeName DepartmentEmployee
                              ,GPM.[OtherCompanyName]
                              ,GPM.[PersonName]
                              ,GPM.[MobileNo]
                              ,GPM.[Address]
                              ,GPM.[Remarks]
                              ,GPM.[CheckedBy]
                              ,EI5.EmployeeName CheckedByEmployee                           
                              ,GPM.[CheckedByStatus]
                              ,GPM.[CheckedHoldRejectReason]
                              ,GPM.[ApprovedBy]
                              ,EI6.EmployeeName ApprovedByEmployee      
                              ,GPM.[ApprovedByStatus]
                              ,GPM.[ApprovedHoldRejectReason]
                              ,GPM.[SenderSecurityEmployeeId]
	                           ,EI3.EmployeeName SenderSecurityEmployee
                              ,GPM.[SenderSecurityApprovedStatus]
                              ,GPM.[ReceiverSecurityEmployeeId]
	                          ,EI4.EmployeeName ReceiverSecurityEmployee
                              ,GPM.[ReceiverSecurityApprovedStatus]
                              ,GPM.[VendorBuyerOtherCompanyReceivedStatus]
                              ,GPM.[AddedBy]
                              ,GPM.[AddedDate]
                              ,GPM.[AddedFromIP]
                              ,GPM.[UpdatedBy]
                              ,GPM.[UpdatedDate]
                              ,GPM.[UpdatedFromIP],GPM.ChallanNo,GPM.PurposeofGatePass,GPM.ConsignmentNo,GPM.DriverName,GPM.NoOfPackages,GPM.InvoiceNo,GPM.InvoiceValue
                          FROM [TRN].[GatePassMaster] GPM
                          LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.FromEmployeeId
                          LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.RunnerEmployeeId
                          LEft JOIN hkp.Party P On p.code=GPM.ToPartyCode
                          LEft JOIN [HKP].[Buyer] BUYer On BUYer.Id=GPM.ToBuyerId
                          LEFT JOIN org.Plant Plant ON Plant.id=GPM.ToPlantId
                          LEFT JOIN org.Unit Unit ON Unit.id=GPM.ToUnitId
                          LEFT JOIN org.Division Division ON Division.id=GPM.ToDivisionId
                          LEFT JOIN org.Department Department ON Department.id=GPM.ToDepartment
                          LEFT JOIN Employeeinformation EI2 on EI2.SystemId= GPM.DepartmentEmployeeId
                          LEFT JOIN Employeeinformation EI3 on EI3.SystemId= GPM.SenderSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI4 on EI4.SystemId= GPM.ReceiverSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI5 on EI5.SystemId= GPM.CheckedBy
                          LEFT JOIN Employeeinformation EI6 on EI6.SystemId= GPM.ApprovedBy
                          WHERE  GPM.[GatePassStatus]='Returnable' AND GPM.GateRegisterType='" + InOutStatus + @"' AND GPM.[CheckedByStatus]= 'Checked' " + "AND GPM.[ApprovedByStatus]= 'Approved'";//" AND GPM.[CheckedByStatus]= 'Checked' " + "AND GPM.[ApprovedByStatus]= 'Approved'  GPM.DepartmentEmployeeId='" + identity.EmployeeId + "' AND
				}
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
			// return Json(_gateEntryService.PlantWiseGateCbo(IsSysAdmin, UserId, plantId), JsonRequestBehavior.AllowGet);
		}

		[Authorize, HttpGet]
		public JsonResult GetChallanNoDetailsForToAddress(string DepartmentEmployeeId)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";

				sql = @"SELECT plant.UserName PlantName,plant.Id ToPlantId
                              ,unit.UserName UnitName,Unit.Id ToUnitId
                              ,Division.UserName DivisionName,Division.Id ToDivisionId
                              ,Department.UserName DepartmentName,Department.Id ToDepartment
			                   from employeeinformation EI
			                LEFT JOIN org.Plant Plant ON Plant.id=EI.PlantId
			                LEFT JOIN org.Unit Unit ON Unit.id=EI.UnitId
			                LEFT JOIN org.Division Division ON Division.id=EI.DivisionId
			                LEFT JOIN org.Department Department ON Department.id=EI.DepartmentId
                        WHERE EI.SystemId='" + DepartmentEmployeeId + @"'";

				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
			// return Json(_gateEntryService.PlantWiseGateCbo(IsSysAdmin, UserId, plantId), JsonRequestBehavior.AllowGet);
		}

		#endregion


		#region Gate Pass Checked And Approved
		[Authorize, HttpGet]
		public JsonResult GetCheckedApprovedList(int tabType)
		{

			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";
				

				if (tabType == 1)//"UnCheckedList")
				{
					sql = @"
                        SELECT  GPM.[Id]
                              ,GPM.[CompanyGroupId]
                              --,GPM.[CompanyId]
                              ,GPM.[PlantId]
                              ,GPM.[GatePassType]
                              ,GPM.[GatePassStatus]
                              ,REPLACE(CONVERT(CHAR(11), GPM.[ReturnableDate], 106),' ','-') AS ReturnableDate
                              ,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
                              ,GPM.[FromEmployeeId]
	                          ,EI.EmployeeName SenderName
                              ,GPM.[Through]
                              ,GPM.[CourierName]
                              ,GPM.[RunnerEmployeeId]
	                          ,EI1.EmployeeName RunnerEmployee
                              ,GPM.[ToType]
                              ,GPM.[ToPartyCode]
	                          ,p.UserName Vendor
                              ,GPM.[ToBuyerId]
	                          ,BUYer.UserName BuyerName
                              ,GPM.[ToPlantId]
	                          ,Plant.UserName PlantName
                              ,GPM.[ToUnitId]
	                          ,Unit.UserName UnitName
                              ,GPM.[ToDivisionId]
	                          ,Division.UserName DivisionName
                              ,GPM.[ToDepartment]
	                          ,Department.UserName DepartmentName
                              ,GPM.[DepartmentEmployeeId]
	                          ,EI2.EmployeeName DepartmentEmployee
                              ,GPM.[OtherCompanyName]
                              ,GPM.[PersonName]
                              ,GPM.[MobileNo]
                              ,GPM.[Address]
                              ,GPM.[Remarks]
                              ,GPM.[CheckedBy]
                              ,EI5.EmployeeName CheckedByEmployee                           
                              ,GPM.[CheckedByStatus]
                              ,GPM.[CheckedHoldRejectReason]
                              ,GPM.[ApprovedBy]
                              ,EI6.EmployeeName ApprovedByEmployee      
                              ,GPM.[ApprovedByStatus]
                              ,GPM.[ApprovedHoldRejectReason]
                              ,GPM.[SenderSecurityEmployeeId]
	                           ,EI3.EmployeeName SenderSecurityEmployee
                              ,GPM.[SenderSecurityApprovedStatus]
                              ,GPM.[ReceiverSecurityEmployeeId]
	                          ,EI4.EmployeeName ReceiverSecurityEmployee
                              ,GPM.[ReceiverSecurityApprovedStatus]
                              ,GPM.[VendorBuyerOtherCompanyReceivedStatus]
                              ,GPM.[AddedBy]
                              ,GPM.[AddedDate]
                              ,GPM.[AddedFromIP]
                              ,GPM.[UpdatedBy]
                              ,GPM.[UpdatedDate]
                              ,GPM.[UpdatedFromIP],GPM.ChallanNo,GPM.GateRegisterType
                          FROM [TRN].[GatePassMaster] GPM
                          LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.FromEmployeeId
                          LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.RunnerEmployeeId
                          LEft JOIN hkp.Party P On p.code=GPM.ToPartyCode
                          LEft JOIN [HKP].[Buyer] BUYer On BUYer.Id=GPM.ToBuyerId
                          LEFT JOIN org.Plant Plant ON Plant.id=GPM.ToPlantId
                          LEFT JOIN org.Unit Unit ON Unit.id=GPM.ToUnitId
                          LEFT JOIN org.Division Division ON Division.id=GPM.ToDivisionId
                          LEFT JOIN org.Department Department ON Department.id=GPM.ToDepartment
                          LEFT JOIN Employeeinformation EI2 on EI2.SystemId= GPM.DepartmentEmployeeId
                          LEFT JOIN Employeeinformation EI3 on EI3.SystemId= GPM.SenderSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI4 on EI4.SystemId= GPM.ReceiverSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI5 on EI5.SystemId= GPM.CheckedBy
                          LEFT JOIN Employeeinformation EI6 on EI6.SystemId= GPM.ApprovedBy
                          Where GPM.CheckedBy='" + identity.EmployeeId + @"' AND GPM.GateRegisterType='Out'
                          AND GPM.CheckedBy Is NOT NULL 
                          And GPM.[CheckedByStatus]='ForChecked' 
                          AND GPM.[ApprovedBy] IS NULL 
                          AND GPM.[ApprovedByStatus] Is NULL";
				}
				else if (tabType == 2)// "HoldRejectCheckedList")
				{
					sql = @"
                        SELECT  GPM.[Id]
                              ,GPM.[CompanyGroupId]
                              --,GPM.[CompanyId]
                              ,GPM.[PlantId]
                              ,GPM.[GatePassType]
                              ,GPM.[GatePassStatus]
                              ,REPLACE(CONVERT(CHAR(11), GPM.[ReturnableDate], 106),' ','-') AS ReturnableDate
                              ,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
                              ,GPM.[FromEmployeeId]
	                          ,EI.EmployeeName SenderName
                              ,GPM.[Through]
                              ,GPM.[CourierName]
                              ,GPM.[RunnerEmployeeId]
	                          ,EI1.EmployeeName RunnerEmployee
                              ,GPM.[ToType]
                              ,GPM.[ToPartyCode]
	                          ,p.UserName Vendor
                              ,GPM.[ToBuyerId]
	                          ,BUYer.UserName BuyerName
                              ,GPM.[ToPlantId]
	                          ,Plant.UserName PlantName
                              ,GPM.[ToUnitId]
	                          ,Unit.UserName UnitName
                              ,GPM.[ToDivisionId]
	                          ,Division.UserName DivisionName
                              ,GPM.[ToDepartment]
	                          ,Department.UserName DepartmentName
                              ,GPM.[DepartmentEmployeeId]
	                          ,EI2.EmployeeName DepartmentEmployee
                              ,GPM.[OtherCompanyName]
                              ,GPM.[PersonName]
                              ,GPM.[MobileNo]
                              ,GPM.[Address]
                              ,GPM.[Remarks]
                              ,GPM.[CheckedBy]
                              ,EI5.EmployeeName CheckedByEmployee                           
                              ,GPM.[CheckedByStatus]
                              ,GPM.[CheckedHoldRejectReason]
                              ,GPM.[ApprovedBy]
                              ,EI6.EmployeeName ApprovedByEmployee      
                              ,GPM.[ApprovedByStatus]
                              ,GPM.[ApprovedHoldRejectReason]
                              ,GPM.[SenderSecurityEmployeeId]
	                           ,EI3.EmployeeName SenderSecurityEmployee
                              ,GPM.[SenderSecurityApprovedStatus]
                              ,GPM.[ReceiverSecurityEmployeeId]
	                          ,EI4.EmployeeName ReceiverSecurityEmployee
                              ,GPM.[ReceiverSecurityApprovedStatus]
                              ,GPM.[VendorBuyerOtherCompanyReceivedStatus]
                              ,GPM.[AddedBy]
                              ,GPM.[AddedDate]
                              ,GPM.[AddedFromIP]
                              ,GPM.[UpdatedBy]
                              ,GPM.[UpdatedDate]
                              ,GPM.[UpdatedFromIP],GPM.ChallanNo,GPM.GateRegisterType
                          FROM [TRN].[GatePassMaster] GPM
                          LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.FromEmployeeId
                          LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.RunnerEmployeeId
                          LEft JOIN hkp.Party P On p.code=GPM.ToPartyCode
                          LEft JOIN [HKP].[Buyer] BUYer On BUYer.Id=GPM.ToBuyerId
                          LEFT JOIN org.Plant Plant ON Plant.id=GPM.ToPlantId
                          LEFT JOIN org.Unit Unit ON Unit.id=GPM.ToUnitId
                          LEFT JOIN org.Division Division ON Division.id=GPM.ToDivisionId
                          LEFT JOIN org.Department Department ON Department.id=GPM.ToDepartment
                          LEFT JOIN Employeeinformation EI2 on EI2.SystemId= GPM.DepartmentEmployeeId
                          LEFT JOIN Employeeinformation EI3 on EI3.SystemId= GPM.SenderSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI4 on EI4.SystemId= GPM.ReceiverSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI5 on EI5.SystemId= GPM.CheckedBy
                          LEFT JOIN Employeeinformation EI6 on EI6.SystemId= GPM.ApprovedBy
                          Where GPM.CheckedBy='" + identity.EmployeeId + @"' AND GPM.GateRegisterType='Out'
                          AND GPM.CheckedBy Is NOT NULL 
                          And GPM.[CheckedByStatus]='Hold' OR GPM.[CheckedByStatus]='Reject'
                          AND GPM.[ApprovedBy] IS NULL OR GPM.ApprovedBy = ''
                          AND GPM.[ApprovedByStatus] Is NULL";
				}
				else if (tabType == 3)// "CheckedList")
				{
					sql = @"
                        SELECT  GPM.[Id]
                              ,GPM.[CompanyGroupId]
                              --,GPM.[CompanyId]
                              ,GPM.[PlantId]
                              ,GPM.[GatePassType]
                              ,GPM.[GatePassStatus]
                              ,REPLACE(CONVERT(CHAR(11), GPM.[ReturnableDate], 106),' ','-') AS ReturnableDate
                              ,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
                              ,GPM.[FromEmployeeId]
	                          ,EI.EmployeeName SenderName
                              ,GPM.[Through]
                              ,GPM.[CourierName]
                              ,GPM.[RunnerEmployeeId]
	                          ,EI1.EmployeeName RunnerEmployee
                              ,GPM.[ToType]
                              ,GPM.[ToPartyCode]
	                          ,p.UserName Vendor
                              ,GPM.[ToBuyerId]
	                          ,BUYer.UserName BuyerName
                              ,GPM.[ToPlantId]
	                          ,Plant.UserName PlantName
                              ,GPM.[ToUnitId]
	                          ,Unit.UserName UnitName
                              ,GPM.[ToDivisionId]
	                          ,Division.UserName DivisionName
                              ,GPM.[ToDepartment]
	                          ,Department.UserName DepartmentName
                              ,GPM.[DepartmentEmployeeId]
	                          ,EI2.EmployeeName DepartmentEmployee
                              ,GPM.[OtherCompanyName]
                              ,GPM.[PersonName]
                              ,GPM.[MobileNo]
                              ,GPM.[Address]
                              ,GPM.[Remarks]
                              ,GPM.[CheckedBy]
                              ,EI5.EmployeeName CheckedByEmployee                           
                              ,GPM.[CheckedByStatus]
                              ,GPM.[CheckedHoldRejectReason]
                              ,GPM.[ApprovedBy]
                              ,EI6.EmployeeName ApprovedByEmployee      
                              ,GPM.[ApprovedByStatus]
                              ,GPM.[ApprovedHoldRejectReason]
                              ,GPM.[SenderSecurityEmployeeId]
	                           ,EI3.EmployeeName SenderSecurityEmployee
                              ,GPM.[SenderSecurityApprovedStatus]
                              ,GPM.[ReceiverSecurityEmployeeId]
	                          ,EI4.EmployeeName ReceiverSecurityEmployee
                              ,GPM.[ReceiverSecurityApprovedStatus]
                              ,GPM.[VendorBuyerOtherCompanyReceivedStatus]
                              ,GPM.[AddedBy]
                              ,GPM.[AddedDate]
                              ,GPM.[AddedFromIP]
                              ,GPM.[UpdatedBy]
                              ,GPM.[UpdatedDate]
                              ,GPM.[UpdatedFromIP],GPM.ChallanNo,GPM.GateRegisterType
                          FROM [TRN].[GatePassMaster] GPM
                          LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.FromEmployeeId
                          LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.RunnerEmployeeId
                          LEft JOIN hkp.Party P On p.code=GPM.ToPartyCode
                          LEft JOIN [HKP].[Buyer] BUYer On BUYer.Id=GPM.ToBuyerId
                          LEFT JOIN org.Plant Plant ON Plant.id=GPM.ToPlantId
                          LEFT JOIN org.Unit Unit ON Unit.id=GPM.ToUnitId
                          LEFT JOIN org.Division Division ON Division.id=GPM.ToDivisionId
                          LEFT JOIN org.Department Department ON Department.id=GPM.ToDepartment
                          LEFT JOIN Employeeinformation EI2 on EI2.SystemId= GPM.DepartmentEmployeeId
                          LEFT JOIN Employeeinformation EI3 on EI3.SystemId= GPM.SenderSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI4 on EI4.SystemId= GPM.ReceiverSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI5 on EI5.SystemId= GPM.CheckedBy
                          LEFT JOIN Employeeinformation EI6 on EI6.SystemId= GPM.ApprovedBy
                          Where GPM.CheckedBy='" + identity.EmployeeId + @"' AND GPM.GateRegisterType='Out'
                          AND GPM.CheckedBy Is NOT NULL 
                          And GPM.[CheckedByStatus]='Checked' 
                          AND GPM.[ApprovedBy] IS NOT NULL 
                          AND GPM.[ApprovedByStatus] Is NULL";
				}
				else if (tabType == 4)// "UnApprovedList")
				{
					sql = @"
                        SELECT  GPM.[Id]
                              ,GPM.[CompanyGroupId]
                              --,GPM.[CompanyId]
                              ,GPM.[PlantId]
                              ,GPM.[GatePassType]
                              ,GPM.[GatePassStatus]
                              ,REPLACE(CONVERT(CHAR(11), GPM.[ReturnableDate], 106),' ','-') AS ReturnableDate
                              ,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
                              ,GPM.[FromEmployeeId]
	                          ,EI.EmployeeName SenderName
                              ,GPM.[Through]
                              ,GPM.[CourierName]
                              ,GPM.[RunnerEmployeeId]
	                          ,EI1.EmployeeName RunnerEmployee
                              ,GPM.[ToType]
                              ,GPM.[ToPartyCode]
	                          ,p.UserName Vendor
                              ,GPM.[ToBuyerId]
	                          ,BUYer.UserName BuyerName
                              ,GPM.[ToPlantId]
	                          ,Plant.UserName PlantName
                              ,GPM.[ToUnitId]
	                          ,Unit.UserName UnitName
                              ,GPM.[ToDivisionId]
	                          ,Division.UserName DivisionName
                              ,GPM.[ToDepartment]
	                          ,Department.UserName DepartmentName
                              ,GPM.[DepartmentEmployeeId]
	                          ,EI2.EmployeeName DepartmentEmployee
                              ,GPM.[OtherCompanyName]
                              ,GPM.[PersonName]
                              ,GPM.[MobileNo]
                              ,GPM.[Address]
                              ,GPM.[Remarks]
                              ,GPM.[CheckedBy]
                              ,EI5.EmployeeName CheckedByEmployee                           
                              ,GPM.[CheckedByStatus]
                              ,GPM.[CheckedHoldRejectReason]
                              ,GPM.[ApprovedBy]
                              ,EI6.EmployeeName ApprovedByEmployee      
                              ,GPM.[ApprovedByStatus]
                              ,GPM.[ApprovedHoldRejectReason]
                              ,GPM.[SenderSecurityEmployeeId]
	                           ,EI3.EmployeeName SenderSecurityEmployee
                              ,GPM.[SenderSecurityApprovedStatus]
                              ,GPM.[ReceiverSecurityEmployeeId]
	                          ,EI4.EmployeeName ReceiverSecurityEmployee
                              ,GPM.[ReceiverSecurityApprovedStatus]
                              ,GPM.[VendorBuyerOtherCompanyReceivedStatus]
                              ,GPM.[AddedBy]
                              ,GPM.[AddedDate]
                              ,GPM.[AddedFromIP]
                              ,GPM.[UpdatedBy]
                              ,GPM.[UpdatedDate]
                              ,GPM.[UpdatedFromIP],GPM.ChallanNo,GPM.GateRegisterType
                          FROM [TRN].[GatePassMaster] GPM
                          LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.FromEmployeeId
                          LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.RunnerEmployeeId
                          LEft JOIN hkp.Party P On p.code=GPM.ToPartyCode
                          LEft JOIN [HKP].[Buyer] BUYer On BUYer.Id=GPM.ToBuyerId
                          LEFT JOIN org.Plant Plant ON Plant.id=GPM.ToPlantId
                          LEFT JOIN org.Unit Unit ON Unit.id=GPM.ToUnitId
                          LEFT JOIN org.Division Division ON Division.id=GPM.ToDivisionId
                          LEFT JOIN org.Department Department ON Department.id=GPM.ToDepartment
                          LEFT JOIN Employeeinformation EI2 on EI2.SystemId= GPM.DepartmentEmployeeId
                          LEFT JOIN Employeeinformation EI3 on EI3.SystemId= GPM.SenderSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI4 on EI4.SystemId= GPM.ReceiverSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI5 on EI5.SystemId= GPM.CheckedBy
                          LEFT JOIN Employeeinformation EI6 on EI6.SystemId= GPM.ApprovedBy
                          Where GPM.ApprovedBy='" + identity.EmployeeId + @"' AND GPM.GateRegisterType='Out'
                          AND GPM.CheckedBy Is NOT NULL 
                          And GPM.[CheckedByStatus]='Checked' 
                           AND GPM.[ApprovedBy] IS NOT NULL 
                          AND GPM.[ApprovedByStatus] ='For Approval'";
				}
				else if (tabType == 5)// "HoldRejectApprovedList")
				{
					sql = @"
                        SELECT  GPM.[Id]
                              ,GPM.[CompanyGroupId]
                              --,GPM.[CompanyId]
                              ,GPM.[PlantId]
                              ,GPM.[GatePassType]
                              ,GPM.[GatePassStatus]
                              ,REPLACE(CONVERT(CHAR(11), GPM.[ReturnableDate], 106),' ','-') AS ReturnableDate
                              ,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
                              ,GPM.[FromEmployeeId]
	                          ,EI.EmployeeName SenderName
                              ,GPM.[Through]
                              ,GPM.[CourierName]
                              ,GPM.[RunnerEmployeeId]
	                          ,EI1.EmployeeName RunnerEmployee
                              ,GPM.[ToType]
                              ,GPM.[ToPartyCode]
	                          ,p.UserName Vendor
                              ,GPM.[ToBuyerId]
	                          ,BUYer.UserName BuyerName
                              ,GPM.[ToPlantId]
	                          ,Plant.UserName PlantName
                              ,GPM.[ToUnitId]
	                          ,Unit.UserName UnitName
                              ,GPM.[ToDivisionId]
	                          ,Division.UserName DivisionName
                              ,GPM.[ToDepartment]
	                          ,Department.UserName DepartmentName
                              ,GPM.[DepartmentEmployeeId]
	                          ,EI2.EmployeeName DepartmentEmployee
                              ,GPM.[OtherCompanyName]
                              ,GPM.[PersonName]
                              ,GPM.[MobileNo]
                              ,GPM.[Address]
                              ,GPM.[Remarks]
                              ,GPM.[CheckedBy]
                              ,EI5.EmployeeName CheckedByEmployee                           
                              ,GPM.[CheckedByStatus]
                              ,GPM.[CheckedHoldRejectReason]
                              ,GPM.[ApprovedBy]
                              ,EI6.EmployeeName ApprovedByEmployee      
                              ,GPM.[ApprovedByStatus]
                              ,GPM.[ApprovedHoldRejectReason]
                              ,GPM.[SenderSecurityEmployeeId]
	                           ,EI3.EmployeeName SenderSecurityEmployee
                              ,GPM.[SenderSecurityApprovedStatus]
                              ,GPM.[ReceiverSecurityEmployeeId]
	                          ,EI4.EmployeeName ReceiverSecurityEmployee
                              ,GPM.[ReceiverSecurityApprovedStatus]
                              ,GPM.[VendorBuyerOtherCompanyReceivedStatus]
                              ,GPM.[AddedBy]
                              ,GPM.[AddedDate]
                              ,GPM.[AddedFromIP]
                              ,GPM.[UpdatedBy]
                              ,GPM.[UpdatedDate]
                              ,GPM.[UpdatedFromIP],GPM.ChallanNo,GPM.GateRegisterType
                          FROM [TRN].[GatePassMaster] GPM
                          LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.FromEmployeeId
                          LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.RunnerEmployeeId
                          LEft JOIN hkp.Party P On p.code=GPM.ToPartyCode
                          LEft JOIN [HKP].[Buyer] BUYer On BUYer.Id=GPM.ToBuyerId
                          LEFT JOIN org.Plant Plant ON Plant.id=GPM.ToPlantId
                          LEFT JOIN org.Unit Unit ON Unit.id=GPM.ToUnitId
                          LEFT JOIN org.Division Division ON Division.id=GPM.ToDivisionId
                          LEFT JOIN org.Department Department ON Department.id=GPM.ToDepartment
                          LEFT JOIN Employeeinformation EI2 on EI2.SystemId= GPM.DepartmentEmployeeId
                          LEFT JOIN Employeeinformation EI3 on EI3.SystemId= GPM.SenderSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI4 on EI4.SystemId= GPM.ReceiverSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI5 on EI5.SystemId= GPM.CheckedBy
                          LEFT JOIN Employeeinformation EI6 on EI6.SystemId= GPM.ApprovedBy
                          Where GPM.ApprovedBy='" + identity.EmployeeId + @"' AND GPM.GateRegisterType='Out'
                          AND GPM.CheckedBy Is NOT NULL 
                          And GPM.[CheckedByStatus]='Checked' 
                          AND GPM.[ApprovedBy] IS NOT NULL 
                          AND GPM.[ApprovedByStatus]='Hold' Or  GPM.[ApprovedByStatus]='Reject'
                          AND SenderSecurityEmployeeId is not null";
				}
				else if (tabType == 6)// "ApprovedList")
				{
					sql = @"
                        SELECT  GPM.[Id]
                              ,GPM.[CompanyGroupId]
                              --,GPM.[CompanyId]
                              ,GPM.[PlantId]
                              ,GPM.[GatePassType]
                              ,GPM.[GatePassStatus]
                              ,REPLACE(CONVERT(CHAR(11), GPM.[ReturnableDate], 106),' ','-') AS ReturnableDate
                              ,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
                              ,GPM.[FromEmployeeId]
	                          ,EI.EmployeeName SenderName
                              ,GPM.[Through]
                              ,GPM.[CourierName]
                              ,GPM.[RunnerEmployeeId]
	                          ,EI1.EmployeeName RunnerEmployee
                              ,GPM.[ToType]
                              ,GPM.[ToPartyCode]
	                          ,p.UserName Vendor
                              ,GPM.[ToBuyerId]
	                          ,BUYer.UserName BuyerName
                              ,GPM.[ToPlantId]
	                          ,Plant.UserName PlantName
                              ,GPM.[ToUnitId]
	                          ,Unit.UserName UnitName
                              ,GPM.[ToDivisionId]
	                          ,Division.UserName DivisionName
                              ,GPM.[ToDepartment]
	                          ,Department.UserName DepartmentName
                              ,GPM.[DepartmentEmployeeId]
	                          ,EI2.EmployeeName DepartmentEmployee
                              ,GPM.[OtherCompanyName]
                              ,GPM.[PersonName]
                              ,GPM.[MobileNo]
                              ,GPM.[Address]
                              ,GPM.[Remarks]
                              ,GPM.[CheckedBy]
                              ,EI5.EmployeeName CheckedByEmployee                           
                              ,GPM.[CheckedByStatus]
                              ,GPM.[CheckedHoldRejectReason]
                              ,GPM.[ApprovedBy]
                              ,EI6.EmployeeName ApprovedByEmployee      
                              ,GPM.[ApprovedByStatus]
                              ,GPM.[ApprovedHoldRejectReason]
                              ,GPM.[SenderSecurityEmployeeId]
	                           ,EI3.EmployeeName SenderSecurityEmployee
                              ,GPM.[SenderSecurityApprovedStatus]
                              ,GPM.[ReceiverSecurityEmployeeId]
	                          ,EI4.EmployeeName ReceiverSecurityEmployee
                              ,GPM.[ReceiverSecurityApprovedStatus]
                              ,GPM.[VendorBuyerOtherCompanyReceivedStatus]
                              ,GPM.[AddedBy]
                              ,GPM.[AddedDate]
                              ,GPM.[AddedFromIP]
                              ,GPM.[UpdatedBy]
                              ,GPM.[UpdatedDate]
                              ,GPM.[UpdatedFromIP],GPM.ChallanNo,GPM.GateRegisterType
                          FROM [TRN].[GatePassMaster] GPM
                          LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.FromEmployeeId
                          LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.RunnerEmployeeId
                          LEft JOIN hkp.Party P On p.code=GPM.ToPartyCode
                          LEft JOIN [HKP].[Buyer] BUYer On BUYer.Id=GPM.ToBuyerId
                          LEFT JOIN org.Plant Plant ON Plant.id=GPM.ToPlantId
                          LEFT JOIN org.Unit Unit ON Unit.id=GPM.ToUnitId
                          LEFT JOIN org.Division Division ON Division.id=GPM.ToDivisionId
                          LEFT JOIN org.Department Department ON Department.id=GPM.ToDepartment
                          LEFT JOIN Employeeinformation EI2 on EI2.SystemId= GPM.DepartmentEmployeeId
                          LEFT JOIN Employeeinformation EI3 on EI3.SystemId= GPM.SenderSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI4 on EI4.SystemId= GPM.ReceiverSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI5 on EI5.SystemId= GPM.CheckedBy
                          LEFT JOIN Employeeinformation EI6 on EI6.SystemId= GPM.ApprovedBy
                          Where GPM.ApprovedBy='" + identity.EmployeeId + @"' AND GPM.GateRegisterType='Out'
                          AND GPM.CheckedBy Is NOT NULL 
                           And Isnull(GPM.[CheckedByStatus],'')='Checked' 
                          AND GPM.[ApprovedBy] IS NOT NULL 
                          AND Isnull(GPM.[ApprovedByStatus],'')='Approved'";
				}

				else if (tabType == 9)//"UnDispatchList")
				{
					sql = @"
                        SELECT  GPM.[Id]
                              ,GPM.[CompanyGroupId]
                              --,GPM.[CompanyId]
                              ,GPM.[PlantId]
                              ,GPM.[GatePassType]
                              ,GPM.[GatePassStatus]
                              ,REPLACE(CONVERT(CHAR(11), GPM.[ReturnableDate], 106),' ','-') AS ReturnableDate
                              ,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
                              ,GPM.[FromEmployeeId]
	                          ,EI.EmployeeName SenderName
                              ,GPM.[Through]
                              ,GPM.[CourierName]
                              ,GPM.[RunnerEmployeeId]
	                          ,EI1.EmployeeName RunnerEmployee
                              ,GPM.[ToType]
                              ,GPM.[ToPartyCode]
	                          ,p.UserName Vendor
                              ,GPM.[ToBuyerId]
	                          ,BUYer.UserName BuyerName
                              ,GPM.[ToPlantId]
	                          ,Plant.UserName PlantName
                              ,GPM.[ToUnitId]
	                          ,Unit.UserName UnitName
                              ,GPM.[ToDivisionId]
	                          ,Division.UserName DivisionName
                              ,GPM.[ToDepartment]
	                          ,Department.UserName DepartmentName
                              ,GPM.[DepartmentEmployeeId]
	                          ,EI2.EmployeeName DepartmentEmployee
                              ,GPM.[OtherCompanyName]
                              ,GPM.[PersonName]
                              ,GPM.[MobileNo]
                              ,GPM.[Address]
                              ,GPM.[Remarks]
                              ,GPM.[CheckedBy]
                              ,EI5.EmployeeName CheckedByEmployee                           
                              ,GPM.[CheckedByStatus]
                              ,GPM.[CheckedHoldRejectReason]
                              ,GPM.[ApprovedBy]
                              ,EI6.EmployeeName ApprovedByEmployee      
                              ,GPM.[ApprovedByStatus]
                              ,GPM.[ApprovedHoldRejectReason]
                              ,GPM.[SenderSecurityEmployeeId]
	                           ,EI3.EmployeeName SenderSecurityEmployee
                              ,GPM.[SenderSecurityApprovedStatus]
                              ,GPM.[ReceiverSecurityEmployeeId]
	                          ,EI4.EmployeeName ReceiverSecurityEmployee
                              ,GPM.[ReceiverSecurityApprovedStatus]
                              ,GPM.[VendorBuyerOtherCompanyReceivedStatus]
                              ,GPM.[AddedBy]
                              ,GPM.[AddedDate]
                              ,GPM.[AddedFromIP]
                              ,GPM.[UpdatedBy]
                              ,GPM.[UpdatedDate]
                              ,GPM.[UpdatedFromIP],GPM.ChallanNo,GPM.GateRegisterType
                          FROM [TRN].[GatePassMaster] GPM
                          LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.FromEmployeeId
                          LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.RunnerEmployeeId
                          LEft JOIN hkp.Party P On p.code=GPM.ToPartyCode
                          LEft JOIN [HKP].[Buyer] BUYer On BUYer.Id=GPM.ToBuyerId
                          LEFT JOIN org.Plant Plant ON Plant.id=GPM.ToPlantId
                          LEFT JOIN org.Unit Unit ON Unit.id=GPM.ToUnitId
                          LEFT JOIN org.Division Division ON Division.id=GPM.ToDivisionId
                          LEFT JOIN org.Department Department ON Department.id=GPM.ToDepartment
                          LEFT JOIN Employeeinformation EI2 on EI2.SystemId= GPM.DepartmentEmployeeId
                          LEFT JOIN Employeeinformation EI3 on EI3.SystemId= GPM.SenderSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI4 on EI4.SystemId= GPM.ReceiverSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI5 on EI5.SystemId= GPM.CheckedBy
                          LEFT JOIN Employeeinformation EI6 on EI6.SystemId= GPM.ApprovedBy
                          --Where GPM.SenderSecurityEmployeeId='" + identity.EmployeeId + @"' AND GPM.GateRegisterType='Out'
                          Where GPM.CheckedBy Is NOT NULL 
                          And GPM.[CheckedByStatus]='Checked' 
                          AND GPM.[ApprovedBy] IS NOT NULL 
                          AND GPM.[ApprovedByStatus] ='Approved' AND isnull(GPM.GateOutStatus,0)=0";//AND GPM.SenderSecurityApprovedStatus IS null
				}
				else if (tabType == 7)//"HoldRejectDispatchList")
				{
					sql = @"
                        SELECT  GPM.[Id]
                              ,GPM.[CompanyGroupId]
                              --,GPM.[CompanyId]
                              ,GPM.[PlantId]
                              ,GPM.[GatePassType]
                              ,GPM.[GatePassStatus]
                              ,REPLACE(CONVERT(CHAR(11), GPM.[ReturnableDate], 106),' ','-') AS ReturnableDate
                              ,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
                              ,GPM.[FromEmployeeId]
	                          ,EI.EmployeeName SenderName
                              ,GPM.[Through]
                              ,GPM.[CourierName]
                              ,GPM.[RunnerEmployeeId]
	                          ,EI1.EmployeeName RunnerEmployee
                              ,GPM.[ToType]
                              ,GPM.[ToPartyCode]
	                          ,p.UserName Vendor
                              ,GPM.[ToBuyerId]
	                          ,BUYer.UserName BuyerName
                              ,GPM.[ToPlantId]
	                          ,Plant.UserName PlantName
                              ,GPM.[ToUnitId]
	                          ,Unit.UserName UnitName
                              ,GPM.[ToDivisionId]
	                          ,Division.UserName DivisionName
                              ,GPM.[ToDepartment]
	                          ,Department.UserName DepartmentName
                              ,GPM.[DepartmentEmployeeId]
	                          ,EI2.EmployeeName DepartmentEmployee
                              ,GPM.[OtherCompanyName]
                              ,GPM.[PersonName]
                              ,GPM.[MobileNo]
                              ,GPM.[Address]
                              ,GPM.[Remarks]
                              ,GPM.[CheckedBy]
                              ,EI5.EmployeeName CheckedByEmployee                           
                              ,GPM.[CheckedByStatus]
                              ,GPM.[CheckedHoldRejectReason]
                              ,GPM.[ApprovedBy]
                              ,EI6.EmployeeName ApprovedByEmployee      
                              ,GPM.[ApprovedByStatus]
                              ,GPM.[ApprovedHoldRejectReason]
                              ,GPM.[SenderSecurityEmployeeId]
	                           ,EI3.EmployeeName SenderSecurityEmployee
                              ,GPM.[SenderSecurityApprovedStatus]
                              ,GPM.[ReceiverSecurityEmployeeId]
	                          ,EI4.EmployeeName ReceiverSecurityEmployee
                              ,GPM.[ReceiverSecurityApprovedStatus]
                              ,GPM.[VendorBuyerOtherCompanyReceivedStatus]
                              ,GPM.[AddedBy]
                              ,GPM.[AddedDate]
                              ,GPM.[AddedFromIP]
                              ,GPM.[UpdatedBy]
                              ,GPM.[UpdatedDate]
                              ,GPM.[UpdatedFromIP],GPM.ChallanNo,GPM.GateRegisterType
                          FROM [TRN].[GatePassMaster] GPM
                          LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.FromEmployeeId
                          LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.RunnerEmployeeId
                          LEft JOIN hkp.Party P On p.code=GPM.ToPartyCode
                          LEft JOIN [HKP].[Buyer] BUYer On BUYer.Id=GPM.ToBuyerId
                          LEFT JOIN org.Plant Plant ON Plant.id=GPM.ToPlantId
                          LEFT JOIN org.Unit Unit ON Unit.id=GPM.ToUnitId
                          LEFT JOIN org.Division Division ON Division.id=GPM.ToDivisionId
                          LEFT JOIN org.Department Department ON Department.id=GPM.ToDepartment
                          LEFT JOIN Employeeinformation EI2 on EI2.SystemId= GPM.DepartmentEmployeeId
                          LEFT JOIN Employeeinformation EI3 on EI3.SystemId= GPM.SenderSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI4 on EI4.SystemId= GPM.ReceiverSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI5 on EI5.SystemId= GPM.CheckedBy
                          LEFT JOIN Employeeinformation EI6 on EI6.SystemId= GPM.ApprovedBy
                          --Where GPM.SenderSecurityEmployeeId='" + identity.EmployeeId + @"' AND GPM.GateRegisterType='Out'
                          Where GPM.CheckedBy Is NOT NULL 
                             And Isnull(GPM.[CheckedByStatus],'')='Checked' 
                          AND GPM.[ApprovedBy] IS NOT NULL 
                          AND Isnull(GPM.[ApprovedByStatus],'')='Approved'
                          AND GPM.SenderSecurityApprovedStatus='Hold' Or  AND GPM.SenderSecurityApprovedStatus='Reject'";


				}
				else if (tabType == 8)//"DispatchList")
				{
					sql = @"
                        SELECT  GPM.[Id]
                              ,GPM.[CompanyGroupId]
                              --,GPM.[CompanyId]
                              ,GPM.[PlantId]
                              ,GPM.[GatePassType]
                              ,GPM.[GatePassStatus]
                              ,REPLACE(CONVERT(CHAR(11), GPM.[ReturnableDate], 106),' ','-') AS ReturnableDate
                              ,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
                              ,GPM.[FromEmployeeId]
	                          ,EI.EmployeeName SenderName
                              ,GPM.[Through]
                              ,GPM.[CourierName]
                              ,GPM.[RunnerEmployeeId]
	                          ,EI1.EmployeeName RunnerEmployee
                              ,GPM.[ToType]
                              ,GPM.[ToPartyCode]
	                          ,p.UserName Vendor
                              ,GPM.[ToBuyerId]
	                          ,BUYer.UserName BuyerName
                              ,GPM.[ToPlantId]
	                          ,Plant.UserName PlantName
                              ,GPM.[ToUnitId]
	                          ,Unit.UserName UnitName
                              ,GPM.[ToDivisionId]
	                          ,Division.UserName DivisionName
                              ,GPM.[ToDepartment]
	                          ,Department.UserName DepartmentName
                              ,GPM.[DepartmentEmployeeId]
	                          ,EI2.EmployeeName DepartmentEmployee
                              ,GPM.[OtherCompanyName]
                              ,GPM.[PersonName]
                              ,GPM.[MobileNo]
                              ,GPM.[Address]
                              ,GPM.[Remarks]
                              ,GPM.[CheckedBy]
                              ,EI5.EmployeeName CheckedByEmployee                           
                              ,GPM.[CheckedByStatus]
                              ,GPM.[CheckedHoldRejectReason]
                              ,GPM.[ApprovedBy]
                              ,EI6.EmployeeName ApprovedByEmployee      
                              ,GPM.[ApprovedByStatus]
                              ,GPM.[ApprovedHoldRejectReason]
                              ,GPM.[SenderSecurityEmployeeId]
	                           ,EI3.EmployeeName SenderSecurityEmployee
                              ,GPM.[SenderSecurityApprovedStatus]
                              ,GPM.[ReceiverSecurityEmployeeId]
	                          ,EI4.EmployeeName ReceiverSecurityEmployee
                              ,GPM.[ReceiverSecurityApprovedStatus]
                              ,GPM.[VendorBuyerOtherCompanyReceivedStatus]
                              ,GPM.[AddedBy]
                              ,GPM.[AddedDate]
                              ,GPM.[AddedFromIP]
                              ,GPM.[UpdatedBy]
                              ,GPM.[UpdatedDate]
                              ,GPM.[UpdatedFromIP],GPM.ChallanNo,GPM.GateRegisterType
                          FROM [TRN].[GatePassMaster] GPM
                          LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.FromEmployeeId
                          LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.RunnerEmployeeId
                          LEft JOIN hkp.Party P On p.code=GPM.ToPartyCode
                          LEft JOIN [HKP].[Buyer] BUYer On BUYer.Id=GPM.ToBuyerId
                          LEFT JOIN org.Plant Plant ON Plant.id=GPM.ToPlantId
                          LEFT JOIN org.Unit Unit ON Unit.id=GPM.ToUnitId
                          LEFT JOIN org.Division Division ON Division.id=GPM.ToDivisionId
                          LEFT JOIN org.Department Department ON Department.id=GPM.ToDepartment
                          LEFT JOIN Employeeinformation EI2 on EI2.SystemId= GPM.DepartmentEmployeeId
                          LEFT JOIN Employeeinformation EI3 on EI3.SystemId= GPM.SenderSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI4 on EI4.SystemId= GPM.ReceiverSecurityEmployeeId
                          LEFT JOIN Employeeinformation EI5 on EI5.SystemId= GPM.CheckedBy
                          LEFT JOIN Employeeinformation EI6 on EI6.SystemId= GPM.ApprovedBy
                          --Where GPM.SenderSecurityEmployeeId='" + identity.EmployeeId + @"' AND GPM.GateRegisterType='Out'
                          Where GPM.CheckedBy Is NOT NULL 
                          And GPM.[CheckedByStatus]='Checked' 
                          AND GPM.[ApprovedBy] IS NOT NULL 
                          AND GPM.[ApprovedByStatus] ='Approved' AND isnull(GPM.GateOutStatus,0)=1";//AND GPM.SenderSecurityApprovedStatus='GateOut'
				}
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
			// return Json(_gateEntryService.PlantWiseGateCbo(IsSysAdmin, UserId, plantId), JsonRequestBehavior.AllowGet);
		}
		[Authorize, HttpPost]
		public void GatePassCheckedAndApproved(string Id, string PoValue, string CheckedApprovedStataus, string CheckedApprovedBy, string RejectReason, string UIType)
		{
			var ApprovedById = "";
			if (UIType == "gate-pass-checked")
			{
				try
				{


					PoValue = "0";
					//  var Id = GetPK();
					if (CheckedApprovedStataus == "Checked")
					{
						if (CheckedApprovedBy == null || CheckedApprovedBy == "")
						{
							throw new CustomException("Select Approved By");
						}
						ApprovedById = CheckedApprovedBy;
						//DailySendMailRequisitionApproved(RequisitionType, RequirmentType, CheckedBy, AuthorizedById, PoId, PreparedBY);

					}
					else
					{
						ApprovedById = null;

					}
					var Status = CheckedApprovedStataus;
					var UpdatedBy = "";
					var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
					var ip = identity.IPAddress;
					var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
					var AddedBy = identity.Name;
					var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
					var CompanyGroupId = identity.CompanyGroupId;
					var CompanyId = identity.CompanyId;
					var PlantId = identity.PlantId;
					string _sql = "Update TRN.GatePassMaster set CheckedByStatus='" + Status + "',ApprovedBy='" + ApprovedById + "',ApprovedByStatus='For Approval',CheckedHoldRejectReason='" + RejectReason + "' where id='" + Id + "'";
					_sqlRepository.ExecuteSqlCommand(_sql);
					string _sql1 = "Insert into TRN.GatePassSystemApprovalLog(" +
					"CompanyGroupId,CompanyId,PlantId,ApprovedBy,Date,ReqValue,Status,AddedBy,AddedDate,AddedFromIp,UpdatedBy,UpdatedDate,UpdatedFromIp,GatePassId) " +
					"values ('" + CompanyGroupId + "'," +
					"'" + CompanyId + "'," +
					"'" + PlantId + "'," +
					"'" + AddedBy + "'," +
					"'" + AddedDate + "'," +
					"'" + PoValue + "'," +
					"'" + Status + "'," +
					"'" + AddedBy + "'," +
					"'" + AddedDate + "'," +
					"'" + ip + "'," +
					"'" + UpdatedBy + "'," +
					"'" + updatedDate + "', " +
					"'" + ip + "','" + Id + "')";
					_sqlRepository.ExecuteSqlCommand(_sql1);
				}
				catch (Exception ex)
				{
					throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
				}

			}
			else if (UIType == "gate-pass-approved")
			{
				try
				{
					var IsApproved = 0;

					PoValue = "0";
					//  var Id = GetPK();
					if (CheckedApprovedStataus == "Approved")
					{
						IsApproved = 1;
						ApprovedById = CheckedApprovedBy;
					}
					else
					{
						IsApproved = 0;
						ApprovedById = null;
					}
					var Status = CheckedApprovedStataus;
					var UpdatedBy = "";
					var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
					var ip = identity.IPAddress;
					var updatedDate = Convert.ToDateTime(DateTime.Now).ToString();
					var AddedBy = identity.Name;
					var AddedDate = Convert.ToDateTime(DateTime.Now).ToString();
					var CompanyGroupId = identity.CompanyGroupId;
					var CompanyId = identity.CompanyId;
					var PlantId = identity.PlantId;
					string _sql = "Update TRN.GatePassMaster set ApprovedByStatus='" + Status + "',ApprovedHoldRejectReason='" + RejectReason + "' where id='" + Id + "'";//,SenderSecurityEmployeeId='" + ApprovedById + "'
					_sqlRepository.ExecuteSqlCommand(_sql);
					string _sql1 = "Insert into TRN.GatePassSystemApprovalLog(" +
					"CompanyGroupId,CompanyId,PlantId,ApprovedBy,Date,ReqValue,Status,AddedBy,AddedDate,AddedFromIp,UpdatedBy,UpdatedDate,UpdatedFromIp,GatePassId) " +
					"values ('" + CompanyGroupId + "'," +
					"'" + CompanyId + "'," +
					"'" + PlantId + "'," +
					"'" + AddedBy + "'," +
					"'" + AddedDate + "'," +
					"'" + PoValue + "'," +
					"'" + Status + "'," +
					"'" + AddedBy + "'," +
					"'" + AddedDate + "'," +
					"'" + ip + "'," +
					"'" + UpdatedBy + "'," +
					"'" + updatedDate + "', " +
					"'" + ip + "','" + Id + "')";
					_sqlRepository.ExecuteSqlCommand(_sql1);
				}
				catch (Exception ex)
				{
					throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
				}
			}

		}
		[Authorize, HttpGet]
		public JsonResult GetGatePassCheckedBy()
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				var sql = @"select E.SystemId As Value, E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='GatePassCheckedBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}

		[Authorize, HttpGet]
		public JsonResult GetGatePassApproveddBy()
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				var sql = @"select E.SystemId As Value, E.SystemId+'-'+E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='GatePassApproveBy' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}
		[Authorize, HttpGet]
		public JsonResult GetGatePassTobeApproveddSecurityBy()
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				var sql = @"select E.SystemId As Value, E.SystemId+'-'+E.EmployeeName As Text from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where  A.ActionStatus='GatePassApproveBySecurity' AND E.EmployeeStatus='Active'";//A.PlantId='" + identity.PlantId + "' AND
				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
		}
		#endregion





		#region Employee Gate Pass System
		[Authorize, HttpGet]
		public JsonResult GetDataForGatePass()
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";

				sql = @"
                        SELECT EI.SystemId FromEmployeeId
	                    , EI.EmployeeName EmployeeName                              
                        ,EI.PlantId
	                    ,Plant.UserName PlantName
                        ,EI.UnitId
	                    ,Unit.UserName UnitName
                        ,EI.DivisionId
	                    ,Division.UserName DivisionName
                        ,EI.DepartmentId
	                    ,Department.UserName DepartmentName
	                    ,Section.UserName SectionName
	                    ,Section.Id SectionId
                        ,SubSection.UserName SebSectionName
	                    ,SubSection.Id SubSectionId
	                    ,line.UserName LineName
	                    ,line.Id LineId
	                    ,LegalDesignation.UserName LegalDesignationName
	                    ,LegalDesignation.Id LegalDesignationId
	                    ,Designation.UserName DesignationName
	                    ,Designation.Id DesignationId
                    FROM Employeeinformation EI   
left join MST.ManpowerBudget MPB on MPB.Id=ei.BudgetCode
                            left join org.Entity E on E.Id =MPB.EntityId
							left join ORG.Position P on P.Id=MPB.PositionID
                    LEFT JOIN org.Plant Plant ON Plant.id=EI.PlantId
                    LEFT JOIN org.Unit Unit ON Unit.id=E.UnitId
                    LEFT JOIN org.Division Division ON Division.id=P.DivisionId
                    LEFT JOIN org.Department Department ON Department.id=P.DepartmentId
                    LEFT JOIN Org.Section Section ON section.Id=p.SectionId
                    LEFT JOIN org.SubSection  SubSection ON SubSection.Id=p.SubSectionId
                    LEFT JOIN org.Line line ON line.id=MPB.LineId
                    Left JOIN hkp.LegalDesignation LegalDesignation ON LegalDesignation.Id=EI.LegalDesignationId
                    LEFT JOIN hkp.Designation Designation On Designation.Id=EI.GivenDesignationId
						  where EI.SystemId='" + identity.EmployeeId + @"'";

				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
			// return Json(_gateEntryService.PlantWiseGateCbo(IsSysAdmin, UserId, plantId), JsonRequestBehavior.AllowGet);
		}

		#endregion

		//Gateentry Register report by nurul huda
		[Authorize, HttpGet]
		public ActionResult GateEntryReportExcel(string fromDate, string toDate)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				// if (string.IsNullOrEmpty(MasterLCList))
				//   throw new Exception("Please select at least one master Order");

				ExcelEngine excelEngine = new ExcelEngine();

				IWorkbook workbook = GetGatenntryRegisterList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId ,  fromDate,  toDate);

				string strFileName = "Gate Entry Register.xlsx";
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
		public ActionResult GatenntryRegisterListPdf(string fromDate,string toDate)

		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				ExcelEngine excelEngine = new ExcelEngine();
				IWorkbook workbook = GetGatenntryRegisterList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId,fromDate,toDate);
				string strFileName = "Gate Entry Register.pdf";
				ExcelToPdfConverter convert = new ExcelToPdfConverter(workbook);
				PdfDocument pdfDoc = convert.Convert();
				workbook.Close();
				pdfDoc.Save(strFileName, System.Web.HttpContext.Current.Response, HttpReadType.Save);
				//workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);

			}
			catch (Exception ex)
			{
				return Json(ex.Message, JsonRequestBehavior.AllowGet);
			}
			return null;
		}

		// GetExpenseBookingApprovalList  GatenntryRegisterList
		[Authorize, HttpGet]
		private IWorkbook GetGatenntryRegisterList(string companyGroupId, string companyId, string plantId, string fromDate, string toDate)
		{

			//Start EmployeeAdvanceDueList


			ExcelEngine excelEngine = new ExcelEngine();
			//Instantiate the Excel application object
			IApplication application = excelEngine.Excel;

			//Set the default application version
			application.DefaultVersion = ExcelVersion.Excel2013;

			//Load the existing Excel workbook into IWorkbook
			var report = new ReportUtility();
			
			IWorkbook workbook = application.Workbooks.Create(1);

			//Get the first worksheet in the workbook into IWorksheet
			IWorksheet worksheet = workbook.Worksheets[0];

            //DataTable dtGatenntryRegisterList = _sqlRepository.GetDataTable(@"SELECT  ROW_NUMBER() Over (Order by GE.Id) As [S.N], CG.UserName CompanyGroup
            //             ,C.UserName Company
            //             ,P.UserName PlantName
            //            ,Format(GE.EntryDate,'dd-MMM-yyyy') EntryDate
            //             ,party.UserName PartyName
            //             --,InvParty.UserName InvoicingPartyName
            //             --,DeliParty.UserName DeliveryPartyName
            //             ,GE.Description
            //             ,GE.PackageQty
            //             ,GE.ModeofTransport
            //             ,GE.Bill
            //             ,GE.PersonName
            //             ,GE.MobileNo
            //             ,GE.Remarks
            //             ,Format (GE.GateEntryTime, 'dd-MMM-yyyy') GateEntryTime
            //             ,EI.EmployeeName
            //             ,EI1.EmployeeName GateEntryBy
            //             ,GE.GateEntryType
            //             ,GE.LocalImported ,IR.Id GRNNo,GE.Id GateEntryNo
            //             FROM 
            //             [TRN].[GateEntry] GE
            //             LEFT JOIN ORG.CompanyGroup CG ON CG.Id=GE.CompanyGroupId
            //             LEFT JOIN ORG.Company C ON C.Id=GE.CompanyId
            //             LEFT JOIN ORG.Plant P On P.Id=GE.PlantId
            //            LEFT JOIN HKP.Party party ON party.Id=GE.PartyId
            //            -- LEFT JOIN HKP.Party InvParty ON party.Id=GE.InvoicingPartyPlantId
            //             --LEFT JOIN HKP.Party DeliParty ON party.Id=GE.DeliveryPartyPlantId
            //             LEFT JOIN [dbo].[PlantWiseGate] PWG ON PWG.Id=GE.PlantWiseGateId
            //             LEFT JOIN EmployeeInformation EI ON EI.SystemId=GE.EmployeeId
            //             LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=GE.EmployeeId
            //             left Join TRN.InventoryReceive IR On IR.GateEntryNo=GE.Id");



            DataTable dtGatenntryRegisterList = _sqlRepository.GetDataTable(@"SELECT  ROW_NUMBER() Over (Order by GE.Id) As [S.N],
							'Gate Entry For GRN' GateEntryFor
							,CG.Id CompanyGroupId
							, CG.UserName CompanyGroup
							,C.UserName Company
							,p.Id PlantId
							,P.UserName PlantName
							,Format(GE.EntryDate,'dd-MMM-yyyy') EntryDate
							,party.UserName PartyName
							--,InvParty.UserName InvoicingPartyName
							--,DeliParty.UserName DeliveryPartyName
							,GE.Description
							,cast(CAST(GE.PackageQty AS bigint) as nvarchar(255)) PackageQty
							,GE.ModeofTransport
							,GE.Bill
							,GE.PersonName
							,GE.MobileNo
							,GE.Remarks
							,Format (GE.GateEntryTime, N'hh:mm tt') GateEntryTime
							,EI.EmployeeName
							,EI1.EmployeeName GateEntryBy
							,GE.GateEntryType
							,GE.LocalImported 
							,IR.Id GRNNo
							,GE.Id GateEntryNo

							--------From Here You have to Add Column--
							,'' GatePassType 
							,'' GatePassStatus
							,'' GatePassDetailId
							,'' GatePassMasterId 
							,'' MaterialMasterId 
							,''  Material 
							, '' ArticleId
							, '' Article
							,'' FirstCharacteristicsId
							,'' FirstCharacteristics
							, ''FirstCharacteristicsValueId
							,  ''FirstCharacteristicsValue
							,''SecondCharacteristicsId
							, ''SecondCharacteristics
							, ''SecondCharacteristicsValueId
							, '' SecondCharacteristicsValue
							,''ThirdCharacteristicsId
							, '' ThirdCharacteristics
							,''ThirdCharacteristicsValueId
							, ''ThirdCharacteristicsValue
							,''MaterialDetail 
							,0 TransactionQty 
							, ''TransactionUoMId 
							, '' UOM
							, ''GPDRemarks 
							, 0 IsReturnable
							,'' GPDReturnableDate
							,0 IsMutilated 
							, 0 Rate
							, '' ChallanNo 
							, ''ChallanNoDetailId 
							, ''PurposeofGatePass 
							,''ConsignmentNo 
							, '' GPDDriverName
							--- Master Table

							,'' GPMReturnableDate
							, ''FromEmployeeId 
							,'' FromEmployee
							,''Through
							,''CourierName
							,'' RunnerEmployeeId
							,'' ToType
							,'' ToPartyCode 
							,''  ToParty
							,'' ToBuyerId
							,'' ToBuyer
							,''ToPlantId
							, '' ToPlant
							, ''ToUnitId 
							, '' ToUnit
							,''ToDivisionId
							, '' ToDivision
							,'' ToDepartmentId
							,'' ToDepartment
							,''DepartmentEmployeeId
							, '' DepartmentEmployee
							, ''OtherCompanyName
							,''GatePassPersonName
							,''GatePassMobileNo
							,''Address
							,'' GPMRemarks
							,''CheckedBy
							, '' CheckedByEmployee
							, ''CheckedByStatus 
							, ''CheckedHoldRejectReason
							, ''ApprovedBy 
							, '' ApprovedByEmployee
							,''ApprovedHoldRejectReason
							, ''SenderSecurityEmployeeId 
							, '' SenderSecurityEmployee 
							, ''SenderSecurityApprovedStatus
							,''ReceiverSecurityEmployeeId 
							,'' ReceiverSecurityEmployee 
							, ''ReceiverSecurityApprovedStatus 
							, ''VendorBuyerOtherCompanyReceivedStatus
							,'' GPMChallanNo 
							, '' TransportAgentMobileNo
							, '' TransportAgentName 
							, '' VehicleNo
							, '' GateOutStatus
							, '' GateRegisterType
							, '' ReceivedChallanNO
							, ''InvoiceNo
							, '' GPMPurposeOfGatePass
							, '' GPMConsignmentNo
							, ''DriverName
							FROM 
							[TRN].[GateEntry] GE
							LEFT JOIN ORG.CompanyGroup CG ON CG.Id=GE.CompanyGroupId
							LEFT JOIN ORG.Company C ON C.Id=GE.CompanyId
							LEFT JOIN ORG.Plant P On P.Id=GE.PlantId
							LEFT JOIN HKP.Party party ON party.Id=GE.PartyId
							-- LEFT JOIN HKP.Party InvParty ON party.Id=GE.InvoicingPartyPlantId
							--LEFT JOIN HKP.Party DeliParty ON party.Id=GE.DeliveryPartyPlantId
							LEFT JOIN [dbo].[PlantWiseGate] PWG ON PWG.Id=GE.PlantWiseGateId
							LEFT JOIN EmployeeInformation EI ON EI.SystemId=GE.EmployeeId
							LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=GE.EmployeeId
							left Join TRN.InventoryReceive IR On IR.GateEntryNo=GE.Id

					where GE.EntryDate between '" + fromDate +@"' AND '"+ toDate + @"'

					UNION ALL

						select  ROW_NUMBER() Over (Order by gpm.Id) As [S.N],
						'Gate Pass For Material' GateEntryFor
						, gpm.CompanyGroupId
						, cg.UserName as CompanyGroup
						,'' Company
						,gpm.PlantId 
						, p.UserName as PlantName
						,format(gpm.GatePassEntryDate,'dd-MMM-yyyy') as EntryDate
						,'' PartyName
						,'' Description
						, gpm.NoOfPackages PackageQty
						,'' ModeofTransport
						,'' Bill
						,'' PersonName
						,'' MobileNo
						,'' Remarks
						,'' GateEntryTime
						,'' EmployeeName
						,'' GateEntryBy
						,'' GateEntryType
						,'' LocalImported 
						,'' GRNNo
						,gpm.Id GateEntryNo
						--------From Here You have to Add Column--
						, gpm.GatePassType 
						, gpm.GatePassStatus
						,gpd.Id as GatePassDetailId,gpd.GatePassMasterId , gpd.MaterialMasterId ,mm.UserName as Material , gpd.ArticleId, mma.StandardName as Article,
						gpd.FirstCharacteristicsId, cf.UserName as FirstCharacteristics, gpd.FirstCharacteristicsValueId, cvf.UserName as FirstCharacteristicsValue,
						gpd.SecondCharacteristicsId, cs.UserName as SecondCharacteristics, gpd.SecondCharacteristicsValueId, cvs.UserName as SecondCharacteristicsValue,
						gpd.ThirdCharacteristicsId, ct.UserName as ThirdCharacteristics, gpd.ThirdCharacteristicsValueId, cvt.UserName as ThirdCharacteristicsValue,
						gpd.MaterialDetail ,gpd.TransactionQty , gpd.TransactionUoMId , uom.Username as UOM, gpd.Remarks GPDRemarks, gpd.IsReturnable , 
						format(gpd.ReturnableDate,'dd-MMM-yyyy') as GPDReturnableDate,gpd.IsMutilated , gpd.Rate, gpd.ChallanNo , gpd.ChallanNoDetailId , gpd.PurposeofGatePass ,
						gpd.ConsignmentNo , gpd.DriverName as GPDDriverName,
						--- Master Table

						format(gpm.ReturnableDate,'dd-MMM-yyyy') as GPMReturnableDate, gpm.FromEmployeeId ,
						fromemp.EmployeeName as FromEmployee, gpm.Through, gpm.CourierName,gpm.RunnerEmployeeId, gpm.ToType, gpm.ToPartyCode , 
						pty.Username as ToParty, gpm.ToBuyerId, b.UserName as ToBuyer,gpm.ToPlantId, pl.Username as ToPlant, gpm.ToUnitId , u.UserName as ToUnit,
						gpm.ToDivisionId , div.Username as ToDivision,gpm.ToDepartment as ToDepartmentId, dept.UserName as ToDepartment,
						gpm.DepartmentEmployeeId, DeptEmp.EmployeeName as DepartmentEmployee, gpm.OtherCompanyName,gpm.PersonName GatePassPersonName,gpm.MobileNo GatePassMobileNo,gpm.Address,gpm.Remarks as GPMRemarks,
						gpm.CheckedBy, CheckEmp.EmployeeName as CheckedByEmployee, gpm.CheckedByStatus , gpm.CheckedHoldRejectReason, gpm.ApprovedBy , ApprEmp.EmployeeName as ApprovedByEmployee,
						gpm.ApprovedHoldRejectReason, gpm.SenderSecurityEmployeeId , SenderSecEmp.EmployeeName as SenderSecurityEmployee , gpm.SenderSecurityApprovedStatus,
						gpm.ReceiverSecurityEmployeeId , ReceiverSecEmp.EmployeeName as ReceiverSecurityEmployee , gpm.ReceiverSecurityApprovedStatus , gpm.VendorBuyerOtherCompanyReceivedStatus,
						gpm.ChallanNo as GPMChallanNo , gpm.TransportAgentMobileNo, gpm.TransportAgentName , gpm.VehicleNo, gpm.GateOutStatus, gpm.GateRegisterType, gpm.ReceivedChallanNO,
						gpm.InvoiceNo, gpm.PurposeofGatePass as GPMPurposeOfGatePass, gpm.ConsignmentNo as GPMConsignmentNo, gpm.DriverName
						from trn.GatePassDetails gpd
						left join mst.MaterialMasterArticle mma on mma.Id = gpd.ArticleId
						left join mst.MaterialMaster mm on mm.Id = mma.MaterialMasterId
						left join hkp.Characteristics cf on cf.Id = gpd.FirstCharacteristicsId
						left join hkp.CharacteristicsValue cvf on cvf.Id = gpd.FirstCharacteristicsValueId
						left join hkp.Characteristics cs on cs.Id = gpd.SecondCharacteristicsId
						left join hkp.CharacteristicsValue cvs on cvs.Id = gpd.SecondCharacteristicsValueId
						left join hkp.Characteristics ct on ct.Id = gpd.ThirdCharacteristicsId
						left join hkp.CharacteristicsValue cvt on cvt.Id = gpd.ThirdCharacteristicsValueId
						left join scs.UnitOfMeasurement uom on uom.Id = gpd.TransactionUoMId
						left join trn.GatePassMaster gpm on gpm.Id = gpd.GatePassMasterId
						left join org.CompanyGroup cg on cg.Id = gpm.CompanyGroupId
						left join org.Plant p on p.Id = gpm.PlantId
						left join dbo.EmployeeInformation FromEmp on fromemp.SystemId = gpm.FromEmployeeId
						left join dbo.EmployeeInformation DeptEmp on DeptEmp.SystemId = gpm.DepartmentEmployeeId
						left join dbo.EmployeeInformation CheckEmp on CheckEmp.SystemId = gpm.CheckedBy
						left join dbo.EmployeeInformation ApprEmp on ApprEmp.SystemId = gpm.ApprovedBy
						left join dbo.EmployeeInformation SenderSecEmp on SenderSecEmp.SystemId = gpm.SenderSecurityEmployeeId
						left join dbo.EmployeeInformation ReceiverSecEmp on ReceiverSecEmp.SystemId = gpm.ReceiverSecurityEmployeeId
						left join hkp.Party pty on pty.Id = gpm.ToPartyCode
						left join hkp.Buyer b on b.Id = gpm.ToBuyerId
						left join org.Plant pl on pl.Id = gpm.ToPlantId
						left join org.Unit u on u.Id = gpm.ToUnitId
						left join org.Division div on div.Id= gpm.ToDivisionId
						left join org.Department dept on dept.Id = gpm.ToDepartment
					where gpm.GatePassEntryDate between '" + fromDate + @"' AND '" + toDate + @"'");

            if (dtGatenntryRegisterList.Rows.Count == 0)
				throw new Exception("No data found");

			worksheet.Name = "GateEntryRegister";

			int COL = 1; int ROW = 5;
			int startCol = COL;

			// worksheet[ROW, COL].Text = "Employee Advance Due List Details:";
			// worksheet[ROW, COL].CellStyle.Font.Bold = true;
			//  ROW++;
			worksheet[ROW, COL].Text = "SL";
			int ColSl = COL;
			worksheet[ROW, COL].ColumnWidth = 4;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			COL++;

			worksheet[ROW, COL].Text = "Gate Entry For";
			int colGateEntryFor = COL;
			worksheet[ROW, COL].ColumnWidth = 12;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			COL++;

			//worksheet[ROW, COL].Text = "Company Group";
			//int colCompanyGroup = COL;
			//worksheet[ROW, COL].ColumnWidth = 12;
			//worksheet[ROW, COL].CellStyle.Font.Bold = true;
			//COL++;

			//worksheet[ROW, COL].Text = "Company";
			//int colCompany = COL;
			//worksheet[ROW, COL].ColumnWidth = 25;
			//worksheet[ROW, COL].CellStyle.Font.Bold = true;
			//COL++;

			worksheet[ROW, COL].Text = "Plant Name";
			int colPlantName = COL;
			worksheet[ROW, COL].ColumnWidth = 10;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			COL++;
			worksheet[ROW, COL].Text = "Gate Entry/Gate Pass#";
			int colGateEntryNo = COL;
			worksheet[ROW, COL].ColumnWidth = 10;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			COL++;
			worksheet[ROW, COL].Text = "GRN No";
			int colGRNNo = COL;
			worksheet[ROW, COL].ColumnWidth = 10;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			COL++;

			worksheet[ROW, COL].Text = "Entry Date";
			int colEntryDate = COL;
			worksheet[ROW, COL].ColumnWidth = 10;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			COL++;

			worksheet[ROW, COL].Text = "Party Name";
			int colPartyName = COL;
			worksheet[ROW, COL].ColumnWidth = 20;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			COL++;

			worksheet[ROW, COL].Text = "Description";
			int colDescription = COL;
			worksheet[ROW, COL].ColumnWidth = 30;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			//worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
			COL++;

			worksheet[ROW, COL].Text = "Package Qty";
			int colPackageQty = COL;
			worksheet[ROW, COL].ColumnWidth = 10;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
			COL++;

			worksheet[ROW, COL].Text = "Mode of Transport";
			int colModeofTransport = COL;
			worksheet[ROW, COL].ColumnWidth = 12;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			// worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
			COL++;

			worksheet[ROW, COL].Text = "Bill";
			int colBill = COL;
			worksheet[ROW, COL].ColumnWidth = 8;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			// worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
			COL++;

			worksheet[ROW, COL].Text = "Person Name";
			int colPersonName = COL;
			worksheet[ROW, COL].ColumnWidth = 10;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			//worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
			COL++;

			worksheet[ROW, COL].Text = "Mobile No";
			int colMobileNo = COL;
			worksheet[ROW, COL].ColumnWidth = 10;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
			COL++;

			worksheet[ROW, COL].Text = "Remarks";
			int colRemarks = COL;
			worksheet[ROW, COL].ColumnWidth = 10;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			// worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
			COL++;

			worksheet[ROW, COL].Text = "Gate Entry Time";
			int colGateEntryTime = COL;
			worksheet[ROW, COL].ColumnWidth = 12;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			// worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
			COL++;

			worksheet[ROW, COL].Text = "Employee Name";
			int colEmployeeName = COL;
			worksheet[ROW, COL].ColumnWidth = 18;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			//worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
			COL++;

			worksheet[ROW, COL].Text = "Gate Entry By";
			int colGateEntryBy = COL;
			worksheet[ROW, COL].ColumnWidth = 18;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			// worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
			COL++;


			worksheet[ROW, COL].Text = "Gate Entry Type";
			int colGateEntryType = COL;
			worksheet[ROW, COL].ColumnWidth = 12;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			//worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
			COL++;

			worksheet[ROW, COL].Text = "Local Imported";
			int colLocalImported = COL;
			worksheet[ROW, COL].ColumnWidth = 12;
			worksheet[ROW, COL].CellStyle.Font.Bold = true;
			//workworksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
			COL++;


            report.SetHeaderText(ref worksheet, ROW, COL, "Gate Pass Detail Id", 13, ExcelHAlign.HAlignCenter);
            int ColGPDId = COL;
            COL++;

            //report.SetHeaderText(ref worksheet, ROW, COL, "Gate Pass Master Id", 13, ExcelHAlign.HAlignCenter);
            //int ColGPMId = COL;
            //COL++;

            //report.SetHeaderText(ref worksheet, ROW, COL, "Company Group", 13, ExcelHAlign.HAlignCenter);
            //int ColCompanyGroup = COL;
            //COL++;

            //report.SetHeaderText(ref worksheet, ROW, COL, "Plant", 13, ExcelHAlign.HAlignCenter);
            //int ColPlant = COL;
            //COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Gate Pass Type", 13, ExcelHAlign.HAlignCenter);
            int ColGatePassType = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Gate Pass Status", 13, ExcelHAlign.HAlignCenter);
            int ColGatePassStatus = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "GPM Returnable Date", 13, ExcelHAlign.HAlignCenter);
            int ColRetDate = COL;
            COL++;

            //report.SetHeaderText(ref worksheet, ROW, COL, "GatePassEntryDate", 13, ExcelHAlign.HAlignCenter);
            //int ColEntryDate = COL;
            //COL++;
            report.SetHeaderText(ref worksheet, ROW, COL, "Material", 13, ExcelHAlign.HAlignCenter);
            int ColMaterial = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Article", 13, ExcelHAlign.HAlignCenter);
            int ColArt = COL;
            COL++;

            //report.SetHeaderText(ref worksheet, ROW, COL, "First Characteristics", 13, ExcelHAlign.HAlignCenter);
            //int ColFCV = COL;
            //COL++;

            //report.SetHeaderText(ref worksheet, ROW, COL, "Second Characteristics", 13, ExcelHAlign.HAlignCenter);
            //int ColSC = COL;
            //COL++;

            //report.SetHeaderText(ref worksheet, ROW, COL, "Second Characteristics Value", 13, ExcelHAlign.HAlignCenter);
            //int ColSCV = COL;
            //COL++;

            //report.SetHeaderText(ref worksheet, ROW, COL, "Third Characteristics", 13, ExcelHAlign.HAlignCenter);
            //int ColTC = COL;
            //COL++;

            //report.SetHeaderText(ref worksheet, ROW, COL, "Third Characteristics Value", 13, ExcelHAlign.HAlignCenter);
            //int ColTCV = COL;
            //COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "SKU1", 13, ExcelHAlign.HAlignCenter);
            int ColSKU1 = COL;
            COL++;
			report.SetHeaderText(ref worksheet, ROW, COL, "SKU2", 13, ExcelHAlign.HAlignCenter);
			int ColSKU2 = COL;
			COL++;
			report.SetHeaderText(ref worksheet, ROW, COL, "SKU3", 13, ExcelHAlign.HAlignCenter);
			int ColSKU3 = COL;
			COL++;
			report.SetHeaderText(ref worksheet, ROW, COL, "Material Detail", 13, ExcelHAlign.HAlignCenter);
			int ColmaterialDetail = COL;
			COL++;

			report.SetHeaderText(ref worksheet, ROW, COL, "Transaction Qty", 13, ExcelHAlign.HAlignCenter);
            int ColTrnQty = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "UOM", 13, ExcelHAlign.HAlignCenter);
            int ColUom = COL;
            COL++;

			report.SetHeaderText(ref worksheet, ROW, COL, "GPM-Remarks", 13, ExcelHAlign.HAlignCenter);
			int ColGPMRemarks = COL;
			COL++;

			report.SetHeaderText(ref worksheet, ROW, COL, "GPD-Remarks", 13, ExcelHAlign.HAlignCenter);
            int ColGPDRemarks = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "IsReturnable", 13, ExcelHAlign.HAlignCenter);
            int ColRet = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "GPD Returnable Date", 13, ExcelHAlign.HAlignCenter);
            int ColRtDate = COL;
            COL++;

            //report.SetHeaderText(ref worksheet, ROW, COL, "IsMutilated", 13, ExcelHAlign.HAlignCenter);
            //int ColMul = COL;
            //COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Rate", 13, ExcelHAlign.HAlignCenter);
            int ColRate = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Challan No", 13, ExcelHAlign.HAlignCenter);
            int ColChlNo = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Challan No Detail Id", 13, ExcelHAlign.HAlignCenter);
            int ColChlDetail = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Purpose of Gate Pass", 13, ExcelHAlign.HAlignCenter);
            int ColPGP = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Consignment No", 13, ExcelHAlign.HAlignCenter);
            int ColConsigNo = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "GP Driver", 13, ExcelHAlign.HAlignCenter);
            int ColDriverN = COL;
            COL++;



            report.SetHeaderText(ref worksheet, ROW, COL, "From Employee", 13, ExcelHAlign.HAlignCenter);
            int ColFrEmp = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Through", 13, ExcelHAlign.HAlignCenter);
            int Colthrough = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Courier Name", 13, ExcelHAlign.HAlignCenter);
            int ColCourierName = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Runner Employee Id", 13, ExcelHAlign.HAlignCenter);
            int ColRunner = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "To Type", 13, ExcelHAlign.HAlignCenter);
            int ColTType = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "To Party", 13, ExcelHAlign.HAlignCenter);
            int ColTParty = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "To Buyer", 13, ExcelHAlign.HAlignCenter);
            int ColTBuyer = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "To Plant", 13, ExcelHAlign.HAlignCenter);
            int ColTPlant = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "To Unit", 13, ExcelHAlign.HAlignCenter);
            int ColTUnit = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "To Division", 13, ExcelHAlign.HAlignCenter);
            int ColTDiv = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "To Department", 13, ExcelHAlign.HAlignCenter);
            int ColTDep = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Department Employee", 13, ExcelHAlign.HAlignCenter);
            int ColDepEmp = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Other Company Name", 13, ExcelHAlign.HAlignCenter);
            int ColOtherCmp = COL;
            COL++;

			report.SetHeaderText(ref worksheet, ROW, COL, "Gate Pass Person Name", 13, ExcelHAlign.HAlignCenter);
			int ColGatePassPersonName = COL;
			COL++;

			report.SetHeaderText(ref worksheet, ROW, COL, "Gate Pass Mobile No", 13, ExcelHAlign.HAlignCenter);
			int ColGatePassMobileNo = COL;
			COL++;

			report.SetHeaderText(ref worksheet, ROW, COL, "Mobile No", 13, ExcelHAlign.HAlignCenter);
            int ColMobNo = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Address", 13, ExcelHAlign.HAlignCenter);
            int ColAdd = COL;
            COL++;

            //report.SetHeaderText(ref worksheet, ROW, COL, "GPM Remarks", 13, ExcelHAlign.HAlignCenter);
            //int ColRemarks = COL;
            //COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Checked By Employee", 13, ExcelHAlign.HAlignCenter);
            int ColChkEmp = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Checked By Status", 13, ExcelHAlign.HAlignCenter);
            int ColChkSt = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Checked Hold Reject Reason", 13, ExcelHAlign.HAlignCenter);
            int ColChkRej = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Approved By Employee", 13, ExcelHAlign.HAlignCenter);
            int ColAppEmp = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Approved Hold Reject Reason", 13, ExcelHAlign.HAlignCenter);
            int ColAppRej = COL;
            COL++;

            //report.SetHeaderText(ref worksheet, ROW, COL, "Sender Security Employee", 13, ExcelHAlign.HAlignCenter);
            //int ColSecEmp = COL;
            //COL++;

            //report.SetHeaderText(ref worksheet, ROW, COL, "Sender Security Approved Status", 13, ExcelHAlign.HAlignCenter);
            //int ColSecSt = COL;
            //COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Receiver Security Employee", 13, ExcelHAlign.HAlignCenter);
            int ColRecEmp = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Receiver Security Approved Status", 13, ExcelHAlign.HAlignCenter);
            int ColRecSt = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Vendor Buyer Other Company Received Status", 13, ExcelHAlign.HAlignCenter);
            int ColVendor = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "GPM Challan No", 13, ExcelHAlign.HAlignCenter);
            int ColChalanNo = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Transport Agent Mobile No", 13, ExcelHAlign.HAlignCenter);
            int ColTrpMob = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Transport Agent Name", 13, ExcelHAlign.HAlignCenter);
            int ColTrpName = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Vehicle No", 13, ExcelHAlign.HAlignCenter);
            int ColVehNo = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Gate Out Status", 13, ExcelHAlign.HAlignCenter);
            int ColOutSt = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Gate Register Type", 13, ExcelHAlign.HAlignCenter);
            int ColRegType = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Received ChallanNo", 13, ExcelHAlign.HAlignCenter);
            int ColRecChlNo = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Invoice No", 13, ExcelHAlign.HAlignCenter);
            int ColInvNo = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Purpose of Gate Pass", 13, ExcelHAlign.HAlignCenter);
            int ColPurGatePass = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "GPM Consignment No", 13, ExcelHAlign.HAlignCenter);
            int ColConsignNo = COL;
            COL++;

            report.SetHeaderText(ref worksheet, ROW, COL, "Driver Name", 13, ExcelHAlign.HAlignCenter);
            int ColDriverName = COL;
    


         
            int endCol = COL;
			worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
			worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
			worksheet.Range[ROW, startCol, ROW, COL].CellStyle.ColorIndex = ExcelKnownColors.Grey_40_percent;
			ROW++;

			for (int i = 0; i < dtGatenntryRegisterList.Rows.Count; i++)
			{
				// int i = 0; i < dtMasterOrderItem.Rows.Count; i++
				worksheet[ROW, ColSl].Text = dtGatenntryRegisterList.Rows[i]["S.N"].ToString();
				worksheet[ROW, colGateEntryFor].Text = dtGatenntryRegisterList.Rows[i]["GateEntryFor"].ToString();
				//worksheet[ROW, colCompanyGroup].Text = dtGatenntryRegisterList.Rows[i]["CompanyGroup"].ToString();
				//worksheet[ROW, colCompany].Text = dtGatenntryRegisterList.Rows[i]["Company"].ToString();
				worksheet[ROW, colPlantName].Text = dtGatenntryRegisterList.Rows[i]["PlantName"].ToString();
				worksheet[ROW, colGateEntryNo].Text = dtGatenntryRegisterList.Rows[i]["GateEntryNo"].ToString();
				worksheet[ROW, colGRNNo].Text = dtGatenntryRegisterList.Rows[i]["GRNNO"].ToString();
				worksheet[ROW, colEntryDate].Text = dtGatenntryRegisterList.Rows[i]["EntryDate"].ToString();
				worksheet[ROW, colPartyName].Text = dtGatenntryRegisterList.Rows[i]["PartyName"].ToString();
				//worksheet[ROW, colInvoicingPartyName].Text = dtGatenntryRegisterList.Rows[i]["InvoicingPartyName"].ToString();
				//worksheet[ROW, colDeliveryPartyName].Text = dtGatenntryRegisterList.Rows[i]["DeliveryPartyName"].ToString();
				worksheet[ROW, colDescription].Text = dtGatenntryRegisterList.Rows[i]["Description"].ToString();
				worksheet[ROW, colPackageQty].Number = clsStaticInfo.dbl(dtGatenntryRegisterList.Rows[i]["PackageQty"].ToString());
				worksheet[ROW, colPackageQty].NumberFormat = clsStaticInfo.NumberFormat();
				worksheet[ROW, colModeofTransport].Text = dtGatenntryRegisterList.Rows[i]["ModeofTransport"].ToString();
				worksheet[ROW, colBill].Text = dtGatenntryRegisterList.Rows[i]["Bill"].ToString();
				worksheet[ROW, colPersonName].Text = dtGatenntryRegisterList.Rows[i]["PersonName"].ToString();
				worksheet[ROW, colMobileNo].Text = dtGatenntryRegisterList.Rows[i]["MobileNo"].ToString();
				worksheet[ROW, colRemarks].Text = dtGatenntryRegisterList.Rows[i]["Remarks"].ToString();
				worksheet[ROW, colGateEntryTime].Text = dtGatenntryRegisterList.Rows[i]["GateEntryTime"].ToString();
				worksheet[ROW, colEmployeeName].Text = dtGatenntryRegisterList.Rows[i]["EmployeeName"].ToString();
				worksheet[ROW, colGateEntryBy].Text = dtGatenntryRegisterList.Rows[i]["GateEntryBy"].ToString();
				worksheet[ROW, colGateEntryType].Text = dtGatenntryRegisterList.Rows[i]["GateEntryType"].ToString();
				////worksheet[ROW, colLocalImported].Text = dtGatenntryRegisterList.Rows[i]["LocalImported"].ToString();


				worksheet[ROW, ColGPDId].Text = dtGatenntryRegisterList.Rows[i]["GatePassDetailId"].ToString();
				//worksheet[ROW, ColGPMId].Text = dtGatenntryRegisterList.Rows[i]["GatePassMasterId"].ToString();
				//worksheet[ROW, ColCompanyGroup].Text = dtGatenntryRegisterList.Rows[i]["CompanyGroup"].ToString();
				//worksheet[ROW, ColPlant].Text = dtGatenntryRegisterList.Rows[i]["Plant"].ToString();
				worksheet[ROW, ColGatePassType].Text = dtGatenntryRegisterList.Rows[i]["GatePassType"].ToString();
				worksheet[ROW, ColGatePassStatus].Text = dtGatenntryRegisterList.Rows[i]["GatePassStatus"].ToString();
				worksheet[ROW, ColRetDate].Text = dtGatenntryRegisterList.Rows[i]["GPMReturnableDate"].ToString();
				//worksheet[ROW, ColEntryDate].Text = dtGatenntryRegisterList.Rows[i]["GatePassEntryDate"].ToString();
				worksheet[ROW, ColMaterial].Text = dtGatenntryRegisterList.Rows[i]["Material"].ToString();
				worksheet[ROW, ColArt].Text = dtGatenntryRegisterList.Rows[i]["Article"].ToString();
				worksheet[ROW, ColSKU1].Text = dtGatenntryRegisterList.Rows[i]["FirstCharacteristicsValue"].ToString();
				worksheet[ROW, ColSKU2].Text = dtGatenntryRegisterList.Rows[i]["SecondCharacteristicsValue"].ToString();
				worksheet[ROW, ColSKU3].Text = dtGatenntryRegisterList.Rows[i]["ThirdCharacteristicsValue"].ToString();
				worksheet[ROW, ColmaterialDetail].Text = dtGatenntryRegisterList.Rows[i]["MaterialDetail"].ToString();
                worksheet[ROW, ColTrnQty].Number = clsStaticInfo.dbl( dtGatenntryRegisterList.Rows[i]["TransactionQty"].ToString());
				worksheet[ROW, ColTrnQty].NumberFormat = clsStaticInfo.NumberFormat(2);
				worksheet[ROW, ColUom].Text = dtGatenntryRegisterList.Rows[i]["UoM"].ToString();
				worksheet[ROW, ColGPMRemarks].Text = dtGatenntryRegisterList.Rows[i]["GPMRemarks"].ToString();
				worksheet[ROW, ColRet].Text = dtGatenntryRegisterList.Rows[i]["IsReturnable"].ToString();
				worksheet[ROW, ColRtDate].Text = dtGatenntryRegisterList.Rows[i]["GPDReturnableDate"].ToString();
				//worksheet[ROW, ColMul].Text = dtGatenntryRegisterList.Rows[i]["IsMutilated"].ToString();
				worksheet[ROW, ColRate].Number =clsStaticInfo.dbl( dtGatenntryRegisterList.Rows[i]["Rate"].ToString());
				worksheet[ROW, ColRate].NumberFormat = clsStaticInfo.NumberFormat(4);

				worksheet[ROW, ColChlNo].Text = dtGatenntryRegisterList.Rows[i]["ChallanNo"].ToString();
				worksheet[ROW, ColChlDetail].Text = dtGatenntryRegisterList.Rows[i]["ChallanNoDetailId"].ToString();
				worksheet[ROW, ColPGP].Text = dtGatenntryRegisterList.Rows[i]["PurposeofGatePass"].ToString();
				worksheet[ROW, ColConsigNo].Text = dtGatenntryRegisterList.Rows[i]["ConsignmentNo"].ToString();
				worksheet[ROW, ColDriverN].Text = dtGatenntryRegisterList.Rows[i]["GPDDriverName"].ToString();
				worksheet[ROW, ColFrEmp].Text = dtGatenntryRegisterList.Rows[i]["FromEmployee"].ToString();
				worksheet[ROW, Colthrough].Text = dtGatenntryRegisterList.Rows[i]["Through"].ToString();
				worksheet[ROW, ColCourierName].Text = dtGatenntryRegisterList.Rows[i]["CourierName"].ToString();
				worksheet[ROW, ColRunner].Text = dtGatenntryRegisterList.Rows[i]["RunnerEmployeeId"].ToString();
				worksheet[ROW, ColTType].Text = dtGatenntryRegisterList.Rows[i]["ToType"].ToString();
				worksheet[ROW, ColTParty].Text = dtGatenntryRegisterList.Rows[i]["ToParty"].ToString();
				worksheet[ROW, ColTBuyer].Text = dtGatenntryRegisterList.Rows[i]["ToBuyer"].ToString();
				worksheet[ROW, ColTPlant].Text = dtGatenntryRegisterList.Rows[i]["ToPlant"].ToString();
				worksheet[ROW, ColTUnit].Text = dtGatenntryRegisterList.Rows[i]["ToUnit"].ToString();
				worksheet[ROW, ColTDiv].Text = dtGatenntryRegisterList.Rows[i]["ToDivision"].ToString();
				worksheet[ROW, ColTDep].Text = dtGatenntryRegisterList.Rows[i]["ToDepartment"].ToString();
				worksheet[ROW, ColDepEmp].Text = dtGatenntryRegisterList.Rows[i]["ToDepartment"].ToString();
				worksheet[ROW, ColOtherCmp].Text = dtGatenntryRegisterList.Rows[i]["OtherCompanyName"].ToString();
				worksheet[ROW, ColMobNo].Text = dtGatenntryRegisterList.Rows[i]["MobileNo"].ToString();
				worksheet[ROW, ColAdd].Text = dtGatenntryRegisterList.Rows[i]["Address"].ToString();
				worksheet[ROW, ColGPDRemarks].Text = dtGatenntryRegisterList.Rows[i]["GPDRemarks"].ToString();
				worksheet[ROW, ColChkEmp].Text = dtGatenntryRegisterList.Rows[i]["CheckedByEmployee"].ToString();
				worksheet[ROW, ColChkSt].Text = dtGatenntryRegisterList.Rows[i]["CheckedByStatus"].ToString();
				worksheet[ROW, ColChkRej].Text = dtGatenntryRegisterList.Rows[i]["CheckedHoldRejectReason"].ToString();
				worksheet[ROW, ColAppEmp].Text = dtGatenntryRegisterList.Rows[i]["ApprovedByEmployee"].ToString();
				worksheet[ROW, ColAppRej].Text = dtGatenntryRegisterList.Rows[i]["ApprovedHoldRejectReason"].ToString();
				//worksheet[ROW, ColSecEmp].Text = dtGatenntryRegisterList.Rows[i]["SenderSecurityEmployee"].ToString();
				//worksheet[ROW, ColSecSt].Text = dtGatenntryRegisterList.Rows[i]["SenderSecurityApprovedStatus"].ToString();
				worksheet[ROW, ColRecEmp].Text = dtGatenntryRegisterList.Rows[i]["ReceiverSecurityEmployee"].ToString();
				worksheet[ROW, ColRecSt].Text = dtGatenntryRegisterList.Rows[i]["ReceiverSecurityApprovedStatus"].ToString();
				worksheet[ROW, ColVendor].Text = dtGatenntryRegisterList.Rows[i]["VendorBuyerOtherCompanyReceivedStatus"].ToString();
				worksheet[ROW, ColChalanNo].Text = dtGatenntryRegisterList.Rows[i]["GPMChallanNo"].ToString();
				worksheet[ROW, ColTrpMob].Text = dtGatenntryRegisterList.Rows[i]["TransportAgentMobileNo"].ToString();
				worksheet[ROW, ColTrpName].Text = dtGatenntryRegisterList.Rows[i]["TransportAgentName"].ToString();
				worksheet[ROW, ColVehNo].Text = dtGatenntryRegisterList.Rows[i]["VehicleNo"].ToString();
				worksheet[ROW, ColOutSt].Text = dtGatenntryRegisterList.Rows[i]["GateOutStatus"].ToString();
				worksheet[ROW, ColRegType].Text = dtGatenntryRegisterList.Rows[i]["GateRegisterType"].ToString();
				worksheet[ROW, ColRecChlNo].Text = dtGatenntryRegisterList.Rows[i]["ReceivedChallanNo"].ToString();
				worksheet[ROW, ColInvNo].Text = dtGatenntryRegisterList.Rows[i]["InvoiceNo"].ToString();
				worksheet[ROW, ColPurGatePass].Text = dtGatenntryRegisterList.Rows[i]["PurposeofGatePass"].ToString();
				worksheet[ROW, ColConsignNo].Text = dtGatenntryRegisterList.Rows[i]["GPMConsignmentNo"].ToString();
				worksheet[ROW, ColDriverName].Text = dtGatenntryRegisterList.Rows[i]["DriverName"].ToString();
				worksheet[ROW, ColGatePassMobileNo].Text = dtGatenntryRegisterList.Rows[i]["GatePassMobileNo"].ToString();
				worksheet[ROW, ColGatePassPersonName].Text = dtGatenntryRegisterList.Rows[i]["GatePassPersonName"].ToString();
				//worksheet[ROW, ColGatePassRemarks].Text = dtGatenntryRegisterList.Rows[i]["GatePassRemarks"].ToString();


				worksheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
				worksheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

				// worksheet.Range[ROW, 1, ROW, endCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;
				ROW++;

			}

			worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
			worksheet.UsedRange.CellStyle.Font.Size = 8f;


			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			ReportUtility reportUtility = new ReportUtility();
			// reportUtility.PlantHeaderWithOutLogo(ref worksheet, endCol, "Gatenntry Register", identity.PlantId);

			reportUtility.PlantHeader(ref worksheet, endCol, "Gate Entry Register", identity.PlantId);
			reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
			// worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			// worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			// worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
			worksheet.Range[1, 1, 4, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
			worksheet["A" + 5].FreezePanes();
			worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
			worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
			worksheet.IsGridLinesVisible = false;

			return workbook;
		}

        //[Authorize, HttpGet]
        //public JsonResult GateEntryLoadOnData()
        //{
        //	var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //	try
        //	{
        //		string _sql = @"select CG.UserName CompanyGroup
        //                              ,C.UserName Company
        //                              ,P.UserName PlantName
        //                               ,Format(GE.EntryDate,'dd-MMM-yyyy') EntryDate
        //                              ,p.UserName PartyName
        //                              --,InvParty.UserName InvoicingPartyName
        //                              --,DeliParty.UserName DeliveryPartyName
        //                              ,GE.Description
        //                              ,GE.PackageQty
        //                              ,GE.ModeofTransport
        //                              ,GE.Bill
        //                              ,GE.PersonName
        //                              ,GE.MobileNo
        //                              ,GE.Remarks
        //                              ,GE.GateEntryTime
        //                              ,EI.EmployeeName
        //                              ,EI1.EmployeeName GateEntryBys
        //                              ,GE.GateEntryType
        //                              ,GE.Id GateEntryNo
        //		                ,IR.Id GRNNo
        //                              ,isnull(GE.LocalImported,'') LocalImported
        //                              FROM
        //                              [TRN].[GateEntry] GE
        //                              LEFT JOIN ORG.CompanyGroup CG ON CG.Id=GE.CompanyGroupId
        //                              LEFT JOIN ORG.Company C ON C.Id=GE.CompanyId                               
        //                              LEFT JOIN ORG.Plant P On P.Id=GE.PlantId
        //                              LEFT JOIN HKP.Party party ON party.Id=GE.PartyId                               
        //                              --LEFT JOIN HKP.Party InvParty ON party.Id=GE.InvoicingPartyPlantId
        //                              --LEFT JOIN HKP.Party DeliParty ON party.Id=GE.DeliveryPartyPlantId
        //                              LEFT JOIN [dbo].[PlantWiseGate] PWG ON PWG.Id=GE.PlantWiseGateId
        //                              LEFT JOIN EmployeeInformation EI ON EI.SystemId=GE.EmployeeId
        //                              LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=GE.EmployeeId
        //                              left Join TRN.InventoryReceive IR On IR.GateEntryNo=GE.Id";
        //		return Json(_sqlRepository.GetDataCollection(_sql), JsonRequestBehavior.AllowGet);

        //	}
        //	catch (Exception ex)
        //	{
        //		throw new CustomException(ex.Message, ex,
        //		Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
        //		ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
        //	}

        //}



        [Authorize, HttpGet]
        public JsonResult GateEntryLoadOnData(string fromDate,string toDate)
       {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
				string _sql = @"SELECT  ROW_NUMBER() Over (Order by GE.Id) As [S.N],
'Gate Entry For GRN' GateEntryFor
,CG.Id CompanyGroupId
, CG.UserName CompanyGroup
,C.UserName Company
,p.Id PlantId
,P.UserName PlantName
,Format(GE.EntryDate,'dd-MMM-yyyy') EntryDate
,party.UserName PartyName
--,InvParty.UserName InvoicingPartyName
--,DeliParty.UserName DeliveryPartyName
,GE.Description
,GE.PackageQty
,GE.ModeofTransport
,GE.Bill
,GE.PersonName
,GE.MobileNo
,GE.Remarks
,Format (GE.GateEntryTime, N'hh:mm tt') GateEntryTime
,EI.EmployeeName
,EI1.EmployeeName GateEntryBy
,GE.GateEntryType
,GE.LocalImported 
,IR.Id GRNNo
,GE.Id GateEntryNo

--------From Here You have to Add Column--
,'' GatePassType 
,'' GatePassStatus
,'' GatePassDetailId
,'' GatePassMasterId 
,'' MaterialMasterId 
,''  Material 
, '' ArticleId
, '' Article
,'' FirstCharacteristicsId
,'' FirstCharacteristics
, ''FirstCharacteristicsValueId
,  ''FirstCharacteristicsValue
,''SecondCharacteristicsId
, ''SecondCharacteristics
, ''SecondCharacteristicsValueId
, '' SecondCharacteristicsValue
,''ThirdCharacteristicsId
, '' ThirdCharacteristics
,''ThirdCharacteristicsValueId
, ''ThirdCharacteristicsValue
,''MaterialDetail 
,0 TransactionQty 
, ''TransactionUoMId 
, '' UOM
, ''GPDRemarks 
, 0 IsReturnable
,'' GPDReturnableDate
,0 IsMutilated 
, 0 Rate
, '' ChallanNo 
, ''ChallanNoDetailId 
, ''PurposeofGatePass 
,''ConsignmentNo 
, '' GPDDriverName
--- Master Table

,'' GPMReturnableDate
, ''FromEmployeeId 
,'' FromEmployee
,''Through
,''CourierName
,'' RunnerEmployeeId
,'' ToType
,'' ToPartyCode 
,''  ToParty
,'' ToBuyerId
,'' ToBuyer
,''ToPlantId
, '' ToPlant
, ''ToUnitId 
, '' ToUnit
,''ToDivisionId
, '' ToDivision
,'' ToDepartmentId
,'' ToDepartment
,''DepartmentEmployeeId
, '' DepartmentEmployee
, ''OtherCompanyName
,''GatePassPersonName
,''GatePassMobileNo
,''Address
,'' GPMRemarks
,''CheckedBy
, '' CheckedByEmployee
, ''CheckedByStatus 
, ''CheckedHoldRejectReason
, ''ApprovedBy 
, '' ApprovedByEmployee
,''ApprovedHoldRejectReason
, ''SenderSecurityEmployeeId 
, '' SenderSecurityEmployee 
, ''SenderSecurityApprovedStatus
,''ReceiverSecurityEmployeeId 
,'' ReceiverSecurityEmployee 
, ''ReceiverSecurityApprovedStatus 
, ''VendorBuyerOtherCompanyReceivedStatus
,'' GPMChallanNo 
, '' TransportAgentMobileNo
, '' TransportAgentName 
, '' VehicleNo
, '' GateOutStatus
, '' GateRegisterType
, '' ReceivedChallanNO
, ''InvoiceNo
, '' GPMPurposeOfGatePass
, '' GPMConsignmentNo
, ''DriverName
FROM 
[TRN].[GateEntry] GE
LEFT JOIN ORG.CompanyGroup CG ON CG.Id=GE.CompanyGroupId
LEFT JOIN ORG.Company C ON C.Id=GE.CompanyId
LEFT JOIN ORG.Plant P On P.Id=GE.PlantId
LEFT JOIN HKP.Party party ON party.Id=GE.PartyId
-- LEFT JOIN HKP.Party InvParty ON party.Id=GE.InvoicingPartyPlantId
--LEFT JOIN HKP.Party DeliParty ON party.Id=GE.DeliveryPartyPlantId
LEFT JOIN [dbo].[PlantWiseGate] PWG ON PWG.Id=GE.PlantWiseGateId
LEFT JOIN EmployeeInformation EI ON EI.SystemId=GE.EmployeeId
LEFT JOIN EmployeeInformation EI1 ON EI1.SystemId=GE.EmployeeId
left Join TRN.InventoryReceive IR On IR.GateEntryNo=GE.Id

					where GE.EntryDate between '" + fromDate + @"' AND '" + toDate + @"'

UNION ALL

select  ROW_NUMBER() Over (Order by gpm.Id) As [S.N],
'Gate Pass For Material' GateEntryFor
, gpm.CompanyGroupId
, cg.UserName as CompanyGroup
,'' Company
,gpm.PlantId 
, p.UserName as PlantName
,format(gpm.GatePassEntryDate,'dd-MMM-yyyy') as EntryDate
,'' PartyName
,'' Description
, gpm.NoOfPackages PackageQty
,'' ModeofTransport
,'' Bill
,'' PersonName
,'' MobileNo
,'' Remarks
,'' GateEntryTime
,'' EmployeeName
,'' GateEntryBy
,'' GateEntryType
,'' LocalImported 
,'' GRNNo
,gpm.Id GateEntryNo
--------From Here You have to Add Column--
, gpm.GatePassType 
, gpm.GatePassStatus
,gpd.Id as GatePassDetailId,gpd.GatePassMasterId , gpd.MaterialMasterId ,mm.UserName as Material , gpd.ArticleId, mma.StandardName as Article,
gpd.FirstCharacteristicsId, cf.UserName as FirstCharacteristics, gpd.FirstCharacteristicsValueId, cvf.UserName as FirstCharacteristicsValue,
gpd.SecondCharacteristicsId, cs.UserName as SecondCharacteristics, gpd.SecondCharacteristicsValueId, cvs.UserName as SecondCharacteristicsValue,
gpd.ThirdCharacteristicsId, ct.UserName as ThirdCharacteristics, gpd.ThirdCharacteristicsValueId, cvt.UserName as ThirdCharacteristicsValue,
gpd.MaterialDetail ,gpd.TransactionQty , gpd.TransactionUoMId , uom.Username as UOM, gpd.Remarks GPDRemarks, gpd.IsReturnable , 
format(gpd.ReturnableDate,'dd-MMM-yyyy') as GPDReturnableDate,gpd.IsMutilated , gpd.Rate, gpd.ChallanNo , gpd.ChallanNoDetailId , gpd.PurposeofGatePass ,
gpd.ConsignmentNo , gpd.DriverName as GPDDriverName,
--- Master Table

format(gpm.ReturnableDate,'dd-MMM-yyyy') as GPMReturnableDate, gpm.FromEmployeeId ,
fromemp.EmployeeName as FromEmployee, gpm.Through, gpm.CourierName,gpm.RunnerEmployeeId, gpm.ToType, gpm.ToPartyCode , 
pty.Username as ToParty, gpm.ToBuyerId, b.UserName as ToBuyer,gpm.ToPlantId, pl.Username as ToPlant, gpm.ToUnitId , u.UserName as ToUnit,
gpm.ToDivisionId , div.Username as ToDivision,gpm.ToDepartment as ToDepartmentId, dept.UserName as ToDepartment,
gpm.DepartmentEmployeeId, DeptEmp.EmployeeName as DepartmentEmployee, gpm.OtherCompanyName,gpm.PersonName GatePassPersonName,gpm.MobileNo GatePassMobileNo,gpm.Address,gpm.Remarks as GPMRemarks,
gpm.CheckedBy, CheckEmp.EmployeeName as CheckedByEmployee, gpm.CheckedByStatus , gpm.CheckedHoldRejectReason, gpm.ApprovedBy , ApprEmp.EmployeeName as ApprovedByEmployee,
gpm.ApprovedHoldRejectReason, gpm.SenderSecurityEmployeeId , SenderSecEmp.EmployeeName as SenderSecurityEmployee , gpm.SenderSecurityApprovedStatus,
gpm.ReceiverSecurityEmployeeId , ReceiverSecEmp.EmployeeName as ReceiverSecurityEmployee , gpm.ReceiverSecurityApprovedStatus , gpm.VendorBuyerOtherCompanyReceivedStatus,
gpm.ChallanNo as GPMChallanNo , gpm.TransportAgentMobileNo, gpm.TransportAgentName , gpm.VehicleNo, gpm.GateOutStatus, gpm.GateRegisterType, gpm.ReceivedChallanNO,
gpm.InvoiceNo, gpm.PurposeofGatePass as GPMPurposeOfGatePass, gpm.ConsignmentNo as GPMConsignmentNo, gpm.DriverName
from trn.GatePassDetails gpd
left join mst.MaterialMaster mm on mm.Id = gpd.MaterialMasterId
left join mst.MaterialMasterArticle mma on mma.Id = gpd.ArticleId
left join hkp.Characteristics cf on cf.Id = gpd.FirstCharacteristicsId
left join hkp.CharacteristicsValue cvf on cvf.Id = gpd.FirstCharacteristicsValueId
left join hkp.Characteristics cs on cs.Id = gpd.SecondCharacteristicsId
left join hkp.CharacteristicsValue cvs on cvs.Id = gpd.SecondCharacteristicsValueId
left join hkp.Characteristics ct on ct.Id = gpd.ThirdCharacteristicsId
left join hkp.CharacteristicsValue cvt on cvt.Id = gpd.ThirdCharacteristicsValueId
left join scs.UnitOfMeasurement uom on uom.Id = gpd.TransactionUoMId
left join trn.GatePassMaster gpm on gpm.Id = gpd.GatePassMasterId
left join org.CompanyGroup cg on cg.Id = gpm.CompanyGroupId
left join org.Plant p on p.Id = gpm.PlantId
left join dbo.EmployeeInformation FromEmp on fromemp.SystemId = gpm.FromEmployeeId
left join dbo.EmployeeInformation DeptEmp on DeptEmp.SystemId = gpm.DepartmentEmployeeId
left join dbo.EmployeeInformation CheckEmp on CheckEmp.SystemId = gpm.CheckedBy
left join dbo.EmployeeInformation ApprEmp on ApprEmp.SystemId = gpm.ApprovedBy
left join dbo.EmployeeInformation SenderSecEmp on SenderSecEmp.SystemId = gpm.SenderSecurityEmployeeId
left join dbo.EmployeeInformation ReceiverSecEmp on ReceiverSecEmp.SystemId = gpm.ReceiverSecurityEmployeeId
left join hkp.Party pty on pty.Id = gpm.ToPartyCode
left join hkp.Buyer b on b.Id = gpm.ToBuyerId
left join org.Plant pl on pl.Id = gpm.ToPlantId
left join org.Unit u on u.Id = gpm.ToUnitId
left join org.Division div on div.Id= gpm.ToDivisionId
left join org.Department dept on dept.Id = gpm.ToDepartment
where gpm.GatePassEntryDate between '"+ fromDate + @"' AND '" + toDate + @"'";

				var jsondata = Json(_sqlRepository.GetDataCollection(_sql), JsonRequestBehavior.AllowGet);
				jsondata.MaxJsonLength = int.MaxValue;
				return jsondata;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }

        }


		#region In/Out Gate Pass
		[Authorize, HttpGet]
		public JsonResult GetPurchaseReturnData()
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				Library.MaterialManagement.InventoryManagements.GatePassAndGateEntryService obj = new Library.MaterialManagement.InventoryManagements.GatePassAndGateEntryService();
				return Json(obj.GetPurchaseReturn(), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}

		}
		[Authorize, HttpGet]
		public JsonResult GetPurchaseReturnMaterialDetails(string Id)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				Library.MaterialManagement.InventoryManagements.GatePassAndGateEntryService obj = new Library.MaterialManagement.InventoryManagements.GatePassAndGateEntryService();
				return Json(obj.GetPurchaseReturnMaterialDetails(Id), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}

		}

		#region FA register
		[HttpPost, Authorize]
		public ActionResult GetFixedAssetRegisterElasticSearchDataList(string fixedAssetRegisterDisposeId)
		{
			//FixedAssetQueryService fixedAssetQueryService = new FixedAssetQueryService(_sqlRepository);
			Library.MaterialManagement.InventoryManagements.GatePassAndGateEntryService obj = new Library.MaterialManagement.InventoryManagements.GatePassAndGateEntryService();

			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			return Json(new { DATA = obj.GetFixedAssetRegisterElasticSearchDataList(identity.CompanyGroupId, identity.CompanyId, identity.PlantId,fixedAssetRegisterDisposeId), Error = false }, JsonRequestBehavior.AllowGet);
		}
		#endregion FA register


		[HttpPost]
		public ActionResult createInOutGatePass(InOutGatePassMaster inOutGatePassMasterModel)
		{
			try
			{
				Library.MaterialManagement.InventoryManagements.GatePassAndGateEntryService obj = new Library.MaterialManagement.InventoryManagements.GatePassAndGateEntryService();
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				inOutGatePassMasterModel.CompanyGroupId = identity.CompanyGroupId;
				inOutGatePassMasterModel.CompanyId = identity.CompanyId;
				inOutGatePassMasterModel.PlantId = identity.PlantId;
				//inOutGatePassMasterModel.CheckedBy = identity.EmployeeId;
				inOutGatePassMasterModel.CheckedByStatus = "Forchecked";
				if (inOutGatePassMasterModel.GatePassType == "PurchaseReturn")
				{
					inOutGatePassMasterModel.PurchaseReturnId = inOutGatePassMasterModel.ChallanItemTypeId;
					inOutGatePassMasterModel.InventoryTransferId = null;
					inOutGatePassMasterModel.InventorySalesId = null;
					inOutGatePassMasterModel.InventoryScrapId = null;
					inOutGatePassMasterModel.FixedAssetRegisterDisposedId = null;
					inOutGatePassMasterModel.FixedAssetScrapId = null;

				}
				else if (inOutGatePassMasterModel.GatePassType == "MaterialTransfer")
				{
					inOutGatePassMasterModel.PurchaseReturnId = null;
					inOutGatePassMasterModel.InventoryTransferId = inOutGatePassMasterModel.ChallanItemTypeId;
					inOutGatePassMasterModel.InventorySalesId = null;
					inOutGatePassMasterModel.InventoryScrapId = null;
					inOutGatePassMasterModel.FixedAssetRegisterDisposedId = null;
					inOutGatePassMasterModel.FixedAssetScrapId = null;
				}
				else if (inOutGatePassMasterModel.GatePassType == "InventorySales")
				{
					inOutGatePassMasterModel.PurchaseReturnId = null;
					inOutGatePassMasterModel.InventoryTransferId = null;
					inOutGatePassMasterModel.InventorySalesId = inOutGatePassMasterModel.ChallanItemTypeId;
					inOutGatePassMasterModel.InventoryScrapId = null;
					inOutGatePassMasterModel.FixedAssetRegisterDisposedId = null;
					inOutGatePassMasterModel.FixedAssetScrapId = null;
				}
				else if (inOutGatePassMasterModel.GatePassType == "InventoryScrap")
				{
					inOutGatePassMasterModel.PurchaseReturnId = null;
					inOutGatePassMasterModel.InventoryTransferId = null;
					inOutGatePassMasterModel.InventorySalesId = null;
					inOutGatePassMasterModel.InventoryScrapId = inOutGatePassMasterModel.ChallanItemTypeId;
					inOutGatePassMasterModel.FixedAssetRegisterDisposedId = null;
					inOutGatePassMasterModel.FixedAssetScrapId = null;
				}
				else if (inOutGatePassMasterModel.GatePassType == "FixedAssetSales")
				{
					inOutGatePassMasterModel.PurchaseReturnId = null;
					inOutGatePassMasterModel.InventoryTransferId = null;
					inOutGatePassMasterModel.InventorySalesId = null;
					inOutGatePassMasterModel.InventoryScrapId = null;
					inOutGatePassMasterModel.FixedAssetRegisterDisposedId = inOutGatePassMasterModel.ChallanItemTypeId;
					inOutGatePassMasterModel.FixedAssetScrapId = null;
				}
				else if (inOutGatePassMasterModel.GatePassType == "FixedAssetScrap")
				{
					inOutGatePassMasterModel.PurchaseReturnId = null;
					inOutGatePassMasterModel.InventoryTransferId = null;
					inOutGatePassMasterModel.InventorySalesId = null;
					inOutGatePassMasterModel.InventoryScrapId = null;
					inOutGatePassMasterModel.FixedAssetRegisterDisposedId = null;
					inOutGatePassMasterModel.FixedAssetScrapId = inOutGatePassMasterModel.ChallanItemTypeId;
				}

				obj.createInOutGatePass(inOutGatePassMasterModel);
				return Json(new { Error = false, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{

				return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
			}


		}
		public ActionResult DeleteInOutGatePass(string Id)
		{
			try
			{
				Library.MaterialManagement.InventoryManagements.GatePassAndGateEntryService obj = new Library.MaterialManagement.InventoryManagements.GatePassAndGateEntryService();
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				obj.DeleteInOutGatePass(Id); 
				return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{

				return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
			}


		}
		[Authorize, HttpGet]
		public JsonResult GetInOutGateIndexGridDataList(string PendingApprvedGateOut)
		{		
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				Library.MaterialManagement.InventoryManagements.GatePassAndGateEntryService obj = new Library.MaterialManagement.InventoryManagements.GatePassAndGateEntryService();
				//return Json(obj.GetInOutGateIndexGridDataList(identity.EmployeeId), JsonRequestBehavior.AllowGet);
				return Json(obj.GetInOutGateIndexGridDataList(identity.Name, PendingApprvedGateOut), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}

		}



		#region IN OUT Gate Pass Report 
		[HttpGet, Authorize]
		public JsonResult InOutGatePassTeamplateReport(string GatePassId)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				Library.MaterialManagement.InventoryManagements.GatePassAndGateEntryService obj = new Library.MaterialManagement.InventoryManagements.GatePassAndGateEntryService();
				return Json(obj.InOutGatePassReport(identity.CompanyGroupId, identity.PlantId,GatePassId), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}

		}
		[HttpGet, Authorize]
		public JsonResult InOutGatePassSalesTeamplateReport(string GatePassId)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				Library.MaterialManagement.InventoryManagements.GatePassAndGateEntryService obj = new Library.MaterialManagement.InventoryManagements.GatePassAndGateEntryService();
				return Json(obj.InOutGatePassSalesReport(identity.CompanyGroupId, identity.PlantId, GatePassId), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}

		}
		[HttpGet, Authorize]
		public JsonResult InOutGatePassScrapTeamplateReport(string GatePassId)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				Library.MaterialManagement.InventoryManagements.GatePassAndGateEntryService obj = new Library.MaterialManagement.InventoryManagements.GatePassAndGateEntryService();
				return Json(obj.InOutGatePassScrapReport(identity.CompanyGroupId, identity.PlantId, GatePassId), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}

		}
		#endregion


		#region Indivisual Gate Pass Report 
		[HttpGet, Authorize]
		public JsonResult IndivisualGatePassTeamplateReport(string GatePassId)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
				Library.MaterialManagement.InventoryManagements.GatePassAndGateEntryService obj = new Library.MaterialManagement.InventoryManagements.GatePassAndGateEntryService();
				return Json(obj.IndivisualGatePassTeamplateReport(identity.CompanyGroupId, identity.PlantId, GatePassId), JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{
				throw ex;
			}

		}
		#endregion

		#endregion

		#region Gate Pass Checked And Approved
		[HttpGet, Authorize]
		public JsonResult GetGatePassCheckedApprovedList(string tabType)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";
				if (tabType == "UnCheckedList")
				{
					sql = @"SELECT  GPM.[Id]
                                ,INR.ID GRNNO
								,GPM.[CompanyGroupId]
								,GPM.[CompanyId]
								,GPM.[PlantId]
								,GPM.[GatePassType]
								,GPM.[GatePassStatus]                             
								,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
								,GPM.[FromEmployeeId]
								, EI.EmployeeName CheckedByName
								,GPM.[Through]
								,GPM.[CourierName]
								,GPM.[RunnerEmployeeId]
								, EI1.EmployeeName ApprovedByName                             
								,GPM.[Remarks]
								,GPM.[CheckedBy]
								,GPM.[CheckedByStatus]
								,GPM.[CheckedHoldRejectReason]
								,GPM.[ApprovedByStatus]
								,GPM.[ApprovedHoldRejectReason]                            
								,GPM.[AddedBy]
								,GPM.[AddedDate]
								,GPM.[AddedFromIP]
								,GPM.[UpdatedBy]
								,GPM.[UpdatedDate]
								,GPM.[UpdatedFromIP]--,GPM.ChallanNo
								,PurchaseReturnId,InventoryTransferId,InventorySalesId,InventoryScrapId,FixedAssetSalesId,FixedAssetScrapId
							FROM [TRN].[InOutGatePassMaster] GPM
							LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.CheckedBy
							LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.ApprovedBy 
                            left jOIN [TRN].[PurchaseReturn] AS IR ON IR.Id=GPM.PurchaseReturnId
                            LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId   
	                        LEFT JOIN TRn.InventoryReceive AS INR ON INR.ID=IR.InventoryReceiveId
                            Where GPM.CheckedByStatus ='Forchecked'--OR GPM.CheckedByStatus Is null
                            Order By GPM.[Id] DESC";
					

				}
				else if (tabType == "HoldRejectCheckedList")
				{
					sql = @"SELECT  GPM.[Id]
                                ,INR.ID GRNNO
								,GPM.[CompanyGroupId]
								,GPM.[CompanyId]
								,GPM.[PlantId]
								,GPM.[GatePassType]
								,GPM.[GatePassStatus]                             
								,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
								,GPM.[FromEmployeeId]
								, EI.EmployeeName CheckedByName
								,GPM.[Through]
								,GPM.[CourierName]
								,GPM.[RunnerEmployeeId]
								, EI1.EmployeeName ApprovedByName                             
								,GPM.[Remarks]
								,GPM.[CheckedBy]
								,GPM.[CheckedByStatus]
								,GPM.[CheckedHoldRejectReason]
								,GPM.[ApprovedBy]
								,GPM.[ApprovedByStatus]
								,GPM.[ApprovedHoldRejectReason]                            
								,GPM.[AddedBy]
								,GPM.[AddedDate]
								,GPM.[AddedFromIP]
								,GPM.[UpdatedBy]
								,GPM.[UpdatedDate]
								,GPM.[UpdatedFromIP]--,GPM.ChallanNo
								,PurchaseReturnId,InventoryTransferId,InventorySalesId,InventoryScrapId,FixedAssetSalesId,FixedAssetScrapId
							FROM [TRN].[InOutGatePassMaster] GPM
							LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.CheckedBy
							LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.ApprovedBy 
                            left jOIN [TRN].[PurchaseReturn] AS IR ON IR.Id=GPM.PurchaseReturnId
                            LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId   
	                        LEFT JOIN TRn.InventoryReceive AS INR ON INR.ID=IR.InventoryReceiveId
                            Where (GPM.CheckedByStatus ='Hold' OR GPM.CheckedByStatus ='Reject')
                            Order By GPM.[Id] DESC";

			
				}
				else if (tabType == "CheckedList")
				{
					sql = @"SELECT  GPM.[Id]
                                ,INR.ID GRNNO
								,GPM.[CompanyGroupId]
								,GPM.[CompanyId]
								,GPM.[PlantId]
								,GPM.[GatePassType]
								,GPM.[GatePassStatus]                             
								,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
								,GPM.[FromEmployeeId]
								, EI.EmployeeName CheckedByName
								,GPM.[Through]
								,GPM.[CourierName]
								,GPM.[RunnerEmployeeId]
								, EI1.EmployeeName ApprovedByName                             
								,GPM.[Remarks]
								,GPM.[CheckedBy]
								,GPM.[CheckedByStatus]
								,GPM.[CheckedHoldRejectReason]
								,GPM.[ApprovedBy]
								,GPM.[ApprovedByStatus]
								,GPM.[ApprovedHoldRejectReason]                            
								,GPM.[AddedBy]
								,GPM.[AddedDate]
								,GPM.[AddedFromIP]
								,GPM.[UpdatedBy]
								,GPM.[UpdatedDate]
								,GPM.[UpdatedFromIP]--,GPM.ChallanNo
								,PurchaseReturnId,InventoryTransferId,InventorySalesId,InventoryScrapId,FixedAssetSalesId,FixedAssetScrapId
							FROM [TRN].[InOutGatePassMaster] GPM
							LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.CheckedBy
							LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.ApprovedBy 
                            left jOIN [TRN].[PurchaseReturn] AS IR ON IR.Id=GPM.PurchaseReturnId
                            LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId   
	                        LEFT JOIN TRn.InventoryReceive AS INR ON INR.ID=IR.InventoryReceiveId
                            Where GPM.CheckedByStatus ='Checked' 
                            Order By GPM.[Id] DESC";
					
				}

				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
			// return Json(_gateEntryService.PlantWiseGateCbo(IsSysAdmin, UserId, plantId), JsonRequestBehavior.AllowGet);
		}

		#endregion



		#region InOutGatePass UncheckAnd UnApproved Update
		[HttpPost, Authorize]
		public ActionResult InOutGatePassUncheckUpdate(string ComId,string CheckedApprovedStataus,string CheckedHoldRejectReason, Dictionary<string, object> UserSendData)
		{
			try
			{
				Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
				obj.InOutGatePassUncheckUpdate(ComId, CheckedApprovedStataus, CheckedHoldRejectReason,UserSendData);
				return Json(new { Error = false, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{

				return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
			}
		}

		#endregion




		#region Pending Gate Out all Function
		#region Gate Grid data Pending and gate out
		[HttpGet, Authorize]
		public JsonResult GetPendingGateOutList(string tabType)
		{
			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
			try
			{
				var sql = "";
				if (tabType == "pendingGateOutList")
				{
					sql = @"SELECT  GPM.[Id]
                                ,INR.ID GRNNO
								,GPM.[CompanyGroupId]
								,GPM.[CompanyId]
								,GPM.[PlantId]
								,GPM.[GatePassType]
								,GPM.[GatePassStatus]                             
								,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
								,GPM.[FromEmployeeId]
								, EI.EmployeeName CheckedByName
								,GPM.[Through]
								,GPM.[CourierName]
								,GPM.[RunnerEmployeeId]
								, EI1.EmployeeName ApprovedByName                             
								,GPM.[Remarks]
								,GPM.[CheckedBy]
								,GPM.[CheckedByStatus]
								,GPM.[CheckedHoldRejectReason]
								,GPM.[ApprovedBy]
								,GPM.[ApprovedByStatus]
								,GPM.[ApprovedHoldRejectReason]                            
								,GPM.[AddedBy]
								,GPM.[AddedDate]
								,GPM.[AddedFromIP]
								,GPM.[UpdatedBy]
								,GPM.[UpdatedDate]
								,GPM.[UpdatedFromIP]--,GPM.ChallanNo
								,PurchaseReturnId,InventoryTransferId,InventorySalesId,InventoryScrapId,FixedAssetSalesId,FixedAssetScrapId
							FROM [TRN].[InOutGatePassMaster] GPM
							LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.CheckedBy
							LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.ApprovedBy 
                            left jOIN [TRN].[PurchaseReturn] AS IR ON IR.Id=GPM.PurchaseReturnId
                            LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId   
	                        LEFT JOIN TRn.InventoryReceive AS INR ON INR.ID=IR.InventoryReceiveId
                            Where GPM.CheckedByStatus ='Checked' And Isnull(GPM.GateOutStatus,0)=0
                            Order By GPM.[Id] DESC";


				}
				else if (tabType == "GateOutList")
				{
					sql = @"SELECT  GPM.[Id]
                                ,INR.ID GRNNO
								,GPM.[CompanyGroupId]
								,GPM.[CompanyId]
								,GPM.[PlantId]
								,GPM.[GatePassType]
								,GPM.[GatePassStatus]                             
								,REPLACE(CONVERT(CHAR(11), GPM.[GatePassEntryDate], 106),' ','-') AS GatePassEntryDate
								,GPM.[FromEmployeeId]
								, EI.EmployeeName CheckedByName
								,GPM.[Through]
								,GPM.[CourierName]
								,GPM.[RunnerEmployeeId]
								, EI1.EmployeeName ApprovedByName                             
								,GPM.[Remarks]
								,GPM.[CheckedBy]
								,GPM.[CheckedByStatus]
								,GPM.[CheckedHoldRejectReason]
								,GPM.[ApprovedBy]
								,GPM.[ApprovedByStatus]
								,GPM.[ApprovedHoldRejectReason]                            
								,GPM.[AddedBy]
								,GPM.[AddedDate]
								,GPM.[AddedFromIP]
								,GPM.[UpdatedBy]
								,GPM.[UpdatedDate]
								,GPM.[UpdatedFromIP]--,GPM.ChallanNo
								,PurchaseReturnId,InventoryTransferId,InventorySalesId,InventoryScrapId,FixedAssetSalesId,FixedAssetScrapId
							FROM [TRN].[InOutGatePassMaster] GPM
							LEFT JOIN Employeeinformation EI on EI.SystemId= GPM.CheckedBy
							LEFT JOIN Employeeinformation EI1 on EI1.SystemId= GPM.ApprovedBy 
                            left jOIN [TRN].[PurchaseReturn] AS IR ON IR.Id=GPM.PurchaseReturnId
                            LEFT JOIN HKP.Party AS P ON P.Id=IR.PartyId   
	                        LEFT JOIN TRn.InventoryReceive AS INR ON INR.ID=IR.InventoryReceiveId
                            Where GPM.CheckedByStatus ='Checked' And Isnull(GPM.GateOutStatus,0)=1
                            Order By GPM.[Id] DESC";


				}
				

				return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

			}
			catch (Exception ex)
			{
				throw new CustomException(ex.Message, ex,
					Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
					ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
			}
			// return Json(_gateEntryService.PlantWiseGateCbo(IsSysAdmin, UserId, plantId), JsonRequestBehavior.AllowGet);
		}

		#endregion



		#region Pending InOut Gate Pass  Update
		[HttpPost, Authorize]
		public ActionResult PendingInOutGatePassUpdate(string ComId, Dictionary<string, object> UserSendData)
		{
			try
			{
				Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
				obj.PendingInOutGatePassUpdate(ComId, UserSendData);
				return Json(new { Error = false, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{

				return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
			}
		}

		#endregion



		#region Indivisul Gate Pass  Update
		[HttpPost]
		public ActionResult IndivisulGatePassUpdate(string ComId, Dictionary<string, object> UserSendData)
		{
			try
			{
				Library.MaterialManagement.InventoryManagements.InventoryReceiveService obj = new Library.MaterialManagement.InventoryManagements.InventoryReceiveService();
				obj.PendingIndivisulGatePassUpdate(ComId, UserSendData);
				return Json(new { Error = false, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
			}
			catch (Exception ex)
			{

				return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
			}
		}

		#endregion

		#endregion

//		#region AgainstGatePassEntry
//		[HttpPost, Authorize]
//		public ActionResult GateAgainstGatePassExl()

//		{
//			var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
//			try
//			{

//				string fileName = "";
//				fileName = GateOutExcelView("GatePassOut");
//				return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);

//			}
//			catch (Exception ex)
//			{
//				return Json(ex.Message, JsonRequestBehavior.AllowGet); ;
//			}
//		}

//		public string GateOutExcelView(string SheetName)
//		{
//			ExcelEngine excelEngine = null;
//			IApplication application = null;
//			IWorkbook workbook = null;
//			IWorksheet sheet = null;
//			var report = new ReportUtility();
//			var filePath = "";

//			try
//			{

//				excelEngine = new ExcelEngine();
//				application = excelEngine.Excel;
//				workbook = application.Workbooks.Create(1);
//				workbook.Worksheets[0].Name = "Gate Pass Out";
//				sheet = workbook.Worksheets[0];
//				DataTable data;
//				GetAgainstGetePassEntry(out data);
//				int ROW = 6; int COL = 1;

//				#region Columns

//				report.SetHeaderText(ref sheet, ROW, COL, "RGPNo", 9, ExcelHAlign.HAlignRight);				
//				int ColRGPNo = COL;
//				COL++;

//				sheet[ROW, COL].Text = "RGPDate";
//				sheet[ROW, COL].ColumnWidth = 10;
//				int ColRGPDate = COL;
//				COL++;

//				sheet[ROW, COL].Text = "Party";
//				sheet[ROW, COL].ColumnWidth = 16;
//				sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
//				int ColParty = COL;
//				COL++;

//				sheet[ROW, COL].Text = "City";
//				sheet[ROW, COL].ColumnWidth = 16;
//				sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
//				int ColCity = COL;
//				COL++;

//				sheet[ROW, COL].Text = "Material";
//				sheet[ROW, COL].ColumnWidth = 16;
//				sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
//				int ColItemDesc = COL;
//				COL++;

//				sheet[ROW, COL].Text = "Article";
//				sheet[ROW, COL].ColumnWidth = 16;
//				sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
//				int ColArticle = COL;
//				COL++;

//				sheet[ROW, COL].Text = "UOM";
//				sheet[ROW, COL].ColumnWidth = 8;
//				sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
//				int ColUOM = COL;
//				COL++;

//				sheet[ROW, COL].Text = "OutQty";
//				sheet[ROW, COL].ColumnWidth = 10;
//				sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
//				int ColOutQty = COL;
//				COL++;

//				sheet[ROW, COL].Text = "Rate";
//				sheet[ROW, COL].ColumnWidth = 10;
//				sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
//				int ColRate = COL;
//				COL++;

//				sheet[ROW, COL].Text = "Amount";
//				sheet[ROW, COL].ColumnWidth = 12;
//				sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
//				int ColAmount = COL;
//				COL++;

//				sheet[ROW, COL].Text = "ReceivedQty";
//				sheet[ROW, COL].ColumnWidth = 10;
//				sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
//				int ColInQty = COL;
//				COL++;

//				sheet[ROW, COL].Text = "Balance";
//				sheet[ROW, COL].ColumnWidth = 8;
//				sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
//				int ColBal = COL;
//				COL++;

//				sheet[ROW, COL].Text = "Expected Return Date";
//				sheet[ROW, COL].ColumnWidth = 10;
//				sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
//				int ColReturnDate = COL;
//				COL++;

//				sheet[ROW, COL].Text = "Status";
//				sheet[ROW, COL].ColumnWidth = 10;
//				sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
//				int ColStatus = COL;
//				COL++;

//                //sheet[ROW, COL].Text = "Challan No.";
//                //sheet[ROW, COL].ColumnWidth = 16;
//                //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
//                //int ColChallanNo = COL;
//                //COL++;

//                //sheet[ROW, COL].Text = "Gate Pass Status";
//                //sheet[ROW, COL].ColumnWidth = 16;
//                //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
//                //int ColGatePassSts = COL;
//                //COL++;

//                //sheet[ROW, COL].Text = "Gate Pass Type";
//                //sheet[ROW, COL].ColumnWidth = 16;
//                //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
//                //int ColGatePassType = COL;
//                //COL++;

//                sheet[ROW, COL].Text = "No Of Packags";
//                sheet[ROW, COL].ColumnWidth = 10;
//                sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
//                int ColNoOfPackags = COL;

//                //sheet[ROW, COL].Text = "GatePassStatus";
//                //sheet[ROW, COL].ColumnWidth = 16;
//                //sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
//                //int ColGatePassStatus = COL;
//                //COL++;

//    //            sheet[ROW, COL].Text = "NoOfPackages";
//				//sheet[ROW, COL].ColumnWidth = 16;
//				//sheet.Range[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
//				//int ColNoOfPackages = COL;
//				#endregion Columns

//				int endCol = COL;
//				sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Black;
//				sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Color = ExcelKnownColors.White;
//				sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
//				sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Size = 9f;
//				sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
//				sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

//				ROW++;
//				int startRow = ROW;
//				double[] arr = new double[3];
//				for (int i = 0; i < data.Rows.Count; i++)
//				{

//					sheet[ROW, ColRGPNo].Number = clsStaticInfo.dbl(data.Rows[i]["Id"].ToString());
//					sheet[ROW, ColRGPDate].Text = data.Rows[i]["RGPDate"].ToString();
//					sheet[ROW, ColParty].Text = data.Rows[i]["Party"].ToString();
//					sheet[ROW, ColCity].Text = data.Rows[i]["City"].ToString();
//					sheet[ROW, ColItemDesc].Text = data.Rows[i]["ItemDescription"].ToString();
//					sheet[ROW, ColUOM].Text = data.Rows[i]["UOM"].ToString();
//					sheet[ROW, ColOutQty].Number = clsStaticInfo.dbl(data.Rows[i]["OutQty"].ToString());
//					sheet[ROW, ColRate].Number = clsStaticInfo.dbl(data.Rows[i]["Rate"].ToString());
//					sheet[ROW, ColAmount].Number = clsStaticInfo.dbl(data.Rows[i]["Amount"].ToString());
//					sheet[ROW, ColInQty].Number = clsStaticInfo.dbl(data.Rows[i]["ReceivedQty"].ToString());
//					sheet[ROW, ColBal].Number = clsStaticInfo.dbl(data.Rows[i]["balance"].ToString());
//					sheet[ROW, ColReturnDate].Text = data.Rows[i]["ReturnableDate"].ToString();
//					//sheet[ROW, ColChallanNo].Text = data.Rows[i]["ChallanNo"].ToString();
//					//sheet[ROW, ColGatePassSts].Text = data.Rows[i]["GatePassStatus"].ToString();
//					//sheet[ROW, ColGatePassType].Text = data.Rows[i]["GatePassType"].ToString();
//					sheet[ROW, ColNoOfPackags].Number = clsStaticInfo.dbl(data.Rows[i]["NoOfPackages"].ToString());
//					sheet[ROW, ColStatus].Text = data.Rows[i]["Status"].ToString();
//					sheet[ROW, ColArticle].Text = data.Rows[i]["Article"].ToString();
					
					
//					ROW++;
//				}


//				sheet.UsedRange.WrapText = false;
//				sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
//				sheet.Range[startRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;
//				sheet["A" + startRow.ToString()].FreezePanes();

//				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
//				ReportUtility reportUtility = new ReportUtility();
//				reportUtility.PlantHeader(ref sheet, endCol, "Gate Passout", identity.PlantId);
//				reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
//				sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
//				sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
//				sheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
//				sheet.UsedRange.WrapText = false;
//				sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
//				sheet.IsGridLinesVisible = true;
//				sheet.PageSetup.TopMargin = 0.2;
//				sheet.PageSetup.BottomMargin = 0.8;
//				//sheet.PageSetup.PrintTitleRows = "$1:$6";
//				sheet.PageSetup.LeftMargin = 0.2;
//				sheet.PageSetup.RightMargin = 0.2;
//				sheet.PageSetup.Orientation = ExcelPageOrientation.Landscape;
//				sheet.PageSetup.FitToPagesTall = 0;
//				sheet.PageSetup.FitToPagesWide = 1;
//				sheet.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
//				sheet.PageSetup.CenterHorizontally = true;


//				filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xlsx");
//				workbook.SaveAs(filePath);
//				workbook.Close();
//				excelEngine.Dispose();
//				return filePath;

//			}
//			catch (Exception ex)
//			{
//				throw ex;
//			}

//		}

//		// Written by Nitesh
//		public void GetAgainstGetePassEntry(out DataTable data)
//		{
//			try
//			{
//				var sql = @"select gpm.Id,gdp.Id Detail_Row, FORMAT(gpm.GatePassEntryDate,'dd-MMM-yyyy') RGPDate, P.UserName Party, C.UserName City, MM.UserName ItemDescription
//,MMA.StandardName Article, UOM.UserName UOM
//,ISNULL(gdp.TransactionQty,0) OutQty,isnull(gdp.Rate,0)Rate,Amount=IsNULL(gdp.TransactionQty*gdp.Rate,0), Isnull(gpd2.InQty,0) ReceivedQty
//,balance=isnull(gdp.TransactionQty-gpd2.InQty,0),  am.Address1
//,  gpm.InvoiceNo,  FORMAT(gpm.ReturnableDate,'dd-MMM-yyyy')ReturnableDate, '' LotDate, gpd2.ChallanNo,
//gpm.GatePassStatus,gpm.GatePassType,gpm.NoOfPackages
//,[Status]= case when gdp.TransactionQty-gpd2.InQty=0 then 'Received' else 'Pending' end
//        from trn.GatePassDetails gdp
//        left join MST.MaterialMaster MM on MM.Id = gdp.MaterialMasterId
//        left join MST.MaterialMasterArticle MMA on MMA.Id = gdp.ArticleId
//        join TRN.GatePassMaster gpm on gpm.Id=gdp.GatePassMasterId
//        left join hkp.Party P ON P.Id=gpm.ToPartyCode
//        left join MST.AddressMaster am on am.Id=p.AddressMasterId
//        left join SCS.City C on C.Id = am.CityId
//        left join SCS.UnitOfMeasurement UOM on UOM.Id = gdp.TransactionUoMId
//        left join(
//                    select sum(isnull(cgpd.TransactionQty,0)) InQty,gpmR.ChallanNo,cgpd.ChallanNoDetailId, cgpd.Rate,cgpd.MaterialMasterId,cgpd.ArticleId

 

//                    from TRN.GatePassDetails cgpd
//                    left join TRN.GatePassMaster gpmR ON gpmR.id=cgpd.GatePassMasterId --and gpmR.GatePassType='Return' and gpmR.GatePassStatus='NonReturnable'

 

//                    group by gpmR.ChallanNo, cgpd.Rate,cgpd.MaterialMasterId,cgpd.ArticleId,cgpd.ChallanNoDetailId
//                ) gpd2 on gpd2.ChallanNo=gpm.Id and gdp.MaterialMasterId=gpd2.MaterialMasterId and gpd2.ChallanNoDetailId=gdp.Id
//      where gpm.GatePassType='Send' and gpm.GatePassStatus='Returnable'
//	  ";

//				data = _sqlRepository.GetDataTable(sql);
//			}
//			catch (Exception ex)
//			{
//				throw ex;
//			}
//		}
//		#endregion AgainstGatePassEntry

	}


}