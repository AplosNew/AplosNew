using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Data;
using Library.Data.Sql;
using OTSBD;
using Library.Service.Helpers;
using Library.Model.Setups;
using Library.Model.Enums;
using Syncfusion.XlsIO;
using ConnectionManager;
using System.Globalization;
using bplib;

namespace Aplos.HumanResource
{
    public class MyAppPaySlipService
    {

        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public MyAppPaySlipService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }


        public string GetEmployeePaySlip(string companyGroupId, string companyId, string plantId, string month, string year, string EmpId, string languageId, bool isActive, bool isSeperated, bool isMaternity)
        {
            MasterPaySlipData master = new MasterPaySlipData();
            DataView dvSlrProc = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;

            string FactoryName = "";
            string CmpName = "";
            int _x = 0;
            double _basic = 0;
            double _netPay = 0;
            double _StrCTC = 0;

            try
            {
                ParamList para = new ParamList();

                para.PlantId = plantId;
                para.FromDate = "01-" + bplib.clsWebLib.GetMonthName(month) + "-" + year;

                var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month

                para.ToDate = daysInMonth + "-" + bplib.clsWebLib.GetMonthName(month) + "-" + year;

                para.UnitId = "ALL";
                para.SubSectionId = "ALL";
                para.SectionId = "ALL";
                para.DivisionId = "ALL";

                #region DataSet

                DataSet dsEmpLoyeeInfo = null;
                DataTable dtSalaryHeadSheet = null;
                GetEmployeeInfoDetailSalaryLogWise(companyGroupId, companyId, plantId, para.FromDate, para.ToDate, EmpId, isActive, isSeperated, isMaternity, out dsEmpLoyeeInfo);//Sql Query For Salary  Data

                Dictionary<string, List<DataRow>> dicEmpSalry = GetEmployeeSalaryInfoDetailPaySlip(companyGroupId, companyId, plantId, para.FromDate, para.ToDate, languageId, EmpId, isActive, isSeperated, isMaternity, out dtSalaryHeadSheet);

                Dictionary<string, string> dicPF = GetEmployeePFESIC("PF", EmpId);
                Dictionary<string, string> dicESIC = GetEmployeePFESIC("ESIC", EmpId);



                DataSet dsGrade = null;
                GetGrade(EmpId, month, year, out dsGrade);
                dvSlrProc = new DataView();
                DataTable dtEmpInfo = dsEmpLoyeeInfo.Tables[0];
                SelectedPlantWiseCompany(plantId, out dsCmp);

                SelectedPlant(plantId, out dsFactory);

                DataSet dsExtraAbsent = null;
                DataView dvExtraAbsent = null;
                GetExtraAbsent(plantId, EmpId, Convert.ToInt32(month), Convert.ToInt32(year), out dsExtraAbsent);
                dvExtraAbsent = new DataView(dsExtraAbsent.Tables[0]);
                #endregion DataSet
                if (dtEmpInfo.Rows.Count > 0)
                {

                    double SLESIC = 0.00;
                   
                    string x = "";
                    Dictionary<string, List<DataRow>> dicLeaveEmp = GetEmpLeaveInfoPaySlip(para, EmpId);

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    for (int i = 0; i <= dtEmpInfo.Rows.Count - 1; i++)
                    {
                        master.EmployeeCode = clsWebLib.RetValidLen(dtEmpInfo.Rows[i]["EmployeeCode"]).ToString();
                        master.EmployeeStatus = clsWebLib.RetValidLen(dtEmpInfo.Rows[i]["EmployeeStatus"]).ToString();
                        master.DOJ = dtEmpInfo.Rows[i]["DOJ"].ToString();
                        master.DOB = dtEmpInfo.Rows[i]["DOB"].ToString();
                        
                        para.EmployeeId = dtEmpInfo.Rows[i]["EmpSystemId"].ToString();
                        List<DataRow> drLeaveEmp = null;
                        SLESIC = 0.00;
                        master.LWP= clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalLWP"].ToString()) + Convert.ToDouble(SLESIC);


                        if (dicLeaveEmp.ContainsKey(dtEmpInfo.Rows[i]["EmpSystemID"].ToString()))
                        {
                            drLeaveEmp = dicLeaveEmp[dtEmpInfo.Rows[i]["EmpSystemID"].ToString()];

                            foreach (var item in drLeaveEmp)
                            {
                                LeaveMaster mx = new LeaveMaster();
                                mx.Text = item["Code"].ToString();
                                mx.Value = clsStaticInfo.dbl(item["AvailedLeave"].ToString());
                                master.Lvx.Add(mx);
                                
                            }
                        }

                        if ((string.Compare(x.ToUpper(), dtEmpInfo.Rows[i]["EmpSystemId"].ToString().Trim().ToUpper())) != 0)
                        {

                            string _grade = string.Empty;
                            if (dsGrade.Tables[0].Rows.Count > 0)
                            {
                                _grade = dsGrade.Tables[0].Rows[0]["Grade"].ToString();
                            }

                            string pfESICNo = "", pFUANNo = "";

                            if (dicPF.ContainsKey(dtEmpInfo.Rows[i]["EmpSystemId"].ToString().Trim().ToUpper()))
                            {
                                pFUANNo = dicPF[dtEmpInfo.Rows[i]["EmpSystemId"].ToString().Trim().ToUpper()];
                            }

                            pfESICNo = "";
                            if (dicESIC.ContainsKey(dtEmpInfo.Rows[i]["EmpSystemId"].ToString().Trim().ToUpper()))
                            {
                                pfESICNo = dicESIC[dtEmpInfo.Rows[i]["EmpSystemId"].ToString().Trim().ToUpper()];
                            }

                            string _pd = "";
                            string _ad = "";
                            string _wod = "";
                            string _hd = "";
                            string _ld = "";
                            double PDay = 0;

                            _pd = (clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalPresent"].ToString())+ clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalLate"].ToString())).ToString();
                            _ad = (clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalLWP"].ToString())).ToString();
                            _wod = dtEmpInfo.Rows[i]["TotalWeekOff"].ToString();
                            _hd = dtEmpInfo.Rows[i]["TotalHoliDay"].ToString();
                            _ld = clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalLv"].ToString()).ToString();



                            if (!String.IsNullOrEmpty(dtEmpInfo.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper()))
                            {
                                if (dtEmpInfo.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOffAndHoliday.ToString().ToUpper())
                                {
                                    PDay = clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalHoliDay"].ToString()) - clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalWeekOff"].ToString());
                                }
                                if (dtEmpInfo.Rows[i]["WorkingDaysInAMonth"].ToString().ToUpper() == WorkingDaysInAMonth.ExcludingWeekOff.ToString().ToUpper())
                                {
                                    PDay = clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalAbsent"].ToString()) - clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalWeekOff"].ToString());
                                }
                            }
                            else
                            {
                                PDay = clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalProcDate"].ToString()) - clsStaticInfo.dbl(dtEmpInfo.Rows[i]["TotalAbsent"].ToString());
                            }

                            master.TotalPresent = _pd;
                            master.TotalAbsent = _ad;
                            master.TotalWeekOff = _wod;
                            master.TotalHoliDay = _hd;
                            master.TotalLv = _ld;
                            master.PayDay = PDay;
                        }
                        x = dtEmpInfo.Rows[i]["EmpSystemId"].ToString().Trim();

                        double _Total_Earning = 0.00;
                        double _Total_Deduction = 0.00;

                        List<DataRow> drSalaryHeadCollection = null;
                        if (dicEmpSalry.ContainsKey(dtEmpInfo.Rows[i]["EmpSystemID"].ToString()))
                        {
                            drSalaryHeadCollection = dicEmpSalry[dtEmpInfo.Rows[i]["EmpSystemID"].ToString()];
                        }


                        LoadSalaryEarnHead_CurrLess(dtSalaryHeadSheet, out _Total_Earning, "E", drSalaryHeadCollection, ref master);

                        LoadSalaryDeductHead_CurrLess(dtSalaryHeadSheet, out _Total_Deduction, "D", drSalaryHeadCollection, ref master);


                        var result = drSalaryHeadCollection.Where(row => row["HeadCategory"].Equals("Basic")).FirstOrDefault();

                        if (result != null)
                        {
                            _basic = clsStaticInfo.dbl(result["DisbusmentAmount"].ToString());
                        }

                        result = drSalaryHeadCollection.Where(row => row["HeadCategory"].Equals("Net Payable")).FirstOrDefault();
                        if (result != null)
                        {
                            _netPay = clsStaticInfo.dbl(result["DisbusmentAmount"].ToString());
                        }

                        result = drSalaryHeadCollection.Where(row => row["HeadCategory"].Equals("CTC")).FirstOrDefault();
                        if (result != null)
                        {
                            _StrCTC = clsStaticInfo.dbl(result["DisbusmentAmount"].ToString());
                        }

                        double TotalEarning = _Total_Earning;
                        double totDeduction = 0.00;
                        if ((double)_Total_Deduction > 0)
                        {
                            totDeduction = (double)_Total_Deduction;
                        }
                        else
                        {
                            totDeduction = (double)_Total_Deduction * (-1);
                        }
                        master.TotalDeduction = totDeduction;
                        master.NetPay = _netPay;
                        master.StrCTC = _StrCTC;
                        master.TotalEarning = TotalEarning;

                    }

                }

                return JsonConvert.SerializeObject(master, Formatting.Indented);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
       
        public Dictionary<string, string> GetEmployeePFESIC(string profileType, string EmpId)
        {
            string strSQL;
            DataTable dtTable = null;
            Dictionary<string, string> dicPF = new Dictionary<string, string>();
            try
            {
                strSQL = @"SELECT ISNULL(ed.DocNumber,'') DocNumber,ED.EmpSystemID from 
												EmployeeDocument ED 											
												INNER JOIN (SELECT * FROM HKP.ComplianceDocument WHERE ProfileType = '" + profileType + @"') CD ON CD.Id = ED.ComplianceDocumentId where ed.EmpSystemID='" + EmpId + "'";

                dtTable = _sqlRepository.GetDataTable(strSQL);

                for (int i = 0; i < dtTable.Rows.Count; i++)
                {
                    dicPF.Add(dtTable.Rows[i]["EmpSystemID"].ToString(), dtTable.Rows[i]["DocNumber"].ToString());
                }
                return dicPF;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                //objCon = null;
            }
        }//End Function


        public Dictionary<string, List<DataRow>> GetEmpLeaveInfoPaySlip(ParamList leavePara, string EmpId)
        {
            Dictionary<string, List<DataRow>> dicBonus = new Dictionary<string, List<DataRow>>();
            var paraDate = Convert.ToDateTime(leavePara.FromDate);

            DataSet dsRef = null;

            var days = DateTime.DaysInMonth(paraDate.Year, paraDate.Month);//Number of Days in a month
            string monthNameString = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(paraDate.Month);//Month Name from Month No
            var lastDate = days + "-" + monthNameString + "-" + paraDate.Year;
            var firstDate = "1" + "-" + monthNameString + "-" + paraDate.Year;

            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            clsStaticInfo obs = null;
            try
            {
                obs = new clsStaticInfo();

                strSql = @"SELECT 
	                        LTR.EmpSystemID,LT.Code,LT.LeaveType
	                        ,SUM(LTD.LeaveDuration) AvailedLeave	                     
                             FROM  EmployeeInformation  EEI                             
							iNNER join LeaveTransaction LTR  ON EEI.SystemId = LTR.EmpSystemID
                            Inner   JOIN LeaveType LT ON  LTR.LTSystemID = LT.Id 
							inner JOIN (
							 SELECT LTD.LvTrnsSystemID,LTD.LeaveDuration,WorkDate FROM LeaveTransactionDetails LTD WHERE LTd.WorkDate BETWEEN   '" + leavePara.FromDate + @"' AND  '" + leavePara.ToDate + @"'  AND  LTD.IsAvailed = 1
							) LTD ON LTD.LvTrnsSystemID =LTR.SystemID 
							iNNER JOIN
							( SELECT * FROM AttdnProcessData  apd 
						 LEFT JOIN DayType dt on apd.DayStatus = DT.DayType WHERE  WorkDate BETWEEN    '" + leavePara.FromDate + @"' AND  '" + leavePara.ToDate + @"' --AND EEI.PlantID = '202022'
						 ) APD ON   EEI.SystemId = apd.EmpSystemID AND apd.WorkDate = ltd.WorkDate 
							WHERE  LTR.IsApproved = 1 			
                            AND EEI.SystemId='"+EmpId+"' AND EEI.PlantId = '" + leavePara.PlantId + @"'				          
                         GROUP BY LTR.EmpSystemID,LT.Code,LT.LeaveType order by LTR.EmpSystemID,LT.Code";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");

                List<DataRow> _data = new List<DataRow>();
                string empId = "";
                for (int i = 0; i < dsRef.Tables[0].Rows.Count; i++)
                {
                    try
                    {
                        if (empId != dsRef.Tables[0].Rows[i]["EmpSystemID"].ToString())
                        {
                            _data = new List<DataRow>();
                            dicBonus.Add(dsRef.Tables[0].Rows[i]["EmpSystemID"].ToString(), _data);
                        }
                        _data.Add(dsRef.Tables[0].Rows[i]);

                        empId = dsRef.Tables[0].Rows[i]["EmpSystemID"].ToString();
                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }
                }
                return dicBonus;
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


        private void LoadSalaryEarnHead_CurrLess(DataTable dt, out double TotalED, string EorD, List<DataRow> drSalaryHeadCollection, ref MasterPaySlipData master)
        {

            double ColGrsSlr = 0.00;
            TotalED = 0;
            try
            {

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    bool _IsGrossComponent = bplib.clsWebLib.GetBoolData(dt.Rows[i]["IsGrossComponent"].ToString());
                    bool _IsCTCComponent = bplib.clsWebLib.GetBoolData(dt.Rows[i]["IsCTCComponent"].ToString());
                    bool _IsNetPayEffect = bplib.clsWebLib.GetBoolData(dt.Rows[i]["PartOfNetPay"].ToString());
                    double structureValue = 0.00;
                    if (dt.Rows[i]["HeadType"].ToString().ToUpper() == "E".ToUpper() && Convert.ToInt32(dt.Rows[i]["PartOfNetPay"]) == 1 && EorD == "E")
                    {

                        EarningHead eh = new EarningHead();
                        ActEarningHead aeh = new ActEarningHead();
                        ColGrsSlr = 0;
                        string salaryhead = dt.Rows[i]["SalaryHeadLocal"].ToString();

                        structureValue = 0.00;
                        var processedValue = string.Empty;
                        bool isDecimal = true;
                        var decimalNo = 0;

                        var result = drSalaryHeadCollection.Where(row => row["SalaryHeadId"].Equals(dt.Rows[i]["SalaryHeadId"].ToString())).FirstOrDefault();

                        if (result != null)
                        {
                            structureValue = clsStaticInfo.dbl(result["EntryAmount"].ToString());
                            ColGrsSlr = clsStaticInfo.dbl(result["DisbusmentAmount"].ToString());

                            isDecimal = Convert.ToBoolean(result["IntegerInDisb"].ToString());
                            decimalNo = (int)clsStaticInfo.dbl(result["DecimalNo"].ToString());
                        }
                        else
                        {
                            ColGrsSlr = 0.00;
                        }                                                                                                                                          //sheet1.Range[xlsRow, xlsColEarning + 2].NumberFormat = ru.NumberFormatIntLocal(localLanguage);


                        TotalED += ColGrsSlr;

                        if (ColGrsSlr < 0)
                        {
                            ColGrsSlr = ColGrsSlr * (-1);
                        }
                        if (result != null)
                        {

                            eh.Value = ColGrsSlr;
                            eh.Text = salaryhead;

                            aeh.Value = structureValue;
                            aeh.Text = salaryhead;
                        }
                        master.actearning.Add(aeh);
                        master.earning.Add(eh);
                    }

                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void LoadSalaryDeductHead_CurrLess(DataTable dt, out double TotalED, string EorD, List<DataRow> drSalaryHeadCollection, ref MasterPaySlipData master)
        {


            double ColGrsSlr = 0.00;
            DeductionHead dx = new DeductionHead();
            TotalED = 0;
            try
            {

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    bool _IsGrossComponent = bplib.clsWebLib.GetBoolData(dt.Rows[i]["IsGrossComponent"].ToString());
                    bool _IsCTCComponent = bplib.clsWebLib.GetBoolData(dt.Rows[i]["IsCTCComponent"].ToString());
                    bool _IsNetPayEffect = bplib.clsWebLib.GetBoolData(dt.Rows[i]["PartOfNetPay"].ToString());
                    double structureValue = 0.00;

                    if (dt.Rows[i]["HeadType"].ToString().ToUpper() == "D".ToUpper() && EorD == "D")
                    {

                        DeductionHead dh = new DeductionHead();
                        ActDeductionHead adh = new ActDeductionHead();
                        ColGrsSlr = 0;
                        String salaryhead = dt.Rows[i]["SalaryHeadLocal"].ToString();

                        structureValue = 0.00;
                        var processedValue = string.Empty;
                        bool isDecimal = true;
                        var decimalNo = 0;

                        var result = drSalaryHeadCollection.Where(row => row["SalaryHeadId"].Equals(dt.Rows[i]["SalaryHeadId"].ToString())).FirstOrDefault();

                        if (result != null)
                        {
                            structureValue = clsStaticInfo.dbl(result["EntryAmount"].ToString());
                            ColGrsSlr = clsStaticInfo.dbl(result["DisbusmentAmount"].ToString());

                            isDecimal = Convert.ToBoolean(result["IntegerInDisb"].ToString());
                            decimalNo = (int)clsStaticInfo.dbl(result["DecimalNo"].ToString());
                        }
                        else
                        {
                            ColGrsSlr = 0.00;
                            structureValue = 0.00;
                        }
                        if (structureValue < 0)
                        {
                            structureValue = structureValue * (-1);
                        }


                        if (ColGrsSlr < 0)
                        {
                            ColGrsSlr = ColGrsSlr * (-1);
                        }

                        TotalED += ColGrsSlr;

                        if (result != null)
                        {

                            dh.Text = salaryhead;
                            dh.Value = ColGrsSlr;

                            adh.Text = salaryhead;
                            adh.Value = structureValue;
                        }
                        master.actdeduction.Add(adh);
                        master.deduction.Add(dh);

                    }

                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetEmployeeInfoDetailSalaryLogWise(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string EmpId, bool isActive, bool isSeperated, bool isMaternity, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            string salaryProcessId = "";
            var _wc = string.Empty;
            var wcSalaryProcessSystemIdStr = "";


            wcSalaryProcessSystemIdStr = @"SystemID IN( SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = Month('" + fromDate + "') AND YearNo = Year('" + fromDate + "')  )";


            string strSql = @"SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo =  MONTH('" + fromDate + @"') AND YearNo =  YEAR('" + fromDate + @"')";

            DataTable dtSalPrcId = _sqlRepository.GetDataTable(strSql);
            salaryProcessId = "''";
            for (int si = 0; si < dtSalPrcId.Rows.Count; si++)
            {
                salaryProcessId += ",'" + dtSalPrcId.Rows[si]["SystemID"].ToString() + "'";
            }

            string wcEmpStatus = " AND (1=0 ";

            if (isActive == true && isSeperated == true && isMaternity == true)
            {
                wcEmpStatus = " AND (1=1 ";
            }
            else
            {
                if (isActive == true)
                {
                    wcEmpStatus += " OR EmpBasic.EmployeeStatus ='Regular'";
                }
                if (isSeperated == true)
                {
                    wcEmpStatus += " OR EmpBasic.EmployeeStatus ='SEPARATED'";
                }
                if (isMaternity == true)
                {
                    wcEmpStatus += " OR EmpBasic.EmployeeStatus ='MLV_PRE'";

                }
            }

            wcEmpStatus += ")";

            try
            {
                strSQL = @"SELECT EmpBasic.*,MMDSA.*,ISNULL(MW.Grade,'') Grade,ISNULL(MW.SalaryHeadValue,0) MinimumWage
                            FROM
                                    (
									SELECT DISTINCT E.SystemID EmpSystemId,ISNULL(EmployeeCodePreFix,'') EmployeeCodePreFix,ISNULL(EmployeeCodeNumeric,0) EmployeeCodeNumeric,E.GroupID CompanyGroupId,E.CompanyId, E.EmployeeCode, E.EmployeeName, E.EmployeeStatus EmployeeStatusReal,E.EmployeeCurrentStatus
											, DG.UserName DesignationGroupName, E.DesignationSystemID, DE.UserName DesignationName,
											'' UserGroupSystemID,  F.Id PlantID, F.UserName PlantName, 
											FU.UserName UnitName,  DV.UserName DivisionName,  DP.UserName DepartmentName,
											 S.UserName SectionName, E.SubSectionID, SS.UserName SubSectionName, E.EmployeeCategorySystemID,
											EC.UserName EmpCategoryName,EC.WorkingDaysInAMonth--, BK.BankNameShort BankName, BK.BankNameFull, E.BankAccNo
                                            ,egdsgg.GivenDesignationGroup,e.SalaryRuleMasterSystemID,Format(E.DOJ,'dd-MMM-yyyy') DOJ,Format(E.DOS,'dd-MMM-yyyy') DOS,Format(E.DOB,'dd-MMM-yyyy') DOB
											,ISNULL(LDS.UserName,'') LegalDesignation,ISNULL(E.NationalID,'') NationalID
											,ISNULL(Line.UserName,'') LineName
											,ISNULL(E.GenderID,'') Gender
                                            ,ISNULL(LSalGr.Code,'') GradeCode
											,ISNULL(PG.UserName,'') PayRollGroup
                                    , CASE WHEN ISNULL(SPM.SalaryProcFlag,'') = '' THEN 'Regular' ELSE SalaryProcFlag END EmployeeStatus
                                    ,ISNULL(jl.JobLocation, '') JobLocation
									,ISNULL(SPLD.PaymentMode,'') PaymentMode
									,ISNULL(bb.UserName,'') BankName
                                    ,ISNULL(spld.BankAccNo,'') BankAccNo
                                    ,ISNULL(spld.IFSCCode,'') IFSCCode
                                    ,CASE WHEN ISNULL(PO.IsDirect,0) = 0 THEN 'No' ELSE 'Yes' END IsDirect
                                    ,CASE WHEN ISNULL(PO.DirectManpowerCost,0) = 0 THEN 'No' ELSE 'Yes' END DirectManpowerCost

                                     FROM EmployeeInformation E
                                    JOIN (
                                    SELECT DISTINCT EmpInfoSystemID,SlrProcMstSystemID,PlantID ,m.Description,m.SalaryProcFlag
                                    FROM SalaryProcChild c
                                    JOIN SalaryProcMaster m on m.SystemID=c.SlrProcMstSystemID
                                    WHERE SlrProcMstSystemID in (SELECT systemid FROM SalaryProcMaster WHERE MonthNo= MONTH('" + fromDate + @"') AND YearNo=YEAR('" + toDate + @"'))
                                    AND PlantID = '" + plantId + @"'
                                    ) SPM ON spm.EmpInfoSystemID=e.SystemId
									 JOIN SalaryProcessLogDetail SPLD ON SPLD.SalaryProcessId  IN(" + salaryProcessId + @") AND e.SystemId = SPLD.EmpSystemId  --SPLD.SalaryProcessId = SPM.SystemId AND SPC.EmpInfoSystemID = SPLD.EmpSystemId and SPLD.PlantId = '202022' 
                         
									 			LEFT JOIN ORG.Plant F ON SPLD.PlantID = F.Id
												LEFT JOIN hkp.DesignationGroup DG ON E.DesignationGroupId = DG.ID
												LEFT JOIN hkp.Designation DE ON E.GivenDesignationId = DE.Id
												LEFT JOIN hkp.LegalDesignation LDS ON SPLD.LegalDesignationId = LDS.Id
								LEFT OUTER JOIN [MST].[ManpowerBudget] AS MB  on MB.Id = SPLD.BudgetCode
								LEFT OUTER JOIN [ORG].[Position] AS PO ON PO.Id = MB.PositionId
                                LEFT OUTER JOIN [ORG].[Entity] AS ENT ON ENT.Id = MB.EntityId

												LEFT JOIN [ORG].[Line] ON Line.Id = MB.LineId
												  LEFT JOIN [dbo].[JobLocation] jl on jl.SystemID = E.JobLocationID
												  LEFT JOIN [dbo].[EmployeeBankInfo] ebi on ebi.EmpSystemID=e.SystemId
									LEFT JOIN [HKP].[Bank] bb on bb.Id = SPLD.BankSystemID
                                    LEFT OUTER JOIN MST.PayrollGroupMaster PGM ON PGM.employeeid = E.SystemId

									LEFT OUTER JOIN HKP.PayrollGroup PG ON PG.id = PGM.PayrollGroupId
                                                LEFT JOIN MST.LegalSalaryGradeDesignation LSGD ON LSGD.LegalDesignationId = LDS.Id and E.PlantId = LSGD.PlantId
                                                LEFT JOIN SCS.LegalSalaryGrade LSalGr ON LSalGr.Id = SPLD.LegalSalaryGradeId  --and SPLD.PlantId = LSalGr.PlantId
												
												LEFT JOIN org.Unit FU ON ENT.UnitID = FU.Id
												LEFT JOIN org.Division DV ON PO.DivisionID = DV.Id
												LEFT JOIN org.Department DP ON PO.DepartmentID = DP.Id
												LEFT JOIN org.Section S ON PO.SectionID = S.Id
												LEFT JOIN org.SubSection SS ON PO.SubSectionID = SS.Id

												LEFT JOIN
                                                --hkp.EmployeeCategory EC ON E.EmployeeCategorySystemID = EC.Id
            --                                    (
            --                                    SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
												--LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
												--)EC ON EC.DesignationId=E.GivenDesignationId
												[HKP].[EmployeeCategory] EC ON EC.Id = SPLD.EmployeeCategoryId
												LEFT JOIN (SELECT dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									            ,dg.UserName GivenDesignationGroup
									            FROM MST.DesignationMaster dm
									            LEFT JOIN HKP.DesignationGroup dg on dg.Id=dm.DesignationGroupId
									            ) egdsgg ON egdsgg.DesignationId=e.GivenDesignationId
									            AND egdsgg.EmployeeCategoryId=SPLD.EmployeeCategoryId

                                      --Where SPC.SlrProcMstSystemID IN( SELECT SystemID FROM SalaryProcMaster
                                      --WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        --WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        --AND MonthNo =   MONTH('" + fromDate + @"') AND YearNo =  YEAR('" + fromDate + @"')   )   
									) EmpBasic
                                   LEFT JOIN 
													(
													 SELECT E.SystemID, SUM(SV.SalaryHeadValue) SalaryHeadValue,LSG.UserName Grade
														FROM EmployeeInformation E   
																LEFT JOIN MST.ManpowerBudget b ON e.BudgetCode = b.Id
																LEFT JOIN MST.LegalSalaryGradeDesignation GD ON GD.LegalDesignationId = E.LegalDesignationId 
                                                                                                AND E.PlantId = gd.PlantId
																LEFT JOIN (
																			SELECT MAX(EffectiveDate) EffectiveDate, LegalSalaryGradeId, EmployeeLocationId 
																				FROM MST.LegalSalaryStructure 
																				WHERE EffectiveDate <= '" + fromDate + @"'
																			GROUP BY LegalSalaryGradeId, EmployeeLocationId 
																		  ) S ON S.LegalSalaryGradeId = GD.LegalSalaryGradeId AND S.EmployeeLocationId = B.EmployeeLocationId
																LEFT JOIN MST.LegalSalaryStructure SS ON SS.LegalSalaryGradeId = S.LegalSalaryGradeId 
                                                                                            AND SS.EmployeeLocationId = S.EmployeeLocationId 
                                                                                            AND SS.EffectiveDate = S.EffectiveDate
																LEFT JOIN MST.LegalSalaryStructureValue SV ON SV.LegalSalaryStructureId = SS.Id 	
                                                                left join  [SCS].[LegalSalaryGrade] LSG ON LSG.Id=S.LegalSalaryGradeId	
														GROUP BY E.SystemId,LSG.UserName
													) MW ON MW.SystemId = EmpBasic.EmpSystemId
                                    INNER JOIN
		                                    (
													SELECT EmpSystemID,MonthNo,YearNo, ISNULL(TotalProcDate,0) TotalProcDate,IsNULL(TotalPresent,0) TotalPresent,ISNULL(TotalLate,0) TotalLate,ISNULL(TotalAbsent,'') TotalAbsent
										,ISNULL(TotalLv,0) TotalLv
										,ISNULL(TotalMLv,0) TotalMLv,ISNULL(TotalCompAssignLv,0) TotalCompAssignLv,ISNULL(TotalWeekOff,0) +  ISNULL(TotalWeekOffHoliDay,0) TotalWeekOff, ISNULL(TotalWeekOffHoliDay,0) TotalWeekOffHoliDay
										,ISNULL(TotalOTHr,0) TotalOTHr,ISNULL(TotalNormalOTHr,0) TotalNormalOTHr,ISNULL(TotalExtraOTHr,0) TotalExtraOTHr,ISNULL(WeekOffOTHr,0) WeekOffOTHr
										,ISNULL(HoliDayOTHr,0) HoliDayOTHr,ISNULL(TotalLWP,0) TotalLWP,ISNULL(IsOTEntitled,0) IsOTEntitled,ISNULL(OTRate,0) OTRate,ISNULL(TotalHoliDay,0) TotalHoliDay
										  FROM SalaryProceAttdnData MMDSA where MMDSA.MonthNo = MONTH('" + fromDate + @"') AND
						                               MMDSA.YearNo = YEAR('" + fromDate + @"') AND MMDSA.PlantID = '" + plantId + @"' 
											) MMDSA ON EmpBasic.EmpSystemID = MMDSA.EmpSystemID 
                                            WHERE EmpBasic.CompanyGroupId = '" + companyGroupId + @"'  AND EmpBasic.PlantId ='" + plantId + @"' " + wcEmpStatus + @"";

                strSQL += @"and EmpBasic.EmpSystemId='" + EmpId + "'";



                strSQL += @"Order by EmpBasic.EmployeeCodePreFix,EmpBasic.EmployeeCodeNumeric ";

                ConnectionManager.clsConnectionManager con = new clsConnectionManager(600);
                con.getDataSet(strSQL, out dsRef);


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


        public void GetExtraAbsent(string plantid, string EmpId, int smonth, int syear, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT WorkingDate,EmpSystemID
                              FROM [SCS].[WeeklyAbsentismAssignment]
                              where EmpSystemID='" + EmpId + "'and month(WorkingDate)=" + smonth + " and YEAR(WorkingDate)=" + syear + " and plantid='" + plantid + @"' 
                            union

                            SELECT WorkDate WorkingDate,EmpSystemID
                              FROM [trn].[HolidayAbsentismAssignment]
                              where EmpSystemID='" + EmpId + "' and month(WorkDate)=" + smonth + " and YEAR(WorkDate)=" + syear + " and plantid='" + plantid + @"'
                     ";

                //strSql += @" AND EmpSystemID='" + EmpId + "'"; 

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
        }//end function


        public void GetGrade(string EmpId, string sMth, string sYr, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            var _wc = string.Empty;


            try
            {

                strSQL = @"SELECT E.SystemID, SUM(SV.SalaryHeadValue) SalaryHeadValue,LSG.ShortName Grade
                            FROM EmployeeInformation E   
                                    LEFT JOIN MST.ManpowerBudget b ON e.BudgetCode = b.Id
                                    left Join MST.PayrollGroupMaster PGM on PGM.EmployeeId = E.EmployeeId
                                    LEFT JOIN MST.LegalSalaryGradeDesignation GD ON GD.LegalDesignationId = E.LegalDesignationId 
                                                                        AND E.PlantId = gd.PlantId
                                    LEFT JOIN (
                                           		SELECT MAX(EffectiveDate)EffectiveDate, LegalSalaryGradeId, EmployeeLocationId 
                                           			FROM MST.LegalSalaryStructure 
                                           			WHERE EffectiveDate <= GETDATE()
                                           		GROUP BY LegalSalaryGradeId, EmployeeLocationId 
                                           		) S ON S.LegalSalaryGradeId = GD.LegalSalaryGradeId AND S.EmployeeLocationId = B.EmployeeLocationId
                                    LEFT JOIN MST.LegalSalaryStructure SS ON SS.LegalSalaryGradeId = S.LegalSalaryGradeId 
                                                                    AND SS.EmployeeLocationId = S.EmployeeLocationId 
                                                                    AND SS.EffectiveDate = S.EffectiveDate
                                    LEFT JOIN MST.LegalSalaryStructureValue SV ON SV.LegalSalaryStructureId = SS.Id 																							
                                    LEFT JOIN  [SCS].[LegalSalaryGrade] LSG ON LSG.Id =S.LegalSalaryGradeId 
                                    WHERE MONTH(DOJ) <= '" + sMth + @"' AND Year(DOJ) <= '" + sYr + @"'";


                strSQL += @"and E.SystemID='" + EmpId + "'";


                strSQL += @"GROUP BY E.SystemId,LSG.ShortName";

                //if (pPayGrp != "ALL")
                //{
                //    strSQL = strSQL + @" WHERE EmpBasic.PlantID = '" + pPayGrp + @"'";
                //}

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Dictionary<string, List<DataRow>> GetEmployeeSalaryInfoDetailPaySlip(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string languageId, string EmpId, bool isActive, bool isSeperated, bool isMaternity, out DataTable distinctSalaryHead)
        {
            string strSQL;
            DataSet dsRef = null;
            Dictionary<string, List<DataRow>> dicBonus = new Dictionary<string, List<DataRow>>();
            distinctSalaryHead = new DataTable("Tmp");
            try
            {

                string wcEmpStatus = " Where (1=0 ";

                if (isActive == true && isSeperated == true && isMaternity == true)
                {
                    wcEmpStatus = " Where (1=1 ";
                }
                else
                {
                    if (isActive == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='Regular' ";
                    }
                    if (isSeperated == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='SEPARATED'";
                    }
                    if (isMaternity == true)
                    {
                        wcEmpStatus += " OR SalaryProcFlag ='MLV_PRE'";

                    }
                }
                DataTable dtslProcId = _sqlRepository.GetDataTable(@" SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = Month('" + toDate + @"') AND YearNo = Year('" + toDate + @"') ");
                string inSalaryProcParam = "' '";

                for (int i = 0; i < dtslProcId.Rows.Count; i++)
                {
                    inSalaryProcParam += ",'" + dtslProcId.Rows[i]["SystemID"].ToString() + "'";
                }

                wcEmpStatus += ")";

                strSQL = @"SELECT EmpSlr.*,ISNULL(PSH.Sequence,99) Sequence,ISNULL(crc.IsDecimalInDisb,0) IsDecimalInDisb,ISNULL(CRC.IntegerInDisb,1) IntegerInDisb,ISNULL(CRC.DecimalNo,0) DecimalNo FROM(SELECT SPC.SystemID AS SlrProcChdSysID, SPC.SlrProcMstSystemID, SPM.SalaryProcID, SPM.FromDate, SPM.ToDate,
                                                    SPC.EmpInfoSystemID , SPC.PlantID, SPM.UserGroupSystemID, SPM.MonthNo, SPM.YearNo, SPC.PayAbleShSystemID,
                                                    SPC.SalaryHeadID, SPC.EntryCurrencyID, SPC.EntryAmount, SPC.DefineCurrencyID, SPC.DefineAmount,
                                                    SPC.DisbusmentCurrencyID, SPC.DisbusmentAmount, SPC.AcltExcDisbSlrHDID, SPC.AcltExcDisbSlrHDAmt,
                                                    CRE.Name AS PlantWiseExchangeCR, EXR.ToCurrencyBuying ExchangeRate, SPM.AmtDefinitionCurrencyID,
                                                    CR.Name AS AmtDefinitionCurrency, SPM.AmtDefinitionCurrencyRate, SPC.IsNetPayEffect, ISNULL(SH.IsCTCComponent,0) IsCTCComponent, ISNULL(SH.IsGrossComponent,0) IsGrossComponent
                                                    , sh.SalaryHead,ISNULL(ISNULL(BSH.Name,SH.SalaryHead),'') SalaryHeadLocal, sh.HeadCategory, sh.HeadType, ISNULL(SH.PartOfNetPay,0) PartOfNetPay
													, Case when Isnull(SalaryProcFlag,'') = '' THen 'Regular' else SalaryProcFlag end SalaryProcFlag
                                     FROM SalaryProcChild SPC

                                        left JOIN SalaryProcMaster SPM ON SPC.SlrProcMstSystemID = SPM.SystemID
                                                        LEFT JOIN SalaryHead sh on sh.SalaryHeadID=spc.SalaryHeadID
                                                        LEFT JOIN (SELECT * FROM HKP.LocalLanguage WHERE SalaryHeadId IS NOT NULL AND LanguageId = '" + languageId + @"') AS BSH ON BSH.SalaryHeadId = sh.SalaryHeadID --BanglaSalaryHead
                                                        LEFT JOIN scs.Currency CR ON SPM.AmtDefinitionCurrencyID = CR.Id
                                                        LEFT JOIN (
                                                                   SELECT* FROM ExchangerateDateWiseForHR
                                                                   WHERE FromDate IN (SELECT MAX(FromDate) FromDate FROM SalaryProcMaster
                                                                                                           WHERE SystemID IN(" + inSalaryProcParam + @")
																  )) EXR ON SPM.AmtDefinitionCurrencyID = EXR.FromCurrencyCode
                                                                                            AND SPC.PlantID = Exr.PlantID
                                                        LEFT JOIN SCS.Currency CRE ON EXR.FromCurrencyCode = CRE.Id

                                                        WHERE ISNULL(SPC.SlrProcMstSystemID,'')  IN(" + inSalaryProcParam + @")) EmpSlr--ON EmpBasic.SystemID = EmpSlr.EmpInfoSystemID AND EmpBasic.PlantID = EmpSlr.PlantID

                                            Inner join EmployeeInformation EEI ON EEI.SystemId = EmpSlr.EmpInfoSystemID

                                         LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = EEI.SalaryRuleMasterSystemID

                                        LEFT JOIN SalaryRuleGeneral SRG ON SRG.SalaryRuleMasterSystemID = SRM.SystemID  AND SRG.SalaryHeadID = EmpSlr.SalaryHeadID
                                        LEFT JOIN(SELECT* FROM [MST].[PlantSalaryHeadSequence] WHERE PlantId = '" + plantId + @"') PSH
                                                                       ON PSH.SalaryHeadId = EmpSlr.SalaryHeadID
                                        LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID AND CRC.SalaryHeadID = EmpSlr.SalaryHeadID

                                                WHERE EEI.GroupID = '" + companyGroupId + @"' AND  EmpSlr.PlantId = '" + plantId + @"'";

                strSQL += @" AND EmpSlr.EmpInfoSystemID='" + EmpId + "'";


                strSQL += "ORDER BY EmpSlr.EmpInfoSystemID,Sequence";

                ConnectionManager.clsConnectionManager con = new clsConnectionManager(600);
                con.getDataSet(strSQL, out dsRef);

                distinctSalaryHead = dsRef.Tables[0].DefaultView.ToTable(true, "SalaryHeadID", "SalaryHead", "SalaryHeadLocal", "HeadType", "Sequence", "HeadCategory", "IntegerInDisb", "DecimalNo", "IsGrossComponent", "IsCTCComponent", "PartOfNetPay");
                distinctSalaryHead.DefaultView.Sort = "Sequence";
                distinctSalaryHead = distinctSalaryHead.DefaultView.ToTable();

                DataTable dt = dsRef.Tables[0];
                List<DataRow> _data = new List<DataRow>();
                string empId = "";
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    if (empId != dt.Rows[i]["EmpInfoSystemID"].ToString())
                    {
                        _data = new List<DataRow>();
                        dicBonus.Add(dt.Rows[i]["EmpInfoSystemID"].ToString(), _data);
                    }
                    _data.Add(dt.Rows[i]);

                    empId = dt.Rows[i]["EmpInfoSystemID"].ToString();
                }
                return dicBonus;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                //objCon = null;
            }
        }//End Function

        public void SelectedPlantWiseCompany(string sPlantID, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT p.UserName PlantName,c.UserName CompanyName ,ISNULL(a.Address1,'')+','+ ISNULL(a.Address2,'') Address1, a.Phone,a.Email
                                ,cm.Address1 cAddress1 ,cm.Address2 cAddress2
                                FROM org.Plant p
							LEFT OUTER JOIN org.Company c on c.Id=p.CompanyId
							LEFT OUTER JOIN mst.AddressMaster a on a.Id=p.AddressMasterId
							LEFT OUTER JOIN mst.AddressMaster cm on cm.Id=c.AddressMasterId
							WHERE p.Id='" + sPlantID + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end of function

        public void SelectedPlant(string sPlantID, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT P.UserName,AM.Address1+','+ ISNULL(AM.Address2,'') Address1 FROM ORG.Plant P
                            LEFT OUTER JOIN MST.AddressMaster AM ON P.AddressMasterId=AM.Id
                             WHERE P.Id = '" + sPlantID + @"'";
                //strSql = @"SELECT * FROM ORG.Plant WHERE Id = '" + sPlantID + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
            }
        }//end of function

    }


    public class LeaveMaster
    {
        public string Text { get; set; }
        public double Value { get; set; }
    }
    public class EarningHead
    {
        public string Text { get; set; }
        public double Value { get; set; }
    }
    public class ActEarningHead
    {
        public string Text { get; set; }
        public double Value { get; set; }
    }

    public class ActDeductionHead
    {
        public string Text { get; set; }
        public double Value { get; set; }
    }
    public class DeductionHead
    {
        public string Text { get; set; }
        public double Value { get; set; }
    }
    public class MasterPaySlipData
    {
        public List<ActEarningHead> actearning = new List<ActEarningHead>();
        public List<EarningHead> earning = new List<EarningHead>();

        public List<ActDeductionHead> actdeduction = new List<ActDeductionHead>();
        public List<DeductionHead> deduction = new List<DeductionHead>();

        public List<LeaveMaster> Lvx = new List<LeaveMaster>();

        public string TotalPresent { get; set; }
        public string TotalAbsent { get; set; }
        public double PayDay { get; set; }
        public string TotalWeekOff { get; set; }
        public string TotalHoliDay { get; set; }
        public string TotalLv { get; set; }
        public double LWP { get; set; }
        public double TotalDeduction { get; set; }
        public double TotalEarning { get; set; }
        public double NetPay { get; set; }
        public double StrCTC { get; set; }
        public string EmployeeStatus { get; set; }
        public string EmployeeCode { get; set; }
        public string DOJ { get; set; }
        public string DOB { get; set; }
    }
}






