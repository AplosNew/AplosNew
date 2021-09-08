using System;
using System.Collections.Generic;
using Library.Data.Sql;
using System.Data;
using OTSBD;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data;
using Library.Service.Logs;
using System.Reflection;
using Library.Service.Enums;

namespace Library.MaterialManagement.JobWork
{

    public class JobWorkReceiptValueAdded
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        string TableName = "dbo.JobWorkReceiptValueAdded";
        string TableName1 = "dbo.JobWorkReceiptValueAddedChild";

        public JobWorkReceiptValueAdded()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        // RECEIPT

        public IEnumerable<object> GetListOfPOGateEntry(string CompanyGroupId, string CompanyId, string PlantId, string partyCode)
        {
            try
            {

                var Sql = @"Select
                            GE.Id
                            ,REPLACE(CONVERT(CHAR(11), GE.EntryDate , 106),' ','-') AS EntryDate
                            ,P.Code PartyCode
                            ,GE.InvoicingPartyPlantId
                            ,GE.InvoicingByAddress
                            ,GE.DeliveryPartyPlantId
                            ,GE.DeliveryByAddress
                            ,GE.Description
                            ,GE.PackageQty
                            ,GE.ModeofTransport
                            ,GE.Bill
                            ,GE.PersonName
                            ,MobileNo
                            ,GE.Remarks
                            ,GE.AddedBy
                            ,p.UserName
                            ,p.Id
                            FROM TRN.GateEntry GE
                            left Join hkp.Party p on p.Id=GE.PartyId
                            Where GE.CompanyGroupId='" + CompanyGroupId + "' AND GE.CompanyId='" + CompanyId + "' AND GE.PlantId='" + PlantId + "' and p.Id='" + partyCode + "' and GE.GateEntryType='Vendor' AND isnull(GE.Id,'') not in (select isnull(GateEntryNoId, '') from dbo.JobWorkReceiptValueAdded) Order By GE.EntryDate DESC";
                //AND GE.Id not in(select GateEntryNo from trn.InventoryReceive)
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetListGateEntry(string CompanyGroupId, string CompanyId, string PlantId, string partyCode)
        {
            try
            {

                var Sql = @"Select
                        GE.Id
                        ,REPLACE(CONVERT(CHAR(11), GE.EntryDate , 106),' ','-') AS EntryDate
                        ,P.Code PartyCode
                        ,GE.InvoicingPartyPlantId
                        ,GE.InvoicingByAddress
                        ,GE.DeliveryPartyPlantId
                        ,GE.DeliveryByAddress
                        ,GE.Description
                        ,GE.PackageQty
                        ,GE.ModeofTransport
                        ,GE.Bill
                        ,GE.PersonName
                        ,MobileNo
                        ,GE.Remarks
                        ,GE.AddedBy
                        ,p.UserName
                        ,p.Id,GE.PartyId
                        FROM TRN.GateEntry GE
                        left Join hkp.Party p on p.Id=GE.PartyId
                        Where GE.CompanyGroupId='" + CompanyGroupId + "' AND GE.CompanyId='" + CompanyId + "' AND GE.PlantId='" + PlantId + "' and p.Id='" + partyCode + "' and GE.GateEntryType='Vendor' and GE.GateEntryType='Vendor' AND isnull(GE.Id,'') not in (select isnull(GateEntryNo, '') from trn.InventoryReceive) Order By GE.EntryDate DESC";
                //AND GE.Id not in(select GateEntryNo from trn.InventoryReceive)
                return _sqlRepository.GetDataCollection(Sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Product.ToString()));
            }
        }

        public IEnumerable<object> GetIndividualReportData(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select distinct rt.Id,tc.Id as ContractId, rt.Date, FORMAT(rt.Date,'dd-MMM-yyyy') as ReceiveDate, rt.ByWhomId, rt.DocumentReferenceNo,rt.InvoiceNo, rt.GateEntryNoId 
                   ,rt.Remarks, FORMAT(rt.DocumentDate,'dd-MMM-yyyy') as ReceiveDocumentDate, FORMAT(rt.InvoiceDate,'dd-MMM-yyyy') as ReceiveInvoiceDate
                   ,emp.EmployeeName, emp.EmployeeCode
                    from dbo.JobWorkReceiptTransformation rt left join dbo.JobWorkReceiptTransformationChild rtc on rt.Id=rtc.JobWorkReceiptTransformationMasterId
                    left join dbo.EmployeeInformation emp on emp.SystemId=rt.ByWhomId
					left join TRN.GateEntry ge on ge.Id=rt.GateEntryNoId
                    left join dbo.JobWorkTransformationContractChild mp on mp.Id=rtc.MaterialPlanningId
                    left join dbo.JobWorkTransformationContract tc on tc.Id=mp.JobWorkTransformationContractMasterId
                    where tc.Id='" + Id + @"' order by rt.Date desc ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> GetReceiptVAChildData(string PKId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select Sum(irc.Quantity) as TotalIssuedQty,irc.ContractLineItemId,irc.OrderChildId, jwi.UserName as JWOutputItem,jwa.UserName as JobWorkActivity, mma.StandardName as Article, mp.OrderSpecific
									   ,TotalReceivedQty= case when mp.OrderSpecific='Yes' then (ISNULL(kk.RQty,'0')) else (ISNULL(rc.ReceivedQty,'0')) end
									   ,ToReceive= case when mp.OrderSpecific='Yes' then Sum(irc.Quantity)- (ISNULL(kk.RQty,'0')) else Sum(irc.Quantity)- (ISNULL(rc.ReceivedQty,'0')) end
                                       from dbo.JobWorkIssueReturnChild irc
                                       left join dbo.JobWorkValueAddedContractChild mp on mp.Id=irc.ContractLineItemId
									   left join hkp.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
									   left join dbo.JobWorkValueAddedContractChild2 owr on owr.Id=irc.OrderChildId
									   left join dbo.JobWorkValueAddedContract vc on vc.Id=mp.JobWorkValueAddedContractMasterId
									   left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
									   left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleCodeId
									   left join (select Sum(ReceivedQuantity) as RQty, ContractLineItemId, OrderChildId from dbo.JobWorkReceiptValueAddedChild group by ContractLineItemId, OrderChildId)
									    kk on kk.ContractLineItemId=irc.ContractLineItemId and kk.OrderChildId=irc.OrderChildId
										left join (select Sum(ReceivedQuantity) as ReceivedQty, ContractLineItemId from dbo.JobWorkReceiptValueAddedChild group by ContractLineItemId)
									    rc on rc.ContractLineItemId=irc.ContractLineItemId
									   where vc.Id='" + PKId + @"'
									   group by irc.ContractLineItemId,irc.OrderChildId,jwi.UserName,mma.StandardName, kk.RQty,mp.OrderSpecific,rc.ReceivedQty,jwa.UserName ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> GetReceiptVAChildDatabyId(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select rvc.Id,rvc.ContractLineItemId,rvc.OrderChildId, jwi.UserName as JWOutputItem, mma.StandardName as Article, mp.OrderSpecific
                                        ,TotalIssuedQty= case when mp.OrderSpecific='Yes' then (ISNULL(kk.TotalIssuedQty,'0')) else (ISNULL(k.TIssuedQty,'0')) end
                                       ,TotalReceivedQty= case when mp.OrderSpecific='Yes' then (ISNULL(rq.TotalReceQty,'0')) else (ISNULL(r.TRQty,'0')) end
									   ,ToReceive= case when mp.OrderSpecific='Yes' then kk.TotalIssuedQty- rq.TotalReceQty else k.TIssuedQty - r.TRQty end
                                        ,rvc.ReceivedQuantity as ReceivedQty
                                         from dbo.JobWorkReceiptValueAddedChild rvc left join dbo.JobWorkIssueReturnChild irc on 
										 irc.OrderChildId=rvc.OrderChildId
										 left join(select Sum(Quantity) as TotalIssuedQty, OrderChildId from dbo.JobWorkIssueReturnChild group by OrderChildId )
										 kk on kk.OrderChildId=rvc.OrderChildId
										 left join(select Sum(Quantity) as TIssuedQty,ContractLineItemId from dbo.JobWorkIssueReturnChild group by ContractLineItemId )
										 k on k.ContractLineItemId=rvc.ContractLineItemId
										 left join(select Sum(ReceivedQuantity) as TotalReceQty, OrderChildId from dbo.JobWorkReceiptValueAddedChild group by OrderChildId )
										 rq on rq.OrderChildId=rvc.OrderChildId
										 left join(select Sum(ReceivedQuantity) as TRQty,ContractLineItemId from dbo.JobWorkReceiptValueAddedChild group by ContractLineItemId )
										 r on r.ContractLineItemId=rvc.ContractLineItemId
										 left join dbo.JobWorkValueAddedContractChild mp on mp.Id=rvc.ContractLineItemId
										 left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
									     left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleCodeId
                                         where rvc.JobWorkReceiptValueAddedMasterId='" + Id + @"'
										 group by rvc.Id,rvc.ContractLineItemId,rvc.OrderChildId,rvc.ReceivedQuantity,kk.TotalIssuedQty,k.TIssuedQty,rq.TotalReceQty, r.TRQty, mp.OrderSpecific,jwi.UserName,mma.StandardName ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IEnumerable<object> GetGradeWiseQuantityList()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select * from dbo.GradeWiseQuantityDetails order by GradeNo ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetVAGradeWiseQuantityList(string MasterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select gw.*,gwd.GradeName from dbo.ReceiptValueAddedGradeWise gw left join dbo.JobWorkReceiptValueAddedChild rvc on rvc.Id=gw.JobWorkReceiptValueAddedChildMasterId
                                           left join dbo.GradeWiseQuantityDetails gwd on gwd.Id=gw.GradeWiseQuantityId
										   where gw.JobWorkReceiptValueAddedChildMasterId='" + MasterId + @"' ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetTransformationReceiptCurrency(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select c.Id as Value, c.Code as Text from SCS.Currency c left join dbo.JWTransformationPurchaseOrder po on c.Id=po.CurrencyId
                               where po.Id='"+ Id + @"' ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobWorkReceiptValueAdded", out sID);
            return sID;
        }

        public void Create(Dictionary<string, object> data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");               

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    dr["Id"] = "RV" + GetPK();

                    dr["Date"] = data["Date"];
                    dr["ByWhomId"] = data["ByWhomId"];
                    dr["DocumentReferenceNo"] = data["DocumentReferenceNo"];
                    dr["DocumentDate"] = data["DocumentDate"];

                    dr["InvoiceNo"] = data["InvoiceNo"];
                    dr["InvoiceDate"] = data["InvoiceDate"];
                    dr["GateEntryNoId"] = data["GateEntryNoId"];
                    dr["Remarks"] = data["Remarks"];

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

                    dr["Date"] = data["Date"];
                    dr["ByWhomId"] = data["ByWhomId"];
                    dr["DocumentReferenceNo"] = data["DocumentReferenceNo"];
                    dr["DocumentDate"] = data["DocumentDate"];

                    dr["InvoiceNo"] = data["InvoiceNo"];
                    dr["InvoiceDate"] = data["InvoiceDate"];
                    dr["GateEntryNoId"] = data["GateEntryNoId"];
                    dr["Remarks"] = data["Remarks"];

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

        private string GetReceiptVCPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobWorkReceiptValueAddedChild", out sID);
            return sID;
        }

        public void SaveReceiptVAChildTab(IEnumerable<jobworkreceiptvalueaddedchild> ReceiptVAChildData, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet ExistOrNot;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var MPId = "' '";
                var OWRId = "''";
                foreach (var empitem in ReceiptVAChildData)
                {
                    MPId += ",'" + empitem.ContractLineItemId + "' ";
                    OWRId += ",'" + empitem.OrderChildId + "' ";
                }
                con.OpenDataSetThroughAdapter("select * from " + TableName1 + " where (ContractLineItemId IN ( " + MPId + " ) or OrderChildId IN (" + OWRId + ")) and JobWorkReceiptValueAddedMasterId='" + MasterId + "'  ", out ExistOrNot, false, "1");

                foreach (var item in ReceiptVAChildData)
                {
                    if (ExistOrNot.Tables[0].DefaultView.Count == 0 || ExistOrNot.Tables[0].DefaultView.Count > 0)
                    {
                        DataRow dr = ExistOrNot.Tables[0].NewRow();
                        dr["Id"] = "RVA" + GetReceiptVCPK();

                        dr["JobWorkReceiptValueAddedMasterId"] = MasterId;

                        dr["ContractLineItemId"] = item.ContractLineItemId;
                        dr["OrderChildId"] = item.OrderChildId;
                        dr["ReceivedQuantity"] = item.ReceivedQty;

                        dr["Remarks"] = item.Remarks;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        ExistOrNot.Tables[0].Rows.Add(dr);

                    }

                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(ExistOrNot);

            }
            catch (Exception ex)
            {

            }
        }

        // GRADE WISE VALUE ADDED

        private string GetGradeWiseVAPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ReceiptValueAddedGradeWise", out sID);
            return sID;
        }

        public void SaveGradeWiseValueAdded(IEnumerable<ReceiptValueAddedGradeWise> VAGradeWiseData, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet ExistOrNot;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from dbo.ReceiptValueAddedGradeWise where JobWorkReceiptValueAddedChildMasterId='" + MasterId + "'  ", out ExistOrNot, false, "1");

                foreach (var item in VAGradeWiseData)
                {
                    if (ExistOrNot.Tables[0].DefaultView.Count == 0 || ExistOrNot.Tables[0].DefaultView.Count > 0)
                    {
                        DataRow dr = ExistOrNot.Tables[0].NewRow();
                        dr["Id"] = "GV" + GetGradeWiseVAPK();

                        dr["JobWorkReceiptValueAddedChildMasterId"] = MasterId;

                        dr["GradeWiseQuantityId"] = item.Id;
                        dr["GradeWiseQuantity"] = item.GradeWQty;
                        dr["Remarks"] = item.GWRemarks;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        ExistOrNot.Tables[0].Rows.Add(dr);

                    }

                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(ExistOrNot);

            }
            catch (Exception ex)
            {

            }
        }

        // RECEIPT TRANSFORMATION

        private string GetReceiptTPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobWorkReceiptTransformation", out sID);
            return sID;
        }

        public void SaveReceiptTransformation(Dictionary<string, object> data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from TRN.InventoryReceive where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "InventoryReceive", out _Id);

                    data["Id"] =  _Id;

                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();

                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                //#region data update
                //if (dsMaster.Tables[0].Rows.Count == 0)
                //{
                //    DataRow dr = dsMaster.Tables[0].NewRow();
                //    dr["Id"] = "RT" + GetReceiptTPK();

                //    dr["Date"] = data["Date"];
                //    dr["ByWhomId"] = data["ByWhomId"];
                //    dr["DocumentReferenceNo"] = data["DocumentReferenceNo"];
                //    dr["DocumentDate"] = data["DocumentDate"];

                //    dr["InvoiceNo"] = data["InvoiceNo"];
                //    dr["InvoiceDate"] = data["InvoiceDate"];
                //    dr["GateEntryNoId"] = data["GateEntryNoId"];
                //    dr["Remarks"] = data["Remarks"];

                //    dr["AddedBy"] = identity.Name;
                //    dr["AddedDate"] = System.DateTime.Now.ToString();
                //    dr["AddedFromIP"] = identity.IPAddress;
                //    dr["UpdatedBy"] = identity.Name;
                //    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                //    dr["UpdatedFromIP"] = identity.IPAddress;


                //    dsMaster.Tables[0].Rows.Add(dr);
                //}
                //else
                //{
                //    //edit
                //    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                //    dr.BeginEdit();

                //    dr["Date"] = data["Date"];
                //    dr["ByWhomId"] = data["ByWhomId"];
                //    dr["DocumentReferenceNo"] = data["DocumentReferenceNo"];
                //    dr["DocumentDate"] = data["DocumentDate"];

                //    dr["InvoiceNo"] = data["InvoiceNo"];
                //    dr["InvoiceDate"] = data["InvoiceDate"];
                //    dr["GateEntryNoId"] = data["GateEntryNoId"];
                //    dr["Remarks"] = data["Remarks"];

                //    dr["AddedBy"] = identity.Name;
                //    dr["AddedDate"] = System.DateTime.Now.ToString();
                //    dr["AddedFromIP"] = identity.IPAddress;
                //    dr["UpdatedBy"] = identity.Name;
                //    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                //    dr["UpdatedFromIP"] = identity.IPAddress;


                //    dr.EndEdit();
                //}
                //data["Id"] = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                //#endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

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
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

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

        // RECEIPT TRANSFORMATION CHILD DATA

        public IEnumerable<object> GetReceiptTransChildData(string PKId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //                string sql = @"select mp.Id, SUM(mp.Quantity) as PlanQuantity,jwi.UserName as JWOutputItem,jwa.UserName as JobWorkActivity, mma.StandardName as Article
                //,TotalReceivedQty=ISNULL( kk.TotalReceivedQuantity,'0')
                //,ToReceive= Sum(mp.Quantity)- ISNULL( kk.TotalReceivedQuantity,'0')
                //from dbo.JobWorkTransformationContractChild mp left join dbo.JobWorkTransformationContract tc on tc.Id=mp.JobWorkTransformationContractMasterId
                //left join hkp.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
                //left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
                //left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleCodeId
                //left join (select Sum(ReceivedQuantity) as TotalReceivedQuantity,MaterialPlanningId from dbo.JobWorkReceiptTransformationChild group by MaterialPlanningId)
                //kk on kk.MaterialPlanningId=mp.Id
                //where tc.Id='" + PKId + @"'
                //group by mp.Id,jwi.UserName, mma.StandardName,jwa.UserName,kk.TotalReceivedQuantity ";
                string sql = @"select
                        tc.Id JWTCMId
                        ,mp.Id JWTCMDId
                        ,jwi.UserName as JWOutputItem
                        ,jwa.UserName as JobWorkActivity
                        , MGM.UserName AS MaterialGroupMasterName
                        , MM.Id MaterialMasterId
                        , MM.UserName
                        , mma.Id ArticleId
                        , mma.StandardName as StandardName
                        ,null MaterialStorageId
                        ,TUoM.Id BaseUOMId
   
                        , null FirstCharacteristicsId, null  FirstCharacteristics
                        , null FirstCharacteristicsValueId, null  FirstCharacteristicsValue
                        , null SecondCharacteristicsId, null  SecondCharacteristics
                        , null SecondCharacteristicsValueId, null SecondCharacteristicsValue
                        , null ThirdCharacteristicsId, null ThirdCharacteristics
                        , null ThirdCharacteristicsValueId, null  ThirdCharacteristicsValue
                        --, SUM(mp.Quantity) as PlanQuantity
                        --,TotalReceivedQty=ISNULL( kk.TotalReceivedQuantity,'0')
                        --,ToReceive= Sum(mp.Quantity)- ISNULL( kk.TotalReceivedQuantity,'0')
                        , mp.Quantity AS PlanQuantity
                         , ISNULL(rcvqty.TransactionQty,'0') AS GRNRcvQty             
                        ,0 AS TransactionQty
                        ,ISNULL(mp.Quantity,0)- ISNULL(rcvqty.TransactionQty,'0') As Balance
                        ,null QtyStatus
                         , TransactionUoMId=CASE when mp.OutputMaterialUOMId IS NULL THEN mp.TransactionUoMId ELSE mp.OutputMaterialUOMId END
                        , TransactionUoM= CASE when mp.OutputMaterialUOMId  IS NULL then TUoM1.UserName ELSE TUoM.UserName END
                        , 0 TransactionRate
                        , null  CurrencyName
                        , 0 ToCurrencyRate
                        ,0 TransactionAmount
                        ,0 AS TrnAmount  
                        ,0 AS BaseTaxAmount
                        ,0 AS TaxAmount
                        , 0 AS ChargesAmount
                        ,0 AS  ServiceCharge
                        , 0 AS ServiceTax
                        , null CountryId
                        ,'True' enableid
                        ,null POMaterialTaxList                            
                        ,0 AS TotalMaterialTranAmount
                        , 0 AS ToTalMaterialBooksCurrencyAmount
                        ,null InvoicingByAddress
                        ,null DeliveryByAddress
                        ,null RequisitionId
                        ,null RequisitionDetailId
                        ,0 ShortageQty
                        ,0 RejectionQty
                        ,null MaterialDetail
                        ,null AS [check] 
                        ,null MaterialDetail
                        ,null PurchaseDocAcceptanceDetailId
                        ,0 POClosStatus
                        ,null CountryName
                        ,null CountryId 
                        ,MM.IsAsset
                        ,0 TotalTaxAmount
                        ,0 GrossAmount
                        ,0 DiscountAmount
                        ,'' QualityStatus
                        ,null POUoMId
                        ,0 Tolerance--,sum(CC3.GrossConsumption*vvvv.Rate) GrossConsumption--,sum(vvvv.Rate) Rate
                        --,(CC3.GrossConsumption*vvvv.Rate) GrossConsumption
                        --,GrossConsumption=isnull((CC3.GrossConsumption*vvvv.Rate),'0')
						,vvvv.ConsumptionAmount as GrossConsumption
                        from dbo.JobWorkTransformationContractChild mp 
                        left join dbo.JWTransformationPurchaseOrder tc on tc.Id=mp.JobWorkTransformationContractMasterId
                        left join hkp.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
                        left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
                        left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleId
                        left JOIN MST.MaterialMaster AS MM ON MM.Id=mma.MaterialMasterId
                        LEFT JOIN MST.MaterialGroupMaster AS MGM ON MM.MaterialGroupMasterId=MGM.Id
                        --LEFT JOIN MST.MaterialMasterArticle AS ART ON IRD.ArticleId=ART.Id
                        --LEFT JOIN HKP.Characteristics AS FC ON IRD.FirstCharacteristicsId=FC.Id
                        --LEFT JOIN HKP.Characteristics AS SC ON IRD.SecondCharacteristicsId=SC.Id
                        --LEFT JOIN HKP.Characteristics AS TC ON IRD.ThirdCharacteristicsId=TC.Id
                        --LEFT JOIN HKP.CharacteristicsValue AS FCV ON IRD.FirstCharacteristicsValueId=FCV.Id
                        --LEFT JOIN HKP.CharacteristicsValue AS SCV ON IRD.SecondCharacteristicsValueId=SCV.Id
                        --LEFT JOIN HKP.CharacteristicsValue AS TCV ON IRD.ThirdCharacteristicsValueId=TCV.Id
                        LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON mp.OutputMaterialUOMId=TUoM.Id
                        	LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM1 ON mp.TransactionUoMId=TUoM1.Id
                        left join (select Sum(IRD.TransactionQty) as TotalReceivedQuantity,IR.TransformationContractId from TRN.InventoryReceiveDetail IRD left join TRN.InventoryReceive IR
                        on IRD.InventoryReceiveId=IR.Id where MaterialFor='JWOUTPUTMaterial' group by IR.TransformationContractId)kk on kk.TransformationContractId=mp.JobWorkTransformationContractMasterId
                        left join (
                        --select JobWorkTransformationContractChildMasterId, Sum(GrossConsumption) GrossConsumption  from dbo.JobWorkTransformationContractChild3 
                        --group by JobWorkTransformationContractChildMasterId
                         select Sum(x.GrossConsumption) as GrossConsumption ,x.JobWorkTransformationContractChildMasterId
									from (
									--Select GrossConsumption, JobWorkTransformationContractChildMasterId from dbo.JobWorkTransformationContractChild3 
									--group by ArticleId, JobWorkTransformationContractChildMasterId,GrossConsumption
                                    Select GrossConsumption, JobWorkTransformationContractChildMasterId from dbo.JobWorkTransformationContractChild3 mi
									left join TRN.InventoryIssueDetail IID on IID.JWTCMID=mi.JobWorkTransformationContractChildMasterId
									left join TRN.InventoryIssue II on II.Id=IID.InventoryIssueId
									where II.JWContractId='" + PKId + @"'
									group by mi.ArticleId, mi.JobWorkTransformationContractChildMasterId,mi.GrossConsumption
									) x group by x.JobWorkTransformationContractChildMasterId
                         )CC3 ON CC3.JobWorkTransformationContractChildMasterId=mp.Id
                        left join(select JWTCMDId, Sum(isnull(TransactionQty,0)) TransactionQty from trn.InventoryReceiveDetail group by JWTCMDId)rcvqty ON rcvqty.JWTCMDId=mp.Id
                        left join( --select  mp1.Id ,II.JWContractId 
								 --,sum(IID.PolicyAmount/IID.TransactionQty) Rate
                                 --,sum(IID.PolicyAmount) PolicyAmt,sum(IID.TransactionQty) TQty
								 --,Rate=round((sum(IID.PolicyAmount) / sum(IID.TransactionQty)),4)
								 --FROM trn.InventoryIssueDetail IID
								 --left join trn.InventoryIssue II On II.Id=IID.InventoryIssueId
								 --left join trn.InventoryMaterial IM ON IM.Id=IID.InventoryMaterialId
								 --left JOIN MST.MaterialMaster AS MM ON MM.Id=IM.MaterialMasterId
								 --left join MST.MaterialMasterArticle mma on mma.Id=IM.ArticleId
								 --left join dbo.JWTransformationPurchaseOrder tc on tc.Id=II.JWContractId
								 --left join dbo.JobWorkTransformationContractChild mp1 ON mp1.JobWorkTransformationContractMasterId=Tc.Id								
								 --group by  mp1.Id,II.JWContractId 

                                   select  IID.JWTCMID,II.JWContractId 
								 ,sum(IID.PolicyAmount) PolicyAmt,sum(IID.TransactionQty) TQty
								 ,Rate=round((sum(IID.PolicyAmount) / sum(IID.TransactionQty)),4)
                                 ,ConsumptionAmount= (round((sum(IID.PolicyAmount) / sum(IID.TransactionQty)),4) * sum(IID.TransactionQty))
								 FROM trn.InventoryIssueDetail IID
								 left join trn.InventoryIssue II On II.Id=IID.InventoryIssueId
								 left join trn.InventoryMaterial IM ON IM.Id=IID.InventoryMaterialId
								 left JOIN MST.MaterialMaster AS MM ON MM.Id=IM.MaterialMasterId
								 left join MST.MaterialMasterArticle mma on mma.Id=IM.ArticleId
								 left join dbo.JWTransformationPurchaseOrder tc on tc.Id=II.JWContractId
								 where II.JWContractId='" + PKId + @"'
								 group by II.JWContractId,IID.JWTCMID
                                 )vvvv ON vvvv.JWContractId=tc.Id 
                        where tc.Id='" + PKId + @"' group by mp.Quantity ,ISNULL(rcvqty.TransactionQty,'0'),mp.Id,jwi.UserName, mma.StandardName,jwa.UserName,kk.TotalReceivedQuantity
                        , MGM.UserName, MM.Id, MM.UserName, mma.Id ,MM.IsAsset,tc.Id, TUoM.Id, TUoM.UserName,TUoM.Id,TUoM1.Id,TUoM1.UserName,mp.TransactionUoMId , mp.OutputMaterialUOMId
                         ,CC3.GrossConsumption,vvvv.Rate,vvvv.ConsumptionAmount";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }

        }

        private string GetTransChildPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobWorkReceiptTransformationChild", out sID);
            return sID;
        }

        public void SaveReceiptTransChildTab(IEnumerable<JobWorkReceiptTransformationChild> ReceiptTransChildData, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet ExistOrNot;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from dbo.JobWorkReceiptTransformationChild where JobWorkReceiptTransformationMasterId='" + MasterId + "'  ", out ExistOrNot, false, "1");

                foreach (var item in ReceiptTransChildData)
                {
                    if (ExistOrNot.Tables[0].DefaultView.Count == 0 || ExistOrNot.Tables[0].DefaultView.Count > 0)
                    {
                        DataRow dr = ExistOrNot.Tables[0].NewRow();
                        dr["Id"] = "RTC" + GetTransChildPK();

                        dr["JobWorkReceiptTransformationMasterId"] = MasterId;

                        dr["MaterialPlanningId"] = item.Id;
                        dr["ReceivedQuantity"] = item.ReceivedQty;
                        dr["Remarks"] = item.Remarks;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        ExistOrNot.Tables[0].Rows.Add(dr);

                    }

                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(ExistOrNot);

            }
            catch (Exception ex)
            {

            }
        }

        //  GRADE WISE QUANTITY TRANSFORMATION

        public IEnumerable<object> GetTransGradeQuantityList(string MasterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select gw.*,gwd.GradeName from dbo.ReceiptTransformationGradeWise gw left join dbo.JobWorkReceiptTransformationChild rtc on rtc.Id=gw.JobWorkReceiptTransformationChildMasterId
                                           left join dbo.GradeWiseQuantityDetails gwd on gwd.Id=gw.GradeWiseQuantityId
										   where gw.JobWorkReceiptTransformationChildMasterId='" + MasterId + @"' ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetGradeWiseTransPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ReceiptTransformationGradeWise", out sID);
            return sID;
        }

        public void SaveGradeWiseTrans(IEnumerable<ReceiptTransformationGradeWise> TransGradeWiseData, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet ExistOrNot;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from dbo.ReceiptTransformationGradeWise where JobWorkReceiptTransformationChildMasterId='" + MasterId + "'  ", out ExistOrNot, false, "1");

                foreach (var item in TransGradeWiseData)
                {
                    if (ExistOrNot.Tables[0].DefaultView.Count == 0 || ExistOrNot.Tables[0].DefaultView.Count > 0)
                    {
                        DataRow dr = ExistOrNot.Tables[0].NewRow();
                        dr["Id"] = "GT" + GetGradeWiseTransPK();

                        dr["JobWorkReceiptTransformationChildMasterId"] = MasterId;

                        dr["GradeWiseQuantityId"] = item.Id;
                        dr["GradeWiseQuantity"] = item.GradeWQty;
                        dr["Remarks"] = item.GWRemarks;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        ExistOrNot.Tables[0].Rows.Add(dr);

                    }

                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(ExistOrNot);

            }
            catch (Exception ex)
            {

            }
        }

        public IEnumerable<object> GetReceiptTransChildDatabyId(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select mp.Id, SUM(mp.Quantity) as PlanQuantity,jwi.UserName as JWOutputItem,jwa.UserName as JobWorkActivity, mma.StandardName as Article
,TotalReceivedQty=ISNULL( kk.TotalReceivedQuantity,'0')
,ToReceive= Sum(mp.Quantity)- ISNULL( kk.TotalReceivedQuantity,'0'), rtc.ReceivedQuantity as ReceivedQty, rtc.Id as ReceiptTransChildId, rtc.Remarks
from dbo.JobWorkTransformationContractChild mp left join dbo.JobWorkTransformationContract tc on tc.Id=mp.JobWorkTransformationContractMasterId
left join hkp.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleCodeId
left join (select Sum(ReceivedQuantity) as TotalReceivedQuantity,MaterialPlanningId from dbo.JobWorkReceiptTransformationChild group by MaterialPlanningId)
kk on kk.MaterialPlanningId=mp.Id
left join dbo.JobWorkReceiptTransformationChild rtc on rtc.MaterialPlanningId=mp.Id
where rtc.JobWorkReceiptTransformationMasterId='" + Id + @"'
group by mp.Id,jwi.UserName, mma.StandardName,jwa.UserName,kk.TotalReceivedQuantity,rtc.ReceivedQuantity,rtc.Id,rtc.Remarks ";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        // BY PRODUCT RECEIPT

        public IEnumerable<object> GetByProductApplicableList(string Id)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                //        string sql = @"select tbp.Id,jwit.UserName as JWOutputItem,jwi.UserName as ByProductItem,mma.StandardName as ByProductArticle, mm.UserName as ByProductMaterial           
                //,TQty=(mi.NetConsumption * mp.Quantity)
                //                      ,TotalReqQty=((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100)
                //                      ,ISNULL(rvbp.TotalReceivedQuantity,'0') as TotalReceivedQty
                //                      , ToReceive=((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) - ISNULL(rvbp.TotalReceivedQuantity,'0')
                //                       from dbo.JobWorkTransformationContractChild4 tbp 
                //                      left join HKP.JobWorkItem jwi on jwi.Id=tbp.JobWorkItemId
                //                     left join dbo.JobWorkTransformationContractChild3 mi on mi.Id=tbp.JobWorkTransformationContractChild3MasterId
                //                     left join dbo.JobWorkTransformationContractChild mp on mp.Id=mi.JobWorkTransformationContractChildMasterId
                //                     left join HKP.JobWorkItem jwit on jwit.Id=mp.JobWorkItemMasterId
                //                     left join MST.MaterialMasterArticle mma on mma.Id=tbp.ArticleId
                //left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
                //                     left join (Select SUM(ReceivedQuantity) as TotalReceivedQuantity,ByProductId from dbo.JobWorkReceiptTransformationByProduct group by ByProductId)
                //                     rvbp on rvbp.ByProductId=tbp.Id
                //                     left join dbo.JobWorkTransformationContract tc on tc.Id=mp.JobWorkTransformationContractMasterId
                //                      where tc.Id='" + Id + @"' ";

                string sql = @"select 
                                mi.Id JWTCMByProductId
                                ,tbp.Id JWTCMDByProductId
                                ,jwit.UserName as JWOutputItem
                                ,jwi.UserName as ByProductItem
                                , mma.Id ArticleId
                                , mma.StandardName as StandardName
                                , MM.UserName
                                , MM.Id MaterialMasterId
                                ,null MaterialStorageId
                                ,TUoM.Id BaseUOMId
                                , null FirstCharacteristicsId, null  FirstCharacteristics
                                , null FirstCharacteristicsValueId, null  FirstCharacteristicsValue
                                , null SecondCharacteristicsId, null  SecondCharacteristics
                                , null SecondCharacteristicsValueId, null SecondCharacteristicsValue
                                , null ThirdCharacteristicsId, null ThirdCharacteristics
                                , null ThirdCharacteristicsValueId, null  ThirdCharacteristicsValue
                                , sum(((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100)) AS PlanQuantity
                                , Sum(ISNULL(rcvqty.TransactionQty,'0')) AS GRNRcvQty          
                                ,0 AS TransactionQty
                                ,SUM(((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) - ISNULL(rcvqty.TransactionQty,'0')) As Balance
                                ,null QtyStatus
                                , TUoM.Id TransactionUoMId
                                , TUoM.UserName TransactionUoM
                                --, 0 TransactionRate
                                , null  CurrencyName
                                , tbp.StandardRate TransactionRate
                                ,0 TransactionAmount
                                ,0 AS TrnAmount  
                                ,0 AS BaseTaxAmount
                                ,0 AS TaxAmount
                                , 0 AS ChargesAmount
                                ,0 AS  ServiceCharge
                                , 0 AS ServiceTax
                                , null CountryId
                                ,'True' enableid
                                ,null POMaterialTaxList                            
                                ,0 AS TotalMaterialTranAmount
                                , 0 AS ToTalMaterialBooksCurrencyAmount
                                ,null InvoicingByAddress
                                ,null DeliveryByAddress
                                ,null RequisitionId
                                ,null RequisitionDetailId
                                ,0 ShortageQty
                                ,0 RejectionQty
                                ,null MaterialDetail
                                ,null AS [check] 
                                ,null MaterialDetail
                                ,null PurchaseDocAcceptanceDetailId
                                ,0 POClosStatus
                                ,null CountryName
                                ,null CountryId 
                                ,MM.IsAsset
                                ,0 TotalTaxAmount
                                ,0 GrossAmount
                                ,0 DiscountAmount
                                ,'' QualityStatus
                                ,null POUoMId
                                ,0 Tolerance
                                from dbo.JobWorkTransformationContractChild4 tbp 
                                left join HKP.JobWorkItem jwi on jwi.Id=tbp.JobWorkItemId
                                left join dbo.JobWorkTransformationContractChild3 mi on mi.Id=tbp.JobWorkTransformationContractChild3MasterId
                                left join dbo.JobWorkTransformationContractChild mp on mp.Id=mi.JobWorkTransformationContractChildMasterId
                                left join HKP.JobWorkItem jwit on jwit.Id=mp.JobWorkItemMasterId
                                left join MST.MaterialMasterArticle mma on mma.Id=tbp.ArticleId
                                left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
                                left join (select Sum(IRD.TransactionQty) as TotalReceivedQuantity,IR.TransformationContractId from TRN.InventoryReceiveDetail IRD left join TRN.InventoryReceive IR
                                 on IRD.InventoryReceiveId=IR.Id where MaterialFor='JWBYPRODUCTMaterial' group by IR.TransformationContractId)
                                rvbp on rvbp.TransformationContractId=mp.JobWorkTransformationContractMasterId
                                --rvbp on rvbp.ByProductId=tbp.Id
                                left join dbo.JWTransformationPurchaseOrder tc on tc.Id=mp.JobWorkTransformationContractMasterId
                                LEFT JOIN [SCS].[UnitOfMeasurement] AS TUoM ON mp.OutputMaterialUOMId=TUoM.Id
                                left join(select JWTCMDByProductId, Sum(isnull(TransactionQty,0)) TransactionQty from trn.InventoryReceiveDetail group by JWTCMDByProductId)rcvqty ON rcvqty.JWTCMDByProductId=tbp.Id
                                where tc.Id='" + Id + @"'
                                group by tbp.Id
                                ,jwit.UserName 
                                ,jwi.UserName 
                                , mma.Id 
                                , mma.StandardName 
                                , MM.UserName
                                , MM.Id 
                                ,MM.IsAsset,mi.Id,TUoM.Id, TUoM.UserName,TUoM.Id, tbp.StandardRate";

                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetByProductPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobWorkReceiptTransformationByProduct", out sID);
            return sID;
        }

        public void SaveByProduct(IEnumerable<JobWorkReceiptTransformationByProduct> ByProductData, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet ExistOrNot;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from dbo.JobWorkReceiptTransformationByProduct where JobWorkReceiptTransformationMasterId='" + MasterId + "'  ", out ExistOrNot, false, "1");

                foreach (var item in ByProductData)
                {
                    if (ExistOrNot.Tables[0].DefaultView.Count == 0 || ExistOrNot.Tables[0].DefaultView.Count > 0)
                    {
                        DataRow dr = ExistOrNot.Tables[0].NewRow();
                        dr["Id"] = "RBP" + GetByProductPK();

                        dr["JobWorkReceiptTransformationMasterId"] = MasterId;

                        dr["ByProductId"] = item.Id;
                        dr["ReceivedQuantity"] = item.ReceiveQuantity;
                        dr["Remarks"] = item.Remarks;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        ExistOrNot.Tables[0].Rows.Add(dr);

                    }

                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(ExistOrNot);

            }
            catch (Exception ex)
            {

            }
        }

        // Report code

        public DataTable GetTransformationContractReportDataById(string PrintTabId, string IssueId)
        {
            try
            {
                //       string _sql = @"select tc.Id,TabType='Transformation', tc.EntityId,tc.VendorPartyId,tc.Remarks,FORMAT(tc.Date,'dd-MMM-yyyy') as ValueAddedDate,CONVERT(varchar(5),tc.[Time],108)[VACTime]
                //                           ,FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as VAProcessStartDate,
                //                           FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as VAProcessEndDate,FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as VAContractClosingDate,
                //                           e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName
                //,rt.Id as ReceiptId, rt.Date, FORMAT(rt.Date,'dd-MMM-yyyy') as ReceiveDate, rt.ByWhomId, rt.DocumentReferenceNo,rt.InvoiceNo, rt.GateEntryNoId 
                //                          ,rt.Remarks as ReceiptRemarks, FORMAT(rt.DocumentDate,'dd-MMM-yyyy') as ReceiveDocumentDate, FORMAT(rt.InvoiceDate,'dd-MMM-yyyy') as ReceiveInvoiceDate
                //                           ,emp.EmployeeName, emp.EmployeeCode
                //                           from dbo.JobWorkTransformationContract tc left join ORG.Entity e on e.Id=tc.EntityId
                //left join HKP.Party p on p.Id=tc.VendorPartyId
                //left join dbo.JobWorkTransformationContractChild mp on tc.Id=mp.JobWorkTransformationContractMasterId
                //left join dbo.JobWorkReceiptTransformationChild rtc on mp.Id=rtc.MaterialPlanningId
                //left join dbo.JobWorkReceiptTransformation rt on rt.Id=rtc.JobWorkReceiptTransformationMasterId
                // 			left join dbo.EmployeeInformation emp on emp.SystemId=rt.ByWhomId
                //           	left join TRN.GateEntry ge on ge.Id=rt.GateEntryNoId
                //                           WHERE tc.Id='" + PrintTabId + @"' and rt.Id='" + IssueId + @"' ";

                string _sql = @"select tc.Id,TabType='Transformation', tc.EntityId,tc.PartyId,tc.Remarks,FORMAT(tc.PODate,'dd-MMM-yyyy') as ValueAddedDate,CONVERT(varchar(5),tc.[Time],108)[VACTime]
                                    ,FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as VAProcessStartDate,
                                    FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as VAProcessEndDate,FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as VAContractClosingDate,
                                    e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName
									,rt.Id as ReceiptId, rt.GRNDate, FORMAT(rt.GRNDate,'dd-MMM-yyyy') as JWGRNDate, rt.ByWhomEmployeeId, rt.DocRefNo,rt.InvoiceNo
									, rt.GateEntryNo 
                                   --,rt.Remarks as ReceiptRemarks
								   , FORMAT(rt.DocDate,'dd-MMM-yyyy') as ReceiveDocumentDate, FORMAT(rt.InvoiceDate,'dd-MMM-yyyy') as ReceiveInvoiceDate
                                    ,emp.EmployeeName, emp.EmployeeCode
                                    from dbo.JWTransformationPurchaseOrder tc left join ORG.Entity e on e.Id=tc.EntityId
									left join HKP.Party p on p.Id=tc.PartyId
									left join dbo.JobWorkTransformationContractChild mp on tc.Id=mp.JobWorkTransformationContractMasterId
									left join trn.inventoryreceive rt on rt.TransformationContractId=tc.Id
					     			left join dbo.EmployeeInformation emp on emp.SystemId=rt.ByWhomEmployeeId
                                    WHERE tc.Id='" + PrintTabId + @"' and rt.Id='" + IssueId + @"' ";

                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetTransformationIssueReturnChildDataById(string PrintTabId, string IssueId)
        {
            try
            {
                //string _sql = @"select mp.Id, SUM(mp.Quantity) as PlanQuantity,jwi.UserName as JWOutputItem,jwa.UserName as JobWorkActivity, mma.StandardName as Article
                //               ,TotalReceivedQty=ISNULL( kk.TotalReceivedQuantity,'0')
                //               ,ToReceive= Sum(mp.Quantity)- ISNULL( kk.TotalReceivedQuantity,'0'), rtc.ReceivedQuantity
                //               from dbo.JobWorkTransformationContractChild mp left join dbo.JobWorkTransformationContract tc on tc.Id=mp.JobWorkTransformationContractMasterId
                //               left join hkp.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
                //               left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
                //               left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleCodeId
                //               left join (select Sum(ReceivedQuantity) as TotalReceivedQuantity,MaterialPlanningId from dbo.JobWorkReceiptTransformationChild group by MaterialPlanningId)
                //               kk on kk.MaterialPlanningId=mp.Id
                //               left join dbo.JobWorkReceiptTransformationChild rtc on rtc.MaterialPlanningId=mp.Id
                //               where tc.Id='" + PrintTabId + @"' and rtc.JobWorkReceiptTransformationMasterId='" + IssueId + @"'
                //               group by mp.Id,jwi.UserName, mma.StandardName,jwa.UserName,kk.TotalReceivedQuantity,rtc.ReceivedQuantity ";

                string _sql = @"select mp.Id, SUM(mp.Quantity) as PlanQuantity,jwi.UserName as JWOutputItem,jwa.UserName as JobWorkActivity,mm.Code as MaterialCode,mm.UserName as Material
                                ,mma.Code as ArticleCode, mma.StandardName as Article
                               ,TotalReceivedQty=ISNULL( kk.TotalReceivedQuantity,'0')
                               ,ToReceive= Sum(mp.Quantity)- ISNULL( kk.TotalReceivedQuantity,'0'), rtc.TransactionQty
                               from dbo.JobWorkTransformationContractChild mp left join dbo.JWTransformationPurchaseOrder tc on tc.Id=mp.JobWorkTransformationContractMasterId
                               left join hkp.JobWorkActivity jwa on jwa.Id=mp.JobActivityId
                               left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
                               left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleId
							   left join MST.MaterialMaster mm on mm.Id=mp.MaterialMasterId
                               left join (select Sum(TransactionQty) as TotalReceivedQuantity,JWTCMDId from TRN.InventoryReceiveDetail where JWTCMDId is not null group by JWTCMDId)
                               kk on kk.JWTCMDId=mp.Id
                               left join TRN.InventoryReceiveDetail rtc on rtc.JWTCMDId=mp.Id
                               where tc.Id='" + PrintTabId + @"' and rtc.InventoryReceiveId='" + IssueId + @"'
                               group by mp.Id,jwi.UserName, mma.StandardName,jwa.UserName,kk.TotalReceivedQuantity,rtc.TransactionQty,mm.Code,mm.UserName,mma.Code ";

                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetTransformationByProductDataById(string PrintTabId, string IssueId)
        {
            try
            {
                //       string _sql = @"select tbp.Id,jwit.UserName as JWOutputItem,jwi.UserName as ByProductItem,mma.StandardName as ByProductArticle, mm.UserName as ByProductMaterial           
                //,TQty=(mi.NetConsumption * mp.Quantity)
                //                     ,TotalReqQty=((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100)
                //                     ,ISNULL(rvbp.TotalReceivedQuantity,'0') as TotalReceivedQty
                //                     , ToReceive=((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) - ISNULL(rvbp.TotalReceivedQuantity,'0')
                //,rtbp.ReceivedQuantity
                //                     from dbo.JobWorkTransformationContractChild4 tbp 
                //                     left join HKP.JobWorkItem jwi on jwi.Id=tbp.JobWorkItemId
                //                     left join dbo.JobWorkTransformationContractChild3 mi on mi.Id=tbp.JobWorkTransformationContractChild3MasterId
                //                     left join dbo.JobWorkTransformationContractChild mp on mp.Id=mi.JobWorkTransformationContractChildMasterId
                //                     left join HKP.JobWorkItem jwit on jwit.Id=mp.JobWorkItemMasterId
                //                     left join MST.MaterialMasterArticle mma on mma.Id=tbp.ArticleId
                //left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
                //                     left join (Select SUM(ReceivedQuantity) as TotalReceivedQuantity,ByProductId from dbo.JobWorkReceiptTransformationByProduct group by ByProductId)
                //                     rvbp on rvbp.ByProductId=tbp.Id
                //                     left join dbo.JobWorkTransformationContract tc on tc.Id=mp.JobWorkTransformationContractMasterId
                //left join dbo.JobWorkReceiptTransformationByProduct rtbp on rtbp.ByProductId=tbp.Id
                //                     where tc.Id='" + PrintTabId + @"' 
                //and rtbp.JobWorkReceiptTransformationMasterId='" + IssueId + @"' ";

                string _sql = @"select tbp.Id,jwit.UserName as JWOutputItem,jwi.UserName as ByProductItem,mma.StandardName as ByProductArticle, mm.UserName as ByProductMaterial           
							  ,TQty=(mi.NetConsumption * mp.Quantity)
                              ,TotalReqQty=((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100)
                              ,ISNULL(rvbp.TotalReceivedQuantity,'0') as TotalReceivedQty
                              , ToReceive=((tbp.PercentageOfInput * (mi.NetConsumption * mp.Quantity))/100) - ISNULL(rvbp.TotalReceivedQuantity,'0')
							  ,rtbp.TransactionQty
                              from dbo.JobWorkTransformationContractChild4 tbp 
                              left join HKP.JobWorkItem jwi on jwi.Id=tbp.JobWorkItemId
                              left join dbo.JobWorkTransformationContractChild3 mi on mi.Id=tbp.JobWorkTransformationContractChild3MasterId
                              left join dbo.JobWorkTransformationContractChild mp on mp.Id=mi.JobWorkTransformationContractChildMasterId
                              left join HKP.JobWorkItem jwit on jwit.Id=mp.JobWorkItemMasterId
                              left join MST.MaterialMasterArticle mma on mma.Id=tbp.ArticleId
							  left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
                              left join (Select SUM(TransactionQty) as TotalReceivedQuantity,JWTCMDByProductId from TRN.InventoryReceiveDetail where JWTCMDByProductId is not null group by JWTCMDByProductId)
                              rvbp on rvbp.JWTCMDByProductId=tbp.Id
                              left join dbo.JWTransformationPurchaseOrder tc on tc.Id=mp.JobWorkTransformationContractMasterId
							  left join TRN.InventoryReceiveDetail rtbp on rtbp.JWTCMDByProductId=tbp.Id
                              where tc.Id='" + PrintTabId + @"' and rtbp.InventoryReceiveId='" + IssueId + @"' ";

                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetTransformationWIPData(string PrintTabId, string IssueId)
        {
            try
            {
                //         string _sql = @"select distinct mi.Id,mi.JobWorkTransformationContractChildMasterId, jwi.UserName as JWOutputItem,jwii.UserName as JWInputItem ,mm.Id as JWInputMaterialMasterId
                //                     , mm.UserName as JWInputMaterial ,mma.Id as JWInputMaterialArticleId, mma.StandardName as JWInputArticle
                //                     ,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
                //                     ,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalQuantity,'0'))
                //                     ,kk.TotalQuantity as TIRCTotalQty, ISNULL(R.TotalReceivedQty,'0')  as TotalReceiptQuantity, ISNULL(rtc.ReceivedQuantity,'0') as ReceiptQuantity
                //,QuantityUsed=ISNULL(rtc.ReceivedQuantity * mi.GrossConsumption,'0'), TotalQuantityUsed=ISNULL(R.TotalReceivedQty * mi.GrossConsumption,'0')
                //,WIPQuantity= isnull((kk.TotalQuantity - (R.TotalReceivedQty * mi.GrossConsumption)),'0')
                //                      from dbo.JobWorkTransformationIssueReturnChild tirc left join dbo.JobWorkTransformationContractChild3 mi on tirc.MaterialInputId=mi.Id
                // left join HKP.JobWorkItem jwii on jwii.Id=mi.JobWorkItemId
                // left join MST.MaterialMasterArticle mma on mma.Id=tirc.MaterialMasterArticleId
                // left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId and mm.Id=tirc.MaterialMasterId
                //                      left join dbo.JobWorkTransformationContractChild mp on mp.Id=mi.JobWorkTransformationContractChildMasterId
                // left  join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
                //                      left join(select SUM(Quantity) as TotalQuantity,MaterialInputId FROM dbo.JobWorkTransformationIssueReturnChild group by MaterialInputId) kk on kk.MaterialInputId=mi.id
                //                      left join TRN.InventoryMaterial inm on inm.MaterialMasterId=mm.Id and inm.ArticleId=mma.Id
                // left join (Select SUM(ReceivedQuantity) as TotalReceivedQty, MaterialPlanningId from dbo.JobWorkReceiptTransformationChild group by MaterialPlanningId)
                // R on  R.MaterialPlanningId=mp.Id
                // left join dbo.JobWorkReceiptTransformationChild rtc on rtc.MaterialPlanningId=mp.Id
                //  	where rtc.JobWorkReceiptTransformationMasterId='" + IssueId + @"'
                // group by mi.Id, mm.Id, mm.UserName,mma.Id, mma.StandardName,mp.Quantity,mi.GrossConsumption,kk.TotalQuantity,mi.JobWorkTransformationContractChildMasterId,jwi.UserName,jwii.UserName,R.TotalReceivedQty,rtc.ReceivedQuantity ";


                string _sql = @"select distinct mi.Id,mi.JobWorkTransformationContractChildMasterId, jwi.UserName as JWOutputItem,jwii.UserName as JWInputItem ,mm.Id as JWInputMaterialMasterId
                            , mm.UserName as JWInputMaterial ,mma.Id as JWInputMaterialArticleId, mma.StandardName as JWInputArticle
                            ,RequiredQuantity=(mp.Quantity * mi.GrossConsumption)
                            ,BalanceToIssue=(mp.Quantity * mi.GrossConsumption)-(ISNULL(kk.TotalIssuedQty,'0'))
                            ,kk.TotalIssuedQty as TIRCTotalQty, ISNULL(R.TotalReceivedQuantity,'0')  as TotalReceiptQuantity, ISNULL(rtc.TransactionQty,'0') as ReceiptQuantity
							,QuantityUsed=ISNULL(rtc.TransactionQty * mi.GrossConsumption,'0'), TotalQuantityUsed=ISNULL(R.TotalReceivedQuantity * mi.GrossConsumption,'0')
							,WIPQuantity= isnull((kk.TotalIssuedQty - (R.TotalReceivedQuantity * mi.GrossConsumption)),'0')
							 from TRN.InventoryIssueDetail tirc left join dbo.JobWorkTransformationContractChild mp on mp.Id=tirc.JWTCMID
							 left join dbo.JobWorkTransformationContractChild3 mi on mp.Id=mi.JobWorkTransformationContractChildMasterId
							 left join HKP.JobWorkItem jwii on jwii.Id=mi.JobWorkItemId
							 left join TRN.InventoryMaterial IM on IM.Id=tirc.InventoryMaterialId
							 left join MST.MaterialMasterArticle mma on mma.Id=IM.ArticleId
							 left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId and mm.Id=IM.MaterialMasterId
							 left  join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
                             left join(
							            select Sum(IID.TransactionQty) as TotalIssuedQty, IM.MaterialMasterId,mm.UserName as Material,mma.StandardName as Article, IM.ArticleId,IID.InventoryMaterialId
							            from TRN.InventoryIssue II inner join TRN.InventoryIssueDetail IID on II.Id=IID.InventoryIssueId
                                        left join TRN.InventoryMaterial IM on IM.Id=IID.InventoryMaterialId
                                        left join MST.MaterialMaster mm on mm.Id=IM.MaterialMasterId
                                        left join MST.MaterialMasterArticle mma on mma.Id=IM.ArticleId
										where II.JWContractId='" + PrintTabId + @"'
										group by IM.MaterialMasterId,IM.ArticleId,IID.InventoryMaterialId,mm.UserName,mma.StandardName) 
							 kk on kk.InventoryMaterialId=IM.Id
							 left join (select Sum(TransactionQty) as TotalReceivedQuantity,JWTCMDId from TRN.InventoryReceiveDetail where JWTCMDId is not null group by JWTCMDId)
							 R on  R.JWTCMDId=mp.Id
							 left join TRN.InventoryReceiveDetail rtc on rtc.JWTCMId=mp.Id
						   	where rtc.InventoryReceiveId='" + IssueId + @"'
							 group by mi.Id, mm.Id, mm.UserName,mma.Id, mma.StandardName,mp.Quantity,mi.GrossConsumption,kk.TotalIssuedQty
							 ,mi.JobWorkTransformationContractChildMasterId,jwi.UserName,jwii.UserName,R.TotalReceivedQuantity,rtc.TransactionQty ";

                return _sqlRepository.GetDataTable(_sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
public class jobworkreceiptvalueaddedchild
{

    #region Scalar Properties
    public string Id { get; set; }
    public string ContractLineItemId { get; set; }
    public string OrderChildId { get; set; }
    public string ReceivedQty { get; set; }
    public string Remarks { get; set; }

    public string JWOutputItem { get; set; }
    public string Article { get; set; }
    public string TotalIssuedQty { get; set; }
    public string TotalReceivedQty { get; set; }
    public string ToReceive { get; set; }


    #endregion Scalar Properties
}
public class ReceiptValueAddedGradeWise
{

    #region Scalar Properties
    public string Id { get; set; }
    public string GradeName { get; set; }
    public string GradeWQty { get; set; }
    public string GWRemarks { get; set; }

    #endregion Scalar Properties
}
public class JobWorkReceiptTransformationChild
{

    #region Scalar Properties
    public string Id { get; set; }
    public string ReceivedQty { get; set; }
    public string Remarks { get; set; }

    #endregion Scalar Properties
}
public class ReceiptTransformationGradeWise
{

    #region Scalar Properties
    public string Id { get; set; }
    public string GradeName { get; set; }
    public string GradeWQty { get; set; }
    public string GWRemarks { get; set; }

    #endregion Scalar Properties
}
public class JobWorkReceiptTransformationByProduct
{

    #region Scalar Properties
    public string Id { get; set; }
    public string ReceiveQuantity { get; set; }
    public string Remarks { get; set; }

    #endregion Scalar Properties
}