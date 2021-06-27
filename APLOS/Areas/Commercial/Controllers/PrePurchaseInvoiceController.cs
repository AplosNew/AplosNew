#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Payrolls;
using Library.Service.Helpers;
using Library.Service.Payrolls;
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
    public class PrePurchaseInvoiceController : BaseController
    {
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        public PrePurchaseInvoiceController(IUnitOfWork U, ISqlRepository R)
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
            string sql = @"SELECT  P.[Id]
                          ,P.[InvoiceNo]
                          ,FORMAT(P.InvoiceDate,'dd-MMM-yyyy') InvoiceDate
                          ,P.InvoiceAttachment
                          ,P.[PurchaseLCId]
                          ,P.[BLAWBNo]
                          ,FORMAT(P.BLAWBDate,'dd-MMM-yyyy') BLAWBDate
                          ,P.BLAWBAttachment
                          ,P.[ShipmentModeId]
                          ,P.[PackingDescription]
                          ,P.[MaterialDescription]
                          ,P.[VesselDetail]
                          ,P.[VesselSalesDetail]
                          ,P.[VesselAttachment]
                          ,P.[VesselTrackingNo]
                          ,P.[ETA]
                          ,P.[PackingListAttachment]
                          ,P.[NegotiableDocDispatchNo]
                          ,FORMAT(P.NegotiableDocDispatchDate,'dd-MMM-yyyy') NegotiableDocDispatchDate
                          ,P.[CNFAgentDocument]
                          ,P.[CNFAgent]
                          ,P.[TransportAgent]
                          ,P.[TransportDcument]
                          ,P.[TransportDocumentDetail]
                          ,P.[TransportDocumentAttachment]
                          ,P.[PortOfArrival]
                          ,P.[VechileNo]
                          ,P.[AddedBy]
                          ,FORMAT(P.AddedDate,'dd-MMM-yyyy') AddedDate
                          ,P.[AddedFromIP]
                          ,P.[UpdatedBy]
                          ,FORMAT(P.UpdatedDate,'dd-MMM-yyyy') UpdatedDate
                          ,P.[UpdatedFromIP]
						  ,C.UserName CNFAgentName
						  ,T.UserName TransportAgentName
                          ,PT.UserName [Port]
						  ,SP.UserName ShipMode ,V.UserName Vendor, CRN.Code Currency
						  ,PLC.LCDate,PLC.Amount,PLC.LCRef PurchaseLCNo, P.CustomsEntryNo,P.PassBookNo
                      FROM [dbo].[PrePurchaseInvoice] P
					  LEFT JOIN HKP.Party C ON C.Id=P.CNFAgent
					  LEFT JOIN HKP.Party T ON T.Id=P.TransportAgent
					  LEFT JOIN MST.[Port] PT ON PT.Id=P.PortOfArrival
					  LEFT JOIN [MST].[ShipMode] SP ON SP.Id=P.ShipmentModeId
                      LEFT JOIN [dbo].[PurchaseLC] PLC ON PLC.Id=P.PurchaseLCId
                      LEFT JOIN [dbo].[Contract] CN ON CN.Id=PLC.ContractId
                      LEFT JOIN [HKP].[Party] V On V.Id=PLC.VendorId
			          LEFT JOIN SCS.Currency CRN ON CRN.Id=PLC.CurrencyId";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetPortByPlantCbo()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"select Id,UserName from MST.[Port] Where CountryId=(
                        SELECT A.CountryId FROM ORG.Plant P
                        LEFT JOIN MST.AddressMaster A ON A.Id=P.AddressMasterId Where P.Id='" + identity.PlantId + "')";
            return Json(_sqlRepository.GetCombo(sql, "Id", "UserName"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetLCList()
        {
            string sql = @"SELECT PLC.Id
	                ,PLC.ContractId
	                ,PLC.BenificiaryBankId
	                ,B.StandardName BenificiaryBank
	                ,PLC.OpeningBankMasterId
	                ,BM.AccountTitle
	                ,PLC.LeinBankId
	                ,L.StandardName LeinBank
	                ,PLC.OrderSpecific
	                ,FORMAT(PLC.LCDate,'dd-MMM-yyyy') LCDate
	                ,FORMAT(PLC.ExpiryDate,'dd-MMM-yyyy') ExpiryDate
	                ,PLC.Amount
	                ,PLC.FinalDestinationId
	                ,PLc.PortOfLandingId
	                ,prt.UserName PortOfLanding
	                ,PLC.Type
	                ,PLC.Tenure,PLC.VendorId
	                ,PLC.CurrencyId, CN.Code Currency,P.UserName Vendor,PLC.LCRef,C.ContractNo
                FROM [dbo].[PurchaseLC] PLC
                LEFT JOIN [dbo].[Contract] C ON C.Id=PLC.ContractId
                LEFT JOIN [HKP].[Party] P On P.Id=PLC.VendorId
                LEFT JOIN [HKP].[Bank] B On B.Id=PLC.BenificiaryBankId
                LEFT JOIN [HKP].[Bank] L On L.Id=PLC.LeinBankId
                LEFT JOIN [MST].[BankMaster] BM On BM.Id=PLC.OpeningBankMasterId ANd BM.AccountType='HouseBank'
                LEFT JOIN [MST].[Port] Prt ON prt.id=PLC.PortOfLandingId
                LEFT JOIN SCS.Currency CN ON CN.Id=PLC.CurrencyId
                WHERE PLC.IsClose=0 ";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(PrePurchaseInvoice entity)
        {

            try
            {
                SaveData(entity, out string masterId);
                return Json(new { Id = masterId, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, ex.Message });
            }

        }
        
        [HttpPost, Authorize]
        public JsonResult CreateAttachment(FormCollection form, HttpPostedFileBase[] file)
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                var model = JsonConvert.DeserializeObject<PrePurchaseInvoice>(form["data"], settings);

                var directory = "";
                if (model.Flag == "Invoice")
                {
                    directory = ResourcesPathReader.GetPrePurchaseInvoiceDocPath();
                }
                else if (model.Flag == "BLAWB")
                {
                    directory = ResourcesPathReader.GetPrePurchaseBLAWBDocPath();
                }
                else if (model.Flag == "Vessel")
                {
                    directory = ResourcesPathReader.GetPrePurchaseVesselDocPath();
                }
                else if (model.Flag == "Packing")
                {
                    directory = ResourcesPathReader.GetPrePurchasePackingDocPath();
                }
                else if (model.Flag == "Transport")
                {
                    directory = ResourcesPathReader.GetPrePurchaseTransportDocPath();
                }
                else if (model.Flag == "CNF")
                {
                    directory = ResourcesPathReader.GetPrePurchaseCNFDocPath();
                }

                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                string path = Path.Combine(directory);
                string _id = "";
                var fileName = "";
                Dictionary<string, object> filedata = null;
                if (model.Flag == "Invoice")
                {
                    filedata = GetInvoiceAttachmentFile(model.Id);
                }
                else if (model.Flag == "BLAWB")
                {
                    filedata = GetBLAWBAttachmentFile(model.Id);
                }
                else if (model.Flag == "Vessel")
                {
                    filedata = GetVesselAttachmentFile(model.Id);
                }
                else if (model.Flag == "Packing")
                {
                    filedata = GetPackingAttachmentFile(model.Id);
                }
                else if (model.Flag == "Transport")
                {
                    filedata = GetTransportAttachmentFile(model.Id);
                }
                else if (model.Flag == "CNF")
                {
                    filedata = GetCNFAttachmentFile(model.Id);
                }

                if (file.IsNotNull())
                {
                    for (int i = 0; i < file.Length; i++)
                    {
                        ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
                    }
                }
                if (model.Flag == "Invoice")
                {
                    if (filedata.Count > 0)
                    {
                        if (
                            !string.IsNullOrEmpty(filedata["InvoiceAttachment"].ToString()))
                            fileName = filedata["InvoiceAttachment"].ToString();

                        if (fileName != model.InvoiceAttachment)
                            if (System.IO.File.Exists(path + model.Id + Path.GetExtension(fileName)))
                                System.IO.File.Delete(path + model.Id + Path.GetExtension(fileName));
                    }
                }
                else if (model.Flag == "BLAWB")
                {
                    if (filedata.Count > 0)
                    {
                        if (
                            !string.IsNullOrEmpty(filedata["BLAWBAttachment"].ToString()))
                            fileName = filedata["BLAWBAttachment"].ToString();

                        if (fileName != model.BLAWBAttachment)
                            if (System.IO.File.Exists(path + model.Id + Path.GetExtension(fileName)))
                                System.IO.File.Delete(path + model.Id + Path.GetExtension(fileName));
                    }
                }
                else if (model.Flag == "Vessel")
                {
                    if (filedata.Count > 0)
                    {
                        if (
                            !string.IsNullOrEmpty(filedata["VesselAttachment"].ToString()))
                            fileName = filedata["VesselAttachment"].ToString();

                        if (fileName != model.VesselAttachment)
                            if (System.IO.File.Exists(path + model.Id + Path.GetExtension(fileName)))
                                System.IO.File.Delete(path + model.Id + Path.GetExtension(fileName));
                    }
                }
                else if (model.Flag == "Packing")
                {
                    if (filedata.Count > 0)
                    {
                        if (
                            !string.IsNullOrEmpty(filedata["PackingListAttachment"].ToString()))
                            fileName = filedata["PackingListAttachment"].ToString();

                        if (fileName != model.PackingListAttachment)
                            if (System.IO.File.Exists(path + model.Id + Path.GetExtension(fileName)))
                                System.IO.File.Delete(path + model.Id + Path.GetExtension(fileName));
                    }
                }
                else if (model.Flag == "Transport")
                {
                    if (filedata.Count > 0)
                    {
                        if (
                            !string.IsNullOrEmpty(filedata["TransportDocumentAttachment"].ToString()))
                            fileName = filedata["TransportDocumentAttachment"].ToString();

                        if (fileName != model.TransportDocumentAttachment)
                            if (System.IO.File.Exists(path + model.Id + Path.GetExtension(fileName)))
                                System.IO.File.Delete(path + model.Id + Path.GetExtension(fileName));
                    }
                }
                else if (model.Flag == "CNF")
                {
                    if (filedata.Count > 0)
                    {
                        if (
                            !string.IsNullOrEmpty(filedata["CNFAgentDocument"].ToString()))
                            fileName = filedata["CNFAgentDocument"].ToString();

                        if (fileName != model.CNFAgentDocument)
                            if (System.IO.File.Exists(path + model.Id + Path.GetExtension(fileName)))
                                System.IO.File.Delete(path + model.Id + Path.GetExtension(fileName));
                    }
                }

                SaveData(model, out string masterId);

                if (model.Flag == "Invoice")
                {
                    if (file.IsNotNull())
                    {
                        foreach (var item in file)
                        {
                            if (item != null)
                            {
                                if (System.IO.File.Exists(path + item.FileName))
                                    System.IO.File.Delete(path + model.InvoiceAttachment);
                                item.SaveAs(path + model.InvoiceAttachment);
                            }
                        }
                    }
                }
                else if (model.Flag == "BLAWB")
                {
                    if (file.IsNotNull())
                    {
                        foreach (var item in file)
                        {
                            if (item != null)
                            {
                                if (System.IO.File.Exists(path + item.FileName))
                                    System.IO.File.Delete(path + model.VesselAttachment);
                                item.SaveAs(path + model.VesselAttachment);
                            }
                        }
                    }
                }
                else if (model.Flag == "Packing")
                {
                    if (file.IsNotNull())
                    {
                        foreach (var item in file)
                        {
                            if (item != null)
                            {
                                if (System.IO.File.Exists(path + item.FileName))
                                    System.IO.File.Delete(path + model.PackingListAttachment);
                                item.SaveAs(path + model.PackingListAttachment);
                            }
                        }
                    }
                }
                else if (model.Flag == "Transport")
                {
                    if (file.IsNotNull())
                    {
                        foreach (var item in file)
                        {
                            if (item != null)
                            {
                                if (System.IO.File.Exists(path + item.FileName))
                                    System.IO.File.Delete(path + model.TransportDocumentAttachment);
                                item.SaveAs(path + model.TransportDocumentAttachment);
                            }
                        }
                    }
                }
                else if (model.Flag == "CNF")
                {
                    if (file.IsNotNull())
                    {
                        foreach (var item in file)
                        {
                            if (item != null)
                            {
                                if (System.IO.File.Exists(path + item.FileName))
                                    System.IO.File.Delete(path + model.CNFAgentDocument);
                                item.SaveAs(path + model.CNFAgentDocument);
                            }
                        }
                    }
                }
                else if (model.Flag == "Vessel")
                {
                    if (file.IsNotNull())
                    {
                        foreach (var item in file)
                        {
                            if (item != null)
                            {
                                if (System.IO.File.Exists(path + item.FileName))
                                    System.IO.File.Delete(path + model.VesselAttachment);
                                item.SaveAs(path + model.VesselAttachment);
                            }
                        }
                    }
                }
                return Json(new { Id = masterId, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }

        public Dictionary<string, object> GetInvoiceAttachmentFile(string Id)
        {
            try
            {
                var sql = @"SELECT InvoiceAttachment FROM [dbo].[PrePurchaseInvoice]  WHERE Id='" + Id + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Dictionary<string, object> GetBLAWBAttachmentFile(string Id)
        {
            try
            {
                var sql = @"SELECT BLAWBAttachment FROM [dbo].[PrePurchaseInvoice]  WHERE Id='" + Id + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Dictionary<string, object> GetVesselAttachmentFile(string Id)
        {
            try
            {
                var sql = @"SELECT VesselAttachment FROM [dbo].[PrePurchaseInvoice]  WHERE Id='" + Id + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Dictionary<string, object> GetPackingAttachmentFile(string Id)
        {
            try
            {
                var sql = @"SELECT PackingListAttachment FROM [dbo].[PrePurchaseInvoice]  WHERE Id='" + Id + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Dictionary<string, object> GetCNFAttachmentFile(string Id)
        {
            try
            {
                var sql = @"SELECT CNFAgentDocument FROM [dbo].[PrePurchaseInvoice]  WHERE Id='" + Id + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Dictionary<string, object> GetTransportAttachmentFile(string Id)
        {
            try
            {
                var sql = @"SELECT TransportDocumentAttachment FROM [dbo].[PrePurchaseInvoice]  WHERE Id='" + Id + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(PrePurchaseInvoice), out sID);
            return sID;
        }


        private void SaveData(PrePurchaseInvoice data, out string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            ConnectionManager.DAL.ConManager objCon;
            try
            {

                string sql = "SELECT * FROM [dbo].[PrePurchaseInvoice] WHERE Id='" + data.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = GetPK();
                    dr["InvoiceNo"] = data.InvoiceNo;
                    dr["InvoiceDate"] = data.InvoiceDate;
                    dr["InvoiceAttachment"] = data.InvoiceAttachment;
                    dr["PurchaseLCId"] = data.PurchaseLCId;
                    dr["CustomsEntryNo"] = data.CustomsEntryNo;
                    dr["PassBookNo"] = data.PassBookNo;
                    dr["BLAWBNo"] = data.BLAWBNo;

                    if (String.IsNullOrEmpty(data.BLAWBDate.ToString()))
                    {
                        dr["BLAWBDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["BLAWBDate"] = data.BLAWBDate;
                    }

                    dr["BLAWBAttachment"] = data.BLAWBAttachment;
                    dr["ShipmentModeId"] = data.ShipmentModeId;
                    dr["PackingDescription"] = data.PackingDescription;
                    dr["MaterialDescription"] = data.MaterialDescription;
                    dr["VesselDetail"] = data.VesselDetail;
                    dr["VesselSalesDetail"] = data.VesselSalesDetail;
                    dr["VesselAttachment"] = data.VesselAttachment;
                    dr["VesselTrackingNo"] = data.VesselTrackingNo;

                    if (String.IsNullOrEmpty(data.ETA.ToString()))
                    {
                        dr["ETA"] = DBNull.Value;
                    }
                    else
                    {
                        dr["ETA"] = data.ETA;
                    }

                    dr["PackingListAttachment"] = data.PackingListAttachment;
                    dr["NegotiableDocDispatchNo"] = data.NegotiableDocDispatchNo;

                    if (String.IsNullOrEmpty(data.NegotiableDocDispatchDate.ToString()))
                    {
                        dr["NegotiableDocDispatchDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["NegotiableDocDispatchDate"] = data.NegotiableDocDispatchDate;
                    }

                    dr["CNFAgent"] = data.CNFAgent;
                    dr["CNFAgentDocument"] = data.CNFAgentDocument;
                    dr["TransportAgent"] = data.TransportAgent;
                    dr["TransportDcument"] = data.TransportDcument;
                    dr["TransportDocumentAttachment"] = data.TransportDocumentAttachment;
                    dr["TransportDocumentDetail"] = data.TransportDocumentDetail;
                    dr["PortOfArrival"] = data.PortOfArrival;
                    dr["VechileNo"] = data.VechileNo;

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

                    dr["InvoiceNo"] = data.InvoiceNo;
                    dr["InvoiceDate"] = data.InvoiceDate;
                    dr["InvoiceAttachment"] = data.InvoiceAttachment;
                    dr["PurchaseLCId"] = data.PurchaseLCId;
                    dr["BLAWBNo"] = data.BLAWBNo;
                    dr["CustomsEntryNo"] = data.CustomsEntryNo;
                    dr["PassBookNo"] = data.PassBookNo;
                    if (String.IsNullOrEmpty(data.BLAWBDate.ToString()))
                    {
                        dr["BLAWBDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["BLAWBDate"] = data.BLAWBDate;
                    }

                    dr["BLAWBAttachment"] = data.BLAWBAttachment;
                    dr["ShipmentModeId"] = data.ShipmentModeId;
                    dr["PackingDescription"] = data.PackingDescription;
                    dr["MaterialDescription"] = data.MaterialDescription;
                    dr["VesselDetail"] = data.VesselDetail;
                    dr["VesselSalesDetail"] = data.VesselSalesDetail;
                    dr["VesselAttachment"] = data.VesselAttachment;
                    dr["VesselTrackingNo"] = data.VesselTrackingNo;

                    if (String.IsNullOrEmpty(data.ETA.ToString()))
                    {
                        dr["ETA"] = DBNull.Value;
                    }
                    else
                    {
                        dr["ETA"] = data.ETA;
                    }

                    dr["PackingListAttachment"] = data.PackingListAttachment;
                    dr["NegotiableDocDispatchNo"] = data.NegotiableDocDispatchNo;

                    if (String.IsNullOrEmpty(data.NegotiableDocDispatchDate.ToString()))
                    {
                        dr["NegotiableDocDispatchDate"] = DBNull.Value;
                    }
                    else
                    {
                        dr["NegotiableDocDispatchDate"] = data.NegotiableDocDispatchDate;
                    }

                    dr["CNFAgent"] = data.CNFAgent;
                    dr["CNFAgentDocument"] = data.CNFAgentDocument;
                    dr["TransportAgent"] = data.TransportAgent;
                    dr["TransportDcument"] = data.TransportDcument;
                    dr["TransportDocumentAttachment"] = data.TransportDocumentAttachment;
                    dr["TransportDocumentDetail"] = data.TransportDocumentDetail;
                    dr["PortOfArrival"] = data.PortOfArrival;
                    dr["VechileNo"] = data.VechileNo;


                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();

                    dr.EndEdit();
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
            }
            catch (Exception ex)
            {

                throw (ex);
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
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM PrePurchaseInvoice WHERE Id = '" + Id + "'";
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

        #endregion
    }

    public class PrePurchaseInvoice : BaseModel
    {
        public string Id { get; set; }
        public string InvoiceNo { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string InvoiceAttachment { get; set; }
        public string PurchaseLCId { get; set; }
        public string BLAWBNo { get; set; }
        public DateTime? BLAWBDate { get; set; }
        public string BLAWBAttachment { get; set; }
        public string PassBookNo { get; set; }
        public string CustomsEntryNo { get; set; }
        public string ShipmentModeId { get; set; }
        public string PackingDescription { get; set; }
        public string MaterialDescription { get; set; }
        public string NegotiableDocDispatchNo { get; set; }
        public DateTime? NegotiableDocDispatchDate { get; set; }
        public string VesselDetail { get; set; }
        public string VesselTrackingNo { get; set; }
        public DateTime? ETA { get; set; }
        public string PackingListAttachment { get; set; }
        public string CNFAgent { get; set; }
        public string TransportAgent { get; set; }
        public string PortOfArrival { get; set; }
        public string VechileNo { get; set; }
        public string CNFAgentDocument { get; set; }
        public string TransportDcument { get; set; }
        public string TransportDocumentDetail { get; set; }
        public string TransportDocumentAttachment { get; set; }
        public string VesselSalesDetail { get; set; }
        public string VesselAttachment { get; set; }

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
        public string Flag { get; set; }

    }
}