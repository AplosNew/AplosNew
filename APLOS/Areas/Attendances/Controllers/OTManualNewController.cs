#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Setups;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;

using Library.HumanResource.NewAttendanceProcess;

#endregion Using

namespace Aplos.Areas.Attendances.Controllers
{
    public class OTManualNewController : BaseController
    {
        string TableName = "dbo.OTfromApp";
      
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public OTManualNewController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor
       
    
        public ActionResult Aplos()
        {
            return View();
        }

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id as Value,UserName AS Text FROM "+ TableName +"  "), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getplant()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_sqlRepository.GetDataCollection("select Id as Value,UserName as Text from ORG.Plant where Id='" + identity.PlantId + @"' order by UserName"), JsonRequestBehavior.AllowGet);

        }

        [Authorize, HttpGet]
        public JsonResult getentity()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return Json(_sqlRepository.GetDataCollection("select Id as Value,UserName as Text from ORG.Entity where PlantId='" + identity.PlantId + @"' order by UserName"), JsonRequestBehavior.AllowGet);

        }

        [Authorize, HttpGet]
        public JsonResult getdepartment()
        {
            return Json(_sqlRepository.GetDataCollection("select Id as Value,UserName as Text from Org.Department order by UserName"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getsection()
        {
            return Json(_sqlRepository.GetDataCollection("select Id as Value,UserName as Text from org.Section order by UserName"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getsubsection()
        {
            return Json(_sqlRepository.GetDataCollection("select Id as Value,UserName as Text from org.SubSection order by UserName"), JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpGet]
        public JsonResult getshift(string PlantId)
        {
        
            return Json(_sqlRepository.GetDataCollection("SELECT SystemID as Value,ShiftDefinationDescription AS Text FROM dbo.ShiftDefination where PlantId='"+ PlantId + @"' order by ShiftDefinationDescription"), JsonRequestBehavior.AllowGet);

        }


        [Authorize, HttpPost]
        public ActionResult Get(string Id)
        {
            try
            {
                var _master = _sqlRepository.GetDataCollection("select * from dbo.OTfromApp where Id = '" + Id + "' ");


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
           

            string sql = @"Select top 100 * from (select distinct ot.*,FORMAT(ot.WorkDate,'dd-MMM-yyyy') as OTWorkDate,ei.SystemId,ei.EmployeeCode, ei.EmployeeName as EmpName,ei.EmployeeStatus, p.UserName as Plant
                                                                    from AttdnProcessData ot
                                                                    left join dbo.EmployeeInformation ei on ei.SystemId=ot.EmpSystemId
																	left join ORG.Plant p on p.Id=ei.PlantId
																	where ei.PlantId='" + identity.PlantId + @"')
                                                                    as Temp
																    WHERE " + strkey + " order by Temp.WorkDate desc ";

          return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data, IEnumerable<OTfromApp> SaveMultipleEmpOT)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {   
                    DataSet EmpExistOrNot;
                    DataSet EmpDayStatus;
                    DataSet IsEmpSalaryLocked;
                    DataSet EmpExistInAttProData;
                    string RowsEdit = "''";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                var empdetails = "' '";
                foreach (var empitem in SaveMultipleEmpOT)
                {
                    empdetails += ",'"+ empitem.EmployeeSystemId +"' ";
                }
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where EmpSystemId IN ( "+ empdetails + " ) and WorkDate='"+ data["WorkDate"] +"' ", out EmpExistOrNot, false, "1");
                con.OpenDataSetThroughAdapter("select * from AttdnProcessData where EmpSystemId IN ( " + empdetails + " ) and WorkDate='" + data["WorkDate"] + "' ", out EmpExistInAttProData, false, "1");

         //       con.OpenDataSetThroughAdapter("select apd.EmpSystemID,apd.DayStatus, dt.Category from AttdnProcessData apd left join DayType dt on apd.DayStatus=dt.DayType where apd.EmpSystemID IN ( " + empdetails + " ) and apd.WorkDate ='" + data["WorkDate"] + "' ", out EmpDayStatus, false, "1");

                string EmpYear = Convert.ToDateTime(data["WorkDate"]).ToString("yyyy");
                string EmpMonth = Convert.ToDateTime(data["WorkDate"]).ToString("MM");
                con.OpenDataSetThroughAdapter("select Id, EmpSystemId, YearNo, MonthNo, IsLocked from SalaryLock where YearNo = '" + EmpYear + "' and MonthNo = '" + EmpMonth + "' and EmpSystemId IN ( " + empdetails + " ) ", out IsEmpSalaryLocked, false, "1");

                // new validation of Day Lock

                DataTable dtLock = _sqlRepository.GetDataTable("SELECT * FROM PlantWiseAttendanceLock AS pwal WHERE  isActive=1 AND pwal.LockedDate ='" + data["WorkDate"] + "' AND pwal.PlantId='" + identity.PlantId + "'");
                DataTable dtLockEmployee = _sqlRepository.GetDataTable("SELECT * FROM ExceptionEmployeeAttendanceUnlock WHERE EmpSystemId IN (" + empdetails + @")");

                for (int i = 0; i < dtLock.Rows.Count; i++)
                {
                    foreach (var item in SaveMultipleEmpOT)
                    {
                        dtLockEmployee.DefaultView.RowFilter = "EmpSystemId='" + item.EmployeeSystemId + "' AND WorkDate=#" + data["WorkDate"] + "#";
                        if (dtLockEmployee.DefaultView.Count == 0)
                        {
                            throw new Exception("" + item.EmployeeName + " " + item.Code + " Day Locked");
                        }
                    }
                }


                foreach (var item in SaveMultipleEmpOT)
                {
                    IsEmpSalaryLocked.Tables[0].DefaultView.RowFilter = "EmpSystemId='" + item.EmployeeSystemId + "'";
                    bool islocked = false;
                    if (IsEmpSalaryLocked.Tables[0].DefaultView.Count > 0)
                    {
                        islocked = bplib.clsWebLib.GetBoolData(IsEmpSalaryLocked.Tables[0].DefaultView[0]["IsLocked"].ToString());
                        throw new Exception(" " + item.EmployeeName + " " + item.Code + " Salary is Locked for the Month");

                    }
                    if (islocked==false)
                        {
                        //      EmpDayStatus.Tables[0].DefaultView.RowFilter = "EmpSystemID ='" + item.EmployeeSystemId + "'";

                        //if(EmpDayStatus.Tables[0].DefaultView.Count>0)
                        //    {
                        //if (EmpDayStatus.Tables[0].DefaultView[0]["Category"].ToString() == "Present" || EmpDayStatus.Tables[0].DefaultView[0]["Category"].ToString() == "Late" || EmpDayStatus.Tables[0].DefaultView[0]["Category"].ToString() == "Weekend" || EmpDayStatus.Tables[0].DefaultView[0]["Category"].ToString() == "Holiday")

                        //{

                        string newformat = Convert.ToDateTime(data["WorkDate"]).ToString("yyyyMMdd");

               //         dsMaster.Tables[0].DefaultView.RowFilter = "RowId='" + newformat + EmpSystemid + "'";

                //        EmpExistInAttProData.Tables[0].DefaultView.RowFilter = "EmpSystemID ='" + item.EmployeeSystemId + "'";
                        EmpExistInAttProData.Tables[0].DefaultView.RowFilter = "EmpSystemID ='" + item.EmployeeSystemId + "' and RowId='" + newformat + item.EmployeeSystemId + "' ";

                        if (EmpExistInAttProData.Tables[0].DefaultView.Count != 0)
                          {
                            bool ManFlag = true;
                            //edit
                            DataRow dr = EmpExistInAttProData.Tables[0].DefaultView[0].Row;

                            dr.BeginEdit();

                            dr["ManualOt"] = data["OThour"];

                            dr["ManualByWhom"] = identity.Name;
                            dr["ManualEntryTime"] = System.DateTime.Now.ToString();
                            dr["ManualFlag"] = ManFlag;

                            dr["OTComfirmBy"] = DBNull.Value;
                            dr["DateOTComfirm"] = DBNull.Value;
                            dr["IsOTComfirm"] = false;

                            dr["PlanOT"] = DBNull.Value;
                            dr["AllowedOTLimit"] = DBNull.Value;
                            dr["AppliedOTLimit"] = DBNull.Value;
                            dr["StandardOT"] = DBNull.Value;
                            dr["AdditionalOT"] = DBNull.Value;
                            dr["TargetOT"] = DBNull.Value;

                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = System.DateTime.Now.ToString();

                            dr.EndEdit();
                            RowsEdit = RowsEdit + ",'" + dr["RowId"].ToString() + "'";
                        }

                        else
                        {
                            EmpExistOrNot.Tables[0].DefaultView.RowFilter = "EmpSystemID ='" + item.EmployeeSystemId + "'";
                    //        EmpExistInAttProData.Tables[0].DefaultView.RowFilter = "EmpSystemID ='" + item.EmployeeSystemId + "' and RowId='" + newformat + item.EmployeeSystemId + "' ";

                            if (EmpExistOrNot.Tables[0].DefaultView.Count == 0)
                            {
                                DataRow dr = EmpExistOrNot.Tables[0].NewRow();
                                dr["Id"] = "OT" + GetOTPK();

                                dr["WorkDate"] = data["WorkDate"];

                                dr["OThour"] = data["OThour"];
                                dr["EmpSystemId"] = item.EmployeeSystemId;

                                dr["Remarks"] = data["Remarks"];
                                dr["IsConfirmed"] = data["IsConfirmed"];

                                dr["AddedBy"] = identity.Name;
                                dr["AddedDate"] = System.DateTime.Now.ToString();

                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();


                                EmpExistOrNot.Tables[0].Rows.Add(dr);
                        //        RowsEdit = RowsEdit + ",'" + dr["RowId"].ToString() + "'";
                            }
                            else
                            {

                                //edit
                                DataRow dr = EmpExistOrNot.Tables[0].DefaultView[0].Row;

                                dr.BeginEdit();

                                dr["WorkDate"] = data["WorkDate"];

                                dr["OThour"] = data["OThour"];
                                dr["EmpSystemId"] = item.EmployeeSystemId;

                                dr["Remarks"] = data["Remarks"];
                                dr["IsConfirmed"] = data["IsConfirmed"];

                                dr["AddedBy"] = identity.Name;
                                dr["AddedDate"] = System.DateTime.Now.ToString();

                                dr["UpdatedBy"] = identity.Name;
                                dr["UpdatedDate"] = System.DateTime.Now.ToString();

                                dr.EndEdit();
                         //       RowsEdit = RowsEdit + ",'" + dr["RowId"].ToString() + "'";
                            }
                        }

                        //          }
                        //         }

                    }
                      
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(EmpExistInAttProData,EmpExistOrNot);

                NewAttendanceProcessService ap = new NewAttendanceProcessService();
                ap.ManualScheduler(identity.PlantId, RowsEdit);

                return Json(new { Error = false, Message = AplosMessage.Updated });

            }
            

            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });

            }
        }

        [Authorize]
        private string GetOTPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(OTfromApp), out sID);
            return sID;
        }


    

        [HttpPost, Authorize]
        public ActionResult LoadAllEmpDetailsForSelection(string EmpCode, string EmpWorkDate, string Id, string PlantId, string DepartmentId, string SectionId, string SubSectionId)
        {
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                DataSet dsMaster;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = "";
                if (!string.IsNullOrEmpty(DepartmentId) || !string.IsNullOrEmpty(SectionId) || !string.IsNullOrEmpty(SubSectionId))
                {
                    sql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS EmployeeSystemId, EMP.EmployeeStatus,
                            EMP.EmployeeName,EMP.EmployeeCode AS Code,apd.InTime as EMPAPDInTime, CONVERT(varchar(5),apd.[InTime],108)[APDInTime],apd.OutTime as EMPAPDOutTime, CONVERT(varchar(5),apd.[OutTime],108)[APDOutTime], apd.OTHr, FORMAT(apd.WorkDate,'dd-MMM-yyyy') as APDEmpWorkDate,
							apd.DayStatus,dt.Category,FORMAT(apd.InTime,'dd-MMM-yyyy HH:mm') as APDEmpInDateAndTime,FORMAT(apd.OutTime,'dd-MMM-yyyy HH:mm') as APDEmpOutDateAndTime,
                            EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName DepartmentName,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant,PL.Id as PlantId
                            --,mo.OThour as ManualOT
							,ManualOT=case when apd.EmpSystemID is not null and apd.WorkDate is not null then apd.ManualOt else mo.OThour End
                            ,IsOTEntitled =CASE WHEN dmc.IsOTEntitled=1 THEN 'Yes' ELSE 'No' END
                            ,apd.ProcessedOT
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=pr.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=pr.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            left join mst.DesignationMasterLegalDesignation dd on dd.LegalDesignationId =D.Id
                            left join scs.DesignationMasterConfiguration dmc on dmc.DesignationMasterId=dd.DesignationMasterId and dmc.PlantId=EMP.PlantId
                            left join AttdnProcessData apd on apd.EmpSystemID=EMP.SystemId and apd.WorkDate='" + EmpWorkDate + @"'
							left join DayType dt on dt.DayType=apd.DayStatus
                            left join OTfromApp mo on mo.EmpSystemID=emp.SystemId and mo.WorkDate=apd.WorkDate
                            WHERE emp.GroupID='" + identity.CompanyGroupId + @"' and emp.doj<='" + EmpWorkDate + @"' and (dos is null or dos>='" + EmpWorkDate + @"')
                            And EMP.PlantId='" + PlantId + @"'
                            and (PR.DepartmentId='" + DepartmentId + @"' or PR.SectionId='" + SectionId + @"' or PR.SubSectionId='" + SubSectionId + @"')";

                }
                else
                {
                    sql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS EmployeeSystemId, EMP.EmployeeStatus,
                            EMP.EmployeeName,EMP.EmployeeCode AS Code,apd.InTime as EMPAPDInTime, CONVERT(varchar(5),apd.[InTime],108)[APDInTime],apd.OutTime as EMPAPDOutTime, CONVERT(varchar(5),apd.[OutTime],108)[APDOutTime], apd.OTHr, FORMAT(apd.WorkDate,'dd-MMM-yyyy') as APDEmpWorkDate,
							apd.DayStatus,dt.Category,FORMAT(apd.InTime,'dd-MMM-yyyy HH:mm') as APDEmpInDateAndTime,FORMAT(apd.OutTime,'dd-MMM-yyyy HH:mm') as APDEmpOutDateAndTime,
                            EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName DepartmentName,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant,PL.Id as PlantId
                            --,mo.OThour as ManualOT
							,ManualOT=case when apd.EmpSystemID is not null and apd.WorkDate is not null then apd.ManualOt else mo.OThour End
                            ,IsOTEntitled =CASE WHEN dmc.IsOTEntitled=1 THEN 'Yes' ELSE 'No' END
                            ,apd.ProcessedOT
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=pr.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=pr.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            left join mst.DesignationMasterLegalDesignation dd on dd.LegalDesignationId =D.Id
                            left join scs.DesignationMasterConfiguration dmc on dmc.DesignationMasterId=dd.DesignationMasterId and dmc.PlantId=EMP.PlantId
                            left join AttdnProcessData apd on apd.EmpSystemID=EMP.SystemId and apd.WorkDate='" + EmpWorkDate + @"'
							left join DayType dt on dt.DayType=apd.DayStatus
                            left join OTfromApp mo on mo.EmpSystemID=emp.SystemId and mo.WorkDate=apd.WorkDate
                            WHERE emp.GroupID='" + identity.CompanyGroupId + @"' and emp.doj<='" + EmpWorkDate + @"' and (dos is null or dos>='" + EmpWorkDate + @"')
                            And EMP.PlantId='" + PlantId + @"' ";

                }

                var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }

            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }

        [HttpPost, Authorize]
        public ActionResult getempinouttime(string EmpCode, string EmpWorkDate, string Id, string PlantId, string DepartmentId, string SectionId, string SubSectionId)
        {
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                DataSet dsMaster;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = "";
                if (!string.IsNullOrEmpty(DepartmentId) || !string.IsNullOrEmpty(SectionId) || !string.IsNullOrEmpty(SubSectionId))
                {
                        sql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS EmployeeSystemId, EMP.EmployeeStatus,
                            EMP.EmployeeName,EMP.EmployeeCode AS Code,apd.InTime as EMPAPDInTime, CONVERT(varchar(5),apd.[InTime],108)[APDInTime],apd.OutTime as EMPAPDOutTime, CONVERT(varchar(5),apd.[OutTime],108)[APDOutTime], apd.OTHr, FORMAT(apd.WorkDate,'dd-MMM-yyyy') as APDEmpWorkDate,
							apd.DayStatus,dt.Category,FORMAT(apd.InTime,'dd-MMM-yyyy HH:mm') as APDEmpInDateAndTime,FORMAT(apd.OutTime,'dd-MMM-yyyy HH:mm') as APDEmpOutDateAndTime,
                            EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName DepartmentName,S.UserName Section,
                            PR,SS.UserName SubSection
                            ,PL.UserName Plant,PL.Id as PlantId
                            --,mo.OThour as ManualOT
							,ManualOT=case when apd.EmpSystemID is not null and apd.WorkDate is not null then apd.ManualOt else mo.OThour End
                            ,dmc.IsOTEntitled
                            ,apd.ProcessedOT
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=pr.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=pr.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            left join mst.DesignationMasterLegalDesignation dd on dd.LegalDesignationId =D.Id
                            left join scs.DesignationMasterConfiguration dmc on dmc.DesignationMasterId=dd.DesignationMasterId and dmc.PlantId=EMP.PlantId
                            left join AttdnProcessData apd on apd.EmpSystemID=EMP.SystemId and apd.WorkDate='" + EmpWorkDate + @"'
							left join DayType dt on dt.DayType=apd.DayStatus
                            left join OTfromApp mo on mo.EmpSystemID=emp.SystemId and mo.WorkDate=apd.WorkDate
                            WHERE emp.GroupID='" + identity.CompanyGroupId + @"' and emp.doj<='" + EmpWorkDate + @"' and (dos is null or dos>='" + EmpWorkDate + @"')
                            And EMP.PlantId='" + PlantId + @"' and EMP.EmployeeCode='"+ EmpCode + @"'
                            and (PR.DepartmentId='" + DepartmentId + @"' or PR.SectionId='" + SectionId + @"' or PR.SubSectionId='" + SubSectionId + @"')";

                   }
                    else
                    {
                    sql = @"SELECT distinct convert(bit,0) AS isSelected, Emp.SystemID AS EmployeeSystemId, EMP.EmployeeStatus,
                            EMP.EmployeeName,EMP.EmployeeCode AS Code,apd.InTime as EMPAPDInTime, CONVERT(varchar(5),apd.[InTime],108)[APDInTime],apd.OutTime as EMPAPDOutTime, CONVERT(varchar(5),apd.[OutTime],108)[APDOutTime], apd.OTHr, FORMAT(apd.WorkDate,'dd-MMM-yyyy') as APDEmpWorkDate,
							apd.DayStatus,dt.Category,FORMAT(apd.InTime,'dd-MMM-yyyy HH:mm') as APDEmpInDateAndTime,FORMAT(apd.OutTime,'dd-MMM-yyyy HH:mm') as APDEmpOutDateAndTime,
                            EMP.BudgetCode,E.UserName EntityName,isnull(D.UserName,'') Designation,
                            PR.UserName PositionName,
                            DEPT.UserName DepartmentName,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant,PL.Id as PlantId
                            --,mo.OThour as ManualOT
							,ManualOT=case when apd.EmpSystemID is not null and apd.WorkDate is not null then apd.ManualOt else mo.OThour End
                            ,dmc.IsOTEntitled
                            ,apd.ProcessedOT
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=pr.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=pr.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            left join mst.DesignationMasterLegalDesignation dd on dd.LegalDesignationId =D.Id
                            left join scs.DesignationMasterConfiguration dmc on dmc.DesignationMasterId=dd.DesignationMasterId and dmc.PlantId=EMP.PlantId
                            left join AttdnProcessData apd on apd.EmpSystemID=EMP.SystemId and apd.WorkDate='" + EmpWorkDate + @"'
							left join DayType dt on dt.DayType=apd.DayStatus
                            left join OTfromApp mo on mo.EmpSystemID=emp.SystemId and mo.WorkDate=apd.WorkDate
                            WHERE emp.GroupID='" + identity.CompanyGroupId + @"' and emp.doj<='" + EmpWorkDate + @"' and (dos is null or dos>='" + EmpWorkDate + @"')
                            And EMP.PlantId='" + PlantId + @"' and EMP.EmployeeCode='" + EmpCode + @"' ";

                }

                var jsondata = Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }

            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);

            }

        }


        [HttpGet, Authorize]
        public JsonResult LoadEmpOfShiftWorkDate(string EmpWorkDate)
        {

            string sql = @"select ot.*,ot.OThour as ManualOT, ei.SystemId,ei.EmployeeCode as Code,ei.EmployeeName, ei.EmployeeStatus
                                    ,FORMAT(ot.WorkDate,'dd-MMM-yyyy') as APDEmpWorkDate,dt.Category, apd.OTHr	
									,FORMAT(apd.InTime,'dd-MMM-yyyy HH:mm') as APDEmpInDateAndTime,FORMAT(apd.OutTime,'dd-MMM-yyyy HH:mm') as APDEmpOutDateAndTime
									from dbo.OTfromApp ot left join dbo.EmployeeInformation ei on ei.SystemId=ot.EmpSystemId
									left join AttdnProcessData apd on apd.EmpSystemID=ot.EmpSystemId
									left join DayType dt on dt.DayType=apd.DayStatus
									where apd.WorkDate='" + EmpWorkDate + "' ";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }


    }
}
//public class OTfromApp : BaseModel
//{

//    #region Scalar Properties

//    /// <summary>
//    /// Primary key.
//    /// </summary>
//    public string EmployeeSystemId { get; set; }


//    /// <summary>
//    /// This is Item Code.
//    /// </summary>
//    public string EmployeeName { get; set; }


//    /// <summary>
//    /// This is Short Name.
//    /// </summary>
//    public string Code { get; set; }
//    public string APDInTime { get; set; }
//    public string APDOutTime { get; set; }
//    public string OTHr { get; set; }
//    public string APDEmpWorkDate { get; set; }
//    public string EMPAPDInTime { get; set; }
//    public string EMPAPDOutTime { get; set; }

//    public string WorkDate { get; set; }

//    #endregion Scalar Properties

//    #region Audit Properties

//    /// <summary>
//    ///This is  AddedBy.Who add data keep track by AddedBy.
//    /// </summary>
//    [NeverUpdate]
//    public string AddedBy { get; set; }

//    /// <summary>
//    ///This is  AddedDate.Added date keep track by AddedDate.
//    /// </summary>
//    [NeverUpdate]
//    public DateTime AddedDate { get; set; }

//    /// <summary>
//    /// Record insert by user from IP address.
//    /// </summary>
//    //[NeverUpdate]
//    //public string AddedFromIP { get; set; }

//    /// <summary>
//    /// Record updated user name.
//    /// </summary>
//    public string UpdatedBy { get; set; }


//    /// <summary>
//    /// Record updated by user date and time.
//    /// </summary>
//    public DateTime? UpdatedDate { get; set; }


//    /// <summary>
//    /// Record updated by user IP address.
//    /// </summary>
//    //public string UpdatedFromIP { get; set; }

//    #endregion Audit Properties
//}