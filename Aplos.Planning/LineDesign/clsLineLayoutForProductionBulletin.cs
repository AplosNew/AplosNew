using Library.Crosscutting.Security;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.Planning.LineDesign
{
    public class clsLineLayoutForProductionBulletin
    {
        ISqlRepository _sqlRepository;
        public clsLineLayoutForProductionBulletin()
        {
            _sqlRepository = new SqlRepository();
        }
        public IEnumerable<object> GetProductionOrderData(string entityId)
        {

            try
            {
                string sql = @"SELECT PO.Id POId,PS.UserName ProductionStatus, PO.RequiredTimeUnit, FORMAT(LSD,'dd-MMM-yyyy') LSD 
								   ,FORMAT(CommitmentDate,'dd-MMM-yyyy') CommitmentDate, PD.Product, PD.ProductCategory,PD.Buyer,PD.Customer 
                                   ,PD.BuyerOrder,PD.OwnOrder,PD.BuyerItem,PD.OwnItem,PD.Description,PD.PONumber,PO.EntityId,E.UserName Entity
									,SONo=STUFF((select distinct ','+XSO.Id from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,pbtm.Id AS ProductionBulletinTemplateMasterId,pbtm.ProcessId BaseProcess
								   FROM TRN.ProductionOrder PO 
								   LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
								   LEFT JOIN ORG.Entity E ON E.Id=PO.EntityId
								  
								   LEFT JOIN 
								   (select distinct POD.ProductionOrderId,PM.UserName AS Product,pc.UserName AS ProductCategory
								   
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
								   LEFT JOIN TRN.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
								   LEFT JOIN [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
                                   LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
								   ) PD ON PD.ProductionOrderId=PO.Id
									LEFT JOIN [TRN].[ProductionOrderProcessSet] POSP ON POSP.ProductionOrderId = PO.Id and POSP.IsBaseProcess=1
									join trn.ProductionBulletinTemplate pbt on pbt.ProductionOrderId = PO.Id 
									Join TRN.ProductionBulletinTemplateMaster pbtm on pbtm.ProductionBulletinTemplateId = pbt.Id and pbtm.ProcessId=posp.ProcessId
								    WHERE  E.Id='" + entityId + "'";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public List<Dictionary<string, object>> GetDesign(string BulletinId)
        {

            try
            {
                string sql = @"select * from LineLayoutByProductionBulletin where ProductionBulletinTemplateMasterId = '" + BulletinId + "' ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveData(List<Html> Nodes, string Design, string ProductionBulletinTemplateMasterId, string EntityId, string ProductionOrderId, string ProcessId)
        {
            bplib.clsGenID objGenID = null;
            string idFromDB = "";
            string idFromDBC = "";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            List<addInfo> HtmlsInfo = new List<addInfo>();
            for (int i = 0; i < Nodes.Count; i++)
            {
                try
                {
                    Html TempHtml = Nodes[i];
                    if (TempHtml == null)
                        continue;

                    if (TempHtml.addInfo == null)
                        continue;

                    if (string.IsNullOrEmpty(TempHtml.addInfo.OperationVariationId))
                        continue;

                    HtmlsInfo.Add(TempHtml.addInfo);
                }
                catch (Exception ex)
                {

                }
            }

            DataSet dsMaster, dsChild;
            ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
            con.OpenDataSetThroughAdapter("select * from LineLayoutByProductionBulletin where ProductionBulletinTemplateMasterId='" + ProductionBulletinTemplateMasterId + "'", out dsMaster, false, "1");
            con.OpenDataSetThroughAdapter("select * from LineLayoutByProductionBulletinData where ProductionBulletinTemplateMasterId='" + ProductionBulletinTemplateMasterId + "'", out dsChild, false, "1");

            string PrimaryKey = "";
            if (dsMaster.Tables[0].Rows.Count == 0)
            {
                //create PK

                objGenID = new bplib.clsGenID();
                objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "LineLayoutByProductionBulletin", out idFromDB);
                DataRow dr = dsMaster.Tables[0].NewRow();
                PrimaryKey = "LM-" + idFromDB;
                dr["Id"] = PrimaryKey;
                dr["ProductionOrderId"] = ProductionOrderId;
                dr["Layout"] = Design;
                dr["ProcessId"] = ProcessId;
                dr["EntityId"] = EntityId;
                dr["ProductionBulletinTemplateMasterId"] = ProductionBulletinTemplateMasterId;
                dr["AddedBy"] = identity.Name;
                dr["AddedDate"] = DateTime.Now;
                dr["AddedFromIP"] = identity.IPAddress;

                dr["UpdatedBy"] = identity.Name;
                dr["UpdatedDate"] = DateTime.Now;
                dr["UpdatedFromIP"] = identity.IPAddress;
                dsMaster.Tables[0].Rows.Add(dr);
            }
            else
            {
                DataRow dr = dsMaster.Tables[0].Rows[0];
                PrimaryKey = dr["Id"].ToString();
                dr.BeginEdit();
                dr["ProductionOrderId"] = ProductionOrderId;
                dr["ProcessId"] = ProcessId;
                dr["ProductionBulletinTemplateMasterId"] = ProductionBulletinTemplateMasterId;
                dr["Layout"] = Design;
                dr["UpdatedBy"] = identity.Name;
                dr["UpdatedDate"] = DateTime.Now;
                dr["UpdatedFromIP"] = identity.IPAddress;
                dr.EndEdit();
            }

            //delete missing items from db
            while (dsChild.Tables[0].DefaultView.Count > 0)
                dsChild.Tables[0].DefaultView[0].Delete();

            string ChildPK = "";
            for (int i = 0; i < HtmlsInfo.Count; i++)
            {
                if (ChildPK == "")
                {
                    objGenID = new bplib.clsGenID();
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "LineLayoutByProductionBulletinData", out idFromDBC);
                    ChildPK = "LD-" + idFromDBC;
                }
                DataRow dr = dsChild.Tables[0].NewRow();
                dr["Id"] = ChildPK + "-" + (i + 1);
                dr["LineLayoutByProductionBulletinId"] = PrimaryKey;
                dr["OperationId"] = bplib.clsWebLib.RetValidLen(OTSBD.clsStaticInfo.nullrecorder(HtmlsInfo[i].OperationId));
                dr["MaterialMasterId"] = bplib.clsWebLib.RetValidLen(OTSBD.clsStaticInfo.nullrecorder(HtmlsInfo[i].MaterialMasterId));
                dr["ArticleId"] = bplib.clsWebLib.RetValidLen(OTSBD.clsStaticInfo.nullrecorder(HtmlsInfo[i].ArticleId));
                dr["OperationVariationId"] = bplib.clsWebLib.RetValidLen(OTSBD.clsStaticInfo.nullrecorder(HtmlsInfo[i].OperationVariationId));
                dr["OperationId"] = bplib.clsWebLib.RetValidLen(OTSBD.clsStaticInfo.nullrecorder(HtmlsInfo[i].OperationId));
                dr["ProductionBulletinTemplateMasterId"] = ProductionBulletinTemplateMasterId;
                dr["Sequence"] = bplib.clsWebLib.RetValidLen(OTSBD.clsStaticInfo.nullrecorder(HtmlsInfo[i].Sequence));

                dr["AddedBy"] = identity.Name;
                dr["AddedDate"] = DateTime.Now;
                dr["AddedFromIP"] = identity.IPAddress;

                dr["UpdatedBy"] = identity.Name;
                dr["UpdatedDate"] = DateTime.Now;
                dr["UpdatedFromIP"] = identity.IPAddress;
                dsChild.Tables[0].Rows.Add(dr);
            }

            OTSBD.clsStaticInfo SaveInfo = new OTSBD.clsStaticInfo();
            SaveInfo.SaveDataSets(dsMaster, dsChild);
        }


    }
    public class GenerateLineDiagraForLineLayout
    {
        SqlRepository _sqlRepository = new SqlRepository();
        List<DiagramShapes> AllShapes = new List<DiagramShapes>();
        public List<object> AllShapesForJson = new List<object>();
        public enum DrawType { Linear, TwoLines }
        public void MakeBulletinList(string BulletinId, DrawType drawType)
        {

            try
            {
                DataTable dtBulletin = _sqlRepository.GetDataTable(@"SELECT  ROW_NUMBER() OVER(ORDER BY D.Sequence,d.id) AS SQ,d.Id,ov.Id AS OperationVariationId,ov.UserName AS OperationVariation,
                                            d.AllotedManpower,MM.Id MaterialMasterId,MM.UserName AS MaterialMasterDesc
											,o.IsMachineRequired,M.StandardName AS ArticleDesc,o.Id as OperationId,o.UserName as OperationDesc
                                                ,M.Id ArticleId    ,d.Sequence,NULL AS Designation,isnull(Ov.color,'#ffffff') AS Color,
                                            mv.Id AS MachineId,mv.UserName AS MachineDesc,d.AllotedWorkstation,d.RequiredManPower,d.TotalSPT,
                                            '1800001.jpg' AS EmpPicPath
                                                FROM trn.ProductionBulletinTemplateDetail D
                                            INNER JOIN mst.OperationVariation AS ov ON ov.Id=d.OperationVariationId
                                            LEFT JOIN [MST].[MaterialMasterArticle] M ON M.Id = OV.ArticleId
                                            LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=m.MaterialMasterId
                                            INNER JOIN mst.Operation AS o ON o.Id=ov.OperationId
                                            LEFT OUTER JOIN hkp.MachineVariant AS mv ON mv.Id=d.MachineVarientId
                                            
                                            WHERE d.ProductionBulletinTemplateMasterId='" + BulletinId + "' ORDER BY D.Sequence");

                int ItemWidth = 160; int ItemHeight = 120;

                if (drawType == DrawType.Linear)
                    makeShapesLinear(dtBulletin, 0, 0, ItemWidth, ItemHeight);

                if (drawType == DrawType.TwoLines)
                {
                    int TotalRows = dtBulletin.Rows.Count;
                    if (TotalRows <= 10)
                        makeShapesLinear(dtBulletin, 0, 0, ItemWidth, ItemHeight);
                    else
                    {
                        int Half = (int)TotalRows / 2;
                        dtBulletin.DefaultView.RowFilter = "SQ<=" + Half;
                        makeShapesLinear(dtBulletin.DefaultView.ToTable(), 0, 0, ItemWidth, ItemHeight);


                        dtBulletin.DefaultView.RowFilter = "SQ>" + Half;
                        makeShapesLinear(dtBulletin.DefaultView.ToTable(), 0, ItemHeight * 2, ItemWidth, ItemHeight);
                    }

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        private void makeShapesLinear(DataTable dtBulletin, int offsetX = 0, int offsetY = 0, int Width = 100, int Height = 100)
        {
            try
            {

                int PaddingTop = 10; int paddingLeft = 5;

                int ItemIndex = 0;
                for (int i = 0; i < dtBulletin.Rows.Count; i++)
                {
                    double allotedWorkstations = OTSBD.clsStaticInfo.dbl(dtBulletin.Rows[i]["AllotedWorkstation"].ToString());
                    while (allotedWorkstations > 0)
                    {
                        ItemIndex++;
                        GroupingData group = new GroupingData();
                        group.id = "group" + dtBulletin.Rows[i]["Id"].ToString();

                        //offsetY = 0;
                        #region Employee Image
                        Html emp = new Html();
                        AllShapes.Add(emp);
                        emp.height = Height;
                        emp.width = Width;
                        emp.offsetX = offsetX + (emp.width / 2);
                        emp.offsetY = offsetY + (emp.height / 2);
                        emp.fillColor = dtBulletin.Rows[i]["Color"].ToString();
                        emp.id = "E" + dtBulletin.Rows[i]["Id"].ToString() + System.DateTime.Now.Ticks.ToString() + ItemIndex;
                        emp.name = "E" + dtBulletin.Rows[i]["Id"].ToString() + System.DateTime.Now.Ticks.ToString() + ItemIndex;

                        emp.labels.Add(new labels { text = "" });

                        emp.addInfo = new addInfo
                        {
                            //EmployeeId = dtBulletin.Rows[i]["EmployeeId"].ToString(),
                            MachineOrHand = dtBulletin.Rows[i]["IsMachineRequired"].ToString(),
                            MaterialMasterId = dtBulletin.Rows[i]["MaterialMasterId"].ToString(),
                            MaterialMasterDesc = dtBulletin.Rows[i]["MaterialMasterDesc"].ToString(),
                            ArticleId = dtBulletin.Rows[i]["ArticleId"].ToString(),
                            ArticleDesc = dtBulletin.Rows[i]["ArticleDesc"].ToString(),
                            OperationId = dtBulletin.Rows[i]["OperationId"].ToString(),
                            OperationDesc = dtBulletin.Rows[i]["OperationDesc"].ToString(),
                            OperationVariationId = dtBulletin.Rows[i]["OperationVariationId"].ToString(),
                            OperationVariationDesc = dtBulletin.Rows[i]["OperationVariation"].ToString(),
                            TotalSPT = OTSBD.clsStaticInfo.dbl(dtBulletin.Rows[i]["TotalSPT"].ToString()),
                            //Designation = dtBulletin.Rows[i]["Designation"].ToString(),
                            //EmpPicPath = dtBulletin.Rows[i]["EmpPicPath"].ToString(),
                            Sequence = OTSBD.clsStaticInfo.dbl(dtBulletin.Rows[i]["Sequence"].ToString()),
                            RequiredManPower = OTSBD.clsStaticInfo.dbl(dtBulletin.Rows[i]["RequiredManPower"].ToString()),
                            AllotedWorkstation = OTSBD.clsStaticInfo.dbl(dtBulletin.Rows[i]["AllotedWorkstation"].ToString())
                        };



                        offsetX += emp.width + paddingLeft;
                        AllShapesForJson.Add(emp);
                        //group.children.Add(emp.name);

                        #endregion Employee Image

                        RightArrow arrow = new RightArrow();
                        arrow.id = "R" + dtBulletin.Rows[i]["Id"].ToString() + System.DateTime.Now.Ticks.ToString() + ItemIndex;
                        arrow.name = "R" + dtBulletin.Rows[i]["Id"].ToString() + System.DateTime.Now.Ticks.ToString() + ItemIndex;
                        arrow.height = 50;
                        arrow.width = 50;
                        arrow.offsetX = offsetX + (arrow.width / 2);
                        arrow.offsetY = offsetY + (arrow.height / 2);



                        AllShapes.Add(arrow);
                        AllShapesForJson.Add(arrow);
                        //AllShapesForJson.Add(group);
                        offsetX += arrow.width + paddingLeft;

                        allotedWorkstations--;
                    }
                }
            }
            catch (Exception ex)
            {

            }

        }
    }



}
