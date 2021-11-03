#region Using
using Library.Model.OrderManagements;
using Aplos.Properties;
using Library.Service.OrderManagements;
using Library.Core;
using System.Web.Mvc;
using Aplos.Controllers;
using System;
using OTSBD;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using Library.Data.Sql;
using System.Collections.Generic;
using Syncfusion.XlsIO;

#endregion

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class MixingController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public MixingController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages

        public ActionResult Aplos()
        {
            return View();
        }
        #endregion


        #region Operations
        [HttpGet, Authorize]
        public ActionResult GetList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var sql = @"select Mm.Id, cont.ContractNo, P.UserName AS CustomerName, Mm.ContractId
							, MLC.Id MasterLCNo
							,MLC.LCRef,MM.Description						
                            ,[BuyerItem]=STUFF((select distinct ','+XMOI.BuyerReferenceNo from
                            trn.MasterOrderItem XMOI
                            INNER JOIN MixingChild MC ON MC.MasterOrderItemId = XMOI.Id
                            WHERE XMOI.ContractId=mm.ContractId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, ''),
                            [OwnItem]=STUFF((select distinct ','+XMOI.OwnReferenceNo from
                            trn.MasterOrderItem XMOI
                            INNer join MixingChild MC ON MC.MasterOrderItemId = XMOI.Id
                            WHERE  XMOI.ContractId=mm.ContractId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')                            
                            FROM MixingMaster MM 
                            LEFT JOIN Contract Cont On Cont.Id = Mm.ContractId
                            JOIN [HKP].[Party] AS P ON Cont.CustomerId=P.Id
							LEFT JOIN [dbo].[MasterLC] MLC ON MLC.Id=Cont.MasterLCId--MLC ON MLC.ContractId=C.Id
							ORDER BY Cont.CustomerId";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpGet, Authorize]
        public ActionResult GetMasterOrderListbyContract(string contractId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var sql = @"SELECT case WHen ISNULL(MxM.ContractId,'') <>'' then  Convert(bit,1) else Convert(bit,0) end isToBeSelect

, A.Id AS  MasterOrderId,I.Id MasterOrderItemId, A.PartyId, P.UserName AS CustomerName, A.MasterOrderNo, A.CurrencyId, SI.TotalQty	
                            ,A.TotalQtyUOMId,PL.UserName,C.Code Currency, 0 Active,B.UserName Buyer, SO.Amount,SO.Qty,ISNULL(A.BuyerReferenceNo,'') BuyerReferenceNo,ISNULL(A.OwnReferenceNo,'') OwnReferenceNo,ISNULL(I.BuyerReferenceNo,'') BuyerItem,ISNULL(I.OwnReferenceNo,'') OwnItem
                            ,MM.UserName MaterialMaster,MMA.ShortName Article
                            FROM [TRN].[MasterOrderItem] AS I
							inner join [TRN].[MasterOrder] AS A ON A.Id=I.MasterOrderId
                            JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
                            LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                            LEFT JOIN [HKP].[Buyer] AS B ON B.Id=A.BuyerId
							LEFT JOIN MST.MaterialMaster MM ON MM.Id=I.MaterialMasterId
							LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=I.ArticleId
							Left Join MixingMaster MxM On MxM.ContractId = i.ContractId
                            LEFT JOIN (
							Select SUM(TotalQty) TotalQty,MasterOrderId,Id FROM [TRN].[MasterOrderItem] Group By MasterOrderId,Id
							) SI ON SI.Id=I.Id
                            LEFT JOIN (
							SELECT SUM(S.Qty) Qty, SUM(S.Qty*S.Rate) Amount, MOI.Id
							FROM TRN.SalesOrder S
							LEFT JOIN TRN.MasterOrderItem MOI ON MOI.Id=S.MasterOrderItemId
							GROUP BY MOI.Id
							) SO ON SO.Id=I.Id
                            WHERE A.CompanyId='" + identity.CompanyId + "'  AND A.PlantId='" + identity.PlantId + "' AND I.ContractId='" + contractId + "' ORDER BY P.Id";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpGet, Authorize]
        public ActionResult GetMasterOrderListbySavedContract(string contractId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var sql = @"SELECT case WHen ISNULL(MxM.ContractId,'') <>'' then  Convert(bit,1) else Convert(bit,0) end isToBeSelect,MxM.ContractId 

, A.Id AS  MasterOrderId,I.Id MasterOrderItemId, A.PartyId, P.UserName AS CustomerName, A.MasterOrderNo, A.CurrencyId, SI.TotalQty	
                            ,A.TotalQtyUOMId,PL.UserName,C.Code Currency, 0 Active,B.UserName Buyer, SO.Amount,SO.Qty,ISNULL(A.BuyerReferenceNo,'') BuyerReferenceNo,ISNULL(A.OwnReferenceNo,'') OwnReferenceNo,ISNULL(I.BuyerReferenceNo,'') BuyerItem,ISNULL(I.OwnReferenceNo,'') OwnItem
                            ,MM.UserName MaterialMaster,MMA.ShortName Article
                            FROM [TRN].[MasterOrderItem] AS I
							inner join [TRN].[MasterOrder] AS A ON A.Id=I.MasterOrderId
                            JOIN [HKP].[Party] AS P ON A.PartyId=P.Id
                            LEFT JOIN ORG.Plant AS PL ON A.PlantId=PL.Id
                            LEFT JOIN [SCS].[Currency] AS C ON C.Id=A.CurrencyId
                            LEFT JOIN [HKP].[Buyer] AS B ON B.Id=A.BuyerId
							LEFT JOIN MST.MaterialMaster MM ON MM.Id=I.MaterialMasterId
							LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id=I.ArticleId
							left Join MixingMaster MxM On MxM.ContractId = i.ContractId
                            LEFT JOIN (
							Select SUM(TotalQty) TotalQty,MasterOrderId,Id FROM [TRN].[MasterOrderItem] Group By MasterOrderId,Id
							) SI ON SI.Id=I.Id
                            LEFT JOIN (
							SELECT SUM(S.Qty) Qty, SUM(S.Qty*S.Rate) Amount, MOI.Id
							FROM TRN.SalesOrder S
							LEFT JOIN TRN.MasterOrderItem MOI ON MOI.Id=S.MasterOrderItemId
							GROUP BY MOI.Id
							) SO ON SO.Id=I.Id
                            WHERE A.CompanyId='" + identity.CompanyId + "'  AND A.PlantId='" + identity.PlantId + "' AND MxM.ContractId='" + contractId + "' ORDER BY P.Id";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpGet, Authorize]
        public ActionResult GetBOQMixingMasterOrderItem(string cotractId)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"SELECT MOI.Id, B.MaterialMasterId, B.ArticleId, B.GrossConsumption, MOI.TotalQty, MOI.TotalQty * B.
                            	GrossConsumption MixQty, MM.UserName MaterialMaster, MMA.ShortName Article, U.Code UoM,U.Id UoMId, C.UserName CostingItem, C.Id CostingItemId,b.MaterialCostPerUnit
                            	, EntityOrVendorName = CASE 
                            		WHEN B.EntityIdWithinCompany <> ''
                            			THEN EWC.UserName
                            		WHEN B.EntityIdWithinGroup <> ''
                            			THEN EWG.UserName
                            		WHEN B.VendorId <> ''
                            			THEN PRT.UserName
                            		ELSE PRT.UserName
                            		END, PR.UserName Process
                            FROM TRN.MasterOrderItem MOI
                            JOIN dbo.QuickBOQ B ON B.MasterOrderItemId = MOI.Id
                            INNER JOIN MST.MaterialMaster MM ON MM.Id = B.MaterialMasterId
                            LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id = B.ArticleId
                            LEFT JOIN SCS.UnitOfMeasurement U ON U.Id = B.UoMId
                            LEFT JOIN HKP.CostingItem C ON C.Id = B.CostingItemId
                            LEFT JOIN ORG.Entity AS EWC ON B.EntityIdWithinCompany = EWC.Id
                            LEFT JOIN ORG.Entity AS EWG ON B.EntityIdWithinGroup = EWG.Id
                            LEFT JOIN HKP.Party AS PRT ON B.VendorId = PRT.Id
                            LEFT JOIN HKP.Process AS PR ON B.ProcessId = PR.Id
                            WHERE MOI.ContractId = '" + cotractId + @"'";
                return Json(_sqlRepository.GetDataCollection(strSQL), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

       




        public Dictionary<string, object> Create(Dictionary<string, object> data, List<Dictionary<string, object>> MixingChildList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string _MixingMasterId = "";
            DataSet dsMaster;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter("SELECT * FROM MixingMaster WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

            try
            {
                _MixingMasterId = "";
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("MixingMaster", out _MixingMasterId);
                    data["Id"] = "MM" + _MixingMasterId;
                    _MixingMasterId = data["Id"].ToString();









                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _MixingMasterId = data["Id"].ToString();



                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                #region Activity
                string sql = "";
                string _mixingChildId = "";
                DataSet dsMixingChild = null;
                sql = "SELECT * FROM MixingChild WHERE MixingMasterId='" + _MixingMasterId + "'";
                _sqlRepository.ExecuteSqlCommand(@"DELETE FROM MixingChild WHERE MixingMasterId='" + _MixingMasterId + @"'");
                con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter(sql, out dsMixingChild, false, "1");


                if (MixingChildList != null)
                {
                    //for (int i = 0; i < dsMixingChild.Tables[0].Rows.Count; i++)
                    //{
                    //    var containsActivity = MixingChildList.(dsMixingChild.Tables[0].Rows[i]["MixingMasterId"].ToString());
                    //    if (containsActivity)
                    //        continue;
                    //    else
                    //        dsMixingChild.Tables[0].Rows[i].Delete();
                    //}
                    for (int i = 0; i < MixingChildList.Count; i++)
                    {
                        dsMixingChild.Tables[0].DefaultView.RowFilter = "MixingMasterId='" + MixingChildList[i] + "'";
                        //if (Convert.ToBoolean(ActivityList[i]["isToBeSelect"]))
                        //{

                        if (dsMixingChild.Tables[0].DefaultView.Count == 0)
                        {

                            if (_mixingChildId == "")
                            {
                                bplib.clsGenID id = new bplib.clsGenID();
                                id.GenID("MixingChild", out _mixingChildId);
                                _mixingChildId = "MC" + _mixingChildId;
                            }
                            DataRow dr = dsMixingChild.Tables[0].NewRow();
                            dr["Id"] = _mixingChildId + "-" + (i + 1).ToString();

                            dr["MixingMasterId"] = _MixingMasterId;
                            dr["MasterOrderItemId"] = MixingChildList[i]["Id"];
                            dr["MaterialMasterId"] = MixingChildList[i]["MaterialMasterId"];
                            dr["ArticleId"] = MixingChildList[i]["ArticleId"];
                            dr["UoMId"] = MixingChildList[i]["UoMId"];//
                            dr["CostingItemId"] = MixingChildList[i]["CostingItemId"];
                            dr["GrossConsumption"] = MixingChildList[i]["GrossConsumption"];
                            dr["TotalQty"] = MixingChildList[i]["TotalQty"];
                            dr["FinishGoodQty"] = MixingChildList[i]["MixQty"];
                            dr["CostPerUnit"] = bplib.clsWebLib.RetValidLen(MixingChildList[i]["MaterialCostPerUnit"]);
                            dr["TotalCost"] = clsStaticInfo.dbl(MixingChildList[i]["MixQty"].ToString()) * clsStaticInfo.dbl(MixingChildList[i]["TotalQty"].ToString());

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;


                            dsMixingChild.Tables[0].Rows.Add(dr);
                        }
                        else
                        {
                            DataRow dr = dsMixingChild.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["MixingMasterId"] = _MixingMasterId;
                            dr["MasterOrderItemId"] = MixingChildList[i]["Id"];
                            dr["MaterialMasterId"] = MixingChildList[i]["MaterialMasterId"];
                            dr["ArticleId"] = MixingChildList[i]["ArticleId"];
                            dr["UoMId"] = MixingChildList[i]["UoMId"];//
                            dr["CostingItemId"] = MixingChildList[i]["CostingItemId"];
                            dr["GrossConsumption"] = MixingChildList[i]["GrossConsumption"];
                            dr["TotalQty"] = MixingChildList[i]["TotalQty"];
                            dr["FinishGoodQty"] = MixingChildList[i]["MixQty"];
                            dr["CostPerUnit"] = MixingChildList[i]["MaterialCostPerUnit"];
                            dr["TotalCost"] = clsStaticInfo.dbl(MixingChildList[i]["MixQty"].ToString()) * clsStaticInfo.dbl(MixingChildList[i]["TotalQty"].ToString());

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();

                        }

                    }
                }


                #endregion              


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsMixingChild);
                return data;// Json(new { data = data, Message = AplosMessage.Success + " PO no <b>" + data["Id"] + "</b>" });
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
                    dr[item] = bplib.clsWebLib.RetValidLen(sourceData[item]);
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
                    if (item.ToUpper() == "TRANSACTIONAMOUNT")
                    {

                    }
                    if (item.ToUpper() == "ID")
                        continue;
                    dr[item] = bplib.clsWebLib.RetValidLen(sourceData[item]);
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
        #endregion Operations
    }

}