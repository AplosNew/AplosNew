#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Commercial;
using Library.Model.Inventory;
using Library.Model.OrderManagements;
using Library.Model.Payrolls;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Payrolls;
using Newtonsoft.Json;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Commercial.Controllers
{
    public class PurchaseLCAmendmentController : BaseController
    {
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        public PurchaseLCAmendmentController(IUnitOfWork U, ISqlRepository R)
        {
            _unitOfWork = U;
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages
     
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"
                        SELECT P.UserName PartyName,OB.AccountTitle OpeningBank
                        ,PR.UserName PortOfLanding,CN.Code Currency 
                        ,PLC.Id,PLC.Version, PLC.ContractId, PLC.VendorId, PLC.BenificiaryBank, PLC.OpeningBankMasterId, PLC.BenificiaryBankDescription, 
                         PLC.LeinBank, PLC.LeinBankDescription, PLC.OrderSpecific, PLC.LCRef, FORMAT(PLC.LCDate,'dd-MMM-yyyy') LCDate,
                         FORMAT(PLC.ExpiryDate,'dd-MMM-yyyy') ExpiryDate, PLC.Amount, PLC.[Type], PLC.Tenure, PLC.CurrencyId, PLC.Rate, PLC.FinalDestination, FORMAT(PLC.AmendmentDate,'dd-MMM-yyyy') AmendmentDate, 
                         PLC.PortOfLandingId, PLC.[Status], PLC.AddedBy, FORMAT(PLC.AddedDate,'dd-MMM-yyyy') AddedDate, PLC.AddedFromIP, PLC.UpdatedBy, FORMAT(PLC.UpdatedDate,'dd-MMM-yyyy') UpdatedDate, PLC.UpdatedFromIP
                        ,PLC.LCANo,PLC.LIBOUR,PLC.InsuranceCoverNoteNo,PLC.InsuranceAttachment,PLC.PaymentBasedOn,C.Id ContractId,ISNULL(C.ContractNo,'')ContractNo,PLC.PortOfLoading,PLC.InsuranceValue,PT.UserName CustomerName,FORMAT(PLC.ShipmentDate,'dd-MMM-yyyy') ShipmentDate,PLC.PINo,OB.CurrencyId BankCurrency,MLC.LCRef MasterLCNo,C.Remarks ContractRemarks ,PLC.Remarks
                        FROM [dbo].[PurchaseLC] PLC
                        LEFT JOIN dbo.[Contract] C ON C.Id=PLC.ContractId
                        LEFT JOIN dbo.MasterLC MLC ON MLC.Id=C.MasterLCId
						LEFT JOIN HKP.Party PT ON PT.Id=C.CustomerId
                        LEFT JOIN HKP.Party P  ON P.Id=PLC.VendorId
                        LEFT JOIN MST.BankMaster OB  ON OB.Id=PLC.OpeningBankMasterId
                        LEFT JOIN [MST].[Port] PR ON PR.Id=PLC.PortOfLandingId
                        LEFT JOIN SCS.Currency CN ON CN.Id=PLC.CurrencyId
                        Where PLC.PlantId='" + identity.PlantId + "'";
            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
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
                          WHERE PLC.PurchaseLCId='" + purchaseLCId + "' AND LCT.Type='"+ ChargesType.Amendment + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetListByVersion(string purchaseLCId, string Version)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"
                      SELECT P.UserName PartyName, OB.AccountTitle OpeningBank
                     ,PR.UserName PortOfLanding
                     ,PLC.PurchaseLCId Id,PLC.Version, PLC.ContractId, PLC.VendorId, PLC.BenificiaryBank, PLC.OpeningBankMasterId, PLC.BenificiaryBankDescription, 
                     PLC.LeinBank, PLC.LeinBankDescription, PLC.OrderSpecific, PLC.LCRef, PLC.AmendmentDate, FORMAT(PLC.LCDate,'dd-MMM-yyyy') LCDate,
                     FORMAT(PLC.ExpiryDate,'dd-MMM-yyyy') ExpiryDate, PLC.Amount, PLC.[Type], PLC.Tenure, PLC.CurrencyId, PLC.Rate, PLC.FinalDestination, 
                     PLC.PortOfLandingId, PLC.[Status], PLC.AddedBy, FORMAT(PLC.AddedDate,'dd-MMM-yyyy') AddedDate, PLC.AddedFromIP, PLC.UpdatedBy, FORMAT(PLC.UpdatedDate,'dd-MMM-yyyy') UpdatedDate, PLC.UpdatedFromIP
                     ,PLC.LCANo,PLC.LIBOUR,PLC.InsuranceCoverNoteNo,PLC.InsuranceAttachment,PLC.PaymentBasedOn,PLC.PortOfLoading,PLC.InsuranceValue,PLC.IsAccepptanceFirst, PLC.PlantId,PLC.ShipmentDate,PLC.PINo
                     ,0 Flag   
                     FROM [dbo].[PurchaseLCVersion] PLC
                     LEFT JOIN dbo.[Contract] C ON C.Id=PLC.ContractId
                     LEFT JOIN HKP.Party P  ON P.Id=PLC.VendorId
                     LEFT JOIN MST.BankMaster OB  ON OB.Id=PLC.OpeningBankMasterId
                     LEFT JOIN [MST].[Port] PR ON PR.Id=PLC.PortOfLandingId
                     WHERE PLC.Version='" + Version + @"' AND PLC.PurchaseLCId='" + purchaseLCId + @"'
                     UNION 
                     SELECT P.UserName PartyName,OB.AccountTitle OpeningBank
                     ,PR.UserName PortOfLanding
                     , PLC.Id,PLC.Version, PLC.ContractId, PLC.VendorId, PLC.BenificiaryBank, PLC.OpeningBankMasterId, PLC.BenificiaryBankDescription, 
                     PLC.LeinBank, PLC.LeinBankDescription, PLC.OrderSpecific, PLC.LCRef, PLC.AmendmentDate, FORMAT(PLC.LCDate,'dd-MMM-yyyy') LCDate,
                     FORMAT(PLC.ExpiryDate,'dd-MMM-yyyy') ExpiryDate, PLC.Amount, PLC.[Type], PLC.Tenure, PLC.CurrencyId, PLC.Rate, PLC.FinalDestination, 
                     PLC.PortOfLandingId, PLC.[Status], PLC.AddedBy, FORMAT(PLC.AddedDate,'dd-MMM-yyyy') AddedDate, PLC.AddedFromIP, PLC.UpdatedBy, FORMAT(PLC.UpdatedDate,'dd-MMM-yyyy') UpdatedDate, PLC.UpdatedFromIP
                     ,PLC.LCANo,PLC.LIBOUR,PLC.InsuranceCoverNoteNo,PLC.InsuranceAttachment,PLC.PaymentBasedOn ,PLC.PortOfLoading,PLC.InsuranceValue,PLC.IsAccepptanceFirst , PLC.PlantId,PLC.ShipmentDate,PLC.PINo
                     ,1 Flag
                     FROM [dbo].[PurchaseLC] PLC
                     LEFT JOIN dbo.[Contract] C ON C.Id=PLC.ContractId
                     LEFT JOIN HKP.Party P  ON P.Id=PLC.VendorId
                     LEFT JOIN MST.BankMaster OB  ON OB.Id=PLC.OpeningBankMasterId
                     LEFT JOIN [MST].[Port] PR ON PR.Id=PLC.PortOfLandingId
                     WHERE PLC.Version='" + Version + @"' AND PLC.Id='" + purchaseLCId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetPurchaseLCChargesDataByVersion(string purchaseLCId, string Version)
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
                          WHERE PLC.PurchaseLCId='" + purchaseLCId + "' AND PLC.Version='" + Version + "' AND LCT.Type='" + ChargesType.Amendment + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetVersionCbo(string purchaseLCId)
        {
            string sql = @"SELECT Id,Version FROM PurchaseLC WHERE Id='" + purchaseLCId + @"'
                           UNION
                           SELECT PurchaseLCId Id,Version  FROM PurchaseLCVersion where PurchaseLCId='" + purchaseLCId + "'";
            return Json(_sqlRepository.GetCombo(sql, "Id", "Version"), JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public JsonResult Create(PurchaseLC model, IEnumerable<PurchaseLCCharges> Charges, string flg)
        //{
        //    try
        //    {
        //        if (flg == "Amendment")
        //        {
        //            SaveAmendmentData(model, out string version, out string masterId);
        //            SaveAmendmentChargeData(Charges, masterId, version);
        //            return Json(new { Version = version, Id = masterId, Message = AplosMessage.Insert });
        //        }
        //        else
        //        {
        //            SaveData(model, out string version, out string masterId);
        //            SaveChargeData(Charges, masterId);
        //            return Json(new { Version = version, Id = masterId, Message = AplosMessage.Insert });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { Error = true, ex.Message });
        //    }

        //}

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
                IEnumerable<PurchaseOrder> POList = JsonConvert.DeserializeObject<IEnumerable<PurchaseOrder>>(form["POList"], settings);
                IEnumerable<ServicePOMaster> SPOList = JsonConvert.DeserializeObject<IEnumerable<ServicePOMaster>>(form["SPOList"], settings);
                IEnumerable<OSTransformationPO> JWPOList = JsonConvert.DeserializeObject<IEnumerable<OSTransformationPO>>(form["JWPOList"], settings);


                var directory = ResourcesPathReader.GetLCDocPath();
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                string path = Path.Combine(directory);
                string _id = "";
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


                if (model.flag == "Amendment")
                {
                    SaveAmendmentData(model, out string version, out string masterId);
                    SaveAmendmentChargeData(Charges, masterId, version);
                    UpdatePurchaseOrder(POList, masterId, model);
                    UpdateServiceOrderPO(SPOList, masterId, model);
                    UpdateJWPO(JWPOList, masterId, model);

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
                else
                {
                    SaveData(model, out string version, out string masterId);
                    SaveChargeData(Charges, masterId);
                    UpdatePurchaseOrder(POList, masterId, model);
                    UpdateServiceOrderPO(SPOList, masterId, model);
                    UpdateJWPO(JWPOList, masterId, model);

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

               

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }

        private void UpdatePurchaseOrder(IEnumerable<PurchaseOrder> POList, string masterId, PurchaseLC model)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (POList.Any())
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    foreach (var item in POList)
                    {
                        string sql = "SELECT * FROM [dbo].[POLCMap] WHERE PurchaseOrderId='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                        if (dsMaster.Tables[0].Rows.Count > 0)
                        {
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();

                            dr["ContractId"] = model.ContractId;
                            dr["PurchaseLCId"] = masterId;
                            dr["OrderSpecific"] = model.OrderSpecific;
                            dr["PurchaseOrderId"] = item.Id;
                            dr["Amount"] = item.TransactionAmount;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }
                        else //if (dsMaster.Tables[0].Rows.Count == 0)
                        {
                            DataRow dr = dsMaster.Tables[0].NewRow();

                            dr["ContractId"] = model.ContractId;
                            dr["PurchaseLCId"] = masterId;
                            dr["PurchaseOrderId"] = item.Id;
                            dr["OrderSpecific"] = model.OrderSpecific;
                            dr["Amount"] = item.TransactionAmount;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = DateTime.Now;
                            dr["AddedFromIP"] = identity.IPAddress;
                            dsMaster.Tables[0].Rows.Add(dr);

                        }

                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
                else
                {
                    string _sql = "Update TRN.PurchaseOrder SET PurchaseLCId=NULL WHERE PurchaseLCId='" + masterId + "'";
                    _sqlRepository.ExecuteSqlCommand(_sql);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

      

        private void UpdateServiceOrderPO(IEnumerable<ServicePOMaster> SPOList, string masterId, PurchaseLC model)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (SPOList.Any())
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    foreach (var item in SPOList)
                    {
                        string sql = "SELECT * FROM TRN.ServicePOMaster WHERE Id='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                        if (dsMaster.Tables[0].Rows.Count > 0)
                        {
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();

                            dr["ContractId"] = model.ContractId;
                            dr["PurchaseLCId"] = masterId;
                            dr["OrderSpecific"] = model.OrderSpecific;

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }

                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
                else
                {
                    string _sql = "Update TRN.ServicePOMaster SET PurchaseLCId=NULL WHERE PurchaseLCId='" + masterId + "'";
                    _sqlRepository.ExecuteSqlCommand(_sql);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void UpdateJWPO(IEnumerable<OSTransformationPO> SPOList, string masterId, PurchaseLC model)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (SPOList.Any())
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    foreach (var item in SPOList)
                    {
                        string sql = "SELECT * FROM [dbo].[OSTransformationPO] WHERE Id='" + item.Id + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                        if (dsMaster.Tables[0].Rows.Count > 0)
                        {
                            DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();

                            //dr["ContractId"] = model.ContractId;
                            dr["PurchaseLCId"] = masterId;
                            dr["OrderSpecific"] = model.OrderSpecific;

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();
                        }

                        clsStaticInfo obj = new clsStaticInfo();
                        obj.SaveDataSets(dsMaster);
                    }
                }
                else
                {
                    string _sql = "Update [dbo].[OSTransformationPO] SET PurchaseLCId=NULL WHERE PurchaseLCId='" + masterId + "'";
                    _sqlRepository.ExecuteSqlCommand(_sql);
                }
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

        //[HttpPost]
        //public JsonResult CreateCharge(IEnumerable<PurchaseLCCharges> Charges)
        //{
        //    try
        //    {
        //        SaveChargeData(Charges);
        //        return Json(new {Message = AplosMessage.Insert });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { Error = true, ex.Message });
        //    }

        //}

        private void SaveChargeData(IEnumerable<PurchaseLCCharges> data, string masterId)
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
                            dr["Version"] = item.Version==1? item.Version+1: item.Version;

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
                            dr["Version"] = item.Version;

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

        private void SaveAmendmentChargeData(IEnumerable<PurchaseLCCharges> data, string masterId, string version)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (data != null)
                {
                    ConnectionManager.DAL.ConManager objCon;
                    DataSet dsMaster;
                    string id = string.Empty;
                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM [dbo].[PurchaseLCCharges] WHERE Id='" + id + "'";
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
        private string GetVersionPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "PurchaseLCVersion", out sID);
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
                //var IsUniquePINo = CheckUniquePINo(data);
                //if (!IsUniquePINo)
                //{
                //    throw new Exception("PI No is Unique.");
                //}

                //var IsUniqueLCRef = CheckUniqueLCRef(data);
                //if (IsUniqueLCRef)
                //{
                //    throw new Exception("LC Ref No is Unique.");
                //}

                GetAutoSequence(data.Id, out dsSeq);
                decimal seq = Convert.ToDecimal(dsSeq.Tables[0].Rows[0]["Version"].ToString());

                string _sql = "SELECT * FROM [dbo].[PurchaseLC] WHERE Id='" + data.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(_sql, out dsMaster, false, "1");

                /// Update data in PurchaseLC Table

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
                    dr["Remarks"] = data.Remarks;
                    

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

                    dr["Version"] = data.Version;
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
                    dr["Remarks"] = data.Remarks;

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

        private void SaveAmendmentData(PurchaseLC data, out string version, out string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster, dsVersion;
            string contId = string.Empty;
            string id = string.Empty;
            DataSet dsSeq = null;
            try
            {

                var IsUniqueLCRef = CheckUniqueLCRef(data);
                if (!IsUniqueLCRef)
                {
                    throw new Exception("LC Ref No is Unique.");
                }

                var IsUniquePINo = CheckUniquePINo(data);
                if (!IsUniquePINo)
                {
                    throw new Exception("PI No is Unique.");
                }

                GetAutoSequence(data.Id, out dsSeq);
                decimal seq = Convert.ToDecimal(dsSeq.Tables[0].Rows[0]["Version"].ToString());

                string _sql = "SELECT * FROM [dbo].[PurchaseLC] WHERE Id='" + data.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(_sql, out dsMaster, false, "1");

                // keep data in Version Table

                string sql = "SELECT * FROM [dbo].[PurchaseLCVersion] WHERE Id='" + id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsVersion, false, "1");

                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    if (dsVersion.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsVersion.Tables[0].NewRow();

                        dr["Id"] = "PLCV" + GetVersionPK();
                        dr["PurchaseLCId"] = data.Id;
                        dr["ContractId"] = dsMaster.Tables[0].Rows[0]["ContractId"];
                        dr["VendorId"] = dsMaster.Tables[0].Rows[0]["VendorId"];
                        dr["BenificiaryBank"] = dsMaster.Tables[0].Rows[0]["BenificiaryBank"];
                        dr["OpeningBankMasterId"] = dsMaster.Tables[0].Rows[0]["OpeningBankMasterId"];
                        dr["BenificiaryBankDescription"] = dsMaster.Tables[0].Rows[0]["BenificiaryBankDescription"];
                        dr["LeinBank"] = dsMaster.Tables[0].Rows[0]["LeinBank"];
                        dr["LeinBankDescription"] = dsMaster.Tables[0].Rows[0]["LeinBankDescription"];
                        dr["OrderSpecific"] = dsMaster.Tables[0].Rows[0]["OrderSpecific"];
                        dr["LCRef"] = dsMaster.Tables[0].Rows[0]["LCRef"];
                        dr["LCDate"] = dsMaster.Tables[0].Rows[0]["LCDate"];
                        dr["AmendmentDate"] = dsMaster.Tables[0].Rows[0]["AmendmentDate"] != System.DBNull.Value ? dsMaster.Tables[0].Rows[0]["AmendmentDate"] : System.DBNull.Value;
                        dr["ExpiryDate"] = dsMaster.Tables[0].Rows[0]["ExpiryDate"];
                        dr["Amount"] = dsMaster.Tables[0].Rows[0]["Amount"];
                        dr["Type"] = dsMaster.Tables[0].Rows[0]["Type"];
                        dr["Tenure"] = dsMaster.Tables[0].Rows[0]["Tenure"];
                        dr["CurrencyId"] = dsMaster.Tables[0].Rows[0]["CurrencyId"];
                        dr["Rate"] = dsMaster.Tables[0].Rows[0]["Rate"];
                        dr["FinalDestination"] = dsMaster.Tables[0].Rows[0]["FinalDestination"];
                        dr["PortOfLandingId"] = dsMaster.Tables[0].Rows[0]["PortOfLandingId"];
                        dr["Status"] = dsMaster.Tables[0].Rows[0]["Status"];
                        dr["Remarks"] = dsMaster.Tables[0].Rows[0]["Remarks"];
                        dr["LCANo"] = dsMaster.Tables[0].Rows[0]["LCANo"];
                        dr["LIBOUR"] = dsMaster.Tables[0].Rows[0]["LIBOUR"];
                        dr["InsuranceCoverNoteNo"] = dsMaster.Tables[0].Rows[0]["InsuranceCoverNoteNo"];
                        dr["InsuranceAttachment"] = dsMaster.Tables[0].Rows[0]["InsuranceAttachment"];
                        dr["PaymentBasedOn"] = dsMaster.Tables[0].Rows[0]["PaymentBasedOn"];
                        dr["InsuranceValue"] = dsMaster.Tables[0].Rows[0]["InsuranceValue"];
                        dr["IsAccepptanceFirst"] = dsMaster.Tables[0].Rows[0]["IsAccepptanceFirst"];
                        dr["PlantId"] = dsMaster.Tables[0].Rows[0]["PlantId"];
                        dr["PortOfLoading"] = dsMaster.Tables[0].Rows[0]["PortOfLoading"];

                        dr["ShipmentDate"] = dsMaster.Tables[0].Rows[0]["ShipmentDate"];
                        dr["PINo"] = dsMaster.Tables[0].Rows[0]["PINo"];
                        dr["Remarks"] = dsMaster.Tables[0].Rows[0]["Remarks"];

                        dr["Version"] = dsMaster.Tables[0].Rows[0]["Version"];
                        dr["AddedBy"] = dsMaster.Tables[0].Rows[0]["AddedBy"];
                        dr["AddedDate"] = dsMaster.Tables[0].Rows[0]["AddedDate"];
                        dr["AddedFromIP"] = dsMaster.Tables[0].Rows[0]["AddedFromIP"];

                        dr["UpdatedBy"] = dsMaster.Tables[0].Rows[0]["UpdatedBy"];
                        dr["UpdatedDate"] = dsMaster.Tables[0].Rows[0]["UpdatedDate"];
                        dr["UpdatedFromIP"] = dsMaster.Tables[0].Rows[0]["UpdatedFromIP"];

                        dsVersion.Tables[0].Rows.Add(dr);
                    }
                }

                /// Update data in PurchaseLC Table

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = "PLC" + GetPK();
                    dr["Version"] = seq;
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
                    dr["ExpiryDate"] = data.ExpiryDate;
                    dr["Amount"] = data.Amount;                    

                    dr["Type"] = data.Type;
                    dr["Tenure"] = data.Tenure;
                    dr["CurrencyId"] = data.CurrencyId;
                    dr["Rate"] = data.Rate;
                    dr["FinalDestination"] = data.FinalDestination;
                    dr["PortOfLandingId"] = data.PortOfLandingId;
                    dr["Status"] = data.Status;
                    dr["Remarks"] = data.Remarks;
                    dr["AmendmentDate"] = data.AmendmentDate == null ? System.DBNull.Value : (object)data.AmendmentDate;
                    dr["LCANo"] = data.LCANo;
                    dr["LIBOUR"] = data.LIBOUR;
                    dr["InsuranceCoverNoteNo"] = data.InsuranceCoverNoteNo;
                    dr["InsuranceAttachment"] = data.InsuranceAttachment;
                    dr["PaymentBasedOn"] = data.PaymentBasedOn;
                    dr["InsuranceValue"] =data.InsuranceValue;
                    dr["IsAccepptanceFirst"] = data.IsAccepptanceFirst;
                    dr["PortOfLoading"] = data.PortOfLoading;
                    dr["PlantId"] = identity.PlantId;
                    dr["Remarks"] = data.Remarks;

                    dr["ShipmentDate"] = data.ShipmentDate == null ? System.DBNull.Value : (object)data.ShipmentDate;
                    dr["PINo"] = data.PINo;

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
                    dr["ExpiryDate"] = data.ExpiryDate;
                    dr["Amount"] = data.Amount;

                    dr["Type"] = data.Type;
                    dr["Tenure"] = data.Tenure;
                    dr["CurrencyId"] = data.CurrencyId;
                    dr["Rate"] = data.Rate;
                    dr["FinalDestination"] = data.FinalDestination;
                    dr["PortOfLandingId"] = data.PortOfLandingId;
                    dr["Status"] = data.Status;
                    dr["Remarks"] = data.Remarks;
                    dr["AmendmentDate"] = data.AmendmentDate == null ? System.DBNull.Value : (object)data.AmendmentDate;
                    dr["LCANo"] = data.LCANo;
                    dr["LIBOUR"] = data.LIBOUR;
                    dr["InsuranceCoverNoteNo"] = data.InsuranceCoverNoteNo;
                    dr["InsuranceAttachment"] = data.InsuranceAttachment;
                    dr["PaymentBasedOn"] = data.PaymentBasedOn;
                    dr["InsuranceValue"] = data.InsuranceValue;
                    dr["IsAccepptanceFirst"] = data.IsAccepptanceFirst;
                    dr["PortOfLoading"] = data.PortOfLoading;
                    dr["PlantId"] = identity.PlantId;

                    dr["ShipmentDate"] = data.ShipmentDate == null ? System.DBNull.Value : (object)data.ShipmentDate;
                    dr["PINo"] = data.PINo;
                    dr["Version"] = data.Version + 1;
                    dr["Remarks"] = data.Remarks;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }


                clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster, dsVersion);

                masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                version = dsMaster.Tables[0].Rows[0]["Version"].ToString();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetAmendmentLCChargesGLData()
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
                            WHERE LCCTGL.GLType='Purchase' AND LCT.Type='" + ChargesType.Amendment + "'";
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
                catch (Exception exx)
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
                catch (Exception exx)
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

        #endregion
    }

}