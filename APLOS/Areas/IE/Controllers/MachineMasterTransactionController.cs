#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Setups;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.IE.Controllers
{
    public class MachineMasterTransactionController : BaseController
    {

       
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        public MachineMasterTransactionController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor


        public ActionResult Aplos()
        {
            return View();
        }
        //Omar start
        [Authorize, HttpPost]
        public ActionResult GetEntity()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"Select e.Id as EntityId, e.UserName as EntityName , p.UserName as Plant, c.UserName as Company from org.Entity e
                                left join org.Plant p on p.Id = e.PlantId
                                left join org.Company c on c.Id = p.CompanyId";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetDepartment()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select D.Id DepartmentId,D.Code,D.Sequence,D.ShortName,D.StandardName
						                ,D.UserName DepartmentName,D.Description,D.Remarks 
						                from ORG.Department D";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetDetentionDepartment()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select distinct DD.DepartmentId,D.Code,D.Sequence,D.ShortName,D.StandardName
,D.UserName DepartmentName,D.Description,D.Remarks from DetentionMasterDepartment DD
left outer join ORG.Department D on D.id=DD.DepartmentId";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetDetentionResponsible(string detentionId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select distinct E.SystemId as ResponsiblePersonId,E.EmployeeCode,E.EmployeeName as ResponsiblePerson,DEP.UserName AS Department,S.UserName as Section,
  SS.UserName as SubSection,DEG.UserName AS [LegalDesignation],DR.DetentionMasterId from DetentionMasterResponsible DR
left join EmployeeInformation AS E ON E.SystemId=DR.ResponsibleMasterId
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
							LEFT JOIN HKP.LegalDesignation AS DEG ON DEG.Id=E.LegalDesignationId
                            LEFT JOIN ORG.Department AS DEP ON DEP.id=PR.DepartmentId
							LEFT OUTER JOIN ORG.Section S ON S.Id=PR.SectionId
							LEFT OUTER JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
where DetentionMasterId='" + detentionId + "'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult GetShift()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select SD.SystemID ShiftId,P.Id PlantId,P.UserName Plant,SD.ShiftDefinationDescription
						,SD.UserName ShiftDefination,SD.InTime,SD.OutTime
						
						from ShiftDefination SD
						left join ORG.Plant P on P.Id=SD.PlantID";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }


        [Authorize, HttpPost]
        public ActionResult GetMachine()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select MM.Id MachineMasterId,MM.Sequence,MM.Code,MM.ShortName 
						                ,MM.StandardName,MM.UserName MachineMaster
						                from mst.MachineMaster MM";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetMinute(MachineMasterTransaction data)
        {
            var ts = data.ToTime.Subtract(data.FromTime);
            return Json(ts.TotalMinutes, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult GetProcess(string machineMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string str = @"select P.Id,P.Sequence,P.Code,P.ShortName,P.StandardName,P.Id ProcessId,P.UserName Process
			                            from MachineMasterProcess MMP
			                            left join HKP.Process P on P.Id=MMP.ProcessId
										where MMP.MachineMasterId='" + machineMasterId + @"'";

            return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetDetentionList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"Select DetentionUserName As Text, Id As Value,IsAssetApplicable,IsWorkCenterApplicable,IsMachineParaApplicable from DetentionMaster";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetDetentionListWC(string DetentiontypeId)
        {
            var sql = "";
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            
            sql = @"Select distinct DetentionUserName As Text, Id As Value,IsAssetApplicable,IsWorkCenterApplicable from DetentionMaster where DetentionTypeId='" + DetentiontypeId + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetDetentionTypeList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"Select DT.UserName As Text, DT.Id As Value from hkp.DetentionType DT";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult GetDetentionCodeList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"Select DetentionCode As Text, Id As Value,IsAssetApplicable,IsWorkCenterApplicable,IsMachineParaApplicable from DetentionMaster";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getDetentionTypeListByDepartment(string departmentid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"select distinct DT.UserName As Text, DT.Id As Value from DetentionMasterDepartment DD
                        left join DetentionMaster DM ON DM.Id=DD.DetentionMasterId
                        left join hkp.DetentionType DT ON DT.id=DM.DetentionTypeId
            where DepartmentId='" + departmentid + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult getDetentionListByDepartment(string departmentid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"select distinct DM.DetentionUserName As Text, DM.Id As Value from DetentionMasterDepartment DD
                        left join DetentionMaster DM ON DM.Id=DD.DetentionMasterId
            where DepartmentId='" + departmentid + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public JsonResult GetAssetTypeList(string machineMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = @"select MMA.Id,MMA.AssetCode,MMA.AssetName,MMA.AssetDetail,MMA.AssetReference,E.Id EntityId,E.UserName Entity
			                            from MachineMasterAsset MMA
			                            left join ORG.Entity E on E.Id=MMA.EntityId
										where MMA.MachineMasterId='" + machineMasterId + @"'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data, List<Dictionary<string, object>> DetentionParaList)
        {
            try
            {
                SaveMachineMasterTransactionData(data);
                if (DetentionParaList != null)
                {
                    SaveMasterOrderItemCostingRateData(DetentionParaList,data["Id"].ToString());
                }

                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }

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
        private void SaveMachineMasterTransactionData(Dictionary<string, object> data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMasterOrder;
            string id = string.Empty;
            try
            {
                string mosql = "SELECT * FROM MachineMasterTransaction WHERE Id ='" + data["Id"] + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(mosql, out dsMasterOrder, false, "1");

                string cId = string.Empty;
                string MachineMasterTransactionId = "";



                DataView dv = new DataView(dsMasterOrder.Tables[0]);
                dv.RowFilter = "Id='" + data["Id"] + "'";

                if (dsMasterOrder.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "MachineMasterTransaction", out MachineMasterTransactionId);

                    data["Id"] = MachineMasterTransactionId;
                    AddNewMachineMasterTransactionRow(dsMasterOrder.Tables[0], data);
                }
                else
                {
                    data["Id"] = data["Id"];
                    EditMachineMasterTransactionRow(dsMasterOrder.Tables[0].Rows[0], data);
                }
              
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMasterOrder);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void SaveMasterOrderItemCostingRateData(List<Dictionary<string, object>> data, string masterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (string.IsNullOrEmpty(masterId))
                {
                    throw new Exception("Select Line Item.");
                }
                #region FUND 
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster = null;
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.DetentionMasterMachineParameterValue where  MachineMasterTransactionId='" + masterId + "'", out dsMaster, false, "1");
                int idc = 0;
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        idc++;
                        DataView dv = new DataView(dsMaster.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = masterId + idc;
                            item["MachineMasterTransactionId"] = masterId;

                            AddNewRow(dsMaster.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            item["MachineMasterTransactionId"] = masterId;
                            EditRow(drmo, item);
                        }
                    }
                }

                #endregion

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);


            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        [Authorize, HttpGet]
        public JsonResult GetMachineMasterTransaction()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"SELECT MMT.Id, MMT.EntityId, MMT.DetentionId, MMT.DetentionTypeId, MMT.MachineMasterId, MMT.ProcessId, MMT.DepartmentId, MMT.ShiftId, MMT.AssetId, MMT.ResponsiblePersonId, MMT.Remark, MMT.AddedBy, MMT.AddedDate, MMT.AddedFromIP, MMT.UpdatedBy, MMT.UpdatedDate, MMT.UpdatedFromIP
,E.UserName Entity,D.UserName Department,DM.DetentionUserName Detention,DT.UserName DetentionType,FORMAT(MMT.Date,'dd-MMM-yyyy')[Date],MM.UserName MachineMaster,P.UserName Process
										,CONVERT(varchar(5),MMT.FromTime,108)FromTime,CONVERT(VARCHAR(5), MMT.ToTime, 108) ToTime,MMT.Minute,SD.UserName Shift
										,MMA.Id AssetId,MMA.AssetName Asset,EI.EmployeeName ResponsiblePerson,EI.EmployeeCode ResponsiblePersonCode,MMT.Remark,MMT.WorkCenterId,WC.UserName as WorkCenter,
                                         MMT.DetentionCodeId,DM.DetentionCode DetentionCode
			                            from MachineMasterTransaction MMT
			                            left join ORG.Entity E on E.Id=MMT.EntityId
										left join ORG.Department D on D.Id=MMT.DepartmentId
										left join DetentionMaster DM on DM.Id=MMT.DetentionId
										left join MST.MachineMaster MM on MM.Id=MMT.MachineMasterId
										left join HKP.Process P on P.Id=MMT.ProcessId
										left join ShiftDefination SD on SD.SystemID=MMT.ShiftId
										left join MachineMasterAsset MMA on MMA.Id=MMT.AssetId
										left Join hkp.DetentionType DT ON DT.Id=MMT.DetentionTypeId
                                        left Join SCS.WorkCenterMaster WC on WC.id=MMT.WorkCenterId
										left join EmployeeInformation EI on EI.SystemId=MMT.ResponsiblePersonId";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpGet]
        public JsonResult GetDetentionParameter(string DetentionId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select DM.DetentionUserName as Detention,MMT.DetentionId,MMT.Id from MachineMasterTransaction MMT
                        left Join DetentionMaster DM ON DM.id=MMT.DetentionId where MMT.DetentionId='"+ DetentionId + "' and DM.IsMachineParaApplicable=1";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        //Omar End
        [HttpGet, Authorize]
        public JsonResult GetWCCbo(string entityId, string processId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var sql = "";
            if (entityId != "null" && processId != "null")
            {
            sql = @"SELECT Id,UserName FROM SCS.WorkCenterMaster WHERE PlantId='" + identity.PlantId + "'  AND EntityId='" + entityId + "' AND CompanyId = '" + identity.CompanyId + "' AND ProcessId = '"+ processId +"' Order by Sequence";
            }
            else
            {
                sql = @"SELECT Id,UserName FROM SCS.WorkCenterMaster WHERE PlantId='" + identity.PlantId + "' AND CompanyId = '" + identity.CompanyId + "' Order by Sequence";
            }
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public JsonResult GetEntityProcessByWorkCenter(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
           
             var  sql = @"SELECT distinct E.UserName as Entity,P.UserName as Process,WC.EntityId,WC.ProcessId,WC.Id,WC.UserName as Workcenter 
                           FROM SCS.WorkCenterMaster WC
                           left Join Org.Entity E ON E.Id=WC.EntityId
                           left join hkp.Process P ON P.Id=WC.ProcessId
                           where WC.Id='" + id + "'";
            
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

        }

        [HttpGet, Authorize]
        public JsonResult GetDetentionTypeById(string id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select DT.UserName As DetentionType, DT.Id As DetentionTypeId from DetentionMaster DM
                        left join hkp.DetentionType DT ON DT.Id=DM.DetentionTypeId
                        where DM.Id='" + id + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

        }
        [HttpGet, Authorize]
        public JsonResult GetDetentionTypeAndDetentionByCode(string code)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            var sql = @"select DT.UserName As DetentionType, DT.Id As DetentionTypeId,DM.DetentionUserName as Detention,DM.Id as DetentionId from DetentionMaster DM
                        left join hkp.DetentionType DT ON DT.Id=DM.DetentionTypeId
                        where DM.DetentionCode='" + code + "'";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);

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
            //dr["UpdatedBy"] = identity.Name;
            //dr["UpdatedDate"] = System.DateTime.Now.ToString();
            //dr["UpdatedFromIP"] = identity.IPAddress;

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



    }

    public class MachineMasterTransaction
    {
        #region Scalar Properties

        public string Id { get; set; }
        public string EntityId { get; set; }
        public string DetentionId { get; set; }
        public string DetentionTypeId { get; set; }
        public string MachineMasterId { get; set; }
        public string ProcessId { get; set; }
        public DateTime Date { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }
        public int Minute { get; set; }
        public string ShiftId { get; set; }
        public string AssetId { get; set; }
        public string ResponsiblePersonId { get; set; }
        public string Remarks { get; set; }

        #endregion Scalar Properties

        #region Audit Properties

        /// <summary>
        ///This is  AddedBy.Who add data keep track by AddedBy.
        /// </summary>
        [NeverUpdate]
        public string AddedBy { get; set; }

        /// <summary>
        ///This is  AddedDate.Added date keep track by AddedDate.
        /// </summary>
        [NeverUpdate]
        public DateTime AddedDate { get; set; }

        /// <summary>
        /// Record insert by user from IP address.
        /// </summary>
        [NeverUpdate]
        public string AddedFromIP { get; set; }

        /// <summary>
        /// Record updated user name.
        /// </summary>
        public string UpdatedBy { get; set; }

        /// <summary>
        /// Record updated by user date and time.
        /// </summary>
        public DateTime? UpdatedDate { get; set; }

        /// <summary>
        /// Record updated by user IP address.
        /// </summary>
        public string UpdatedFromIP { get; set; }

        #endregion Audit Properties
    }
}