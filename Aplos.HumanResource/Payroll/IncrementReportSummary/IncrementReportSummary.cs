using HRService;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Core;
using Library.Service.Helpers;
using Library.Service.Payrolls.OT;
using OTSBD;
using Syncfusion.DocIO.DLS;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.Payroll.IncrementReportSummary
{
    public class IncrementReportSummary
    {
        SqlRepository _sqlRepository = null;

        public IncrementReportSummary()
        {
            _sqlRepository = new SqlRepository();

        }

        public string SaveFileName { get; private set; }

        
        private string EmployeeInfoSql( string EmpSystemId,string languageId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                        return @"SELECT distinct format(salaryInfoTo.EffectiveDate,'dd-MMM-yyyy')AppraisalDate,
			CONVERT(NUMERIC(10,2),salaryInfoFrom.EntryAmount) PreviousGross,
			CONVERT(NUMERIC(10,2),salaryInfoTo.EntryAmount) NewGross,
			CONVERT(NUMERIC(10,2),salaryInfoTo.EntryAmount-salaryInfoFrom.EntryAmount) IncrementAmount			
             ,sh.SalaryHead
            ,ei.EmpPicPath
            ,ei.Employeecode
            ,ei.Employeename
            
            ,ISNULL(DP.Name, isnull(OLD.Department,dep.username)) as PreviousDepartment
            ,ISNULL(DPN.Name,NEW.Department) as NewDepartment
            ,ISNULL(DG.Name,OLDG.LegalDesignation) as PreviousDesignation
            ,ISNULL(DGN.Name,NEWG.LegalDesignation) as NewDesignation
            ,ISNULL(SG.Name,OLDG.SalaryGrade) as PreviousSalaryGrade
            ,ISNULL(SGN.Name,NEWG.SalaryGrade) as NewSalaryGrade
            
            from
            IncrementHistory IH
            LEFT JOIN (
            SELECT SM.SystemID,SM.EmpInfoSystemID,SM.EffectiveDate,SD.EntryAmount,SD.SalaryHeadID FROM SalaryInfoDefineMaster SM
            LEFT JOIN SalaryInfoDefine SD ON SD.SalaryID=SM.SystemID
            WHERE SM.EmpInfoSystemID='" + EmpSystemId + @"' AND SM.IsApproved=1
            Union
            SELECT SMB.SystemID,SMB.EmpInfoSystemID,SMB.EffectiveDate,SDB.EntryAmount,SDB.SalaryHeadID FROM SalaryInfoBackMaster SMB
            LEFT JOIN SalaryInfoBack SDB ON SDB.SalaryID=SMB.SystemID
            WHERE SMB.EmpInfoSystemID='" + EmpSystemId + @"'
            ) salaryInfoTo on IH.EmpSystemID=salaryInfoTo.EmpInfoSystemID AND IH.ToEffectiveDate=salaryInfoTo.EffectiveDate --and IH.ToSalaryId=salaryInfoTo.SystemID
            LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=salaryInfoTo.SalaryHeadID
            
            LEFT JOIN (
            SELECT SM.SystemID,SM.EmpInfoSystemID,SM.EffectiveDate,SD.EntryAmount,SD.SalaryHeadID FROM SalaryInfoDefineMaster SM
            LEFT JOIN SalaryInfoDefine SD ON SD.SalaryID=SM.SystemID
            WHERE SM.EmpInfoSystemID='" + EmpSystemId + @"' AND SM.IsApproved=1
            Union
            SELECT SMB.SystemID,SMB.EmpInfoSystemID,SMB.EffectiveDate,SDB.EntryAmount,SDB.SalaryHeadID FROM SalaryInfoBackMaster SMB
            LEFT JOIN SalaryInfoBack SDB ON SDB.SalaryID=SMB.SystemID
            WHERE SMB.EmpInfoSystemID='" + EmpSystemId + @"'
              ) salaryInfoFrom on IH.EmpSystemID=salaryInfoFrom.EmpInfoSystemID AND IH.FromEffectiveDate=salaryInfoFrom.EffectiveDate --and IH.FromSalaryId=salaryInfoFrom.SystemID
            LEFT JOIN SalaryHead SH1 ON SH1.SalaryHeadID=salaryInfoFrom.SalaryHeadID
            LEFT JOIN EmployeeInformation ei ON EI.SystemId=salaryInfoTo.EmpInfoSystemID
			LEFT JOIN MST.ManpowerBudget mb ON mb.Id = EI.BudgetCode
            LEFT JOIN ORG.Position PR ON MB.PositionId=PR.Id
			left join org.Department dep on dep.Id = PR.DepartmentId   
            LEFT JOIN hkp.LegalDesignation LD ON IH.ToLegalDesignationId = LD.Id
            left join (
            --Select distinct dep.Id as DepartmentId, dep.UserName as Department ,mpb.Code
            --from org.Position p
            --left join mst.ManpowerBudget mpb on mpb.PositionId = p.Id
            --left join org.Department dep on dep.Id = p.DepartmentId       
			select dep.Id as DepartmentId, dep.UserName as Department ,mb.Code from MST.ManpowerBudget MB
			LEFT JOIN [dbo].[EmployeeBudgetCodeHistory] H ON H.BudgetId=MB.Id AND H.Id=
			(select top(1) Id from [dbo].[EmployeeBudgetCodeHistory] where BudgetId=MB.Id Order BY AddedDate DESC)
			left join org.Position p on p.Id =mb.PositionId 
			left join org.Department dep on dep.Id = p.DepartmentId 
			where h.EmpSystemID='" + EmpSystemId + @"'
            ) OLD on old.Code=IH.FromBudgetCode
            left join (
            --Select distinct dep.Id as DepartmentId, dep.UserName as Department ,mpb.Code
            --from org.Position p
            --left join mst.ManpowerBudget mpb on mpb.PositionId = p.Id
            --left join org.Department dep on dep.Id = p.DepartmentId  
			select dep.Id as DepartmentId, dep.UserName as Department ,e.BudgetCode Code from EmployeeInformation E 
			left join mst.ManpowerBudget mpb on mpb.Id = e.BudgetCode
            LEFT JOIN ORG.Position PR ON mpb.PositionId=PR.Id
			left join org.Department dep on dep.Id = PR.DepartmentId  
            ) NEW on NEW.Code=IH.ToBudgetCode  
            left join (
            select LSG.Id as SalaryGradeId, LSG.UserName SalaryGrade,LD.UserName LegalDesignation,LSGD.LegalDesignationId,lsgd.PlantId from [MST].[LegalSalaryGradeDesignation] LSGD
            LEFT JOIN [SCS].[LegalSalaryGrade] LSG ON LSGD.LegalSalaryGradeId = LSG.Id
            LEFT JOIN hkp.LegalDesignation LD ON LSGD.LegalDesignationId = LD.Id           
            ) NEWG ON NEWG.LegalDesignationId = IH.ToLegalDesignationId and NEWG.PlantId=ei.PlantId            
            left join (
            select LSG.Id as SalaryGradeId,  LSG.UserName SalaryGrade,LD.UserName LegalDesignation,LSGD.LegalDesignationId,lsgd.PlantId from [MST].[LegalSalaryGradeDesignation] LSGD
            LEFT JOIN [SCS].[LegalSalaryGrade] LSG ON LSGD.LegalSalaryGradeId = LSG.Id
            LEFT JOIN hkp.LegalDesignation LD ON LSGD.LegalDesignationId = LD.Id            
            ) OLDG ON OLDG.LegalDesignationId = IH.FROMLegalDesignationId and OLDG.PlantId=ei.PlantId

            LEFT JOIN HKP.LocalLanguage DP ON DP.DepartmentId =OLD.DepartmentId AND DP.LanguageId='" + languageId + @"'                                  
            LEFT JOIN HKP.LocalLanguage DPN ON DPN.DepartmentId =NEW.DepartmentId AND DPN.LanguageId='" + languageId + @"'
            LEFT JOIN HKP.LocalLanguage DG ON DG.LegalDesignationId=OLDG.LegalDesignationId AND DG.LanguageId='" + languageId + @"'
            LEFT JOIN HKP.LocalLanguage DGN ON DGN.LegalDesignationId=NEWG.LegalDesignationId AND DGN.LanguageId='" + languageId + @"'
            LEFT JOIN HKP.LocalLanguage SG ON SG.LegalSalaryGradeId =OLDG.SalaryGradeId AND SG.LanguageId='" + languageId + @"'
            LEFT JOIN HKP.LocalLanguage SGN ON SGN.LegalSalaryGradeId =NEWG.SalaryGradeId AND SGN.LanguageId='" + languageId + @"'
            where IH.EmpSystemID='" + EmpSystemId + @"' and sh.HeadCategory='gross' and sh1.HeadCategory='gross'
            --ORDER BY convert(date,salaryInfoFrom.EffectiveDate)
";

        }
        public void EmployeeInformation(string EmpSystemId,string languageId )
        {
            try
            {                
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;               
                ExcelEngine excelEngine = new ExcelEngine();
                //Instantiate the Excel application object
                IApplication application = excelEngine.Excel;

                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

                sheet.Name = "Increment Summary Report";
                 DataTable dtEmpHeaderInfo = GetEmpHeaderInfo(EmpSystemId,languageId);
                string sql = EmployeeInfoSql(EmpSystemId , languageId);
                DataTable dtEmployeeData = _sqlRepository.GetDataTable(sql);
                ReportUtility ru = new ReportUtility();

                var lang = _sqlRepository.GetDataCollection(@"SELECT * FROM scs.Language where Id='" + languageId + "'");//GetLanguage(plantId, tempId, reportType);

                Dictionary<string, string> labelList = ru.LocalLanguageLabelList(identity.PlantId, languageId);


                var localLanguage = "";
                var printFont = "";
                bool isLocalLanguage = false;
                localLanguage = ru.LocalLanguageListSql(identity.PlantId , languageId, out isLocalLanguage);
                if (localLanguage == "Bengali")
                {
                    printFont = "SolaimanLipi";
                }
                else
                {
                    printFont = "Arial Narrow";
                }
                int ROW = 6;
                int COL = 1;

                #region Headerinfo

                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.WorkerName.ToString(), "Employee Name"), sheet, ROW , ref COL, out int colEmployeeName, 15, printFont, 0, 11);

                sheet.Range[ ROW, colEmployeeName, ROW, colEmployeeName + 1].Merge();
                if (languageId == "7")
                {                    
                        sheet[ROW, colEmployeeName + 2].Text = dtEmpHeaderInfo.Rows[0]["EmployeeNameLocal"].ToString();
                }
                else
                {
                    sheet[ROW, colEmployeeName + 2].Text = dtEmpHeaderInfo.Rows[0]["EmployeeName"].ToString();
                }               
                sheet.Range[ROW, colEmployeeName + 2, ROW, colEmployeeName + 3].Merge();
                COL += 4;
                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.IDNo.ToString(), "Employee ID No"), sheet, ROW, ref COL, out int colEmployeeIDNo, 15, printFont, 0, 11);
                sheet[ROW, colEmployeeIDNo + 1].Text =ru.cnDgt( dtEmpHeaderInfo.Rows[0]["EmployeeCode"].ToString(), lang[0]["UserName"].ToString());
                sheet.Range[ROW, colEmployeeIDNo + 1, ROW, colEmployeeIDNo + 2].Merge();

                ROW++;

                COL = colEmployeeName;
                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.Department.ToString(), "Department"), sheet, ROW, ref COL, out int colDepartment, 15, printFont, 0, 11);
                sheet.Range[ROW, colDepartment, ROW, colDepartment + 1].Merge();
                sheet[ROW, colDepartment + 2].Text = dtEmpHeaderInfo.Rows[0]["Department"].ToString();
                sheet.Range[ROW, colDepartment + 2, ROW, colDepartment + 3].Merge();

                COL += 4;
                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.DOJ.ToString(), "Joining Date"), sheet, ROW, ref COL, out int colJoiningDate, 15, printFont, 0, 11);
                sheet[ROW, colJoiningDate + 1].Text =ru.GetFormatedDateA( dtEmpHeaderInfo.Rows[0]["DateOfJoin"].ToString() , lang[0]["UserName"].ToString());               
                sheet.Range[ROW, colJoiningDate + 1, ROW, colJoiningDate + 2].Merge();

                ROW++;

                COL = colEmployeeName;
                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.Designation.ToString(), "Designation"), sheet, ROW, ref COL, out int colDesignation, 15, printFont, 0, 11);
                sheet.Range[ROW, colDesignation, ROW, colDesignation + 1].Merge();
                sheet[ROW, colDesignation + 2].Text = dtEmpHeaderInfo.Rows[0]["Designation"].ToString();
                sheet.Range[ROW, colDesignation + 2, ROW, colDesignation + 3].Merge();

                COL += 4;
                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.Section.ToString(), "Section"), sheet, ROW, ref COL, out int colSection, 15, printFont, 0, 11);
                sheet[ROW, colSection + 1].Text = dtEmpHeaderInfo.Rows[0]["Section"].ToString();
                sheet.Range[ROW, colSection + 1, ROW, colSection + 2].Merge();

                ROW++;
                COL = colEmployeeName;      
                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.StaffCategory.ToString(), "Staff Category"), sheet, ROW, ref COL, out int colCategory, 15, printFont, 0, 11);
                sheet.Range[ROW, colCategory, ROW, colCategory + 1].Merge();
                sheet[ROW, colCategory + 2].Text = dtEmpHeaderInfo.Rows[0]["Category"].ToString();
                sheet.Range[ROW, colCategory + 2, ROW, colCategory + 3].Merge();


                ROW++;
                #endregion
                COL = colEmployeeName;
                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.SrNo.ToString(), "Sl.No"), sheet, ROW, ref COL, out int colSlNo, 6, printFont, 0, 11);
                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.AppraisalDate.ToString(), "Appraisal Date"), sheet, ROW , ref COL, out int colAppraisalDate, 15, printFont, 0, 11);
                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.PreviousDepartment.ToString(), "Previous Department"), sheet, ROW, ref COL, out int colPreviousDepartment, 15, printFont, 0, 11);
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;
                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.PreviousGross.ToString(), "Previous Gross"), sheet, ROW, ref COL, out int colPreviousGross, 15, printFont, 0, 11);
                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.PreviousDesignation.ToString(), "Previous Designation"), sheet, ROW, ref COL, out int colPreviousDesignation, 15, printFont, 0, 11);
                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.PreviousGrade.ToString(), "Previous Grade"), sheet, ROW, ref COL, out int colPreviousSalaryGrade, 15, printFont, 0, 11);
                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.NewDepartment.ToString(), "New Department"), sheet, ROW, ref COL, out int colNewDepartment, 15, printFont, 0, 11);
                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.NewDesignation.ToString(), "New Designation"), sheet, ROW, ref COL, out int colNewDesignation, 15, printFont, 0, 11);
                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.NewGrade.ToString(), "New Grade"), sheet, ROW, ref COL, out int colNewGrade, 15, printFont, 0, 11);
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;

                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.NewGross.ToString(), "New Gross"), sheet, ROW , ref COL, out int colNewGross, 15, printFont, 0, 11);
                sheet[ROW, COL].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;

                SetCellValueBangla(ru.GetLabelname(labelList, LabelNameInLocalLanguage.IncrementAmount.ToString(), "Increment Amount"), sheet, ROW, ref COL, out int colIncrementAmount, 15, printFont, 0, 11);
                COL--;
                 int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                ROW++;
                
                int StartRow = ROW; //row 20
                for (int i = 0; i < dtEmployeeData.Rows.Count; i++)
                {

                    sheet[ROW, colSlNo].Number = (i + 1);

                    sheet[ROW, colAppraisalDate].Text = ru.GetFormatedDateA( dtEmployeeData.Rows[i]["AppraisalDate"].ToString(), lang[0]["UserName"].ToString());
                    sheet[ROW, colPreviousDepartment].Text = dtEmployeeData.Rows[i]["PreviousDepartment"].ToString();
                    sheet[ROW, colPreviousGross].Text =ru.cnDgt(dtEmployeeData.Rows[i]["PreviousGross"].ToString(), lang[0]["UserName"].ToString());
                   // sheet[ROW, colPreviousGross].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet[ROW, colPreviousGross].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;

                    sheet[ROW, colPreviousDesignation].Text = dtEmployeeData.Rows[i]["PreviousDesignation"].ToString();
                    sheet[ROW, colNewDepartment].Text = dtEmployeeData.Rows[i]["NewDepartment"].ToString();
                    sheet[ROW, colNewDesignation].Text = dtEmployeeData.Rows[i]["NewDesignation"].ToString();
                    sheet[ROW, colNewGrade].Text = dtEmployeeData.Rows[i]["NewSalaryGrade"].ToString();
                    sheet[ROW, colPreviousSalaryGrade].Text = dtEmployeeData.Rows[i]["PreviousSalaryGrade"].ToString();
                    sheet[ROW, colIncrementAmount].Text =ru.cnDgt(dtEmployeeData.Rows[i]["IncrementAmount"].ToString(), lang[0]["UserName"].ToString());
                    sheet[ROW, colIncrementAmount].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;

                    //sheet[ROW, colIncrementAmount].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet[ROW, colNewGross].Text =ru.cnDgt(dtEmployeeData.Rows[i]["NewGross"].ToString(), lang[0]["UserName"].ToString());
                    sheet[ROW, colNewGross].CellStyle.HorizontalAlignment = ExcelHAlign.HAlignRight;

                    //sheet[ROW, colNewGross].NumberFormat = "#,##0.00;(#,##0.00)";
                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;

                }

                sheet.IsGridLinesVisible = false;
                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

                sheet["A" + StartRow.ToString()].FreezePanes();

                sheet.Range[StartRow, colSlNo, ROW, colSlNo].NumberFormat = clsStaticInfo.NumberFormat();
                sheet.Range[StartRow, colSlNo, ROW, colSlNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;


                ReportUtility reportUtility = new ReportUtility();
                reportUtility.PlantHeader(ref sheet, endCol, "Increment Summary Report", identity.PlantId);
                reportUtility.PageSetup(ref sheet, 5, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 5, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                string strFileName = "IncrementSummaryReport.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception)
            {

                throw;
            }
        }
        private void SetCellValueBangla(string text, IWorksheet sheet, int xlsRow, ref int xlsCol, out int ColIndex, double width, string printFont, int rotationDegree, double fontSize)
        {
            ColIndex = 0;
            sheet.Range[xlsRow , xlsCol].Text = text;
            sheet.Range[xlsRow , xlsCol].ColumnWidth = width;
            sheet.Range[xlsRow , xlsCol].CellStyle.Font.FontName = printFont;
            sheet.Range[xlsRow , xlsCol].CellStyle.Rotation = rotationDegree;
            sheet.Range[xlsRow , xlsCol].CellStyle.Font.Size = fontSize;
            ColIndex = xlsCol;
            xlsCol += 1;
        }
        public Dictionary<string, object> GetLanguage(string plantId, string pkId, string templateType)
        {
            Library.Service.Enums.LetterType.ServiceBook.GetDescription();
            var sql = @"SELECT Id,Language FROM SCS.RptConfigTemplate WHERE  Id='" + pkId + "'  AND PlantId='" + plantId + "' and type='" + templateType + "'";
            //var sql = "SELECT Id,Language FROM SCS.RptConfigTemplate WHERE  [type]='" + pkId + "'  AND PlantId='" + plantId + "'";
            return _sqlRepository.GetData(sql);
        }

        public void IncrementSummaryReport(string EmpSystemId , string languageId)
        {
            try
            {

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsCompanyInfo;
                ConnectionManager.DAL.ConManager objCon;

                ReportUtility oRU = new ReportUtility();
                string File = "";
                string strPath = "";

                File = "Appraisal Report.docx";
                strPath = Path.Combine(ResourcesPathReader.GetConfirmationLetterPath(), File);  // IDCardEng.xlsx
                if (!System.IO.File.Exists(strPath))
                {
                    throw new CustomException("File <" + File + "> Not Found.");
                }


                FileInfo DocFile = new FileInfo(strPath);
                if (DocFile.Exists == false)
                {
                    //DocFile = new FileInfo(System.Web.HttpContext.Current.Server.MapPath(".") + "\\Doc1.docx");
                    throw new CustomException("File Not Found");


                    string strCompanyInfoSQL = @"select a.Address1 CompanyAddress, c.UserName CompanyName from org.Company c
                                            left join  mst.addressmaster a on a.id=c.AddressMasterId
                                            where c.id='" + identity.CompanyId + @"'";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(strCompanyInfoSQL, out dsCompanyInfo, false, "1");

                    DataTable dtEmpInfo = GetEmpInfo(EmpSystemId);
                    DataTable dtEmpHeaderInfo = GetEmpHeaderInfo(EmpSystemId,languageId);


                    ////A opens input document.
                    WordDocument document = new WordDocument(DocFile.FullName);
                    WSection section = document.Sections[0];
                    //TextSelection[] allresult = document.FindAll(new Regex("{.*?}"));
                    //Dictionary<string, int> replaced = new Dictionary<string, int>();



                    document.Replace("{EmployeeCode}", dtEmpHeaderInfo.Rows[0]["Id"].ToString(), false, true);
                    document.Replace("{EmployeeName}", dtEmpHeaderInfo.Rows[0]["Name"].ToString(), false, true);
                    document.Replace("{DateOfJoin}", dtEmpHeaderInfo.Rows[0]["DateOfJoin"].ToString(), false, true);
                    document.Replace("{DepartmentName}", dtEmpHeaderInfo.Rows[0]["Department"].ToString(), false, true);
                    document.Replace("{SectionName}", dtEmpHeaderInfo.Rows[0]["Section"].ToString(), false, true);
                    document.Replace("{DesignationName}", dtEmpHeaderInfo.Rows[0]["Designation"].ToString(), false, true);

                    document.Replace("{Category}", dtEmpHeaderInfo.Rows[0]["Category"].ToString(), false, true);
                    //document.Replace("{Line}", dtEmpHeaderInfo.Rows[0]["Line"].ToString(), false, true);

                    //document.Replace("{Agreement}", dtEmpHeaderInfo.Rows[0]["Agreement"].ToString(), false, true);


                    makeAppraisalDetailTable(document, dtEmpInfo);




                    //#endregion
                    SaveFileName = File;
                    document.Save(SaveFileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.InBrowser);
                    //document.Save(SaveFileName, Syncfusion.DocIO.FormatType.Automatic, System.Web.HttpContext.Current.Response, Syncfusion.DocIO.HttpContentDisposition.Attachment);
                    document.Close();

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable GetEmpInfo(string EmpSystemId)
        {

            string Sql = @" SELECT salaryInfoTo.EffectiveDate As AppraisalDate,salaryInfoFrom.EntryAmount PreviousGross,salaryInfoTo.EntryAmount NewGross,salaryInfoTo.EntryAmount-salaryInfoFrom.EntryAmount IncrementAmount,sh.SalaryHead,LD.UserName PreviousDesignation,'' NewDesignation,ei.EmpPicPath from  
IncrementHistory IH 
LEFT JOIN (
SELECT SM.SystemID,SM.EmpInfoSystemID,SM.EffectiveDate,SD.EntryAmount,SD.SalaryHeadID FROM SalaryInfoDefineMaster SM
LEFT JOIN SalaryInfoDefine SD ON SD.SalaryID=SM.SystemID
WHERE SM.EmpInfoSystemID='" + EmpSystemId + @"' AND SM.IsApproved=1
Union
SELECT SMB.SystemID,SMB.EmpInfoSystemID,SMB.EffectiveDate,SDB.EntryAmount,SDB.SalaryHeadID FROM SalaryInfoBackMaster SMB
LEFT JOIN SalaryInfoBack SDB ON SDB.SalaryID=SMB.SystemID
WHERE SMB.EmpInfoSystemID='" + EmpSystemId + @"'
) salaryInfoTo on IH.EmpSystemID=salaryInfoTo.EmpInfoSystemID AND IH.ToEffectiveDate=salaryInfoTo.EffectiveDate and IH.ToSalaryId=salaryInfoTo.SystemID
LEFT JOIN SalaryHead SH ON SH.SalaryHeadID=salaryInfoTo.SalaryHeadID

LEFT JOIN (
SELECT SM.SystemID,SM.EmpInfoSystemID,SM.EffectiveDate,SD.EntryAmount,SD.SalaryHeadID FROM SalaryInfoDefineMaster SM
LEFT JOIN SalaryInfoDefine SD ON SD.SalaryID=SM.SystemID
WHERE SM.EmpInfoSystemID='" + EmpSystemId + @"' AND SM.IsApproved=1
Union
SELECT SMB.SystemID,SMB.EmpInfoSystemID,SMB.EffectiveDate,SDB.EntryAmount,SDB.SalaryHeadID FROM SalaryInfoBackMaster SMB
LEFT JOIN SalaryInfoBack SDB ON SDB.SalaryID=SMB.SystemID
WHERE SMB.EmpInfoSystemID='" + EmpSystemId + @"'
) salaryInfoFrom on IH.EmpSystemID=salaryInfoFrom.EmpInfoSystemID AND IH.FromEffectiveDate=salaryInfoFrom.EffectiveDate and IH.FromSalaryId=salaryInfoFrom.SystemID
LEFT JOIN SalaryHead SH1 ON SH1.SalaryHeadID=salaryInfoFrom.SalaryHeadID
LEFT JOIN EmployeeInformation ei ON EI.SystemId=salaryInfoTo.EmpInfoSystemID                          
LEFT JOIN hkp.LegalDesignation LD ON IH.ToLegalDesignationId = LD.Id


where IH.EmpSystemID='" + EmpSystemId + @"' and sh.HeadCategory='gross' and sh1.HeadCategory='gross'
ORDER BY convert(date,salaryInfoFrom.EffectiveDate) ";
            return _sqlRepository.GetDataTable(Sql);
        }

        private DataTable GetEmpHeaderInfo(string EmpSystemId , string languageId)
        {

            string Sql = @"select
e.SystemId,e.EmployeeCode ,format(e.DOJ,'dd-MMM-yyyy')DateOfJoin,
L.UserName as Line
,e.EmploymentType as Agreement ,
e.EmployeeNameLocal,e.EmployeeName
,ISNULL(DP.Name, d.UserName ) as Department
,ISNULL(DG.Name,LG.UserName) as Designation
,ISNULL(SC.Name,S.UserName) as Section
,ISNULL(CT.Name,C.UserName) as Category
from EmployeeInformation as e
LEFT JOIN MST.ManpowerBudget mb ON mb.Id = e.BudgetCode
LEFT JOIN ORG.Position PO ON MB.PositionId=PO.Id
left outer join ORG.Department as d on d.Id=PO.DepartmentId
left outer join HKP.LegalDesignation as LG on LG.Id=e.LegalDesignationId
left outer join org.Section as S on S.Id=PO.SectionId
LEFT JOIN MST.DesignationMaster dm ON E.GivenDesignationId = dm.DesignationId
LEFT JOIN HKP.EmployeeCategory C ON C.Id=DM.EmployeeCategoryId
left outer join ORG.line as L on L.Id=MB.LineId
LEFT JOIN HKP.LocalLanguage DP ON DP.DepartmentId =PO.DepartmentId AND DP.LanguageId='" + languageId + @"'
LEFT JOIN HKP.LocalLanguage SC on SC.SectionId=PO.SectionId AND SC.LanguageId='" + languageId + @"'
LEFT JOIN HKP.LocalLanguage DG ON DG.LegalDesignationId=e.LegalDesignationId AND DG.LanguageId='" + languageId + @"'
LEFT JOIN HKP.LocalLanguage CT ON CT.EmployeeCategoryId=e.BudgetCategoryID AND CT.LanguageId='" + languageId + @"'

 where e.SystemId ='" + EmpSystemId + @"'

";
            return _sqlRepository.GetDataTable(Sql);
        }

        public void makeAppraisalDetailTable(WordDocument document, DataTable dsEmpMaster)
        {
            string replaceString = "{employeeTable}";

            //DataTable dsOrderItems, dsTax;



            int LasColumnIndex = 6;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();

            //LasColumnIndex++;
            //dicTaxes.Add("totaltax", LasColumnIndex);



            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            wTable.TableFormat.IsAutoResized = true;

            int ROW = 0; int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();
            #region column headers
            document.EnsureMinimal();


            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = true;

            IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("S.NO.");
            range.ApplyCharacterFormat(FontBold);
            int colSlNo = COL; COL++;
            wTable.Rows[ROW].Cells[colSlNo].Width = 40;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Appraisal Date");
            range.ApplyCharacterFormat(FontBold);
            int colAppraisalDate = COL; COL++;
            wTable.Rows[ROW].Cells[colAppraisalDate].Width = 80;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Previous Gross ");
            range.ApplyCharacterFormat(FontBold);
            int colPreviousGross = COL; COL++;
            wTable.Rows[ROW].Cells[colPreviousGross].Width = 80;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Previous Designation");
            range.ApplyCharacterFormat(FontBold);
            int colPreviousDesignation = COL; COL++;
            wTable.Rows[ROW].Cells[colPreviousDesignation].Width = 40;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("New Gross");
            range.ApplyCharacterFormat(FontBold);
            int colNewGross = COL; COL++;
            wTable.Rows[ROW].Cells[colNewGross].Width = 40;

            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("New Designation");
            range.ApplyCharacterFormat(FontBold);
            int colNewDesignation = COL; COL++;
            wTable.Rows[ROW].Cells[colNewDesignation].Width = 40;


            range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Increment Amount");
            range.ApplyCharacterFormat(FontBold);
            int colIncrementAmount = COL; COL++;
            wTable.Rows[ROW].Cells[colIncrementAmount].Width = 60;

            #endregion column headers
            double totalValue = 0;
            int startRow = ROW + 1;
            int slno = 0;
            for (int i = 0; i < dsEmpMaster.Rows.Count; i++)
            {
                slno++;
                ROW++;
                wTable.AddRow();
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {
                    foreach (WParagraph item in TROW.Cells[CE].Paragraphs)
                    {
                        item.Text = "";
                    }
                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                //TROW.Cells[colSLId].AddParagraph().AppendText(sl.ToString());
                //TROW.Cells[colSLId].Width = 30;
                TROW.Cells[colSlNo].AddParagraph().AppendText(slno.ToString());
                TROW.Cells[colAppraisalDate].AddParagraph().AppendText(Convert.ToDateTime(dsEmpMaster.Rows[i]["AppraisalDate"]).ToString("dd-MMM-yyyy"));
                TROW.Cells[colPreviousGross].AddParagraph().AppendText(clsStdLib.dbl(dsEmpMaster.Rows[i]["PreviousGross"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colPreviousDesignation].AddParagraph().AppendText(dsEmpMaster.Rows[i]["PreviousDesignation"].ToString());
                TROW.Cells[colNewGross].AddParagraph().AppendText(clsStdLib.dbl(dsEmpMaster.Rows[i]["NewGross"].ToString()).ToString("#,##0.00"));
                TROW.Cells[colNewDesignation].AddParagraph().AppendText(dsEmpMaster.Rows[i]["NewDesignation"].ToString());
                TROW.Cells[colIncrementAmount].AddParagraph().AppendText(clsStdLib.dbl(dsEmpMaster.Rows[i]["IncrementAmount"].ToString()).ToString("#,##0.00"));





                //TROW.Cells[colTotalTaxableAmount].AddParagraph().AppendText(clsStdLib.dbl(dsEmpMaster.Rows[i]["IncrementAmount"].ToString()).ToString("#,##0.00"));



            }
            WSection section = document.Sections[0];
            if (!string.IsNullOrEmpty(dsEmpMaster.Rows[0]["EmpPicPath"].ToString()))
            {
                var pic = dsEmpMaster.Rows[0]["EmpPicPath"].ToString();
                //string picpath = ResourcesPathReader.GetEmployeeDestinationPicPath() + pic;
                var picpath = Path.Combine(ResourcesPathReader.GetEmployeeDestinationPicPath()+ pic);

                //WPicture ImgwPicture = new WPicture(document);
               
                if (System.IO.File.Exists(picpath))
                {
                    try
                    {
                        Image Img = Image.FromFile(picpath);
                        Image newImage = ResizeImageDoc(Img, 120, 120);
                        //wPicture.LoadImage(Image.FromFile(picpath));
                        //TextBodyPart textBodyPart = new TextBodyPart(document);

                        section.Tables[0].Rows[1].Cells[2].Paragraphs[0].AppendPicture(newImage);

                        //document.Replace()
                        //document.Replace("{emppic}", textBodyPart, true, true);
                    }
                    catch (Exception ex)
                    {
                        throw (ex);
                    }
                }
                ROW++;
                #region Total

                #endregion Total

                ROW++;

                //ROW++;

                #region paragrpath formats
                //Adds a new paragraph style named "MyStyle"
                IWParagraphStyle myStyle = document.AddParagraphStyle("MyStyle");
                //Sets the formatting of the style
                myStyle.CharacterFormat.FontSize = 8f;
                myStyle.CharacterFormat.TextColor = Color.Black;
                myStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

                #endregion paragrpath formats

                #region merging section

                IWParagraphStyle style = document.AddParagraphStyle("SubTotalStyle");
                style.CharacterFormat.Bold = true;
                style.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Left;
                //Adds new paragraph to the section

                #endregion merging section

                TextBodyPart textBodyPart = new TextBodyPart(document);
                textBodyPart.BodyItems.Add(wTable);
                document.Replace(replaceString, textBodyPart, true, true);

            }
        }

        public Image ResizeImageDoc(Image image, int new_height, int new_width)
        {
            Bitmap new_image = new Bitmap(new_width, new_height);
            Graphics g = Graphics.FromImage((Image)new_image);
            g.InterpolationMode = InterpolationMode.High;
            g.DrawImage(image, 0, 0, new_width, new_height);
            return new_image;
        }

    }

}
