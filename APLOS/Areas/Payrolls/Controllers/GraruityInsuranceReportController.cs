using Aplos.Controllers;
using Aplos.Properties;
//using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Biometrics;
using Library.Model.HumanResources;
using Library.Service.Biometrics;
using Library.Service.Employees;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using OTSBD;
using Syncfusion.DocIO.DLS;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Payrolls.Controllers
{
    public class GraruityInsuranceReportController : BaseController
    {
        #region Constructor


        private readonly ISqlRepository _sqlRepository;

        public GraruityInsuranceReportController(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region -- Pages
       
        public ActionResult Aplos()
        {
            return View();
        }


        #endregion -- Pages

        #region -- Operations

        [HttpGet, Authorize]
        public ActionResult GetGratuityInsuranceAgreement()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string sql = @"SELECT * FROM GratuityInsuranceAgreement";

                var data = _sqlRepository.GetDataCollection(sql);
                JsonResult json = Json(data, JsonRequestBehavior.AllowGet);
                json.MaxJsonLength = int.MaxValue;
                return json;
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        [HttpGet, Authorize]
        public ActionResult GetData(string AgreementId, string FromDate, string ToDate)
        {
            try
            {
                #region declare
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;



                #endregion

                List<IndividualGratuityPolicyModel> ob = new List<IndividualGratuityPolicyModel>();
                DataSet dsData;
                DataSet dsSalHd;
                DataSet dsSalaryDataEmpWise;
                DataSet dsGratuityInsuranceAgreement;
                DataSet dsCompanyInfo;
                ConnectionManager.DAL.ConManager objCon;
                try
                {






                    string strGratuityInsuranceAgreementSQL = @"SELECT AgreementNo,Format(AgreementDate,'dd-MMM-yyyy') AgreementDate,InsuranceCompanyName,Branch                      
                            FROM  GratuityInsuranceAgreement                            
                            WHERE Id='" + AgreementId + @"'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(strGratuityInsuranceAgreementSQL, out dsGratuityInsuranceAgreement, false, "1");

                    string strSQL = @"SELECT ROW_NUMBER() OVER(ORDER BY EI.EmployeeCodePreFix,EI.EmployeeCodeNumeric ) AS  SNO
                            ,EI.SystemId,EI.EmployeeCode,EI.EmployeeName
                            ,FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
                            ,FORMAT(EI.DOS,'dd-MMM-yyyy') DOS
                            ,FORMAT(EI.DOB,'dd-MMM-yyyy') DOB
                            ,DateDiff(day,EI.doj,Isnull(EI.dos,CONVERT(date,'" + ToDate + @"')))+1 TenureInDays
                            ,IGP.PolicyNo
                            ,IGP.EmployeeSystemId
                            ,GA.AgreementNo
                            ,GA.AgreementDate
                            ,GA.Branch
                        
                            ,ISNULL(FS.GratuityAmount,0) GratuityAmount
                            ,ISNULL(FS.BasicAmount,0) BasicAmount
							,ISNULL(FS.GrossAmount,0) GrossAmount
                            FROM IndividualGratuityPolicy IGP
                            LEFT JOIN GratuityInsuranceAgreement GA ON GA.Id=IGP.AgreementId
                            LEFT JOIN EmployeeInformation EI on EI.SystemId=IGP.EmployeeSystemId
                            LEFT JOIN EmployeeFinalSettlement FS ON FS.EmpSystemId=IGP.EmployeeSystemId
                            WHERE IGP.AgreementId='" + AgreementId + @"'  AND EI.PlantId='" + identity.PlantId + @"' ---AND ISNULL(FS.GratuityAmount,0)>0   
                            AND EI.DOS BETWEEN '" + FromDate + @"' AND '" + ToDate + @"'                           
                            ORDER BY EI.EmployeeCodePreFix,EI.EmployeeCodeNumeric";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(strSQL, out dsData, false, "1");


                    Dictionary<string, DataRow> DicData = new Dictionary<string, DataRow>();
                    for (int i = 0; i < dsData.Tables[0].Rows.Count; i++)
                    {
                        DicData.Add(dsData.Tables[0].Rows[i]["SystemID"].ToString(), dsData.Tables[0].Rows[i]);
                    }

                    //if (string.IsNullOrEmpty(ToDate))
                    //{
                    //    ToDate = DateTime.Now.ToString("dd-MMM-yyyy");
                    //}
                    //List<SPSalaryHead> dicSalaryHead = new List<SPSalaryHead>();
                    //GetSalaryHead(out dsSalHd);
                    //DataView dvsh = new DataView(dsSalHd.Tables[0]);
                    //DataTable dtSalHdx = dvsh.ToTable(true, "SalaryHeadID");

                    //if (dtSalHdx.Rows.Count > 0)
                    //    dicSalaryHead = dtSalHdx.ToList<SPSalaryHead>();


                    //GetMultipleEmployeeSalaryData(identity.PlantId, FromDate, out dsSalaryDataEmpWise);
                    //Dictionary<string, List<DataRow>> DicAllEmpSalaryInfo = new Dictionary<string, List<DataRow>>();

                    //string _empId = "";
                    //List<DataRow> _data = new List<DataRow>();
                    //for (int i = 0; i < dsSalaryDataEmpWise.Tables[0].Rows.Count; i++)
                    //{
                    //    if (_empId != dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString())
                    //    {
                    //        _data = new List<DataRow>();
                    //        DicAllEmpSalaryInfo.Add(dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString(), _data);
                    //        _empId = dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString();
                    //    }
                    //    _data.Add(dsSalaryDataEmpWise.Tables[0].Rows[i]);
                    //}


                    foreach (string key in DicData.Keys)
                    {
                        if (key == "1800085")
                        {

                        }
                        List<SPvalueHeadWise> dtValue = null;
                        DataRow dr = DicData[key];


                        if (DicData.ContainsKey(key) == true)
                        {
                            //DataRow drData = DicData[key];
                            //if (drData["EncashmentBasis"].ToString() == "DOJ" && !string.IsNullOrEmpty(drData["LvEncashmentFormulaDesID"].ToString()))
                            //{


                            IndividualGratuityPolicyModel o = new IndividualGratuityPolicyModel();
                            int SNO = 1;
                            o.SNo = dr["SNO"].ToString();
                            o.EmpSystemId = dr["SystemID"].ToString();
                            o.EmployeeCode = dr["EmployeeCode"].ToString();
                            o.EmployeeName = dr["EmployeeName"].ToString();
                            //o.LeaveType = dr["LeaveType"].ToString();

                            //o.CheckBoxSelect = Convert.ToBoolean(dr["CheckBoxSelect"].ToString());
                            o.DOJ = dr["DOJ"].ToString();
                            o.DOB = dr["DOB"].ToString();
                            o.DOS = dr["DOS"].ToString();
                            o.PolicyNo = dr["PolicyNo"].ToString();
                            o.TenureInDays = Convert.ToInt32(dr["TenureInDays"].ToString());
                            o.BasicAmount = Convert.ToDecimal(string.Format("{0:F2}", dr["BasicAmount"]));
                            o.GratuityAmount = Convert.ToDecimal(string.Format("{0:F2}", dr["GratuityAmount"]));
                            //o.GratuityRate = Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult));
                            //o.GratuityNumberOfYears = GratuityNumberOfYears;




                            //if (DicAllEmpSalaryInfo.ContainsKey(dr["SystemID"].ToString()) == false)
                            //    continue;
                            //List<DataRow> salaryStructure = DicAllEmpSalaryInfo[dr["SystemID"].ToString()];
                            #region Create Table                    
                            dtValue = new List<SPvalueHeadWise>();
                            #endregion Create Table

                            //for (int j = 0; j < salaryStructure.Count; j++)
                            //{
                            //    SPvalueHeadWise sp = new SPvalueHeadWise();
                            //    sp.SalaryHeadID = salaryStructure[j]["SalaryHeadID"].ToString().Trim();
                            //    sp.EntryCurrencyID = salaryStructure[j]["EntryCurrencyID"].ToString().Trim();
                            //    sp.EntryAmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                            //    dtValue.Add(sp);
                            //    if (salaryStructure[j]["HeadCategory"].ToString().ToUpper().Trim() == "BASIC")
                            //    {
                            //        o.BasicAmount = Convert.ToDecimal(salaryStructure[j]["EntryAmount"]);
                            //    }
                            //    //if (salaryStructure[j]["HeadCategory"].ToString().ToUpper().Trim() == "GROSS")
                            //    //{
                            //    //    o.GrossAmmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                            //    //}
                            //}
                           




                            //TimeSpan DaysNo = TimeSpan.FromDays(Convert.ToInt32(dr["TenureInDays"].ToString()));
                            //DateTime zeroTime = new DateTime(1, 1, 1);
                          
                            //DateTime Now;
                            //if (!string.IsNullOrEmpty(dr["DOS"].ToString()))
                            //{
                            //    Now = Convert.ToDateTime(dr["DOS"].ToString());
                            //}
                            //else
                            //{
                            //    Now = Convert.ToDateTime(ToDate.ToString());
                            //}

                            //int years = new DateTime(DateTime.Now.Subtract(Convert.ToDateTime(dr["DOJ"].ToString())).Ticks).Year - 1;
                            //DateTime PastYearDate = (Convert.ToDateTime(dr["DOJ"].ToString())).AddYears(years);

                            //int month = 0;
                            //for (int i = 1; i <= 12; i++)
                            //{
                            //    if (PastYearDate.AddMonths(i) == Now)
                            //    {
                            //        month = i;
                            //        break;
                            //    }
                            //    else if (PastYearDate.AddMonths(i) >= Now)
                            //    {
                            //        month = i - 1;
                            //        break;
                            //    }
                            //}
                            //int days = Now.Subtract(PastYearDate.AddMonths(month)).Days + 1;
                            //int Hours = Now.Subtract(PastYearDate).Hours;
                            //int Minutes = Now.Subtract(PastYearDate).Minutes;
                            //int Seconds = Now.Subtract(PastYearDate).Seconds;








                            //o.TenureDayNo = days;
                            //o.TenureMonthNo = month;
                            //o.TenureYearNo = years;




                            //Calculate Gratuity
                            //string _formulaValueG = "0";
                            //string sFormulaResult = "0";
                            //int GratuityNumberOfYears = 0;
                            //DataSet dsGratuityPolicy = null;
                            //GetGratuityPolicy(identity.PlantId, out dsGratuityPolicy);

                            //DataView dvGratuityPolicy = new DataView(dsGratuityPolicy.Tables[0]);
                            //dvGratuityPolicy.RowFilter = "MaturityFromYear<= " + years + " and " + years + "<= MaturityToYear";
                            //if (dvGratuityPolicy.Count > 0)
                            //{
                            //    if (Convert.ToBoolean(dvGratuityPolicy[0]["IsSubSequentRoudingSixMonth"]))
                            //    {
                            //        if (month > 6)
                            //        {
                            //            GratuityNumberOfYears = years + 1;
                            //        }
                            //        else if (month == 6 && days > 0)
                            //        {
                            //            GratuityNumberOfYears = years + 1;
                            //        }
                            //        else
                            //        {
                            //            GratuityNumberOfYears = years;
                            //        }
                            //    }
                            //    else
                            //    {
                            //        GratuityNumberOfYears = years;
                            //    }

                            //    DataView dvGratuityPolicyTemp = new DataView(dsGratuityPolicy.Tables[0]);
                            //    dvGratuityPolicyTemp.RowFilter = "MaturityFromYear<= " + GratuityNumberOfYears + " and " + GratuityNumberOfYears + "<= MaturityToYear";
                            //    if (dvGratuityPolicy.Count > 0)
                            //    {
                            //        //obSSrecal.ReLoadFormulaWithValue(dvGratuityPolicyTemp[0]["MaturityFormulaDesID"].ToString(), ref dtValue, dsSalaryData.Tables[0].Rows[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValueG, ref dtSlrHd);
                            //        ReLoadFormulaWithGrossValueNew(dvGratuityPolicyTemp[0]["MaturityFormulaDesID"].ToString(), salaryStructure[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValueG, dtValue, dicSalaryHead);

                            //        sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValueG).ToString();
                            //    }

                            //}
                            //dvGratuityPolicy.RowFilter = null;

                            //o.GratuityAmount = Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult)) * GratuityNumberOfYears;
                            //o.GratuityRate = Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult));
                            //o.GratuityNumberOfYears = GratuityNumberOfYears;






















                            ob.Add(o);


                        }
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







                //"GratuityInsuranceAgreement.docx";


                //GratuityInsuranceAgreementSummary.docx


                AllReport("GratuityInsuranceAgreement.docx", dsGratuityInsuranceAgreement, ob, "GratuityInsuranceAgreement.docx");
                //Report("GratuityInsuranceAgreementSummary.docx", dsGratuityInsuranceAgreement, ob, "GratuityInsuranceAgreementSummary.docx");
                return null;

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }

        [HttpGet, Authorize]
        public ActionResult xGetData(string AgreementId, string FromDate, string ToDate)
        {
            try
            {
                #region declare
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;



                #endregion

                List<IndividualGratuityPolicyModel> ob = new List<IndividualGratuityPolicyModel>();
                DataSet dsData;
                DataSet dsSalHd;
                DataSet dsSalaryDataEmpWise;
                DataSet dsGratuityInsuranceAgreement;
                DataSet dsCompanyInfo;
                ConnectionManager.DAL.ConManager objCon;
                try
                {






                    string strGratuityInsuranceAgreementSQL = @"SELECT AgreementNo,Format(AgreementDate,'dd-MMM-yyyy') AgreementDate,InsuranceCompanyName,Branch                      
                            FROM  GratuityInsuranceAgreement                            
                            WHERE Id='" + AgreementId + @"'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(strGratuityInsuranceAgreementSQL, out dsGratuityInsuranceAgreement, false, "1");

                    string strSQL = @"SELECT ROW_NUMBER() OVER(ORDER BY EI.EmployeeCodePreFix,EI.EmployeeCodeNumeric ) AS  SNO
                            ,EI.SystemId,EI.EmployeeCode,EI.EmployeeName
                            ,FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
                            ,FORMAT(EI.DOS,'dd-MMM-yyyy') DOS
                            ,FORMAT(EI.DOB,'dd-MMM-yyyy') DOB
                            ,DateDiff(day,EI.doj,Isnull(EI.dos,CONVERT(date,'" + ToDate + @"')))+1 TenureInDays
                            ,IGP.PolicyNo
                            ,IGP.EmployeeSystemId
                            ,GA.AgreementNo
                            ,GA.AgreementDate
                            ,GA.Branch
                        
                            ,FS.GratuityAmount
                            ,FS.BasicAmount
							,FS.GrossAmount
                            FROM IndividualGratuityPolicy IGP
                            LEFT JOIN GratuityInsuranceAgreement GA ON GA.Id=IGP.AgreementId
                            LEFT JOIN EmployeeInformation EI on EI.SystemId=IGP.EmployeeSystemId
                            LEFT JOIN EmployeeFinalSettlement FS ON FS.EmpSystemId=IGP.EmployeeSystemId
                            WHERE IGP.AgreementId='" + AgreementId + @"'  AND EI.PlantId='" + identity.PlantId + @"' AND ISNULL(FS.GrossAmount,0)>0   
                            AND EI.DOS BETWEEN '" + FromDate + @"' AND '" + ToDate + @"'                           
                            ORDER BY EI.EmployeeCodePreFix,EI.EmployeeCodeNumeric";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(strSQL, out dsData, false, "1");


                    Dictionary<string, DataRow> DicData = new Dictionary<string, DataRow>();
                    for (int i = 0; i < dsData.Tables[0].Rows.Count; i++)
                    {
                        DicData.Add(dsData.Tables[0].Rows[i]["SystemID"].ToString(), dsData.Tables[0].Rows[i]);
                    }

                    if (string.IsNullOrEmpty(ToDate))
                    {
                        ToDate = DateTime.Now.ToString("dd-MMM-yyyy");
                    }
                    List<SPSalaryHead> dicSalaryHead = new List<SPSalaryHead>();
                    GetSalaryHead(out dsSalHd);
                    DataView dvsh = new DataView(dsSalHd.Tables[0]);
                    DataTable dtSalHdx = dvsh.ToTable(true, "SalaryHeadID");

                    if (dtSalHdx.Rows.Count > 0)
                        dicSalaryHead = dtSalHdx.ToList<SPSalaryHead>();


                    GetMultipleEmployeeSalaryData(identity.PlantId, FromDate, out dsSalaryDataEmpWise);
                    Dictionary<string, List<DataRow>> DicAllEmpSalaryInfo = new Dictionary<string, List<DataRow>>();

                    string _empId = "";
                    List<DataRow> _data = new List<DataRow>();
                    for (int i = 0; i < dsSalaryDataEmpWise.Tables[0].Rows.Count; i++)
                    {
                        if (_empId != dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString())
                        {
                            _data = new List<DataRow>();
                            DicAllEmpSalaryInfo.Add(dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString(), _data);
                            _empId = dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString();
                        }
                        _data.Add(dsSalaryDataEmpWise.Tables[0].Rows[i]);
                    }


                    foreach (string key in DicData.Keys)
                    {
                        if (key == "1800085")
                        {

                        }
                        List<SPvalueHeadWise> dtValue = null;
                        DataRow dr = DicData[key];


                        if (DicData.ContainsKey(key) == true)
                        {
                            //DataRow drData = DicData[key];
                            //if (drData["EncashmentBasis"].ToString() == "DOJ" && !string.IsNullOrEmpty(drData["LvEncashmentFormulaDesID"].ToString()))
                            //{


                            IndividualGratuityPolicyModel o = new IndividualGratuityPolicyModel();
                            int SNO = 1;
                            o.SNo = dr["SNO"].ToString();
                            o.EmpSystemId = dr["SystemID"].ToString();
                            o.EmployeeCode = dr["EmployeeCode"].ToString();
                            o.EmployeeName = dr["EmployeeName"].ToString();
                            //o.LeaveType = dr["LeaveType"].ToString();

                            //o.CheckBoxSelect = Convert.ToBoolean(dr["CheckBoxSelect"].ToString());
                            o.DOJ = dr["DOJ"].ToString();
                            o.DOB = dr["DOB"].ToString();
                            o.DOS = dr["DOS"].ToString();
                            o.PolicyNo = dr["PolicyNo"].ToString();
                            o.TenureInDays = Convert.ToInt32(dr["TenureInDays"].ToString());






                            if (DicAllEmpSalaryInfo.ContainsKey(dr["SystemID"].ToString()) == false)
                                continue;
                            List<DataRow> salaryStructure = DicAllEmpSalaryInfo[dr["SystemID"].ToString()];
                            #region Create Table                    
                            dtValue = new List<SPvalueHeadWise>();
                            #endregion Create Table

                            for (int j = 0; j < salaryStructure.Count; j++)
                            {
                                SPvalueHeadWise sp = new SPvalueHeadWise();
                                sp.SalaryHeadID = salaryStructure[j]["SalaryHeadID"].ToString().Trim();
                                sp.EntryCurrencyID = salaryStructure[j]["EntryCurrencyID"].ToString().Trim();
                                sp.EntryAmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                                dtValue.Add(sp);
                                if (salaryStructure[j]["HeadCategory"].ToString().ToUpper().Trim() == "BASIC")
                                {
                                    o.BasicAmount = Convert.ToDecimal(salaryStructure[j]["EntryAmount"]);
                                }
                                //if (salaryStructure[j]["HeadCategory"].ToString().ToUpper().Trim() == "GROSS")
                                //{
                                //    o.GrossAmmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                                //}
                            }
                            //try
                            //{
                            //    string _formulaValue = string.Empty;
                            //    string sFormulaResult = string.Empty;
                            //    ReLoadFormulaWithGrossValueNew(dr["MaturityFormulaDesID"].ToString(), salaryStructure[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, dtValue, dicSalaryHead);
                            //    //obSSrecal.ReLoadFormulaWithValue(dsEarnLeavePolicy.Tables[0].Rows[0]["LvEncashmentFormulaDesID"].ToString(), ref dtValue, dsSalaryDataEmpWise.Tables[0].Rows[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, ref dtSlrHd);
                            //    sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString();
                            //    o.EntitlementforGratuity = Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult));
                            //}
                            //catch (Exception ex)
                            //{
                            //    throw ex;
                            //}




                            TimeSpan DaysNo = TimeSpan.FromDays(Convert.ToInt32(dr["TenureInDays"].ToString()));
                            DateTime zeroTime = new DateTime(1, 1, 1);
                            //int years = (zeroTime + DaysNo).Year - 1;
                            //int month = (zeroTime + DaysNo).Month - 1;
                            //int days = (zeroTime + DaysNo).Day-1;
                            DateTime Now;
                            if (!string.IsNullOrEmpty(dr["DOS"].ToString()))
                            {
                                Now = Convert.ToDateTime(dr["DOS"].ToString());
                            }
                            else
                            {
                                Now = Convert.ToDateTime(ToDate.ToString());
                            }

                            int years = new DateTime(DateTime.Now.Subtract(Convert.ToDateTime(dr["DOJ"].ToString())).Ticks).Year - 1;
                            DateTime PastYearDate = (Convert.ToDateTime(dr["DOJ"].ToString())).AddYears(years);

                            int month = 0;
                            for (int i = 1; i <= 12; i++)
                            {
                                if (PastYearDate.AddMonths(i) == Now)
                                {
                                    month = i;
                                    break;
                                }
                                else if (PastYearDate.AddMonths(i) >= Now)
                                {
                                    month = i - 1;
                                    break;
                                }
                            }
                            int days = Now.Subtract(PastYearDate.AddMonths(month)).Days + 1;
                            int Hours = Now.Subtract(PastYearDate).Hours;
                            int Minutes = Now.Subtract(PastYearDate).Minutes;
                            int Seconds = Now.Subtract(PastYearDate).Seconds;








                            o.TenureDayNo = days;
                            o.TenureMonthNo = month;
                            o.TenureYearNo = years;




                            //Calculate Gratuity
                            string _formulaValueG = "0";
                            string sFormulaResult = "0";
                            int GratuityNumberOfYears = 0;
                            DataSet dsGratuityPolicy = null;
                            GetGratuityPolicy(identity.PlantId, out dsGratuityPolicy);

                            DataView dvGratuityPolicy = new DataView(dsGratuityPolicy.Tables[0]);
                            dvGratuityPolicy.RowFilter = "MaturityFromYear<= " + years + " and " + years + "<= MaturityToYear";
                            if (dvGratuityPolicy.Count > 0)
                            {
                                if (Convert.ToBoolean(dvGratuityPolicy[0]["IsSubSequentRoudingSixMonth"]))
                                {
                                    if (month > 6)
                                    {
                                        GratuityNumberOfYears = years + 1;
                                    }
                                    else if (month == 6 && days > 0)
                                    {
                                        GratuityNumberOfYears = years + 1;
                                    }
                                    else
                                    {
                                        GratuityNumberOfYears = years;
                                    }
                                }
                                else
                                {
                                    GratuityNumberOfYears = years;
                                }

                                DataView dvGratuityPolicyTemp = new DataView(dsGratuityPolicy.Tables[0]);
                                dvGratuityPolicyTemp.RowFilter = "MaturityFromYear<= " + GratuityNumberOfYears + " and " + GratuityNumberOfYears + "<= MaturityToYear";
                                if (dvGratuityPolicy.Count > 0)
                                {
                                    //obSSrecal.ReLoadFormulaWithValue(dvGratuityPolicyTemp[0]["MaturityFormulaDesID"].ToString(), ref dtValue, dsSalaryData.Tables[0].Rows[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValueG, ref dtSlrHd);
                                    ReLoadFormulaWithGrossValueNew(dvGratuityPolicyTemp[0]["MaturityFormulaDesID"].ToString(), salaryStructure[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValueG, dtValue, dicSalaryHead);

                                    sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValueG).ToString();
                                }

                            }
                            dvGratuityPolicy.RowFilter = null;

                            o.GratuityAmount = Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult)) * GratuityNumberOfYears;
                            o.GratuityRate = Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult));
                            o.GratuityNumberOfYears = GratuityNumberOfYears;






















                            //SNO= SNO+1;
                            ob.Add(o);


                        }
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







                //"GratuityInsuranceAgreement.docx";


                //GratuityInsuranceAgreementSummary.docx


                AllReport("GratuityInsuranceAgreement.docx", dsGratuityInsuranceAgreement, ob, "GratuityInsuranceAgreement.docx");
                //Report("GratuityInsuranceAgreementSummary.docx", dsGratuityInsuranceAgreement, ob, "GratuityInsuranceAgreementSummary.docx");
                return null;

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }





        [HttpGet, Authorize]
        public ActionResult GetSummaryData(string AgreementId, string FromDate, string ToDate)
        {
            try
            {
                #region declare
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;



                #endregion

                List<IndividualGratuityPolicyModel> ob = new List<IndividualGratuityPolicyModel>();
                DataSet dsData;
                DataSet dsSalHd;
                DataSet dsSalaryDataEmpWise;
                DataSet dsGratuityInsuranceAgreement;
                ConnectionManager.DAL.ConManager objCon;
                try
                {


                    string strGratuityInsuranceAgreementSQL = @"SELECT AgreementNo,Format(AgreementDate,'dd-MMM-yyyy') AgreementDate,InsuranceCompanyName,Branch                      
                            FROM  GratuityInsuranceAgreement                            
                            WHERE Id='" + AgreementId + @"'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(strGratuityInsuranceAgreementSQL, out dsGratuityInsuranceAgreement, false, "1");

                    string strSQL = @"SELECT ROW_NUMBER() OVER(ORDER BY EI.EmployeeCodePreFix,EI.EmployeeCodeNumeric ) AS  SNO
                            ,EI.SystemId,EI.EmployeeCode,EI.EmployeeName
                            ,FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
                            ,FORMAT(EI.DOS,'dd-MMM-yyyy') DOS
                            ,FORMAT(EI.DOB,'dd-MMM-yyyy') DOB
                            ,DateDiff(day,EI.doj,Isnull(EI.dos,CONVERT(date,'" + ToDate + @"')))+1 TenureInDays
                            ,IGP.PolicyNo
                            ,IGP.EmployeeSystemId
                            ,GA.AgreementNo
                            ,GA.AgreementDate
                            ,GA.Branch
                        
                            
                            FROM IndividualGratuityPolicy IGP
                            LEFT JOIN GratuityInsuranceAgreement GA ON GA.Id=IGP.AgreementId
                            LEFT JOIN EmployeeInformation EI on EI.SystemId=IGP.EmployeeSystemId
                           
                            WHERE IGP.AgreementId='" + AgreementId + @"'  AND EI.PlantId='" + identity.PlantId + @"'  
                            ----AND EI.DOS BETWEEN '" + FromDate + @"' AND '" + ToDate + @"'                           
                            ORDER BY EI.EmployeeCodePreFix,EI.EmployeeCodeNumeric";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(strSQL, out dsData, false, "1");


                    Dictionary<string, DataRow> DicData = new Dictionary<string, DataRow>();
                    for (int i = 0; i < dsData.Tables[0].Rows.Count; i++)
                    {
                        DicData.Add(dsData.Tables[0].Rows[i]["SystemID"].ToString(), dsData.Tables[0].Rows[i]);
                    }

                    if (string.IsNullOrEmpty(ToDate))
                    {
                        ToDate = DateTime.Now.ToString("dd-MMM-yyyy");
                    }
                    List<SPSalaryHead> dicSalaryHead = new List<SPSalaryHead>();
                    GetSalaryHead(out dsSalHd);
                    DataView dvsh = new DataView(dsSalHd.Tables[0]);
                    DataTable dtSalHdx = dvsh.ToTable(true, "SalaryHeadID");

                    if (dtSalHdx.Rows.Count > 0)
                        dicSalaryHead = dtSalHdx.ToList<SPSalaryHead>();


                    GetMultipleEmployeeSalaryData(identity.PlantId, FromDate, out dsSalaryDataEmpWise);
                    Dictionary<string, List<DataRow>> DicAllEmpSalaryInfo = new Dictionary<string, List<DataRow>>();

                    string _empId = "";
                    List<DataRow> _data = new List<DataRow>();
                    for (int i = 0; i < dsSalaryDataEmpWise.Tables[0].Rows.Count; i++)
                    {
                        if (_empId != dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString())
                        {
                            _data = new List<DataRow>();
                            DicAllEmpSalaryInfo.Add(dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString(), _data);
                            _empId = dsSalaryDataEmpWise.Tables[0].Rows[i]["EmpInfoSystemID"].ToString();
                        }
                        _data.Add(dsSalaryDataEmpWise.Tables[0].Rows[i]);
                    }


                    foreach (string key in DicData.Keys)
                    {
                        if (key == "1800085")
                        {

                        }
                        List<SPvalueHeadWise> dtValue = null;
                        DataRow dr = DicData[key];


                        if (DicData.ContainsKey(key) == true)
                        {
                            //DataRow drData = DicData[key];
                            //if (drData["EncashmentBasis"].ToString() == "DOJ" && !string.IsNullOrEmpty(drData["LvEncashmentFormulaDesID"].ToString()))
                            //{


                            IndividualGratuityPolicyModel o = new IndividualGratuityPolicyModel();
                            int SNO = 1;
                            o.SNo = dr["SNO"].ToString();
                            o.EmpSystemId = dr["SystemID"].ToString();
                            o.EmployeeCode = dr["EmployeeCode"].ToString();
                            o.EmployeeName = dr["EmployeeName"].ToString();
                            //o.LeaveType = dr["LeaveType"].ToString();

                            //o.CheckBoxSelect = Convert.ToBoolean(dr["CheckBoxSelect"].ToString());
                            o.DOJ = dr["DOJ"].ToString();
                            o.DOB = dr["DOB"].ToString();
                            o.DOS = dr["DOS"].ToString();
                            o.PolicyNo = dr["PolicyNo"].ToString();
                            o.TenureInDays = Convert.ToInt32(dr["TenureInDays"].ToString());






                            if (DicAllEmpSalaryInfo.ContainsKey(dr["SystemID"].ToString()) == false)
                                continue;
                            List<DataRow> salaryStructure = DicAllEmpSalaryInfo[dr["SystemID"].ToString()];
                            #region Create Table                    
                            dtValue = new List<SPvalueHeadWise>();
                            #endregion Create Table

                            for (int j = 0; j < salaryStructure.Count; j++)
                            {
                                SPvalueHeadWise sp = new SPvalueHeadWise();
                                sp.SalaryHeadID = salaryStructure[j]["SalaryHeadID"].ToString().Trim();
                                sp.EntryCurrencyID = salaryStructure[j]["EntryCurrencyID"].ToString().Trim();
                                sp.EntryAmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                                dtValue.Add(sp);
                                if (salaryStructure[j]["HeadCategory"].ToString().ToUpper().Trim() == "BASIC")
                                {
                                    o.BasicAmount = Convert.ToDecimal(salaryStructure[j]["EntryAmount"]);
                                }
                                //if (salaryStructure[j]["HeadCategory"].ToString().ToUpper().Trim() == "GROSS")
                                //{
                                //    o.GrossAmmount = salaryStructure[j]["EntryAmount"].ToString().Trim();
                                //}
                            }
                            //try
                            //{
                            //    string _formulaValue = string.Empty;
                            //    string sFormulaResult = string.Empty;
                            //    ReLoadFormulaWithGrossValueNew(dr["MaturityFormulaDesID"].ToString(), salaryStructure[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, dtValue, dicSalaryHead);
                            //    //obSSrecal.ReLoadFormulaWithValue(dsEarnLeavePolicy.Tables[0].Rows[0]["LvEncashmentFormulaDesID"].ToString(), ref dtValue, dsSalaryDataEmpWise.Tables[0].Rows[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValue, ref dtSlrHd);
                            //    sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValue).ToString();
                            //    o.EntitlementforGratuity = Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult));
                            //}
                            //catch (Exception ex)
                            //{
                            //    throw ex;
                            //}




                            TimeSpan DaysNo = TimeSpan.FromDays(Convert.ToInt32(dr["TenureInDays"].ToString()));
                            DateTime zeroTime = new DateTime(1, 1, 1);
                            //int years = (zeroTime + DaysNo).Year - 1;
                            //int month = (zeroTime + DaysNo).Month - 1;
                            //int days = (zeroTime + DaysNo).Day-1;
                            DateTime Now;
                            if (!string.IsNullOrEmpty(dr["DOS"].ToString()))
                            {
                                Now = Convert.ToDateTime(dr["DOS"].ToString());
                            }
                            else
                            {
                                Now = Convert.ToDateTime(ToDate.ToString());
                            }

                            int years = new DateTime(DateTime.Now.Subtract(Convert.ToDateTime(dr["DOJ"].ToString())).Ticks).Year - 1;
                            DateTime PastYearDate = (Convert.ToDateTime(dr["DOJ"].ToString())).AddYears(years);

                            int month = 0;
                            for (int i = 1; i <= 12; i++)
                            {
                                if (PastYearDate.AddMonths(i) == Now)
                                {
                                    month = i;
                                    break;
                                }
                                else if (PastYearDate.AddMonths(i) >= Now)
                                {
                                    month = i - 1;
                                    break;
                                }
                            }
                            int days = Now.Subtract(PastYearDate.AddMonths(month)).Days + 1;
                            int Hours = Now.Subtract(PastYearDate).Hours;
                            int Minutes = Now.Subtract(PastYearDate).Minutes;
                            int Seconds = Now.Subtract(PastYearDate).Seconds;








                            o.TenureDayNo = days;
                            o.TenureMonthNo = month;
                            o.TenureYearNo = years;




                            //Calculate Gratuity
                            string _formulaValueG = "0";
                            string sFormulaResult = "0";
                            int GratuityNumberOfYears = 0;
                            DataSet dsGratuityPolicy = null;
                            GetGratuityPolicy(identity.PlantId, out dsGratuityPolicy);

                            DataView dvGratuityPolicy = new DataView(dsGratuityPolicy.Tables[0]);
                            dvGratuityPolicy.RowFilter = "MaturityFromYear<= " + years + " and " + years + "<= MaturityToYear";
                            if (dvGratuityPolicy.Count > 0)
                            {
                                if (Convert.ToBoolean(dvGratuityPolicy[0]["IsSubSequentRoudingSixMonth"]))
                                {
                                    if (month > 6)
                                    {
                                        GratuityNumberOfYears = years + 1;
                                    }
                                    else if (month == 6 && days > 0)
                                    {
                                        GratuityNumberOfYears = years + 1;
                                    }
                                    else
                                    {
                                        GratuityNumberOfYears = years;
                                    }
                                }
                                else
                                {
                                    GratuityNumberOfYears = years;
                                }

                                DataView dvGratuityPolicyTemp = new DataView(dsGratuityPolicy.Tables[0]);
                                dvGratuityPolicyTemp.RowFilter = "MaturityFromYear<= " + GratuityNumberOfYears + " and " + GratuityNumberOfYears + "<= MaturityToYear";
                                if (dvGratuityPolicy.Count > 0)
                                {
                                    //obSSrecal.ReLoadFormulaWithValue(dvGratuityPolicyTemp[0]["MaturityFormulaDesID"].ToString(), ref dtValue, dsSalaryData.Tables[0].Rows[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValueG, ref dtSlrHd);
                                    ReLoadFormulaWithGrossValueNew(dvGratuityPolicyTemp[0]["MaturityFormulaDesID"].ToString(), salaryStructure[0]["EntryCurrencyID"].ToString().Trim(), "0", out _formulaValueG, dtValue, dicSalaryHead);

                                    sFormulaResult = clsSalaryStructureAplos.Evaluate(_formulaValueG).ToString();
                                }

                            }
                            dvGratuityPolicy.RowFilter = null;

                            o.GratuityAmount = Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult)) * GratuityNumberOfYears;
                            o.GratuityRate = Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult));
                            o.GratuityNumberOfYears = GratuityNumberOfYears;






















                            //SNO= SNO+1;
                            ob.Add(o);


                        }
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







                //"GratuityInsuranceAgreement.docx";


                //GratuityInsuranceAgreementSummary.docx


                //Report("GratuityInsuranceAgreement.docx", dsGratuityInsuranceAgreement, ob, "GratuityInsuranceAgreement.docx");
                Report("GratuityInsuranceAgreementSummary.docx", dsGratuityInsuranceAgreement, ob, "GratuityInsuranceAgreementSummary.docx");
                return null;

            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
                //throw new Exception(ex.Message);
            }
        }

        public void Report(String TemplateFileName, DataSet dsGratuityInsuranceAgreement, List<IndividualGratuityPolicyModel> ob, string SaveFileName)
        {
            try
            {

                ReportUtility oRU = new ReportUtility();
                string File = "";
                string strPath = "";


                strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/TemplateFileName);  // IDCardEng.xlsx
                File = TemplateFileName;
                if (!System.IO.File.Exists(strPath))
                {
                    throw new CustomException("File <" + TemplateFileName + "> Not Found.");
                }


                FileInfo DocFile = new FileInfo(strPath);
                if (DocFile.Exists == false)
                {
                    //DocFile = new FileInfo(System.Web.HttpContext.Current.Server.MapPath(".") + "\\Doc1.docx");
                    throw new CustomException("File Not Found");
                }





                ////A opens input document.
                WordDocument document = new WordDocument(DocFile.FullName);
                WSection section = document.Sections[0];
                //TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                //Dictionary<string, int> replaced = new Dictionary<string, int>();




                if (dsGratuityInsuranceAgreement.Tables[0].Rows.Count > 0)
                {
                    document.Replace("{AgreementNo}", dsGratuityInsuranceAgreement.Tables[0].Rows[0]["AgreementNo"].ToString(), false, true);
                    document.Replace("{AgreementDate}", dsGratuityInsuranceAgreement.Tables[0].Rows[0]["AgreementDate"].ToString(), false, true);
                    document.Replace("{InsuranceCompanyName}", dsGratuityInsuranceAgreement.Tables[0].Rows[0]["InsuranceCompanyName"].ToString(), false, true);
                    document.Replace("{Branch}", dsGratuityInsuranceAgreement.Tables[0].Rows[0]["Branch"].ToString(), false, true);
                    //document.Replace("{Date}", GetFormatedDate(System.DateTime.Now.ToString("dd-MMM-yyyy"), tempId), false, true);
                    //document.Replace("{Date}", GetFormatedDate(System.DateTime.Now.ToString("dd-MMM-yyyy"), tempId), false, true);
                    document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, true);
                }













                #region loop 

                WTable table2 = (WTable)section.Body.Tables[0];
                WTableRow copiedRow2 = table2.Rows[1].Clone();

                WTableRow row2;

                for (int ROW = 0; ROW < ob.Count; ROW++)
                {
                    if (ROW > 0)
                    {
                        row2 = copiedRow2.Clone();
                        table2.Rows.Add(row2);
                    }
                    table2.Replace("{SNo}", ob[ROW].SNo, false, true);
                    table2.Replace("{PolicyNo}", ob[ROW].PolicyNo, false, true);
                    table2.Replace("{EmployeeCode}", ob[ROW].EmployeeCode, false, true);
                    table2.Replace("{EmployeeName}", ob[ROW].EmployeeName, false, true);
                    table2.Replace("{DOJ}", ob[ROW].DOJ, false, true);
                    table2.Replace("{DOB}", ob[ROW].DOB, false, true);
                    table2.Replace("{DOS}", ob[ROW].DOS, false, true);
                    table2.Replace("{TenureInDays}", ob[ROW].TenureInDays.ToString(), false, true);
                    table2.Replace("{BasicAmount}", string.Format("{0:0.00}", ob[ROW].BasicAmount), false, true);
                    table2.Replace("{GratuityAmount}", string.Format("{0:0.00}", ob[ROW].GratuityAmount), false, true);
                    //table2.Replace("{EmployeeName}", ob[ROW].EmployeeName, false, true);
                    //table2.Replace("{Description}", dtDisciplinaryAction.Rows[ROW]["Description"].ToString(), false, true);

                }

                #endregion



                //if (!string.IsNullOrEmpty(dtEmp.Rows[0]["EmployeeCode"].ToString()))
                //{

                //    fileNames = dtEmp.Rows[0]["EmployeeCode"].ToString() + "-ServiceBook.docx";

                //}
                //else
                //{
                //    fileNames = "-ServiceBook.docx";
                //}
                //fileNames = "GratuityInsuranceAgreement.docx";
                document.Save(SaveFileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                //document.Save(SaveFileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.Attachment);
                document.Close();






            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void AllReport(String TemplateFileName, DataSet dsGratuityInsuranceAgreement, List<IndividualGratuityPolicyModel> ob, string SaveFileName)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsCompanyInfo;
                ConnectionManager.DAL.ConManager objCon;

                ReportUtility oRU = new ReportUtility();
                string File = "";
                string strPath = "";


                strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/TemplateFileName);  // IDCardEng.xlsx
                File = TemplateFileName;
                if (!System.IO.File.Exists(strPath))
                {
                    throw new CustomException("File <" + TemplateFileName + "> Not Found.");
                }


                FileInfo DocFile = new FileInfo(strPath);
                if (DocFile.Exists == false)
                {
                    //DocFile = new FileInfo(System.Web.HttpContext.Current.Server.MapPath(".") + "\\Doc1.docx");
                    throw new CustomException("File Not Found");
                }



                string strCompanyInfoSQL = @"select a.Address1 CompanyAddress, c.UserName CompanyName from org.Company c
                                            left join  mst.addressmaster a on a.id=c.AddressMasterId
                                            where c.id='" + identity.CompanyId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strCompanyInfoSQL, out dsCompanyInfo, false, "1");



                ////A opens input document.
                WordDocument document = new WordDocument(DocFile.FullName);
                WSection section = document.Sections[0];
                //TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                //Dictionary<string, int> replaced = new Dictionary<string, int>();




                if (dsGratuityInsuranceAgreement.Tables[0].Rows.Count > 0)
                {
                    document.Replace("{AgreementNo}", dsGratuityInsuranceAgreement.Tables[0].Rows[0]["AgreementNo"].ToString(), false, true);
                    document.Replace("{AgreementDate}", dsGratuityInsuranceAgreement.Tables[0].Rows[0]["AgreementDate"].ToString(), false, true);
                    document.Replace("{InsuranceCompanyName}", dsGratuityInsuranceAgreement.Tables[0].Rows[0]["InsuranceCompanyName"].ToString(), false, true);
                    document.Replace("{Branch}", dsGratuityInsuranceAgreement.Tables[0].Rows[0]["Branch"].ToString(), false, true);
                    //document.Replace("{Date}", GetFormatedDate(System.DateTime.Now.ToString("dd-MMM-yyyy"), tempId), false, true);
                    //document.Replace("{Date}", GetFormatedDate(System.DateTime.Now.ToString("dd-MMM-yyyy"), tempId), false, true);
                    document.Replace("{Date}", System.DateTime.Now.ToString("dd-MMM-yyyy"), false, true);
                }


                if (dsCompanyInfo.Tables[0].Rows.Count > 0)
                {
                    document.Replace("{CompanyAddress}", dsCompanyInfo.Tables[0].Rows[0]["CompanyAddress"].ToString(), false, true);
                    document.Replace("{CompanyName}", dsCompanyInfo.Tables[0].Rows[0]["CompanyName"].ToString(), false, true);
                }



                decimal total = ob.Sum(item => item.GratuityAmount);

                if (total>0)
                {
                    document.Replace("{TotalAmount}",total.ToString("N2"), false, true);
                }





                #region loop 

                WTable table2 = (WTable)section.Body.Tables[0];
                WTableRow copiedRow2 = table2.Rows[1].Clone();

                WTableRow row2;

                for (int ROW = 0; ROW < ob.Count; ROW++)
                {
                    if (ROW > 0)
                    {
                        row2 = copiedRow2.Clone();
                        table2.Rows.Add(row2);
                    }
                    table2.Replace("{SNo}", ob[ROW].SNo, false, true);
                    table2.Replace("{PolicyNo}", ob[ROW].PolicyNo, false, true);
                    table2.Replace("{EmployeeCode}", ob[ROW].EmployeeCode, false, true);
                    table2.Replace("{EmployeeName}", ob[ROW].EmployeeName, false, true);
                    table2.Replace("{DOJ}", ob[ROW].DOJ, false, true);
                    table2.Replace("{DOB}", ob[ROW].DOB, false, true);
                    table2.Replace("{DOS}", ob[ROW].DOS, false, true);
                    table2.Replace("{TenureInDays}", ob[ROW].TenureInDays.ToString(), false, true);
                    table2.Replace("{BasicAmount}", string.Format("{0:N2}", ob[ROW].BasicAmount), false, true);
                    if (ob[ROW].GratuityAmount>0)
                    {
                        table2.Replace("{GratuityAmount}", string.Format("{0:N2}", ob[ROW].GratuityAmount), false, true);
                    }
                    else
                    {
                        table2.Replace("{GratuityAmount}", string.Format("{0:0.00}", "Not Settled"), false, true);
                    }
                    
                    //table2.Replace("{EmployeeName}", ob[ROW].EmployeeName, false, true);
                    //table2.Replace("{Description}", dtDisciplinaryAction.Rows[ROW]["Description"].ToString(), false, true);

                }





                WTable table3 = (WTable)section.Body.Tables[1];
                WTableRow copiedRow3 = table3.Rows[1].Clone();

                WTableRow row3;

                for (int ROW = 0; ROW < ob.Count; ROW++)
                {
                    if (ROW > 0)
                    {
                        row3 = copiedRow3.Clone();
                        table3.Rows.Add(row3);
                    }
                    table3.Replace("{SNo}", ob[ROW].SNo, false, true);
                    table3.Replace("{PolicyNo}", ob[ROW].PolicyNo, false, true);
                    table3.Replace("{EmployeeCode}", ob[ROW].EmployeeCode, false, true);
                    table3.Replace("{EmployeeName}", ob[ROW].EmployeeName, false, true);
                    table3.Replace("{DOJ}", ob[ROW].DOJ, false, true);
                    table3.Replace("{DOB}", ob[ROW].DOB, false, true);
                    table3.Replace("{DOS}", ob[ROW].DOS, false, true);
                    table3.Replace("{TenureInDays}", ob[ROW].TenureInDays.ToString(), false, true);
                    table3.Replace("{BasicAmount}", string.Format("{0:N2}", ob[ROW].BasicAmount), false, true);
                    //table3.Replace("{GratuityAmount}", string.Format("{0:0.00}", ob[ROW].GratuityAmount), false, true);


                    if (ob[ROW].GratuityAmount > 0)
                    {
                        table3.Replace("{GratuityAmount}", string.Format("{0:N2}", ob[ROW].GratuityAmount), false, true);
                    }
                    else
                    {
                        table3.Replace("{GratuityAmount}", string.Format("{0:0.00}", "Not Settled"), false, true);
                    }


                }
                #endregion




                TextSelection[] X = document.FindAll(new Regex("{.*?}")).ToArray();
                List<string> allDeleteresult = new List<string>();
                for (int i = 0; i < X.Length; i++)
                    allDeleteresult.Add(X[i].SelectedText);

                foreach (string item in allDeleteresult)
                {
                    
                        document.Replace(item, "", false, true);
                }

                //if (!string.IsNullOrEmpty(dtEmp.Rows[0]["EmployeeCode"].ToString()))
                //{

                //    fileNames = dtEmp.Rows[0]["EmployeeCode"].ToString() + "-ServiceBook.docx";

                //}
                //else
                //{
                //    fileNames = "-ServiceBook.docx";
                //}
                //fileNames = "GratuityInsuranceAgreement.docx";
                document.Save(SaveFileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                //document.Save(SaveFileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.Attachment);
                document.Close();






            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        public void GetGratuityPolicy(string PlantId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT gpm.Id, gpm.UserName, gpm.IsSubSequentRoudingSixMonth, gpm.IsSubSequentRoudingSixMonth,
                                Convert(float, gpd.MaturityFromYear) MaturityFromYear, Convert(float, gpd.MaturityToYear) MaturityToYear,
                               gpd.MaturityFormulaDesID, gpd.MaturityFormulaDescription
                        FROM GratuityPolicyMaster AS gpm
                        LEFT JOIN GratuityPolicyDetails AS gpd ON gpd.GratuityPolicyMasterId = gpm.Id
                        WHERE gpm.plantId='" + PlantId + @"' AND gpm.Active=1 ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
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

        public void GetMultipleEmployeeSalaryData(string PlantId, string sEffectiveDate, out System.Data.DataSet dsRef)
        {
            dsRef = null;
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"SELECT * FROM ( SELECT (x.EffectiveDate) EffectiveDate,m.SystemID,m.EmpInfoSystemID from (
												select max(	EffectiveDate) 	EffectiveDate,EmpInfoSystemID FROM (
																	SELECT   EffectiveDate   ,EmpInfoSystemID
																	FROM SalaryInfoDefineMaster  
																	WHERE IsApproved =1 AND EffectiveDate<= '" + sEffectiveDate + @"'  AND PlantID='" + PlantId + @"'
																	union
																	SELECT  EffectiveDate  ,EmpInfoSystemID
																	FROM SalaryInfoBackMaster  
																	WHERE IsApproved =1 AND EffectiveDate<= '" + sEffectiveDate + @"' AND PlantID='" + PlantId + @"'
 	 												) zz GROUP BY EmpInfoSystemID		
											) x
						
						INNER JOIN (
							 SELECT  EffectiveDate,SystemID,EmpInfoSystemID
							   FROM SalaryInfoDefineMaster  
							  WHERE    IsApproved =1 
                        union
                        SELECT  EffectiveDate,SystemID ,EmpInfoSystemID
								FROM SalaryInfoBackMaster  
                                WHERE IsApproved =1 
						) m ON m.EffectiveDate=x.EffectiveDate AND m.EmpInfoSystemID= x.EmpInfoSystemID ) mas
						INNER JOIN (
						SELECT s.SystemID,s.SalaryID,s.SalaryHeadID,s.EntryCurrencyID,s.EntryAmount,s.DefineCurrencyID,s.DefineAmount,s.AmtDefinitionCurrencyID,s.AmtDefinitionRate,s.SequenceNo,s.SalaryCategory
                        ,sh.HeadCategory,sh.SalaryHead  FROM SalaryInfoDefine s
						LEFT JOIN SalaryHead AS sh on s.SalaryHeadID=sh.SalaryHeadID 
						UNION
						SELECT sb.SystemID,sb.SalaryID,sb.SalaryHeadID,sb.EntryCurrencyID,sb.EntryAmount,sb.DefineCurrencyID,sb.DefineAmount,sb.AmtDefinitionCurrencyID,sb.AmtDefinitionRate,sb.SequenceNo,sb.SalaryCategory
                        ,sh.HeadCategory,sh.SalaryHead FROM  SalaryInfoBack sb
						LEFT JOIN SalaryHead AS sh on sb.SalaryHeadID=sh.SalaryHeadID
                        ) d ON mas.SystemID=d.SalaryID   ORDER BY mas.EmpInfoSystemID ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
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
        public void GetSalaryHead(out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM SalaryHead";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
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

        public void ReLoadFormulaWithGrossValueNew(string strFormulaID, string sLocalCurrencyID, string sForeignCurRate,
        out string sFormulaValue, List<SPvalueHeadWise> dtValue, List<SPSalaryHead> dicSlrHd)
        {
            DataSet dsLocal = null;
            //DataView dvLocal = null;
            //DataView dvSlrHd = null;
            string strTemp = "";

            try
            {

                dsLocal = new DataSet();
                string strFormulaIDTemp = strFormulaID.Trim();
                //string sLocalCurrencyID = para.lblLocalCurrencyID;
                //string sForeignCurRate = para.lblLocalCurRate;

                if (sForeignCurRate == "")
                { sForeignCurRate = "1"; }

                sFormulaValue = "";

                string[] strIdCol = strFormulaIDTemp.Split(' ');

                DataTable dt = new DataTable();
                dt.TableName = "IDLIST";
                dt.Columns.Add("ID");
                DataRow dr = null;
                foreach (string id in strIdCol)
                {
                    dr = dt.NewRow();
                    dr["ID"] = id.Trim();
                    dt.Rows.Add(dr);
                }
                dsLocal.Tables.Add(dt);

                for (int i = 0; i < dsLocal.Tables[0].Rows.Count; i++)
                {
                    strTemp = "";

                    strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                    if (strTemp.Trim() == "+" || strTemp.Trim() == "-" || strTemp.Trim() == "*" || strTemp.Trim() == "/" || strTemp.Trim() == ">" || strTemp.Trim() == "<" || strTemp.Trim() == "(" || strTemp.Trim() == ")")
                    {
                        strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                    }
                    else
                    {

                        var dtv = dtValue.FindAll(x => x.SalaryHeadID == strTemp.Trim());
                        if (dtv.Count() > 0)
                        {

                            if (dtv[0].EntryCurrencyID == sLocalCurrencyID)
                            {
                                strTemp = dtv[0].EntryAmount;
                                strTemp = GetAbsValue(strTemp);
                            }
                            else
                            {
                                strTemp = (Convert.ToDecimal(dtv[0].EntryAmount) * Convert.ToDecimal(sForeignCurRate.Trim())).ToString();
                                strTemp = " " + GetAbsValue(strTemp) + " ";
                            }


                        }
                        else
                        {
                            var dicsh = dicSlrHd.FindAll(x => x.SalaryHeadID == strTemp.Trim());
                            if (dicsh.Count() > 0)
                            {
                                strTemp = "0.00";
                            }
                        }


                    }


                    sFormulaValue += strTemp.Trim();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function 


        string GetAbsValue(string strTemp)
        {
            try
            {
                var vv = Math.Abs(Convert.ToDecimal(strTemp.Trim()));
                string _vv = vv.ToString();
                return _vv;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion -- Operations

    }
    public class IndividualGratuityPolicyModel
    {










        public string SNo { get; set; }
        public string PolicyNo { get; set; }
        public string EmpSystemId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeCode { get; set; }
        public string DOJ { get; set; }
        public string DOB { get; set; }
        public string DOS { get; set; }
        public decimal BasicAmount { get; set; }
        public decimal EntitlementforGratuity { get; set; }
        public decimal GratuityAmount { get; set; }
        public decimal GratuityRate { get; set; }
        public int GratuityNumberOfYears { get; set; }
        public int TenureInDays { get; set; }
        public int TenureDayNo { get; set; }
        public int TenureMonthNo { get; set; }
        public int TenureYearNo { get; set; }
        //sGratuityAmount = Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult)) * GratuityNumberOfYears;
        //sGratuityRate = Convert.ToDecimal(string.Format("{0:F2}", sFormulaResult));
        //sGratuityYearNo = GratuityNumberOfYears;



    }

}