using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Service.Productions.ProductionBooking
{
    public class ProductionServices
    {
        private readonly ISqlRepository _sqlRepository;

        public ProductionServices(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }

        private string MakeKey(string TargetDate, string WorkCenterMasterID, string ProductionOrderId)
        {
            return Convert.ToDateTime(TargetDate).ToString("dd-MMM-yyyy") + "-" + WorkCenterMasterID + "-" + ProductionOrderId;
        }
        public void UpdateDailyTarget(string date, string plantId)
        {
            bool ConsiderBulletinParameters = false;
            try
            {
                DataTable dtPlanData = new DataTable();

                dtPlanData = _sqlRepository.GetDataTable(@"--without bulletin
                                        SELECT e.PlantId,FORMAT(ppt.ProductionDate,'dd-MMM-yyyy') AS ProductionDate, ppt.ProductionOrderID, ppt.MaterialMasterId,art.ArticleId,ppt.isBuildUp,ppt.WorkCenterMasterId,
                                            ppt.Quantity,t.SPT,T.NoOfWorkStation AS Manpower,ppt.ProductionHours
                                          FROM ProductionPlanningType1 AS ppt
                                          LEFT JOIN org.Entity AS e ON e.Id=ppt.EntityID
                                          LEFT OUTER JOIN ProductionOrderSchedulingParametersType1 AS T ON t.ProductionOrderID=ppt.ProductionOrderID
                                          LEFT OUTER JOIN (SELECT DISTINCT pod.ProductionOrderId,moi.ArticleId
					                                          FROM trn.ProductionOrderDetail AS pod
					                                          INNER JOIN trn.SalesOrder AS so ON so.id=pod.SalesOrderId
					                                          INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                          ) AS ART ON art.ProductionOrderId=ppt.ProductionOrderID
                                        WHERE ppt.ProductionDate>='" + date + "'");





                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.getDataSet("SELECT * FROM trn.DailyProductionTarget AS dpt WHERE dpt.TargetDate>='" + date + "'", out DataSet dsLocal);
                Dictionary<string, DataRow> dicTargetData = new Dictionary<string, DataRow>();
                foreach (DataRow item in dsLocal.Tables[0].Rows)
                    dicTargetData.Add(MakeKey(item["TargetDate"].ToString(), item["WorkCenterMasterID"].ToString(), item["ProductionOrderId"].ToString()), item);



                string id = "";
                for (int i = 0; i < dtPlanData.Rows.Count; i++)
                {


                    string Key = MakeKey(dtPlanData.Rows[i]["ProductionDate"].ToString(), dtPlanData.Rows[i]["WorkCenterMasterId"].ToString(), dtPlanData.Rows[i]["ProductionOrderId"].ToString());
                    if (dicTargetData.ContainsKey(Key) == false)
                    {
                        if (id == "")
                        {
                            bplib.clsGenID clsgenid = new bplib.clsGenID();
                            clsgenid.GenID("trn.DailyProductionTarget", out id);
                        }
                        DataRow dr = dsLocal.Tables[0].NewRow();

                        dr["ID"] = id + "-" + (i + 1).ToString();

                        dr["PlantID"] = dtPlanData.Rows[i]["PlantId"].ToString();
                        dr["ProductionOrderId"] = dtPlanData.Rows[i]["ProductionOrderId"].ToString();
                        dr["ProductionOrderIdPlanning"] = dtPlanData.Rows[i]["ProductionOrderId"].ToString();
                        dr["MaterialMasterId"] = bplib.clsWebLib.RetValidLen(dtPlanData.Rows[i]["MaterialMasterId"].ToString());
                        dr["MaterialMasterIdPlanning"] = bplib.clsWebLib.RetValidLen(dtPlanData.Rows[i]["MaterialMasterId"].ToString());
                        dr["MaterialMasterArticleId"] = bplib.clsWebLib.RetValidLen(dtPlanData.Rows[i]["ArticleId"].ToString());
                        dr["MaterialMasterArticleIdPlanning"] = bplib.clsWebLib.RetValidLen(dtPlanData.Rows[i]["ArticleId"].ToString());
                        dr["isBuildUp"] = dtPlanData.Rows[i]["isBuildUp"];
                        dr["isBuildUpPlanning"] = dtPlanData.Rows[i]["isBuildUp"];
                        dr["WorkCenterMasterID"] = dtPlanData.Rows[i]["WorkCenterMasterId"].ToString();
                        dr["WorkCenterMasterIDPlanning"] = dtPlanData.Rows[i]["WorkCenterMasterId"].ToString();
                        dr["TargetDate"] = dtPlanData.Rows[i]["ProductionDate"].ToString();
                        dr["Quantity"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["Quantity"].ToString());
                        dr["QuantityPlanning"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["Quantity"].ToString());
                        dr["SMV"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["SPT"].ToString());
                        dr["SMVPlanning"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["SPT"].ToString());

                        dr["ManPowerWithMachine"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["Manpower"].ToString());
                        dr["ManPowerWithHand"] = 0;
                        dr["Manpower"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["Manpower"].ToString());
                        dr["ManpowerPlanning"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["Manpower"].ToString());
                        dr["TotalHour"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["ProductionHours"].ToString());
                        dr["TotalHourPlanning"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["ProductionHours"].ToString());

                        dr["AddedBy"] = "System";
                        dr["AddedDate"] = System.DateTime.Now;
                        dr["UpdatedDate"] = System.DateTime.Now;
                        dr["UpdatedBy"] = "System";

                        dsLocal.Tables[0].Rows.Add(dr);

                        dicTargetData.Add(MakeKey(dr["TargetDate"].ToString(), dr["WorkCenterMasterID"].ToString(), dr["ProductionOrderId"].ToString()), dsLocal.Tables[0].Rows[dsLocal.Tables[0].Rows.Count - 1]);

                    }
                    else
                    {

                        DataRow dr = dicTargetData[Key];
                        if (bplib.clsWebLib.GetBoolData(dr["isManual"]))
                            continue;


                        dr.BeginEdit();

                        if (dr["MaterialMasterId"].ToString() == dr["MaterialMasterIdPlanning"].ToString())
                            dr["MaterialMasterId"] = bplib.clsWebLib.RetValidLen(dtPlanData.Rows[i]["MaterialMasterId"].ToString());
                        dr["MaterialMasterIdPlanning"] = bplib.clsWebLib.RetValidLen(dtPlanData.Rows[i]["MaterialMasterId"].ToString());

                        if (dr["MaterialMasterArticleId"].ToString() == dr["MaterialMasterArticleIdPlanning"].ToString())
                            dr["MaterialMasterArticleId"] = bplib.clsWebLib.RetValidLen(dtPlanData.Rows[i]["ArticleId"].ToString());
                        dr["MaterialMasterArticleIdPlanning"] = bplib.clsWebLib.RetValidLen(dtPlanData.Rows[i]["ArticleId"].ToString());

                        if (dr["isBuildUp"].ToString() == dr["isBuildUpPlanning"].ToString())
                            dr["isBuildUp"] = dtPlanData.Rows[i]["isBuildUp"];
                        dr["isBuildUpPlanning"] = dtPlanData.Rows[i]["isBuildUp"];

                        if (dr["WorkCenterMasterID"].ToString() == dr["WorkCenterMasterIDPlanning"].ToString())
                            dr["WorkCenterMasterID"] = dtPlanData.Rows[i]["WorkCenterMasterId"].ToString();
                        dr["WorkCenterMasterIDPlanning"] = dtPlanData.Rows[i]["WorkCenterMasterId"].ToString();


                        dr["TargetDate"] = dtPlanData.Rows[i]["ProductionDate"].ToString();

                        if (dr["Quantity"].ToString() == dr["QuantityPlanning"].ToString())
                            dr["Quantity"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["Quantity"].ToString());
                        dr["QuantityPlanning"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["Quantity"].ToString());

                        if (dr["SMV"].ToString() == dr["SMVPlanning"].ToString())
                            dr["SMV"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["SPT"].ToString());
                        dr["SMVPlanning"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["SPT"].ToString());

                        if (dr["Manpower"].ToString() == dr["ManpowerPlanning"].ToString())
                        {
                            dr["Manpower"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["Manpower"].ToString());
                            dr["ManPowerWithMachine"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["Manpower"].ToString());
                            dr["ManPowerWithHand"] = 0;
                        }

                        dr["ManpowerPlanning"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["Manpower"].ToString());

                        if (dr["TotalHour"].ToString() == dr["TotalHourPlanning"].ToString())
                            dr["TotalHour"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["ProductionHours"].ToString());
                        dr["TotalHourPlanning"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["ProductionHours"].ToString());


                        dr["UpdatedDate"] = System.DateTime.Now;
                        dr["UpdatedBy"] = "System";
                        dr.EndEdit();

                    }

                }



                dtPlanData = _sqlRepository.GetDataTable(@"--with bulletin
                                         SELECT e.PlantId, ppt.ProductionOrderID,FORMAT(ppt.ProductionDate,'dd-MMM-yyyy') AS ProductionDate, ppt.MaterialMasterId,art.ArticleId,ppt.isBuildUp,ppt.WorkCenterMasterId,bul.RequiredStdTarget AS Quantity,
                                           bul.TotalSPT AS SPT,bul.AllotedManpower AS Manpower,bul.PlannedHoursPerDay AS ProductionHours,bul.WithMachine,bul.WithoutMachine
                                              FROM ProductionPlanningType1 AS ppt
                                              LEFT JOIN org.Entity AS e ON e.Id=ppt.EntityID
                                           
                                           LEFT JOIN (
                                           select
													pbt.ProductionOrderId ,pbt.Id ProductionBulletinTemplateId,pbtm.ProcessId,
													pbtm.PlannedHoursPerDay ,pbtm.RequiredStdTarget ,
													WithMachine = SUM(case when  isnull(pbtd.MachineVarientId,'')<>'' then AllotedManpower else 0 end),
													WithoutMachine = SUM(case when  isnull(pbtd.MachineVarientId,'')='' then AllotedManpower else 0 end),
													sum (pbtd.TotalSPT) TotalSPT
													,sum(pbtd.AllotedManpower)AllotedManpower,sum(pbtd.RequiredManPower) RequiredManPower,sum(pbtd.AllotedWorkstation) AllotedWorkstation
													,sum(pbtd.OperationTargetPerHr) OperationTargetPerHr
													from trn.ProductionBulletinTemplate as pbt
													left join trn.ProductionBulletinTemplateMaster pbtm on pbtm.ProductionBulletinTemplateId=pbt.Id
													left join trn.ProductionBulletinTemplateDetail pbtd on pbtd.ProductionBulletinTemplateMasterId=pbtm.Id
			
													group by pbt.Id,pbt.ProductionOrderId,pbtm.ProcessId ,pbtm.PlannedHoursPerDay,pbtm.RequiredStdTarget	
                                           ) BUL ON BUL.ProductionOrderId=ppt.ProductionOrderID AND bul.ProcessId=ppt.ProcessID
                                              LEFT OUTER JOIN (SELECT DISTINCT pod.ProductionOrderId,moi.ArticleId
					                                              FROM trn.ProductionOrderDetail AS pod
					                                              INNER JOIN trn.SalesOrder AS so ON so.id=pod.SalesOrderId
					                                              INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                              ) AS ART ON art.ProductionOrderId=ppt.ProductionOrderID
                                        WHERE ppt.ProductionDate>='" + date + "'");


                id = "";
                for (int i = 0; i < dtPlanData.Rows.Count; i++)
                {
                    string Key = MakeKey(dtPlanData.Rows[i]["ProductionDate"].ToString(), dtPlanData.Rows[i]["WorkCenterMasterId"].ToString(), dtPlanData.Rows[i]["ProductionOrderId"].ToString());
                    if (dicTargetData.ContainsKey(Key) == false)
                    {
                        if (id == "")
                        {
                            bplib.clsGenID clsgenid = new bplib.clsGenID();
                            clsgenid.GenID("trn.DailyProductionTarget", out id);
                        }
                        DataRow dr = dsLocal.Tables[0].NewRow();

                        dr["ID"] = id + "-" + (i + 1).ToString();

                        // ProductionOrderID MaterialMasterId    ArticleId isBuildUp   WorkCenterMasterId Quantity    SPT Manpower    ProductionHours
                        dr["PlantID"] = dtPlanData.Rows[i]["PlantId"].ToString();
                        dr["ProductionOrderId"] = dtPlanData.Rows[i]["ProductionOrderId"].ToString();
                        dr["ProductionOrderIdPlanning"] = dtPlanData.Rows[i]["ProductionOrderId"].ToString();
                        dr["MaterialMasterId"] = bplib.clsWebLib.RetValidLen(dtPlanData.Rows[i]["MaterialMasterId"].ToString());
                        dr["MaterialMasterIdPlanning"] = bplib.clsWebLib.RetValidLen(dtPlanData.Rows[i]["MaterialMasterId"].ToString());
                        dr["MaterialMasterArticleId"] = bplib.clsWebLib.RetValidLen(dtPlanData.Rows[i]["ArticleId"].ToString());
                        dr["MaterialMasterArticleIdPlanning"] = bplib.clsWebLib.RetValidLen(dtPlanData.Rows[i]["ArticleId"].ToString());
                        dr["isBuildUp"] = dtPlanData.Rows[i]["isBuildUp"];
                        dr["isBuildUpPlanning"] = dtPlanData.Rows[i]["isBuildUp"];
                        dr["WorkCenterMasterID"] = dtPlanData.Rows[i]["WorkCenterMasterId"].ToString();
                        dr["WorkCenterMasterIDPlanning"] = dtPlanData.Rows[i]["WorkCenterMasterId"].ToString();
                        dr["TargetDate"] = dtPlanData.Rows[i]["ProductionDate"].ToString();

                        dr["QuantityBulletin"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["Quantity"].ToString());
                        dr["SMVBulletin"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["SPT"].ToString());
                        dr["ManpowerBulletin"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["Manpower"].ToString());
                        dr["TotalHourBulletin"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["ProductionHours"].ToString());

                        dr["Quantity"] = MaxValue(dr["QuantityBulletin"].ToString(), dr["QuantityPlanning"].ToString());// clsStaticInfo.dbl(dtPlanData.Rows[i]["Quantity"].ToString());
                        dr["SMV"] = MaxValue(dr["SMVBulletin"].ToString(), dr["SMVPlanning"].ToString());// clsStaticInfo.dbl(dtPlanData.Rows[i]["SPT"].ToString());
                        //dr["Manpower"] = MaxValue(dr["ManpowerBulletin"].ToString(), dr["ManpowerPlanning"].ToString());// clsStaticInfo.dbl(dtPlanData.Rows[i]["Manpower"].ToString());


                        dr["Manpower"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["Manpower"].ToString());
                        dr["ManPowerWithMachine"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["WithMachine"].ToString());
                        dr["ManPowerWithHand"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["WithoutMachine"].ToString());

                        dr["TotalHour"] = MaxValue(dr["TotalHourBulletin"].ToString(), dr["TotalHourPlanning"].ToString());//clsStaticInfo.dbl(dtPlanData.Rows[i]["ProductionHours"].ToString());

                        dr["AddedBy"] = "System";
                        dr["AddedDate"] = System.DateTime.Now;
                        dr["UpdatedDate"] = System.DateTime.Now;
                        dr["UpdatedBy"] = "System";

                        dsLocal.Tables[0].Rows.Add(dr);

                        dicTargetData.Add(MakeKey(dr["TargetDate"].ToString(), dr["WorkCenterMasterID"].ToString(), dr["ProductionOrderId"].ToString()), dsLocal.Tables[0].Rows[dsLocal.Tables[0].Rows.Count - 1]);

                    }
                    else
                    {
                        DataRow dr = dicTargetData[Key];
                        if (bplib.clsWebLib.GetBoolData(dr["isManual"]))
                            continue;

                        dr.BeginEdit();

                        if (dr["MaterialMasterId"].ToString() == dr["MaterialMasterIdPlanning"].ToString())
                            dr["MaterialMasterId"] = bplib.clsWebLib.RetValidLen(dtPlanData.Rows[i]["MaterialMasterId"].ToString());
                        dr["MaterialMasterIdPlanning"] = bplib.clsWebLib.RetValidLen(dtPlanData.Rows[i]["MaterialMasterId"].ToString());

                        if (dr["MaterialMasterArticleId"].ToString() == dr["MaterialMasterArticleIdPlanning"].ToString())
                            dr["MaterialMasterArticleId"] = bplib.clsWebLib.RetValidLen(dtPlanData.Rows[i]["ArticleId"].ToString());
                        dr["MaterialMasterArticleIdPlanning"] = bplib.clsWebLib.RetValidLen(dtPlanData.Rows[i]["ArticleId"].ToString());

                        if (dr["isBuildUp"].ToString() == dr["isBuildUpPlanning"].ToString())
                            dr["isBuildUp"] = dtPlanData.Rows[i]["isBuildUp"];
                        dr["isBuildUpPlanning"] = dtPlanData.Rows[i]["isBuildUp"];

                        if (dr["WorkCenterMasterID"].ToString() == dr["WorkCenterMasterIDPlanning"].ToString())
                            dr["WorkCenterMasterID"] = dtPlanData.Rows[i]["WorkCenterMasterId"].ToString();
                        dr["WorkCenterMasterIDPlanning"] = dtPlanData.Rows[i]["WorkCenterMasterId"].ToString();


                        dr["TargetDate"] = dtPlanData.Rows[i]["ProductionDate"].ToString();

                        //if (dr["Quantity"].ToString() == dr["QuantityPlanning"].ToString())
                        //    dr["Quantity"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["Quantity"].ToString());
                        dr["QuantityBulletin"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["Quantity"].ToString());

                        //if (dr["SMV"].ToString() == dr["SMVPlanning"].ToString())
                        //    dr["SMV"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["SPT"].ToString());
                        dr["SMVBulletin"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["SPT"].ToString());

                        //if (dr["Manpower"].ToString() == dr["ManpowerPlanning"].ToString())
                        //    dr["Manpower"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["Manpower"].ToString());
                        dr["ManpowerBulletin"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["Manpower"].ToString());

                        //if (dr["TotalHour"].ToString() == dr["TotalHourPlanning"].ToString())
                        //    dr["TotalHour"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["ProductionHours"].ToString());
                        dr["TotalHourBulletin"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["ProductionHours"].ToString());


                        dr["Quantity"] = MaxValue(dr["QuantityBulletin"].ToString(), dr["QuantityPlanning"].ToString());// clsStaticInfo.dbl(dtPlanData.Rows[i]["Quantity"].ToString());
                        dr["SMV"] = MaxValue(dr["SMVBulletin"].ToString(), dr["SMVPlanning"].ToString());// clsStaticInfo.dbl(dtPlanData.Rows[i]["SPT"].ToString());
                        //dr["Manpower"] = MaxValue(dr["ManpowerBulletin"].ToString(), dr["ManpowerPlanning"].ToString());// clsStaticInfo.dbl(dtPlanData.Rows[i]["Manpower"].ToString());
                        dr["TotalHour"] = MaxValue(dr["TotalHourBulletin"].ToString(), dr["TotalHourPlanning"].ToString());//clsStaticInfo.dbl(dtPlanData.Rows[i]["ProductionHours"].ToString());

                        dr["Manpower"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["Manpower"].ToString());
                        dr["ManPowerWithMachine"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["WithMachine"].ToString());
                        dr["ManPowerWithHand"] = clsStaticInfo.dbl(dtPlanData.Rows[i]["WithoutMachine"].ToString());

                        dr["UpdatedDate"] = System.DateTime.Now;
                        dr["UpdatedBy"] = "System";
                        dr.EndEdit();

                    }

                }


                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsLocal);

            }
            catch (Exception ex)
            {

                throw (ex);
            }

        }


        private double MaxValue(string FirstValue, string SecondValue)
        {
            double fv = clsStaticInfo.dbl(FirstValue);

            if (clsStaticInfo.dbl(SecondValue) > fv)
                return clsStaticInfo.dbl(SecondValue);

            return fv;
        }

    }


}
