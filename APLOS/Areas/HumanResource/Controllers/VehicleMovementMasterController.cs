using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Security.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class VehicleMovementMasterController : BaseController
    {
        private readonly SqlRepository _sqlRepository;
        public VehicleMovementMasterController()
        {
            _sqlRepository = new SqlRepository();
        }

        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult VehicleMovementRequisition()
        {
            return View();
        }

        public ActionResult VehicleReqForApprove()
        {
            return View();
        }

        public ActionResult VehicleMovement()
        {
            return View();
        }

        public ActionResult VehicleInOut()
        {
            return View();
        }



        #region VehicleMaster
        public JsonResult GetVehicleMasterData()
        {
            string sql = @"select VM.*, FM.FuelType from [HKP].[VehicleMaster] VM
                            LEFT JOIN HKP.FuelMaster FM on FM.Id = VM.FuelTypeId";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateVehicleMaster(Dictionary<string, object> data)
        {
            try
            {

                string TableName = "HKP.VehicleMaster";
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);
                    data["Id"] = _Id;
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

                return Json(new { Error = false, Data = data, Sequence = GetSequence(), Message = AplosMessage.Insert }); ;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public JsonResult deleteVehicleMaster(string id)
        {
            try
            {
                string TableName = "[HKP].[VehicleMaster]";

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                throw ex;

            }
        }
        #endregion VehicleMaster

        #region VehicleMovement
        public JsonResult GetVehicleMovementData()
        {
            string sql = @"
Select VM.*, FLM.UserName FromLocation, TLM.UserName ToLocation from [HKP].[VehicleMovement] VM
left join HKP.LocationMaster FLM on FLM.Id = VM.FromLocationId
Left join HKP.LocationMaster  TLM on TLM.Id = VM.ToLocationId
";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        

        public void CreateVehicleMovement(Dictionary<string, object> data)
        {
            try
            {

                string TableName = "[HKP].[VehicleMovement]";
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");



                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);
                    data["Id"] = _Id;
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


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public ActionResult SaveVehicleMovement(Dictionary<string, object> data)

        {
            try
            {
                CreateVehicleMovement(data);
                return Json(new { Error = false, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public JsonResult deleteVehicleMovement(string id)
        {
            try
            {
                string TableName = "[HKP].[VehicleMovement]";

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                throw ex;

            }
        }

        #endregion VehicleMovement

        #region Purpose Master
        public JsonResult GetList()
        {
            string sql = "Select * from HKP.PurposeMaster";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        public double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM HKP.PurposeMaster");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        public Dictionary<string, object> CreateNewPurpose(Dictionary<string, object> data)
        {
            try
            {
               
                string TableName = "HKP.PurposeMaster";
                DataSet dsMaster;
               
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                #region validations
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where User = '"+ data["UserName"] + "' and Id <>'" + data["Id"] + "'", out dsMaster, false, "1");
                
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where User = '" + data["Sequence"] + "' and Id <>'" + data["Id"] + "'", out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Sequence exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where User = '" + data["Code"] + "' and Id <>'" + data["Id"] + "'", out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");
                #endregion validations

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);
                    data["Id"] = _Id;                    
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

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        

        

        public void DeletePurpose(string id)
        {
            try
            {
                string TableName = "HKP.PurposeMaster";

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();

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
            dr["AddedDate"] = DateTime.Now.ToString();
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
            dr["UpdatedDate"] = DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }

        [HttpPost, Authorize]
        public JsonResult Save(Dictionary<string, object> datas)

        {
            try
            {
                var data = CreateNewPurpose(datas);
                return Json(new { Error = false, Data = data, Sequence = GetSequence(), Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public ActionResult Delete(string id)
        {
            try
            {
                DeletePurpose(id);

                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }
        #endregion Purpose Master

        #region LocationMaster
        public JsonResult GetLocationList()
        {
            string sql = "Select * from HKP.LocationMaster";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        public double GetLocationSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM HKP.LocationMaster");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        public void CreateNewLocation(Dictionary<string, object> data)
        {
            try
            {

                string TableName = "HKP.LocationMaster";
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                #region validations
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where User = '" + data["UserName"] + "' and Id <>'" + data["Id"] + "'", out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where User = '" + data["Sequence"] + "' and Id <>'" + data["Id"] + "'", out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Sequence exists!!!");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where User = '" + data["Code"] + "' and Id <>'" + data["Id"] + "'", out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");
                #endregion validations

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);
                    data["Id"] = _Id;
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

               
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public ActionResult SaveLocation(Dictionary<string, object> data)

        {
            try
            {
                 CreateNewLocation(data);
                return Json(new { Error = false, Sequence = GetLocationSequence(), Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public void DeleteCreatedLocation(string id)
        {
            try
            {
                string TableName = "HKP.PurposeMaster";

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();

            }
            catch (Exception ex)
            {

                throw ex;

            }
        }
        public ActionResult DeleteLocation(string id)
        {
            try
            {
                DeletePurpose(id);

                return Json(new { Error = false, Sequence = GetLocationSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }
        #endregion LocationMaster

        #region Driver Master
        public JsonResult GetDriverMasterData()
        {
            string sql = @"Select DM.*, EI.EmployeeName DriverName from HKP.DriverMaster DM
                        left join EmployeeInformation EI on EI.SystemId = DM.DriverId";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult SaveDriverMaster(Dictionary<string, object> data)
        {
            try
            {

                string TableName = "HKP.DriverMaster";
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                #region validations
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where User = '" + data["DriverId"] + "' and Id <>'" + data["Id"] + "'", out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Driver Name already exists!!!");               
                #endregion validations

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);
                    data["Id"] = _Id;
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

                return Json(new { Error = false, Data = data ,Sequence = GetLocationSequence(), Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult DeleteDriverMaster(string id)
        {
            try
            {
                string TableName = "HKP.DriverMaster";

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
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
        #endregion Driver Master

        #region VehicleReq
        public JsonResult GetVehicleRequisitiontData()
        {
            string sql = @"Select VMR.Id, VMR.AppliedId ,Format(VMR.FromDate,'dd-MMM-yyyy')FromDate , Format(VMR.ToDate,'dd-MMM-yyyy')ToDate, Format(VMR.FromTime,'hh:mm tt') FromTime, Format(VMR.ToTime,'hh:mm tt')ToTime, VMR.PersonalOfficial
,VMR.PurposeId,PM.UserName Purpose, VMR.Remarks,EI.EmployeeName, EI.EmployeeCode ResponsiblePersonCode, DEP.UserName Department

                           -- ,STUFF((Select ',' + FLM.UserName
						--	from TRN.VehicleMovementRequisitionChild VRC
						--	LEFT JOIN HKP.LocationMaster FLM on FLM.Id = VRC.FromLocationId
						--	where VehicleMovementRequisitionId = VMR.Id
						--	FOR XML PATH('')),1,1,'') FromLocation

						--	,STUFF((Select ',' + TLM.UserName
						--	from TRN.VehicleMovementRequisitionChild VRC
						--	LEFT JOIN HKP.LocationMaster TLM on TLM.Id = VRC.ToLocationId
						--	where VehicleMovementRequisitionId = VMR.Id
						--	FOR XML PATH('')),1,1,'') ToLocation

                            from [TRN].[VehicleMovementRequisition] VMR							
                            left join EmployeeInformation EI on EI.SystemId = VMR.EmpSystemId
                            left join HKP.PurposeMaster PM on PM.Id = VMR.PurposeId
							LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId							
                            --where VMR.AppliedId is null";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetVehicleRequisitionChildData(string headerid)
        {
            string sql = @"select VRC.*, FLM.UserName FromLocation, TLM.UserName ToLocation from TRN.VehicleMovementRequisitionChild VRC
                            LEFT JOIN HKP.LocationMaster FLM on FLM.Id = VRC.FromLocationId
                            LEFT JOIN HKP.LocationMaster TLM on TLM.Id = VRC.ToLocationId
                            ";
            // where VehicleMovementRequisitionId = '"+ headerid + "'
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
       
        

        public void CreateVehicleRequisition(Dictionary<string, object> data)
        {
            try
            {
                
                string TableName = "[TRN].[VehicleMovementRequisition]";
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

               

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);
                    data["Id"] = _Id;
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


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult SaveRequisitionChid(List<Dictionary<string, object>> data, string headerId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsChild;
                string _Id = "";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                var id = "";
                foreach (var item in data)
                {
                    if (id == "")
                        id = "'" + item["Id"] + "'";
                    else
                        id = id + ",'" + item["Id"] + "'";
                }

                con.OpenDataSetThroughAdapter($"select * from TRN.VehicleMovementRequisitionChild where VehicleMovementRequisitionId = '{headerId}'", out dsChild, false, "1");
                foreach (var item in data)
                {
                    DataView dv = new DataView(dsChild.Tables[0]);

                    dv.RowFilter = "Id='" + item["Id"] + "'";
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID("MachineMasterTransaction", out _Id);
                    DataRow dr = dsChild.Tables[0].NewRow();
                    dr["Id"] = _Id;
                    dr["VehicleMovementRequisitionId"] = headerId;
                    dr["FromLocationId"] = item["FromLocationId"];
                    dr["ToLocationId"] = item["ToLocationId"];
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dsChild.Tables[0].Rows.Add(dr);

                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsChild);
                return Json(new { Error = false, Data = data ,Message = AplosMessage.Insert });
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public ActionResult SaveVehicleRequisition(Dictionary<string, object> data)

        {
            try
            {
                CreateVehicleRequisition(data);
                return Json(new { Error = false,  Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        public JsonResult deleteVehicleRequisition(string id)
        {
            try
            {
                string TableName = "[TRN].[VehicleMovement]";

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                throw ex;

            }
        }

        #endregion VehicleReq

        #region Fuel
        public JsonResult GetFuelData()
        {
            string sql = @"Select * from HKP.FuelMaster";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public JsonResult SaveFuelMaster(Dictionary<string, object> data)
        {
            try
            {
                string TableName = "HKP.FuelMaster";
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);
                    data["Id"] = _Id;
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
                 return Json(new { Error = false, Message = AplosMessage.Insert });

                }
                catch (Exception ex)
                {
                    throw ex;
                }

         }

        public ActionResult DeleteFuel(string id)
        {
            try
            {
                string TableName = "HKP.FuelMaster";

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();
                return Json(new { Error = false,  Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                throw ex;

            }
        }
        #endregion Fuel

        #region VehicleApproval
        public JsonResult GetVehicleRequisitiontDataForApproval()
        {
            string sql = @"Select VMR.Id, VMR.AppliedId ,Format(VMR.FromDate,'dd-MMM-yyyy')FromDate , Format(VMR.ToDate,'dd-MMM-yyyy')ToDate, Format(VMR.FromTime,'hh:mm tt') FromTime, Format(VMR.ToTime,'hh:mm tt')ToTime, VMR.PersonalOfficial
,VMR.PurposeId,PM.UserName Purpose, VMR.Remarks,EI.EmployeeName, EI.EmployeeCode ResponsiblePersonCode, DEP.UserName Department                         

                            from [TRN].[VehicleMovementRequisition] VMR							
                            left join EmployeeInformation EI on EI.SystemId = VMR.EmpSystemId
                            left join HKP.PurposeMaster PM on PM.Id = VMR.PurposeId
							LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId							
                            where VMR.AppliedId is null";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetVehicleAllocation()
        {
            string sql = @"select VA.Id, FORMAT(VA.FromDate, 'dd-MMM-yyyy')FromDate, FORMAT(VA.ToDate, 'dd-MMM-yyyy')ToDate, FORMAT(VA.FromTime, 'hh:mm tt')FromTime, FORMAT(VA.ToTime, 'hh:mm tt')ToTime
                        ,VM.VehicleName, EI.EmployeeName DriverName, VA.DriverMasterId, VA.VehicleMasterId
                        from TRN.VehicleAllocation VA
                        left join HKP.VehicleMaster VM on VM.Id = VA.VehicleMasterId
                        left join HKP.DriverMaster DM on DM.Id = VA.DriverMasterId
                        left join EmployeeInformation EI on EI.SystemId = DM.DriverId";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);


        }

        public JsonResult GetMergedRequisition(string appliedid)
        {
            string sql = @"Select VMR.Id, VMR.Id VehicleMovementRequisitionId ,VMR.AppliedId ,Format(VMR.FromDate,'dd-MMM-yyyy')FromDate , Format(VMR.ToDate,'dd-MMM-yyyy')ToDate, Format(VMR.FromTime,'hh:mm:ss tt') FromTime, Format(VMR.ToTime,'hh:mm:ss tt')ToTime, VMR.PersonalOfficial
                            ,VMR.PurposeId,PM.UserName Purpose, VMR.Remarks,EI.EmployeeName ByWhom, EI.EmployeeCode ResponsiblePersonCode, DEP.UserName Department
                            ,STUFF((Select ',' + FLM.UserName
							from TRN.VehicleMovementRequisitionChild VRC
							LEFT JOIN HKP.LocationMaster FLM on FLM.Id = VRC.FromLocationId
							where VehicleMovementRequisitionId = VMR.Id
							FOR XML PATH('')),1,1,'') FromLocation

							,STUFF((Select ',' + TLM.UserName
							from TRN.VehicleMovementRequisitionChild VRC
							LEFT JOIN HKP.LocationMaster TLM on TLM.Id = VRC.ToLocationId
							where VehicleMovementRequisitionId = VMR.Id
							FOR XML PATH('')),1,1,'') ToLocation
                                    
                            from [TRN].[VehicleMovementRequisition] VMR							
                            left join EmployeeInformation EI on EI.SystemId = VMR.EmpSystemId
                            left join HKP.PurposeMaster PM on PM.Id = VMR.PurposeId
							LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId
                            ";
            //where VMR.AppliedId = '" + appliedid + "'
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public ActionResult SaveVehicleAllocation(Dictionary<string, object> data, List<Dictionary<string, object>> reqdata) 
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                #region Requisition
                string ReqTable = "[TRN].[VehicleMovementRequisition]";
                DataSet dsRequisition;
                var id = "";
                foreach (var item in reqdata)
                {
                    if (id == "")
                        id = "'" + item["Id"] + "'";
                    else
                        id = id + ",'" + item["Id"] + "'";
                }
                con.OpenDataSetThroughAdapter($"select * from [TRN].[VehicleMovementRequisition]  where Id in ({id})", out dsRequisition, false, "1");
                

                
                #endregion Requisition
                

                string TableName = "TRN.VehicleTrip";
                DataSet dsMaster;

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);
                    data["Id"] = _Id;
                    
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();

                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                foreach (var item in reqdata)
                {

                    DataView dv = new DataView(dsRequisition.Tables[0]);

                    dv.RowFilter = "Id='" + item["Id"] + "'";
                    if (dv.Count > 0)
                    {
                        DataRow dr = dv[0].Row;
                        dr.BeginEdit();
                        dr["AppliedId"] = _Id;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();

                    }
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsRequisition);
                return Json(new { Error = false,  Message = AplosMessage.Insert });
            }
            catch (Exception ex) {
                throw ex;
            }

        }


        #endregion VehicleApproval

        #region Trip
        public JsonResult GetTripData()
        {
            string sql = @"select VT.Id, VT.Id AppliedId ,FORMAT(VT.FromDate, 'dd-MMM-yyyy')FromDate, FORMAT(VT.ToDate, 'dd-MMM-yyyy')ToDate, FORMAT(VT.FromTime, 'hh:mm tt')FromTime
                            , FORMAT(VT.ToTime, 'hh:mm tt')ToTime
                            from TRN.VehicleTrip VT";
            return Json(_sqlRepository.GetDataCollection(sql));
        }
        #endregion Trip

        #region VehicleIn
        public JsonResult GetVehicleInData()
        {
            string sql = @"select Id, FORMAT(InDate, 'dd-MMM-yyy')InDate, FORMAT(InTime, 'hh:mm tt')InTime, InKillometer, InRemarks from TRN.VehicleMovementInOut";
            return Json(_sqlRepository.GetDataCollection(sql));
        }

        public JsonResult SaveVehicleIn(Dictionary<string, object> data, string headerId)
        {
            try
            {
                string TableName = "TRN.VehicleMovementInOut";
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);
                    data["Id"] = _Id;
                    data["VehicleAllocationId"] = headerId;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    data["VehicleAllocationId"] = headerId;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return Json(new { Error = false, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public ActionResult DeleteVehicleIn(string id)
        {
            try
            {
                string TableName = "TRN.VehicleMovementInOut";

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                throw ex;

            }
        }
        #endregion VehicleIn

        #region VehicleOut
        public JsonResult GetVehicleOutData()
        {
            string sql = @"select FORMAT(OutDate, 'dd-MMM-yyy')OutDate, FORMAT(OutTime, 'hh:mm tt')OutTime, OutKillometer, OutRemarks from TRN.VehicleMovementInOut";
            return Json(_sqlRepository.GetDataCollection(sql));
        }

        public JsonResult SaveVehicleOut(Dictionary<string, object> data)
        {
            try
            {
                string TableName = "TRN.VehicleMovementInOut";
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);
                    data["Id"] = _Id;
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
                return Json(new { Error = false, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public ActionResult DeleteVehicleOut(string id)
        {
            try
            {
                string TableName = "TRN.VehicleMovementInOut";

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();
                return Json(new { Error = false, Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                throw ex;

            }
        }
        #endregion VehicleOut

        #region Get
        public JsonResult GetFromToLocationList()
        {
            string sql = @"Select Id Value, UserName Text from HKP.LocationMaster order by Text ";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public JsonResult ToLocationListBasedOnFromLoc(string fromlocId)
        {
            string sql = @"select TLM.Id Value, TLM.UserName Text from HKP.VehicleMovement VM
                            left join HKP.LocationMaster FLM on FLM.Id = VM.FromLocationId
                            left join  HKP.LocationMaster TLM on TLM.Id = VM.ToLocationId
                            where VM.FromLocationId = '"+ fromlocId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetPurposeList()
        {
            string sql = @"Select Id Value, UserName Text from HKP.PurposeMaster order by Text ";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public GridModel GetEmployeeListByWhom(GridParameter parameters, string companyId, string plantId, string partyAccountGroupId, string partyId)
        {
            try
            {
                parameters.CmdText = @"SELECT EI.SystemId, EI.PositionId AS PositionCode, EI.BudgetCode, EI.EmployeeCode, EI.FirstName, EI.MiddleName, EI.LastName
                                    , EI.EmployeeName, EI.DOB, EI.EmployeeStatus, DEG.UserName AS [Designation], MB.EntityId
                                    , EN.UserName AS EntityName, DEP.UserName AS Department, EI.EmploymentType
                            FROM dbo.EmployeeInformation AS EI
                            LEFT JOIN HKP.Designation AS DEG ON DEG.Id=EI.DesignationSystemID
                            LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId
                            LEFT JOIN [MST].[ManpowerBudget] AS MB ON MB.Id=EI.BudgetCode
                            LEFT JOIN ORG.Entity AS EN ON EN.Id=MB.EntityId
                            WHERE EI.CompanyId='" + companyId + "' AND EI.PlantId='" + plantId + "' AND EI.EmployeeStatus='Active'";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        [Authorize, HttpGet]
        public JsonResult GetEmployeeListByWhom(GridParameter parameters, string plantId, string partyAccountGroupId, string partyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            if (string.IsNullOrEmpty(plantId))
            {
                plantId = identity.PlantId;
            }
            return Json(GetEmployeeListByWhom(parameters, identity.CompanyId, plantId, partyAccountGroupId, partyId), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFuelList()
        {
            string sql = @"Select Id Value, FuelType Text from HKP.FuelMaster order by Text ";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetVehicleList()
        {
            string sql = @"Select Id Value, VehicleName Text from HKP.VehicleMaster order by Text ";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDriverList()
        {
            string sql = @"select DM.Id Value, EI.EmployeeName Text from HKP.DriverMaster DM
left join EmployeeInformation EI on EI.SystemId = DM.DriverId";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        #endregion Get

    }
}