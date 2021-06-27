#region Using
using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using System;
using System.Data;
using OTSBD;
using clsAttendance;
using System.Collections.Generic;

#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class AttendanceBonusPolicyController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IStoppageService _stoppageService;
        public AttendanceBonusPolicyController(
              IStoppageService stoppageService,
              ISqlRepository sqlRepository
            )
        {
            _stoppageService = stoppageService;
            _sqlRepository = sqlRepository;
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        
        [HttpGet]
        public ActionResult GetList(string plantid)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select M.ID AS MID,M.AttenBnsPolicyName,M.AttenBnsPolicyDescription,M.PlantID,M.GroupID,p.CompanyId
                            from [dbo].[AttdnBonusPmtPolicyMaster] as M 
                            left join ORG.Plant p on p.Id = M.PlantID
                            WHERE M.PlantID='" + plantid + "' ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetDetailsList( string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT M.ID AS MID,D.ID ,D.IsEarlyOutApplicable,D.IsLunchOutApplicable,D.IsLateInApplicable,D.IsLunchOutApplicable
                                ,D.IsAbsentApplicable,D.IsLateApplicable,D.IsRouteApplicableForLate
                            ,D.IsLeaveApplicable,D.IsLeaveWithOutPayApplicable,D.EOLIFromValue,D.EOLIToValue,D.LunchOutFromValue,D.LunchOutToValue,D.AbsentFromValue
                            ,D.AbsentToValue,D.LateFromValue,D.LateToValue,D.LeaveFromValue,D.LeaveToValue,D.LeaveWithOutPayFromValue,D.LeaveWithOutPayToValue 
                            ,D.FixedOrFormula,D.EOLIFromValue,D.FormulaDesID,D.FormulaDes,D.FixedValue,D.IsFixed,D.IsFormula	                        
                            FROM [dbo].[AttdnBonusPmtPolicyDetails] D
                            LEFT JOIN [dbo].[AttdnBonusPmtPolicyMaster] M ON M.ID=D.AttdnBonusPmtPolicyID                           
                            WHERE D.AttdnBonusPmtPolicyID ='" + MasterId+@"' ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetLeaveDetailsChildList(string DetailsId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"  select Active=case when  D.Id is null then  CONVERT(bit,0) else  CONVERT(bit,1) end ,LT.Id,LT.LeaveTypeId as LeaveId,LT.IsPreApplied,LET.UserName
                                FROM [dbo].[AttdnBonusPmtPolicyDetails] D
                                LEFT JOIN [dbo].[AttdnBonusLeaveType] LT on  LT.AttdnBonusPmtPolicyDetailsId=D.ID
							LEFT JOIN LeaveType LET ON LET.Id=LT.LeaveTypeId
							where lt.AttdnBonusPmtPolicyDetailsId='" + DetailsId +@"'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetLeaveList( string AttdnBonusPmtPolicyDetailsId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" select distinct UserName,lt.Id as LeaveId ,  IsPreApplied = ABLT.IsPreApplied 
                            ,CheckBoxSelect=case when  ABLT.AttdnBonusPmtPolicyDetailsId is null then  CONVERT(bit,0) else  CONVERT(bit,1) end 
                             ,ABLT.AttdnBonusPmtPolicyDetailsId,ABLT.AttdnBonusPmtPolicyMasterId
									from LeaveType lt
									LEFT JOIN LeavePolicyDetail LPD ON LPD.LTSystemID=LT.Id
                                    LEFT JOIN LeavePolicyMaster LPM ON LPM.SystemID=LPD.LPMSystemID
									  LEFT JOIN (SELECT DC.LeavePolicyMasterId,DM.DesignationId FROM MST.DesignationMaster DM
                                    LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                    WHERE DC.PlantId='" + identity.PlantId + @"') DM ON DM.LeavePolicyMasterId=LPM.SystemID
										LEFT JOIN [dbo].[AttdnBonusLeaveType] ABLT ON ABLT.LeaveTypeId=LT.Id and ABLT.AttdnBonusPmtPolicyDetailsId='"+ AttdnBonusPmtPolicyDetailsId +@"'
									 where CompanyGroupId='" + identity.CompanyGroupId + @"' ";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        
        [HttpPost, Authorize]
        public ActionResult Save(AttdnBonusPmtPolicyDetails Details , List<AttdnBonusLeaveType> LeaveList, string MasterId)
        {
            try
            {
                string DetailsId = string.Empty;
                DetailsId=SaveDetails(Details);
                if (LeaveList != null)
                {
                    SaveChild(LeaveList, DetailsId, MasterId);

                }
                else
                {
                    DataSet dsLeaveType;
                    ConnectionManager.DAL.ConManager objCon;
                    string sql1 = "DELETE FROM [dbo].[AttdnBonusLeaveType] WHERE AttdnBonusPmtPolicyDetailsId='" + DetailsId + "' ";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql1, out dsLeaveType, false, "1");
                }

                return Json(new {Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpPost]
        public ActionResult SaveM(AttdnBonusPmtPolicyMaster Master)
        {
            try
            {
                string MasterId = string.Empty;               
                MasterId = SaveMaster(Master);              
                return Json(new { MasterId,Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpPost, Authorize]
        public string SaveMaster(AttdnBonusPmtPolicyMaster Master)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                string Id = string.Empty;

                string sql = "SELECT * FROM [dbo].[AttdnBonusPmtPolicyMaster] WHERE ID='" + Master.MID + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[dbo].[AttdnBonusPmtPolicyMaster]", out sID);
                    Id = "APM" + sID;
                    dr["ID"] = Id;
                    dr["GroupID"] = identity.CompanyGroupId;
                    dr["PlantID"] = Master.PlantID;
                    dr["AttenBnsPolicyName"] = Master.AttenBnsPolicyName;
                    dr["AttenBnsPolicyDescription"] = Master.AttenBnsPolicyDescription;
                    
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    Id = dr["ID"].ToString();

                    dr["GroupID"] = identity.CompanyGroupId;
                    dr["PlantID"] = Master.PlantID;
                    dr["AttenBnsPolicyName"] = Master.AttenBnsPolicyName;
                    dr["AttenBnsPolicyDescription"] = Master.AttenBnsPolicyDescription;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                return Id;
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost, Authorize]
        public string SaveDetails(AttdnBonusPmtPolicyDetails Details)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                string DetailsId = string.Empty;
                string sql = "SELECT * FROM [dbo].[AttdnBonusPmtPolicyDetails] WHERE ID='" + Details.ID + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (Details.FixedOrFormula == "Fixed")
                {
                    Details.IsFixed = true;
                    Details.IsFormula = false;
                }
                else
                {
                    Details.IsFormula = true;
                    Details.IsFixed = false;
                }

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[dbo].[AttdnBonusPmtPolicyDetails]", out sID);
                    DetailsId = "APC" + sID;
                    dr["ID"] = DetailsId;
                    dr["AttdnBonusPmtPolicyID"] = Details.MID;
                    dr["FixedOrFormula"] = Details.FixedOrFormula;
                    dr["IsFixed"] = Details.IsFixed;
                    dr["IsFormula"] = Details.IsFormula;
                    dr["FormulaDes"] = Details.FormulaDes;
                    dr["FormulaDesID"] = Details.FormulaDesID;
                    dr["IsEarlyOutApplicable"] = Details.IsEarlyOutApplicable;
                    dr["IsLateInApplicable"] = Details.IsLateInApplicable;
                    dr["IsLunchOutApplicable"] = Details.IsLunchOutApplicable;
                    dr["IsAbsentApplicable"] = Details.IsAbsentApplicable;
                    dr["IsLateApplicable"] = Details.IsLateApplicable;
                    dr["IsRouteApplicableForLate"] = Details.IsRouteApplicableForLate;
                    dr["IsLeaveApplicable"] = Details.IsLeaveApplicable;
                    dr["IsLeaveWithOutPayApplicable"] = Details.IsLeaveWithOutPayApplicable;
                    dr["FixedValue"] = Details.FixedValue;
                    dr["EOLIFromValue"] = Details.EOLIFromValue;
                    dr["EOLIToValue"] = Details.EOLIToValue;
                    dr["LunchOutFromValue"] = Details.LunchOutFromValue;
                    dr["LunchOutToValue"] = Details.LunchOutToValue;
                    dr["AbsentFromValue"] = Details.AbsentFromValue;
                    dr["AbsentToValue"] = Details.AbsentToValue;
                    dr["LateFromValue"] = Details.LateFromValue;
                    dr["LateToValue"] = Details.LateToValue;
                    dr["LeaveFromValue"] = Details.LeaveFromValue;
                    dr["LeaveToValue"] = Details.LeaveToValue;
                    dr["LeaveWithOutPayFromValue"] = Details.LeaveWithOutPayFromValue;
                    dr["LeaveWithOutPayToValue"] = Details.LeaveWithOutPayToValue;

                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    DetailsId = dr["ID"].ToString();

                    dr["AttdnBonusPmtPolicyID"] = Details.MID;
                    dr["FixedOrFormula"] = Details.FixedOrFormula;
                    dr["IsFixed"] = Details.IsFixed;
                    dr["IsFormula"] = Details.IsFormula;
                    dr["FormulaDes"] = Details.FormulaDes;
                    dr["FormulaDesID"] = Details.FormulaDesID;
                    dr["IsEarlyOutApplicable"] = Details.IsEarlyOutApplicable;
                    dr["IsLateInApplicable"] = Details.IsLateInApplicable;
                    dr["IsLunchOutApplicable"] = Details.IsLunchOutApplicable;
                    dr["IsAbsentApplicable"] = Details.IsAbsentApplicable;
                    dr["IsLateApplicable"] = Details.IsLateApplicable;
                    dr["IsRouteApplicableForLate"] = Details.IsRouteApplicableForLate;
                    dr["IsLeaveApplicable"] = Details.IsLeaveApplicable;
                    dr["IsLeaveWithOutPayApplicable"] = Details.IsLeaveWithOutPayApplicable;
                    dr["FixedValue"] = Details.FixedValue;
                    dr["EOLIFromValue"] = Details.EOLIFromValue;
                    dr["EOLIToValue"] = Details.EOLIToValue;
                    dr["LunchOutFromValue"] = Details.LunchOutFromValue;
                    dr["LunchOutToValue"] = Details.LunchOutToValue;
                    dr["AbsentFromValue"] = Details.AbsentFromValue;
                    dr["AbsentToValue"] = Details.AbsentToValue;
                    dr["LateFromValue"] = Details.LateFromValue;
                    dr["LateToValue"] = Details.LateToValue;
                    dr["LeaveFromValue"] = Details.LeaveFromValue;
                    dr["LeaveToValue"] = Details.LeaveToValue;
                    dr["LeaveWithOutPayFromValue"] = Details.LeaveWithOutPayFromValue;
                    dr["LeaveWithOutPayToValue"] = Details.LeaveWithOutPayToValue;

                    dr.EndEdit();
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                return DetailsId;
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost, Authorize]
        public void SaveChild(List<AttdnBonusLeaveType> LeaveList, string DetailsId, string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            DataSet dsLeaveType;
            try
            {
                string sql1 = "DELETE FROM [dbo].[AttdnBonusLeaveType] WHERE AttdnBonusPmtPolicyDetailsId='" + DetailsId + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql1, out dsLeaveType, false, "1");
                foreach (var item in LeaveList)
                {
                    string sql = "SELECT * FROM [dbo].[AttdnBonusLeaveType] WHERE ID='" + item.Id + "' ";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                    
                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();
                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[dbo].[AttdnBonusLeaveType]", out sID);
                        dr["ID"] = "ALT" + sID;
                        dr["AttdnBonusPmtPolicyMasterId"] = MasterId;
                        dr["AttdnBonusPmtPolicyDetailsId"] = DetailsId;
                        dr["LeaveTypeId"] = item.LeaveId;
                        dr["IsPreApplied"] = item.IsPreApplied;                        
                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["AttdnBonusPmtPolicyMasterId"] = MasterId;
                        dr["AttdnBonusPmtPolicyDetailsId"] = DetailsId;
                        dr["LeaveTypeId"] = item.LeaveId;
                        dr["IsPreApplied"] = item.IsPreApplied;
                        dr.EndEdit();
                    }
                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);
                }
               
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet, Authorize]
        public ActionResult Delete(string SystemID)
        {
            string strChildSQL;
            string strMasterSQL;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                strChildSQL = "DELETE FROM  ShiftTimeChgChild WHERE STCMasterSystemID='" + SystemID + "'";
                strMasterSQL = "DELETE FROM  ShiftTimeChgMaster WHERE SystemID='" + SystemID + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strChildSQL, out dsExceptionEmployeeList, false, "1");
                objCon.OpenDataSetThroughAdapter(strMasterSQL, out dsExceptionEmployeeList, false, "1");

            }
            catch (Exception ex)
            {

                throw (ex);
            }

            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult DeleteM(string SystemID)
        {
            DataSet dsMaster;
            string strMasterSQL;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;

            try
            {
                string sql = "SELECT * FROM [dbo].[AttdnBonusPmtPolicyDetails] WHERE AttdnBonusPmtPolicyID='" + SystemID + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    Exception ex = new Exception("Please Delete Details First....");
                    throw (ex);
                }
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                strMasterSQL = "DELETE FROM [dbo].[AttdnBonusPmtPolicyMaster] WHERE ID='" + SystemID + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strMasterSQL, out dsExceptionEmployeeList, false, "1");

            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public ActionResult DeleteDetails(string DetailsId)
        {
            string strDetailsSQL;
            string strChildSQL;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                strChildSQL = "DELETE FROM [dbo].[AttdnBonusLeaveType] WHERE AttdnBonusPmtPolicyDetailsId='" + DetailsId + "'";
                strDetailsSQL = "DELETE FROM  [dbo].[AttdnBonusPmtPolicyDetails] WHERE ID='" + DetailsId + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strChildSQL, out dsExceptionEmployeeList, false, "1");
                objCon.OpenDataSetThroughAdapter(strDetailsSQL, out dsExceptionEmployeeList, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }
        public class AttdnBonusPmtPolicyMaster : BaseModel
        {
            #region Scalar Properties            
            public string ID { get; set; }
            public string GroupID { get; set; }
            public string PlantID { get; set; }
            public string AttenBnsPolicyName { get; set; }
            public string AttenBnsPolicyDescription { get; set; }

            public int FixedValue { get; set; }
            public string FormulaDes { get; set; }
            public string FormulaDesID { get; set; }
            public string DayType { get; set; }
            public string DayTypeOperator { get; set; }
            public int DayTypeOperatorValue { get; set; }
            public string LeaveType { get; set; }
            public string LeaveAppType { get; set; }
            public int SequenceNo { get; set; }
            public bool IsDisbusted { get; set; }
            public string MID { get; set; }
            #endregion Scalar Properties

            #region Audit Properties
            [NeverUpdate]
            public string AddedBy { get; set; }
            [NeverUpdate]
            public DateTime? AddedDate { get; set; }
            public string AddedFromIP { get; set; }

            public string UpdatedBy { get; set; }
            public DateTime? UpdatedDate { get; set; }
            public string UpdatedFromIP { get; set; }

            #endregion Audit Properties
        }

        public class AttdnBonusPmtPolicyDetails : BaseModel
        {
            #region Scalar Properties            
            public string ID { get; set; }
            public string GroupID { get; set; }
            public string PlantID { get; set; }
            public string AttdnBonusPmtPolicyID { get; set; }
            public bool IsFixed { get; set; }
            public bool IsFormula { get; set; }
            public int FixedValue { get; set; }
            public string FormulaDes { get; set; }
            public string FormulaDesID { get; set; }
            public int SequenceNo { get; set; }
            public string MID { get; set; }
            public string FixedOrFormula { get; set; }
            public bool IsEarlyOutApplicable { get; set; }
            public bool IsLateInApplicable { get; set; }
            public bool IsLunchOutApplicable { get; set; }
            public bool IsAbsentApplicable { get; set; }
            public bool IsLateApplicable { get; set; }
            public bool IsRouteApplicableForLate { get; set; }
            public bool IsLeaveApplicable { get; set; }
            public bool IsLeaveWithOutPayApplicable { get; set; }

            public string EOLIFromValue { get; set; }
            public string EOLIToValue { get; set; }
            public string LunchOutFromValue { get; set; }
            public string LunchOutToValue { get; set; }
            public string AbsentFromValue { get; set; }

            public string AbsentToValue { get; set; }
            public string LateFromValue { get; set; }
            public string LateToValue { get; set; }
            public string LeaveFromValue { get; set; }
            public string LeaveToValue { get; set; }
            public string LeaveWithOutPayFromValue { get; set; }
            public string LeaveWithOutPayToValue { get; set; }

            #endregion Scalar Properties

        }
        public class AttdnBonusLeaveType : BaseModel
        {
            #region Scalar Properties            
            public string Id { get; set; }
            public string AttdnBonusPmtPolicyMasterId { get; set; }
            public string AttdnBonusPmtPolicyDetailsId { get; set; }
            public string LeaveTypeId { get; set; }
            public bool IsPreApplied { get; set; }
            public string LeaveId { get; set; }
            public string UserName { get; set; }
            #endregion Scalar Properties
        }
        #endregion
    }
}