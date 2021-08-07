using Aplos.Controllers;
using Aplos.Properties;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.JobWork.Controllers
{
    public class JobWorkTransformationMasterController : BaseController
    {
        #region Constructor
        private readonly SqlRepository _sqlRepository;
        public JobWorkTransformationMasterController(SqlRepository Repository)
        {
            _sqlRepository = Repository;
        }
        #endregion
        #region Pages
        // GET: IE/JobWorkTransformationMaster
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion
        #region Code Area

        [HttpGet, Authorize]
        public JsonResult GetAllData()
        {
            string sql = "";
            sql = @"SELECT M.Id,JA.UserName  JobWorkActivityId,I.UserName JobWorkActivityChildId,M.RateApplicable
                    ,C.Code Currency,M.MinRate,M.MaxRate,M.CycleTime,E.EmployeeName ResponsiblePerson,M.ByProductApplicable,M.Remarks
					,ItemUOM=case when I.MaterialMasterId is not null then mmuom.UserName else U.UserName End
                    FROM [MST].[JobWorkTransformationMaster] M
                    LEFT JOIN [SCS].[Currency] C ON C.Id = M.CurrencyId
                    LEFT JOIN [HKP].[JobWorkActivity] JA ON JA.Id = M.JobWorkActivityId
                    LEFT JOIN [HKP].[JobWorkItem] I ON I.Id = M.JobWorkActivityChildId
					LEFT JOIN Dbo.EmployeeInformation E ON E.SystemId = M.ResponsiblePersonId
					LEFT JOIN SCS.UnitOfMeasurement U ON U.Id = I.UOMId
					left join MST.MaterialMaster mm on mm.Id=I.MaterialMasterId
					left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetAllMaterialInput(string Id)
        {
            string sql = "";
            sql = @"SELECT M.Id,M.JobWorkTransformationMasterId,I.Id JobWorkItemId,M.ItemSpecification,M.NetConsumption,
                    M.Rejection,M.ValueLoss,M.GrossConsumption,M.ResponsiblePersonId,E.EmployeeName ResponsiblePerson,M.Remarks
					,UOM=case when I.MaterialMasterId is not null then mmuom.UserName else U.UserName End
                    FROM [MST].[JobWorkTransformationMasterMaterialInput] M
                    LEFT JOIN dbo.EmployeeInformation E ON E.SystemId = M.ResponsiblePersonId
                    LEFT JOIN [HKP].[JobWorkItem] I ON I.Id = M.JobWorkItemId
					LEFT JOIN SCS.UnitOfMeasurement U ON U.Id = I.UOMId
					left join MST.MaterialMaster mm on mm.Id=I.MaterialMasterId
					left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
                    WHERE M.JobWorkTransformationMasterId='" + Id + "' ";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAllByProduct(string Id)
        {
            string sql = "";
            sql = @"SELECT M.Id,M.JobWorkTransformationMasterId,M.JobWorkItemId,M.ItemSpecification,M.PercentageOfInput
                    ,M.CurrencyId,M.StandardRate,M.ResponsiblePersonId,E.EmployeeName ResponsiblePerson,M.Remarks 
					,UOM=case when I.MaterialMasterId is not null then mmuom.UserName else U.UserName End
                    FROM [MST].[JobWorkTransformationMasterByProduct] M
                    LEFT JOIN dbo.EmployeeInformation E ON E.SystemId = M.ResponsiblePersonId
					left JOIN [HKP].[JobWorkItem] I ON I.Id = M.JobWorkItemId
					left JOIN SCS.UnitOfMeasurement U ON U.Id = I.UOMId
					left join MST.MaterialMaster mm on mm.Id=I.MaterialMasterId
					left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
                    WHERE JobWorkTransformationMasterId='" + Id + "' ";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAllCurrency()
        {
            string sql = "";
            sql = @"SELECT ID,Code FROM [SCS].[Currency] ORDER BY Code";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAllActivityUserName()
        {
            string sql = "";
            sql = @"SELECT Id,UserName FROM [HKP].[JobWorkActivity] WHERE Type='Transformation' AND IsActive = 1 ORDER BY UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAllProcessName()
        {
            string sql = "";
            sql = @"SELECT Id,UserName FROM [HKP].[Process] WHERE Active = 1 ORDER BY UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult getJobWorkItemUOM(string Id)
        {
            string sql = "";
            sql = @"SELECT jwi.UOMId,uom.UserName as JWIUnit, jwi.MaterialMasterId, mm.Code as MaterialCode, mm.UserName as Material, mm.BaseUOMId,unt.UserName as MMUnit  
                    FROM [HKP].[JobWorkItem] jwi
                    left JOIN [SCS].[UnitOfMeasurement] uom ON uom.Id = jwi.UOMId
					left join MST.MaterialMaster mm on mm.Id=jwi.MaterialMasterId
					left join scs.UnitOfMeasurement unt on unt.Id=mm.BaseUOMId
					where jwi.Id='"+ Id + @"' ORDER BY uom.UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetActivityChildItems(string Id)
        {
            string sql = "";
            sql = @"SELECT JobWorkItemId,I.UserName JobWorkItem
                    FROM [HKP].[JobWorkActivityChild] C
                    INNER JOIN HKP.JobWorkItem I ON I.Id = C.JobWorkItemId 
                    WHERE JobWorkActivityId ='" + Id + "' ORDER BY I.UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult SaveData(Dictionary<string, object> saveData, List<Dictionary<string, string>> childData, List<Dictionary<string, string>> materialInput, List<Dictionary<string, string>> byProduct)
        {
            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();

            if (childData == null || childData.Count == 0)
                throw new Exception("No child data found");

            if (materialInput == null || materialInput.Count == 0)
                throw new Exception("No material data found");

            if (saveData["ByProductApplicable"].ToString() == "Yes") {
                if (byProduct == null || byProduct.Count == 0)
                    throw new Exception("No By Product data found");
            }

            DataTable dtChild = new DataTable();
            DataTable dtMaterial = new DataTable();
            DataTable dtByProduct = new DataTable();

            var columnNames = childData.SelectMany(dict => dict.Keys).Distinct();
            dtChild.Columns.AddRange(columnNames.Select(c => new DataColumn(c)).ToArray());
            foreach (Dictionary<string, string> item in childData)
            {
                var row = dtChild.NewRow();
                foreach (var key in item.Keys)
                {
                    row[key] = item[key];
                }

                dtChild.Rows.Add(row);
            }

            var materialcolumnNames = materialInput.SelectMany(dict => dict.Keys).Distinct();
            dtMaterial.Columns.AddRange(materialcolumnNames.Select(c => new DataColumn(c)).ToArray());
            foreach (Dictionary<string, string> item in materialInput)
            {
                var row = dtMaterial.NewRow();
                foreach (var key in item.Keys)
                {
                    row[key] = item[key];
                }

                dtMaterial.Rows.Add(row);
            }

            if(saveData["ByProductApplicable"].ToString() == "Yes")
            {
                var byProductcolumnNames = byProduct.SelectMany(dict => dict.Keys).Distinct();
                dtByProduct.Columns.AddRange(byProductcolumnNames.Select(c => new DataColumn(c)).ToArray());
                foreach (Dictionary<string, string> item in byProduct)
                {
                    var row = dtByProduct.NewRow();
                    foreach (var key in item.Keys)
                    {
                        row[key] = item[key];
                    }

                    dtByProduct.Rows.Add(row);
                }
            }
            


            try
            {
                ConnectionManager.DAL.ConManager con2 = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                bplib.clsGenID _genId = new bplib.clsGenID();
                DataSet dsMaster;
                DataSet ValidateService;
                string _Message = "";
                string Id = "";
                con2.OpenDataSetThroughAdapter("select * from MST.JobWorkTransformationMaster where JobWorkActivityId='"+ saveData["JobWorkActivityId"] + "' and JobWorkActivityChildId='" + saveData["JobWorkActivityChildId"] + "' AND  Id<>'" + saveData["Id"].ToString() + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Activity and Output Material already exists!!!");

                con2.OpenDataSetThroughAdapter("select * from MST.JobWorkTransformationMaster where JobWorkActivityId='" + saveData["JobWorkActivityId"] + "' and JobWorkActivityChildId='" + saveData["JobWorkActivityChildId"] + "' and ServiceId='" + saveData["ServiceId"] + "' AND  Id<>'" + saveData["Id"].ToString() + "'", out ValidateService, false, "1");
                if (ValidateService.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Service already exists!!!");

                con.getDataSet("SELECT * FROM [MST].[JobWorkTransformationMaster] WHERE Id='" + saveData["Id"].ToString() + "'", out DataSet dsOut);
                if (dsOut.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsOut.Tables[0].NewRow();
                    _genId.GenID("[MST].[JobWorkTransformationMaster]", out Id);
                    Id = "TM-" + Id;
                    dr["Id"] = Id.ToString();

                    dr["JobWorkActivityId"] = saveData["JobWorkActivityId"].ToString();
                    dr["JobWorkActivityChildId"] = saveData["JobWorkActivityChildId"].ToString();
                    dr["RateApplicable"] = saveData["RateApplicable"].ToString();
                    dr["CurrencyId"] = saveData["CurrencyId"].ToString();
                    dr["MinRate"] = OTSBD.clsStaticInfo.dbl(saveData["MinRate"].ToString());
                    dr["MaxRate"] = OTSBD.clsStaticInfo.dbl(saveData["MaxRate"].ToString());
                    dr["CycleTime"] = OTSBD.clsStaticInfo.dbl(saveData["CycleTime"].ToString());
                    dr["ResponsiblePersonId"] = saveData["ResponsiblePersonId"].ToString();
                    dr["ByProductApplicable"] = saveData["ByProductApplicable"].ToString();
                    dr["Remarks"] = saveData["Remarks"].ToString();
                    dr["ServiceId"] = saveData["ServiceId"].ToString();
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsOut.Tables[0].Rows.Add(dr);

                    _Message = "Data Save Successfully..!";

                }
                else
                {
                    DataRow dr = dsOut.Tables[0].Rows[0];
                    Id = dr["Id"].ToString();
                    dr.BeginEdit();
                    dr["JobWorkActivityId"] = saveData["JobWorkActivityId"].ToString();
                    dr["JobWorkActivityChildId"] = saveData["JobWorkActivityChildId"].ToString();
                    dr["RateApplicable"] = saveData["RateApplicable"].ToString();
                    dr["CurrencyId"] = saveData["CurrencyId"].ToString();
                    dr["MinRate"] = OTSBD.clsStaticInfo.dbl(saveData["MinRate"].ToString());
                    dr["MaxRate"] = OTSBD.clsStaticInfo.dbl(saveData["MaxRate"].ToString());
                    dr["CycleTime"] = OTSBD.clsStaticInfo.dbl(saveData["CycleTime"].ToString());
                    dr["ResponsiblePersonId"] = saveData["ResponsiblePersonId"].ToString();
                    dr["ByProductApplicable"] = saveData["ByProductApplicable"].ToString();
                    dr["Remarks"] = saveData["Remarks"].ToString();
                    dr["ServiceId"] = saveData["ServiceId"].ToString();
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();

                    _Message = "Data Updated Successfully..!";
                }

                con.getDataSet("Select * from [MST].[JobWorkTransformationMasterProcess] where JobWorkTransformationMasterId='" + Id.ToString() + "'", out DataSet dsChild);

                for (int i = 0; i < dsChild.Tables[0].Rows.Count; i++)
                {
                    dtChild.DefaultView.RowFilter = "ProcessId='" + dsChild.Tables[0].Rows[i]["ProcessId"].ToString() + "'";
                    if (dtChild.DefaultView.Count == 0)
                        dsChild.Tables[0].Rows[i].Delete();
                }
                string ChildId = "";
                dtChild.DefaultView.RowFilter = null;
                for (int i = 0; i < dtChild.DefaultView.Count; i++)
                {
                    dsChild.Tables[0].DefaultView.RowFilter = "ProcessId='" + dtChild.Rows[i]["ProcessId"].ToString() + "'";
                    if (dsChild.Tables[0].DefaultView.Count == 0)
                    {
                        _genId.GenID("[MST].[JobWorkTransformationMasterProcess]", out ChildId);
                        ChildId = "TMP-" + ChildId;

                        DataRow dr = dsChild.Tables[0].NewRow();
                        dr["Id"] = ChildId;
                        dr["JobWorkTransformationMasterId"] = Id;
                        dr["ProcessId"] = dtChild.Rows[i]["ProcessId"].ToString();
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;

                        dsChild.Tables[0].Rows.Add(dr);
                    }
                }

                con.getDataSet("Select * from [MST].[JobWorkTransformationMasterMaterialInput] where JobWorkTransformationMasterId='" + Id.ToString() + "'", out DataSet dsMaterialChild);
                string materialChildId = "";
                for (int i = 0; i < dtMaterial.Rows.Count; i++)
                {
                    dsMaterialChild.Tables[0].DefaultView.RowFilter = "Id='" + dtMaterial.Rows[i]["Id"].ToString() + "'";
                    if (dsMaterialChild.Tables[0].DefaultView.Count == 0)
                    {
                        //addnew
                        _genId.GenID("[MST].[JobWorkTransformationMasterMaterialInput]", out materialChildId);
                        materialChildId = "TMMI-" + materialChildId;

                        DataRow dr = dsMaterialChild.Tables[0].NewRow();
                        dr["Id"] = materialChildId;
                        dr["JobWorkTransformationMasterId"] = Id;
                        dr["JobWorkItemId"] = dtMaterial.Rows[i]["JobWorkItemId"].ToString();
                        dr["ItemSpecification"] = dtMaterial.Rows[i]["ItemSpecification"].ToString();
                        dr["NetConsumption"] = OTSBD.clsStaticInfo.dbl(dtMaterial.Rows[i]["NetConsumption"].ToString());
                        dr["Rejection"] = OTSBD.clsStaticInfo.dbl(dtMaterial.Rows[i]["Rejection"].ToString());
                        dr["ValueLoss"] = OTSBD.clsStaticInfo.dbl(dtMaterial.Rows[i]["ValueLoss"].ToString());
                        dr["GrossConsumption"] = OTSBD.clsStaticInfo.dbl(dtMaterial.Rows[i]["GrossConsumption"].ToString());
                        dr["ResponsiblePersonId"] = dtMaterial.Rows[i]["ResponsiblePersonId"].ToString();
                        dr["Remarks"] = dtMaterial.Rows[i]["Remarks"].ToString();
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;

                        dsMaterialChild.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        DataRow dr = dsMaterialChild.Tables[0].Rows[i];
                        dr.BeginEdit();

                        dr["JobWorkTransformationMasterId"] = saveData["Id"].ToString();
                        dr["JobWorkItemId"] = dtMaterial.Rows[i]["JobWorkItemId"].ToString();
                        dr["ItemSpecification"] = dtMaterial.Rows[i]["ItemSpecification"].ToString();
                        dr["NetConsumption"] = OTSBD.clsStaticInfo.dbl(dtMaterial.Rows[i]["NetConsumption"].ToString());
                        dr["Rejection"] = OTSBD.clsStaticInfo.dbl(dtMaterial.Rows[i]["Rejection"].ToString());
                        dr["ValueLoss"] = OTSBD.clsStaticInfo.dbl(dtMaterial.Rows[i]["ValueLoss"].ToString());
                        dr["GrossConsumption"] = OTSBD.clsStaticInfo.dbl(dtMaterial.Rows[i]["GrossConsumption"].ToString());
                        dr["ResponsiblePersonId"] = dtMaterial.Rows[i]["ResponsiblePersonId"].ToString();
                        dr["Remarks"] = dtMaterial.Rows[i]["Remarks"].ToString();
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();
                    }
                }

                con.getDataSet("Select * from [MST].[JobWorkTransformationMasterByProduct] where JobWorkTransformationMasterId='" + Id.ToString() + "'", out DataSet dsJobWorkByProductChild);
                string byProductChildId = "";

                if (saveData["ByProductApplicable"].ToString() == "No") {
                    dtByProduct.Rows.Clear();

                    for (int i = 0; i < dsJobWorkByProductChild.Tables[0].Rows.Count; i++)
                    {
                        dtByProduct.DefaultView.RowFilter = "Id='" + dsJobWorkByProductChild.Tables[0].Rows[i]["Id"].ToString() + "'";
                        if (dtByProduct.DefaultView.Count == 0)
                            dsJobWorkByProductChild.Tables[0].Rows[i].Delete();
                    }
                }

                for (int i = 0; i < dtByProduct.Rows.Count; i++)
                {
                    dsJobWorkByProductChild.Tables[0].DefaultView.RowFilter = "Id='" + dtByProduct.Rows[i]["Id"].ToString() + "'";
                    if (dsJobWorkByProductChild.Tables[0].DefaultView.Count == 0)
                    {
                        //addnew
                        _genId.GenID("[MST].[JobWorkTransformationMasterByProduct]", out byProductChildId);
                        byProductChildId = "TMBP-" + byProductChildId;

                        DataRow dr = dsJobWorkByProductChild.Tables[0].NewRow();
                        dr["Id"] = byProductChildId;
                        dr["JobWorkTransformationMasterId"] = Id;
                        dr["JobWorkItemId"] = dtByProduct.Rows[i]["JobWorkItemId"].ToString();
                        dr["ItemSpecification"] = dtByProduct.Rows[i]["ItemSpecification"].ToString();
                        dr["PercentageOfInput"] = OTSBD.clsStaticInfo.dbl(dtByProduct.Rows[i]["PercentageOfInput"].ToString());
                        dr["StandardRate"] = OTSBD.clsStaticInfo.dbl(dtByProduct.Rows[i]["StandardRate"].ToString());
                        dr["CurrencyId"] = dtByProduct.Rows[i]["CurrencyId"].ToString();
                        dr["ResponsiblePersonId"] = dtByProduct.Rows[i]["ResponsiblePersonId"].ToString();
                        dr["Remarks"] = dtByProduct.Rows[i]["Remarks"].ToString();
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;

                        dsJobWorkByProductChild.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        DataRow dr = dsJobWorkByProductChild.Tables[0].Rows[i];
                        dr.BeginEdit();

                        dr["JobWorkTransformationMasterId"] = saveData["Id"].ToString();
                        dr["JobWorkItemId"] = dtByProduct.Rows[i]["JobWorkItemId"].ToString();
                        dr["ItemSpecification"] = dtByProduct.Rows[i]["ItemSpecification"].ToString();
                        dr["PercentageOfInput"] = OTSBD.clsStaticInfo.dbl(dtByProduct.Rows[i]["PercentageOfInput"].ToString());
                        dr["StandardRate"] = OTSBD.clsStaticInfo.dbl(dtByProduct.Rows[i]["StandardRate"].ToString());
                        dr["CurrencyId"] = dtByProduct.Rows[i]["CurrencyId"].ToString();
                        dr["ResponsiblePersonId"] = dtByProduct.Rows[i]["ResponsiblePersonId"].ToString();
                        dr["Remarks"] = dtByProduct.Rows[i]["Remarks"].ToString();
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();
                    }
                }

                OTSBD.clsStaticInfo _info = new OTSBD.clsStaticInfo();
                _info.SaveDataSets(dsOut, dsChild, dsMaterialChild, dsJobWorkByProductChild);

                return Json(new { Error = false, Message = _Message }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet, Authorize]
        public JsonResult GetSelectedProcessData(string Id)
        {
            string sql = "";
            sql = @"SELECT V.Id,V.ProcessId,P.UserName Process
                    FROM [MST].[JobWorkTransformationMasterProcess] V
                    INNER JOIN [HKP].[Process] P ON P.Id = V.ProcessId
                    WHERE V.JobWorkTransformationMasterId='" + Id + "' ORDER BY P.UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult GetSelectedData(string Id)
        {
            string sql = "";
            sql = @"SELECT M.Id,JobWorkActivityId,jwa.UserName as Activity,M.JobWorkActivityChildId,M.RateApplicable,U.UserName UOM,M.CurrencyId,M.MinRate,
                    M.MaxRate,M.CycleTime,M.ResponsiblePersonId,E.EmployeeName ResponsiblePerson,M.ByProductApplicable,M.Remarks
					,SM.UserName as Service,M.ServiceId
                    FROM [MST].[JobWorkTransformationMaster] M
                    LEFT JOIN dbo.EmployeeInformation E ON E.SystemId = M.ResponsiblePersonId
                    left JOIN HKP.JobWorkItem I ON I.Id = M.JobWorkActivityChildId
                    left JOIN SCS.UnitOfMeasurement U ON U.Id = I.UOMId
					left join HKP.JobWorkActivity jwa on jwa.Id=M.JobWorkActivityId
					left join HKP.ServiceMaster SM on SM.Id=M.ServiceId
                    WHERE M.Id='" + Id + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetJobWorkItemNames()
        {
            string sql = "";
            sql = @"SELECT Id,UserName FROM HKP.JobWorkItem WHERE IsActive = 1 ORDER BY UserName";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetMaterialMasterUOM(string Id)
        {
            string sql = "";
            sql = @"SELECT mm.Id as MaterialId, mm.UserName as Material 
                    ,UOMId=case when I.MaterialMasterId is not null then mmuom.UserName else U.UserName End
					FROM [HKP].[JobWorkItem] I
                    left JOIN [SCS].[UnitOfMeasurement] U ON U.Id = I.UOMId
					left join MST.MaterialMaster mm on mm.Id=I.MaterialMasterId
					left join SCS.UnitOfMeasurement mmuom on mmuom.Id=mm.BaseUOMId
                    WHERE I.Id = '" + Id + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult DeleteSelectedData(string Id)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con2 = new ConnectionManager.DAL.ConManager("1");

                if (string.IsNullOrEmpty(Id.ToString()))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                if (!string.IsNullOrEmpty(Id))
                {
                    con2.OpenDataSetThroughAdapter("select * from MST.JobWorkTransformationMasterMaterialInput where JobWorkTransformationMasterId='" + Id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete Material Input Data");
                    }

                    con2.OpenDataSetThroughAdapter("select * from MST.JobWorkTransformationMasterByProduct where JobWorkTransformationMasterId='" + Id + "' ", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("First Delete By Product Data");
                    }
                }

                con.BeginTransaction();
                con.executeQuery("DELETE FROM [MST].[JobWorkTransformationMasterProcess] WHERE JobWorkTransformationMasterId='" + Id.ToString() + "'");
                con.executeQuery("DELETE FROM [MST].[JobWorkTransformationMaster] WHERE Id='" + Id.ToString() + "'");

                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet, Authorize]
        public ActionResult DeleteMaterialChildData(string Id)
        {
            try
            {
                if (string.IsNullOrEmpty(Id.ToString()))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("DELETE FROM [MST].[JobWorkTransformationMasterMaterialInput] WHERE Id='" + Id.ToString() + "'");

                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet, Authorize]
        public ActionResult DeleteProductChildData(string Id)
        {
            try
            {
                if (string.IsNullOrEmpty(Id.ToString()))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("DELETE FROM MST.JobWorkTransformationMasterByProduct WHERE Id='" + Id + "'");

                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}