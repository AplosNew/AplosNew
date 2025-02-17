using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Addresses;
using Library.Model.Parties;
using Library.Security.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Parties;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Linq;
using System.Web.Mvc;
using System.Linq.Expressions;
using Library.Service.Properties;

namespace Aplos.Areas.Parties.Controllers
{
    public class PartyController : BaseController
    {
        private readonly IPartyService _partyService;
        private readonly IPartyBankService _partyBankService;
        private readonly IPartyReportService _partyReportService;
        private readonly ISqlRepository _sqlRepository;
        public PartyController(
            IPartyService partyService
            , IPartyBankService partyBankService
            , IPartyReportService partyReportService
            , ISqlRepository sqlRepository)
        {
            _partyService = partyService;
            _partyBankService = partyBankService;
            _partyReportService = partyReportService;
            _sqlRepository = sqlRepository;
        }

        [HttpGet]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Approve()
        {
            return View();
        }

        [Authorize, HttpGet]
        public ActionResult Director()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ViewBag.CompanyGroup = identity.CompanyGroupName;
            return View();
        }

        [Authorize, HttpGet]
        public ActionResult PartyReport()
        {
            return View();
        }

        [Authorize, HttpGet]
        public ActionResult PartyMapping()
        {
            return View();
        }

        [Authorize, HttpGet]
        public ActionResult PartyBrand()
        {
            return View();
        }

        [Authorize, HttpGet]
        public ActionResult PartyCategory()
        {
            return View();
        }

        [Authorize, HttpGet]
        public ActionResult PartySubCategory()
        {
            return View();
        }

        #region --Party Operations

        [Authorize, HttpGet]
        public ActionResult GetCompanyPartyNewList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_partyService.Query(identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetCompanyPartyList(string partyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_partyService.Query(identity.CompanyGroupId, partyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCompanyDirectorDataList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_partyService.GetCompanyPartyList(parameters, identity.CompanyGroupId, PartyType.Director), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCompanyOtherDataList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_partyService.GetCompanyPartyList(parameters, identity.CompanyGroupId, PartyType.Other), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCompanyPartyDataList(GridParameter parameters, string partyType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(GetCompanyPartyList(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, partyType), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetCompanyPartyDataListByGateEntry(GridParameter parameters, string partyType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(GetCompanyPartyListByGateEntry(parameters, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, partyType), JsonRequestBehavior.AllowGet);
        }


        [HttpPost, Authorize]
        public JsonResult GetCompanyPartyDataListByGateEntryANDPO(string column, string value, string partyType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var res = GetCompanyPartyListByGateEntryANDPO(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, column, value, partyType);
            var jsondata = Json(res, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        public List<Dictionary<string, object>> GetCompanyPartyListByGateEntryANDPO(string companyGroupId, string companyId, string plantId, string column, string value, string partyType)
        {
            try
            {
                string temp = null;
                if (partyType == "Vendor" || partyType == "Customer")
                {
                    temp = partyType;
                }
                if (partyType == null || partyType == "null")
                {
                    temp = "Vendor" + "','" + "Customer";
                }
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"select top 100 * from (SELECT  P.Id AS PartyId, P.Code AS PartyCode, P.UserName AS PartyName, P.Id, P.Code, P.UserName, CP.PartyType, CP.PartyAccountGroupId, PAG.Code AS PartyAccountGroupCode, PAG.UserName AS PartyAccountGroupName, CP.CurrencyId, C.Code AS CurrencyCode, C.[Name] AS CurrencyName
                                    , CP.PaymentTermId, PT.Code AS PaymentTermCode, PT.UserName AS PaymentTermName, CP.IsPaymentTermChangeable
                                    , NULL AS InvoicingPartyPlantId, NULL AS DeliveryPartyPlantId, CO.Code AS CountryCode, CO.UserName AS CountryName, S.Code AS StateCode, S.UserName AS StateName
                                    , RGL.ReconciliationGLId, RGL.ReconciliationGLCode, RGL.ReconciliationGLName
                                    , RGL.ReconciliationBudgetId, RGL.ReconciliationBudgetCode, RGL.ReconciliationBudgetName
                                    , RGL.ReconciliationActivityId, RGL.ReconciliationActivityCode, RGL.ReconciliationActivityName
                                    , CP.TaxApplicable, CP.IsTaxApplicableChangeable
									, (SELECT COUNT(Id) FROM [HKP].[PartyPlant] WHERE PartyId=P.Id) AS TotalPartyPlant
                                    FROM [HKP].[Party] AS P
                                    LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId=P.Id
                                    LEFT JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=CP.PartyAccountGroupId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=CP.CurrencyId
                                    LEFT JOIN [MST].[PaymentTerm] AS PT ON PT.Id=CP.PaymentTermId
                                    LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=P.AddressMasterId
									LEFT JOIN [SCS].[Country] AS CO ON CO.Id=AM.CountryId
									LEFT JOIN [SCS].[State] AS S ON S.Id=AM.StateId
                                    LEFT JOIN(
                                    SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS ReconciliationGLId, GL.AccountCode AS ReconciliationGLCode, GL.UserName AS ReconciliationGLName
                                    , CPGL.BudgetMasterId AS ReconciliationBudgetId, B.Code AS ReconciliationBudgetCode, B.UserName AS ReconciliationBudgetName
                                    , CPGL.ActivityId AS ReconciliationActivityId, A.Code AS ReconciliationActivityCode, A.UserName AS ReconciliationActivityName
                                    FROM [HKP].[CompanyPartyGL] AS CPGL
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
                                    WHERE CPGL.PartyGLType='" + PartyGLType.ReconciliationGL + @"'
                                    ) AS RGL ON RGL.CompanyPartyId=CP.Id
                                    JOIN (SELECT DISTINCT G.PartyId FROM TRN.GateEntry G   WHERE ISNULL(G.Id,'') NOT IN (SELECT ISNULL(GateEntryNo,'') FROM TRN.InventoryReceive ) AND G.PartyId<>'' AND G.FlagStatus='OK') GE ON GE.PartyId=P.Id
                                    JOIN (SELECT DISTINCT po.PartyId FROM TRN.PurchaseOrder po join trn.PurchaseOrderDetail pod ON pod.InventoryReceiveId=po.Id 
						            WHERE po.IsClosed=0 AND po.IsApproved=1 and pod.TransactionQty>(select isnull(sum(ird.TransactionQty),0) TransactionQty from trn.InventoryReceiveDetail ird 
						            join trn.InventoryReceive Ir on ir.Id=ird.InventoryReceiveId
						            where ird.PODetailsId=pod.Id and 
						            (ir.AuthorizedByStatus!='Reject' OR ir.CheckedByStatus!='Reject'))) PO ON PO.PartyId=P.Id
                                    WHERE P.Archive=0 AND P.Active=1 AND P.CompanyGroupId='" + companyGroupId + "' AND P.PartyType IN ('" + PartyType.Party + "', '" + PartyType.Company + "') AND CP.CompanyId='" + companyId + "' AND CP.PlantId='" + plantId + "' AND CP.PartyType in ('" + temp + "')) AS TEMP WHERE " + strkey + " ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }


        [HttpGet, Authorize]
        public JsonResult GetCompanyPartyDataListByPlantId(GridParameter parameters, string CompanyId, string PlantId, string partyType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(GetCompanyPartyList(parameters, identity.CompanyGroupId, CompanyId, PlantId, partyType), JsonRequestBehavior.AllowGet);
        }

        public GridModel GetCompanyPartyList(GridParameter parameters, string companyGroupId, string companyId, string plantId, string customerVendor)
        {
            try
            {
                parameters.CmdText = @"
                                    SELECT top(300) P.Id AS PartyId, P.Code AS PartyCode, P.UserName AS PartyName, P.Id, P.Code, P.UserName, CP.PartyType, CP.PartyAccountGroupId, PAG.Code AS PartyAccountGroupCode, PAG.UserName AS PartyAccountGroupName, CP.CurrencyId, C.Code AS CurrencyCode, C.[Name] AS CurrencyName
                                    , CP.PaymentTermId, PT.Code AS PaymentTermCode, PT.UserName AS PaymentTermName, CP.IsPaymentTermChangeable
                                    , NULL AS InvoicingPartyPlantId, NULL AS DeliveryPartyPlantId, CO.Code AS CountryCode, CO.UserName AS CountryName, S.Code AS StateCode, S.UserName AS StateName
                                    , RGL.ReconciliationGLId, RGL.ReconciliationGLCode, RGL.ReconciliationGLName
                                    , RGL.ReconciliationBudgetId, RGL.ReconciliationBudgetCode, RGL.ReconciliationBudgetName
                                    , RGL.ReconciliationActivityId, RGL.ReconciliationActivityCode, RGL.ReconciliationActivityName
                                    , DGL.DownPaymentGLId, DGL.DownPaymentGLCode, DGL.DownPaymentGLName
                                    , DGL.DownPaymentBudgetId, DGL.DownPaymentBudgetCode, DGL.DownPaymentBudgetName
                                    , DGL.DownPaymentActivityId, DGL.DownPaymentActivityCode, DGL.DownPaymentActivityName
                                    , SGL.SuspenseGLId, SGL.SuspenseGLCode, SGL.SuspenseGLName
                                    , SGL.SuspenseBudgetId, SGL.SuspenseBudgetCode, SGL.SuspenseBudgetName
                                    , SGL.SuspenseActivityId, SGL.SuspenseActivityCode, SGL.SuspenseActivityName
                                    , CP.TaxApplicable, CP.IsTaxApplicableChangeable
									, (SELECT COUNT(Id) FROM [HKP].[PartyPlant] WHERE PartyId=P.Id) AS TotalPartyPlant
                                    FROM [HKP].[Party] AS P
                                    LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId=P.Id
                                    LEFT JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=CP.PartyAccountGroupId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=CP.CurrencyId
                                    LEFT JOIN [MST].[PaymentTerm] AS PT ON PT.Id=CP.PaymentTermId
                                    LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=P.AddressMasterId
									LEFT JOIN [SCS].[Country] AS CO ON CO.Id=AM.CountryId
									LEFT JOIN [SCS].[State] AS S ON S.Id=AM.StateId
                                    LEFT JOIN(
                                    SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS ReconciliationGLId, GL.AccountCode AS ReconciliationGLCode, GL.UserName AS ReconciliationGLName
                                    , CPGL.BudgetMasterId AS ReconciliationBudgetId, B.Code AS ReconciliationBudgetCode, B.UserName AS ReconciliationBudgetName
                                    , CPGL.ActivityId AS ReconciliationActivityId, A.Code AS ReconciliationActivityCode, A.UserName AS ReconciliationActivityName
                                    FROM [HKP].[CompanyPartyGL] AS CPGL
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
                                    WHERE CPGL.PartyGLType='" + PartyGLType.ReconciliationGL + @"'
                                    ) AS RGL ON RGL.CompanyPartyId=CP.Id
                                    LEFT JOIN(
                                    SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS DownPaymentGLId, GL.AccountCode AS DownPaymentGLCode, GL.UserName AS DownPaymentGLName
                                    , CPGL.BudgetMasterId AS DownPaymentBudgetId, B.Code AS DownPaymentBudgetCode, B.UserName AS DownPaymentBudgetName
                                    , CPGL.ActivityId AS DownPaymentActivityId, A.Code AS DownPaymentActivityCode, A.UserName AS DownPaymentActivityName
                                    FROM [HKP].[CompanyPartyGL] AS CPGL
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
                                    WHERE CPGL.PartyGLType='" + PartyGLType.DownPaymentGL + @"'
                                    ) AS DGL ON DGL.CompanyPartyId=CP.Id
                                    LEFT JOIN(
										SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS SuspenseGLId, GL.AccountCode AS SuspenseGLCode, GL.UserName AS SuspenseGLName
										, CPGL.BudgetMasterId AS SuspenseBudgetId, B.Code AS SuspenseBudgetCode, B.UserName AS SuspenseBudgetName
										, CPGL.ActivityId AS SuspenseActivityId, A.Code AS SuspenseActivityCode, A.UserName AS SuspenseActivityName
										FROM [HKP].[CompanyPartyGL] AS CPGL
										LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
										LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
										LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
										LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
										WHERE CPGL.PartyGLType='" + PartyGLType.SuspenseGL + @"'
                                    ) AS SGL ON SGL.CompanyPartyId=CP.Id
                                    WHERE P.Archive=0 AND P.Active=1 AND P.CompanyGroupId='" + companyGroupId + "' AND P.PartyType IN ('" + PartyType.Party + "', '" + PartyType.Company + "') AND CP.CompanyId='" + companyId + "' AND CP.PlantId='" + plantId + "' ";
                // If this params null will return all customer and vendor list either specific.
                if (!string.IsNullOrEmpty(customerVendor))
                    parameters.CmdText += " AND CP.PartyType='" + customerVendor + "'";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }
        public GridModel GetCompanyPartyListByGateEntry(GridParameter parameters, string companyGroupId, string companyId, string plantId, string customerVendor)
        {
            try
            {
                parameters.CmdText = @"
                                    SELECT top(300) P.Id AS PartyId, P.Code AS PartyCode, P.UserName AS PartyName, P.Id, P.Code, P.UserName, CP.PartyType, CP.PartyAccountGroupId, PAG.Code AS PartyAccountGroupCode, PAG.UserName AS PartyAccountGroupName, CP.CurrencyId, C.Code AS CurrencyCode, C.[Name] AS CurrencyName
                                    , CP.PaymentTermId, PT.Code AS PaymentTermCode, PT.UserName AS PaymentTermName, CP.IsPaymentTermChangeable
                                    , NULL AS InvoicingPartyPlantId, NULL AS DeliveryPartyPlantId, CO.Code AS CountryCode, CO.UserName AS CountryName, S.Code AS StateCode, S.UserName AS StateName
                                    , RGL.ReconciliationGLId, RGL.ReconciliationGLCode, RGL.ReconciliationGLName
                                    , RGL.ReconciliationBudgetId, RGL.ReconciliationBudgetCode, RGL.ReconciliationBudgetName
                                    , RGL.ReconciliationActivityId, RGL.ReconciliationActivityCode, RGL.ReconciliationActivityName
                                    , CP.TaxApplicable, CP.IsTaxApplicableChangeable
									, (SELECT COUNT(Id) FROM [HKP].[PartyPlant] WHERE PartyId=P.Id) AS TotalPartyPlant
                                    FROM [HKP].[Party] AS P
                                    LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId=P.Id
                                    LEFT JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=CP.PartyAccountGroupId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=CP.CurrencyId
                                    LEFT JOIN [MST].[PaymentTerm] AS PT ON PT.Id=CP.PaymentTermId
                                    LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=P.AddressMasterId
									LEFT JOIN [SCS].[Country] AS CO ON CO.Id=AM.CountryId
									LEFT JOIN [SCS].[State] AS S ON S.Id=AM.StateId
                                    LEFT JOIN(
                                    SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS ReconciliationGLId, GL.AccountCode AS ReconciliationGLCode, GL.UserName AS ReconciliationGLName
                                    , CPGL.BudgetMasterId AS ReconciliationBudgetId, B.Code AS ReconciliationBudgetCode, B.UserName AS ReconciliationBudgetName
                                    , CPGL.ActivityId AS ReconciliationActivityId, A.Code AS ReconciliationActivityCode, A.UserName AS ReconciliationActivityName
                                    FROM [HKP].[CompanyPartyGL] AS CPGL
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
                                    WHERE CPGL.PartyGLType='" + PartyGLType.ReconciliationGL + @"'
                                    ) AS RGL ON RGL.CompanyPartyId=CP.Id
                                    JOIN (SELECT DISTINCT G.PartyId FROM TRN.GateEntry G   WHERE ISNULL(G.Id,'') NOT IN (SELECT ISNULL(GateEntryNo,'') FROM TRN.InventoryReceive ) AND G.PartyId<>'' AND G.FlagStatus='OK') GE ON GE.PartyId=P.Id
                                    WHERE P.Archive=0 AND P.Active=1 AND P.CompanyGroupId='" + companyGroupId + "' AND P.PartyType IN ('" + PartyType.Party + "', '" + PartyType.Company + "') AND CP.CompanyId='" + companyId + "' AND CP.PlantId='" + plantId + "' ";
                // If this params null will return all customer and vendor list either specific.
                if (!string.IsNullOrEmpty(customerVendor))
                    parameters.CmdText += " AND CP.PartyType='" + customerVendor + "'";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }


        [HttpPost, Authorize]
        public JsonResult GetCompanyPartyDataSearch(string column, string value, string partyType, string CompanyId, string PlantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(CompanyId))
            {
                CompanyId = identity.CompanyId;
            }
            if (string.IsNullOrEmpty(PlantId))
            {
                PlantId = identity.PlantId;
            }
            var res = GetCompanyPartyListNew(identity.CompanyGroupId, CompanyId, PlantId, column, value, partyType);
            var jsondata = Json(res, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public JsonResult GetCompanyPartyDataListNew(string column, string value, string partyType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var res = GetCompanyPartyListNew(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, column, value, partyType);
            var jsondata = Json(res, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        public List<Dictionary<string, object>> GetCompanyPartyListNew(string companyGroupId, string companyId, string plantId, string column, string value, string customerVendor)
        {
            try
            {
                string temp = null;
                if (customerVendor == "Vendor" || customerVendor == "Customer" || customerVendor == "Director")
                {
                    temp = customerVendor;
                }
                if (customerVendor == null || customerVendor == "null")
                {
                    temp = "Vendor" + "','" + "Customer";
                }
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"select top 100 * from (SELECT CheckState=CAST(0 AS bit),P.Id AS PartyId, P.Code AS PartyCode, P.UserName AS PartyName, P.Id, P.Code, P.UserName, CP.PartyType, CP.PartyAccountGroupId, PAG.Code AS PartyAccountGroupCode, PAG.UserName AS PartyAccountGroupName, CP.CurrencyId, C.Code AS CurrencyCode, C.[Name] AS CurrencyName
                                    , CP.PaymentTermId, PT.Code AS PaymentTermCode, PT.UserName AS PaymentTermName, CP.IsPaymentTermChangeable
                                    , NULL AS InvoicingPartyPlantId, NULL AS DeliveryPartyPlantId, CO.Code AS CountryCode, CO.UserName AS CountryName, S.Code AS StateCode, S.UserName AS StateName
                                    , RGL.ReconciliationGLId, RGL.ReconciliationGLCode, RGL.ReconciliationGLName
                                    , RGL.ReconciliationBudgetId, RGL.ReconciliationBudgetCode, RGL.ReconciliationBudgetName
                                    , RGL.ReconciliationActivityId, RGL.ReconciliationActivityCode, RGL.ReconciliationActivityName
                                    , DGL.DownPaymentGLId, DGL.DownPaymentGLCode, DGL.DownPaymentGLName
                                    , DGL.DownPaymentBudgetId, DGL.DownPaymentBudgetCode, DGL.DownPaymentBudgetName
                                    , DGL.DownPaymentActivityId, DGL.DownPaymentActivityCode, DGL.DownPaymentActivityName
                                    , SGL.SuspenseGLId, SGL.SuspenseGLCode, SGL.SuspenseGLName
                                    , SGL.SuspenseBudgetId, SGL.SuspenseBudgetCode, SGL.SuspenseBudgetName
                                    , SGL.SuspenseActivityId, SGL.SuspenseActivityCode, SGL.SuspenseActivityName
                                    , CP.TaxApplicable, CP.IsTaxApplicableChangeable
									, (SELECT COUNT(Id) FROM [HKP].[PartyPlant] WHERE PartyId=P.Id) AS TotalPartyPlant
                                    FROM [HKP].[Party] AS P
                                    LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId=P.Id
                                    LEFT JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=CP.PartyAccountGroupId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=CP.CurrencyId
                                    LEFT JOIN [MST].[PaymentTerm] AS PT ON PT.Id=CP.PaymentTermId
                                    LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=P.AddressMasterId
									LEFT JOIN [SCS].[Country] AS CO ON CO.Id=AM.CountryId
									LEFT JOIN [SCS].[State] AS S ON S.Id=AM.StateId
                                    LEFT JOIN(
                                    SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS ReconciliationGLId, GL.AccountCode AS ReconciliationGLCode, GL.UserName AS ReconciliationGLName
                                    , CPGL.BudgetMasterId AS ReconciliationBudgetId, B.Code AS ReconciliationBudgetCode, B.UserName AS ReconciliationBudgetName
                                    , CPGL.ActivityId AS ReconciliationActivityId, A.Code AS ReconciliationActivityCode, A.UserName AS ReconciliationActivityName
                                    FROM [HKP].[CompanyPartyGL] AS CPGL
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
                                    WHERE CPGL.PartyGLType='" + PartyGLType.ReconciliationGL + @"'
                                    ) AS RGL ON RGL.CompanyPartyId=CP.Id
                                    LEFT JOIN(
                                    SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS DownPaymentGLId, GL.AccountCode AS DownPaymentGLCode, GL.UserName AS DownPaymentGLName
                                    , CPGL.BudgetMasterId AS DownPaymentBudgetId, B.Code AS DownPaymentBudgetCode, B.UserName AS DownPaymentBudgetName
                                    , CPGL.ActivityId AS DownPaymentActivityId, A.Code AS DownPaymentActivityCode, A.UserName AS DownPaymentActivityName
                                    FROM [HKP].[CompanyPartyGL] AS CPGL
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
                                    WHERE CPGL.PartyGLType='" + PartyGLType.DownPaymentGL + @"'
                                    ) AS DGL ON DGL.CompanyPartyId=CP.Id
                                    LEFT JOIN(
										SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS SuspenseGLId, GL.AccountCode AS SuspenseGLCode, GL.UserName AS SuspenseGLName
										, CPGL.BudgetMasterId AS SuspenseBudgetId, B.Code AS SuspenseBudgetCode, B.UserName AS SuspenseBudgetName
										, CPGL.ActivityId AS SuspenseActivityId, A.Code AS SuspenseActivityCode, A.UserName AS SuspenseActivityName
										FROM [HKP].[CompanyPartyGL] AS CPGL
										LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
										LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
										LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
										LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
										WHERE CPGL.PartyGLType='" + PartyGLType.SuspenseGL + @"'
                                    ) AS SGL ON SGL.CompanyPartyId=CP.Id
                                    WHERE P.Archive=0 AND P.Active=1 AND P.IsApproved=1 AND P.CompanyGroupId='" + companyGroupId + "' AND CP.PartyType IN ('" + temp + "') AND CP.CompanyId='" + companyId + "' AND CP.PlantId='" + plantId + @"'
                                    ) AS TEMP WHERE " + strkey + " order by Code ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }
        [HttpPost, Authorize]
        public JsonResult GetCompanyPartyDataListNew_Invoice(string column, string value, string partyType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var res = GetCompanyPartyListNew_Invoice(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, column, value, partyType);
            var jsondata = Json(res, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        public List<Dictionary<string, object>> GetCompanyPartyListNew_Invoice(string companyGroupId, string companyId, string plantId, string column, string value, string customerVendor)
        {
            try
            {
                string temp = null;
                if (customerVendor == "Vendor" || customerVendor == "Customer" || customerVendor == "Director")
                {
                    temp = customerVendor;
                }
                if (customerVendor == null || customerVendor == "null")
                {
                    temp = "Vendor" + "','" + "Customer";
                }
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"select top 500 * from (SELECT CheckState=CAST(0 AS bit),P.Id AS PartyId, P.Code AS PartyCode, P.UserName AS PartyName, P.Id, P.Code, P.UserName, CP.PartyType, CP.PartyAccountGroupId, PAG.Code AS PartyAccountGroupCode, PAG.UserName AS PartyAccountGroupName, CP.CurrencyId, C.Code AS CurrencyCode, C.[Name] AS CurrencyName
                                    , CP.PaymentTermId, PT.Code AS PaymentTermCode, PT.UserName AS PaymentTermName, CP.IsPaymentTermChangeable
                                    , NULL AS InvoicingPartyPlantId, NULL AS DeliveryPartyPlantId, CO.Code AS CountryCode, CO.UserName AS CountryName, S.Code AS StateCode, S.UserName AS StateName
                                    , RGL.ReconciliationGLId, RGL.ReconciliationGLCode, RGL.ReconciliationGLName
                                    , RGL.ReconciliationBudgetId, RGL.ReconciliationBudgetCode, RGL.ReconciliationBudgetName
                                    , RGL.ReconciliationActivityId, RGL.ReconciliationActivityCode, RGL.ReconciliationActivityName
                                    , DGL.DownPaymentGLId, DGL.DownPaymentGLCode, DGL.DownPaymentGLName
                                    , DGL.DownPaymentBudgetId, DGL.DownPaymentBudgetCode, DGL.DownPaymentBudgetName
                                    , DGL.DownPaymentActivityId, DGL.DownPaymentActivityCode, DGL.DownPaymentActivityName
                                    , SGL.SuspenseGLId, SGL.SuspenseGLCode, SGL.SuspenseGLName
                                    , SGL.SuspenseBudgetId, SGL.SuspenseBudgetCode, SGL.SuspenseBudgetName
                                    , SGL.SuspenseActivityId, SGL.SuspenseActivityCode, SGL.SuspenseActivityName
                                    , CP.TaxApplicable, CP.IsTaxApplicableChangeable
									, (SELECT COUNT(Id) FROM [HKP].[PartyPlant] WHERE PartyId=P.Id) AS TotalPartyPlant
                                    FROM [HKP].[Party] AS P
                                    LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId=P.Id
                                    LEFT JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=CP.PartyAccountGroupId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=CP.CurrencyId
                                    LEFT JOIN [MST].[PaymentTerm] AS PT ON PT.Id=CP.PaymentTermId
                                    LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=P.AddressMasterId
									LEFT JOIN [SCS].[Country] AS CO ON CO.Id=AM.CountryId
									LEFT JOIN [SCS].[State] AS S ON S.Id=AM.StateId
                                    LEFT JOIN(
                                    SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS ReconciliationGLId, GL.AccountCode AS ReconciliationGLCode, GL.UserName AS ReconciliationGLName
                                    , CPGL.BudgetMasterId AS ReconciliationBudgetId, B.Code AS ReconciliationBudgetCode, B.UserName AS ReconciliationBudgetName
                                    , CPGL.ActivityId AS ReconciliationActivityId, A.Code AS ReconciliationActivityCode, A.UserName AS ReconciliationActivityName
                                    FROM [HKP].[CompanyPartyGL] AS CPGL
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
                                    WHERE CPGL.PartyGLType='" + PartyGLType.ReconciliationGL + @"'
                                    ) AS RGL ON RGL.CompanyPartyId=CP.Id
                                    LEFT JOIN(
                                    SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS DownPaymentGLId, GL.AccountCode AS DownPaymentGLCode, GL.UserName AS DownPaymentGLName
                                    , CPGL.BudgetMasterId AS DownPaymentBudgetId, B.Code AS DownPaymentBudgetCode, B.UserName AS DownPaymentBudgetName
                                    , CPGL.ActivityId AS DownPaymentActivityId, A.Code AS DownPaymentActivityCode, A.UserName AS DownPaymentActivityName
                                    FROM [HKP].[CompanyPartyGL] AS CPGL
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
                                    WHERE CPGL.PartyGLType='" + PartyGLType.DownPaymentGL + @"'
                                    ) AS DGL ON DGL.CompanyPartyId=CP.Id
                                    LEFT JOIN(
										SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS SuspenseGLId, GL.AccountCode AS SuspenseGLCode, GL.UserName AS SuspenseGLName
										, CPGL.BudgetMasterId AS SuspenseBudgetId, B.Code AS SuspenseBudgetCode, B.UserName AS SuspenseBudgetName
										, CPGL.ActivityId AS SuspenseActivityId, A.Code AS SuspenseActivityCode, A.UserName AS SuspenseActivityName
										FROM [HKP].[CompanyPartyGL] AS CPGL
										LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
										LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
										LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
										LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
										WHERE CPGL.PartyGLType='" + PartyGLType.SuspenseGL + @"'
                                    ) AS SGL ON SGL.CompanyPartyId=CP.Id
                                    WHERE P.Archive=0 AND P.Active=1 AND P.IsApproved=1 AND P.CompanyGroupId='" + companyGroupId + "' AND CP.PartyType IN ('" + temp + "') AND CP.CompanyId='" + companyId + "' AND CP.PlantId='" + plantId + @"'
                                        AND P.Id IN(SELECT IV.PartyId FROM [TRN].[Invoice] AS IV
                                        LEFT JOIN[TRN].[Voucher] AS V ON V.Id = IV.VoucherId
                                        WHERE IV.SourceType IN ('InventoryPayable','VendorInvoice','SuspensePayable','ServicePayable','EmployeePayable','PostInvoice')
                                        AND IV.Archive = 0 AND IV.IsWrittenOff = 0 AND ISNULL(IV.PurchaseLCId,'')='' AND V.IsPark = 0)                                    
                                    ) AS TEMP WHERE " + strkey + " order by Code ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }
        [HttpPost, Authorize]
        public JsonResult GetCompanyPartyDataListForReport(string column, string value, string partyType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var res = GetCompanyPartyListForReport(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, column, value, partyType);
            var jsondata = Json(res, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        public List<Dictionary<string, object>> GetCompanyPartyListForReport(string companyGroupId, string companyId, string plantId, string column, string value, string customerVendor)
        {
            try
            {
                string temp = null;
                var sql = "";
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                if (customerVendor == "Vendor" || customerVendor == "Customer" || customerVendor == "Director")
                {
                    temp = customerVendor;
                    sql = @"select top 100 * from (SELECT CheckState=CAST(0 AS bit),P.Id AS PartyId, P.Code AS PartyCode, P.UserName AS PartyName, P.Id, P.Code, P.UserName, CP.PartyType, CP.PartyAccountGroupId, PAG.Code AS PartyAccountGroupCode, PAG.UserName AS PartyAccountGroupName, CP.CurrencyId, C.Code AS CurrencyCode, C.[Name] AS CurrencyName
                                    , CP.PaymentTermId, PT.Code AS PaymentTermCode, PT.UserName AS PaymentTermName, CP.IsPaymentTermChangeable
                                    , NULL AS InvoicingPartyPlantId, NULL AS DeliveryPartyPlantId, CO.Code AS CountryCode, CO.UserName AS CountryName, S.Code AS StateCode, S.UserName AS StateName
                                    , RGL.ReconciliationGLId, RGL.ReconciliationGLCode, RGL.ReconciliationGLName
                                    , RGL.ReconciliationBudgetId, RGL.ReconciliationBudgetCode, RGL.ReconciliationBudgetName
                                    , RGL.ReconciliationActivityId, RGL.ReconciliationActivityCode, RGL.ReconciliationActivityName
                                    , DGL.DownPaymentGLId, DGL.DownPaymentGLCode, DGL.DownPaymentGLName
                                    , DGL.DownPaymentBudgetId, DGL.DownPaymentBudgetCode, DGL.DownPaymentBudgetName
                                    , DGL.DownPaymentActivityId, DGL.DownPaymentActivityCode, DGL.DownPaymentActivityName
                                    , SGL.SuspenseGLId, SGL.SuspenseGLCode, SGL.SuspenseGLName
                                    , SGL.SuspenseBudgetId, SGL.SuspenseBudgetCode, SGL.SuspenseBudgetName
                                    , SGL.SuspenseActivityId, SGL.SuspenseActivityCode, SGL.SuspenseActivityName
                                    , CP.TaxApplicable, CP.IsTaxApplicableChangeable
									, (SELECT COUNT(Id) FROM [HKP].[PartyPlant] WHERE PartyId=P.Id) AS TotalPartyPlant
                                    FROM [HKP].[Party] AS P
                                    LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId=P.Id
                                    LEFT JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=CP.PartyAccountGroupId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=CP.CurrencyId
                                    LEFT JOIN [MST].[PaymentTerm] AS PT ON PT.Id=CP.PaymentTermId
                                    LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=P.AddressMasterId
									LEFT JOIN [SCS].[Country] AS CO ON CO.Id=AM.CountryId
									LEFT JOIN [SCS].[State] AS S ON S.Id=AM.StateId
                                    LEFT JOIN(
                                    SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS ReconciliationGLId, GL.AccountCode AS ReconciliationGLCode, GL.UserName AS ReconciliationGLName
                                    , CPGL.BudgetMasterId AS ReconciliationBudgetId, B.Code AS ReconciliationBudgetCode, B.UserName AS ReconciliationBudgetName
                                    , CPGL.ActivityId AS ReconciliationActivityId, A.Code AS ReconciliationActivityCode, A.UserName AS ReconciliationActivityName
                                    FROM [HKP].[CompanyPartyGL] AS CPGL
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
                                    WHERE CPGL.PartyGLType='" + PartyGLType.ReconciliationGL + @"'
                                    ) AS RGL ON RGL.CompanyPartyId=CP.Id
                                    LEFT JOIN(
                                    SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS DownPaymentGLId, GL.AccountCode AS DownPaymentGLCode, GL.UserName AS DownPaymentGLName
                                    , CPGL.BudgetMasterId AS DownPaymentBudgetId, B.Code AS DownPaymentBudgetCode, B.UserName AS DownPaymentBudgetName
                                    , CPGL.ActivityId AS DownPaymentActivityId, A.Code AS DownPaymentActivityCode, A.UserName AS DownPaymentActivityName
                                    FROM [HKP].[CompanyPartyGL] AS CPGL
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
                                    WHERE CPGL.PartyGLType='" + PartyGLType.DownPaymentGL + @"'
                                    ) AS DGL ON DGL.CompanyPartyId=CP.Id
                                    LEFT JOIN(
										SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS SuspenseGLId, GL.AccountCode AS SuspenseGLCode, GL.UserName AS SuspenseGLName
										, CPGL.BudgetMasterId AS SuspenseBudgetId, B.Code AS SuspenseBudgetCode, B.UserName AS SuspenseBudgetName
										, CPGL.ActivityId AS SuspenseActivityId, A.Code AS SuspenseActivityCode, A.UserName AS SuspenseActivityName
										FROM [HKP].[CompanyPartyGL] AS CPGL
										LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
										LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
										LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
										LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
										WHERE CPGL.PartyGLType='" + PartyGLType.SuspenseGL + @"'
                                    ) AS SGL ON SGL.CompanyPartyId=CP.Id
                                    WHERE P.Archive=0 AND P.Active=1 AND P.CompanyGroupId='" + companyGroupId + "' AND CP.PartyType IN ('" + temp + "') AND CP.CompanyId='" + companyId + "' AND CP.PlantId='" + plantId + @"'
                                    ) AS TEMP WHERE " + strkey + " order by Code ";
                }
                if (customerVendor == null || customerVendor == "null")
                {
                    temp = "Vendor" + "','" + "Customer";
                    sql = @"select top 100 * from (SELECT distinct P.Id AS PartyId,P.Code AS PartyCode, P.UserName AS PartyName, P.Id, P.Code, P.UserName
                                    FROM [HKP].[Party] AS P
                                    LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId=P.Id
                                    LEFT JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=CP.PartyAccountGroupId
                                    WHERE P.Archive=0 AND P.Active=1 AND P.CompanyGroupId='" + companyGroupId + "' AND CP.PartyType IN ('" + temp + "') AND CP.CompanyId='" + companyId + "' AND CP.PlantId='" + plantId + @"'
                                    ) AS TEMP WHERE " + strkey + " order by Code ";
                }
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }
        [HttpPost, Authorize]
        public JsonResult GetCompanyPartyDataListNew_Loan(string column, string value, string partyType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var res = GetCompanyPartyListNew_Loan(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, column, value, partyType);
            var jsondata = Json(res, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        public List<Dictionary<string, object>> GetCompanyPartyListNew_Loan(string companyGroupId, string companyId, string plantId, string column, string value, string customerVendor)
        {
            try
            {
                string temp = null;
                if (customerVendor == "Vendor" || customerVendor == "Customer")
                {
                    temp = customerVendor;
                }
                if (customerVendor == null || customerVendor == "null")
                {
                    temp = "Vendor" + "','" + "Customer";
                }
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"select top 100 * from (SELECT  P.Id AS PartyId, P.Code AS PartyCode, P.UserName AS PartyName, P.Id, P.Code, P.UserName, CP.PartyType, CP.PartyAccountGroupId, PAG.Code AS PartyAccountGroupCode, PAG.UserName AS PartyAccountGroupName, CP.CurrencyId, C.Code AS CurrencyCode, C.[Name] AS CurrencyName
                                    , CP.PaymentTermId, PT.Code AS PaymentTermCode, PT.UserName AS PaymentTermName, CP.IsPaymentTermChangeable
                                    , NULL AS InvoicingPartyPlantId, NULL AS DeliveryPartyPlantId, CO.Code AS CountryCode, CO.UserName AS CountryName, S.Code AS StateCode, S.UserName AS StateName
                                    , RGL.ReconciliationGLId, RGL.ReconciliationGLCode, RGL.ReconciliationGLName
                                    , RGL.ReconciliationBudgetId, RGL.ReconciliationBudgetCode, RGL.ReconciliationBudgetName
                                    , RGL.ReconciliationActivityId, RGL.ReconciliationActivityCode, RGL.ReconciliationActivityName
                                    , DGL.DownPaymentGLId, DGL.DownPaymentGLCode, DGL.DownPaymentGLName
                                    , DGL.DownPaymentBudgetId, DGL.DownPaymentBudgetCode, DGL.DownPaymentBudgetName
                                    , DGL.DownPaymentActivityId, DGL.DownPaymentActivityCode, DGL.DownPaymentActivityName
                                    , SGL.SuspenseGLId, SGL.SuspenseGLCode, SGL.SuspenseGLName
                                    , SGL.SuspenseBudgetId, SGL.SuspenseBudgetCode, SGL.SuspenseBudgetName
                                    , SGL.SuspenseActivityId, SGL.SuspenseActivityCode, SGL.SuspenseActivityName
                                    , CP.TaxApplicable, CP.IsTaxApplicableChangeable
									, (SELECT COUNT(Id) FROM [HKP].[PartyPlant] WHERE PartyId=P.Id) AS TotalPartyPlant
                                    FROM [HKP].[Party] AS P
                                    LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId=P.Id
                                    LEFT JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=CP.PartyAccountGroupId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=CP.CurrencyId
                                    LEFT JOIN [MST].[PaymentTerm] AS PT ON PT.Id=CP.PaymentTermId
                                    LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=P.AddressMasterId
									LEFT JOIN [SCS].[Country] AS CO ON CO.Id=AM.CountryId
									LEFT JOIN [SCS].[State] AS S ON S.Id=AM.StateId
                                    LEFT JOIN(
                                    SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS ReconciliationGLId, GL.AccountCode AS ReconciliationGLCode, GL.UserName AS ReconciliationGLName
                                    , CPGL.BudgetMasterId AS ReconciliationBudgetId, B.Code AS ReconciliationBudgetCode, B.UserName AS ReconciliationBudgetName
                                    , CPGL.ActivityId AS ReconciliationActivityId, A.Code AS ReconciliationActivityCode, A.UserName AS ReconciliationActivityName
                                    FROM [HKP].[CompanyPartyGL] AS CPGL
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
                                    WHERE CPGL.PartyGLType='" + PartyGLType.ReconciliationGL + @"'
                                    ) AS RGL ON RGL.CompanyPartyId=CP.Id
                                    LEFT JOIN(
                                    SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS DownPaymentGLId, GL.AccountCode AS DownPaymentGLCode, GL.UserName AS DownPaymentGLName
                                    , CPGL.BudgetMasterId AS DownPaymentBudgetId, B.Code AS DownPaymentBudgetCode, B.UserName AS DownPaymentBudgetName
                                    , CPGL.ActivityId AS DownPaymentActivityId, A.Code AS DownPaymentActivityCode, A.UserName AS DownPaymentActivityName
                                    FROM [HKP].[CompanyPartyGL] AS CPGL
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
                                    WHERE CPGL.PartyGLType='" + PartyGLType.DownPaymentGL + @"'
                                    ) AS DGL ON DGL.CompanyPartyId=CP.Id
                                    LEFT JOIN(
										SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS SuspenseGLId, GL.AccountCode AS SuspenseGLCode, GL.UserName AS SuspenseGLName
										, CPGL.BudgetMasterId AS SuspenseBudgetId, B.Code AS SuspenseBudgetCode, B.UserName AS SuspenseBudgetName
										, CPGL.ActivityId AS SuspenseActivityId, A.Code AS SuspenseActivityCode, A.UserName AS SuspenseActivityName
										FROM [HKP].[CompanyPartyGL] AS CPGL
										LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
										LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
										LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
										LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
										WHERE CPGL.PartyGLType='" + PartyGLType.SuspenseGL + @"'
                                    ) AS SGL ON SGL.CompanyPartyId=CP.Id
                                    WHERE P.Archive=0 AND P.Active=1 AND P.CompanyGroupId='" + companyGroupId + "' AND CP.PartyType IN ('" + temp + "') AND CP.CompanyId='" + companyId + "' AND CP.PlantId='" + plantId + @"'
                                    ) AS TEMP WHERE " + strkey + " order by Code ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        [HttpPost, Authorize]
        public JsonResult GetCompanyPartyDataByGateEntryListNew(string column, string value, string partyType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var res = GetCompanyPartyByGateEntryListNew(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, column, value, partyType);
            var jsondata = Json(res, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        public List<Dictionary<string, object>> GetCompanyPartyByGateEntryListNew(string companyGroupId, string companyId, string plantId, string column, string value, string customerVendor)
        {
            try
            {
                string temp = null;
                if (customerVendor == "Vendor" || customerVendor == "Customer" || customerVendor == "Director")
                {
                    temp = customerVendor;
                }
                if (customerVendor == null || customerVendor == "null")
                {
                    temp = "Vendor" + "','" + "Customer";
                }
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"select top 100 * from (SELECT  P.Id AS PartyId, P.Code AS PartyCode, P.UserName AS PartyName, P.Id, P.Code, P.UserName, CP.PartyType, CP.PartyAccountGroupId, PAG.Code AS PartyAccountGroupCode, PAG.UserName AS PartyAccountGroupName, CP.CurrencyId, C.Code AS CurrencyCode, C.[Name] AS CurrencyName
                                    , CP.PaymentTermId, PT.Code AS PaymentTermCode, PT.UserName AS PaymentTermName, CP.IsPaymentTermChangeable
                                    , NULL AS InvoicingPartyPlantId, NULL AS DeliveryPartyPlantId, CO.Code AS CountryCode, CO.UserName AS CountryName, S.Code AS StateCode, S.UserName AS StateName
                                    , RGL.ReconciliationGLId, RGL.ReconciliationGLCode, RGL.ReconciliationGLName
                                    , RGL.ReconciliationBudgetId, RGL.ReconciliationBudgetCode, RGL.ReconciliationBudgetName
                                    , RGL.ReconciliationActivityId, RGL.ReconciliationActivityCode, RGL.ReconciliationActivityName
                                    , CP.TaxApplicable, CP.IsTaxApplicableChangeable
									, (SELECT COUNT(Id) FROM [HKP].[PartyPlant] WHERE PartyId=P.Id) AS TotalPartyPlant
                                    FROM [HKP].[Party] AS P
                                    LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId=P.Id
                                    LEFT JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=CP.PartyAccountGroupId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=CP.CurrencyId
                                    LEFT JOIN [MST].[PaymentTerm] AS PT ON PT.Id=CP.PaymentTermId
                                    LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=P.AddressMasterId
									LEFT JOIN [SCS].[Country] AS CO ON CO.Id=AM.CountryId
									LEFT JOIN [SCS].[State] AS S ON S.Id=AM.StateId
                                    LEFT JOIN(
                                    SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS ReconciliationGLId, GL.AccountCode AS ReconciliationGLCode, GL.UserName AS ReconciliationGLName
                                    , CPGL.BudgetMasterId AS ReconciliationBudgetId, B.Code AS ReconciliationBudgetCode, B.UserName AS ReconciliationBudgetName
                                    , CPGL.ActivityId AS ReconciliationActivityId, A.Code AS ReconciliationActivityCode, A.UserName AS ReconciliationActivityName
                                    FROM [HKP].[CompanyPartyGL] AS CPGL
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
                                    WHERE CPGL.PartyGLType='" + PartyGLType.ReconciliationGL + @"'
                                    ) AS RGL ON RGL.CompanyPartyId=CP.Id
                                    JOIN (SELECT DISTINCT G.PartyId FROM TRN.GateEntry G   WHERE ISNULL(G.Id,'') NOT IN (SELECT ISNULL(GateEntryNo,'') FROM TRN.InventoryReceive where ISNULL(PartyId,'')<>'' ) AND G.PartyId<>'') GE ON GE.PartyId=P.Id
                                    WHERE P.Archive=0 AND P.Active=1 AND P.CompanyGroupId='" + companyGroupId + "' AND CP.PartyType IN ('" + temp + "') AND CP.CompanyId='" + companyId + "' AND CP.PlantId='" + plantId + @"'
                                    ) AS TEMP WHERE " + strkey + " order by Code ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        #region vendor BY Contract
        [HttpPost, Authorize]
        public JsonResult GetCompanyPartyDataListByContract(string column, string value, string partyType, string ContractId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var res = GetCompanyPartyDataListByContract(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, column, value, partyType, ContractId);
            var jsondata = Json(res, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }
        public List<Dictionary<string, object>> GetCompanyPartyDataListByContract(string companyGroupId, string companyId, string plantId, string column, string value, string customerVendor, string ContractId)
        {
            try
            {
                string strkey = "1=1";
                if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                    strkey = column + " like '%" + value + "%'";
                var sql = @"select top 100 * from (SELECT distinct P.Id AS PartyId, P.Code AS PartyCode, P.UserName AS PartyName, P.Id, P.Code, P.UserName, CP.PartyType, CP.PartyAccountGroupId, PAG.Code AS PartyAccountGroupCode, PAG.UserName AS PartyAccountGroupName, CP.CurrencyId, C.Code AS CurrencyCode, C.[Name] AS CurrencyName
                                    , CP.PaymentTermId, PT.Code AS PaymentTermCode, PT.UserName AS PaymentTermName, CP.IsPaymentTermChangeable
                                    , NULL AS InvoicingPartyPlantId, NULL AS DeliveryPartyPlantId, CO.Code AS CountryCode, CO.UserName AS CountryName, S.Code AS StateCode, S.UserName AS StateName
                                    , RGL.ReconciliationGLId, RGL.ReconciliationGLCode, RGL.ReconciliationGLName
                                    , RGL.ReconciliationBudgetId, RGL.ReconciliationBudgetCode, RGL.ReconciliationBudgetName
                                    , RGL.ReconciliationActivityId, RGL.ReconciliationActivityCode, RGL.ReconciliationActivityName
                                    , DGL.DownPaymentGLId, DGL.DownPaymentGLCode, DGL.DownPaymentGLName
                                    , DGL.DownPaymentBudgetId, DGL.DownPaymentBudgetCode, DGL.DownPaymentBudgetName
                                    , DGL.DownPaymentActivityId, DGL.DownPaymentActivityCode, DGL.DownPaymentActivityName
                                    , SGL.SuspenseGLId, SGL.SuspenseGLCode, SGL.SuspenseGLName
                                    , SGL.SuspenseBudgetId, SGL.SuspenseBudgetCode, SGL.SuspenseBudgetName
                                    , SGL.SuspenseActivityId, SGL.SuspenseActivityCode, SGL.SuspenseActivityName
                                    , CP.TaxApplicable, CP.IsTaxApplicableChangeable
									, (SELECT COUNT(Id) FROM [HKP].[PartyPlant] WHERE PartyId=P.Id) AS TotalPartyPlant
                                    FROM [HKP].[Party] AS P
                                    LEFT JOIN [HKP].[CompanyParty] AS CP ON CP.PartyId=P.Id
                                    LEFT JOIN [HKP].[PartyAccountGroup] AS PAG ON PAG.Id=CP.PartyAccountGroupId
                                    LEFT JOIN [SCS].[Currency] AS C ON C.Id=CP.CurrencyId
                                    LEFT JOIN [MST].[PaymentTerm] AS PT ON PT.Id=CP.PaymentTermId
                                    LEFT JOIN [MST].[AddressMaster] AS AM ON AM.Id=P.AddressMasterId
									LEFT JOIN [SCS].[Country] AS CO ON CO.Id=AM.CountryId
									LEFT JOIN [SCS].[State] AS S ON S.Id=AM.StateId
                                    LEFT JOIN(
                                    SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS ReconciliationGLId, GL.AccountCode AS ReconciliationGLCode, GL.UserName AS ReconciliationGLName
                                    , CPGL.BudgetMasterId AS ReconciliationBudgetId, B.Code AS ReconciliationBudgetCode, B.UserName AS ReconciliationBudgetName
                                    , CPGL.ActivityId AS ReconciliationActivityId, A.Code AS ReconciliationActivityCode, A.UserName AS ReconciliationActivityName
                                    FROM [HKP].[CompanyPartyGL] AS CPGL
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
                                    WHERE CPGL.PartyGLType='" + PartyGLType.ReconciliationGL + @"'
                                    ) AS RGL ON RGL.CompanyPartyId=CP.Id
                                    LEFT JOIN(
                                    SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS DownPaymentGLId, GL.AccountCode AS DownPaymentGLCode, GL.UserName AS DownPaymentGLName
                                    , CPGL.BudgetMasterId AS DownPaymentBudgetId, B.Code AS DownPaymentBudgetCode, B.UserName AS DownPaymentBudgetName
                                    , CPGL.ActivityId AS DownPaymentActivityId, A.Code AS DownPaymentActivityCode, A.UserName AS DownPaymentActivityName
                                    FROM [HKP].[CompanyPartyGL] AS CPGL
                                    LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
                                    LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
                                    LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
                                    LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
                                    WHERE CPGL.PartyGLType='" + PartyGLType.DownPaymentGL + @"'
                                    ) AS DGL ON DGL.CompanyPartyId=CP.Id
                                    LEFT JOIN(
										SELECT CPGL.CompanyPartyId, CPGL.GLGeneralInfoId AS SuspenseGLId, GL.AccountCode AS SuspenseGLCode, GL.UserName AS SuspenseGLName
										, CPGL.BudgetMasterId AS SuspenseBudgetId, B.Code AS SuspenseBudgetCode, B.UserName AS SuspenseBudgetName
										, CPGL.ActivityId AS SuspenseActivityId, A.Code AS SuspenseActivityCode, A.UserName AS SuspenseActivityName
										FROM [HKP].[CompanyPartyGL] AS CPGL
										LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id=CPGL.GLGeneralInfoId
										LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id=CPGL.BudgetMasterId
										LEFT JOIN [HKP].[Budget] AS B ON B.Id=BM.BudgetId
										LEFT JOIN [HKP].[Activity] AS A ON A.Id=CPGL.ActivityId
										WHERE CPGL.PartyGLType='" + PartyGLType.SuspenseGL + @"'
                                    ) AS SGL ON SGL.CompanyPartyId=CP.Id
                                    Left join boq boq ON boq.VendorId=p.Id									
									LEFT OUTER JOIN trn.MasterOrderItem AS moi ON moi.Id=boq.MasterOrderItemId
									LEFT JOIN TRN.SalesOrder so on moi.Id=so.MasterOrderItemId
                                    WHERE P.Archive=0 AND P.Active=1 AND P.CompanyGroupId='" + companyGroupId + "' AND CP.PartyType IN ('" + customerVendor + "') AND CP.CompanyId='" + companyId + "' AND CP.PlantId='" + plantId + @"' and so.ContractId='" + ContractId + @"'
                                    ) AS TEMP WHERE " + strkey + " order by Code ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }



        #endregion


        [HttpGet, Authorize]
        public ActionResult GetCompanyPartyReconAdditionalGLList(string partyId, PartyType partyType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_partyService.GetCompanyPartyReconAdditionalGLList(identity.CompanyId, identity.PlantId, partyId, partyType), JsonRequestBehavior.AllowGet);
        }


        public IEnumerable<object> GetAuthorizeByEmployee(string employeeId)
        {
            try
            {
                var _sql = @"SELECT * FROM dbo.[AuthorizationConfig] WHERE ActionStatus ='PartyApproveBy' AND EmployeeId='"+ employeeId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpGet, Authorize]
        public ActionResult GetPartyListToApprove(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var entity = GetAuthorizeByEmployee(identity.EmployeeId);
            if (entity == null || !entity.Any())
            throw new CustomException("You are not authorize person to approve.");
            return Json(_partyService.GetUnApproveParty(parameters, identity.CompanyGroupId, PartyType.Party), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPartyList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_partyService.Query(parameters, identity.CompanyGroupId, PartyType.Party), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetPartyCodeList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_partyService.GetPartyCodeList(parameters, identity.CompanyGroupId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetInvoiceVendorData(GridParameter parameters, string partyid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_partyService.GetInvoiceVendorData(parameters, partyid, identity.CompanyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetInvoiceCustomerByParty(GridParameter parameters, string partyId)
        {
            return Json(_partyService.GetInvoiceCustomerByParty(parameters, partyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetParty(string id)
        {
            return Json(_partyService.Find(id), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCompanyPartyGLList(string partyId)
        {
            return Json(_partyService.GetCompanyPartyGLList(partyId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetCompanyPartyDownPaymentGL(string partyId, PartyType partyType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_partyService.GetCompanyPartyGLList(partyId, identity.CompanyId, identity.PlantId, PartyGLType.DownPaymentGL, partyType), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetAutoSequence()
        {
            return Json(_partyService.GetAutoSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Party party, AddressMaster addressMaster, IEnumerable<ContactMaster> contactmasters,
            IEnumerable<CompanyParty> companyPartyDataList, IEnumerable<PartyPartnerFunction> partnerFunctionList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            party.CompanyGroupId = identity.CompanyGroupId;
            party.PartyType = PartyType.Party.ToString();
            _partyService.Insert(party, addressMaster, contactmasters, companyPartyDataList, partnerFunctionList);
            return Json(new { Party = party, Sequence = _partyService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(Party party, AddressMaster addressMaster, IEnumerable<ContactMaster> contactmasters,
            IEnumerable<CompanyParty> companyPartyDataList, IEnumerable<CompanyPartyGL> companyPartyGLDataList,
            IEnumerable<PartyPartnerFunction> vendorPartnerFunction, bool isCustomerCurrencyChanges, bool isVendorCurrencyChanges)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            party.CompanyGroupId = identity.CompanyGroupId;
            party.PartyType = PartyType.Party.ToString();

            var listCount = companyPartyDataList.ToList().Count;
            if (isCustomerCurrencyChanges == true && listCount > 0)
            {
                foreach (var item in companyPartyDataList.Where(r => r.PartyType == "Customer"))
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster = null;
                    string sql = "select* from trn.VoucherDetail where PartyId = '" + party.Id + "' and PartyType='" + item.PartyType + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new CustomException("Customer Currency update not allowed!! Voucher have already done against this Party.");
                    }
                }
            }
            if (isVendorCurrencyChanges == true && listCount > 0)
            {
                foreach (var item in companyPartyDataList.Where(r => r.PartyType == "Vendor"))
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster = null;
                    string sql = "select* from trn.VoucherDetail where PartyId = '" + party.Id + "' and PartyType='" + item.PartyType + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new CustomException("Vendor Currency update not allowed !! Voucher have already done against this Party.");
                    }
                }
            }

            _partyService.Update(party, addressMaster, contactmasters, companyPartyDataList, companyPartyGLDataList, vendorPartnerFunction);
            return Json(new { Party = party, Sequence = _partyService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _partyService.Delete(id);
            return Json(new { Sequence = _partyService.GetAutoSequence(), Message = AplosMessage.Deleted });
        }

        [Authorize, HttpGet]
        public JsonResult GetPartyPalntSequence()
        {
            return Json(_partyService.GetPartyPalntSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult CreatePartyPlant(PartyPlant entity, AddressMaster addressMaster)
        {
            _partyService.InsertOrUpdatePartyPlant(entity, addressMaster);
            return Json(new { PartyPlant = entity, Sequence = _partyService.GetPartyPalntSequence(), Message = AplosMessage.Success });
        }

        [HttpGet, Authorize]
        public ActionResult GetPartyPlantByPartyId(string partyId)
        {
            return Json(_partyService.GetPartyPlantList(partyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult DeletePartyPlant(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _partyService.DeletePartyPlant(id);
            return Json(new { Sequence = _partyService.GetPartyPalntSequence(), Message = AplosMessage.Deleted });
        }
        [HttpPost, Authorize]
        public JsonResult DeleteInterCompanyPartyPlant(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _partyService.DeleteInterCompanyPartyPlant(id);
            return Json(new { Sequence = _partyService.GetPartyPalntSequence(), Message = AplosMessage.Deleted });
        }
        [HttpPost, Authorize]
        public JsonResult DeleteCompanyPartyGL(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _partyService.DeleteCompanyPartyGL(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public JsonResult GetPartyCbobyPartyTypeAccountGroup(string partyType, string accountGroupId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_partyService.GetPartyCbobyPartyTypeAccountGroup(identity.CompanyId, identity.PlantId, partyType, accountGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetPartyPlantCbo(string partyId)
        {
            return Json(_partyService.GetPartyPlantCbo(partyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetPartyGSTINCbo(string partyId)
        {
            return Json(_partyService.GetPartyGSTINCbo(partyId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetPartyGSTINCboByPartyPlant(string partyId, string partyPlantId)
        {
            return Json(_partyService.GetPartyGSTINCbo(partyId, partyPlantId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAllPartyPlantCbo(string invoiceId)
        {
            return Json(_partyService.GetAllPartyPlantCbo(invoiceId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAllPartyPlantJournalCbo(string voucherId)
        {
            return Json(_partyService.GetAllPartyPlantJournalCbo(voucherId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost,Authorize]
        public JsonResult CreatePartyPlantContact(Dictionary<string, object> entity)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                
                con.OpenDataSetThroughAdapter("select * from dbo.PartyPlantContact where Id='" + entity["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "PartyPlantContact", out _Id);

                    entity["Id"] =_Id;
                    AddNewRow(dsMaster.Tables[0], entity);
                }
                else
                {
                    _Id = entity["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], entity);
                }
                #endregion data update


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);


                return Json(new { Error = false, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpGet, Authorize]
        public ActionResult GetPartyPlantContactData(string PartyPlantId)
        {
            try
            {
                var sql = @"SELECT * FROM PartyPlantContact  Where PartyPlantId='"+ PartyPlantId + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion --Party Operations

        [HttpPost, Authorize]
        public JsonResult DeleteBank(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new CustomException(Resources.IdNotFound);
            _partyBankService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #region Director

        [Authorize, HttpGet]
        public ActionResult GetDirectorList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_partyService.Query(parameters, identity.CompanyGroupId, PartyType.Director), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertDirector(Party party, AddressMaster addressMaster, IEnumerable<ContactMaster> contactmasters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            party.CompanyGroupId = identity.CompanyGroupId;
            party.PartyType = PartyType.Director.ToString();
            _partyService.Insert(party, addressMaster, contactmasters);
            return Json(new { Party = party, Sequence = _partyService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult EditDirector(Party party, AddressMaster addressMaster, IEnumerable<ContactMaster> contactmasters)
        {
            _partyService.Update(party, addressMaster, contactmasters);
            party.PartyType = PartyType.Director.ToString();
            return Json(new { Party = party, Sequence = _partyService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        #endregion Director

        #region Other

        [Authorize, HttpGet]
        public ActionResult Other()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ViewBag.CompanyGroup = identity.CompanyGroupName;
            return View();
        }

        [Authorize, HttpGet]
        public ActionResult GetOtherList(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_partyService.Query(parameters, identity.CompanyGroupId, PartyType.Other), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertOther(Party party, IEnumerable<ContactMaster> contactmasters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            party.CompanyGroupId = identity.CompanyGroupId;
            party.PartyType = PartyType.Other.ToString();
            _partyService.Insert(party, null, contactmasters);
            return Json(new { Party = party, Sequence = _partyService.GetAutoSequence(), Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult EditOther(Party party, IEnumerable<ContactMaster> contactmasters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            party.CompanyGroupId = identity.CompanyGroupId;
            party.PartyType = PartyType.Other.ToString();
            _partyService.Update(party, null, contactmasters);
            return Json(new { Party = party, Sequence = _partyService.GetAutoSequence(), Message = AplosMessage.Updated });
        }

        #endregion Other

        [Authorize]
        public ActionResult GetPartyReport(string type, string companyGroupId, string companyId, string plantId)
        {
            var fileName = "Party Report " + System.DateTime.Now.ToString("ddMMMyyyy") + "";
            var workbook = _partyReportService.PartyReport(type, companyGroupId, companyId, plantId);
            workbook.SaveAs(fileName + ".xlsx", HttpContext.ApplicationInstance.Response, ExcelDownloadType.PromptDialog);
            return null;
        }

        #region PartyBank

        private bool CheckCombination(Dictionary<string, object> data)
        {
            try
            {

                var _sql = @"SELECT * FROM [HKP].[PartyBank]  where id<>'" + data["Id"] + "' and CompanyPartyId='" + data["CompanyPartyId"] + "' AND Bank='" + data["Bank"] + "' AND BankBranch='" + data["BankBranch"] + "' AND BankAccountNo='" + data["BankAccountNo"] + "'";
                var list = _sqlRepository.GetDataCollection(_sql, null);

                if (list.Count > 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpPost, Authorize]
        public JsonResult CreatePartyBank(Dictionary<string, object> data)
        {
            try
            {
                if (data != null)
                {
                    var IsDuplicateEntryAllowed = CheckCombination(data);

                    if (IsDuplicateEntryAllowed)
                    {


                        DataSet dsMaster;
                        ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                        con.OpenDataSetThroughAdapter("SELECT * FROM [HKP].[PartyBank] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                        string _Id = "";

                        #region data update
                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "PartyBank", out _Id);

                            data["Id"] = "PB" + _Id;
                            AddNewRow(dsMaster.Tables[0], data);
                        }
                        else
                        {
                            _Id = data["Id"].ToString();
                            EditRow(dsMaster.Tables[0].Rows[0], data);
                        }
                        #endregion data update

                        clsStaticInfo _info = new clsStaticInfo();
                        _info.SaveDataSets(dsMaster);
                    }
                    else
                    {
                        throw new Exception("Selected combination already exists...");
                    }
                }
                return Json(new { Error = false, Data = data, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        [HttpPost, Authorize]
        public JsonResult EditPartyBank(Dictionary<string, object> data)
        {
            try
            {
                if (data != null)
                {
                    var IsDuplicateEntryAllowed = CheckCombination(data);

                    if (IsDuplicateEntryAllowed)
                    {

                        DataSet dsMaster;
                        ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                        con.OpenDataSetThroughAdapter("SELECT * FROM [HKP].[PartyBank] WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                        string _Id = "";

                        #region data update
                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "PartyBank", out _Id);

                            data["Id"] = "PB" + _Id;
                            AddNewRow(dsMaster.Tables[0], data);
                        }
                        else
                        {
                            _Id = data["Id"].ToString();
                            EditRow(dsMaster.Tables[0].Rows[0], data);
                        }
                        #endregion data update

                        clsStaticInfo _info = new clsStaticInfo();
                        _info.SaveDataSets(dsMaster);
                    }
                    else
                    {
                        throw new Exception("Selected combination already exists...");
                    }
                }
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }

        [HttpPost]
        public JsonResult PartyApprovalUpdate(Dictionary<string, object> data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM HKP.Party WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                DataView dv = new DataView(dsMaster.Tables[0]);
                dv.RowFilter = "Id='" + data["Id"] + "'";
                if (dv.Count > 0) 
                {
                    DataRow drmo = dv[0].Row;
                    data["Active"] = true;
                    data["IsApproved"] = data["IsApproved"].ToString();
                    data["ApprovedBy"] = identity.EmployeeId;
                    data["ApprovedDate"] =DateTime.Now;
                    EditRow(drmo, data);
                }
                 
                #endregion data update
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });

            }
        }

    }
}