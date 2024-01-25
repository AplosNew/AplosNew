using Aplos.Controllers;
using Aplos.MaterialManagement;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Security.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
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
        #region Views
        public ActionResult Aplos()
        {
            return View();
        }

        [AllowAnonymous]
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

        public ActionResult VehicleTrip()
        {
            return View();
        }
        public ActionResult Vehiclereport()
        {
            return View();
        }

        #endregion Views


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

        public ActionResult SavePurposeRP(List<Dictionary<string, object>> datalist, string headerid)
        {
            

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;

            DataSet dsChild;
            string id = string.Empty;

            string _Id = "";
            string _UserGroupId = string.Empty;


            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");

                objCon.OpenDataSetThroughAdapter("select * from [TRN].[VehiclePurposeResponsiblePerson]  where VehiclePurposeId = '" + headerid + "'", out dsChild, false, "1");
                foreach (var item in datalist)
                {
                    DataView dv = new DataView(dsChild.Tables[0]);

                    dv.RowFilter = "Id='" + item["Id"] + "'";
                    if (dv.Count > 0)
                    {
                        DataRow dr = dv[0].Row;
                        dr.BeginEdit();
                        dr["VehiclePurposeId"] = headerid;
                        dr["ResponsiblePersonId"] = item["EmployeeCode"];
                        
                        dr["IsActive"] = item["isSelected"];
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();

                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("TRN.VehiclePurposeResponsiblePerson", out _UserGroupId);
                        DataRow dr = dsChild.Tables[0].NewRow();
                        dr["Id"] = _UserGroupId;
                        dr["VehiclePurposeId"] = headerid;
                        dr["ResponsiblePersonId"] = item["SystemID"];
                       
                        dr["IsActive"] = item["isSelected"];
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dsChild.Tables[0].Rows.Add(dr);
                    }
                }
               

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsChild);
                return Json(new { Error = false, Data = datalist, Sequence = GetSequence(), Message = AplosMessage.Insert });
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
            string sql = @"Select DM.*, EI.EmployeeName DriverName, EI.EmployeeCode DriverCode, EI.SystemId DriverId from HKP.DriverMaster DM
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
        [AllowAnonymous]
        public JsonResult GetVehicleRequisitiontData()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"Select VMR.Id, VMR.AppliedId ,Format(VMR.FromDate,'dd-MMM-yyyy')FromDate , Format(VMR.ToDate,'dd-MMM-yyyy')ToDate, Format(VMR.FromTime,'hh:mm tt') FromTime, Format(VMR.ToTime,'hh:mm tt')ToTime, VMR.PersonalOfficial
                           ,VMR.PurposeId,PM.UserName Purpose, VMR.Remarks,EI.EmployeeName, EI.EmployeeCode ResponsiblePersonCode, DEP.UserName Department, VMR.NumberOfPassengers ,VMR.[Name], VMR.VehiclePurposeResponsiblePersonId
                            from [TRN].[VehicleMovementRequisition] VMR							
                            left join EmployeeInformation EI on EI.SystemId = VMR.EmpSystemId
                            left join HKP.PurposeMaster PM on PM.Id = VMR.PurposeId
                            --(select distinct VPP.ResponsiblePersonId, RPE.EmployeeName from EmployeeInformation RPE
							--left join TRN.VehiclePurposeResponsiblePerson VPP on VPP.ResponsiblePersonId = RPE.SystemId
							
							--) ResponsiblePersonId on ResponsiblePersonId.ResponsiblePersonId = VMR.VehiclePurposeResponsiblePersonId
							LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId							
                            where VMR.AppliedId is null and VMR.isCancel is null and EI.SystemId = '" + identity.UserId+"'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, AllowAnonymous]
        public JsonResult GetRequisitionApprovedGridData()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string userid = identity.EmployeeId;
            string sql = @"Select VMR.Id, VMR.Id VehicleMovementRequisitionId  ,VMR.AppliedId ,Format(VMR.FromDate,'dd-MMM-yyyy')FromDate , Format(VMR.ToDate,'dd-MMM-yyyy')ToDate, Format(VMR.FromTime,'hh:mm tt') FromTime, Format(VMR.ToTime,'hh:mm tt')ToTime, VMR.PersonalOfficial
                    , VMR.PurposeId,PM.UserName Purpose, VMR.Remarks,EI.EmployeeName, EI.EmployeeCode ResponsiblePersonCode, DEP.UserName Department, VMR.NumberOfPassengers
                    ,RequisitionStatus = case when VMR.AppliedId is not null then 'Approved' 
					when VMR.IsReject = 1 then 'Reject'
					end, VT.AddedBy ApprovedBy
					, RejectBy = case when VMR.IsReject = 1 then VMR.UpdatedBy end
                    from[TRN].[VehicleMovementRequisition] VMR
                    left join EmployeeInformation EI on EI.SystemId = VMR.EmpSystemId
                    left join HKP.PurposeMaster PM on PM.Id = VMR.PurposeId
                    LEFT JOIN ORG.Department AS DEP ON DEP.Id = EI.DepartmentId
                    left join TRN.VehicleTrip VT on VT.Id = VMR.AppliedId
                    where EI.SystemId = '"+ userid + "' and VMR.isCancel is null";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public JsonResult LoadRequisitionRejectGridData() {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string userid = identity.EmployeeId;
            string sql = @"Select VMR.Id, VMR.Id VehicleMovementRequisitionId  ,VMR.AppliedId ,Format(VMR.FromDate,'dd-MMM-yyyy')FromDate , Format(VMR.ToDate,'dd-MMM-yyyy')ToDate, Format(VMR.FromTime,'hh:mm tt') FromTime, Format(VMR.ToTime,'hh:mm tt')ToTime, VMR.PersonalOfficial
                    , VMR.PurposeId,PM.UserName Purpose, VMR.Remarks,EI.EmployeeName, EI.EmployeeCode ResponsiblePersonCode, DEP.UserName Department, VMR.NumberOfPassengers
                    ,RequisitionStatus = case when VMR.AppliedId is not null then 'Approved' 
					when VMR.IsReject = 1 then 'Reject'
					end, VT.AddedBy ApprovedBy
					, RejectBy = case when VMR.IsReject = 1 then VMR.UpdatedBy end, VMR.Remarks RejectionRemarks
                    from[TRN].[VehicleMovementRequisition] VMR
                    left join EmployeeInformation EI on EI.SystemId = VMR.EmpSystemId
                    left join HKP.PurposeMaster PM on PM.Id = VMR.PurposeId
                    LEFT JOIN ORG.Department AS DEP ON DEP.Id = EI.DepartmentId
                    left join TRN.VehicleTrip VT on VT.Id = VMR.AppliedId
                    where VMR.IsReject = 1 and VMR.isCancel is null";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public JsonResult GetVehicleRequisitionChildData(string headerid)
        {
            string sql = @"select ROW_NUMBER() OVER (PARTITION BY VRC.VehicleMovementRequisitionId order by VRC.Id) AS Row_Num, VRC.*, FLM.UserName FromLocation, TLM.UserName ToLocation from TRN.VehicleMovementRequisitionChild VRC
                            LEFT JOIN HKP.LocationMaster FLM on FLM.Id = VRC.FromLocationId
                            LEFT JOIN HKP.LocationMaster TLM on TLM.Id = VRC.ToLocationId
                            ";
            // where VehicleMovementRequisitionId = '"+ headerid + "'
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public JsonResult GetVehicleRequisitionLocationData(string headerid)
        {
            string sql = @"select VRC.*, isSelected=CAST (CASE WHEN VRC.Id IS NULL THEN 0 ELSE 1 END AS bit) ,FLM.UserName FromLocation, TLM.UserName ToLocation from TRN.VehicleMovementRequisitionChild VRC
                            LEFT JOIN HKP.LocationMaster FLM on FLM.Id = VRC.FromLocationId
                            LEFT JOIN HKP.LocationMaster TLM on TLM.Id = VRC.ToLocationId
                            where VehicleMovementRequisitionId = '" + headerid + "'";
            
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public JsonResult GetVehiclePrposeRP(string purposeid)
        {
            string sql = @"select EI.SystemId Value, EI.EmployeeName Text from TRN.VehiclePurposeResponsiblePerson RP
                            Left join EmployeeInformation EI on EI.SystemId = RP.ResponsiblePersonId
                            where IsActive = 1 and RP.VehiclePurposeId = '" + purposeid + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize,AllowAnonymous]
        public ActionResult CreateVehicleRequisition(Dictionary<string, object> data)
        {
            try
            {
                
                string TableName = "[TRN].[VehicleMovementRequisition]";
                DataSet dsMaster, ds;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

               

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + data["Id"] + "'", out dsMaster, false, "1");
                //con.OpenDataSetThroughAdapter($"select top 1 * from {TableName} order by FromDate DESC", out ds, false, "1");
                //SqlDataAdapter da = new SqlDataAdapter($"select top 1 * from [TRN].[VehicleMovementRequisition]  order by FromDate DESC", con.ToString());
                //DataTable dt = new DataTable();
                //da.Fill(dt);

                string _Id = "";

                #region data Master update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);
                    
                    data["Id"] = 23 + _Id;
                    
                   
                    //if (dt.Rows[0]["Id"] == data["Id"])
                    //{
                    //    data["Id"] = int.Parse(_Id) + 1;
                    //}
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
                return Json(new { Error = false, Data = data, Message = AplosMessage.Insert });


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [Authorize, AllowAnonymous]
        public ActionResult SaveRequisitionChild(List<Dictionary<string, object>> data, string headerId)
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
                    if (dv.Count > 0) {
                        DataRow dr = dv[0].Row;
                        dr.BeginEdit();
                        dr["FromLocationId"] = item["FromLocationId"];
                        dr["ToLocationId"] = item["ToLocationId"];
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();
                    }
                    else
                    {
                        dv.RowFilter = "Id='" + item["Id"] + "'";
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("MachineMasterTransaction", out _Id);
                        DataRow dr = dsChild.Tables[0].NewRow();
                        dr["Id"] = 23 + _Id;
                        dr["VehicleMovementRequisitionId"] = headerId;
                        dr["FromLocationId"] = item["FromLocationId"];
                        dr["ToLocationId"] = item["ToLocationId"];
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dsChild.Tables[0].Rows.Add(dr);
                    }

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
                return Json(new { Error = false, Data = data ,Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [AllowAnonymous]
        public JsonResult deleteVehicleRequisition(string id)
        {
            try
            {
                string TableName = "[TRN].[VehicleMovementRequisition]";

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                //con.executeQuery("delete from TRN.VehicleMovementRequisitionChild where VehicleMovementRequisitionId='" + id + "'");
                con.executeQuery("update TRN.VehicleMovementRequisition set isCancel = 1 where Id ='" + id + "'");
                //con.executeQuery("delete from " + TableName + " where id='" + id + "'");
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

        #region RequisitionApproval
        public JsonResult GetVehicleRequisitiontDataForApproval()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"Select distinct VMR.Id, VMR.AppliedId ,Format(VMR.FromDate,'dd-MMM-yyyy')FromDate , Format(VMR.ToDate,'dd-MMM-yyyy')ToDate, Format(VMR.FromTime,'hh:mm tt') FromTime, Format(VMR.ToTime,'hh:mm tt')ToTime, VMR.PersonalOfficial, VMR.NumberOfPassengers
,VMR.PurposeId,PM.UserName Purpose,VMR.Name, VMR.Remarks,EI.EmployeeName, EI.EmployeeCode ResponsiblePersonCode, DEP.UserName Department,                         
FromLocation = stuff((select ', ' + LM.UserName 
							from TRN.VehicleMovementRequisitionChild VMC
							left join HKP.LocationMaster LM on LM.Id = VMC.FromLocationId
							where VMC.VehicleMovementRequisitionId = VMR.Id FOR XML PATH('')), 1,1,''),

							ToLocation =  stuff((select ', ' + TM.UserName 
							from TRN.VehicleMovementRequisitionChild VMC
							left join HKP.LocationMaster TM on TM.Id = VMC.ToLocationId
							where VMC.VehicleMovementRequisitionId = VMR.Id FOR XML PATH('')), 1,1,'')

                             from [TRN].[VehicleMovementRequisition] VMR	
							left join TRN.VehicleMovementRequisitionChild VMC on VMC.VehicleMovementRequisitionId = VMR.Id
                            left join EmployeeInformation EI on EI.SystemId = VMR.EmpSystemId
                            left join HKP.PurposeMaster PM on PM.Id = VMR.PurposeId
							LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId							
                            where VMR.IsApprove is null and VMR.IsReject is null and VMR.isCancel is null and VMC.FromLocationId is not null and 
							VMC.ToLocationId is not null and VMR.VehiclePurposeResponsiblePersonId = '" + identity.EmployeeId+@"'
							--order by VMR.Id Desc, FORMAT(VMR.AddedDate, 'dd-MMM-yyy') Desc";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetVehicleRequisitiontAproveddData()
        {
            string sql = @"Select distinct VMR.Id, VMR.AppliedId ,Format(VMR.FromDate,'dd-MMM-yyyy')FromDate , Format(VMR.ToDate,'dd-MMM-yyyy')ToDate, Format(VMR.FromTime,'hh:mm tt') FromTime, Format(VMR.ToTime,'hh:mm tt')ToTime, VMR.PersonalOfficial, VMR.NumberOfPassengers
,VMR.PurposeId,PM.UserName Purpose,VMR.Name, VMR.Remarks,EI.EmployeeName, EI.EmployeeCode ResponsiblePersonCode, DEP.UserName Department,                         
FromLocation = stuff((select ', ' + LM.UserName 
							from TRN.VehicleMovementRequisitionChild VMC
							left join HKP.LocationMaster LM on LM.Id = VMC.FromLocationId
							where VMC.VehicleMovementRequisitionId = VMR.Id FOR XML PATH('')), 1,1,''),

							ToLocation =  stuff((select ', ' + TM.UserName 
							from TRN.VehicleMovementRequisitionChild VMC
							left join HKP.LocationMaster TM on TM.Id = VMC.ToLocationId
							where VMC.VehicleMovementRequisitionId = VMR.Id FOR XML PATH('')), 1,1,'')

                             from [TRN].[VehicleMovementRequisition] VMR	
							left join TRN.VehicleMovementRequisitionChild VMC on VMC.VehicleMovementRequisitionId = VMR.Id
                            left join EmployeeInformation EI on EI.SystemId = VMR.EmpSystemId
                            left join HKP.PurposeMaster PM on PM.Id = VMR.PurposeId
							LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId							
                            where VMR.IsApprove = 1  and VMC.FromLocationId is not null and 
							VMC.ToLocationId is not null
							--order by VMR.Id Desc, FORMAT(VMR.AddedDate, 'dd-MMM-yyy') Desc";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetVehicleAllocation()
        {
            string sql = @"select VA.Id, VA.TripId TripNumber ,FORMAT(VA.FromDate, 'dd-MMM-yyyy')FromDate, FORMAT(VA.ToDate, 'dd-MMM-yyyy')ToDate, FORMAT(VA.FromTime, 'hh:mm tt')FromTime, FORMAT(VA.ToTime, 'hh:mm tt')ToTime
                        ,VM.VehicleName, EI.EmployeeName DriverName, VA.DriverMasterId, VA.VehicleMasterId, LM.UserName FromLocation
                        from TRN.VehicleAllocation VA
                        left join HKP.VehicleMaster VM on VM.Id = VA.VehicleMasterId
                        left join HKP.DriverMaster DM on DM.Id = VA.DriverMasterId
                        left join EmployeeInformation EI on EI.SystemId = DM.DriverId
						left join TRN.VehicleMovementRequisition VMR on VMR.AppliedId = VA.TripId
						left join TRN.VehicleMovementRequisitionChild VRC on VRC.VehicleMovementRequisitionId = VMR.Id
						left join HKP.LocationMaster LM on LM.Id = VRC.FromLocationId
                        where VMR.isCancel is null";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);


        }

        public JsonResult GetMergedRequisition(string appliedid)
        {
            string sql = @"Select Row_Number() OVER(PARTITION BY VMR.AppliedId Order by VMR.Id)Row_Num, VMR.Id, VMR.Id VehicleMovementRequisitionId ,VMR.IsApprove, VMR.AppliedId ,Format(VMR.FromDate,'dd-MMM-yyyy')FromDate , Format(VMR.ToDate,'dd-MMM-yyyy')ToDate, Format(VMR.FromTime,'hh:mm:ss tt') FromTime, Format(VMR.ToTime,'hh:mm:ss tt')ToTime, VMR.PersonalOfficial
                            ,VMR.PurposeId,PM.UserName Purpose, VMR.Remarks,EI.EmployeeName ByWhom, EI.EmployeeCode ResponsiblePersonCode, DEP.UserName Department,VMR.Name
                            
                                    
                            from [TRN].[VehicleMovementRequisition] VMR							
                            left join EmployeeInformation EI on EI.SystemId = VMR.EmpSystemId
                            left join HKP.PurposeMaster PM on PM.Id = VMR.PurposeId
							LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId
                            ";
            //where VMR.AppliedId = '" + appliedid + "'
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public ActionResult RequisitionApproved(List<Dictionary<string, object>> reqdata) 
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

                con.OpenDataSetThroughAdapter($"select * from  {ReqTable} where Id in ({id})", out dsRequisition, false, "1");
                
                #endregion Requisition
                

               

                foreach (var item in reqdata)
                {

                    DataView dv = new DataView(dsRequisition.Tables[0]);

                    dv.RowFilter = "Id='" + item["Id"] + "'";
                    if (dv.Count > 0)
                    {
                        DataRow dr = dv[0].Row;
                        dr.BeginEdit();
                        dr["IsApprove"] = item["isMerge"];
                        //dr["UpdatedBy"] = identity.Name;
                        //dr["UpdatedDate"] = DateTime.Now.ToString();
                        //dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();

                    }
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsRequisition);
                return Json(new { Error = false,  Message = AplosMessage.Insert });
            }
            catch (Exception ex) {
                throw ex;
            }

        }

        public ActionResult UpdateVehicleAllocation(Dictionary<string, object> data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                string TableName = "[TRN].[VehicleAllocation]";
                DataSet dsMaster;

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + data["VAID"] + "'", out dsMaster, false, "1");

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
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return Json(new { Error = false, Message = AplosMessage.Insert });
            }
            catch(Exception ex)
            {
                throw ex;
            }
            #endregion data update
        }

        public ActionResult UpdateVehicleMovement(Dictionary<string, object> data)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                string TableName = "[TRN].[VehicleMovementRequisition]";
                DataSet dsMaster;

                con.OpenDataSetThroughAdapter("select * from [TRN].[VehicleMovementRequisition] where Id ='" + data["MovementMasterId"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                if (dsMaster.Tables[0].Rows.Count > 0)
                {

                    data["Id"] = data["MovementMasterId"];
                    data["isCancel"] = true;

                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return Json(new { Error = false, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                throw ex;
            }
            #endregion data update
        }

        public ActionResult saveRejectForm(Dictionary<string, object> data, List<Dictionary<string, object>> reqdata)
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


                foreach (var item in reqdata)
                {

                    DataView dv = new DataView(dsRequisition.Tables[0]);

                    dv.RowFilter = "Id='" + item["Id"] + "'";
                    if (dv.Count > 0)
                    {
                        DataRow dr = dv[0].Row;
                        dr.BeginEdit();
                        
                        dr["IsReject"] = data["IsReject"];
                        dr["Remarks"] = data["Remarks"];
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();

                    }
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsRequisition);
                return Json(new { Error = false, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }


        #endregion RequisitionApproval

        #region Trip Scheduling
        public JsonResult GetTripData()
        {
            string sql = @"Select Row_Number() OVER(Order by VT.Id)Row_Num 
,FromLocation = stuff((
							Select ',  ' + LM.UserName from HKP.LocationMaster LM
							left join TRN.VehicleMovementRequisitionChild VRC on VRC.FromLocationId = LM.Id
							left join TRN.VehicleMovementRequisition VMR on VMR.Id = VRC.VehicleMovementRequisitionId
							where VMR.AppliedId = VT.Id
							FOR XML PATH('')),1,1,'')
							, ToLocation = stuff((
							Select ',  ' + LM.UserName from HKP.LocationMaster LM
							left join TRN.VehicleMovementRequisitionChild VRC on VRC.ToLocationId = LM.Id
							left join TRN.VehicleMovementRequisition VMR on VMR.Id = VRC.VehicleMovementRequisitionId
							where VMR.AppliedId = VT.Id 
							FOR XML PATH('')),1,1,'')
							,FORMAT(VT.FromDate, 'dd-MMM-yyyy')FromDate, FORMAT(VT.ToDate, 'dd-MMM-yyyy')ToDate, FORMAT(VT.FromTime, 'hh:mm tt')FromTime
                            , FORMAT(VT.ToTime, 'hh:mm tt')ToTime
							,EI.EmployeeName ByWhom,  DEP.UserName Department ,PM.StandardName Purpose, VT.Id, VT.Id AppliedId , ABE.EmployeeName ApprovedBy,VMR.Name
						
							
                            from TRN.VehicleTrip VT
                            left join TRN.VehicleAllocation VA on VA.TripId = VT.Id
							left join TRN.VehicleMovementRequisition VMR on VMR.AppliedId = VT.Id
                            left join EmployeeInformation ABE on ABE.SystemId = VMR.VehiclePurposeResponsiblePersonId
							left join EmployeeInformation EI on EI.SystemId = VMR.EmpSystemId
							 left join HKP.PurposeMaster PM on PM.Id = VMR.PurposeId
							LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId			
                            where VA.TripId is null";
            return Json(_sqlRepository.GetDataCollection(sql));
        }

        public JsonResult GetTripApproved()
        {
            string sql = @"select Row_Number() OVER(PARTITION BY VA.Id Order by VT.Id)Row_Num, VT.Id, VT.Id TripNumber ,VA.TripId ,VT.Id AppliedId ,FORMAT(VT.FromDate, 'dd-MMM-yyyy')FromDate, FORMAT(VT.ToDate, 'dd-MMM-yyyy')ToDate, FORMAT(VT.FromTime, 'hh:mm tt')FromTime
 , FORMAT(VT.ToTime, 'hh:mm tt')ToTime from TRN.VehicleTrip VT
left join TRN.VehicleAllocation VA on VA.TripId = VT.Id
where VA.TripId is not null";
            return Json(_sqlRepository.GetDataCollection(sql));
        }

        public JsonResult GetDataMappedWithTrip()
        {
            string sql = @"select Row_Number() OVER(Order by VA.Id)Row_Num, VA.Id, Format(VA.FromDate, 'dd-MMM-yyyy')FromDate, Format(VA.ToDate, 'dd-MMM-yyyy') ToDate, Format(VA.FromTime, 'hh:mm tt')FromTime
                            ,Format(VA.ToTime, 'hh:mm tt')ToTime, VA.DriverMasterId ,EI.EmployeeName DriverName, VA.VehicleMasterId, VM.VehicleName, VM.VehicleNumber, VA.TripId from TRN.VehicleAllocation VA 
                            left join HKP.VehicleMaster VM on VM.Id = VA.VehicleMasterId
                            left join HKP.DriverMaster DM on DM.Id = VA.DriverMasterId
                            left join EmployeeInformation EI on EI.SystemId = DM.DriverId
                            where TripId is not null";
            return Json(_sqlRepository.GetDataCollection(sql));
        }

        public ActionResult SaveVehicleDriverAllocation(Dictionary<string, object> data, string tripId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
               
                string TableName = "TRN.VehicleAllocation";
                DataSet dsMaster;

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);
                    data["Id"] = _Id;
                    data["TripId"] = tripId;

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
        #endregion Trip Scheduling

        #region VehicleIn
        public JsonResult GetVehicleInData(string vehicleallocationid)
        {
            string sql = @"select Id, FORMAT(OutDate, 'dd-MMM-yyy')OutDate, FORMAT(OutTime, 'hh:mm tt')OutTime, OutReading ,FORMAT(InDate, 'dd-MMM-yyy')InDate, FORMAT(InTime, 'hh:mm tt')InTime, InReading, InRemarks from TRN.VehicleMovementInOut where VehicleAllocationId = '" + vehicleallocationid + "'";
            return Json(_sqlRepository.GetDataCollection(sql));
        }

        public JsonResult GetPendingInTrip()
        {
            string sql = @"select distinct Row_Number() OVER(Order by VT.Id)Row_Num, VT.Id, VT.Id TripNumber ,VA.TripId ,VT.Id AppliedId ,FORMAT(VT.FromDate, 'dd-MMM-yyyy')FromDate, FORMAT(VT.ToDate, 'dd-MMM-yyyy')ToDate, FORMAT(VT.FromTime, 'hh:mm tt')FromTime
, FORMAT(VT.ToTime, 'hh:mm tt')ToTime, VA.DriverMasterId ,EI.EmployeeName DriverName, VA.VehicleMasterId, VM.VehicleNumber , VM.VehicleName ,VIO.Id ,  VA.Id VehicleAllocationId 
,stuff((select ',' + LM.UserName from HKP.LocationMaster LM
left join TRN.VehicleMovementRequisitionChild VMC on VMC.FromLocationId = LM.Id
left join TRN.VehicleMovementRequisition VMR on VMR.Id = VMC.VehicleMovementRequisitionId
where VMR.AppliedId = VT.Id FOR XML PATH('')), 1,1,'')FromLocation

,stuff((select ',' + LM.UserName from HKP.LocationMaster LM
left join TRN.VehicleMovementRequisitionChild VMC on VMC.ToLocationId = LM.Id
left join TRN.VehicleMovementRequisition VMR on VMR.Id = VMC.VehicleMovementRequisitionId
where VMR.AppliedId = VT.Id FOR XML PATH('')), 1,1,'')ToLocation
,AppliedId.ByWhom , AppliedId.Purpose, AppliedId.Name,AppliedId.PersonalOfficial

from TRN.VehicleTrip VT
left join TRN.VehicleAllocation VA on VA.TripId = VT.Id
left join (select VMR.AppliedId, RP.EmployeeName ByWhom, PM.UserName Purpose,VMR.Name,VMR.PersonalOfficial from TRN.VehicleMovementRequisition VMR 
left join EmployeeInformation RP on RP.SystemId = VMR.EmpSystemId
left join HKP.PurposeMaster PM on PM.Id = VMR.PurposeId
group by VMR.AppliedId, RP.EmployeeName, PM.UserName,VMR.Name,VMR.PersonalOfficial
)AppliedId on AppliedId = VT.Id

left join HKP.VehicleMaster VM on VM.Id = VA.VehicleMasterId
left join HKP.DriverMaster DM on DM.Id = VA.DriverMasterId
left join EmployeeInformation EI on EI.SystemId = DM.DriverId
left join TRN.VehicleMovementInOut VIO on VIO.VehicleAllocationId = VA.Id
where VIO.OutReading is not null and VIO.InReading is null and VA.Id is not null and VA.VehicleMasterId is not null and VA.DriverMasterId is not null and VA.TripId is not null  ";
            return Json(_sqlRepository.GetDataCollection(sql));
        }

        public JsonResult GetPendingOutTrip()
        {
            string sql = @"select distinct Row_Number() OVER(Order by VT.Id)Row_Num,VA.Id VAID, VT.Id, VT.Id TripNumber ,VA.TripId ,VT.Id AppliedId ,FORMAT(VT.FromDate, 'dd-MMM-yyyy')FromDate, FORMAT(VT.ToDate, 'dd-MMM-yyyy')ToDate, FORMAT(VT.FromTime, 'hh:mm tt')FromTime
, FORMAT(VT.ToTime, 'hh:mm tt')ToTime, VA.DriverMasterId ,EI.EmployeeName DriverName, VA.VehicleMasterId, VM.VehicleNumber , VM.VehicleName ,VIO.Id ,  VA.Id VehicleAllocationId,VMR.Id as MovementMasterId
,stuff((select ',' + LM.UserName from HKP.LocationMaster LM
left join TRN.VehicleMovementRequisitionChild VMC on VMC.FromLocationId = LM.Id
left join TRN.VehicleMovementRequisition VMR on VMR.Id = VMC.VehicleMovementRequisitionId
where VMR.AppliedId = VT.Id FOR XML PATH('')), 1,1,'')FromLocation

,stuff((select ',' + LM.UserName from HKP.LocationMaster LM
left join TRN.VehicleMovementRequisitionChild VMC on VMC.ToLocationId = LM.Id
left join TRN.VehicleMovementRequisition VMR on VMR.Id = VMC.VehicleMovementRequisitionId
where VMR.AppliedId = VT.Id FOR XML PATH('')), 1,1,'')ToLocation
,AppliedId.ByWhom , AppliedId.Purpose, AppliedId.Name,AppliedId.PersonalOfficial

from TRN.VehicleTrip VT
left join TRN.VehicleAllocation VA on VA.TripId = VT.Id
left join (select VMR.AppliedId, RP.EmployeeName ByWhom, PM.UserName Purpose,VMR.Name,VMR.PersonalOfficial from TRN.VehicleMovementRequisition VMR 
left join EmployeeInformation RP on RP.SystemId = VMR.EmpSystemId
left join HKP.PurposeMaster PM on PM.Id = VMR.PurposeId
group by VMR.AppliedId, RP.EmployeeName, PM.UserName,VMR.Name,VMR.PersonalOfficial
)AppliedId on AppliedId = VT.Id

left join HKP.VehicleMaster VM on VM.Id = VA.VehicleMasterId
left join HKP.DriverMaster DM on DM.Id = VA.DriverMasterId
left join EmployeeInformation EI on EI.SystemId = DM.DriverId
left join TRN.VehicleMovementInOut VIO on VIO.VehicleAllocationId = VA.Id
left join TRN.VehicleMovementRequisition VMR on VMR.AppliedId = VT.Id
where VIO.OutReading is null and VA.Id is not null and VA.VehicleMasterId is not null and VA.DriverMasterId is not null and VA.TripId is not null and VIO.Id is null and VMR.isCancel is null";
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
                    data["Id"] = 23 + _Id;
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
            string sql = @"select  FORMAT(OutDate, 'dd-MMM-yyy')OutDate, FORMAT(OutTime, 'hh:mm tt')OutTime, OutKillometer, OutRemarks from TRN.VehicleMovementInOut";
            return Json(_sqlRepository.GetDataCollection(sql));
        }

        public JsonResult SaveVehicleOut(Dictionary<string, object> data, string headerId)
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
                    data["Id"] = 23 + _Id;
                    data["VehicleAllocationId"] = headerId;
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
        [AllowAnonymous]
        public JsonResult GetFromToLocationList(string id)
        {
           
            string sql = @"Select Id Value, UserName Text from HKP.LocationMaster order by Text";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [AllowAnonymous]
        public JsonResult GetToLocationList(string id)
        {
            string sql = @"Select Id Value, UserName Text from HKP.LocationMaster where Id not in ('"+id+"') order by Text ";
           
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

        [AllowAnonymous]
        public JsonResult GetPurposeList()
        {
            string sql = @"Select Id Value, UserName Text from HKP.PurposeMaster Where Active = 1 order by Text ";
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

       
        public JsonResult GetVehicleList(string vehicleId)
        {
            string sql = @"Select Id Value, VehicleNumber Text from HKP.VehicleMaster  where Id = '" + vehicleId + "' order by Text";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        
        public JsonResult GetVehiclNameList()
        {
            string sql = @"Select Id Value, VehicleName Text from HKP.VehicleMaster order by Text";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetDriverList()
        {
            string sql = @"select DM.Id Value, EI.EmployeeName Text from HKP.DriverMaster DM
left join EmployeeInformation EI on EI.SystemId = DM.DriverId";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public JsonResult getemployeeDataList(string headerid)
        {
            try
            {
                //var Today = DateTime.Now;
                //string FirstDayOfTheMonth = "01-" + Convert.ToDateTime(Today).ToString("MMM") + "-" + Convert.ToDateTime(Today).ToString("yyyy");
                //string LastDayOfTheMonth = Convert.ToDateTime(FirstDayOfTheMonth).AddMonths(1).AddDays(-1).ToString("dd-MMM-yyyy");
                string CmdText = "";
                if (headerid != null) {
               
                 CmdText = @"SELECT --isSelected=(CAST(0 as bit)), 
                                  RP.IsActive isSelected, RP.IsActive ,RP.Id, Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode, Emp.EmployeeStatus, Emp.EmployeeCurrentStatus,
                                    Emp.DOS,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                    
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,EMP.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,FORMAT(emp.DOJ,'dd-MMM-yyyy') DOJ,FORMAT(emp.DOS,'dd-MMM-yyyy') DOS
                                        ,EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric, RG.UserName ResidenceGroup, PR.PaymentLink Skill, EC.UserName EmployeeCategory
                                        ,RM.Location, RM.ResidenceCategory, EMP.GenderID
										FROM EmployeeInformation EMP
                                        left join TRN.VehiclePurposeResponsiblePerson RP on RP.ResponsiblePersonId = EMP.SystemId and RP.VehiclePurposeId = '" + headerid + @"'
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=EMP.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                        LEFT JOIN ResidenceGroup RG on RG.Id = EMP.ResidenceGroupId 
										LEFT JOIN ResidenceAllocatedEmployees RAE on RAE.EmployeeSystemId = EMP.SystemId
										LEFT JOIN ResidenceMaster RM on RM.Id = RAE.ResidenceId
										LEFT JOIN MST.DesignationMaster DM on DM.DesignationId = D.Id
										LEFT JOIN HKP.EmployeeCategory EC on EC.Id = DM.EmployeeCategoryId
										
                                        Where EMP.EmployeeStatus='Active' 
                                        order by RP.IsActive DESC
                                        --ORDER BY EmployeeCodePreFix,EmployeeCodeNumeric";
                }
                else
                {
                    CmdText = @"SELECT 
                                   Emp.SystemID,EMP.EmployeeName,EMP.EmployeeCode, Emp.EmployeeStatus, Emp.EmployeeCurrentStatus,
                                    Emp.DOS,EMP.EmpPicPath,EMP.BudgetCode,E.UserName EntityName,D.UserName Designation,
                                    
                                        PR.UserName PositionName,DEG.UserName GivenDesignation,DEPT.UserName Department,S.UserName Section,EMP.SectionId,SS.UserName SubSection
                                        ,PL.UserName Plant,LDEG.UserName LegalDesignation, L.UserName Line,FORMAT(emp.DOJ,'dd-MMM-yyyy') DOJ,FORMAT(emp.DOS,'dd-MMM-yyyy') DOS
                                        ,EMP.EmployeeCodePreFix,EMP.EmployeeCodeNumeric, RG.UserName ResidenceGroup, PR.PaymentLink Skill, EC.UserName EmployeeCategory
                                        ,RM.Location, RM.ResidenceCategory, EMP.GenderID
										FROM EmployeeInformation EMP
                                       
                                        LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                                        LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                                        LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                                        LEFT JOIN ORG.Section S ON S.Id=EMP.SectionId
                                        LEFT JOIN ORG.SubSection SS ON SS.Id=EMP.SubSectionId
                                        LEFT JOIN HKP.Designation D ON PR.DesignationId=D.Id
                                        LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                                        LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                                        LEFT JOIN ORG.Line L ON L.Id=EMP.LineId
                                        LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
                                        LEFT JOIN HKP.LegalDesignation LDEG ON EMP.LegalDesignationId=LDEG.Id
                                        LEFT JOIN ResidenceGroup RG on RG.Id = EMP.ResidenceGroupId 
										LEFT JOIN ResidenceAllocatedEmployees RAE on RAE.EmployeeSystemId = EMP.SystemId
										LEFT JOIN ResidenceMaster RM on RM.Id = RAE.ResidenceId
										LEFT JOIN MST.DesignationMaster DM on DM.DesignationId = D.Id
										LEFT JOIN HKP.EmployeeCategory EC on EC.Id = DM.EmployeeCategoryId
										
                                        Where EMP.EmployeeStatus='Active' ";
                }

                return Json(_sqlRepository.GetDataCollection(CmdText), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        [AllowAnonymous]
        public JsonResult GetDefaultLoginEmployee()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select  EI.SystemId, EI.EmployeeCode ,EI.EmployeeName from EmployeeInformation EI Where EI.SystemId = '"+ identity.UserId + "'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        #endregion Get

        #region Report
        public ActionResult GetReportData()
        {
            string sql = @"select 
FromLocation = stuff((select ', ' + LM.UserName 
							from TRN.VehicleMovementRequisitionChild VMC
							left join HKP.LocationMaster LM on LM.Id = VMC.FromLocationId
							where VMC.VehicleMovementRequisitionId = VMR.Id FOR XML PATH('')), 1,1,''),

ToLocation =  stuff((select ', ' + TM.UserName 
							from TRN.VehicleMovementRequisitionChild VMC
							left join HKP.LocationMaster TM on TM.Id = VMC.ToLocationId
							where VMC.VehicleMovementRequisitionId = VMR.Id FOR XML PATH('')), 1,1,'')

,EI.EmployeeName , PM.UserName Purpose, VMR.PersonalOfficial, VMR.NumberOfPassengers, VM.VehicleNumber, DEI.EmployeeName DriverName
,FORMAT(VIO.InDate, 'dd-MMM-yyy')Vehicle_InDate, FORMAT(VIO.InTime, 'hh:mm tt')Vehicle_InTime ,FORMAT(VIO.OutDate, 'dd-MMM-yyy')Vehicle_OutDate, FORMAT(VIO.OutTime, 'hh:mm tt')Vehicle_OutTime
,DATEDIFF(HOUR, VIO.OutTime, VIO.InTime) TripTime
from TRN.VehicleMovementInOut VIO
LEFT JOIN TRN.VehicleAllocation VA ON VA.Id = VIO.VehicleAllocationId
left join HKP.VehicleMaster VM on VM.Id = VA.VehicleMasterId
left join HKP.DriverMaster DM on DM.Id = VA.DriverMasterId
left join EmployeeInformation DEI on DEI.SystemId = DM.DriverId
left join TRN.VehicleTrip VT on VT.Id = VA.TripId
left join TRN.VehicleMovementRequisition VMR on VMR.AppliedId = VT.Id
left join TRN.VehicleMovementRequisitionChild VRC on VRC.VehicleMovementRequisitionId = VMR.Id
left join HKP.PurposeMaster PM on PM.Id = VMR.PurposeId
left join EmployeeInformation EI on EI.SystemId = VMR.EmpSystemId
where VMR.AppliedId = 1";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public ActionResult CompleteVehicleMovementCycle(string fromDate, string toDate)
        {
            string sql = @"select distinct Row_Number() OVER(Order by VMR.Id) SrNo,BHW.EmployeeCode ByWhomEmpCode, BHW.EmployeeName ByWhom, AB.EmployeeCode ForWhomeEmpCode , AB.EmployeeName ForWhom
                            ,FORMAT(VMR.FromDate, 'dd-MMM-yyyy')FromDate, FORMAT(VMR.ToDate, 'dd-MMM-yyyy')ToDate, FORMAT(VMR.FromTime, 'hh:mm:ss tt')FromTime
                            , FORMAT(VMR.ToTime, 'hh:mm:ss tt')ToTime ,FromLocation = stuff((select ',  ' + LM.UserName from TRN.VehicleMovementRequisitionChild VMC							
                            left join HKP.LocationMaster LM on LM.Id = VMC.FromLocationId
                            where VMC.VehicleMovementRequisitionId = VMR.Id FOR XML PATH('')), 1,1,''),ToLocation =  stuff((select ',  ' + TM.UserName 
                            from TRN.VehicleMovementRequisitionChild VMC
                            left join HKP.LocationMaster TM on TM.Id = VMC.ToLocationId
                            where VMC.VehicleMovementRequisitionId = VMR.Id FOR XML PATH('')), 1,1,''), VMR.PersonalOfficial, GuestName = case when VMR.[Name] is null then '-' else VMR.[Name] end
                            , PM.UserName Purpose, VMR.NumberOfPassengers,isnull(VMR.Remarks,'') ReqRemark 
                            ,RequisitionStatus = case when VMR.AppliedId is not null then 'Approved' when VMR.IsReject = 1 then 'Reject'end
                            ,ApprovedRejectBy = case when VMR.AppliedId is not null then VT.AddedBy  when VMR.IsReject = 1 then VMR.UpdatedBy end
                            ,EDM.EmployeeCode DriverEmpCode, EDM.EmployeeName DriverName , FORMAT(DM.ExpiryDate, 'dd-MMM-yyyy')[License Exp Date], VM.VehicleName, VM.VehicleNumber
                            ,format(VIO.OutDate, 'dd-MMM-yyyy')OutDate ,format(VIO.OutTime, 'hh:mm:ss tt')OutTime , VIO.OutReading,format(VIO.InDate, 'dd-MMM-yyyy')InDate 
                            ,format(VIO.InTime, 'hh:mm:ss tt')InTime, VIO.InReading, isnull((VIO.InReading - Vio.OutReading),0)TotalTripReading ,DATEDIFF(MINUTE, VIO.OutDate, VIO.InDate)Total_Trip_Time

                            from TRN.VehicleMovementRequisition VMR 
                            left join DBO.EmployeeInformation BHW on BHW.SystemId = VMR.EmpSystemId
                            left join DBO.EmployeeInformation AB on AB.SystemId = VMR.AddedBy
                            left join HKP.PurposeMaster PM on PM.Id = VMR.PurposeId
                            left join TRN.VehicleMovementRequisitionChild VMC on VMC.VehicleMovementRequisitionId = VMR.Id
                            left join TRN.VehicleTrip VT on VT.Id = VMR.AppliedId
                            left join  TRN.VehicleAllocation VA on VA.TripId = VT.Id
                            left join HKP.VehicleMaster VM on VM.Id = VA.VehicleMasterId
                            left join HKP.DriverMaster DM on DM.Id = VA.DriverMasterId
                            left join EmployeeInformation EDM on EDM.SystemId = DM.DriverId
                            left join TRN.VehicleMovementInOut VIO on VIO.VehicleAllocationId = VA.Id
                            where VMR.AppliedId is not null and VA.DriverMasterId is not null and VIO.InDate is not null
                            and VMR.FromDate between '" + fromDate + @"' AND '" + toDate + @"' AND VMR.ToDate between '" + fromDate + @"' AND '" + toDate + @"'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

        }
        #endregion Report

        #region Generate Trip

        public JsonResult GetTripGenerated()
        {
            string sql = @"Select distinct VMR.Id, VMR.AppliedId ,Format(VMR.FromDate,'dd-MMM-yyyy')FromDate , Format(VMR.ToDate,'dd-MMM-yyyy')ToDate, Format(VMR.FromTime,'hh:mm tt') FromTime, Format(VMR.ToTime,'hh:mm tt')ToTime, VMR.PersonalOfficial, VMR.NumberOfPassengers
,VMR.PurposeId,PM.UserName Purpose,VMR.Name, VMR.Remarks,EI.EmployeeName, EI.EmployeeCode ResponsiblePersonCode, DEP.UserName Department,   VMR.NumberOfPassengers                      
,FromLocation = stuff((select ', ' + LM.UserName 
							from TRN.VehicleMovementRequisitionChild VMC
							left join HKP.LocationMaster LM on LM.Id = VMC.FromLocationId
							where VMC.VehicleMovementRequisitionId = VMR.Id FOR XML PATH('')), 1,1,''),

							ToLocation =  stuff((select ', ' + TM.UserName 
							from TRN.VehicleMovementRequisitionChild VMC
							left join HKP.LocationMaster TM on TM.Id = VMC.ToLocationId
							where VMC.VehicleMovementRequisitionId = VMR.Id FOR XML PATH('')), 1,1,'')

                             from [TRN].[VehicleMovementRequisition] VMR	
							left join TRN.VehicleMovementRequisitionChild VMC on VMC.VehicleMovementRequisitionId = VMR.Id
                            left join EmployeeInformation EI on EI.SystemId = VMR.EmpSystemId
                            left join HKP.PurposeMaster PM on PM.Id = VMR.PurposeId
							LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId							
                            where VMR.IsApprove = 1 and VMR.AppliedId is not null and VMC.FromLocationId is not null and 
							VMC.ToLocationId is not null
							--order by VMR.Id Desc, FORMAT(VMR.AddedDate, 'dd-MMM-yyy') Desc";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public JsonResult ApprovedRequisitionForMerged()
        {
            string sql = @"Select distinct VMR.Id, VMR.AppliedId ,Format(VMR.FromDate,'dd-MMM-yyyy')FromDate , Format(VMR.ToDate,'dd-MMM-yyyy')ToDate, Format(VMR.FromTime,'hh:mm tt') FromTime, Format(VMR.ToTime,'hh:mm tt')ToTime, VMR.PersonalOfficial, VMR.NumberOfPassengers
,VMR.PurposeId,PM.UserName Purpose,VMR.Name, VMR.Remarks,EI.EmployeeName, EI.EmployeeCode ResponsiblePersonCode, DEP.UserName Department,   VMR.NumberOfPassengers                      
,FromLocation = stuff((select ', ' + LM.UserName 
							from TRN.VehicleMovementRequisitionChild VMC
							left join HKP.LocationMaster LM on LM.Id = VMC.FromLocationId
							where VMC.VehicleMovementRequisitionId = VMR.Id FOR XML PATH('')), 1,1,''),

							ToLocation =  stuff((select ', ' + TM.UserName 
							from TRN.VehicleMovementRequisitionChild VMC
							left join HKP.LocationMaster TM on TM.Id = VMC.ToLocationId
							where VMC.VehicleMovementRequisitionId = VMR.Id FOR XML PATH('')), 1,1,'')

                             from [TRN].[VehicleMovementRequisition] VMR	
							left join TRN.VehicleMovementRequisitionChild VMC on VMC.VehicleMovementRequisitionId = VMR.Id
                            left join EmployeeInformation EI on EI.SystemId = VMR.EmpSystemId
                            left join HKP.PurposeMaster PM on PM.Id = VMR.PurposeId
							LEFT JOIN ORG.Department AS DEP ON DEP.Id=EI.DepartmentId							
                            where VMR.IsApprove = 1 and VMR.AppliedId is null and VMC.FromLocationId is not null and 
							VMC.ToLocationId is not null
							--order by VMR.Id Desc, FORMAT(VMR.AddedDate, 'dd-MMM-yyy') Desc";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GenerateTripNumber(Dictionary<string, object> data, List<Dictionary<string, object>> reqdata)
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
                return Json(new { Error = false, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        #endregion Generate Trip

        #region Trip Schedule
        //public ActionResult PendingTripSchedule()
        //{

        //}
        #endregion Trip Schedule

        #region vehicle inout report
        [HttpPost, Authorize]
        public ActionResult GetVehicleReport(List<Dictionary<string, object>> data, string reportFileName)
        {
            try
            {
                string fileName = "";
                InventoryReceiveQueryService obj = new InventoryReceiveQueryService(_sqlRepository);
                fileName = obj.GetVehicleReport(data, "", reportFileName);
                return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
 
        #endregion vehicle inout report
    }
}