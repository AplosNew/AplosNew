using System;
using System.Collections.Generic;
using Library.Data.Sql;
using System.Data;
using OTSBD;
using Library.Crosscutting.Security;
using System.Threading;

namespace Library.MaterialManagement.JobWork
{

    public class JobWorkIssueReturn
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public JobWorkIssueReturn()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        //       FOR JOB WORK MODULE

        //public IEnumerable<object> GetMaterialInputData(IEnumerable<MaterialPlanning> SelectedMaterialPlanningData)
        //{
        //    try
        //    {
        //        ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
        //        var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //        var MPId = "' '";

        //        foreach (var get in SelectedMaterialPlanningData)
        //        {
        //            MPId += ",'" + get.Id + "' ";

        //        }

        //        string sql = "";
        //        if (!string.IsNullOrEmpty(MPId))
        //        {
        //            sql = @"select distinct NULL AS LotNumberList, mi.Id,mi.JobWorkTransformationContractChildMasterId, jwi.UserName as JWOutputItem ,mm.Id as MaterialMasterId, mm.UserName as Material
        //                    ,mma.Id as MaterialArticleId, mma.StandardName as Article, InvDetail.InventoryMaterialId
        //                    ,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
        //                    ,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0'))
        //                    ,SUM(tirc.Quantity) as TIRCQty
        //                    ,(InvDetail.Rate) as Rate
        //                    ,Sum(kk.TotalQuantity) as TIRCTotalQty
        //                     from dbo.JobWorkTransformationContractChild3 mi
        //                     left join dbo.JobWorkTransformationIssueReturnChild tirc on tirc.MaterialInputId=mi.Id
        //                     left join MST.MaterialMaster mm on mm.Id=mi.MaterialMasterId
        //                     left join dbo.JobWorkTransformationContractChild mp on mp.Id=mi.JobWorkTransformationContractChildMasterId
        //left  join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
        //                     left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleCodeId
        //                     left join(select SUM(Quantity) as TotalQuantity,MaterialInputId FROM dbo.JobWorkTransformationIssueReturnChild group by MaterialInputId) kk on kk.MaterialInputId=mi.id
        //                     left join TRN.InventoryMaterial inm on inm.MaterialMasterId=mm.Id and inm.ArticleId=mma.Id
        //                     left join (Select InventoryMaterialId,(sum( MaterialTranAmount)/sum(TransactionQty)) as Rate from TRN.InventoryReceiveDetail group by InventoryMaterialId) InvDetail on InvDetail.InventoryMaterialId=inm.Id
        //                     where mi.JobWorkTransformationContractChildMasterId IN ("+ MPId + ") group by mi.Id, mm.Id, mm.UserName,InvDetail.Rate ,mma.Id, mma.StandardName,mp.Quantity,mi.GrossConsumption,kk.TotalQuantity, InvDetail.InventoryMaterialId,mi.JobWorkTransformationContractChildMasterId,jwi.UserName ";
        //        }
        //            var SqlData = _sqlRepository.GetDataCollection(sql);
        //            StringCollection strCol = new StringCollection();
        //            string MaterialMasterList = "''";
        //            string MaterialMasterArticleList = "''";
        //            for (int i = 0; i < SqlData.Count; i++)
        //            {
        //                if (strCol.Contains(SqlData[i]["MaterialMasterId"].ToString()) == true && strCol.Contains(SqlData[i]["MaterialArticleId"].ToString()) == true)
        //                    continue;
        //                strCol.Add(SqlData[i]["MaterialMasterId"].ToString());
        //                strCol.Add(SqlData[i]["MaterialArticleId"].ToString());
        //                MaterialMasterList += ",'" + SqlData[i]["MaterialMasterId"].ToString() + "'";
        //                MaterialMasterArticleList += ",'" + SqlData[i]["MaterialArticleId"].ToString() + "'";

        //            }

        //            var LotNoList = _sqlRepository.GetDataCollection(@"select IRD.LotNo Text, IRD.LotNo Value,IM.MaterialMasterId, IM.ArticleId from trn.InventoryReceiveDetail IRD
        //                                                                   left join trn.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
        //                                                                   where IM.MaterialMasterId IN (" + MaterialMasterList + ") and IM.ArticleId IN (" + MaterialMasterArticleList + ") ");

        //            for (int i = 0; i < SqlData.Count; i++)
        //            {
        //                var temp = LotNoList.Where(ee => ee["MaterialMasterId"].ToString() == SqlData[i]["MaterialMasterId"].ToString() && ee["ArticleId"].ToString() == SqlData[i]["MaterialArticleId"].ToString()).ToList();

        //                SqlData[i]["LotNumberList"] = temp;
        //            }

        //            return SqlData;
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }

        //}

        public IEnumerable<object> GetMaterialInputData(IEnumerable<MaterialPlanning> SelectedMaterialPlanningData)
        {
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var MPId = "' '";

                foreach (var get in SelectedMaterialPlanningData)
                {
                    MPId += ",'" + get.Id + "' ";

                }

                string sql = @"select mi.Id,mi.JobWorkTransformationContractChildMasterId, jwi.UserName as JWOutputItem,jwii.UserName as JWInputItem
                            ,mm.Id as InputMaterialId,mm.Id MaterialMasterId,mm.UserName as MaterialMaster,mm.Code as InputMaterialCode, uom.UserName as MMUnit
                            ,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
                            ,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0'))
                            ,SUM(tirc.Quantity) as TIRCQty
                            ,Sum(kk.TotalQuantity) as TIRCTotalQty
                             from dbo.JobWorkTransformationContractChild3 mi
                             left join dbo.JobWorkTransformationIssueReturnChild tirc on tirc.MaterialInputId=mi.Id
							 left join HKP.JobWorkItem jwii on jwii.Id=mi.JobWorkItemId
							 left join MST.MaterialMaster mm on mm.Id=jwii.MaterialMasterId
							 left join scs.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
                             left join dbo.JobWorkTransformationContractChild mp on mp.Id=mi.JobWorkTransformationContractChildMasterId
							 left  join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
                             left join(select SUM(Quantity) as TotalQuantity,MaterialInputId FROM dbo.JobWorkTransformationIssueReturnChild group by MaterialInputId) kk on kk.MaterialInputId=mi.id
                             where mi.JobWorkTransformationContractChildMasterId IN (" + MPId + @")
							 group by mi.Id, mm.Id, mm.UserName,mp.Quantity,mi.GrossConsumption,kk.TotalQuantity,mi.JobWorkTransformationContractChildMasterId,jwi.UserName,jwii.UserName,uom.UserName,mm.Code  ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> GetLotNoRate(string LotNumber)
        {
            try
            {
                string sql = @"select IRD.Id, IRD.MaterialTranRate, IRD.InventoryMaterialId, IM.MaterialMasterId, IM.ArticleId from trn.InventoryReceiveDetail IRD
                               left join trn.InventoryMaterial IM ON IM.Id=IRD.InventoryMaterialId
                               where IRD.LotNo='" + LotNumber + @"' ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> getentitylist()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select Id as Value, UserName as Text from ORG.Entity where PlantId='" + identity.PlantId + "' ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> gejobworklocation()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select Id as Value, UserName as Text from HKP.MaterialStorage order by UserName ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetTransformationPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "InventoryIssue", out sID);
            return sID;
        }

        public void SaveIssueTransformation(Dictionary<string, object> data, string ContractId, string ContractType)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                //con.OpenDataSetThroughAdapter("select * from " + TableName + " where PositionCodeId='" + data["PositionCodeId"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //    throw new Exception("Same Position Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from trn.InventoryIssue where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    dr["Id"] = "IT" + GetTransformationPK();

                    dr["IssueDate"] = data["Date"];
                    dr["EmployeeId"] = data["EmployeeId"];
                    dr["Types"] = data["Types"];
                    dr["IssueType"] = data["IssueType"];
                    dr["MaterialStorageId"] = data["JobWorkLocationId"];
                    dr["IsConfirmed"] = data["IsConfirmed"];
                    dr["Remarks"] = data["Remarks"];
                    dr["EntityId"] = data["EntityId"];
                    dr["JWContractId"] = ContractId;
                    dr["ContractType"] = ContractType;

                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["CompanyId"] = identity.CompanyId;
                    dr["PlantId"] = identity.PlantId;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();

                    dr["IssueDate"] = data["Date"];
                    dr["EmployeeId"] = data["EmployeeId"];
                    dr["Types"] = data["Types"];
                    dr["IssueType"] = data["IssueType"];
                    dr["MaterialStorageId"] = data["JobWorkLocationId"];
                    dr["IsConfirmed"] = data["IsConfirmed"];
                    dr["Remarks"] = data["Remarks"];
                    dr["EntityId"] = data["EntityId"];
                    dr["JWContractId"] = ContractId;
                    dr["ContractType"] = ContractType;

                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["CompanyId"] = identity.CompanyId;
                    dr["PlantId"] = identity.PlantId;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;


                    dr.EndEdit();
                }
                data["Id"] = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // New Changes

        public IEnumerable<object> GetCostCenterLoadNewFun(string EntityId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"Select CostCn.Id Value,CostCn.UserName Text from [ORG].[EntityCostCenter] EnCostCn
                LEFT JOIN [ORG].[CostCenter] AS CostCn ON CostCn.Id=EnCostCn.CostCenterId
                LEFT JOIN [ORG].[Entity] AS En ON En.Id=EnCostCn.EntityId
                WHERE En.Id='" + EntityId + @"' ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }


    }
}
public class MaterialPlanning
{

    #region Scalar Properties

    public string Id { get; set; }
    public string JobWorkItem { get; set; }
    public string MaterialType { get; set; }
    public string ArticleCode { get; set; }

    public string OutputUnit { get; set; }
    public string Quantity { get; set; }
    public string OrderSpecific { get; set; }
    public string MaterialLocation { get; set; }


    #endregion Scalar Properties
}