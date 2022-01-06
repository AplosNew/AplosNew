using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
namespace Library.OrderManagement.ProformaInvoice
{
    public class ProformaInvoice
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public ProformaInvoice()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        public string Save(Dictionary<string, object> PIPackingListMasterData, Dictionary<string, object> MaterialData, List<Dictionary<string, object>> DataList)
        {
            try
            {
                if (DataList != null)
                {
                    for (int i = 0; i < DataList.Count; i++)
                    {
                        if (clsStaticInfo.dbl(DataList[i]["DistributeQTY"]) <= 0)
                        {
                            throw new Exception("Quantity is missing");
                        }
                    }
                }

                ConnectionManager.DAL.ConManager conPIMaster = new ConnectionManager.DAL.ConManager("1");
                conPIMaster.OpenDataSetThroughAdapter("SELECT * FROM PIPackingListMaster where Id='" + PIPackingListMasterData["Id"] + "'", out DataSet dsMaster, false, "1");
                string _Id = "";
                string PIPackingListID = "";
                string PIPackingListMaterialID = "";

                ConnectionManager.DAL.ConManager conPIVersion = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("PIPackingListMaster", out _Id);
                    _Id = "PPL" + "-" + _Id;
                    PIPackingListMasterData["Id"] = _Id;
                    //PIPackingListMasterData["Id"] = _Id;
                    PIPackingListID = PIPackingListMasterData["Id"].ToString();
                    AddNewRow(dsMaster.Tables[0], PIPackingListMasterData);
                    dsMaster.Tables[0].Rows[0]["PImasterId"] = MaterialData["PIMasterId"];
                }
                else
                {
                    //PIMasterId = PIPackingListMasterData["Id"].ToString();
                    _Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], PIPackingListMasterData);
                    dsMaster.Tables[0].Rows[0]["Id"] = _Id;
                }

                ConnectionManager.DAL.ConManager conPIMaterial = new ConnectionManager.DAL.ConManager("1");
                conPIMaterial.OpenDataSetThroughAdapter("SELECT * FROM PIPackingListMaterial where PIPackingListMasterId='" + _Id + "' AND PIMaterialId='" + MaterialData["Id"] + "' ", out DataSet dsMaterial, false, "1");
                string _IdM = "";
                #region data update
                if (dsMaterial.Tables[0].Rows.Count == 0)
                {

                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("PIPackingListMaterial", out _IdM);
                    _IdM = "PLM" + "-" + _IdM;


                    AddNewRow(dsMaterial.Tables[0], MaterialData);
                    dsMaterial.Tables[0].Rows[0]["Id"] = _IdM;
                    dsMaterial.Tables[0].Rows[0]["PIQuantity"] = MaterialData["Quantity"];
                    dsMaterial.Tables[0].Rows[0]["PIMaterialId"] = MaterialData["Id"];
                    dsMaterial.Tables[0].Rows[0]["PIUoMId"] = MaterialData["UoMId"];
                    dsMaterial.Tables[0].Rows[0]["PIPackingListMasterId"] = _Id;
                }
                else
                {
                    _IdM = dsMaterial.Tables[0].Rows[0]["Id"].ToString();
                    EditRow(dsMaterial.Tables[0].Rows[0], MaterialData);
                    dsMaterial.Tables[0].Rows[0]["Id"] = _IdM;
                    dsMaterial.Tables[0].Rows[0]["PIPackingListMasterId"] = _Id;
                }

                ConnectionManager.DAL.ConManager conPIDetail = new ConnectionManager.DAL.ConManager("1");
                conPIDetail.OpenDataSetThroughAdapter("select * from PIPackingListDetail where PIPackingListMasterId='" + _Id + "' AND PIMaterialId='" + MaterialData["Id"] + @"'", out DataSet dsPIDetail, false, "1");

                if (DataList == null || DataList.Count == 0)
                {
                    while (dsPIDetail.Tables[0].DefaultView.Count > 0)
                        dsPIDetail.Tables[0].DefaultView[0].Delete();
                }

                if (DataList != null)
                {
                    for (int i = 0; i < dsPIDetail.Tables[0].Rows.Count; i++)
                    {
                        var item = DataList.Where(x => x["PODetailId"].ToString() == dsPIDetail.Tables[0].Rows[i]["PODetailId"].ToString()).FirstOrDefault();
                        if (item == null || item.Count == 0)
                        {
                            dsPIDetail.Tables[0].Rows[i].Delete();
                        }
                    }
                    foreach (var item in DataList)
                    {
                        dsPIDetail.Tables[0].DefaultView.RowFilter = "PODetailId='" + clsStaticInfo.nullrecorder(item["PODetailId"]) + "'";

                        DataView dv = new DataView(dsPIDetail.Tables[0]);
                        dv.RowFilter = "PODetailId='" + clsStaticInfo.nullrecorder(item["PODetailId"]) + "'";
                        if (dv.Count > 0)
                        {
                            //edit

                            DataRow drmo = dv[0].Row;
                            drmo.BeginEdit();
                            drmo["Quantity"] = clsStaticInfo.dbl(item["DistributeQTY"]);

                            drmo["UpdatedBy"] = identity.Name;
                            drmo["UpdatedDate"] = System.DateTime.Now.ToString();
                            drmo["UpdatedFromIP"] = identity.IPAddress;
                            drmo["PIPackingListMasterId"] = _Id;
                            drmo.EndEdit();

                        }
                        else
                        {
                            string PLDetailId = "";
                            //add new
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID("PIPackingListDetail", out PLDetailId);
                            PLDetailId = "PLD" + "-" + PLDetailId;
                            item["Id"] = PLDetailId;
                            AddNewRow(dsPIDetail.Tables[0], item);

                            DataRow drmo = dsPIDetail.Tables[0].Rows[dsPIDetail.Tables[0].Rows.Count - 1];

                            drmo["PIMaterialId"] = MaterialData["Id"];
                            drmo["Quantity"] = clsStaticInfo.dbl(item["DistributeQTY"]);
                            drmo["PIPackingListMasterId"] =_Id;
                            drmo["PIPackingListMaterialId"] = _IdM;
                            

                        }
                    }
                }


                #endregion data update
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsMaterial, dsPIDetail);

                return _Id;
            }
            catch (Exception ex)
            {
                throw ex;
                //return Json(new { Error = true, Message = ex.Message });
            }
            return null;
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


        public IEnumerable<object> GetTermsAndConditionPOPopUp(string TermsAndConditionsPIDetailId)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"select * from TermsAndConditionsPIDetails where TermsAndConditionsPIChildId='" + TermsAndConditionsPIDetailId + "' order by sequence";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public string DeletePIDetailPopUp(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from TermsAndConditionsPIDetails where Id='" + id + "'");

                con.CommitTransaction();

                return "Success";

            }
            catch (Exception ex)
            {

                return ex.Message;

            }
        }

    }
}
