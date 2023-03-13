#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Commercial;
using Library.Model.Enums;
using Library.Model.Taxations;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Invoices;
using Library.ViewModel.Invoices;
using Library.ViewModel.OrderManagements;
using Library.ViewModel.Vouchers;
using Newtonsoft.Json;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading;
using System.Web;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Commercial.Controllers
{
    public class PurchaseLCController : BaseController
    {
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IInvoiceWriteOffService _invoiceWriteOffService;
        private readonly IInvoiceReportService _invoiceReportService;

        public PurchaseLCController(IUnitOfWork U
            , ISqlRepository R
            , IInvoiceWriteOffService invoiceWriteOffService
            , IInvoiceReportService invoiceReportService
            )
        {
            _unitOfWork = U;
            _sqlRepository = R;
            _invoiceWriteOffService = invoiceWriteOffService;
            _invoiceReportService = invoiceReportService;
        }
        #endregion

        #region -- Pages
       
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        #region PurchaseLC
        [HttpGet,Authorize]
        public ActionResult GetList()
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

        [HttpGet, Authorize]
        public ActionResult GetPurchaseLCChargesData(string purchaseLCId)
        {

            string sql = @"
                          SELECT PLC.*,C.Id BankCurrencyId,OB.AccountTitle OpeningBankMaster,BD.UserName Budget,(GL.AccountCode + ' - ' + GL.UserName) GL,A.UserName Activity,LCT.UserName OverHeadType,V.VoucherNo,LCT.Type
                          FROM [dbo].[PurchaseLCCharges] PLC
                          INNER JOIN [HKP].[OverHeadTypeGL] LCGL ON LCGL.Id=PLC.OverHeadTypeGLId
                          LEFT JOIN [HKP].OverHeadType LCT ON LCT.Id=LCGL.OverHeadTypeId
                          LEFT JOIN MST.BankMaster OB  ON OB.Id=PLC.OpeningBankMasterId
                          LEFT JOIN SCS.Currency C ON C.Id=OB.CurrencyId
                          LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id = LCGL.ExpensesBudgetMasterId
                          LEFT JOIN [HKP].[Activity] AS A ON A.Id = LCGL.ExpensesActivityId
                          LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id = LCGL.ExpensesGLId
                          LEFT JOIN [HKP].Budget BD ON BD.Id=BM.BudgetId
                          LEFT JOIN TRN.Voucher V ON V.Id=PLC.VoucherId
                          WHERE PLC.PurchaseLCId='" + purchaseLCId + "' AND  LCT.Type='" + ChargesType.Open + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPurchaseLCBackData(string purchaseLCId, string Version)
        {

            string sql = @"SELECT V.*, 1 Back FROM [dbo].[PurchaseLCVersion] V Where PurchaseLCId='" + purchaseLCId + "' AND Version=" + Version + "";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPurchaseLCChargesVersionData(string purchaseLCId, string Version)
        {
            string sql = @"
                          SELECT PLC.*,C.Id BankCurrencyId,OB.AccountTitle OpeningBankMaster,BD.UserName Budget,(GL.AccountCode + ' - ' + GL.UserName) GL,A.UserName Activity,LCT.UserName OverHeadType,V.VoucherNo,LCT.Type
                        , isTax=(SELECT ISNULL(COUNT(DISTINCT PurchaseLCChargesId),0) FROM [TRN].[PurchaseLCTax] WHERE PurchaseLCChargesId=PLC.Id)
                          FROM [dbo].[PurchaseLCCharges] PLC
                          INNER JOIN [HKP].[OverHeadTypeGL] LCGL ON LCGL.Id=PLC.OverHeadTypeGLId
                          LEFT JOIN [HKP].OverHeadType LCT ON LCT.Id=LCGL.OverHeadTypeId
                          LEFT JOIN MST.BankMaster OB  ON OB.Id=PLC.OpeningBankMasterId
                          LEFT JOIN SCS.Currency C ON C.Id=OB.CurrencyId
                          LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id = LCGL.ExpensesBudgetMasterId
                          LEFT JOIN [HKP].[Activity] AS A ON A.Id = LCGL.ExpensesActivityId
                          LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id = LCGL.ExpensesGLId
                          LEFT JOIN [HKP].Budget BD ON BD.Id=BM.BudgetId
                          LEFT JOIN TRN.Voucher V ON V.Id=PLC.VoucherId
                          WHERE PLC.PurchaseLCId='" + purchaseLCId + "'  AND PLC.Version='" + Version + "' AND  LCT.Type='" + ChargesType.Open + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(FormCollection form, HttpPostedFileBase[] file)
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                var model = JsonConvert.DeserializeObject<PurchaseLC>(form["model"], settings);
                IEnumerable<PurchaseLCCharges> Charges = JsonConvert.DeserializeObject<IEnumerable<PurchaseLCCharges>>(form["Charges"], settings);


                var directory = ResourcesPathReader.GetLCDocPath();
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                string path = Path.Combine(directory);
               
                var fileName = "";
                var filedata = GetFile(model.Id);
                if (file.IsNotNull())
                {
                    for (int i = 0; i < file.Length; i++)
                    {
                        ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
                    }
                }
                if (filedata.Count > 0)
                {
                    if (
                        !string.IsNullOrEmpty(filedata["InsuranceAttachment"].ToString()))
                        fileName = filedata["InsuranceAttachment"].ToString();

                    if (fileName != model.InsuranceAttachment)
                        if (System.IO.File.Exists(path + model.Id + Path.GetExtension(fileName)))
                            System.IO.File.Delete(path + model.Id + Path.GetExtension(fileName));
                }


                SaveData(model, out string version, out string masterId);
                SaveChargeData(Charges, masterId, version);


                if (file.IsNotNull())
                {
                    foreach (var item in file)
                    {
                        if (item != null)
                        {
                            if (System.IO.File.Exists(path + item.FileName))
                                System.IO.File.Delete(path + masterId + Path.GetExtension(item.FileName));
                            item.SaveAs(path + masterId + Path.GetExtension(item.FileName));
                        }
                    }
                }

                return Json(new { Version = version, Id = masterId, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }

        public Dictionary<string, object> GetFile(string Id)
        {
            try
            {
                var sql = @"SELECT InsuranceAttachment FROM [dbo].[PurchaseLC]  WHERE Id='" + Id + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetChargesPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(PurchaseLCCharges), out sID);
            return sID;
        }

        private void SaveChargeData(IEnumerable<PurchaseLCCharges> data, string masterId, string version)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [dbo].[PurchaseLCCharges] WHERE Id='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();
                            dr["Id"] = GetChargesPK();
                            dr["PurchaseLCId"] = item.PurchaseLCId ?? masterId;
                            dr["OverHeadTypeGLId"] = item.OverHeadTypeGLId;
                            dr["OpeningBankMasterId"] = item.OpeningBankMasterId;
                            dr["ChargesValue"] = item.ChargesValue;
                            dr["Remarks"] = item.Remarks;
                            dr["CurrencyId"] = item.CurrencyId;
                            dr["Rate"] = item.Rate;
                            dr["BankAmount"] = item.BankAmount;
                            dr["Version"] = version;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["PurchaseLCId"] = item.PurchaseLCId ?? masterId;
                            dr["OverHeadTypeGLId"] = item.OverHeadTypeGLId;
                            dr["OpeningBankMasterId"] = item.OpeningBankMasterId;
                            dr["ChargesValue"] = item.ChargesValue;
                            dr["Remarks"] = item.Remarks;
                            dr["CurrencyId"] = item.CurrencyId;
                            dr["Rate"] = item.Rate;
                            dr["BankAmount"] = item.BankAmount;
                            dr["Version"] = version;

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetBankCbo()
        {
            var sql = @"select Id, UserName from HKP.Bank  where Active=1 Order By UserName";
            return Json(_sqlRepository.GetCombo(sql, "Id", "UserName"), JsonRequestBehavior.AllowGet);
        }

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(PurchaseLC), out sID);
            return sID;
        }

        private bool CheckUniqueLCRef(PurchaseLC data)
        {
            try
            {
                var _sql = @"SELECT LCRef FROM [dbo].[PurchaseLC] where Id<>'" + data.Id + "' AND LCRef='" + data.LCRef + "'";
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
        private bool CheckUniquePINo(PurchaseLC data)
        {
            try
            {
                var _sql = @"SELECT PINo FROM [dbo].[PurchaseLC] where Id<>'" + data.Id + "' AND PINo='" + data.PINo + "'";
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
        private void SaveData(PurchaseLC data, out string version, out string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            string contId = string.Empty;
            string id = string.Empty;
            DataSet dsSeq = null;
            try
            {
                var IsUniquePINo = CheckUniquePINo(data);
                if (!IsUniquePINo)
                {
                    throw new Exception("PI No is Unique.");
                }

                var IsUniqueLCRef = CheckUniqueLCRef(data);
                if (!IsUniqueLCRef)
                {
                    throw new Exception("LC Ref No is Unique.");
                }

                

                GetAutoSequence(data.Id, out dsSeq);
                decimal seq = Convert.ToDecimal(dsSeq.Tables[0].Rows[0]["Version"].ToString());

                string sql = "SELECT * FROM [dbo].[PurchaseLC] WHERE Id='" + data.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = "PLC" + GetPK();
                    dr["Version"] = seq;
                    dr["PlantId"] = data.PlantId ?? identity.PlantId;
                    dr["ContractId"] = data.ContractId;
                    dr["VendorId"] = data.VendorId;
                    dr["BenificiaryBank"] = data.BenificiaryBank;
                    dr["OpeningBankMasterId"] = data.OpeningBankMasterId;
                    dr["BenificiaryBankDescription"] = data.BenificiaryBankDescription;
                    dr["LeinBank"] = data.LeinBank;
                    dr["LeinBankDescription"] = data.LeinBankDescription;
                    dr["OrderSpecific"] = data.OrderSpecific;
                    dr["LCRef"] = data.LCRef;
                    dr["LCDate"] = data.LCDate;
                    dr["AmendmentDate"] = data.AmendmentDate == null ? System.DBNull.Value : (object)data.AmendmentDate;
                    dr["ExpiryDate"] = data.ExpiryDate;
                    dr["Amount"] = data.Amount;
                    dr["Type"] = data.Type;
                    dr["Tenure"] = data.Tenure;
                    dr["FinalDestination"] = data.FinalDestination;
                    dr["PortOfLandingId"] = data.PortOfLandingId;
                    dr["CurrencyId"] = data.CurrencyId;
                    dr["Rate"] = data.Rate;
                    dr["Status"] = data.Status;
                    dr["Remarks"] = data.Remarks;
                    dr["ShipmentDate"] = data.ShipmentDate == null ? System.DBNull.Value : (object)data.ShipmentDate;
                    dr["PINo"] = data.PINo;
                    dr["LCANo"] = data.LCANo;
                    dr["LIBOUR"] = data.LIBOUR;
                    dr["InsuranceCoverNoteNo"] = data.InsuranceCoverNoteNo;
                    dr["InsuranceAttachment"] = data.InsuranceAttachment;
                    dr["InsuranceValue"] = data.InsuranceValue;
                    dr["PaymentBasedOn"] = data.PaymentBasedOn;
                    dr["IsAccepptanceFirst"] = data.IsAccepptanceFirst;
                    dr["PortOfLoading"] = data.PortOfLoading;


                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsMaster.Tables[0].Rows.Add(dr);

                    contId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();
                    dr["Version"] = data.Version != 0 ? data.Version : seq;
                    dr["PlantId"] = data.PlantId ?? identity.PlantId;
                    dr["ContractId"] = data.ContractId;
                    dr["VendorId"] = data.VendorId;
                    dr["BenificiaryBank"] = data.BenificiaryBank;
                    dr["OpeningBankMasterId"] = data.OpeningBankMasterId;
                    dr["BenificiaryBankDescription"] = data.BenificiaryBankDescription;
                    dr["LeinBank"] = data.LeinBank;
                    dr["LeinBankDescription"] = data.LeinBankDescription;
                    dr["OrderSpecific"] = data.OrderSpecific;
                    dr["LCRef"] = data.LCRef;
                    dr["LCDate"] = data.LCDate;
                    dr["AmendmentDate"] = data.AmendmentDate == null ? System.DBNull.Value : (object)data.AmendmentDate;
                    dr["ExpiryDate"] = data.ExpiryDate;
                    dr["Amount"] = data.Amount;
                    dr["Type"] = data.Type;
                    dr["Tenure"] = data.Tenure;
                    dr["FinalDestination"] = data.FinalDestination;
                    dr["PortOfLandingId"] = data.PortOfLandingId;
                    dr["CurrencyId"] = data.CurrencyId;
                    dr["Rate"] = data.Rate;
                    dr["Status"] = data.Status;
                    dr["Remarks"] = data.Remarks;
                    dr["ShipmentDate"] = data.ShipmentDate == null ? System.DBNull.Value : (object)data.ShipmentDate;
                    dr["PINo"] = data.PINo;
                    dr["LCANo"] = data.LCANo;
                    dr["LIBOUR"] = data.LIBOUR;
                    dr["InsuranceCoverNoteNo"] = data.InsuranceCoverNoteNo;
                    dr["InsuranceAttachment"] = data.InsuranceAttachment;
                    dr["InsuranceValue"] = data.InsuranceValue;
                    dr["PaymentBasedOn"] = data.PaymentBasedOn;
                    dr["IsAccepptanceFirst"] = data.IsAccepptanceFirst;
                    dr["PortOfLoading"] = data.PortOfLoading;
                    

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);

                masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                version = dsMaster.Tables[0].Rows[0]["Version"].ToString();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetOpenLCChargesGLData()
        {
            try
            {
                var sql = @"SELECT 0 Active, BD.UserName Budget,(GL.AccountCode + ' - ' + GL.UserName) GL,A.UserName Activity,LCT.UserName OverHeadType,LCT.Type,LCCTGL.* 
                            FROM [HKP].[OverHeadTypeGL] LCCTGL
                            LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id = LCCTGL.ExpensesBudgetMasterId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id = LCCTGL.ExpensesActivityId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id = LCCTGL.ExpensesGLId
                            LEFT JOIN [HKP].Budget BD ON BD.Id=BM.BudgetId 
                            LEFT JOIN [HKP].OverHeadType LCT ON LCT.Id=LCCTGL.OverHeadTypeId
                            WHERE LCCTGL.GLType='Purchase' AND LCT.Type='" + ChargesType.Open + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetOpenLCChargesGLDataForAcceptance()
        {
            try
            {
                var sql = @"SELECT 0 Active, BD.UserName Budget,(GL.AccountCode + ' - ' + GL.UserName) GL,A.UserName Activity,LCT.UserName AcceptancehargesType,LCT.Type,LCCTGL.* 
                            FROM [HKP].[OverHeadTypeGL] LCCTGL
                            LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id = LCCTGL.ExpensesBudgetMasterId
                            LEFT JOIN [HKP].[Activity] AS A ON A.Id = LCCTGL.ExpensesActivityId
                            LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id = LCCTGL.ExpensesGLId
                            LEFT JOIN [HKP].Budget BD ON BD.Id=BM.BudgetId 
                            LEFT JOIN [HKP].OverHeadType LCT ON LCT.Id=LCCTGL.OverHeadTypeId
                            WHERE LCCTGL.GLType='Purchase' AND LCT.Type='" + ChargesType.Acceptance.ToString() + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            DeleteData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteData(string Id)
        {
            string strSQL, strCSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strCSQL = "DELETE FROM [dbo].[PurchaseLCCharges] WHERE PurchaseLCId='" + Id + "'";
                strSQL = "DELETE FROM dbo.PurchaseLC WHERE Id = '" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strCSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function

        [HttpPost]
        public JsonResult DeleteCharges(string id)
        {
            DeleteChargesData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteChargesData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[PurchaseLCCharges] WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function

        public void GetAutoSequence(string Id, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager Obj;

            try
            {
                string sql = @"SELECT (ISNULL((MAX(ISNULL(Version,0))),0)+1) Version FROM [dbo].[PurchaseLC] Where Id='" + Id + "'";
                Obj = new ConnectionManager.DAL.ConManager("1");
                Obj.OpenDataSetThroughAdapter(sql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetVendorCountry(string vendorId)
        {
            try
            {
                var Sql = @"Select AM.CountryId PartyCountryId from HKP.Party PR
                            join MST.AddressMaster AM ON AM.Id=PR.AddressMasterId
                            Where PR.Id='" + vendorId + "'";
                return Json(_sqlRepository.GetDataCollection(Sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetPlantCountry()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var Sql = @"Select AM.CountryId PlantCountryId from ORG.Plant P 
                            join MST.AddressMaster AM ON AM.Id=P.AddressMasterId
                            Where P.Id='" + identity.PlantId + "'";
                return Json(_sqlRepository.GetDataCollection(Sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetPurchaseLCUsedInAcceptance(string purchaseLCId)
        {
            try
            {
                var Sql = @"Select * from [TRN].[PurchaseDocAcceptance] Where PurchaseLCId='"+ purchaseLCId + "'";
                return Json(_sqlRepository.GetDataCollection(Sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region PurchaseLCChargesPost

        public ActionResult PurchaseLCChargesPost()
        {
            return View();
        }

        [HttpGet, Authorize]
        public ActionResult GetPurchaseLCUnPostList()
        {
            string sql = @"
                        SELECT A.* FROM 
					    (SELECT Type=CASE WHEN M.Version=1 THEN 'Open' ELSE 'Amendment' END,0 Active, M.Id,M.Version,FORMAT(M.LCDate,'dd-MMM-yyyy') LCDate,M.Amount
						,FORMAT(M.AmendmentDate,'dd-MMM-yyyy') AmendmentDate,0 AmendmentAmount,M.Rate CompanyCurrencyRate,M.Rate
						,FORMAT(M.ExpiryDate,'dd-MMM-yyyy') ExpiryDate,CH.CurrencyId, CN.Code Currency,P.UserName PartyName,OB.AccountTitle OpeningBank,M.LCRef,CH.PurchaseLCId,CH.VoucherId,M.OpeningBankMasterId
                        FROM [dbo].[PurchaseLC] M 
                        LEFT JOIN dbo.[Contract] C ON C.Id=M.ContractId
                        LEFT JOIN HKP.Party P  ON P.Id=M.VendorId
                        LEFT JOIN MST.BankMaster OB  ON OB.Id=M.OpeningBankMasterId
                        LEFT JOIN SCS.Currency CN ON CN.Id=M.CurrencyId
						LEFT JOIN  dbo.PurchaseLCCharges CH ON CH.PurchaseLCId=M.Id AND CH.Version=M.Version WHERE ISNULL(CH.VoucherId,'')=''
						UNION
						SELECT Type=CASE WHEN M.Version=1 THEN 'Open' ELSE 'Amendment' END,0 Active, M.PurchaseLCId Id,M.Version,FORMAT(M.LCDate,'dd-MMM-yyyy') LCDate,0 Amount
						,FORMAT(M.AmendmentDate,'dd-MMM-yyyy') AmendmentDate, M.Amount AmendmentAmount ,M.Rate CompanyCurrencyRate,M.Rate
						,FORMAT(M.ExpiryDate,'dd-MMM-yyyy') ExpiryDate,CH.CurrencyId, CN.Code Currency,P.UserName PartyName,OB.AccountTitle OpeningBank,M.LCRef,CH.PurchaseLCId,CH.VoucherId,M.OpeningBankMasterId
                        FROM [dbo].[PurchaseLCVersion] M 
                        LEFT JOIN dbo.[Contract] C ON C.Id=M.ContractId
                        LEFT JOIN HKP.Party P  ON P.Id=M.VendorId
                        LEFT JOIN MST.BankMaster OB  ON OB.Id=M.OpeningBankMasterId
                        LEFT JOIN SCS.Currency CN ON CN.Id=M.CurrencyId
						LEFT JOIN  dbo.PurchaseLCCharges CH ON CH.PurchaseLCId=M.PurchaseLCId AND CH.Version=M.Version  WHERE ISNULL(CH.VoucherId,'')=''
					) A ORDER BY A.Id ";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPurchaseLCChargesList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT PLC.Id,PLC.PurchaseLCId,PLC.OpeningBankMasterId,PLC.VoucherId,PLC.ChargesValue,PLC.CurrencyId
						  ,PLC.Version,PLC.BankAmount,
						  Rate=CASE WHEN PLC.CurrencyId=COM.BaseCurrencyId THEN 1 ELSE NULL END
						 ,Flag=CASE WHEN PLC.CurrencyId=COM.BaseCurrencyId THEN 1 ELSE 0 END
					     ,OB.AccountTitle OpeningBankMaster,BD.UserName Budget,(GL.AccountCode + ' - ' + GL.UserName) GL,A.UserName Activity
                          ,LCT.[Type],LCT.UserName OverHeadType,V.VoucherNo,C.Code CurrencyCode,LCGL.ExpensesGLId ,LCGL.ExpensesBudgetMasterId ,LCGL.ExpensesActivityId 
                         , BGL.UserName BankGLGeneralInfoId,BBD.UserName BankBudgetMasterId,BA.UserName BankActivityId,OB.GLGeneralInfoId,OB.BudgetMasterId,OB.ActivityId
						  FROM [dbo].[PurchaseLCCharges] PLC
                          INNER JOIN [HKP].[OverHeadTypeGL] LCGL ON LCGL.Id=PLC.OverHeadTypeGLId
                          LEFT JOIN [HKP].OverHeadType LCT ON LCT.Id=LCGL.OverHeadTypeId
                          LEFT JOIN MST.BankMaster OB  ON OB.Id=PLC.OpeningBankMasterId
                          LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id = LCGL.ExpensesGLId
                          LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id = LCGL.ExpensesBudgetMasterId
                          LEFT JOIN [HKP].Budget BD ON BD.Id=BM.BudgetId
                          LEFT JOIN [HKP].[Activity] AS A ON A.Id = LCGL.ExpensesActivityId
                          LEFT JOIN TRN.Voucher V ON V.Id=PLC.VoucherId
						  LEFT JOIN SCS.Currency C ON C.Id=PLC.CurrencyId 
						  LEFT JOIN [HKP].[GLGeneralInfo] AS BGL ON GL.Id = OB.GLGeneralInfoId
                          LEFT JOIN [MST].[BudgetMaster] AS BBM ON BM.Id = OB.BudgetMasterId
                          LEFT JOIN [HKP].Budget BBD ON BD.Id=BBM.BudgetId
                          LEFT JOIN [HKP].[Activity] AS BA ON A.Id = OB.ActivityId
						  LEFT JOIN ORG.Company COM ON COM.BaseCurrencyId=PLC.CurrencyId AND COM.Id='" + identity.CompanyId + @"' WHERE ISNULL(PLC.VoucherId,'')=''";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPurchaseLCChargesPostedData()
        {

            string sql = @"
                           SELECT PLC.PurchaseLCId,LC.LCRef, V.VoucherNo,PLC.VoucherId,V.DocRefNo,V.SourceType,C.Code CurrencyCode,V.DocRefNo, OB.AccountTitle OpeningBankMaster, P.UserName VendorName
						  ,SUM(PLC.ChargesValue) Amount,IsPark=case when V.IsPark=1 then 'Parked' else 'Posted' end
						  [Type]=CASE WHEN PL.[Version]=1 THEN 'Open' ELSE 'Amendment' END
						  FROM [dbo].[PurchaseLCCharges] PLC
						  join [dbo].[PurchaseLC] LC ON LC.Id=PLC.PurchaseLCId
                          INNER JOIN [HKP].[OverHeadTypeGL] LCGL ON LCGL.Id=PLC.OverHeadTypeGLId
                          LEFT JOIN [HKP].OverHeadType LCT ON LCT.Id=LCGL.OverHeadTypeId
						  LEFT JOIN [dbo].PurchaseLC PL ON PL.Id=PLC.PurchaseLCId
                          LEFT JOIN MST.BankMaster OB  ON OB.Id=PLC.OpeningBankMasterId
                          LEFT JOIN [HKP].[GLGeneralInfo] AS GL ON GL.Id = LCGL.ExpensesGLId
                          LEFT JOIN [MST].[BudgetMaster] AS BM ON BM.Id = LCGL.ExpensesBudgetMasterId
                          LEFT JOIN [HKP].Budget BD ON BD.Id=BM.BudgetId
                          LEFT JOIN [HKP].[Activity] AS A ON A.Id = LCGL.ExpensesActivityId
                          LEFT JOIN TRN.Voucher V ON V.Id=PLC.VoucherId
						  LEFT JOIN SCS.Currency C ON C.Id=PL.CurrencyId 
						  LEFT JOIN HKP.Party P ON P.Id=PL.VendorId
						  where V.Archive=0 AND PLC.VoucherId <>''AND V.SourceType='" + SourceType.PurchaseLCOpeningCharges.ToString() + @"'
						  group by V.VoucherNo,LC.LCRef,OB.AccountTitle ,PLC.PurchaseLCId, PL.[Version], V.VoucherNo,V.SourceType,c.Code,P.UserName
						  ,PLC.VoucherId,V.DocRefNo,V.IsPark";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult InsertPurchaseLCChargesPost(string voucherTypeId, IEnumerable<PurchaseLCCharges> voucherRows, IEnumerable<PurchaseLCChargesViewModel> purchaseLCChargesList, IEnumerable<InvoiceTaxViewModel> taxDetailVMList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (voucherTypeId == null)
                throw new CustomException("LC Charges Voucher Type not found.");
            if (voucherRows != null)
            {
                var voucherVM = new VoucherViewModel();
                voucherVM.CompanyGroupId = identity.CompanyGroupId;
                voucherVM.CompanyId = identity.CompanyId;
                voucherVM.PlantId = identity.PlantId;
                voucherVM.VoucherDate = DateTime.Now;
                voucherVM.SourceType = SourceType.PurchaseLCOpeningCharges.ToString();
                voucherVM.VoucherTypeId = voucherTypeId;
                _invoiceWriteOffService.PurchaseLCChargesPost(voucherVM, voucherRows, purchaseLCChargesList);
            }
            return Json(new { Message = AplosMessage.Posted });
        }

        [HttpGet, Authorize]
        public ActionResult PurchaseLCChargesReport(ReportFormat reportFormat, string voucherId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            //var reportFileName = "Purchase LC Charges" + voucherId + "";
            var workbook = _invoiceReportService.GetPurchaseLCChargesReport(out string reportFileName, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.PlantName, voucherId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }

        [HttpPost]
        public ActionResult DeletePostedPurchaseLCCharges(string purchaseLCId, string voucherId)
        {
            try
            {
                DeletePostedPurchaseLCCharges(purchaseLCId, voucherId);
                return Json(new { Message = AplosMessage.Deleted });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //private void DeleteLCChargesPosting(string purchaseLCId, s, List<Dictionary<string, object>> taxList, List<Dictionary<string, object>> itemScanCildList)
        //{
        //    ConnectionManager.DAL.ConManager objCon;
        //    DataSet dsMaster;
        //    DataSet dsDetail;
        //    DataSet dstax;
        //    DataSet dsitemscanChild;
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

        //    try
        //    {
        //        MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
        //        string sqlmaster = "SELECT * FROM [TRN].[SalesReturn] WHERE Id='" + data["SalesId"].ToString() + "'";
        //        string sqlDetail = "SELECT * FROM [TRN].[SalesReturnDetail] WHERE SalesId='" + data["SalesId"].ToString() + "'";
        //        string taxsql = "SELECT * FROM [TRN].[SalesReturnTax] WHERE SalesId='" + data["SalesId"].ToString() + "'";
        //        string itemScanChildsql = "SELECT * FROM dbo.ItemScanChild WHERE SalesId='" + data["SalesId"].ToString() + "'";
        //        //string poUpdateLogsql = "SELECT Top(1) * FROM [TRN].[PurchaseOrderUpdateLog] WHERE 1=2";
        //        objCon = new ConnectionManager.DAL.ConManager("1");
        //        objCon.OpenDataSetThroughAdapter(sqlmaster, out dsMaster, false, "1");
        //        objCon.OpenDataSetThroughAdapter(sqlDetail, out dsDetail, false, "1");
        //        objCon.OpenDataSetThroughAdapter(taxsql, out dstax, false, "1");
        //        objCon.OpenDataSetThroughAdapter(itemScanChildsql, out dsitemscanChild, false, "1");

        //        if (dsMaster.Tables[0].Rows.Count == 0)
        //        {
        //            DataRow dr = dsMaster.Tables[0].NewRow();

        //            dr["SalesId"] = data["SalesId"].ToString();
        //            dr["DocRefNo"] = data["DocRefNo"].ToString();
        //            dr["SalesReturnDate"] = data["SalesReturnDate"].ToString();
        //            dr["Narration"] = data["Narration"].ToString();
        //            dr["EntryDate"] = DateTime.Now;

        //            dr["AddedBy"] = identity.Name;
        //            dr["AddedDate"] = DateTime.Now;
        //            dr["AddedFromIP"] = identity.IPAddress;

        //            dsMaster.Tables[0].Rows.Add(dr);
        //        }
        //        string _Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();
        //        int ccount = 0;
        //        int taxcount = 0;
        //        if (detaildataList != null)
        //        {
        //            foreach (var item in detaildataList)
        //            {
        //                DataView dv = new DataView(dsDetail.Tables[0]);
        //                dv.RowFilter = "Id='" + item["Id"] + "'";
        //                if (dv.Count == 0)
        //                {
        //                    ccount++;
        //                    string detailid = materialCommonService.MakePK(_Id, ccount, 2);
        //                    item["Id"] = detailid;
        //                    item["SalesReturnId"] = _Id;
        //                    item["TransactionQty"] = item["ReturnQty"];
        //                    item["BaseQty"] = item["ReturnQty"];
        //                    item["BaseAmount"] = item["Amount"];
        //                    item["TransactionAmount"] = item["Amount"];
        //                    item["BooksCurrencyTransactionAmount"] = item["Amount"];
        //                    item["BooksCurrencyTaxAmount"] = item["TaxAmount"];
        //                    item["BooksCurrencyBaseRate"] = item["BaseRate"];
        //                    item["AddedBy"] = identity.Name;
        //                    item["AddedDate"] = DateTime.Now;
        //                    item["AddedFromIP"] = identity.IPAddress;
        //                    materialCommonService.AddNewRowD(dsDetail.Tables[0], item);

        //                    if (taxList != null)
        //                    {
        //                        foreach (var tx in taxList.Where(r => r["SalesMaterialId"].ToString() == item["SalesMaterialId"].ToString()))
        //                        {
        //                            DataView dvtx = new DataView(dstax.Tables[0]);
        //                            dvtx.RowFilter = "Id='" + tx["Id"] + "'";

        //                            if (dvtx.Count == 0)
        //                            {
        //                                taxcount++;
        //                                string taxid = materialCommonService.MakePK(detailid, taxcount, 2);
        //                                tx["Id"] = taxid;
        //                                tx["SalesReturnId"] = _Id;
        //                                tx["SalesReturnDetailId"] = detailid;
        //                                tx["AddedBy"] = identity.Name;
        //                                tx["AddedDate"] = DateTime.Now;
        //                                tx["AddedFromIP"] = identity.IPAddress;
        //                                materialCommonService.AddNewRowD(dstax.Tables[0], tx);
        //                            }

        //                        }
        //                    }
        //                    if (itemScanCildList != null)
        //                    {
        //                        foreach (var scitem in itemScanCildList.Where(r => r["SalesId"].ToString() == item["SalesId"].ToString()
        //                            && r["ActualPackingId"].ToString() == item["PackingId"].ToString()
        //                            && r["SalesOrderId"].ToString() == item["SalesOrderId"].ToString()))
        //                        {
        //                            DataView dvsc = new DataView(dsitemscanChild.Tables[0]);
        //                            dvsc.RowFilter = "Id='" + scitem["Id"] + "'";

        //                            if (dvsc.Count > 0)
        //                            {
        //                                DataRow drmo = dvsc[0].Row;
        //                                drmo.BeginEdit();
        //                                drmo["SalesReturnId"] = _Id;
        //                                drmo["UpdatedBy"] = identity.Name;
        //                                drmo["UpdatedDate"] = DateTime.Now.ToString();
        //                                drmo.EndEdit();

        //                            }

        //                        }
        //                    }
        //                }

        //            }
        //        }

        //        clsStaticInfo obj = new clsStaticInfo();
        //        obj.SaveDataSets(dsMaster, dsDetail, dstax, dsitemscanChild);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw (ex);
        //    }
        //}

        #endregion

        #region Tax
        [HttpGet, Authorize]
        public ActionResult GetPurchaseLCChargesTax(string purchaseLCChargesId)
        {
            try
            {
                var Sql = @"Select TC.UserName,HSN.Code AS HSNCode,T.* from TRN.PurchaseLCTax  T
                            LEFT JOIN [MST].[TaxCategory] AS TC ON T.TaxCategoryId=TC.Id
							LEFT JOIN [HKP].[HSNCode] AS HSN ON T.HSNCodeId = HSN.Id
                            WHERE T.PurchaseLCChargesId='" + purchaseLCChargesId + "'";
                return Json(_sqlRepository.GetDataCollection(Sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetPurchaseLCChargesTaxByLCId(string purchaseLCId)
        {
            try
            {
                var Sql = @"Select TC.UserName,HSN.Code AS HSNCode,T.* from TRN.PurchaseLCTax  T
                            LEFT JOIN [MST].[TaxCategory] AS TC ON T.TaxCategoryId=TC.Id
							LEFT JOIN [HKP].[HSNCode] AS HSN ON T.HSNCodeId = HSN.Id
                            WHERE T.PurchaseLCId='" + purchaseLCId + "'";
                return Json(_sqlRepository.GetDataCollection(Sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetTaxCategoryListByBankMaster(string companyGroupId, string bankMasterId, string plantId, string hsnCodeId, string PODate)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var sql = @"DECLARE @bankMasterId varchar(10)='" + bankMasterId + @"'
                                   , @bankState varchar(30)
                                  , @bankCountry varchar(10)
                                  , @plantState varchar(30)
                                  , @plantCountry varchar(10)
                                  , @plantId varchar(30)='" + identity.PlantId + @"'
                                  , @hsnCodeId varchar(30)='" + hsnCodeId + @"'
                    SET @bankCountry =(SELECT AM.CountryId FROM MST.BankMaster AS BM  LEFT JOIN HKP.BankBranch BB ON BB.Id=BM.BankBranchId LEFT JOIN MST.AddressMaster AS AM ON BB.AddressMasterId=AM.Id WHERE BM.Id=@bankMasterId)
                   SET @bankState =(SELECT AM.StateId FROM MST.BankMaster AS BM  LEFT JOIN HKP.BankBranch BB ON BB.Id=BM.BankBranchId LEFT JOIN MST.AddressMaster AS AM ON BB.AddressMasterId=AM.Id WHERE BM.Id=@bankMasterId)

                    SET @plantState =(SELECT AD.StateId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @plantCountry =(SELECT AD.CountryId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SELECT TVD.Id TaxVariantDetailId, TVD.TaxCategoryId, HP.HSNCodeId, HN.Code AS HSNCode, TC.UserName, ISNULL(HP.[Percentage], 0) AS [Percentage], NULL TotalAmount, NULL ServiceMasterId
                    FROM [MST].[TaxVariantDetail] AS TVD
                    JOIN [MST].[TaxVariant] AS TV ON TVD.TaxVariantId=TV.Id
                    JOIN [MST].[TaxCategory] AS TC ON TVD.TaxCategoryId=TC.Id
                    --LEFT JOIN (SELECT * FROM [MST].[HSNTaxPercentage] WHERE HSNCodeId=@hsnCodeId) AS HP ON HP.TaxCategoryId=TC.Id
					LEFT JOIN (SELECT * FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY TaxCategoryId, HSNCodeId ORDER BY EffectiveDate DESC) AS RN
								FROM [MST].[HSNTaxPercentage] WHERE CountryId=@plantCountry AND HSNCodeId=@hsnCodeId AND convert(DATE, EffectiveDate)<='" + PODate + @"') AS TBL WHERE RN=1) AS HP ON HP.TaxCategoryId=TC.Id

                    LEFT JOIN [HKP].[HSNCode] AS HN ON HP.HSNCodeId=HN.Id
                    WHERE TV.CompanyGroupId='" + identity.CompanyGroupId + @"' AND TV.CountryId=@plantCountry --AND HP.HSNCodeId=@hsnCodeId
                    AND TV.TaxFor=CASE WHEN @bankCountry=@plantCountry THEN '" + TaxFor.DomesticPurchase + @"'
				                        WHEN @bankCountry<>@plantCountry THEN '" + TaxFor.OverseasPurchase + @"' END
                    AND (TV.Different=CASE WHEN @bankCountry=@plantCountry AND @bankState=@plantState AND TV.DifferentIn='State' THEN 'Same'
					                       WHEN @bankCountry=@plantCountry AND @bankState<>@plantState AND TV.DifferentIn='State' THEN 'Different' END
	                    OR TV.Different IS NULL)
                    ORDER BY TC.[Sequence]";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetTaxCategoryList(string companyGroupId, string receiveId, string plantId, string hsnCodeId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var sql = @"DECLARE @receiveId varchar(10)='" + receiveId + @"'
                                  , @partyState varchar(30)
                                  , @partyCountry varchar(10)
                                  , @plantState varchar(30)
                                  , @plantCountry varchar(10)
                                  , @plantId varchar(30)='" + identity.PlantId + @"'
                                  , @hsnCodeId varchar(30)='" + hsnCodeId + @"'
                   SET @partyCountry =(SELECT AM.CountryId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id WHERE PP.PartyId=@receiveId)

                    SET @partyState =(SELECT AM.StateId FROM HKP.PartyPlant AS PP LEFT JOIN MST.AddressMaster AS AM ON PP.AddressMasterId=AM.Id WHERE PP.PartyId=@receiveId)

                    SET @plantState =(SELECT AD.StateId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SET @plantCountry =(SELECT AD.CountryId FROM MST.AddressMaster AS AD JOIN ORG.Plant AS PLNT ON AD.Id=PLNT.AddressMasterId WHERE PLNT.Id=@plantId)-- AND AD.Active=1 AND AD.Archive=0)
                    SELECT TVD.Id, TVD.TaxCategoryId, HP.HSNCodeId, HN.Code AS HSNCode, TC.UserName, ISNULL(HP.[Percentage], 0) AS [Percentage], NULL TotalAmount, NULL ServiceMasterId
                    FROM [MST].[TaxVariantDetail] AS TVD
                    JOIN [MST].[TaxVariant] AS TV ON TVD.TaxVariantId=TV.Id
                    JOIN [MST].[TaxCategory] AS TC ON TVD.TaxCategoryId=TC.Id
                    --LEFT JOIN (SELECT * FROM [MST].[HSNTaxPercentage] WHERE HSNCodeId=@hsnCodeId) AS HP ON HP.TaxCategoryId=TC.Id
					LEFT JOIN (SELECT * FROM (SELECT *, ROW_NUMBER() OVER (PARTITION BY TaxCategoryId, HSNCodeId ORDER BY EffectiveDate DESC) AS RN
								FROM [MST].[HSNTaxPercentage] WHERE CountryId=@plantCountry AND HSNCodeId=@hsnCodeId) AS TBL WHERE RN=1) AS HP ON HP.TaxCategoryId=TC.Id

                    LEFT JOIN [HKP].[HSNCode] AS HN ON HP.HSNCodeId=HN.Id
                    WHERE TV.CompanyGroupId='" + identity.CompanyGroupId + @"' AND TV.CountryId=@plantCountry --AND HP.HSNCodeId=@hsnCodeId
                    AND TV.TaxFor=CASE WHEN @partyCountry=@plantCountry THEN '" + TaxFor.DomesticPurchase + @"'
				                        WHEN @partyCountry<>@plantCountry THEN '" + TaxFor.OverseasPurchase + @"' END
                    AND (TV.Different=CASE WHEN @partyCountry=@plantCountry AND @partyState=@plantState AND TV.DifferentIn='State' THEN 'Same'
					                       WHEN @partyCountry=@plantCountry AND @partyState<>@plantState AND TV.DifferentIn='State' THEN 'Different' END
	                    OR TV.Different IS NULL)
                    ORDER BY TC.[Sequence]";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateTax(IEnumerable<PurchaseLCTax> entities)
        {
            try
            {
                SaveChargeTax(entities);

                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }

        private void SaveChargeTax(IEnumerable<PurchaseLCTax> data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    int c = 0;
                    foreach (var item in data)
                    {
                        c++;
                        string sql = "SELECT * FROM [TRN].[PurchaseLCTax] WHERE Id='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");


                        if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();
                            dr["Id"] = item.PurchaseLCChargesId + "-"+c;
                            dr["PurchaseLCId"] = item.PurchaseLCId;
                            dr["PurchaseLCChargesId"] = item.PurchaseLCChargesId;
                            dr["TaxCategoryId"] = item.TaxCategoryId;
                            dr["HSNCodeId"] = item.HSNCodeId;
                            dr["Percentage"] = item.Percentage;
                            dr["TaxAmount"] = item.TaxAmount;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;

                            dsMaster.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            //edit
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["PurchaseLCId"] = item.PurchaseLCId;
                            dr["PurchaseLCChargesId"] = item.PurchaseLCChargesId;
                            dr["TaxCategoryId"] = item.TaxCategoryId;
                            dr["HSNCodeId"] = item.HSNCodeId;
                            dr["Percentage"] = item.Percentage;
                            dr["TaxAmount"] = item.TaxAmount;

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }
                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        #endregion

        #endregion
    }


}