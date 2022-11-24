using Library.Data.Sql;
using Library.Service.Helpers;
using OTSBD;
using System.Drawing;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using Library.Service.Payrolls.OT;

namespace Library.HumanResource.Report.OT
{
    public class MHourlyOT
    {
        SqlRepository _sqlRepository = null;

        public MHourlyOT()
        {
            _sqlRepository = new SqlRepository();
        }
        public IWorkbook GetMIndividualDailyOT(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName, string FromDate, string ToDate, string OTDuration, bool CheckBox, string OTfinal, string filePath = "")
        {

            #region declare
            clsReport objRpt = null;
            clsReport objRptSR = null;
            ReportUtility oru = new ReportUtility();
            //DataSet dsHourlyOffDutyTag = null;
            DataTable dtHourlyOffDutyTag = null;
            //DataSet dsCmp = null;
            //DataSet dsFactory = null;
            DataView dvOT = null;

            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string OTConsiderOn = string.Empty;
            var OTOverstay1 = 0.00;
            var OTOverstay2 = 0.00;
            var OTOverstay = 0.00;
            DataSet dsOTPolicy = null;//
            DataSet dsSStructure = null;
            string _currencyId = string.Empty;
            Dictionary<string, double> dicNW = null;
            Dictionary<string, double> dicW = null;
            Dictionary<string, double> dicH = null;
            #endregion
            try
            {
                ExcelEngine excelEngine = null;
                IApplication application = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2013;

                IWorkbook workbook = application.Workbooks.Create(4);

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), CompanyId + ".jpg");  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                #endregion
                objRpt = new clsReport();

                objRptSR = new clsReport(_sqlRepository);

                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                #region OT Rate
                if (CheckBox)
                {
                    DataSet dsCurrency = null;
                    clsOTCalculation otc = new clsOTCalculation();
                    otc.LoadOverTimePolicy(PlantId, FromDate, ToDate, out dsOTPolicy);
                    otc.LoadSalaryStructure(PlantId, FromDate, ToDate, out dsSStructure);

                    clsSalaryInfo objSal = new clsSalaryInfo();
                    objSal.GetLocalCurrency(CompanyGroupId, PlantId, out dsCurrency);
                    if (dsCurrency.Tables[0].Rows.Count > 0)
                    {
                        _currencyId = "" + dsCurrency.Tables[0].Rows[0]["LocalCurrency"].ToString().Trim();
                    }
                    else
                    {
                        throw new Exception("No currency found...");
                    }

                    GenerateDic(dsOTPolicy, dsSStructure, _currencyId, out dicNW, out dicW, out dicH);
                }
                #endregion

                dtHourlyOffDutyTag = objRptSR.GetCIndividualDailyOT(FromDate, ToDate, OTDuration, OTfinal, PlantId, CompanyId, CompanyGroupId);


                var dtCmp = objRptSR.SelectedPlantWiseCompanyDT(PlantId);

                var dtFactory = objRptSR.SelectedPlantDT(PlantId);
                #region Variable
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                var iName = 0;
                var iEmployeeCode = 0;
                var iSubSection = 0;
                var iSection = 0;
                var iTotalHr = 0;
                var iTotalHrReal = 0;
                var iOutTime = 0;
                var iInTime = 0;

                var iLine = 0;
                var iDOJ = 0;
                var iDepartment = 0;
                var iDesignation = 0;
                var isl = 0;
                var iWorkDate = 0;
                var SLNo = 1;
                #endregion

                if (dtHourlyOffDutyTag.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }
                #region Hourly Ot
                //workbook = application.Workbooks.Create(4);
                IWorksheet sheet1 = null;
                IWorksheet sheet2 = null;
                IWorksheet sheet3 = null;
                IWorksheet sheet4 = null;

                sheet1 = workbook.Worksheets[0];
                sheet2 = workbook.Worksheets[1];
                sheet3 = workbook.Worksheets[2];
                sheet4 = workbook.Worksheets[3];


                xlsRow = 6;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No:";
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow - 1, 1].RowHeight = 20;
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Bold = true;

                #region ------------------Column Header------------------
                isl = xlsCol;
                sheet1.Range[xlsRow, isl].Text = "SL";
                sheet1.Range[xlsRow, isl].ColumnWidth = 7;

                xlsCol += 1;
                iEmployeeCode = xlsCol;
                sheet1.Range[xlsRow, iEmployeeCode].Text = "Emp Code";
                sheet1.Range[xlsRow, iEmployeeCode].ColumnWidth = 10;

                xlsCol += 1;
                iName = xlsCol;
                sheet1.Range[xlsRow, iName].Text = "Emp Name";
                sheet1.Range[xlsRow, iName].ColumnWidth = 25;



                xlsCol += 1;
                iDOJ = xlsCol;
                sheet1.Range[xlsRow, iDOJ].Text = "DOJ";
                sheet1.Range[xlsRow, iDOJ].ColumnWidth = 20;

                xlsCol += 1;
                iDepartment = xlsCol;
                sheet1.Range[xlsRow, iDepartment].Text = "Department";
                sheet1.Range[xlsRow, iDepartment].ColumnWidth = 25;

                xlsCol += 1;
                iDesignation = xlsCol;
                sheet1.Range[xlsRow, iDesignation].Text = "Designation";
                sheet1.Range[xlsRow, iDesignation].ColumnWidth = 25;

                xlsCol += 1;
                iSection = xlsCol;
                sheet1.Range[xlsRow, iSection].Text = "Section";
                sheet1.Range[xlsRow, iSection].ColumnWidth = 14;

                xlsCol += 1;
                iSubSection = xlsCol;
                sheet1.Range[xlsRow, iSubSection].Text = "Sub Section";
                sheet1.Range[xlsRow, iSubSection].ColumnWidth = 16;

                xlsCol += 1;
                iLine = xlsCol;
                sheet1.Range[xlsRow, iLine].Text = "Line";
                sheet1.Range[xlsRow, iLine].ColumnWidth = 12;
                xlsCol += 1;
                iWorkDate = xlsCol;
                sheet1.Range[xlsRow, iWorkDate].Text = "Work Date";
                sheet1.Range[xlsRow, iWorkDate].ColumnWidth = 12;

                xlsCol += 1;
                iInTime = xlsCol;
                sheet1.Range[xlsRow, iInTime].Text = "In Time";
                sheet1.Range[xlsRow, iInTime].ColumnWidth = 12;

                xlsCol += 1;
                iOutTime = xlsCol;
                sheet1.Range[xlsRow, iOutTime].Text = "Out Time";
                sheet1.Range[xlsRow, iOutTime].ColumnWidth = 12;

                xlsCol += 1;
                iTotalHr = xlsCol;
                sheet1.Range[xlsRow, iTotalHr].Text = "OT Hours";
                sheet1.Range[xlsRow, iTotalHr].ColumnWidth = 15;
                //xlsCol += 1;
                //iTotalHrReal = xlsCol;
                //sheet1.Range[xlsRow, iTotalHrReal].Text = "Total(Hours)";
                //sheet1.Range[xlsRow, iTotalHrReal].ColumnWidth = 15;

                xlsCol += 1;
                int iBasicHead = xlsCol;
                sheet1.Range[xlsRow, iBasicHead].Text = "Basic";
                sheet1.Range[xlsRow, iBasicHead].ColumnWidth = 15;

                int inw_rate = 0;
                int iAmount = 0;
                //int iw_rate = 0;
                //int ih_rate = 0;
                if (CheckBox)
                {
                    xlsCol += 1;
                    inw_rate = xlsCol;
                    sheet1.Range[xlsRow, inw_rate].Text = "Rate";
                    sheet1.Range[xlsRow, inw_rate].ColumnWidth = 12;

                    //xlsCol += 1;
                    //iw_rate = xlsCol;
                    //sheet1.Range[xlsRow, iw_rate].Text = "Weekoff Rate";
                    //sheet1.Range[xlsRow, iw_rate].ColumnWidth = 12;

                    //xlsCol += 1;
                    //ih_rate = xlsCol;
                    //sheet1.Range[xlsRow, ih_rate].Text = "Holiday Rate";
                    //sheet1.Range[xlsRow, ih_rate].ColumnWidth = 12;

                    xlsCol += 1;
                    iAmount = xlsCol;
                    sheet1.Range[xlsRow, iAmount].Text = "Amount";
                    sheet1.Range[xlsRow, iAmount].ColumnWidth = 12;
                }

                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                sheet1.Range[xlsRow, isl, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;


                xlsRow++;

                #endregion ------------------Column Header------------------


                int startXlsRow = xlsRow;
                for (int i = 0; i < dtHourlyOffDutyTag.Rows.Count; i++)
                {
                    #region ----------------------Data-----------------------  
                    string yot = string.Empty;
                    double nwRate = 0;
                    OTOverstay = 0.00;
                    OTOverstay1 = 0.00;
                    OTOverstay2 = 0.00;

                    try
                    {
                        if (dtHourlyOffDutyTag.Rows[i]["EmployeeCode"].ToString() == "6051")
                        {

                        }


                        #region OT Rate

                        string _empid = dtHourlyOffDutyTag.Rows[i]["systemid"].ToString();
                        string _daytype = dtHourlyOffDutyTag.Rows[i]["daytype"].ToString();
                        //if(_empid== "2001587")
                        //{
                        //    //if(conv dtHourlyOffDutyTag.Rows[i]["WorkDate"].ToString()=Convert.ToDateTime("04-jul-2020"))
                        //    var kk = dtHourlyOffDutyTag.Rows[i]["WorkDate"].ToString();
                        if (CheckBox)
                        {
                            //GetFormula(dsOTPolicy, dsSStructure, _currencyId, _empid, _daytype, out nwRate);
                            ///GenerateDic(dsOTPolicy, dsSStructure, _currencyId,out dic);
                            try
                            {
                                if (_daytype.ToUpper() == "W")
                                {
                                    nwRate = dicW[_empid];
                                }
                                else if (_daytype.ToUpper() == "H")
                                {
                                    nwRate = dicH[_empid];
                                }
                                else
                                {
                                    nwRate = dicNW[_empid];
                                }
                            }
                            catch (Exception ex)
                            {


                            }

                        }
                        //}
                        #endregion

                        oru.GetOT(dtHourlyOffDutyTag.Rows[i]["OTConsiderOn"].ToString(), dtHourlyOffDutyTag.Rows[i]["TotalOT"].ToString(), out yot);
                        sheet1.Range[xlsRow, isl].Text = SLNo.ToString();
                        sheet1.Range[xlsRow, iName].Text = dtHourlyOffDutyTag.Rows[i]["EmployeeName"].ToString();
                        sheet1.Range[xlsRow, iEmployeeCode].Text = dtHourlyOffDutyTag.Rows[i]["EmployeeCode"].ToString();
                        sheet1.Range[xlsRow, iDOJ].Text = dtHourlyOffDutyTag.Rows[i]["DOJ"].ToString();
                        sheet1.Range[xlsRow, iDesignation].Text = dtHourlyOffDutyTag.Rows[i]["Designation"].ToString();
                        sheet1.Range[xlsRow, iDepartment].Text = dtHourlyOffDutyTag.Rows[i]["Department"].ToString();
                        sheet1.Range[xlsRow, iSection].Text = dtHourlyOffDutyTag.Rows[i]["Section"].ToString();
                        sheet1.Range[xlsRow, iSubSection].Text = dtHourlyOffDutyTag.Rows[i]["SubSection"].ToString();
                        sheet1.Range[xlsRow, iLine].Text = dtHourlyOffDutyTag.Rows[i]["Line"].ToString();
                        sheet1.Range[xlsRow, iWorkDate].Text = dtHourlyOffDutyTag.Rows[i]["WorkDate"].ToString();

                        dsSStructure.Tables[0].DefaultView.RowFilter = "EmpInfoSystemId = '" + dtHourlyOffDutyTag.Rows[i]["systemid"].ToString() + @"' AND HeadCategory = 'Basic'";
                        var basic = clsStaticInfo.dbl(dsSStructure.Tables[0].DefaultView[0]["Amount"].ToString());
                        sheet1.Range[xlsRow, iBasicHead].Number = basic;
                        //sheet1.Range[xlsRow, iInTime].NumberFormat = "hh:mm AM/PM";
                        sheet1.Range[xlsRow, iInTime].DateTime = Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["InTimeShow"].ToString());
                        sheet1.Range[xlsRow, iInTime].NumberFormat = "hh:mm AM/PM";

                        //sheet1.Range[xlsRow, iOutTime].NumberFormat = "hh:mm AM/PM";
                        //sheet1.Range[xlsRow, iOutTime].Text = Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["OutTime"].ToString()).ToString("hh:mm tt", CultureInfo.InvariantCulture);
                        ////sheet1.Range[xlsRow, iInTime].NumberFormat = "hh:mm AM/PM";
                        //sheet1.Range[xlsRow, iInTime].Text = Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["InTime"].ToString()).ToString("hh:mm tt", CultureInfo.InvariantCulture);
                        #region OutTime Modification
                        if (dtHourlyOffDutyTag.Rows[i]["OutTimeShow"].ToString() != "")
                        {

                            DateTime NewRealOutTime;
                            string TakeDate = Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["WorkDate"].ToString().Trim()).ToString("dd-MMM-yyyy");
                            string ot = Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["ShiftOutTime"].ToString().Trim()).ToString("hh:mm tt");

                            //check night shift
                            string _sOUTtime = TakeDate + " " + ot;
                            string _sINtime = TakeDate + " " + Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["ShiftInTime"].ToString().Trim()).ToString("hh:mm tt");
                            if (Convert.ToDateTime(_sOUTtime) < Convert.ToDateTime(_sINtime))
                            {
                                TakeDate = Convert.ToDateTime(TakeDate).AddDays(1).ToString("dd-MMM-yyyy");
                            }

                            string TateandTime = TakeDate + " " + ot;
                            int minutesadd = Convert.ToInt32(dtHourlyOffDutyTag.Rows[i]["MaxOTPerDay"].ToString().Trim()) + Convert.ToInt32(dtHourlyOffDutyTag.Rows[i]["MaxExtraOTPerDay"].ToString().Trim());
                            DateTime NewOutTime = Convert.ToDateTime(TateandTime).AddMinutes(minutesadd);
                            DateTime RealOutTime = Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["OutTimeShow"].ToString().Trim());

                            if (Convert.ToDateTime(RealOutTime) > Convert.ToDateTime(NewOutTime))
                            {
                                //long WorkDateTickCount = Convert.ToDateTime(Convert.ToDateTime(dvBioDvAC[i]["PDate"].ToString()).ToString("dd-MMM-yyyy")).Ticks;
                                //int EmployeeSystemId = (int)Convert.ToInt64(dvBioDvAC[i]["SystemId"].ToString());

                                long WorkDateTickCount = Convert.ToInt64(Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["WorkDate"].ToString()).ToString("yyMMddHHmmss"));
                                int EmployeeSystemId = (int)Convert.ToInt64(dtHourlyOffDutyTag.Rows[i]["EmployeeCodeNumeric"].ToString());

                                WorkDateTickCount += EmployeeSystemId;

                                Random rnd = new Random((int)(WorkDateTickCount));
                                int RandomMinutes = rnd.Next(0, 15);
                                NewRealOutTime = Convert.ToDateTime(NewOutTime).AddMinutes(RandomMinutes);
                            }

                            else
                            {
                                NewRealOutTime = Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["OutTimeShow"].ToString().Trim());
                            }
                            DateTime RandomTime = Convert.ToDateTime(NewRealOutTime);
                            DateTime ShiftTime = Convert.ToDateTime(TateandTime);
                            TimeSpan span = RandomTime - ShiftTime;
                            double totalMinutes = span.TotalMinutes;

                            sheet1.Range[xlsRow, iOutTime].NumberFormat = "hh:mm AM/PM";
                            sheet1.Range[xlsRow, iOutTime].DateTime = NewRealOutTime;
                            sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        }
                        #endregion
                        string overstay = string.Empty;
                        if (dtHourlyOffDutyTag.Rows[i]["OutTimeShow"].ToString() != "")
                        {
                            DateTime NewRealOutTime;
                            string TakeDate = Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["WorkDate"].ToString().Trim()).ToString("dd-MMM-yyyy");
                            string ot = Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["ShiftOutTime"].ToString().Trim()).ToString("hh:mm tt");

                            //check night shift
                            string _sOUTtime = TakeDate + " " + ot;
                            string _sINtime = TakeDate + " " + Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["ShiftInTime"].ToString().Trim()).ToString("hh:mm tt");
                            if (Convert.ToDateTime(_sOUTtime) < Convert.ToDateTime(_sINtime))
                            {
                                TakeDate = Convert.ToDateTime(TakeDate).AddDays(1).ToString("dd-MMM-yyyy");
                            }

                            string TateandTime = TakeDate + " " + ot;
                            //int minutesadd = Convert.ToInt32(dtHourlyOffDutyTag.Rows[i]["MaxOTPerDay"].ToString().Trim());
                            int minutesadd = Convert.ToInt32(dtHourlyOffDutyTag.Rows[0]["MaxOTPerDay"].ToString().Trim()) + Convert.ToInt32(dtHourlyOffDutyTag.Rows[0]["MaxExtraOTPerDay"].ToString().Trim());
                            DateTime NewOutTime = Convert.ToDateTime(TateandTime).AddMinutes(minutesadd);
                            DateTime RealOutTime = Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["OutTimeShow"].ToString().Trim());
                            double totalMinutes;

                            if (Convert.ToDateTime(RealOutTime) >= Convert.ToDateTime(NewOutTime))
                            {
                                long WorkDateTickCount = Convert.ToDateTime(Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["WorkDate"].ToString()).ToString("dd-MMM-yyyy")).Ticks;
                                int EmployeeSystemId = (int)Convert.ToInt64(dtHourlyOffDutyTag.Rows[i]["SystemId"].ToString());
                                WorkDateTickCount += EmployeeSystemId;

                                Random rnd = new Random((int)(WorkDateTickCount));
                                int RandomMinutes = rnd.Next(0, 15);
                                NewRealOutTime = Convert.ToDateTime(NewOutTime).AddMinutes(RandomMinutes);
                                DateTime RandomTime = Convert.ToDateTime(NewRealOutTime);
                                DateTime ShiftTime = Convert.ToDateTime(TateandTime);
                                TimeSpan span = RandomTime - ShiftTime;

                                if (Convert.ToInt32(dtHourlyOffDutyTag.Rows[0]["MaxOTPerDay"].ToString().Trim()) > 0)
                                {
                                    totalMinutes = span.TotalMinutes;
                                    oru.GetOT(dtHourlyOffDutyTag.Rows[0]["OTConsiderOn"].ToString(), minutesadd.ToString(), out overstay);
                                    OTOverstay1 += clsStaticInfo.dbl(minutesadd);
                                    OTOverstay1 = OTOverstay1 - Convert.ToInt32(dtHourlyOffDutyTag.Rows[i]["MaxOTPerDay"].ToString().Trim());
                                }
                            }
                            else
                            {
                                NewRealOutTime = Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["OutTimeShow"].ToString().Trim());
                                oru.GetOT(dtHourlyOffDutyTag.Rows[0]["OTConsiderOn"].ToString(), dtHourlyOffDutyTag.Rows[i]["TotalOT"].ToString(), out overstay);
                                OTOverstay2 += clsStaticInfo.dbl(dtHourlyOffDutyTag.Rows[i]["TotalOT"].ToString());
                                OTOverstay2 = OTOverstay2 - Convert.ToInt32(dtHourlyOffDutyTag.Rows[i]["MaxOTPerDay"].ToString().Trim());

                            }

                        }
                        OTOverstay += clsStaticInfo.dbl(OTOverstay1 + OTOverstay2);

                        if (OTOverstay < 0)
                        {
                            OTOverstay = 0.00;
                        }

                        string GTotalOt = string.Empty;
                        oru.GetOT(dtHourlyOffDutyTag.Rows[0]["OTConsiderOn"].ToString(), OTOverstay.ToString(), out GTotalOt);

                        sheet1.Range[xlsRow, iTotalHr].Text = GTotalOt;
                        //sheet1.Range[xlsRow, iTotalHrReal].Number = clsStaticInfo.dbl(dtHourlyOffDutyTag.Rows[i]["TotalOTH"].ToString());
                        if (CheckBox)
                        {
                            sheet1.Range[xlsRow, inw_rate].Number = nwRate;
                            double amt = (OTOverstay / 60) * nwRate;
                            sheet1.Range[xlsRow, iAmount].Number = amt;
                        }

                        xlsRow++;
                        SLNo++;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                sheet1.Range[startXlsRow, iTotalHr, xlsRow, iTotalHr].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, iOutTime].Text = "Total";

                sheet1.Range[xlsRow, iOutTime, xlsRow, iOutTime].CellStyle.Font.Bold = true;

                string totalOT = "";
                string totalOTCal = "";

                object sumObject;
                sumObject = dtHourlyOffDutyTag.Compute("Sum(TotalOT)", "");
                totalOT = sumObject.ToString();
                oru.GetOT(dtHourlyOffDutyTag.Rows[0]["OTConsiderOn"].ToString(), totalOT, out totalOTCal);

                sheet1.Range[xlsRow, iTotalHr].Text = totalOTCal;

                sheet1.Range[xlsRow, iTotalHr, xlsRow, iTotalHr].CellStyle.Font.Bold = true;

                if (CheckBox)
                {
                    sheet1.Range[xlsRow, iAmount].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(iAmount) + startXlsRow + ":" + clsStaticInfo.GetxlsCol(iAmount) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, iOutTime, xlsRow, iAmount].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, iOutTime, xlsRow, iAmount].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, iAmount, xlsRow, iAmount].CellStyle.Font.Bold = true;
                }
                else
                {
                    sheet1.Range[xlsRow, iOutTime, xlsRow, iTotalHr].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, iOutTime, xlsRow, iTotalHr].BorderAround(ExcelLineStyle.Hair);
                }


                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;
                int sheetEndXlsRow = xlsRow;
                #endregion ----------------------Data-----------------------

                #region ******************Report Header******************



                xlsRow = 1;
                xlsCol = 3;
                try
                {
                    if (companyLogo != null)
                    {

                        double totalWidth = sheet1.GetColumnWidth(isl) + sheet1.GetColumnWidth(iEmployeeCode);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);
                        //pic.Height = 80;
                        //pic.Width = 220;
                    }
                }
                catch (Exception ex)
                {
                }

                FactoryName = string.Empty;

                string FactoryAddress = string.Empty;

                if (dtCmp.Rows.Count > 0)
                {
                    CmpName = dtCmp.Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    FactoryName = dtFactory.Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryAddress = dtFactory.Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Excess OT From " + FromDate + " TO " + ToDate;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = false;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "Excess OT";
                #endregion Page Setup

                #endregion  Individual Daily OT
                //string fPath = filePath;
                //if (string.IsNullOrEmpty(filePath))
                //{
                //    filePath = HostingEnvironment.MapPath("~/") + "TempIndividualDailyOT.xlsx";
                //}

                //workbook.SaveAs(filePath);
                //workbook = application.Workbooks.Open(filePath);
                //IWorksheet worksheet = workbook.Worksheets[0];
                //try
                //{

                //    #region PivotSheet1
                //    IWorksheet pivotSheet = workbook.Worksheets[1];
                //    pivotSheet.Name = "Summary";


                //    try
                //    {

                //        if (companyLogo != null)
                //        {
                //            double totalWidth = pivotSheet.GetColumnWidth(isl) + pivotSheet.GetColumnWidth(iEmployeeCode);
                //            int totalWidthPixel = (int)(totalWidth * 7.5);
                //            int totalheight = (int)((pivotSheet.GetRowHeight(1) + pivotSheet.GetRowHeight(2) + pivotSheet.GetRowHeight(3) + pivotSheet.GetRowHeight(3)) * 1.50);

                //            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                //            IPictureShape pic = null;

                //            pic = pivotSheet.Pictures.AddPicture(1, 1, companyLogo);

                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //    }

                //    #region Report Header
                //    xlsRow = 1;
                //    xlsCol = 1;


                //    pivotSheet.Range[xlsRow, 3].Text = CmpName;
                //    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                //    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                //    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                //    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                //    xlsRow += 1;

                //    pivotSheet.Range[xlsRow, 3].Text = FactoryName;
                //    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                //    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                //    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;



                //    xlsRow += 1;

                //    pivotSheet.Range[xlsRow, 3].Text = FactoryAddress;
                //    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //    //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                //    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                //    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                //    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //    xlsRow += 1;
                //    pivotSheet.Range[xlsRow, 3].Text = "Excess OT From " + FromDate + " TO " + ToDate;
                //    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                //    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                //    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                //    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //    #endregion

                //    pivotSheet.Range[xlsRow + 1, 1].Text = "Report Ref No:";
                //    pivotSheet.Range[xlsRow + 1, 1].CellStyle.Font.Size = 10;
                //    pivotSheet.Range[xlsRow + 1, 1].RowHeight = 20;
                //    pivotSheet.Range[xlsRow + 1, 1].CellStyle.Font.Bold = true;


                //    IPivotCache cache = workbook.PivotCaches.Add(worksheet["A6:" + oru.GetColumnNameForXls(endXlsCol) + (sheetEndXlsRow - 1)]);

                //    #region Second Pivot table
                //    pivotSheet.Range[xlsRow + 2, 1].Text = "Total Manpower in OverTime";
                //    pivotSheet.Range[xlsRow + 2, 1].CellStyle.Font.Size = 12;
                //    pivotSheet.Range[xlsRow + 2, 1].CellStyle.Font.Bold = true;

                //    IPivotTable pivotTable2 = pivotSheet.PivotTables.Add("PivotTable2", pivotSheet["A8"], cache);

                //    //Add Pivot table fields (Row and Column fields)
                //    pivotTable2.Fields[iDepartment - 1].Axis = PivotAxisTypes.Row;
                //    pivotTable2.Fields[iSection - 1].Axis = PivotAxisTypes.Row;
                //    pivotTable2.Fields[iTotalHrReal - 2].Axis = PivotAxisTypes.Column;


                //    IPivotTable pivotTable2_1 = pivotSheet.PivotTables["PivotTable2"];
                //    pivotTable2_1.Options.RowLayout = PivotTableRowLayout.Tabular;
                //    pivotTable2_1.Options.ShowDrillIndicators = false;

                //    pivotTable2_1.DisplayFieldCaptions = true;


                //    //Add data field
                //    IPivotField field2 = pivotTable2_1.Fields[iTotalHrReal - 1];
                //    pivotTable2_1.DataFields.Add(field2, "Total Employees", PivotSubtotalTypes.Count);
                //    pivotTable2_1.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;


                //    int totalColumns = pivotTable2_1.RowFields.Count + pivotTable2_1.ColumnFields.Count;
                //    for (int i = 0; i < pivotTable2_1.ColumnFields.Count; i++)
                //    {
                //        totalColumns += pivotTable2_1.ColumnFields[i].Items.Count;
                //    }

                //    int lastCloumn = totalColumns;

                //    #endregion


                //    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].Text = "OverTime Hours Summary";
                //    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Size = 12;
                //    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Bold = true;



                //    //Create "PivotTable1" with the cache at the specified range
                //    IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTable1", pivotSheet[clsStaticInfo.GetxlsCol(lastCloumn + 2) + "8"], cache);

                //    //Add Pivot table fields (Row and Column fields)
                //    pivotTable.Fields[iDepartment - 1].Axis = PivotAxisTypes.Row;
                //    pivotTable.Fields[iSection - 1].Axis = PivotAxisTypes.Row;
                //    pivotTable.Fields[iTotalHrReal - 2].Axis = PivotAxisTypes.Column;


                //    IPivotTable pivotTable1 = pivotSheet.PivotTables["PivotTable1"];
                //    pivotTable1.Options.RowLayout = PivotTableRowLayout.Tabular;
                //    pivotTable1.Options.ShowDrillIndicators = false;

                //    pivotTable1.DisplayFieldCaptions = true;
                //    pivotTable1.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;


                //    //Add data field
                //    IPivotField field = pivotTable.Fields[iTotalHrReal - 1];
                //    pivotTable.DataFields.Add(field, "Total Hours", PivotSubtotalTypes.Sum);




                //    pivotSheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                //    pivotSheet.IsGridLinesVisible = false;

                //    #endregion
                //    if (string.IsNullOrEmpty(fPath))
                //    {
                //        #region PivotSheet2

                //        IWorksheet pivotSheet2 = workbook.Worksheets[2];
                //        pivotSheet2.Name = "OT SUMMARY Sec";
                //        pivotSheet2.IsGridLinesVisible = false;

                //        try
                //        {
                //            if (companyLogo != null)
                //            {
                //                double totalWidth = pivotSheet2.GetColumnWidth(isl) + pivotSheet2.GetColumnWidth(iEmployeeCode);
                //                int totalWidthPixel = (int)(totalWidth * 7.5);
                //                int totalheight = (int)((pivotSheet2.GetRowHeight(1) + pivotSheet2.GetRowHeight(2) + pivotSheet2.GetRowHeight(3) + pivotSheet2.GetRowHeight(3)) * 1.50);

                //                companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                //                IPictureShape pic = null;
                //                pic = pivotSheet2.Pictures.AddPicture(1, 1, companyLogo);

                //            }
                //        }
                //        catch (Exception ex)
                //        {
                //        }

                //        #region Report Header
                //        xlsRow = 1;
                //        xlsCol = 1;


                //        pivotSheet2.Range[xlsRow, 3].Text = CmpName;
                //        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //        pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                //        pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                //        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                //        pivotSheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //        pivotSheet2.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                //        xlsRow += 1;

                //        pivotSheet2.Range[xlsRow, 3].Text = FactoryName;
                //        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //        pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                //        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                //        pivotSheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //        pivotSheet2.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                //        xlsRow += 1;

                //        pivotSheet2.Range[xlsRow, 3].Text = FactoryAddress;
                //        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //        pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                //        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                //        pivotSheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //        pivotSheet2.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                //        xlsRow += 1;
                //        pivotSheet2.Range[xlsRow, 3].Text = "OT Summary From " + FromDate + " TO " + ToDate;
                //        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //        pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                //        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                //        pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                //        pivotSheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //        pivotSheet2.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                //        #endregion
                //        pivotSheet2.Range[xlsRow + 1, 1].Text = "Report Ref No:";
                //        pivotSheet2.Range[xlsRow + 1, 1].CellStyle.Font.Size = 10;
                //        pivotSheet2.Range[xlsRow + 1, 1].RowHeight = 20;
                //        pivotSheet2.Range[xlsRow + 1, 1].CellStyle.Font.Bold = true;

                //        IPivotTable pivotTableSec = pivotSheet2.PivotTables.Add("PivotTableSEC", pivotSheet2["A7"], cache);

                //        //Add Pivot table fields (Row and Column fields)
                //        pivotTableSec.Fields[iDepartment - 1].Axis = PivotAxisTypes.Row;
                //        pivotTableSec.Fields[iSection - 1].Axis = PivotAxisTypes.Row;
                //        pivotTableSec.Fields[iTotalHrReal - 2].Axis = PivotAxisTypes.Column;


                //        IPivotTable pivotTableSec_1 = pivotSheet2.PivotTables["PivotTableSEC"];
                //        pivotTableSec_1.Options.RowLayout = PivotTableRowLayout.Tabular;
                //        pivotTableSec_1.Options.ShowDrillIndicators = false;

                //        pivotTableSec_1.DisplayFieldCaptions = true;


                //        //Add data field
                //        IPivotField fieldSec = pivotTableSec_1.Fields[iTotalHrReal - 1];
                //        pivotTableSec_1.DataFields.Add(fieldSec, "Hours", PivotSubtotalTypes.Sum);

                //        IPivotField fieldSec2 = pivotTableSec_1.Fields[iTotalHrReal - 1];
                //        pivotTableSec_1.DataFields.Add(fieldSec2, "Employees", PivotSubtotalTypes.Count);

                //        if (CheckBox)
                //        {
                //            IPivotField fieldSec3 = pivotTableSec_1.Fields[iAmount - 1];
                //            pivotTableSec_1.DataFields.Add(fieldSec3, "Amount", PivotSubtotalTypes.Sum);
                //        }

                //        pivotTableSec_1.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;



                //        pivotSheet2.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                //        #endregion

                //        #region PivotSheet3

                //        IWorksheet pivotSheet3 = workbook.Worksheets[3];
                //        pivotSheet3.Name = "OT SUMMARY Dept.";
                //        pivotSheet3.IsGridLinesVisible = false;

                //        try
                //        {

                //            if (companyLogo != null)
                //            {
                //                double totalWidth = pivotSheet3.GetColumnWidth(1) + pivotSheet3.GetColumnWidth(2);
                //                int totalWidthPixel = (int)(totalWidth * 7.5);
                //                int totalheight = (int)((pivotSheet3.GetRowHeight(1) + pivotSheet3.GetRowHeight(2) + pivotSheet3.GetRowHeight(3) + pivotSheet3.GetRowHeight(3)) * 1.50);

                //                companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                //                IPictureShape pic = null;

                //                pic = pivotSheet3.Pictures.AddPicture(1, 1, companyLogo);

                //            }
                //        }
                //        catch (Exception ex)
                //        {
                //        }

                //        #region Report Header
                //        xlsRow = 1;
                //        xlsCol = 1;


                //        pivotSheet3.Range[xlsRow, 3].Text = CmpName;
                //        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //        pivotSheet3.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                //        pivotSheet3.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                //        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                //        pivotSheet3.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //        pivotSheet3.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                //        xlsRow += 1;

                //        pivotSheet3.Range[xlsRow, 3].Text = FactoryName;
                //        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //        pivotSheet3.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                //        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                //        pivotSheet3.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //        pivotSheet3.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                //        xlsRow += 1;

                //        pivotSheet3.Range[xlsRow, 3].Text = FactoryAddress;
                //        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //        pivotSheet3.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                //        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                //        pivotSheet3.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //        pivotSheet3.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                //        xlsRow += 1;
                //        pivotSheet3.Range[xlsRow, 3].Text = "OT Summary From " + FromDate + " TO " + ToDate;
                //        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //        pivotSheet3.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                //        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                //        pivotSheet3.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                //        pivotSheet3.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                //        pivotSheet3.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                //        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                //        #endregion
                //        pivotSheet3.Range[xlsRow + 1, 1].Text = "Report Ref No:";
                //        pivotSheet3.Range[xlsRow + 1, 1].CellStyle.Font.Size = 10;
                //        pivotSheet3.Range[xlsRow + 1, 1].RowHeight = 20;
                //        pivotSheet3.Range[xlsRow + 1, 1].CellStyle.Font.Bold = true;

                //        IPivotTable pivotTableDept = pivotSheet3.PivotTables.Add("PivotTableDept", pivotSheet2["A7"], cache);

                //        //Add Pivot table fields (Row and Column fields)
                //        pivotTableDept.Fields[iDepartment - 1].Axis = PivotAxisTypes.Row;
                //        //pivotTableDept.Fields[iSection - 1].Axis = PivotAxisTypes.Row;
                //        pivotTableDept.Fields[iTotalHrReal - 2].Axis = PivotAxisTypes.Column;


                //        IPivotTable pivotTableDept_1 = pivotSheet3.PivotTables["PivotTableDept"];
                //        pivotTableDept_1.Options.RowLayout = PivotTableRowLayout.Tabular;
                //        pivotTableDept_1.Options.ShowDrillIndicators = false;

                //        pivotTableDept_1.DisplayFieldCaptions = true;


                //        //Add data field
                //        IPivotField fieldDept = pivotTableDept_1.Fields[iTotalHrReal - 1];
                //        pivotTableDept_1.DataFields.Add(fieldDept, "Hours", PivotSubtotalTypes.Sum);

                //        IPivotField fieldDept2 = pivotTableDept_1.Fields[iTotalHrReal - 1];
                //        pivotTableDept_1.DataFields.Add(fieldDept2, "Employees", PivotSubtotalTypes.Count);

                //        if (CheckBox)
                //        {
                //            IPivotField fieldDept3 = pivotTableDept_1.Fields[iAmount - 1];
                //            pivotTableDept_1.DataFields.Add(fieldDept3, "Amount", PivotSubtotalTypes.Sum);
                //        }
                //        pivotTableDept_1.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;



                //        pivotSheet3.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                //        #endregion
                //    }

                //}
                //catch (Exception)
                //{

                //}
                //try
                //{
                //    worksheet.ShowColumn(iTotalHrReal, false);
                //    if (string.IsNullOrEmpty(fPath))
                //    {
                //        System.IO.File.Delete(filePath);
                //    }
                //}
                //catch (Exception)
                //{
                //}
                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IWorkbook GetEMIndividualDailyOT(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName, string FromDate, string ToDate, string OTDuration, bool CheckBox, string OTfinal, string filePath = "")
        {

            #region declare
            clsReport objRpt = null;
            clsReport objRptSR = null;
            ReportUtility oru = new ReportUtility();
            //DataSet dsHourlyOffDutyTag = null;
            DataTable dtHourlyOffDutyTag = null;
            //DataSet dsCmp = null;
            //DataSet dsFactory = null;
            DataView dvOT = null;

            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string OTConsiderOn = string.Empty;
            var OTOverstay1 = 0.00;
            var OTOverstay2 = 0.00;
            var OTOverstay = 0.00;
            var OTMinimum1 = 0.00;
            var OTMinimum2 = 0.00;
            var OTMinimum = 0.00;
            DataSet dsOTPolicy = null;//
            DataSet dsSStructure = null;
            string _currencyId = string.Empty;
            Dictionary<string, double> dicNW = null;
            Dictionary<string, double> dicW = null;
            Dictionary<string, double> dicH = null;
            #endregion
            try
            {
                ExcelEngine excelEngine = null;
                IApplication application = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2013;

                IWorkbook workbook = application.Workbooks.Create(1);

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), CompanyId + ".jpg");  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                #endregion
                objRpt = new clsReport();

                objRptSR = new clsReport(_sqlRepository);

                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                #region OT Rate
                if (CheckBox)
                {
                    DataSet dsCurrency = null;
                    clsOTCalculation otc = new clsOTCalculation();
                    otc.LoadOverTimePolicy(PlantId, FromDate, ToDate, out dsOTPolicy);
                    otc.LoadSalaryStructure(PlantId, FromDate, ToDate, out dsSStructure);

                    clsSalaryInfo objSal = new clsSalaryInfo();
                    objSal.GetLocalCurrency(CompanyGroupId, PlantId, out dsCurrency);
                    if (dsCurrency.Tables[0].Rows.Count > 0)
                    {
                        _currencyId = "" + dsCurrency.Tables[0].Rows[0]["LocalCurrency"].ToString().Trim();
                    }
                    else
                    {
                        throw new Exception("No currency found...");
                    }

                    GenerateDic(dsOTPolicy, dsSStructure, _currencyId, out dicNW, out dicW, out dicH);
                }
                #endregion

                dtHourlyOffDutyTag = objRptSR.GetCIndividualDailyOT(FromDate, ToDate, OTDuration, OTfinal, PlantId, CompanyId, CompanyGroupId);


                var dtCmp = objRptSR.SelectedCompanyDT(PlantId);

                var dtFactory = objRptSR.SelectedPlantDT(PlantId);
                #region Variable
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                var iName = 0;
                var iEmployeeCode = 0;
                var iSubSection = 0;
                var iSection = 0;
                var iTotalHr = 0;
                var iTotalHrReal = 0;
                var iOutTime = 0;
                var iInTime = 0;

                var iLine = 0;
                var iDOJ = 0;
                var iDepartment = 0;
                var iDesignation = 0;
                var isl = 0;
                var iWorkDate = 0;
                var SLNo = 1;
                #endregion

                if (dtHourlyOffDutyTag.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }
                #region Hourly Ot
                //workbook = application.Workbooks.Create(4);
                IWorksheet sheet1 = null;
                IWorksheet sheet2 = null;
                IWorksheet sheet3 = null;
                IWorksheet sheet4 = null;

                sheet1 = workbook.Worksheets[0];
                sheet2 = workbook.Worksheets[1];
                sheet3 = workbook.Worksheets[2];
                sheet4 = workbook.Worksheets[3];


                xlsRow = 6;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No:";
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow - 1, 1].RowHeight = 20;
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Bold = true;

                #region ------------------Column Header------------------
                isl = xlsCol;
                sheet1.Range[xlsRow, isl].Text = "SL";
                sheet1.Range[xlsRow, isl].ColumnWidth = 7;

                xlsCol += 1;
                iEmployeeCode = xlsCol;
                sheet1.Range[xlsRow, iEmployeeCode].Text = "Emp Code";
                sheet1.Range[xlsRow, iEmployeeCode].ColumnWidth = 10;

                xlsCol += 1;
                iName = xlsCol;
                sheet1.Range[xlsRow, iName].Text = "Emp Name";
                sheet1.Range[xlsRow, iName].ColumnWidth = 25;



                xlsCol += 1;
                iDOJ = xlsCol;
                sheet1.Range[xlsRow, iDOJ].Text = "DOJ";
                sheet1.Range[xlsRow, iDOJ].ColumnWidth = 20;

                xlsCol += 1;
                iDepartment = xlsCol;
                sheet1.Range[xlsRow, iDepartment].Text = "Department";
                sheet1.Range[xlsRow, iDepartment].ColumnWidth = 25;

                xlsCol += 1;
                iDesignation = xlsCol;
                sheet1.Range[xlsRow, iDesignation].Text = "Designation";
                sheet1.Range[xlsRow, iDesignation].ColumnWidth = 25;

                xlsCol += 1;
                iSection = xlsCol;
                sheet1.Range[xlsRow, iSection].Text = "Section";
                sheet1.Range[xlsRow, iSection].ColumnWidth = 14;

                xlsCol += 1;
                iSubSection = xlsCol;
                sheet1.Range[xlsRow, iSubSection].Text = "Sub Section";
                sheet1.Range[xlsRow, iSubSection].ColumnWidth = 16;

                xlsCol += 1;
                iLine = xlsCol;
                sheet1.Range[xlsRow, iLine].Text = "Line";
                sheet1.Range[xlsRow, iLine].ColumnWidth = 12;
                xlsCol += 1;
                iWorkDate = xlsCol;
                sheet1.Range[xlsRow, iWorkDate].Text = "Work Date";
                sheet1.Range[xlsRow, iWorkDate].ColumnWidth = 12;

                xlsCol += 1;
                iInTime = xlsCol;
                sheet1.Range[xlsRow, iInTime].Text = "In Time";
                sheet1.Range[xlsRow, iInTime].ColumnWidth = 12;

                xlsCol += 1;
                iOutTime = xlsCol;
                sheet1.Range[xlsRow, iOutTime].Text = "Out Time";
                sheet1.Range[xlsRow, iOutTime].ColumnWidth = 12;

                xlsCol += 1;
                iTotalHr = xlsCol;
                sheet1.Range[xlsRow, iTotalHr].Text = "OT Hours";
                sheet1.Range[xlsRow, iTotalHr].ColumnWidth = 15;
                xlsCol += 1;
                int irealTotalHr = xlsCol;
                sheet1.Range[xlsRow, irealTotalHr].Text = "Real OT Hours";
                sheet1.Range[xlsRow, irealTotalHr].ColumnWidth = 15;
                //xlsCol += 1;
                //iTotalHrReal = xlsCol;
                //sheet1.Range[xlsRow, iTotalHrReal].Text = "Total(Hours)";
                //sheet1.Range[xlsRow, iTotalHrReal].ColumnWidth = 15;

                xlsCol += 1;
                int iBasicHead = xlsCol;
                sheet1.Range[xlsRow, iBasicHead].Text = "Basic";
                sheet1.Range[xlsRow, iBasicHead].ColumnWidth = 15;

                int inw_rate = 0;
                int iAmount = 0;
                //int iw_rate = 0;
                //int ih_rate = 0;
                if (CheckBox)
                {
                    xlsCol += 1;
                    inw_rate = xlsCol;
                    sheet1.Range[xlsRow, inw_rate].Text = "Rate";
                    sheet1.Range[xlsRow, inw_rate].ColumnWidth = 12;

                   

                    xlsCol += 1;
                    iAmount = xlsCol;
                    sheet1.Range[xlsRow, iAmount].Text = "Amount";
                    sheet1.Range[xlsRow, iAmount].ColumnWidth = 12;
                }

                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                sheet1.Range[xlsRow, isl, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;


                xlsRow++;

                #endregion ------------------Column Header------------------


                int startXlsRow = xlsRow;
                for (int i = 0; i < dtHourlyOffDutyTag.Rows.Count; i++)
                {
                    #region ----------------------Data-----------------------  
                    string yot = string.Empty;
                    double nwRate = 0;
                    OTOverstay = 0.00;
                    OTOverstay1 = 0.00;
                    OTOverstay2 = 0.00;

                    try
                    {
                        if (dtHourlyOffDutyTag.Rows[i]["EmployeeCode"].ToString() == "190328")
                        {
                            if (dtHourlyOffDutyTag.Rows[i]["WorkDate"].ToString() == "08-May-2021")
                            {

                            }
                        }
                        #region OT Rate
                        string _empid = dtHourlyOffDutyTag.Rows[i]["systemid"].ToString();
                        string _daytype = dtHourlyOffDutyTag.Rows[i]["daytype"].ToString();
                        //if(_empid== "2001587")
                        //{
                        //    //if(conv dtHourlyOffDutyTag.Rows[i]["WorkDate"].ToString()=Convert.ToDateTime("04-jul-2020"))
                        //    var kk = dtHourlyOffDutyTag.Rows[i]["WorkDate"].ToString();
                        if (CheckBox)
                        {
                            //GetFormula(dsOTPolicy, dsSStructure, _currencyId, _empid, _daytype, out nwRate);
                            ///GenerateDic(dsOTPolicy, dsSStructure, _currencyId,out dic);
                            try
                            {
                                if (_daytype.ToUpper() == "W")
                                {
                                    nwRate = dicW[_empid];
                                }
                                else if (_daytype.ToUpper() == "H")
                                {
                                    nwRate = dicH[_empid];
                                }
                                else
                                {
                                    nwRate = dicNW[_empid];
                                }
                            }
                            catch (Exception ex)
                            {


                            }

                        }
                        //}
                        #endregion

                        oru.GetOT(dtHourlyOffDutyTag.Rows[i]["OTConsiderOn"].ToString(), dtHourlyOffDutyTag.Rows[i]["TotalOT"].ToString(), out yot);
                        sheet1.Range[xlsRow, isl].Text = SLNo.ToString();
                        sheet1.Range[xlsRow, iName].Text = dtHourlyOffDutyTag.Rows[i]["EmployeeName"].ToString();
                        sheet1.Range[xlsRow, iEmployeeCode].Text = dtHourlyOffDutyTag.Rows[i]["EmployeeCode"].ToString();
                        sheet1.Range[xlsRow, iDOJ].Text = dtHourlyOffDutyTag.Rows[i]["DOJ"].ToString();
                        sheet1.Range[xlsRow, iDesignation].Text = dtHourlyOffDutyTag.Rows[i]["Designation"].ToString();
                        sheet1.Range[xlsRow, iDepartment].Text = dtHourlyOffDutyTag.Rows[i]["Department"].ToString();
                        sheet1.Range[xlsRow, iSection].Text = dtHourlyOffDutyTag.Rows[i]["Section"].ToString();
                        sheet1.Range[xlsRow, iSubSection].Text = dtHourlyOffDutyTag.Rows[i]["SubSection"].ToString();
                        sheet1.Range[xlsRow, iLine].Text = dtHourlyOffDutyTag.Rows[i]["Line"].ToString();
                        sheet1.Range[xlsRow, iWorkDate].Text = dtHourlyOffDutyTag.Rows[i]["WorkDate"].ToString();

                        dsSStructure.Tables[0].DefaultView.RowFilter = "EmpInfoSystemId = '" + dtHourlyOffDutyTag.Rows[i]["systemid"].ToString() + @"' AND HeadCategory = 'Basic'";
                        var basic = 0.00;
                        if (dsSStructure.Tables[0].DefaultView.Count > 0)
                        {
                             basic = clsStaticInfo.dbl(dsSStructure.Tables[0].DefaultView[0]["Amount"].ToString());

                        }
                        sheet1.Range[xlsRow, iBasicHead].Number = basic;
                        //sheet1.Range[xlsRow, iInTime].NumberFormat = "hh:mm AM/PM";
                        sheet1.Range[xlsRow, iInTime].DateTime = Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["InTimeShow"].ToString());
                        sheet1.Range[xlsRow, iInTime].NumberFormat = "hh:mm AM/PM";
                        if (dtHourlyOffDutyTag.Rows[i]["DayType"].ToString().Trim() == "W" && Convert.ToBoolean(dtHourlyOffDutyTag.Rows[i]["IsNoPunchOnWeekOffForOTEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dtHourlyOffDutyTag.Rows[i]["IsOTEntitled"].ToString().Trim()) == false)
                        {
                            sheet1.Range[xlsRow, iInTime].Text = "";
                            sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                           
                        }
                        else if (dtHourlyOffDutyTag.Rows[i]["DayType"].ToString().Trim() == "W" && Convert.ToBoolean(dtHourlyOffDutyTag.Rows[i]["IsNoPunchOnWeekOffForOTNotEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dtHourlyOffDutyTag.Rows[i]["IsOTEntitled"].ToString().Trim()) == true)
                        {
                            sheet1.Range[xlsRow, iInTime].Text = "";
                            sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        

                        }
                        else if (dtHourlyOffDutyTag.Rows[i]["DayType"].ToString().Trim() == "H" && Convert.ToBoolean(dtHourlyOffDutyTag.Rows[i]["IsNoPunchOnHolidayForOTEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dtHourlyOffDutyTag.Rows[i]["IsOTEntitled"].ToString().Trim()) == false)
                        {
                            sheet1.Range[xlsRow, iInTime].Text = "";
                            sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                         

                        }
                        else if (dtHourlyOffDutyTag.Rows[i]["DayType"].ToString().Trim() == "H" && Convert.ToBoolean(dtHourlyOffDutyTag.Rows[i]["IsNoPunchOnHolidayForOTNotEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dtHourlyOffDutyTag.Rows[i]["IsOTEntitled"].ToString().Trim()) == true)
                        {
                            sheet1.Range[xlsRow, iInTime].Text = "";
                            sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                           

                        }

                        else if (dtHourlyOffDutyTag.Rows[i]["DayStatus"].ToString().Trim().Contains("LV") || dtHourlyOffDutyTag.Rows[i]["DayStatus"].ToString().Trim() == "W")
                        {
                            sheet1.Range[xlsRow, iInTime].Text = "";
                            sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                          
                        }
                        else
                        {
                            if (dtHourlyOffDutyTag.Rows[i]["InTimeShow"].ToString() != "")
                            {
                                sheet1.Range[xlsRow, iInTime].NumberFormat = "hh:mm AM/PM";
                                sheet1.Range[xlsRow, iInTime].DateTime = Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["InTimeShow"].ToString());
                                sheet1.Range[xlsRow, iInTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iInTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            }
                          
                            else
                            {
                                
                            }

                        }
                        if (dtHourlyOffDutyTag.Rows[i]["DayType"].ToString().Trim() == "W" && Convert.ToBoolean(dtHourlyOffDutyTag.Rows[i]["IsNoPunchOnWeekOffForOTEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dtHourlyOffDutyTag.Rows[i]["IsOTEntitled"].ToString().Trim()) == false)
                        {
                            sheet1.Range[xlsRow, iOutTime].Text = "";
                            sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        }
                        else if (dtHourlyOffDutyTag.Rows[i]["DayType"].ToString().Trim() == "W" && Convert.ToBoolean(dtHourlyOffDutyTag.Rows[i]["IsNoPunchOnWeekOffForOTNotEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dtHourlyOffDutyTag.Rows[i]["IsOTEntitled"].ToString().Trim()) == true)
                        {
                            sheet1.Range[xlsRow, iOutTime].Text = "";
                            sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        }
                        else if (dtHourlyOffDutyTag.Rows[i]["DayType"].ToString().Trim() == "H" && Convert.ToBoolean(dtHourlyOffDutyTag.Rows[i]["IsNoPunchOnHolidayForOTEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dtHourlyOffDutyTag.Rows[i]["IsOTEntitled"].ToString().Trim()) == false)
                        {
                            sheet1.Range[xlsRow, iOutTime].Text = "";
                            sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter; ;
                        }
                        else if (dtHourlyOffDutyTag.Rows[i]["DayType"].ToString().Trim() == "H" && Convert.ToBoolean(dtHourlyOffDutyTag.Rows[i]["IsNoPunchOnHolidayForOTNotEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dtHourlyOffDutyTag.Rows[i]["IsOTEntitled"].ToString().Trim()) == true)
                        {
                            sheet1.Range[xlsRow, iOutTime].Text = "";
                            sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        }

                        else if (dtHourlyOffDutyTag.Rows[i]["DayStatus"].ToString().Trim().Contains("LV") || dtHourlyOffDutyTag.Rows[i]["DayStatus"].ToString().Trim() == "W")
                        {
                            sheet1.Range[xlsRow, iOutTime].Text = "";
                            sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                            sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        }
                        #region OutTime Modification
                        else
                        {
                            if (dtHourlyOffDutyTag.Rows[i]["OutTimeShow"].ToString() != "")
                            {

                                DateTime NewRealOutTime;
                                string TakeDate = Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["WorkDate"].ToString().Trim()).ToString("dd-MMM-yyyy");
                                string ot = Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["ShiftOutTime"].ToString().Trim()).ToString("hh:mm tt");

                                //check night shift
                                string _sOUTtime = TakeDate + " " + ot;
                                string _sINtime = TakeDate + " " + Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["ShiftInTime"].ToString().Trim()).ToString("hh:mm tt");
                                if (Convert.ToDateTime(_sOUTtime) < Convert.ToDateTime(_sINtime))
                                {
                                    TakeDate = Convert.ToDateTime(TakeDate).AddDays(1).ToString("dd-MMM-yyyy");
                                }

                                string TateandTime = TakeDate + " " + ot;
                                //int minutesadd = Convert.ToInt32(dtHourlyOffDutyTag.Rows[i]["MaxOTPerDay"].ToString().Trim()) + Convert.ToInt32(dtHourlyOffDutyTag.Rows[i]["MaxExtraOTPerDay"].ToString().Trim());
                                int minutesadd = Convert.ToInt32(dtHourlyOffDutyTag.Rows[i]["MaxOTPerDay"].ToString().Trim()) + Convert.ToInt32(dtHourlyOffDutyTag.Rows[i]["MaxExtraOTPerDay"].ToString().Trim());

                                DateTime NewOutTime = Convert.ToDateTime(TateandTime).AddMinutes(minutesadd);
                                DateTime RealOutTime = Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["OutTimeShow"].ToString().Trim());

                                if (Convert.ToDateTime(RealOutTime) > Convert.ToDateTime(NewOutTime))
                                {
                                    //long WorkDateTickCount = Convert.ToDateTime(Convert.ToDateTime(dvBioDvAC[i]["PDate"].ToString()).ToString("dd-MMM-yyyy")).Ticks;
                                    //int EmployeeSystemId = (int)Convert.ToInt64(dvBioDvAC[i]["SystemId"].ToString());

                                    long WorkDateTickCount = Convert.ToInt64(Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["WorkDate"].ToString()).ToString("yyMMddHHmmss"));
                                    int EmployeeSystemId = (int)Convert.ToInt64(dtHourlyOffDutyTag.Rows[i]["EmployeeCodeNumeric"].ToString());

                                    WorkDateTickCount += EmployeeSystemId;

                                    Random rnd = new Random((int)(WorkDateTickCount));
                                    int RandomMinutes = rnd.Next(0, 15);
                                    NewRealOutTime = Convert.ToDateTime(NewOutTime).AddMinutes(RandomMinutes);
                                }

                                else
                                {
                                    NewRealOutTime = Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["OutTimeShow"].ToString().Trim());
                                }
                                DateTime RandomTime = Convert.ToDateTime(NewRealOutTime);
                                DateTime ShiftTime = Convert.ToDateTime(TateandTime);
                                TimeSpan span = RandomTime - ShiftTime;
                                double totalMinutes = span.TotalMinutes;

                                sheet1.Range[xlsRow, iOutTime].NumberFormat = "hh:mm AM/PM";
                                sheet1.Range[xlsRow, iOutTime].DateTime = NewRealOutTime;
                                sheet1.Range[xlsRow, iOutTime].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                                sheet1.Range[xlsRow, iOutTime].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            }
                            #endregion
                            string overstay = string.Empty;
                            if (dtHourlyOffDutyTag.Rows[i]["OutTimeShow"].ToString() != "")
                            {
                                DateTime NewRealOutTime;
                                string TakeDate = Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["WorkDate"].ToString().Trim()).ToString("dd-MMM-yyyy");
                                string ot = Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["ShiftOutTime"].ToString().Trim()).ToString("hh:mm tt");

                                //check night shift
                                string _sOUTtime = TakeDate + " " + ot;
                                string _sINtime = TakeDate + " " + Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["ShiftInTime"].ToString().Trim()).ToString("hh:mm tt");
                                if (Convert.ToDateTime(_sOUTtime) < Convert.ToDateTime(_sINtime))
                                {
                                    TakeDate = Convert.ToDateTime(TakeDate).AddDays(1).ToString("dd-MMM-yyyy");
                                }

                                string TateandTime = TakeDate + " " + ot;
                                //int minutesadd = Convert.ToInt32(dtHourlyOffDutyTag.Rows[i]["MaxOTPerDay"].ToString().Trim());
                                int minutesadd = Convert.ToInt32(dtHourlyOffDutyTag.Rows[0]["MaxOTPerDay"].ToString().Trim()) + Convert.ToInt32(dtHourlyOffDutyTag.Rows[0]["MaxExtraOTPerDay"].ToString().Trim());
                                DateTime NewOutTime = Convert.ToDateTime(TateandTime).AddMinutes(minutesadd);
                                DateTime RealOutTime = Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["OutTimeShow"].ToString().Trim());
                                double totalMinutes;

                                if (Convert.ToDateTime(RealOutTime) >= Convert.ToDateTime(NewOutTime) && (dtHourlyOffDutyTag.Rows[i]["DayStatus"].ToString() !="H" && dtHourlyOffDutyTag.Rows[i]["DayStatus"].ToString() != "W"))
                                {
                                    long WorkDateTickCount = Convert.ToDateTime(Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["WorkDate"].ToString()).ToString("dd-MMM-yyyy")).Ticks;
                                    int EmployeeSystemId = (int)Convert.ToInt64(dtHourlyOffDutyTag.Rows[i]["SystemId"].ToString());
                                    WorkDateTickCount += EmployeeSystemId;

                                    Random rnd = new Random((int)(WorkDateTickCount));
                                    int RandomMinutes = rnd.Next(0, 15);
                                    NewRealOutTime = Convert.ToDateTime(NewOutTime).AddMinutes(RandomMinutes);
                                    DateTime RandomTime = Convert.ToDateTime(NewRealOutTime);
                                    DateTime ShiftTime = Convert.ToDateTime(TateandTime);
                                    TimeSpan span = RandomTime - ShiftTime;

                                    if (Convert.ToInt32(dtHourlyOffDutyTag.Rows[0]["MaxOTPerDay"].ToString().Trim()) > 0)
                                    {
                                        totalMinutes = span.TotalMinutes;
                                        oru.GetOT(dtHourlyOffDutyTag.Rows[0]["OTConsiderOn"].ToString(), minutesadd.ToString(), out overstay);
                                        OTOverstay1 += clsStaticInfo.dbl(minutesadd);
                                        if (OTOverstay1 > Convert.ToInt32(dtHourlyOffDutyTag.Rows[0]["MaxOTPerDay"].ToString().Trim())) // If overstay1 is Greater then  MaxOtPerDay
                                        {
                                            OTMinimum1 = Convert.ToInt32(dtHourlyOffDutyTag.Rows[0]["MaxOTPerDay"].ToString().Trim());
                                        }
                                        else
                                        {
                                            OTMinimum1 = OTOverstay1;
                                        }

                                        OTOverstay1 = OTOverstay1 - Convert.ToInt32(dtHourlyOffDutyTag.Rows[i]["MaxOTPerDay"].ToString().Trim());
                                        if(OTOverstay1 == Convert.ToInt32(dtHourlyOffDutyTag.Rows[i]["MaxExtraOTPerDay"].ToString().Trim()))
                                        {
                                            OTOverstay1 = Convert.ToInt32(dtHourlyOffDutyTag.Rows[i]["MaxExtraOTPerDay"].ToString().Trim()) - Convert.ToInt32(dtHourlyOffDutyTag.Rows[i]["MaxOTPerDay"].ToString().Trim());
                                        }

                                    }
                                }
                                else
                                {
                                    NewRealOutTime = Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["OutTimeShow"].ToString().Trim());
                                    oru.GetOT(dtHourlyOffDutyTag.Rows[0]["OTConsiderOn"].ToString(), dtHourlyOffDutyTag.Rows[i]["TotalOT"].ToString(), out overstay);


                                    OTOverstay2 += clsStaticInfo.dbl(dtHourlyOffDutyTag.Rows[i]["TotalOT"].ToString());
                                    OTOverstay2 = OTOverstay2 - Convert.ToInt32(dtHourlyOffDutyTag.Rows[i]["MaxOTPerDay"].ToString().Trim());
                                    if (OTOverstay2 >= Convert.ToInt32(dtHourlyOffDutyTag.Rows[i]["MaxExtraOTPerDay"].ToString().Trim()))
                                    {
                                        OTOverstay2 = Convert.ToInt32(dtHourlyOffDutyTag.Rows[i]["MaxExtraOTPerDay"].ToString().Trim()) - Convert.ToInt32(dtHourlyOffDutyTag.Rows[i]["MaxOTPerDay"].ToString().Trim());
                                    }
                                }
                            }
                        }



                     
                        OTOverstay += clsStaticInfo.dbl(OTOverstay1 + OTOverstay2);

                        if (OTOverstay < 0)
                        {
                            OTOverstay = 0.00;
                        }

                        string GTotalOt = string.Empty;
                        string GTotalOtMin = string.Empty;

                        oru.GetOT(dtHourlyOffDutyTag.Rows[0]["OTConsiderOn"].ToString(), OTOverstay.ToString(), out GTotalOt);
                        oru.GetOT(dtHourlyOffDutyTag.Rows[0]["OTConsiderOn"].ToString(), OTMinimum1.ToString(), out GTotalOtMin);

                        if (dtHourlyOffDutyTag.Rows[i]["DayType"].ToString().Trim() == "W" && Convert.ToBoolean(dtHourlyOffDutyTag.Rows[i]["IsNoPunchOnWeekOffForOTEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dtHourlyOffDutyTag.Rows[i]["IsOTEntitled"].ToString().Trim()) == false)
                        {
                            GTotalOt = "";
                            GTotalOtMin = "";

                        }
                        else if (dtHourlyOffDutyTag.Rows[i]["DayType"].ToString().Trim() == "W" && Convert.ToBoolean(dtHourlyOffDutyTag.Rows[i]["IsNoPunchOnWeekOffForOTNotEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dtHourlyOffDutyTag.Rows[i]["IsOTEntitled"].ToString().Trim()) == true)
                        {
                            GTotalOt = "";
                            GTotalOtMin = "";


                        }
                        else if (dtHourlyOffDutyTag.Rows[i]["DayType"].ToString().Trim() == "H" && Convert.ToBoolean(dtHourlyOffDutyTag.Rows[i]["IsNoPunchOnHolidayForOTEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dtHourlyOffDutyTag.Rows[i]["IsOTEntitled"].ToString().Trim()) == false)
                        {
                            GTotalOt = "";
                            GTotalOtMin = "";


                        }
                        else if (dtHourlyOffDutyTag.Rows[i]["DayType"].ToString().Trim() == "H" && Convert.ToBoolean(dtHourlyOffDutyTag.Rows[i]["IsNoPunchOnHolidayForOTNotEntitle"].ToString().Trim()) == true && Convert.ToBoolean(dtHourlyOffDutyTag.Rows[i]["IsOTEntitled"].ToString().Trim()) == true)
                        {
                            GTotalOt = "";
                            GTotalOtMin = "";

                        }

                        sheet1.Range[xlsRow, iTotalHr].Text = GTotalOt;
                        sheet1.Range[xlsRow, irealTotalHr].Text = GTotalOtMin;

                        //sheet1.Range[xlsRow, iTotalHrReal].Number = clsStaticInfo.dbl(dtHourlyOffDutyTag.Rows[i]["TotalOTH"].ToString());
                        if (CheckBox)
                        {
                            sheet1.Range[xlsRow, inw_rate].Number = nwRate;
                            double amt = (OTOverstay / 60) * nwRate;
                            sheet1.Range[xlsRow, iAmount].Number = amt;
                        }

                        xlsRow++;
                        SLNo++;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                sheet1.Range[startXlsRow, iTotalHr, xlsRow, iTotalHr].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, iOutTime].Text = "Total";

                sheet1.Range[xlsRow, iOutTime, xlsRow, iOutTime].CellStyle.Font.Bold = true;

                string totalOT = "";
                string totalOTCal = "";

                object sumObject;
                sumObject = dtHourlyOffDutyTag.Compute("Sum(TotalOT)", "");
                totalOT = sumObject.ToString();
                oru.GetOT(dtHourlyOffDutyTag.Rows[0]["OTConsiderOn"].ToString(), totalOT, out totalOTCal);

                sheet1.Range[xlsRow, iTotalHr].Text = totalOTCal;

                sheet1.Range[xlsRow, iTotalHr, xlsRow, iTotalHr].CellStyle.Font.Bold = true;

                if (CheckBox)
                {
                    sheet1.Range[xlsRow, iAmount].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(iAmount) + startXlsRow + ":" + clsStaticInfo.GetxlsCol(iAmount) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, iOutTime, xlsRow, iAmount].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, iOutTime, xlsRow, iAmount].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, iAmount, xlsRow, iAmount].CellStyle.Font.Bold = true;
                }
                else
                {
                    sheet1.Range[xlsRow, iOutTime, xlsRow, iTotalHr].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, iOutTime, xlsRow, iTotalHr].BorderAround(ExcelLineStyle.Hair);
                }


                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;
                int sheetEndXlsRow = xlsRow;
                #endregion ----------------------Data-----------------------

                #region ******************Report Header******************



                xlsRow = 1;
                xlsCol = 3;
                try
                {
                    if (companyLogo != null)
                    {

                        double totalWidth = sheet1.GetColumnWidth(isl) + sheet1.GetColumnWidth(iEmployeeCode);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);
                        //pic.Height = 80;
                        //pic.Width = 220;
                    }
                }
                catch (Exception ex)
                {
                }

                FactoryName = string.Empty;

                string FactoryAddress = string.Empty;

                if (dtCmp.Rows.Count > 0)
                {
                    CmpName = dtCmp.Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    FactoryName = dtFactory.Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryAddress = dtFactory.Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Excess OT From " + FromDate + " TO " + ToDate;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = false;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "Excess OT";
                #endregion Page Setup

                #endregion  Individual Daily OT

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        void GenerateDic(DataSet dsPolicy, DataSet dsSalaryStruc, string _currencyId, out Dictionary<string, double> dicNW, out Dictionary<string, double> dicW, out Dictionary<string, double> dicH)
        {
            double nwRate = 0;
            double wRate = 0;
            double hRate = 0;
            dicNW = null;
            dicW = null;
            dicH = null;
            try
            {

                DataTable dtemp = new DataView(dsSalaryStruc.Tables[0]).ToTable(true, "EmpInfoSystemID");
                dicNW = new Dictionary<string, double>();
                dicW = new Dictionary<string, double>();
                dicH = new Dictionary<string, double>();

                for (int i = 0; i < dtemp.Rows.Count; i++)
                {
                    string _empid = dtemp.Rows[i]["EmpInfoSystemID"].ToString();


                    GetFormula(dsPolicy, dsSalaryStruc, _currencyId, _empid, out nwRate, out wRate, out hRate);
                    dicNW.Add(_empid, nwRate);
                    dicW.Add(_empid, wRate);
                    dicH.Add(_empid, hRate);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void GetFormula(DataSet dsPolicy, DataSet dsSalaryStruc, string _currencyId, string empid, out double nwRate, out double wRate, out double hRate)
        {
            nwRate = 0;
            wRate = 0;
            hRate = 0;
            //out string FormulaDesIDN, out string FormulaDesIDW, out string FormulaDesIDH
            string FormulaDesIDN = string.Empty;
            string FormulaDesIDW = string.Empty;
            string FormulaDesIDH = string.Empty;
            try
            {
                DataView dv = new DataView(dsPolicy.Tables[0]);
                dv.RowFilter = "systemid='" + empid + "'";
                if (dv.Count > 0)
                {
                    FormulaDesIDN = dv[0]["FormulaDesIDN"].ToString();
                    FormulaDesIDW = dv[0]["FormulaDesIDW"].ToString();
                    FormulaDesIDH = dv[0]["FormulaDesIDH"].ToString();
                    string EmployeeCode = dv[0]["EmployeeCode"].ToString();

                    if (string.IsNullOrEmpty(FormulaDesIDN))
                    {
                        throw new Exception("Employee " + EmployeeCode + " has no OT policy with her/his designation ...");
                    }


                    DataView dvss = new DataView(dsSalaryStruc.Tables[0]);
                    dvss.RowFilter = "EmpInfoSystemID='" + empid + "'";
                    if (dvss.Count > 0)
                    {
                        string FormulaValue = string.Empty;
                        DataTable dtValue = dvss.ToTable();
                        DataTable dtSalaryHead = dvss.ToTable(true, "SalaryHeadID", "SalaryHead");


                        GetFormulValue(FormulaDesIDH, ref dtValue, _currencyId, out hRate, ref dtSalaryHead);

                        GetFormulValue(FormulaDesIDW, ref dtValue, _currencyId, out wRate, ref dtSalaryHead);

                        GetFormulValue(FormulaDesIDN, ref dtValue, _currencyId, out nwRate, ref dtSalaryHead);

                    }//if
                }//if dv

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void GetFormulValue(string FormulaDesIDN, ref DataTable dtValue, string _currencyId, out double nwRate, ref DataTable dtSalaryHead)
        {
            string FormulaValue = string.Empty;
            nwRate = 0;
            try
            {
                clsSalaryUtility su = new clsSalaryUtility();
                su.ReLoadFormulaWithValue(FormulaDesIDN, ref dtValue, _currencyId, "1", out FormulaValue, ref dtSalaryHead);
                string sFormulaResult = clsSalaryStructureAplos.Evaluate(FormulaValue).ToString();
                if (sFormulaResult == "NaN")
                {
                    throw new Exception("Salary Head is not orderly tagged in Salary Rule");
                }

                //get formula wise value
                var vv = Convert.ToDouble(sFormulaResult).ToString("00.00");
                nwRate = Convert.ToDouble(vv);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Individual Daily OT 
        public IWorkbook GetExtraIndividualDailyOT(string Name, string CompanyGroupId, string PlantId, string CompanyId, string PlantName, string FromDate, string ToDate, string OTDuration, bool CheckBox, string OTfinal, string filePath = "")
        {

            #region declare
            clsReport objRpt = null;
            clsReport objRptSR = null;
            ReportUtility oru = new ReportUtility();
            //DataSet dsHourlyOffDutyTag = null;
            DataTable dtHourlyOffDutyTag = null;
            //DataSet dsCmp = null;
            //DataSet dsFactory = null;
            DataView dvOT = null;

            clsStaticInfo objStatic = null;
            objStatic = new clsStaticInfo();
            string OTConsiderOn = string.Empty;

            DataSet dsOTPolicy = null;//
            DataSet dsSStructure = null;
            string _currencyId = string.Empty;
            Dictionary<string, double> dicNW = null;
            Dictionary<string, double> dicW = null;
            Dictionary<string, double> dicH = null;
            #endregion
            try
            {
                ExcelEngine excelEngine = null;
                IApplication application = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2013;

                IWorkbook workbook = application.Workbooks.Create(4);

                string strHOTsql = @"select * from HourlyOt
                            where WorkDate Between '" + FromDate + @"' and '" + ToDate + @"' and PlantId = '" + PlantId + @"'";

                DataTable dtHourlyOT = _sqlRepository.GetDataTable(strHOTsql);

                #region Logo
                string strPath = "";
                Image companyLogo = null;
                try
                {
                    strPath = Path.Combine(ResourcesPathReader.GetLogoOrImagePath(), CompanyId + ".jpg");  // IDCardEng.xlsx
                    companyLogo = Image.FromFile(strPath);
                }
                catch (Exception)
                {
                }
                #endregion
                objRpt = new clsReport();

                objRptSR = new clsReport(_sqlRepository);

                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");

                #region OT Rate
                if (CheckBox)
                {
                    DataSet dsCurrency = null;
                    clsOTCalculation otc = new clsOTCalculation();
                    otc.LoadOverTimePolicy(PlantId, FromDate, ToDate, out dsOTPolicy);
                    otc.LoadSalaryStructure(PlantId, FromDate, ToDate, out dsSStructure);

                    clsSalaryInfo objSal = new clsSalaryInfo();
                    objSal.GetLocalCurrency(CompanyGroupId, PlantId, out dsCurrency);
                    if (dsCurrency.Tables[0].Rows.Count > 0)
                    {
                        _currencyId = "" + dsCurrency.Tables[0].Rows[0]["LocalCurrency"].ToString().Trim();
                    }
                    else
                    {
                        throw new Exception("No currency found...");
                    }

                    GenerateDic(dsOTPolicy, dsSStructure, _currencyId, out dicNW, out dicW, out dicH);
                }
                #endregion

                dtHourlyOffDutyTag = objRptSR.GetEIndividualDailyOT(FromDate, ToDate, OTDuration, OTfinal, PlantId, CompanyId, CompanyGroupId);


                var dtCmp = objRptSR.SelectedPlantWiseCompanyDT(PlantId);

                var dtFactory = objRptSR.SelectedPlantDT(PlantId);
                #region Variable
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                string FactoryName = "";
                string CmpName = "";
                var iName = 0;
                var iEmployeeCode = 0;
                var iSubSection = 0;
                var iSection = 0;
                var iTotalHr = 0;
                var iTotalHrReal = 0;
                int iTotalHrOT = 0; // Not extra OT 
                var iOutTime = 0;
                var iInTime = 0;

                var iLine = 0;
                var iDOJ = 0;
                var iDepartment = 0;
                var iDesignation = 0;
                var isl = 0;
                var iWorkDate = 0;
                var SLNo = 1;
                #endregion

                if (dtHourlyOffDutyTag.Rows.Count == 0)
                {
                    throw new Exception("No Data Found....");
                }
                #region Hourly Ot
                //workbook = application.Workbooks.Create(4);
                IWorksheet sheet1 = null;
                IWorksheet sheet2 = null;
                IWorksheet sheet3 = null;
                IWorksheet sheet4 = null;

                sheet1 = workbook.Worksheets[0];
                sheet2 = workbook.Worksheets[1];
                sheet3 = workbook.Worksheets[2];
                sheet4 = workbook.Worksheets[3];


                xlsRow = 6;
                sheet1.Range[xlsRow - 1, 1].Text = "Report Ref No:";
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow - 1, 1].RowHeight = 20;
                sheet1.Range[xlsRow - 1, 1].CellStyle.Font.Bold = true;

                #region ------------------Column Header------------------
                isl = xlsCol;
                sheet1.Range[xlsRow, isl].Text = "SL";
                sheet1.Range[xlsRow, isl].ColumnWidth = 7;

                xlsCol += 1;
                iEmployeeCode = xlsCol;
                sheet1.Range[xlsRow, iEmployeeCode].Text = "Emp Code";
                sheet1.Range[xlsRow, iEmployeeCode].ColumnWidth = 10;

                xlsCol += 1;
                iName = xlsCol;
                sheet1.Range[xlsRow, iName].Text = "Emp Name";
                sheet1.Range[xlsRow, iName].ColumnWidth = 25;



                xlsCol += 1;
                iDOJ = xlsCol;
                sheet1.Range[xlsRow, iDOJ].Text = "DOJ";
                sheet1.Range[xlsRow, iDOJ].ColumnWidth = 20;

                xlsCol += 1;
                iDepartment = xlsCol;
                sheet1.Range[xlsRow, iDepartment].Text = "Department";
                sheet1.Range[xlsRow, iDepartment].ColumnWidth = 25;

                xlsCol += 1;
                iDesignation = xlsCol;
                sheet1.Range[xlsRow, iDesignation].Text = "Designation";
                sheet1.Range[xlsRow, iDesignation].ColumnWidth = 25;

                xlsCol += 1;
                iSection = xlsCol;
                sheet1.Range[xlsRow, iSection].Text = "Section";
                sheet1.Range[xlsRow, iSection].ColumnWidth = 14;

                xlsCol += 1;
                iSubSection = xlsCol;
                sheet1.Range[xlsRow, iSubSection].Text = "Sub Section";
                sheet1.Range[xlsRow, iSubSection].ColumnWidth = 16;

                xlsCol += 1;
                iLine = xlsCol;
                sheet1.Range[xlsRow, iLine].Text = "Line";
                sheet1.Range[xlsRow, iLine].ColumnWidth = 12;
                xlsCol += 1;
                iWorkDate = xlsCol;
                sheet1.Range[xlsRow, iWorkDate].Text = "Work Date";
                sheet1.Range[xlsRow, iWorkDate].ColumnWidth = 12;

                xlsCol += 1;
                iInTime = xlsCol;
                sheet1.Range[xlsRow, iInTime].Text = "In Time";
                sheet1.Range[xlsRow, iInTime].ColumnWidth = 12;

                xlsCol += 1;
                iOutTime = xlsCol;
                sheet1.Range[xlsRow, iOutTime].Text = "Out Time";
                sheet1.Range[xlsRow, iOutTime].ColumnWidth = 12;



                xlsCol += 1;
                iTotalHr = xlsCol;
                sheet1.Range[xlsRow, iTotalHr].Text = "OT Hours";
                sheet1.Range[xlsRow, iTotalHr].ColumnWidth = 15;
                xlsCol += 1;
                int irealTotalHr = xlsCol;
                sheet1.Range[xlsRow, irealTotalHr].Text = "Extra OT";
                sheet1.Range[xlsRow, irealTotalHr].ColumnWidth = 15;
                xlsCol += 1;

                int iTotalOTEHour = xlsCol;
                sheet1.Range[xlsRow, xlsCol].Text = "Total OT";
                sheet1.Range[xlsRow, xlsCol].ColumnWidth = 15;
                xlsCol += 1;
                iTotalHrReal = xlsCol;
                sheet1.Range[xlsRow, iTotalHrReal].Text = "Total(Hours)";
                sheet1.Range[xlsRow, iTotalHrReal].ColumnWidth = 15;

                int inw_rate = 0;
                int iAmount = 0;
                //int iw_rate = 0;
                //int ih_rate = 0;
                if (CheckBox)
                {
                    xlsCol += 1;
                    inw_rate = xlsCol;
                    sheet1.Range[xlsRow, inw_rate].Text = "Rate";
                    sheet1.Range[xlsRow, inw_rate].ColumnWidth = 12;

           

                    xlsCol += 1;
                    iAmount = xlsCol;
                    sheet1.Range[xlsRow, iAmount].Text = "Amount";
                    sheet1.Range[xlsRow, iAmount].ColumnWidth = 12;
                }

                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;
                sheet1.Range[xlsRow, isl, xlsRow, endXlsCol].CellStyle.FillBackground = ExcelKnownColors.Grey_40_percent;


                xlsRow++;

                #endregion ------------------Column Header------------------
                var totalOtHr = 0.00;

                int startXlsRow = xlsRow;


                dtHourlyOffDutyTag.DefaultView.RowFilter = "ISNULL(OTConsiderOn,'')<>''";

                string strOTConsiderOn = dtHourlyOffDutyTag.DefaultView[0]["OTConsiderOn"].ToString();

                for (int i = 0; i < dtHourlyOffDutyTag.Rows.Count; i++)
                {
                    #region ----------------------Data-----------------------  
                    string yot = string.Empty;
                    string got = string.Empty;

                    double nwRate = 0;
                    try
                    {
                        #region OT Rate

                        string _empid = dtHourlyOffDutyTag.Rows[i]["systemid"].ToString();
                        string _daytype = dtHourlyOffDutyTag.Rows[i]["daytype"].ToString();



                        if (CheckBox)
                        {

                            try
                            {
                                if (_daytype.ToUpper() == "W")
                                {
                                    nwRate = dicW[_empid];
                                }
                                else if (_daytype.ToUpper() == "H")
                                {
                                    nwRate = dicH[_empid];
                                }
                                else
                                {
                                    nwRate = dicNW[_empid];
                                }
                            }
                            catch (Exception ex)
                            {


                            }

                        }
                        //}
                        #endregion

                        oru.GetOT(strOTConsiderOn, dtHourlyOffDutyTag.Rows[i]["TotalOT"].ToString(), out yot);
                        oru.GetOT(strOTConsiderOn, dtHourlyOffDutyTag.Rows[i]["RealOt"].ToString(), out got);

                        sheet1.Range[xlsRow, isl].Text = SLNo.ToString();
                        sheet1.Range[xlsRow, iName].Text = dtHourlyOffDutyTag.Rows[i]["EmployeeName"].ToString();
                        sheet1.Range[xlsRow, iEmployeeCode].Text = dtHourlyOffDutyTag.Rows[i]["EmployeeCode"].ToString();
                        sheet1.Range[xlsRow, iDOJ].Text = dtHourlyOffDutyTag.Rows[i]["DOJ"].ToString();
                        sheet1.Range[xlsRow, iDesignation].Text = dtHourlyOffDutyTag.Rows[i]["Designation"].ToString();
                        sheet1.Range[xlsRow, iDepartment].Text = dtHourlyOffDutyTag.Rows[i]["Department"].ToString();
                        sheet1.Range[xlsRow, iSection].Text = dtHourlyOffDutyTag.Rows[i]["Section"].ToString();
                        sheet1.Range[xlsRow, iSubSection].Text = dtHourlyOffDutyTag.Rows[i]["SubSection"].ToString();
                        sheet1.Range[xlsRow, iLine].Text = dtHourlyOffDutyTag.Rows[i]["Line"].ToString();
                        sheet1.Range[xlsRow, iWorkDate].Text = dtHourlyOffDutyTag.Rows[i]["WorkDate"].ToString();

                        if (!string.IsNullOrEmpty(dtHourlyOffDutyTag.Rows[i]["OutTime"].ToString()))
                        {
                            sheet1.Range[xlsRow, iOutTime].Text = Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["OutTime"].ToString()).ToString("hh:mm tt", CultureInfo.InvariantCulture);

                        }
                        if (!string.IsNullOrEmpty(dtHourlyOffDutyTag.Rows[i]["InTime"].ToString()))
                        {
                            sheet1.Range[xlsRow, iInTime].Text = Convert.ToDateTime(dtHourlyOffDutyTag.Rows[i]["InTime"].ToString()).ToString("hh:mm tt", CultureInfo.InvariantCulture);

                        }

                        sheet1.Range[xlsRow, iTotalHr].Text = yot;
                        sheet1.Range[xlsRow, iTotalHrReal].Number = clsStaticInfo.dbl(dtHourlyOffDutyTag.Rows[i]["TotalOTH"].ToString()) + (clsStaticInfo.dbl(dtHourlyOffDutyTag.Rows[i]["RealOt"].ToString()) / 60);
                        sheet1.Range[xlsRow, irealTotalHr].Text = got;//clsStaticInfo.dbl(dtHourlyOffDutyTag.Rows[i]["TotalOTH"].ToString()) + clsStaticInfo.dbl(dtHourlyOffDutyTag.Rows[i]["RealOt"].ToString());
                        got = "";
                        totalOtHr = clsStaticInfo.dbl(dtHourlyOffDutyTag.Rows[i]["TotalOT"].ToString()) + clsStaticInfo.dbl(dtHourlyOffDutyTag.Rows[i]["RealOt"].ToString());
                        oru.GetOT(strOTConsiderOn, totalOtHr.ToString(), out got);
                        sheet1.Range[xlsRow, iTotalOTEHour].Text = got;//clsStaticInfo.dbl(dtHourlyOffDutyTag.Rows[i]["TotalOTH"].ToString()) + clsStaticInfo.dbl(dtHourlyOffDutyTag.Rows[i]["RealOt"].ToString());


                        if (CheckBox)
                        {
                            sheet1.Range[xlsRow, inw_rate].Number = nwRate;
                            double amt = (clsStaticInfo.dbl(dtHourlyOffDutyTag.Rows[i]["TotalOTH"].ToString()) + (clsStaticInfo.dbl(dtHourlyOffDutyTag.Rows[i]["RealOt"].ToString()) / 60)) * nwRate;
                            sheet1.Range[xlsRow, iAmount].Number = amt;
                        }

                        xlsRow++;
                        SLNo++;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
                sheet1.Range[startXlsRow, iTotalHr, xlsRow, iTotalHr].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[startXlsRow, irealTotalHr, xlsRow, irealTotalHr].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[startXlsRow, iTotalOTEHour, xlsRow, iTotalOTEHour].HorizontalAlignment = ExcelHAlign.HAlignRight;


                sheet1.Range[xlsRow, iOutTime].Text = "Total";

                sheet1.Range[xlsRow, iOutTime, xlsRow, iOutTime].CellStyle.Font.Bold = true;

                string totalOT = "";
                string totalOTE = "";

                string totalOTCal = "";

                object sumObject;
                sumObject = dtHourlyOffDutyTag.Compute("Sum(TotalOTH)", "");
                totalOT = sumObject.ToString();



                sheet1.Range[xlsRow, iTotalHr].Text = totalOTCal;

                sheet1.Range[xlsRow, iTotalHr, xlsRow, iTotalHr].CellStyle.Font.Bold = true;

                object sumObject2;
                sumObject2 = dtHourlyOffDutyTag.Compute("Sum(RealOt)/60", "");
                totalOTE = sumObject2.ToString();
                oru.GetOT(strOTConsiderOn, totalOTE, out totalOTCal);
                sheet1.Range[xlsRow, iTotalHrReal].Text = totalOTCal;

                sheet1.Range[xlsRow, iTotalHrReal, xlsRow, iTotalHrReal].CellStyle.Font.Bold = true;
                if (CheckBox)
                {
                    sheet1.Range[xlsRow, iAmount].Formula = "=SUM(" + clsStaticInfo.GetxlsCol(iAmount) + startXlsRow + ":" + clsStaticInfo.GetxlsCol(iAmount) + (xlsRow - 1) + ")";
                    sheet1.Range[xlsRow, iOutTime, xlsRow, iAmount].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, iOutTime, xlsRow, iAmount].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, iAmount, xlsRow, iAmount].CellStyle.Font.Bold = true;
                }
                else
                {
                    sheet1.Range[xlsRow, iOutTime, xlsRow, iTotalHr].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow, iOutTime, xlsRow, iTotalHr].BorderAround(ExcelLineStyle.Hair);
                }


                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[6, 1, xlsRow - 1, endXlsCol].WrapText = true;
                int sheetEndXlsRow = xlsRow;
                #endregion ----------------------Data-----------------------

                #region ******************Report Header******************



                xlsRow = 1;
                xlsCol = 3;
                try
                {
                    if (companyLogo != null)
                    {

                        double totalWidth = sheet1.GetColumnWidth(isl) + sheet1.GetColumnWidth(iEmployeeCode);
                        int totalWidthPixel = (int)(totalWidth * 7.5);
                        int totalheight = (int)((sheet1.GetRowHeight(1) + sheet1.GetRowHeight(2) + sheet1.GetRowHeight(3) + sheet1.GetRowHeight(3)) * 1.50);

                        companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                        IPictureShape pic = null;

                        pic = sheet1.Pictures.AddPicture(1, 1, companyLogo);
                        //pic.Height = 80;
                        //pic.Width = 220;
                    }
                }
                catch (Exception ex)
                {
                }

                FactoryName = string.Empty;

                string FactoryAddress = string.Empty;

                if (dtCmp.Rows.Count > 0)
                {
                    CmpName = dtCmp.Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, 3].Text = CmpName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    //FactoryName = dsFactory.Tables[0].Rows[0]["PlantName"].ToString();
                    FactoryName = dtFactory.Rows[0]["UserName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryName;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 14;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dtFactory.Rows.Count > 0)
                {
                    FactoryAddress = dtFactory.Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, 3].Text = FactoryAddress;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, 3].Text = "Excess OT From " + FromDate + " TO " + ToDate;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                sheet1.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet1.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;


                #endregion ******************Report Header******************

                #region Freeze Panes

                sheet1.IsDisplayZeros = false;
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 6;

                #endregion Freeze Panes

                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = false;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 14;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                sheet1.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.IsDisplayZeros = false;
                sheet1.Name = "Excess OT";
                #endregion Page Setup

                #endregion  Individual Daily OT
                string fPath = filePath;
                if (string.IsNullOrEmpty(filePath))
                {
                    filePath = HostingEnvironment.MapPath("~/") + "TempIndividualDailyOT.xlsx";
                }

                workbook.SaveAs(filePath);
                workbook = application.Workbooks.Open(filePath);
                IWorksheet worksheet = workbook.Worksheets[0];
                try
                {

                    #region PivotSheet1
                    IWorksheet pivotSheet = workbook.Worksheets[1];
                    pivotSheet.Name = "Summary";


                    try
                    {

                        if (companyLogo != null)
                        {
                            double totalWidth = pivotSheet.GetColumnWidth(isl) + pivotSheet.GetColumnWidth(iEmployeeCode);
                            int totalWidthPixel = (int)(totalWidth * 7.5);
                            int totalheight = (int)((pivotSheet.GetRowHeight(1) + pivotSheet.GetRowHeight(2) + pivotSheet.GetRowHeight(3) + pivotSheet.GetRowHeight(3)) * 1.50);

                            companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                            IPictureShape pic = null;

                            pic = pivotSheet.Pictures.AddPicture(1, 1, companyLogo);

                        }
                    }
                    catch (Exception ex)
                    {
                    }

                    #region Report Header
                    xlsRow = 1;
                    xlsCol = 1;


                    pivotSheet.Range[xlsRow, 3].Text = CmpName;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    xlsRow += 1;

                    pivotSheet.Range[xlsRow, 3].Text = FactoryName;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;



                    xlsRow += 1;

                    pivotSheet.Range[xlsRow, 3].Text = FactoryAddress;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    //sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    xlsRow += 1;
                    pivotSheet.Range[xlsRow, 3].Text = "Excess OT From " + FromDate + " TO " + ToDate;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                    pivotSheet.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                    pivotSheet.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    pivotSheet.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    pivotSheet.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                    #endregion

                    pivotSheet.Range[xlsRow + 1, 1].Text = "Report Ref No:";
                    pivotSheet.Range[xlsRow + 1, 1].CellStyle.Font.Size = 10;
                    pivotSheet.Range[xlsRow + 1, 1].RowHeight = 20;
                    pivotSheet.Range[xlsRow + 1, 1].CellStyle.Font.Bold = true;


                    IPivotCache cache = workbook.PivotCaches.Add(worksheet["A6:" + oru.GetColumnNameForXls(endXlsCol) + (sheetEndXlsRow - 1)]);

                    #region Second Pivot table
                    pivotSheet.Range[xlsRow + 2, 1].Text = "Total Manpower in OverTime";
                    pivotSheet.Range[xlsRow + 2, 1].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow + 2, 1].CellStyle.Font.Bold = true;

                    IPivotTable pivotTable2 = pivotSheet.PivotTables.Add("PivotTable2", pivotSheet["A8"], cache);

                    //Add Pivot table fields (Row and Column fields)
                    pivotTable2.Fields[iDepartment - 1].Axis = PivotAxisTypes.Row;
                    pivotTable2.Fields[iSection - 1].Axis = PivotAxisTypes.Row;
                    pivotTable2.Fields[iTotalHrReal - 1].Axis = PivotAxisTypes.Column;


                    IPivotTable pivotTable2_1 = pivotSheet.PivotTables["PivotTable2"];
                    pivotTable2_1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable2_1.Options.ShowDrillIndicators = false;

                    pivotTable2_1.DisplayFieldCaptions = true;


                    //Add data field
                    IPivotField field2 = pivotTable2_1.Fields[iTotalHrReal - 1];
                    pivotTable2_1.DataFields.Add(field2, "Total Employees", PivotSubtotalTypes.Count);
                    pivotTable2_1.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;


                    int totalColumns = pivotTable2_1.RowFields.Count + pivotTable2_1.ColumnFields.Count;
                    for (int i = 0; i < pivotTable2_1.ColumnFields.Count; i++)
                    {
                        totalColumns += pivotTable2_1.ColumnFields[i].Items.Count;
                    }

                    int lastCloumn = totalColumns;

                    #endregion


                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].Text = "OverTime Hours Summary";
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Size = 12;
                    pivotSheet.Range[xlsRow + 2, lastCloumn + 2].CellStyle.Font.Bold = true;



                    //Create "PivotTable1" with the cache at the specified range
                    IPivotTable pivotTable = pivotSheet.PivotTables.Add("PivotTable1", pivotSheet[clsStaticInfo.GetxlsCol(lastCloumn + 2) + "8"], cache);

                    //Add Pivot table fields (Row and Column fields)
                    pivotTable.Fields[iDepartment - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[iSection - 1].Axis = PivotAxisTypes.Row;
                    pivotTable.Fields[iTotalHrReal - 1].Axis = PivotAxisTypes.Column;


                    IPivotTable pivotTable1 = pivotSheet.PivotTables["PivotTable1"];
                    pivotTable1.Options.RowLayout = PivotTableRowLayout.Tabular;
                    pivotTable1.Options.ShowDrillIndicators = false;

                    pivotTable1.DisplayFieldCaptions = true;
                    pivotTable1.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;


                    //Add data field
                    IPivotField field = pivotTable.Fields[iTotalHrReal - 1];
                    pivotTable.DataFields.Add(field, "Total Hours", PivotSubtotalTypes.Sum);




                    pivotSheet.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                    pivotSheet.IsGridLinesVisible = false;

                    #endregion
                    if (string.IsNullOrEmpty(fPath))
                    {
                        #region PivotSheet2

                        IWorksheet pivotSheet2 = workbook.Worksheets[2];
                        pivotSheet2.Name = "OT SUMMARY Sec";
                        pivotSheet2.IsGridLinesVisible = false;

                        try
                        {
                            if (companyLogo != null)
                            {
                                double totalWidth = pivotSheet2.GetColumnWidth(isl) + pivotSheet2.GetColumnWidth(iEmployeeCode);
                                int totalWidthPixel = (int)(totalWidth * 7.5);
                                int totalheight = (int)((pivotSheet2.GetRowHeight(1) + pivotSheet2.GetRowHeight(2) + pivotSheet2.GetRowHeight(3) + pivotSheet2.GetRowHeight(3)) * 1.50);

                                companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                                IPictureShape pic = null;
                                pic = pivotSheet2.Pictures.AddPicture(1, 1, companyLogo);

                            }
                        }
                        catch (Exception ex)
                        {
                        }

                        #region Report Header
                        xlsRow = 1;
                        xlsCol = 1;


                        pivotSheet2.Range[xlsRow, 3].Text = CmpName;
                        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                        pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                        pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                        pivotSheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        pivotSheet2.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                        xlsRow += 1;

                        pivotSheet2.Range[xlsRow, 3].Text = FactoryName;
                        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                        pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                        pivotSheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        pivotSheet2.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                        xlsRow += 1;

                        pivotSheet2.Range[xlsRow, 3].Text = FactoryAddress;
                        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                        pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                        pivotSheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        pivotSheet2.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                        xlsRow += 1;
                        pivotSheet2.Range[xlsRow, 3].Text = "OT Summary From " + FromDate + " TO " + ToDate;
                        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                        pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                        pivotSheet2.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                        pivotSheet2.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        pivotSheet2.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        pivotSheet2.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                        #endregion
                        pivotSheet2.Range[xlsRow + 1, 1].Text = "Report Ref No:";
                        pivotSheet2.Range[xlsRow + 1, 1].CellStyle.Font.Size = 10;
                        pivotSheet2.Range[xlsRow + 1, 1].RowHeight = 20;
                        pivotSheet2.Range[xlsRow + 1, 1].CellStyle.Font.Bold = true;

                        IPivotTable pivotTableSec = pivotSheet2.PivotTables.Add("PivotTableSEC", pivotSheet2["A7"], cache);

                        //Add Pivot table fields (Row and Column fields)
                        pivotTableSec.Fields[iDepartment - 1].Axis = PivotAxisTypes.Row;
                        pivotTableSec.Fields[iSection - 1].Axis = PivotAxisTypes.Row;
                        pivotTableSec.Fields[iTotalHrReal - 1].Axis = PivotAxisTypes.Column;


                        IPivotTable pivotTableSec_1 = pivotSheet2.PivotTables["PivotTableSEC"];
                        pivotTableSec_1.Options.RowLayout = PivotTableRowLayout.Tabular;
                        pivotTableSec_1.Options.ShowDrillIndicators = false;

                        pivotTableSec_1.DisplayFieldCaptions = true;


                        //Add data field
                        IPivotField fieldSec = pivotTableSec_1.Fields[iTotalHrReal - 1];
                        pivotTableSec_1.DataFields.Add(fieldSec, "Hours", PivotSubtotalTypes.Sum);

                        IPivotField fieldSec2 = pivotTableSec_1.Fields[iTotalHrReal - 1];
                        pivotTableSec_1.DataFields.Add(fieldSec2, "Employees", PivotSubtotalTypes.Count);

                        if (CheckBox)
                        {
                            IPivotField fieldSec3 = pivotTableSec_1.Fields[iAmount - 1];
                            pivotTableSec_1.DataFields.Add(fieldSec3, "Amount", PivotSubtotalTypes.Sum);
                        }

                        pivotTableSec_1.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;



                        pivotSheet2.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                        #endregion

                        #region PivotSheet3

                        IWorksheet pivotSheet3 = workbook.Worksheets[3];
                        pivotSheet3.Name = "OT SUMMARY Dept.";
                        pivotSheet3.IsGridLinesVisible = false;

                        try
                        {

                            if (companyLogo != null)
                            {
                                double totalWidth = pivotSheet3.GetColumnWidth(1) + pivotSheet3.GetColumnWidth(2);
                                int totalWidthPixel = (int)(totalWidth * 7.5);
                                int totalheight = (int)((pivotSheet3.GetRowHeight(1) + pivotSheet3.GetRowHeight(2) + pivotSheet3.GetRowHeight(3) + pivotSheet3.GetRowHeight(3)) * 1.50);

                                companyLogo = ReportUtility.FixedSize(companyLogo, totalWidthPixel, totalheight);
                                IPictureShape pic = null;

                                pic = pivotSheet3.Pictures.AddPicture(1, 1, companyLogo);

                            }
                        }
                        catch (Exception ex)
                        {
                        }

                        #region Report Header
                        xlsRow = 1;
                        xlsCol = 1;


                        pivotSheet3.Range[xlsRow, 3].Text = CmpName;
                        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                        pivotSheet3.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                        pivotSheet3.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 17;
                        pivotSheet3.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        pivotSheet3.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                        xlsRow += 1;

                        pivotSheet3.Range[xlsRow, 3].Text = FactoryName;
                        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                        pivotSheet3.Range[xlsRow, 3].CellStyle.Font.Size = 12;
                        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 18;
                        pivotSheet3.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        pivotSheet3.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                        xlsRow += 1;

                        pivotSheet3.Range[xlsRow, 3].Text = FactoryAddress;
                        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                        pivotSheet3.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 22;
                        pivotSheet3.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        pivotSheet3.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                        xlsRow += 1;
                        pivotSheet3.Range[xlsRow, 3].Text = "OT Summary From " + FromDate + " TO " + ToDate;
                        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].Merge();
                        pivotSheet3.Range[xlsRow, 3].CellStyle.Font.Size = 10;
                        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].RowHeight = 20;
                        pivotSheet3.Range[xlsRow, 3].CellStyle.Font.Bold = true;
                        pivotSheet3.Range[xlsRow, 3].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        pivotSheet3.Range[xlsRow, 3].VerticalAlignment = ExcelVAlign.VAlignCenter;
                        pivotSheet3.Range[xlsRow, 3, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                        #endregion
                        pivotSheet3.Range[xlsRow + 1, 1].Text = "Report Ref No:";
                        pivotSheet3.Range[xlsRow + 1, 1].CellStyle.Font.Size = 10;
                        pivotSheet3.Range[xlsRow + 1, 1].RowHeight = 20;
                        pivotSheet3.Range[xlsRow + 1, 1].CellStyle.Font.Bold = true;

                        IPivotTable pivotTableDept = pivotSheet3.PivotTables.Add("PivotTableDept", pivotSheet2["A7"], cache);

                        //Add Pivot table fields (Row and Column fields)
                        pivotTableDept.Fields[iDepartment - 1].Axis = PivotAxisTypes.Row;
                        //pivotTableDept.Fields[iSection - 1].Axis = PivotAxisTypes.Row;
                        pivotTableDept.Fields[iTotalHrReal - 1].Axis = PivotAxisTypes.Column;


                        IPivotTable pivotTableDept_1 = pivotSheet3.PivotTables["PivotTableDept"];
                        pivotTableDept_1.Options.RowLayout = PivotTableRowLayout.Tabular;
                        pivotTableDept_1.Options.ShowDrillIndicators = false;

                        pivotTableDept_1.DisplayFieldCaptions = true;


                        //Add data field
                        IPivotField fieldDept = pivotTableDept_1.Fields[iTotalHrReal - 1];
                        pivotTableDept_1.DataFields.Add(fieldDept, "Hours", PivotSubtotalTypes.Sum);

                        IPivotField fieldDept2 = pivotTableDept_1.Fields[iTotalHrReal - 1];
                        pivotTableDept_1.DataFields.Add(fieldDept2, "Employees", PivotSubtotalTypes.Count);

                        if (CheckBox)
                        {
                            IPivotField fieldDept3 = pivotTableDept_1.Fields[iAmount - 1];
                            pivotTableDept_1.DataFields.Add(fieldDept3, "Amount", PivotSubtotalTypes.Sum);
                        }
                        pivotTableDept_1.BuiltInStyle = PivotBuiltInStyles.PivotStyleMedium15;



                        pivotSheet3.UsedRange.CellStyle.Font.FontName = "Arial Narrow";
                        #endregion
                    }

                }
                catch (Exception)
                {

                }
                try
                {
                    worksheet.ShowColumn(iTotalHrReal, false);
                    if (string.IsNullOrEmpty(fPath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
                catch (Exception)
                {
                }
                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #endregion

    }
}
