#region Using
using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.General.Commercial;
using Library.Model.OrderManagements;
using Library.Model.Payrolls;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Payrolls;
using Newtonsoft.Json;
using OTSBD;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Mvc;
#endregion

namespace Aplos.Areas.Commercial.Controllers
{
    public class ContractController : BaseController
    {
        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        clsContract clsCon = new clsContract();
        public ContractController(IUnitOfWork U, ISqlRepository R)
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
        public ActionResult Aplos1()
        {
            return View();
        }
        public ActionResult LCPendingReport()
        {
            return View();
        }
        public ActionResult MasterLC()
        {
            return View();
        }
        public ActionResult MasterLCAmendment()
        {
            return View();
        }
        public ActionResult ContractSummary()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpPost, Authorize]
        public JsonResult GetCompanyPartyDataListNew(string column, string value, string partyType)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var res = clsCon.GetCompanyPartyListNew(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, column, value, partyType);
            var jsondata = Json(res, JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            JsonResult json = Json(clsCon.GetContractList(column, value, identity.PlantId), JsonRequestBehavior.AllowGet);
            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet, Authorize]
        public ActionResult GetTermsAndConditionsList()
        {
            string sql = @"SELECT TC.Id TermsAndConditionsId,Flag=CAST(0 AS bit),IsVarified=CAST(0 AS bit),TC.Sequence,TC.Code,TC.ShortName,TC.StandardName,TC.UserName,TC.Description,TG.GroupName [Group] FROM [HKP].[TermsAndConditions] TC
LEFT JOIN dbo.TermsandConditionGroup TG ON TG.Id=TC.GroupId Where [Type]='" + TermsAndConditionsEnum.Contract + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetTermsAndConditionsDataList(string contractId)
        {
            string sql = @"SELECT distinct CT.TermsAndConditionsId,Flag=CAST(0 AS bit),IsVarified=CAST(0 AS bit),TC.Sequence,TC.Code,TC.ShortName,TC.StandardName,TC.UserName,TC.Description,'' Remarks  
FROM [dbo].[ContractTermsAndConditions] CT
LEFT JOIN HKP.TermsAndConditions TC ON TC.Id=CT.TermsAndConditionsId
WHERE CT.ContractId " + contractId+"";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetContractTermsAndConditionsList(string ContractId)
        {
            return Json(clsCon.GetContractTermsAndConditionsList(ContractId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetContractDetail(string partyId, string contractId)
        {
            return Json(clsCon.GetContractDetail(partyId, contractId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetContractListByCustomer(string customerId)
        {

            return Json(clsCon.GetContractListByCustomer(customerId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetSavedContractList(string masterLCId)
        {
            return Json(clsCon.GetSavedContractList(masterLCId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Contract model, string selectedSalesOrderList, List<Dictionary<string, object>> funds)
        {
            try
            {
                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                List<MasterOrderItemModel> masterOrderItem = JsonConvert.DeserializeObject<List<MasterOrderItemModel>>(selectedSalesOrderList, settings);

                SaveData(model, masterOrderItem, out string contractId, funds);


                return Json(new { Contract = model, Id = contractId, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }



        [HttpPost]
        public JsonResult UpdateContract(Contract model)
        {
            try
            {
                UpdateContractData(model);

                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }

        [HttpPost, Authorize]
        public JsonResult CreateMasterLC(MasterLC entity, Contract contract)
        {
            try
            {
                SaveMasterLCData(entity, out string masterId);

                contract.MasterLCId = masterId;
                UpdateContractData(contract);
                return Json(new { Id = masterId, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }

        [HttpPost]
        public JsonResult SaveMasterLC(MasterLC entity)
        {
            try
            {
                SaveMasterLCData(entity, out string masterId);

                return Json(new { Id = masterId, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }

        private string GetContractFundPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(ContractFund), out sID);
            return sID;
        }

        private void SaveFundUtilizationData(List<Dictionary<string, object>> data, string contractId, out DataSet dsMaster)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                dsMaster = null;
                if (data != null)
                {

                    ConnectionManager.DAL.ConManager objCon;
                    //DataSet dsMaster;
                    foreach (var item in data)
                    {
                        string sql = "SELECT * FROM dbo.ContractFund WHERE FundUtilization='" + item["FundUtilization"] + "' AND ContractId='" + contractId + "'";
                        objCon = new ConnectionManager.DAL.ConManager("1");
                        objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                        DataView dv = new DataView(dsMaster.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = GetContractFundPK();
                            item[" ContractId"] = contractId;

                            AddNewRow(dsMaster.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost]
        public JsonResult SaveMasterLCAmendment(MasterLCAmendment entity)
        {
            try
            {
                SaveMasterLCAmendmentData(entity, out string version, out string masterId);

                return Json(new { Id = masterId, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }
        private void SaveMasterLCAmendmentData(MasterLCAmendment data, out string Version, out string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster, dsSeq;
            string contractId = string.Empty;
            string id = string.Empty;
            try
            {

                GetAmandmentAutoSequence(data.Id, out dsSeq);
                decimal seq = Convert.ToDecimal(dsSeq.Tables[0].Rows[0]["Version"].ToString());


                string _sql = "SELECT * FROM [dbo].[MasterLC] WHERE Id='" + data.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(_sql, out dsMaster, false, "1");

                // keep data in Version Table
                string sql = "SELECT * FROM [dbo].[MasterLCHistory] WHERE Id='" + id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsSeq, false, "1");

                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    if (dsSeq.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsSeq.Tables[0].NewRow();

                        dr["Id"] = "MLCA" + GetAmandmentVersionPK();
                        dr["MasterLCId"] = data.Id;
                        dr["Version"] = dsMaster.Tables[0].Rows[0]["Version"];
                        dr["CustomerId"] = dsMaster.Tables[0].Rows[0]["CustomerId"];
                        dr["IsClose"] = dsMaster.Tables[0].Rows[0]["IsClose"];
                        dr["BenificiaryBankId"] = dsMaster.Tables[0].Rows[0]["BenificiaryBankId"];
                        dr["OpeningBank"] = dsMaster.Tables[0].Rows[0]["OpeningBank"];
                        dr["OpeningDescription"] = dsMaster.Tables[0].Rows[0]["OpeningDescription"];
                        dr["LeinBank"] = dsMaster.Tables[0].Rows[0]["LeinBank"];
                        dr["LeinDescription"] = dsMaster.Tables[0].Rows[0]["LeinDescription"];
                        dr["LCRef"] = dsMaster.Tables[0].Rows[0]["LCRef"];
                        dr["LCDate"] = dsMaster.Tables[0].Rows[0]["LCDate"];
                        dr["ExpiryDate"] = dsMaster.Tables[0].Rows[0]["ExpiryDate"];
                        dr["AmendmentDate"] = dsMaster.Tables[0].Rows[0]["AmendmentDate"] != System.DBNull.Value ? dsMaster.Tables[0].Rows[0]["AmendmentDate"] : System.DBNull.Value;
                        dr["Amount"] = dsMaster.Tables[0].Rows[0]["Amount"];
                        dr["Type"] = dsMaster.Tables[0].Rows[0]["Type"];
                        dr["Tenure"] = dsMaster.Tables[0].Rows[0]["Tenure"];
                        dr["FinalDestinationId"] = dsMaster.Tables[0].Rows[0]["FinalDestinationId"];
                        dr["PortOfLandingId"] = dsMaster.Tables[0].Rows[0]["PortOfLandingId"];
                        dr["CurrencyId"] = dsMaster.Tables[0].Rows[0]["CurrencyId"];
                        dr["LCShipmentDate"] = dsMaster.Tables[0].Rows[0]["LCShipmentDate"];
                        dr["ShipmentModeId"] = dsMaster.Tables[0].Rows[0]["ShipmentModeId"];
                        dr["PortOfLoadingId"] = dsMaster.Tables[0].Rows[0]["PortOfLoadingId"];
                        dr["Remarks"] = dsMaster.Tables[0].Rows[0]["Remarks"];

                        dr["AddedBy"] = dsMaster.Tables[0].Rows[0]["AddedBy"];
                        dr["AddedDate"] = dsMaster.Tables[0].Rows[0]["AddedDate"];
                        dr["AddedFromIP"] = dsMaster.Tables[0].Rows[0]["AddedFromIP"];

                        dr["UpdatedBy"] = dsMaster.Tables[0].Rows[0]["UpdatedBy"];
                        dr["UpdatedDate"] = dsMaster.Tables[0].Rows[0]["UpdatedDate"];
                        dr["UpdatedFromIP"] = dsMaster.Tables[0].Rows[0]["UpdatedFromIP"];

                        dsSeq.Tables[0].Rows.Add(dr);
                    }
                }


                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = "MLC" + GetMasterLCPK();
                    dr["Version"] = seq;
                    dr["CustomerId"] = data.CustomerId;
                    dr["IsClose"] = data.IsClose;
                    dr["BenificiaryBankId"] = data.BenificiaryBankId;
                    dr["OpeningBank"] = data.OpeningBank;
                    dr["OpeningDescription"] = data.OpeningDescription;
                    dr["LeinBank"] = data.LeinBank;
                    dr["LeinDescription"] = data.LeinDescription;
                    dr["LCRef"] = data.LCRef;
                    dr["LCDate"] = data.LCDate;
                    dr["AmendmentDate"] = data.AmendmentDate == null ? System.DBNull.Value : (object)data.AmendmentDate;
                    dr["ExpiryDate"] = data.ExpiryDate;
                    dr["Amount"] = data.Amount;
                    dr["Type"] = data.Type;
                    dr["Tenure"] = data.Tenure;
                    dr["FinalDestinationId"] = data.FinalDestinationId;
                    dr["PortOfLandingId"] = data.PortOfLandingId;
                    dr["CurrencyId"] = data.CurrencyId;
                    dr["LCShipmentDate"] = data.LCShipmentDate;
                    dr["ShipmentModeId"] = data.ShipmentModeId;
                    dr["PortOfLoadingId"] = data.PortOfLoadingId;
                    dr["Remarks"] = data.Remarks;

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

                    dr["Version"] = data.Version + 1;
                    dr["CustomerId"] = data.CustomerId;
                    dr["IsClose"] = data.IsClose;
                    dr["BenificiaryBankId"] = data.BenificiaryBankId;
                    dr["OpeningBank"] = data.OpeningBank;
                    dr["OpeningDescription"] = data.OpeningDescription;
                    dr["LeinBank"] = data.LeinBank;
                    dr["LeinDescription"] = data.LeinDescription;
                    dr["LCRef"] = data.LCRef;
                    dr["LCDate"] = data.LCDate;
                    dr["ExpiryDate"] = data.ExpiryDate;
                    dr["AmendmentDate"] = data.AmendmentDate == null ? System.DBNull.Value : (object)data.AmendmentDate;
                    dr["Amount"] = data.Amount;
                    dr["Type"] = data.Type;
                    dr["Tenure"] = data.Tenure;
                    dr["FinalDestinationId"] = data.FinalDestinationId;
                    dr["PortOfLandingId"] = data.PortOfLandingId;
                    dr["CurrencyId"] = data.CurrencyId;
                    dr["LCShipmentDate"] = data.LCShipmentDate;
                    dr["ShipmentModeId"] = data.ShipmentModeId;
                    dr["PortOfLoadingId"] = data.PortOfLoadingId;
                    dr["Remarks"] = data.Remarks;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsSeq);
                masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                Version = dsMaster.Tables[0].Rows[0]["Version"].ToString();

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost]
        public JsonResult UpdateMasterLCAmendment(MasterLCAmendment entity)
        {
            try
            {
                UpdateAmendmentData(entity, out string version, out string masterId);

                return Json(new { Id = masterId, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }
        private void UpdateAmendmentData(MasterLCAmendment data, out string Version, out string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            string contId = string.Empty;
            string id = string.Empty;
            DataSet dsSeq = null;
            try
            {
                GetAutoSequence(data.Id, out dsSeq);
                decimal seq = Convert.ToDecimal(dsSeq.Tables[0].Rows[0]["Version"].ToString());

                string _sql = "SELECT * FROM [dbo].[MasterLC] WHERE Id='" + data.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(_sql, out dsMaster, false, "1");

                /// Update data in PurchaseLC Table

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = "MLC" + GetMasterLCPK();
                    dr["Version"] = seq;
                    dr["CustomerId"] = data.CustomerId;
                    dr["IsClose"] = data.IsClose;
                    dr["BenificiaryBankId"] = data.BenificiaryBankId;
                    dr["OpeningBank"] = data.OpeningBank;
                    dr["OpeningDescription"] = data.OpeningDescription;
                    dr["LeinBank"] = data.LeinBank;
                    dr["LeinDescription"] = data.LeinDescription;
                    dr["LCRef"] = data.LCRef;
                    dr["LCDate"] = data.LCDate;
                    dr["AmendmentDate"] = data.AmendmentDate == null ? System.DBNull.Value : (object)data.AmendmentDate;
                    dr["ExpiryDate"] = data.ExpiryDate;
                    dr["Amount"] = data.Amount;
                    dr["Type"] = data.Type;
                    dr["Tenure"] = data.Tenure;
                    dr["FinalDestinationId"] = data.FinalDestinationId;
                    dr["PortOfLandingId"] = data.PortOfLandingId;
                    dr["CurrencyId"] = data.CurrencyId;
                    dr["LCShipmentDate"] = data.LCShipmentDate;
                    dr["ShipmentModeId"] = data.ShipmentModeId;
                    dr["PortOfLoadingId"] = data.PortOfLoadingId;
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
                    dr["CustomerId"] = data.CustomerId;
                    dr["IsClose"] = data.IsClose;
                    dr["BenificiaryBankId"] = data.BenificiaryBankId;
                    dr["OpeningBank"] = data.OpeningBank;
                    dr["OpeningDescription"] = data.OpeningDescription;
                    dr["LeinBank"] = data.LeinBank;
                    dr["LeinDescription"] = data.LeinDescription;
                    dr["LCRef"] = data.LCRef;
                    dr["LCDate"] = data.LCDate;
                    dr["ExpiryDate"] = data.ExpiryDate;
                    dr["AmendmentDate"] = data.AmendmentDate == null ? System.DBNull.Value : (object)data.AmendmentDate;
                    dr["Amount"] = data.Amount;
                    dr["Type"] = data.Type;
                    dr["Tenure"] = data.Tenure;
                    dr["FinalDestinationId"] = data.FinalDestinationId;
                    dr["PortOfLandingId"] = data.PortOfLandingId;
                    dr["CurrencyId"] = data.CurrencyId;
                    dr["LCShipmentDate"] = data.LCShipmentDate;
                    dr["ShipmentModeId"] = data.ShipmentModeId;
                    dr["PortOfLoadingId"] = data.PortOfLoadingId;
                    dr["Remarks"] = data.Remarks;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);

                masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                Version = dsMaster.Tables[0].Rows[0]["Version"].ToString();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private string GetAmandmentVersionPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "MasterLCAmandment", out sID);
            return sID;
        }

        [HttpGet, Authorize]
        public ActionResult GetAmandmentVersionCbo(string masterLCAId)
        {
            string sql = @"SELECT Id,Version FROM MasterLC WHERE Id='" + masterLCAId + @"'
                           UNION
                           SELECT MasterLCId Id,Version FROM MasterLCHistory where MasterLCId='" + masterLCAId + "'";
            return Json(_sqlRepository.GetCombo(sql, "Id", "Version"), JsonRequestBehavior.AllowGet);
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


        [HttpGet, Authorize]
        public ActionResult GetMasterOrderList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(clsCon.GetMasterOrderList(identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
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

        [HttpGet, Authorize]
        public ActionResult GetMasterOrderListbyContract(string contractId)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(clsCon.GetMasterOrderListbyContract(contractId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }



        }

        [HttpGet, Authorize]
        public ActionResult GetMasterOrderListbyCustomer(string customerId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(clsCon.GetMasterOrderListbyCustomer(identity.CompanyId, identity.PlantId, customerId), JsonRequestBehavior.AllowGet);
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
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(Contract), out sID);
            return sID;
        }
        private string GetMasterLCPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(MasterLC), out sID);
            return sID;

        }

        public void RemoveMOICon(string Id)
        {
            string strUSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strUSQL = "update TRN.SalesOrder set ContractId=NULL Where ContractId='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strUSQL, true, "1");
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
                    throw exx;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function

        [HttpPost, Authorize]
        public ActionResult DeleteSO(string id)
        {
            DeleteSOData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }


        public void DeleteSOData(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "UPDATE TRN.SalesOrder set ContractId=NULL Where Id='" + id + "'";

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

        private void SaveData(Contract data, List<MasterOrderItemModel> masterOrderItem, out string contractId, List<Dictionary<string, object>> funds)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster, dsMasterOrder, dsChild;
            string contId = string.Empty;
            string id = string.Empty;
            try
            {
                RemoveMOICon(data.Id);
                string sql = "SELECT * FROM [dbo].[Contract] WHERE Id='" + data.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = "C" + GetPK();
                    dr["PlantId"] = identity.PlantId;
                    dr["CustomerId"] = data.CustomerId;
                    dr["Description"] = data.Description;
                    dr["Remarks"] = data.Remarks;
                    dr["IsLC"] = data.IsLC;
                    dr["ContractNo"] = data.ContractNo;
                    dr["Amount"] = data.Amount;
                    dr["TotalQty"] = data.TotalQty;
                    dr["UDNo"] = data.UDNo;
                    dr["FileNo"] = data.FileNo;
                    if (data.UDDate == null)
                        dr["UDDate"] = DBNull.Value;
                    else
                        dr["UDDate"] = data.UDDate;
                    if (data.ContractDate == null)
                        dr["ContractDate"] = DBNull.Value;
                    else
                        dr["ContractDate"] = data.ContractDate;

                    dr["SOQty"] = data.SOQty;

                    dr["IsPrint"] = data.IsPrint;
                    dr["IsMarketingCommisssionApplicable"] = data.IsMarketingCommisssionApplicable;
                    dr["MarketingCommisssionId"] = data.MarketingCommisssionId;
                    dr["IsBusinessDevelopmentChargesApplicable"] = data.IsBusinessDevelopmentChargesApplicable;
                    dr["BusinessDevelopmentCharge"] = data.BusinessDevelopmentCharge;
                    dr["BusinessDevelopmentChargeValue"] = data.BusinessDevelopmentChargeValue;
                    dr["MarketingCommisssionCharge"] = data.MarketingCommisssionCharge;
                    dr["MarketingCommisssionValue"] = data.MarketingCommisssionValue;
                    dr["InvoicingPartyPlantId"] = data.InvoicingPartyPlantId;
                    dr["DeliveryPartyPlantId"] = data.DeliveryPartyPlantId;
                    dr["InvoicingByAddress"] = data.InvoicingByAddress;
                    dr["DeliveryByAddress"] = data.DeliveryByAddress;
                    dr["BankId"] = data.BankId;
                    dr["LCRequiredDaysfromShipment"] = data.LCRequiredDaysfromShipment;

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

                    dr["PlantId"] = identity.PlantId;
                    dr["CustomerId"] = data.CustomerId;
                    dr["Description"] = data.Description;
                    dr["Remarks"] = data.Remarks;
                    dr["IsLC"] = data.IsLC;
                    dr["ContractNo"] = data.ContractNo;
                    dr["Amount"] = data.Amount;
                    dr["TotalQty"] = data.TotalQty;
                    dr["SOQty"] = data.SOQty;
                    dr["UDNo"] = data.UDNo;
                    dr["FileNo"] = data.FileNo;
                    if (data.UDDate == null)
                        dr["UDDate"] = DBNull.Value;
                    else
                        dr["UDDate"] = data.UDDate;
                    if (data.ContractDate == null)
                        dr["ContractDate"] = DBNull.Value;
                    else
                        dr["ContractDate"] = data.ContractDate;

                    dr["IsPrint"] = data.IsPrint;
                    dr["IsMarketingCommisssionApplicable"] = data.IsMarketingCommisssionApplicable;
                    dr["MarketingCommisssionId"] = data.MarketingCommisssionId;
                    dr["IsBusinessDevelopmentChargesApplicable"] = data.IsBusinessDevelopmentChargesApplicable;
                    dr["BusinessDevelopmentCharge"] = data.BusinessDevelopmentCharge;
                    dr["BusinessDevelopmentChargeValue"] = data.BusinessDevelopmentChargeValue;
                    dr["MarketingCommisssionCharge"] = data.MarketingCommisssionCharge;
                    dr["MarketingCommisssionValue"] = data.MarketingCommisssionValue;
                    dr["InvoicingPartyPlantId"] = data.InvoicingPartyPlantId;
                    dr["DeliveryPartyPlantId"] = data.DeliveryPartyPlantId;
                    dr["InvoicingByAddress"] = data.InvoicingByAddress;
                    dr["DeliveryByAddress"] = data.DeliveryByAddress;
                    dr["BankId"] = data.BankId;
                    dr["LCRequiredDaysfromShipment"] = data.LCRequiredDaysfromShipment;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }

                foreach (var item in masterOrderItem)
                {
                    if (id == "")
                        id = "'" + item.SalesOrderId + "'";
                    else
                        id = id + ",'" + item.SalesOrderId + "'";
                }
                string mosql = "SELECT * FROM TRN.SalesOrder WHERE Id IN (" + id + ")";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(mosql, out dsMasterOrder, false, "1");

                string cId = string.Empty;
                foreach (var item in masterOrderItem)
                {
                    DataView dv = new DataView(dsMasterOrder.Tables[0]);
                    dv.RowFilter = "Id='" + item.SalesOrderId + "'";
                    if (string.IsNullOrEmpty(contId))
                    {
                        cId = data.Id;
                    }
                    else
                    {
                        cId = contId;
                    }
                    if (dv.Count > 0)
                    {
                        DataRow drmo = dv[0].Row;

                        drmo.BeginEdit();

                        drmo["ContractId"] = cId;
                        drmo["UpdatedBy"] = identity.Name;
                        drmo["UpdatedDate"] = DateTime.Now.ToString();
                        drmo["UpdatedFromIP"] = identity.IPAddress;

                        drmo.EndEdit();

                    }

                }
                contractId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                #region FUND 
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.ContractFund where  ContractId='" + contractId + "'", out dsChild, false, "1");
                if (funds != null)
                {
                    foreach (var item in funds)
                    {
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        item["ContractId"] = contractId;
                        if (dv.Count == 0)
                        {
                            item["Id"] = GetContractFundPK();
                            item["ContractId"] = contractId;

                            AddNewRow(dsChild.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }

                #endregion

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsMasterOrder, dsChild);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost, Authorize]
        public JsonResult UpdateSO(List<Dictionary<string, object>> data)
        {
            try
            {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsChild;
                string id = "";
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        if (id == "")
                            id = "'" + item["MasterOrderItemId"] + "'";
                        else
                            id = id + ",'" + item["MasterOrderItemId"] + "'";
                    }
                    string mosql = "SELECT * FROM TRN.[MasterOrderItem] WHERE Id IN (" + id + ")";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(mosql, out dsChild, false, "1");

                    string cId = string.Empty;
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["MasterOrderItemId"] + "'";
                        
                        if (dv.Count > 0)
                        {
                            DataRow drmo = dv[0].Row;

                            drmo.BeginEdit();

                            drmo["LCArticle"] = item["LCArticle"];
                            drmo["UpdatedBy"] = identity.Name;
                            drmo["UpdatedDate"] = DateTime.Now.ToString();
                            drmo["UpdatedFromIP"] = identity.IPAddress;

                            drmo.EndEdit();

                        }

                    }

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsChild);
                }


                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }

        private void XSaveData(Dictionary<string, object> data, List<MasterOrderItemModel> masterOrderItem, out string contractId, List<Dictionary<string, object>> funds)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster, dsMasterOrder, dsChild;
            string contId = string.Empty;
            string id = string.Empty;
            try
            {
                RemoveMOICon(data["Id"].ToString());
                string sql = "SELECT * FROM [dbo].[Contract] WHERE Id='" + data["Id"].ToString() + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    data["Id"] = "C" + GetPK();
                    data["PlantId"] = identity.PlantId;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                foreach (var item in masterOrderItem)
                {
                    if (id == "")
                        id = "'" + item.MasterOrderItemId + "'";
                    else
                        id = id + ",'" + item.MasterOrderItemId + "'";
                }
                string mosql = "SELECT * FROM TRN.MasterOrderItem WHERE Id IN (" + id + ")";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(mosql, out dsMasterOrder, false, "1");

                string cId = string.Empty;
                foreach (var item in masterOrderItem)
                {
                    DataView dv = new DataView(dsMasterOrder.Tables[0]);
                    dv.RowFilter = "Id='" + item.MasterOrderItemId + "'";
                    if (string.IsNullOrEmpty(contId))
                    {
                        cId = data["Id"].ToString();
                    }
                    else
                    {
                        cId = contId;
                    }
                    if (dv.Count > 0)
                    {
                        DataRow drmo = dv[0].Row;

                        drmo.BeginEdit();

                        drmo["ContractId"] = cId;
                        drmo["UpdatedBy"] = identity.Name;
                        drmo["UpdatedDate"] = DateTime.Now.ToString();
                        drmo["UpdatedFromIP"] = identity.IPAddress;

                        drmo.EndEdit();

                    }

                }
                contractId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                #region FUND 
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.ContractFund where  ContractId='" + contractId + "'", out dsChild, false, "1");
                if (funds != null)
                {
                    foreach (var item in funds)
                    {
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        item["ContractId"] = contractId;
                        if (dv.Count == 0)
                        {
                            item["Id"] = GetContractFundPK();
                            item["ContractId"] = contractId;

                            AddNewRow(dsChild.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }

                #endregion

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsMasterOrder, dsChild);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private string GetTNCPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ContractTermsAndConditions", out sID);
            return sID;
        }

        [HttpPost, Authorize]
        public JsonResult CreateTNC(List<Dictionary<string, object>> data, string contractId)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsChild;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.ContractTermsAndConditions where  contractId='" + contractId + "'", out dsChild, false, "1");

                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = GetTNCPK();

                            AddNewRow(dsChild.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsChild);
                }


                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }

      

        private void UpdateContractData(Contract data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            string contractId = string.Empty;
            string id = string.Empty;
            try
            {
                string sql = "SELECT * FROM [dbo].[Contract] WHERE Id='" + data.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count > 0)
                {

                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();
                    dr["MasterLCId"] = data.MasterLCId;
                    dr["PlantId"] = identity.PlantId;
                    dr["Description"] = data.Description;
                    dr["IsLC"] = data.IsLC;
                    dr["ContractNo"] = data.ContractNo;
                    dr["Amount"] = data.Amount;
                    dr["TotalQty"] = data.TotalQty;
                    dr["SOQty"] = data.SOQty;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void SaveMasterLCData(MasterLC data, out string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster, dsSeq;
            string contractId = string.Empty;
            string id = string.Empty;
            try
            {

                GetAutoSequence(data.Id, out dsSeq);
                decimal seq = Convert.ToDecimal(dsSeq.Tables[0].Rows[0]["Version"].ToString());

                string sql = "SELECT * FROM [dbo].[MasterLC] WHERE Id='" + data.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = "MLC" + GetMasterLCPK();
                    dr["Version"] = seq;
                    dr["CustomerId"] = data.CustomerId;
                    dr["IsClose"] = data.IsClose;
                    dr["BenificiaryBankId"] = data.BenificiaryBankId;
                    dr["OpeningBankId"] = data.OpeningBankId;
                    dr["OpeningDescription"] = data.OpeningDescription;
                    dr["LeinBank"] = data.LeinBank;
                    dr["LeinDescription"] = data.LeinDescription;
                    dr["LCRef"] = data.LCRef;
                    dr["LCDate"] = data.LCDate;
                    dr["ExpiryDate"] = data.ExpiryDate;
                    dr["Amount"] = data.Amount;
                    dr["Type"] = data.Type;
                    dr["Tenure"] = data.Tenure;
                    dr["FinalDestinationId"] = data.FinalDestinationId;
                    dr["PortOfLandingId"] = data.PortOfLandingId;
                    dr["CurrencyId"] = data.CurrencyId;
                    dr["LCShipmentDate"] = data.LCShipmentDate;
                    dr["ShipmentModeId"] = data.ShipmentModeId;
                    dr["PortOfLoadingId"] = data.PortOfLoadingId;
                    dr["Remarks"] = data.Remarks;
                    dr["DescriptionOfGoodsAndOrServices"] = data.DescriptionOfGoodsAndOrServices;
                    dr["TermsandConditionVarify"] = data.TermsandConditionVarify;
                    dr["PaymentTermVarify"] = data.PaymentTermVarify;
                    dr["AdditionalInfoVarify"] = data.AdditionalInfoVarify;
                    dr["NegotiatingBankId"] = data.NegotiatingBankId;
                    dr["InsuranceCompany"] = data.InsuranceCompany;
                    dr["InsuranceCompanyDescription"] = data.InsuranceCompanyDescription;
                    dr["InsuranceCoverNote"] = data.InsuranceCoverNote;


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

                    dr["Version"] = seq;
                    dr["CustomerId"] = data.CustomerId;
                    dr["IsClose"] = data.IsClose;
                    dr["BenificiaryBankId"] = data.BenificiaryBankId;
                    dr["OpeningBankId"] = data.OpeningBankId;
                    dr["OpeningDescription"] = data.OpeningDescription;
                    dr["LeinBank"] = data.LeinBank;
                    dr["LeinDescription"] = data.LeinDescription;
                    dr["LCRef"] = data.LCRef;
                    dr["LCDate"] = data.LCDate;
                    dr["ExpiryDate"] = data.ExpiryDate;
                    dr["Amount"] = data.Amount;
                    dr["Type"] = data.Type;
                    dr["Tenure"] = data.Tenure;
                    dr["FinalDestinationId"] = data.FinalDestinationId;
                    dr["PortOfLandingId"] = data.PortOfLandingId;
                    dr["CurrencyId"] = data.CurrencyId;
                    dr["LCShipmentDate"] = data.LCShipmentDate;
                    dr["ShipmentModeId"] = data.ShipmentModeId;
                    dr["PortOfLoadingId"] = data.PortOfLoadingId;
                    dr["Remarks"] = data.Remarks;
                    dr["DescriptionOfGoodsAndOrServices"] = data.DescriptionOfGoodsAndOrServices;
                    dr["TermsandConditionVarify"] = data.TermsandConditionVarify;
                    dr["PaymentTermVarify"] = data.PaymentTermVarify;
                    dr["AdditionalInfoVarify"] = data.AdditionalInfoVarify;
                    dr["NegotiatingBankId"] = data.NegotiatingBankId;
                    dr["InsuranceCompany"] = data.InsuranceCompany;
                    dr["InsuranceCompanyDescription"] = data.InsuranceCompanyDescription;
                    dr["InsuranceCoverNote"] = data.InsuranceCoverNote;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

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

        public void GetAutoSequence(string Id, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager Obj;

            try
            {
                string sql = @"SELECT (ISNULL((MAX(ISNULL(Version,0))),0)+1) Version FROM [dbo].[MasterLC] Where Id='" + Id + "'";
                Obj = new ConnectionManager.DAL.ConManager("1");
                Obj.OpenDataSetThroughAdapter(sql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetAmandmentAutoSequence(string Id, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager Obj;

            try
            {
                string sql = @"SELECT (ISNULL((MAX(ISNULL(Version,0))),0)+1) Version FROM [dbo].[MasterLCHistory] Where Id='" + Id + "'";
                Obj = new ConnectionManager.DAL.ConManager("1");
                Obj.OpenDataSetThroughAdapter(sql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult CreateContractWithMasterLC(IEnumerable<Contract> models, string masterLcId)
        {
            try
            {
                UpdateContractWithMasterLC(models, masterLcId);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }

        private void UpdateContractWithMasterLC(IEnumerable<Contract> models, string masterLcId)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster = null;
            DataSet dsMLC = null;
            DataSet dsChild = null;
            string id = string.Empty;
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                foreach (var item in models)
                {

                    if (id == "")
                        id = "'" + item.Id + "'";
                    else
                        id = id + ",'" + item.Id + "'";

                    string sql = "SELECT * FROM [dbo].[Contract] WHERE Id='" + item.Id + "'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                    string mlctcsql = "SELECT * FROM [dbo].[MasterLCTermsAndConditions]  WHERE MasterLCId='"+ masterLcId + "'";
                    objCon.OpenDataSetThroughAdapter(mlctcsql, out dsMLC, false, "1");

                    objCon.OpenDataSetThroughAdapter(@"SELECT distinct CT.TermsAndConditionsId,TC.Sequence,TC.Code,TC.ShortName,TC.StandardName,TC.UserName FROM dbo.ContractTermsAndConditions CT
LEFT JOIN HKP.TermsAndConditions TC ON TC.Id = CT.TermsAndConditionsId
WHERE CT.ContractId IN(" + id + ") AND TermsAndConditionsId NOT IN(SELECT TermsAndConditionsId FROM [dbo].[MasterLCTermsAndConditions] WHERE MasTerLCId='" + masterLcId + "')", out dsChild, false, "1");

                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        //edit
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();
                        dr["PlantId"] = identity.PlantId;
                        dr["MasterLCId"] = masterLcId;
                        dr["IsLC"] = true;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();
                    }

                    for (int j = 0; j < dsChild.Tables[0].Rows.Count; j++)
                    {
                        DataRow drMLC = dsMLC.Tables[0].NewRow();
                        CopyRow(dsChild.Tables[0].Rows[j], ref drMLC);
                        drMLC["Id"] = GetMasterLCTNCPK();
                        drMLC["MasterLCId"] = masterLcId;
                        drMLC["UserName"] = dsChild.Tables[0].Rows[j]["UserName"];
                        dsMLC.Tables[0].Rows.Add(drMLC);
                    }
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsMLC);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void CopyRow(DataRow drSource, ref DataRow drDestination)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            for (int COL = 0; COL < drSource.Table.Columns.Count; COL++)
            {
                try
                {
                    drDestination[drSource.Table.Columns[COL].ColumnName] = drSource[drSource.Table.Columns[COL].ColumnName];

                }
                catch (Exception ex)
                {
                }
                try
                {
                    drDestination["AddedBy"] = identity.Name;
                    drDestination["AddedDate"] = DateTime.Now;
                    drDestination["AddedFromIP"] = identity.IPAddress;
                    drDestination["UpdatedBy"] = identity.Name;
                    drDestination["UpdatedFromIP"] = identity.IPAddress;
                    drDestination["UpdatedDate"] = DateTime.Now;

                }
                catch (Exception ex)
                {
                }
            }

        }

        [HttpPost, Authorize]
        public ActionResult RemoveContract(string id)
        {
            RemoveContractFromMasterLC(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        private void RemoveContractFromMasterLC(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;

            try
            {
                string sql = "SELECT * FROM [dbo].[Contract] WHERE Id='" + id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();
                    dr["IsLC"] = false;
                    dr["MasterLCId"] = DBNull.Value;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetMasterLcData(string contractId)
        {
            try
            {
                return Json(clsCon.GetMasterLcData(contractId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetMasterLCList(string customerId)
        {
            return Json(clsCon.GetMasterLCList(customerId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetMasterLCDataList()
        {
            var jsondata = Json(clsCon.GetMasterLCDataList(), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpGet, Authorize]
        public ActionResult GetContractFundData(string contractId)
        {
            return Json(clsCon.GetContractFundData(contractId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteMasterLC(string id)
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
                strSQL = "DELETE FROM dbo.MasterLC WHERE Id = '" + Id + "'";
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
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw exx;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        [HttpPost]
        public JsonResult Delete(string id)
        {
            DeleteContract(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteContract(string Id)
        {
            string strSQL, strUSQL, strCFSQL, strCNFSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strUSQL = "Update TRN.SalesOrder set ContractId=NULL Where ContractId='" + Id + "'";
                strCFSQL = "delete from dbo.ContractFund Where ContractId='" + Id + "'";
                strCNFSQL = "delete from dbo.ContractTermsAndConditions Where ContractId='" + Id + "'";
                strSQL = "delete from dbo.Contract Where Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strUSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strCFSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strCNFSQL, true, "1");
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

        [Authorize, HttpPost]
        public ActionResult DeleteContractTermsAndConditions(string id)
        {
            DeleteContractTermsAndConditionsData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }


        public void DeleteContractTermsAndConditionsData(string id)
        {
            string strSQL, strDSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[ContractTermsAndConditions] WHERE Id = '" + id + "'";

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


        [HttpGet, Authorize]
        public ActionResult GetSalesOrderList(string customerId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(clsCon.GetSalesOrderList(identity.CompanyId, identity.PlantId, customerId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetEditSalesOrderList(string customerId, string contractId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(clsCon.GetEditSalesOrderList(identity.CompanyId, identity.PlantId, customerId, contractId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetSalesOrderListByContract(string customerId, string contractId)
        {
            try
            {
                return Json(clsCon.GetSalesOrderListByContract(customerId, contractId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        [HttpGet, Authorize]
        public ActionResult GetContractSummaryReport(string ContractId)
        { 
            try
            { 
                ExcelEngine excelEngine = new ExcelEngine(); 
                IWorkbook workbook = clsCon.GetContractSummaryReportXlx(ContractId);

                string strFileName = "Contract Summary.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet); 
            } 
            return null;
        }

        #region Master Order & Items Details Report 

        [HttpGet, Authorize]
        public ActionResult MasterOrder(string ContractId, bool isMatrix)
        {

            try
            {
                // if (string.IsNullOrEmpty(MasterLCList))
                //   throw new Exception("Please select at least one master Order");



                ExcelEngine excelEngine = new ExcelEngine();

                IWorkbook workbook = GetMasterOrderReport(ContractId, isMatrix);

                string strFileName = "Contract Details.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }


            return null;
        }

        private IWorkbook GetMasterOrderReport(string ContractId, bool isMatrix)
        {
            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;

            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            try
            {
                DataTable dtOrderMaster = _sqlRepository.GetDataTable(@"select mo.Id, mo.type, b.UserName as Buyer, p.UserName as Customer,mo.PartyId
				                , c.ContractNo,SO.ContractId,   c.MasterLCId, ml.LCRef As MasterLCNo, moi.id as MasterOrderItemNo
                                ,  mo.OrderYear as Year, mo.TotalQty as TotalQuantity
                                    , uom.UserName as UnitOfMeasurement, mo.NoOfLineItem, mo.OrderWastagePercentage
                                    , mo.ExtraOrderPercentage, mo.BuyerReferenceNo, mo.OwnReferenceNo, BDept.UserName as BuyerDepartment
                                    , BDev.UserName as BuyerDevision, MoCur.Code MasterOrderCurrency
			    	
					                 from trn.MasterOrder MO 
                                    left join scs.Currency MoCur on MoCur.id = mo.CurrencyId 
                                    left join scs.UnitOfMeasurement UOM on uom.id = mo.TotalQtyUOMId 
                                    left join hkp.buyer B on b.id = mo.buyerid 
                                    left join hkp.party p on p.id = mo.partyid 
                                    left join hkp.BuyerDepartment BDept on BDept.id = mo.buyerDepartmentid 
                                    left join HKP.buyerdivision BDev on BDev.id = mo.BuyerDivisionId 
					                left join trn.MasterOrderItem moi on moi.MasterOrderId = mo.Id
					                left join trn.SalesOrder SO on SO.MasterOrderItemid =MOI.Id
					                left join dbo.Contract C on c.id = SO.ContractId
					                left join MasterLC ml on ml.Id = c.MasterLCId
                                    where c.Id='" + ContractId + "'");

                if (dtOrderMaster.Rows.Count == 0)
                    throw new Exception("No data found");

                DataTable dtMasterOrderItem = _sqlRepository.GetDataTable(@"select moi.id as MasterOrderItemNo
                                    ,moi.BuyerReferenceNo as BuyerItem,moi.OwnReferenceNo as OwnItem
				            ,b.UserName as Buyer, moi.TotalQty as TotalMOIQuantity, moi.MasterOrderId
			                ,mo.BuyerReferenceNo as BuyerOrder, mo.OwnReferenceNo as OwnOrder
                             ,moi.OrderWastagePercentage, moi.ExtraOrderPercentage ,mm.UserName as Material ,mma.StandardName as Article, moi.Type
                             from trn.MasterOrderItem MOI
                             left join TRN.MasterOrder mo  on mo.id=moi.MasterOrderId
                             left join TRN.SalesOrder So on So.MasterOrderItemid=moi.Id
                             left join MST.MaterialMaster MM on mm.id = moi.MaterialMasterId
                             left join MST.MaterialMasterArticle mma on mma.id= moi.ArticleId
                             left join scs.TestingStandard ts on ts.id=moi.TestingStandardId
				             left join hkp.buyer B on b.id = mo.buyerid
                            where so.ContractId='" + ContractId + "'");


                DataTable dtSalesOrderItem = _sqlRepository.GetDataTable(@"select so.MasterOrderItemId
                , so.id as SalesOrderNo,cpo.PONumber,os.UserName as OrderStatus,d.UserName as Destination
                ,so.Qty as Quantity, so.UpCharge, so.MainRawMaterialInhouseDate, so.Description
                ,so.SOType, oc.username as OrderCategory
                ,so.DeliveryDate, sm.UserName as ShipmentMode
                ,so.Rate, so.Discount,so.CM,so.LSD, so.OtherRawMaterialInhouseDate , so.Reason , so.CommitmentDate

                ,c1.username as FirstCharacteristics, isnull(fcs.ValueFreeText,CV1.UserName) as FirstCharacteristicsValue
                ,c2.username as SecondCharacteristics ,isnull(SCS.ValueFreeText, CV2.UserName) as SecondCharacteristicsValue
                ,c3.username as ThirdCharacteristics , isnull(ThirdCS.ValueFreeText,CV3.UserName) as ThirdCharacteristicsValue
                ,case when isnull(thirdCs.Id,'')<>'' THEN ThirdCs.Qty
                ELSE case when isnull(scs.id,'')<>'' THEN scs.Qty
                ELSE case when isnull(fcs.Id,'')<>'' THEN fcs.Qty
                ELSE 0 END END END AS Qty
                from trn.SalesOrder SO
                left join trn.masterorderitem moi on moi.id= so.masterorderitemid
                left join HKP.OrderCategory OC on oc.id = so.OrderCategoryId
                left join hkp.OrderStatus OS on os.id = so.OrderStatusId
                left join mst.shipMode SM on sm.id = so.shipmentModeId
                left join mst.Destination d on d.id =so.DestinationId
                left join trn.CustomerPO CPO on cpo.id =so.CustomerPOId

                left join TRN.FirstCharacteristics FCS on fcs.SalesOrderId = so.id
                left join hkp.Characteristics C1 on c1.id = fcs.CharacteristicsId
                left join HKP.CharacteristicsValue CV1 on cv1.id= fcs.CharacteristicsValueId

                left join TRN.SecondCharacteristics SCS on scs.SalesOrderId=so.id and scs.FirstCharacteristicsId=fcs.Id
                left join hkp.Characteristics C2 on c2.id = scs.CharacteristicsId
                left join HKP.CharacteristicsValue CV2 on cv2.id= scs.CharacteristicsValueId

                left join TRN.ThirdCharacteristics ThirdCS on ThirdCS.SalesOrderId=so.id and scs.id=ThirdCS.SecondCharacteristicsId
                left join hkp.Characteristics C3 on c3.id = ThirdCS.CharacteristicsId
                left join HKP.CharacteristicsValue CV3 on CV3.id= ThirdCS.CharacteristicsValueId

                where so.ContractId='" + ContractId + "'");

                worksheet.Name = "MasterOrderDetailsReport";

                int ROW = 5; int COL = 1;
                int MasterOrderDetailsStartRow = ROW;
                worksheet[ROW, COL].Text = "Sales Contract Details:";
                worksheet[ROW, COL].CellStyle.Font.Bold = true;
                ROW++;

                int leftColumnCaption = COL;
                int leftColumnValue = leftColumnCaption + 1;

                //int MiddleColumnCaption = leftColumnValue + 2;
                int MiddleColumnCaption = leftColumnValue + 1;
                int MiddleColumnValue = MiddleColumnCaption + 1;

                int RightColumnCaption = MiddleColumnValue + 1;
                int RightColumnValue = RightColumnCaption + 1;

                //Contract.............................................................

                worksheet[ROW, leftColumnCaption].Text = "ContractNo#";
                worksheet[ROW, leftColumnValue].Text = dtOrderMaster.Rows[0]["ContractNo"].ToString();
                worksheet.Range[ROW, leftColumnValue, ROW, leftColumnValue].CellStyle.Font.Color = ExcelKnownColors.Blue;
                worksheet[ROW, leftColumnValue].ColumnWidth = 16;
                worksheet.Range[ROW, leftColumnCaption, ROW, leftColumnValue].CellStyle.Font.Bold = true;
                worksheet[ROW, leftColumnCaption].ColumnWidth = 16;

                worksheet[ROW, MiddleColumnCaption].Text = "Customer";
                worksheet[ROW, MiddleColumnValue].Text = dtOrderMaster.Rows[0]["Customer"].ToString();
                worksheet.Range[ROW, MiddleColumnCaption, ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;
                worksheet[ROW, MiddleColumnCaption].ColumnWidth = 10;
                worksheet[ROW, MiddleColumnValue].ColumnWidth = 14;

                worksheet[ROW, RightColumnCaption].Text = "Master LC No.";
                worksheet[ROW, RightColumnValue].Text = dtOrderMaster.Rows[0]["MasterLCNo"].ToString();
                worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;
                worksheet[ROW, RightColumnCaption].ColumnWidth = 10;
                worksheet[ROW, RightColumnValue].ColumnWidth = 13;
                ROW++;

                worksheet.Range[MasterOrderDetailsStartRow, leftColumnCaption, ROW, RightColumnValue].CellStyle.Interior.ColorIndex = ExcelKnownColors.Custom44;

                ROW += 2;


                //Master Order & Items....................................................................................................
                StringCollection strColSO = new StringCollection();

                for (int i = 0; i < dtMasterOrderItem.Rows.Count; i++)
                {
                    int MasterItemsStartRow = ROW; // row 9
                    worksheet[ROW, COL].Text = "Item Details:"; //col 1
                    worksheet[ROW, COL].CellStyle.Font.Bold = true;
                    ROW++;

                    strColSO = new StringCollection();

                    worksheet[ROW, leftColumnCaption].Text = "Order#";
                    worksheet[ROW, leftColumnValue].Text = dtMasterOrderItem.Rows[i]["MasterOrderId"].ToString();
                    worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;

                    worksheet[ROW, MiddleColumnCaption].Text = "Own Order";
                    worksheet[ROW, MiddleColumnValue].Text = dtMasterOrderItem.Rows[i]["OwnOrder"].ToString();
                    worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                    worksheet[ROW, RightColumnCaption].Text = "Buyer Order";
                    worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;
                    worksheet[ROW, RightColumnValue].Text = dtMasterOrderItem.Rows[i]["BuyerOrder"].ToString();

                    ROW++;

                    worksheet[ROW, leftColumnCaption].Text = "Material";
                    worksheet[ROW, leftColumnValue].Text = dtMasterOrderItem.Rows[i]["Material"].ToString();
                    worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;

                    worksheet[ROW, MiddleColumnCaption].Text = "Buyer";
                    worksheet[ROW, MiddleColumnValue].Text = dtMasterOrderItem.Rows[i]["Buyer"].ToString();
                    worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;
                    ROW++;

                    worksheet[ROW, leftColumnCaption].Text = "Article";
                    worksheet[ROW, leftColumnValue].Text = dtMasterOrderItem.Rows[i]["Article"].ToString();
                    worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;
                    ROW++;

                    worksheet[ROW, leftColumnCaption].Text = "Buyer Item";
                    worksheet[ROW, leftColumnValue].Text = dtMasterOrderItem.Rows[i]["BuyerItem"].ToString();
                    worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;

                    worksheet[ROW, MiddleColumnCaption].Text = "Own Item";
                    worksheet[ROW, MiddleColumnValue].Text = dtMasterOrderItem.Rows[i]["OwnItem"].ToString();
                    worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;
                    worksheet[ROW, MiddleColumnValue].ColumnWidth = 15;

                    worksheet[ROW, RightColumnCaption].Text = "Qty";
                    worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;
                    worksheet[ROW, RightColumnValue].Number = clsStaticInfo.dbl(dtMasterOrderItem.Rows[i]["TotalMOIQuantity"].ToString());
                    worksheet[ROW, RightColumnValue, ROW, RightColumnValue].NumberFormat = clsStaticInfo.NumberFormat();

                    worksheet.Range[MasterItemsStartRow, leftColumnCaption, ROW, RightColumnValue].CellStyle.Interior.ColorIndex = ExcelKnownColors.Custom18;
                    ROW++;

                    //So.......................
                    dtSalesOrderItem.DefaultView.RowFilter = "MasterOrderItemId='" + dtMasterOrderItem.Rows[i]["MasterOrderItemNo"].ToString() + "'";
                    DataTable dtSalesOrderFilteredByItem = dtSalesOrderItem.DefaultView.ToTable();
                    for (int KK = 0; KK < dtSalesOrderItem.DefaultView.Count; KK++)
                    {


                        if (strColSO.Contains(dtSalesOrderItem.DefaultView[KK]["SalesOrderNo"].ToString()))
                            continue;
                        int SOStartRow = ROW;  //row 16
                        worksheet[SOStartRow, COL].Text = "Sales Order Details & Breakdown:";
                        worksheet[SOStartRow, COL].CellStyle.Font.Bold = true;
                        ROW++;

                        // int SOStartRow = ROW;

                        strColSO.Add(dtSalesOrderItem.DefaultView[KK]["SalesOrderNo"].ToString());

                        worksheet[ROW, leftColumnCaption].Text = "SO No";
                        worksheet[ROW, leftColumnValue].Text = dtSalesOrderItem.DefaultView[KK]["SalesOrderNo"].ToString();
                        worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;

                        worksheet[ROW, MiddleColumnCaption].Text = "Del. Date";
                        worksheet[ROW, MiddleColumnValue].Text = Convert.ToDateTime(dtSalesOrderItem.DefaultView[KK]["DeliveryDate"].ToString()).ToString("dd-MMM-yyyy");
                        worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                        worksheet[ROW, RightColumnCaption].Text = "Qty";
                        worksheet[ROW, RightColumnValue].Number = clsStaticInfo.dbl(dtSalesOrderItem.DefaultView[KK]["Quantity"].ToString());
                        worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;
                        worksheet[ROW, RightColumnValue, ROW, RightColumnValue].NumberFormat = clsStaticInfo.NumberFormat();

                        ROW++;

                        worksheet[ROW, leftColumnCaption].Text = "Dest.";
                        worksheet[ROW, leftColumnValue].Text = dtSalesOrderItem.DefaultView[KK]["Destination"].ToString();
                        worksheet[ROW, leftColumnCaption].CellStyle.Font.Bold = true;

                        worksheet[ROW, MiddleColumnCaption].Text = "Ship Mode";
                        worksheet[ROW, MiddleColumnValue].Text = dtSalesOrderItem.DefaultView[KK]["ShipmentMode"].ToString();
                        worksheet[ROW, MiddleColumnCaption].CellStyle.Font.Bold = true;

                        worksheet[ROW, RightColumnCaption].Text = "Ord. Status";
                        worksheet[ROW, RightColumnValue].Text = dtSalesOrderItem.DefaultView[KK]["OrderStatus"].ToString();
                        worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;

                        worksheet.Range[SOStartRow, leftColumnCaption, ROW, RightColumnValue].CellStyle.Interior.ColorIndex = ExcelKnownColors.Custom19;

                        ROW++;

                        dtSalesOrderFilteredByItem.DefaultView.RowFilter = "SalesOrderNo='" + dtSalesOrderItem.DefaultView[KK]["SalesOrderNo"].ToString() + "'"; //????
                        DataTable dtBreakdownData = dtSalesOrderFilteredByItem.DefaultView.ToTable();
                        DrawSOBreakdownData(dtBreakdownData, worksheet, ref ROW, isMatrix);

                        ROW++;
                    }

                    ROW += 2; // Gap for Material
                }

                int endCol = RightColumnValue;


                worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                worksheet.UsedRange.CellStyle.Font.Size = 8f;


                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref worksheet, endCol, "Contract NO#" + dtOrderMaster.Rows[0]["ContractNo"].ToString(), identity.PlantId);
                // reportUtility.PlantHeader(ref worksheet, endCol, "Contract NO#" + ContractId, identity.PlantId);
                reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                worksheet.Range[1, 1, 5, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                worksheet.IsGridLinesVisible = false;
                return workbook;

            }
            catch (Exception ex)
            {
                throw (ex);

            }




        }

        [Authorize, HttpGet]
        public ActionResult ProformaInvoice(string ContractId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            GetProformaInvoice(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.UserId, ContractId);

            return View();
        }

        [Authorize, HttpGet]
        public ActionResult ProformaInvoice1(string ContractId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            GetProformaInvoice1(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, identity.Name, ContractId);

            return View();
        }

        public void GetProformaInvoice1(string companyGroupId, string companyId, string plantId, string Name, string ContractId)
        {
            var fileName = "";
            var strPath = "";
            var File = "";

            ReportUtility ru = new ReportUtility();
            fileName = "ProformaInvoiceContracts" + plantId + ".docx";

            strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
            File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }

            WordDocument document = new WordDocument(File, FormatType.Docx);

            try
            {
                WSection section = document.Sections[0];

                DataTable dsOrderMaster;
                DataTable dsTermsAndCondition;

                dsOrderMaster = clsCon.ProformaInvoiceSQL(ContractId);
                dsTermsAndCondition = clsCon.TermsAndConditionSQL(ContractId);

                Dictionary<string, string> columns = new Dictionary<string, string>();

                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                var MaterialTotal = makeCProformaInvoiceService(companyGroupId, companyId, plantId, ContractId, document, dsOrderMaster);   // {materialItems}
                var TermsAndCondition = makeTermsAndCondition(companyGroupId, companyId, plantId, ContractId, document, dsTermsAndCondition);   // {materialItems}

                document.Replace("{GrandTotal}", (MaterialTotal).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                //document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("F2"), true, true);
                document.Replace("{TotalInWords}", ru.InWord((MaterialTotal), dsOrderMaster.Rows[0]["CurrencyId"].ToString()), true, true);


                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();

                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));

                //creating secondary array to prevent memory leak and accidental over-writing (Tarek Talukder-26-May-2019)
                List<string> strReplace = new List<string>();
                for (int i = 0; i < allresult.Length; i++)
                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());

                for (int i = 0; i < strReplace.Count; i++)
                {
                    string text = strReplace[i].ToUpper();
                    ReplaceInfo.Add(text, 0);
                    if (columns.ContainsKey(text.ToUpper()))
                    {
                        //ReplaceInfo[text] = document.Replace(text, dsOrderMaster.Tables[0].Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                        document.Replace(text, dsOrderMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    }
                    if (text == "{PRINTEDBY}")
                    {
                        document.Replace(text, Name, false, false);
                    }
                    if (text == "{DT}")
                    {
                        document.Replace(text, DateTime.Now.ToString("dd-MMM-yyyy h:mm tt"), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);

                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "N/A", false, false);
                }

                /////////////////////
                ///

                //DocToPDFConverter converter = new DocToPDFConverter();

                ////Converts Word document into PDF document
                //PdfDocument pdfDocument = converter.ConvertToPDF(document);
                //pdfDocument.PageSettings.Width = 1200;
                //pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                ////Releases all resources used by DocToPDFConverter
                //converter.Dispose();

                ////Closes the instance of document objects

                ////Saves the PDF file 
                //string Prefix = "ProformaContractInvoice" + plantId;

                //pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                ////Closes the instance of document objects
                //pdfDocument.Close(true);
                document.Save(fileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();
            }
            catch (Exception ex)
            {


            }

            document.Close();
        }

        public void GetProformaInvoice(string companyGroupId, string companyId, string plantId, string UserId, string ContractId)
        {
            var fileName = "";
            var strPath = "";
            var File = "";

            ReportUtility ru = new ReportUtility();
            fileName = "ProformaInvoiceContract" + plantId + ".docx";

            strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
            File = strPath;
            if (!System.IO.File.Exists(strPath))
            {
                throw new CustomException("File <" + fileName + "> Not Found.");
            }

            WordDocument document = new WordDocument(File, FormatType.Docx);

            try
            {
                WSection section = document.Sections[0];

                DataTable dsOrderMaster;
                DataTable dsTermsAndCondition;

                dsOrderMaster = clsCon.ProformaInvoiceSQL(ContractId);
                dsTermsAndCondition = clsCon.TermsAndConditionSQL(ContractId);

                Dictionary<string, string> columns = new Dictionary<string, string>();

                foreach (DataColumn item in dsOrderMaster.Columns)
                    columns.Add("{" + item.ColumnName.ToUpper() + "}", item.ColumnName);

                var MaterialTotal = makeCProformaInvoiceService(companyGroupId, companyId, plantId, ContractId, document, dsOrderMaster);   // {materialItems}
                var TermsAndCondition = makeTermsAndCondition(companyGroupId, companyId, plantId, ContractId, document, dsTermsAndCondition);   // {materialItems}

                document.Replace("{GrandTotal}", (MaterialTotal).ToString("#,##0.00") + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString(), true, true);
                //document.Replace("{GrandTotal}", (materialTotal + serviceTotal).ToString("F2"), true, true);
                document.Replace("{TotalInWords}", ru.InWord((MaterialTotal), dsOrderMaster.Rows[0]["CurrencyId"].ToString()), true, true);


                Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();

                TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));

                //creating secondary array to prevent memory leak and accidental over-writing (Tarek Talukder-26-May-2019)
                List<string> strReplace = new List<string>();
                for (int i = 0; i < allresult.Length; i++)
                    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());

                for (int i = 0; i < strReplace.Count; i++)
                {
                    string text = strReplace[i].ToUpper();
                    ReplaceInfo.Add(text, 0);
                    if (columns.ContainsKey(text.ToUpper()))
                    {
                        //ReplaceInfo[text] = document.Replace(text, dsOrderMaster.Tables[0].Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                        document.Replace(text, dsOrderMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    }
                }

                document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, false);

                foreach (var item in ReplaceInfo.Keys)
                {
                    if (ReplaceInfo[item.ToString()] == 0)
                        document.Replace(item.ToString(), "N/A", false, false);
                }

                /////////////////////
                ///

                DocToPDFConverter converter = new DocToPDFConverter();

                //Converts Word document into PDF document
                PdfDocument pdfDocument = converter.ConvertToPDF(document);
                pdfDocument.PageSettings.Width = 1200;
                pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                //Releases all resources used by DocToPDFConverter
                converter.Dispose();

                //Closes the instance of document objects

                //Saves the PDF file 
                string Prefix = "ProformaContractInvoice" + ContractId;

                pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                //Closes the instance of document objects
                pdfDocument.Close(true);
                document.Save(fileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                document.Close();
            }
            catch (Exception ex)
            {


            }

            document.Close();
        }
        public double makeCProformaInvoiceService(string companyGroupId, string companyId, string plantId, string salesId, WordDocument document, DataTable dsOrderMaster)
        {
            string replaceString = "{MaterialDescription}";

            DataTable sales, materialTax;

            int LasColumnIndex = 8;

            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            WCharacterFormat DFontSize = new WCharacterFormat(document);
            FontBold.Bold = true;
            FontBold.FontSize = 8f;
            DFontSize.FontSize = 8f;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Sl#");
            range.ApplyCharacterFormat(FontBold);
            int colSrNo = COL; COL++;
            wTable.Rows[ROW].Cells[colSrNo].Width = 30;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Article");
            range.ApplyCharacterFormat(FontBold);
            int colArticle = COL; COL++;
            wTable.Rows[ROW].Cells[colArticle].Width = 140;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Buyer Item#");
            range.ApplyCharacterFormat(FontBold);
            int colBuyerReferenceNo = COL; COL++;
            wTable.Rows[ROW].Cells[colBuyerReferenceNo].Width = 60;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Del. Date");
            range.ApplyCharacterFormat(FontBold);
            int colDeliveryDate = COL; COL++;
            wTable.Rows[ROW].Cells[colDeliveryDate].Width = 45;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("SO#");
            range.ApplyCharacterFormat(FontBold);
            int colSONo = COL; COL++;
            wTable.Rows[ROW].Cells[colSONo].Width = 45;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Description of Material");
            //range.ApplyCharacterFormat(FontBold);
            //int colDescriptionOfMaterial = COL; COL++;
            //wTable.Rows[ROW].Cells[colDescriptionOfMaterial].Width = 80;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Destination");
            //range.ApplyCharacterFormat(FontBold);
            //int colDestination = COL; COL++;
            //wTable.Rows[ROW].Cells[colDestination].Width = 60;



            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("HSN");
            range.ApplyCharacterFormat(FontBold);
            int colHSN = COL; COL++;
            wTable.Rows[ROW].Cells[colHSN].Width = 46;



            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Qty" + "(" + "" + dsOrderMaster.Rows[0]["UoM"].ToString() + "" + ")" + " ");
            range.ApplyCharacterFormat(FontBold);
            int colQty = COL; COL++;
            wTable.Rows[ROW].Cells[colQty].Width = 55;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Rate");
            range.ApplyCharacterFormat(FontBold);
            int colRate = COL; COL++;
            wTable.Rows[ROW].Cells[colRate].Width = 35;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("UpCharge");
            //range.ApplyCharacterFormat(FontBold);
            //int colUpCharge = COL; COL++;
            //wTable.Rows[ROW].Cells[colUpCharge].Width = 45;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Amount" + "(" + " " + dsOrderMaster.Rows[0]["CurrencyName"].ToString() + " " + ")" + " ");
            range.ApplyCharacterFormat(FontBold);
            int colAmount = COL;
            wTable.Rows[ROW].Cells[colAmount].Width = 100;



            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsOrderMaster.Rows.Count; i++)
            {
                ROW++;
                sl++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                TROW.Cells[colSrNo].AddParagraph().AppendText(sl.ToString()).ApplyCharacterFormat(DFontSize);
                //TROW.Cells[colDescriptionOfMaterial].AddParagraph().AppendText(dsOrderMaster.Rows[i]["MaterialDescription"].ToString());
                TROW.Cells[colArticle].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Article"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colHSN].AddParagraph().AppendText(dsOrderMaster.Rows[i]["HSNCode"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colSONo].AddParagraph().AppendText(dsOrderMaster.Rows[i]["SONo"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colBuyerReferenceNo].AddParagraph().AppendText(dsOrderMaster.Rows[i]["BuyerReferenceNo"].ToString()).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colDeliveryDate].AddParagraph().AppendText(dsOrderMaster.Rows[i]["DeliveryDate"].ToString()).ApplyCharacterFormat(DFontSize);
                //TROW.Cells[colDestination].AddParagraph().AppendText(dsOrderMaster.Rows[i]["Destination"].ToString());            
                TROW.Cells[colQty].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["Qty"].ToString()).ToString("#,##0.00")).ApplyCharacterFormat(DFontSize);
                TROW.Cells[colRate].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["Rate"].ToString()).ToString("#,##0.00")).ApplyCharacterFormat(DFontSize);
                //TROW.Cells[colUpCharge].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["UpCharge"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colAmount].AddParagraph().AppendText(clsStdLib.dbl(dsOrderMaster.Rows[i]["Amount"].ToString()).ToString("#,##0.00")).ApplyCharacterFormat(DFontSize);


                //totalValue += clsStdLib.dbl(sales.Rows[i]["TrnAmount"].ToString());
            }

            ROW++;
            #region Total
            int TotalRow = ROW;
            wTable.AddRow();
            WTableRow _TROW = wTable.LastRow;
            _TROW.Cells[0].AddParagraph().AppendText("Total").ApplyCharacterFormat(FontBold);

            range.ApplyCharacterFormat(FontBold);
            double total = 0;
            for (int C = 1; C <= wTable.LastCell.GetCellIndex(); C++)
            {
                //|| dicTaxes.ContainsValue(C)
                if (C == colArticle || C == colRate ||/* C == colQty ||*//* C == colDestination ||*/ C == colHSN || C == colDeliveryDate || C == colSONo)
                    continue;

                double value = 0;
                for (int i = startRow; i < TotalRow; i++)
                {

                    foreach (WParagraph item in wTable.Rows[i].Cells[C].Paragraphs)
                    {
                        value += clsStdLib.dbl(item.Text);
                    }
                }
                total = value;
                _TROW.Cells[C].AddParagraph().AppendText(value.ToString("#,##0.00")).ApplyCharacterFormat(FontBold);
            }
            #endregion Total
            ROW++;
            #region Sub Total

            #endregion Total
            ROW++;
            #region Total Payable

            #endregion Total Payable

            ROW++;


            #region paragrpath formats
            //Adds a new paragraph style named "MyStyle"
            IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;


            #endregion paragrpath formats


            #region merging section


            //tax codes merging (horizontal)
            ROW = 0;
            //for (int i = 0; i < dv.Count; i++)
            //    wTable.ApplyHorizontalMerge(ROW, dicTaxes[dv[i]["TaxCode"].ToString()], dicTaxes[dv[i]["TaxCode"].ToString()] + 1);

            //primary cells merging (veritcal)
            ROW++;
            //for (int i = 0; i <= colTotalTaxableAmount; i++)
            //    wTable.ApplyVerticalMerge(i, ROW - 1, ROW);


            IWParagraphStyle style = document.AddParagraphStyle("SubTotalStyle");
            style.CharacterFormat.Bold = true;
            style.CharacterFormat.FontSize = 8f;
            style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
            //Adds new paragraph to the section


            //for (int CELL = 0; CELL < wTable.Rows[SubTotalRow].Cells.Count; CELL++)
            //    foreach (WParagraph PARA in wTable.Rows[SubTotalRow].Cells[CELL].Paragraphs)
            //        PARA.ApplyStyle("SubTotalStyle");

            //wTable.ApplyHorizontalMerge(SubTotalRow, 1, wTable.LastCell.GetCellIndex());
            #endregion merging section



            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);

            return total;
        }

        public double makeTermsAndCondition(string companyGroupId, string companyId, string plantId, string salesId, WordDocument document, DataTable dsTermsAndCondition)
        {
            string replaceString = "{TermsAndCondition}";


            IWParagraphStyle rightAlign = document.AddParagraphStyle("rightAlign");
            //Sets the formatting of the style
            rightAlign.CharacterFormat.FontSize = 8f;
            rightAlign.CharacterFormat.TextColor = Color.Black;
            rightAlign.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;

            int LasColumnIndex = 1;
            WTable wTable = new WTable(document);
            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex);
            WTableRow TemplateRow = wTable.Rows[0].Clone();

            #region column headers
            document.EnsureMinimal();

            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            WCharacterFormat DFontBold = new WCharacterFormat(document);
            DFontBold.FontSize = 8f;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("TERMS OF DELIVERY AND PAYMENT");
            range.ApplyCharacterFormat(FontBold);
            int colTermsAndCondition = COL; COL++;
            wTable.Rows[ROW].Cells[colTermsAndCondition].Width = 290;

            #endregion column headers
            double totalValue = 0;
            int sl = 0;
            int startRow = 0;
            for (int i = 0; i < dsTermsAndCondition.Rows.Count; i++)
            {
                ROW++;
                sl++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                TROW.Cells[colTermsAndCondition].AddParagraph().AppendText(dsTermsAndCondition.Rows[i]["RoWNo"].ToString() + "." + dsTermsAndCondition.Rows[i]["TermsAndConditions"].ToString()).ApplyCharacterFormat(DFontBold);

            }
            ROW++;

            #region Total
            //int TotalRow = ROW;
            //wTable.AddRow();
            //WTableRow _TROW = wTable.LastRow;

            //range.ApplyCharacterFormat(FontBold);
            #endregion Total
            ROW++;
            #region paragrpath formats

            IWParagraphStyle myStyle = document.AddParagraphStyle("ServiceStyle");
            //Sets the formatting of the style
            myStyle.CharacterFormat.FontSize = 8f;
            myStyle.CharacterFormat.TextColor = Color.Black;
            myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

            #endregion paragrpath formats

            #region merging section

            //tax codes merging (horizontal)
            ROW = 0;
            ROW++;
            #endregion merging section
            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, true, true);

            return 0;
        }

        private void DrawSOBreakdownData(DataTable dtData, IWorksheet sheet, ref int ROW, bool Matrix = true)
        {

            string FirstCharacteristicsName = "";
            string SecondCharacteristicsName = "";
            string ThirdCharacteristicsName = "";

            DataView dvDistinctCharName = new DataView(dtData.DefaultView.ToTable(true, "FirstCharacteristics")); //all yellow ??
            if (dvDistinctCharName.Count > 0)
                FirstCharacteristicsName = dvDistinctCharName[0]["FirstCharacteristics"].ToString();

            dvDistinctCharName = new DataView(dtData.DefaultView.ToTable(true, "SecondCharacteristics"));
            if (dvDistinctCharName.Count > 0)
                SecondCharacteristicsName = dvDistinctCharName[0]["SecondCharacteristics"].ToString();


            dvDistinctCharName = new DataView(dtData.DefaultView.ToTable(true, "ThirdCharacteristics"));
            if (dvDistinctCharName.Count > 0)
                ThirdCharacteristicsName = dvDistinctCharName[0]["ThirdCharacteristics"].ToString();


            if (FirstCharacteristicsName == "" && SecondCharacteristicsName == "" && ThirdCharacteristicsName == "")
                return;

            if (FirstCharacteristicsName != "" && SecondCharacteristicsName == "" && ThirdCharacteristicsName == "")
            {
                PrintSingleDimensionData(dtData, sheet, FirstCharacteristicsName, ref ROW);
            }

            if (FirstCharacteristicsName != "" && SecondCharacteristicsName != "" && ThirdCharacteristicsName == "")
            {
                if (Matrix == true)
                    PrintMatrixData(dtData, sheet, ref ROW);
                else
                    PrintLinearData(dtData, sheet, ref ROW);
            }


        }

        void PrintSingleDimensionData(DataTable dtData, IWorksheet sheet, string FirstCharacteristicsName, ref int ROW)
        {
            int COL = 1;
            sheet[ROW, COL].Text = FirstCharacteristicsName;  // Heading FirstCharacteristicsName ??? 
            int ColCharValue = COL;
            COL++;
            sheet[ROW, COL].Text = "Qty";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            int colQuantity = COL;

            sheet.Range[ROW, 1, ROW, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW, 1, ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
            ROW++;

            int StartRow = ROW;
            for (int i = 0; i < dtData.Rows.Count; i++)
            {
                sheet[ROW, ColCharValue].Text = dtData.Rows[i]["FirstCharacteristicsValue"].ToString();
                sheet[ROW, colQuantity].Number = clsStaticInfo.dbl(dtData.Rows[i]["Qty"].ToString());

                sheet[ROW, colQuantity].NumberFormat = clsStaticInfo.NumberFormat();
                ROW++;
            }
            sheet[ROW, colQuantity].Formula = "SUM(" + CellAddr(colQuantity, StartRow) + ":" + CellAddr(colQuantity, ROW - 1) + ")";
            sheet[StartRow, colQuantity, ROW, colQuantity].NumberFormat = clsStaticInfo.NumberFormat();
            sheet.Range[ROW, 1, ROW, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW, 1, ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
        }
        void PrintMatrixData(DataTable dtData, IWorksheet sheet, ref int ROW)
        {

            if (dtData.Rows.Count == 0)
                return;

            int COL = 0;

            COL++;  // 0+1=1 FG Color/FG Size Row 19
            sheet[ROW, COL].Text = dtData.Rows[0]["FirstCharacteristics"].ToString() + "/" + dtData.Rows[0]["SecondCharacteristics"].ToString();
            int colFirstChar = COL;// colFirstChar=FG Color/FG Size
            int colFirstSecCharValue = colFirstChar + 1;

            DataView dvDistinctSecondCharateristicsValues = new DataView(dtData.DefaultView.ToTable(true, "SecondCharacteristicsValue"));
            Dictionary<string, int> dicColumnIndex = new Dictionary<string, int>();
            for (int i = 0; i < dvDistinctSecondCharateristicsValues.Count; i++)
            {
                COL++;
                sheet[ROW, COL].Text = dvDistinctSecondCharateristicsValues[i]["SecondCharacteristicsValue"].ToString();
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                dicColumnIndex.Add(dvDistinctSecondCharateristicsValues[i]["SecondCharacteristicsValue"].ToString(), COL);


            }

            COL++;
            sheet[ROW, COL].Text = "Total Qty";
            sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
            int colTotal = COL;

            sheet.Range[ROW, 1, ROW, COL].CellStyle.Font.Bold = true; //row 19 of heading 
            sheet.Range[ROW, 1, ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
            int endCol = COL;
            ROW++;

            StringCollection strCol = new StringCollection();
            int StartRow = ROW; //row 20
            for (int i = 0; i < dtData.Rows.Count; i++)
            {
                if (strCol.Contains(dtData.Rows[i]["FirstCharacteristicsValue"].ToString()) == false)
                {
                    strCol.Add(dtData.Rows[i]["FirstCharacteristicsValue"].ToString());

                    sheet[ROW, colFirstChar].Text = dtData.Rows[i]["FirstCharacteristicsValue"].ToString();
                    dtData.DefaultView.RowFilter = "FirstCharacteristicsValue='" + dtData.Rows[i]["FirstCharacteristicsValue"].ToString() + "'";
                    for (int SL = 0; SL < dtData.DefaultView.Count; SL++)
                    {
                        sheet[ROW, dicColumnIndex[dtData.DefaultView[SL]["SecondCharacteristicsValue"].ToString()]].Number = clsStaticInfo.dbl(dtData.DefaultView[SL]["Qty"].ToString());
                        sheet[ROW, dicColumnIndex[dtData.DefaultView[SL]["SecondCharacteristicsValue"].ToString()]].NumberFormat = clsStaticInfo.NumberFormat();
                    }

                    sheet[ROW, colTotal].Formula = "SUM(" + CellAddr(colFirstSecCharValue, ROW) + ":" + CellAddr(colTotal - 1, ROW) + ")";
                    sheet[ROW, colTotal].NumberFormat = clsStaticInfo.NumberFormat();
                    sheet[ROW, colTotal].CellStyle.Font.Bold = true;


                    ROW++;
                }
            }

            sheet[ROW, colFirstChar].Text = "Total Qty"; //row 21
            sheet[ROW, colFirstChar].CellStyle.Font.Bold = true;
            for (int colSum = colFirstSecCharValue; colSum <= colTotal; colSum++)
            {
                sheet[ROW, colSum].Formula = "SUM(" + CellAddr(colSum, StartRow) + ":" + CellAddr(colSum, ROW - 1) + ")";

                sheet[ROW, colSum].NumberFormat = clsStaticInfo.NumberFormat();
            }
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;

            sheet[ROW, endCol].NumberFormat = clsStaticInfo.NumberFormat();
            sheet[StartRow, colFirstChar + 1, ROW, colTotal - 1].NumberFormat = clsStaticInfo.NumberFormat();

            sheet.Range[StartRow - 1, colTotal, ROW, colTotal].CellStyle.Font.Bold = true;
            sheet.Range[ROW, 1, ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
        }

        void PrintLinearData(DataTable dtData, IWorksheet sheet, ref int ROW)
        {

            if (dtData.Rows.Count == 0)
                return;

            int COL = 0;
            COL++;
            sheet[ROW, COL].Text = dtData.Rows[0]["FirstCharacteristics"].ToString();
            int colFirstChar = COL;
            COL++;

            sheet[ROW, COL].Text = dtData.Rows[0]["SecondCharacteristics"].ToString();
            int colSecondChar = COL;
            COL++;

            sheet[ROW, COL].Text = "Qty";
            sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
            sheet[ROW, COL].CellStyle.Font.Bold = true;
            int colQuantity = COL;

            sheet.Range[ROW, 1, ROW, COL].CellStyle.Font.Bold = true;
            sheet.Range[ROW, 1, ROW, COL].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
            int endCol = COL;
            ROW++;

            StringCollection strCol = new StringCollection();
            int StartRow = ROW;
            for (int i = 0; i < dtData.Rows.Count; i++)
            {
                sheet[ROW, colFirstChar].Text = dtData.Rows[i]["FirstCharacteristicsValue"].ToString();
                sheet[ROW, colSecondChar].Text = dtData.Rows[i]["SecondCharacteristicsValue"].ToString();
                sheet[ROW, colQuantity].Number = clsStaticInfo.dbl(dtData.Rows[i]["Qty"].ToString());

                ROW++;

            }

            sheet[ROW, colFirstChar].Text = "Total Qty";
            sheet[ROW, colFirstChar].CellStyle.Font.Bold = true;

            sheet[ROW, colQuantity].Formula = "SUM(" + CellAddr(colQuantity, StartRow) + ":" + CellAddr(colQuantity, ROW - 1) + ")";
            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
            sheet[ROW, colQuantity].NumberFormat = clsStaticInfo.NumberFormat();

            sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_25_percent;
            sheet.Range[StartRow - 1, colQuantity, ROW, colQuantity].CellStyle.Font.Bold = true;
        }
        private string CellAddr(int Col, int Row)
        {
            return clsStaticInfo.GetxlsCol(Col) + Row.ToString();
        }

        #endregion

        #region Contract Summary Report
        [HttpGet, Authorize]
        public ActionResult GetContractDetailsReport(string ContractId, bool isMatrix)
        {

            try
            {
                // if (string.IsNullOrEmpty(MasterLCList))
                //   throw new Exception("Please select at least one master Order");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;



                ExcelEngine excelEngine = new ExcelEngine();

                IWorkbook workbook = GetContractDetailsReportList(ContractId, isMatrix, identity.CompanyId);

                string strFileName = "Contract Summary.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);

            }


            return null;
        }

        private IWorkbook GetContractDetailsReportList(string ContractId, bool isMatrix, string companyId)
        {
            ExcelEngine excelEngine = new ExcelEngine();
            //Instantiate the Excel application object
            IApplication application = excelEngine.Excel;
            ReportUtility reportUtility = new ReportUtility();
            //Set the default application version
            application.DefaultVersion = ExcelVersion.Excel2013;

            //Load the existing Excel workbook into IWorkbook
            IWorkbook workbook = application.Workbooks.Create(1);

            //Get the first worksheet in the workbook into IWorksheet
            IWorksheet worksheet = workbook.Worksheets[0];
            try
            {
                DataTable dtOrderMaster = _sqlRepository.GetDataTable(@"select mo.Id, mo.type, b.UserName as Buyer, p.UserName as Customer,mo.PartyId
				               ,format(c.AddedDate,'dd-MMM-yyyy') as ContractDate, c.ContractNo,SO.ContractId,   c.MasterLCId, ml.LCRef As MasterLCNo, moi.id as MasterOrderItemNo
                                ,  mo.OrderYear as Year, mo.TotalQty as TotalQuantity
                                    , uom.UserName as UnitOfMeasurement, mo.NoOfLineItem, mo.OrderWastagePercentage
                                    , mo.ExtraOrderPercentage, mo.BuyerReferenceNo, mo.OwnReferenceNo, BDept.UserName as BuyerDepartment
                                    , BDev.UserName as BuyerDevision, MoCur.Code MasterOrderCurrency
			    	
					                 from trn.MasterOrder MO 
                                    left join scs.Currency MoCur on MoCur.id = mo.CurrencyId 
                                    left join scs.UnitOfMeasurement UOM on uom.id = mo.TotalQtyUOMId 
                                    left join hkp.buyer B on b.id = mo.buyerid 
                                    left join hkp.party p on p.id = mo.partyid 
                                    left join hkp.BuyerDepartment BDept on BDept.id = mo.buyerDepartmentid 
                                    left join HKP.buyerdivision BDev on BDev.id = mo.BuyerDivisionId 
					                left join trn.MasterOrderItem moi on moi.MasterOrderId = mo.Id
					                left join trn.SalesOrder SO ON SO.MasterOrderItemId=MOI.Id
					                left join dbo.Contract C on c.id = SO.ContractId
					                left join MasterLC ml on ml.Id = c.MasterLCId
                                    where c.Id='" + ContractId + "'");

                if (dtOrderMaster.Rows.Count == 0)
                    throw new Exception("No data found");

                DataTable dtMasterOrderItem = _sqlRepository.GetDataTable(@"
                select mo.BuyerReferenceNo as BuyerOrder, mo.OwnReferenceNo as OwnOrder
				, moi.BuyerReferenceNo as BuyerItem
                , moi.OwnReferenceNo as OwnItem ,cpo.PONumber, moi.BuyerItemDescription as ItemDetails, format(so.DeliveryDate,'dd-MMM-yyyy')as DeliveryDate
				, so.Qty as Quantity ,so.Rate, Amount=so.Qty*so.Rate
				--,b.UserName as Buyer ,mm.UserName as Material ,mma.StandardName as Article, moi.Type, moi.MasterOrderId, so.MasterOrderItemId
                --, os.UserName as OrderStatus,d.UserName as Destination
                --, so.UpCharge, so.MainRawMaterialInhouseDate, so.Description
                --,so.SOType, oc.username as OrderCategory
                --, sm.UserName as ShipmentMode
                --, so.Discount,so.CM,so.LSD, so.OtherRawMaterialInhouseDate , so.Reason , so.CommitmentDate

                --,c1.username as FirstCharacteristics, isnull(fcs.ValueFreeText,CV1.UserName) as FirstCharacteristicsValue
                --,c2.username as SecondCharacteristics ,isnull(SCS.ValueFreeText, CV2.UserName) as SecondCharacteristicsValue
                --,c3.username as ThirdCharacteristics , isnull(ThirdCS.ValueFreeText,CV3.UserName) as ThirdCharacteristicsValue
                --,case when isnull(thirdCs.Id,'')<>'' THEN ThirdCs.Qty
                --ELSE case when isnull(scs.id,'')<>'' THEN scs.Qty
                --ELSE case when isnull(fcs.Id,'')<>'' THEN fcs.Qty
                --ELSE 0 END END END AS Qty, 
				,so.id as SalesOrderNo
                from trn.SalesOrder SO
		
                left join trn.masterorderitem moi on moi.id= so.masterorderitemid
				left join TRN.MasterOrder mo  on mo.id=moi.MasterOrderId
				--left join MST.MaterialMaster MM on mm.id = moi.MaterialMasterId
                --left join MST.MaterialMasterArticle mma on mma.id= moi.ArticleId
			 --left join hkp.buyer B on b.id = mo.buyerid 
             -- left join HKP.OrderCategory OC on oc.id = so.OrderCategoryId
              --  left join hkp.OrderStatus OS on os.id = so.OrderStatusId
              --  left join mst.shipMode SM on sm.id = so.shipmentModeId
              --left join mst.Destination d on d.id =so.DestinationId
                left join trn.CustomerPO CPO on cpo.id =so.CustomerPOId

                --left join TRN.FirstCharacteristics FCS on fcs.SalesOrderId = so.id
                --left join hkp.Characteristics C1 on c1.id = fcs.CharacteristicsId
                --left join HKP.CharacteristicsValue CV1 on cv1.id= fcs.CharacteristicsValueId

                --left join TRN.SecondCharacteristics SCS on scs.SalesOrderId=so.id and scs.FirstCharacteristicsId=fcs.Id
                --left join hkp.Characteristics C2 on c2.id = scs.CharacteristicsId
                --left join HKP.CharacteristicsValue CV2 on cv2.id= scs.CharacteristicsValueId

                --left join TRN.ThirdCharacteristics ThirdCS on ThirdCS.SalesOrderId=so.id and scs.id=ThirdCS.SecondCharacteristicsId
                --left join hkp.Characteristics C3 on c3.id = ThirdCS.CharacteristicsId
                --left join HKP.CharacteristicsValue CV3 on CV3.id= ThirdCS.CharacteristicsValueId
                  where SO.ContractId='" + ContractId + "'");

                worksheet.Name = "ContractSummaryReport";

                //int ROW = 4; int COL = 1;
                int ROW = 4; int COL = 1;

                DataTable dtCustomerDetail = null;
                DataTable dtVendorDetail = null;

                dtCustomerDetail = clsCon.dtConsigneeNameAddress(dtOrderMaster.Rows[0]["PartyId"].ToString());
                dtVendorDetail = clsCon.dtNameAddressVendor(companyId);

                string customerName = dtCustomerDetail.Rows[0]["CustomerName"].ToString();
                string vendorName = dtVendorDetail.Rows[0]["CompanyName"].ToString();


                worksheet[ROW, COL].Text = "Contract NO#" + dtOrderMaster.Rows[0]["ContractNo"].ToString();
                worksheet[ROW, COL].CellStyle.Font.Bold = true;
                ROW++;
                worksheet[ROW, COL].Text = "Master LC NO : " + dtOrderMaster.Rows[0]["MasterLCNo"].ToString();
                worksheet[ROW, COL].CellStyle.Font.Bold = true;
                ROW++;
                ROW++;


                worksheet[ROW, COL].Text = "Purchase Contract Between: " + customerName + " and " + vendorName + " Signed after the following understainds:";
                worksheet[ROW, COL].CellStyle.Font.Bold = true;
                //worksheet[ROW, COL].ColumnWidth = 20;

                //worksheet[ROW, 9].Text = "Contract Date: " + dtOrderMaster.Rows[0]["ContractDate"].ToString();
                //worksheet[ROW, 9].CellStyle.Font.Bold = true;
                reportUtility.SetMasterHeaderText(ref worksheet, ROW, 9, "Contract Date");
                reportUtility.SetText(ref worksheet, ROW, 10, dtOrderMaster.Rows[0]["ContractDate"].ToString());

                ROW++;
                ROW++;

                ROW++;


                //int MasterOrderDetailsStartRow = ROW;
                worksheet[ROW, COL].Text = "Vendor Name and Address:";
                worksheet[ROW, COL].CellStyle.Font.Bold = true;

                //worksheet[ROW, COL + 2].Text = "Vendor Name and Address";
                //worksheet[ROW, COL + 2].CellStyle.Font.Bold = true;
                //ROW++;
                worksheet[ROW, COL + 2].Text = dtVendorDetail.Rows[0]["CompanyName"].ToString() + Environment.NewLine + dtVendorDetail.Rows[0]["Address1"].ToString()
                    + " " + dtVendorDetail.Rows[0]["DistrictName"].ToString() + " " + dtVendorDetail.Rows[0]["CountryName"].ToString()
                    + " " + dtVendorDetail.Rows[0]["Phone"].ToString();
                worksheet[ROW, COL + 2].CellStyle.Font.Bold = true;
                worksheet.Range[ROW, COL + 2, ROW, COL + 2 + 1].Merge();
                worksheet[ROW, COL].RowHeight = 50;

                ROW++;

                worksheet[ROW, COL].Text = "Name and address  of Consignee:";
                worksheet[ROW, COL].CellStyle.Font.Bold = true;
                worksheet[ROW, COL + 2].Text = dtCustomerDetail.Rows[0]["CustomerName"].ToString() + Environment.NewLine + dtCustomerDetail.Rows[0]["Address1"].ToString() + Environment.NewLine + dtCustomerDetail.Rows[0]["CountryName"].ToString();

                worksheet[ROW, COL + 2].CellStyle.Font.Bold = true;
                worksheet.Range[ROW, COL + 2, ROW, COL + 2 + 1].Merge();
                worksheet[ROW, COL].RowHeight = 50;
                ROW++;
                //worksheet[ROW, COL + 2].Text = "";// Customer address
                //worksheet[ROW, COL + 2].CellStyle.Font.Bold = true;
                //worksheet[ROW, COL].ColumnWidth = 20;



                //int MasterOrderDetailsStartRow = ROW;
                //worksheet[ROW, COL].Text = "Contract Summary:";
                //worksheet[ROW, COL].CellStyle.Font.Bold = true;
                ////worksheet[ROW, COL].ColumnWidth = 20;
                //ROW++;

                //int leftColumnCaption = COL;
                //int leftColumnValue = leftColumnCaption + 1;

                ////int MiddleColumnCaption = leftColumnValue + 2;
                //int MiddleColumnCaption = leftColumnValue + 1;
                //int MiddleColumnValue = MiddleColumnCaption + 1;

                //int RightColumnCaption = MiddleColumnValue + 1;
                //int RightColumnValue = RightColumnCaption + 1;

                //worksheet[ROW, leftColumnCaption].Text = "ContractNo#";
                //worksheet[ROW, leftColumnValue].Text = dtOrderMaster.Rows[0]["ContractNo"].ToString();
                //worksheet.Range[ROW, leftColumnValue, ROW, leftColumnValue].CellStyle.Font.Color = ExcelKnownColors.Blue;
                ////worksheet[ROW, leftColumnValue].ColumnWidth = 20;
                //worksheet.Range[ROW, leftColumnCaption, ROW, leftColumnValue].CellStyle.Font.Bold = true;
                ////worksheet[ROW, leftColumnCaption].ColumnWidth = 20;


                //worksheet[ROW, RightColumnCaption].Text = "Master LC No.";
                //worksheet[ROW, RightColumnValue].Text = dtOrderMaster.Rows[0]["MasterLCNo"].ToString();
                //worksheet[ROW, RightColumnCaption].CellStyle.Font.Bold = true;
                //ROW++;

                //worksheet[ROW, leftColumnCaption].Text = "Customer";
                //worksheet[ROW, leftColumnValue].Text = dtOrderMaster.Rows[0]["Customer"].ToString();
                //worksheet.Range[ROW, leftColumnCaption, ROW, leftColumnCaption].CellStyle.Font.Bold = true;

                //worksheet[ROW, RightColumnCaption].Text = "Buyer";
                //worksheet[ROW, RightColumnValue].Text = dtOrderMaster.Rows[0]["Buyer"].ToString();
                //worksheet.Range[ROW, RightColumnCaption, ROW, RightColumnValue].CellStyle.Font.Bold = true;
                //int endRow = ROW;
                //ROW++;

                //worksheet.Range[MasterOrderDetailsStartRow, 1, endRow, RightColumnValue].BorderAround(ExcelLineStyle.Thin);
                //worksheet.Range[MasterOrderDetailsStartRow, 1, endRow, RightColumnValue].BorderInside(ExcelLineStyle.Thin);
                //// worksheet.Range[MasterOrderDetailsStartRow, leftColumnCaption, ROW, RightColumnValue].CellStyle.Interior.ColorIndex = ExcelKnownColors.Custom44;
                ROW += 2;

                //Master Order & Item....................................................................................................
                int strCaptionRow = ROW;

                worksheet[ROW, COL].Text = "Buyer Order#";
                int colBuyerOrder = COL;
                worksheet[ROW, COL].ColumnWidth = 13;
                worksheet[ROW, COL].CellStyle.Font.Bold = true;
                COL++;

                worksheet[ROW, COL].Text = "Own Order#";
                int colOwnOrder = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                worksheet[ROW, COL].CellStyle.Font.Bold = true;
                COL++;

                worksheet[ROW, COL].Text = "Buyer Item#";
                int colBuyerItem = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                worksheet[ROW, COL].CellStyle.Font.Bold = true;
                COL++;

                worksheet[ROW, COL].Text = "Own Item Ref#";
                int colOwnItem = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                worksheet[ROW, COL].CellStyle.Font.Bold = true;
                COL++;

                worksheet[ROW, COL].Text = "Buyer PO No.";
                int colPONumber = COL;
                worksheet[ROW, COL].ColumnWidth = 13;
                worksheet[ROW, COL].CellStyle.Font.Bold = true;
                COL++;

                worksheet[ROW, COL].Text = "Item Details";
                int colItemDetails = COL;
                worksheet[ROW, COL].ColumnWidth = 20;
                worksheet[ROW, COL].CellStyle.Font.Bold = true;
                COL++;

                worksheet[ROW, COL].Text = "Delevery Date";
                int colDeliveryDate = COL;
                worksheet[ROW, COL].ColumnWidth = 15;
                worksheet[ROW, COL].CellStyle.Font.Bold = true;
                COL++;

                worksheet[ROW, COL].Text = "Qty";
                int colQuantity = COL;
                worksheet[ROW, COL].ColumnWidth = 10;
                worksheet[ROW, COL].CellStyle.Font.Bold = true;
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                COL++;

                worksheet[ROW, COL].Text = "Rate";
                int colRate = COL;
                worksheet[ROW, COL].ColumnWidth = 12;
                worksheet[ROW, COL].CellStyle.Font.Bold = true;
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                COL++;


                worksheet[ROW, COL].Text = "Amount";
                int colAmount = COL;
                worksheet[ROW, COL].ColumnWidth = 10;
                worksheet[ROW, COL].CellStyle.Font.Bold = true;
                worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignRight;
                ROW++;
                int strRow = ROW;
                for (int i = 0; i < dtMasterOrderItem.Rows.Count; i++)
                {
                    worksheet[ROW, colBuyerOrder].Text = dtMasterOrderItem.Rows[i]["BuyerOrder"].ToString();
                    worksheet[ROW, colOwnOrder].Text = dtMasterOrderItem.Rows[i]["OwnOrder"].ToString();
                    worksheet[ROW, colBuyerItem].Text = dtMasterOrderItem.Rows[i]["BuyerItem"].ToString();
                    worksheet[ROW, colOwnItem].Text = dtMasterOrderItem.Rows[i]["OwnItem"].ToString();
                    worksheet[ROW, colPONumber].Text = dtMasterOrderItem.Rows[i]["PONumber"].ToString();
                    worksheet[ROW, colItemDetails].Text = dtMasterOrderItem.Rows[i]["ItemDetails"].ToString();
                    worksheet[ROW, colDeliveryDate].Text = dtMasterOrderItem.Rows[i]["DeliveryDate"].ToString();
                    worksheet[ROW, colQuantity].Number = clsStaticInfo.dbl(dtMasterOrderItem.Rows[i]["Quantity"].ToString());
                    worksheet[ROW, colQuantity].NumberFormat = clsStaticInfo.NumberFormat();

                    worksheet[ROW, colRate].Number = clsStaticInfo.dbl(dtMasterOrderItem.Rows[i]["Rate"].ToString());
                    worksheet[ROW, colRate].NumberFormat = clsStaticInfo.NumberFormat();
                    worksheet[ROW, colRate].NumberFormat = "#,##0.000;(#,##0.000)";

                    worksheet[ROW, colAmount].Number = clsStaticInfo.dbl(dtMasterOrderItem.Rows[i]["Amount"].ToString());
                    worksheet[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat();
                    worksheet[ROW, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";



                    //worksheet[ROW, colAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    //worksheet[ROW, colAmount].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    ROW++;

                }

                var lastRow = ROW;
                reportUtility.SetHeaderText(ref worksheet, lastRow, 7, "Total :", ExcelHAlign.HAlignRight);
                worksheet.Range[ROW, 7].BorderAround(ExcelLineStyle.Thin);
                worksheet.Range[ROW, 1, ROW, 7].Merge();
                //reportUtility.SetText(ref sheet, lastRow, 5, "Total :", true);


                worksheet[ROW, colQuantity].Formula = "SUM(" + CellAddr(colQuantity, strRow) + ":" + CellAddr(colQuantity, ROW - 1) + ")";
                worksheet[ROW, colQuantity].NumberFormat = clsStaticInfo.NumberFormat();
                worksheet[ROW, colQuantity].CellStyle.Font.Bold = true;
                worksheet[ROW, colQuantity].HorizontalAlignment = ExcelHAlign.HAlignRight;


                worksheet[ROW, colAmount].Formula = "SUM(" + CellAddr(colAmount, strRow) + ":" + CellAddr(colAmount, ROW - 1) + ")";
                worksheet[ROW, colAmount].NumberFormat = clsStaticInfo.NumberFormat();
                worksheet[ROW, colAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                worksheet[ROW, colAmount].CellStyle.Font.Bold = true;
                worksheet[ROW, colAmount].HorizontalAlignment = ExcelHAlign.HAlignRight;

                worksheet.Range[strCaptionRow, 1, ROW, colAmount].BorderAround(ExcelLineStyle.Thin);
                worksheet.Range[strCaptionRow, 1, ROW, colAmount].BorderInside(ExcelLineStyle.Thin);


                //int endCol = RightColumnValue;
                int endCol = COL;

                worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                worksheet.UsedRange.CellStyle.Font.Size = 8f;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //ReportUtility reportUtility = new ReportUtility();
                // reportUtility.PlantHeader(ref worksheet, endCol, "Contract NO#" + dtOrderMaster.Rows[0]["ContractNo"].ToString(), identity.PlantId);
                // reportUtility.PlantHeaderWithOutLogo(ref worksheet, endCol, identity.CompanyId, identity.PlantId);
                reportUtility.PageSetup(ref worksheet, 5, ExcelPageOrientation.Landscape);
                //  worksheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                worksheet.Range[1, 1, 5, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                worksheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                worksheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                //worksheet.IsGridLinesVisible = false;
                return workbook;

            }
            catch (Exception ex)
            {
                throw (ex);

            }




        }


        #endregion

        #region ContractItem

        [HttpGet, Authorize]
        public JsonResult GetContractItemDataList(string contractId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                return Json(clsCon.GetContractItemDataList(identity.CompanyId, identity.PlantId, contractId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetMasterOrderDataList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                return Json(clsCon.GetMasterOrderDataList(identity.CompanyId, identity.PlantId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetContractItemCPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ContractItem", out sID);
            return sID;
        }

        [HttpPost, Authorize]
        public JsonResult CreateContractItem(List<Dictionary<string, object>> data, string contractId)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsChild;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.ContractItems where  contractId='" + contractId + "'", out dsChild, false, "1");

                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = GetContractItemCPK();
                            item["ContractId"] = contractId;

                            AddNewRow(dsChild.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsChild);
                }


                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }

        [HttpPost]
        public ActionResult DeleteContractItems(string id)
        {
            DeleteContractItemsData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }


        public void DeleteContractItemsData(string id)
        {
            string strSQL, strDSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[ContractItems] WHERE Id = '" + id + "'";

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

        #endregion

        [HttpPost, Authorize]
        public ActionResult GetContarctReport()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //var workbook = clsCon.GetContarctWorkbook(identity.CompanyId);
                var workbook = clsCon.GetContarctWorkbookExcel(identity.CompanyGroupId, identity.CompanyId, identity.PlantId);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "BANK LIEN Report.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);


                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost, Authorize]
        public JsonResult SaveMasterLCAddInfo(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                //con.OpenDataSetThroughAdapter("select * from dbo.MasterLCAddInfo where Description='" + data["Description"] + "'  AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //    throw new Exception("Description already exists!!!");

                con.OpenDataSetThroughAdapter("select * from dbo.MasterLCAddInfo where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "MasterLCAddInfo", out _Id);

                    data["Id"] = _Id;
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


                return Json(new { Error = false, Sequence = clsCon.GetSequence(_Id), Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost, Authorize]
        public JsonResult SaveMasterlcclause(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                //con.OpenDataSetThroughAdapter("select * from dbo.MasterLCAddInfo where Description='" + data["Description"] + "'  AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //    throw new Exception("Description already exists!!!");

                con.OpenDataSetThroughAdapter("select * from dbo.lcclauses where MasterLCId='" + data["MasterLCId"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "lcclauses", out _Id);

                    data["Id"] = _Id;
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


                return Json(new { Error = false, Sequence = clsCon.GetSequence(_Id), Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence(string masterLcId)
        {
            return Json(clsCon.GetSequence(masterLcId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMasterLCAddInfoData(string masterLcId)
        {
            try
            {
                return Json(clsCon.GetMasterLCAddInfoData(masterLcId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private string GetMasterLCTNCPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "MasterLCTermsAndConditions", out sID);
            return sID;
        }


        [HttpPost, Authorize]
        public JsonResult CreateMasterLCTNC(List<Dictionary<string, object>> data, string masterLCId)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsChild;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.MasterLCTermsAndConditions where  MasterLCId='" + masterLCId + "'", out dsChild, false, "1");

                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = GetMasterLCTNCPK();

                            AddNewRow(dsChild.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsChild);
                }


                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }

        [HttpPost, Authorize]
        public JsonResult SaveTNCRowData(Dictionary<string, object> data)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsChild;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.MasterLCTermsAndConditions where  Id='" + data["Id"] + "'", out dsChild, false, "1");

                if (data != null)
                {
                    DataView dv = new DataView(dsChild.Tables[0]);
                    dv.RowFilter = "Id='" + data["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        data["Id"] = GetMasterLCTNCPK();
                        AddNewRow(dsChild.Tables[0], data);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, data);
                    }

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsChild);
                }


                return Json(new { Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetMasterLCTermsAndConditionsList(string masTerLCId)
        {
            return Json(clsCon.GetMasterLCTermsAndConditionsList(masTerLCId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetMasterLCLCClausesList(string masTerLCId)
        {
            return Json(clsCon.GetMasterLCLCClausesList(masTerLCId), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult DeleteMasterLCTermsAndConditions(string id)
        {
            DeleteMasterLCTermsAndConditionsData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }


        public void DeleteMasterLCTermsAndConditionsData(string id)
        {
            string strSQL, strDSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[MasterLCTermsAndConditions] WHERE Id = '" + id + "'";

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

        [Authorize, HttpPost]
        public ActionResult RemoveAddInfo(string id)
        {
            RemoveAddInfoData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }


        public void RemoveAddInfoData(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM dbo.MasterLCAddInfo WHERE Id = '" + id + "'";

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

        [Authorize, HttpPost]
        public ActionResult DeleteBank(string id)
        {
            DeleteNegotiatingBankData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }


        public void DeleteNegotiatingBankData(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM dbo.NegotiatingBank WHERE Id = '" + id + "'";

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

        private string GetNegotiatingBankPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "NegotiatingBank", out sID);
            return sID;
        }


        [HttpGet, Authorize]
        public ActionResult GetNegotiatingBankList()
        {
            return Json(clsCon.GetNegotiatingBankList(), JsonRequestBehavior.AllowGet);
        }

         [HttpGet, Authorize]
        public ActionResult GetNegotiatingContractBankList()
        {
            return Json(clsCon.GetNegotiatingContractBankList(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult GetNegotiatingBankDataList(string column, string value)
        {
            return Json(clsCon.GetNegotiatingBankDataList(column,value), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult SaveNegotiatingBank(Dictionary<string, object> data)
        {
            try
            {
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsChild;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.NegotiatingBank where  Id='" + data["Id"] + "'", out dsChild, false, "1");

                if (data != null)
                {
                    DataView dv = new DataView(dsChild.Tables[0]);
                    dv.RowFilter = "Id='" + data["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        data["Id"] = GetNegotiatingBankPK();
                        AddNewRow(dsChild.Tables[0], data);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, data);
                    }

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsChild);
                }


                return Json(new { Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }
        }

        [HttpPost, Authorize]
        public ActionResult GetLCPendingReport()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var workbook = clsCon.GetLCPendingReportWorkbookExcel(identity.CompanyGroupId, identity.CompanyId, identity.PlantId);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + " " + "LCPendingReport.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);


                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

    }

    class clsStdLib
    {
        public static string passWord = "prodDisplay";
        public clsStdLib()
        {

        }
        public enum mType
        {
            Error,
            Success,
            Information
        }
        public static bool passwordGet = true;
        public static string[] sMonth = new string[] { "<Unselect>", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

        public static string DataRankNames(int dayNo)
        {

            if (dayNo <= 0)
                return "";

            //if (dayNo.ToString().Length > 1)
            //{
            //    string Right = dayNo.ToString().Substring(dayNo.ToString().Length - 2, 2);
            //    if (clsStdLib.dbl(Right) >= 10 && clsStdLib.dbl(Right) <= 20)
            //        return dayNo + "th";
            //}

            string RightString = dayNo.ToString().Substring(dayNo.ToString().Length - 1, 1);
            switch (RightString)
            {
                case "1":
                    return dayNo + "st";
                case "2":
                    return dayNo + "nd";
                case "3":
                    return dayNo + "rd";
                default:
                    return dayNo + "th";

            }

        }

        #region date related
        public static readonly string dateFormat = "dd-MMM-yyyy";
        public static readonly string sqliteDateFormat = "yyyy-MM-dd";
        public static readonly string AppToDBdateFormat = "yyyy-MM-dd hh:mm:ss";
        public static bool IsDateOK(string strdate)
        {
            try
            {
                if (strdate.Length != 11)
                {
                    return false;
                }
                if (strdate.Substring(2, 1) != "-" && strdate.Substring(6, 1) != "-")
                {
                    return false;
                }
                System.DateTime myDt = System.Convert.ToDateTime(strdate);
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
            finally
            {
                //
            }
        }// end function
        private static bool DateOkCheck(string strdate)
        {
            try
            {
                System.DateTime myDt = System.Convert.ToDateTime(strdate);
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
            finally
            {
                //
            }
        }// end function
        public static object chk_NullDateData(object dateValue)
        {
            if (DateOkCheck("" + dateValue.ToString()) == false)
            {
                dateValue = "";
            }

            if (("" + dateValue.ToString()) == "")
            {
                System.DateTime dt = new System.DateTime(1901, 1, 1);
                dateValue = (object)dt;
            }
            return (object)dateValue;
        }
        public static System.DateTime AppDateConvert(object dateValue, string input_date_format, string output_date_format)
        {
            string strDate = null;
            dateValue = chk_NullDateData(dateValue);
            strDate = dateValue.ToString();
            if (strDate != "")
            {
                if (input_date_format.Trim() != "")
                {
                    if (output_date_format.Trim() != "")
                    {
                        System.Globalization.DateTimeFormatInfo InputFormat = new System.Globalization.DateTimeFormatInfo();
                        InputFormat.ShortDatePattern = input_date_format;
                        System.DateTime myDt = System.Convert.ToDateTime(strDate, InputFormat);
                        strDate = myDt.ToString(output_date_format);
                    }
                }
            }
            return System.Convert.ToDateTime(strDate);
        }// End of function
        public static Object DateData_AppToDB(object dateValue, string DB_Level_date_format)
        {
            if (string.IsNullOrEmpty((string)dateValue))
                return DBNull.Value;

            string strDate = null;
            strDate = dateValue.ToString();
            if (DB_Level_date_format != "")
            {
                // Collecting the user terminal set format 
                System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
                strDate = AppDateConvert(strDate, USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString(), DB_Level_date_format).ToString();
            }

            string m = System.Convert.ToDateTime(strDate).ToString(AppToDBdateFormat);
            return System.Convert.ToDateTime(strDate).ToString(AppToDBdateFormat);


        }// End of function
        public static System.DateTime DateData_DBToApp(object dateValue)
        {
            string strDate = null;
            strDate = dateValue.ToString();

            System.Globalization.DateTimeFormatInfo myDBDateFormat = new System.Globalization.CultureInfo("en-US", false).DateTimeFormat;
            strDate = DateData_DBToApp(dateValue, myDBDateFormat.ShortDatePattern.ToString()).ToString();
            return System.Convert.ToDateTime(strDate);
        }// End function
        public static System.DateTime DateData_DBToApp(object dateValue, string DB_Level_date_format)
        {
            string strDate = null;
            strDate = dateValue.ToString();
            if (DB_Level_date_format != "")
            {
                // Collecting the user terminal set format 
                System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
                strDate = AppDateConvert(strDate, DB_Level_date_format, USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString()).ToString();
            }
            return System.Convert.ToDateTime(strDate);
        }// End of function
        public static String makeBaseBlank(object dateValue)
        {
            System.DateTime dt;
            dt = System.Convert.ToDateTime(dateValue.ToString());
            if (dt.Year == 1901)
            {
                return "";
            }
            else
            {
                return dateValue.ToString();
            }
        }// End of function
        ///<summary>
        ///return day difference in integer. 
        ///    Example 1: firstDate[Less Than]lastDate returns positive value
        ///    Example 2: firstDate>lastDate returns negative value
        ///    Example 3: firstDate=lastDate returns 0 [zero]**/
        /// </summary>
        public static int dateDiff(string firstDate, string lastDate)
        {

            int difference = 0;
            try
            {
                firstDate = Convert.ToDateTime(firstDate).ToString("dd-MMM-yyyy");
                lastDate = Convert.ToDateTime(lastDate).ToString("dd-MMM-yyyy");

                if (IsDateOK(firstDate) == false)
                {
                    Exception ex = new Exception("Invalid [First Date]");
                    throw (ex);
                }
                if (IsDateOK(lastDate) == false)
                {
                    Exception ex = new Exception("Invalid [Last Date]");
                    throw (ex);
                }
                DateTime dateFirstDate = Convert.ToDateTime(firstDate);
                DateTime dateLastDate = Convert.ToDateTime(lastDate);
                TimeSpan TimeSpan = dateLastDate.Subtract(dateFirstDate);


                difference = TimeSpan.Days;
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return difference;
        }



        public static string getSqliteDate(string standardDate)
        {
            return (Convert.ToDateTime(standardDate).ToString(sqliteDateFormat));
        }
        public static string getStandardDateFromSqliteDate(string SqliteDate)
        {
            if (SqliteDate.Length != 10)
                return "";
            if (SqliteDate.Split('-').Length != 3)
                return "";
            //many things to validate 
            //but i have less time :)
            string month = ValidLength(sMonth[Convert.ToInt32(SqliteDate.Split('-')[1])], 3).ToString();


            return SqliteDate.Split('-')[2] + "-" + month + "-" + SqliteDate.Split('-')[0];
        }
        #endregion date related

        #region numeric
        public static bool IsNumeric(string strNumber)
        {
            Double d;
            System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
            if (strNumber.Length == 0)
            {
                return false;
            }
            return Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d);
        } // End Function
        public static string GetNumericData(string strNumber)
        {
            double d;
            strNumber = strNumber.Replace(",", "");
            System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
            if (strNumber.Trim() == "")
            { return "0"; }
            else if (System.Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
            {
                return strNumber;
            }
            else
            {
                return "0";
            }
        }// end function
        public static string GetNumericDataInDecimalFormat(string strNumber, int precision)
        {
            if (precision < 1)
                return strNumber;

            string s_precision = new String('0', precision);

            double d;
            System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
            if (strNumber.Trim() == "")
            { return "0." + s_precision; }
            else if (System.Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
            {
                return string.Format("{0:0." + s_precision + "}", d);
            }
            else
            {
                return "0." + s_precision;
            }
        }// end function
        public static double dbl(string d)
        {
            return Convert.ToDouble(GetNumericData(d));

        }
        public static int Percentage(int total, double percentage)
        {
            return (int)(total * (percentage / 100));

        }
        //validation
        public static void numericValidation(string value, bool isMandatory, bool isInteger, bool negativeAllowed, string fieldName)
        {

            try
            {



                if (isMandatory == true)
                {
                    if (value.Trim() == "")
                    {
                        Exception ex = new Exception("please insert [" + fieldName + "]");
                        throw (ex);
                    }
                    if (Convert.ToDouble(GetNumericData(value.Trim())) == 0)
                    {
                        Exception ex = new Exception("please insert [" + fieldName + "]");
                        throw (ex);
                    }

                    if (value.Trim() != "")
                    {
                        if (IsNumeric(value.Trim()) == false)
                        {
                            Exception ex = new Exception("Invalid numeric value [" + value + "] for the field [" + fieldName + "]");
                            throw (ex);
                        }
                    }
                }

                if (value.Trim() != "")
                {
                    if (IsNumeric(value.Trim()) == false)
                    {
                        Exception ex = new Exception("Invalid numeric value [" + value + "] for the field [" + fieldName + "]");
                        throw (ex);
                    }
                    if (isInteger == true)
                    {

                        if (isInt(value.Trim()) == false)
                        {
                            Exception ex = new Exception("Number must be integer for the field [" + fieldName + "]");
                            throw (ex);
                        }

                    }
                    if (negativeAllowed == false)
                    {
                        if (Convert.ToDouble(GetNumericData(value.Trim())) < 0)
                        {
                            Exception ex = new Exception("Negative values are not allowed for the field [" + fieldName + "]");
                            throw (ex);
                        }
                    }
                }



            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }


        }

        ///<summary>
        ///check whether a value is integer or not returns true if integer, 
        ///false if floating or string containing alpahnumeric
        ///</summary>
        public static bool isInt(string num)
        {

            bool isInt;
            int number;
            try
            {
                isInt = System.Int32.TryParse(num, out number);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
            return isInt;
        }


        #endregion numeric

        #region string

        public static readonly string excelNegativePOsitiveSign = @"+#,##0.00;-#,##0.00;* ??;@";
        public static readonly string NegativePOsitiveSign = @"+#,##0.00;-#,##0.00;0";
        public static readonly string NumberFormatString = "#,##0.000;(#,##0.000);* ??;@";
        public static readonly string NumberFormatStringFourDecimal = "#,##0.0000;(#,##0.0000);* ??;@";
        public static readonly string NumberFormatStringFiveDecimal = "#,##0.00000;(#,##0.00000);* ??;@";
        public static readonly string NumberFormatStringTwoDecimal = "#,##0.00;(#,##0.00);* ??;@";
        public static readonly string NumberFormatStringTwoDecimalWithZero = "#,##0.00;(#,##0.00)";
        public static readonly string NumberFormatStringInteger = "#,##0;(#,##0);* ??;@";
        public static readonly string NumberFormatStringIntegerWithZero = "#,##0;(#,##0)";
        public static readonly string NumberFormatStringText = "@"; //format cell data as text


        public static object ValidLength(string str)
        {

            string removechar = "";
            if (str.Trim() == "")
            {
                return (object)Convert.DBNull;
            }
            removechar = str.Trim();
            removechar = removechar.Replace("'", " ");

            return (object)removechar.Trim();

        }
        public static object ValidLength(string str, int length)
        {

            string removechar = "";
            if (str.Trim() == "")
            {
                return (object)Convert.DBNull;
            }
            removechar = str.Trim();
            removechar = removechar.Replace("'", " ");


            int strLen = removechar.Length;
            if (strLen > length)
                removechar = removechar.Substring(0, length);

            return (object)removechar.Trim();

        }
        public static string FileNameLegalChar(string fileName)
        {
            string illegalChar = @"~`!@#$%^&*=/\|>,<";
            foreach (char c in illegalChar)
            {
                fileName = fileName.Replace(c.ToString(), " ");
            }

            return fileName;
        }
        private StringCollection getTableColumns(ref DataSet dsLocal)
        {
            StringCollection strcol = new StringCollection();
            for (int COL = 0; COL < dsLocal.Tables[0].Columns.Count; COL++)
            {
                strcol.Add(dsLocal.Tables[0].Columns[COL].ColumnName.ToUpper());
            }

            return strcol;

        }
        public static string emptyString(string str)
        {
            //this function returns an empty string(not a null) from null or empty or '&nbsp;' from the page
            if (str == "&nbsp;")
                str = "";
            if (string.IsNullOrEmpty(str) == true)
                str = "";


            return str;
        }//this function returns an empty string(not a null) from null or empty '&nbsp;' from the page
        #endregion string


        #region others
        public void copyDataset(DataSet source, ref DataSet destination)
        {
            StringCollection strColDestinationColumns = getTableColumns(ref destination);//upper case
            DataRow drLocal = null;
            for (int ROW = 0; ROW < source.Tables[0].Rows.Count; ROW++)
            {
                drLocal = destination.Tables[0].NewRow();
                for (int COL = 0; COL < source.Tables[0].Columns.Count; COL++)
                {
                    if (strColDestinationColumns.Contains(source.Tables[0].Columns[COL].ToString().ToUpper()))
                    {
                        drLocal[source.Tables[0].Columns[COL].ToString()] = ValidLength(source.Tables[0].Rows[ROW][source.Tables[0].Columns[COL].ToString()].ToString());
                    }
                }
                destination.Tables[0].Rows.Add(drLocal);
            }


        }
        public static string GetxlsCol(int intCol)
        {
            //returns excel columns based on column number. tested 1 to 256 column numbers
            try
            {
                if (intCol < 1 || intCol > 256)
                {
                    System.Exception ex = new Exception("Invalid Column Value");
                    throw (ex);
                }
                intCol = intCol - 1;
                int intFirstLetter = ((intCol) / 512) + 64;
                int intSecondLetter = ((intCol % 512) / 26) + 64;
                int intThirdLetter = (intCol % 26) + 65;
                char FirstLetter;
                char SecondLetter;
                if (intFirstLetter > 64)
                    FirstLetter = (char)intFirstLetter;
                else
                    FirstLetter = ' ';

                if (intSecondLetter > 64)
                    SecondLetter = (char)intSecondLetter;
                else
                    SecondLetter = ' ';

                char ThirdLetter = (char)intThirdLetter;
                return string.Concat(FirstLetter, SecondLetter, ThirdLetter).Trim();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }//returns excel columns based on column number. tested 1 to 256 column numbers
        #endregion others

        public static object RetValidLen(string Data)
        {
            if (string.IsNullOrEmpty(Data))
                return DBNull.Value;

            return Data;
        }
        public static double sum(string columnName, DataTable dtLocal, string criteria)
        {
            double total = 0;
            DataRow[] dr = dtLocal.Select(criteria);
            foreach (DataRow d in dr)
            {
                total += dbl(d[columnName].ToString());
            }


            return total;
        }

    }


    #region Classes

    public class Contract
    {
        public string Id { get; set; }

        public string ContractNo { get; set; }
        public string CustomerId { get; set; }
        public string UDNo { get; set; }
        public string FileNo { get; set; }
        public string MasterLCId { get; set; }
        public string BankId { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public decimal TotalQty { get; set; }
        public decimal SOQty { get; set; }
        public decimal Amount { get; set; }
        public bool IsLC { get; set; }
        public bool IsPrint { get; set; }
        public bool IsMarketingCommisssionApplicable { get; set; }
        public string MarketingCommisssionId { get; set; }
        public bool IsBusinessDevelopmentChargesApplicable { get; set; }
        public string BusinessDevelopmentCharge { get; set; }
        public decimal BusinessDevelopmentChargeValue { get; set; }
        public string MarketingCommisssionCharge { get; set; }
        public decimal MarketingCommisssionValue { get; set; }
        public string InvoicingPartyPlantId { get; set; }
        public string DeliveryPartyPlantId { get; set; }
        public string InvoicingByAddress { get; set; }
        public string DeliveryByAddress { get; set; }
        public int LCRequiredDaysfromShipment { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public DateTime? UDDate { get; set; }
        public DateTime? ContractDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }

    public class MasterLC : BaseModel
    {
        public string Id { get; set; }
        public decimal Version { get; set; }
        public string CustomerId { get; set; }
        public bool IsClose { get; set; }
        public string BenificiaryBankId { get; set; }
        public string OpeningBankId { get; set; }
        public string OpeningDescription { get; set; }
        public string LeinBank { get; set; }
        public string LeinDescription { get; set; }
        public string LCRef { get; set; }
        public string LCDate { get; set; }
        public string ExpiryDate { get; set; }
        public string Amount { get; set; }
        public string Type { get; set; }
        public int Tenure { get; set; }
        public string FinalDestinationId { get; set; }
        public string PortOfLandingId { get; set; }
        public string CurrencyId { get; set; }
        public DateTime? LCShipmentDate { get; set; }
        public string ShipmentModeId { get; set; }
        public string PortOfLoadingId { get; set; }
        public string Remarks { get; set; }
        public string DescriptionOfGoodsAndOrServices { get; set; }
        public bool TermsandConditionVarify { get; set; }
        public bool PaymentTermVarify { get; set; }
        public bool AdditionalInfoVarify { get; set; }
        public string InsuranceCompany { get; set; }
        public string InsuranceCoverNote { get; set; }
        public string InsuranceCompanyDescription { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
        public string NegotiatingBankId { get; set; }
    }

    public class ContractFund : BaseModel
    {
        public string Id { get; set; }
        public string ContractId { get; set; }
        public string FundUtilization { get; set; }
        public string FundUtilizationText { get; set; }
        public decimal Percentage { get; set; }
        public decimal OldPercentage { get; set; }
        public decimal Commission { get; set; }
        public decimal PurchaseMargin { get; set; }
        public string CurrencyId { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }

    public class MasterOrderItemModel : BaseModel
    {
        public string MasterOrderItemId { get; set; }
        public string MasterOrderId { get; set; }
        public string SalesOrderId { get; set; }
    }

    public class MasterLCAmendment : BaseModel
    {
        public string Id { get; set; }
        public decimal Version { get; set; }
        public string CustomerId { get; set; }
        public bool IsClose { get; set; }
        public string BenificiaryBankId { get; set; }
        public string OpeningBank { get; set; }
        public string OpeningDescription { get; set; }
        public string LeinBank { get; set; }
        public string LeinDescription { get; set; }
        public string LCRef { get; set; }
        public string LCDate { get; set; }
        public string ExpiryDate { get; set; }
        public string AmendmentDate { get; set; }
        public string Amount { get; set; }
        public string Type { get; set; }
        public int Tenure { get; set; }
        public string FinalDestinationId { get; set; }
        public string PortOfLandingId { get; set; }
        public string CurrencyId { get; set; }
        public DateTime? LCShipmentDate { get; set; }
        public string ShipmentModeId { get; set; }
        public string PortOfLoadingId { get; set; }
        public string Remarks { get; set; }

        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }

    #endregion
}