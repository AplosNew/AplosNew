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

        public void Save(Dictionary<string, object> PIPackingListMasterData, Dictionary<string, object> MaterialData, List<Dictionary<string, object>> DataList)
        {
            try
            {
              
                ConnectionManager.DAL.ConManager conPIMaster = new ConnectionManager.DAL.ConManager("1");
                conPIMaster.OpenDataSetThroughAdapter("SELECT * FROM PIPackingListMaster where Id='" + PIPackingListMasterData["Id"] + "'", out DataSet dsMaster, false, "1");
                string _Id = "";
                string PIPackingListID = "";
                string PIPackingListMaterialID = "";
       
                ConnectionManager.DAL.ConManager conPIVersion = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    //if (string.IsNullOrEmpty(PIPackingListMasterData["PINo"].ToString()) == dsMaster.Tables[0]["PINo"].ToString())
                    //    throw new Exception("Please select Customer.");
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("PIPackingListMaster", out _Id);
                    _Id = "PPL" + "-" + _Id;
                    PIPackingListMasterData["Id"] = _Id;
                    PIPackingListID = PIPackingListMasterData["Id"].ToString();
                    AddNewRow(dsMaster.Tables[0], PIPackingListMasterData);
                }
                else
                {
                    //PIMasterId = PIPackingListMasterData["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], PIPackingListMasterData);
                }

                ConnectionManager.DAL.ConManager conPIMaterial = new ConnectionManager.DAL.ConManager("1");
                conPIMaterial.OpenDataSetThroughAdapter("SELECT * FROM PIPackingListMaterial where PIPackingListMasterId='" + PIPackingListID + "' ", out DataSet dsMaterial, false, "1");
                string _IdM = "";
                string PackingListMaterialId = "";
                #region data update
                if (dsMaterial.Tables[0].Rows.Count == 0)
                {
                
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("PIPackingListMaterial", out _IdM);
                    _IdM = "PLM" + "-" + _IdM;
                    MaterialData["Id"] = _IdM;
                    MaterialData["PIPackingListMasterId"] = PIPackingListID;
                    //dsMaterial.Tables[0].Rows["PIQuantity"] = MaterialData["PIQuantity"];
                    //dsMaterial.Tables[0]["PIMaterialId"] = MaterialData["Id"];
                    //dsMaterial.Tables[0]["PIUoMId"] = MaterialData["UoMId"];

                    PackingListMaterialId = MaterialData["Id"].ToString();
                    AddNewRow(dsMaterial.Tables[0], MaterialData);
                }
                else
                {
                    //PIMasterId = PIPackingListMasterData["Id"].ToString();
                    EditRow(dsMaterial.Tables[0].Rows[0], MaterialData);
                }

                ConnectionManager.DAL.ConManager conPIDetail = new ConnectionManager.DAL.ConManager("1");
                conPIDetail.OpenDataSetThroughAdapter("select * from PIPackingListDetail where PIMaterialId='" + MaterialData["Id"] +@"'", out DataSet dsPIDetail, false, "1");

                if (DataList == null || DataList.Count == 0)
                {
                    while (dsPIDetail.Tables[0].DefaultView.Count > 0)
                        dsPIDetail.Tables[0].DefaultView[0].Delete();
                }

                if (DataList != null )
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
                        dsPIDetail.Tables[0].DefaultView.RowFilter = "Id='" + clsStaticInfo.nullrecorder(item["Id"]) + "'";

                        DataView dv = new DataView(dsPIDetail.Tables[0]);
                        dv.RowFilter = "Id='" + clsStaticInfo.nullrecorder(item["Id"]) + "'";
                        if (dv.Count > 0)
                        {
                            //edit
                           
                            DataRow drmo = dv[0].Row;

                            //drmo["PIMaterialID"] = item["PIMaterialId"];
                            //drmo["PODetailId"] = item["PODetailId"];
                            //drmo["QuantityAtPIUoM"] = item["QuantityAtPIUoM"];
                            //drmo["PIUoMId"] = item["PIUoMId"];
                            //drmo["POQuantity"] = item["POQuantity"];
                            //drmo["POUoMId"] = item["POUoMId"];

                            EditRow(drmo, item);

                        }
                        else
                        {
                            string PLDetailId = "";
                            //add new
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID("PIPackingListDetail", out PLDetailId);
                            PLDetailId = "PLD"+"-"+ PLDetailId;
                            item["Id"] = PLDetailId;
                            item["PIPackingListMasterId"] = PIPackingListID;
                            item["PIMaterialId"] = MaterialData["Id"];
                            //item["Quantity"] =item[] ;
                            //item["PIUoMId"] = item[];

                            AddNewRow(dsPIDetail.Tables[0], item);

                        }
                    }
                }


                #endregion data update
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsMaterial, dsPIDetail);
               // return Json(new { Error = false, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw ex;
                //return Json(new { Error = true, Message = ex.Message });
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
    }
}
