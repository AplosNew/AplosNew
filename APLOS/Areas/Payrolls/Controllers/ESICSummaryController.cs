using Aplos.Controllers;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Employees;
using Library.Service.Helpers;
using Library.Service.HumanResources;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using static Library.Service.Helpers.ReportUtility;
using static Library.Service.HumanResources.PayRegisterBDReportService;

namespace Aplos.Areas.Payrolls.Controllers
{
    public class ESICSummaryController : BaseController
    {
        #region Constructor

        private readonly IPayRegisterBDReportService _payRegisterBDReportService;
        private readonly IEmployeeProfileService _employeeProfileService;
        private readonly ISqlRepository _sqlRepository;

        public ESICSummaryController(
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

        
        public ActionResult Aplos()
        {
            return View();
        }

        #endregion -- Pages

        #region -- Operations  
        [HttpGet,Authorize]
        public ActionResult GetESICSummaryReport(string year)
        {
            CustomIdentity identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            #region Variable
            clsReport objRpt = null;
            int slCount = 0;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            DataSet dsWalfareSummary = null;
            DataTable dtWalfareSummary = null;
            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = null;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            #endregion Variable

            try
            {
                ru = new ReportUtility();

                objRpt = new clsReport();

                #region Variable
                ParamList para = new ParamList();
                ParamList leavePara = new ParamList();
                ParamList attdnProcessParam = new ParamList();

                var FactoryName = "";
                var CmpName = "";
                #endregion Variable
                var oRU = new ReportUtility();

                var colSr = 0;
                var colTotal = 0;
                var colEmployerShare = 0;
                var colMonth = 0;
                var colEmployeeShare = 0;

                #region DataSet

                objRpt.GetESISummary(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, year, out dsWalfareSummary);
                dtWalfareSummary = dsWalfareSummary.Tables[0];

                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);

                objRpt.SelectedPlant(identity.PlantId, out dsFactory);

                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                xlsRow = 5;
                xlsCol = 1;
                #region------------------Column Header------------------

                SetHeaderValue("S.No.", sheet1, xlsRow, ref xlsCol, out colSr, 6);
                SetHeaderValue("Month", sheet1, xlsRow, ref xlsCol, out colMonth, 10);
                SetHeaderValue("Employee Share", sheet1, xlsRow, ref xlsCol, out colEmployeeShare, 15);
                SetHeaderValue("Employer Share", sheet1, xlsRow, ref xlsCol, out colEmployerShare, 15);
                SetHeaderValue("Total", sheet1, xlsRow, ref xlsCol, out colTotal, 12);
                endXlsCol = colTotal;
                #endregion------------------Column Header------------------
                var fPanRow = xlsRow + 1;//Freeze pan starting rows

                #region Data to Excel Column
                xlsRow++;
                var formulaStartRow = xlsRow;
                for (int mi = 1; mi <= 12; mi++)
                {
                    slCount++;
                    SetSLText(ref sheet1, xlsRow, colSr, slCount);
                    ru.SetText(ref sheet1, xlsRow, colMonth, CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(mi));


                    DataView dvwf = new DataView(dtWalfareSummary);
                    dvwf.RowFilter = "MonthNo='" + mi + "' and HeadCategory='ESIC Employee Contribution'";
                    if (dvwf.Count > 0)
                    {
                        ru.SetText(ref sheet1, xlsRow, colEmployeeShare, Convert.ToInt32(dvwf[0]["DisbusmentAmount"])*-1);

                    }
                    DataView dvwf2 = new DataView(dtWalfareSummary);
                    dvwf2.RowFilter = "MonthNo='" + mi + "' and HeadCategory='ESIC Employer Contribution'";
                    if (dvwf2.Count > 0)
                    {
                        ru.SetText(ref sheet1, xlsRow, colEmployerShare, Convert.ToInt32(dvwf2[0]["DisbusmentAmount"]));

                    }

                    var formulaText = "=SUM(" + ru.GetColumnNameForXls(colEmployeeShare) + xlsRow + "+" + ru.GetColumnNameForXls(colEmployerShare) + xlsRow + ")";

                    ru.SetColFormula(ref sheet1, xlsRow, colTotal, formulaText, false);
                    sheet1.Range[xlsRow, colTotal].HorizontalAlignment = ExcelHAlign.HAlignRight;

                    xlsRow++;
                }
                var summationRowLimit = xlsRow - 1;
                sheet1.Range[xlsRow, colMonth].Text = "Total";
                sheet1.Range[xlsRow, colMonth].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, colMonth].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                sheet1.Range[xlsRow, colEmployeeShare].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, colEmployeeShare].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, colEmployeeShare].Formula = "=SUM(" + ru.GetColumnNameForXls(colEmployeeShare) + formulaStartRow + ":" + ru.GetColumnNameForXls(colEmployeeShare) + (summationRowLimit) + ")";
                sheet1.Range[xlsRow, colEmployeeShare].NumberFormat = ru.NumberFormatInt();


                sheet1.Range[xlsRow, colEmployerShare].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, colEmployerShare].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, colEmployerShare].Formula = "=SUM(" + ru.GetColumnNameForXls(colEmployerShare) + formulaStartRow + ":" + ru.GetColumnNameForXls(colEmployerShare) + (summationRowLimit) + ")";
                sheet1.Range[xlsRow, colEmployerShare].NumberFormat = ru.NumberFormatInt();

                sheet1.Range[xlsRow, colTotal].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, colTotal].HorizontalAlignment = ExcelHAlign.HAlignRight;
                sheet1.Range[xlsRow, colTotal].Formula = "=SUM(" + ru.GetColumnNameForXls(colTotal) + formulaStartRow + ":" + ru.GetColumnNameForXls(colTotal) + (summationRowLimit) + ")";
                sheet1.Range[xlsRow, colTotal].NumberFormat = ru.NumberFormatInt();

                #endregion

                #region *****************Report Header*****************

                objRpt.SelectedPlantWiseCompany(identity.PlantId, out dsCmp);
                xlsRow = 1;
                xlsCol = 1;
                FactoryName = string.Empty;
                var FactoryAddress = string.Empty;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                }
                else
                {
                    CmpName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 12;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                }
                else
                {
                    FactoryName = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryName;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 13;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                if (dsCmp.Tables[0].Rows.Count > 0)
                {
                    FactoryAddress = dsCmp.Tables[0].Rows[0]["Address1"].ToString();
                }
                else
                {
                    FactoryAddress = "";
                }
                sheet1.Range[xlsRow, xlsCol].Text = FactoryAddress;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 13;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                xlsRow += 1;
                sheet1.Range[xlsRow, xlsCol].Text = "ESIC summary Report of: " + year;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 18;
                sheet1.Range[xlsRow, 1].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1].VerticalAlignment = ExcelVAlign.VAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                #endregion *****************Report Header*****************

                #region Freeze Panes
                sheet1.UsedRange["A" + fPanRow].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 5;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                //sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + identity.Name + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                sheet1.Name = "ESIC Summary";
                #endregion
                workbook.Version = ExcelVersion.Excel97to2003;
                string fileName = "ESISummary" + year + ".xls";             
                workbook.SaveAs(fileName, HttpContext.ApplicationInstance.Response, ExcelDownloadType.Open);
                
            }

            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objRpt = null;
                excelEngine = null;
                application = null;
                workbook = null;
                sheet1 = null;
            }

            return null;
        }
        #endregion -- Operations
        public void SetSLText(ref IWorksheet sheet, int row, int col, int txt)
        {
            sheet.Range[row, col].Number = txt;
            sheet.Range[row, col].NumberFormat = NumberFormatIntWithComma();
            //sheet.Range[row, col].ColumnWidth = 15;
            sheet.Range[row, col].HorizontalAlignment = ExcelHAlign.HAlignCenter;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignTop;
            sheet.Range[row, col].BorderAround(ExcelLineStyle.Hair);
        }
        public string NumberFormatIntWithComma()
        {
            return "#,#,#0;";
        }
        private void SetHeaderValue(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width)
        {
            ColIndex = 0;
            sheet.Range[xlsRow, xlsCol].Text = text;
            sheet.Range[xlsRow, xlsCol].ColumnWidth = width;
            sheet.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
            //sheet.Range[row, col].CellStyle.ColorIndex = ExcelKnownColors.Grey_25_percent;
            sheet.Range[xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;
            sheet.Range[xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[xlsRow, xlsCol].BorderAround(ExcelLineStyle.Thin);
            ColIndex = xlsCol;
            xlsCol += 1;
        }
        public void GetESICEmpInfo(out DataSet dsRef, string plantId, int monthName, string year, bool isActive, bool isSeperated)
        {
            string strSQL;
            var days = DateTime.DaysInMonth(Convert.ToInt32(year), monthName);//Number of Days in a month
            string monthNameString = CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(monthName);//Month Name from Month No
            var date = days + "-" + monthNameString + "-" + year;

            string empStatus = "";


            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (isActive == true)
                {
                    empStatus = @"AND EmpBasic.EmployeeStatus = 'Active'";
                }
                if (isSeperated == true)
                {
                    empStatus = @"AND EmpBasic.EmployeeStatus = 'Separated'";
                }
                if (isActive == true && isSeperated == true)
                {
                    empStatus = "";
                }

                strSQL = @"SELECT DISTINCT EmpSlr.EmpInfoSystemID, EmpBasic.EmployeeCode,CONVERT(INT,EmpBasic.EmployeeCode) EmployeeCodeS, EmpBasic.EmployeeName,DocNumber,EmpBasic.Age ,
                       
								(ISNULL(MMDSA.TotalPresent, 0) + ISNULL(MMDSA.TotalLate, 0)) PresentDays,
								ISNULL(MMDSA.TotalHoliDay, 0) HoliDay, ISNULL(MMDSA.TotalWeekOff, 0) WeekOff,
								(ISNULL(MMDSA.TotalLv, 0) + ISNULL(MMDSA.TotalMLv, 0)) LeaveDays,
								FORMAT(CONVERT(dECIMAL(18,2),(
								(ISNULL(MMDSA.TotalPresent, 0) + ISNULL(MMDSA.TotalLate, 0)
								+ ISNULL(MMDSA.TotalWeekOff, 0)+ISNULL(MMDSA.TotalHoliDay, 0) + ISNULL(MMDSA.TotalLv, 0)
								 + ISNULL(MMDSA.TotalMLv, 0)))),'##0.##') workingDays
                                ,EmpSlr.PlantID, EmpSlr.FromDate, EmpSlr.ToDate, EmpSlr.MonthNo, EmpSlr.YearNo, EmpSlr.PayAbleShSystemID,
                                EmpSlr.SalaryHeadID, EmpSlr.EntryCurrencyID, EmpSlr.EntryAmount, EmpSlr.DefineCurrencyID, EmpSlr.DefineAmount,
                                EmpSlr.DisbusmentCurrencyID, EmpSlr.DisbusmentAmount, EmpSlr.AcltExcDisbSlrHDID, EmpSlr.AcltExcDisbSlrHDAmt,
                                EmpSlr.AmtDefinitionCurrencyID,
                                EmpSlr.AmtDefinitionCurrencyRate, EmpSlr.IsNetPayEffect
                                ,EMPSLR.cat,EmpSlr.SalaryHead,EmpSlr.HeadCategory,EmpSlr.HeadType,IsCTCComponent,IsGrossComponent
                                ,ISNULL(empslr.IsDecimalInDisb,0) IsDecimalInDisb,ISNULL(empslr.DecimalNo,0) DecimalNo,ISNULL(empslr.IntegerInDisb,0) IntegerInDisb
                            FROM
                                    (
										 SELECT E.SystemID, E.EmployeeCode, E.EmployeeName, E.DOJ, E.EmployeeStatus,ED.DocNumber,DATEDIFF(YY,E.DOB,'" + date + @"') As Age,
											DG.UserName DesignationGroupName, E.DesignationSystemID, DE.UserName DesignationName,GVDE.UserName GivenDesignationName,
											'' UserGroupSystemID, E.PlantID, F.UserName PlantName, E.UnitID,
											FU.UserName UnitName, E.DivisionID, DV.UserName DivisionName, E.DepartmentID, DP.UserName DepartmentName,
											E.SectionID, S.UserName SectionName, E.SubSectionID, SS.UserName SubSectionName, E.EmployeeCategorySystemID,
											EC.UserName EmpCategoryName--, BK.BankNameShort BankName, BK.BankNameFull, E.BankAccNo
                                            ,egdsgg.GivenDesignationGroup
                                     FROM EmployeeInformation E
												LEFT JOIN org.Plant F ON E.PlantID = F.Id
												LEFT JOIN hkp.DesignationGroup DG ON E.DesignationGroupId = DG.ID
												LEFT JOIN hkp.Designation DE ON E.GivenDesignationId = DE.Id
												LEFT JOIN hkp.Designation GVDE ON E.GivenDesignationId = GVDE.Id
												LEFT JOIN org.Unit FU ON E.UnitID = FU.Id
												LEFT JOIN org.Division DV ON E.DivisionID = DV.Id
												LEFT JOIN org.Department DP ON E.DepartmentID = DP.Id
												LEFT JOIN org.Section S ON E.SectionID = S.Id
												LEFT JOIN org.SubSection SS ON E.SubSectionID = SS.Id
												LEFT JOIN
                                                --hkp.EmployeeCategory EC ON E.EmployeeCategorySystemID = EC.Id
                                                (
                                                SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
												LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
												)EC ON EC.DesignationId=E.GivenDesignationId
												LEFT JOIN (SELECT dm.DesignationGroupId,dm.DesignationId,dm.EmployeeCategoryId
									            ,dg.UserName GivenDesignationGroup
									            FROM MST.DesignationMaster dm
									            LEFT JOIN HKP.DesignationGroup dg ON dg.Id=dm.DesignationGroupId
									            ) egdsgg ON egdsgg.DesignationId=e.GivenDesignationId
									            and egdsgg.EmployeeCategoryId=e.EmployeeCategorySystemID
                                                  LEFT JOIN EmployeeDocument ED ON ED.EmpSystemID = E.SystemId
                                                 AND ComplianceDocumentId = 
												(
												SELECT Id	FROM HKP.ComplianceDocument WHERE ProfileType = 'ESIC'
												)		) EmpBasic
												--INNER  JOIN EmployeeDocument ED ON E.SystemId = ED.EmpSystemID
												--INNER JOIN HKP.ComplianceDocument CD ON CD.Id = ED.ComplianceDocumentId  AND CD.ProfileType = 'ESIC' 
												--where E.EmployeeStatus = 'Active' OR E.DOS 

									--) EmpBasic
                                    INNER JOIN
											(
											 SELECT SPC.SystemID AS SlrProcChdSysID, SPC.SlrProcMstSystemID, SPM.SalaryProcID, SPM.FromDate, SPM.ToDate,
													SPC.EmpInfoSystemID, SPC.PlantID, SPM.UserGroupSystemID, SPM.MonthNo, SPM.YearNo, SPC.PayAbleShSystemID,
													SPC.SalaryHeadID, SPC.EntryCurrencyID, SPC.EntryAmount, SPC.DefineCurrencyID, SPC.DefineAmount,
													SPC.DisbusmentCurrencyID, SPC.DisbusmentAmount, SPC.AcltExcDisbSlrHDID, SPC.AcltExcDisbSlrHDAmt,
												    SPM.AmtDefinitionCurrencyID,
													SPM.AmtDefinitionCurrencyRate, SPC.IsNetPayEffect
                                                    ,sh.SalaryHead,sh.HeadCategory,sh.HeadType
                                                    ,sh.IsCTCComponent,sh.IsGrossComponent,sh.Cat,crc.IsDecimalInDisb,crc.DecimalNo,crc.IntegerInDisb
											 FROM SalaryProcChild SPC
														INNER JOIN SalaryProcMaster SPM ON SPC.SlrProcMstSystemID = SPM.SystemID
																							AND SPM.SystemID IN( SELECT SystemID FROM SalaryProcMaster
                                                                WHERE SystemID IN(SELECT SlrProcMstSystemID FROM SalaryProcChild
                                                        WHERE PlantID = '" + plantId + @"' GROUP BY SlrProcMstSystemID)
                                        AND MonthNo = '" + monthName + @"' AND YearNo = '" + year + @"' )
                                                        INNER JOIN (--Salary Head
                                                                               (SELECT *,'B' Cat FROM SalaryHead where HeadCategory in ('Basic'))
																			    UNION														
																				SELECT *,'ESICER' FROM SalaryHead WHERE HeadCategory IN ('ESIC Employer Contribution')
																				UNION
																				SELECT *,'ESICEE' FROM SalaryHead WHERE HeadCategory IN ('ESIC Employee Contribution')
                                                                                UNION
                                                                                SELECT *,'GROSS' FROM SalaryHead WHERE HeadCategory = 'GROSS'																			
																	)--Salary Head 
														SH ON SH.SalaryHeadID=SPC.SalaryHeadID
                                                  INNER JOIN (SELECT EESHE.EmpSystemId,EESHE.SalaryStructureId,EESHE.IsEligible,EESHE.SalaryHeadEnum,SalStruc.SalaryRuleMasterSystemID FROM [EmployeeEligibleForSalaryHeadEnum] EESHE
                                                   INNER JOIN
                                                   (SELECT SystemId SalaryId, EmpInfoSystemID, EffectiveDate, SalaryRuleMasterSystemID FROM SalaryInfoDefineMaster
                                                    UNION
                                                    SELECT SystemId SalaryId, EmpInfoSystemID, EffectiveDate, SalaryRuleMasterSystemID FROM SalaryInfoBackMaster WHERE EffectiveDate <= '" + date + @"' ) 
                        							SalStruc on EESHE.SalaryStructureId = SalStruc.SalaryId and EESHE.EmpSystemId = SalStruc.EmpInfoSystemID where EESHE.SalaryHeadEnum = 'ESIC' AND EESHE.PlantId = '" + plantId + @"' AND IsEligible = 1) 
													ESICELIGIBLE ON SPC.SalaryID = ESICELIGIBLE.SalaryStructureId and SPC.EmpInfoSystemID = ESICELIGIBLE.EmpSystemId
                                                         LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID = ESICELIGIBLE.SalaryRuleMasterSystemID
                                                                LEFT JOIN CurrencyRuleMaster crm on crm.SystemID = sRM.CurrencyRuleSystemID
                                                                LEFT JOIN CurrencyRuleChild crc on crc.MstSystemID = CRM.SystemID		
														
											) EmpSlr ON EmpBasic.SystemID = EmpSlr.EmpInfoSystemID AND EmpBasic.PlantID = EmpSlr.PlantID
                                    LEFT JOIN
		                                    (
											 SELECT EmpSystemID, MonthNo, YearNo, TotalProcDate, TotalPresent, TotalLate,
													TotalAbsent AbsentDays, TotalLv, TotalMLv, TotalCompAssignLv, TotalWeekOff, TotalHoliDay,
													TotalWeekOffHoliDay, TotalOTHr, TotalNormalOTHr, TotalExtraOTHr
				                              FROM SalaryProceAttdnData
											  WHERE   MonthNo = MONTH(CONVERT(DATE,'" + date + @"')) AND
						                                YearNo = YEAR(CONVERT(DATE,'" + date + @"'))	

											) MMDSA ON EmpSlr.EmpInfoSystemID = MMDSA.EmpSystemID 											   
													   WHERE 
														
													EmpSlr.MonthNo = " + monthName + @" 
                                                    AND EmpBasic.PlantId = '" + plantId + @"' " + empStatus + @" ORDER BY EmployeeCodeS ";

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
        }//end function

    }
}