using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Library.Core;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using OTSBD;
using System.Web.Script.Serialization;
using System.Net.Http;

using Library.Model.IE;
using Library.Service.IEnumerable;
using Library.Model.Enums;
using Syncfusion.XlsIO;
using Newtonsoft.Json;

namespace Aplos.Areas.JobWork.Controllers
{
    public class JWValueAddedContractController : BaseController
    {
        string TableName = "dbo.JobWorkValueAddedContract";
        string TableName2 = "dbo.JobWorkValueAddedContractChild2";
        string TableName3 = "dbo.JobWorkTransformationContract";
        #region Constructor
        private readonly SqlRepository _sqlRepository;
        public JWValueAddedContractController(SqlRepository Repository)
        {
            _sqlRepository = Repository;
        }
        #endregion
        #region Pages
        // GET: JobWork/JobWorkValueAddedContract
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion
        #region Dropdown Code Area

        [HttpGet, Authorize]
        public JsonResult getentity()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = "";
            sql = @"select Id as Value, UserName as Text from ORG.Entity where PlantId='"+identity.PlantId+"' order by UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult getmateriallocation(string EntityId, string JWActivityId)
        {
            string sql = "";
            sql = @"select jl.Id as Value, jl.LocationName as Text, StoreLocationId,ms.UserName as MaterialStorage
                    from HKP.JobWorkLocation jl left join HKP.MaterialStorage ms on ms.Id=jl.StoreLocationId
					left join HKP.JobWorkLocationChild JLC on JLC.JobWorkLocationId=jl.Id
					where jl.EntityId='"+ EntityId + @"' and JLC.JobWorkActivityId='"+ JWActivityId + @"' order by jl.LocationName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getjobworkitemlist(string JWActivityId)
        {
            return Json(_sqlRepository.GetDataCollection("select jwi.Id as Value, jwi.UserName as Text from MST.JobWorkValueAddedMaster vm left join HKP.JobWorkItem jwi on jwi.Id=vm.JobWorkActivityChildId where vm.JobWorkActivityId='"+ JWActivityId + @"' order by jwi.UserName "), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getjobworkactivitylist()
        {
            return Json(_sqlRepository.GetDataCollection("select Id as Value, UserName as Text from HKP.JobWorkActivity where Type='Value Added' order by UserName"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetVMValues(string JWActivityId, string JobWorkItemId)
        {
            string sql = "";
            sql = @"select vm.StdRejection, vm.StdValueLoss, vm.RateApplicable,vm.CurrencyId, c.Code as Currency, vm.MinRate, vm.MaxRate, jwi.MaterialMasterId, mm.Code as MaterialCode
                   ,mm.UserName as Material,UnitId=case when jwi.MaterialMasterId is not null then mmuom.Id else jwiuom.Id End
                   , Unit=case when jwi.MaterialMasterId is not null then mmuom.UserName else jwiuom.UserName End
                    from MST.JobWorkValueAddedMaster vm left join SCS.Currency C on C.Id=vm.CurrencyId 
                    left join HKP.JobWorkItem jwi on jwi.Id=vm.JobWorkActivityChildId
                    left join MST.MaterialMaster mm on mm.Id=jwi.MaterialMasterId
                    left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
                    left join SCS.UnitOfMeasurement jwiuom on jwiuom.Id=jwi.UOMId
                    where vm.JobWorkActivityId='" + JWActivityId + @"' and vm.JobWorkActivityChildId='"+ JobWorkItemId + @"' ";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getactivitylistTransformation(string ContractType)
        {
            string sql = "";
            if(ContractType== "JWTransformationPO")
            {
                sql = @"select distinct jwa.Id as Value, jwa.UserName as Text from MST.JobWorkTransformationMaster tm 
                        left join HKP.JobWorkActivity jwa on jwa.Id=tm.JobWorkActivityId order by jwa.UserName ";
            }

            if (ContractType == "JWValueAddedPO")
            {
                sql = @"select distinct jwa.Id as Value, jwa.UserName as Text from MST.JobWorkValueAddedMaster tm 
                        left join HKP.JobWorkActivity jwa on jwa.Id=tm.JobWorkActivityId order by jwa.UserName ";
            }


            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getTransformationjobworkitemlist(string ActivityId, string ContractType)
        {
            string sql = "";
            if (ContractType == "JWTransformationPO")
            {
                sql = @"select jwi.Id as Value, jwi.UserName as Text from MST.JobWorkTransformationMaster tm 
                        left join HKP.JobWorkItem jwi on jwi.Id=tm.JobWorkActivityChildId where tm.JobWorkActivityId='" + ActivityId + @"' order by jwi.UserName ";
            }

            if (ContractType == "JWValueAddedPO")
            {
                sql = @"select jwi.Id as Value, jwi.UserName as Text from MST.JobWorkValueAddedMaster vm 
                        left join HKP.JobWorkItem jwi on jwi.Id=vm.JobWorkActivityChildId where vm.JobWorkActivityId='" + ActivityId + @"' order by jwi.UserName";
            }


            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

          //  return Json(_sqlRepository.GetDataCollection("select jwi.Id as Value, jwi.UserName as Text from MST.JobWorkTransformationMaster tm left join HKP.JobWorkItem jwi on jwi.Id=tm.JobWorkActivityChildId where tm.JobWorkActivityId='"+ ActivityId + @"' order by jwi.UserName"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetJWitemDataFromTrans(string ActivityId, string JWItemId, string ContractType)
        {
            string sql = "";
            if(ContractType== "JWTransformationPO")
            {
                sql = @"select tm.Id, tm.RateApplicable,tm.ByProductApplicable,c.Id as CId,c.Code as Currency,uom.Id as UOMId ,uom.UserName as Unit 
                    from MST.JobWorkTransformationMaster tm left join scs.Currency c on c.Id=tm.CurrencyId
                    left join HKP.JobWorkItem jwi on jwi.Id=tm.JobWorkActivityChildId
                    left join scs.UnitOfMeasurement uom on uom.Id=jwi.UOMId
                    where tm.JobWorkActivityId='" + ActivityId + @"' and tm.JobWorkActivityChildId='" + JWItemId + @"' ";
            }

            return Json(_sqlRepository.GetDataCollection(sql,null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult getoutputunit()
        {
            string sql = "";
            sql = @"select Id as Value, UserName as Text from SCS.UnitOfMeasurement order by UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult getarticlecode(string JobWorkItemId)
        {
            string sql = "";
            sql = @"Select mma.Id as Value, mma.StandardName as Text from MST.MaterialMasterArticle mma left join MST.MaterialMaster mm on mma.MaterialMasterId=mm.Id inner join HKP.JobWorkItem jwi on jwi.MaterialMasterId=mm.Id where jwi.Id='" + JobWorkItemId + "' order by mma.StandardName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult getrateapplylist(string JobWorkItemId)
        {
            string sql = "";
            sql = @"Select Id as Value, RateApplicable as Text from MST.JobWorkValueAddedMaster where JobWorkActivityChildId='"+ JobWorkItemId + "' order by RateApplicable";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult getcurrency(string JWActivityId, string JobWorkItemId)
        {
            string sql = "";
            sql = @"Select distinct c.Id as Value, c.Code as Text from scs.Currency c left join MST.JobWorkValueAddedMaster vam on vam.CurrencyId=c.Id where vam.JobWorkActivityId='" + JWActivityId + @"' and vam.JobWorkActivityChildId='" + JobWorkItemId + "' order by c.Code";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        //[HttpGet, Authorize]
        //public JsonResult getcurrency()
        //{
        //    string sql = "";
        //    sql = @"Select distinct Id as Value, Code as Text from scs.Currency order by Code";

        //    return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        //}

        [HttpGet, Authorize]
        public JsonResult gettransformationrateapplylist(string JobWorkItemId, string ActivityId, string ContractType)
        {

            string sql = "";
            if (ContractType == "JWTransformationPO")
            {
                sql = @"Select M.RateApplicable as Value, M.RateApplicable as Text,M.MinRate, M.MaxRate,SM.UserName as Service,M.ServiceId 
                        from MST.JobWorkTransformationMaster M
                        left join HKP.ServiceMaster SM on SM.Id=M.ServiceId
                        where JobWorkActivityChildId='" + JobWorkItemId + "' and JobWorkActivityId='" + ActivityId + @"' order by RateApplicable ";
            }

            if (ContractType == "JWValueAddedPO")
            {
                sql = @"Select M.RateApplicable as Value, M.RateApplicable as Text,M.MinRate, M.MaxRate,SM.UserName as Service,M.ServiceId
                        from MST.JobWorkValueAddedMaster M
                        left join HKP.ServiceMaster SM on SM.Id=M.ServiceId
                        where JobWorkActivityChildId='" + JobWorkItemId + @"' and JobWorkActivityId='"+ ActivityId + @"' order by RateApplicable";
            }


            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

            //string sql = "";
            //sql = @"Select RateApplicable as Value, RateApplicable as Text,MinRate, MaxRate from MST.JobWorkTransformationMaster where JobWorkActivityChildId='" + JobWorkItemId + "' and JobWorkActivityId='"+ ActivityId + @"' order by RateApplicable";

            //return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult gettransformationcurrency(string JobWorkItemId, string ActivityId, string ContractType)
        {
            string sql = "";
            if (ContractType == "JWTransformationPO")
            {
                sql = @"Select distinct c.Id as Value, c.Code as Text from scs.Currency c left join MST.JobWorkTransformationMaster tm on tm.CurrencyId=c.Id 
                        where tm.JobWorkActivityId='" + ActivityId + @"' and tm.JobWorkActivityChildId='" + JobWorkItemId + "' order by c.Code ";
            }

            if (ContractType == "JWValueAddedPO")
            {
                sql = @"Select distinct c.Id as Value, c.Code as Text, vam.StdRejection, vam.StdValueLoss 
                        from scs.Currency c left join MST.JobWorkValueAddedMaster vam on vam.CurrencyId=c.Id 
                        where vam.JobWorkActivityId='" + ActivityId + @"' and vam.JobWorkActivityChildId='" + JobWorkItemId + "' order by c.Code";
            }


            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

            //string sql = "";
            //sql = @"Select distinct c.Id as Value, c.Code as Text from scs.Currency c left join MST.JobWorkTransformationMaster tm on tm.CurrencyId=c.Id where tm.JobWorkActivityId='"+ ActivityId + @"' and tm.JobWorkActivityChildId='" + JobWorkItemId + "' order by c.Code";

            //return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult getcustomerlist()
        {
            string sql = "";
            sql = @"select distinct p.Id as Value, p.UserName as Text from HKP.Party p inner join TRN.MasterOrder mo on mo.PartyId=p.Id order by p.UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult getmasterorderlist(string CustomerId)
        {
            string sql = "";
            sql = @"select Id as Value, MasterOrderNo as Text from TRN.MasterOrder where PartyId='" + CustomerId + "' order by MasterOrderNo";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult getmasterorderitemlist(string MasterOrderNoId)
        {
            string sql = "";
            sql = @"select moi.Id as Value, mm.UserName as Text from MST.MaterialMaster mm left join TRN.MasterOrderItem moi on moi.MaterialMasterId=mm.Id where moi.MasterOrderId='" + MasterOrderNoId + "' order by mm.UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult getplant()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = "";
            sql = @"select Id as Value, UserName as Text from ORG.Plant where CompanyGroupId='"+identity.CompanyGroupId+ "' and CompanyId='" + identity.CompanyId +"' order by UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAllEntity(string PlantId)
        {
            string sql = "";
            sql = @"select Id as Value, UserName as Text from ORG.Entity where PlantId='" + PlantId + "' order by UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult getmateriallist()
        {
            string sql = "";
            sql = @"select mm.Id as Value, mm.UserName as Text from MST.MaterialMaster mm inner join HKP.JobWorkItem jwi on jwi.MaterialMasterId=mm.Id order by mm.UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult getbyproductcurrency()
        {
            string sql = "";
            sql = @"select Id as Value, Code as Text from SCS.Currency order by Code";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMaterialfromJW(string JobWorkItemId)
        {
            string sql = "";
            sql = @"select mm.Id, mm.Code, mm.UserName as Material,mm.BaseUOMId, mmuom.UserName as BaseUom,jwi.UOMId, uom.UserName as JWIUom
                     ,UnitId=case when jwi.MaterialMasterId is not null then mm.BaseUOMId else jwi.UOMId End
                     from HKP.JobWorkItem jwi left join MST.MaterialMaster mm on mm.Id=jwi.MaterialMasterId
                     left join scs.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
					 left join SCS.UnitOfMeasurement uom on uom.Id=jwi.UOMId
                     where jwi.Id='" + JobWorkItemId + @"' ";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region Load Data

        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from dbo.JobWorkValueAddedContract where Id = '" + Id + "' ");


                return Json(new { master = _master }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }

        }

        [HttpPost, Authorize]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select vac.Id,TabType='Value Added',vac.ContractStatus,vac.PlantId, vac.EntityId,vac.VendorPartyId,vac.Remarks,vac.Date,FORMAT(vac.Date,'dd-MMM-yyyy') as ValueAddedDate,CONVERT(varchar(5),vac.[Time],108)[VACTime],FORMAT(vac.ProcessStartDate,'dd-MMM-yyyy') as VAProcessStartDate,
                                    FORMAT(vac.ProcessEndDate,'dd-MMM-yyyy') as VAProcessEndDate,FORMAT(vac.ContractClosingDate,'dd-MMM-yyyy') as VAContractClosingDate,
                                    PL.UserName as Plant, e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName
                                    from dbo.JobWorkValueAddedContract vac left join ORG.Entity e on e.Id=vac.EntityId
									left join HKP.Party p on p.Id=vac.VendorPartyId
									left join ORG.Plant PL on PL.Id=vac.PlantId
                                    union
                          select tc.Id,TabType='Transformation',tc.ContractStatus,tc.PlantId, tc.EntityId,tc.VendorPartyId,tc.Remarks,tc.Date,FORMAT(tc.Date,'dd-MMM-yyyy') as ValueAddedDate,CONVERT(varchar(5),tc.[Time],108)[VACTime],FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as VAProcessStartDate,
                                    FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as VAProcessEndDate,FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as VAContractClosingDate,
                                    PL.UserName as Plant, e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName
                                    from dbo.JobWorkTransformationContract tc left join ORG.Entity e on e.Id=tc.EntityId
									left join HKP.Party p on p.Id=tc.VendorPartyId
									left join ORG.Plant PL on PL.Id=tc.PlantId
                                    WHERE " + strkey + " order by Date desc";



            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

      

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobWorkValueAddedContract", out sID);
            return sID;
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                //con.OpenDataSetThroughAdapter("select * from " + TableName + " where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //    throw new Exception("Same Code already exists!!!");

                //con.OpenDataSetThroughAdapter("select * from " + TableName + " where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                //if (dsMaster.Tables[0].Rows.Count > 0)
                //    throw new Exception("Same User Name already exists!!!");


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = "VAC" + GetPK();
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

                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                DataSet dsMaster;

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                if (!string.IsNullOrEmpty(id))
                {
                    con.OpenDataSetThroughAdapter("select * from dbo.JobWorkValueAddedContractChild where JobWorkValueAddedContractMasterId='" + id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Material Planning Data");
                    }
                }

               
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

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

        #endregion

        // To get Vendor for value added contract
        [HttpPost, Authorize]
        public ActionResult LoadAllPartyDetailsForSelection(string Id)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select p.Id, p.Sequence, p.Code, p.ShortName, p.StandardName, p.UserName,pg.UserName as PartyGroup
                               from HKP.Party p left join HKP.PartyGroup pg on pg.Id=p.PartyGroupId
                               WHERE p.CompanyGroupId='" + identity.CompanyGroupId + @"' and p.PartyType='Party'
                               AND isnull(p.Id,'') not in (select isnull(VendorPartyId,'') from dbo.JobWorkValueAddedContract where Id='" + Id + @"')
                               order by p.Sequence";

                var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception)
            {
                throw;
            }    
        }

        // Child data

        [HttpPost, Authorize]
        public JsonResult saveUrlMaterialPlanning(FormCollection form, HttpPostedFileBase[] file)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            var model = JsonConvert.DeserializeObject<JobWorkValueAddedContractChild>(form["MaterialPlanning"], settings);



            SaveData(model, out string masterId);
            if (file.IsNotNull())
            {
                var directory = ResourcesPathReader.GetValueAddedContractMaterialChildFile();
                var path = Path.Combine(directory);



                for (int i = 0; i < file.Length; i++)
                {
                    ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
                }

                var fileId = "";
                var fileName = "";
                var filedata = GetFile(model.Id);
                if (filedata.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filedata["Id"].ToString()) &&
                        !string.IsNullOrEmpty(filedata["FileName"].ToString()))
                        fileId = filedata["Id"].ToString();
                    fileName = filedata["FileName"].ToString();

                    if (fileName != model.FileName)
                        if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                            System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
                }



                foreach (var item in file)
                {
                    if (item != null)
                    {
                        if (System.IO.File.Exists(path + item.FileName))
                            System.IO.File.Delete(path + Path.GetExtension(item.FileName));
                        item.SaveAs(path + Path.GetFileNameWithoutExtension(item.FileName) + Path.GetExtension(item.FileName));
                    }
                }
            }
            return Json(new { Data = masterId, Message = AplosMessage.Success });
        }

        private string GetChildPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(JobWorkValueAddedContractChild), out sID);
            return sID;
        }

        private void SaveData(JobWorkValueAddedContractChild data, out string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            string contId = string.Empty;
            string id = string.Empty;
            DataSet dsSeq = null;
            try
            {

                string sql = "SELECT * FROM [dbo].[JobWorkValueAddedContractChild] WHERE Id='" + data.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = "MP" + GetChildPK();
                    dr["JobWorkValueAddedContractMasterId"] = data.JobWorkValueAddedContractMasterId;
                    dr["JobWorkItemMasterId"] = data.JobWorkItemMasterId;
                    dr["JobActivityId"] = data.JobActivityId;
                    dr["MaterialLocationId"] = data.MaterialLocationId;
                    dr["MaterialType"] = data.MaterialType;
                    dr["FinalOutputCategory"] = data.FinalOutputCategory;

                    dr["MaterialSpecification"] = data.MaterialSpecification;
                    dr["MaterialReference"] = data.MaterialReference;
                    dr["OutputMaterialUOMId"] = data.OutputMaterialUOMId;
                    dr["Quantity"] = data.Quantity;
                    dr["ArticleCodeId"] = data.ArticleCodeId;
                    dr["OrderSpecific"] = data.OrderSpecific;
                    dr["Remarks"] = data.Remarks;
                    dr["RequiredCapacity"] = data.RequiredCapacity;
                    dr["RateApplyId"] = data.RateApplyId;
                    dr["CurrencyId"] = data.CurrencyId;
                    dr["RatePerUnit"] = data.RatePerUnit;
                    dr["Rejection"] = data.Rejection;
                    dr["ValueLoss"] = data.ValueLoss;
                    dr["ResponsiblePersonId"] = data.ResponsiblePersonId;
                    dr["FileName"] = data.FileName;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsMaster.Tables[0].Rows.Add(dr);

                    contId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();
                    dr["JobWorkValueAddedContractMasterId"] = data.JobWorkValueAddedContractMasterId;
                    dr["JobWorkItemMasterId"] = data.JobWorkItemMasterId;
                    dr["JobActivityId"] = data.JobActivityId;
                    dr["MaterialLocationId"] = data.MaterialLocationId;
                    dr["MaterialType"] = data.MaterialType;
                    dr["FinalOutputCategory"] = data.FinalOutputCategory;
                    dr["MaterialSpecification"] = data.MaterialSpecification;
                    dr["MaterialReference"] = data.MaterialReference;
                    dr["OutputMaterialUOMId"] = data.OutputMaterialUOMId;
                    dr["Quantity"] = data.Quantity;
                    dr["ArticleCodeId"] = data.ArticleCodeId;
                    dr["OrderSpecific"] = data.OrderSpecific;
                    dr["Remarks"] = data.Remarks;
                    dr["RequiredCapacity"] = data.RequiredCapacity;
                    dr["RateApplyId"] = data.RateApplyId;
                    dr["CurrencyId"] = data.CurrencyId;
                    dr["RatePerUnit"] = data.RatePerUnit;
                    dr["Rejection"] = data.Rejection;
                    dr["ValueLoss"] = data.ValueLoss;
                    dr["ResponsiblePersonId"] = data.ResponsiblePersonId;
                    dr["FileName"] = data.FileName;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }
                masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public Dictionary<string, object> GetFile(string Id)
        {
            try
            {
                var sql = @"SELECT Id, FileName FROM [dbo].[JobWorkValueAddedContractChild]  WHERE Id='" + Id + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpPost, Authorize]
        public ActionResult LoadAllEmpDetails(string Id)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS Id, EMP.EmployeeStatus,
                        EMP.EmployeeName,EMP.EmployeeCode AS Code,
                        EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName DepartmentName,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
    
                        WHERE emp.GroupID='" + identity.CompanyGroupId + @"' and emp.CompanyId='" + identity.CompanyId + @"' and emp.EmployeeStatus='Active' and EMP.EmpType='Local'
                   AND isnull(Emp.SystemID,'') not in (select isnull(ResponsiblePersonId,'') from dbo.JobWorkValueAddedContractChild where JobWorkValueAddedContractMasterId='" + Id + @"')
                  order by EMP.EmployeeCode";

                var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet, Authorize]
        public JsonResult getMaterialPlanningData(string MasterId)
        {

            string sql = @"select vcc.*, jwi.UserName as JobWorkItem,jwa.UserName as JobWorkActivity , uom.UserName as OutputUnit,mma.Code as ArticleCode,mma.StandardName as ArticleName
                                           ,c.Code as Currency,emp.EmployeeCode, emp.EmployeeName as ResponsiblePerson, emp.EmployeeStatus
										   ,MS.UserName as MaterialLocation
                                           from dbo.JobWorkValueAddedContractChild vcc left join HKP.JobWorkItem jwi on jwi.Id=vcc.JobWorkItemMasterId
										   left join SCS.UnitOfMeasurement uom on uom.Id=vcc.OutputMaterialUOMId
										   left join MST.MaterialMasterArticle mma on mma.Id=vcc.ArticleCodeId
										   left join scs.Currency c on c.Id=vcc.CurrencyId
										   left join dbo.EmployeeInformation emp on emp.SystemId=vcc.ResponsiblePersonId
										   left join hkp.JobWorkActivity jwa on jwa.Id=vcc.JobActivityId
										   left join HKP.MaterialStorage MS on MS.Id=vcc.MaterialLocationId
										   where vcc.JobWorkValueAddedContractMasterId='" + MasterId + "' ";


            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult MaterialMSTDataToEdit(string ArticleId)
        {

            string sql = @"select mm.Id as MaterialMasterId, mm.Code as MaterialCode,mm.UserName as MaterialName, mp.ArticleCodeId,mp.OutputMaterialUOMId 
                                             ,Unit=case when mp.ArticleCodeId is not null then mmuom.UserName else uom.UserName End
											 from dbo.JobWorkValueAddedContractChild mp left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleCodeId
                                             left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
											 left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
											 left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
											 left join SCS.UnitOfMeasurement uom on uom.Id=jwi.UOMId
											 where mp.ArticleCodeId='" + ArticleId + @"' ";


            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult DelMaterialPlanning(string Id)
        {
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                DataSet dsMaster;

                if (!string.IsNullOrEmpty(Id))
                {
                    con.OpenDataSetThroughAdapter("select * from dbo.JobWorkValueAddedContractChild2 where JobWorkValueAddedContractChildMasterId='" + Id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Order Wise Requirement Data");
                    }
                }

                string sql = @" delete from dbo.JobWorkValueAddedContractChild where Id='" + Id + "'";

                
                con.BeginTransaction();

                con.executeQuery(sql);

                con.CommitTransaction();
                return Json(new
                {
                    Error = false,
                    Message = "Material Planning deleted successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // Order Wise Requirement

        private string GetOWRPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobWorkValueAddedContractChild2", out sID);
            return sID;
        }

        [HttpPost, Authorize]
        public JsonResult SaveOrderWiseReq(Dictionary<string, object> data, string ChildMasterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from dbo.JobWorkValueAddedContractChild2 where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = "OR" + GetChildPK();
                    dr["JobWorkValueAddedContractChildMasterId"] = ChildMasterId;
                    dr["OrderType"] = data["OrderType"];
                    dr["CustomerId"] = data["CustomerId"];
                    dr["MasterOrderNoId"] = data["MasterOrderNoId"];
                    dr["MasterOrderItemId"] = data["MasterOrderItemId"];

                    dr["ParticularSpecification"] = data["ParticularSpecification"];
                    dr["OutputMaterialUOMId"] = data["OutputMaterialUOMId"];
                    dr["Quantity"] = data["Quantity"];
                    dr["PlanQuantity"] = data["PlanQuantity"];
                    
                    dr["Remarks"] = data["Remarks"];
                   
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();
                    dr["JobWorkValueAddedContractChildMasterId"] = ChildMasterId;
                    dr["OrderType"] = data["OrderType"];
                    dr["CustomerId"] = data["CustomerId"];
                    dr["MasterOrderNoId"] = data["MasterOrderNoId"];
                    dr["MasterOrderItemId"] = data["MasterOrderItemId"];

                    dr["ParticularSpecification"] = data["ParticularSpecification"];
                    dr["OutputMaterialUOMId"] = data["OutputMaterialUOMId"];
                    dr["Quantity"] = data["Quantity"];
                    dr["PlanQuantity"] = data["PlanQuantity"];

                    dr["Remarks"] = data["Remarks"];
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpGet, Authorize]
        public ActionResult DelOrderWise(string Id)
        {
            try
            {

                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName2 + " where Id='" + Id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpGet, Authorize]
        public JsonResult getOrderWiseData(string MaterialMasterId)
        {

            string sql = @"select owr.*,P.UserName as Customer,mo.MasterOrderNo,mm.UserName as MaterialOrderItem, uom.UserName as UOM 
                                                    from dbo.JobWorkValueAddedContractChild2 owr left join HKP.Party P on P.Id=owr.CustomerId
                                                    left join TRN.MasterOrder mo on mo.Id=owr.MasterOrderNoId												
													left join TRN.MasterOrderItem moi on moi.Id=owr.MasterOrderItemId
													left join MST.MaterialMaster mm on mm.Id=moi.MaterialMasterId
													left join SCS.UnitOfMeasurement uom on uom.Id=owr.OutputMaterialUOMId
										            where owr.JobWorkValueAddedContractChildMasterId='" + MaterialMasterId + "' ";


            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        // TRANSFORMATION TAB

        [HttpPost, Authorize]
        public ActionResult LoadAllVendorDetails(string Id)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"select distinct p.Id, p.Sequence, p.Code, p.ShortName, p.StandardName, p.UserName,pg.UserName as PartyGroup
                               from HKP.Party p left join HKP.PartyGroup pg on pg.Id=p.PartyGroupId
							   left join hkp.CompanyParty cp on p.Id=cp.PartyId
                               WHERE p.CompanyGroupId='" + identity.CompanyGroupId + @"' and cp.PartyType='Vendor'
                               AND isnull(p.Id,'') not in (select isnull(VendorPartyId,'') from dbo.JobWorkTransformationContract where Id='" + Id + @"')
                               order by p.Sequence";

                var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private string GetTPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JobWorkTransformationContract", out sID);
            return sID;
        }

        [HttpPost]
        public JsonResult SaveTransformation(Dictionary<string, object> data)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
    
                con.OpenDataSetThroughAdapter("select * from " + TableName3 + " where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName3, out _Id);

                    data["Id"] = "T" + GetTPK();
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

                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpGet, Authorize]
        public ActionResult DelTransData(string Id)
        {
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                DataSet dsMaster;

                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select entry first");

                if (!string.IsNullOrEmpty(Id))
                {
                    con.OpenDataSetThroughAdapter("select * from dbo.OSTransformationPODetail where OSTransformationPOId='" + Id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Material Planning Data");
                    }
                }

                con.BeginTransaction();
                con.executeQuery("delete from dbo.JobWorkTransformationContract where Id='" + Id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        // Material Planning tab of Transformation Tab

        [HttpPost, Authorize]
        public ActionResult LoadAllMaterialMstDetails(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select mm.Id, mm.Code, mm.UserName as MaterialName,mc.UserName as MaterialCategory, mgm.UserName as MaterialGroupMaster,mm.BaseUOMId, buom.UserName as BaseUOM
                                      ,WithSKU=case when mm.WithSKU=0 then 'No' else 'Yes' END
									  ,IsAsset=case when mm.IsAsset=0 then 'No' else 'Yes' END
                                      from MST.MaterialMaster mm left join MST.MaterialGroupMaster mgm on mm.MaterialGroupMasterId=mgm.Id
									  left join SCS.UnitOfMeasurement buom on buom.Id=mm.BaseUOMId
									  left join HKP.MaterialCategory mc on mc.Id=mm.MaterialCategoryId
                                      WHERE mm.CompanyGroupId='" + identity.CompanyGroupId + @"' order by mm.Code";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult LoadAllMaterialMstArticle(string Id, string MaterialMstId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"Select mm.Code as MaterialCode,mm.UserName as Material,mgm.UserName as MaterialGroupMaster,mma.Id as ArticleId ,mma.Code as ArticleCode, mma.ShortName, mma.StandardName 
                           from MST.MaterialMasterArticle mma left join MST.MaterialMaster mm on mma.MaterialMasterId=mm.Id
                           left join MST.MaterialGroupMaster mgm on mm.MaterialGroupMasterId=mgm.Id
                            where mm.Id='" + MaterialMstId + @"'
                          AND isnull(mma.Id,'') not in (select isnull(ArticleCodeId,'') from dbo.OSTransformationPODetail where Id='" + Id + @"')
                          order by mm.Code";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult LoadAllMMArticle(string Id, string MaterialMstId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"Select mm.Code as MaterialCode,mm.UserName as Material,mgm.UserName as MaterialGroupMaster,mma.Id as ArticleId ,mma.Code as ArticleCode, mma.ShortName, mma.StandardName 
                           from MST.MaterialMasterArticle mma left join MST.MaterialMaster mm on mma.MaterialMasterId=mm.Id
                           left join MST.MaterialGroupMaster mgm on mm.MaterialGroupMasterId=mgm.Id
                            where mm.Id='" + MaterialMstId + @"'
                          AND isnull(mma.Id,'') not in (select isnull(ArticleCodeId,'') from dbo.JobWorkValueAddedContractChild where Id='" + Id + @"')
                          order by mm.Code";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public JsonResult saveUrlMatPlanning(FormCollection form, HttpPostedFileBase[] file)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
            var model = JsonConvert.DeserializeObject<OSTransformationPODetail>(form["MatPlanning"], settings);



            SaveMatPlanningData(model, out string masterId);
            if (file.IsNotNull())
            {
                var directory = ResourcesPathReader.GetTransformationContractMaterialChildFile();
                var path = Path.Combine(directory);



                for (int i = 0; i < file.Length; i++)
                {
                    ResourcesPathReader.IsValidFileExtention(Path.GetExtension(file[i].FileName));
                }

                var fileId = "";
                var fileName = "";
                var filedata = GetFile(model.Id);
                if (filedata.Count > 0)
                {
                    if (!string.IsNullOrEmpty(filedata["Id"].ToString()) &&
                        !string.IsNullOrEmpty(filedata["FileName"].ToString()))
                        fileId = filedata["Id"].ToString();
                    fileName = filedata["FileName"].ToString();

                    if (fileName != model.FileName)
                        if (System.IO.File.Exists(path + fileId + Path.GetExtension(fileName)))
                            System.IO.File.Delete(path + fileId + Path.GetExtension(fileName));
                }



                foreach (var item in file)
                {
                    if (item != null)
                    {
                        if (System.IO.File.Exists(path + item.FileName))
                            System.IO.File.Delete(path + Path.GetExtension(item.FileName));
                            item.SaveAs(path + Path.GetFileNameWithoutExtension(item.FileName) + Path.GetExtension(item.FileName));
                        //  item.SaveAs(path + Path.GetFileName(item.FileName));
                    }
                }
            }
            return Json(new { Data = masterId, Message = AplosMessage.Success });
        }

        private string GetTransformationChildPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(OSTransformationPODetail), out sID);
            return sID;
        }

        private void SaveMatPlanningData(OSTransformationPODetail data, out string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            string contId = string.Empty;
            string id = string.Empty;
            DataSet dsSeq = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");

                objCon.OpenDataSetThroughAdapter("select * from dbo.OSTransformationPODetail where JobActivityId='"+ data.JobActivityId +"' and JobWorkItemMasterId='"+ data.JobWorkItemMasterId +"' and ArticleCodeId='"+ data.ArticleCodeId + "' and OSTransformationPOId='"+ data.OSTransformationPOId + "' AND  Id<>'" + data.Id + "' ", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same Activity, JW Output Item, Material and Article already exist.");
                }

                if (data.Tolerance == null)
                {
                    data.Tolerance =Convert.ToString(0);
                }

                string sql = "SELECT * FROM [dbo].[OSTransformationPODetail] WHERE Id='" + data.Id + "'";
    //            objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = "M" + GetTransformationChildPK();
                    dr["OSTransformationPOId"] = data.OSTransformationPOId;
                    dr["JobWorkItemMasterId"] = data.JobWorkItemMasterId;
                    dr["JobActivityId"] = data.JobActivityId;
                    dr["MaterialLocationId"] = data.MaterialLocationId;
                    dr["MaterialType"] = data.MaterialType;
                    dr["FinalOutputCategory"] = data.FinalOutputCategory;

                    dr["MaterialSpecification"] = data.MaterialSpecification;
                    dr["MaterialReference"] = data.MaterialReference;
                    dr["OutputMaterialUOMId"] = data.OutputMaterialUOMId;
                    dr["Quantity"] = data.Quantity;
                    dr["ArticleCodeId"] = data.ArticleCodeId;
                    dr["OrderSpecific"] = data.OrderSpecific;
                    dr["Remarks"] = data.Remarks;
                    dr["RequiredCapacity"] = data.RequiredCapacity;
                    dr["ByProductApplicable"] = data.ByProductApplicable;
                    dr["RateApplyId"] = data.RateApplyId;
                    dr["CurrencyId"] = data.CurrencyId;
                    dr["RatePerUnit"] = data.RatePerUnit;
                    dr["Rejection"] = data.Rejection;
                    dr["ValueLoss"] = data.ValueLoss;
                    dr["Tolerance"] = data.Tolerance;
                    dr["ResponsiblePersonId"] = data.ResponsiblePersonId;
                    dr["FileName"] = data.FileName;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsMaster.Tables[0].Rows.Add(dr);

                    contId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();
                    dr["OSTransformationPOId"] = data.OSTransformationPOId;
                    dr["JobWorkItemMasterId"] = data.JobWorkItemMasterId;
                    dr["MaterialLocationId"] = data.MaterialLocationId;
                    dr["MaterialType"] = data.MaterialType;
                    dr["FinalOutputCategory"] = data.FinalOutputCategory;

                    dr["MaterialSpecification"] = data.MaterialSpecification;
                    dr["MaterialReference"] = data.MaterialReference;
                    dr["OutputMaterialUOMId"] = data.OutputMaterialUOMId;
                    dr["Quantity"] = data.Quantity;
                    dr["ArticleCodeId"] = data.ArticleCodeId;
                    dr["OrderSpecific"] = data.OrderSpecific;
                    dr["Remarks"] = data.Remarks;
                    dr["RequiredCapacity"] = data.RequiredCapacity;
                    dr["ByProductApplicable"] = data.ByProductApplicable;
                    dr["RateApplyId"] = data.RateApplyId;
                    dr["CurrencyId"] = data.CurrencyId;
                    dr["RatePerUnit"] = data.RatePerUnit;
                    dr["Rejection"] = data.Rejection;
                    dr["ValueLoss"] = data.ValueLoss;
                    dr["Tolerance"] = data.Tolerance;
                    dr["ResponsiblePersonId"] = data.ResponsiblePersonId;
                    dr["FileName"] = data.FileName;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }
                masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public Dictionary<string, object> GetMatPlanningFile(string Id)
        {
            try
            {
                var sql = @"SELECT Id, FileName FROM [dbo].[OSTransformationPODetail]  WHERE Id='" + Id + "'";
                return _sqlRepository.GetData(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        [HttpPost, Authorize]
        public ActionResult LoadAllResponsiblePersonDetails(string Id)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS Id, EMP.EmployeeStatus,
                        EMP.EmployeeName,EMP.EmployeeCode AS Code,
                        EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName DepartmentName,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
    
                        WHERE emp.GroupID='" + identity.CompanyGroupId + @"' and emp.CompanyId='"+ identity.CompanyId + @"' and emp.EmployeeStatus='Active' and EMP.EmpType='Local'
                   AND isnull(Emp.SystemID,'') not in (select isnull(ResponsiblePersonId,'') from dbo.OSTransformationPODetail where OSTransformationPOId='" + Id + @"')
                  order by EMP.EmployeeCode";

                var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet, Authorize]
        public JsonResult getMatMstDataToEdit(string ArticleId)
        {

            string sql = @"select mm.Id as MaterialMasterId, mm.Code as MaterialCode,mm.UserName as MaterialName, mp.ArticleCodeId,mp.OutputMaterialUOMId 
                                             ,Unit=case when mp.ArticleCodeId is not null then mmuom.UserName else uom.UserName End
											 from dbo.OSTransformationPODetail mp left join MST.MaterialMasterArticle mma on mma.Id=mp.ArticleCodeId
                                             left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
											 left join HKP.JobWorkItem jwi on jwi.Id=mp.JobWorkItemMasterId
											 left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
											 left join SCS.UnitOfMeasurement uom on uom.Id=jwi.UOMId
											 where mp.ArticleCodeId='"+ ArticleId + @"' ";


            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult getMatPlanningData(string MasterId)
        {

            string sql = @"select tcc.*, jwi.UserName as JWOutputItem,jwa.UserName as JobWorkActivity, uom.UserName as OutputUnit, mma.Code as ArticleCode ,mma.StandardName as ArticleName
                                          ,mm.Id as MaterialMasterId,mm.Code as MaterialCode, mm.UserName as MaterialName
                                           ,c.Code as Currency, emp.EmployeeName as ResponsiblePerson, emp.EmployeeCode, emp.EmployeeStatus
										   ,MS.UserName as MaterialLocation
                                           from dbo.OSTransformationPODetail tcc left join HKP.JobWorkItem jwi on jwi.Id=tcc.JobWorkItemMasterId
										   left join SCS.UnitOfMeasurement uom on uom.Id=tcc.OutputMaterialUOMId
										   left join MST.MaterialMasterArticle mma on mma.Id=tcc.ArticleCodeId
										   left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
										   left join scs.Currency c on c.Id=tcc.CurrencyId
										   left join dbo.EmployeeInformation emp on emp.SystemId=tcc.ResponsiblePersonId
										   left join hkp.JobWorkActivity jwa on jwa.Id=tcc.JobActivityId
										   left join HKP.MaterialStorage MS on MS.Id=tcc.MaterialLocationId
										   where tcc.OSTransformationPOId='" + MasterId + "' ";


            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult DelMatPlanning(string Id)
        {
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                DataSet dsMaster;

                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select entry first");

                if (!string.IsNullOrEmpty(Id))
                {
                    con.OpenDataSetThroughAdapter("select * from dbo.OSTransformationPOMasterOrderItem where OSTransformationPODetailId='" + Id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Order Wise Requirement Data");
                    }

                    con.OpenDataSetThroughAdapter("select * from dbo.OSTransformationPOInputMaterial where OSTransformationPODetailId='" + Id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Material Input Data");
                    }
                }

                string sql = @" delete from dbo.OSTransformationPODetail where Id='" + Id + "'";

                con.BeginTransaction();

                con.executeQuery(sql);

                con.CommitTransaction();
                return Json(new
                {
                    Error = false,
                    Message = "Material Planning deleted successfully"
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new
                {
                    Error = true,
                    Message = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        //  ORDER WISE REQUIREMENT OF TRANSFORMATION TAB

        // Order Wise Requirement

        private string GetOWTRPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JWTransformationPOMasterOrderItem", out sID);
            return sID;
        }

        [HttpPost, Authorize]
        public JsonResult SaveTransformOrderWiseReqTab(Dictionary<string, object> data, string ChildMasterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from dbo.JWTransformationPOMasterOrderItem where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = "OW" + GetOWTRPK();
                    dr["JWTransformationPODetailId"] = ChildMasterId;
                    dr["OrderType"] = data["OrderType"];
                    dr["CustomerId"] = data["CustomerId"];
                    dr["MasterOrderNoId"] = data["MasterOrderNoId"];
                    dr["MasterOrderItemId"] = data["MasterOrderItemId"];

                    dr["ParticularSpecification"] = data["ParticularSpecification"];
                    dr["OutputMaterialUOMId"] = data["OutputMaterialUOMId"];
                    dr["Quantity"] = data["Quantity"];
                    dr["PlanQuantity"] = data["PlanQuantity"];
                    dr["SalesOrderId"] = data["SalesOrderId"];

                    dr["Remarks"] = data["Remarks"];

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    //edit
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;

                    dr.BeginEdit();
                    dr["JWTransformationPODetailId"] = ChildMasterId;
                    dr["OrderType"] = data["OrderType"];
                    dr["CustomerId"] = data["CustomerId"];
                    dr["MasterOrderNoId"] = data["MasterOrderNoId"];
                    dr["MasterOrderItemId"] = data["MasterOrderItemId"];

                    dr["ParticularSpecification"] = data["ParticularSpecification"];
                    dr["OutputMaterialUOMId"] = data["OutputMaterialUOMId"];
                    dr["Quantity"] = data["Quantity"];
                    dr["PlanQuantity"] = data["PlanQuantity"];
                    dr["SalesOrderId"] = data["SalesOrderId"];

                    dr["Remarks"] = data["Remarks"];
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpGet, Authorize]
        public ActionResult DelTransformOrderWise(string Id)
        {
            try
            {

                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from dbo.JWTransformationPOMasterOrderItem where Id='" + Id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpGet, Authorize]
        public JsonResult getTransformOrderWiseData(string MaterialMasterId)
        {

            //string sql = @"select owr.*,P.UserName as Customer,mo.MasterOrderNo,mm.UserName as MaterialOrderItem, uom.UserName as UOM 
            //                                        from dbo.OSTransformationPOMasterOrderItem owr left join HKP.Party P on P.Id=owr.CustomerId
            //                                        left join TRN.MasterOrder mo on mo.Id=owr.MasterOrderNoId												
            //	left join TRN.MasterOrderItem moi on moi.Id=owr.MasterOrderItemId
            //	left join MST.MaterialMaster mm on mm.Id=moi.MaterialMasterId
            //	left join SCS.UnitOfMeasurement uom on uom.Id=owr.OutputMaterialUOMId
            //          where owr.OSTransformationPODetailId='" + MaterialMasterId + "' ";

            string sql = @"SELECT ROW_NUMBER() OVER (ORDER BY SO.MasterOrderItemId) AS RN,POD.ProductionOrderId
	                            , MOI.MasterOrderId, MO.MasterOrderNo, SO.MasterOrderItemId,moi.BuyerReferenceNo,moi.OwnReferenceNo,mo.BuyerReferenceNo BuyerOrderNo,mo.OwnReferenceNo AS OwnOrderNo
	                            , SO.Id AS SalesOrderId, P.UserName AS Customer,B.UserName AS Buyer,PM.Id AS ProductID,isnull(MOI.ProductionGrouping,'') AS ProductionGrouping
	                            , MOI.MaterialMasterId, MM.UserName AS MaterialMasterName,PM.UserName AS ProductName
	                            , MOI.ArticleId, ART.StandardName AS ArticleName
	                            , DeliveryDate = REPLACE(CONVERT(CHAR(11), DeliveryDate, 106),' ','-')
	                            , CommitmentDate = REPLACE(CONVERT(CHAR(11), CommitmentDate, 106),' ','-')
	                            , isnull(DEST.UserName,'') AS DestinationName, isnull(SHP.UserName,'') AS ShipmentModeName
	                            , isnull(PO.PONumber,'') AS PONumber, OS.UserName AS OrderStatusName, OC.UserName AS OrderCategoryName
	                            , SO.Qty, SO.Rate,SO.Description,CASE WHEN isnull(so.WeekNo,0)=0 THEN  DATEPART(week,so.DeliveryDate) ELSE so.WeekNo END AS DeliveryWeek
	                            , Flag = CAST(0 AS BIT),SO.DestinationDescription
								,CN.ContractNo,MLC.LCRef MasterLCNo, Uom.UserName as MasterOrderUoM
								,owr.OrderType, owr.ParticularSpecification,owr.PlanQuantity as OWPlanQuantity, owr.Remarks as OWRemarks, owr.Id
                       FROM dbo.JWTransformationPOMasterOrderItem owr left join [TRN].[SalesOrder] AS SO on owr.SalesOrderId=SO.Id 
                        left outer join [TRN].[ProductionOrderDetail] POD on POD.SalesOrderId=SO.Id 
                       JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
                       JOIN [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
                       LEFT JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id 
					   LEFT JOIN trn.ProductDefinition AS pd ON pd.MaterialMasterId=moi.MaterialMasterId
					   LEFT JOIN [MST].[ProductMaster] PM ON pm.Id=pd.ProductMasterId
                       LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = ART.Id
                       LEFT JOIN [HKP].[Party] AS P ON MO.PartyId = P.Id
					   LEFT JOIN HKP.BUYER b on b.Id=MO.BuyerId
                       LEFT JOIN [MST].[Destination] AS DEST ON SO.DestinationId = DEST.Id
                       LEFT JOIN [MST].[ShipMode] AS SHP ON SO.ShipmentModeId = SHP.Id
                       LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                       LEFT JOIN [HKP].[OrderStatus] AS OS ON SO.OrderStatusId = OS.Id
                       LEFT JOIN [HKP].[OrderCategory] AS OC ON SO.OrderCategoryId = OC.Id
                       LEFT JOIN dbo.[Contract] AS CN ON CN.Id=SO.ContractId
                       LEFT JOIN dbo.MasterLC AS MLC ON MLC.Id=CN.MasterLCId
					   left join SCS.UnitOfMeasurement Uom on Uom.Id=MO.TotalQtyUOMId
					   where owr.JWTransformationPODetailId='" + MaterialMasterId + @"' ";


            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        //  MATERIAL INPUT TAB
        [HttpGet, Authorize]
        public JsonResult getMatInputListData(string JobWorkItemId, string ActivityId, string Id)
        {

            string sql = @"select tmi.*, jwi.UserName as JWInputItem,jwi.UOMId as JWIUnitId,juom.UserName as JWIUnit, emp.EmployeeCode, emp.EmployeeName,mm.Id as InputMaterialId
                                        , mm.UserName as InputMaterial, uom.UserName as MMUnit
                                        ,Unit=case when jwi.MaterialMasterId is not null then uom.UserName else juom.UserName end 
										from MST.JobWorkTransformationMasterMaterialInput tmi left join HKP.JobWorkItem jwi on jwi.Id=tmi.JobWorkItemId
                                        left join dbo.EmployeeInformation emp on emp.SystemId=tmi.ResponsiblePersonId
										left join MST.JobWorkTransformationMaster tm on tm.Id=tmi.JobWorkTransformationMasterId
										left join MST.MaterialMaster mm on mm.Id=jwi.MaterialMasterId
										left join scs.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
										left join scs.UnitOfMeasurement juom on juom.Id=jwi.UOMId
                           where tm.JobWorkActivityId='" + ActivityId + @"' and tm.JobWorkActivityChildId='" + JobWorkItemId + @"'
                           --AND isnull(tmi.JobWorkItemId,'') not in (select isnull(JobWorkItemId,'') from dbo.OSTransformationPOInputMaterial where OSTransformationPODetailId='" + Id + @"') 
                            ";


            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        private string GetMaterialInputPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JWTransformationPOInputMaterial", out sID);
            return sID;
        }

        [HttpPost, Authorize]
        public JsonResult SaveMaterialInputTab(IEnumerable<JobWorkMaterialInputData> SelectedMatInputData, string ChildMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet ExistOrNot;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var JWIId = "' '";
                var ArtId = "' '";

                foreach (var empitem in SelectedMatInputData)
                {
                    JWIId += ",'" + empitem.JobWorkItemId + "' ";
                    ArtId += ",'" + empitem.ArticleId + "' ";

                }
                con.OpenDataSetThroughAdapter("select * from dbo.JWTransformationPOInputMaterial where JobWorkItemId IN ( " + JWIId + " ) and ArticleId IN ("+ ArtId + ") and JWTransformationPODetailId='" + ChildMasterId + "'  ", out ExistOrNot, false, "1");

                foreach (var item in SelectedMatInputData)
                {
                    ExistOrNot.Tables[0].DefaultView.RowFilter = "JobWorkItemId ='" + item.JobWorkItemId + "' and ArticleId='"+ item.ArticleId +"' and JWTransformationPODetailId='" + ChildMasterId + "' ";

                    if (ExistOrNot.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = ExistOrNot.Tables[0].NewRow();
                        dr["Id"] = "MI" + GetMaterialInputPK();

                        dr["JWTransformationPODetailId"] = ChildMasterId;
                        dr["JobWorkItemId"] = item.JobWorkItemId;
                        dr["ItemSpecification"] = item.ItemSpecification;
                        dr["NetConsumption"] = item.NetConsumption;
                        dr["Rejection"] = item.Rejection;
                        dr["ValueLoss"] = item.ValueLoss;
                        dr["GrossConsumption"] = item.GrossConsumption;
                        dr["ResponsiblePersonId"] = item.ResponsiblePersonId;
                        dr["Remarks"] = item.Remarks;
                        dr["ArticleId"] = item.ArticleId;
                        dr["BOQRequiredQuantity"] = 0;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;

                        ExistOrNot.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        //edit
                        DataRow dr = ExistOrNot.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();

                        dr["JWTransformationPODetailId"] = ChildMasterId;
                        dr["JobWorkItemId"] = item.JobWorkItemId;
                        dr["ItemSpecification"] = item.ItemSpecification;
                        dr["NetConsumption"] = item.NetConsumption;
                        dr["Rejection"] = item.Rejection;
                        dr["ValueLoss"] = item.ValueLoss;
                        dr["GrossConsumption"] = item.GrossConsumption;
                        dr["ResponsiblePersonId"] = item.ResponsiblePersonId;
                        dr["Remarks"] = item.Remarks;
                        dr["ArticleId"] = item.ArticleId;
                        dr["BOQRequiredQuantity"] = 0;

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();

                    }

                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(ExistOrNot);

                return Json(new { Error = false, Message = AplosMessage.Updated });

            }


            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpGet, Authorize]
        public ActionResult DelMaterialInput(string Id)
        {
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                DataSet dsMaster;

                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select entry first");

                if (!string.IsNullOrEmpty(Id))
                {
                    con.OpenDataSetThroughAdapter("select * from dbo.JWTransformationPOByProduct where JWTransformationPOInputMaterialId='" + Id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete By Product Data");
                    }

                }

                con.BeginTransaction();
                con.executeQuery("delete from dbo.JWTransformationPOInputMaterial where Id='" + Id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpGet, Authorize]
        public JsonResult getMaterialInputData(string MaterialMasterId)
        {

            string sql = @"select mi.*,jwi.UserName as JWInputItem,juom.UserName as JWIUnit, emp.EmployeeCode, emp.EmployeeStatus, emp.EmployeeName as ResponsiblePerson
                                                   ,mm.UserName as InputMaterial, uom.UserName as MMUnit, mma.StandardName as InputArticle, mma.Id as InputArticleId
                                                    ,Unit=case when jwi.MaterialMasterId is not null then uom.UserName else juom.UserName end 
													from dbo.JWTransformationPOInputMaterial mi 
													left join HKP.JobWorkItem jwi on jwi.Id=mi.JobWorkItemId
													left join dbo.EmployeeInformation emp on emp.SystemId=mi.ResponsiblePersonId
													left join MST.MaterialMaster mm on mm.Id=jwi.MaterialMasterId
							             			left join scs.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
							            			left join scs.UnitOfMeasurement juom on juom.Id=jwi.UOMId
													left join MST.MaterialMasterArticle mma on mma.Id=mi.ArticleId
										            where mi.JWTransformationPODetailId='" + MaterialMasterId + "' ";


            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult LoadMatInputResponsiblePersonDetails(string Id)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS Id, EMP.EmployeeStatus,
                        EMP.EmployeeName,EMP.EmployeeCode AS Code,
                        EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName DepartmentName,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=pr.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
    
                        WHERE emp.GroupID='" + identity.CompanyGroupId + @"' and emp.CompanyId='" + identity.CompanyId + @"' and emp.EmployeeStatus='Active'
                   AND isnull(Emp.SystemID,'') not in (select isnull(ResponsiblePersonId,'') from dbo.OSTransformationPOInputMaterial where OSTransformationPODetailId='" + Id + @"')
                  order by EMP.EmployeeCode";

                var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //  BY PRODUCT TAB

        private string GetByProductPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "JWTransformationPOByProduct", out sID);
            return sID;
        }

        [HttpPost]
        public JsonResult SaveByProductTab(IEnumerable<JobWorkByProductData> ByProductMstData, string ChildMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                DataSet ExistOrNot;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var JWId = "' '";
                var ArtId = "' '";

                foreach (var empitem in ByProductMstData)
                {
                    JWId += ",'" + empitem.JobWorkItemId + "' ";
                    ArtId += ",'" + empitem.BPArticleId + "' ";

                }
                con.OpenDataSetThroughAdapter("select * from dbo.JWTransformationPOByProduct where JobWorkItemId IN ( " + JWId + " ) and ArticleId IN (" + ArtId + ") and JWTransformationPOInputMaterialId='" + ChildMasterId + "'  ", out ExistOrNot, false, "1");

                foreach (var item in ByProductMstData)
                {
                    ExistOrNot.Tables[0].DefaultView.RowFilter = "JobWorkItemId ='" + item.JobWorkItemId + "' and ArticleId='" + item.BPArticleId + "' and JWTransformationPOInputMaterialId='" + ChildMasterId + "' ";

                    if (ExistOrNot.Tables[0].DefaultView.Count == 0)
                    {
                        DataRow dr = ExistOrNot.Tables[0].NewRow();
                        dr["Id"] = "BP" + GetByProductPK();

                        dr["JWTransformationPOInputMaterialId"] = ChildMasterId;
                        dr["JobWorkItemId"] = item.JobWorkItemId;
                        dr["ItemSpecification"] = item.ItemSpecification;
                        dr["CurrencyId"] = item.CurrencyId;
                        dr["StandardRate"] = item.StandardRate;
                        dr["Tolerance"] = item.Tolerance;
                        dr["PercentageOfInput"] = item.PercentageOfInput;
                        dr["ResponsiblePersonId"] = item.ResponsiblePersonId;
                        dr["Remarks"] = item.Remarks;
                        dr["ArticleId"] = item.BPArticleId;

                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
     
                        ExistOrNot.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        //edit
                        DataRow dr = ExistOrNot.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();

                        dr["JWTransformationPOInputMaterialId"] = ChildMasterId;
                        dr["JobWorkItemId"] = item.JobWorkItemId;
                        dr["ItemSpecification"] = item.ItemSpecification;
                        dr["CurrencyId"] = item.CurrencyId;
                        dr["StandardRate"] = item.StandardRate;
                        dr["Tolerance"] = item.Tolerance;
                        dr["PercentageOfInput"] = item.PercentageOfInput;
                        dr["ResponsiblePersonId"] = item.ResponsiblePersonId;
                        dr["Remarks"] = item.Remarks;
                        dr["ArticleId"] = item.BPArticleId;

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();

                    }

                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(ExistOrNot);

                return Json(new { Error = false, Message = AplosMessage.Updated });

            }


            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [HttpGet, Authorize]
        public ActionResult DelByProduct(string Id)
        {
            try
            {

                if (string.IsNullOrEmpty(Id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from dbo.JWTransformationPOByProduct where Id='" + Id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpGet, Authorize]
        public JsonResult getByProductMasterData(string JobWorkItemId, string ActivityId, string Id)
        {

            string sql = @"select bp.*,jwi.UserName as ByProductItem, c.Code as Currency, emp.EmployeeCode, emp.EmployeeName,mm.Id as BPMaterialId, mm.UserName as ByProductMaterial, mm.Code as BPMaterialCode
                           ,Unit= case when jwi.MaterialMasterId is not null then mmuom.UserName else uom.UserName End
                           ,0 Tolerance
                           from MST.JobWorkTransformationMasterByProduct bp left join HKP.JobWorkItem jwi on jwi.Id=bp.JobWorkItemId
                           left join scs.Currency c on c.Id=bp.CurrencyId
                           left join dbo.EmployeeInformation emp on emp.SystemId=bp.ResponsiblePersonId
                           left join MST.JobWorkTransformationMaster tm on tm.Id=bp.JobWorkTransformationMasterId
						   left join MST.MaterialMaster mm on mm.Id=jwi.MaterialMasterId
						   left join scs.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
						   left join scs.UnitOfMeasurement uom on uom.Id=jwi.UOMId
                           where tm.JobWorkActivityId='" + ActivityId + @"' and tm.JobWorkActivityChildId='"+ JobWorkItemId + @"'
                           --AND isnull(bp.JobWorkItemId,'') not in (select isnull(JobWorkItemId,'') from dbo.JobWorkTransformationContractChild4 where JobWorkTransformationContractChild3MasterId='" + Id + @"') 
                           ";


            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult getByProductData(string MaterialInputId)
        {

            string sql = @"select bp.*,jwi.UserName as ByProductItem, c.Code as Currency, emp.EmployeeCode, emp.EmployeeName, mma.StandardName as ArticleName, mma.Code as ArticleCode
                                                    , mm.UserName as ByProductMaterial, mm.Code as BPMaterialCode
													,Unit= case when bp.ArticleId is not null then mmuom.UserName else uom.UserName End
                                                    from dbo.JWTransformationPOByProduct bp
													left join HKP.JobWorkItem jwi on jwi.Id=bp.JobWorkItemId
                                                    left join scs.Currency c on c.Id=bp.CurrencyId
                                                    left join dbo.EmployeeInformation emp on emp.SystemId=bp.ResponsiblePersonId
													left join MST.MaterialMasterArticle mma on mma.Id=bp.ArticleId
													left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
													left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
													left join SCS.UnitOfMeasurement uom on uom.Id=jwi.UOMId
										            where bp.JWTransformationPOInputMaterialId='" + MaterialInputId + "' ";


            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult LoadMaterialMstDetails(string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"select mm.Id, mm.Code, mm.UserName as MaterialName,mc.UserName as MaterialCategory, mgm.UserName as MaterialGroupMaster,mm.BaseUOMId, buom.UserName as BaseUOM
                                      ,WithSKU=case when mm.WithSKU=0 then 'No' else 'Yes' END
									  ,IsAsset=case when mm.IsAsset=0 then 'No' else 'Yes' END
                                      from MST.MaterialMaster mm left join MST.MaterialGroupMaster mgm on mm.MaterialGroupMasterId=mgm.Id
									  left join SCS.UnitOfMeasurement buom on buom.Id=mm.BaseUOMId
									  left join HKP.MaterialCategory mc on mc.Id=mm.MaterialCategoryId
                                      WHERE mm.CompanyGroupId='" + identity.CompanyGroupId + @"' order by mm.Code";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult LoadMaterialMstArticle(string MaterialMstId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string sql = @"Select mm.Code as MaterialCode,mm.UserName as Material,mgm.UserName as MaterialGroupMaster,mma.Id as ArticleId ,mma.Code as ArticleCode, mma.ShortName, mma.StandardName 
                           from MST.MaterialMasterArticle mma left join MST.MaterialMaster mm on mma.MaterialMasterId=mm.Id
                           left join MST.MaterialGroupMaster mgm on mm.MaterialGroupMasterId=mgm.Id
                            where mm.Id='" + MaterialMstId + @"'
                            order by mm.Code";

            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        

        #region Reports for Value Added Contract

        private void SetHeaderTextTop(ref IWorksheet sheet, int row, int col, string txt, int width, ExcelHAlign al)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = width;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].HorizontalAlignment = al;

        }

        [HttpGet, Authorize]
        public ActionResult GetValueAddedPrintReport(ReportFormat reportFormat, string PrintTabId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = " Value Added Contract " + PrintTabId + "";
            var workbook = GetContractReportWorkSheet(PrintTabId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }

        private IWorkbook GetContractReportWorkSheet(string PrintTabId)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];
            //var sheet1 = workbook.Worksheets[1];
            //var sheet2 = workbook.Worksheets[2];

            sheet.Name = "ValueAddedContract";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            DataTable data = GetContractReportDataById(PrintTabId);
            DataTable MaterialPlanningChilddata = GetMaterialPlanningChildReportDataById(PrintTabId);
            if (data.Rows.Count > 0)
            {
                int ColValueAddedDateHeader = 1;
                int ColValueAddedDateEnd;
                int ColVACTimeHeader;
                int ColVACTimeEnd;
                int ColVACTimeName;
                int ColEntityHeader;
                int ColEntityEnd;
                int ColEntityName;
                int ColPartyNameHeader;
                int ColPartyNameEnd;
                int ColPartyNameName;
                int ColVAProcessStartDateHeader = 1;
                int ColVAProcessStartDateEnd;


                SetHeaderTextTop(ref sheet, ROW, ColValueAddedDateHeader, "Date", 12, ExcelHAlign.HAlignLeft);
                ColValueAddedDateHeader++;
                ColValueAddedDateEnd = ColValueAddedDateHeader + 1;
                sheet.Range[ROW, ColValueAddedDateHeader, ROW, ColValueAddedDateEnd].Text = data.Rows[0]["TransformationDate"].ToString();
                sheet.Range[ROW, ColValueAddedDateHeader, ROW, ColValueAddedDateEnd].Merge();
                sheet.Range[ROW, ColValueAddedDateHeader, ROW, ColValueAddedDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColValueAddedDateHeader, ROW, ColValueAddedDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColValueAddedDateEnd++;

                //ColVACTimeHeader = ColValueAddedDateEnd;
                //SetHeaderTextTop(ref sheet, ROW, ColVACTimeHeader, "Time", 20, ExcelHAlign.HAlignLeft);
                //ColVACTimeHeader++;
                //ColVACTimeEnd = ColVACTimeHeader + 1;
                //ColVACTimeName = ColVACTimeHeader;
                //sheet.Range[ROW, ColVACTimeName, ROW, ColVACTimeEnd].Text = data.Rows[0]["TCTime"].ToString();
                //sheet.Range[ROW, ColVACTimeName, ROW, ColVACTimeEnd].Merge();
                //sheet.Range[ROW, ColVACTimeName, ROW, ColVACTimeEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet.Range[ROW, ColVACTimeName, ROW, ColVACTimeEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ////            ROW++;
                //ColVACTimeEnd++;

                ColEntityHeader = ColValueAddedDateEnd;
                SetHeaderTextTop(ref sheet, ROW, ColEntityHeader, "Entity", 20, ExcelHAlign.HAlignLeft);
                ColEntityHeader++;
                ColEntityEnd = ColEntityHeader + 1;
                ColEntityName = ColEntityHeader;
                sheet.Range[ROW, ColEntityName, ROW, ColEntityEnd].Text = data.Rows[0]["Entity"].ToString();
                sheet.Range[ROW, ColEntityName, ROW, ColEntityEnd].Merge();
                sheet.Range[ROW, ColEntityName, ROW, ColEntityEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColEntityName, ROW, ColEntityEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //           ROW++;
                ColEntityEnd++;

                ColPartyNameHeader = ColEntityEnd;
                SetHeaderTextTop(ref sheet, ROW, ColPartyNameHeader, "PartyName", 20, ExcelHAlign.HAlignLeft);
                ColPartyNameHeader++;
                ColPartyNameEnd = ColPartyNameHeader + 1;
                ColPartyNameName = ColPartyNameHeader;
                sheet.Range[ROW, ColPartyNameName, ROW, ColPartyNameEnd].Text = data.Rows[0]["PartyName"].ToString();
                sheet.Range[ROW, ColPartyNameName, ROW, ColPartyNameEnd].Merge();
                sheet.Range[ROW, ColPartyNameName, ROW, ColPartyNameEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColPartyNameName, ROW, ColPartyNameEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;
                //          ColPartyNameEnd++;


                SetHeaderTextTop(ref sheet, ROW, ColVAProcessStartDateHeader, "Process Start Date", 12, ExcelHAlign.HAlignLeft);
                ColVAProcessStartDateHeader++;
                ColVAProcessStartDateEnd = ColVAProcessStartDateHeader + 1;
                int ColAddress = ColVAProcessStartDateHeader;
                sheet.Range[ROW, ColVAProcessStartDateHeader, ROW, ColVAProcessStartDateEnd].Text = data.Rows[0]["TCProcessStartDate"].ToString();
                sheet.Range[ROW, ColVAProcessStartDateHeader, ROW, ColVAProcessStartDateEnd].Merge();
                sheet.Range[ROW, ColVAProcessStartDateHeader, ROW, ColVAProcessStartDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColVAProcessStartDateHeader, ROW, ColVAProcessStartDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColVAProcessStartDateEnd++;

                SetHeaderTextTop(ref sheet, ROW, ColVAProcessStartDateEnd, "Process End Date", 20, ExcelHAlign.HAlignLeft);
                ColVAProcessStartDateEnd++;
                int ColVAProcessEndDate = ColVAProcessStartDateEnd;
                int ColVAProcessEndDateEnd = ColVAProcessStartDateEnd + 1;
                sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].Text = data.Rows[0]["TCProcessEndDate"].ToString();
                sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].Merge();
                sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //  ROW++;
                ColVAProcessEndDateEnd++;

                SetHeaderTextTop(ref sheet, ROW, ColVAProcessEndDateEnd, "Contract Closing Date", 20, ExcelHAlign.HAlignLeft);
                ColVAProcessEndDateEnd++;
                int ColVAContractClosingDate = ColVAProcessEndDateEnd;
                int ColVAContractClosingDateEnd = ColVAProcessEndDateEnd + 1;
                sheet.Range[ROW, ColVAContractClosingDate, ROW, ColVAContractClosingDateEnd].Text = data.Rows[0]["TCContractClosingDate"].ToString();
                sheet.Range[ROW, ColVAContractClosingDate, ROW, ColVAContractClosingDateEnd].Merge();
                sheet.Range[ROW, ColVAContractClosingDate, ROW, ColVAContractClosingDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColVAContractClosingDate, ROW, ColVAContractClosingDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //   ROW++;
                ColVAContractClosingDateEnd++;

                SetHeaderTextTop(ref sheet, ROW, ColVAContractClosingDateEnd, "Contract Id", 20, ExcelHAlign.HAlignLeft);
                ColVAContractClosingDateEnd++;
                int ColUserContractReference = ColVAContractClosingDateEnd;
                int ColUserContractReferenceEnd = ColVAContractClosingDateEnd + 1;
                sheet.Range[ROW, ColUserContractReference, ROW, ColUserContractReferenceEnd].Text = data.Rows[0]["Id"].ToString();
                sheet.Range[ROW, ColUserContractReference, ROW, ColUserContractReferenceEnd].Merge();
                sheet.Range[ROW, ColUserContractReference, ROW, ColUserContractReferenceEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColUserContractReference, ROW, ColUserContractReferenceEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColUserContractReferenceEnd++;

                SetHeaderTextTop(ref sheet, ROW, ColUserContractReferenceEnd, "Remarks", 20, ExcelHAlign.HAlignLeft);
                ColUserContractReferenceEnd++;
                int ColRemarks = ColUserContractReferenceEnd;
                int ColRemarksEnd = ColUserContractReferenceEnd + 1;
                sheet.Range[ROW, ColRemarks, ROW, ColRemarksEnd].Text = data.Rows[0]["Remarks"].ToString();
                sheet.Range[ROW, ColRemarks, ROW, ColRemarksEnd].Merge();
                sheet.Range[ROW, ColRemarks, ROW, ColRemarksEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColRemarks, ROW, ColRemarksEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;

            }

            #region Headers
            //report.SetHeaderText(ref sheet, ROW, COL, "Job Work Item", 12, ExcelHAlign.HAlignLeft);
            //int ColJobWorkItem = COL;
            //COL++;

            //report.SetHeaderText(ref sheet, ROW, COL, "Material Location", 8, ExcelHAlign.HAlignLeft);
            //int ColMaterialLocation = COL;
            //COL++;

            //report.SetHeaderText(ref sheet, ROW, COL, "Material Type", 8, ExcelHAlign.HAlignLeft);
            //int ColMaterialType = COL;
            //COL++;


            //report.SetHeaderText(ref sheet, ROW, COL, "Final Output Category", 15, ExcelHAlign.HAlignLeft);
            //int ColFinalOutputCategory = COL;
            //COL++;

            //report.SetHeaderText(ref sheet, ROW, COL, "Material Specification", 15, ExcelHAlign.HAlignLeft);
            //int ColMaterialSpecification = COL;
            //COL++;

            //report.SetHeaderText(ref sheet, ROW, COL, "Material Reference", 20, ExcelHAlign.HAlignLeft);
            //int ColMaterialReference = COL;
            //COL++;

            //report.SetHeaderText(ref sheet, ROW, COL, "UOM", 11, ExcelHAlign.HAlignLeft);
            //int ColUOM = COL;
            //COL++;


            //report.SetHeaderText(ref sheet, ROW, COL, "Quantity", 11, ExcelHAlign.HAlignLeft);
            //int ColQuantity = COL;
            //COL++;

            //report.SetHeaderText(ref sheet, ROW, COL, "Material", 10, ExcelHAlign.HAlignLeft);
            //int ColMaterial = COL;
            //COL++;

            //report.SetHeaderText(ref sheet, ROW, COL, "Article", 10, ExcelHAlign.HAlignLeft);
            //int ColArticleCode = COL;
            //COL++;

            //report.SetHeaderText(ref sheet, ROW, COL, "Order Specific", 10, ExcelHAlign.HAlignLeft);
            //int ColOrderSpecific = COL;
            //COL++;

            //report.SetHeaderText(ref sheet, ROW, COL, "Required Capacity/ Day", 10, ExcelHAlign.HAlignLeft);
            //int ColRequiredCapacity = COL;
            //COL++;

            //report.SetHeaderText(ref sheet, ROW, COL, "Rate Applicable", 10, ExcelHAlign.HAlignLeft);
            //int ColRateApplicable = COL;
            //COL++;

            //report.SetHeaderText(ref sheet, ROW, COL, "Currency", 10, ExcelHAlign.HAlignLeft);
            //int ColCurrency = COL;
            //COL++;

            //report.SetHeaderText(ref sheet, ROW, COL, "Rate/ Unit", 8, ExcelHAlign.HAlignLeft);
            //int ColRatePerUnit = COL;
            //COL++;

            //report.SetHeaderText(ref sheet, ROW, COL, "Rejection", 10, ExcelHAlign.HAlignLeft);
            //int ColRejection = COL;
            //COL++;

            //report.SetHeaderText(ref sheet, ROW, COL, "Value Loss", 10, ExcelHAlign.HAlignLeft);
            //int ColValueLoss = COL;
            //COL++;

            //report.SetHeaderText(ref sheet, ROW, COL, "Employee Code", 10, ExcelHAlign.HAlignLeft);
            //int ColEmployeeCode = COL;
            //COL++;

            //report.SetHeaderText(ref sheet, ROW, COL, "Responsible Person", 8, ExcelHAlign.HAlignLeft);
            //int ColEmployeeName = COL;
            //COL++;

            //report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 10, ExcelHAlign.HAlignLeft);
            //int ColVCCRemarks = COL;
            //ROW++;
            //endCol = COL;

            report.SetHeaderText(ref sheet, ROW, COL, "Line Item Id", 12, ExcelHAlign.HAlignLeft);
            int ColLineItemId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Type", 12, ExcelHAlign.HAlignLeft);
            int ColOutputMaterialType = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Job Work Item", 12, ExcelHAlign.HAlignLeft);
            int ColJobWorkItem = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Material", 8, ExcelHAlign.HAlignLeft);
            int ColOutputMaterial = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Article", 8, ExcelHAlign.HAlignLeft);
            int ColArticleCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Material Location", 8, ExcelHAlign.HAlignLeft);
            int ColMaterialLocation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Input Material Category", 8, ExcelHAlign.HAlignLeft);
            int ColMaterialType = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Output Material Category", 15, ExcelHAlign.HAlignLeft);
            int ColFinalOutputCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Material Specification", 15, ExcelHAlign.HAlignLeft);
            int ColMaterialSpecification = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Material Reference", 20, ExcelHAlign.HAlignLeft);
            int ColMaterialReference = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Order Specific", 10, ExcelHAlign.HAlignLeft);
            int ColOrderSpecific = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Required Capacity/ Day", 10, ExcelHAlign.HAlignLeft);
            int ColRequiredCapacity = COL;
            COL++;

            //report.SetHeaderText(ref sheet, ROW, COL, "By Product Applicable", 10, ExcelHAlign.HAlignLeft);
            //int ColByProductApplicable = COL;
            //COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "UOM", 11, ExcelHAlign.HAlignLeft);
            int ColUOM = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Total Output Quantity", 11, ExcelHAlign.HAlignLeft);
            int ColQuantity = COL;
            COL++;

            //report.SetHeaderText(ref sheet, ROW, COL, "Total Gross Input Quantity", 12, ExcelHAlign.HAlignLeft);
            //int ColTotalGrossInputQuantity = COL;
            //COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Rate Applicable", 10, ExcelHAlign.HAlignLeft);
            int ColRateApplicable = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Currency", 10, ExcelHAlign.HAlignLeft);
            int ColCurrency = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Rate/ Unit", 8, ExcelHAlign.HAlignLeft);
            int ColRatePerUnit = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Amount", 12, ExcelHAlign.HAlignLeft);
            int ColAmount = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Rejection", 10, ExcelHAlign.HAlignLeft);
            int ColRejection = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Value Loss", 10, ExcelHAlign.HAlignLeft);
            int ColValueLoss = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Code", 10, ExcelHAlign.HAlignLeft);
            int ColEmployeeCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Responsible Person", 8, ExcelHAlign.HAlignLeft);
            int ColEmployeeName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 10, ExcelHAlign.HAlignLeft);
            int ColVCCRemarks = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Base UoM", 10, ExcelHAlign.HAlignLeft);
            int ColIssueBaseUoM = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Issue Qty", 8, ExcelHAlign.HAlignLeft);
            int ColIssuedQty = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Amount BC", 10, ExcelHAlign.HAlignLeft);
            int ColAmtBDT = COL;
            ROW++;

            endCol = COL;
            #endregion Headers


            //DataView dvOperationGrup = new DataView(data);

            //Dictionary<string, double> dist = new Dictionary<string, double>();

            //DataTable dtOperationGroup = dvOperationGrup.ToTable(true, "OperationGroup");

            string JobWorkItems = "";
            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < data.Rows.Count; i++)
            {

                if (JobWorkItems != data.Rows[i]["JobWorkItem"].ToString())
                {

                    if (RowIndex < ROW)
                    {
                        //sheet.Range[RowIndex, ColJobWorkItem, ROW - 1, ColJobWorkItem].Merge();
                        sheet.Range[RowIndex, ColJobWorkItem, ROW - 1, ColJobWorkItem].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[RowIndex, ColJobWorkItem, ROW - 1, ColJobWorkItem].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    }
                    RowIndex = ROW;
                }

                //sheet[ROW, ColMaterialType].Text = data.Rows[i]["MaterialType"].ToString();
                //sheet[ROW, ColJobWorkItem].Text = data.Rows[i]["JobWorkItem"].ToString();
                //sheet[ROW, ColMaterialLocation].Text = data.Rows[i]["MaterialLocation"].ToString();
                //sheet[ROW, ColFinalOutputCategory].Text = data.Rows[i]["FinalOutputCategory"].ToString();
                //sheet[ROW, ColMaterialSpecification].Text = data.Rows[i]["MaterialSpecification"].ToString();
                //sheet[ROW, ColMaterialReference].Text = data.Rows[i]["MaterialReference"].ToString();

                //sheet[ROW, ColUOM].Text = data.Rows[i]["UOM"].ToString();
                //sheet[ROW, ColQuantity].Text = data.Rows[i]["Quantity"].ToString();

                ////sheet[ROW, ColMaterial].Text = data.Rows[i]["Material"].ToString();
                ////sheet[ROW, ColArticleCode].Text = data.Rows[i]["Article"].ToString();

                //sheet[ROW, ColOutputMaterial].Text = data.Rows[i]["OutputMaterial"].ToString();
                //sheet[ROW, ColArticleCode].Text = data.Rows[i]["ArticleCode"].ToString();

                //sheet[ROW, ColOrderSpecific].Text = data.Rows[i]["OrderSpecific"].ToString();
                //sheet[ROW, ColRequiredCapacity].Number = clsStaticInfo.dbl(data.Rows[i]["ReqCapacity"].ToString());
                //sheet[ROW, ColRateApplicable].Text = data.Rows[i]["RateApplicable"].ToString();
                //sheet[ROW, ColCurrency].Text = data.Rows[i]["Currency"].ToString();

                //sheet[ROW, ColRatePerUnit].Number = clsStaticInfo.dbl(data.Rows[i]["RatePerUnit"].ToString());
                //sheet[ROW, ColRejection].Number = clsStaticInfo.dbl(data.Rows[i]["VccRejection"].ToString());
                //sheet[ROW, ColValueLoss].Number = clsStaticInfo.dbl(data.Rows[i]["ValueLoss"].ToString());
                //sheet[ROW, ColEmployeeCode].Text = data.Rows[i]["EmployeeCode"].ToString();
                //sheet[ROW, ColEmployeeName].Text = data.Rows[i]["EmployeeName"].ToString();
                //sheet[ROW, ColVCCRemarks].Text = data.Rows[i]["VCCRemarks"].ToString();

                sheet[ROW, ColLineItemId].Text = data.Rows[i]["LineItemId"].ToString();
                sheet[ROW, ColMaterialType].Text = data.Rows[i]["MaterialType"].ToString();
                sheet[ROW, ColJobWorkItem].Text = data.Rows[i]["JobWorkItem"].ToString();
                sheet[ROW, ColMaterialLocation].Text = data.Rows[i]["MaterialLocation"].ToString();
                sheet[ROW, ColFinalOutputCategory].Text = data.Rows[i]["FinalOutputCategory"].ToString();
                sheet[ROW, ColMaterialSpecification].Text = data.Rows[i]["MaterialSpecification"].ToString();
                sheet[ROW, ColMaterialReference].Text = data.Rows[i]["MaterialReference"].ToString();

                sheet[ROW, ColUOM].Text = data.Rows[i]["UOM"].ToString();
                sheet[ROW, ColQuantity].Text = data.Rows[i]["Quantity"].ToString();
                //sheet[ROW, ColTotalGrossInputQuantity].Number = clsStaticInfo.dbl(data.Rows[i]["TotalGrossInputQuantity"].ToString());
             //   sheet[ROW, ColTotalGrossInputQuantity].Number = clsStaticInfo.dbl(data.Rows[i]["TotalGrossConsumptionPerUnit"].ToString());

                sheet[ROW, ColOutputMaterial].Text = data.Rows[i]["OutputMaterial"].ToString();

                sheet[ROW, ColArticleCode].Text = data.Rows[i]["ArticleCode"].ToString();

                sheet[ROW, ColOrderSpecific].Text = data.Rows[i]["OrderSpecific"].ToString();
                sheet[ROW, ColRequiredCapacity].Number = clsStaticInfo.dbl(data.Rows[i]["RequiredCapacity"].ToString());
          //      sheet[ROW, ColByProductApplicable].Text = data.Rows[i]["ByProductApplicable"].ToString();
                sheet[ROW, ColRateApplicable].Text = data.Rows[i]["RateApplyId"].ToString();
                sheet[ROW, ColCurrency].Text = data.Rows[i]["Currency"].ToString();

              //  sheet[ROW, ColRatePerUnit].Number = clsStaticInfo.dbl(data.Rows[i]["RatePerUnit"].ToString());
                sheet[ROW, ColRatePerUnit].Text = data.Rows[i]["RatePerUnit"].ToString();
                //sheet[ROW, ColAmount].Text = data.Rows[i]["Amount"].ToString();
                sheet[ROW, ColAmount].Number = Math.Round(clsStaticInfo.dbl(data.Rows[i]["Amount"].ToString()),2);
                sheet[ROW, ColRejection].Number = clsStaticInfo.dbl(data.Rows[i]["Rejection"].ToString());
                sheet[ROW, ColValueLoss].Number = clsStaticInfo.dbl(data.Rows[i]["ValueLoss"].ToString());
                sheet[ROW, ColEmployeeCode].Text = data.Rows[i]["EmployeeCode"].ToString();
                sheet[ROW, ColEmployeeName].Text = data.Rows[i]["EmployeeName"].ToString();
                sheet[ROW, ColVCCRemarks].Text = data.Rows[i]["TCCRemarks"].ToString();
                sheet[ROW, ColOutputMaterialType].Text = data.Rows[i]["OutputMaterialType"].ToString();

                sheet[ROW, ColIssueBaseUoM].Text = data.Rows[i]["IssueBaseUoM"].ToString();
                sheet[ROW, ColIssuedQty].Number = Math.Round(clsStaticInfo.dbl(data.Rows[i]["IssuedQty"].ToString()), 2);
                sheet[ROW, ColAmtBDT].Number = Math.Round(clsStaticInfo.dbl(data.Rows[i]["AmtBDT"].ToString()), 2);

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                JobWorkItems = data.Rows[i]["JobWorkItem"].ToString();

                ROW++;
            }

            endRow = ROW - 1;

            if (RowIndex < ROW - 1)
            {
                //sheet.Range[RowIndex, ColJobWorkItem, ROW - 1, ColJobWorkItem].Merge();
                sheet.Range[RowIndex, ColJobWorkItem, ROW - 1, ColJobWorkItem].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColJobWorkItem, ROW - 1, ColJobWorkItem].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            }

            // ORDER WISE REQUIREMENT

            int MPChildROW = ROW + 1;
            int MPChildendCol = 1;
            int MPChildCOL = 1;

            #region Material Planning Child Headers

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Order Wise Requirement", 12, ExcelHAlign.HAlignLeft);
            MPChildROW++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Order Type", 12, ExcelHAlign.HAlignLeft);
            int ColOrderType = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Master Order No", 8, ExcelHAlign.HAlignLeft);
            int ColMasterOrderNo = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Material Order Item", 12, ExcelHAlign.HAlignLeft);
            int ColMaterialOrderItem = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Particular Specification", 8, ExcelHAlign.HAlignLeft);
            int ColParticularSpecification = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "UOM", 12, ExcelHAlign.HAlignLeft);
            int ColMPCUOM = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Quantity", 8, ExcelHAlign.HAlignLeft);
            int ColMPCQuantity = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Plan Quantity", 12, ExcelHAlign.HAlignLeft);
            int ColPlanQuantity = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Customer", 8, ExcelHAlign.HAlignLeft);
            int ColCustomer = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Remarks", 10, ExcelHAlign.HAlignLeft);
            int ColMPCRemarks = MPChildCOL;
            MPChildROW++;
            MPChildendCol = MPChildCOL;
            #endregion Headers

            string OrderTpe = "";
            var StartRows = 0;
            var EndRows = 0;
            int RowIndexNo = MPChildROW;
            StartRows = MPChildROW;

            for (int i = 0; i < MaterialPlanningChilddata.Rows.Count; i++)
            {

                if (OrderTpe != MaterialPlanningChilddata.Rows[i]["OrderType"].ToString())
                {

                    if (RowIndexNo < MPChildROW)
                    {
                        //sheet.Range[RowIndexNo, ColOrderType, MPChildROW - 1, ColOrderType].Merge();
                        sheet.Range[RowIndexNo, ColOrderType, MPChildROW - 1, ColOrderType].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[RowIndexNo, ColOrderType, MPChildROW - 1, ColOrderType].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    }
                    RowIndexNo = MPChildROW;
                }

                sheet[MPChildROW, ColMasterOrderNo].Text = MaterialPlanningChilddata.Rows[i]["MasterOrderNo"].ToString();
                sheet[MPChildROW, ColOrderType].Text = MaterialPlanningChilddata.Rows[i]["OrderType"].ToString();
                sheet[MPChildROW, ColMaterialOrderItem].Text = MaterialPlanningChilddata.Rows[i]["MaterialOrderItem"].ToString();
                sheet[MPChildROW, ColParticularSpecification].Text = MaterialPlanningChilddata.Rows[i]["ParticularSpecification"].ToString();
                sheet[MPChildROW, ColMPCUOM].Text = MaterialPlanningChilddata.Rows[i]["UOM"].ToString();
                sheet[MPChildROW, ColMPCQuantity].Number = clsStaticInfo.dbl(MaterialPlanningChilddata.Rows[i]["Quantity"].ToString());
                sheet[MPChildROW, ColPlanQuantity].Number = clsStaticInfo.dbl(MaterialPlanningChilddata.Rows[i]["PlanQuantity"].ToString());
                sheet[MPChildROW, ColCustomer].Text = MaterialPlanningChilddata.Rows[i]["Customer"].ToString();
                sheet[MPChildROW, ColMPCRemarks].Text = MaterialPlanningChilddata.Rows[i]["Remarks"].ToString();

                sheet.Range[MPChildROW, 1, MPChildROW, MPChildendCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[MPChildROW, 1, MPChildROW, MPChildendCol].BorderAround(ExcelLineStyle.Hair);
                OrderTpe = MaterialPlanningChilddata.Rows[i]["OrderType"].ToString();

                MPChildROW++;
            }

            EndRows = MPChildROW - 1;

            if (RowIndexNo < MPChildROW - 1)
            {
                //sheet.Range[RowIndexNo, ColOrderType, MPChildROW - 1, ColOrderType].Merge();
                sheet.Range[RowIndexNo, ColOrderType, MPChildROW - 1, ColOrderType].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndexNo, ColOrderType, MPChildROW - 1, ColOrderType].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            }

            //GetWorkSheetBulletinTamplateCalculation(ref sheet1, ref report, data, "Bulletin Tamplate Calculation");
            //GetWorkSheetTamplateFormula(ref sheet2, ref report, data, "Bulletin Tamplate Calculation Formula");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.000";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyPlantHeader(ref sheet, endCol, "Job Work PO (Value Added)", identity.CompanyId, identity.PlantName, null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }

        private DataTable GetContractReportDataById(string PrintTabId)
        {

            var sql = @"select tc.*,tcc.Id as LineItemId, TabType='Value Added',OutputMaterialType='Service',FORMAT(tc.PODate,'dd-MMM-yyyy') as TransformationDate,FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as TCProcessStartDate,
                                    FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as TCProcessEndDate,FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as TCContractClosingDate,
                                    Pnt.UserName as Plant,e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName,jwi.UserName as JobWorkItem, tcc.MaterialSpecification, tcc.MaterialReference
									--,uom.UserName as UOM
									,UOM=case when tc.OrderSpecific='Yes' then mmuom.UserName else uom.UserName End, tcc.Quantity,mm.UserName as OutputMaterial
									, mma.StandardName as ArticleCode, tcc.OrderSpecific as OutputOrderSpecific, tcc.RequiredCapacity,tcc.ByProductApplicable ,tcc.RateApplyId--, c.Code as Currency
                                    ,Currency=case when tcc.CurrencyId is not null then c.Code else cc.Code End
									,RatePerUnit=ROUND((tcc.RatePerUnit),4), tcc.Rejection,tcc.ValueLoss,emp.EmployeeName,emp.EmployeeCode,tcc.Remarks as TCCRemarks,MS.UserName as MaterialLocation,tcc.MaterialType,tcc.FinalOutputCategory
								--	, mi.TotalGrossConsumptionPerUnit, TotalGrossInputQuantity=(mi.TotalGrossConsumptionPerUnit * tcc.Quantity)
									--, Amount= case when tcc.RateApplyId='Output' then (tcc.Quantity * tcc.RatePerUnit) else ((mi.TotalGrossConsumptionPerUnit * tcc.Quantity) * tcc.RatePerUnit) End
                                    , Amount= round((tcc.Quantity * tcc.RatePerUnit),2)
                                    ,round(ISNULL(BA.IssuedQty,'0'),2) IssuedQty, round(ISNULL(BA.AmountBDT,'0'),2) AmtBDT, BA.BaseUoM as IssueBaseUoM
                                    from dbo.JWTransformationPO tc left join ORG.Entity e on e.Id=tc.EntityId
									left join ORG.Plant Pnt on Pnt.Id=tc.PlantId
									left join HKP.Party p on p.Id=tc.PartyId
									left join dbo.JWTransformationPODetail tcc on tcc.JWTransformationPOId=tc.Id
									left join HKP.JobWorkItem jwi on jwi.Id=tcc.JobWorkItemMasterId
									left join SCS.UnitOfMeasurement uom on uom.Id=tcc.OutputMaterialUOMId
									left join MST.MaterialMasterArticle mma on mma.Id=tcc.ArticleId
									left join MST.MaterialMaster mm on mm.Id=tcc.MaterialMasterId
									left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
									left join SCS.Currency c on c.Id=tcc.CurrencyId
                                    left join SCS.Currency cc on cc.Id=tc.CurrencyId
									left join dbo.EmployeeInformation emp on emp.SystemId=tcc.ResponsiblePersonId
									left join HKP.MaterialStorage MS on MS.Id=tcc.MaterialLocationId
                                    left join (select SUM(IIH.Qty) as IssuedQty, Sum(IIH.TotalMaterialBooksCurrencyAmount) AmountBDT, IM.ArticleId, IID.InventoryMaterialId
													,BUom.UserName as BaseUoM
													from TRN.InventoryIssueDetail IID left join TRN.InventoryIssueHistory IIH on IIH.InventoryIssueDetailId=IID.Id
													left join TRN.InventoryMaterial IM on IM.Id=IID.InventoryMaterialId
													left join SCS.UnitOfMeasurement BUom on BUom.Id=IID.BaseUOMId
                                                    left join TRN.InventoryIssue II on II.Id=IID.InventoryIssueId
													where II.JobWorkContractId='" + PrintTabId + @"'
													group by IM.ArticleId,IID.InventoryMaterialId,BUom.UserName) BA on BA.ArticleId=tcc.ArticleId
                                    where tc.Id = '" + PrintTabId + @"' ";

            return _sqlRepository.GetDataTable(sql);
        }

        private DataTable GetMaterialPlanningChildReportDataById(string PrintTabId)
        {
            var sql = @"select owr.*,P.UserName as Customer,mo.MasterOrderNo,mm.UserName as MaterialOrderItem, uom.UserName as UOM 
                                                    from dbo.JWTransformationPOMasterOrderItem owr left join HKP.Party P on P.Id=owr.CustomerId
                                                    left join TRN.MasterOrder mo on mo.Id=owr.MasterOrderNoId												
													left join TRN.MasterOrderItem moi on moi.Id=owr.MasterOrderItemId
													left join MST.MaterialMaster mm on mm.Id=moi.MaterialMasterId
													left join SCS.UnitOfMeasurement uom on uom.Id=owr.OutputMaterialUOMId
													left join dbo.JWTransformationPODetail mp on mp.Id=owr.JWTransformationPODetailId
													left join dbo.JWTransformationPO vac on vac.Id=mp.JWTransformationPOId
													where vac.Id='" + PrintTabId + "' ";

            return _sqlRepository.GetDataTable(sql);
        }

        #endregion end Reports for Value Added Contract

        #region Reports for Transformation Contract

        [HttpGet, Authorize]
        public ActionResult GetTransformationContractReport(ReportFormat reportFormat, string PrintTabId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var reportFileName = " Transformation Contract " + PrintTabId + "";
            var workbook = GetTransformationContractReportWorkSheet(PrintTabId);
            switch (reportFormat)
            {
                case ReportFormat.Pdf:
                    return RenderReportAsPdf(workbook, reportFileName);

                case ReportFormat.Excel:
                    return RenderReportAsExcel(workbook, reportFileName);

                default:
                    return RenderReportAsExcel(workbook, reportFileName);
            }
        }


        private IWorkbook GetTransformationContractReportWorkSheet(string PrintTabId)
        {

            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];
            //var sheet1 = workbook.Worksheets[1];
            //var sheet2 = workbook.Worksheets[2];

            sheet.Name = "TransformationContract";


            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            DataTable data = GetTransformationContractDataById(PrintTabId);
            DataTable MaterialPlanningChilddata = GetMatPlanningChildDataById(PrintTabId);
            DataTable MaterialInputChilddata = GetMaterialInputChildDataById(PrintTabId);
            DataTable ByProductChilddata = GetByProductChildDataById(PrintTabId);

            if (data.Rows.Count > 0)
            {
                int ColValueAddedDateHeader = 1;
                int ColValueAddedDateEnd;
                int ColVACTimeHeader;
                int ColVACTimeEnd;
                int ColVACTimeName;
                int ColPlantHeader;
                int ColPlantEnd;
                int ColPlantName;
                int ColEntityHeader;
                int ColEntityEnd;
                int ColEntityName;
                int ColPartyNameHeader;
                int ColPartyNameEnd;
                int ColPartyNameName;
                int ColVAProcessStartDateHeader = 1;
                int ColVAProcessStartDateEnd;


                SetHeaderTextTop(ref sheet, ROW, ColValueAddedDateHeader, "Date", 12, ExcelHAlign.HAlignLeft);
                ColValueAddedDateHeader++;
                ColValueAddedDateEnd = ColValueAddedDateHeader + 1;
                sheet.Range[ROW, ColValueAddedDateHeader, ROW, ColValueAddedDateEnd].Text = data.Rows[0]["TransformationDate"].ToString();
                sheet.Range[ROW, ColValueAddedDateHeader, ROW, ColValueAddedDateEnd].Merge();
                sheet.Range[ROW, ColValueAddedDateHeader, ROW, ColValueAddedDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColValueAddedDateHeader, ROW, ColValueAddedDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColValueAddedDateEnd++;

                //ColVACTimeHeader = ColValueAddedDateEnd;
                //SetHeaderTextTop(ref sheet, ROW, ColVACTimeHeader, "Time", 20, ExcelHAlign.HAlignLeft);
                //ColVACTimeHeader++;
                //ColVACTimeEnd = ColVACTimeHeader + 1;
                //ColVACTimeName = ColVACTimeHeader;
                //sheet.Range[ROW, ColVACTimeName, ROW, ColVACTimeEnd].Text = data.Rows[0]["TCTime"].ToString();
                //sheet.Range[ROW, ColVACTimeName, ROW, ColVACTimeEnd].Merge();
                //sheet.Range[ROW, ColVACTimeName, ROW, ColVACTimeEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //sheet.Range[ROW, ColVACTimeName, ROW, ColVACTimeEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ////            ROW++;
                //ColVACTimeEnd++;

                ColPlantHeader = ColValueAddedDateEnd;
                SetHeaderTextTop(ref sheet, ROW, ColPlantHeader, "Plant", 20, ExcelHAlign.HAlignLeft);
                ColPlantHeader++;
                ColPlantEnd = ColPlantHeader + 1;
                ColPlantName = ColPlantHeader;
                sheet.Range[ROW, ColPlantName, ROW, ColPlantEnd].Text = data.Rows[0]["Plant"].ToString();
                sheet.Range[ROW, ColPlantName, ROW, ColPlantEnd].Merge();
                sheet.Range[ROW, ColPlantName, ROW, ColPlantEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColPlantName, ROW, ColPlantEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //            ROW++;
                ColPlantEnd++;

                ColEntityHeader = ColPlantEnd;
                SetHeaderTextTop(ref sheet, ROW, ColEntityHeader, "Entity", 20, ExcelHAlign.HAlignLeft);
                ColEntityHeader++;
                ColEntityEnd = ColEntityHeader + 1;
                ColEntityName = ColEntityHeader;
                sheet.Range[ROW, ColEntityName, ROW, ColEntityEnd].Text = data.Rows[0]["Entity"].ToString();
                sheet.Range[ROW, ColEntityName, ROW, ColEntityEnd].Merge();
                sheet.Range[ROW, ColEntityName, ROW, ColEntityEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColEntityName, ROW, ColEntityEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //           ROW++;
                ColEntityEnd++;

                ColPartyNameHeader = ColEntityEnd;
                SetHeaderTextTop(ref sheet, ROW, ColPartyNameHeader, "PartyName", 20, ExcelHAlign.HAlignLeft);
                ColPartyNameHeader++;
                ColPartyNameEnd = ColPartyNameHeader + 1;
                ColPartyNameName = ColPartyNameHeader;
                sheet.Range[ROW, ColPartyNameName, ROW, ColPartyNameEnd].Text = data.Rows[0]["PartyName"].ToString();
                sheet.Range[ROW, ColPartyNameName, ROW, ColPartyNameEnd].Merge();
                sheet.Range[ROW, ColPartyNameName, ROW, ColPartyNameEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColPartyNameName, ROW, ColPartyNameEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;
                //          ColPartyNameEnd++;


                SetHeaderTextTop(ref sheet, ROW, ColVAProcessStartDateHeader, "Process Start Date", 12, ExcelHAlign.HAlignLeft);
                ColVAProcessStartDateHeader++;
                ColVAProcessStartDateEnd = ColVAProcessStartDateHeader + 1;
                int ColAddress = ColVAProcessStartDateHeader;
                sheet.Range[ROW, ColVAProcessStartDateHeader, ROW, ColVAProcessStartDateEnd].Text = data.Rows[0]["TCProcessStartDate"].ToString();
                sheet.Range[ROW, ColVAProcessStartDateHeader, ROW, ColVAProcessStartDateEnd].Merge();
                sheet.Range[ROW, ColVAProcessStartDateHeader, ROW, ColVAProcessStartDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColVAProcessStartDateHeader, ROW, ColVAProcessStartDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ColVAProcessStartDateEnd++;

                SetHeaderTextTop(ref sheet, ROW, ColVAProcessStartDateEnd, "Process End Date", 20, ExcelHAlign.HAlignLeft);
                ColVAProcessStartDateEnd++;
                int ColVAProcessEndDate = ColVAProcessStartDateEnd;
                int ColVAProcessEndDateEnd = ColVAProcessStartDateEnd + 1;
                sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].Text = data.Rows[0]["TCProcessEndDate"].ToString();
                sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].Merge();
                sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColVAProcessEndDate, ROW, ColVAProcessEndDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //  ROW++;
                ColVAProcessEndDateEnd++;

                SetHeaderTextTop(ref sheet, ROW, ColVAProcessEndDateEnd, "Contract Closing Date", 20, ExcelHAlign.HAlignLeft);
                ColVAProcessEndDateEnd++;
                int ColVAContractClosingDate = ColVAProcessEndDateEnd;
                int ColVAContractClosingDateEnd = ColVAProcessEndDateEnd + 1;
                sheet.Range[ROW, ColVAContractClosingDate, ROW, ColVAContractClosingDateEnd].Text = data.Rows[0]["TCContractClosingDate"].ToString();
                sheet.Range[ROW, ColVAContractClosingDate, ROW, ColVAContractClosingDateEnd].Merge();
                sheet.Range[ROW, ColVAContractClosingDate, ROW, ColVAContractClosingDateEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColVAContractClosingDate, ROW, ColVAContractClosingDateEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //   ROW++;
                ColVAContractClosingDateEnd++;

                SetHeaderTextTop(ref sheet, ROW, ColVAContractClosingDateEnd, "Contract Id", 20, ExcelHAlign.HAlignLeft);
                ColVAContractClosingDateEnd++;
                int ColTransContractId = ColVAContractClosingDateEnd;
                int ColTransContractIdEnd = ColVAContractClosingDateEnd + 1;
                sheet.Range[ROW, ColTransContractId, ROW, ColTransContractIdEnd].Text = data.Rows[0]["Id"].ToString();
                sheet.Range[ROW, ColTransContractId, ROW, ColTransContractIdEnd].Merge();
                sheet.Range[ROW, ColTransContractId, ROW, ColTransContractIdEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColTransContractId, ROW, ColTransContractIdEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //   ROW++;
                ColTransContractIdEnd++;

                SetHeaderTextTop(ref sheet, ROW, ColTransContractIdEnd, "Remarks", 20, ExcelHAlign.HAlignLeft);
                ColTransContractIdEnd++;
                int ColRemarks = ColTransContractIdEnd;
                int ColRemarksEnd = ColTransContractIdEnd + 1;
                sheet.Range[ROW, ColRemarks, ROW, ColRemarksEnd].Text = data.Rows[0]["Remarks"].ToString();
                sheet.Range[ROW, ColRemarks, ROW, ColRemarksEnd].Merge();
                sheet.Range[ROW, ColRemarks, ROW, ColRemarksEnd].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[ROW, ColRemarks, ROW, ColRemarksEnd].VerticalAlignment = ExcelVAlign.VAlignCenter;
                ROW++;

            }

            #region Headers
            report.SetHeaderText(ref sheet, ROW, COL, "Line Item Id", 12, ExcelHAlign.HAlignLeft);
            int ColLineItemId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Type", 12, ExcelHAlign.HAlignLeft);
            int ColOutputMaterialType = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Job Work Item", 12, ExcelHAlign.HAlignLeft);
            int ColJobWorkItem = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Material", 8, ExcelHAlign.HAlignLeft);
            int ColOutputMaterial = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Article", 8, ExcelHAlign.HAlignLeft);
            int ColArticleCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Material Location", 8, ExcelHAlign.HAlignLeft);
            int ColMaterialLocation = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Input Material Category", 8, ExcelHAlign.HAlignLeft);
            int ColMaterialType = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Output Material Category", 15, ExcelHAlign.HAlignLeft);
            int ColFinalOutputCategory = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Material Specification", 15, ExcelHAlign.HAlignLeft);
            int ColMaterialSpecification = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Material Reference", 20, ExcelHAlign.HAlignLeft);
            int ColMaterialReference = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Order Specific", 10, ExcelHAlign.HAlignLeft);
            int ColOrderSpecific = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Required Capacity/ Day", 10, ExcelHAlign.HAlignLeft);
            int ColRequiredCapacity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "By Product Applicable", 10, ExcelHAlign.HAlignLeft);
            int ColByProductApplicable = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "UOM", 11, ExcelHAlign.HAlignLeft);
            int ColUOM = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Total Output Quantity", 11, ExcelHAlign.HAlignLeft);
            int ColQuantity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Total Gross Input Quantity", 12, ExcelHAlign.HAlignLeft);
            int ColTotalGrossInputQuantity = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Rate Applicable", 10, ExcelHAlign.HAlignLeft);
            int ColRateApplicable = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Currency", 10, ExcelHAlign.HAlignLeft);
            int ColCurrency = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Rate/ Unit", 8, ExcelHAlign.HAlignLeft);
            int ColRatePerUnit = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Amount", 8, ExcelHAlign.HAlignLeft);
            int ColAmount = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Rejection", 10, ExcelHAlign.HAlignLeft);
            int ColRejection = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Value Loss", 10, ExcelHAlign.HAlignLeft);
            int ColValueLoss = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Code", 10, ExcelHAlign.HAlignLeft);
            int ColEmployeeCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Responsible Person", 8, ExcelHAlign.HAlignLeft);
            int ColEmployeeName = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Remarks", 10, ExcelHAlign.HAlignLeft);
            int ColVCCRemarks = COL;
            ROW++;
            endCol = COL;
            #endregion Headers


            //DataView dvOperationGrup = new DataView(data);

            //Dictionary<string, double> dist = new Dictionary<string, double>();

            //DataTable dtOperationGroup = dvOperationGrup.ToTable(true, "OperationGroup");

            string JobWorkItems = "";
            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < data.Rows.Count; i++)
            {

                if (JobWorkItems != data.Rows[i]["JobWorkItem"].ToString())
                {

                    if (RowIndex < ROW)
                    {
                        //sheet.Range[RowIndex, ColJobWorkItem, ROW - 1, ColJobWorkItem].Merge();
                        sheet.Range[RowIndex, ColJobWorkItem, ROW - 1, ColJobWorkItem].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[RowIndex, ColJobWorkItem, ROW - 1, ColJobWorkItem].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    }
                    RowIndex = ROW;
                }
                sheet[ROW, ColLineItemId].Text = data.Rows[i]["LineItemId"].ToString();
                sheet[ROW, ColMaterialType].Text = data.Rows[i]["MaterialType"].ToString();
                sheet[ROW, ColJobWorkItem].Text = data.Rows[i]["JobWorkItem"].ToString();
                sheet[ROW, ColMaterialLocation].Text = data.Rows[i]["MaterialLocation"].ToString();
                sheet[ROW, ColFinalOutputCategory].Text = data.Rows[i]["FinalOutputCategory"].ToString();
                sheet[ROW, ColMaterialSpecification].Text = data.Rows[i]["MaterialSpecification"].ToString();
                sheet[ROW, ColMaterialReference].Text = data.Rows[i]["MaterialReference"].ToString();

                sheet[ROW, ColUOM].Text = data.Rows[i]["UOM"].ToString();
                sheet[ROW, ColQuantity].Text = data.Rows[i]["Quantity"].ToString();
                //sheet[ROW, ColTotalGrossInputQuantity].Number = clsStaticInfo.dbl(data.Rows[i]["TotalGrossInputQuantity"].ToString());
                sheet[ROW, ColTotalGrossInputQuantity].Number = clsStaticInfo.dbl(data.Rows[i]["TotalGrossConsumptionPerUnit"].ToString());

                sheet[ROW, ColOutputMaterial].Text = data.Rows[i]["OutputMaterial"].ToString();

                sheet[ROW, ColArticleCode].Text = data.Rows[i]["ArticleCode"].ToString();

                sheet[ROW, ColOrderSpecific].Text = data.Rows[i]["OutputOrderSpecific"].ToString();
                sheet[ROW, ColRequiredCapacity].Number = clsStaticInfo.dbl(data.Rows[i]["RequiredCapacity"].ToString());
                sheet[ROW, ColByProductApplicable].Text = data.Rows[i]["ByProductApplicable"].ToString();
                sheet[ROW, ColRateApplicable].Text = data.Rows[i]["RateApplyId"].ToString();
                sheet[ROW, ColCurrency].Text = data.Rows[i]["Currency"].ToString();

                sheet[ROW, ColRatePerUnit].Number = clsStaticInfo.dbl(data.Rows[i]["RatePerUnit"].ToString());
                sheet[ROW, ColAmount].Text = data.Rows[i]["Amount"].ToString();
                sheet[ROW, ColRejection].Number = clsStaticInfo.dbl(data.Rows[i]["Rejection"].ToString());
                sheet[ROW, ColValueLoss].Number = clsStaticInfo.dbl(data.Rows[i]["ValueLoss"].ToString());
                sheet[ROW, ColEmployeeCode].Text = data.Rows[i]["EmployeeCode"].ToString();
                sheet[ROW, ColEmployeeName].Text = data.Rows[i]["EmployeeName"].ToString();
                sheet[ROW, ColVCCRemarks].Text = data.Rows[i]["TCCRemarks"].ToString();
                sheet[ROW, ColOutputMaterialType].Text = data.Rows[i]["OutputMaterialType"].ToString();

                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                JobWorkItems = data.Rows[i]["JobWorkItem"].ToString();

                ROW++;
            }

            endRow = ROW - 1;

            if (RowIndex < ROW - 1)
            {
                //sheet.Range[RowIndex, ColJobWorkItem, ROW - 1, ColJobWorkItem].Merge();
                sheet.Range[RowIndex, ColJobWorkItem, ROW - 1, ColJobWorkItem].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndex, ColJobWorkItem, ROW - 1, ColJobWorkItem].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            }

            // MATERIAL PLANNING CHILD

            int MPChildROW = ROW + 1;
            int MPChildendCol = 1;
            int MPChildCOL = 1;

            #region Material Planning Child Headers

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Order Wise Requirement", 12, ExcelHAlign.HAlignLeft);
     //       int ColOrderType = MPChildCOL;
            MPChildROW++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Order Type", 12, ExcelHAlign.HAlignLeft);
            int ColOrderType = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Customer", 15, ExcelHAlign.HAlignLeft);
            int ColCustomer = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Buyer", 15, ExcelHAlign.HAlignLeft);
            int ColBuyer = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Sales Order No", 15, ExcelHAlign.HAlignLeft);
            int ColSalesOrderId = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Master Order No", 10, ExcelHAlign.HAlignLeft);
            int ColMasterOrderNo = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Material Order Item", 12, ExcelHAlign.HAlignLeft);
            int ColMaterialOrderItem = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Particular Specification", 12, ExcelHAlign.HAlignLeft);
            int ColParticularSpecification = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "UOM", 10, ExcelHAlign.HAlignLeft);
            int ColMPCUOM = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Quantity", 10, ExcelHAlign.HAlignLeft);
            int ColMPCQuantity = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Plan Quantity", 10, ExcelHAlign.HAlignLeft);
            int ColPlanQuantity = MPChildCOL;
            MPChildCOL++;

            report.SetHeaderText(ref sheet, MPChildROW, MPChildCOL, "Remarks", 10, ExcelHAlign.HAlignLeft);
            int ColMPCRemarks = MPChildCOL;
            MPChildROW++;
            MPChildendCol = MPChildCOL;
            #endregion Headers

            string OrderTpe = "";
            var StartRows = 0;
            var EndRows = 0;
            int RowIndexNo = MPChildROW;
            StartRows = MPChildROW;

            for (int i = 0; i < MaterialPlanningChilddata.Rows.Count; i++)
            {

                if (OrderTpe != MaterialPlanningChilddata.Rows[i]["OrderType"].ToString())
                {

                    if (RowIndexNo < MPChildROW)
                    {
                        //sheet.Range[RowIndexNo, ColOrderType, MPChildROW - 1, ColOrderType].Merge();
                        sheet.Range[RowIndexNo, ColOrderType, MPChildROW - 1, ColOrderType].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[RowIndexNo, ColOrderType, MPChildROW - 1, ColOrderType].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    }
                    RowIndexNo = MPChildROW;
                }

                sheet[MPChildROW, ColMasterOrderNo].Text = MaterialPlanningChilddata.Rows[i]["MasterOrderNo"].ToString();
                sheet[MPChildROW, ColOrderType].Text = MaterialPlanningChilddata.Rows[i]["OrderType"].ToString();
                sheet[MPChildROW, ColMaterialOrderItem].Text = MaterialPlanningChilddata.Rows[i]["MaterialOrderItemId"].ToString();
                sheet[MPChildROW, ColParticularSpecification].Text = MaterialPlanningChilddata.Rows[i]["ParticularSpecification"].ToString();
                sheet[MPChildROW, ColMPCUOM].Text = MaterialPlanningChilddata.Rows[i]["MasterOrderUoM"].ToString();
                sheet[MPChildROW, ColMPCQuantity].Number = clsStaticInfo.dbl(MaterialPlanningChilddata.Rows[i]["Qty"].ToString());
                sheet[MPChildROW, ColPlanQuantity].Number = clsStaticInfo.dbl(MaterialPlanningChilddata.Rows[i]["OWPlanQuantity"].ToString());
                sheet[MPChildROW, ColCustomer].Text = MaterialPlanningChilddata.Rows[i]["Customer"].ToString();
                sheet[MPChildROW, ColMPCRemarks].Text = MaterialPlanningChilddata.Rows[i]["OWRemarks"].ToString();

                sheet[MPChildROW, ColBuyer].Text = MaterialPlanningChilddata.Rows[i]["Buyer"].ToString();
                sheet[MPChildROW, ColSalesOrderId].Text = MaterialPlanningChilddata.Rows[i]["SalesOrderId"].ToString();

                sheet.Range[MPChildROW, 1, MPChildROW, MPChildendCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[MPChildROW, 1, MPChildROW, MPChildendCol].BorderAround(ExcelLineStyle.Hair);
                OrderTpe = MaterialPlanningChilddata.Rows[i]["OrderType"].ToString();

                MPChildROW++;
            }

            EndRows = MPChildROW - 1;

            if (RowIndexNo < MPChildROW - 1)
            {
                //sheet.Range[RowIndexNo, ColOrderType, MPChildROW - 1, ColOrderType].Merge();
                sheet.Range[RowIndexNo, ColOrderType, MPChildROW - 1, ColOrderType].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[RowIndexNo, ColOrderType, MPChildROW - 1, ColOrderType].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            }

            // MATERIAL INPUT CHILD

            int MIChildROW = MPChildROW + 1;
            int MIChildendCol = 1;
            int MIChildCOL = 1;

            #region Material Input Child Headers

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Input Material", 12, ExcelHAlign.HAlignLeft);
        //    int ColMaterial = MIChildCOL;
            MIChildROW++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Line Item Id", 12, ExcelHAlign.HAlignLeft);
            int ColJobWorkTransformationContractChildMasterId = MIChildCOL;
            MIChildCOL++;

            //report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Input Id", 12, ExcelHAlign.HAlignLeft);
            //int ColId = MIChildCOL;
            //MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "JW Input Item", 12, ExcelHAlign.HAlignLeft);
            int ColMaterial = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "JW Input Material", 12, ExcelHAlign.HAlignLeft);
            int ColJWInputMaterial = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "JW Input Article", 12, ExcelHAlign.HAlignLeft);
            int ColJWInputArticle = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Item Specification", 12, ExcelHAlign.HAlignLeft);
            int ColMatSpecification = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "UOM", 10, ExcelHAlign.HAlignLeft);
            int ColMICUOM = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Net Consumption/ Output Unit", 12, ExcelHAlign.HAlignLeft);
            int ColNetConsumptionOutputUnit = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Total Net Consumption", 12, ExcelHAlign.HAlignLeft);
            int ColTotalNetConsumption = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Rejection", 10, ExcelHAlign.HAlignLeft);
            int ColMIRejection = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Value Loss", 10, ExcelHAlign.HAlignLeft);
            int ColMIValueLoss = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Gross Consumption/ Unit", 10, ExcelHAlign.HAlignLeft);
            int ColGrossConsumption = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Total Gross Consumption", 10, ExcelHAlign.HAlignLeft);
            int ColTotalGrossConsumption = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Employee Code", 12, ExcelHAlign.HAlignLeft);
            int ColMIEmployeeCode = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Responsible Person", 15, ExcelHAlign.HAlignLeft);
            int ColResponsiblePerson = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Remarks", 12, ExcelHAlign.HAlignLeft);
            int ColMIRemarks = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Base UoM", 12, ExcelHAlign.HAlignLeft);
            int ColIssueBaseUoM = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Issue Qty", 15, ExcelHAlign.HAlignLeft);
            int ColIssuedQty = MIChildCOL;
            MIChildCOL++;

            report.SetHeaderText(ref sheet, MIChildROW, MIChildCOL, "Amount BC", 12, ExcelHAlign.HAlignLeft);
            int ColAmtBDT = MIChildCOL;
            MIChildROW++;

            MIChildendCol = MIChildCOL;
            #endregion Headers

            string Material = "";
            var MIStartRows = 0;
            var MIEndRows = 0;
            int MIRowIndexNo = MIChildROW;
            MIStartRows = MIChildROW;

            for (int i = 0; i < MaterialInputChilddata.Rows.Count; i++)
            {

                if (Material != MaterialInputChilddata.Rows[i]["JWInputItem"].ToString())
                {

                    if (MIRowIndexNo < MIChildROW)
                    {
                        //sheet.Range[MIRowIndexNo, ColMaterial, MIChildROW - 1, ColMaterial].Merge();
                        sheet.Range[MIRowIndexNo, ColMaterial, MIChildROW - 1, ColMaterial].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[MIRowIndexNo, ColMaterial, MIChildROW - 1, ColMaterial].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    }
                    MIRowIndexNo = MIChildROW;
                }

                sheet[MIChildROW, ColJobWorkTransformationContractChildMasterId].Text = MaterialInputChilddata.Rows[i]["JWTransformationPODetailId"].ToString();
                //sheet[MIChildROW, ColId].Text = MaterialInputChilddata.Rows[i]["Id"].ToString();
                sheet[MIChildROW, ColMaterial].Text = MaterialInputChilddata.Rows[i]["JWInputItem"].ToString();
                sheet[MIChildROW, ColJWInputMaterial].Text = MaterialInputChilddata.Rows[i]["JWInputMaterial"].ToString();
                sheet[MIChildROW, ColJWInputArticle].Text = MaterialInputChilddata.Rows[i]["JWInputArticle"].ToString();

                sheet[MIChildROW, ColMatSpecification].Text = MaterialInputChilddata.Rows[i]["ItemSpecification"].ToString();
                sheet[MIChildROW, ColMICUOM].Text = MaterialInputChilddata.Rows[i]["Unit"].ToString();
                sheet[MIChildROW, ColNetConsumptionOutputUnit].Number = clsStaticInfo.dbl(MaterialInputChilddata.Rows[i]["NetConsumption"].ToString());
                sheet[MIChildROW, ColTotalNetConsumption].Number = clsStaticInfo.dbl(MaterialInputChilddata.Rows[i]["TotalNetConsumption"].ToString());
                sheet[MIChildROW, ColMIRejection].Number = clsStaticInfo.dbl(MaterialInputChilddata.Rows[i]["Rejection"].ToString());
                sheet[MIChildROW, ColMIValueLoss].Number = clsStaticInfo.dbl(MaterialInputChilddata.Rows[i]["ValueLoss"].ToString());
                sheet[MIChildROW, ColGrossConsumption].Number = clsStaticInfo.dbl(MaterialInputChilddata.Rows[i]["GrossConsumption"].ToString());
                sheet[MIChildROW, ColTotalGrossConsumption].Number = clsStaticInfo.dbl(MaterialInputChilddata.Rows[i]["TotalGrossConsumption"].ToString());
                sheet[MIChildROW, ColMIEmployeeCode].Text = MaterialInputChilddata.Rows[i]["EmployeeCode"].ToString();
                sheet[MIChildROW, ColResponsiblePerson].Text = MaterialInputChilddata.Rows[i]["ResponsiblePerson"].ToString();
                sheet[MIChildROW, ColMIRemarks].Text = MaterialInputChilddata.Rows[i]["Remarks"].ToString();

                sheet[MIChildROW, ColIssueBaseUoM].Text = MaterialInputChilddata.Rows[i]["IssueBaseUoM"].ToString();
                sheet[MIChildROW, ColIssuedQty].Number = clsStaticInfo.dbl(MaterialInputChilddata.Rows[i]["IssuedQty"].ToString());
                sheet[MIChildROW, ColAmtBDT].Number = clsStaticInfo.dbl(MaterialInputChilddata.Rows[i]["AmtBDT"].ToString());

                sheet.Range[MIChildROW, 1, MIChildROW, MIChildendCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[MIChildROW, 1, MIChildROW, MIChildendCol].BorderAround(ExcelLineStyle.Hair);
                Material = MaterialInputChilddata.Rows[i]["JWInputItem"].ToString();

                MIChildROW++;
            }

            MIEndRows = MIChildROW - 1;

            if (MIRowIndexNo < MIChildROW - 1)
            {
                //sheet.Range[MIRowIndexNo, ColMaterial, MIChildROW - 1, ColMaterial].Merge();
                sheet.Range[MIRowIndexNo, ColMaterial, MIChildROW - 1, ColMaterial].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[MIRowIndexNo, ColMaterial, MIChildROW - 1, ColMaterial].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            }

            // BY PRODUCT CHILD REPORT

            int BPChildROW = MIChildROW + 1;
            int BPChildendCol = 1;
            int BPChildCOL = 1;

            #region By Product Child Headers

            report.SetHeaderText(ref sheet, BPChildROW, BPChildCOL, "By Product", 12, ExcelHAlign.HAlignLeft);
      //      int ColBPMaterial = BPChildCOL;
            BPChildROW++;

            report.SetHeaderText(ref sheet, BPChildROW, BPChildCOL, "Input Line Item Id", 12, ExcelHAlign.HAlignLeft);
            int ColJobWorkTransformationContractChild3MasterId = BPChildCOL;
            BPChildCOL++;

            report.SetHeaderText(ref sheet, BPChildROW, BPChildCOL, "By Product Id", 12, ExcelHAlign.HAlignLeft);
            int ColBPId = BPChildCOL;
            BPChildCOL++;

            report.SetHeaderText(ref sheet, BPChildROW, BPChildCOL, "JW Input Item", 12, ExcelHAlign.HAlignLeft);
            int ColJWInputItem = BPChildCOL;
            BPChildCOL++;

            report.SetHeaderText(ref sheet, BPChildROW, BPChildCOL, "JW By Product", 12, ExcelHAlign.HAlignLeft);
            int ColBPMaterial = BPChildCOL;
            BPChildCOL++;

            report.SetHeaderText(ref sheet, BPChildROW, BPChildCOL, "Item Specification", 12, ExcelHAlign.HAlignLeft);
            int ColBPMatSpecification = BPChildCOL;
            BPChildCOL++;

            report.SetHeaderText(ref sheet, BPChildROW, BPChildCOL, "Currency", 12, ExcelHAlign.HAlignLeft);
            int ColBPCurrency = BPChildCOL;
            BPChildCOL++;

            report.SetHeaderText(ref sheet, BPChildROW, BPChildCOL, "Standard Rate/ Unit", 10, ExcelHAlign.HAlignLeft);
            int ColStandardRatePerUnit = BPChildCOL;
            BPChildCOL++;

            report.SetHeaderText(ref sheet, BPChildROW, BPChildCOL, "Percentage Of Input", 12, ExcelHAlign.HAlignLeft);
            int ColPercentageOfInput = BPChildCOL;
            BPChildCOL++;

            report.SetHeaderText(ref sheet, BPChildROW, BPChildCOL, "By Product Quantity", 12, ExcelHAlign.HAlignLeft);
            int ColByProductQuantity = BPChildCOL;
            BPChildCOL++;

            report.SetHeaderText(ref sheet, BPChildROW, BPChildCOL, "Amount", 12, ExcelHAlign.HAlignLeft);
            int ColByProductAmount = BPChildCOL;
            BPChildCOL++;

            report.SetHeaderText(ref sheet, BPChildROW, BPChildCOL, "Employee Code", 12, ExcelHAlign.HAlignLeft);
            int ColEMPCode = BPChildCOL;
            BPChildCOL++;

            report.SetHeaderText(ref sheet, BPChildROW, BPChildCOL, "Responsible Person", 15, ExcelHAlign.HAlignLeft);
            int ColBPEmployeeName = BPChildCOL;
            BPChildCOL++;

            report.SetHeaderText(ref sheet, BPChildROW, BPChildCOL, "Remarks", 12, ExcelHAlign.HAlignLeft);
            int ColBPRemarks = BPChildCOL;
            BPChildROW++;
            BPChildendCol = BPChildCOL;
            #endregion Headers

            string BPMaterial = "";
            var BPStartRows = 0;
            var BPEndRows = 0;
            int BPRowIndexNo = BPChildROW;
            BPStartRows = BPChildROW;

            for (int i = 0; i < ByProductChilddata.Rows.Count; i++)
            {

                if (BPMaterial != ByProductChilddata.Rows[i]["JWInputItem"].ToString())
                {

                    if (BPRowIndexNo < BPChildROW)
                    {
                        //sheet.Range[BPRowIndexNo, ColBPMaterial, BPChildROW - 1, ColBPMaterial].Merge();
                        sheet.Range[BPRowIndexNo, ColBPMaterial, BPChildROW - 1, ColBPMaterial].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        sheet.Range[BPRowIndexNo, ColBPMaterial, BPChildROW - 1, ColBPMaterial].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    }
                    BPRowIndexNo = BPChildROW;
                }

                sheet[BPChildROW, ColJobWorkTransformationContractChild3MasterId].Text = ByProductChilddata.Rows[i]["JobWorkTransformationContractChild3MasterId"].ToString();
                sheet[BPChildROW, ColBPId].Text = ByProductChilddata.Rows[i]["Id"].ToString();
                sheet[BPChildROW, ColJWInputItem].Text = ByProductChilddata.Rows[i]["JWInputItem"].ToString();
                sheet[BPChildROW, ColBPMaterial].Text = ByProductChilddata.Rows[i]["JWByProduct"].ToString();
                sheet[BPChildROW, ColBPMatSpecification].Text = ByProductChilddata.Rows[i]["ItemSpecification"].ToString();
                sheet[BPChildROW, ColBPCurrency].Text = ByProductChilddata.Rows[i]["Currency"].ToString();
                sheet[BPChildROW, ColStandardRatePerUnit].Number = clsStaticInfo.dbl(ByProductChilddata.Rows[i]["StandardRate"].ToString());
                sheet[BPChildROW, ColPercentageOfInput].Number = clsStaticInfo.dbl(ByProductChilddata.Rows[i]["PercentageOfInput"].ToString());
                sheet[BPChildROW, ColByProductQuantity].Text = ByProductChilddata.Rows[i]["ByProductQuantity"].ToString();
                sheet[BPChildROW, ColByProductAmount].Text = ByProductChilddata.Rows[i]["ByProductAmount"].ToString();
                sheet[BPChildROW, ColEMPCode].Text = ByProductChilddata.Rows[i]["EMPCode"].ToString();
                sheet[BPChildROW, ColBPEmployeeName].Text = ByProductChilddata.Rows[i]["EmployeeName"].ToString();
                sheet[BPChildROW, ColBPRemarks].Text = ByProductChilddata.Rows[i]["Remarks"].ToString();

                sheet.Range[BPChildROW, 1, BPChildROW, BPChildendCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[BPChildROW, 1, BPChildROW, BPChildendCol].BorderAround(ExcelLineStyle.Hair);
                BPMaterial = ByProductChilddata.Rows[i]["JWInputItem"].ToString();

                BPChildROW++;
            }

            BPEndRows = BPChildROW - 1;

            if (BPRowIndexNo < BPChildROW - 1)
            {
                //sheet.Range[BPRowIndexNo, ColBPMaterial, BPChildROW - 1, ColBPMaterial].Merge();
                sheet.Range[BPRowIndexNo, ColBPMaterial, BPChildROW - 1, ColBPMaterial].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet.Range[BPRowIndexNo, ColBPMaterial, BPChildROW - 1, ColBPMaterial].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            }

            //GetWorkSheetBulletinTamplateCalculation(ref sheet1, ref report, data, "Bulletin Tamplate Calculation");
            //GetWorkSheetTamplateFormula(ref sheet2, ref report, data, "Bulletin Tamplate Calculation Formula");

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.NumberFormat = "#,##0.000";
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            report.CompanyPlantHeader(ref sheet, endCol, "Job Work PO (Transformation)", identity.CompanyId, identity.PlantName, null);
            report.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
            return workbook;
        }

        //private DataTable GetTransformationContractDataById(string PrintTabId)
        //{
        //    var sql = @"select tc.*,tcc.Id as LineItemId, TabType='Transformation',FORMAT(tc.Date,'dd-MMM-yyyy') as TransformationDate,CONVERT(varchar(5),tc.[Time],108)[TCTime],FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as TCProcessStartDate,
        //                            FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as TCProcessEndDate,FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as TCContractClosingDate,
        //                            Pnt.UserName as Plant,e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName,jwi.UserName as JobWorkItem, tcc.MaterialSpecification, tcc.MaterialReference
        //	,uom.UserName as UOM, tcc.Quantity, mma.StandardName as ArticleCode, tcc.OrderSpecific, tcc.RequiredCapacity,tcc.ByProductApplicable ,tcc.RateApplyId, c.Code as Currency
        //	,tcc.RatePerUnit, tcc.Rejection,tcc.ValueLoss,emp.EmployeeName,emp.EmployeeCode,tcc.Remarks as TCCRemarks,jl.LocationName as MaterialLocation,tcc.MaterialType,tcc.FinalOutputCategory
        //	, mi.TotalGrossConsumptionPerUnit, TotalGrossInputQuantity=(mi.TotalGrossConsumptionPerUnit * tcc.Quantity)
        //	, Amount= case when tcc.RateApplyId='Output' then (tcc.Quantity * tcc.RatePerUnit) else ((mi.TotalGrossConsumptionPerUnit * tcc.Quantity) * tcc.RatePerUnit) End
        //                            from dbo.jobworktransformationcontract tc left join ORG.Entity e on e.Id=tc.EntityId
        //	left join ORG.Plant Pnt on Pnt.Id=tc.PlantId
        //	left join HKP.Party p on p.Id=tc.VendorPartyId
        //	left join dbo.OSTransformationPODetail tcc on tcc.OSTransformationPOId=tc.Id
        //	left join HKP.JobWorkItem jwi on jwi.Id=tcc.JobWorkItemMasterId
        //	left join SCS.UnitOfMeasurement uom on uom.Id=tcc.OutputMaterialUOMId
        //	left join MST.MaterialMasterArticle mma on mma.Id=tcc.ArticleCodeId
        //	left join SCS.Currency c on c.Id=tcc.CurrencyId
        //	left join dbo.EmployeeInformation emp on emp.SystemId=tcc.ResponsiblePersonId
        //	left join HKP.JobWorkLocation jl on jl.Id=tcc.MaterialLocationId
        //	left join (select Sum(GrossConsumption) as TotalGrossConsumptionPerUnit, OSTransformationPODetailId from dbo.OSTransformationPOInputMaterial group by OSTransformationPODetailId)
        //	mi on mi.OSTransformationPODetailId=tcc.Id			
        //                            where tc.Id = '"+ PrintTabId + @"' ";

        //    return _sqlRepository.GetDataTable(sql);
        //}

        private DataTable GetTransformationContractDataById(string PrintTabId)
        {

            var sql = @"select tc.*,tcc.Id as LineItemId, TabType='Transformation',OutputMaterialType='Service',FORMAT(tc.PODate,'dd-MMM-yyyy') as TransformationDate,FORMAT(tc.ProcessStartDate,'dd-MMM-yyyy') as TCProcessStartDate,
    FORMAT(tc.ProcessEndDate,'dd-MMM-yyyy') as TCProcessEndDate,FORMAT(tc.ContractClosingDate,'dd-MMM-yyyy') as TCContractClosingDate,
    Pnt.UserName as Plant,e.UserName as Entity,p.Code as PartyCode, p.UserName as PartyName,jwi.UserName as JobWorkItem, tcc.MaterialSpecification, tcc.MaterialReference
	,UOM=case when tc.OrderSpecific='Yes' then mmuom.UserName else uom.UserName End, tcc.Quantity,mm.UserName as OutputMaterial
	, mma.StandardName as ArticleCode, tcc.OrderSpecific as OutputOrderSpecific, tcc.RequiredCapacity,tcc.ByProductApplicable ,tcc.RateApplyId, c.Code as Currency
	,RatePerUnit=ROUND((tcc.RatePerUnit),4), tcc.Rejection,tcc.ValueLoss,emp.EmployeeName,emp.EmployeeCode,tcc.Remarks as TCCRemarks,MS.UserName as MaterialLocation,tcc.MaterialType,tcc.FinalOutputCategory
	, mi.TotalGrossConsumptionPerUnit, TotalGrossInputQuantity=(mi.TotalGrossConsumptionPerUnit * tcc.Quantity)
    , Amount= round((tcc.Quantity * tcc.RatePerUnit),2)
    from dbo.JWTransformationPO tc left join ORG.Entity e on e.Id=tc.EntityId
	left join ORG.Plant Pnt on Pnt.Id=tc.PlantId
	left join HKP.Party p on p.Id=tc.PartyId
	left join dbo.JWTransformationPODetail tcc on tcc.JWTransformationPOId=tc.Id
	left join HKP.JobWorkItem jwi on jwi.Id=tcc.JobWorkItemMasterId
	left join SCS.UnitOfMeasurement uom on uom.Id=tcc.OutputMaterialUOMId
	left join MST.MaterialMasterArticle mma on mma.Id=tcc.ArticleId
	left join MST.MaterialMaster mm on mm.Id=tcc.MaterialMasterId
	left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
	left join SCS.Currency c on c.Id=tcc.CurrencyId
	left join dbo.EmployeeInformation emp on emp.SystemId=tcc.ResponsiblePersonId
	left join HKP.MaterialStorage MS on MS.Id=tcc.MaterialLocationId
	left join (
	select
	Sum(x.GrossConsumption) as TotalGrossConsumptionPerUnit,x.JWTransformationPODetailId
	from (
	Select GrossConsumption, JWTransformationPODetailId from dbo.JWTransformationPOInputMaterial 
	group by ArticleId,JWTransformationPODetailId,GrossConsumption
	) x group by x.JWTransformationPODetailId
	)mi on mi.JWTransformationPODetailId=tcc.Id				
                                    where tc.Id = '" + PrintTabId + @"' ";

            return _sqlRepository.GetDataTable(sql);
        }


        private DataTable GetMatPlanningChildDataById(string PrintTabId)
        {

            var sql = @"SELECT ROW_NUMBER() OVER (ORDER BY SO.MasterOrderItemId) AS RN,POD.ProductionOrderId
	                            , MOI.MasterOrderId, MO.MasterOrderNo, SO.MasterOrderItemId,moi.BuyerReferenceNo,moi.OwnReferenceNo,mo.BuyerReferenceNo BuyerOrderNo,mo.OwnReferenceNo AS OwnOrderNo
	                            , SO.Id AS SalesOrderId, P.UserName AS Customer,B.UserName AS Buyer,PM.Id AS ProductID,isnull(MOI.ProductionGrouping,'') AS ProductionGrouping
	                            , MOI.MaterialMasterId, MM.UserName AS MaterialMasterName,PM.UserName AS ProductName
	                            , MOI.ArticleId, ART.StandardName AS ArticleName
	                            , DeliveryDate = REPLACE(CONVERT(CHAR(11), SO.DeliveryDate, 106),' ','-')
	                            , CommitmentDate = REPLACE(CONVERT(CHAR(11), CommitmentDate, 106),' ','-')
	                            , isnull(DEST.UserName,'') AS DestinationName, isnull(SHP.UserName,'') AS ShipmentModeName
	                            , isnull(PO.PONumber,'') AS PONumber, OS.UserName AS OrderStatusName, OC.UserName AS OrderCategoryName
	                            , SO.Qty, SO.Rate,SO.Description,CASE WHEN isnull(so.WeekNo,0)=0 THEN  DATEPART(week,so.DeliveryDate) ELSE so.WeekNo END AS DeliveryWeek
	                            , Flag = CAST(0 AS BIT),SO.DestinationDescription
								,CN.ContractNo,MLC.LCRef MasterLCNo, Uom.UserName as MasterOrderUoM
								,owr.OrderType, owr.ParticularSpecification,owr.PlanQuantity as OWPlanQuantity, owr.Remarks as OWRemarks, owr.Id
                       FROM dbo.JWTransformationPOMasterOrderItem owr 
                        left join [TRN].[SalesOrder] AS SO on owr.SalesOrderId=SO.Id 
                        left outer join [TRN].[ProductionOrderDetail] POD on POD.SalesOrderId=SO.Id 
                       JOIN [TRN].[MasterOrderItem] AS MOI ON SO.MasterOrderItemId=MOI.Id
                       JOIN [TRN].[MasterOrder] AS MO ON MOI.MasterOrderId = MO.Id
                       LEFT JOIN [MST].[MaterialMaster] AS MM ON MOI.MaterialMasterId = MM.Id 
					   LEFT JOIN trn.ProductDefinition AS pd ON pd.MaterialMasterId=moi.MaterialMasterId
					   LEFT JOIN [MST].[ProductMaster] PM ON pm.Id=pd.ProductMasterId
                       LEFT JOIN [MST].[MaterialMasterArticle] AS ART ON MOI.ArticleId = ART.Id
                       LEFT JOIN [HKP].[Party] AS P ON MO.PartyId = P.Id
					   LEFT JOIN HKP.BUYER b on b.Id=MO.BuyerId
                       LEFT JOIN [MST].[Destination] AS DEST ON SO.DestinationId = DEST.Id
                       LEFT JOIN [MST].[ShipMode] AS SHP ON SO.ShipmentModeId = SHP.Id
                       LEFT JOIN [TRN].[CustomerPO] AS PO ON SO.CustomerPOId = PO.Id
                       LEFT JOIN [HKP].[OrderStatus] AS OS ON SO.OrderStatusId = OS.Id
                       LEFT JOIN [HKP].[OrderCategory] AS OC ON SO.OrderCategoryId = OC.Id
                       LEFT JOIN dbo.[Contract] AS CN ON CN.Id=SO.ContractId
                       LEFT JOIN dbo.MasterLC AS MLC ON MLC.Id=CN.MasterLCId
					   left join SCS.UnitOfMeasurement Uom on Uom.Id=MO.TotalQtyUOMId
					   left join dbo.JWTransformationPODetail mp on mp.Id=owr.JWTransformationPODetailId
					  left join dbo.JWTransformationPO tc on tc.Id=mp.JWTransformationPOId
				        where tc.Id='" + PrintTabId + @"' ";

            return _sqlRepository.GetDataTable(sql);
        }

        private DataTable GetMaterialInputChildDataById(string PrintTabId)
        {

            var sql = @"select mi.JWTransformationPODetailId,mi.JobWorkItemId,mi.ItemSpecification,mi.NetConsumption,mi.Rejection,mi.ValueLoss,mi.GrossConsumption,mi.ResponsiblePersonId
                                                    ,mi.ArticleId,jwi.UserName as JWInputItem,juom.UserName as JWIUnit,mm.UserName as JWInputMaterial,mma.StandardName as JWInputArticle
													, uom.UserName as BaseUOM, emp.EmployeeCode, emp.EmployeeStatus, emp.EmployeeName as ResponsiblePerson     
													,Unit=case when mi.ArticleId is not null then uom.UserName else juom.UserName END
                                                    ,TotalNetConsumption= (mi.NetConsumption * mp.Quantity)
													,TotalGrossConsumption=(mi.GrossConsumption * mp.Quantity),mi.Remarks
                                                    ,round(ISNULL(BA.IssuedQty,'0'),2) IssuedQty, round(ISNULL(BA.AmountBDT,'0'),2) AmtBDT, BA.BaseUoM as IssueBaseUoM
                                                    from dbo.JWTransformationPOInputMaterial mi
													left join HKP.JobWorkItem jwi on jwi.Id=mi.JobWorkItemId
													left join MST.MaterialMasterArticle mma on mma.Id=mi.ArticleId
													left join MST.MaterialMaster mm on mm.Id=mma.MaterialMasterId
													left join SCS.UnitOfMeasurement uom on uom.Id=mm.BaseUOMId
													left join scs.UnitOfMeasurement juom on juom.Id=jwi.UOMId
													left join dbo.EmployeeInformation emp on emp.SystemId=mi.ResponsiblePersonId
													left join dbo.JWTransformationPODetail mp on mp.Id=mi.JWTransformationPODetailId
													left join dbo.JWTransformationPO tc on tc.Id=mp.JWTransformationPOId
													left join (select SUM(GrossConsumption) as GrossConsumptionPerUnit, JWTransformationPODetailId 
													from dbo.JWTransformationPOInputMaterial group by JWTransformationPODetailId,ArticleId)
													tmi on tmi.JWTransformationPODetailId=mp.Id
                                                    left join (select SUM(IIH.Qty) as IssuedQty, Sum(IIH.TotalMaterialBooksCurrencyAmount) AmountBDT, IM.ArticleId, IID.InventoryMaterialId
													,BUom.UserName as BaseUoM
													from TRN.InventoryIssueDetail IID left join TRN.InventoryIssueHistory IIH on IIH.InventoryIssueDetailId=IID.Id
													left join TRN.InventoryMaterial IM on IM.Id=IID.InventoryMaterialId
													left join SCS.UnitOfMeasurement BUom on BUom.Id=IID.BaseUOMId
                                                    left join TRN.InventoryIssue II on II.Id=IID.InventoryIssueId
													where II.JobWorkContractId='" + PrintTabId + @"'
													group by IM.ArticleId,IID.InventoryMaterialId,BUom.UserName) BA on BA.ArticleId=mi.ArticleId
										            where tc.Id='" + PrintTabId + @"'
                                                    group by mi.JWTransformationPODetailId,mi.JobWorkItemId,mi.ItemSpecification,mi.NetConsumption,mi.Rejection
													,mi.ValueLoss,mi.GrossConsumption,mi.ResponsiblePersonId,mi.ArticleId,jwi.UserName,juom.UserName,mm.UserName
													,mma.StandardName, uom.UserName, emp.EmployeeCode, emp.EmployeeStatus, emp.EmployeeName,mp.Quantity,mi.Remarks
                                                    ,BA.IssuedQty,BA.AmountBDT,BA.BaseUoM ";

            return _sqlRepository.GetDataTable(sql);
        }

        private DataTable GetByProductChildDataById(string PrintTabId)
        {
            var sql = @"select bp.*,jwii.UserName as JWInputItem,jwi.UserName as JWByProduct,c.Code as Currency, emp.EmployeeCode as EMPCode
                                                    ,emp.EmployeeStatus as EMPStatus, emp.EmployeeName
													,ByProductQuantity= ((bp.PercentageOfInput * mi.NetConsumption * mp.Quantity)/100)
													,ByProductAmount=((bp.PercentageOfInput * mi.NetConsumption * mp.Quantity)/100) * bp.StandardRate
                                                    from dbo.JWTransformationPOByProduct bp
													left join HKP.JobWorkItem jwi on jwi.Id=bp.JobWorkItemId
													left join dbo.EmployeeInformation emp on emp.SystemId=bp.ResponsiblePersonId
													left join SCS.Currency c on c.Id=bp.CurrencyId
										            left join dbo.JWTransformationPOInputMaterial mi on mi.Id=bp.JWTransformationPOInputMaterialId
													left join HKP.JobWorkItem jwii on jwii.Id=mi.JobWorkItemId
													left join dbo.JWTransformationPODetail mp on mp.Id=mi.JWTransformationPODetailId
													left join dbo.JWTransformationPO tc on tc.Id=mp.JWTransformationPOId
										            where tc.Id='" + PrintTabId + "' ";

            return _sqlRepository.GetDataTable(sql);
        }

        #endregion end Reports for Transformation Contract

    }
}
