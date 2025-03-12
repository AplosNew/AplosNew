#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Library.OrderManagement.Production;
using Library.Crosscutting.Security;
using System.Data;
using Library.Security.Core;
using System.Threading;
using Library.MaterialManagement.Material;
using System.Web;
using Newtonsoft.Json;
using Library.Service.Helpers;
using System.IO;
using Library.Core;

#endregion Using

namespace Aplos.Areas.Productions.Controllers
{
    public class MarkerController : BaseController
    {
        #region Constructor
        string TableName = "dbo.MarkerMaster";
        string DetailTableName = "dbo.MarkerDetails";
        private readonly ISqlRepository _sqlRepository;
        public MarkerController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor

        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult Check()
        {
            return View();
        }

        public ActionResult Approve()
        {
            return View();
        }


        [Authorize, HttpGet]
        public JsonResult GetCheckByCbo()
        {
            var sql = @"select distinct E.SystemId As Value,(E.EmployeeCode+'-'+ E.EmployeeName) Text,A.ActionStatus  
                          from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where E.EmployeeStatus='Active' AND A.ActionStatus= 'MarkerCheckedBy'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetApprovedByCbo()
        {
            var sql = @"select distinct E.SystemId As Value,(E.EmployeeCode+'-'+ E.EmployeeName) Text,A.ActionStatus  
                          from dbo.AuthorizationConfig A 
                          Inner JOin dbo.EmployeeInformation E On E.systemId=A.EmployeeId 
                          where E.EmployeeStatus='Active' AND A.ActionStatus='MarkerApproveBy'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM " + TableName + ""), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetFabricWidth()
        {
            return Json(_sqlRepository.GetDataCollection("select Id,UserName FabricWidthName From FabricWidth"), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetShrinkageGroup()
        {
            return Json(_sqlRepository.GetDataCollection("select Id,UserName ShrinkageGroupName From ShrinkageGroup"), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from dbo.MarkerMaster where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from dbo.MarkerMaster where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from dbo.MarkerMaster where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = _Id;
                    data["CheckByStatus"] = "To Be Check";
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

                return Json(new { Error = false, Data = data, Sequence = GetSequence(), Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public JsonResult Checked(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                
                con.OpenDataSetThroughAdapter("select * from dbo.MarkerMaster where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                 
                    _Id = data["Id"].ToString();
                    data["CheckByDate"] = DateTime.Now;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data = data, Sequence = GetSequence(), Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public JsonResult Approved(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from dbo.MarkerMaster where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count > 0)
                {

                    _Id = data["Id"].ToString();
                    data["ApproveByDate"] = DateTime.Now;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data = data, Sequence = GetSequence(), Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

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


        [Authorize, HttpGet]
        public JsonResult GetShade()
        {
            return Json(_sqlRepository.GetDataCollection("select Id,UserName ShadeName From Shade"), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetWidthUnit()
        {
            return Json(_sqlRepository.GetDataCollection("select Id,UserName from SCS.UnitOfMeasurement Where IsWidthUnit=1"), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetLengthUnit()
        {
            return Json(_sqlRepository.GetDataCollection("select Id,UserName from SCS.UnitOfMeasurement Where IsLengthUnit=1"), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from " + TableName + " where Id = '" + Id + "' ");
                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize, HttpPost]
        public ActionResult GetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select m.*,mm.UserName FGMaterialMaster, mma.StandardName FGArticle ,c.UserName HeaderName
                                From MarkerMaster m
                                left join MST.MaterialMaster mm on mm.Id= m.FGMaterialMasterId
                                left join MST.MaterialMasterArticle mma on mma.Id= m.FGArticleId
                                left join HKP.Characteristics c on c.Id= m.CharacteristicsId
                                Where CheckByStatus IN('To Be Check','Pending','Reject')
                                order by m.Sequence ";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetSOList(string MasterPlanId)
        {
            string sql = @"select distinct SO.Id SONo,CAST(0 as bit) Flag,FORMAT(SO.DeliveryDate,'dd-MMM-yyyy')DeliveryDate,MOI.OwnReferenceNo,MOI.BuyerReferenceNo,SO.Qty
,isnull((select SOPlanQty from [MST].[MasterPlanSODetails] where SalesOrderId=SO.Id),SO.Qty + (MO.ExtraOrderPercentage*SO.Qty / 100)) as SOPlanQty,SO.Reason Remarks

from TRN.ProductionOrder PO
left join TRN.ProductionOrderProcessSet PPS ON PPS.ProductionOrderId=PO.Id
left join TRN.ProductionOrderDetail POD ON POD.ProductionOrderId=PO.Id
left join TRN.SalesOrder SO ON SO.Id=POD.SalesOrderId
left join [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
left join [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
LEFT JOIN [MST].[MasterPlanSODetails] CPD on CPD.SalesOrderId=SO.Id and CPD.MasterPlanId='" + MasterPlanId + @"'
where CPD.MasterPlanId = '" + MasterPlanId + @"'
and PO.ProductionStatusId in (select Id from HKP.ProductionStatus where MasterPlanApplicable=1)
and SO.OrderStatusId in (select Id from HKP.OrderStatus OS where OS. MasterPlanApplicable=1)
and SO.Id in (select SalesOrderId from MST.MasterPlanSODetails where MasterPlanId='" + MasterPlanId + @"' and Status=1) ";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public ActionResult GetFabricGRNRowList(string soId)
        {
            string sql = @"SELECT CAST(0 as bit) FlagG, MP.InventoryReceiveDetailId,MP.FirstCharacteristicsValueId,IRD.InventoryReceiveId,IRD.TransactionQty,UOM.UserName UOM,BUoM.UserName BaseUoM,IR.Id GRNNo,IR.GRNDate
,P.UserName PartyName,MM.UserName MaterialMasterName,MMA.StandardName ArticleName,CV.UserName SKUValue
FROM dbo.GRNSOMap MP
LEFT JOIN  [TRN].[InventoryReceiveDetail] IRD ON MP.InventoryReceiveDetailId=IRD.Id
LEFT JOIN TRN.InventoryReceive IR ON IRD.InventoryReceiveId=IR.Id
LEFT JOIN HKP.Party P ON IR.PartyId=P.Id
LEFT JOIN TRN.InventoryMaterial IM ON IRD.InventoryMaterialId=IM.Id
LEFT JOIN [SCS].[Currency] AS CU ON IR.CurrencyId=CU.Id
LEFT JOIN SCS.UnitOfMeasurement UOM ON IRD.TransactionUoMId=UOM.Id
LEFT JOIN SCS.UnitOfMeasurement BUoM ON IRD.BaseUOMId=BUoM.Id
LEFT JOIN MST.MaterialMaster MM ON IM.MaterialMasterId=MM.Id
LEFT JOIN MST.MaterialMasterArticle MMA ON IM.ArticleId=MMA.Id
LEFT JOIN [HKP].[CharacteristicsValue] CV ON MP.FirstCharacteristicsValueId=CV.Id
Where MP.SalesOrderId  " + soId+"";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetCheckByList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select m.*,mm.UserName FGMaterialMaster, mma.StandardName FGArticle ,c.UserName HeaderName
                                From MarkerMaster m
                                left join MST.MaterialMaster mm on mm.Id= m.FGMaterialMasterId
                                left join MST.MaterialMasterArticle mma on mma.Id= m.FGArticleId
                                left join HKP.Characteristics c on c.Id= m.CharacteristicsId
                                Where m.CheckByStatus='To Be Check' AND CheckById='"+identity.EmployeeId+@"'
                                order by m.Sequence ";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetApproveByList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select m.*,mm.UserName FGMaterialMaster, mma.StandardName FGArticle ,c.UserName HeaderName
                                From MarkerMaster m
                                left join MST.MaterialMaster mm on mm.Id= m.FGMaterialMasterId
                                left join MST.MaterialMasterArticle mma on mma.Id= m.FGArticleId
                                left join HKP.Characteristics c on c.Id= m.CharacteristicsId
                                Where CheckByStatus='Checked' AND ApproveById='" + identity.EmployeeId + @"'
                                order by m.Sequence ";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetDetailsList(string masterid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT CV.Id AS CharacteristicsValueId,CV.Code, CV.UserName AS [Text] 
                                ,Ratio = case when M.Id is null then '' else M.Ratio end,M.Id
                                FROM MarkerDetails M
                            LEFT JOIN hkp.CharacteristicsValue CV ON CV.Id=M.CharacteristicsValueId
                            Where M.MarkerMasterId='" + masterid + "'  Order by CV.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult getCharacteristicsValueByCharacteristicsIdAfterSave(string materialMasterId, string characteristicsId, string valueAssignmentLevel, string MarkerMasterId)
        {
            try
            {
                clsMaterial ep = new clsMaterial();
                return Json(ep.GetCharacteristicsValueCboByCharacteristicsIdAfterSave(materialMasterId, characteristicsId, valueAssignmentLevel, MarkerMasterId), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpPost, Authorize]
        public ActionResult getCharacteristicsValueByCharacteristicsId(string materialMasterId, string characteristicsId, string valueAssignmentLevel)
        {
            try
            {
                clsMaterial ep = new clsMaterial();
                return Json(ep.GetCharacteristicsValueCboByCharacteristicsId(materialMasterId, characteristicsId, valueAssignmentLevel), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpPost]
        public JsonResult XCreate(/*Dictionary<string, object> data, List<Dictionary<string, object>> details, */FormCollection form, HttpPostedFileBase[] file)
        {
            try
            {

                var settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    MissingMemberHandling = MissingMemberHandling.Ignore
                };
                var data = JsonConvert.DeserializeObject<MarkerMaster>(form["data"], settings);
                IEnumerable<MarkerDetails> details = JsonConvert.DeserializeObject<IEnumerable<MarkerDetails>>(form["details"], settings);


                var directory = ResourcesPathReader.GetMarkerDocPath();
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
                string path = Path.Combine(directory);

                DataSet dsMaster;
                DataSet dsChild;
                DataRow dr = null;
                int count = 0;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Code='" + data.Code + "' AND  Id<>'" + data.Id + "' ", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName='" + data.UserName + "' AND  Id<>'" + data.Id + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same user name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data.Id + "'", out dsMaster, false, "1");

                string _Id = "";
                string MasterId = string.Empty;

                #region Master data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {

                    AddNewRow(dsMaster.Tables[0], data);
                    MasterId = data.Id;
                }
                else
                {
                    MasterId = data.Id;
                    //EditRow(dsMaster.Tables[0].DefaultView[0].Row, data);
                    dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    dr["Sequence"] = data.Sequence;
                    dr["Code"] = data.Code;
                    dr["ShortName"] = data.ShortName;
                    dr["StandardName"] = data.StandardName;
                    dr["UserName"] = data.UserName;
                    dr["Description"] = data.Description;
                    dr["Remarks"] = data.Remarks;
                    dr["Active"] = data.Active;
                    dr["FGMaterialMasterId"] = data.FGMaterialMasterId;
                    dr["FabricWidthId"] = data.FabricWidthId;
                    dr["FGArticleId"] = data.FGArticleId;
                    dr["ShrinkageGroupId"] = data.ShrinkageGroupId;
                    dr["CharacteristicsId"] = data.CharacteristicsId;
                    dr["ShadeId"] = data.ShadeId;
                    dr["Length"] = data.Length;
                    dr["Attachment"] = data.Attachment;
                    dr.EndEdit();
                }
                #endregion data update

                #region Child Data Update

                string DetailsId = string.Empty;
                con.OpenDataSetThroughAdapter("select * from " + DetailTableName + " where MarkerMasterId= '" + MasterId + "' ", out dsChild, false, "1");
                string sID = string.Empty;
                bplib.clsGenID objGenID = new bplib.clsGenID();
                objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[dbo].[MarkerDetails]", out sID);

                foreach (var item in details)
                {
                    dsChild.Tables[0].DefaultView.RowFilter = "Id = '" + item.Id + "' ";

                    if (dsChild.Tables[0].DefaultView.Count == 0)
                    {
                        if (item.Id == null && item.Ratio != 0)
                        {
                            count++;
                            DetailsId = "MD" + sID + "_" + count;
                            dr = dsChild.Tables[0].NewRow();
                            dr["Id"] = DetailsId;
                            dr["MarkerMasterId"] = MasterId;
                            dr["CharacteristicsValueId"] = item.CharacteristicsValueId;
                            dr["Ratio"] = item.Ratio;
                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            dsChild.Tables[0].Rows.Add(dr);
                        }
                    }
                    else
                    {
                        //if (item.Id != null && item.Ratio != 0)
                        //{
                            dr = dsChild.Tables[0].DefaultView[0].Row;
                            dr.BeginEdit();
                            dr["Ratio"] = item.Ratio;

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dr.EndEdit();
                        //}
                    }
                }
                for (int i = 0; i < dsChild.Tables[0].Rows.Count; i++)
                {
                    if (clsStaticInfo.dbl(dsChild.Tables[0].Rows[i]["Ratio"].ToString()) == 0)
                    {
                        dsChild.Tables[0].Rows[i].Delete();
                    }
                }

                var fileName = "";
                var filedata = GetFile(MasterId);
                if (file.IsNotNull())
                {
                    for (int i = 0; i < file.Length; i++)
                    {
                        ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
                    }
                }
                if (filedata.Count > 0)
                {
                    if (
                        !string.IsNullOrEmpty(filedata["Attachment"].ToString()))
                        fileName = filedata["Attachment"].ToString();

                    if (fileName != data.Attachment)
                        if (System.IO.File.Exists(path + MasterId + Path.GetExtension(fileName)))
                            System.IO.File.Delete(path + MasterId + Path.GetExtension(fileName));
                }
                if (filedata.Count > 0)
                {
                    if (
                        !string.IsNullOrEmpty(filedata["Attachment"].ToString()))
                        fileName = filedata["Attachment"].ToString();

                    if (fileName != data.Attachment)
                        if (System.IO.File.Exists(path + MasterId + Path.GetExtension(fileName)))
                            System.IO.File.Delete(path + MasterId + Path.GetExtension(fileName));
                }

                if (file.IsNotNull())
                {
                    foreach (var item in file)
                    {
                        if (item != null)
                        {
                            if (System.IO.File.Exists(path + item.FileName))
                                System.IO.File.Delete(path + MasterId + Path.GetExtension(item.FileName));
                            item.SaveAs(path + MasterId + Path.GetExtension(item.FileName));
                        }
                    }
                }

                #endregion

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsChild);

                return Json(new { Error = false, Data = data, Sequence = GetSequence(), Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }
        public Dictionary<string, object> GetFile(string Id)
        {
            try
            {
                var sql = @"SELECT Attachment FROM [dbo].[MarkerMaster]  WHERE Id='" + Id + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ActionResult Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                var directory = ResourcesPathReader.GetMarkerDocPath();
                var filedata = GetFile(id);

                string path = Path.Combine(directory);
                System.IO.File.Delete(path + id + Path.GetExtension(filedata["Attachment"].ToString()));

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + DetailTableName + " where MarkerMasterId='" + id + "'");
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();



                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        private void AddNewRow(DataTable dt, MarkerMaster data)
        {
            string _Id = "";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            bplib.clsGenID genid = new bplib.clsGenID();
            genid.GenID(TableName, out _Id);

            data.Id = "M" + _Id;
            dr["Id"] = data.Id;
            dr["Sequence"] = data.Sequence;
            dr["Code"] = data.Code;
            dr["ShortName"] = data.ShortName;
            dr["StandardName"] = data.StandardName;
            dr["UserName"] = data.UserName;
            dr["Description"] = data.Description;
            dr["Remarks"] = data.Remarks;
            dr["Active"] = data.Active;
            dr["FGMaterialMasterId"] = data.FGMaterialMasterId;
            dr["FabricWidthId"] = data.FabricWidthId;
            dr["FGArticleId"] = data.FGArticleId;
            dr["ShrinkageGroupId"] = data.ShrinkageGroupId;
            dr["CharacteristicsId"] = data.CharacteristicsId;
            dr["ShadeId"] = data.ShadeId;
            dr["Length"] = data.Length;
            dr["Attachment"] = data.Attachment;

            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, MarkerMaster data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();
            dr["Sequence"] = data.Sequence;
            dr["Code"] = data.Code;
            dr["Code"] = data.ShortName;
            dr["StandardName"] = data.StandardName;
            dr["UserName"] = data.UserName;
            dr["Description"] = data.Description;
            dr["Remarks"] = data.Remarks;
            dr["Active"] = data.Active;
            dr["FGMaterialMasterId"] = data.FGMaterialMasterId;
            dr["FGArticleId"] = data.FGArticleId;
            dr["ShrinkageGroupId"] = data.ShrinkageGroupId;
            dr["CharacteristicsId"] = data.CharacteristicsId;
            dr["ShadeId"] = data.ShadeId;
            dr["Length"] = data.Length;
            dr["Attachment"] = data.Attachment;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM " + TableName + "");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;
            return 1;
        }


    }

    public class MarkerMaster
    {
        public string Id { get; set; }
        public decimal Sequence { get; set; }
        public string Code { get; set; }
        public string ShortName { get; set; }
        public string StandardName { get; set; }
        public string UserName { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public bool Active { get; set; }
        public string FGMaterialMasterId { get; set; }
        public string FabricWidthId { get; set; }
        public string FGArticleId { get; set; }
        public string ShrinkageGroupId { get; set; }
        public string CharacteristicsId { get; set; }
        public string ShadeId { get; set; }
        public decimal Length { get; set; }
        public string Attachment { get; set; }
    }
    public class MarkerDetails
    {
        public string Id { get; set; }
        public string MarkerMasterId { get; set; }
        public string CharacteristicsValueId { get; set; }
        public decimal Ratio { get; set; }
    }
}
