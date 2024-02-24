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

namespace Aplos.Areas.IE.Controllers
{
    public class WorkcenterWiseDetentionController : BaseController
    {
        private readonly ISqlRepository _sqlRepository;
        public WorkcenterWiseDetentionController(SqlRepository R)
        {
            _sqlRepository = R;
        }
        public ActionResult Aplos()
        {
            return View();
        }
        [Authorize, HttpPost]
        public ActionResult GetShift(string processId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select Distinct SD.SystemID ShiftId,P.Id PlantId,P.UserName Plant,SD.ShiftDefinationDescription
						,SD.UserName ShiftDefination,SD.InTime,SD.OutTime from [dbo].[WorkCenterWiseShift] WCS
LEFT JOIN dbo.ShiftDefination AS SD ON SD.SystemID = WCS.ShiftDefinationID
LEFT JOIN ORG.Plant P on P.Id=SD.PlantID
WHERE WorkCenterMasterId IN(SELECT Id FROM SCS.WorkCenterMaster AS wcm WHERE wcm.ProcessId='" + processId + @"')";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetProcess(string machineMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select distinct P.Id,P.Sequence,P.Code,P.ShortName,P.StandardName,P.Id ProcessId,P.UserName Process
			                            from HKP.Process P
			                            --left join HKP.Process P on P.Id=MMP.ProcessId";
            //where MMP.MachineMasterId='" + machineMasterId + @"'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetDetentionMaster()
        {
            string str = @"Select DetentionUserName As Text, Id As Value from DetentionMaster";
            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetWorkcenter(string entityid, string processid, string shiftId, string detentionId, string date)
        {
            //            var sqlCondition = "";
            //            if (headerid == null || headerid == "")
            //            {
            //               sqlCondition = $"where WCM.EntityId = '{entityid}' and WCM.ProcessId = '{processid}' and Active=1 and WCM.StandardName is not null";
            //            }

            //            else
            //            {
            //                sqlCondition = $"where MMT.Id = '{headerid}'";
            //            }
            //            string str = @"SELECT distinct WCM.Id WorkcenterId, WCM.StandardName, '' Id ,'' EntityId, '' DetentionId,  '' FromTime, '' ToTime, '' [Date] , '' ProcessId, '' ShiftId ,'' Minute, '' Detention , ''ResponsiblePersonId,'' Remark FROM  SCS.WorkCenterMaster WCM  
            //" + sqlCondition + " order by WCM.StandardName";
            string str = "";
            str = @"SELECT  WCM.Id WorkcenterId, WCM.StandardName, '' Id ,'' EntityId, '' DetentionId,  '' FromTime, '' ToTime, '' [Date] , '' ProcessId, '' ShiftId ,'' Minute, '' Detention , ''ResponsiblePersonId,'' Remark 
FROM  SCS.WorkCenterMaster WCM  
where WCM.EntityId = '" + entityid + @"' and WCM.ProcessId = '" + processid + @"' and Active=1 and WCM.StandardName IS NOT NULL 
AND  WCM.Id NOT IN(Select  WorkCenterId from MachineMasterTransaction MMT where MMT.EntityId = '" + entityid + @"' and MMT.DetentionId = '" + detentionId + @"' and MMT.ProcessId = '" + processid + @"' and format(MMT.Date, 'dd-MMM-yyyy') = '" + date + @"' and MMT.ShiftId = '" + shiftId + @"')
ORDER BY WCM.StandardName";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetDetentionResponsible(string detentionId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select distinct E.SystemId as ResponsiblePersonId,E.EmployeeCode,E.EmployeeName as ResponsiblePerson,DEP.UserName AS Department,S.UserName as Section,
  SS.UserName as SubSection,DEG.UserName AS [LegalDesignation],DR.DetentionMasterId from DetentionMasterResponsible DR
left join EmployeeInformation AS E ON E.SystemId=DR.ResponsibleMasterId
							LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=E.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.id=E.DepartmentId
							LEFT OUTER JOIN ORG.Section S ON S.Id=E.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=E.SubSectionId
where DetentionMasterId='" + detentionId + "'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
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

        public JsonResult GetSavedWorkCenterForUpdate(string entityid, string detentionid, string processid, string date, string shiftid, string minute)
        {
            string sql = @"SELECT CAST(1 AS BIT)isSelected,MMT.Id, WCM.Id WorkcenterId, WCM.StandardName, MMT.Minute, MMT.DetentionId, DM.DetentionUserName,EI.EmployeeName from MachineMasterTransaction MMT
	left join SCS.WorkCenterMaster WCM  on WCM.Id = MMT.WorkCenterId
	left join DetentionMaster DM on DM.Id=MMT.DetentionId  
	left join EmployeeInformation EI on EI.SystemId=MMT.ResponsiblePersonId
	 where MMT.EntityId = '" + entityid + @"' and MMT.DetentionId = '" + detentionid + @"' and MMT.ProcessId = '" + processid + @"' and format(MMT.Date, 'dd-MMM-yyyy') = '" + date + @"' and MMT.ShiftId = '" + shiftid + @"' --and MMT.Minute = '" + minute + @"'
	 order by FORMAT(MMT.AddedDate, 'dd-MMM-yyyy') DESC";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult Create(List<Dictionary<string, object>> data)
        {
            try
            {
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        if (item["ResponsiblePersonId"] == null)
                        {
                            throw new Exception("Responsible Person should not empty");
                        }
                    }
                    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                    string TableName = "MachineMasterTransaction";

                    DataSet dsMaster;


                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                    string _Id = "";

                    #region UserGroup (From Pop Screen)
                    var id = "";
                    foreach (var item in data)
                    {
                        if (id == "")
                            id = "'" + item["Id"] + "'";
                        else
                            id = id + ",'" + item["Id"] + "'";
                    }
                    string _UserGroupId = "";
                    con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id In (" + id + ")", out dsMaster, false, "1");
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsMaster.Tables[0]);

                        dv.RowFilter = "Id='" + item["Id"] + "'";
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("MachineMasterTransaction", out _Id);
                        DataRow dr = dsMaster.Tables[0].NewRow();
                        dr["Id"] = _Id;
                        dr["ResponsiblePersonId"] = item["ResponsiblePersonId"];
                        dr["WorkcenterId"] = item["WorkcenterId"];
                        dr["EntityId"] = item["EntityId"];
                        dr["DetentionId"] = item["DetentionId"];
                        dr["ProcessId"] = item["ProcessId"];
                        dr["ShiftId"] = item["ShiftId"];
                        dr["Date"] = item["Date"];
                        dr["FromTime"] = item["FromTime"];
                        dr["ToTime"] = item["ToTime"];
                        dr["Minute"] = item["Minute"];
                        //dr["Remark"] = item["Remark"];
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dsMaster.Tables[0].Rows.Add(dr);

                    }

                    #endregion UserGroup (From Pop Screen)

                    clsStaticInfo _info = new clsStaticInfo();
                    _info.SaveDataSets(dsMaster);

                }
                return Json(new { Data = data, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult UpdateOrDelete(List<Dictionary<string, object>> data)
        {
            try
            {
                DataSet dsMaster;
                string id = "";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                foreach (var item in data)
                {
                    if (id == "")
                        id = "'" + item["Id"] + "'";
                    else
                        id = id + ",'" + item["Id"] + "'";
                }
                con.OpenDataSetThroughAdapter("select * from dbo.MachineMasterTransaction where Id In (" + id + ")", out dsMaster, false, "1");

                foreach (var item in data)
                {
                    DataView dv = new DataView(dsMaster.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";
                    if (dv.Count > 0 && Convert.ToBoolean(item["isSelected"].ToString()) == true)
                    {
                        DataRow drmo = dv[0].Row;
                        drmo.BeginEdit();
                        drmo["Minute"] = item["Minute"];
                        drmo["UpdatedBy"] = identity.Name;
                        drmo["UpdatedDate"] = DateTime.Now.ToString();
                        drmo["UpdatedFromIP"] = identity.IPAddress;

                        drmo.EndEdit();
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        drmo.Delete();
                    }
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                return Json(new { Data = data, Message = AplosMessage.Updated });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region AddDefaultColumn
        private void AddNewMachineMasterTransactionRow(DataTable dt, Dictionary<string, object> sourceData)
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

        private void EditMachineMasterTransactionRow(DataRow dr, Dictionary<string, object> sourceData)
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
        #endregion AddDefaultColumn
        [Authorize, HttpGet]
        public JsonResult GetMachineMasterTransaction()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"SELECT  MMT.Id, MMT.EntityId, MMT.DetentionId,  MMT.ProcessId,  MMT.ShiftId
                        ,E.UserName Entity,DM.DetentionUserName Detention, FORMAT(MMT.Date,'dd-MMM-yyyy')[Date],P.UserName Process, CONVERT(VARCHAR(5), MMT.FromTime, 108)FromTime,CONVERT(VARCHAR(5), MMT.ToTime, 108) ToTime,MMT.Minute,SD.UserName Shift
                        ,MMT.Remark,MMT.WorkCenterId,WC.UserName as WorkCenter,MMT.DetentionCodeId,DM.DetentionCode DetentionCode, EI.EmployeeName ResponsiblePerson,EI.EmployeeCode ResponsiblePersonCode, EI.SystemId ResponsiblePersonId
                        ,  MMT.Remark ,MMT.AddedBy, MMT.AddedDate, MMT.AddedFromIP, MMT.UpdatedBy, MMT.UpdatedDate, MMT.UpdatedFromIP
			                            from MachineMasterTransaction MMT
			                            left join ORG.Entity E on E.Id=MMT.EntityId										
										left join DetentionMaster DM on DM.Id=MMT.DetentionId									
										left join HKP.Process P on P.Id=MMT.ProcessId
										left join ShiftDefination SD on SD.SystemID=MMT.ShiftId
										left Join SCS.WorkCenterMaster WC on WC.id=MMT.WorkCenterId
										left join EmployeeInformation EI on EI.SystemId=MMT.ResponsiblePersonId
										where --MMT.addedby in ('devendras', 'talwinders') and 
                                        Format(MMT.AddedDate, 'dd-MMM-yyyy') between dateadd(month,datediff(month,0,getdate()),0)
										and dateadd(day,-1,dateadd(month,datediff(month,-1,getdate()),0))
                                        order by FORMAT(MMT.AddedDate, 'dd-MMM-yyyy') DESC";
            var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        public ActionResult Delete(string id)
        {
            string strUSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strUSQL = "delete dbo.MachineMasterTransaction Where Id='" + id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strUSQL, true, "1");
                objCon.CommitTransaction();

                return Json(new { Message = AplosMessage.Deleted });
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }
    }
}