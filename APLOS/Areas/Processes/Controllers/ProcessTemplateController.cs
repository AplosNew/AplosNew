using Aplos.Properties;
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

namespace Aplos.Areas.Processes.Controllers
{
    public class ProcessTemplateController : Controller
    {
        string TableName = "ProcessTemplate";
        private readonly ISqlRepository _sqlRepository;
        public ProcessTemplateController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        public ActionResult Aplos()
        {
            return View();
        }

        public ActionResult GetProcessTempData()
        {
            string sql = @"select PT.Id, PT.UserName, PT.StandardName, EI.EmployeeName ResponsiblePerson, PT.ResponsiblePersonId, PT.Remarks from dbo.ProcessTemplate PT
                           left join dbo.EmployeeInformation EI on EI.SystemId = PT.ResponsiblePersonId";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ProcessManagementDD()
        {
            string sql = @"Select Id Value, UserName Text from dbo.ProcessManagement order by Text";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetProcesDataList(string headerid)
        {
            string sql = "";
            if (headerid != null)
            {
                sql = @"select PTP.Id, isnull(convert(bit, PTP.Id),0) Flag , isnull(convert(bit, PTP.Id),null)isActive
,P.Id ProcessId, P.Code ProcessCode, P.UserName Process, PG.Code ProcessGroupCode ,PG.UserName ProcessGroup, SP.UserName SubProcess
from HKP.Process P
left join dbo.ProcessTemplateProcess PTP on PTP.ProcessId = P.Id
left join HKP.ProcessGroup PG on  PG.Id = P.ProcessGroupId
left join HKP.SubProcess SP on SP.ProcessId = P.Id
where P.Active = 1 and PG.Active = 1
order by PTP.Id desc";
            }
            else
            {
                sql = @"select P.Id, P.Code ProcessCode, P.UserName Process, PG.Code ProcessGroupCode ,PG.UserName ProcessGroup, SP.UserName SubProcess
from HKP.Process P
left join HKP.ProcessGroup PG on  PG.Id = P.ProcessGroupId
left join HKP.SubProcess SP on SP.ProcessId = P.Id
where P.Active = 1 and PG.Active = 1";
            }
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetProcessParamaData()
        {
            string sql = @"select PM.Id, PM.ItemName, UOM.UserName UnitOfMeasurement, PM.[Min], PM.[Max]  from dbo.ProcessParameter PM
                        left join SCS.UnitOfMeasurement UOM on UOM.Id = PM.UOMId";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSavedProcessByHeader(string headerid)
        {
            string sql = @"select PTP.Id, P.UserName Process, SP.UserName SubProcess from dbo.ProcessTemplateProcess PTP
                            left join HKP.Process P on P.Id = PTP.ProcessId
                            left join HKP.SubProcess SP on SP.ProcessId = P.Id
                            where P.Active = 1 and PTP.ProcessTemplateId = '"+ headerid + "'";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetProcessUtility() { 
            string sql = @"select PMU.Id,  PMU.[Min], PMU.[Max] ,UM.Id, UM.UserName UtilityName, UM.StandardName UtilityStdName, UM.UtilityCategory, UM.UtilitySubCategory ,UOM.UserName UOM 
from [dbo].[ProcessManagementUtility] PMU 
left join UtilityMaster UM on PMU.UtilityMasterId = UM.Id
left join SCS.UnitOfMeasurement UOM on UOM.Id = UM.UoMId
where UM.Active = 1 order by PMU.IsActive desc";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        #region AddUpdateColumn
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
        #endregion AddUpdateColumn

        #region HeaderSave
        [HttpPost]
        public JsonResult Save(Dictionary<string, object> data)
        {

            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from dbo.ProcessTemplate where UserName='" + data["UserName"] + "'  AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("User Name already exists!!!");

                con.OpenDataSetThroughAdapter("select * from dbo.ProcessTemplate where StandardName='" + data["StandardName"] + "'  AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Standard Name already exists!!!");



                con.OpenDataSetThroughAdapter("select * from dbo.ProcessTemplate where Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";




                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(TableName), out _Id);

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


                return Json(new { Error = false, Id = _Id, Message = AplosMessage.Insert });

            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }
        #endregion HeaderSave

        #region ProcessTab Save
        public ActionResult SaveProcess(List<Dictionary<string, object>> datalist, string headerid)
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

                objCon.OpenDataSetThroughAdapter("select * from [dbo].[ProcessTemplateProcess]  where ProcessTemplateId = '" + headerid + "'", out dsChild, false, "1");
                foreach (var item in datalist)
                {
                    DataView dv = new DataView(dsChild.Tables[0]);

                    dv.RowFilter = "Id='" + item["Id"] + "'";
                    if (dv.Count > 0)
                    {
                        DataRow dr = dv[0].Row;
                        dr.BeginEdit();
                        dr["ProcessTemplateId"] = headerid;
                        dr["ProcessId"] = item["Id"];
                        dr["IsActive"] = item["Flag"];
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();

                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("dbo.ProcessTemplateProcess", out _UserGroupId);
                        DataRow dr = dsChild.Tables[0].NewRow();
                        dr["Id"] = _UserGroupId;
                        dr["ProcessTemplateId"] = headerid;
                        dr["ProcessId"] = item["Id"];
                        dr["IsActive"] = item["Flag"];
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dsChild.Tables[0].Rows.Add(dr);
                    }
                }


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsChild);
                return Json(new { Error = false, Data = datalist, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        #endregion ProcessTab Save

        #region ProcessParam
        public ActionResult SaveProcessTempParam(string rowId, string headerid, List<Dictionary<string, object>> processParamList)
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

                objCon.OpenDataSetThroughAdapter("select * from [dbo].[ProcecessManagementTempParam]  where ProcessTemplateId = '" + headerid + "'", out dsChild, false, "1");
                foreach (var item in processParamList)
                {
                    DataView dv = new DataView(dsChild.Tables[0]);

                    dv.RowFilter = "Id='" + item["Id"] + "'";
                    if (dv.Count > 0)
                    {
                        DataRow dr = dv[0].Row;
                        dr.BeginEdit();
                        dr["ProcessParameterId"] = item["Id"];
                        dr["ProcessTemplateId"] = headerid;
                        dr["ProcessTempProcessId"] = rowId;
                        dr["IsActive"] = item["Flag"];
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();

                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("dbo.ProcessTemplateProcess", out _UserGroupId);
                        DataRow dr = dsChild.Tables[0].NewRow();
                        dr["Id"] = _UserGroupId;
                        dr["ProcessParameterId"] = item["Id"];
                        dr["ProcessTemplateId"] = headerid;
                        dr["ProcessTempProcessId"] = rowId;
                        dr["IsActive"] = item["Flag"];
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dsChild.Tables[0].Rows.Add(dr);
                    }
                }


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsChild);
                return Json(new { Error = false, Data = processParamList, Message = AplosMessage.Insert });
            }
            catch (Exception)
            {

                throw;
            }
        }
        #endregion ProcessParam

        #region MaterialTab Save
        public ActionResult SaveProcesMaterial(string rowId, string headerid, List<Dictionary<string, object>> processMaterialList)
        {


            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;

            DataSet dsprocessutlity;
            string id = string.Empty;

            string _Id = "";
            string _UserGroupId = string.Empty;


            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");

                #region Utility
                objCon.OpenDataSetThroughAdapter("select * from [dbo].[ProcessManagementTemptMaterial]  where ProcessManagementId = '" + headerid + "'", out dsprocessutlity, false, "1");
                foreach (var item in processMaterialList)
                {
                    
                        DataView dv = new DataView(dsprocessutlity.Tables[0]);

                        dv.RowFilter = "Id='" + item["Id"] + "'";
                        if (dv.Count > 0)
                        {
                            DataRow dr = dv[0].Row;
                            dr.BeginEdit();
                            dr["MaterialMasterId"] = item["Id"];
                            dr["ProcessTemplateId"] = headerid;
                            dr["ProcessTempProcessId"] = rowId;
                            dr["IsActive"] = item["Flag"];
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dr.EndEdit();

                        }
                        else
                        {
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID("dbo.ProcessManagementTemptMaterial", out _UserGroupId);
                            DataRow dr = dsprocessutlity.Tables[0].NewRow();
                            dr["Id"] = _UserGroupId;
                            dr["MaterialMasterId"] = item["Id"];
                            dr["ProcessTemplateId"] = headerid;
                            dr["ProcessTempProcessId"] = rowId;
                            dr["IsActive"] = item["Flag"];
                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            dsprocessutlity.Tables[0].Rows.Add(dr);
                        }
                    
                    
                }
                #endregion Utility


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsprocessutlity);
                return Json(new { Error = false, Data = processMaterialList, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        #endregion MaterialTab Save

        #region UtilityTab Save
        public ActionResult SaveProcessUtility(string rowId, string headerid, List<Dictionary<string, object>> processutlityList)
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

                objCon.OpenDataSetThroughAdapter("select * from [dbo].[ProcessManagementTempUtility]  where ProcessManagementId = '" + headerid + "'", out dsChild, false, "1");
                foreach (var item in processutlityList)
                {
                    DataView dv = new DataView(dsChild.Tables[0]);

                    dv.RowFilter = "Id='" + item["Id"] + "'";
                    if (dv.Count > 0)
                    {
                        DataRow dr = dv[0].Row;
                        dr.BeginEdit();
                        dr["UtilityMasterId"] = item["Id"];
                        dr["ProcessTemplateId"] = headerid;
                        dr["ProcessTempProcessId"] = rowId;
                        dr["IsActive"] = item["Flag"];
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;

                        dr.EndEdit();

                    }
                    else
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("dbo.ProcessManagementTempUtility", out _UserGroupId);
                        DataRow dr = dsChild.Tables[0].NewRow();
                        dr["Id"] = _UserGroupId;
                        dr["UtilityMasterId"] = item["Id"];
                        dr["ProcessTemplateId"] = headerid;
                        dr["ProcessTempProcessId"] = rowId;
                        dr["IsActive"] = item["Flag"];
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dsChild.Tables[0].Rows.Add(dr);
                    }
                }


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsChild);
                return Json(new { Error = false, Data = processutlityList, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        #endregion UtilityTab Save
    }
}