using Aplos.Controllers;
using ConnectionManager;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using OTSBD;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocToPDFConverter;
using Syncfusion.Pdf;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web.Mvc;
using static Library.Service.Helpers.ReportUtility;
using static Library.Service.HumanResources.PayRegisterBDReportService;

namespace Aplos.Areas.Payrolls.Controllers
{
    public class FinalSettlementVoucherController : BaseController
    {
        #region Constructor

        private readonly IPayRegisterBDReportService _payRegisterBDReportService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly ISqlRepository _sqlRepository;



        public FinalSettlementVoucherController(
              IPayRegisterBDReportService payRegisterBDReportService, IEmployeeProfileService employeeProfileService,
              ISqlRepository sqlRepository
            )
        {
            _payRegisterBDReportService = payRegisterBDReportService;
            _employeeProfileService = employeeProfileService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        #region -- Pages

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations
        [HttpPost]
        public ActionResult GetEmployeeInformationForFinalSettlement(string monthNo, string yearNo)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                var cmdText = @"SELECT  ISNULL(E.SYSTEmId,'') EmpSystemId
									,ISNULL(e.EmployeeId,'')  EmployeeId                                     
                                    ,ISNULL(e.EmployeeCode,'') EmployeeCode
                                    ,ISNULL(e.EmployeeName,'') EmployeeName								
                                    ,ISNULL(mpb.EntityId,'') EntityId
									,ISNULL(mpb.PositionId,'') PositionId                                     
                                    ,isnull(ISNULL(egdsg.UserName,ld.UserName),'') Designation                                       
									,ISNULL(Department.UserName,'') Department 
									,ISNULL(Division.UserName,'') Division 									
									,ISNULL(Plant.UserName,'') Plant 
									,ISNULL(Section.UserName,'') Section 
									,ISNULL(SubSection.UserName,'') SubSection 
									,ISNULL(Unit.UserName,'') Unit 
                                    ,ISNULL(Line.UserName,'') Line
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOJ, 106), ' ', '-'),'') DOJ
                                    ,ISNULL(REPLACE(CONVERT(VARCHAR(11), e.DOS, 106), ' ', '-'),'') DOS
                                    ,ISNULL(e.EmployeeStatus,'') EmployeeStatus 
									FROM EmployeeInformation e
                                                             
                                    LEFT OUTER JOIN HKP.Designation edsg on edsg.id=e.DesignationSystemID
                                    LEFT OUTER JOIN HKP.DesignationGroup edsgg on edsgg.id=e.DesignationGroupId
									LEFT OUTER JOIN HKP.Designation egdsg on egdsg.id=e.GivenDesignationId
                                    LEFT OUTER JOIN HKP.LegalDesignation  ld on ld.Id=e.LegalDesignationId
									LEFT OUTER JOIN MST.ManpowerBudget mpb on mpb.Id=e.BudgetCode
									LEFT OUTER JOIN ORG.Position PO ON mpb.PositionId=PO.Id
                                    LEFT OUTER JOIN ORG.Entity EN ON mpb.EntityId=EN.Id
                                    LEFT JOIN [ORG].[Department] ON Department.Id = PO.DepartmentId
                                    LEFT JOIN [ORG].[Division] ON Division.Id = EN.DivisionId
                                    LEFT JOIN [ORG].[Plant] ON Plant.Id = EN.PlantId
                                    LEFT JOIN [ORG].[Section] ON Section.Id = PO.SectionId
                                    LEFT JOIN [ORG].[SubSection] ON SubSection.Id = PO.SubSectionId
                                    LEFT JOIN [ORG].[Unit] ON Unit.Id = EN.UnitId
                                    Left join [org].[Line] on Line.Id = Mpb.LineId
									where
									 e.plantId='" + identity.PlantId + @"'";



                // return _sqlRepository.GetDataCollection(cmdText);
                return Json(_sqlRepository.GetDataCollection(cmdText), JsonRequestBehavior.AllowGet);

            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpGet, Authorize]
        public ActionResult GetFinalSettlementVoucherReport(string year, string month, string employeeSystemId)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Convert.ToInt32(month));//Month Name from Month No
            var daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(year), Convert.ToInt32(month));//Number of Days in a month
            var ldateOfMonth = daysInMonth + "-" + monthName + "-" + year;
            var fdateOfMonth = "1" + "-" + monthName + "-" + year;
            string calculationDate = ldateOfMonth;
            ReportUtility ru = new ReportUtility();

            clsReport objRpt = null;

            DataSet dsCmp = null;
            DataSet dsFactory = null;

            DataSet dsEmpGratuity = null;
            DataTable dtEmpGratuity = null;
            DataSet dsSlrSheet = null;
            DataSet dsSalaryProcessId = null;

            DataSet dsBonus = null;
            DataTable dtBonus = null;

            //DataTable dtEmpSalary = null;
            DataSet dsEmpLeaveEncash = null;
            DataTable dtEmpLeaveEncash = null;

            
            #endregion Variable

            try
            {
                ru = new ReportUtility();

                objRpt = new clsReport();
                GetSalaryProcessId("", "", "", calculationDate, out dsSalaryProcessId);
                DataTable dtSalaryProcessId = dsSalaryProcessId.Tables[0];

                if (dtSalaryProcessId.Rows.Count <= 0)
                {

                    Exception ex = new Exception("Salary is not Processed in this Month");
                    throw (ex);
                }

                #region Variable
                ParamList para = new ParamList();

                var FactoryName = "";
                var CmpName = "";



                if (dtSalaryProcessId.Rows.Count > 0)
                {
                    para.SalaryProcessId = dtSalaryProcessId.Rows[0]["SalaryProcID"].ToString();
                }
                //para.EmpStatus = ddlStatus.SelectedValue.Trim();
                #endregion Variable
                var oRU = new ReportUtility();

                DateTime firstDayOfTheYear = new DateTime(Convert.ToInt32(year), 1, 1);
                var fromDate = firstDayOfTheYear.ToString();
                var toDate = calculationDate;

                var DOJ = string.Empty;
                var DOS = string.Empty;
                var bonus = 0.00;
                var exGratia = 0.00;
                var currentWages = 0.00;
                double currentBonus = 0.00;
                DataView dvBonus = null;
                var encachedLeave = 0.00;
                var eligibleYear = 0;
                var eligibleGratuityAmount = 0.00;

                var cmpAddress = string.Empty;
                var empName = string.Empty;
                
                #region DataSet

                objRpt.GetEmpFinalDischargeVoucherInfo(calculationDate, identity.CompanyGroupId, identity.CompanyId, identity.PlantId, employeeSystemId, out dsEmpGratuity);
                dtEmpGratuity = dsEmpGratuity.Tables[0];

           


                objRpt.GetEmpLeaveEncashmentInfo(fromDate, toDate, identity.CompanyGroupId, identity.CompanyId, identity.PlantId,  employeeSystemId, out dsEmpLeaveEncash);
                dtEmpLeaveEncash = dsEmpLeaveEncash.Tables[0];

                DataTable dtEmpSalary = GetEmployeeSalaryInfoDetail(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, toDate, toDate, employeeSystemId);
                DataView dvEmpSalary = new DataView(dtEmpSalary);
                dvBonus = new DataView(dtEmpSalary);

                dvEmpSalary.RowFilter = "HeadCategory = 'GROSS'";
                //dvBonus.RowFilter = "Salary";


                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);

                objRpt.SelectedPlant(identity.PlantId, out dsFactory);

                #endregion DataSet


                #region Gratuity Calculation
                if (dtEmpGratuity.Rows.Count > 0)
                {
                    empName = dtEmpGratuity.Rows[0]["EmployeeName"].ToString();
                    DOJ = dtEmpGratuity.Rows[0]["DOJ"].ToString();
                    DOS = dtEmpGratuity.Rows[0]["DOS"].ToString();



                    if (Convert.ToInt64(dtEmpGratuity.Rows[0]["totalYear"]) >= 5)
                    {
                        if (Convert.ToInt64(dtEmpGratuity.Rows[0]["totalMonthAfterYear"]) >= 6)
                        {
                            eligibleYear = Convert.ToInt32(dtEmpGratuity.Rows[0]["totalYear"]) + 1;
                        }
                        else
                        {
                            eligibleYear = Convert.ToInt32(dtEmpGratuity.Rows[0]["totalYear"]);
                        }
                        eligibleGratuityAmount = (Convert.ToDouble(dtEmpGratuity.Rows[0]["EntryAmount"].ToString()) / 26) * 15 * Convert.ToDouble(eligibleYear);
                    }
                }
                #endregion

                #region Leave Encashment Calculation
                if (dtEmpLeaveEncash.Rows.Count > 0)
                {
                    encachedLeave = Convert.ToDouble(dtEmpLeaveEncash.Rows[0]["Amount"].ToString());
                }
                #endregion

                #region Salary Calculation // Current wages

                currentWages = Convert.ToDouble(dvEmpSalary.Table.Rows[0]["DisbusmentAmount"]);
                var currencyId = dvEmpSalary.Table.Rows[0]["DisbusmentCurrencyID"].ToString();

                #endregion
                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);

                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString().ToUpper();
                    cmpAddress = dsCmp.Tables[0].Rows[0]["cAddress1"].ToString();
                }
                else
                {
                    CmpName = "";
                    cmpAddress = "";
                }
                
                string fileName = "FinalSettlementVoucher" + identity.PlantId + ".docx";
                string strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), /*"IDCardBengali.xlsx"*/fileName);  // IDCardEng.xlsx
                string File = strPath;
                if (!System.IO.File.Exists(strPath))
                {
                    throw new CustomException("File <" + fileName + "> Not Found.");
                }

                ////A opens input document.
                WordDocument document = new WordDocument(File, FormatType.Docx);
                //Gets the paragraph at index 1
                try
                {

                    WSection section = document.Sections[0];

                    DataTable dtEmpMaster, dtSalary;
                    dtEmpMaster = null;
                    DataView dvEmp = null;
                    dvEmp = new DataView();
                    dvEmp.Table = dtEmpGratuity;

                    //dtEmpMaster = dvEmp.ToTable(true, "SystemId", "EmployeeName", "FatherName", "GenderID", "Salutation");
                    //DataView dvSalaryHead = new DataView(dtEmpSalary);

                    //dvSalaryHead.Sort = "HeadType desc,Sequence";
                    //DataTable dtSalaryHead = dvSalaryHead.ToTable(true, "SalaryHeadID", "SalaryHead", "HeadType", "Sequence", "HeadCategory", "IsGrossComponent", "IntegerInDisb", "DecimalNo", "DisbusmentAmount", "DisbusmentCurrencyID");

                    Dictionary<string, string> columns = new Dictionary<string, string>();

                    dtSalary = null;                   

                    var totalPayable = 0.00;
                    var totalAmount = Convert.ToDouble(eligibleGratuityAmount) + Convert.ToDouble(encachedLeave) + Convert.ToDouble(currentWages) + Convert.ToDouble(bonus) + Convert.ToDouble(exGratia);

                    document.Replace("{CompanyName}", identity.CompanyName, true, true);
                    document.Replace("{CompanyAddress}", cmpAddress, true, true);
                    document.Replace("{EmployeeName}", empName, true, true);
                    document.Replace("{DOJ}", DOJ, true, true);
                    document.Replace("{DOS}", DOS, true, true);

                    var totalInWords = ""; //ru.InWord((totalAmount), dtSalaryHead.Rows[0]["DisbusmentCurrencyID"].ToString());

                    document.Replace("{amountinNumber}", totalAmount.ToString("#,##0.00"), true, true);
                    document.Replace("{amountInWord}", ru.InWord(totalAmount,currencyId), true, true);
                    document.Replace("{GratuityAmount}", eligibleGratuityAmount.ToString("#,##0.00"), true, true);
                    document.Replace("{LeaveEncashmentAmount}", encachedLeave.ToString("#,##0.00"), true, true);
                    document.Replace("{CurrentWage}", currentWages.ToString("#,##0.00"), true, true);





                    Dictionary<string, int> ReplaceInfo = new Dictionary<string, int>();

                    TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));

                    //List<string> strReplace = new List<string>();
                    //for (int i = 0; i < allresult.Length; i++)
                    //    strReplace.Add(allresult[i].SelectedText.ToString().ToUpper());

                    //StringCollection strColDistinct = new StringCollection();
                    //for (int i = 0; i < strReplace.Count; i++)
                    //{
                    //    if (strColDistinct.Contains(strReplace[i].ToUpper()))
                    //        continue;

                    //    strColDistinct.Add(strReplace[i].ToUpper());

                    //    string text = strReplace[i].ToUpper();
                    //    ReplaceInfo.Add(text, 0);
                    //    if (columns.ContainsKey(text.ToUpper()))
                    //    {
                    //        ReplaceInfo[text] = document.Replace(text, dtEmpMaster.Rows[0][columns[text.ToUpper()]].ToString(), false, false);
                    //    }
                    //}


                    //foreach (var item in ReplaceInfo.Keys)
                    //{
                    //    if (ReplaceInfo[item.ToString()] == 0)
                    //        document.Replace(item.ToString(), "", false, false);

                    //}

                    DocToPDFConverter converter = new DocToPDFConverter(); //----ai line ta new kono report a bosanor for error asbe ---suzation thake prothm ta chose kore dita hoba----

                    PdfDocument pdfDocument = converter.ConvertToPDF(document);
                    pdfDocument.PageSettings.Width = 1200;
                    pdfDocument.PageSettings.Orientation = PdfPageOrientation.Landscape;
                    converter.Dispose();

                    string Prefix = "FinalSettlementVoucher" + employeeSystemId;

                    pdfDocument.Save(Prefix + ".pdf", System.Web.HttpContext.Current.Response, HttpReadType.Save);
                    pdfDocument.Close(true);
                    document.Close();

                    return Json(new { FileName = Prefix, Error = false }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception ex)
                {
                    return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
                    // throw ex;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public DataTable GetEmployeeSalaryInfoDetail(string companyGroupId, string companyId, string plantId, string fromDate, string toDate, string empSystemId)
        {
            string strSQL;
            DataSet dsRef = null;
            Dictionary<string, List<DataRow>> dicBonus = new Dictionary<string, List<DataRow>>();
            
            try
            {
                strSQL = @"select EmpSlr.*,PSH.Sequence,crc.IsDecimalInDisb,crc.IntegerInDisb,CRC.DecimalNo from(SELECT SPC.SystemID AS SlrProcChdSysID, SPC.SlrProcMstSystemID, SPM.SalaryProcID, SPM.FromDate, SPM.ToDate,
                                                    SPC.EmpInfoSystemID EmpSystemID, SPC.PlantID, SPM.UserGroupSystemID, SPM.MonthNo, SPM.YearNo, SPC.PayAbleShSystemID,
                                                    SPC.SalaryHeadID, SPC.EntryCurrencyID, SPC.EntryAmount, SPC.DefineCurrencyID, SPC.DefineAmount,
                                                    SPC.DisbusmentCurrencyID, SPC.DisbusmentAmount, SPC.AcltExcDisbSlrHDID, SPC.AcltExcDisbSlrHDAmt,
                                                    CRE.Name AS PlantWiseExchangeCR, EXR.ToCurrencyBuying ExchangeRate, SPM.AmtDefinitionCurrencyID,
                                                    CR.Name AS AmtDefinitionCurrency, SPM.AmtDefinitionCurrencyRate, SPC.IsNetPayEffect, SH.IsCTCComponent, SH.IsGrossComponent
                                                    , sh.SalaryHead, sh.HeadCategory, sh.HeadType, SH.PartOfNetPay

                                     FROM SalaryProcChild SPC

                                        left JOIN SalaryProcMaster SPM ON SPC.SlrProcMstSystemID = SPM.SystemID



                                                        LEFT JOIN SalaryHead sh on sh.SalaryHeadID= spc.SalaryHeadID


                                                        LEFT JOIN scs.Currency CR ON SPM.AmtDefinitionCurrencyID = CR.Id

                                                        LEFT JOIN (
                                                                   SELECT* FROM ExchangerateDateWiseForHR

                                                                   WHERE FromDate IN (SELECT MAX(FromDate) FromDate FROM SalaryProcMaster


                                                                                                            WHERE SystemID IN(SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = Month('" + fromDate + @"') AND YearNo = Year('" + fromDate + @"')  )
                                                                                    )
																  ) EXR ON SPM.AmtDefinitionCurrencyID = EXR.FromCurrencyCode

                                                                                            AND SPC.PlantID = Exr.PlantID

                                                        LEFT JOIN SCS.Currency CRE ON EXR.FromCurrencyCode = CRE.Id

                                                        where isnull(SPC.SlrProcMstSystemID,'')  IN(SELECT SystemID FROM SalaryProcMaster
                                      WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = Month('" + fromDate + @"') AND YearNo = Year('" + fromDate + @"'))
											) EmpSlr--ON EmpBasic.SystemID = EmpSlr.EmpInfoSystemID AND EmpBasic.PlantID = EmpSlr.PlantID

                                            Inner join EmployeeInformation EEI ON EEI.SystemId = EmpSlr.EmpSystemID

                                         LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = EEI.SalaryRuleMasterSystemID

                                        LEFT JOIN SalaryRuleGeneral SRG ON SRG.SalaryRuleMasterSystemID = SRM.SystemID  AND SRG.SalaryHeadID = EmpSlr.SalaryHeadID
                                        LEFT JOIN(SELECT* FROM [MST].[PlantSalaryHeadSequence] WHERE PlantId = '" + plantId + @"') PSH
                                                                       ON PSH.SalaryHeadId = EmpSlr.SalaryHeadID
                                        LEFT JOIN CurrencyRuleChild CRC ON CRC.MstSystemID = srm.CurrencyRuleSystemID AND CRC.SalaryHeadID = EmpSlr.SalaryHeadID

                                                WHERE EEI.GroupID = '" + companyGroupId + @"' AND  EEI.CompanyId = '" + companyId + @"' AND  EEI.PlantId = '" + plantId + @"'";

               
                            strSQL += @"AND EmpSlr.EmpSystemID  = '"+ empSystemId + @"'";              
                strSQL += "ORDER BY EmpSlr.EmpSystemID";
                //ConnectionManager.clsConnectionManager con = new clsConnectionManager(600);
                //con.getDataSet(strSQL, out dsRef);

               

                //DataTable dt = dsRef.Tables[0];
                //List<DataRow> _data = new List<DataRow>();
                //string empId = "";
                //for (int i = 0; i < dt.Rows.Count; i++)
                //{
                //    if (empId != dt.Rows[i]["EmpSystemID"].ToString())
                //    {
                //        _data = new List<DataRow>();
                //        dicBonus.Add(dt.Rows[i]["EmpSystemID"].ToString(), _data);
                //    }
                //    _data.Add(dt.Rows[i]);

                //    empId = dt.Rows[i]["EmpSystemID"].ToString();
                //}

                return _sqlRepository.GetDataTable(strSQL);


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



        protected void GetSalaryProcessId(string companyGrpId, string companyId, string plantId, string calcDate, out System.Data.DataSet dsRef)
        {
            var monthNo = string.Empty;
            monthNo = Convert.ToDateTime(calcDate).ToString("MM");
            var mNo = Convert.ToInt16(monthNo);
            var yearNo = string.Empty;
            yearNo = Convert.ToDateTime(calcDate).ToString("yyyy");
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT DISTINCT TOP (1) SystemID,SalaryProcID,SalaryProcDate FROM SalaryProcMaster 
                                WHERE MonthNo = " + mNo.ToString() + @" AND  YearNo = " + yearNo + @"
                                                        ORDER BY SalaryProcDate DESC ";
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


    }
}