using Library.Data.Sql;
using Syncfusion.DocIO.DLS;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Service.Extension.HumanResource.FinalSettlement
{
    public class cFinalSettlement
    {
        private readonly ISqlRepository _sqlRepository;
        public cFinalSettlement(ISqlRepository ISqlRepository)
        {
            _sqlRepository = ISqlRepository;
        }


        public System.Data.DataTable GetEmployeeBasicInformation(string SystemId, string plantId)
        {
            try
            {

                string sql = @"select m.MonthNo,m.YearNo
                           ,left( DateName( month , DateAdd( month , m.MonthNo , -1 )),3) [MonthName]
                            ,h.SalaryHead,c.EntryAmount StructureAmount,c.DisbusmentAmount EarnedAmount
                            --,att.TotalProcDate,att.TotalAbsent,att.TotalHoliDay,att.TotalLWP,att.TotalWeekOff
                            ,ActualWorkingDays=att.TotalProcDate-att.TotalAbsent-att.TotalHoliDay-att.TotalLWP-att.TotalWeekOff
                            ,b.BonusAmount,b.EffectiveDate
                            from SalaryProcChild c
                            inner join (select * from SalaryHead where HeadCategory in( 'Gross')) h on h.SalaryHeadID=c.SalaryHeadID
                            left join SalaryProcMaster m on m.SystemID=c.SlrProcMstSystemID
                            left join SalaryProceAttdnData att on att.SlrProcMstSystemID=m.SystemID and att.EmpSystemID='" + SystemId + @"'
                            --left join [BonusPaymentActualMaster] bm on month(bm.EffectiveDate)=m.MonthNo
                            left join (select c.*,m.EffectiveDate from BonusPaymentActual c
                            left join [BonusPaymentActualMaster] m on m.SystemID=c.BnsMstSystemID
                            )b on b.EmpSystemID='" + SystemId + @"' and month(b.EffectiveDate)=m.MonthNo
                            where c.SlrProcMstSystemID in (
                            select SystemID from SalaryProcMaster where (YearNo=2019 and MonthNo=8) or (YearNo=2019 and MonthNo=9) or (YearNo=2019 and MonthNo=10)
                            )
                            and c.EmpInfoSystemID='" + SystemId + @"'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetEmpInformationforfinalsettlement(string plantId, string SystemId, string LanguageId, string UserName)
        {
            try
            {

                string sql = @"select case when isnull(cg.Id,'')='' THEN isnull(E.EmployeeNameLocal,E.EmployeeName) ELSE EmployeeName END as EmployeeName,ISNULL(ll.Name, LDN.UserName)AS Designation , ISNULL(e.EmployeeCode,e.EmployeeCode) EmployeeCode,
        ISNULL(lls.Name,Se.UserName)AS Section,FORMAT(e.DOJ,'dd-MMM-yyyy')DOJ,FORMAT(e.DOS,'dd-MMM-yyyy')DOS,FORMAT(e.DOS,'MMMM-yyyy') DOSMonth,E.FatherName
                                        from EmployeeInformation e
        	LEFT JOIN  hkp.LegalDesignation LDN ON LDN.Id=E.LegalDesignationId
        								LEFT JOIN MST.ManpowerBudget PMB ON e.BudgetCode = PMB.Id
                                LEFT JOIN ORG.Position PR ON PMB.PositionId = PR.Id
                                LEFT JOIN ORG.Entity ET ON PMB.EntityId = ET.Id
        						LEFT JOIN ORG.Section AS Se ON Se.Id = PR.SectionID
                                        left join [HKP].[LocalLanguage] ll on ll.LegalDesignationId=e.LegalDesignationId and ll.LanguageId='" + LanguageId + @"'
                                        left join [HKP].[LocalLanguage] lls on lls.SectionId=e.SectionId and lls.LanguageId='" + LanguageId + @"'
                                        left join [ORG].[Plant] p on p.Id=e.PlantId
                                        LEFT JOIN org.CompanyGroup  CG on e.GroupID=cg.Id and CG.LanguageId='" + LanguageId + @"'
                                        where e.SystemId='" + SystemId + @"'and p.Id='" + plantId + @"' ";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable GetFinalSettlementData(string plantId, string SystemId, string LanguageId, string UserName,string fromDate,string toDate)
        {
            try
            {
                string round = "";
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT RoundDayINFinalSettlement FROM [dbo].[PlantWiseHRMSSetting] Where Plantid='" + plantId + "'", out DataSet dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    if (Convert.ToBoolean(dsMaster.Tables[0].Rows[0]["RoundDayINFinalSettlement"]))
                    {
                        round = "CONVERT(NUMERIC(10,0),efs.[LvEncashmentDayNo])";
                    }
                    else
                    {
                        round = "CONVERT(NUMERIC(10,2),efs.[LvEncashmentDayNo])";
                    }
                }


                //         string sql = @"Select efs.Id, FORMAT(efs.FinalSettlementDate,'dd-MMM-yyy') FinalSettlementDate
                //--,efs.SalaryRate 
                //                     ,SalRate = case when  efs.GratuityAmount = 0 then (efs.GrossAmount/26) else (efs.GrossAmount/30) End
                //                     ,LeaveEncash = CONVERT(NUMERIC(10,0),case when  efs.GratuityAmount = 0 then (efs.GrossAmount/26) else (efs.GrossAmount/30) End * "+ round + @")
                //,NetPayAmount = CONVERT(NUMERIC(10,0),(case when convert(int,ROUND(efs.SeparationTypeAmount,0)) = 0 then 0 else convert(numeric,ROUND(efs.SeparationTypeAmount,0)) end + 
                //(case when  efs.GratuityAmount = 0 then (efs.GrossAmount/26) else (efs.GrossAmount/30) End * " + round + @") +
                //convert(int,ROUND(efs.LastMonthNetPayAmount,0))))-convert(int,ROUND(efs.[TotalDeductionAmount],0))
                //,efs.OTRate
                //                     ,convert(int,ROUND(efs.[TotalDeductionAmount],0)) TotalDeductionAmount
                //                     ,convert(int,ROUND(efs.LvEncashmentAmount,0)) LvEncashmentAmount
                //,convert(int,ROUND(efs.EarningAmount,0)) EarningAmount
                //----,convert(int,ROUND(efs.DeductionAmount,0)) DeductionAmount
                //,convert(int,ROUND(efs.GratuityAmount,0)) GratuityAmount
                //,efs.[LastMonthAbsentDay]
                //                     ,efs.OTRate OTRateA
                //,convert(int,ROUND(efs.[TotalPayableAmount],0)) TotalPayableAmount
                //--,convert(int,ROUND(efs.[NetPayAmount],0)) NetPayAmount
                //,efs.[LastMonthOTHour]
                //,efs.[LastMonthOTAmount]
                //                     ---,efs.[StampAmount]
                //,efs.[LastMonthAbsenteeismAmount]
                //                     ," + round + @" LvEncashmentDayNo
                //,convert(int,ROUND(efs.[LastMonthProcDay],0)) LastMonthProcDay
                //,convert(int,ROUND(efs.[LastMonthGrossAmount],0)) LastMonthGrossAmount

                //,SY.UserName+'Day' AS RetirementDayT
                //,SY.UserName+'Rate' AS RetirementRateT
                //,SY.UserName+'Amount' AS RetirementAmountT

                //                     ,SY.UserName+'Day' AS ResignationDayT
                //,SY.UserName+'Rate' AS ResignationRateT
                //,SY.UserName+'Amount' AS ResignationAmountT

                //---,efs.PolicyDayNo
                //                     ,CONVERT(INT, ISNULL(efs.PolicyYearNo,0)*ISNULL(efs.PolicyDayNo,0)) PolicyDayNo
                //                     ,SY.UserName AS SeprationName
                //                     ,convert(int,ROUND(efs.TenureDayNo,0)) TenureDayNo
                //,case when convert(int,ROUND(efs.SeparationTypeAmount,0)) = 0 then 0 else convert(int,ROUND(efs.SeparationTypeAmount,0)) end  SeparationTypeAmount
                //,convert(int,ROUND(efs.GrossAmount,0)) GrossAmount
                //,convert(int,ROUND(efs.BasicAmount,0)) BasicAmount
                //,convert(int,efs.[TenureYearNo]) TenureYearNo
                //,convert(int,efs.[TenureMonthNo]) TenureMonthNo
                //,convert(int,efs.TenureDayNo) TenureDayNoA
                //,convert(int,ROUND(efs.LastMonthNetPayAmount,0)) LastMonthNetPayAmount
                //,efs.LvEncashmentRateAmount
                //                     ,efs.LvEncashmentRateAmount Dailywages
                //,SY.UserName AS SeparationType
                //,CONVERT(int,ISNULL(efs.PolicyYearNo,0)*ISNULL(efs.PolicyDayNo,0),0) SeparationTypeDay
                //,case when PolicyDayNo*PolicyYearNo = 0 then 'N/A' else convert(varchar(100),(SeparationTypeAmount/(PolicyDayNo*PolicyYearNo)),0) end SeparationTypeRate,format(apd.WorkDate,'dd-MMM-yyyy') LastPayDate,SPAD.TotalPayDay
                //                     --,isnull(efs.GratuityNoOfDaysOrYear,0) SeparationTypeDay

                //                     From [dbo].[EmployeeFinalSettlement] efs 
                //                  LEFT JOIN [HKP].[SeparationType] SY ON SY.Id=efs.SeparationTypeId
                //                     LEFT JOIN EmployeeInformation E ON E.SystemId=efs.EmpSystemID
                //                     Left join AttdnProcessData APD ON CONCAT(apd.WorkDate,'-',APD.EmpSystemId)=(select top 1 CONCAT(WorkDate,'-',EmpSystemId) from AttdnProcessData where EmpSystemID=  '" + SystemId + @"' and PayDayValue=1 and WorkDate <= E.DOS order by workdate desc ) 
                //                     LEFT OUTER JOIN ORG.Plant ep on ep.id=e.PlantId
                //                     left join (select top 1 * from SalaryProceAttdnData where empsystemId='" + SystemId + @"' order by FromDate Desc) SPAD on SPAD.EmpSystemID=efs.EmpSystemID
                //                     where efs.EmpSystemId='" + SystemId + @"'and ep.Id='" + plantId + @"'";

                string sql = @"select efs.Id, FORMAT(efs.FinalSettlementDate,'dd-MMM-yyy') FinalSettlementDate,efs.SalaryRate SalRate
,LeaveEncash = CONVERT(NUMERIC(10,0),case when  efs.GratuityAmount = 0 then (efs.GrossAmount/26) else (efs.GrossAmount/30) End * CONVERT(NUMERIC(10,2),efs.[LvEncashmentDayNo])),efs.NetPayAmount
,convert(int,ROUND(efs.[TotalDeductionAmount],0)) TotalDeductionAmount,efs.OTRate,convert(int,ROUND(efs.LvEncashmentAmount,0)) LvEncashmentAmount
,convert(int,ROUND(efs.EarningAmount,0)) EarningAmount,convert(int,ROUND(efs.GratuityAmount,0)) GratuityAmount,efs.[LastMonthAbsentDay]
,efs.OTRate OTRateA,convert(int,ROUND(efs.[TotalPayableAmount],0)) TotalPayableAmount,efs.[LastMonthOTHour],efs.[LastMonthOTAmount]
 ,efs.[LastMonthAbsenteeismAmount],CONVERT(NUMERIC(10,2),efs.[LvEncashmentDayNo]) LvEncashmentDayNo,convert(int,ROUND(efs.[LastMonthProcDay],0)) LastMonthProcDay
,convert(int,ROUND(efs.[LastMonthGrossAmount],0)) LastMonthGrossAmount,SY.UserName+'Day' AS RetirementDayT,SY.UserName+'Rate' AS RetirementRateT
,SY.UserName+'Amount' AS RetirementAmountT,SY.UserName+'Day' AS ResignationDayT,SY.UserName+'Rate' AS ResignationRateT,SY.UserName+'Amount' AS ResignationAmountT
,efs.PolicyDayNo,SY.UserName AS SeprationName,convert(int,ROUND(efs.TenureDayNo,0)) TenureDayNo,convert(int,ROUND(efs.SeparationTypeAmount,0))SeparationTypeAmount
,convert(int,ROUND(efs.GrossAmount,0)) GrossAmount,convert(int,ROUND(efs.BasicAmount,0)) BasicAmount,convert(int,efs.[TenureYearNo]) TenureYearNo
,convert(int,efs.[TenureMonthNo]) TenureMonthNo,convert(int,efs.TenureDayNo) TenureDayNoA,convert(int,ROUND(efs.LastMonthNetPayAmount,0)) LastMonthNetPayAmount
,efs.LvEncashmentRateAmount,efs.LvEncashmentRateAmount Dailywages,SeparationTypeDay=(CONVERT(int,ISNULL(efs.PolicyYearNo,0)*ISNULL(efs.PolicyDayNo,0),0))+S.PresentDays,SeparationTypeRate=efs.SalaryRate
,SY.UserName AS SeparationType,format(apd.WorkDate,'dd-MMM-yyyy') LastPayDate,SPAD.TotalPayDay
from dbo.EmployeeFinalSettlement efs
LEFT JOIN [HKP].[SeparationType] SY ON SY.Id=efs.SeparationTypeId
LEFT JOIN EmployeeInformation E ON E.SystemId=efs.EmpSystemID
Left join AttdnProcessData APD ON CONCAT(apd.WorkDate,'-',APD.EmpSystemId)=(select top 1 CONCAT(WorkDate,'-',EmpSystemId) from AttdnProcessData where EmpSystemID='" + SystemId + @"' and PayDayValue=1 and WorkDate <= E.DOS order by workdate desc ) 
left join (select top 1 * from SalaryProceAttdnData where empsystemId='" + SystemId + @"' order by FromDate Desc) SPAD on SPAD.EmpSystemID=efs.EmpSystemID
LEFT JOIN(SELECT A.EmpSystemID,PresentDays=
									CASE WHEN DATEDIFF(Year,E.DOJ,E.DOS)<9.9 THEN
									(CASE 
									WHEN COUNT(A.EmpSystemID) between 120 AND 240 THEN 7 
									WHEN COUNT(A.EmpSystemID)>240 THEN 14 
									ELSE 0 END) 
									ELSE 
									(CASE 
									WHEN COUNT(A.EmpSystemID) between 120 AND 240 THEN 15 
									WHEN COUNT(A.EmpSystemID)>240 THEN 30 
									ELSE 0 END) END
									from dbo.AttdnProcessData A
									LEFT JOIN dbo.EmployeeInformation E ON E.SystemId=A.EmpSystemID
									Where A.EmpSystemID='" + SystemId + @"' AND A.DayStatus !='A' 
									AND A.WorkDate between '" + fromDate + @"' AND '" + toDate + @"'
									GROUP BY A.EmpSystemID,E.DOJ,E.DOS)S ON S.EmpSystemID= efs.EmpSystemId
Where efs.EmpSystemId='" + SystemId + @"'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public DataTable GetFinalSettlementDeductionData(string EmployeeFinalSettlementId,string LanguageId)
        {
            try
            {

                string sqlx = @" select dh.ShortName+'Amount' AS DeductionAmount,fs.Amount from FinalSettlementDeductionDetails fs
                                        left join [dbo].[FinalSettlementDeductionHead] dh on dh.id=fs.FinalSettlementDeductionHeadId
                                        where fs.EmployeeFinalSettlementId='" + EmployeeFinalSettlementId + @"'";

                string sql = @" SELECT ROW_NUMBER() OVER(ORDER BY FinalSettlementHead ASC) AS RowNumber,* FROM (					
                                SELECT ISNULL(ll.Name, dh.UserName) AS FinalSettlementHead ,Amount=convert(int,ROUND(fs.Amount,0)),dh.Category 
                                FROM FinalSettlementDeductionDetails fs
                                left join [dbo].[FinalSettlementDeductionHead] dh on dh.id=fs.FinalSettlementDeductionHeadId
                                left join [HKP].[LocalLanguage] ll on ll.FinalSettlementHeadId=fs.Id and ll.LanguageId='" + LanguageId + @"'
                                WHERE  dh.Category='Deduction' AND fs.EmployeeFinalSettlementId='" + EmployeeFinalSettlementId + @"' 
                                UNION
                                SELECT FinalSettlementHead='Notice Period',Amount= convert(int,ROUND( NoticePeriodAmount,0)),'Deduction' Category---,NoticePeriodDayNo,NoticePeriodType,NoticePeriodRate 
                                FROM EmployeeFinalSettlement 
                                WHERE NoticePeriodType='Deduction' AND NoticePeriodAmount>0 AND Id='" + EmployeeFinalSettlementId + @"'
                                UNION
                                SELECT FinalSettlementHead='Earn Leave Deduction',Amount= convert(int,ROUND(EarnLvDeductionAmount,0)),'Deduction' Category--- EarnLvDeductionDayNo,EarnLvDeductionAmount 
                                FROM EmployeeFinalSettlement where EarnLvDeductionAmount>0 AND Id='" + EmployeeFinalSettlementId + @"'
                                ) x ";

                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public DataTable GetFinalSettlementEarningData(string EmployeeFinalSettlementId,string LanguageId)
        {
            try
            {


                string sql = @"SELECT ROW_NUMBER() OVER(ORDER BY FinalSettlementHead ASC) AS RowNumber,* FROM (
                                    SELECT   ISNULL(ll.Name, dh.UserName) AS FinalSettlementHead ,Amount= convert(int,ROUND(fs.Amount,0)),dh.Category 
                                        FROM FinalSettlementDeductionDetails fs
                                        left join [dbo].[FinalSettlementDeductionHead] dh on dh.id=fs.FinalSettlementDeductionHeadId
                                        left join [HKP].[LocalLanguage] ll on ll.FinalSettlementHeadId=fs.Id and ll.LanguageId='" + LanguageId + @"'
                                        where fs.EmployeeFinalSettlementId='" + EmployeeFinalSettlementId + @"' and dh.Category='Earning'
                                        UNION
                                       SELECT ISNULL(ll.Name, concat(SH.SalaryHead, ' ','(',case when R.YearStatus = 'PreviousYear' then 'Previous Year' else 'Current Year' end,')')) AS FinalSettlementHead,Amount= convert(int,ROUND(R.Amount,0)),'Earning' Category  from FinalSettlementRetainedDetails R
                                        left Join SalaryHead SH ON SH.SalaryHeadID= R.SalaryHeadId
                                        left join [HKP].[LocalLanguage] ll on ll.SalaryHeadId=SH.SalaryHeadID and ll.LanguageId='" + LanguageId + @"'
                                        WHERE EmployeeFinalSettlementId='" + EmployeeFinalSettlementId + @"'
                                        UNION
                                        SELECT FinalSettlementHead='Notice Period',Amount= convert(int,ROUND(NoticePeriodAmount,0)),'Earning' Category---,NoticePeriodDayNo,NoticePeriodType,NoticePeriodRate 
                                        FROM EmployeeFinalSettlement 
                                        WHERE NoticePeriodType='Earning' AND NoticePeriodAmount>0 AND Id='" + EmployeeFinalSettlementId + @"'


) x";

                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void GetFinalSettlementHeadWiseData(string replaceString, WordDocument document, DataTable dsFinalSettlementHeadWiseData, string lng)
        {
            //string replaceString = "{employeeTable}";





            int LasColumnIndex = 2;
            Dictionary<string, int> dicTaxes = new Dictionary<string, int>();





            WTable wTable = new WTable(document);
            wTable.TableFormat.Borders.LineWidth = 1;
            wTable.TableFormat.Borders.BorderType = BorderStyle.Single;
            wTable.TableFormat.IsAutoResized = true;
            //wTable.TableFormat.Paddings.All = 0;


            int ROW = 0; //int COL = 0;
            wTable.ResetCells(1, LasColumnIndex + 1);

            WTableRow TemplateRow = wTable.Rows[0].Clone();
            #region column headers
            document.EnsureMinimal();


            WCharacterFormat FontBold = new WCharacterFormat(document);
            FontBold.Bold = false;
            FontBold.FontSize = 9;



            //IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Earning.");
            //range.ApplyCharacterFormat(FontBold);
            //int colSlNo = COL; COL++;
            //wTable.Rows[ROW].Cells[colSlNo].Width = 150;

            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Amount");
            //range.ApplyCharacterFormat(FontBold);
            //int colAppraisalDate = COL; COL++;
            //wTable.Rows[ROW].Cells[colAppraisalDate].Width = 60;




            //range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Increment Amount");
            //range.ApplyCharacterFormat(FontBold);
            //int colIncrementAmount = COL; COL++;
            //wTable.Rows[ROW].Cells[colIncrementAmount].Width = 60;






            #endregion column headers
            //double totalValue = 0;
            int startRow = ROW;
            //int slno = 0;
            for (int i = 0; i < dsFinalSettlementHeadWiseData.Rows.Count; i++)
            {
                //slno++; 
                WTableRow TROW = wTable.LastRow;

                // WTableRow TROW = wTable.Rows[1].Clone();
                for (int CE = 0; CE < TROW.Cells.Count; CE++)
                {

                    TROW.Cells[CE].Width = wTable.Rows[0].Cells[CE].Width;
                }
                //TROW.Cells[colSLId].AddParagraph().AppendText(sl.ToString());
                //TROW.Cells[colSLId].Width = 30;
                TROW.Cells[0].CellFormat.Paddings.Left = 0;
                IWTextRange range0 = TROW.Cells[0].AddParagraph().AppendText(cnDgt(dsFinalSettlementHeadWiseData.Rows[i]["RowNumber"].ToString(), lng));
                TROW.Cells[0].Width = document.Sections[0].Tables[1].Rows[0].Cells[0].Width;
                range0.ApplyCharacterFormat(FontBold);

                IWTextRange range = TROW.Cells[1].AddParagraph().AppendText(dsFinalSettlementHeadWiseData.Rows[i]["FinalSettlementHead"].ToString());
                range.ApplyCharacterFormat(FontBold);


                TROW.Cells[1].Width = document.Sections[0].Tables[1].Rows[0].Cells[document.Sections[0].Tables[1].Rows[0].Cells.Count - 2].Width
                                      + document.Sections[0].Tables[1].Rows[0].Cells[document.Sections[0].Tables[1].Rows[0].Cells.Count - 3].Width
                                      + document.Sections[0].Tables[1].Rows[0].Cells[document.Sections[0].Tables[1].Rows[0].Cells.Count - 4].Width;

                IWTextRange range2 = TROW.Cells[2].AddParagraph().AppendText(cnDgt(Convert.ToDecimal(dsFinalSettlementHeadWiseData.Rows[i]["Amount"]).ToString("N0"), lng));
                TROW.Cells[2].Width = document.Sections[0].Tables[1].Rows[0].Cells[document.Sections[0].Tables[1].Rows[0].Cells.Count - 1].Width;
                range2.ApplyCharacterFormat(FontBold);

                foreach (WParagraph item in TROW.Cells[2].Paragraphs)
                {

                    item.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Right;
                }
                //TROW.Cells[2].CellFormat.h = "Left";

                //IWParagraph para = TROW.Cells[2].AddParagraph();//
                //para.ParagraphFormat.HorizontalAlignment = Syncfusion.DocIO.DLS.HorizontalAlignment.Center;

                //TROW.Cells[colPreviousGross].AddParagraph().AppendText(clsStdLib.dbl(dsFinalSettlementHeadWiseData.Rows[i]["PreviousGross"].ToString()).ToString("#,##0.00"));

                //IWTextRange range = wTable.Rows[ROW].Cells[COL].AddParagraph().AppendText("Earning.");
                //range.ApplyCharacterFormat(FontBold);

                if (i < dsFinalSettlementHeadWiseData.Rows.Count - 1)
                {
                    ROW++;
                    wTable.AddRow();
                }
            }
            //WSection section = document.Sections[0];
            TextBodyPart textBodyPart = new TextBodyPart(document);
            textBodyPart.BodyItems.Add(wTable);
            document.Replace(replaceString, textBodyPart, false, false);


        }


        public string cnDgt(string input, string lng)
        {
            if (lng == "Bengali")
            {
                return input.Replace('0', '০')
                    .Replace('1', '১')
                    .Replace('2', '২')
                    .Replace('3', '৩')
                    .Replace('4', '৪')
                    .Replace('5', '৫')
                    .Replace('6', '৬')
                    .Replace('7', '৭')
                    .Replace('8', '৮')
                    .Replace('9', '৯');
            }
            else if (lng == "Hindi")
            {
                return input.Replace('0', '०')
                    .Replace('1', '१')
                    .Replace('2', '२')
                    .Replace('3', '३')
                    .Replace('4', '४')
                    .Replace('5', '५')
                    .Replace('6', '६')
                    .Replace('7', '७')
                    .Replace('8', '८')
                    .Replace('9', '९');
            }
            else if (lng == "English")
            {
                return input.Replace('0', '0')
                    .Replace('1', '1')
                    .Replace('2', '2')
                    .Replace('3', '3')
                    .Replace('4', '4')
                    .Replace('5', '5')
                    .Replace('6', '6')
                    .Replace('7', '7')
                    .Replace('8', '8')
                    .Replace('9', '9');
            }
            return input;
        }

        public string GetFormatedDate(string date, string lng)
        {
            var formateDate = string.Empty;
            var day = cnDgt(date.Substring(0, 2), lng);
            var mon = ChangeMonth(date.Substring(3, 3), lng);
            var year = cnDgt(date.Substring(7, 4), lng);
            return formateDate = day + "-" + mon + "-" + year;
        }

        public string ChangeMonth(string input, string lng)
        {
            if (lng == "Bengali")
            {
                return input
                    .Replace("Jan", "জানুয়ারি")
                    .Replace("Feb", "ফেব্রুয়ারি")
                    .Replace("Mar", "মার্চ")
                    .Replace("Apr", "এপ্রিল")
                    .Replace("May", "মে")
                    .Replace("Jun", "জুন")
                    .Replace("Jul", "জুলাই")
                    .Replace("Aug", "আগস্ট")
                    .Replace("Sep", "সেপ্টেম্বর")
                    .Replace("Oct", "অক্টোবর")
                    .Replace("Nov", "নভেম্বর")
                    .Replace("Dec", "ডিসেম্বর");
            }
            else if (lng == "Hindi")
            {
                return input
                    .Replace("Jan", "जनवरी")
                    .Replace("Feb", "फरवरी")
                    .Replace("Mar", "मार्च")
                    .Replace("Apr", "अप्रैल")
                    .Replace("May", "मई")
                    .Replace("Jun", "जून")
                    .Replace("Jul", "जुलाई")
                    .Replace("Aug", "अगस्त")
                    .Replace("Sep", "सितम्बर")
                    .Replace("Oct", "अक्तूबर")
                    .Replace("Nov", "नवम्बर")
                    .Replace("Dec", "दिसम्बर");
            }
            return input;
        }





    }
}
