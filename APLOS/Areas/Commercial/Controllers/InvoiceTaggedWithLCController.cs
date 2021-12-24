#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Accounting.Accounts;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.MaterialManagement.Material;
using Library.Model.Commercial;
using Library.Model.Enums;
using Library.Model.Parties;
using Library.Security.Core;
using Library.Service.Finances;
using Library.ViewModel.Accounts;
using Library.ViewModel.Vouchers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Commercial.Controllers
{
    public class InvoiceTaggedWithLCController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IAutoLoanService _autoLoanService;
        clsInvoiceTagWithLc ep = new clsInvoiceTagWithLc();
        public InvoiceTaggedWithLCController( ISqlRepository R
           , IAutoLoanService autoLoanService
            )
        {
            _sqlRepository = R;
            _autoLoanService = autoLoanService;
        }
        #endregion

        #region -- Pages
       
        public ActionResult Aplos()
        {
            return View();
		}

		#endregion

		#region Operation

		[HttpGet, Authorize]
		public ActionResult GetVendorAvailableInvoiceList(string FromDate,string ToDate,bool DateRange)
		{
			try
			{
				var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var jsondata = Json(ep.VendorAvailableInvoiceList(identity.CompanyGroupId,identity.CompanyId, FromDate,ToDate,DateRange), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
			catch (Exception ex)
			{
				return Json(new { Error = true, Message = ex.Message });
			}
        }

        [HttpGet, Authorize]
        public ActionResult purchaseLCList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = @"
                        SELECT 
                         PLCV.[Version] PreVersion, PLCV.Amount AmendmentAmount, FORMAT(PLC.AmendmentDate,'dd-MMM-yyyy') AmendmentDate, 
						 PLC.Id,PLC.Version, PLC.ContractId, PLC.VendorId, PLC.BenificiaryBank, PLC.OpeningBankMasterId, PLC.BenificiaryBankDescription, 
                         PLC.LeinBank, PLC.LeinBankDescription, PLC.OrderSpecific, PLC.LCRef, FORMAT(PLC.LCDate,'dd-MMM-yyyy') LCDate,
                         FORMAT(PLC.ExpiryDate,'dd-MMM-yyyy') ExpiryDate, PLC.Amount, PLC.[Type], PLC.Tenure, PLC.CurrencyId, PLC.Rate, PLC.FinalDestination, 
                         PLC.PortOfLandingId, PLC.[Status], PLC.AddedBy, FORMAT(PLC.AddedDate,'dd-MMM-yyyy') AddedDate, PLC.AddedFromIP, PLC.UpdatedBy, FORMAT(PLC.UpdatedDate,'dd-MMM-yyyy') UpdatedDate, PLC.UpdatedFromIP
						,P.UserName PartyName, OB.AccountTitle OpeningBank,CN.Code Currency,PLC.LCANo,PLC.LIBOUR,PLC.InsuranceCoverNoteNo,PLC.InsuranceAttachment,PLC.PaymentBasedOn,C.ContractNo , PLC.InsuranceValue,PLC.IsAccepptanceFirst,PLC.PortOfLoading,PT.UserName CustomerName
						,FORMAT(PLC.ShipmentDate,'dd-MMM-yyyy') ShipmentDate,PLC.PINo,OB.CurrencyId BankCurrency,MLC.LCRef MasterLCNo,C.Remarks ContractRemarks ,PLC.Remarks 
						 FROM [dbo].[PurchaseLC] PLC
                        LEFT JOIN dbo.[Contract] C ON C.Id=PLC.ContractId
                        LEFT JOIN dbo.MasterLC MLC ON MLC.Id=C.MasterLCId
						LEFT JOIN HKP.Party PT ON PT.Id=C.CustomerId
                        LEFT JOIN HKP.Party P  ON P.Id=PLC.VendorId
                        LEFT JOIN MST.BankMaster OB  ON OB.Id=PLC.OpeningBankMasterId
						LEFT JOIN SCS.Currency CN ON CN.Id=PLC.CurrencyId
						LEFT JOIN [dbo].[PurchaseLCVersion] PLCV ON PLCV.PurchaseLCId=PLC.Id  
						AND PLCV.Id=(SELECT TOP 1 Id FROM [dbo].[PurchaseLCVersion] WHERE PurchaseLCId=PLC.Id  ORDER BY [Version] ASC) Where PLC.PlantId='" + identity.PlantId + "'   ORDER BY PLC.AddedDate DESC";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public JsonResult Create(List<Dictionary<string,object>> DataList, Dictionary<string, object> LcData)
        {
            try
            {
                #region Validation
                if (DataList.Count == 0)
                {
                    throw new Exception("Select from Invoice list ");
                }
                for (int i = 0; i < DataList.Count; i++)
                {
                    if (DataList[i]["PartyId"].ToString() != LcData["VendorId"].ToString())
                    {
                        throw new Exception("Vendor should be matched with Purchase LC for [" + DataList[i]["PartyPlantName"].ToString() + "]");
                    }
                    if (DataList[i]["CurrencyId"].ToString() != LcData["CurrencyId"].ToString())
                    {
                        throw new Exception("Currency should be matched with Purchase LC for [" + DataList[i]["PartyPlantName"].ToString() + "]");
                    }
                }
                #endregion
                ep.Save(DataList, LcData);
                return Json(new { Error = false, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }

        #endregion

    }
}