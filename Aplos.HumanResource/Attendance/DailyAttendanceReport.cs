using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Extension;
using Library.Service.Helpers;
using Library.Service.Setups;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.HumanResource.Attendance
{
    public class DailyAttendanceReport
    {
        ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<MailReceiverDetail> _mailReceiverDetailRepository;

        public DailyAttendanceReport(IRepositoryAsync<MailReceiverDetail> mailReceiverDetailRepository)
        {
            _sqlRepository = new SqlRepository();
            _mailReceiverDetailRepository = mailReceiverDetailRepository;
        }

        public void GetDailyDayStatusS(string WorkDate, string PrevWorkDate, string sPlantID, string DepartmentId, string SectionId, string SubsectionId, string LineId, string dayStatus, string employeeCategory, string shift, string entity, string designationList, string JobLocation, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            string secSQL = string.Empty;
            string xxy = string.Empty;
            string XJobLocation = string.Empty;
            clsStaticInfo obs = null;
            string ShiftIds_WC = "";
            try
            {
                if (shift != "ALL" && shift != "''" && shift != "'ALL'")
                {
                    ShiftIds_WC = " and sd.SystemID in (" + shift + ") ";
                }

                if (dayStatus != null)
                {
                    if (dayStatus.ToUpper() != "ALL" && dayStatus != "null" && dayStatus != "" && dayStatus != "''")
                    {
                        xxy = " and dt.Category in (" + dayStatus + ")";
                    }
                }
                //XJobLocation += " And J.SystemID in (" + JobLocation + ")";
                obs = new clsStaticInfo();
                strSql = @" select e.SystemId
                                            from EmployeeInformation e
                                            left join mst.ManpowerBudget mp on mp.id=e.BudgetCode
											left join org.Entity en on en.id=mp.EntityId    
											left join ORG.Position p on p.Id = mp.PositionId
											left join org.Department dep on dep.Id = p.DepartmentId
											left join org.Section s on s.Id = p.SectionId
											left join org.SubSection ss on ss.Id = p.SubSectionId                                       
                                            LEFT JOIN org.Line L ON L.Id = mp.LineId
                                            LEFT JOIN hkp.LegalDesignation LG ON e.LegalDesignationId = LG.Id 
											left join MST.DesignationMasterLegalDesignation dml on dml.LegalDesignationId = LG.Id
											left join mst.DesignationMaster dm on dm.Id = dml.DesignationMasterId
											left join HKP.EmployeeCategory ec on ec.Id=dm.EmployeeCategoryId
                                            
											where   e.PlantId='" + sPlantID + @"' and e.DOJ <= ( '" + WorkDate + @"') and (e.DOS is null or e.DOS >= '" + WorkDate + @"')";


                if (DepartmentId != "ALL")
                {
                    strSql = strSql + @" AND dep.Id in ( " + DepartmentId + ")";
                }
                if (SectionId != "ALL")
                {
                    strSql = strSql + @" AND s.Id in (" + SectionId + ")";
                }
                if (SubsectionId != "ALL")
                {
                    strSql = strSql + @" AND ss.Id in (" + SubsectionId + ")";
                }

                if (employeeCategory != "ALL")
                {
                    strSql = strSql + @" AND ec.Id in (" + employeeCategory + ")";
                }

                if (entity != "ALL")
                {
                    strSql = strSql + @" AND en.Id in (" + entity + ")";
                }
                if (LineId != "ALL" && LineId != "''")
                {
                    strSql = strSql + @" AND isnull(L.Id,'') in (" + LineId + ")";
                }
                if (designationList != "ALL" && designationList != "''")
                {
                    strSql = strSql + @" AND LG.Id in (" + designationList + ")";
                }
                if (JobLocation != "ALL" && JobLocation != "''")
                {
                    strSql = strSql + @"And J.SystemID in (" + JobLocation + ")";
                }
                secSQL = @"SELECT e.SystemId EmpSystemId,e.EmployeeCode,e.FatherName,dt.Category
								,dep.username Department,CONVERT(VARCHAR(5), AD.InTime, 108)iintime
								,iShiftIn = CASE
							 WHEN cs.InTime IS NULL
							 THEN CONVERT(varchar(5),SD.InTime , 108)
							 ELSE CONVERT(VARCHAR(5), cs.InTime , 108)
						     END
                                , e.EmployeeName,L.Id LineID,L.UserName Line,SS.Id SubSectionId,SS.UserName SubSection
								,sd.UserName ShiftName,AD.IsOTEntitled IsOTEntitledToday
                                , ShiftIn  = CASE
							 WHEN cs.InTime IS NULL
							 THEN CONVERT(varchar(15),CAST(SD.InTime AS TIME),100)
							 ELSE CONVERT(VARCHAR(15), CAST(cs.InTime AS TIME), 100)
						     END							
								,ShiftOut = CASE                                   
                           WHEN cs.OutTime IS NULL
                           THEN CONVERT(varchar(15),CAST(SD.OutTime AS TIME),100)
                           ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
                           END
                                , FORMAT(CAST(ap.InTime AS datetime2), N'hh:mm tt') InTime
								,FORMAT(CAST( ap.OutTime AS datetime2), N'hh:mm tt') OutTime
	                            ,  REPLACE(CONVERT(VARCHAR(11), ap.WorkDate, 113), ' ', '-') PDate
	                            , ap.DayStatus TodayStatus
	                            , ap.OTHr ,AD.ToDayDayCategory
                                ,ap.IsOTEntitled IsOTEntitledYesterday, ISNULL(ap.IsOTComfirm,0) IsTodayOTComfirm, ISNULL(AD.IsOTComfirm,0) IsYesterDayOTComfirm
                                    ,ToDayReConfirm = CASE WHEN AD.IsOTComfirm=0 AND AD.FIOTWorkDate IS NOT NULL THEN 1 ELSE 0  END
                                    ,YesterDayReConfirm= CASE WHEN ap.IsOTComfirm=0 AND AD.FIOTWorkDate IS NOT NULL THEN 1 ELSE 0  END
                        , LG.UserName Designation
                         , kk.PrvDayStatus,kk.YesterDayDayCategory
						,kk.YesterdayOTHr,ap.IsManualInTime,ap.IsManualOutTime,hr.OTConsiderOn

                        from EmployeeInformation e

                        left join AttdnProcessData ap on ap.EmpSystemID = e.SystemId
left join DayType dt on dt.DayType = ap.DayStatus
INNER JOIN (SELECT APD.*, FIOT.NormalOTHr, FIOT.WorkDate FIOTWorkDate,dt.Category ToDayDayCategory,Dt.Category
                                            ,SEQ=case when  LTSystemid in (select  id from leavetype where LeaveType='Maternity') then 1
													 when isnull(MaternityStatus,'')<>''  then 1 else 0 end
											--,DS=(select  code from leavetype where LeaveType='Maternity' and id=LTSystemid)
											,DS=case when LTSystemid in (select  id from leavetype where LeaveType='Maternity') then (select  code from leavetype where LeaveType='Maternity' and id=LTSystemid)
											when isnull(MaternityStatus,'')<>'' then MaternityStatus else null end 
                             from dbo.AttdnProcessData APD
							LEFT JOIN FINALOT FIOT on FIOT.EmpSystemID = APD.EmpSystemID AND FIOT.WorkDate=APD.WorkDate
							LEFT JOIN DayType dt on dt.Daytype=APD.DayStatus
							WHERE APD.WorkDate  = '" + WorkDate + @"' 
							) AD ON AD.EmpSystemID = E.SystemID

                        LEFT JOIN dbo.ShiftDefination SD ON ap.ShiftSystemID = SD.SystemID
                        LEFT OUTER JOIN ShiftTimeChgMaster AS cs ON ap.WorkDate BETWEEN cs.FromDate AND cs.ToDate AND sd.SystemID=cs.ShiftDefinationID
                                            left join mst.ManpowerBudget mp on mp.id = e.BudgetCode
                                            left join org.Entity en on en.id = mp.EntityId
                                            left join ORG.Position p on p.Id = mp.PositionId
                                            left join org.Department dep on dep.Id = p.DepartmentId
                                            left join org.Section s on s.Id = p.SectionId
                                            LEFT JOIN PlantWiseHRMSSetting hr on HR.PlantID=e.PlantId
                                            left join org.SubSection ss on ss.Id = p.SubSectionId
                                            LEFT JOIN org.Line L ON L.Id = mp.LineId
                                            LEFT JOIN hkp.LegalDesignation LG ON e.LegalDesignationId = LG.Id
                                            LEFT JOIN JobLocation J ON J.SystemID = e.JobLocationID
                                            left join MST.DesignationMasterLegalDesignation dml on dml.LegalDesignationId = LG.Id
                                            left join mst.DesignationMaster dm on dm.Id = dml.DesignationMasterId
                                            left join HKP.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId
                                            left join(select yap.DayStatus PrvDayStatus, yap.OTHr YesterdayOTHr, yap.EmpSystemID,ydt.Category YesterDayDayCategory from AttdnProcessData yap
                                                left join DayType ydt on ydt.DayType = yap.DayStatus
                                                where yap.WorkDate = '" + PrevWorkDate + @"') kk on kk.EmpSystemID = e.SystemId

where  ap.WorkDate='" + WorkDate + @"' and e.SystemId in (" + strSql + ")  " + ShiftIds_WC + " " + xxy + " ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");

                objCon.BeginTransaction();
                objCon.getDataSet(secSQL, out dsRef);
                objCon.CommitTransaction();
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

        public string GetDailyAttendanceEmpInfo(string companyGroupId, string companyId, string plantId, string SheetHeader, string SheetName, string workDate, string shift, string Entity, string Dept, string Ydate, string Sec, string SSec, string empCategoryList, string designationList, string lineList, string Dstatus, bool WithFatherName, string JobLocation)
        {
            try
            {
                #region Variable
                //clsReport objRpt = null;
                var filePath = "";
                string yot = string.Empty;
                DataTable dtEntity = null;
                DataTable dtPosition = null;

                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet1 = null;
                ReportUtility oRU = null;

                StringCollection dayStatus = null;

                var xlsRow = 1;
                var xlsCol = 1;
                var IsBudgetCodeApplicable = true;

                #endregion Variable
                //objRpt = new clsReport();
                oRU = new ReportUtility();

                Library.Service.Extension.Mail.HumanResourceMailService HRMS = new Library.Service.Extension.Mail.HumanResourceMailService(_mailReceiverDetailRepository);





                dayStatus = new StringCollection();
                StringCollection myCol = new StringCollection();

                if(string.IsNullOrEmpty(Dstatus))
                {
                    string[] DayStatusArr = new string[] { "Present", "Late", "Absent", "Leave", "Weekend", "Half Day", "Holiday" };
                    dayStatus.AddRange(DayStatusArr);
                }
                else
                {
                    var sft = Dstatus.Split(',');
                    foreach (var item in sft)
                    {
                        if (item != "")
                        {
                            String[] dayStat = new String[] { item };
                            dayStatus.AddRange(dayStat);                           

                        }
                    }
                }
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(dayStatus.Count);

                Dictionary<string, List<DataRow>> dicEmpMonthAttdnSummary = HRMS.GetMontlyAttdnSummary(companyGroupId, plantId, workDate);
                for (int dsi = 0; dsi < dayStatus.Count; dsi++)
                {
                    var daylyAttdnEmpInfo = HRMS.GetDAttendanceEmployee(companyGroupId, companyId, plantId, dayStatus[dsi], workDate, shift, Entity, Dept, Ydate, Sec, SSec, empCategoryList, designationList, lineList, Dstatus, JobLocation);



                    HRMS.GetEntityPosition(companyGroupId, out DataSet dsEntityPosition);

                    var dvEntity = new DataView(dsEntityPosition.Tables[0])
                    {
                        RowFilter = "RType = 'Entity'",
                        Sort = "eSequence"
                    };
                    dtEntity = dvEntity.ToTable(true, "UserName");

                    var dvPosition = new DataView(dsEntityPosition.Tables[0])
                    {
                        RowFilter = "RType = 'Position'",
                        Sort = "pSequence"
                    };
                    dtPosition = dvPosition.ToTable(true, "UserName");

                    var dvBC = new DataView(daylyAttdnEmpInfo);

                    sheet1 = workbook.Worksheets[dsi];

                    string xx = dayStatus[dsi].Replace("\"", "").Trim();

                    sheet1.Name = xx;




                    xlsRow = 5;
                    #region variable
                    var cEmployeeCode = 0; var cBudgetCode = 0; var cCurrentMonthAbsent = 0; var cName = 0; var cDOJ = 0; var cDOB = 0;
                    var cTotalAbsentORLate = 0; var cShiftInTime = 0; var cShiftOutTime = 0;
                    var cDesignation = 0; var cGivenDesignation = 0; var cLD = 0; var cLeaveType = 0;
                    var cDayStatus = 0; var cEmpCatg = 0; var cEmpLocation = 0; var cDepertment = 0; var cEntity = 0; var cSl = 0;
                    var endXlsCol = 0;
                    var colNum = 0;
                    var cYesterdayDaystatus = 0;
                    var cYesterdayOverStayHour = 0;
                    var cRemarks = 0;
                    #endregion variable
                    //xlsRow++;
                    xlsCol = 1;
                    #region Header
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Sl. No.", 5); cSl = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Code", 12); cEmployeeCode = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Name", 30); cName = xlsCol; xlsCol++;
                    if (WithFatherName == true)
                    {
                        oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Father Name", 15); cDOB = xlsCol; xlsCol++;
                    }

                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Department", 19); cDepertment = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Designation", 17); cLD = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Emp. Category", 10); cEmpCatg = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Emp. Location", 13); cEmpLocation = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Shift Name", 20); cDesignation = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "In Time", 12); cDOJ = xlsCol; xlsCol++;

                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Month Absent", 9); cCurrentMonthAbsent = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Month Late", 9); cGivenDesignation = xlsCol; xlsCol++;

                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Yesterday Status", 10); cYesterdayDaystatus = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Yesterday OverStay", 10); cYesterdayOverStayHour = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Remarks", 19); cRemarks = xlsCol;

                    #endregion Header



                    if (daylyAttdnEmpInfo.Rows.Count > 0)
                    {

                        var fPanRow = xlsRow + 1;//Freeze pan starting rows
                        xlsCol--;
                        endXlsCol = xlsCol;
                        xlsRow++;
                        var slCount = 0;
                        for (int i = 0; i < daylyAttdnEmpInfo.Rows.Count; i++)
                        {
                            slCount++;
                            #region Loop

                            if (dayStatus[dsi] == "Absent" || dayStatus[dsi] == "Late")
                            {
                                if (dicEmpMonthAttdnSummary.ContainsKey(daylyAttdnEmpInfo.Rows[i]["SystemId"].ToString()))
                                {
                                    List<DataRow> drTotalAbsentOrLate = dicEmpMonthAttdnSummary[daylyAttdnEmpInfo.Rows[i]["SystemId"].ToString()];

                                    if (dayStatus[dsi] == "Absent")
                                    {
                                        oRU.SetText(ref sheet1, xlsRow, cTotalAbsentORLate, Convert.ToInt32(clsStaticInfo.dbl(drTotalAbsentOrLate[0]["TotalAbsent"].ToString())));

                                    }
                                    if (dayStatus[dsi] == "Late")
                                    {
                                        oRU.SetText(ref sheet1, xlsRow, cTotalAbsentORLate, Convert.ToInt32(clsStaticInfo.dbl(drTotalAbsentOrLate[0]["TotalLate"].ToString())));

                                    }
                                }

                            }
                            oRU.SetText(ref sheet1, xlsRow, cSl, slCount.ToString());
                            oRU.SetText(ref sheet1, xlsRow, cEmployeeCode, daylyAttdnEmpInfo.Rows[i]["EmployeeCode"].ToString());
                            oRU.SetText(ref sheet1, xlsRow, cDepertment, daylyAttdnEmpInfo.Rows[i]["Department"].ToString());
                            //oRU.SetText(ref sheet1, xlsRow, cEntity, daylyAttdnEmpInfo.Rows[i]["Entity"].ToString());

                            oRU.SetText(ref sheet1, xlsRow, cName, daylyAttdnEmpInfo.Rows[i]["EmployeeName"].ToString());
                            if (WithFatherName == true)
                            {
                                oRU.SetText(ref sheet1, xlsRow, cDOB, daylyAttdnEmpInfo.Rows[i]["FatherName"].ToString());
                            }
                            oRU.SetText(ref sheet1, xlsRow, cDOJ, daylyAttdnEmpInfo.Rows[i]["Intime"].ToString());
                            oRU.SetText(ref sheet1, xlsRow, cEmpCatg, daylyAttdnEmpInfo.Rows[i]["empCategory"].ToString());
                            //oRU.SetText(ref sheet1, xlsRow, cEmpLocation, daylyAttdnEmpInfo.Rows[i]["EmployeeLocation"].ToString());

                            //if (dayStatus[dsi] == "Leave")
                            //{
                            //    oRU.SetText(ref sheet1, xlsRow, cLeaveType, daylyAttdnEmpInfo.Rows[i]["LeaveType"].ToString());
                            //}
                            //if (dayStatus[dsi] == "Work Off")
                            //{
                            //    if (!string.IsNullOrEmpty(daylyAttdnEmpInfo.Rows[i]["DayStatus"].ToString()))
                            //    {
                            //        oRU.SetText(ref sheet1, xlsRow, cDayStatus, daylyAttdnEmpInfo.Rows[i]["DayStatus"].ToString());
                            //    }
                            //}

                            oRU.SetText(ref sheet1, xlsRow, cEmpLocation, daylyAttdnEmpInfo.Rows[i]["Location"].ToString());
                            oRU.SetText(ref sheet1, xlsRow, cDesignation, daylyAttdnEmpInfo.Rows[i]["ShiftName"].ToString());
                            //oRU.SetText(ref sheet1, xlsRow, cShiftInTime, daylyAttdnEmpInfo.Rows[i]["ShiftIn"].ToString());
                            //oRU.SetText(ref sheet1, xlsRow, cShiftOutTime, daylyAttdnEmpInfo.Rows[i]["ShiftOut"].ToString());

                            oRU.SetText(ref sheet1, xlsRow, cLD, daylyAttdnEmpInfo.Rows[i]["Designation"].ToString());
                            oRU.SetText(ref sheet1, xlsRow, cGivenDesignation, daylyAttdnEmpInfo.Rows[i]["CurrentMonthLate"].ToString());
                            oRU.SetText(ref sheet1, xlsRow, cCurrentMonthAbsent, daylyAttdnEmpInfo.Rows[i]["CurrentMonthAbsent"].ToString());
                            if (!string.IsNullOrEmpty(daylyAttdnEmpInfo.Rows[i]["YesterdayOTHr"].ToString()))
                            {
                                oRU.GetOT(daylyAttdnEmpInfo.Rows[i]["OTConsiderOn"].ToString(), daylyAttdnEmpInfo.Rows[i]["YesterdayOTHr"].ToString(), out yot);
                            }
                            if (yot == "0:00")
                            {
                                oRU.SetText(ref sheet1, xlsRow, cYesterdayOverStayHour, "");
                            }
                            else
                            {
                                oRU.SetText(ref sheet1, xlsRow, cYesterdayOverStayHour, yot);
                            }

                            oRU.SetText(ref sheet1, xlsRow, cYesterdayDaystatus, daylyAttdnEmpInfo.Rows[i]["PrvDayStatus"].ToString());

                            #endregion Loop
                            xlsRow++;
                        }

                        oRU.SetHeaderText(ref sheet1, 4, 1, xx + " Report", ExcelHAlign.HAlignCenter);
                        //oRU.SetHeaderText(ref sheet1, 4, 1, xx + " Report", ExcelHAlign.HAlignCenter);
                        sheet1.Range[4, 1, 4, endXlsCol].Merge();
                        var attdnHeader = SheetHeader + " On " + workDate;
                        if (!string.IsNullOrEmpty(plantId))
                            oRU.PlantHeader(ref sheet1, endXlsCol, attdnHeader, plantId);
                        else
                            oRU.MainCompanyGroupHeader(ref sheet1, endXlsCol, attdnHeader, companyGroupId);

                        #region UsedRange Alignment
                        sheet1.UsedRange.WrapText = true;
                        sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                        sheet1.UsedRange["A" + fPanRow].FreezePanes();
                        #endregion UsedRange Alignment

                        oRU.PageSetupAuto(ref sheet1, 5, ExcelPageOrientation.Landscape, "TS");

                    }


                }
                workbook.Version = ExcelVersion.Excel97to2003;
                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xls");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public string GetDailyAttendanceEmpInformation(string companyGroupId, string companyId, string plantId, string SheetHeader, string SheetName, string workDate, string shift, string Entity, string Dept, string Ydate, string Sec, string SSec, string empCategoryList, string designationList, string lineList, string Dstatus, bool WithFatherName, string JobLocation)
        {
            try
            {
                #region Variable
                //clsReport objRpt = null;
                var filePath = "";
                string yot = string.Empty;
                DataTable dtEntity = null;
                DataTable dtPosition = null;

                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                IWorksheet sheet1 = null;
                ReportUtility oRU = null;

                StringCollection dayStatus = null;

                var xlsRow = 1;
                var xlsCol = 1;
                var IsBudgetCodeApplicable = true;

                #endregion Variable
                //objRpt = new clsReport();
                oRU = new ReportUtility();

                Library.Service.Extension.Mail.HumanResourceMailService HRMS = new Library.Service.Extension.Mail.HumanResourceMailService(_mailReceiverDetailRepository);





                dayStatus = new StringCollection();
                StringCollection myCol = new StringCollection();

                if (string.IsNullOrEmpty(Dstatus))
                {
                    string[] DayStatusArr = new string[] { "Present", "Late", "Absent", "Leave", "Weekend", "Half Day", "Holiday" };
                    dayStatus.AddRange(DayStatusArr);
                }
                else
                {
                    var sft = Dstatus.Split(',');
                    foreach (var item in sft)
                    {
                        if (item != "")
                        {
                            String[] dayStat = new String[] { item };
                            dayStatus.AddRange(dayStat);

                        }
                    }
                }
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(dayStatus.Count);

                Dictionary<string, List<DataRow>> dicEmpMonthAttdnSummary = HRMS.GetMontlyAttdnSummary(companyGroupId, plantId, workDate);
                for (int dsi = 0; dsi < dayStatus.Count; dsi++)
                {
                    var daylyAttdnEmpInfo = GetDAttendanceEmployees(companyGroupId, companyId, plantId, dayStatus[dsi], workDate, shift, Entity, Dept, Ydate, Sec, SSec, empCategoryList, designationList, lineList, Dstatus, JobLocation);



                    HRMS.GetEntityPosition(companyGroupId, out DataSet dsEntityPosition);

                    var dvEntity = new DataView(dsEntityPosition.Tables[0])
                    {
                        RowFilter = "RType = 'Entity'",
                        Sort = "eSequence"
                    };
                    dtEntity = dvEntity.ToTable(true, "UserName");

                    var dvPosition = new DataView(dsEntityPosition.Tables[0])
                    {
                        RowFilter = "RType = 'Position'",
                        Sort = "pSequence"
                    };
                    dtPosition = dvPosition.ToTable(true, "UserName");

                    var dvBC = new DataView(daylyAttdnEmpInfo);

                    sheet1 = workbook.Worksheets[dsi];

                    string xx = dayStatus[dsi].Replace("\"", "").Trim();

                    sheet1.Name = xx;




                    xlsRow = 5;
                    #region variable
                    var cEmployeeCode = 0; var cBudgetCode = 0; var cCurrentMonthAbsent = 0; var cName = 0; var cDOJ = 0; var cDOB = 0;
                    var cTotalAbsentORLate = 0; var cShiftInTime = 0; var cShiftOutTime = 0;
                    var cDesignation = 0; var cGivenDesignation = 0; var cLD = 0; var cLeaveType = 0;
                    var cDayStatus = 0; var cEmpCatg = 0; var cEmpLocation = 0; var cDepertment = 0; var cEntity = 0; var cSl = 0;
                    var endXlsCol = 0;
                    var colNum = 0;
                    var cYesterdayDaystatus = 0;
                    var cYesterdayOverStayHour = 0;
                    var cRemarks = 0;
                    #endregion variable
                    //xlsRow++;
                    xlsCol = 1;
                    #region Header
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Sl. No.", 5); cSl = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Code", 12); cEmployeeCode = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Name", 30); cName = xlsCol; xlsCol++;
                    if (WithFatherName == true)
                    {
                        oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Father Name", 15); cDOB = xlsCol; xlsCol++;
                    }

                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Department", 19); cDepertment = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Designation", 17); cLD = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Emp. Category", 10); cEmpCatg = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Emp. Location", 13); cEmpLocation = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Shift Name", 20); cDesignation = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "In Time", 12); cDOJ = xlsCol; xlsCol++;

                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Month Absent", 9); cCurrentMonthAbsent = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Month Late", 9); cGivenDesignation = xlsCol; xlsCol++;

                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Yesterday Status", 10); cYesterdayDaystatus = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Yesterday OverStay", 10); cYesterdayOverStayHour = xlsCol; xlsCol++;
                    oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Remarks", 19); cRemarks = xlsCol;

                    #endregion Header



                    if (daylyAttdnEmpInfo.Rows.Count > 0)
                    {

                        var fPanRow = xlsRow + 1;//Freeze pan starting rows
                        xlsCol--;
                        endXlsCol = xlsCol;
                        xlsRow++;
                        var slCount = 0;
                        for (int i = 0; i < daylyAttdnEmpInfo.Rows.Count; i++)
                        {
                            slCount++;
                            #region Loop

                            if (dayStatus[dsi] == "Absent" || dayStatus[dsi] == "Late")
                            {
                                if (dicEmpMonthAttdnSummary.ContainsKey(daylyAttdnEmpInfo.Rows[i]["SystemId"].ToString()))
                                {
                                    List<DataRow> drTotalAbsentOrLate = dicEmpMonthAttdnSummary[daylyAttdnEmpInfo.Rows[i]["SystemId"].ToString()];

                                    if (dayStatus[dsi] == "Absent")
                                    {
                                        oRU.SetText(ref sheet1, xlsRow, cTotalAbsentORLate, Convert.ToInt32(clsStaticInfo.dbl(drTotalAbsentOrLate[0]["TotalAbsent"].ToString())));

                                    }
                                    if (dayStatus[dsi] == "Late")
                                    {
                                        oRU.SetText(ref sheet1, xlsRow, cTotalAbsentORLate, Convert.ToInt32(clsStaticInfo.dbl(drTotalAbsentOrLate[0]["TotalLate"].ToString())));

                                    }
                                }

                            }
                            oRU.SetText(ref sheet1, xlsRow, cSl, slCount.ToString());
                            oRU.SetText(ref sheet1, xlsRow, cEmployeeCode, daylyAttdnEmpInfo.Rows[i]["EmployeeCode"].ToString());
                            oRU.SetText(ref sheet1, xlsRow, cDepertment, daylyAttdnEmpInfo.Rows[i]["Department"].ToString());
                            //oRU.SetText(ref sheet1, xlsRow, cEntity, daylyAttdnEmpInfo.Rows[i]["Entity"].ToString());

                            oRU.SetText(ref sheet1, xlsRow, cName, daylyAttdnEmpInfo.Rows[i]["EmployeeName"].ToString());
                            if (WithFatherName == true)
                            {
                                oRU.SetText(ref sheet1, xlsRow, cDOB, daylyAttdnEmpInfo.Rows[i]["FatherName"].ToString());
                            }
                            oRU.SetText(ref sheet1, xlsRow, cDOJ, daylyAttdnEmpInfo.Rows[i]["Intime"].ToString());
                            oRU.SetText(ref sheet1, xlsRow, cEmpCatg, daylyAttdnEmpInfo.Rows[i]["empCategory"].ToString());
                            //oRU.SetText(ref sheet1, xlsRow, cEmpLocation, daylyAttdnEmpInfo.Rows[i]["EmployeeLocation"].ToString());

                            //if (dayStatus[dsi] == "Leave")
                            //{
                            //    oRU.SetText(ref sheet1, xlsRow, cLeaveType, daylyAttdnEmpInfo.Rows[i]["LeaveType"].ToString());
                            //}
                            //if (dayStatus[dsi] == "Work Off")
                            //{
                            //    if (!string.IsNullOrEmpty(daylyAttdnEmpInfo.Rows[i]["DayStatus"].ToString()))
                            //    {
                            //        oRU.SetText(ref sheet1, xlsRow, cDayStatus, daylyAttdnEmpInfo.Rows[i]["DayStatus"].ToString());
                            //    }
                            //}

                            oRU.SetText(ref sheet1, xlsRow, cEmpLocation, daylyAttdnEmpInfo.Rows[i]["Location"].ToString());
                            oRU.SetText(ref sheet1, xlsRow, cDesignation, daylyAttdnEmpInfo.Rows[i]["ShiftName"].ToString());
                            //oRU.SetText(ref sheet1, xlsRow, cShiftInTime, daylyAttdnEmpInfo.Rows[i]["ShiftIn"].ToString());
                            //oRU.SetText(ref sheet1, xlsRow, cShiftOutTime, daylyAttdnEmpInfo.Rows[i]["ShiftOut"].ToString());

                            oRU.SetText(ref sheet1, xlsRow, cLD, daylyAttdnEmpInfo.Rows[i]["Designation"].ToString());
                            oRU.SetText(ref sheet1, xlsRow, cGivenDesignation, daylyAttdnEmpInfo.Rows[i]["CurrentMonthLate"].ToString());
                            oRU.SetText(ref sheet1, xlsRow, cCurrentMonthAbsent, daylyAttdnEmpInfo.Rows[i]["CurrentMonthAbsent"].ToString());
                            if (!string.IsNullOrEmpty(daylyAttdnEmpInfo.Rows[i]["YesterdayOTHr"].ToString()))
                            {
                                oRU.GetOT(daylyAttdnEmpInfo.Rows[i]["OTConsiderOn"].ToString(), daylyAttdnEmpInfo.Rows[i]["YesterdayOTHr"].ToString(), out yot);
                            }
                            if (yot == "0:00")
                            {
                                oRU.SetText(ref sheet1, xlsRow, cYesterdayOverStayHour, "");
                            }
                            else
                            {
                                oRU.SetText(ref sheet1, xlsRow, cYesterdayOverStayHour, yot);
                            }

                            oRU.SetText(ref sheet1, xlsRow, cYesterdayDaystatus, daylyAttdnEmpInfo.Rows[i]["PrvDayStatus"].ToString());

                            #endregion Loop
                            xlsRow++;
                        }

                        oRU.SetHeaderText(ref sheet1, 4, 1, xx + " Report", ExcelHAlign.HAlignCenter);
                        //oRU.SetHeaderText(ref sheet1, 4, 1, xx + " Report", ExcelHAlign.HAlignCenter);
                        sheet1.Range[4, 1, 4, endXlsCol].Merge();
                        var attdnHeader = SheetHeader + " On " + workDate;
                        if (!string.IsNullOrEmpty(plantId))
                            oRU.PlantHeader(ref sheet1, endXlsCol, attdnHeader, plantId);
                        else
                            oRU.MainCompanyGroupHeader(ref sheet1, endXlsCol, attdnHeader, companyGroupId);

                        #region UsedRange Alignment
                        sheet1.UsedRange.WrapText = true;
                        sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                        sheet1.UsedRange["A" + fPanRow].FreezePanes();
                        #endregion UsedRange Alignment

                        oRU.PageSetupAuto(ref sheet1, 5, ExcelPageOrientation.Landscape, "TS");

                    }


                }
                workbook.Version = ExcelVersion.Excel97to2003;
                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SheetName + ".xls");
                workbook.SaveAs(filePath);
                workbook.Close();
                excelEngine.Dispose();
                return filePath;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public DataTable GetDAttendanceEmployees(string companyGroupId, string companyId, string plantId, string dayStatus, string attendanceDate, string shift, string Entity, string Dept, string Ydate, string Sec, string SSec, string empCategoryList, string designationList, string lineList, string Dstatus, string JobLocation)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            string secSQL = string.Empty;
            string xxy = string.Empty;
            string XJobLocation = string.Empty;
            clsStaticInfo obs = null;
            string ShiftIds_WC = "";
            try
            {
                string xx = "'" + dayStatus.Replace('"', ' ').Trim() + "'";

                if (shift != "ALL" && shift != "''")
                {
                    ShiftIds_WC = " and sd.SystemID in (" + shift + ") ";
                }

                //if (xx == "'Other'")
                //{
                //    xxy += " AND  dt.Category in( 'Half Day','Holiday','Working Day')";
                //}
                //else
                //{
                xxy += " AND  DT.Category = " + xx + "";
                XJobLocation += " And J.SystemID in (" + JobLocation + ")";
                //}

                obs = new clsStaticInfo();
                strSql = @" select e.SystemId
                                            from EmployeeInformation e
                                            left join mst.ManpowerBudget mp on mp.id=e.BudgetCode
											left join org.Entity en on en.id=mp.EntityId    
											left join ORG.Position p on p.Id = mp.PositionId
											left join org.Department dep on dep.Id = p.DepartmentId
											left join org.Section s on s.Id = p.SectionId
											left join org.SubSection ss on ss.Id = p.SubSectionId                                       
                                            LEFT JOIN org.Line L ON L.Id = mp.LineId
                                            LEFT JOIN hkp.LegalDesignation LG ON e.LegalDesignationId = LG.Id 
											left join MST.DesignationMasterLegalDesignation dml on dml.LegalDesignationId = LG.Id
											left join mst.DesignationMaster dm on dm.Id = dml.DesignationMasterId
											left join HKP.EmployeeCategory ec on ec.Id=dm.EmployeeCategoryId
											where   e.PlantId='" + plantId + @"' and e.DOJ <= ( '" + attendanceDate + @"') and (e.DOS is null or e.DOS >= '" + attendanceDate + @"')";


                if (Dept != "ALL")
                {
                    strSql = strSql + @" AND dep.Id in ( " + Dept + ")";
                }
                if (Sec != "ALL")
                {
                    strSql = strSql + @" AND s.Id in (" + Sec + ")";
                }
                if (SSec != "ALL")
                {
                    strSql = strSql + @" AND ss.Id in (" + SSec + ")";
                }

                if (empCategoryList != "ALL" && !string.IsNullOrEmpty(empCategoryList))
                {
                    strSql = strSql + @" AND ec.Id in (" + empCategoryList + ")";
                }

                if (Entity != "ALL")
                {
                    strSql = strSql + @" AND en.Id in (" + Entity + ")";
                }
                if (lineList != "ALL" && lineList != "''" && !string.IsNullOrEmpty(lineList))
                {
                    strSql = strSql + @" AND isnull(L.Id,'') in (" + lineList + ")";
                }
                if (designationList != "ALL" && designationList != "''" && !string.IsNullOrEmpty(designationList))
                {
                    strSql = strSql + @" AND LG.Id in (" + designationList + ")";
                }

                secSQL = @"SELECT e.SystemId,e.EmployeeCode,e.FatherName,el.UserName [Location]
								,dep.username Department,en.UserName Entity,ec.UserName empCategory
                                , e.EmployeeName,Lc CurrentMonthLate,LT.UserName LeaveType
									,lcA CurrentMonthAbsent
								,sd.UserName ShiftName
                                , ShiftIn  = CASE
							 WHEN cs.InTime IS NULL
							 THEN CONVERT(varchar(15),CAST(SD.InTime AS TIME),100)
							 ELSE CONVERT(VARCHAR(15), CAST(cs.InTime AS TIME), 100)
						     END
								,ShiftOut = CASE                                   
                           WHEN cs.OutTime IS NULL
                           THEN CONVERT(varchar(15),CAST(SD.OutTime AS TIME),100)
                           ELSE CONVERT(VARCHAR(15), CASt(cs.OutTime AS TIME), 100)
                           END
                                , FORMAT(CAST(ap.InTime AS datetime2), N'hh:mm tt') InTime
								,FORMAT(CAST( ap.OutTime AS datetime2), N'hh:mm tt') OutTime
	                            ,  REPLACE(CONVERT(VARCHAR(11), ap.WorkDate, 113), ' ', '-') PDate
	                            , ap.DayStatus
	                            , ap.OTHr TodaysOT
                        , LG.UserName Designation
                         , kk.PrvDayStatus
						,kk.YesterdayOTHr,ap.IsManualInTime,ap.IsManualOutTime,hr.OTConsiderOn

                        from EmployeeInformation e

                        left join AttdnProcessData ap on ap.EmpSystemID = e.SystemId
                        left join DayType DT on DT.DayType = ap.DayStatus
                        LEFT JOIN LeaveType LT ON LT.Id = AP.LTSystemID
                        LEFT JOIN dbo.ShiftDefination SD ON ap.ShiftSystemID = SD.SystemID
                        LEFT OUTER JOIN ShiftTimeChgMaster AS cs ON ap.WorkDate BETWEEN cs.FromDate AND cs.ToDate AND sd.SystemID=cs.ShiftDefinationID
                                            left join mst.ManpowerBudget mp on mp.id = e.BudgetCode
                                            left join hkp.EmployeeLocation el on el.Id = mp.EmployeeLocationId
                                            left join org.Entity en on en.id = mp.EntityId

                                            left join ORG.Position p on p.Id = mp.PositionId

                                            left join org.Department dep on dep.Id = p.DepartmentId

                                            left join org.Section s on s.Id = p.SectionId
                                            LEFT JOIN PlantWiseHRMSSetting hr on HR.PlantID=e.PlantId
                                            left join org.SubSection ss on ss.Id = p.SubSectionId
                                            LEFT JOIN org.Line L ON L.Id = mp.LineId
                                            LEFT JOIN hkp.LegalDesignation LG ON e.LegalDesignationId = LG.Id

                                            left join MST.DesignationMasterLegalDesignation dml on dml.LegalDesignationId = LG.Id

                                            left join mst.DesignationMaster dm on dm.Id = dml.DesignationMasterId

                                            left join HKP.EmployeeCategory ec on ec.Id = dm.EmployeeCategoryId

                                            LEFT JOIN JobLocation J ON J.SystemID = e.JobLocationID

                                    left join (
									select count(atdnd.WorkDate)Lc, atdnd.EmpSystemID from AttdnProcessData atdnd
									left join DayType DT on DT.DayType= atdnd.DayStatus
									where MONTH(atdnd.WorkDate)=MONTH('" + attendanceDate + @"') and YEAR(atdnd.WorkDate)=YEAR('" + attendanceDate + @"')  and dt.Category = 'Late'
									Group By EmpSystemID
									) lc on lc.EmpSystemID = E.SystemID
									left join (
									select count(atdnd.WorkDate)LcA, atdnd.EmpSystemID from AttdnProcessData atdnd
									left join DayType DT on DT.DayType= atdnd.DayStatus
									where MONTH(atdnd.WorkDate)=MONTH('" + attendanceDate + @"') and YEAR(atdnd.WorkDate)=YEAR('" + attendanceDate + @"')  and dt.Category = 'Absent'
									Group By EmpSystemID
									) lcA on lcA.EmpSystemID = E.SystemID

                                            left join(select yap.DayStatus PrvDayStatus, yap.OTHr YesterdayOTHr, yap.EmpSystemID from AttdnProcessData yap where yap.WorkDate = '" + Ydate + @"') kk on kk.EmpSystemID = e.SystemId

                                  where  ap.WorkDate='" + attendanceDate + @"' and e.SystemId in (" + strSql + ")  " + ShiftIds_WC + " " + xxy + " " + XJobLocation + "";


                return _sqlRepository.GetDataTable(secSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }
    }
}
