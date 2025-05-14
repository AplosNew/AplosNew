#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Commercial;
using Library.Model.Enums;
using Library.Model.Inventory;
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
using System.Xml.Serialization;

#endregion

namespace Aplos.Areas.Commercial.Controllers
{
    public class PurchaseLCWithPOController : BaseController
    {
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        public PurchaseLCWithPOController(IUnitOfWork U, ISqlRepository R)
        {
            _unitOfWork = U;
            _sqlRepository = R;
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

        #region PurchaseLC
        [HttpGet, Authorize]
        public ActionResult GetList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string sql = @"SELECT PLCV.[Version] PreVersion, PLCV.Amount AmendmentAmount, FORMAT(PLC.AmendmentDate,'dd-MMM-yyyy') AmendmentDate, 
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
						AND PLCV.Id=(SELECT TOP 1 Id FROM [dbo].[PurchaseLCVersion] WHERE PurchaseLCId=PLC.Id  ORDER BY [Version] ASC) Where PLC.PlantId='" + identity.PlantId + "'  ORDER BY PLC.AddedDate DESC";
                var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetPurchaseLCChargesData(string purchaseLCId)
        {

            try
            {
                string sql = @"
                          SELECT PLC.*,C.Id BankCurrencyId,OB.AccountTitle OpeningBankMaster,BD.UserName Budget,(GL.AccountCode + ' - ' + GL.UserName) GL,A.UserName Activity,LCT.UserName OverHeadType,V.VoucherNo,LCT.Type
                          FROM [dbo].[PurchaseLCCharges] PLC
                          INNER JOIN [HKP].[OverHeadTypeGL] LCGL ON LCGL.Id=PLC.OverHeadTypeGLId
                          LEFT JOIN [HKP].OverHeadType LCT ON LCT.Id=LCGL.LCChargesTypeId
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
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetPurchaseLCBackData(string purchaseLCId, string Version)
        {

            try
            {
                string sql = @"SELECT V.*, 1 Back FROM [dbo].[PurchaseLCVersion] V Where PurchaseLCId='" + purchaseLCId + "' AND Version=" + Version + "";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetPurchaseLCChargesVersionData(string purchaseLCId, string Version)
        {
            try
            {
                string sql = @"
                          SELECT PLC.*,C.Id BankCurrencyId,OB.AccountTitle OpeningBankMaster,BD.UserName Budget,(GL.AccountCode + ' - ' + GL.UserName) GL,A.UserName Activity,LCT.UserName OverHeadType,V.VoucherNo,LCT.Type
                          FROM [dbo].[PurchaseLCCharges] PLC
                          INNER JOIN [HKP].[OverHeadTypeGL] LCGL ON LCGL.Id=PLC.OverHeadTypeGLId
                          LEFT JOIN [HKP].OverHeadType LCT ON LCT.Id=LCGL.LCChargesTypeId
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

                SaveData(model, out string version, out string masterId);
                SaveChargeData(Charges, masterId, version);
               // UpdatePurchaseOrder(POList, masterId, model);
                UpdatePOLCMap(POList, masterId, model);
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
                        string sql = "SELECT * FROM TRN.PurchaseOrder WHERE Id='" + item.Id + "'";
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
                    string _sql = "Update TRN.PurchaseOrder SET PurchaseLCId=NULL WHERE PurchaseLCId='" + masterId + "'";
                    _sqlRepository.ExecuteSqlCommand(_sql);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void UpdatePOLCMap(IEnumerable<PurchaseOrder> POList, string masterId, PurchaseLC model)
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
                            dr["Amount"] = item.Amount;
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
                            dr["Amount"] = item.Amount;

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
                var _sql = @"SELECT LCRef FROM [dbo].[PurchaseLC] where Id<>'" + data.Id + "' AND LCRef='"+data.LCRef + "'";
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
                var _sql = @"SELECT PINo FROM [dbo].[PurchaseLC] where Id<>'" + data.Id + "' AND PINo='"+data.PINo + "'";
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

                GetAutoVersion(data.Id, out dsSeq);
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
                    //dr["AcceptanceDate"] = data.AcceptanceDate;
                    //dr["MaturityDate"] = data.MaturityDate;
                    //dr["PaymentDate"] = data.PaymentDate;
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
                    //dr["AcceptanceDate"] = data.AcceptanceDate;
                    //dr["MaturityDate"] = data.MaturityDate;
                    //dr["PaymentDate"] = data.PaymentDate;
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
                            LEFT JOIN [HKP].OverHeadType LCT ON LCT.Id=LCCTGL.LCChargesTypeId
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
                            LEFT JOIN [HKP].OverHeadType LCT ON LCT.Id=LCCTGL.LCChargesTypeId
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

        public void GetAutoVersion(string Id, out DataSet dsRef)
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

        [HttpGet, Authorize]
        public ActionResult GetAlldataPOWithoutLCMap()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {

                var sql = @"SELECT [check]=CAST (0 AS bit),
                                    PO.Id,REPLACE(CONVERT(CHAR(11), PO.PODate, 106),' ','-') AS PODate,PO.PartyId,
                                    InvPP.StandardName ,ISNULL(PO.OrderSpecific,'')OrderSpecifi,PO.ContractId,PO.PurchaseLCId, CN.Code Currency,PO.CurrencyId
                                    ,CONVERT(NUMERIC(10,2),POD.TransactionAmount+ISNULL(POC.Amount,0)+ISNULL(POT.TaxAmount,0)) TransactionAmount,ISNULL(PLC.LCAmount,0) LCAmount,
									BalanceAmount=CONVERT(NUMERIC(10,2),POD.TransactionAmount+ISNULL(POC.Amount,0)+ISNULL(POT.TaxAmount,0))-ISNULL(PLC.LCAmount,0),ISNULL(C.ContractNo,'')ContractNo,Flag='MaterialPO',CC.UserName CustomerName,PT.UserName PaymentTerm
                                    ,IsFirst=case when GRN.GRNId>0 then 1 else 0 end,PO.DocRefNo,PO.PaymentTermId
                                    FROM TRN.PurchaseOrder PO
                                    INNER JOIN (SELECT SUM(TransactionAmount) TransactionAmount, InventoryReceiveId 
							        FROM [TRN].[PurchaseOrderDetail] GROUP BY InventoryReceiveId) POD ON POD.InventoryReceiveId=PO.Id
									LEFT JOIN (SELECT InventoryReceiveId,SUM(TaxAmount) TaxAmount FROM TRN.PurchaseOrderTax GROUP BY InventoryReceiveId) POT ON POT.InventoryReceiveId=PO.ID
                                    LEFT JOIN [HKP].[Party] AS InvPP ON PO.PartyId=InvPP.Id
                                    LEFT JOIN [MST].[PaymentTerm] PT ON PT.id=PO.PaymentTermId 
                                    LEFT JOIN [dbo].[Contract] C ON C.Id=PO.ContractId
                                    LEFT JOIN [HKP].[Party] AS CC ON CC.Id=C.CustomerId
                                    LEFT JOIN SCS.Currency CN ON CN.Id=PO.CurrencyId
                                    LEFT JOIN (select PurchaseOrderId,SUM(Amount)LCAmount from dbo.POLCMap Group By PurchaseOrderId) PLC ON PLC.PurchaseOrderId=PO.Id
                                    LEFT JOIN (Select PoId,COUNT(GRNId) GRNId from TRN.POGGRNMap GROUP BY PoId) GRN ON GRN.PoId=PO.Id
                                    LEFT JOIN (SELECT InventoryReceiveId,SUM(Amount) Amount FROM TRN.POService GROUP BY InventoryReceiveId) POC ON POC.InventoryReceiveId=PO.Id
                                    WHERE PO.PlantId='" + identity.PlantId + @"' AND PT.PaymentMode = 'LC' 
                                    AND (POD.TransactionAmount!=ISNULL(PLC.LCAmount,0)) 
                                    AND PO.IsClosed=0  AND AuthorizedByStatus='Approved' 
                            UNION 
                            SELECT [check]=CAST (CASE WHEN PO.PurchaseLCId IS NULL THEN 0 ELSE 1 END AS bit),
                                    PO.Id,REPLACE(CONVERT(CHAR(11), PO.PODate, 106),' ','-') AS PODate,PO.PartyId,
                                    InvPP.StandardName ,ISNULL(PO.OrderSpecific,'')OrderSpecifi,PO.ContractId,PO.PurchaseLCId, CN.Code Currency,PO.CurrencyId
                                    ,CONVERT(NUMERIC(10,2),POD.TransactionAmount) TransactionAmount,0 LCAmount,BalanceAmount=CONVERT(NUMERIC(10,2),POD.TransactionAmount),ISNULL(C.ContractNo,'')ContractNo,Flag='ServicePO',CC.UserName CustomerName
                                    ,PT.UserName PaymentTerm,IsFirst=case when GRN.GRNId>0 then 1 else 0 end,PO.DocRefNo,PO.PaymentTermId
                                    FROM [TRN].[ServicePOMaster] PO
                                    INNER JOIN (SELECT SUM(Amount) TransactionAmount, ServicePOMasterId 
							        FROM [TRN].[ServicePODetail] GROUP BY ServicePOMasterId) POD ON POD.ServicePOMasterId=PO.Id
                                    LEFT JOIN [HKP].[Party] AS InvPP ON PO.PartyId=InvPP.Id
                                    LEFT JOIN [MST].[PaymentTerm] PT ON PT.id=PO.PaymentTermId 
                                    LEFT JOIN [dbo].[Contract] C ON C.Id=PO.ContractId
                                    LEFT JOIN [HKP].[Party] AS CC ON CC.Id=C.CustomerId
                                    LEFT JOIN SCS.Currency CN ON CN.Id=PO.CurrencyId 
                                    LEFT JOIN (Select ServicePoId,COUNT(ServiceAckId) GRNId from TRN.ServivePOAcknowledgementMap GROUP BY ServicePoId) GRN ON GRN.ServicePoId=PO.Id
                                    WHERE PO.PlantId='" + identity.PlantId + @"' AND PT.PaymentMode = 'LC' AND ISNULL(PO.PurchaseLCId,'')='' AND PO.IsClosed=0  AND ApprovedByStatus='Approved'
                                     UNION 
                            SELECT [check]=CAST (CASE WHEN PO.PurchaseLCId IS NULL THEN 0 ELSE 1 END AS bit),
                                    PO.Id,REPLACE(CONVERT(CHAR(11), PO.PODate, 106),' ','-') AS PODate,PO.PartyId,
                                    InvPP.StandardName ,ISNULL(PO.OrderSpecific,'')OrderSpecifi,PO.ContractId,PO.PurchaseLCId, CN.Code Currency,PO.CurrencyId
                                    ,CONVERT(NUMERIC(10,2),POD.TransactionAmount) TransactionAmount,0 LCAmount,BalanceAmount=CONVERT(NUMERIC(10,2),POD.TransactionAmount),ISNULL(C.ContractNo,'')ContractNo,Flag='OutSourcePO',CC.UserName CustomerName
                                    ,PT.UserName PaymentTerm,IsFirst=0,PO.DocRefNo,PO.PaymentTermId
                                    FROM [dbo].[OSTransformationPO] PO
                                    INNER JOIN (SELECT SUM(ISNULL(TransactionAmount,0)) TransactionAmount, OSTransformationPOId 
							        FROM [dbo].[OSTransformationPODetail] GROUP BY OSTransformationPOId) POD ON POD.OSTransformationPOId=PO.Id
                                    LEFT JOIN [HKP].[Party] AS InvPP ON PO.PartyId=InvPP.Id
                                    LEFT JOIN [MST].[PaymentTerm] PT ON PT.id=PO.PaymentTermId 
                                    LEFT JOIN [dbo].[Contract] C ON C.Id=PO.ContractId
                                    LEFT JOIN [HKP].[Party] AS CC ON CC.Id=C.CustomerId
                                    LEFT JOIN SCS.Currency CN ON CN.Id=PO.CurrencyId 
                                    --LEFT JOIN (Select ServicePoId,COUNT(ServiceAckId) GRNId from TRN.ServivePOAcknowledgementMap GROUP BY ServicePoId) GRN ON GRN.ServicePoId=PO.Id
                                    WHERE PO.PlantId='" + identity.PlantId + @"' AND PT.PaymentMode = 'LC' AND ISNULL(PO.PurchaseLCId,'')='' AND ISNULL(PO.IsClosed,0)=0 AND PO.IsApproved=1";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetAlldataPOWithLCMap(string purchaseLCId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var sql = @"SELECT 
                             distinct PO.Id,REPLACE(CONVERT(CHAR(11), PO.PODate, 106),' ','-') AS PODate,PO.PartyId,
                            InvPP.StandardName ,ISNULL(PLC.OrderSpecific,PO.OrderSpecific) OrderSpecifi,ISNULL(PLC.ContractId, PO.ContractId) ContractId
							,PLCM.PurchaseLCId, CN.Code Currency,PO.CurrencyId
                            ,CONVERT(NUMERIC(10,2),POD.TransactionAmount+ISNULL(POC.Amount,0)+ISNULL(POT.TaxAmount,0)) TransactionAmount
                            ,PLCM.Amount LCAmount,BalanceAmount=CONVERT(NUMERIC(10,2),POD.TransactionAmount+ISNULL(POT.TaxAmount,0)+ISNULL(POC.Amount,0))-PLCM.Amount,0 Amount
                            , 0 AS [check],Flag='MaterialPO',PLC.LCRef,PO.DocRefNo,PO.PaymentTermId,PT.UserName PaymentTerm
                            FROM  [dbo].[POLCMap] PLCM
							LEFT JOIN TRN.PurchaseOrder PO ON PO.Id=PLCM.PurchaseOrderId
                            INNER JOIN (SELECT SUM(TransactionAmount) TransactionAmount, InventoryReceiveId FROM [TRN].[PurchaseOrderDetail] GROUP BY InventoryReceiveId) POD ON POD.InventoryReceiveId=PO.Id
                            LEFT JOIN (SELECT InventoryReceiveId,SUM(TaxAmount) TaxAmount FROM TRN.PurchaseOrderTax GROUP BY InventoryReceiveId) POT ON POT.InventoryReceiveId=PO.ID   
                            LEFT JOIN [HKP].[Party] AS InvPP ON PO.PartyId=InvPP.Id
                            LEFT JOIN [MST].[PaymentTerm] PT ON PT.id=PO.PaymentTermId 
                            LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=PO.PurchaseLCId  
                            LEFT JOIN SCS.Currency CN ON CN.Id=PO.CurrencyId 
                            LEFT JOIN (SELECT InventoryReceiveId,SUM(Amount) Amount FROM TRN.POService GROUP BY InventoryReceiveId) POC ON POC.InventoryReceiveId=PO.Id
                            WHERE PO.PlantId='" + identity.PlantId + @"' AND PT.PaymentMode = 'LC'  AND PLCM.PurchaseLCId ='" + purchaseLCId + @"'
                    UNION
                    SELECT 
                            distinct PO.Id,REPLACE(CONVERT(CHAR(11), PO.PODate, 106),' ','-') AS PODate,PO.PartyId,
                            InvPP.StandardName ,ISNULL(PLC.OrderSpecific,PO.OrderSpecific) OrderSpecifi,ISNULL(PLC.ContractId, PO.ContractId) ContractId,PO.PurchaseLCId, CN.Code Currency,PO.CurrencyId
                            ,CONVERT(NUMERIC(10,2),POD.TransactionAmount+ISNULL(POT.TaxAmount,0)) TransactionAmount,0 LCAmount,0 BalanceAmount,0 Amount, 0 AS [check],Flag='ServicePO',PLC.LCRef,PO.DocRefNo,PO.PaymentTermId,PT.UserName PaymentTerm
                            FROM TRN.[ServicePOMaster] PO
                            INNER JOIN (SELECT SUM(Amount) TransactionAmount, ServicePOMasterId FROM [TRN].[ServicePODetail] GROUP BY ServicePOMasterId) POD ON POD.ServicePOMasterId=PO.Id
                            LEFT JOIN (SELECT InventoryReceiveId,SUM(TaxAmount) TaxAmount FROM TRN.PurchaseOrderTax GROUP BY InventoryReceiveId) POT ON POT.InventoryReceiveId=PO.ID
                            LEFT JOIN [HKP].[Party] AS InvPP ON PO.PartyId=InvPP.Id
                            LEFT JOIN [MST].[PaymentTerm] PT ON PT.id=PO.PaymentTermId 
                            LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=PO.PurchaseLCId 
                            LEFT JOIN SCS.Currency CN ON CN.Id=PO.CurrencyId 
                            WHERE PO.PlantId='" + identity.PlantId + @"' AND PT.PaymentMode = 'LC'  AND PO.PurchaseLCId='" + purchaseLCId + @"'
                    UNION
                    SELECT 
                            distinct PO.Id,REPLACE(CONVERT(CHAR(11), PO.PODate, 106),' ','-') AS PODate,PO.PartyId,
                            InvPP.StandardName ,ISNULL(PLC.OrderSpecific,PO.OrderSpecific) OrderSpecifi,ISNULL(PLC.ContractId, PO.ContractId) ContractId,PO.PurchaseLCId, CN.Code Currency,PO.CurrencyId
                            ,CONVERT(NUMERIC(10,2),POD.TransactionAmount+ISNULL(POT.TaxAmount,0)) TransactionAmount,0 LCAmount,0 BalanceAmount,0 Amount, 0 AS [check],Flag='OutSourcePO',PLC.LCRef,PO.DocRefNo,PO.PaymentTermId,PT.UserName PaymentTerm
                            FROM [dbo].[OSTransformationPO] PO
                            INNER JOIN (SELECT SUM(ISNULL(TransactionAmount,0)) TransactionAmount, OSTransformationPOId FROM [dbo].[OSTransformationPODetail] GROUP BY OSTransformationPOId) POD ON POD.OSTransformationPOId=PO.Id
                            LEFT JOIN (SELECT InventoryReceiveId,SUM(TaxAmount) TaxAmount FROM TRN.PurchaseOrderTax GROUP BY InventoryReceiveId) POT ON POT.InventoryReceiveId=PO.ID          
                            LEFT JOIN [HKP].[Party] AS InvPP ON PO.PartyId=InvPP.Id
                            LEFT JOIN [MST].[PaymentTerm] PT ON PT.id=PO.PaymentTermId 
                            LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=PO.PurchaseLCId 
                            LEFT JOIN SCS.Currency CN ON CN.Id=PO.CurrencyId 
                            WHERE PO.PlantId='" + identity.PlantId + @"' AND PT.PaymentMode = 'LC'  AND PO.PurchaseLCId='"+ purchaseLCId + "'";

                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetPurchaseLCChargesTax(string Id)
        {
            try
            {
                var Sql = @"SELECT PT.Id,PT.PurchaseLCId,PT.PurchaseLCChargesId,PT.TaxCategoryId
                        ,PT.[Percentage],PT.HSNCodeId,PT.TaxAmount,TC.UserName TaxCategory 
                        FROM trn.PurchaseLCTax PT
                        LEFT JOIN MST.TaxCategory TC ON TC.Id=PT.TaxCategoryId
                        WHERE PT.PurchaseLCId='" + Id + "'";
                return Json(_sqlRepository.GetDataCollection(Sql), JsonRequestBehavior.AllowGet);
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
                            Where PR.Id='"+ vendorId + "'";
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
                            Where P.Id='"+ identity.PlantId + "'";
                return Json(_sqlRepository.GetDataCollection(Sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetPortByPlantCountry(string CountryId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var Sql = @"Select P.Id [Value], P.UserName [Text] from MST.[Port] P
                            LEFT JOIN [MST].[CompanyGroupPort] CP ON CP.PortId=P.Id
                            WHERE P.CountryId='"+ CountryId + "'";
                return Json(_sqlRepository.GetDataCollection(Sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion



    }
    public class OSTransformationPO
    {

        #region Scalar Properties

        public string Id { get; set; }
        public string DocRefNo { get; set; }
        public DateTime DocDate { get; set; }
        public string FixedAssetOrInventory { get; set; }
        public bool PODepended { get; set; }
        public DateTime? BaseOnDueDate { get; set; }
        public int BaseNoOfDays { get; set; }
        public DateTime? MatureDate { get; set; }
        public bool IsNonCreditable { get; set; }
        public string Status { get; set; }
        public string InvoicingByAddress { get; set; }
        public string DeliveryByAddress { get; set; }
        public DateTime PODate { get; set; }
        public decimal ToCurrencyRate { get; set; }
        public bool IsTaxApplicable { get; set; }
        public bool IsApproved { get; set; }
        public bool IsPaymentHold { get; set; }
        public string PartyType { get; set; }
        public string POType { get; set; }
        public string MasterOrderId { get; set; }

        public string DeliveryInstruction { get; set; }

        public string SpecialInstruction { get; set; }
        public string CheckedBy { get; set; }

        public string ApprovedBy { get; set; }
        public string CheckedByStatus { get; set; }

        public string ApprovedByStatus { get; set; }

        public string RequisitionId { get; set; }

        public string CheckedHoldRejectReason { get; set; }

        public string ApprovedHoldRejectReason { get; set; }

        public string FileName { get; set; }


        public string ContractId { get; set; }

        public string PurchaseLCId { get; set; }
        public string OrderSpecific { get; set; }

        #endregion Scalar Properties

        #region Audit Properties
        [NeverUpdate]
        public string AddedBy { get; set; }
       
        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        [NeverUpdate]
        public string AddedFromIP { get; set; }

        public string UpdatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties

        #region Navigation Properties

        [NeverUpdate, XmlIgnore]
        public string CompanyGroupId { get; set; }

        [NeverUpdate]
        public string CompanyId { get; set; }

        [NeverUpdate]
        public string EntityId { get; set; }

        [NeverUpdate]
        public string PlantId { get; set; }

        public string PartyId { get; set; }

        public string MaterialStorageId { get; set; }

        public string CurrencyId { get; set; }

        public string PaymentTermId { get; set; }

        public string BaseCurrencyId { get; set; }

        public string InvoicingPartyPlantId { get; set; }

        public string DeliveryPartyPlantId { get; set; }

        public string EmployeeId { get; set; }
        public bool IsClosed { get; set; }


        #endregion Navigation Properties
    }

}
