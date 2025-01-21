#region Using
using Aplos.Controllers;
using Aplos.Properties;
using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.HumanResource.NewAttendanceProcess;
using Library.Model.Attendances;
using Library.Model.Biometrics;
using Library.Service.Attendances;
using Library.Service.Biometrics;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;
#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class OTLimitTransactionController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IOTManagementService _OTManagementService;
        public OTLimitTransactionController(ISqlRepository sqlRepository, IOTManagementService OTManagementService)
        {
            _sqlRepository = sqlRepository;
            _OTManagementService = OTManagementService;
        }
        #endregion

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }


        [HttpGet, Authorize]
        public ActionResult GetOTLimitSetting()
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" select * from dbo.OTLimitSetting where PlantID ='" + identity.PlantId + @"' ";




            var data = _sqlRepository.GetDataCollection(sql);

            JsonResult json = Json(new
            {
                data


            }, JsonRequestBehavior.AllowGet);

            json.MaxJsonLength = int.MaxValue;
            return json;
        }


        [HttpGet, Authorize]
        public ActionResult GetOTLimitSettingDetails(string Id)
        {


            string sql = @" select * from dbo.OTLimitSetting where Id ='" + Id + @"' ";
            var data = _sqlRepository.GetDataCollection(sql);

            JsonResult json = Json(new
            {
                data


            }, JsonRequestBehavior.AllowGet);

            json.MaxJsonLength = int.MaxValue;
            return json;
        }


        [HttpGet]
        public ActionResult GetOTLimitOverlapData(string YearNo, string MonthNo, string OTLimitSettingId)
        {
            DataSet dsOTLimitSetting;
            string OTLimit = string.Empty;
            string FromDate = string.Empty;
            string ToDate = string.Empty;
            decimal FactorOT = 0;

            ConnectionManager.DAL.ConManager objCon;
            string sql2 = "SELECT * FROM OTLimitSetting WHERE Id='" + OTLimitSettingId + "'";
            objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(sql2, out dsOTLimitSetting, false, "1");
            if (dsOTLimitSetting.Tables[0].Rows.Count > 0)
            {
                OTLimit = dsOTLimitSetting.Tables[0].Rows[0]["MinOTLimitParDay"].ToString();

                string week = dsOTLimitSetting.Tables[0].Rows[0]["Week"].ToString();
                if (week == "First Week")
                {
                    FromDate = "01-" + MonthNo + "-" + YearNo;
                    ToDate = "07-" + MonthNo + "-" + YearNo;
                }

                if (week == "Second Week")
                {
                    FromDate = "08-" + MonthNo + "-" + YearNo;
                    ToDate = "14-" + MonthNo + "-" + YearNo;
                }
                if (week == "Third Week")
                {
                    FromDate = "15-" + MonthNo + "-" + YearNo;
                    ToDate = "21-" + MonthNo + "-" + YearNo;
                }
                if (week == "Last Week")
                {
                    FromDate = "22-" + MonthNo + "-" + YearNo;
                    ToDate = Convert.ToDateTime("01-" + MonthNo + "-" + YearNo).AddMonths(1).AddDays(-1).ToString("dd-MMMM-yyyy");
                }



                FactorOT = Convert.ToDecimal(dsOTLimitSetting.Tables[0].Rows[0]["OTreductionFactor"].ToString());

            }




            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" SELECT  [CheckBoxSelect] = Convert(BIT, 'False'), apd.EmpSystemID
                              ,SUM(apd.ProcessedOT) TotalOTHr
                              ,FactorOT=SUM(apd.ProcessedOT)*" + FactorOT + @" --,dt.OriginalDayType
                              ,EI.EmployeeCode
                              ,EI.EmployeeName
                              ,format(EI.DOJ,'dd-MMM-yyyy') DOJ                            
                              --,DG.UserName GivenDesignation
                              --,DP.UserName Department
                              --,PMB.Code
                              --,PR.UserName PositionName
                              --,E.UserName EntityName
                              --,DSG.UserName Designation
                              --,PR.DesignationId
                              ---,PG.StandardName PayRollGroupName
                              --,PG.Id PayRollGroupId							
                              ,ld.UserName LegalDesignation
                              --,Isnull(excot.IsExceptionOT,0) IsExceptionOT
                              --,ec.UserName EmployeeCategory
                              --,LSG.UserName SalaryGrade ,s.UserName  Section,sb.UserName SubSection,IsExceptionOT
                             FROM  AttdnProcessData AS apd
                             INNER JOIN EmployeeInformation EI ON EI.SystemId = apd.EmpSystemID 
                             
                             LEFT JOIN  HKP.LegalDesignation AS ld ON ld.Id=ei.LegalDesignationId---- 
                            
                             LEFT JOIN HourlyOT eot  on eot.EmpSystemId=apd.EmpSystemID and eot.WorkDate=apd.WorkDate 
                            
                             WHERE apd.WorkDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"'  AND EI.PlantID='" + identity.PlantId + @"'                            
                             AND  apd.IsOTEntitled=1 AND isnull(APD.IsOTComfirm,0)=0 --AND ISNULL(apd.ProcessedOT,0)>0
                           

                            GROUP BY apd.EmpSystemID 
                              ,EI.EmployeeCode
                              ,EI.EmployeeName
                              ,format(EI.DOJ,'dd-MMM-yyyy')  
                              ,ld.UserName";

            var data = _sqlRepository.GetDataCollection(sql);

            JsonResult json = Json(new
            {
                data


            }, JsonRequestBehavior.AllowGet);

            json.MaxJsonLength = int.MaxValue;
            return json;
        }

        #region ProcessedOT


        #endregion ProcessedOT

        [HttpPost]
        public ActionResult SaveOTLimitOverlapData(string YearNo, string MonthNo, string OTLimitSettingId, string[] EmpSystemIds)
        {


            DataSet dsOTLimitSetting;
            string OTLimit = string.Empty;
            string FromDate = string.Empty;
            string ToDate = string.Empty;
            string EmpSytemId = string.Empty;
            decimal MinOTLimitParDay = 0;
            decimal MaxOTLimitParDay = 0;
            decimal MaxOTLimitParWeek = 0;
            decimal OTreductionFactor = 0;

            decimal MaxWeekOffOTLimitParDay = 0;
            decimal MaxHolidayOTLimitParDay = 0;

            List<OTLimitTransactionVM> oOTLimitTransaction = new List<OTLimitTransactionVM>();



            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;

            foreach (var item in EmpSystemIds)
            {
                if (EmpSytemId == "")
                    EmpSytemId = "'" + item.ToString() + "'";
                else
                    EmpSytemId = EmpSytemId + ",'" + item.ToString() + "'";
            }


            string sql2 = "SELECT * FROM OTLimitSetting WHERE Id='" + OTLimitSettingId + "'";
            objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(sql2, out dsOTLimitSetting, false, "1");
            if (dsOTLimitSetting.Tables[0].Rows.Count > 0)
            {
                OTLimit = dsOTLimitSetting.Tables[0].Rows[0]["MinOTLimitParDay"].ToString();
                //FromDate = dsOTLimitSetting.Tables[0].Rows[0]["FromDay"].ToString() + "-" + MonthNo + "-" + YearNo;
                //ToDate = dsOTLimitSetting.Tables[0].Rows[0]["ToDay"].ToString() + "-" + MonthNo + "-" + YearNo;
                string week = dsOTLimitSetting.Tables[0].Rows[0]["Week"].ToString();
                if (week == "First Week")
                {
                    FromDate = "01-" + MonthNo + "-" + YearNo;
                    ToDate = "07-" + MonthNo + "-" + YearNo;
                }

                if (week == "Second Week")
                {
                    FromDate = "08-" + MonthNo + "-" + YearNo;
                    ToDate = "14-" + MonthNo + "-" + YearNo;
                }
                if (week == "Third Week")
                {
                    FromDate = "15-" + MonthNo + "-" + YearNo;
                    ToDate = "21-" + MonthNo + "-" + YearNo;
                }
                if (week == "Last Week")
                {
                    FromDate = "22-" + MonthNo + "-" + YearNo;
                    ToDate = Convert.ToDateTime("01-" + MonthNo + "-" + YearNo).AddMonths(1).AddDays(-1).ToString("dd-MMMM-yyyy");
                }
                MinOTLimitParDay = Convert.ToDecimal(dsOTLimitSetting.Tables[0].Rows[0]["MinOTLimitParDay"].ToString());
                MaxOTLimitParDay = Convert.ToDecimal(dsOTLimitSetting.Tables[0].Rows[0]["MaxOTLimitParDay"].ToString());
                MaxOTLimitParWeek = Convert.ToDecimal(dsOTLimitSetting.Tables[0].Rows[0]["MaxOTLimitParWeek"].ToString());
                OTreductionFactor = Convert.ToDecimal(dsOTLimitSetting.Tables[0].Rows[0]["OTreductionFactor"].ToString());

                MaxWeekOffOTLimitParDay = Convert.ToDecimal(dsOTLimitSetting.Tables[0].Rows[0]["MaxWeekOffOTLimitParDay"].ToString());
                MaxHolidayOTLimitParDay = Convert.ToDecimal(dsOTLimitSetting.Tables[0].Rows[0]["MaxHolidayOTLimitParDay"].ToString());


            }


            ////rollback all riginal manual IN OUT data to manual IN/OUT columns
            //RollBackAllInOutData(FromDate, ToDate, EmpSytemId);


            ////Process data to generate final IN/OUT data (based on punch and manual data)
            //FinalInOut(FromDate, ToDate, EmpSytemId);

            ////process ProcessedOT based on Final IN/OUT
            //ProcessForProcessedOT(EmpSytemId);

            //get OT data after rollback and IN/OUT Process
            DataSet dsOTDetails = null;
            GetOTData(FromDate, ToDate, EmpSytemId, out dsOTDetails);




            if (EmpSystemIds.Length > 0)
            {
                foreach (var item in EmpSystemIds)
                {

                    DataView dv = new DataView(dsOTDetails.Tables[0]);
                    dv.RowFilter = "EmpSystemID='" + item + "'";
                    //dv.Count
                    if (dv.Count > 0)
                    {

                        decimal TotalWeeklyOT = 0;
                        for (int i = 0; i < dv.Count; i++)
                        {
                            OTLimitTransactionVM o = new OTLimitTransactionVM();
                            //string EmpSystemID = dv[i]["EmpSystemID"].ToString();
                            decimal TotalOTHr = Convert.ToDecimal(dv[i]["TotalOTHr"].ToString());
                            //string WorkDate = dv[i]["WorkDate"].ToString();
                            //string ShiftName = dv[i]["ShiftName"].ToString();
                            //string ShiftInTime = dv[i]["ShiftInTime"].ToString();
                            //string ShiftOutTime = dv[i]["ShiftOutTime"].ToString();
                            //string InTime = dv[i]["InTime"].ToString();
                            //string OutTime = dv[i]["OutTime"].ToString();
                            //string DayStatus = dv[i]["DayStatus"].ToString();
                            decimal ExtraOT = 0;
                            decimal NewOT = 0;




                            o.OTreductionFactor = OTreductionFactor;
                            decimal DailyOT = Convert.ToDecimal(dv[i]["TotalOTHr"]) * OTreductionFactor;
                            string OriginalDayType = dv[i]["OriginalDayType"].ToString();
                            bool IsExceptionOT = Convert.ToBoolean(dv[i]["IsExceptionOT"]);

                            //decimal FirstSlabMin = Convert.ToDecimal(dv[i]["FirstSlabMin"]);
                            if (IsExceptionOT)
                            {
                                if (DailyOT > 0)
                                {


                                    if (DailyOT >= MinOTLimitParDay)
                                    {
                                        ExtraOTCalculation(DailyOT, MaxOTLimitParWeek, DailyOT, ref TotalWeeklyOT, out NewOT, out ExtraOT);
                                        //ExtraOTCalculation(DailyOT, MaxOTLimitParWeek, MaxOTLimitParDay, ref TotalWeeklyOT, out NewOT, out ExtraOT);

                                    }
                                    else
                                    {
                                        NewOT = 0;
                                        //ExtraOT = DailyOT;
                                        ExtraOT = 0;
                                    }


                                }
                                else// all r 0
                                {
                                    NewOT = 0;
                                    ExtraOT = 0;
                                }
                                o.IsExtraOTOnly = false;
                            }
                            else///regular
                            {
                                if (dv[i]["OriginalDayType"].ToString().ToUpper() == "NW")
                                {
                                    if (DailyOT > 0)
                                    {


                                        if (DailyOT >= MinOTLimitParDay)
                                        {
                                            ExtraOTCalculation(DailyOT, MaxOTLimitParWeek, MaxOTLimitParDay, ref TotalWeeklyOT, out NewOT, out ExtraOT);

                                        }
                                        else
                                        {
                                            NewOT = 0;
                                            //ExtraOT = DailyOT;
                                            ExtraOT = 0;
                                        }


                                    }
                                    else// all r 0
                                    {
                                        NewOT = 0;
                                        ExtraOT = 0;
                                    }
                                    o.IsExtraOTOnly = false;

                                }

                                else if (dv[i]["OriginalDayType"].ToString().ToUpper() == "W")
                                {
                                    if (DailyOT > 0)
                                    {


                                        if (DailyOT >= MinOTLimitParDay)
                                        {
                                            ExtraOTCalculation(DailyOT, MaxOTLimitParWeek, MaxWeekOffOTLimitParDay, ref TotalWeeklyOT, out NewOT, out ExtraOT);

                                        }
                                        else
                                        {
                                            NewOT = 0;
                                            //ExtraOT = DailyOT;
                                            ExtraOT = 0;
                                        }


                                    }
                                    else// all r 0
                                    {
                                        NewOT = 0;
                                        ExtraOT = 0;
                                    }
                                    o.IsExtraOTOnly = false;
                                    if (MaxWeekOffOTLimitParDay == 0)
                                    {
                                        o.IsExtraOTOnly = true;
                                    }
                                }
                                else if (dv[i]["OriginalDayType"].ToString().ToUpper() == "H")
                                {
                                    if (DailyOT > 0)
                                    {


                                        if (DailyOT >= MinOTLimitParDay)
                                        {
                                            ExtraOTCalculation(DailyOT, MaxOTLimitParWeek, MaxHolidayOTLimitParDay, ref TotalWeeklyOT, out NewOT, out ExtraOT);

                                        }
                                        else
                                        {
                                            NewOT = 0;
                                            //ExtraOT = DailyOT;
                                            ExtraOT = 0;
                                        }


                                    }
                                    else// all r 0
                                    {
                                        NewOT = 0;
                                        ExtraOT = 0;
                                    }
                                    o.IsExtraOTOnly = false;
                                    if (MaxHolidayOTLimitParDay == 0)
                                    {
                                        o.IsExtraOTOnly = true;
                                    }
                                }


                                else//W and H
                                {





                                    NewOT = 0;
                                    ExtraOT = DailyOT;
                                    o.IsExtraOTOnly = true;
                                }
                            }



                            //o.FirstSlabMin = FirstSlabMin;
                            o.EmpSystemId = dv[i]["EmpSystemID"].ToString();
                            o.WorkDate = dv[i]["WorkDate"].ToString();
                            o.ShiftInTime = dv[i]["ShiftInTime"].ToString();
                            o.ShiftOutTime = dv[i]["ShiftOutTime"].ToString();
                            o.InTime = dv[i]["InTime"].ToString();
                            o.OutTime = dv[i]["OutTime"].ToString();

                            o.NewInTime = dv[i]["InTime"].ToString();
                            o.NewOutTime = dv[i]["OutTime"].ToString();

                            o.DayStatus = dv[i]["DayStatus"].ToString();
                            o.OriginalDayType = dv[i]["OriginalDayType"].ToString();
                            o.ShiftName = dv[i]["ShiftName"].ToString();

                            o.OverStay = (decimal)OTSBD.clsStaticInfo.dbl(dv[i]["OverStay"].ToString());

                            o.IsManualInTime = Convert.ToBoolean(dv[i]["IsManualInTime"].ToString());
                            o.ManualInTime = dv[i]["ManualInTime"].ToString();


                            o.IsManualOutTime = Convert.ToBoolean(dv[i]["IsManualOutTime"].ToString());
                            o.ManualOutTime = dv[i]["ManualOutTime"].ToString();

                            if (string.IsNullOrEmpty(o.NewOutTime) == false)
                            {
                                //double TimeToReduce = (double)(((NewOT + ExtraOT) / OTreductionFactor) - NewOT);
                                double TimeToReduce = (double)((o.OverStay * -1) + (NewOT));
                                o.NewOutTime = Convert.ToDateTime(o.NewOutTime).AddMinutes(TimeToReduce).ToString("dd-MMM-yyyy hh:mm:ss tt");
                            }


                            o.TotalOT = DailyOT;
                            o.OT = NewOT;
                            o.ExtraOT = ExtraOT;
                            oOTLimitTransaction.Add(o);

                        }
                    }
                    dv.RowFilter = null;

                }
            }

            SaveDataWithOTConfirmed(oOTLimitTransaction, EmpSytemId, FromDate, ToDate);
            return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);


        }





        public void ExtraOTCalculation(decimal DailyOT, decimal WeeklyLimit, decimal DailyMaxLimit, ref decimal TotalWeeklyOT, out decimal OT, out decimal ExtraOT)
        {
            OT = 0;
            ExtraOT = 0;
            decimal CrossValue = 0;
            try
            {
                decimal TotalWeeklyOT_New = TotalWeeklyOT + DailyOT;
                if (TotalWeeklyOT_New > WeeklyLimit || DailyOT > DailyMaxLimit)
                {
                    if (TotalWeeklyOT_New > WeeklyLimit && DailyOT >= DailyMaxLimit)
                    {
                        decimal CrossValueWeekly = TotalWeeklyOT_New - WeeklyLimit;//60
                        decimal CrossValueDaily = DailyOT - DailyMaxLimit;//60

                        if (CrossValueWeekly > CrossValueDaily)//w
                        {
                            CrossValue = CrossValueWeekly;

                            OT = DailyOT - CrossValue;//30

                            TotalWeeklyOT += OT;
                            ExtraOT = CrossValue;
                        }
                        else//d
                        {
                            CrossValue = CrossValueDaily;
                            OT = DailyMaxLimit;
                            TotalWeeklyOT += DailyMaxLimit;
                            ExtraOT = CrossValue;
                        }

                    }
                    else if (DailyOT > DailyMaxLimit)//d
                    {
                        CrossValue = DailyOT - DailyMaxLimit;//210-90=120
                        ExtraOT = CrossValue;//120
                        OT = DailyMaxLimit;
                        TotalWeeklyOT += DailyMaxLimit;
                    }
                    else //if (TotalWeeklyOT_New > WeeklyLimit)//w
                    {
                        CrossValue = TotalWeeklyOT_New - WeeklyLimit;//360-300=60

                        OT = DailyOT - CrossValue;//30
                        ExtraOT = CrossValue;//60
                        TotalWeeklyOT += OT;

                    }
                    //--------------------------------------------------

                }
                else
                {


                    //if (TotalWeeklyOT < DailyLimit)
                    //{

                    //}
                    //else
                    //{
                    //    OT = 0;
                    //    ExtraOT = DailyOT;
                    //}


                    //if (DailyOT <= DailyLimit)
                    //{
                    OT = DailyOT;
                    TotalWeeklyOT += DailyOT;
                    //}
                    //else
                    //{
                    //    OT = DailyLimit;
                    //    ExtraOT = DailyOT - DailyLimit;
                    //}
                }
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void RollBackAllInOutData(string FromDate, string ToDate, string EmpSystemId)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {

                //strSql = @"UPDATE AttdnProcessData SET ManualInTime = ISNULL(OriginalManualInTime,ManualInTime),
                //            IsManualInTime = CASE WHEN ISNULL(ISNULL(OriginalManualInTime,ManualInTime),'')<>'' THEN 1 ELSE 0 END,
                //            ManualOutTime = ISNULL(OriginalManualOutTime,ManualOutTime),
                //            IsManualOutTime = CASE WHEN ISNULL(ISNULL(OriginalManualOutTime,ManualOutTime),'')<>'' THEN 1 ELSE 0 END
                //            WHERE WorkDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"' AND EmpSystemID IN (" + EmpSystemId + @")";


                strSql = @"UPDATE AttdnProcessData SET ManualInTime = OriginalManualInTime,
                            IsManualInTime = CASE WHEN ISNULL(OriginalManualInTime,'')<>'' OR IsManualInTime=1 THEN 1 ELSE 0 END,
                            ManualOutTime = OriginalManualOutTime,
                            IsManualOutTime = CASE WHEN ISNULL(OriginalManualOutTime,'')<>'' OR IsManualOutTime=1 THEN 1 ELSE 0 END,
                            ManualFlag=1
                            WHERE WorkDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"' AND EmpSystemID IN (" + EmpSystemId + @")";

                ConnectionManager.clsConnection _con = new ConnectionManager.clsConnection();
                _con.BeginTransaction();
                _con.executeQuery(strSql);
                _con.CommitTransaction();
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
        public void FinalInOut(string FromDate, string ToDate, string EmpSystemId)
        {
            try
            {
                var sql = @"update AttdnProcessData set InTime=ISNULL(ManualInTime,PunchInTime),OutTime=
				 ISNULL(ManualOutTime,PunchOutTime),UpdatedBy='OTLIMIT ROLLBACK',DateUpdated=GETDATE()
				 WHERE WorkDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"' AND EmpSystemID IN (" + EmpSystemId + @")";

                ConnectionManager.DAL.ConManager objCone = null;
                objCone = new ConnectionManager.DAL.ConManager("1");
                objCone.OpenConnection("1");
                objCone.BeginTransaction();

                objCone.ExecuteNonQueryWrapper(sql, true, "1");
                objCone.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void ProcessForProcessedOT(string EmpSystemId)
        {
            try
            {
                var sql = @"SELECT DISTINCT PlantId FROM EmployeeInformation AS ei WHERE ei.SystemID IN (" + EmpSystemId + @")";

                ConnectionManager.clsConnection _con = new ConnectionManager.clsConnection();
                _con.getDataSet(sql, out DataSet dsRef);

                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    NewAttendanceProcessService _service = new NewAttendanceProcessService();
                    _service.ManualScheduler(dsRef.Tables[0].Rows[i]["PlantId"].ToString());
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public void GetOTData(string FromDate, string ToDate, string EmpSystemId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {




                strSql = @"SELECT  apd.EmpSystemID
                           ,APD.ProcessedOT TotalOTHr,apd.OverStay
                           ,FORMAT(apd.WorkDate, 'dd-MMM-yyyy') WorkDate
                           ,sd.UserName ShiftName
                           ,FORMAT(sd.InTime, 'hh:mm tt') ShiftInTime
                           ,FORMAT(sd.OutTime, 'hh:mm tt') ShiftOutTime  
                           ,FORMAT(apd.InTime, 'dd-MMM-yyyy hh:mm tt') InTime
                           ,FORMAT(apd.OutTime, 'dd-MMM-yyyy hh:mm tt') OutTime
                           ,apd.DayStatus,dt.OriginalDayType
                           ,apd.IsManualInTime
						   ,apd.IsManualOutTime
                           ,ManualInTime=CASE WHEN ISNULL(apd.IsManualInTime,0)=1 THEN  apd.InTime  END 
						   ,ManualOutTime=CASE WHEN ISNULL(apd.IsManualOutTime,0)=1 THEN  apd.OutTime  END 
                           ---,FirstSlabMin= Isnull(pl.firstSlab,0)*60 
                           ,Isnull(excot.IsExceptionOT,0) IsExceptionOT
                           FROM  AttdnProcessData AS apd
                           --INNER JOIN OTfromApp OTFA  on OTFA.EmpSystemId=apd.EmpSystemID and OTFA.WorkDate=apd.WorkDate 
                           LEFT JOIN EmployeeInformation EI ON EI.SystemId = APD.EmpSystemID 
                           LEFT JOIN ShiftDefination AS sd ON sd.SystemID=apd.ShiftSystemID 
                           LEFT JOIN DayType dt on dt.DayType=apd.DayStatus
                           LEFT JOIN (select IsExceptionOT=case when id is not null then 1 else 0 end ,EmpSystemId,WorkDate from ExceptionOTProcess) excot  on excot.EmpSystemId=apd.EmpSystemID and excot.WorkDate=apd.WorkDate 

                           ---LEFT JOIN OTSlabDefineGeneral pl ON pl.DayType = dt.OriginalDayType                        
															---AND apd.WorkDate BETWEEN pl.FromDate AND pl.ToDate AND pl.PlantID=EI.PlantID
                           WHERE apd.WorkDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"' and isnull(apd.EmpSystemID,'') IN ( " + EmpSystemId + @" )                                                 
                           AND apd.IsOTEntitled=1 AND isnull(APD.ProcessedOT,0)>0 --AND isnull(APD.IsOTComfirm,0)=0 ---AND OTFA.IsConfirmed=0
                           --AND isnull(apd.EmpSystemID,'') IN (select distinct isnull(EmpSystemID,'') from OTfromApp where IsConfirmed=0 and WorkDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"' )
                               
                           ORDER BY apd.EmpSystemID ,APD.ProcessedOT desc";

                ConnectionManager.clsConnection _con = new ConnectionManager.clsConnection();
                _con.getDataSet(strSql, out dsRef);
                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void SaveData(List<OTLimitTransactionVM> AttendanceProcessData, string EmpSytemId, string FromDate, string ToDate)
        {
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;



            DataSet dsManualAttanData = null;
            DataSet dsHourlyOTData = null;

            ConnectionManager.DAL.ConManager objCon;
            try
            {






                clsAttendance.AttendanceProcessAplos obj = new clsAttendance.AttendanceProcessAplos();
                obj.LockValidation(identity.PlantId, FromDate, ToDate, EmpSytemId);


                string sql = "SELECT * FROM [dbo].[AttdnManualData] WHERE EmpSystemID IN (" + EmpSytemId + ") AND WorkDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsManualAttanData, false, "1");

                string sql1 = "SELECT * FROM [dbo].[HourlyOT] WHERE EmpSystemID IN (" + EmpSytemId + ") AND WorkDate  BETWEEN '" + FromDate + @"' AND '" + ToDate + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql1, out dsHourlyOTData, false, "1");





                DataView DvMaster = new DataView(dsManualAttanData.Tables[0]);
                DataView DvHourlyOTData = new DataView(dsHourlyOTData.Tables[0]);
                Random rnd = new Random((int)DateTime.Now.Ticks);
                for (int i = 0; i < AttendanceProcessData.Count; i++)
                {



                    string JoinDT = string.Empty;
                    string Date = Convert.ToDateTime(AttendanceProcessData[i].WorkDate).ToString("dd-MMM-yyyy");
                    string SOutTime = Convert.ToDateTime(AttendanceProcessData[i].ShiftOutTime).ToString("hh:mm tt");
                    string SInTime = Convert.ToDateTime(AttendanceProcessData[i].ShiftInTime).ToString("hh:mm tt");
                    //if (AttendanceProcessData[i].Category == "NW")
                    //{
                    //    JoinDT = Date + " " + SOutTime;
                    //}
                    //if (AttendanceProcessData[i].Category == "W")
                    //{
                    //    JoinDT = Date + " " + SInTime;
                    //}
                    //if (AttendanceProcessData[i].Category == "H")
                    //{
                    //    JoinDT = Date + " " + SInTime;
                    //}

                    //night shift
                    if (Convert.ToDateTime(Date + " " + SInTime) > Convert.ToDateTime(Date + " " + SOutTime))
                    {
                        Date = Convert.ToDateTime(AttendanceProcessData[i].WorkDate).AddDays(1).ToString("dd-MMM-yyyy");
                    }
                    JoinDT = Date + " " + SOutTime;

                    DateTime d1 = Convert.ToDateTime(JoinDT);
                    DateTime NewOutTime = d1.AddMinutes(Convert.ToInt32(AttendanceProcessData[i].OT));

                    int RandomMinutes = rnd.Next(0, 15);
                    var RandomOutTime = NewOutTime.AddMinutes(RandomMinutes);

                    DateTime d2 = Convert.ToDateTime(RandomOutTime);
                    DateTime ExtraOTOutTime = d2.AddMinutes(Convert.ToInt32(AttendanceProcessData[i].ExtraOT));
                    //Manual Attendance 
                    DvMaster.RowFilter = "EmpSystemID='" + AttendanceProcessData[i].EmpSystemId + "' AND WorkDate='" + AttendanceProcessData[i].WorkDate + "'";
                    if (DvMaster.Count == 0)
                    {

                        DataRow dr = dsManualAttanData.Tables[0].NewRow();
                        dr["EmpSystemID"] = AttendanceProcessData[i].EmpSystemId;
                        dr["WorkDate"] = Convert.ToDateTime(AttendanceProcessData[i].WorkDate);
                        dr["GroupID"] = identity.CompanyGroupId;
                        //dr["PlantID"] = identity.PlantId;
                        dr["EntryFlag"] = "OTLIMIT";
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
                        dr["EntryFlag"] = "OTLIMIT";
                        //dr["OutTime"] = AttendanceProcessData[i].NewOutTime;
                        dr["OutTime"] = Convert.ToDateTime(RandomOutTime);
                        dr["UpdatedBy"] = identity.Name;
                        dr["DateUpdated"] = System.DateTime.Now.ToString();
                        dr.EndEdit();

                    }
                    DvMaster.RowFilter = null;



                    DvHourlyOTData.RowFilter = "EmpSystemID='" + AttendanceProcessData[i].EmpSystemId + "' AND WorkDate='" + AttendanceProcessData[i].WorkDate + "'";
                    if (DvHourlyOTData.Count == 0)
                    {
                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "HourlyOT", out sID);
                        DataRow dr = dsHourlyOTData.Tables[0].NewRow();
                        dr["Id"] = "OLEO" + sID;
                        dr["EmpSystemId"] = AttendanceProcessData[i].EmpSystemId;
                        //dr["FromDate"] = AttendanceProcessData[i].ExtraOTInTime;
                        dr["FromDate"] = RandomOutTime;
                        dr["ToDate"] = ExtraOTOutTime;
                        dr["Duration"] = AttendanceProcessData[i].ExtraOT;
                        dr["WorkDate"] = Convert.ToDateTime(AttendanceProcessData[i].WorkDate);

                        dr["IsManualInTime"] = AttendanceProcessData[i].IsManualInTime;
                        if (AttendanceProcessData[i].IsManualInTime)
                        {
                            dr["ManualInTime"] = Convert.ToDateTime(AttendanceProcessData[i].ManualInTime);
                        }
                        dr["IsManualOutTime"] = AttendanceProcessData[i].IsManualOutTime;
                        if (AttendanceProcessData[i].IsManualOutTime)
                        {
                            dr["ManualOutTime"] = Convert.ToDateTime(AttendanceProcessData[i].ManualOutTime);
                        }


                        dr["PlantId"] = identity.PlantId;
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr["OTType"] = "OTLIMIT";
                        dsHourlyOTData.Tables[0].Rows.Add(dr);

                    }
                    else
                    {
                        DataRow dr = DvHourlyOTData[0].Row;
                        dr.BeginEdit();
                        dr["EmpSystemId"] = AttendanceProcessData[i].EmpSystemId;
                        //dr["FromDate"] = AttendanceProcessData[i].ExtraOTInTime;
                        dr["FromDate"] = RandomOutTime;
                        dr["ToDate"] = ExtraOTOutTime;
                        dr["Duration"] = AttendanceProcessData[i].ExtraOT;
                        dr["WorkDate"] = Convert.ToDateTime(AttendanceProcessData[i].WorkDate);
                        dr["IsManualInTime"] = AttendanceProcessData[i].IsManualInTime;
                        if (AttendanceProcessData[i].IsManualInTime)
                        {
                            dr["ManualInTime"] = Convert.ToDateTime(AttendanceProcessData[i].ManualInTime);
                        }
                        dr["IsManualOutTime"] = AttendanceProcessData[i].IsManualOutTime;
                        if (AttendanceProcessData[i].IsManualOutTime)
                        {
                            dr["ManualOutTime"] = Convert.ToDateTime(AttendanceProcessData[i].ManualOutTime);
                        }
                        dr["PlantId"] = identity.PlantId;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr["OTType"] = "OTLIMIT";
                        dr.EndEdit();
                    }
                    DvHourlyOTData.RowFilter = null;
                }

                clsStaticInfo objsave = new clsStaticInfo();
                objsave.SaveDataSets(dsManualAttanData, dsHourlyOTData);

                //if (DeleteEmpSytemId == "")
                //    objsave.SaveDataSets(dsManualAttanData, dsHourlyOTData);
                //else
                //    SaveAttendanceRawDataBackupDataSetsAndDelete(DeleteEmpSytemId, WDate, dsManualAttanData, dsHourlyOTData, dsSaveddataRef);

                DateTime fd = Convert.ToDateTime(FromDate);
                DateTime td = Convert.ToDateTime(ToDate);
                while (fd <= td)
                {
                    AttendanceLog.Log.SaveLog(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.Name + "\\" + System.Reflection.MethodBase.GetCurrentMethod().Name);
                    ReturnType r = obj.SaveTotal(identity.PlantId, fd.ToString("dd-MMM-yyyy"), EmpSytemId, false);
                    fd = fd.AddDays(1);
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





            //return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }
        private string constructKey(string EmpSystemID, string WorkDate)
        {
            return EmpSystemID + "-" + Convert.ToDateTime(WorkDate).ToString("dd-MMM-yyyy");
        }
        public void SaveDataWithOTConfirmed(List<OTLimitTransactionVM> OTLimitTransactionData, string EmpSytemId, string FromDate, string ToDate)
        {
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            bool IsOTConfirmationAuto = false;
            bool IsOTConfirmationAutoForZeroAuto = false;
            bool IsOTConfirmationAfterLock = false;

            DataSet dsFinalOT = null;
            DataRow drFinalOT = null;

            DataSet dsAttProc = null;
            DataRow drAttProc = null;


            AttendanceProcessAplos objAttdnProc;
            objAttdnProc = new AttendanceProcessAplos();

            clsAttnManualOverTime objAttdnManOT;
            objAttdnManOT = new clsAttnManualOverTime();







            DataSet dsLocalHRMSSetting = null;
            string MinimumOTMinute = string.Empty;
            string OTConsiderOn = string.Empty;
            string OTFractionCalculate = string.Empty;

            objStatic.GetPlantWiseHRMSSetting(identity.CompanyGroupId, identity.PlantId, out dsLocalHRMSSetting);
            if (dsLocalHRMSSetting.Tables[0].Rows.Count > 0)
            {
                MinimumOTMinute = dsLocalHRMSSetting.Tables[0].Rows[0]["MinimumOTMinute"].ToString().Trim();
                OTConsiderOn = dsLocalHRMSSetting.Tables[0].Rows[0]["OTConsiderOn"].ToString().Trim();
                OTFractionCalculate = dsLocalHRMSSetting.Tables[0].Rows[0]["OTFractionCalculation"].ToString().Trim();

                if (!string.IsNullOrEmpty(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAuto"].ToString().Trim()))
                {
                    IsOTConfirmationAuto = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAuto"].ToString().Trim());

                }
                if (!string.IsNullOrEmpty(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAutoForZero"].ToString().Trim()))
                {
                    IsOTConfirmationAutoForZeroAuto = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAutoForZero"].ToString().Trim());

                }
                if (!string.IsNullOrEmpty(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAfterLock"].ToString().Trim()))
                {
                    IsOTConfirmationAfterLock = Convert.ToBoolean(dsLocalHRMSSetting.Tables[0].Rows[0]["IsOTConfirmationAfterLock"].ToString().Trim());

                }

            }

            DataSet dsManualAttanData = null;
            DataSet dsHourlyOTData = null;
            //DataSet dsOTfromAppData = null;

            ConnectionManager.DAL.ConManager objCon;
            try
            {




                if (IsOTConfirmationAfterLock == false)
                {
                    clsAttendance.AttendanceProcessAplos obj = new clsAttendance.AttendanceProcessAplos();
                    obj.LockValidation(identity.PlantId, FromDate, ToDate, EmpSytemId);
                }

                Delete(EmpSytemId, FromDate, ToDate);

                string sql1 = "SELECT * FROM [dbo].[HourlyOT] WHERE EmpSystemID IN (" + EmpSytemId + ") AND WorkDate  BETWEEN '" + FromDate + @"' AND '" + ToDate + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql1, out dsHourlyOTData, false, "1");
                while (dsHourlyOTData.Tables[0].DefaultView.Count > 0)
                    dsHourlyOTData.Tables[0].DefaultView[0].Delete();

                Dictionary<string, DataRow> dicHourlyOTData = new Dictionary<string, DataRow>();
                for (int i = 0; i < dsHourlyOTData.Tables[0].Rows.Count; i++)
                {
                    string Key = constructKey(dsHourlyOTData.Tables[0].Rows[i]["EmpSystemID"].ToString(), dsHourlyOTData.Tables[0].Rows[i]["WorkDate"].ToString());
                    if (dicHourlyOTData.ContainsKey(Key) == false)
                        dicHourlyOTData.Add(Key, dsHourlyOTData.Tables[0].Rows[i]);
                }



                string sqlAttProc = "SELECT * FROM [dbo].[AttdnProcessData] WHERE EmpSystemID IN (" + EmpSytemId + ") AND WorkDate  BETWEEN '" + FromDate + @"' AND '" + ToDate + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlAttProc, out dsAttProc, false, "1");
                Dictionary<string, DataRow> dicAttProc = new Dictionary<string, DataRow>();
                for (int i = 0; i < dsAttProc.Tables[0].Rows.Count; i++)
                {
                    string Key = constructKey(dsAttProc.Tables[0].Rows[i]["EmpSystemID"].ToString(), dsAttProc.Tables[0].Rows[i]["WorkDate"].ToString());
                    if (dicAttProc.ContainsKey(Key) == false)
                        dicAttProc.Add(Key, dsAttProc.Tables[0].Rows[i]);

                    dsAttProc.Tables[0].Rows[i].BeginEdit();
                    dsAttProc.Tables[0].Rows[i]["IsOTComfirm"] = true;
                    dsAttProc.Tables[0].Rows[i].EndEdit();

                }


                string sqlFinalOT = "SELECT * FROM [dbo].[FinalOT] WHERE EmpSystemID IN (" + EmpSytemId + ") AND WorkDate  BETWEEN '" + FromDate + @"' AND '" + ToDate + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlFinalOT, out dsFinalOT, false, "1");
                while (dsFinalOT.Tables[0].DefaultView.Count > 0)
                    dsFinalOT.Tables[0].DefaultView[0].Delete();

                Dictionary<string, DataRow> dicFinalOT = new Dictionary<string, DataRow>();
                for (int i = 0; i < dsFinalOT.Tables[0].Rows.Count; i++)
                {
                    string Key = constructKey(dsFinalOT.Tables[0].Rows[i]["EmpSystemID"].ToString(), dsFinalOT.Tables[0].Rows[i]["WorkDate"].ToString());
                    if (dicFinalOT.ContainsKey(Key) == false)
                        dicFinalOT.Add(Key, dsFinalOT.Tables[0].Rows[i]);
                }

                string sID = string.Empty;
                Random rnd = new Random((int)DateTime.Now.Ticks);
                for (int i = 0; i < OTLimitTransactionData.Count; i++)
                {

                    string Key = constructKey(OTLimitTransactionData[i].EmpSystemId, OTLimitTransactionData[i].WorkDate);

                    //string JoinDT = string.Empty;
                    //string Date = Convert.ToDateTime(OTLimitTransactionData[i].WorkDate).ToString("dd-MMM-yyyy");
                    //string SOutTime = Convert.ToDateTime(OTLimitTransactionData[i].ShiftOutTime).ToString("hh:mm tt");
                    //string SInTime = Convert.ToDateTime(OTLimitTransactionData[i].ShiftInTime).ToString("hh:mm tt");


                    ////night shift
                    //if (Convert.ToDateTime(Date + " " + SInTime) > Convert.ToDateTime(Date + " " + SOutTime))
                    //{
                    //    Date = Convert.ToDateTime(OTLimitTransactionData[i].WorkDate).AddDays(1).ToString("dd-MMM-yyyy");
                    //}

                    //if (OTLimitTransactionData[i].IsExtraOTOnly == true)
                    //{
                    //    JoinDT = Date + " " + SInTime;

                    //}
                    //else
                    //{
                    //    JoinDT = Date + " " + SOutTime;

                    //}


                    //DateTime d1 = Convert.ToDateTime(JoinDT);
                    //DateTime NewOutTime = d1.AddMinutes(Convert.ToInt32(OTLimitTransactionData[i].OT));

                    //int RandomMinutes = rnd.Next(0, 0);
                    //var RandomOutTime = NewOutTime.AddMinutes(RandomMinutes);

                    //DateTime d2 = Convert.ToDateTime(RandomOutTime);
                    //DateTime ExtraOTOutTime = d2.AddMinutes(Convert.ToInt32(OTLimitTransactionData[i].ExtraOT));


                    if (OTLimitTransactionData[i].IsExtraOTOnly == false)
                    {
                        #region Final OT

                        if (dicFinalOT.ContainsKey(Key))
                        {
                            drFinalOT = dicFinalOT[Key];// dvFinalOT[0].Row;
                            drFinalOT.BeginEdit();

                            drFinalOT["OTDayType"] = OTLimitTransactionData[i].OriginalDayType;//////
                            drFinalOT["WorkDate"] = OTLimitTransactionData[i].WorkDate;
                            drFinalOT["TotalOTHr"] = OTLimitTransactionData[i].OT;
                            drFinalOT["NormalOTHr"] = OTLimitTransactionData[i].OT;
                            drFinalOT["ExtraOTHr"] = 0;

                            drFinalOT["GroupID"] = bplib.clsWebLib.RetValidLen(identity.CompanyGroupId);
                            drFinalOT["PlantID"] = bplib.clsWebLib.RetValidLen(identity.PlantId);

                            drFinalOT["UpdatedBy"] = bplib.clsWebLib.RetValidLen(identity.Name);
                            drFinalOT["DateUpdated"] = DateTime.Now;
                            drFinalOT.EndEdit();
                        }
                        else
                        {
                            drFinalOT = dsFinalOT.Tables[0].NewRow();
                            drFinalOT["AddedBy"] = bplib.clsWebLib.RetValidLen(identity.Name);
                            drFinalOT["DateAdded"] = DateTime.Now;

                            drFinalOT["OTDayType"] = OTLimitTransactionData[i].OriginalDayType;////////////////
                            drFinalOT["EmpSystemID"] = OTLimitTransactionData[i].EmpSystemId;
                            drFinalOT["WorkDate"] = OTLimitTransactionData[i].WorkDate;
                            drFinalOT["TotalOTHr"] = OTLimitTransactionData[i].OT;
                            drFinalOT["NormalOTHr"] = OTLimitTransactionData[i].OT;
                            drFinalOT["ExtraOTHr"] = 0;

                            drFinalOT["GroupID"] = bplib.clsWebLib.RetValidLen(identity.CompanyGroupId);
                            drFinalOT["PlantID"] = bplib.clsWebLib.RetValidLen(identity.PlantId);

                            drFinalOT["UpdatedBy"] = bplib.clsWebLib.RetValidLen(identity.Name);
                            drFinalOT["DateUpdated"] = DateTime.Now;
                            dsFinalOT.Tables[0].Rows.Add(drFinalOT);
                        }


                        #endregion


                        #region Attdn Proc
                        if (dicAttProc.ContainsKey(Key))
                        {
                            drAttProc = dicAttProc[Key];// dvAttProc[0].Row;
                            drAttProc.BeginEdit();

                            if (string.IsNullOrEmpty(OTLimitTransactionData[i].NewOutTime) == false)
                            {
                                drAttProc["OutTime"] = bplib.clsWebLib.RetValidLen(OTLimitTransactionData[i].NewOutTime);
                                drAttProc["IsManualOutTime"] = true;
                                drAttProc["ManualOutTime"] = bplib.clsWebLib.RetValidLen(OTLimitTransactionData[i].NewOutTime);
                            }
                            drAttProc["OTHr"] = Convert.ToDecimal(OTLimitTransactionData[i].OT);

                            drAttProc["IsOTComfirm"] = true;

                            drAttProc["OTComfirmBy"] = bplib.clsWebLib.RetValidLen(identity.Name);
                            drAttProc["DateOTComfirm"] = DateTime.Now;

                            //drAttProc["OriginalManualInTime"] = bplib.clsWebLib.RetValidLen(OTLimitTransactionData[i].ManualInTime);
                            //drAttProc["OriginalManualOutTime"] = bplib.clsWebLib.RetValidLen(OTLimitTransactionData[i].ManualOutTime);

                            drAttProc.EndEdit();
                        }


                        #endregion
                    }


                    #region Extra OT                   


                    if (dicHourlyOTData.ContainsKey(Key) == false)
                    {
                        if (string.IsNullOrEmpty(sID))
                        {
                            bplib.clsGenID objGenID = new bplib.clsGenID();
                            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "HourlyOT", out sID);
                            sID = "OX" + sID;
                        }

                        DataRow dr = dsHourlyOTData.Tables[0].NewRow();
                        dr["Id"] = sID + "-" + (i + 1).ToString();
                        dr["EmpSystemId"] = OTLimitTransactionData[i].EmpSystemId;
                        //dr["FromDate"] = AttendanceProcessData[i].ExtraOTInTime;
                        dr["FromDate"] = OTLimitTransactionData[i].InTime;
                        dr["ToDate"] = bplib.clsWebLib.RetValidLen(OTLimitTransactionData[i].OutTime);
                        dr["Duration"] = OTLimitTransactionData[i].ExtraOT;
                        dr["WorkDate"] = Convert.ToDateTime(OTLimitTransactionData[i].WorkDate);

                        dr["IsManualInTime"] = OTLimitTransactionData[i].IsManualInTime;
                        if (OTLimitTransactionData[i].IsManualInTime)
                        {
                            dr["ManualInTime"] = bplib.clsWebLib.RetValidLen(clsStaticInfo.GetDateTime(OTLimitTransactionData[i].ManualInTime));
                        }
                        dr["IsManualOutTime"] = OTLimitTransactionData[i].IsManualOutTime;
                        if (OTLimitTransactionData[i].IsManualOutTime)
                        {
                            dr["ManualOutTime"] = bplib.clsWebLib.RetValidLen(clsStaticInfo.GetDateTime(OTLimitTransactionData[i].ManualOutTime));
                        }


                        dr["PlantId"] = identity.PlantId;
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr["OTType"] = "OTLIMIT";
                        dsHourlyOTData.Tables[0].Rows.Add(dr);

                    }
                    else
                    {

                        DataRow dr = dicHourlyOTData[Key];// DvHourlyOTData[0].Row;
                        dr.BeginEdit();
                        dr["EmpSystemId"] = OTLimitTransactionData[i].EmpSystemId;
                        //dr["FromDate"] = AttendanceProcessData[i].ExtraOTInTime;
                        //dr["FromDate"] = RandomOutTime;
                        //dr["ToDate"] = ExtraOTOutTime;
                        dr["FromDate"] = OTLimitTransactionData[i].InTime;
                        dr["ToDate"] = bplib.clsWebLib.RetValidLen(OTLimitTransactionData[i].OutTime);
                        dr["Duration"] = OTLimitTransactionData[i].ExtraOT;
                        dr["WorkDate"] = Convert.ToDateTime(OTLimitTransactionData[i].WorkDate);
                        dr["IsManualInTime"] = OTLimitTransactionData[i].IsManualInTime;
                        if (OTLimitTransactionData[i].IsManualInTime)
                        {
                            dr["ManualInTime"] = bplib.clsWebLib.RetValidLen(clsStaticInfo.GetDateTime(OTLimitTransactionData[i].ManualInTime));
                        }
                        dr["IsManualOutTime"] = OTLimitTransactionData[i].IsManualOutTime;
                        if (OTLimitTransactionData[i].IsManualOutTime)
                        {
                            dr["ManualOutTime"] = bplib.clsWebLib.RetValidLen(clsStaticInfo.GetDateTime(OTLimitTransactionData[i].ManualOutTime));
                        }
                        dr["PlantId"] = identity.PlantId;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr["OTType"] = "OTLIMIT";
                        dr.EndEdit();

                    }

                    #endregion
                }

                clsStaticInfo objsave = new clsStaticInfo();
                objsave.SaveDataSets(dsHourlyOTData, dsFinalOT, dsAttProc);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }





            //return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }



        [HttpPost, Authorize]
        public ActionResult GetDetailsData(string YearNo, string MonthNo, string OTLimitSettingId, string[] EmpSystemIds)
        {
            DataSet dsOTLimitSetting;
            string OTLimit = string.Empty;
            string FromDate = string.Empty;
            string ToDate = string.Empty;
            string EmpSytemId = string.Empty;
            decimal MinOTLimitParDay = 0;
            decimal MaxOTLimitParDay = 0;
            decimal MaxOTLimitParWeek = 0;
            decimal OTreductionFactor = 0;

            decimal MaxWeekOffOTLimitParDay = 0;
            decimal MaxHolidayOTLimitParDay = 0;
            List<OTLimitTransactionVM> oOTLimitTransaction = new List<OTLimitTransactionVM>();



            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;

            foreach (var item in EmpSystemIds)
            {
                if (EmpSytemId == "")
                    EmpSytemId = "'" + item.ToString() + "'";
                else
                    EmpSytemId = EmpSytemId + ",'" + item.ToString() + "'";
            }








            string sql2 = "SELECT * FROM OTLimitSetting WHERE Id='" + OTLimitSettingId + "'";
            objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenDataSetThroughAdapter(sql2, out dsOTLimitSetting, false, "1");
            if (dsOTLimitSetting.Tables[0].Rows.Count > 0)
            {
                OTLimit = dsOTLimitSetting.Tables[0].Rows[0]["MinOTLimitParDay"].ToString();
                //FromDate = dsOTLimitSetting.Tables[0].Rows[0]["FromDay"].ToString() + "-" + MonthNo + "-" + YearNo;
                //ToDate = dsOTLimitSetting.Tables[0].Rows[0]["ToDay"].ToString() + "-" + MonthNo + "-" + YearNo;
                string week = dsOTLimitSetting.Tables[0].Rows[0]["Week"].ToString();
                if (week == "First Week")
                {
                    FromDate = "01-" + MonthNo + "-" + YearNo;
                    ToDate = "07-" + MonthNo + "-" + YearNo;
                }

                if (week == "Second Week")
                {
                    FromDate = "08-" + MonthNo + "-" + YearNo;
                    ToDate = "14-" + MonthNo + "-" + YearNo;
                }
                if (week == "Third Week")
                {
                    FromDate = "15-" + MonthNo + "-" + YearNo;
                    ToDate = "21-" + MonthNo + "-" + YearNo;
                }
                if (week == "Last Week")
                {
                    FromDate = "22-" + MonthNo + "-" + YearNo;
                    ToDate = Convert.ToDateTime("01-" + MonthNo + "-" + YearNo).AddMonths(1).AddDays(-1).ToString("dd-MMMM-yyyy");
                }
                MinOTLimitParDay = Convert.ToDecimal(dsOTLimitSetting.Tables[0].Rows[0]["MinOTLimitParDay"].ToString());
                MaxOTLimitParDay = Convert.ToDecimal(dsOTLimitSetting.Tables[0].Rows[0]["MaxOTLimitParDay"].ToString());
                MaxOTLimitParWeek = Convert.ToDecimal(dsOTLimitSetting.Tables[0].Rows[0]["MaxOTLimitParWeek"].ToString());
                OTreductionFactor = Convert.ToDecimal(dsOTLimitSetting.Tables[0].Rows[0]["OTreductionFactor"].ToString());
                MaxWeekOffOTLimitParDay = Convert.ToDecimal(dsOTLimitSetting.Tables[0].Rows[0]["MaxWeekOffOTLimitParDay"].ToString());
                MaxHolidayOTLimitParDay = Convert.ToDecimal(dsOTLimitSetting.Tables[0].Rows[0]["MaxHolidayOTLimitParDay"].ToString());

            }


            DataSet dsOTDetails = null;
            GetOTData(FromDate, ToDate, EmpSytemId, out dsOTDetails);




            if (EmpSystemIds.Length > 0)
            {
                foreach (var item in EmpSystemIds)
                {

                    DataView dv = new DataView(dsOTDetails.Tables[0]);
                    dv.RowFilter = "EmpSystemID='" + item + "'";
                    //dv.Count
                    if (dv.Count > 0)
                    {

                        decimal TotalWeeklyOT = 0;
                        for (int i = 0; i < dv.Count; i++)
                        {
                            OTLimitTransactionVM o = new OTLimitTransactionVM();
                            //string EmpSystemID = dv[i]["EmpSystemID"].ToString();
                            decimal TotalOTHr = Convert.ToDecimal(dv[i]["TotalOTHr"].ToString());
                            //string WorkDate = dv[i]["WorkDate"].ToString();
                            //string ShiftName = dv[i]["ShiftName"].ToString();
                            //string ShiftInTime = dv[i]["ShiftInTime"].ToString();
                            //string ShiftOutTime = dv[i]["ShiftOutTime"].ToString();
                            //string InTime = dv[i]["InTime"].ToString();
                            //string OutTime = dv[i]["OutTime"].ToString();
                            //string DayStatus = dv[i]["DayStatus"].ToString();
                            decimal ExtraOT = 0;
                            decimal NewOT = 0;





                            decimal DailyOT = Convert.ToDecimal(dv[i]["TotalOTHr"]) * OTreductionFactor;
                            string OriginalDayType = dv[i]["OriginalDayType"].ToString();
                            bool IsExceptionOT = Convert.ToBoolean(dv[i]["IsExceptionOT"]);


                            //if (dv[i]["OriginalDayType"].ToString().ToUpper() == "NW")
                            //if (1 == 1)
                            //{
                            //    o.IsExtraOTOnly = false;
                            //    if (DailyOT > 0)
                            //    {
                            //        if (DailyOT <= FirstSlabMin)// Daily OT < slab value
                            //        {
                            //            if (DailyOT >= MinOTLimitParDay)
                            //            {
                            //                ExtraOTCalculation(DailyOT, MaxOTLimitParWeek, MaxOTLimitParDay, ref TotalWeeklyOT, out NewOT, out ExtraOT);

                            //            }
                            //            else
                            //            {
                            //                NewOT = 0;
                            //                //ExtraOT = DailyOT;
                            //                ExtraOT = 0;
                            //            }
                            //        }
                            //        else // Daily OT > slab value
                            //        {
                            //            if (FirstSlabMin > 0)
                            //            {
                            //                decimal ExtraOTSlab = DailyOT - FirstSlabMin;
                            //                if (FirstSlabMin >= MinOTLimitParDay)
                            //                {
                            //                    ExtraOTCalculation(FirstSlabMin, MaxOTLimitParWeek, MaxOTLimitParDay, ref TotalWeeklyOT, out NewOT, out ExtraOT);
                            //                    ExtraOT = ExtraOT + ExtraOTSlab;

                            //                }
                            //                else
                            //                {
                            //                    NewOT = 0;
                            //                    //ExtraOT = DailyOT;
                            //                    ExtraOT = 0;
                            //                }

                            //            }
                            //            else
                            //            {
                            //                NewOT = 0;
                            //                ExtraOT = DailyOT;
                            //                o.IsExtraOTOnly = true;
                            //            }
                            //        }




                            //    }
                            //    else// all r 0
                            //    {
                            //        NewOT = 0;
                            //        ExtraOT = 0;
                            //    }


                            //}
                            //else//W and H
                            //{
                            //    NewOT = 0;
                            //    ExtraOT = DailyOT;
                            //    o.IsExtraOTOnly = true;
                            //}
                            if (IsExceptionOT)
                            {
                                if (DailyOT > 0)
                                {


                                    if (DailyOT >= MinOTLimitParDay)
                                    {
                                        ExtraOTCalculation(DailyOT, MaxOTLimitParWeek, DailyOT, ref TotalWeeklyOT, out NewOT, out ExtraOT);

                                    }
                                    else
                                    {
                                        NewOT = 0;
                                        //ExtraOT = DailyOT;
                                        ExtraOT = 0;
                                    }


                                }
                                else// all r 0
                                {
                                    NewOT = 0;
                                    ExtraOT = 0;
                                }
                                o.IsExtraOTOnly = false;
                            }
                            else /////////Regular
                            {

                                if (dv[i]["OriginalDayType"].ToString().ToUpper() == "NW")
                                {
                                    if (DailyOT > 0)
                                    {


                                        if (DailyOT >= MinOTLimitParDay)
                                        {
                                            ExtraOTCalculation(DailyOT, MaxOTLimitParWeek, MaxOTLimitParDay, ref TotalWeeklyOT, out NewOT, out ExtraOT);

                                        }
                                        else
                                        {
                                            NewOT = 0;
                                            //ExtraOT = DailyOT;
                                            ExtraOT = 0;
                                        }


                                    }
                                    else// all r 0
                                    {
                                        NewOT = 0;
                                        ExtraOT = 0;
                                    }
                                    o.IsExtraOTOnly = false;

                                }

                                else if (dv[i]["OriginalDayType"].ToString().ToUpper() == "W")
                                {
                                    if (DailyOT > 0)
                                    {


                                        if (DailyOT >= MinOTLimitParDay)
                                        {
                                            ExtraOTCalculation(DailyOT, MaxOTLimitParWeek, MaxWeekOffOTLimitParDay, ref TotalWeeklyOT, out NewOT, out ExtraOT);

                                        }
                                        else
                                        {
                                            NewOT = 0;
                                            //ExtraOT = DailyOT;
                                            ExtraOT = 0;
                                        }


                                    }
                                    else// all r 0
                                    {
                                        NewOT = 0;
                                        ExtraOT = 0;
                                    }
                                    o.IsExtraOTOnly = false;
                                    if (MaxWeekOffOTLimitParDay == 0)
                                    {
                                        o.IsExtraOTOnly = true;
                                    }
                                }
                                else if (dv[i]["OriginalDayType"].ToString().ToUpper() == "H")
                                {
                                    if (DailyOT > 0)
                                    {


                                        if (DailyOT >= MinOTLimitParDay)
                                        {
                                            ExtraOTCalculation(DailyOT, MaxOTLimitParWeek, MaxHolidayOTLimitParDay, ref TotalWeeklyOT, out NewOT, out ExtraOT);

                                        }
                                        else
                                        {
                                            NewOT = 0;
                                            //ExtraOT = DailyOT;
                                            ExtraOT = 0;
                                        }


                                    }
                                    else// all r 0
                                    {
                                        NewOT = 0;
                                        ExtraOT = 0;
                                    }
                                    o.IsExtraOTOnly = false;
                                    if (MaxHolidayOTLimitParDay == 0)
                                    {
                                        o.IsExtraOTOnly = true;
                                    }
                                }


                                else//W and H
                                {





                                    NewOT = 0;
                                    ExtraOT = DailyOT;
                                    o.IsExtraOTOnly = true;
                                }
                            }


                            //o.FirstSlabMin = FirstSlabMin;
                            o.EmpSystemId = dv[i]["EmpSystemID"].ToString();
                            o.WorkDate = dv[i]["WorkDate"].ToString();
                            o.ShiftInTime = dv[i]["ShiftInTime"].ToString();
                            o.ShiftOutTime = dv[i]["ShiftOutTime"].ToString();
                            o.InTime = dv[i]["InTime"].ToString();
                            o.OutTime = dv[i]["OutTime"].ToString();
                            o.DayStatus = dv[i]["DayStatus"].ToString();
                            o.OriginalDayType = dv[i]["OriginalDayType"].ToString();
                            o.ShiftName = dv[i]["ShiftName"].ToString();


                            o.IsManualInTime = Convert.ToBoolean(dv[i]["IsManualInTime"].ToString());
                            o.ManualInTime = dv[i]["ManualInTime"].ToString();
                            o.IsManualOutTime = Convert.ToBoolean(dv[i]["IsManualOutTime"].ToString());
                            o.ManualOutTime = dv[i]["ManualOutTime"].ToString();


                            o.TotalOT = DailyOT;
                            o.OT = NewOT;
                            o.ExtraOT = ExtraOT;
                            oOTLimitTransaction.Add(o);





                        }
                    }
                    dv.RowFilter = null;

                }
            }


            return Json(new { oOTLimitTransaction, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }


        #region MyRegion

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
                            LEFT JOIN OTSlabDefineGeneral pl ON pl.DayType = CASE WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')!='' THEN edwsa.DayType
																			WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')='' THEN odd.OffDayType
																			ELSE edwsa.DayType END                       
															AND apd.WorkDate BETWEEN pl.FromDate AND pl.ToDate AND pl.PlantID=apd.PlantID

                            WHERE apd.WorkDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"' AND apd.EmpSystemID='" + EmpSystemId + @"' AND  apd.IsOTEntitled=1                             
                            AND apd.PlantID='" + identity.PlantId + @"' AND ISNULL(apd.OTHr,0)/60 > pl.firstSlab ORDER BY CONVERT(DATE,apd.WorkDate)";
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





                string sql = "SELECT * FROM [dbo].[AttdnManualData] WHERE EmpSystemID IN (" + EmpSytemId + ") AND WorkDate BETWEEN '" + pFromDate + @"' AND '" + pToDate + @"' AND PlantID='" + identity.PlantId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsManualAttanData, false, "1");

                string sql1 = "SELECT * FROM [dbo].[HourlyOT] WHERE EmpSystemID IN (" + EmpSytemId + ") AND WorkDate BETWEEN '" + pFromDate + @"' AND '" + pToDate + @"'  AND PlantID='" + identity.PlantId + @"'";
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






                strSQL1 = @"SELECT * FROM AttdnRawData WHERE LogDownLoadNum =" + EmpSytemId + " AND PDate BETWEEN '" + pFromDate + @"' AND '" + pToDate + @"'  AND PlantID='" + identity.PlantId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL1, out dsGetdataRef, false, "1");




                strSQL2 = @"SELECT * FROM AttdnRawDataBackUp WHERE LogDownLoadNum =" + EmpSytemId + " AND PDate BETWEEN '" + pFromDate + @"' AND '" + pToDate + @"'  AND PlantID='" + identity.PlantId + @"'";
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
                                dvSaveSummary.RowFilter = " Id ='" + dsGetdataRef.Tables[0].Rows[j]["Id"] + "' AND PlantID = '" + identity.PlantId + @"' AND PDate = '" + AttendanceProcessData[i].WorkDate + @"'";
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

                        DvMaster.RowFilter = "EmpSystemID='" + AttendanceProcessData[i].SystemId + @"' AND WorkDate='" + AttendanceProcessData[i].WorkDate + @"' AND PlantID='" + identity.PlantId + @"'";
                        if (DvMaster.Count == 0)
                        {

                            DataRow dr = dsManualAttanData.Tables[0].NewRow();
                            dr["EmpSystemID"] = AttendanceProcessData[i].SystemId;
                            dr["WorkDate"] = Convert.ToDateTime(AttendanceProcessData[i].WorkDate);
                            dr["GroupID"] = identity.CompanyGroupId;
                            //dr["PlantID"] = identity.PlantId;
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
                            //dr["OutTime"] = AttendanceProcessData[i].NewOutTime;
                            dr["OutTime"] = Convert.ToDateTime(RandomOutTime);
                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = System.DateTime.Now.ToString();
                            dr.EndEdit();

                        }
                    }
                    DvMaster.RowFilter = null;

                    DvHourlyOTData.RowFilter = "EmpSystemID='" + AttendanceProcessData[i].SystemId + "' AND WorkDate='" + AttendanceProcessData[i].WorkDate + @"' AND PlantID='" + identity.PlantId + @"'";
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





            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
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
        [HttpPost, Authorize]
        public ActionResult SaveAttendanceProcessDataDateWise(List<AttendanceProcessDataVM> AttendanceProcessData, string WDate)
        {
            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            string EmpSytemId = "";
            string DeleteEmpSytemId = "";
            DataSet dsManualAttanData = null;
            DataSet dsHourlyOTData = null;

            ConnectionManager.DAL.ConManager objCon;
            try
            {

                for (int i = 0; i < AttendanceProcessData.Count; i++)
                {
                    if (EmpSytemId == "")
                        EmpSytemId = "'" + AttendanceProcessData[i].SystemId.ToString() + "'";
                    else
                        EmpSytemId = EmpSytemId + ",'" + AttendanceProcessData[i].SystemId.ToString() + "'";
                }





                clsAttendance.AttendanceProcessAplos obj = new clsAttendance.AttendanceProcessAplos();
                DateTime ToDate = Convert.ToDateTime(WDate);
                obj.LockValidation(identity.PlantId, ToDate.ToString("dd-MMM-yyyy"), ToDate.ToString("dd-MMM-yyyy"), EmpSytemId);


                string sql = "SELECT * FROM [dbo].[AttdnManualData] WHERE EmpSystemID IN (" + EmpSytemId + ") AND WorkDate ='" + WDate + @"'  AND PlantID='" + identity.PlantId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsManualAttanData, false, "1");

                string sql1 = "SELECT * FROM [dbo].[HourlyOT] WHERE EmpSystemID IN (" + EmpSytemId + ") AND WorkDate ='" + WDate + @"'  AND PlantID='" + identity.PlantId + @"'";
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






                strSQL1 = @"SELECT * FROM AttdnRawData WHERE LogDownLoadNum IN (" + EmpSytemId + ") AND PlantID='" + identity.PlantId + @"' AND PDate='" + WDate + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL1, out dsGetdataRef, false, "1");




                strSQL2 = @"SELECT * FROM AttdnRawDataBackUp WHERE LogDownLoadNum IN (" + EmpSytemId + ") AND PlantID='" + identity.PlantId + @"' AND  PDate='" + WDate + @"'";
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



                        if (DeleteEmpSytemId == "")
                            DeleteEmpSytemId = "'" + AttendanceProcessData[i].SystemId.ToString() + "'";
                        else
                            DeleteEmpSytemId = DeleteEmpSytemId + ",'" + AttendanceProcessData[i].SystemId.ToString() + "'";

                        DataView dvSaveSummary = new DataView(dsSaveddataRef.Tables[0]);
                        for (int j = 0; j < dsGetdataRef.Tables[0].Rows.Count; j++)
                        {
                            dvSaveSummary.RowFilter = " Id ='" + dsGetdataRef.Tables[0].Rows[j]["Id"] + "' AND PlantID = '" + identity.PlantId + @"' AND PDate = '" + WDate + @"'";
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
                            //Old year insert 
                        }
                        //SaveAttendanceRawDataBackupDataSetsAndDelete(AttendanceRawDataId, dsSaveddataRef);

                    }
                    else
                    {  //Manual Attendance 
                        DvMaster.RowFilter = "EmpSystemID='" + AttendanceProcessData[i].SystemId + "' AND PlantID='" + identity.PlantId + @"'";
                        if (DvMaster.Count == 0)
                        {

                            DataRow dr = dsManualAttanData.Tables[0].NewRow();
                            dr["EmpSystemID"] = AttendanceProcessData[i].SystemId;
                            dr["WorkDate"] = Convert.ToDateTime(WDate);
                            dr["GroupID"] = identity.CompanyGroupId;
                            //dr["PlantID"] = identity.PlantId;
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
                            //dr["OutTime"] = AttendanceProcessData[i].NewOutTime;
                            dr["OutTime"] = Convert.ToDateTime(RandomOutTime);
                            dr["UpdatedBy"] = identity.Name;
                            dr["DateUpdated"] = System.DateTime.Now.ToString();
                            dr.EndEdit();

                        }
                        DvMaster.RowFilter = null;
                    }


                    DvHourlyOTData.RowFilter = "EmpSystemID='" + AttendanceProcessData[i].SystemId + "' AND PlantID='" + identity.PlantId + @"'";
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
                        dr["WorkDate"] = Convert.ToDateTime(WDate);
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
                        dr["WorkDate"] = Convert.ToDateTime(WDate);
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


                if (DeleteEmpSytemId == "")
                    objsave.SaveDataSets(dsManualAttanData, dsHourlyOTData);
                else
                    SaveAttendanceRawDataBackupDataSetsAndDelete(DeleteEmpSytemId, WDate, dsManualAttanData, dsHourlyOTData, dsSaveddataRef);
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


        //[HttpPost]
        //public ActionResult xSaveAttendanceProcessDataDateWise(List<AttendanceProcessDataVM> AttendanceProcessData, string WDate)
        //{
        //    clsStaticInfo objStatic = null;
        //    objStatic = new clsStaticInfo();
        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

        //    string EmpSytemId = "";
        //    DataSet dsManualAttanData = null;
        //    DataSet dsHourlyOTData = null;

        //    ConnectionManager.DAL.ConManager objCon;
        //    try
        //    {

        //        for (int i = 0; i < AttendanceProcessData.Count; i++)
        //        {
        //            if (EmpSytemId == "")
        //                EmpSytemId = "'" + AttendanceProcessData[i].SystemId.ToString() + "'";
        //            else
        //                EmpSytemId = EmpSytemId + ",'" + AttendanceProcessData[i].SystemId.ToString() + "'";
        //        }





        //        clsAttendance.AttendanceProcessAplos obj = new clsAttendance.AttendanceProcessAplos();
        //        DateTime ToDate = Convert.ToDateTime(WDate);
        //        obj.LockValidation(identity.PlantId, ToDate.ToString("dd-MMM-yyyy"), ToDate.ToString("dd-MMM-yyyy"), EmpSytemId);


        //        string sql = "SELECT * FROM [dbo].[AttdnManualData] WHERE EmpSystemID IN (" + EmpSytemId + ") AND WorkDate ='" + WDate + @"'  AND PlantID='" + identity.PlantId + @"'";
        //        objCon = new ConnectionManager.DAL.ConManager("1");
        //        objCon.OpenDataSetThroughAdapter(sql, out dsManualAttanData, false, "1");

        //        string sql1 = "SELECT * FROM [dbo].[HourlyOT] WHERE EmpSystemID IN (" + EmpSytemId + ") AND WorkDate ='" + WDate + @"'  AND PlantID='" + identity.PlantId + @"'";
        //        objCon = new ConnectionManager.DAL.ConManager("1");
        //        objCon.OpenDataSetThroughAdapter(sql1, out dsHourlyOTData, false, "1");

        //        DataView DvMaster = new DataView(dsManualAttanData.Tables[0]);
        //        DataView DvHourlyOTData = new DataView(dsHourlyOTData.Tables[0]);
        //        for (int i = 0; i < AttendanceProcessData.Count; i++)
        //        {


        //            DvMaster.RowFilter = "EmpSystemID='" + AttendanceProcessData[i].SystemId + "' AND PlantID='" + identity.PlantId + @"'";
        //            if (DvMaster.Count == 0)
        //            {

        //                DataRow dr = dsManualAttanData.Tables[0].NewRow();
        //                dr["EmpSystemID"] = AttendanceProcessData[i].SystemId;
        //                dr["WorkDate"] = Convert.ToDateTime(WDate);
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

        //            DvHourlyOTData.RowFilter = "EmpSystemID='" + AttendanceProcessData[i].SystemId + "' AND PlantID='" + identity.PlantId + @"'";
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
        //                dr["WorkDate"] = Convert.ToDateTime(WDate);
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
        //                dr["WorkDate"] = Convert.ToDateTime(WDate);
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
        //        ReturnType r = obj.SaveTotal(identity.PlantId, ToDate.ToString("dd-MMM-yyyy"), EmpSytemId, false);//laila                 


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

        [HttpGet, Authorize]
        public ActionResult GetAttendanceProcessDataDateWise(string WDate)
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
	                            	                          
	                            ,NewOutTimeShow= CASE WHEN ISNULL(apd.OTHr,0)/60 > pl.firstSlab THEN FORMAT(DATEADD(minute, -1* (ISNULL(apd.OTHr,0)-pl.firstSlab*60),apd.OutTime), 'hh:mm tt') ELSE null END 
	                            ,ExtraOTInTimeShow= CASE WHEN ISNULL(apd.OTHr,0)/60 > pl.firstSlab THEN FORMAT( DATEADD(minute, -1* (ISNULL(apd.OTHr,0)-pl.firstSlab*60),apd.OutTime), 'hh:mm tt') ELSE null END	 
	                            ,ExtraOTOutTimeShow=FORMAT(apd.OutTime, 'hh:mm tt')
                                ,ExtraOTOutTime=apd.OutTime
	                            ,Duration= CASE WHEN ISNULL(apd.OTHr,0)/60 > pl.firstSlab THEN ISNULL(apd.OTHr,0)-pl.firstSlab*60 ELSE 0 END 
                                ,FirstSlabMin= CASE WHEN ISNULL(apd.OTHr,0)/60 > pl.firstSlab THEN  Isnull(pl.firstSlab,0)*60 ELSE 0 END
                                ,IsManualInTime= CASE WHEN ISNULL(apd.IsManualOutTime,0)=1 THEN  'YES' ELSE 'NO' END  
                            FROM AttdnProcessData AS apd
                            INNER JOIN EmployeeInformation e ON e.SystemId = apd.EmpSystemID
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
                             WHERE odm.OffDayType='H' AND d.PlantId='" + identity.PlantId + @"' AND d.OffDayDate='" + WDate + @"'
                            ) AS odd ON odd.PlantId=apd.PlantID AND odd.OffDayDate='" + WDate + @"'
                            LEFT JOIN [MST].[ExceptionForHolidayEmpList] AS efhel ON efhel.EmpSystemId =apd.EmpSystemID AND efhel.WorkDate =apd.WorkDate
                            LEFT JOIN OTSlabDefineGeneral pl ON pl.DayType = CASE WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')!='' THEN edwsa.DayType
																			WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')='' THEN odd.OffDayType
																			ELSE edwsa.DayType END                       
															AND apd.WorkDate BETWEEN pl.FromDate AND pl.ToDate AND pl.PlantID=apd.PlantID
                            WHERE apd.WorkDate='" + WDate + @"' AND  apd.IsOTEntitled=1                            
                            AND apd.PlantID='" + identity.PlantId + @"' AND ISNULL(apd.OTHr,0)/60 > pl.firstSlab";




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
                            LEFT JOIN OTSlabDefineGeneral pl ON pl.DayType = CASE WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')!='' THEN edwsa.DayType
																			WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')='' THEN odd.OffDayType
																			ELSE edwsa.DayType END                       
															AND apd.WorkDate BETWEEN pl.FromDate AND pl.ToDate AND pl.PlantID=apd.PlantID
                            WHERE apd.WorkDate BETWEEN '" + FromDate + @"' AND '" + ToDate + @"' 
                            AND  apd.IsOTEntitled=1 
                            AND apd.PlantID='" + identity.PlantId + @"' AND ISNULL(apd.OTHr,0)/60 > pl.firstSlab
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
                                ,OT= CASE WHEN ISNULL(apd.OTHr,0)/60 > " + NWDayType + @" AND  CASE WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')!='' THEN edwsa.DayType
											WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')='' THEN odd.OffDayType
											ELSE edwsa.DayType END='NW'	THEN  " + NWDayType + @" 
										  WHEN ISNULL(apd.OTHr,0)/60 > " + WDayType + @" AND  CASE WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')!='' THEN edwsa.DayType
											WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')='' THEN odd.OffDayType
											ELSE edwsa.DayType END='W'	THEN  " + WDayType + @" 
										  WHEN ISNULL(apd.OTHr,0)/60 > " + HDayType + @" AND  CASE WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')!='' THEN edwsa.DayType
											WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')='' THEN odd.OffDayType
											ELSE edwsa.DayType END='H'	THEN  " + HDayType + @" ELSE ISNULL(apd.OTHr,0)/60 END
												
	                            ,ExtraOT= CASE WHEN ISNULL(apd.OTHr,0)/60 > " + NWDayType + @" AND  CASE WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')!='' THEN edwsa.DayType
											WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')='' THEN odd.OffDayType
											ELSE edwsa.DayType END='NW'	THEN  ISNULL(apd.OTHr,0)/60-" + NWDayType + @" 
										  WHEN ISNULL(apd.OTHr,0)/60 > " + WDayType + @" AND  CASE WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')!='' THEN edwsa.DayType
											WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')='' THEN odd.OffDayType
											ELSE edwsa.DayType END='W'	THEN  ISNULL(apd.OTHr,0)/60-" + WDayType + @" 
										  WHEN ISNULL(apd.OTHr,0)/60 > " + HDayType + @" AND  CASE WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')!='' THEN edwsa.DayType
											WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')='' THEN odd.OffDayType
											ELSE edwsa.DayType END='H'	THEN ISNULL(apd.OTHr,0)/60-" + HDayType + @" ELSE 0 END
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
                                ,FirstSlabMin= CASE WHEN ISNULL(apd.OTHr,0)/60 > " + NWDayType + @" AND  CASE WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')!='' THEN edwsa.DayType
											WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')='' THEN odd.OffDayType
											ELSE edwsa.DayType END='NW'	THEN  60*" + NWDayType + @" 
										  WHEN ISNULL(apd.OTHr,0)/60 > " + WDayType + @" AND  CASE WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')!='' THEN edwsa.DayType
											WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')='' THEN odd.OffDayType
											ELSE edwsa.DayType END='W'	THEN  60*" + WDayType + @" 
										  WHEN ISNULL(apd.OTHr,0)/60 > " + HDayType + @" AND  CASE WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')!='' THEN edwsa.DayType
											WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')='' THEN odd.OffDayType
											ELSE edwsa.DayType END='H'	THEN  60*" + HDayType + @" ELSE 0 END




                                ,IsManualInTime= CASE WHEN ISNULL(apd.IsManualOutTime,0)=1 THEN  'YES' ELSE 'NO' END  
                            FROM AttdnProcessData AS apd
                            INNER JOIN EmployeeInformation e ON e.SystemId = apd.EmpSystemID
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
                            LEFT JOIN OTSlabDefineGeneral pl ON pl.DayType = CASE WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')!='' THEN edwsa.DayType
																			WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')='' THEN odd.OffDayType
																			ELSE edwsa.DayType END 
                                                                            AND apd.WorkDate BETWEEN pl.FromDate AND pl.ToDate AND pl.PlantID=apd.PlantID 
                            WHERE apd.WorkDate='" + WDate + @"' AND  apd.IsOTEntitled=1 
                         
                            AND apd.PlantID='" + identity.PlantId + @"' AND(
                             (ISNULL(apd.OTHr,0)/60 > " + NWDayType + @" AND  CASE WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')!='' THEN edwsa.DayType
											WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')='' THEN odd.OffDayType
											ELSE edwsa.DayType END='NW') OR
                             (ISNULL(apd.OTHr,0)/60 > " + HDayType + @" AND  CASE WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')!='' THEN edwsa.DayType
											WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')='' THEN odd.OffDayType
											ELSE edwsa.DayType END='H') OR  
                             (ISNULL(apd.OTHr,0)/60 > " + WDayType + @" AND  CASE WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')!='' THEN edwsa.DayType
											WHEN ISNULL(odd.OffDayType ,'')='H' AND ISNULL( efhel.EmpSystemId,'')='' THEN odd.OffDayType
											ELSE edwsa.DayType END='W') 
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
        //[HttpGet]
        //public ActionResult xGetAttendanceProcessUserDefine(string WDate, string NWDayType, string HDayType, string WDayType)
        //{

        //    var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
        //    string sql = @"SELECT [CheckBoxSelect] = Convert(BIT, 'False')
        //                     ,FORMAT(apd.WorkDate, 'dd-MMM-yyyy') WorkDate
        //                     ,sd.UserName ShiftName
        //                     ,FORMAT(sd.InTime, 'hh:mm tt') ShiftInTime
        //                     ,FORMAT(sd.OutTime, 'hh:mm tt') ShiftOutTime  
        //                     ,FORMAT(apd.InTime, 'hh:mm tt') InTime
        //                     ,FORMAT(apd.OutTime, 'hh:mm tt') OutTime
        //                     ,apd.DayStatus
        //                     ,apd.OTHr
        //                     ,edwsa.DayType  Category
        //                     ,pl.IsOTExtentNextSlab
        //                     ,pl.firstSlab
        //                     ,pl.IsTotalWorkTimeAsOT
        //                     ,TotalOT=  ISNULL(apd.OTHr,0)/60
        //                     --,OT= CASE WHEN ISNULL(apd.OTHr,0)/60 > pl.firstSlab THEN  pl.firstSlab ELSE ISNULL(apd.OTHr,0)/60 END		
        //                     --,ExtraOT= CASE WHEN ISNULL(apd.OTHr,0)/60 > pl.firstSlab THEN  ISNULL(apd.OTHr,0)/60-pl.firstSlab ELSE 0 END
        //                        ,OT= CASE WHEN ISNULL(apd.OTHr,0)/60 > " + NWDayType + @" AND edwsa.DayType='NW'	THEN  " + NWDayType + @" 
        //		  WHEN ISNULL(apd.OTHr,0)/60 > " + WDayType + @" AND edwsa.DayType='W'	THEN  " + WDayType + @" 
        //		  WHEN ISNULL(apd.OTHr,0)/60 > " + HDayType + @" AND edwsa.DayType='H'	THEN  " + HDayType + @" ELSE ISNULL(apd.OTHr,0)/60 END

        //                     ,ExtraOT= CASE WHEN ISNULL(apd.OTHr,0)/60 > " + NWDayType + @" AND edwsa.DayType='NW'	THEN  ISNULL(apd.OTHr,0)/60-" + NWDayType + @" 
        //		  WHEN ISNULL(apd.OTHr,0)/60 > " + WDayType + @" AND edwsa.DayType='W'	THEN  ISNULL(apd.OTHr,0)/60-" + WDayType + @" 
        //		  WHEN ISNULL(apd.OTHr,0)/60 > " + HDayType + @" AND edwsa.DayType='H'	THEN ISNULL(apd.OTHr,0)/60-" + HDayType + @" ELSE 0 END
        //                     ,E.SystemId
        //                     ,e.EmployeeCode
        //                     ,e.EmployeeName
        //                     ,FORMAT(e.DOJ, 'dd-MMM-yyyy') DOJ
        //                     ,EC.UserName EmpCategoryName
        //                     ,ld.UserName Designation
        //                     ,U.UserName Unit
        //                     ,Dv.UserName Division
        //                     ,Dp.UserName Department
        //                     ,Se.UserName Section
        //                     ,SB.UserName SubSection
        //                     ,L.UserName Line
        //                      ,NewOutTime= CASE WHEN ISNULL(apd.OTHr,0)/60 > pl.firstSlab THEN DATEADD(minute, -1* (ISNULL(apd.OTHr,0)-pl.firstSlab*60),apd.OutTime) ELSE null END 
        //                     ,ExtraOTInTime= CASE WHEN ISNULL(apd.OTHr,0)/60 > pl.firstSlab THEN DATEADD(minute, -1* (ISNULL(apd.OTHr,0)-pl.firstSlab*60),apd.OutTime) ELSE null END

        //                     ,NewOutTimeShow= CASE WHEN ISNULL(apd.OTHr,0)/60 > pl.firstSlab THEN FORMAT(DATEADD(minute, -1* (ISNULL(apd.OTHr,0)-pl.firstSlab*60),apd.OutTime), 'hh:mm tt') ELSE null END 
        //                     ,ExtraOTInTimeShow= CASE WHEN ISNULL(apd.OTHr,0)/60 > pl.firstSlab THEN FORMAT( DATEADD(minute, -1* (ISNULL(apd.OTHr,0)-pl.firstSlab*60),apd.OutTime), 'hh:mm tt') ELSE null END	 
        //                     ,ExtraOTOutTimeShow=FORMAT(apd.OutTime, 'hh:mm tt')
        //                        ,ExtraOTOutTime=apd.OutTime
        //                     ,Duration= CASE WHEN ISNULL(apd.OTHr,0)/60 > pl.firstSlab THEN ISNULL(apd.OTHr,0)-pl.firstSlab*60 ELSE 0 END 
        //                        ,FirstSlabMin= CASE WHEN ISNULL(apd.OTHr,0)/60 > " + NWDayType + @" AND edwsa.DayType='NW'	THEN  60*" + NWDayType + @" 
        //		  WHEN ISNULL(apd.OTHr,0)/60 > " + WDayType + @" AND edwsa.DayType='W'	THEN  60*" + WDayType + @" 
        //		  WHEN ISNULL(apd.OTHr,0)/60 > " + HDayType + @" AND edwsa.DayType='H'	THEN  60*" + HDayType + @" ELSE 0 END




        //                        ,IsManualInTime= CASE WHEN ISNULL(apd.IsManualOutTime,0)=1 THEN  'YES' ELSE 'NO' END  
        //                    FROM AttdnProcessData AS apd
        //                    INNER JOIN EmployeeInformation e ON e.SystemId = apd.EmpSystemID
        //                    LEFT JOIN HKP.EmployeeCategory AS EC ON E.EmployeeCategorySystemID = EC.Id
        //                    LEFT JOIN ORG.Unit U ON E.UnitID = U.Id
        //                    LEFT JOIN ORG.Division Dv ON E.DivisionID = Dv.Id
        //                    LEFT JOIN ORG.Department Dp ON E.DepartmentID = Dp.Id
        //                    LEFT JOIN ORG.Section Se ON E.SectionID = Se.Id
        //                    LEFT JOIN ORG.SubSection SB ON E.SubSectionID = SB.Id
        //                    LEFT JOIN ORG.Line L ON E.LineID = L.Id
        //                    LEFT JOIN HKP.Designation D ON E.DesignationSystemID = D.Id
        //                    LEFT JOIN HKP.LegalDesignation AS ld ON E.LegalDesignationId = ld.Id
        //                    LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
        //                    LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
        //                    LEFT JOIN ShiftDefination AS sd ON sd.SystemID=apd.ShiftSystemID                           
        //                    LEFT JOIN  EmpDateWiseShiftAssign AS edwsa ON edwsa.EmpSystemID = apd.EmpSystemID AND edwsa.WorkDate = apd.WorkDate
        //                    LEFT JOIN OTSlabDefineGeneral pl ON pl.DayType = edwsa.DayType AND apd.WorkDate BETWEEN pl.FromDate AND pl.ToDate AND pl.PlantID=apd.PlantID
        //                    WHERE apd.WorkDate='" + WDate + @"' AND  apd.IsOTEntitled=1 

        //                    AND apd.PlantID='" + identity.PlantId + @"' AND(
        //                     (ISNULL(apd.OTHr,0)/60 > " + NWDayType + @" AND edwsa.DayType='NW') OR
        //                     (ISNULL(apd.OTHr,0)/60 > " + HDayType + @" AND edwsa.DayType='H') OR  
        //                     (ISNULL(apd.OTHr,0)/60 > " + WDayType + @" AND edwsa.DayType='W') 
        //                     )

        //                    ORDER BY  e.EmployeeCodePreFix,e.EmployeeCodeNumeric";




        //    var data = _sqlRepository.GetDataCollection(sql);

        //    JsonResult json = Json(new
        //    {
        //        data


        //    }, JsonRequestBehavior.AllowGet);

        //    json.MaxJsonLength = int.MaxValue;
        //    return json;
        //}

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




        public void Delete(string DeleteEmpSystemId, string FromDate, string ToDate)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                string FOT = "DELETE FROM FinalOT where WorkDate between '" + FromDate + @"' and '" + ToDate + @"'  and EmpSystemID IN (" + DeleteEmpSystemId + ")";
                string HOT = "DELETE FROM HourlyOT where WorkDate between '" + FromDate + @"' and '" + ToDate + @"'  and EmpSystemID IN (" + DeleteEmpSystemId + @") and OTType='OTLIMIT' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                objCon.ExecuteNonQueryWrapper(FOT, true, "1");
                objCon.ExecuteNonQueryWrapper(HOT, true, "1");
                //int i = 0;
                //foreach (DataSet value in dsRef)
                //{
                //    if (dsRef[i] != null)
                //    {
                //        objCon.SaveDataSetThroughAdapter(ref dsRef[i], true, "1");
                //        i = i + 1;
                //    }
                //    else
                //    {
                //        i = i + 1;
                //    }
                //}
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
        #endregion

    }


    public class OTLimitTransactionVM
    {

        public string WorkDate { get; set; }
        public string ShiftName { get; set; }
        public string ShiftInTime { get; set; }
        public string ShiftOutTime { get; set; }
        public string InTime { get; set; }
        public string OutTime { get; set; }
        public string NewInTime { get; set; }
        public string NewOutTime { get; set; }
        public string DayStatus { get; set; }
        public decimal TotalOT { get; set; }
        public decimal OT { get; set; }
        public decimal ExtraOT { get; set; }
        public string EmpSystemId { get; set; }
        public bool IsManualInTime { get; set; }
        public string ManualInTime { get; set; }
        public bool IsManualOutTime { get; set; }
        public string ManualOutTime { get; set; }
        public string OriginalDayType { get; set; }
        public bool IsExtraOTOnly { get; set; }
        public decimal FirstSlabMin { get; set; }
        public decimal OTreductionFactor { get; set; }
        public decimal OverStay { get; set; }



    }
    public class OTLimitCalVM
    {
        public decimal TotalOT { get; set; }
        public decimal OT { get; set; }
        public decimal ExtraOT { get; set; }
    }
}