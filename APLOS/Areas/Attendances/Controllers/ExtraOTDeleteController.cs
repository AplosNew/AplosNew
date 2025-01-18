#region Using
using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Attendances;
using Library.Model.Biometrics;
using Library.Service.Attendances;
using Library.Service.Biometrics;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;
#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class ExtraOTDeleteController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public ExtraOTDeleteController(
               ISqlRepository sqlRepository
            )
        {

            _sqlRepository = sqlRepository;
        }
        #endregion

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #region Employee wise
        [HttpGet, Authorize]
        public ActionResult GetAllEmploteeList()
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT [CheckBoxSelect] = Convert(BIT, 'False') 
	                            ,E.SystemId
	                            ,e.EmployeeCode
	                            ,e.EmployeeName
	                            ,FORMAT(e.DOJ, 'dd-MMM-yyyy') DOJ
	                            ,EC.UserName EmpCategoryName
	                            ,ld.UserName Designation
	                            ,U.UserName Unit
	                            ,Dv.UserName Division
	                            ,Dp.UserName Department
	                            ,Se.UserName Section
	                            ,SB.UserName SubSection
	                            ,L.UserName Line
                            FROM  EmployeeInformation e 
                            LEFT JOIN HKP.EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.Id
                            LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                            LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                            LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id
                            LEFT JOIN ORG.Section Se ON E.SectionID = Se.Id
                            LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
                            LEFT JOIN ORG.Line L ON E.LineID = L.Id
                            LEFT JOIN HKP.Designation D ON E.DesignationSystemID = D.Id
                            LEFT JOIN HKP.LegalDesignation AS ld ON E.LegalDesignationId = ld.Id
                            LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
                            LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode 
                            WHERE E.PlantID='" + identity.PlantId + @"'
                            ORDER BY  e.EmployeeCodePreFix,e.EmployeeCodeNumeric";
            var data = _sqlRepository.GetDataCollection(sql);

            JsonResult json = Json(new
            {
                data


            }, JsonRequestBehavior.AllowGet);

            json.MaxJsonLength = int.MaxValue;
            return json;
        }
        [HttpGet, Authorize]
        public ActionResult GetAttendanceProcessDataEmployeeWise(string FromDate, string ToDate, string EmpSystemId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT [CheckBoxSelect] = Convert(BIT, 'False')
	                            ,FORMAT(apd.WorkDate, 'dd-MMM-yyyy') WorkDate
	                            ,sd.UserName ShiftName
	                            ,FORMAT(sd.InTime, 'hh:mm tt') ShiftInTime
	                            ,FORMAT(sd.OutTime, 'hh:mm tt') ShiftOutTime  
	                            ,FORMAT(apd.InTime, 'hh:mm tt') InTime
	                            ,FORMAT(apd.OutTime, 'hh:mm tt') OutTime
	                            ,apd.DayStatus
	                            ,apd.OTHr
	                            ,Category=CASE WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')!='' THEN edwsa.DayType
											WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')='' THEN odd.OffDayType
											ELSE edwsa.DayType END
	                            ,pl.IsOTExtentNextSlab
	                            ,pl.firstSlab
	                            ,pl.IsTotalWorkTimeAsOT
	                            ,TotalOT=  ISNULL(apd.OTHr,0)/60
	                            ,OT= CASE WHEN ISNULL(apd.OTHr,0)/60 > pl.firstSlab THEN  pl.firstSlab ELSE ISNULL(apd.OTHr,0)/60 END		
	                            ,ExtraOT= CASE WHEN ISNULL(apd.OTHr,0)/60 > pl.firstSlab THEN  ISNULL(apd.OTHr,0)/60-pl.firstSlab ELSE 0 END
	                            ,E.SystemId
	                            ,e.EmployeeCode
	                            ,e.EmployeeName
	                            ,FORMAT(e.DOJ, 'dd-MMM-yyyy') DOJ
	                            ,EC.UserName EmpCategoryName
	                            ,ld.UserName Designation
	                            ,U.UserName Unit
	                            ,Dv.UserName Division
	                            ,Dp.UserName Department
	                            ,Se.UserName Section
	                            ,SB.UserName SubSection
	                            ,L.UserName Line
 	                            ,NewOutTime= CASE WHEN ISNULL(apd.OTHr,0)/60 > pl.firstSlab THEN DATEADD(minute, -1* (ISNULL(apd.OTHr,0)-pl.firstSlab*60),apd.OutTime) ELSE null END 
	                            ,ExtraOTInTime= CASE WHEN ISNULL(apd.OTHr,0)/60 > pl.firstSlab THEN DATEADD(minute, -1* (ISNULL(apd.OTHr,0)-pl.firstSlab*60),apd.OutTime) ELSE null END
	                            	                          
	                            ,NewOutTimeShow= CASE WHEN ISNULL(apd.OTHr,0)/60 > pl.firstSlab THEN FORMAT(DATEADD(minute, -1* (ISNULL(apd.OTHr,0)-pl.firstSlab*60),apd.OutTime), 'hh:mm tt')ELSE null END 
	                            ,ExtraOTInTimeShow= CASE WHEN ISNULL(apd.OTHr,0)/60 > pl.firstSlab THEN FORMAT( DATEADD(minute, -1* (ISNULL(apd.OTHr,0)-pl.firstSlab*60),apd.OutTime),'hh:mm tt') ELSE null END	 
	                            ,ExtraOTOutTimeShow=FORMAT(apd.OutTime, 'hh:mm tt')
                                ,ExtraOTOutTime=apd.OutTime
	                            ,Duration= CASE WHEN ISNULL(apd.OTHr,0)/60 > pl.firstSlab THEN ISNULL(apd.OTHr,0)-pl.firstSlab*60 ELSE 0 END 
                                ,FirstSlabMin= CASE WHEN ISNULL(apd.OTHr,0)/60 > pl.firstSlab THEN  Isnull(pl.firstSlab,0)*60 ELSE 0 END 
                                ,IsManualInTime= CASE WHEN ISNULL(apd.IsManualOutTime,0)=1 THEN  'YES' ELSE 'NO' END  
                            FROM AttdnProcessData AS apd
                            INNER JOIN EmployeeInformation e ON e.SystemId = apd.EmpSystemID
                            Left JOIN DayType dt on dt.DayType=apd.DayStatus
                            LEFT JOIN HKP.EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.Id
                            LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                            LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                            LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id
                            LEFT JOIN ORG.Section Se ON E.SectionID = Se.Id
                            LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
                            LEFT JOIN ORG.Line L ON E.LineID = L.Id
                            LEFT JOIN HKP.Designation D ON E.DesignationSystemID = D.Id
                            LEFT JOIN HKP.LegalDesignation AS ld ON E.LegalDesignationId = ld.Id
                            LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
                            LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ShiftDefination AS sd ON sd.SystemID=apd.ShiftSystemID                           
                            LEFT JOIN  EmpDateWiseShiftAssign AS edwsa ON edwsa.EmpSystemID = apd.EmpSystemID AND edwsa.WorkDate = apd.WorkDate
                           
                            LEFT JOIN
                            (SELECT odm.OffDayType,d.PlantId,d.OffDayDate FROM scs.OffDayDetail  d
                             LEFT JOIN scs.OffDayMaster AS odm ON odm.Id = d.OffDayMasterId 
                             WHERE odm.OffDayType='H' AND d.PlantId='" + identity.PlantId + @"' AND d.OffDayDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"'  
                            ) AS odd ON odd.PlantId=apd.PlantID AND odd.OffDayDate = apd.WorkDate
                            LEFT JOIN [MST].[ExceptionForHolidayEmpList] AS efhel ON efhel.EmpSystemId =apd.EmpSystemID AND efhel.WorkDate =apd.WorkDate
                            LEFT JOIN OTSlabDefineGeneral pl ON pl.DayType =dt.OriginalDayType                     
															AND apd.WorkDate BETWEEN pl.FromDate AND pl.ToDate AND pl.PlantID=apd.PlantID

                            WHERE apd.WorkDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"' 
                            AND apd.EmpSystemID='" + EmpSystemId + @"' 
                            AND  apd.IsOTEntitled=1                             
                            AND ISNULL(apd.OTHr,0)/60 > pl.firstSlab 
                            ORDER BY CONVERT(DATE,apd.WorkDate)";
            var data = _sqlRepository.GetDataCollection(sql);

            JsonResult json = Json(new
            {
                data


            }, JsonRequestBehavior.AllowGet);

            json.MaxJsonLength = int.MaxValue;
            return json;
        }
        [HttpPost, Authorize]
        public ActionResult SaveAttendanceProcessDataEmployeeWise(List<AttendanceProcessDataVM> AttendanceProcessData, string pFromDate, string pToDate)
        {
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string EmpSytemId = "";
            string DeleteDate = "";
            DataSet dsManualAttanData = null;
            DataSet dsHourlyOTData = null;
            ConnectionManager.DAL.ConManager objCon;
            try
            {


                for (int i = 0; i < AttendanceProcessData.Count; i++)
                {
                    if (EmpSytemId == "")
                        EmpSytemId = "'" + AttendanceProcessData[i].SystemId.ToString() + "'";
                    //else
                    //    EmpSytemId = EmpSytemId + ",'" + AttendanceRawData[i].SystemId.ToString() + "'";
                }
                clsAttendance.AttendanceProcessAplos obj = new clsAttendance.AttendanceProcessAplos();
                DateTime FromDate = Convert.ToDateTime(pFromDate);
                DateTime ToDate = Convert.ToDateTime(pToDate);

                if (EmpSytemId != "")
                {
                    obj.LockValidation(identity.PlantId, FromDate.ToString("dd-MMM-yyyy"), ToDate.ToString("dd-MMM-yyyy"), EmpSytemId);
                }





                string sql = "SELECT * FROM [dbo].[AttdnManualData] WHERE EmpSystemID IN (" + EmpSytemId + ") AND WorkDate BETWEEN '" + pFromDate + @"' AND '" + pToDate + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsManualAttanData, false, "1");

                string sql1 = "SELECT * FROM [dbo].[HourlyOT] WHERE EmpSystemID IN (" + EmpSytemId + ") AND WorkDate BETWEEN '" + pFromDate + @"' AND '" + pToDate + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql1, out dsHourlyOTData, false, "1");

                #region Raw data delete data load
                //string AttendanceRawDataId = "";

                DataSet dsRef = null;
                DataSet dsGetdataRef = null;
                DataSet dsSaveddataRef = null;
                DataRow drSaveSummary = null;
                string strSQL;
                string strSQL1;
                string strSQL2;






                strSQL1 = @"SELECT * FROM AttdnRawData WHERE LogDownLoadNum =" + EmpSytemId + " AND PDate BETWEEN '" + pFromDate + @"' AND '" + pToDate + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL1, out dsGetdataRef, false, "1");




                strSQL2 = @"SELECT * FROM AttdnRawDataBackUp WHERE LogDownLoadNum =" + EmpSytemId + " AND PDate BETWEEN '" + pFromDate + @"' AND '" + pToDate + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL2, out dsSaveddataRef, false, "1");


                #endregion



                DataView DvMaster = new DataView(dsManualAttanData.Tables[0]);
                DataView DvHourlyOTData = new DataView(dsHourlyOTData.Tables[0]);

                Random rnd = new Random((int)DateTime.Now.Ticks);
                for (int i = 0; i < AttendanceProcessData.Count; i++)
                {
                    bool IsRawDataDelete = false;
                    string JoinDT = string.Empty;
                    string Date = Convert.ToDateTime(AttendanceProcessData[i].WorkDate).ToString("dd-MMM-yyyy");
                    string SOutTime = Convert.ToDateTime(AttendanceProcessData[i].ShiftOutTime).ToString("hh:mm tt");
                    string SInTime = Convert.ToDateTime(AttendanceProcessData[i].ShiftInTime).ToString("hh:mm tt");
                    //night shift
                    if (Convert.ToDateTime(Date + " " + SInTime) > Convert.ToDateTime(Date + " " + SOutTime))
                    {
                        Date = Convert.ToDateTime(AttendanceProcessData[i].WorkDate).AddDays(1).ToString("dd-MMM-yyyy");
                    }


                    if (AttendanceProcessData[i].Category == "NW")
                    {
                        JoinDT = Date + " " + SOutTime;
                    }
                    if (AttendanceProcessData[i].Category == "W")
                    {
                        JoinDT = Date + " " + SInTime;
                    }
                    if (AttendanceProcessData[i].Category == "H")
                    {
                        JoinDT = Date + " " + SInTime;
                    }
                    if (Convert.ToInt32(AttendanceProcessData[i].FirstSlabMin) == 0)
                    {
                        IsRawDataDelete = true;
                    }

                    DateTime d1 = Convert.ToDateTime(JoinDT);
                    DateTime NewOutTime = d1.AddMinutes(Convert.ToInt32(AttendanceProcessData[i].FirstSlabMin));

                    int RandomMinutes = rnd.Next(0, 15);
                    var RandomOutTime = NewOutTime.AddMinutes(RandomMinutes);

                    if (IsRawDataDelete)
                    { //Raw Data Delete



                        if (DeleteDate == "")
                            DeleteDate = "'" + AttendanceProcessData[i].WorkDate.ToString() + "'";
                        else
                            DeleteDate = DeleteDate + ",'" + AttendanceProcessData[i].WorkDate.ToString() + "'";

                        DataView dvSaveSummary = new DataView(dsSaveddataRef.Tables[0]);
                        for (int j = 0; j < dsGetdataRef.Tables[0].Rows.Count; j++)
                        {

                            if (Convert.ToDateTime(dsGetdataRef.Tables[0].Rows[j]["PDate"].ToString()) == Convert.ToDateTime(AttendanceProcessData[i].WorkDate.ToString()))
                            {
                                dvSaveSummary.RowFilter = " Id ='" + dsGetdataRef.Tables[0].Rows[j]["Id"] + "' AND PDate = '" + AttendanceProcessData[i].WorkDate + @"'";
                                if (dvSaveSummary.Count == 0)
                                {
                                    string sID = string.Empty;
                                    bplib.clsGenID objGenID = new bplib.clsGenID();
                                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "AttdnRawDataBackUp", out sID);
                                    DataRow dr = dsSaveddataRef.Tables[0].NewRow();
                                    dr["Id"] = "AB" + sID;
                                    dr["DeviceID"] = dsGetdataRef.Tables[0].Rows[j]["DeviceID"];
                                    dr["DevSystemID"] = dsGetdataRef.Tables[0].Rows[j]["DevSystemID"];
                                    dr["LogDownLoadNum"] = dsGetdataRef.Tables[0].Rows[j]["LogDownLoadNum"];
                                    dr["PDate"] = dsGetdataRef.Tables[0].Rows[j]["PDate"];
                                    dr["PTime"] = dsGetdataRef.Tables[0].Rows[j]["PTime"];
                                    dr["PType"] = dsGetdataRef.Tables[0].Rows[j]["PType"];
                                    dr["ProcessedFlag"] = dsGetdataRef.Tables[0].Rows[j]["ProcessedFlag"];
                                    dr["GroupID"] = identity.CompanyGroupId;
                                    dr["PlantID"] = identity.PlantId.ToString();
                                    dr["AddedBy"] = identity.Name;
                                    dr["DateAdded"] = System.DateTime.Now.ToString();
                                    dr["BackupType"] = "EXTRAOT";
                                    dsSaveddataRef.Tables[0].Rows.Add(dr);

                                }
                                else
                                {
                                    DataRow dr = dvSaveSummary[0].Row;
                                    dr.BeginEdit();
                                    dr["DeviceID"] = dsGetdataRef.Tables[0].Rows[j]["DeviceID"];
                                    dr["DevSystemID"] = dsGetdataRef.Tables[0].Rows[j]["DevSystemID"];
                                    dr["LogDownLoadNum"] = dsGetdataRef.Tables[0].Rows[j]["LogDownLoadNum"];
                                    dr["PDate"] = dsGetdataRef.Tables[0].Rows[j]["PDate"];
                                    dr["PTime"] = dsGetdataRef.Tables[0].Rows[j]["PTime"];
                                    dr["PType"] = dsGetdataRef.Tables[0].Rows[j]["PType"];
                                    dr["ProcessedFlag"] = dsGetdataRef.Tables[0].Rows[j]["ProcessedFlag"];
                                    dr["GroupID"] = identity.CompanyGroupId;
                                    dr["PlantID"] = identity.PlantId.ToString();
                                    dr["UpdatedBy"] = identity.Name;
                                    dr["DateUpdated"] = System.DateTime.Now.ToString();
                                    dr["BackupType"] = "EXTRAOT";
                                    dr.EndEdit();
                                }


                                dvSaveSummary.RowFilter = null;
                            }

                            //Old year insert 
                        }
                        //SaveAttendanceRawDataBackupDataSetsAndDelete(AttendanceRawDataId, dsSaveddataRef);

                    }
                    else
                    {  //Manual Attendance 

                        DvMaster.RowFilter = "EmpSystemID='" + AttendanceProcessData[i].SystemId + @"' AND WorkDate='" + AttendanceProcessData[i].WorkDate + @"'";
                        if (DvMaster.Count == 0)
                        {

                            DataRow dr = dsManualAttanData.Tables[0].NewRow();
                            dr["EmpSystemID"] = AttendanceProcessData[i].SystemId;
                            dr["WorkDate"] = Convert.ToDateTime(AttendanceProcessData[i].WorkDate);
                            dr["GroupID"] = identity.CompanyGroupId;
                            dr["PlantID"] = identity.PlantId;
                            dr["EntryFlag"] = "EXTRAOT";
                            //dr["OutTime"] = AttendanceProcessData[i].NewOutTime;
                            dr["OutTime"] = Convert.ToDateTime(RandomOutTime);
                            dr["AddedBy"] = identity.Name;
                            dr["DateAdded"] = DateTime.Now;

                            dsManualAttanData.Tables[0].Rows.Add(dr);

                        }
                        else
                        {
                            DataRow dr = DvMaster[0].Row;
                            dr.BeginEdit();
                            dr["EntryFlag"] = "EXTRAOT";
                            //dr["OutTime"] = AttendanceProcessData[i].NewOutTime;
                            dr["OutTime"] = Convert.ToDateTime(RandomOutTime);
                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = System.DateTime.Now.ToString();
                            dr.EndEdit();

                        }
                    }
                    DvMaster.RowFilter = null;

                    DvHourlyOTData.RowFilter = "EmpSystemID='" + AttendanceProcessData[i].SystemId + "' AND WorkDate='" + AttendanceProcessData[i].WorkDate + @"'";
                    if (DvHourlyOTData.Count == 0)
                    {
                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "HourlyOT", out sID);
                        DataRow dr = dsHourlyOTData.Tables[0].NewRow();
                        dr["Id"] = "EO" + sID;
                        dr["EmpSystemId"] = AttendanceProcessData[i].SystemId;
                        //dr["FromDate"] = AttendanceProcessData[i].ExtraOTInTime;
                        dr["FromDate"] = NewOutTime;
                        dr["ToDate"] = AttendanceProcessData[i].ExtraOTOutTime;
                        dr["Duration"] = AttendanceProcessData[i].Duration;
                        dr["WorkDate"] = Convert.ToDateTime(AttendanceProcessData[i].WorkDate);
                        dr["PlantId"] = identity.PlantId;
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr["OTType"] = "EXTRAOT";
                        dsHourlyOTData.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        DataRow dr = DvHourlyOTData[0].Row;
                        dr.BeginEdit();
                        dr["EmpSystemId"] = AttendanceProcessData[i].SystemId;
                        //dr["FromDate"] = AttendanceProcessData[i].ExtraOTInTime;
                        dr["FromDate"] = NewOutTime;
                        dr["ToDate"] = AttendanceProcessData[i].ExtraOTOutTime;
                        dr["Duration"] = AttendanceProcessData[i].Duration;
                        dr["WorkDate"] = Convert.ToDateTime(AttendanceProcessData[i].WorkDate);
                        dr["PlantId"] = identity.PlantId;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr["OTType"] = "EXTRAOT";
                        dr.EndEdit();
                    }
                    DvHourlyOTData.RowFilter = null;
                }

                clsStaticInfo objsave = new clsStaticInfo();
                //objsave.SaveDataSets(dsManualAttanData, dsHourlyOTData);
                if (DeleteDate == "")
                    objsave.SaveDataSets(dsManualAttanData, dsHourlyOTData);
                else
                    SaveAttendanceRawDataBackupDataSetsAndDeleteEmpWise(EmpSytemId, DeleteDate, dsManualAttanData, dsHourlyOTData, dsSaveddataRef);

                //while (FromDate <= ToDate)
                //{

                //    ReturnType r = obj.SaveTotal(identity.PlantId, FromDate.ToString("dd-MMM-yyyy"), EmpSytemId, false);//laila                 
                //    FromDate = FromDate.AddDays(1);
                //}
                foreach (AttendanceProcessDataVM item in AttendanceProcessData)
                {
                    AttendanceLog.Log.SaveLog(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "\\" + System.Reflection.MethodBase.GetCurrentMethod().Name);
                    ReturnType r = obj.SaveTotal(identity.PlantId, item.WorkDate, EmpSytemId, false);//laila    
                }

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }





            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }
        //[HttpPost, Authorize]
        //public ActionResult xSaveAttendanceProcessDataEmployeeWise(List<AttendanceProcessDataVM> AttendanceProcessData, string pFromDate, string pToDate)
        //{
        //    clsStaticInfo objStatic = null;
        //    objStatic = new clsStaticInfo();
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    DateTime[] DataList = null;
        //    string EmpSytemId = "";
        //    DataSet dsManualAttanData = null;
        //    DataSet dsHourlyOTData = null;
        //    ConnectionManager.DAL.ConManager objCon;
        //    try
        //    {
        //        //for (int i = 0; i < AttendanceProcessData.Count; i++)
        //        //{
        //        //    if (AttendanceRawDataId == "")
        //        //        AttendanceRawDataId = "'" + AttendanceProcessData[i].Id.ToString() + "'";
        //        //    else
        //        //        AttendanceRawDataId = AttendanceRawDataId + ",'" + AttendanceProcessData[i].Id.ToString() + "'";
        //        //}

        //        for (int i = 0; i < AttendanceProcessData.Count; i++)
        //        {
        //            if (EmpSytemId == "")
        //                EmpSytemId = "'" + AttendanceProcessData[i].SystemId.ToString() + "'";
        //            //else
        //            //    EmpSytemId = EmpSytemId + ",'" + AttendanceRawData[i].SystemId.ToString() + "'";
        //        }
        //        clsAttendance.AttendanceProcessAplos obj = new clsAttendance.AttendanceProcessAplos();
        //        DateTime FromDate = Convert.ToDateTime(pFromDate);
        //        DateTime ToDate = Convert.ToDateTime(pToDate);

        //        if (EmpSytemId != "")
        //        {
        //            obj.LockValidation(identity.PlantId, FromDate.ToString("dd-MMM-yyyy"), ToDate.ToString("dd-MMM-yyyy"), EmpSytemId);
        //        }





        //        string sql = "SELECT * FROM [dbo].[AttdnManualData] WHERE EmpSystemID IN (" + EmpSytemId + ") AND WorkDate BETWEEN '" + pFromDate + @"' AND '" + pToDate + @"' AND PlantID='" + identity.PlantId + @"'";
        //        objCon = new ConnectionManager.DAL.ConManager("1");
        //        objCon.OpenDataSetThroughAdapter(sql, out dsManualAttanData, false, "1");

        //        string sql1 = "SELECT * FROM [dbo].[HourlyOT] WHERE EmpSystemID IN (" + EmpSytemId + ") AND WorkDate BETWEEN '" + pFromDate + @"' AND '" + pToDate + @"'  AND PlantID='" + identity.PlantId + @"'";
        //        objCon = new ConnectionManager.DAL.ConManager("1");
        //        objCon.OpenDataSetThroughAdapter(sql1, out dsHourlyOTData, false, "1");

        //        DataView DvMaster = new DataView(dsManualAttanData.Tables[0]);
        //        DataView DvHourlyOTData = new DataView(dsHourlyOTData.Tables[0]);
        //        for (int i = 0; i < AttendanceProcessData.Count; i++)
        //        {
        //            DvMaster.RowFilter = "EmpSystemID='" + AttendanceProcessData[i].SystemId + @"' AND WorkDate='" + AttendanceProcessData[i].WorkDate + @"' AND PlantID='" + identity.PlantId + @"'";
        //            if (DvMaster.Count == 0)
        //            {

        //                DataRow dr = dsManualAttanData.Tables[0].NewRow();
        //                dr["EmpSystemID"] = AttendanceProcessData[i].SystemId;
        //                dr["WorkDate"] = Convert.ToDateTime(AttendanceProcessData[i].WorkDate);
        //                dr["GroupID"] = identity.CompanyGroupId;
        //                dr["PlantID"] = identity.PlantId;
        //                dr["OutTime"] = AttendanceProcessData[i].NewOutTime;
        //                dr["AddedBy"] = identity.Name;
        //                dr["DateAdded"] = DateTime.Now;

        //                dsManualAttanData.Tables[0].Rows.Add(dr);

        //            }
        //            else
        //            {
        //                DataRow dr = DvMaster[0].Row;
        //                dr.BeginEdit();
        //                dr["OutTime"] = AttendanceProcessData[i].NewOutTime;
        //                dr["UpdatedBy"] = identity.Name;
        //                dr["DateUpdated"] = System.DateTime.Now.ToString();
        //                dr.EndEdit();

        //            }
        //            DvMaster.RowFilter = null;

        //            DvHourlyOTData.RowFilter = "EmpSystemID='" + AttendanceProcessData[i].SystemId + "' AND WorkDate='" + AttendanceProcessData[i].WorkDate + @"' AND PlantID='" + identity.PlantId + @"'";
        //            if (DvHourlyOTData.Count == 0)
        //            {
        //                string sID = string.Empty;
        //                bplib.clsGenID objGenID = new bplib.clsGenID();
        //                objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "HourlyOT", out sID);
        //                DataRow dr = dsHourlyOTData.Tables[0].NewRow();
        //                dr["Id"] = "EO" + sID;
        //                dr["EmpSystemId"] = AttendanceProcessData[i].SystemId;
        //                dr["FromDate"] = AttendanceProcessData[i].ExtraOTInTime;
        //                dr["ToDate"] = AttendanceProcessData[i].ExtraOTOutTime;
        //                dr["Duration"] = AttendanceProcessData[i].Duration;
        //                dr["WorkDate"] = Convert.ToDateTime(AttendanceProcessData[i].WorkDate);
        //                dr["PlantId"] = identity.PlantId;
        //                dr["AddedBy"] = identity.Name;
        //                dr["AddedDate"] = DateTime.Now;
        //                dr["AddedFromIP"] = identity.IPAddress;
        //                dr["UpdatedBy"] = identity.Name;
        //                dr["UpdatedDate"] = DateTime.Now;
        //                dr["UpdatedFromIP"] = identity.IPAddress;
        //                dr["OTType"] = "EXTRAOT";
        //                dsHourlyOTData.Tables[0].Rows.Add(dr);

        //            }
        //            else
        //            {
        //                DataRow dr = DvHourlyOTData[0].Row;
        //                dr.BeginEdit();
        //                dr["EmpSystemId"] = AttendanceProcessData[i].SystemId;
        //                dr["FromDate"] = AttendanceProcessData[i].ExtraOTInTime;
        //                dr["ToDate"] = AttendanceProcessData[i].ExtraOTOutTime;
        //                dr["Duration"] = AttendanceProcessData[i].Duration;
        //                dr["WorkDate"] = Convert.ToDateTime(AttendanceProcessData[i].WorkDate);
        //                dr["PlantId"] = identity.PlantId;
        //                dr["UpdatedBy"] = identity.Name;
        //                dr["UpdatedDate"] = DateTime.Now;
        //                dr["UpdatedFromIP"] = identity.IPAddress;
        //                dr["OTType"] = "EXTRAOT";
        //                dr.EndEdit();
        //            }
        //            DvHourlyOTData.RowFilter = null;
        //        }

        //        clsStaticInfo objsave = new clsStaticInfo();
        //        objsave.SaveDataSets(dsManualAttanData, dsHourlyOTData);


        //        while (FromDate <= ToDate)
        //        {

        //            ReturnType r = obj.SaveTotal(identity.PlantId, FromDate.ToString("dd-MMM-yyyy"), EmpSytemId, false);//laila                 
        //            FromDate = FromDate.AddDays(1);
        //        }


        //    }
        //    catch (Exception ex)
        //    {
        //        throw (ex);
        //    }
        //    finally
        //    {
        //        objCon = null;
        //    }





        //    return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        //}
        #endregion


        #region Date wise data 



        public void DeleteExtraOTandRecoveryRawData(string RecoveryRawDataEmpSytemId, string DeleteEmpSystemId, string DeleteManualDataEmpSytemId, string WDate, string OTType, params DataSet[] dsRef)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                objCon.ExecuteNonQueryWrapper("DELETE FROM HourlyOT WHERE EmpSystemId IN (" + DeleteEmpSystemId + ") AND WorkDate ='" + WDate + @"' AND OTType='" + OTType + @"'", true, "1");
                if (!string.IsNullOrEmpty(DeleteManualDataEmpSytemId))
                {
                    objCon.ExecuteNonQueryWrapper("DELETE FROM AttdnManualData WHERE EmpSystemID IN (" + DeleteManualDataEmpSytemId + ") AND WorkDate='" + WDate + @"' AND EntryFlag='" + OTType + @"'", true, "1");

                }

                if (!string.IsNullOrEmpty(RecoveryRawDataEmpSytemId))
                {
                    objCon.ExecuteNonQueryWrapper("DELETE FROM AttdnRawDataBackup WHERE LogDownLoadNum IN (" + RecoveryRawDataEmpSytemId + ") AND Pdate='" + WDate + @"' AND BackupType='EXTRAOT'", true, "1");

                }
                //else
                //{
                //    objCon.ExecuteNonQueryWrapper("DELETE FROM AttdnManualData WHERE EmpSystemID IN (" + DeleteEmpSystemId + ") AND WorkDate='" + WDate + @"' AND EntryFlag='" + OTType + @"'", true, "1");

                //}
                int i = 0;
                foreach (DataSet value in dsRef)
                {
                    if (dsRef[i] != null)
                    {
                        objCon.SaveDataSetThroughAdapter(ref dsRef[i], true, "1");
                        i = i + 1;
                    }
                    else
                    {
                        i = i + 1;
                    }
                }
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (IsTransactionStarted)
                {
                    objCon.RollBack();
                }
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function
        public void DeleteExtraOTandRecoveryManualData(string DeleteEmpSystemId,string DeleteManualDataEmpSytemId, string WDate, string OTType, params DataSet[] dsRef)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                objCon.ExecuteNonQueryWrapper("DELETE FROM HourlyOT WHERE EmpSystemId IN (" + DeleteEmpSystemId + ") AND WorkDate ='" + WDate + @"' AND OTType='" + OTType + @"'", true, "1");
                if (!string.IsNullOrEmpty(DeleteManualDataEmpSytemId))
                {
                    objCon.ExecuteNonQueryWrapper("DELETE FROM AttdnManualData WHERE EmpSystemID IN (" + DeleteManualDataEmpSytemId + ") AND WorkDate='" + WDate + @"' AND EntryFlag='" + OTType + @"'", true, "1");

                }
                //else
                //{
                //    objCon.ExecuteNonQueryWrapper("DELETE FROM AttdnManualData WHERE EmpSystemID IN (" + DeleteEmpSystemId + ") AND WorkDate='" + WDate + @"' AND EntryFlag='" + OTType + @"'", true, "1");

                //}
                int i = 0;
                foreach (DataSet value in dsRef)
                {
                    if (dsRef[i] != null)
                    {
                        objCon.SaveDataSetThroughAdapter(ref dsRef[i], true, "1");
                        i = i + 1;
                    }
                    else
                    {
                        i = i + 1;
                    }
                }
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (IsTransactionStarted)
                {
                    objCon.RollBack();
                }
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function
        public void DeleteExtraOT(string DeleteEmpSystemId, string WDate, string OTType)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                objCon.ExecuteNonQueryWrapper("DELETE FROM HourlyOT WHERE EmpSystemId IN (" + DeleteEmpSystemId + ") AND WorkDate ='" + WDate + @"' AND OTType='"+ OTType+@"'", true, "1");
                objCon.ExecuteNonQueryWrapper("DELETE FROM AttdnManualData WHERE EmpSystemID IN (" + DeleteEmpSystemId + ") AND WorkDate ='" + WDate + @"' AND EntryFlag='"+ OTType+@"'", true, "1");
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (IsTransactionStarted)
                {
                    objCon.RollBack();
                }
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function
        [HttpPost]
        public ActionResult DeleteExtraOTDataDateWise(List<ExtraOTVM> AttendanceProcessData, string WDate, string OTType)
        {
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            //string EmpSytemId = "";
            //string DeleteEmpSytemId = "";
            //DataSet dsManualAttanData = null;
            //DataSet dsHourlyOTData = null;

            DataSet dsAttdnRawDataBackUp = null;
            DataSet dsAttdnRawData = null;
            DataSet dsAttdnManualData = null;

            bool IsRecoveryRawData = false;
            bool IsRecoveryManualData = false;

            string EmpSytemId = string.Empty;
            string RecoveryRawDataEmpSytemId = "";
            string RecoveryManualDataEmpSytemId = "";
            string DeleteManualDataEmpSytemId = "";




            ConnectionManager.DAL.ConManager objCon;
            try
            {

                for (int i = 0; i < AttendanceProcessData.Count; i++)
                {


                    if (EmpSytemId == "")
                        EmpSytemId = "'" + AttendanceProcessData[i].SystemId.ToString() + "'";
                    else
                        EmpSytemId = EmpSytemId + ",'" + AttendanceProcessData[i].SystemId.ToString() + "'";


                    // Recovery Manual Attendance Emp SystemId
                    if (AttendanceProcessData[i].IsRecoveryManualData == true)
                    {
                        IsRecoveryManualData = true;
                        if (RecoveryManualDataEmpSytemId == "")
                            RecoveryManualDataEmpSytemId = "'" + AttendanceProcessData[i].SystemId.ToString() + "'";
                        else
                            RecoveryManualDataEmpSytemId = RecoveryManualDataEmpSytemId + ",'" + AttendanceProcessData[i].SystemId.ToString() + "'";
                    }
                    else // Delete Manual Attendance Emp SystemId
                    {
                        if (DeleteManualDataEmpSytemId == "")
                            DeleteManualDataEmpSytemId = "'" + AttendanceProcessData[i].SystemId.ToString() + "'";
                        else
                            DeleteManualDataEmpSytemId = DeleteManualDataEmpSytemId + ",'" + AttendanceProcessData[i].SystemId.ToString() + "'";

                    }


                    // Raw Data Recovery Emp SystemId
                    if (AttendanceProcessData[i].IsRecoveryRawData == true)
                    {
                        IsRecoveryRawData = true;
                        if (RecoveryRawDataEmpSytemId == "")
                            RecoveryRawDataEmpSytemId = "'" + AttendanceProcessData[i].SystemId.ToString() + "'";
                        else
                            RecoveryRawDataEmpSytemId = RecoveryRawDataEmpSytemId + ",'" + AttendanceProcessData[i].SystemId.ToString() + "'";
                    }


                }





                clsAttendance.AttendanceProcessAplos obj = new clsAttendance.AttendanceProcessAplos();
                DateTime ToDate = Convert.ToDateTime(WDate);
                obj.LockValidation(identity.PlantId, ToDate.ToString("dd-MMM-yyyy"), ToDate.ToString("dd-MMM-yyyy"), EmpSytemId);


                if (IsRecoveryManualData)
                {
                    string ManualSql = @"SELECT * FROM AttdnManualData WHERE EmpSystemID IN (" + RecoveryManualDataEmpSytemId + ") AND  WorkDate='" + WDate + @"'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(ManualSql, out dsAttdnManualData, false, "1");


                    DataView DVManualAttanData = new DataView(dsAttdnManualData.Tables[0]);

                    foreach (var item in AttendanceProcessData.Where(x => x.IsRecoveryManualData = true))
                    {
                        //Manual Attendance 
                        DVManualAttanData.RowFilter = "EmpSystemID='" + item.SystemId + "'";
                        if (DVManualAttanData.Count == 0)
                        {

                            //DataRow dr = dsAttdnManualData.Tables[0].NewRow();
                            //dr["EmpSystemID"] = AttendanceProcessData[i].SystemId;
                            //dr["WorkDate"] = Convert.ToDateTime(WDate);
                            //dr["GroupID"] = identity.CompanyGroupId;
                            //dr["PlantID"] = identity.PlantId;
                            //dr["EntryFlag"] = "EXTRAOT";
                            ////dr["OutTime"] = AttendanceProcessData[i].NewOutTime;
                            //dr["OutTime"] = Convert.ToDateTime(RandomOutTime);
                            //dr["AddedBy"] = identity.Name;
                            //dr["DateAdded"] = DateTime.Now;

                            //dsAttdnManualData.Tables[0].Rows.Add(dr);

                        }
                        else
                        {
                            DataRow dr = DVManualAttanData[0].Row;
                            dr.BeginEdit();
                            if (item.IsManualInTime)
                            {
                                dr["InTime"] = Convert.ToDateTime(item.ManualInTime);
                            }

                            if (item.IsManualOutTime)
                            {
                                dr["OutTime"] = Convert.ToDateTime(item.ManualOutTime);
                            }
                            else
                            {
                                dr["OutTime"] = DBNull.Value;
                            }


                            

                            dr["EntryFlag"] = "EXTRAOTRECOVERY";
                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = System.DateTime.Now.ToString();
                            dr.EndEdit();

                        }
                        DVManualAttanData.RowFilter = null;

                    }


                }





                if (IsRecoveryRawData)
                {
                    string AttdnRawDataBackUpSql = @"SELECT * FROM AttdnRawDataBackUp WHERE LogDownLoadNum IN (" + RecoveryRawDataEmpSytemId + ") AND  PDate='" + WDate + @"'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(AttdnRawDataBackUpSql, out dsAttdnRawDataBackUp, false, "1");


                    string AttdnRawDataSQL = @"SELECT * FROM AttdnRawData WHERE LogDownLoadNum IN (" + RecoveryRawDataEmpSytemId + ")  AND PDate='" + WDate + @"'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(AttdnRawDataSQL, out dsAttdnRawData, false, "1");

                    DataView dvSaveSummary = new DataView(dsAttdnRawData.Tables[0]);
                    for (int i = 0; i < dsAttdnRawDataBackUp.Tables[0].Rows.Count; i++)
                    {

                        dvSaveSummary.RowFilter = " Id =''  AND PDate = '" + WDate + @"'";
                        if (dvSaveSummary.Count == 0)
                        {
                            string sID = string.Empty;
                            bplib.clsGenID objGenID = new bplib.clsGenID();
                            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "AttdnRawDataRecovery", out sID);
                            DataRow dr = dsAttdnRawData.Tables[0].NewRow();
                            dr["Id"] = "R" + sID;
                            dr["DeviceID"] = dsAttdnRawDataBackUp.Tables[0].Rows[i]["DeviceID"];
                            dr["DevSystemID"] = dsAttdnRawDataBackUp.Tables[0].Rows[i]["DevSystemID"];
                            dr["LogDownLoadNum"] = dsAttdnRawDataBackUp.Tables[0].Rows[i]["LogDownLoadNum"];
                            dr["PDate"] = dsAttdnRawDataBackUp.Tables[0].Rows[i]["PDate"];
                            dr["PTime"] = dsAttdnRawDataBackUp.Tables[0].Rows[i]["PTime"];
                            dr["PType"] = dsAttdnRawDataBackUp.Tables[0].Rows[i]["PType"];
                            dr["ProcessedFlag"] = dsAttdnRawDataBackUp.Tables[0].Rows[i]["ProcessedFlag"];
                            dr["GroupID"] = identity.CompanyGroupId;
                            dr["PlantID"] = identity.PlantId.ToString();
                            dr["AddedBy"] = identity.Name;
                            dr["DateAdded"] = System.DateTime.Now.ToString();
                            //dr["BackupType"] = "EXTRAOT";
                            dsAttdnRawData.Tables[0].Rows.Add(dr);

                        }
                        //else
                        //{
                        //    DataRow dr = dvSaveSummary[0].Row;
                        //    dr.BeginEdit();
                        //    dr["DeviceID"] = dsAttdnRawData.Tables[0].Rows[i]["DeviceID"];
                        //    dr["DevSystemID"] = dsAttdnRawDataBackUp.Tables[0].Rows[i]["DevSystemID"];
                        //    dr["LogDownLoadNum"] = dsAttdnRawDataBackUp.Tables[0].Rows[i]["LogDownLoadNum"];
                        //    dr["PDate"] = dsAttdnRawDataBackUp.Tables[0].Rows[i]["PDate"];
                        //    dr["PTime"] = dsAttdnRawDataBackUp.Tables[0].Rows[i]["PTime"];
                        //    dr["PType"] = dsAttdnRawDataBackUp.Tables[0].Rows[i]["PType"];
                        //    dr["ProcessedFlag"] = dsAttdnRawDataBackUp.Tables[0].Rows[i]["ProcessedFlag"];
                        //    dr["GroupID"] = identity.CompanyGroupId;
                        //    dr["PlantID"] = identity.PlantId.ToString();
                        //    dr["UpdatedBy"] = identity.Name;
                        //    dr["DateUpdated"] = System.DateTime.Now.ToString();
                        //    //dr["BackupType"] = "EXTRAOT";
                        //    dr.EndEdit();
                        //}
                        dvSaveSummary.RowFilter = null;
                        //Old year insert 

                    }


                }





                clsStaticInfo objsave = new clsStaticInfo();


                if (IsRecoveryRawData)
                {
                    if (IsRecoveryManualData)
                    {
                        DeleteExtraOTandRecoveryRawData(RecoveryRawDataEmpSytemId, EmpSytemId, DeleteManualDataEmpSytemId, WDate, OTType, dsAttdnRawData,dsAttdnManualData);
                    }
                    else
                    {
                        DeleteExtraOTandRecoveryRawData(RecoveryRawDataEmpSytemId, EmpSytemId, DeleteManualDataEmpSytemId, WDate, OTType, dsAttdnRawData);
                    }
                }
                else
                {
                    if (IsRecoveryManualData)
                    {
                        DeleteExtraOTandRecoveryManualData(EmpSytemId, DeleteManualDataEmpSytemId, WDate, OTType, dsAttdnManualData);
                    }
                    else
                    {
                        DeleteExtraOT(EmpSytemId, WDate, OTType);
                    }
                }


                





                    AttendanceLog.Log.SaveLog(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "\\" + System.Reflection.MethodBase.GetCurrentMethod().Name);
                ReturnType r = obj.SaveTotal(identity.PlantId, ToDate.ToString("dd-MMM-yyyy"), EmpSytemId, false);//laila                 


            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }





            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }



        [HttpGet]
        public ActionResult GetExtraOTDataDateWise(string WDate, string OTType)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT [CheckBoxSelect] = Convert(BIT, 'False')
	                            ,FORMAT(apd.WorkDate, 'dd-MMM-yyyy') WorkDate
	                            ,sd.UserName ShiftName
	                            ,FORMAT(sd.InTime, 'hh:mm tt') ShiftInTime
	                            ,FORMAT(sd.OutTime, 'hh:mm tt') ShiftOutTime  
	                            ,FORMAT(apd.InTime, 'hh:mm tt') InTime
	                            ,FORMAT(apd.OutTime, 'hh:mm tt') OutTime
	                            ,apd.DayStatus
	                            ,apd.OTHr
	                            ,Category=dt.OriginalDayType
	                            ,pl.IsOTExtentNextSlab
	                            ,pl.firstSlab
	                            ,pl.IsTotalWorkTimeAsOT
	                            
	                            ,E.SystemId
	                            ,e.EmployeeCode
	                            ,e.EmployeeName
	                            ,FORMAT(e.DOJ, 'dd-MMM-yyyy') DOJ
	                            ,EC.UserName EmpCategoryName
	                            ,ld.UserName Designation
	                            ,U.UserName Unit
	                            ,Dv.UserName Division
	                            ,Dp.UserName Department
	                            ,Se.UserName Section
	                            ,SB.UserName SubSection
	                            ,L.UserName Line
								,FORMAT(HOT.FromDate, 'dd-MMM-yyyy hh:mm tt') FromDate,FORMAT(HOT.ToDate, 'dd-MMM-yyyy hh:mm tt') ToDate,HOT.Duration 	                           
                                ,IsRecoveryRawData=case when pl.firstSlab=0 then 1 else 0 end
                                ,ManualInTimeFlag= CASE WHEN ISNULL(apd.IsManualOutTime,0)=1 THEN  'YES' ELSE 'NO' END 
								,HOT.IsManualInTime
								,HOT.IsManualOutTime
                                ,ManualInTime=CASE WHEN ISNULL(HOT.IsManualInTime,0)=1 THEN  HOT.ManualInTime  END 
								,ManualOutTime=CASE WHEN ISNULL(HOT.IsManualOutTime,0)=1 THEN  HOT.ManualOutTime  END 
                                ,IsRecoveryManualData=case when (ISNULL(HOT.IsManualInTime,0)=1 or ISNULL(HOT.IsManualOutTime,0)=1) then 1 else 0 end
                            FROM AttdnProcessData AS apd
							inner join HourlyOT HOT ON HOT.EmpSystemId=apd.EmpSystemID and HOT.WorkDate=apd.WorkDate
                            INNER JOIN EmployeeInformation e ON e.SystemId = apd.EmpSystemID
                            LEFT JOIN DayType dt on dt.DayType=apd.DayStatus
                            LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
							LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity EN ON MB.EntityId=EN.Id
                            LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
                            LEFT JOIN HKP.EmployeeCategory EC ON EC.Id=DM.EmployeeCategoryId
                            LEFT JOIN ORG.Unit U ON EN.UnitID = U.Id
                            LEFT JOIN ORG.Division Dv ON PR.DivisionID = Dv.Id
                            LEFT JOIN ORG.Department Dp ON PR.DepartmentID = Dp.Id
                            LEFT JOIN ORG.Section Se ON PR.SectionID = Se.Id
                            LEFT JOIN ORG.SubSection SB ON PR.SubSectionID = SB.Id
                            LEFT JOIN ORG.Line L ON MB.LineID = L.Id
                            LEFT JOIN HKP.Designation D ON PR.DesignationID = D.Id
                            LEFT JOIN HKP.LegalDesignation AS ld ON E.LegalDesignationId = ld.Id
                            LEFT JOIN ShiftDefination AS sd ON sd.SystemID=apd.ShiftSystemID                           
                            LEFT JOIN  EmpDateWiseShiftAssign AS edwsa ON edwsa.EmpSystemID = apd.EmpSystemID AND edwsa.WorkDate = apd.WorkDate
                           
                            LEFT JOIN OTSlabDefineGeneral pl ON pl.DayType = dt.OriginalDayType                        
															AND apd.WorkDate BETWEEN pl.FromDate AND pl.ToDate AND pl.PlantID=E.PlantID
                            WHERE apd.WorkDate='" + WDate + @"' AND  apd.IsOTEntitled=1 AND HOT.OTType='"+ OTType+@"'                           
                            AND E.PlantID='" + identity.PlantId + @"' ";




            var data = _sqlRepository.GetDataCollection(sql);

            JsonResult json = Json(new
            {
                data


            }, JsonRequestBehavior.AllowGet);

            json.MaxJsonLength = int.MaxValue;
            return json;
        }


        [HttpGet, Authorize]
        public ActionResult GetAttendanceProcessDataDateRangWise(string FromDate, string ToDate)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT FORMAT(apd.WorkDate,'dd-MMM-yyyy') WorkDate,Count(E.SystemId) EmployeeCount
	                           
                            FROM AttdnProcessData AS apd
                            INNER JOIN EmployeeInformation e ON e.SystemId = apd.EmpSystemID
                            LEFT JOIN DayType dt on dt.DayType=apd.DayStatus
                            LEFT JOIN HKP.EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.Id
                            LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                            LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                            LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id
                            LEFT JOIN ORG.Section Se ON E.SectionID = Se.Id
                            LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
                            LEFT JOIN ORG.Line L ON E.LineID = L.Id
                            LEFT JOIN HKP.Designation D ON E.DesignationSystemID = D.Id
                            LEFT JOIN HKP.LegalDesignation AS ld ON E.LegalDesignationId = ld.Id
                            LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
                            LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ShiftDefination AS sd ON sd.SystemID=apd.ShiftSystemID                           
                            LEFT JOIN  EmpDateWiseShiftAssign AS edwsa ON edwsa.EmpSystemID = apd.EmpSystemID AND edwsa.WorkDate = apd.WorkDate
                            ---LEFT JOIN OTSlabDefineGeneral pl ON pl.DayType = edwsa.DayType AND apd.WorkDate BETWEEN pl.FromDate AND pl.ToDate AND pl.PlantID=apd.PlantID
                            LEFT JOIN
                            (SELECT odm.OffDayType,d.PlantId,d.OffDayDate FROM scs.OffDayDetail  d
                             LEFT JOIN scs.OffDayMaster AS odm ON odm.Id = d.OffDayMasterId 
                             WHERE odm.OffDayType='H' AND d.PlantId='" + identity.PlantId + @"' AND d.OffDayDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"'  
                            ) AS odd ON odd.PlantId=apd.PlantID AND odd.OffDayDate = apd.WorkDate
                            LEFT JOIN [MST].[ExceptionForHolidayEmpList] AS efhel ON efhel.EmpSystemId =apd.EmpSystemID AND efhel.WorkDate =apd.WorkDate
                            LEFT JOIN OTSlabDefineGeneral pl ON pl.DayType = dt.OriginalDayType                       
															AND apd.WorkDate BETWEEN pl.FromDate AND pl.ToDate AND pl.PlantID=E.PlantID
                            WHERE apd.WorkDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"' 
                            AND  apd.IsOTEntitled=1 
                            AND E.PlantID='" + identity.PlantId + @"' AND ISNULL(apd.OTHr,0)/60 > pl.firstSlab
                            GROUP BY apd.WorkDate 
	                        ORDER BY CONVERT(DATE, apd.WorkDate )";




            var data = _sqlRepository.GetDataCollection(sql);

            JsonResult json = Json(new
            {
                data


            }, JsonRequestBehavior.AllowGet);

            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        [HttpGet, Authorize]
        public ActionResult GetAttendanceProcessUserDefine(string WDate, string NWDayType, string HDayType, string WDayType)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"SELECT [CheckBoxSelect] = Convert(BIT, 'False')
	                            ,FORMAT(apd.WorkDate, 'dd-MMM-yyyy') WorkDate
	                            ,sd.UserName ShiftName
	                            ,FORMAT(sd.InTime, 'hh:mm tt') ShiftInTime
	                            ,FORMAT(sd.OutTime, 'hh:mm tt') ShiftOutTime  
	                            ,FORMAT(apd.InTime, 'hh:mm tt') InTime
	                            ,FORMAT(apd.OutTime, 'hh:mm tt') OutTime
	                            ,apd.DayStatus
	                            ,apd.OTHr
	                            ,Category=CASE WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')!='' THEN edwsa.DayType
											WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')='' THEN odd.OffDayType
											ELSE edwsa.DayType END
	                            ,pl.IsOTExtentNextSlab
	                            ,pl.firstSlab
	                            ,pl.IsTotalWorkTimeAsOT
	                            ,TotalOT=  ISNULL(apd.OTHr,0)/60
	                            --,OT= CASE WHEN ISNULL(apd.OTHr,0)/60 > pl.firstSlab THEN  pl.firstSlab ELSE ISNULL(apd.OTHr,0)/60 END		
	                            --,ExtraOT= CASE WHEN ISNULL(apd.OTHr,0)/60 > pl.firstSlab THEN  ISNULL(apd.OTHr,0)/60-pl.firstSlab ELSE 0 END
                                ,OT= CASE WHEN ISNULL(apd.OTHr,0)/60 > " + NWDayType + @" AND  dt.OriginalDayType='NW'	THEN  " + NWDayType + @" 
										  WHEN ISNULL(apd.OTHr,0)/60 > " + WDayType + @" AND dt.OriginalDayType='W'	THEN  " + WDayType + @" 
										  WHEN ISNULL(apd.OTHr,0)/60 > " + HDayType + @" AND  dt.OriginalDayType='H'	THEN  " + HDayType + @" ELSE ISNULL(apd.OTHr,0)/60 END
												
	                            ,ExtraOT= CASE WHEN ISNULL(apd.OTHr,0)/60 > " + NWDayType + @" AND  dt.OriginalDayType='NW'	THEN  ISNULL(apd.OTHr,0)/60-" + NWDayType + @" 
										  WHEN ISNULL(apd.OTHr,0)/60 > " + WDayType + @" AND  dt.OriginalDayType='W'	THEN  ISNULL(apd.OTHr,0)/60-" + WDayType + @" 
										  WHEN ISNULL(apd.OTHr,0)/60 > " + HDayType + @" AND dt.OriginalDayType='H'	THEN ISNULL(apd.OTHr,0)/60-" + HDayType + @" ELSE 0 END
	                            ,E.SystemId
	                            ,e.EmployeeCode
	                            ,e.EmployeeName
	                            ,FORMAT(e.DOJ, 'dd-MMM-yyyy') DOJ
	                            ,EC.UserName EmpCategoryName
	                            ,ld.UserName Designation
	                            ,U.UserName Unit
	                            ,Dv.UserName Division
	                            ,Dp.UserName Department
	                            ,Se.UserName Section
	                            ,SB.UserName SubSection
	                            ,L.UserName Line
 	                            ,NewOutTime= CASE WHEN ISNULL(apd.OTHr,0)/60 > pl.firstSlab THEN DATEADD(minute, -1* (ISNULL(apd.OTHr,0)-pl.firstSlab*60),apd.OutTime) ELSE null END 
	                            ,ExtraOTInTime= CASE WHEN ISNULL(apd.OTHr,0)/60 > pl.firstSlab THEN DATEADD(minute, -1* (ISNULL(apd.OTHr,0)-pl.firstSlab*60),apd.OutTime) ELSE null END
	                            	                          
	                            ,NewOutTimeShow= CASE WHEN ISNULL(apd.OTHr,0)/60 > pl.firstSlab THEN FORMAT(DATEADD(minute, -1* (ISNULL(apd.OTHr,0)-pl.firstSlab*60),apd.OutTime), 'hh:mm tt') ELSE null END 
	                            ,ExtraOTInTimeShow= CASE WHEN ISNULL(apd.OTHr,0)/60 > pl.firstSlab THEN FORMAT( DATEADD(minute, -1* (ISNULL(apd.OTHr,0)-pl.firstSlab*60),apd.OutTime), 'hh:mm tt') ELSE null END	 
	                            ,ExtraOTOutTimeShow=FORMAT(apd.OutTime, 'hh:mm tt')
                                ,ExtraOTOutTime=apd.OutTime
	                            ,Duration= CASE WHEN ISNULL(apd.OTHr,0)/60 > pl.firstSlab THEN ISNULL(apd.OTHr,0)-pl.firstSlab*60 ELSE 0 END 
                                ,FirstSlabMin= CASE WHEN ISNULL(apd.OTHr,0)/60 > " + NWDayType + @" AND  dt.OriginalDayType='NW'	THEN  60*" + NWDayType + @" 
										            WHEN ISNULL(apd.OTHr,0)/60 > " + WDayType + @" AND  dt.OriginalDayType='W'	THEN  60*" + WDayType + @" 
										            WHEN ISNULL(apd.OTHr,0)/60 > " + HDayType + @" AND  dt.OriginalDayType='H'	THEN  60*" + HDayType + @" ELSE 0 END




                                ,IsManualInTime= CASE WHEN ISNULL(apd.IsManualOutTime,0)=1 THEN  'YES' ELSE 'NO' END  
                            FROM AttdnProcessData AS apd
                            INNER JOIN EmployeeInformation e ON e.SystemId = apd.EmpSystemID
                            LEFT JOIN DayType dt on dt.DayType=apd.DayStatus
                            LEFT JOIN HKP.EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.Id
                            LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
                            LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
                            LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id
                            LEFT JOIN ORG.Section Se ON E.SectionID = Se.Id
                            LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
                            LEFT JOIN ORG.Line L ON E.LineID = L.Id
                            LEFT JOIN HKP.Designation D ON E.DesignationSystemID = D.Id
                            LEFT JOIN HKP.LegalDesignation AS ld ON E.LegalDesignationId = ld.Id
                            LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
                            LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
                            LEFT JOIN ShiftDefination AS sd ON sd.SystemID=apd.ShiftSystemID                           
                            LEFT JOIN  EmpDateWiseShiftAssign AS edwsa ON edwsa.EmpSystemID = apd.EmpSystemID AND edwsa.WorkDate = apd.WorkDate
                            LEFT JOIN
                            (SELECT odm.OffDayType,d.PlantId,d.OffDayDate FROM scs.OffDayDetail  d
                             LEFT JOIN scs.OffDayMaster AS odm ON odm.Id = d.OffDayMasterId 
                             WHERE odm.OffDayType='H' AND d.PlantId='" + identity.PlantId + @"' AND d.OffDayDate ='" + WDate + @"' 
                            ) AS odd ON odd.PlantId=apd.PlantID AND odd.OffDayDate = apd.WorkDate
                            LEFT JOIN [MST].[ExceptionForHolidayEmpList] AS efhel ON efhel.EmpSystemId =apd.EmpSystemID AND efhel.WorkDate =apd.WorkDate
                            LEFT JOIN OTSlabDefineGeneral pl ON pl.DayType = dt.OriginalDayType
                                                                            AND apd.WorkDate BETWEEN pl.FromDate AND pl.ToDate AND pl.PlantID=E.PlantID 
                            WHERE apd.WorkDate='" + WDate + @"' AND  apd.IsOTEntitled=1 
                         
                            AND E.PlantID='" + identity.PlantId + @"' AND (
                             (ISNULL(apd.OTHr,0)/60 > " + NWDayType + @" AND  dt.OriginalDayType='NW') 
                                OR
                             (ISNULL(apd.OTHr,0)/60 > " + HDayType + @" AND  dt.OriginalDayType='H') 
                                OR  
                             (ISNULL(apd.OTHr,0)/60 > " + WDayType + @" AND dt.OriginalDayType='W') 
                             )

                            ORDER BY  e.EmployeeCodePreFix,e.EmployeeCodeNumeric";




            var data = _sqlRepository.GetDataCollection(sql);

            JsonResult json = Json(new
            {
                data


            }, JsonRequestBehavior.AllowGet);

            json.MaxJsonLength = int.MaxValue;
            return json;
        }


        [HttpGet, Authorize]
        public ActionResult GetOTSlabDefineGeneral(string WDate)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataSet dsOTSlabDefineGeneral = null;
            decimal NWDayType = 0;
            decimal HDayType = 0;
            decimal WDayType = 0;
            GetOTSlabDefineGeneral(identity.CompanyGroupId, identity.PlantId, WDate, out dsOTSlabDefineGeneral);
            if (dsOTSlabDefineGeneral.Tables[0].Rows.Count > 0)
            {
                for (int i = 0; i < dsOTSlabDefineGeneral.Tables[0].Rows.Count; i++)
                {
                    if (dsOTSlabDefineGeneral.Tables[0].Rows[i]["DayType"].ToString().Trim() == "NW")
                    {
                        if (!string.IsNullOrEmpty(dsOTSlabDefineGeneral.Tables[0].Rows[i]["firstSlab"].ToString()))
                        {
                            NWDayType = Convert.ToDecimal(dsOTSlabDefineGeneral.Tables[0].Rows[i]["firstSlab"].ToString().Trim());
                        }

                    }
                    if (dsOTSlabDefineGeneral.Tables[0].Rows[i]["DayType"].ToString().Trim() == "H")
                    {
                        if (!string.IsNullOrEmpty(dsOTSlabDefineGeneral.Tables[0].Rows[i]["firstSlab"].ToString()))
                        {
                            HDayType = Convert.ToDecimal(dsOTSlabDefineGeneral.Tables[0].Rows[i]["firstSlab"].ToString().Trim());
                        }
                    }

                    if (dsOTSlabDefineGeneral.Tables[0].Rows[i]["DayType"].ToString().Trim() == "W")
                    {
                        if (!string.IsNullOrEmpty(dsOTSlabDefineGeneral.Tables[0].Rows[i]["firstSlab"].ToString()))
                        {
                            WDayType = Convert.ToDecimal(dsOTSlabDefineGeneral.Tables[0].Rows[i]["firstSlab"].ToString().Trim());
                        }

                    }
                }
            }
            JsonResult json = Json(new
            {
                NWDayType,
                HDayType,
                WDayType

            }, JsonRequestBehavior.AllowGet);

            json.MaxJsonLength = int.MaxValue;
            return json;
        }
        #endregion








        public void SaveAttendanceRawDataBackupDataSetsAndDelete(string DeleteEmpSystemId, string WDate, params DataSet[] dsRef)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                objCon.ExecuteNonQueryWrapper("DELETE FROM AttdnRawData WHERE LogDownLoadNum IN (" + DeleteEmpSystemId + ") AND PDate IN ('" + WDate + @"')", true, "1");
                objCon.ExecuteNonQueryWrapper("DELETE FROM AttdnManualData WHERE EmpSystemID IN (" + DeleteEmpSystemId + ") AND WorkDate IN ('" + WDate + @"')", true, "1");
                int i = 0;
                foreach (DataSet value in dsRef)
                {
                    if (dsRef[i] != null)
                    {
                        objCon.SaveDataSetThroughAdapter(ref dsRef[i], true, "1");
                        i = i + 1;
                    }
                    else
                    {
                        i = i + 1;
                    }
                }
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (IsTransactionStarted)
                {
                    objCon.RollBack();
                }
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function
        public void SaveAttendanceRawDataBackupDataSetsAndDeleteEmpWise(string EmpSystemId, string WDateList, params DataSet[] dsRef)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                objCon.ExecuteNonQueryWrapper("DELETE FROM AttdnRawData WHERE LogDownLoadNum =" + EmpSystemId + " AND PDate IN ( " + WDateList + @")", true, "1");
                int i = 0;
                foreach (DataSet value in dsRef)
                {
                    if (dsRef[i] != null)
                    {
                        objCon.SaveDataSetThroughAdapter(ref dsRef[i], true, "1");
                        i = i + 1;
                    }
                    else
                    {
                        i = i + 1;
                    }
                }
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (IsTransactionStarted)
                {
                    objCon.RollBack();
                }
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function


        public void GetOTSlabDefineGeneral(string sGroupID, string sPlantID, string sAttnDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT * FROM dbo.OTSlabDefineGeneral
                           WHERE '" + sAttnDate + @"' BETWEEN FromDate AND ToDate AND GroupID = '" + sGroupID + @"' 
                                 AND PlantID = '" + sPlantID + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//End Function 

    }

    public class ExtraOTVM
    {
        public bool CheckBoxSelect { get; set; }
        public string WorkDate { get; set; }
        public string ShiftName { get; set; }
        public string ShiftInTime { get; set; }
        public string ShiftOutTime { get; set; }
        public string InTime { get; set; }
        public string OutTime { get; set; }
        public string DayStatus { get; set; }
        public string OTHr { get; set; }
        public string Category { get; set; }
        public string IsOTExtentNextSlab { get; set; }
        public string firstSlab { get; set; }
        public string IsTotalWorkTimeAsOT { get; set; }
        public string TotalOT { get; set; }
        public string OT { get; set; }
        public string ExtraOT { get; set; }
        public string SystemId { get; set; }
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }
        public string DOJ { get; set; }
        public string EmpCategoryName { get; set; }
        public string Designation { get; set; }
        public string Unit { get; set; }
        public string Division { get; set; }
        public string Department { get; set; }
        public string Section { get; set; }
        public string SubSection { get; set; }
        public string Line { get; set; }
        public DateTime NewOutTime { get; set; }
        public DateTime ExtraOTInTime { get; set; }
        public DateTime ExtraOTOutTime { get; set; }
        public string NewOutTimeShow { get; set; }
        public string ExtraOTInTimeShow { get; set; }
        public string ExtraOTOutTimeShow { get; set; }
        public string Duration { get; set; }
        public string FirstSlabMin { get; set; }
        //public string IsManualInTime { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public bool IsRecoveryRawData { get; set; }
        //public string Duration { get; set; }
        public bool IsManualInTime { get; set; }
        public string ManualInTime { get; set; }
        public bool IsManualOutTime { get; set; }
        public string ManualOutTime { get; set; }
        public bool IsRecoveryManualData { get; set; }
    }
}