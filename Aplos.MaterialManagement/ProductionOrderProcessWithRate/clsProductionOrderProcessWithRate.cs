using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Extension;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.MaterialManagement.ProductionOrderProcessWithRate
{
    public class clsProductionOrderProcessWithRate
    {
        ISqlRepository _sqlRepository;
        public clsProductionOrderProcessWithRate()
        {
            _sqlRepository = new SqlRepository();
        }
        public IEnumerable<object> GetDetailRate(string ProductionEntityId, string ProcessId, string ProductionOrderId)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"select M.Id MasterId,D.Rate from ProductionOrderProcessWithRateDetails D
                                Left join ProductionOrderProcessWithRateMaster m on m.Id=D.ProductionOrderProcessWithRateMasterId
                                where M.ProductionEntityId='" + ProductionEntityId + "' and M.ProcessId='" + ProcessId + "' and M.ProductionOrderId='" + ProductionOrderId + "' ";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function
        public IEnumerable<object> GetSKU(string ProcessId, string ProductionOrderId, string SkuId, string Sequence)
        {
            try
            {
                string Column = string.Empty;
                string FGValue = string.Empty;
                string AndClause = string.Empty;
                string AndClauses = "";

                if (Sequence == "1")
                {
                    Column = @",fc.CharacteristicsId FirstCharacteristicsId
								,fc.CharacteristicsValueId FirstCharacteristicsValueId,ch.UserName as Char,chv.UserName as CharValue";
                    FGValue = @"left join [TRN].[FirstCharacteristics] fc on fc.SalesOrderId = so.Id
							left join HKP.Characteristics ch on ch.Id = fc.CharacteristicsId
							left join HKP.CharacteristicsValue chv on chv.Id = fc.CharacteristicsValueId";
                    AndClauses = @"and ch.Id= '" + SkuId + @"' ";

                    AndClause = @"and d.FirstCharacteristicsId = c.FirstCharacteristicsId and d.FirstCharacteristicsValueId=c.FirstCharacteristicsValueId";

                }
                else if (Sequence == "2")
                {
                    Column = @" ,sc.CharacteristicsId SecondCharacteristicsId
								,sc.CharacteristicsValueId SecondCharacteristicsValueId
								,ch2.UserName as Char,chv2.UserName as CharValue";

                    FGValue = @"left join [TRN].[FirstCharacteristics] fc on fc.SalesOrderId = so.Id
							left join [TRN].[SecondCharacteristics] sc on sc.SalesOrderId = so.Id and sc.FirstCharacteristicsId = fc.Id
							left join HKP.Characteristics ch2 on ch2.Id = sc.CharacteristicsId 
							left join HKP.CharacteristicsValue chv2 on chv2.Id=sc.CharacteristicsValueId";
                    AndClauses = @"and ch2.Id= '" + SkuId + @"' ";
                    AndClause = @"and d.SecondCharacteristicsId = c.SecondCharacteristicsId and d.SecondCharacteristicsValueId = c.SecondCharacteristicsValueId";
                }
                else
                {
                    Column = @",fc.CharacteristicsId FirstCharacteristicsId
								,fc.CharacteristicsValueId FirstCharacteristicsValueId,ch.UserName as Char1,chv.UserName as CharValue1
								,sc.CharacteristicsId SecondCharacteristicsId
								,sc.CharacteristicsValueId SecondCharacteristicsValueId
								,ch2.UserName as Char2,chv2.UserName as CharValue2";

                    FGValue = @"left join [TRN].[FirstCharacteristics] fc on fc.SalesOrderId = so.Id--
							left join HKP.Characteristics ch on ch.Id = fc.CharacteristicsId
							left join HKP.CharacteristicsValue chv on chv.Id=fc.CharacteristicsValueId--
							left join [TRN].[SecondCharacteristics] sc on sc.SalesOrderId = so.Id and sc.FirstCharacteristicsId = fc.Id
							left join HKP.Characteristics ch2 on ch2.Id = sc.CharacteristicsId 
							left join HKP.CharacteristicsValue chv2 on chv2.Id=sc.CharacteristicsValueId";
                    AndClause = @"and d.FirstCharacteristicsId = c.FirstCharacteristicsId and d.FirstCharacteristicsValueId=c.FirstCharacteristicsValueId
									and d.SecondCharacteristicsId = c.SecondCharacteristicsId and d.SecondCharacteristicsValueId = c.SecondCharacteristicsValueId";
                }

                string strSQL = string.Empty;

                strSQL = @"select c.*,d.Rate,m.Id MasterId from (
								select distinct p.ProductionOrderId,p.ProcessId " + Column + @"								
							from trn.ProductionOrder PR
							join [TRN].[ProductionOrderProcessSet] p on p.ProductionOrderId=pr.Id and  p.ProcessId='" + ProcessId + @"'
							left join trn.ProductionOrderDetail PD ON pd.Id=(select top 1 Id from trn.ProductionOrderDetail PDX where pdx.ProductionOrderId=pr.Id)
							left join trn.SalesOrder SO ON so.Id=pd.SalesOrderId
							" + FGValue + @"
							where PR.Id='" + ProductionOrderId + @"' " + AndClauses + @"
							) c
							left join ProductionOrderProcessWithRateMaster m on m.ProductionOrderId = c.ProductionOrderId and c.ProcessId=m.ProcessId
							left join ProductionOrderProcessWithRateDetails d on d.ProductionOrderProcessWithRateMasterId = m.Id 
							" + AndClause + @"";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function
        public IEnumerable<object> GetProductionOrderData(string entityId, string ProcessId)
        {

            try
            {
                string sql = @"SELECT Ma.Id,null as Charactaristics, SKUId=case when Ma.SelectedDropDownValue is null then '' else Ma.SelectedDropDownValue end,'' as [Sequence],'' Rate,IsDisable= case when Ma.SelectedDropDownValue is null then Convert(bit,'False') else CONVERT(bit,'True') end,
                                    PO.Id POId,PS.UserName ProductionStatus, PO.RequiredTimeUnit, sum(PD.Qty)Qty,FORMAT(LSD,'dd-MMM-yyyy') LSD 
								   ,FORMAT(CommitmentDate,'dd-MMM-yyyy') CommitmentDate, PD.Product, PD.ProductCategory,PD.Buyer,PD.Customer 
                                   ,PD.BuyerOrder,PD.OwnOrder,PD.BuyerItem,PD.OwnItem,PD.Description,PD.PONumber,PO.EntityId,E.UserName Entity,PD.Article,PD.MaterialMaster
									,SONo=STUFF((select distinct ','+XSO.Id from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								   FROM TRN.ProductionOrder PO 
								   LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
								   LEFT JOIN ORG.Entity E ON E.Id=PO.EntityId
								  
								   LEFT JOIN 
								   (select distinct POD.ProductionOrderId,PM.UserName AS Product,pc.UserName AS ProductCategory,SO.Qty
                                        ,mm.UserName MaterialMaster,mma.StandardName Article								   
								        ,Buyer=  REPLACE(REPLACE(
										            STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                            ,'&amp;','&'), 'amp;', '')	
								,Customer= REPLACE(REPLACE(
										              STUFF((select distinct ','+XP.UserName from 
		                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                                                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                        ,'&amp;','&'), 'amp;', '')	
                                ,BuyerOrder = REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								                		,'&amp;','&'), 'amp;', '')
                                ,OwnOrder =REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									                	,'&amp;','&'), 'amp;', '')
							 ,BuyerItem=REPLACE(REPLACE(
										 STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										                ,'&amp;','&'), 'amp;', '')	                                                
                              ,OwnItem=REPLACE(REPLACE(
										STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	 
                               ,PONumber=REPLACE(REPLACE(
										 STUFF((select distinct ','+CPO.PONumber from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
                            , Description=REPLACE(REPLACE(
										 STUFF((select distinct ','+XSO.Description from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                        
                                                                 WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')	
								   FROM TRN.SalesOrder SO
							       LEFT JOIN  TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
								   LEFT JOIN TRN.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
                                   LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
                                   LEFT JOIN MST.MaterialMasterArticle mma on mma.Id=MOI.ArticleId 
								   LEFT JOIN TRN.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
								   LEFT JOIN [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                   LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
								   ) PD ON PD.ProductionOrderId=PO.Id
									LEFT JOIN [TRN].[ProductionOrderProcessSet] POSP ON POSP.ProductionOrderId = PD.ProductionOrderId
                                    left join ProductionOrderProcessWithRateMaster Ma on Ma.ProductionEntityId = PO.EntityId and Ma.ProcessId =POSP.ProcessId and Ma.ProductionOrderId =PO.Id
								   WHERE  E.Id='" + entityId + "' and POSP.ProcessId='" + ProcessId + "' and PS.StandardName in ('Active','Running')" +
                                   "group by Ma.Id,[Sequence], PO.Id ,PS.UserName,PO.RequiredTimeUnit,LSD,CommitmentDate,PD.Product" +
                                   ", PD.ProductCategory,PD.Buyer,PD.Customer" +
                                   ", PD.BuyerOrder,PD.OwnOrder,PD.BuyerItem,PD.OwnItem,PD.Description,PD.PONumber,PO.EntityId,E.UserName,Ma.SelectedDropDownValue,PD.MaterialMaster,PD.Article";
                List<Dictionary<string, object>> data = _sqlRepository.GetDataCollection(sql, null);

                string strSQL = @"select p.ProductionOrderId,c.Id Value,c.UserName Text, m.Sequence
							from trn.ProductionOrder PR
							join [TRN].[ProductionOrderProcessSet] p on p.ProductionOrderId=pr.Id and  p.ProcessId='" + ProcessId + @"'
							left join trn.ProductionOrderDetail PD ON pd.Id=(select top 1 Id from trn.ProductionOrderDetail PDX where pdx.ProductionOrderId=pr.Id)
							left join trn.SalesOrder SO ON so.Id=pd.SalesOrderId
							left join trn.MasterOrderItem MOI ON moi.id=so.MasterOrderItemId
							left join MST.MaterialMasterCharacteristics m on m.MaterialMasterId=MOI.MaterialMasterId
                            left join HKP.Characteristics c on c.Id=m.CharacteristicsId
							where PR.EntityId='" + entityId + @"'";
                List<Dictionary<string, object>> CharList = _sqlRepository.GetDataCollection(strSQL, null);

                for (int i = 0; i < data.Count; i++)
                {
                    List<Dictionary<string, object>> TempData = CharList.Where(r => r["ProductionOrderId"].ToString() == data[i]["POId"].ToString()).ToList();
                    if (TempData.Count > 1)
                    {
                        Dictionary<string, object> DicTemp = new Dictionary<string, object>();
                        DicTemp.Add("ProductionOrderId", data[i]["POId"].ToString());
                        DicTemp.Add("Value", "Both");
                        DicTemp.Add("Text", "Both");
                        DicTemp.Add("Sequence", "Both");
                        TempData.Add(DicTemp);
                    }
                    data[i]["Charactaristics"] = TempData;
                }
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #region S A V E
        public void Save(Dictionary<string, object> Master, List<Dictionary<string, object>> ChildData, string Sequence)
        {

            try
            {
                DataSet dsMaster;
                DataSet dsChild;
                string sID = string.Empty;
                bplib.clsGenID objGenID = new bplib.clsGenID();
                int Count = 0;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from ProductionOrderProcessWithRateMaster where Id='" + Master["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";
                string MasterID = string.Empty;

                #region Master data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ProductionOrderProcessWithRateMaster", out _Id);

                    Master["Id"] = "POPWRM_" + _Id;
                    MasterID = Master["Id"].ToString();
                    AddNewRow(dsMaster.Tables[0], Master);
                }
                else
                {
                    MasterID = Master["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], Master);
                }
                #endregion

                #region Child data Update

                con.OpenDataSetThroughAdapter("select * from ProductionOrderProcessWithRateDetails where ProductionOrderProcessWithRateMasterId='" + MasterID + "'", out dsChild, false, "1");

                while (dsChild.Tables[0].DefaultView.Count > 0)
                {
                    dsChild.Tables[0].DefaultView[0].Delete();
                }

                objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[dbo].[ProductionOrderProcessWithRateDetails]", out sID);
                for (int i = 0; i < ChildData.Count; i++)
                {
                    if ( Convert.ToDecimal(ChildData[i]["Rate"]) != 0 )
                    {
                        DataRow dr = dsChild.Tables[0].NewRow();
                        Count++;
                        dr["Id"] = "POPWRD_" + sID + Count;
                        dr["ProductionOrderProcessWithRateMasterId"] = MasterID;
                        if (Sequence == "1")
                        {
                            dr["FirstCharacteristicsId"] = ChildData[i]["FirstCharacteristicsId"];
                            dr["FirstCharacteristicsValueId"] = ChildData[i]["FirstCharacteristicsValueId"];
                            dr["SecondCharacteristicsId"] = DBNull.Value;
                            dr["SecondCharacteristicsValueId"] = DBNull.Value;
                        }
                        else if (Sequence == "2")
                        {
                            dr["FirstCharacteristicsId"] = DBNull.Value;
                            dr["FirstCharacteristicsValueId"] = DBNull.Value;
                            dr["SecondCharacteristicsId"] = ChildData[i]["SecondCharacteristicsId"];
                            dr["SecondCharacteristicsValueId"] = ChildData[i]["SecondCharacteristicsValueId"];
                        }
                        else if (Sequence == "Both")
                        {
                            dr["FirstCharacteristicsId"] = ChildData[i]["FirstCharacteristicsId"];
                            dr["FirstCharacteristicsValueId"] = ChildData[i]["FirstCharacteristicsValueId"];
                            dr["SecondCharacteristicsId"] = ChildData[i]["SecondCharacteristicsId"];
                            dr["SecondCharacteristicsValueId"] = ChildData[i]["SecondCharacteristicsValueId"];
                        }
                        else
                        {
                            dr["FirstCharacteristicsId"] = DBNull.Value;
                            dr["FirstCharacteristicsValueId"] = DBNull.Value;
                            dr["SecondCharacteristicsId"] = DBNull.Value;
                            dr["SecondCharacteristicsValueId"] = DBNull.Value;
                        }

                        dr["Rate"] = ChildData[i]["Rate"];

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dsChild.Tables[0].Rows.Add(dr);
                    }
                }

                #endregion

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsChild);
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
        #endregion
    }
}
