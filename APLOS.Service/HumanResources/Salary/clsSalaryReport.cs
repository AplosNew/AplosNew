using Library.Core;
using Library.Data.Sql;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using static Library.Service.Helpers.ReportUtility;

namespace OTSBD.clsSalary
{
    public class clsSalaryReport
    {
        #region  -----Salary Head Report

        public void xl_SalaryHead(string companyGroupId, string userName, HttpResponse response)
        {
            DataSet dsEmpInfo = null;

            try
            {
                Param param = new Param();
                param.CompanyGroupId = companyGroupId;
                param.SheetName = "SalaryHeadReport";
                param.SheetHeader = "Salary Head Report";
                param.UserName = userName;
                //get ds
                GetSalaryHead(out dsEmpInfo);
                DataView dvEmpInfo = new DataView(dsEmpInfo.Tables[0]);
                //dvEmpInfo.RowFilter = "ProbationPeriod>='" + DateTime.Now + "'";
                DataTable dtEmpInfo = dvEmpInfo.ToTable();
                //set ds
                GetSalaryHeadReport(param, dtEmpInfo, response);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void GetSalaryHeadReport(Param param, DataTable dtEmpInfo, HttpResponse response)
        {

            #region Variable

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility oRU = null;

            int xlsRow = 1, xlsCol = 1;


            #endregion Variable

            oRU = new ReportUtility();

            if (dtEmpInfo.Rows.Count > 0)
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];

                xlsRow = 5;
                int starXlsRow = xlsRow;
                int endXlsCol = 0;
                int cSalaryHeadID = 0;
                int cSalaryHead = 0;
                int cDescription = 0;
                int cHeadType = 0;
                int cHeadCategory = 0;
                int cExtDataUpload = 0;

                xlsRow++;
                xlsCol = 1;

                //oRU.SetHeaderText(ref sheet1, xlsRow - 1, xlsCol, "Employee info", ExcelHAlign.HAlignCenter);

                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SalaryHeadID"); cSalaryHeadID = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "SalaryHead", 30); cSalaryHead = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Description", 35); cDescription = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "HeadType"); cHeadType = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "HeadCategory"); cHeadCategory = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "ExtDataUpload"); cExtDataUpload = xlsCol; xlsCol++;

                //sheet1.Range[xlsRow - 1, cEntityCode, xlsRow - 1, xlsCol - 1].Merge();

                xlsCol--;
                endXlsCol = xlsCol;
                xlsRow++;

                for (int i = 0; i < dtEmpInfo.Rows.Count; i++)
                {
                    oRU.SetText(ref sheet1, xlsRow, cSalaryHeadID, dtEmpInfo.Rows[i]["SalaryHeadID"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cSalaryHead, dtEmpInfo.Rows[i]["SalaryHead"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cDescription, dtEmpInfo.Rows[i]["Description"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cHeadType, dtEmpInfo.Rows[i]["HeadType"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cHeadCategory, dtEmpInfo.Rows[i]["HeadCategory"].ToString());
                    oRU.SetText(ref sheet1, xlsRow, cExtDataUpload, dtEmpInfo.Rows[i]["ExtDataUpload"].ToString());

                    //if (Convert.ToBoolean(dtEmpInfo.Rows[i]["IsConfirmed"].ToString()) == false)
                    //{
                    //    //sheet1.Range[xlsRow, cLD].CellStyle.ColorIndex = ExcelKnownColors.Red;
                    //    //sheet1.Range[xlsRow, cDOC].CellStyle.Font.Color = ExcelKnownColors.Red;
                    //}
                    xlsRow++;
                }



                oRU.Header(ref sheet1, param, endXlsCol, param.SheetHeader, true);
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                oRU.PageSetup(ref sheet1, param.UserName, starXlsRow, ExcelPageOrientation.Landscape);
                sheet1.Name = param.SheetName;
                workbook.Version = ExcelVersion.Excel97to2003;
                string strFileName = param.SheetName + bplib.clsWebLib.DateData_DBToApp(DateTime.Now.Date, bplib.clsWebLib.STD_DATE_FORMAT).ToString("dd-MMM-yyyy") + ".xls";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, response, ExcelDownloadType.PromptDialog);

                workbook.Close();
                excelEngine.Dispose();
            }
        }
        public void GetSalaryHead(out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" SELECT TOP 1000 [SalaryHeadID]
                        ,[SalaryHead]
                        ,[Description]
                        ,[HeadType]
                        ,[HeadCategory]
                        ,[ExtDataUpload]     
                         FROM [dbo].[SalaryHead]
                         order by HeadType desc,SalaryHead";


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

        #endregion

        #region  ----- Salary Rule Report (Active / WITH condition)

        public void GetSalaryRule(out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" select m.SystemID
                            ,m.SalaryRuleName
                            ,h.SalaryHead
                            ,d.[SalaryHeadID]
                            ,d.[IsGNRNetPayEffect]
                            ,d.[IsGNRTagAndUnTag]
                            ,d.[IsOpen]
                            ,d.[IsFixed]
                            ,d.[IsNA]
                            ,d.[FixedValue]
                            ,d.[IsFormula]
                            ,d.[FormulaDes]
                            ,d.[FormulaDesID]
                            ,d.[IsFixedMonthDay]
                            ,d.[FixedMonthDayValue]
                            ,d.[IsMonthDay]
                            ,d.[IsMonthWorkDay]
                            ,d.[IsFixedDisbus]
                            ,d.[SequenceNo]
                            ,d.[IsDisbusted]
                            ,d.[DateAdded] dDateAdded
                            ,m.[DateAdded] mDateAdded 
                             from [dbo].[SalaryRuleMaster] m 
                            left outer join [dbo].[SalaryRuleGeneral] d  on m.SystemID=d.SalaryRuleMasterSystemID
                            left outer join SalaryHead h on h.SalaryHeadID=d.SalaryHeadID";


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
        }
        public void xl_SalaryRule(string companyGroupId, string userName, HttpResponse response)
        {
            DataSet dsEmpInfo = null;

            try
            {
                Param param = new Param();
                param.CompanyGroupId = companyGroupId;
                param.SheetName = "SalaryRuleReport";
                param.SheetHeader = "Salary Rule Report";
                param.UserName = userName;
                //get ds
                GetSalaryRule(out dsEmpInfo);

                GetSalaryRuleReport(param, dsEmpInfo.Tables[0], response);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public void GetSalaryRuleReport(Param param, DataTable dtEmpInfo, HttpResponse response)
        {

            #region Variable

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility oRU = null;

            int xlsRow = 1, xlsCol = 1;


            #endregion Variable

            oRU = new ReportUtility();

            if (dtEmpInfo.Rows.Count > 0)
            {
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];

                xlsRow = 5;
                int starXlsRow = xlsRow;
                int endXlsCol = 0;

                int srSystemID = 0;
                int srSalaryRuleName = 0;
                int srSalaryHead = 0;
                int srSalaryHeadID = 0;
                int srIsGNRNetPayEffect = 0;
                int srIsGNRTagAndUnTag = 0;
                int srIsOpen = 0;
                int srIsFixed = 0;
                int srIsNA = 0;
                int srFixedValue = 0;
                int srIsFormula = 0;
                int srFormulaDes = 0;
                int srFormulaDesID = 0;
                int srIsFixedMonthDay = 0;
                int srFixedMonthDayValue = 0;
                int srIsMonthDay = 0;
                int srIsMonthWorkDay = 0;
                int srIsFixedDisbus = 0;
                int srSequenceNo = 0;
                int srIsDisbusted = 0;

                xlsRow++;
                xlsCol = 1;

                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "System ID", 13, ExcelHAlign.HAlignCenter); srSystemID = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Salary Rule Name", 13, ExcelHAlign.HAlignCenter); srSalaryRuleName = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Salary Head", 20, ExcelHAlign.HAlignCenter); srSalaryHead = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Salary Head ID", ExcelHAlign.HAlignCenter); srSalaryHeadID = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Is GNR NetPay Effect", ExcelHAlign.HAlignCenter); srIsGNRNetPayEffect = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Is GNR Tag And UnTag", ExcelHAlign.HAlignCenter); srIsGNRTagAndUnTag = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Is Open", 6, ExcelHAlign.HAlignCenter); srIsOpen = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Is Fixed", 6, ExcelHAlign.HAlignCenter); srIsFixed = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Is NA", 6, ExcelHAlign.HAlignCenter); srIsNA = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Fixed Value", 12, ExcelHAlign.HAlignCenter); srFixedValue = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Is Formula", 8, ExcelHAlign.HAlignCenter); srIsFormula = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Formula Description", 20, ExcelHAlign.HAlignCenter); srFormulaDes = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Formula Des. ID", ExcelHAlign.HAlignCenter); srFormulaDesID = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Is Fixed Month Day", 6, ExcelHAlign.HAlignCenter); srIsFixedMonthDay = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Fixed Month Day Value", 8, ExcelHAlign.HAlignCenter); srFixedMonthDayValue = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Is Month Day", 6, ExcelHAlign.HAlignCenter); srIsMonthDay = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Is Month Work Day", 6, ExcelHAlign.HAlignCenter); srIsMonthWorkDay = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Is Fixed Disbus", 6, ExcelHAlign.HAlignCenter); srIsFixedDisbus = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Sequence No", 10, ExcelHAlign.HAlignCenter); srSequenceNo = xlsCol; xlsCol++;
                oRU.SetHeaderText(ref sheet1, xlsRow, xlsCol, "Is Disbusted", 10, ExcelHAlign.HAlignCenter); srIsDisbusted = xlsCol; xlsCol++;

                xlsCol--;
                endXlsCol = xlsCol;
                xlsRow++;


                DataView dvEmpInfo = new DataView(dtEmpInfo);
                dvEmpInfo.Sort = "mDateAdded";
                //dvEmpInfo.RowFilter = "ProbationPeriod>='" + DateTime.Now + "'";
                DataTable dtSR = dvEmpInfo.ToTable(true, "SystemID", "SalaryRuleName", "mDateAdded");

                for (int ir = 0; ir < dtSR.Rows.Count; ir++)
                {
                    string v = dtSR.Rows[ir]["SystemID"].ToString();
                    string _SalaryRuleName = dtSR.Rows[ir]["SalaryRuleName"].ToString();

                    oRU.SetText(ref sheet1, xlsRow, srSystemID, v);
                    oRU.SetText(ref sheet1, xlsRow, srSalaryRuleName, _SalaryRuleName);

                    DataView dv_SR_Merged = new DataView(dtEmpInfo);
                    dv_SR_Merged.RowFilter = "SystemID='" + v + "'";
                    dv_SR_Merged.Sort = "dDateAdded";
                    DataTable dt_SR_Merged = dv_SR_Merged.ToTable();



                    int xlsRow_start = xlsRow;
                    for (int i = 0; i < dt_SR_Merged.Rows.Count; i++)
                    {


                        oRU.SetText(ref sheet1, xlsRow, srSalaryHead, dt_SR_Merged.Rows[i]["SalaryHead"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, srSalaryHeadID, dt_SR_Merged.Rows[i]["SalaryHeadID"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, srIsGNRNetPayEffect, bplib.clsWebLib.GetBoolData(dt_SR_Merged.Rows[i]["IsGNRNetPayEffect"].ToString()) == true ? "Yes" : "");
                        oRU.SetText(ref sheet1, xlsRow, srIsGNRTagAndUnTag, bplib.clsWebLib.GetBoolData(dt_SR_Merged.Rows[i]["IsGNRTagAndUnTag"].ToString()) == true ? "Yes" : "");
                        oRU.SetText(ref sheet1, xlsRow, srIsOpen, bplib.clsWebLib.GetBoolData(dt_SR_Merged.Rows[i]["IsOpen"].ToString()) == true ? "Yes" : "");
                        oRU.SetText(ref sheet1, xlsRow, srIsFixed, bplib.clsWebLib.GetBoolData(dt_SR_Merged.Rows[i]["IsFixed"].ToString()) == true ? "Yes" : "");

                        oRU.SetText(ref sheet1, xlsRow, srIsNA, bplib.clsWebLib.GetBoolData(dt_SR_Merged.Rows[i]["IsNA"].ToString()) == true ? "Yes" : "");
                        oRU.SetText(ref sheet1, xlsRow, srFixedValue, dt_SR_Merged.Rows[i]["FixedValue"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, srIsFormula, bplib.clsWebLib.GetBoolData(dt_SR_Merged.Rows[i]["IsFormula"].ToString()) == true ? "Yes" : "");
                        oRU.SetText(ref sheet1, xlsRow, srFormulaDes, dt_SR_Merged.Rows[i]["FormulaDes"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, srFormulaDesID, dt_SR_Merged.Rows[i]["FormulaDesID"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, srIsFixedMonthDay, bplib.clsWebLib.GetBoolData(dt_SR_Merged.Rows[i]["IsFixedMonthDay"].ToString()) == true ? "Yes" : "");
                        oRU.SetText(ref sheet1, xlsRow, srFixedMonthDayValue, dt_SR_Merged.Rows[i]["FixedMonthDayValue"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, srIsMonthDay, bplib.clsWebLib.GetBoolData(dt_SR_Merged.Rows[i]["IsMonthDay"].ToString()) == true ? "Yes" : "");
                        oRU.SetText(ref sheet1, xlsRow, srIsMonthWorkDay, bplib.clsWebLib.GetBoolData(dt_SR_Merged.Rows[i]["IsMonthWorkDay"].ToString()) == true ? "Yes" : "");
                        oRU.SetText(ref sheet1, xlsRow, srIsFixedDisbus, bplib.clsWebLib.GetBoolData(dt_SR_Merged.Rows[i]["IsFixedDisbus"].ToString()) == true ? "Yes" : "");
                        oRU.SetText(ref sheet1, xlsRow, srSequenceNo, dt_SR_Merged.Rows[i]["SequenceNo"].ToString());
                        oRU.SetText(ref sheet1, xlsRow, srIsDisbusted, bplib.clsWebLib.GetBoolData(dt_SR_Merged.Rows[i]["IsDisbusted"].ToString()) == true ? "Yes" : "");

                        xlsRow++;
                    }//SH
                    sheet1.Range[xlsRow_start, srSystemID, xlsRow - 1, srSystemID].Merge();
                    sheet1.Range[xlsRow_start, srSalaryRuleName, xlsRow - 1, srSalaryRuleName].Merge();

                }//SR


                oRU.Header(ref sheet1, param, endXlsCol, param.SheetHeader, true);
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                oRU.PageSetup(ref sheet1, param.UserName, starXlsRow, ExcelPageOrientation.Landscape);
                sheet1.Name = param.SheetName;
                workbook.Version = ExcelVersion.Excel97to2003;
                string strFileName = param.SheetName + bplib.clsWebLib.DateData_DBToApp(DateTime.Now.Date, bplib.clsWebLib.STD_DATE_FORMAT).ToString("dd-MMM-yyyy") + ".xls";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, response, ExcelDownloadType.PromptDialog);

                workbook.Close();
                excelEngine.Dispose();
            }
        }


        #endregion


        public void SetVPF(DataTable dtSource, ref DataTable dtSelectedDestination)
        {
            try
            {
                if (dtSource.Rows.Count > 0)
                {
                    for (int i = 0; i < dtSource.Rows.Count; i++)
                    {
                        // "xHeadCategory", "xSalaryHeadID", "xSalaryHead", "xIsCTCComponent", "xIsGrossComponent", "xHeadType");
                        string _HeadCategory = dtSource.Rows[i]["xHeadCategory"].ToString();
                        string _SalaryHeadID = dtSource.Rows[i]["xSalaryHeadID"].ToString();
                        string _SalaryHead = dtSource.Rows[i]["xSalaryHead"].ToString();
                        string _IsCTCComponent = bplib.clsWebLib.GetBoolData(dtSource.Rows[i]["xIsCTCComponent"].ToString()).ToString();
                        string _IsGrossComponent = bplib.clsWebLib.GetBoolData(dtSource.Rows[i]["xIsGrossComponent"].ToString()).ToString();
                        string _HeadType = dtSource.Rows[i]["xHeadType"].ToString();
                        if (string.IsNullOrEmpty(_SalaryHeadID) == false)
                        {
                            //"SalaryHeadID", "SalaryHead", "HeadType", "Sequence", "HeadCategory", "IsCTCComponent", "IsGrossComponent"
                            DataRow dr = dtSelectedDestination.NewRow();
                            dr["HeadCategory"] = _HeadCategory;
                            dr["SalaryHeadID"] = _SalaryHeadID;
                            dr["SalaryHead"] = _SalaryHead;
                            dr["IsCTCComponent"] = _IsCTCComponent;
                            dr["IsGrossComponent"] = _IsGrossComponent;
                            dr["HeadType"] = _HeadType;
                            dr["Sequence"] = "99";
                            dtSelectedDestination.Rows.Add(dr);
                            break;
                        }//if not null
                    }//for                   
                }//if count
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void SetBonus(DataTable dtSource, ref DataTable dtSelectedDestination)
        {
            try
            {
                if (dtSource.Rows.Count > 0)
                {
                    for (int i = 0; i < dtSource.Rows.Count; i++)
                    {
                        // "xHeadCategory", "xSalaryHeadID", "xSalaryHead", "xIsCTCComponent", "xIsGrossComponent", "xHeadType");
                        string _HeadCategory = dtSource.Rows[i]["B_HeadCategory"].ToString();
                        string _SalaryHeadID = dtSource.Rows[i]["B_SalaryHeadID"].ToString();
                        string _SalaryHead = dtSource.Rows[i]["B_SalaryHead"].ToString();
                        string _IsCTCComponent = bplib.clsWebLib.GetBoolData(dtSource.Rows[i]["B_IsCTCComponent"].ToString()).ToString();
                        string _IsGrossComponent = bplib.clsWebLib.GetBoolData(dtSource.Rows[i]["B_IsGrossComponent"].ToString()).ToString();
                        string _HeadType = dtSource.Rows[i]["B_HeadType"].ToString();

                        if (string.IsNullOrEmpty(_SalaryHeadID) == false)
                        {
                            //"SalaryHeadID", "SalaryHead", "HeadType", "Sequence", "HeadCategory", "IsCTCComponent", "IsGrossComponent"
                            DataRow dr = dtSelectedDestination.NewRow();
                            dr["HeadCategory"] = _HeadCategory;
                            dr["SalaryHeadID"] = _SalaryHeadID;
                            dr["SalaryHead"] = _SalaryHead;
                            dr["IsCTCComponent"] = _IsCTCComponent;
                            dr["IsGrossComponent"] = _IsGrossComponent;
                            dr["HeadType"] = _HeadType;
                            dr["Sequence"] = "99";
                            dtSelectedDestination.Rows.Add(dr);
                            break;
                        }//if not null
                    }//for
                }//cout
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SetSheetBonus(DataTable dtSource, ref DataTable dtSelectedDestination)
        {
            try
            {
                if (dtSource.Rows.Count > 0)
                {
                    for (int i = 0; i < dtSource.Rows.Count; i++)
                    {
                        // "xHeadCategory", "xSalaryHeadID", "xSalaryHead", "xIsCTCComponent", "xIsGrossComponent", "xHeadType");
                        // string _HeadCategory = dtSource.Rows[i]["HeadCategory"].ToString();
                        string _SalaryHeadID = dtSource.Rows[i]["SalaryHeadID"].ToString();
                        string _SalaryHead = dtSource.Rows[i]["SalaryHead"].ToString();
                        string _IsCTCComponent = bplib.clsWebLib.GetBoolData(dtSource.Rows[i]["IsCTCComponent"].ToString()).ToString();
                        string _IsGrossComponent = bplib.clsWebLib.GetBoolData(dtSource.Rows[i]["IsGrossComponent"].ToString()).ToString();
                        string _HeadType = dtSource.Rows[i]["HeadType"].ToString();
                        string _Sequence = dtSource.Rows[i]["Sequence"].ToString();                       
                        
                        if (string.IsNullOrEmpty(_SalaryHeadID) == false)
                        {
                            DataView dv = new DataView(dtSelectedDestination);
                            dv.RowFilter = "SalaryHeadID='" + _SalaryHeadID + "'";
                            if (dv.Count == 0)
                            {
                                //"SalaryHeadID", "SalaryHead", "HeadType", "Sequence", "HeadCategory", "IsCTCComponent", "IsGrossComponent"
                                DataRow dr = dtSelectedDestination.NewRow();
                                //dr["HeadCategory"] = _HeadCategory;
                                dr["SalaryHeadID"] = _SalaryHeadID;
                                dr["SalaryHead"] = _SalaryHead;
                                dr["IsCTCComponent"] = _IsCTCComponent;
                                dr["IsGrossComponent"] = _IsGrossComponent;
                                dr["HeadType"] = _HeadType;
                                dr["Sequence"] = _Sequence;
                                dtSelectedDestination.Rows.Add(dr);
                            }//count
                            //break;
                        }//if not null
                    }//for
                }//cout
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Salary Structure 181204 
        /// </summary>
        /// <param name="shs"></param>
        /// <returns></returns>
        string GetDecimalFormat(SalaryHeadSequence shs)
        {
            try
            {
                var ob = new ReportUtility();
                if (shs.IsInt)
                {
                    return ob.NumberFormatInt();
                }
                else
                {
                    return ob.GetDynamicDecimalPlace(shs.DecimalNo);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void XlsSalaryInformationRpt(ParaSalaryReport _para)
        {
            #region Variable

            clsReport objRpt = null;

            DataSet dsSlrProc = null;
            DataView dvEmp = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;
            ReportUtility ru = null;

            //clsEmployeeLoad objEmpBasic = null;
            //clsStaticInfo obs = null;
            //DataSet dsLocal = null;
            //clsEntityDropdownlist obe = null;


            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;

            #endregion Variable

            try
            {
                ru = new ReportUtility();
                objRpt = new clsReport();

                #region Variable
                ParamList para = new ParamList();

                para.PlantId = _para.PlantId;
                para.EmployeeId = _para.EmployeeIds;
                para.FromDate = _para.MinWageEffectiveDate;
                para.PayGroup = _para.PayGroup.ToString().Trim();

                #endregion Variable

                //if (string.IsNullOrEmpty(_para.EmployeeIds) == true)
                //{
                //    Exception ex = new Exception("Employee is not Added...Click Button [A] !!!");
                //    throw (ex);
                //}

                #region DataSet

                List<SalaryStructureReport> listdsSlrProc = new List<SalaryStructureReport>();
                objRpt.GetEmpSalaryInformationRpt(para, out dsSlrProc);
                dvEmp = new DataView();
                dvEmp.Table = dsSlrProc.Tables[0];

                if (dsSlrProc.Tables[0].Rows.Count > 0)
                {
                    listdsSlrProc = dsSlrProc.Tables[0].ToList<SalaryStructureReport>();
                }
                DataTable dtEmployees = dvEmp.ToTable(true, "SystemID", "Department", "Designation", "GivenDesignation", "DOJ", "DOS","DOB", "Grade", "EmployeeName", "EmployeeCode", "SalaryHeadValue");
                if (dtEmployees.Rows.Count == 0)
                {
                    Exception ex = new Exception("No Data found...");
                    throw (ex);
                }

                objRpt.SelectedPlantWiseCompany(_para.PlantId,"",out dsCmp);

                objRpt.SelectedPlant(_para.PlantId, out dsFactory);

                #endregion DataSet

                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;

                workbook = application.Workbooks.Create(1);
                sheet1 = workbook.Worksheets[0];
                sheet1.IsGridLinesVisible = true;

                #region------------------Column Header------------------
                xlsRow = 5;
                xlsCol = 1;

                int ColSr = 0;
                int ColIDNo = 0;
                int ColName = 0;
                int ColDOJ = 0;
                int ColDOB = 0;
                int ColDOs = 0;
                int ColGrade = 0;
                //int cDept = 0;
                //int ColDG = 0;
                int ColGVDG = 0;
                //int ColBkNm = 0;
                //int ColBkAcNo = 0;
                int ColGrs = 0;
                int ColCTC = 0;

                //1
                ru.SetCellValue("Sr. No.", sheet1, xlsRow, ref xlsCol, out ColSr);
                ru.SetCellValue("ID No.", sheet1, xlsRow, ref xlsCol, out ColIDNo, 8);
                ru.SetCellValue("Name", sheet1, xlsRow, ref xlsCol, out ColName, 30);
                ru.SetCellValue("DOJ", sheet1, xlsRow, ref xlsCol, out ColDOJ, 12);
                ru.SetCellValue("DOB", sheet1, xlsRow, ref xlsCol, out ColDOB, 12);
                ru.SetCellValue("DOS", sheet1, xlsRow, ref xlsCol, out ColDOs, 12);
                ru.SetCellValue("Grade", sheet1, xlsRow, ref xlsCol, out ColGrade, 20);
                //SetCellValue("Department", sheet1, xlsRow, ref xlsCol, out cDept, 25);//vh

                //SetCellValue("Designation", sheet1, xlsRow, ref xlsCol, out ColDG, 20);//
                ru.SetCellValue("Given Designation", sheet1, xlsRow, ref xlsCol, out ColGVDG, 20);//vh
                //SetCellValue("Bank Name", sheet1, xlsRow + 1, ref xlsCol, out ColBkNm, 10);
                //SetCellValue("Bank Account No", sheet1, xlsRow + 1, ref xlsCol, out ColBkAcNo, 18);

                //SR to
                sheet1.Range[xlsRow, ColSr].Text = "Employee Information";
                sheet1.Range[xlsRow, ColSr, xlsRow, ColGVDG].Merge();
                //xlsCol += 1;
                ColGrs = ColGVDG;
                //SetCellValue("CTC", sheet1, xlsRow, ref xlsCol, out ColCTC, 9);
                //SetCellValue("Gross", sheet1, xlsRow, ref xlsCol, out ColGrs, 9);

                DataView dvSalaryHead = new DataView(dsSlrProc.Tables[0]);
                dvSalaryHead.Sort = "HeadType desc,Sequence";
                DataTable dtSalaryHead = dvSalaryHead.ToTable(true, "SalaryHeadID", "SalaryHead", "HeadType", "Sequence", "HeadCategory", "IsCTCComponent", "IsGrossComponent", "IntegerInDisb", "DecimalNo");

                //DataTable dtVPFHead = dvSalaryHead.ToTable(true, "xHeadCategory", "xSalaryHeadID", "xSalaryHead", "xIsCTCComponent", "xIsGrossComponent", "xHeadType", "IntegerInDisb", "DecimalNo");
                
                //clsSalary.clsSalaryReport sr = new clsSalary.clsSalaryReport();
                //sr.SetVPF(dtVPFHead, ref dtSalaryHead);
               

                int _count_earning_head = 0;
                int _count_earning_ctchead = 0;
                int _count_deducting_head = 0;
                int _total_head_count = 0;
                List<SalaryHeadSequence> list = null;
                CreateDynamicSHead(dtSalaryHead, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColGrs, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out list);
                //CreateDynamicSHead(ss, dtSalaryHead, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref ColLv, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out list);
                xlsCol--;
                //Header Col
                if (_count_earning_head > 0)
                {
                    sheet1.Range[xlsRow, ColGrs + 1].Text = "Earning";
                    sheet1.Range[xlsRow, ColGrs + 1, xlsRow, ColGrs + _count_earning_head + _count_earning_ctchead].Merge();
                }

                int ds = ColGrs + 1 + _count_earning_head + _count_earning_ctchead;

                if (_count_deducting_head > 0)
                {
                    sheet1.Range[xlsRow, ds].Text = "Deduction";
                    sheet1.Range[xlsRow, ds, xlsRow, ds + _count_deducting_head - 1].Merge();
                }
                int np = 0;
                if (list.Count > 0)
                {
                    xlsCol++;
                    np = ColGrs + list.Count;
                    sheet1.Range[xlsRow, np].Text = "Net Payable";
                    sheet1.Range[xlsRow, np].ColumnWidth = 10;
                    sheet1.Range[xlsRow, np, xlsRow + 1, np].Merge();
                }

                xlsCol++;
                int MinWage = ColGrs + list.Count + 1;
                sheet1.Range[xlsRow, MinWage].Text = "Minimum Wage";
                sheet1.Range[xlsRow, MinWage].ColumnWidth = 10;
                sheet1.Range[xlsRow, MinWage, xlsRow + 1, MinWage].Merge();

                sheet1.Range[xlsRow, 1, xlsRow + 1, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                sheet1.Range[xlsRow, 1, xlsRow + 1, xlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow + 1, xlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow + 1, xlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow + 1, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, 1, xlsRow + 1, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;

                endXlsCol = xlsCol;
                #endregion------------------Column Header------------------

                int RowIndex = xlsRow + 3;

                #region ******************Report Header******************
                xlsRow = 1;
                xlsCol = 1;
                Param param = new Param();
                param.CompanyGroupId = _para.GroupId;
                param.CompanyId = _para.CompanyId;
                ru.Header(ref sheet1, param, endXlsCol, "Employee Salary Information");

                #endregion ******************Report Header******************

                #region ----------------------Data-----------------------

                int SrNo = 0;
                string x = "";
                decimal ColGrsSlr = 0;
                decimal ColCTCSlr = 0;
                ReportUtility oRU = new ReportUtility();

                xlsRow = RowIndex;

                //dvSlrProc = new System.Data.DataView();
                //dvSlrProc.Table = dsSlrProc.Tables[0];
                xlsRow--;
                //Test();
                for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
                {
                    //xlsRow++;
                    #region EmpInfo
                    SrNo += 1;
                    x = dtEmployees.Rows[i]["SystemID"].ToString().Trim();
                    ColGrsSlr = 0;
                    //1
                    sheet1.Range[xlsRow, ColSr].Number = (SrNo);
                    sheet1.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //2
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCode"].ToString()) == false)
                        sheet1.Range[xlsRow, ColIDNo].Text = dtEmployees.Rows[i]["EmployeeCode"].ToString();
                    sheet1.Range[xlsRow, ColIDNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColIDNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //3
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeName"].ToString()) == false)
                        sheet1.Range[xlsRow, ColName].Text = dtEmployees.Rows[i]["EmployeeName"].ToString();
                    sheet1.Range[xlsRow, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //4
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOJ"].ToString()) == false)
                        sheet1.Range[xlsRow, ColDOJ].Text = dtEmployees.Rows[i]["DOJ"].ToString();
                    sheet1.Range[xlsRow, ColDOJ].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    //4.2
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOB"].ToString()) == false)
                        sheet1.Range[xlsRow, ColDOB].Text = dtEmployees.Rows[i]["DOB"].ToString();
                    sheet1.Range[xlsRow, ColDOB].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColDOB].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //4.2
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOS"].ToString()) == false)
                        sheet1.Range[xlsRow, ColDOs].Text = dtEmployees.Rows[i]["DOS"].ToString();
                    sheet1.Range[xlsRow, ColDOs].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColDOs].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //4.3
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Grade"].ToString()) == false)
                        sheet1.Range[xlsRow, ColGrade].Text = dtEmployees.Rows[i]["Grade"].ToString();
                    sheet1.Range[xlsRow, ColGrade].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColGrade].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    ////4.2
                    //if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Department"].ToString()) == false)
                    //    sheet1.Range[xlsRow, cDept].Text = dtEmployees.Rows[i]["Department"].ToString();
                    //sheet1.Range[xlsRow, cDept].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    //sheet1.Range[xlsRow, cDept].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //5
                    //5
                    //if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Designation"].ToString()) == false)
                    //    sheet1.Range[xlsRow, ColDG].Text = dtEmployees.Rows[i]["Designation"].ToString();
                    //sheet1.Range[xlsRow, ColDG].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    //sheet1.Range[xlsRow, ColDG].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["GivenDesignation"].ToString()) == false)
                        sheet1.Range[xlsRow, ColGVDG].Text = dtEmployees.Rows[i]["GivenDesignation"].ToString();
                    sheet1.Range[xlsRow, ColGVDG].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColGVDG].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    #endregion

                    int _total_head_count_body = 0;
                    if(i==500)
                    {

                    }
                    if(i==100)
                    {

                    }
                    for (int ci = 0; ci < list.Count; ci++)
                    {
                        //Parallel.For(0, list.Count, ci =>
                        //{

                        #region Head wise loop
                        var ob = list[ci];
                        if (ob.SalaryHead.Length > 0)
                        {
                            if (ob.SalaryHeadId.ToUpper() == "CTC" || ob.SalaryHeadId.ToUpper() == "GROSS")
                            {
                                var formula = ob.SalaryHead;
                                var hId = ob.SalaryHeadId;
                                _total_head_count_body++;

                                sheet1.Range[xlsRow, ob.XLColIndex].Formula = "=" + oRU.SetFormula(formula, xlsRow);
                                sheet1.Range[xlsRow, ob.XLColIndex].NumberFormat = GetDecimalFormat(ob);
                                sheet1.Range[xlsRow, ob.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                sheet1.Range[xlsRow, ob.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            }//ctc , gross
                            else
                            {
                                var hId = ob.SalaryHeadId;
                                _total_head_count_body++;

                                //VPF
                                if (ob.HeadCategory == bplib.clsWebLib.PFHEADCATEGORY)
                                {
                                    DataView dvBody = new DataView(dsSlrProc.Tables[0]);
                                    dvBody.RowFilter = "SystemId='" + x + "'";
                                    var _basic_col = list.Where(r => r.HeadCategory == "BASIC").Select(r => r.XLColIndex).FirstOrDefault();
                                    if (dvBody.Count > 0)
                                    {
                                        //var _basic_cell = GetColumnNameForXls(Convert.ToInt32(_basic_col)) + xlsRow;
                                        var _basic_cell = oRU.GetColumnNameForXls(Convert.ToInt32(_basic_col)) + xlsRow;
                                        //sheet1.Range[xlsRow, ob.XLColIndex].Number = Convert.ToDouble(bplib.clsWebLib.GetNumData(dvBody[0]["VoluntaryPFValue"].ToString()));
                                        sheet1.Range[xlsRow, ob.XLColIndex].Formula = "=(" + _basic_cell + "*" + Convert.ToDouble(bplib.clsWebLib.GetNumData(dvBody[0]["VoluntaryPFValue"].ToString())) + ")/100";
                                        sheet1.Range[xlsRow, ob.XLColIndex].NumberFormat = GetDecimalFormat(ob);
                                        sheet1.Range[xlsRow, ob.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                        sheet1.Range[xlsRow, ob.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    }
                                }

                                else//other all
                                {
                                    //DataView dvBody = new DataView(dsSlrProc.Tables[0]);
                                    //dvBody.RowFilter = "SalaryHeadID='" + hId + "' and SystemId='" + x + "'";

                                    var _data = listdsSlrProc.Where(r => r.SalaryHeadID == hId && r.EmpInfoSystemID == x).FirstOrDefault();

                                    if (_data != null)                                      
                                    {
                                        sheet1.Range[xlsRow, ob.XLColIndex].Number = Convert.ToDouble(bplib.clsWebLib.GetNumData(_data.EntryAmount.ToString()));

                                        sheet1.Range[xlsRow, ob.XLColIndex].NumberFormat = GetDecimalFormat(ob);// oRU.NumberFormatInt();
                                        sheet1.Range[xlsRow, ob.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                        sheet1.Range[xlsRow, ob.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    }//row found
                                }//VPF
                            }
                        }// 
                        #endregion
                        //});
                    }//for dtSalaryHead

                    //CTC-deduction //fro
                    var grossIndex = list.Where(r => r.SalaryHeadId == "Gross").Select(r => r.XLColIndex).FirstOrDefault();
                    var CTCIndex = list.Where(r => r.SalaryHeadId == "CTC").Select(r => r.XLColIndex).FirstOrDefault();

                    var dedIndex = list.Where(r => r.SalaryHeadId == "Deduction").Select(r => r.XLColIndex).FirstOrDefault();
                    var dedFormula = list.Where(r => r.SalaryHeadId == "Deduction").Select(r => r.SalaryHead).FirstOrDefault();

                    var grossAdd = oRU.SetFormula(grossIndex.ToString(), xlsRow);
                    var CTCAdd = oRU.SetFormula(CTCIndex.ToString(), xlsRow);
                    var dedAdd = oRU.SetFormula(dedFormula, xlsRow);

                    if (_para.IsCTCbased)
                    {
                        sheet1.Range[xlsRow, np].Formula = "=" + CTCAdd + "-(" + dedAdd + ")";
                    }
                    else
                    {
                        sheet1.Range[xlsRow, np].Formula = "=" + grossAdd + "-(" + dedAdd + ")";
                    }
                    sheet1.Range[xlsRow, np].NumberFormat = oRU.NumberFormatInt();
                    sheet1.Range[xlsRow, np].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, np].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    double _minWage = 0;
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["SalaryHeadValue"].ToString()) == false)
                    {
                        _minWage = Convert.ToDouble(bplib.clsWebLib.GetNumData(dtEmployees.Rows[i]["SalaryHeadValue"].ToString()));
                    }

                    sheet1.Range[xlsRow, MinWage].Number = _minWage;
                    sheet1.Range[xlsRow, MinWage].NumberFormat = oRU.NumberFormatInt();
                    sheet1.Range[xlsRow, MinWage].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, MinWage].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    //IConditionalFormats condition = sheet1.Range[xlsRow, MinWage].ConditionalFormats;
                    //IConditionalFormat condition1 = condition.AddCondition();

                    ////Represents conditional format rule that the value in target range should be between 10 and 20
                    //condition1.FormatType = ExcelCFType.CellValue;
                    //condition1.Operator = ExcelComparisonOperator.Greater;
                    ////condition1.FirstFormula = "=U7";
                    //string cf = "=" + oRU.GetColumnNameForXls(Convert.ToInt32(MinWage - 5))+ xlsRow;
                    //condition1.FirstFormula = cf;
                    //condition1.FontColor = ExcelKnownColors.Light_orange;

                    xlsRow++;
                }//for emp count

                #endregion ----------------------Data-----------------------

                #region Line Setup
                if (RowIndex >= (xlsRow - 1))
                {
                    xlsRow = RowIndex + 2;
                }

                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[RowIndex, 1, xlsRow - 1, xlsCol].WrapText = true;
                #endregion

                #region Freeze Panes
                sheet1.UsedRange["A7"].FreezePanes();
                sheet1.FirstVisibleColumn = 1;
                sheet1.FirstVisibleRow = 7;
                #endregion

                #region UsedRange Alignment
                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                #endregion UsedRange Alignment

                #region Page Setup
                sheet1.PageSetup.TopMargin = 0.5;
                sheet1.PageSetup.BottomMargin = 0.7;
                sheet1.PageSetup.PrintTitleRows = "$1:$5";
                sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + _para.UserId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:mm tt").ToString();
                sheet1.PageSetup.LeftMargin = 0.5;
                sheet1.PageSetup.RightMargin = 0.2;
                sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                sheet1.PageSetup.FitToPagesTall = 0;
                sheet1.PageSetup.FitToPagesWide = 1;
                sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;

                sheet1.Name = "EmpSalaryInfo";
                #endregion

                workbook.Version = ExcelVersion.Excel97to2003;
                string strFileName = "EmpSalaryInfo" + bplib.clsWebLib.DateData_DBToApp(DateTime.Now.Date, bplib.clsWebLib.STD_DATE_FORMAT).ToString("dd-MMM-yyyy") + ".xls";
                //string strFileName = "Vendor Master Data " + bplib.clsWebLib.DateData_DBToApp(System.DateTime.Now.Date, bplib.clsWebLib.STD_DATE_FORMAT).ToString("dd-MMM-yyyy") + "_" + System.DateTime.Now.Ticks.ToString() + ".xls";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, _para.Response, ExcelDownloadType.PromptDialog);

                workbook.Close();
                excelEngine.Dispose();

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
            }
        }
      

        void Test(IWorksheet sheet1,DataTable dtEmployees,int xlsRow,bool IsCTCbased, List<SalaryHeadSequence> list,DataSet dsSlrProc)
        {
            int ColSr = 0;
            int ColIDNo = 0;
            int ColName = 0;
            int ColDOJ = 0;
            int ColDOB = 0;
            int ColDOs = 0;
            int ColGrade = 0;
            int cDept = 0;
            int ColDG = 0;
            int ColGVDG = 0;
            int ColBkNm = 0;
            int ColBkAcNo = 0;
            int ColGrs = 0;
            int ColCTC = 0;
            int ColGrsSlr = 0;
            int MinWage = 0;
            int np = 0;
            try
            {
                ReportUtility oRU = new ReportUtility();
                int SrNo = 0;
                for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
                {
                    //xlsRow++;
                    #region EmpInfo
                    SrNo += 1;
                  var  x = dtEmployees.Rows[i]["SystemID"].ToString().Trim();
                    ColGrsSlr = 0;
                    //1
                    sheet1.Range[xlsRow, ColSr].Number = (SrNo);
                    sheet1.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //2
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeCode"].ToString()) == false)
                        sheet1.Range[xlsRow, ColIDNo].Text = dtEmployees.Rows[i]["EmployeeCode"].ToString();
                    sheet1.Range[xlsRow, ColIDNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColIDNo].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //3
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["EmployeeName"].ToString()) == false)
                        sheet1.Range[xlsRow, ColName].Text = dtEmployees.Rows[i]["EmployeeName"].ToString();
                    sheet1.Range[xlsRow, ColName].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColName].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //4
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOJ"].ToString()) == false)
                        sheet1.Range[xlsRow, ColDOJ].Text = dtEmployees.Rows[i]["DOJ"].ToString();
                    sheet1.Range[xlsRow, ColDOJ].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColDOJ].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    //4.2
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOB"].ToString()) == false)
                        sheet1.Range[xlsRow, ColDOB].Text = dtEmployees.Rows[i]["DOB"].ToString();
                    sheet1.Range[xlsRow, ColDOB].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColDOB].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //4.2
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["DOS"].ToString()) == false)
                        sheet1.Range[xlsRow, ColDOs].Text = dtEmployees.Rows[i]["DOS"].ToString();
                    sheet1.Range[xlsRow, ColDOs].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColDOs].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //4.3
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Grade"].ToString()) == false)
                        sheet1.Range[xlsRow, ColGrade].Text = dtEmployees.Rows[i]["Grade"].ToString();
                    sheet1.Range[xlsRow, ColGrade].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColGrade].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    ////4.2
                    //if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Department"].ToString()) == false)
                    //    sheet1.Range[xlsRow, cDept].Text = dtEmployees.Rows[i]["Department"].ToString();
                    //sheet1.Range[xlsRow, cDept].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    //sheet1.Range[xlsRow, cDept].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    //5
                    //5
                    //if (string.IsNullOrEmpty(dtEmployees.Rows[i]["Designation"].ToString()) == false)
                    //    sheet1.Range[xlsRow, ColDG].Text = dtEmployees.Rows[i]["Designation"].ToString();
                    //sheet1.Range[xlsRow, ColDG].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    //sheet1.Range[xlsRow, ColDG].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["GivenDesignation"].ToString()) == false)
                        sheet1.Range[xlsRow, ColGVDG].Text = dtEmployees.Rows[i]["GivenDesignation"].ToString();
                    sheet1.Range[xlsRow, ColGVDG].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                    sheet1.Range[xlsRow, ColGVDG].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    #endregion

                    int _total_head_count_body = 0;
                    if (i == 500)
                    {

                    }
                    if (i == 100)
                    {

                    }
                    for (int ci = 0; ci < list.Count; ci++)
                    {
                        //Parallel.For(0, list.Count, ci =>
                        //{

                        #region Head wise loop
                        var ob = list[ci];
                        if (ob.SalaryHead.Length > 0)
                        {
                            if (ob.SalaryHeadId.ToUpper() == "CTC" || ob.SalaryHeadId.ToUpper() == "GROSS")
                            {
                                var formula = ob.SalaryHead;
                                var hId = ob.SalaryHeadId;
                                _total_head_count_body++;

                                sheet1.Range[xlsRow, ob.XLColIndex].Formula = "=" + oRU.SetFormula(formula, xlsRow);
                                sheet1.Range[xlsRow, ob.XLColIndex].NumberFormat = GetDecimalFormat(ob);
                                sheet1.Range[xlsRow, ob.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                sheet1.Range[xlsRow, ob.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                            }//ctc , gross
                            else
                            {
                                var hId = ob.SalaryHeadId;
                                _total_head_count_body++;

                                //VPF
                                if (ob.HeadCategory == bplib.clsWebLib.PFHEADCATEGORY)
                                {
                                    DataView dvBody = new DataView(dsSlrProc.Tables[0]);
                                    dvBody.RowFilter = "SystemId='" + x + "'";
                                    var _basic_col = list.Where(r => r.HeadCategory == "BASIC").Select(r => r.XLColIndex).FirstOrDefault();
                                    if (dvBody.Count > 0)
                                    {
                                        //var _basic_cell = GetColumnNameForXls(Convert.ToInt32(_basic_col)) + xlsRow;
                                        var _basic_cell = oRU.GetColumnNameForXls(Convert.ToInt32(_basic_col)) + xlsRow;
                                        //sheet1.Range[xlsRow, ob.XLColIndex].Number = Convert.ToDouble(bplib.clsWebLib.GetNumData(dvBody[0]["VoluntaryPFValue"].ToString()));
                                        sheet1.Range[xlsRow, ob.XLColIndex].Formula = "=(" + _basic_cell + "*" + Convert.ToDouble(bplib.clsWebLib.GetNumData(dvBody[0]["VoluntaryPFValue"].ToString())) + ")/100";
                                        sheet1.Range[xlsRow, ob.XLColIndex].NumberFormat = GetDecimalFormat(ob);
                                        sheet1.Range[xlsRow, ob.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                        sheet1.Range[xlsRow, ob.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    }
                                }

                                else//other all
                                {
                                    DataView dvBody = new DataView(dsSlrProc.Tables[0]);
                                    dvBody.RowFilter = "SalaryHeadID='" + hId + "' and SystemId='" + x + "'";

                                    if (dvBody.Count > 0)
                                    {
                                        sheet1.Range[xlsRow, ob.XLColIndex].Number = Convert.ToDouble(bplib.clsWebLib.GetNumData(dvBody[0]["EntryAmount"].ToString()));
                                        //sheet1.Range[xlsRow , ob.XLColIndex].CellStyle.Interior.Color = System.Drawing.Color.LightGray;

                                        sheet1.Range[xlsRow, ob.XLColIndex].NumberFormat = GetDecimalFormat(ob);// oRU.NumberFormatInt();
                                        sheet1.Range[xlsRow, ob.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                        sheet1.Range[xlsRow, ob.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    }//row found
                                }//VPF
                            }
                        }// 
                        #endregion
                        //});
                    }//for dtSalaryHead

                    //CTC-deduction //fro
                    var grossIndex = list.Where(r => r.SalaryHeadId == "Gross").Select(r => r.XLColIndex).FirstOrDefault();
                    var CTCIndex = list.Where(r => r.SalaryHeadId == "CTC").Select(r => r.XLColIndex).FirstOrDefault();

                    var dedIndex = list.Where(r => r.SalaryHeadId == "Deduction").Select(r => r.XLColIndex).FirstOrDefault();
                    var dedFormula = list.Where(r => r.SalaryHeadId == "Deduction").Select(r => r.SalaryHead).FirstOrDefault();

                    var grossAdd = oRU.SetFormula(grossIndex.ToString(), xlsRow);
                    var CTCAdd = oRU.SetFormula(CTCIndex.ToString(), xlsRow);
                    var dedAdd = oRU.SetFormula(dedFormula, xlsRow);

                    if (IsCTCbased) //_para.IsCTCbased
                    {
                        sheet1.Range[xlsRow, np].Formula = "=" + CTCAdd + "-(" + dedAdd + ")";
                    }
                    else
                    {
                        sheet1.Range[xlsRow, np].Formula = "=" + grossAdd + "-(" + dedAdd + ")";
                    }
                    sheet1.Range[xlsRow, np].NumberFormat = oRU.NumberFormatInt();
                    sheet1.Range[xlsRow, np].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, np].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    double _minWage = 0;
                    if (string.IsNullOrEmpty(dtEmployees.Rows[i]["SalaryHeadValue"].ToString()) == false)
                    {
                        _minWage = Convert.ToDouble(bplib.clsWebLib.GetNumData(dtEmployees.Rows[i]["SalaryHeadValue"].ToString()));
                    }

                    sheet1.Range[xlsRow, MinWage].Number = _minWage;
                    sheet1.Range[xlsRow, MinWage].NumberFormat = oRU.NumberFormatInt();
                    sheet1.Range[xlsRow, MinWage].HorizontalAlignment = ExcelHAlign.HAlignRight;
                    sheet1.Range[xlsRow, MinWage].VerticalAlignment = ExcelVAlign.VAlignCenter;

                    //IConditionalFormats condition = sheet1.Range[xlsRow, MinWage].ConditionalFormats;
                    //IConditionalFormat condition1 = condition.AddCondition();

                    ////Represents conditional format rule that the value in target range should be between 10 and 20
                    //condition1.FormatType = ExcelCFType.CellValue;
                    //condition1.Operator = ExcelComparisonOperator.Greater;
                    ////condition1.FirstFormula = "=U7";
                    //string cf = "=" + oRU.GetColumnNameForXls(Convert.ToInt32(MinWage - 5))+ xlsRow;
                    //condition1.FirstFormula = cf;
                    //condition1.FontColor = ExcelKnownColors.Light_orange;

                    xlsRow++;
                }//loop
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        //private void CreateDynamicSHead(DataTable dtSalaryHead, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColGrs, out int _count_earning_head, out int _count_deducting_head, out int _count_earning_ctchead, out List<SalaryHeadSequence> list)
        public void CreateDynamicSHead(DataTable dtSalaryHead, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColGrs, out int _count_earning_head, out int _count_deducting_head, out int _count_earning_ctchead, out List<SalaryHeadSequence> list)
        {
            ReportUtility oru = null;
            try
            {
                oru = new ReportUtility();
                list = new List<SalaryHeadSequence>();
                _total_head_count = 0;
                _count_earning_head = 0;
                _count_deducting_head = 0;
                _count_earning_ctchead = 0;
                int countGross = 0;
                string grossFormula = "";
                string deductionFormula = "";
                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    #region loop gross e
                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
                    {
                        if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
                        {
                            if (bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IsGrossComponent"].ToString()))
                            {
                                if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E")
                                {
                                    #region E gross
                                    _total_head_count++;
                                    countGross++;

                                    oru.SetHeaderTextRotate(ref sheet1, xlsRow + 1, ColGrs + countGross, dtSalaryHead.Rows[ci]["SalaryHead"].ToString());

                                    if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
                                    {
                                        sheet1.Range[xlsRow + 1, ColGrs + countGross].CellStyle.Font.Color = ExcelKnownColors.Red;
                                    }
                                    xlsCol += 1;

                                    SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
                                    SetSalaryHead(salaryHeadSequence, dtSalaryHead.Rows[ci], ci, ColGrs + countGross);

                                    //salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
                                    //salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
                                    //salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                                    //salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
                                    //salaryHeadSequence.Sequence = ci;
                                    //salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper();
                                    //salaryHeadSequence.XLColIndex = ColGrs + countGross;
                                    if (grossFormula.Length == 0)
                                    {
                                        grossFormula += salaryHeadSequence.XLColIndex.ToString();
                                    }
                                    else
                                    {
                                        grossFormula += "," + salaryHeadSequence.XLColIndex.ToString();
                                    }
                                    list.Add(salaryHeadSequence);

                                    _count_earning_head++;
                                    #endregion
                                }
                            }//IsGrossComponent
                        }//CTC/Gross
                    }//SalaryHead 
                    #endregion
                }//for
                xlsCol += 1;

                string _gross = string.Empty;
                DataView dvGross = new DataView(dtSalaryHead);
                dvGross.RowFilter = "HeadCategory='GROSS'";
                if (dvGross.Count > 0)
                {
                    _gross = dvGross[0]["SalaryHead"].ToString();
                }
                countGross++;
                _count_earning_head++;
                sheet1.Range[xlsRow + 1, ColGrs + countGross].Text = _gross;
                sheet1.Range[xlsRow + 1, ColGrs + countGross].ColumnWidth = 12;

                SalaryHeadSequence salaryHSGross = new SalaryHeadSequence();

                salaryHSGross.SalaryHead = grossFormula;
                salaryHSGross.SalaryHeadId = "Gross";
                salaryHSGross.XLColIndex = ColGrs + countGross;
                list.Add(salaryHSGross);

                int countCTC = countGross;//======================================ctc

                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    #region loop ctc
                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
                    {
                        if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
                        {
                            if (bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IsCTCComponent"].ToString()) == true && bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IsGrossComponent"].ToString()) == false)
                            {
                                if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E")
                                {
                                    #region E ctc
                                    _total_head_count++;
                                    countCTC++;
                                    oru.SetHeaderTextRotate(ref sheet1, xlsRow + 1, ColGrs + countCTC, dtSalaryHead.Rows[ci]["SalaryHead"].ToString());

                                    if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
                                    {
                                        sheet1.Range[xlsRow + 1, ColGrs + countCTC].CellStyle.Font.Color = ExcelKnownColors.Red;
                                    }
                                    xlsCol += 1;

                                    SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
                                    SetSalaryHead(salaryHeadSequence, dtSalaryHead.Rows[ci], ci, ColGrs + countCTC);

                                    //salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
                                    //salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
                                    //salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                                    //salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
                                    //salaryHeadSequence.Sequence = ci;
                                    //salaryHeadSequence.XLColIndex = ColGrs + countCTC;

                                    //if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == bplib.clsWebLib.FESTIVAL_BONUS)//FESTIVAL_BONUS
                                    //{
                                    //    salaryHeadSequence.HeadCategory = bplib.clsWebLib.FESTIVAL_BONUS;
                                    //}

                                    if (grossFormula.Length == 0)
                                    {
                                        grossFormula += salaryHeadSequence.XLColIndex.ToString();
                                    }
                                    else
                                    {
                                        grossFormula += "," + salaryHeadSequence.XLColIndex.ToString();
                                    }
                                    list.Add(salaryHeadSequence);

                                    _count_earning_ctchead++;
                                    #endregion
                                }
                            }//IsCTCComponent
                        }//CTC/Gross
                    }//SalaryHead 
                    #endregion
                }//for
                xlsCol += 1;

                string _ctc = "CTC";
                //DataView dvCTC = new DataView(dtSalaryHead);
                //dvCTC.RowFilter = "HeadCategory='CTC'";
                //if (dvCTC.Count > 0)
                //{
                //    _ctc = dvCTC[0]["SalaryHead"].ToString();
                //}

                countCTC++;
                _count_earning_ctchead++;
                sheet1.Range[xlsRow + 1, ColGrs + countCTC].Text = _ctc;
                sheet1.Range[xlsRow + 1, ColGrs + countCTC].ColumnWidth = 14;

                SalaryHeadSequence salaryHSCTC = new SalaryHeadSequence();

                salaryHSCTC.SalaryHead = grossFormula;
                salaryHSCTC.SalaryHeadId = "CTC";
                salaryHSCTC.XLColIndex = ColGrs + countCTC;
                list.Add(salaryHSCTC);

                int countDeduction = countCTC;//========================================deduction

                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    #region deduction
                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
                    {
                        if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
                        {
                            if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "D")
                            {
                                #region D
                                _total_head_count++;
                                countDeduction++;

                                oru.SetHeaderTextRotate(ref sheet1, xlsRow + 1, ColGrs + countDeduction, dtSalaryHead.Rows[ci]["SalaryHead"].ToString());

                                if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
                                {
                                    sheet1.Range[xlsRow + 1, ColGrs + countDeduction].CellStyle.Font.Color = ExcelKnownColors.Red;
                                }
                                xlsCol += 1;

                                SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
                                SetSalaryHead(salaryHeadSequence, dtSalaryHead.Rows[ci], ci, ColGrs + countDeduction);

                                //salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
                                //salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
                                //salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                                //salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
                                //salaryHeadSequence.Sequence = ci;
                                //salaryHeadSequence.XLColIndex = ColGrs + countDeduction;
                                //if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == bplib.clsWebLib.PFHEADCATEGORY)//vpf
                                //{
                                //    salaryHeadSequence.HeadCategory = bplib.clsWebLib.PFHEADCATEGORY;
                                //}




                                if (deductionFormula.Length == 0)
                                {
                                    deductionFormula += salaryHeadSequence.XLColIndex.ToString();
                                }
                                else
                                {
                                    deductionFormula += "," + salaryHeadSequence.XLColIndex.ToString();
                                }

                                list.Add(salaryHeadSequence);

                                _count_deducting_head++;
                                #endregion
                            }
                        }//CTC/Gross
                    }//SalaryHead 
                    #endregion
                }//for
                SalaryHeadSequence salaryHSDed = new SalaryHeadSequence();

                salaryHSDed.SalaryHead = deductionFormula;
                salaryHSDed.SalaryHeadId = "Deduction";
                salaryHSDed.XLColIndex = ColGrs + countDeduction;
                list.Add(salaryHSDed);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }//eof

        private string GetLocalSalaryHead(DataSet dataSet, string salaryHead)
        {
            DataView dvBody = new DataView(dataSet.Tables[0]);
            dvBody.RowFilter = "SalaryHead='" + salaryHead + "'";
            if (dvBody.Count > 0)
            {
                if(!string.IsNullOrEmpty(dvBody[0]["Name"].ToString()))
                return dvBody[0]["Name"].ToString();
                else
                    return salaryHead;
            }
            else
            {
                return salaryHead;
            }
        }

        public void CreateDynamicSHead_For_AppointmentLetter(DataTable dtSalaryHead, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColGrs, out int _count_earning_head, out List<SalaryHeadSequence> list)
        {
            ReportUtility oru = null;
            try
            {
                DataSet dsSH, dsYearlyIncrementRate = null;
                oru = new ReportUtility();
                list = new List<SalaryHeadSequence>();
                _total_head_count = 0;
                _count_earning_head = 0;
                int countGross = 0;
                string grossFormula = "";
                var yearlyIncrementRate = string.Empty;
                var basic = "";

                clsSalaryReport objSR = new clsSalaryReport();

                objSR.GetSalaryHeadList(out dsSH);

                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    #region loop gross e
                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
                    {
                        if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
                        {
                            if (bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IsGrossComponent"].ToString()))
                            {
                                if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E")
                                {
                                    #region E gross
                                    _total_head_count++;
                                    countGross++;

                                    var salaryHead = GetLocalSalaryHead(dsSH, dtSalaryHead.Rows[ci]["SalaryHead"].ToString());

                                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString()=="Basic")
                                    {
                                        basic = salaryHead;
                                    }

                                    oru.SetText(ref sheet1, xlsRow + 1, ColGrs + countGross, salaryHead,ExcelHAlign.HAlignCenter);
                                    sheet1.Range[xlsRow + 1, ColGrs + countGross].CellStyle.Font.FontName = "SolaimanLipi";
                                    sheet1.Range[xlsRow + 1, ColGrs + countGross].CellStyle.Font.Size = 30;
                                    sheet1.Range[xlsRow + 1, ColGrs + countGross].CellStyle.Interior.Color = System.Drawing.Color.LightGray;
                                    if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
                                    {
                                        sheet1.Range[xlsRow + 1, ColGrs + countGross].CellStyle.Font.Color = ExcelKnownColors.Black;
                                    }
                                    xlsCol += 1;

                                    SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
                                    SetSalaryHead(salaryHeadSequence, dtSalaryHead.Rows[ci], ci, ColGrs + countGross);
                                    if (grossFormula.Length == 0)
                                    {
                                        grossFormula += salaryHeadSequence.XLColIndex.ToString();
                                    }
                                    else
                                    {
                                        grossFormula += "," + salaryHeadSequence.XLColIndex.ToString();
                                    }
                                    list.Add(salaryHeadSequence);

                                    _count_earning_head++;
                                    #endregion
                                }
                            }//IsGrossComponent
                        }//CTC/Gross
                    }//SalaryHead 
                    #endregion
                }//for  

                xlsCol += 1;

                string _gross = string.Empty;
                DataView dvGross = new DataView(dtSalaryHead);
                dvGross.RowFilter = "HeadCategory='GROSS'";
                if (dvGross.Count > 0)
                {
                    _gross = dvGross[0]["SalaryHead"].ToString();
                }
                countGross++;
                _count_earning_head++;
                oru.SetText(ref sheet1, xlsRow + 1, ColGrs + countGross, GetLocalSalaryHead(dsSH, _gross),ExcelHAlign.HAlignCenter);
                sheet1.Range[xlsRow + 1, ColGrs + countGross].CellStyle.Font.FontName = "SolaimanLipi";
                sheet1.Range[xlsRow + 1, ColGrs + countGross].CellStyle.Font.Size = 30;
                sheet1.Range[xlsRow + 1, ColGrs + countGross].CellStyle.Interior.Color = System.Drawing.Color.LightGray;

                SalaryHeadSequence salaryHSGross = new SalaryHeadSequence();

                salaryHSGross.SalaryHead = grossFormula;
                salaryHSGross.SalaryHeadId = "Gross";
                salaryHSGross.XLColIndex = ColGrs + countGross;
                list.Add(salaryHSGross);

                //ot rate

                oru.SetText(ref sheet1, xlsRow + 1, ColGrs + countGross + 1, GetLocalSalaryHead(dsSH, "OT"), ExcelHAlign.HAlignCenter);
                //sheet1.Range[xlsRow + 1, ColGrs + countGross + 1].ColumnWidth = 30;
                sheet1.Range[xlsRow + 1, ColGrs + countGross + 1].CellStyle.Font.FontName = "SolaimanLipi";
                sheet1.Range[xlsRow + 1, ColGrs + countGross + 1].CellStyle.Font.Size = 30;
                sheet1.Range[xlsRow + 1, ColGrs + countGross + 1].CellStyle.Interior.Color = System.Drawing.Color.LightGray;

                //Yearly Income
                objSR.GetYearlyIncrementRate(out dsYearlyIncrementRate);

                if (dsYearlyIncrementRate.Tables[0].Rows.Count > 0)
                {
                    yearlyIncrementRate = dsYearlyIncrementRate.Tables[0].Rows[0]["Name"].ToString();
                    if (string.IsNullOrEmpty(yearlyIncrementRate))
                        yearlyIncrementRate = dsYearlyIncrementRate.Tables[0].Rows[0]["LabelName"].ToString();
                }
                sheet1.Range[xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightGray;
                oru.SetText(ref sheet1, xlsRow + 1, ColGrs + countGross + 2, yearlyIncrementRate+"("+ basic + ")", ExcelHAlign.HAlignCenter);
                sheet1.Range[xlsRow + 1, ColGrs + countGross + 2].ColumnWidth = 60;
                sheet1.Range[xlsRow+1, ColGrs + countGross + 2].CellStyle.Font.FontName = "SolaimanLipi";
                sheet1.Range[xlsRow+1, ColGrs + countGross + 2].CellStyle.Font.Size = 30;
                sheet1.Range[xlsRow + 1, ColGrs + countGross + 2].CellStyle.Interior.Color = System.Drawing.Color.LightGray;
                sheet1.Range[xlsRow+1, ColGrs+ countGross + 2].CellStyle.ShrinkToFit = true;
                xlsCol += 1;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }//eof
        public void CreateDynamicSHead_For_Staff_AppointmentLetter(DataTable dtSalaryHead, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColGrs, out int _count_earning_head, out List<SalaryHeadSequence> list)
        {
            ReportUtility oru = null;
            try
            {
                DataSet dsSH, dsYearlyIncrementRate = null;
                oru = new ReportUtility();
                list = new List<SalaryHeadSequence>();
                _total_head_count = 0;
                _count_earning_head = 0;
                int countGross = 0;
                string grossFormula = "";
                var yearlyIncrementRate = string.Empty;
                var basic = "";

                clsSalaryReport objSR = new clsSalaryReport();

                objSR.GetSalaryHeadList(out dsSH);

                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    #region loop gross e
                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
                    {
                        if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
                        {
                            if (bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IsGrossComponent"].ToString()))
                            {
                                if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E")
                                {
                                    #region E gross
                                    _total_head_count++;
                                    countGross++;

                                    var salaryHead = GetLocalSalaryHead(dsSH, dtSalaryHead.Rows[ci]["SalaryHead"].ToString());
                                    sheet1.Range[xlsRow, ColGrs + countGross-1].CellStyle.Interior.Color = System.Drawing.Color.LightGray;
                                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString() == "Basic")
                                    {
                                        basic = salaryHead;
                                    }

                                    oru.SetText(ref sheet1, xlsRow , ColGrs + countGross-1, salaryHead, ExcelHAlign.HAlignCenter);
                                    sheet1.Range[xlsRow , ColGrs + countGross-1].CellStyle.Interior.Color = System.Drawing.Color.LightGray;
                                    sheet1.Range[xlsRow , ColGrs + countGross-1].CellStyle.Font.FontName = "SolaimanLipi";
                                    sheet1.Range[xlsRow , ColGrs + countGross-1].CellStyle.Font.Size = 30;
                                    sheet1.Range[xlsRow , ColGrs + countGross-1].ColumnWidth= 65;

                                    sheet1.Range[xlsRow , ColGrs + countGross-1].CellStyle.Interior.Color = System.Drawing.Color.LightGray;
                                    if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
                                    {
                                        sheet1.Range[xlsRow , ColGrs + countGross].CellStyle.Font.Color = ExcelKnownColors.Black;
                                    }
                                    xlsCol += 1;

                                    SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
                                    SetSalaryHead(salaryHeadSequence, dtSalaryHead.Rows[ci], ci, ColGrs + countGross);
                                    if (grossFormula.Length == 0)
                                    {
                                        grossFormula += salaryHeadSequence.XLColIndex.ToString();
                                    }
                                    else
                                    {
                                        grossFormula += "," + salaryHeadSequence.XLColIndex.ToString();
                                    }
                                    list.Add(salaryHeadSequence);

                                    _count_earning_head++;
                                    #endregion
                                }
                            }//IsGrossComponent
                        }//CTC/Gross
                    }//SalaryHead 
                    #endregion
                }//for  

                xlsCol += 1;

                string _gross = string.Empty;
                DataView dvGross = new DataView(dtSalaryHead);
                dvGross.RowFilter = "HeadCategory='GROSS'";
                if (dvGross.Count > 0)
                {
                    _gross = dvGross[0]["SalaryHead"].ToString();
                }
                countGross++;
                _count_earning_head++;
                oru.SetText(ref sheet1, xlsRow , ColGrs + countGross, GetLocalSalaryHead(dsSH, _gross), ExcelHAlign.HAlignCenter);
                sheet1.Range[xlsRow , ColGrs + countGross].CellStyle.Font.FontName = "SolaimanLipi";
                sheet1.Range[xlsRow , ColGrs + countGross].CellStyle.Font.Size = 30;
                sheet1.Range[xlsRow , ColGrs + countGross].CellStyle.Interior.Color = System.Drawing.Color.LightGray;
                sheet1.Range[xlsRow, ColGrs + countGross -1, xlsRow, ColGrs + countGross ].Merge();
                sheet1.Range[xlsRow+1, ColGrs + countGross -1, xlsRow+1, ColGrs + countGross ].Merge();
                SalaryHeadSequence salaryHSGross = new SalaryHeadSequence();

                salaryHSGross.SalaryHead = grossFormula;
                salaryHSGross.SalaryHeadId = "Gross";
                salaryHSGross.XLColIndex = ColGrs + countGross;
                list.Add(salaryHSGross);

               
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void CreateDynamicSHead_For__AppointmentLetter(DataTable dtSalaryHead, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColGrs, out int _count_earning_head, out List<SalaryHeadSequence> list)
        {
            ReportUtility oru = null;
            try
            {
                DataSet dsSH, dsYearlyIncrementRate = null;
                oru = new ReportUtility();
                list = new List<SalaryHeadSequence>();
                _total_head_count = 0;
                _count_earning_head = 0;
                int countGross = 0;
                string grossFormula = "";
                var yearlyIncrementRate = string.Empty;
                var basic = "";

                clsSalaryReport objSR = new clsSalaryReport();

                objSR.GetSalaryHeadList(out dsSH);

                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    #region loop gross e
                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
                    {
                        if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
                        {
                            if (bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IsGrossComponent"].ToString()))
                            {
                                if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E")
                                {
                                    #region E gross
                                    _total_head_count++;
                                    countGross++;

                                    //var salaryHead = GetLocalSalaryHead(dsSH, dtSalaryHead.Rows[ci]["SalaryHead"].ToString());
                                    sheet1.Range[xlsRow, ColGrs + countGross - 1].CellStyle.Interior.Color = System.Drawing.Color.LightGray;
                                    //if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString() == "Basic")
                                    //{
                                    //    basic = salaryHead;
                                    //}

                                    oru.SetText(ref sheet1, xlsRow, ColGrs + countGross - 1, dtSalaryHead.Rows[ci]["SalaryHead"].ToString(), ExcelHAlign.HAlignCenter);
                                    sheet1.Range[xlsRow, ColGrs + countGross - 1].CellStyle.Interior.Color = System.Drawing.Color.LightGray;
                                    sheet1.Range[xlsRow, ColGrs + countGross - 1].CellStyle.Font.FontName = "Arial";
                                    sheet1.Range[xlsRow, ColGrs + countGross - 1].CellStyle.Font.Bold = true;
                                    sheet1.Range[xlsRow, ColGrs + countGross - 1].CellStyle.Font.Size = 26;
                                    sheet1.Range[xlsRow, ColGrs + countGross - 1].ColumnWidth = 65;
                                   

                                    sheet1.Range[xlsRow, ColGrs + countGross - 1].CellStyle.Interior.Color = System.Drawing.Color.LightGray;
                                    if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
                                    {
                                        sheet1.Range[xlsRow, ColGrs + countGross].CellStyle.Font.Color = ExcelKnownColors.Black;
                                    }
                                    xlsCol += 1;

                                    SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
                                    SetSalaryHead(salaryHeadSequence, dtSalaryHead.Rows[ci], ci, ColGrs + countGross);
                                    if (grossFormula.Length == 0)
                                    {
                                        grossFormula += salaryHeadSequence.XLColIndex.ToString();
                                    }
                                    else
                                    {
                                        grossFormula += "," + salaryHeadSequence.XLColIndex.ToString();
                                    }
                                    list.Add(salaryHeadSequence);

                                    _count_earning_head++;
                                    #endregion
                                }
                            }//IsGrossComponent
                        }//CTC/Gross
                    }//SalaryHead 
                    #endregion
                }//for  

                sheet1.Range[xlsRow, ColGrs + countGross].Text = "Monthly Gross";
                sheet1.Range[xlsRow, ColGrs + countGross].CellStyle.Font.FontName = "Arial";
                sheet1.Range[xlsRow, ColGrs + countGross].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                sheet1.Range[xlsRow, ColGrs + countGross].CellStyle.Font.Size = 26;
                sheet1.Range[xlsRow, ColGrs + countGross].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, ColGrs + countGross].CellStyle.Interior.Color = System.Drawing.Color.LightGray;
                sheet1.Range[xlsRow, ColGrs + countGross , xlsRow, ColGrs + countGross+1].Merge();
                //sheet1.Range[xlsRow, ColGrs + countGross].WrapText = false;
                xlsCol += 1;

                string _gross = string.Empty;
                DataView dvGross = new DataView(dtSalaryHead);
                dvGross.RowFilter = "HeadCategory='GROSS'";
                if (dvGross.Count > 0)
                {
                    _gross = dvGross[0]["SalaryHead"].ToString();
                }
                countGross++;
                _count_earning_head++;
                oru.SetText(ref sheet1, xlsRow, ColGrs + countGross, GetLocalSalaryHead(dsSH, _gross), ExcelHAlign.HAlignCenter);
                sheet1.Range[xlsRow, ColGrs + countGross].CellStyle.Font.FontName = "Arial";
                sheet1.Range[xlsRow, ColGrs + countGross].CellStyle.Font.Size = 26;
                sheet1.Range[xlsRow, ColGrs + countGross].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, ColGrs + countGross].CellStyle.Interior.Color = System.Drawing.Color.LightGray;
                sheet1.Range[xlsRow, ColGrs + countGross - 1, xlsRow, ColGrs + countGross].Merge();
                sheet1.Range[xlsRow + 1, ColGrs + countGross - 1, xlsRow + 1, ColGrs + countGross].Merge();
                SalaryHeadSequence salaryHSGross = new SalaryHeadSequence();

                salaryHSGross.SalaryHead = grossFormula;
                salaryHSGross.SalaryHeadId = "Gross";
                salaryHSGross.XLColIndex = ColGrs + countGross;
                list.Add(salaryHSGross);


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void GetSalaryHeadList(out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                strSql = @"SELECT L.Name,L.SalaryHeadId,S.SalaryHead from HKP.LocalLanguage L
                           LEFT JOIN SalaryHead S on S.SalaryHeadID=L.SalaryHeadId
                           WHERE L.SalaryHeadId IS NOT NULL";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void GetGradeHead(out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                strSql = @"Select Name,LabelName from HKP.LocalLanguage  where LabelName='Grade'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
            }
            catch (Exception)
            {

                throw;
            }
        }

        public void GetYearlyIncrementRate(out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                strSql = @"Select Name,LabelName from HKP.LocalLanguage  where LabelName='YearlyIncrementRate'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void SetSalaryHead(SalaryHeadSequence salaryHeadSequence, DataRow Row, int seq, int ColIndex)
        {
            try
            {
                salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(Row["IntegerInDisb"].ToString());
                salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(Row["DecimalNo"].ToString()));
                salaryHeadSequence.SalaryHead = Row["SalaryHead"].ToString();
                salaryHeadSequence.SalaryHeadId = Row["SalaryHeadID"].ToString();
                salaryHeadSequence.Sequence = seq;
                salaryHeadSequence.XLColIndex = ColIndex;
                salaryHeadSequence.HeadCategory = Row["HeadCategory"].ToString().ToUpper();

                //if (Row["HeadCategory"].ToString().ToUpper() == bplib.clsWebLib.PFHEADCATEGORY)//vpf
                //{
                //    salaryHeadSequence.HeadCategory = bplib.clsWebLib.PFHEADCATEGORY;
                //}

                //if (Row["HeadCategory"].ToString().ToUpper() == bplib.clsWebLib.FESTIVAL_BONUS)//FESTIVAL_BONUS
                //{
                //    salaryHeadSequence.HeadCategory = bplib.clsWebLib.FESTIVAL_BONUS;
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        /// <summary>
        /// For getting Leave Information of Each Employees
        /// </summary>
        /// <param name="dvEmpLeaveInfo"></param>
        /// <param name="LeaveType"></param>
        /// <returns></returns>
        private decimal GetLWPEmp(DataView dvEmpLeaveInfo, string LeaveType)
        {
            var basicValue = 0.00m;
            try
            {

                var basic = from r in dvEmpLeaveInfo.ToTable().AsEnumerable()
                            where r.Field<string>("LeaveType") == LeaveType
                            select r;
                if (basic.Count() > 0)
                {

                    DataTable dtt = basic.CopyToDataTable();
                    basicValue = Convert.ToDecimal(dtt.Rows[0]["AvailedLeave"].ToString());


                }
                return basicValue;
            }
            catch (Exception)
            {

                throw;
            }
        }

        /// <summary>
        /// Salary Sheet 181205
        /// </summary>
        /// <param name="ss"></param>
        public void XlsSalarySheetRpt(eSalaryStructure ss, ParaSalaryReport _para)
        {
            #region Variable

            clsReport objRpt = null;

            DataSet dsSlrProc, dsBonus = null;
            DataView dvSlrProc = null;
            DataSet dsCmp = null;
            DataSet dsFactory = null;
            clsStaticInfo objs = null;
            ReportUtility oru = null;
            DataView dvLeaveEmp = null;
            DataSet dsLeaveInfo = null;

            ExcelEngine excelEngine = null;
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet1 = null;

            int xlsRow = 1, xlsCol = 1, endXlsCol = 1;
            string NumberFormatString = "#,##0;(#,##0)";
            //string USDNumberFormatString = "#,##0.00;(#,##0.00)";
            string FactoryName = "";
            string CmpName = "";

            #endregion Variable

            try
            {
                oru = new ReportUtility();
                objRpt = new clsReport();
                objs = new OTSBD.clsStaticInfo();
                if (string.IsNullOrEmpty(_para.SalaryProcessId) == true)
                {
                    Exception ex = new Exception("Please Select Salary Process ID...");
                    //System.Exception ex = new Exception("Please Add Employee ...(Click Button [A] )");
                    throw (ex);
                }

                //if (string.IsNullOrEmpty(this.txtEmployeeCode.Text.Trim()) == true)
                //{
                //    System.Exception ex = new Exception("Employee is not selected...");
                //    throw (ex);
                //}

                #region Variable
                var daysInMonth = 0;
                daysInMonth = DateTime.DaysInMonth(Convert.ToInt32(_para.sYear), Convert.ToInt32(_para.sMonth));//Number of Days in a month
                ParamList para = new ParamList();

                para.PlantId = _para.PlantId;
                para.EmployeeId = _para.EmployeeIds;
                para.FromDate = "01-" + bplib.clsWebLib.GetMonthName(_para.sMonth) + "-" + _para.sYear;
                para.ToDate = daysInMonth + "-" + bplib.clsWebLib.GetMonthName(_para.sMonth) + "-" + _para.sYear;
                para.SalaryProcessId = _para.SalaryProcessId;
                para.EmpStatus = _para.EmpStatus;
                para.PayGroup = _para.PayGroup;
                #endregion Variable

                string toDay = DateTime.Now.ToString("dd-MMM-yyyy");
                

                #region DataSet
                List<SalarySheetReport> listdsSlrProc = new List<SalarySheetReport>();
                objRpt.GetSalaryInfoSlrProcIDWise(para, out dsSlrProc);//Sql Query For Salary  Data
                                                                       //objRpt.GetSalaryInfoSlrProcIDWise(ddlSlrProcID.Text.Trim(), ddlPlant.SelectedValue.Trim(), lblEmpSystemID.Text, ddlStatus.SelectedValue.Trim(), out dsSlrProc);
                if (dsSlrProc.Tables[0].Rows.Count > 0)
                {
                    listdsSlrProc = dsSlrProc.Tables[0].ToList<SalarySheetReport>();
                }
                

                dvSlrProc = new DataView();
                dvSlrProc.Table = dsSlrProc.Tables[0];

                DataView dvEmp = new DataView();
                dvEmp.Table = dsSlrProc.Tables[0];
                DataTable dtEmployees = dvEmp.ToTable(true, "EmpInfoSystemID", "SectionName","SubSectionName", "Line", "LegalDesignation", "DepartmentName", "DivisionName", "UnitName", "EmpCategoryName"
                    , "DesignationName", "DesignationGroupName", "GivenDesignationName", "GivenDesignationGroup", "Grade", "DOJ", "DOS", "EmployeeName", "EmployeeCode", "LeaveDays",  "PresentDays", "AbsentDays", "HoliDay", "WeekOff", "TotalProcDate", "TotalLate", "TotalOTHr","LWP");
                if (dtEmployees.Rows.Count == 0)
                {
                    Exception ex = new Exception("No Data found...");
                    throw (ex);
                }
                //get

                objRpt.SelectedPlantWiseCompany(para.PlantId,"", out dsCmp);
                objRpt.SelectedPlant(para.PlantId, out dsFactory);

                ////////  Extra Absent
                DataSet dsExtraAbsent = null;
                DataView dvExtraAbsent = null;
                objRpt.GetExtraAbsent(_para.PlantId, Convert.ToInt32(_para.sMonth), Convert.ToInt32(_para.sYear), out dsExtraAbsent);
                dvExtraAbsent = new DataView(dsExtraAbsent.Tables[0]);

                #endregion DataSet

                if (dtEmployees.Rows.Count > 0)
                {
                    excelEngine = new ExcelEngine();
                    application = excelEngine.Excel;

                    workbook = application.Workbooks.Create(1);
                    sheet1 = workbook.Worksheets[0];
                    sheet1.IsGridLinesVisible = true;
                 
                     #region------------------Column Header------------------
                    xlsRow = 5;
                    xlsCol = 1;
                    var leavePara = new ParamList();
                    int ColSr = 0;
                    int ColIDNo = 0;
                    int ColName = 0;
                    int ColDOJ = 0;
                    int ColDOS = 0;
                    int ColDG = 0;
                    int ColDGG = 0;
                    int ColGVDG = 0;
                    int ColGVDGG = 0;

                    int ColStCt = 0;
                    int ColUnit = 0;
                    int ColDvN = 0;
                    int ColDpN = 0;
                    int ColGrd = 0;
                    int ColSec = 0;
                    int ColSecS = 0;
                    int colPayDays = 0;

                    int ColPdDy = xlsCol;
                    int ColAbDy = xlsCol;
                    int ColHlDy = xlsCol;
                    int ColWkOf = xlsCol;
                    int ColLv = xlsCol;
                    int colLate = xlsCol;
                    int colOthr = xlsCol;
                    int colLWP = xlsCol;
                    int colSEC = xlsCol;
                    int colSSEC = xlsCol;
                    int colLN = xlsCol;
                    int colLD = xlsCol;
                    int colXtraAbsnt = xlsCol;

                    leavePara.PlantId = _para.PlantId;
                    leavePara.FromDate = "01-" + bplib.clsWebLib.GetMonthName(_para.sMonth) + "-" + _para.sMonth;
                    leavePara.EmpStatus = _para.EmpStatus;
        

                    //1
                    //sheet1.Range[xlsRow + 1, xlsCol].Text = "Sr. No.";
                    //sheet1.Range[xlsRow + 1, xlsCol].ColumnWidth = 4;
                    //int ColSr = xlsCol;
                    //xlsCol += 1;
                    xlsRow += 1;
                    oru.SetCellValue("Sr. No.", sheet1, xlsRow, ref xlsCol, out ColSr, 4);
                    oru.SetHeadText("Emp Code", sheet1, xlsRow, ref xlsCol, out ColIDNo, 8);
                    oru.SetHeadText("Name ", sheet1, xlsRow, ref xlsCol, out ColName, 20);
                    //oru.SetHeadText("Name "  +Environment.NewLine + "(Given Designation)", sheet1, xlsRow, ref xlsCol, out ColName, 25);
                    oru.SetHeadText("DOJ ", sheet1, xlsRow, ref xlsCol, out ColDOJ, 11);
                    //oru.SetHeadText("DOJ " + Environment.NewLine + "(DOB)", sheet1, xlsRow, ref xlsCol, out ColDOJ, 11);
                    oru.SetHeadText("DOS", sheet1, xlsRow, ref xlsCol, out ColDOS, 10.5);
                    //oru.SetHeadText("Designation", sheet1, xlsRow, ref xlsCol, out ColDG);
                    oru.SetHeadText("Given Designation", sheet1, xlsRow, ref xlsCol, out ColGVDG, 15);
                    // oru.SetHeadText("Designation Group", sheet1, xlsRow, ref xlsCol, out ColDGG);
                    //oru.SetHeadText("Given Designation Group", sheet1, xlsRow, ref xlsCol, out ColGVDGG);

                    oru.SetHeadText("Staff Category", sheet1, xlsRow, ref xlsCol, out ColStCt);
                    //oru.SetHeadText("Unit", sheet1, xlsRow, ref xlsCol, out ColUnit);
                    //oru.SetHeadText("Division", sheet1, xlsRow, ref xlsCol, out ColDvN);
                    oru.SetHeadText("Department", sheet1, xlsRow, ref xlsCol, out ColDpN, 20);
                    oru.SetHeadText("Section", sheet1, xlsRow, ref xlsCol, out colSEC, 20);
                    oru.SetHeadText("SubSection", sheet1, xlsRow, ref xlsCol, out colSSEC, 20);
                    oru.SetHeadText("Line", sheet1, xlsRow, ref xlsCol, out colLN, 20);
                    oru.SetHeadText("LegalDesignation", sheet1, xlsRow, ref xlsCol, out colLD, 20);
                    oru.SetHeadText("Grade", sheet1, xlsRow, ref xlsCol, out ColGrd, 15);
                    // oru.SetHeadText("Section", sheet1, xlsRow, ref xlsCol, out ColSec);
                    //oru.SetHeadText("Sub Section", sheet1, xlsRow, ref xlsCol, out ColSecS);
                    
                    oru.SetHeadText("Pay Days", sheet1, xlsRow, ref xlsCol, out colPayDays, 6);
                    oru.SetHeadText("Present", sheet1, xlsRow, ref xlsCol, out ColPdDy, 6);
                    oru.SetHeadText("Absent", sheet1, xlsRow, ref xlsCol, out ColAbDy, 6);
                    oru.SetHeadText("LWP", sheet1, xlsRow, ref xlsCol, out colLWP, 6);
                    oru.SetHeadText("ExtraAbsent", sheet1, xlsRow, ref xlsCol, out colXtraAbsnt, 6);
                    oru.SetHeadText("Leave", sheet1, xlsRow, ref xlsCol, out ColLv, 6);
                    oru.SetHeadText("Total Late", sheet1, xlsRow, ref xlsCol, out colLate, 6);
                    oru.SetHeadText("HoliDay", sheet1, xlsRow, ref xlsCol, out ColHlDy, 6);
                    oru.SetHeadText("WorkOff", sheet1, xlsRow, ref xlsCol, out ColWkOf, 6);
                    oru.SetHeadText("Total OT. Hr.", sheet1, xlsRow, ref xlsCol, out colOthr, 6);
               
                   




                    //Header Col
                    sheet1.Range[xlsRow - 1, ColSr].Text = "Employee Information";
                    sheet1.Range[xlsRow - 1, ColSr, xlsRow - 1, colOthr].Merge();
                    //xlsCol += 1;
                    //6

                    //-------------------------
                    DataView dvSalaryHead = new DataView(dsSlrProc.Tables[0]);
                    dvSalaryHead.Sort = "HeadType desc,Sequence";
                    DataTable dtSalaryHead = dvSalaryHead.ToTable(true, "SalaryHeadID", "SalaryHead", "HeadType", "Sequence", "HeadCategory", "IsCTCComponent", "IsGrossComponent", "IntegerInDisb", "DecimalNo");
                    //DataTable dtSalaryHead = dvSalaryHead.ToTable(true, "SalaryHeadID", "SalaryHead", "HeadType", "Sequence", "HeadCategory", "IsNetPayEffect", "IsCTCComponent", "IsGrossComponent");

                    #region VPF n Bonus

                    // DataTable dtVPFHead = dvSalaryHead.ToTable(true, "xHeadCategory", "xSalaryHeadID", "xSalaryHead", "xIsCTCComponent", "xIsGrossComponent", "xHeadType");
                    objRpt.GetBonus(_para.sMonth, _para.sYear, out dsBonus);
                    DataView dvBonus = new DataView(dsBonus.Tables[0]);
                    DataTable dtBonusHead = dvBonus.ToTable(true, "SalaryHeadID", "HeadCategory", "SalaryHead", "IsCTCComponent", "IsGrossComponent", "HeadType", "Sequence");

                    clsSalary.clsSalaryReport sr = new clsSalary.clsSalaryReport();
                    //sr.SetVPF(dtVPFHead, ref dtSalaryHead);

                    sr.SetSheetBonus(dtBonusHead, ref dtSalaryHead);
                    #endregion

                    int _count_earning_head = 0;
                    int _count_deducting_head = 0;
                    int _total_head_count = 0;
                    int _count_earning_ctchead = 0;
                    List<SalaryHeadSequence> list = null;
                    CreateDynamicSHead(ss, dtSalaryHead, out _total_head_count, ref sheet1, ref xlsRow, ref xlsCol, ref colOthr, out _count_earning_head, out _count_deducting_head, out _count_earning_ctchead, out list);

                    // xlsCol--;
                    //Header Col
                    if (_count_earning_head > 0)
                    {
                        sheet1.Range[xlsRow - 1, colOthr + 1].Text = "Earning";
                        sheet1.Range[xlsRow - 1, colOthr + 1, xlsRow - 1, colOthr + _count_earning_head + _count_earning_ctchead].Merge();
                    }

                    int ds = colOthr + _count_earning_head + _count_earning_ctchead;

                    if (_count_deducting_head > 0)
                    {
                        sheet1.Range[xlsRow - 1, ds + 1].Text = "Deduction";
                        sheet1.Range[xlsRow - 1, ds + 1, xlsRow - 1, ds + _count_deducting_head].Merge();
                    }
                    int np = 0;
                    if (list.Count > 0)
                    {
                        xlsCol++;
                        np = colOthr + list.Count;
                        sheet1.Range[xlsRow, np].Text = "Net Payable";
                        sheet1.Range[xlsRow, np].ColumnWidth = 10;
                        sheet1.Range[xlsRow, np, xlsRow, np].Merge();
                    }
                    //xlsCol++;
                    sheet1.Range[xlsRow, xlsCol].Text = "Signature";
                    sheet1.Range[xlsRow, xlsCol].ColumnWidth = 17;
                    int ColSigna = xlsCol;
                    sheet1.Range[xlsRow, ColSigna, xlsRow, ColSigna].Merge();

                    sheet1.Range[xlsRow - 1, 1, xlsRow, xlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;
                    sheet1.Range[xlsRow - 1, 1, xlsRow, xlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow - 1, 1, xlsRow, xlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[xlsRow - 1, 1, xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow - 1, 1, xlsRow, xlsCol].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow - 1, 1, xlsRow, xlsCol].VerticalAlignment = ExcelVAlign.VAlignCenter;
                    sheet1.Range[xlsRow - 1, 1, xlsRow, xlsCol].WrapText = true;
                    endXlsCol = xlsCol;
                    #endregion------------------Column Header------------------

                    int RowIndex = xlsRow + 3;

                    #region ******************Report Header******************
                    xlsRow = 1;
                    xlsCol = 1;
                    string FactoryAddress = string.Empty;

                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        //CmpName = dsCmp.Tables[0].Rows[0]["UserName"].ToString();
                        CmpName = dsCmp.Tables[0].Rows[0]["CompanyName"].ToString();
                    }
                    else
                    {
                        CmpName = "";
                    }
                    sheet1.Range[xlsRow, xlsCol].Text = CmpName;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 14;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 30;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    if (dsCmp.Tables[0].Rows.Count > 0)
                    {
                        //FactoryName = dsFactory.Tables[0].Rows[0]["UserName"].ToString();
                        FactoryName = dsCmp.Tables[0].Rows[0]["PlantName"].ToString();
                    }
                    else
                    {
                        FactoryName = "";
                    }
                    sheet1.Range[xlsRow, xlsCol].Text = FactoryName;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    sheet1.Range[xlsRow, xlsCol].Text = "Salary Sheet";
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;

                    xlsRow += 1;
                    string strRptDateRange = "";
                    strRptDateRange = "For The Month Of " + _para.SalaryProcessId.Substring(4, 3) + ", " + _para.SalaryProcessId.Substring(0, 4);
                    sheet1.Range[xlsRow, xlsCol].Text = strRptDateRange;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].Merge();
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Bold = true;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 20;
                    sheet1.Range[xlsRow, xlsCol].CellStyle.Font.Size = 10;
                    sheet1.Range[xlsRow, 1].HorizontalAlignment = ExcelHAlign.HAlignCenter;
                    sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.Snow;
                    #endregion ******************Report Header******************

                    #region ----------------------Data-----------------------

                    int SrNo = 0;
                    string x = "";
                    decimal ColGrsSlr = 0;
                    decimal ColCTCSlr = 0;
                    ReportUtility oRU = new ReportUtility();

                    xlsRow = RowIndex;

                    //dvSlrProc = new System.Data.DataView();
                    //dvSlrProc.Table = dsSlrProc.Tables[0];
                    xlsRow--;
                    xlsRow--;
                    for (int i = 0; i <= dtEmployees.Rows.Count - 1; i++)
                    {

                        leavePara.EmployeeId = dtEmployees.Rows[i]["EmpInfoSystemID"].ToString();
                        objRpt.GetEmpLeaveInfo(leavePara, out dsLeaveInfo);
                        dvLeaveEmp = new DataView();

                        dvLeaveEmp.Table = dsLeaveInfo.Tables[0];

                        var LWP = GetLWPEmp(dvLeaveEmp, "Leave Without Pay");
                        //xlsRow++;
                        #region empinfo col Data

                        sheet1.Range[xlsRow, ColSr].Number = (1 + SrNo);
                        sheet1.Range[xlsRow, ColSr].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                        sheet1.Range[xlsRow, ColSr].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        oru.SetText(ref sheet1, xlsRow, ColIDNo, dtEmployees.Rows[i]["EmployeeCode"].ToString());
                        oru.SetText(ref sheet1, xlsRow, ColName, dtEmployees.Rows[i]["EmployeeName"].ToString());
                        //oru.SetText(ref sheet1, xlsRow, ColName, dtEmployees.Rows[i]["EmployeeName"].ToString() + Environment.NewLine + Environment.NewLine + "("+ dtEmployees.Rows[i]["GivenDesignationName"].ToString()+")");
                        oru.SetText(ref sheet1, xlsRow, ColDOJ, dtEmployees.Rows[i]["DOJ"].ToString());
                        //oru.SetText(ref sheet1, xlsRow, ColDOJ, dtEmployees.Rows[i]["DOJ"].ToString() + Environment.NewLine + Environment.NewLine + "("+ dtEmployees.Rows[i]["DOB"].ToString()+")");
                        oru.SetText(ref sheet1, xlsRow, ColDOS, dtEmployees.Rows[i]["DOS"].ToString());
                        //oru.SetText(ref sheet1, xlsRow, ColDG, dtEmployees.Rows[i]["DesignationName"].ToString());
                        oru.SetText(ref sheet1, xlsRow, ColGVDG, dtEmployees.Rows[i]["GivenDesignationName"].ToString());

                        //oru.SetText(ref sheet1, xlsRow, ColDGG, dtEmployees.Rows[i]["DesignationGroupName"].ToString());
                        //oru.SetText(ref sheet1, xlsRow, ColGVDGG, dtEmployees.Rows[i]["GivenDesignationGroup"].ToString());

                        oru.SetText(ref sheet1, xlsRow, ColStCt, dtEmployees.Rows[i]["EmpCategoryName"].ToString());
                        //oru.SetText(ref sheet1, xlsRow, ColUnit, dtEmployees.Rows[i]["UnitName"].ToString());
                        //oru.SetText(ref sheet1, xlsRow, ColDvN, dtEmployees.Rows[i]["DivisionName"].ToString());
                        oru.SetText(ref sheet1, xlsRow, ColDpN, dtEmployees.Rows[i]["DepartmentName"].ToString());
                        oru.SetText(ref sheet1, xlsRow, colSEC, dtEmployees.Rows[i]["SectionName"].ToString());
                        oru.SetText(ref sheet1, xlsRow, colSSEC, dtEmployees.Rows[i]["SubSectionName"].ToString());
                        oru.SetText(ref sheet1, xlsRow, colLN, dtEmployees.Rows[i]["Line"].ToString());
                        oru.SetText(ref sheet1, xlsRow, colLD, dtEmployees.Rows[i]["LegalDesignation"].ToString());
                        oru.SetText(ref sheet1, xlsRow, ColGrd, dtEmployees.Rows[i]["Grade"].ToString());
                        var payDays = Convert.ToDouble(dtEmployees.Rows[i]["TotalProcDate"]) - Convert.ToDouble(dtEmployees.Rows[i]["AbsentDays"]);
                        oru.SetText(ref sheet1, xlsRow, colPayDays, payDays);
                        
                        
                        oru.SetText(ref sheet1, xlsRow, ColPdDy, dtEmployees.Rows[i]["PresentDays"].ToString());
                        oru.SetText(ref sheet1, xlsRow, ColAbDy, dtEmployees.Rows[i]["AbsentDays"].ToString());
                        oru.SetText(ref sheet1, xlsRow, ColHlDy, dtEmployees.Rows[i]["HoliDay"].ToString());
                        oru.SetText(ref sheet1, xlsRow, ColWkOf, dtEmployees.Rows[i]["WeekOff"].ToString());
                        oru.SetText(ref sheet1, xlsRow, colLWP, dtEmployees.Rows[i]["LWP"].ToString());

                        #region -------------Extra Absend--------

                        decimal _ExtraAbsent = 0;
                        dvExtraAbsent.RowFilter = "EmpSystemID='" + dtEmployees.Rows[i]["EmpInfoSystemID"] + "' ";
                        _ExtraAbsent = dvExtraAbsent.Count;
                        oru.SetText(ref sheet1, xlsRow, colXtraAbsnt, _ExtraAbsent.ToString());

                        #endregion -------------Extra Absend--------
                        oru.SetText(ref sheet1, xlsRow, ColLv, dtEmployees.Rows[i]["LeaveDays"].ToString());
                        oru.SetText(ref sheet1, xlsRow, colLate, dtEmployees.Rows[i]["TotalLate"].ToString());
                        oru.SetText(ref sheet1, xlsRow, colOthr, dtEmployees.Rows[i]["TotalOTHr"].ToString());
                  

                        SrNo += 1;
                        #endregion
                        x = dtEmployees.Rows[i]["EmpInfoSystemID"].ToString().Trim().ToUpper();
                        //x = dtEmployees.Rows[i]["EmpInfoSystemID"].ToString().Trim();
                        int _total_head_count_body = 0;
                        for (int ci = 0; ci < list.Count; ci++)
                        {
                            var ob = list[ci];
                            if (ob.SalaryHead.Length > 0)
                            {
                                if (ob.SalaryHeadId.ToUpper() == "CTC" || ob.SalaryHeadId.ToUpper() == "GROSS")
                                {
                                    var formula = ob.SalaryHead;
                                    var hId = ob.SalaryHeadId;
                                    _total_head_count_body++;

                                    sheet1.Range[xlsRow, ob.XLColIndex].Formula = "=" + oRU.SetFormula(formula, xlsRow);
                                    sheet1.Range[xlsRow, ob.XLColIndex].NumberFormat = GetDecimalFormat(ob);
                                    sheet1.Range[xlsRow, ob.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                    sheet1.Range[xlsRow, ob.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                }//ctc , gross
                                else
                                {
                                    var hId = ob.SalaryHeadId;
                                    _total_head_count_body++;
                                    DataView dvBonusData = new DataView(dsBonus.Tables[0]);
                                    dvBonusData.RowFilter = "SalaryHeadID='" + hId + "' and EmpSystemID='" + x + "'";
                                    //var _basic_col = list.Where(r => r.HeadCategory == "BASIC").Select(r => r.XLColIndex).FirstOrDefault();
                                    if (dvBonusData.Count > 0)
                                    {
                                        //var _basic_cell = oRU.GetColumnNameForXls(Convert.ToInt32(_basic_col)) + xlsRow;
                                        //sheet1.Range[xlsRow, ob.XLColIndex].Number = Convert.ToDouble(bplib.clsWebLib.GetNumData(dvBody[0]["VoluntaryPFValue"].ToString()));
                                        sheet1.Range[xlsRow, ob.XLColIndex].Number = Convert.ToDouble(bplib.clsWebLib.GetNumData(dvBonusData[0]["Bonus"].ToString()));
                                        sheet1.Range[xlsRow, ob.XLColIndex].NumberFormat = oRU.NumberFormatInt();
                                        sheet1.Range[xlsRow, ob.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                        sheet1.Range[xlsRow, ob.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                    }
                                    //}
                                    else//other all
                                    {
                                        //DataView dvBody = new DataView(dsSlrProc.Tables[0]);
                                        //dvBody.RowFilter = "SalaryHeadID='" + hId + "' and EmpInfoSystemID='" + x + "'";
                                      var  _data = listdsSlrProc.Where(r => r.SalaryHeadID == hId && r.EmpInfoSystemID==x).FirstOrDefault();
                                        
                                        if (_data!=null)
                                        {
                                            if (ob.Deduction == "Deduction")
                                            {
                                                sheet1.Range[xlsRow, ob.XLColIndex].Number = Convert.ToDouble(_data.DisbusmentAmount) * (-1);
                                            }
                                            else
                                            {
                                                sheet1.Range[xlsRow, ob.XLColIndex].Number = Convert.ToDouble(_data.DisbusmentAmount);
                                            }
                                            sheet1.Range[xlsRow, ob.XLColIndex].NumberFormat = GetDecimalFormat(ob);
                                            sheet1.Range[xlsRow, ob.XLColIndex].HorizontalAlignment = ExcelHAlign.HAlignRight;
                                            sheet1.Range[xlsRow, ob.XLColIndex].VerticalAlignment = ExcelVAlign.VAlignCenter;
                                        }//row found
                                }//-----------------------------------
                            }
                            }//
                        }//for dtSalaryHead

                        //gross-deduction
                        var CTCIndex = list.Where(r => r.SalaryHeadId == "CTC").Select(r => r.XLColIndex).FirstOrDefault();
                        var grossIndex = list.Where(r => r.SalaryHeadId == "Gross").Select(r => r.XLColIndex).FirstOrDefault();
                        var dedIndex = list.Where(r => r.SalaryHeadId == "Deduction").Select(r => r.XLColIndex).FirstOrDefault();
                        var dedFormula = list.Where(r => r.SalaryHeadId == "Deduction").Select(r => r.SalaryHead).FirstOrDefault();

                        var CTCAdd = oRU.SetFormula(CTCIndex.ToString(), xlsRow);
                        var grossAdd = oRU.SetFormula(grossIndex.ToString(), xlsRow);
                        var dedAdd = oRU.SetFormula(dedFormula, xlsRow);

                        if (_para.IsCTCbased)
                        {
                            sheet1.Range[xlsRow, np].Formula = "=" + CTCAdd + "-(" + dedAdd + ")";
                        }
                        else
                        {
                            sheet1.Range[xlsRow, np].Formula = "=" + grossAdd + "-(" + dedAdd + ")";
                        }
                        sheet1.Range[xlsRow, np].NumberFormat = oRU.NumberFormatDecimalTwo();
                        sheet1.Range[xlsRow, np].HorizontalAlignment = ExcelHAlign.HAlignRight;
                        sheet1.Range[xlsRow, np].VerticalAlignment = ExcelVAlign.VAlignCenter;

                        xlsRow++;
                    }//for emp count

                    #endregion ----------------------Data-----------------------

                    #region Line Setup
                    if (RowIndex >= (xlsRow - 1))
                    {
                        xlsRow = RowIndex + 2;
                    }

                    sheet1.Range[5, 1, xlsRow - 1, ColGrd - 1].WrapText = true;
                    sheet1.Range[5, 1, xlsRow - 1, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                    sheet1.Range[5, 1, xlsRow - 1, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                    sheet1.Range[5, ColGrd, xlsRow - 1, endXlsCol].CellStyle.ShrinkToFit = true;
                    #endregion

                    #region Freeze Panes
                    sheet1.UsedRange["A7"].FreezePanes();
                    sheet1.FirstVisibleColumn = 1;
                    sheet1.FirstVisibleRow = 6;
                    #endregion

                    #region UsedRange Alignment
                    //sheet1.UsedRange.WrapText = true;
                    sheet1.UsedRange.CellStyle.Font.Size = 8;
                    sheet1.Range["A1"].CellStyle.Font.Size = 14;
                    sheet1.Range["A2"].CellStyle.Font.Size = 10;
                    sheet1.Range["A3"].CellStyle.Font.Size = 10;
                    sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;
                    #endregion UsedRange Alignment

                    #region Page Setup
                    sheet1.PageSetup.TopMargin = 0.5;
                    sheet1.PageSetup.BottomMargin = 0.7;
                    sheet1.PageSetup.PrintTitleRows = "$1:$7";
                    sheet1.PageSetup.RightFooter = "&\"Times New Roman\"&06" + "Page " + "&p" + " of " + "&N";
                    sheet1.PageSetup.LeftFooter = "&\"Times New Roman\"&06" + "Printed By: " + _para.UserId + "\n" + "Print Date && Time: " + DateTime.Now.ToString("dd-MMM-yyyy h:MM tt").ToString();
                    sheet1.PageSetup.LeftMargin = 0.5;
                    sheet1.PageSetup.RightMargin = 0.2;
                    sheet1.PageSetup.Orientation = ExcelPageOrientation.Landscape;
                    sheet1.PageSetup.FitToPagesTall = 0;
                    sheet1.PageSetup.FitToPagesWide = 1;
                    sheet1.PageSetup.PaperSize = ExcelPaperSize.PaperA4;
                    sheet1.Name = _para.sMonth + "_" + _para.sYear;
                    #endregion
                    //}
                    workbook.Version = ExcelVersion.Excel97to2003;
                    string strFileName = "SalarySheet_" + _para.sMonth + "_" + _para.sYear + ".xls";
                    //string strFileName = "SalarySheet" + bplib.clsWebLib.DateData_DBToApp(DateTime.Now.Date, bplib.clsWebLib.STD_DATE_FORMAT).ToString("dd-MMM-yyyy") + ".xls";
                    workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, _para.Response, ExcelDownloadType.PromptDialog);

                    workbook.Close();
                    excelEngine.Dispose();
                }
                else
                {
                    Exception ex = new Exception("No Data found...");
                    throw (ex);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objRpt = null;
                dsSlrProc = null;
                dvSlrProc = null;
                excelEngine = null;
                application = null;
                workbook = null;
                sheet1 = null;
            }
        }//End Function
        private void CreateDynamicSHead(eSalaryStructure ess, DataTable dtSalaryHead, out int _total_head_count, ref IWorksheet sheet1, ref int xlsRow, ref int xlsCol, ref int ColGrs, out int _count_earning_head, out int _count_deducting_head, out int _count_earning_ctchead, out List<SalaryHeadSequence> list)
        {
            try
            {
                list = new List<SalaryHeadSequence>();
                _total_head_count = 0;
                _count_earning_head = 0;
                _count_earning_ctchead = 0;
                _count_deducting_head = 0;
                int countGross = 0;
                string grossFormula = "";
                string deductionFormula = "";
                ReportUtility oru = new ReportUtility();
                //ColGrs++;
                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
                    {
                        bool _IsNetPayEffect = true;// bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IsNetPayEffect"].ToString());
                        bool _IsValid = false;
                        if (eSalaryStructure.Cash == ess)
                        {
                            if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == "CASH")
                            {
                                _IsValid = true;
                            }//HeadCategory
                        }//Cash
                        else if (eSalaryStructure.Bank == ess)
                        {
                            if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CASH")
                            {
                                _IsValid = true;
                            }//HeadCategory
                        }//Cash
                        else
                        {
                            _IsValid = true;
                        }//Cash

                        if (_IsValid && _IsNetPayEffect)
                        {
                            if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
                            {
                                if (bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IsGrossComponent"].ToString()))
                                {
                                    if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E")
                                    {
                                        _total_head_count++;
                                        countGross++;
                                        oru.SetHeaderTextRotate(ref sheet1, xlsRow, ColGrs + countGross, dtSalaryHead.Rows[ci]["SalaryHead"].ToString());
                                        //sheet1.Range[xlsRow, ColGrs + countGross].Text = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                                        //sheet1.Range[xlsRow, ColGrs + countGross].ColumnWidth = 12;

                                        if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
                                        {
                                            sheet1.Range[xlsRow, ColGrs + countGross].CellStyle.Font.Color = ExcelKnownColors.Red;
                                        }
                                        xlsCol += 1;

                                        SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
                                        salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
                                        salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
                                        salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                                        salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
                                        salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();
                                        salaryHeadSequence.Sequence = ci;
                                        salaryHeadSequence.XLColIndex = ColGrs + countGross;
                                        salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper();

                                        if (grossFormula.Length == 0)
                                        {
                                            grossFormula += salaryHeadSequence.XLColIndex.ToString();
                                        }
                                        else
                                        {
                                            grossFormula += "," + salaryHeadSequence.XLColIndex.ToString();
                                        }
                                        list.Add(salaryHeadSequence);

                                        _count_earning_head++;
                                    }
                                }//IsGrossComponent
                            }//CTC/Gross
                        }//isvalid
                    }//SalaryHead
                }//for
                xlsCol += 1;

                countGross++;
                _count_earning_head++;
                sheet1.Range[xlsRow, ColGrs + countGross].Text = "Gross";
                sheet1.Range[xlsRow, ColGrs + countGross].ColumnWidth = 10;

                SalaryHeadSequence salaryHSGross = new SalaryHeadSequence();

                salaryHSGross.SalaryHead = grossFormula;
                salaryHSGross.SalaryHeadId = "Gross";
                salaryHSGross.XLColIndex = ColGrs + countGross;
                list.Add(salaryHSGross);

                //ctc
                int countCTC = countGross;

                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
                    {
                        if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
                        {
                            if (bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IsCTCComponent"].ToString()) == true && bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IsGrossComponent"].ToString()) == false)
                            {
                                if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "E")
                                {
                                    _total_head_count++;
                                    countCTC++;
                                    oru.SetHeaderTextRotate(ref sheet1, xlsRow, ColGrs + countCTC, dtSalaryHead.Rows[ci]["SalaryHead"].ToString());
                                    //sheet1.Range[xlsRow, ColGrs + countCTC].Text = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                                    //sheet1.Range[xlsRow, ColGrs + countCTC].ColumnWidth = 14;

                                    if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
                                    {
                                        sheet1.Range[xlsRow, ColGrs + countCTC].CellStyle.Font.Color = ExcelKnownColors.Red;
                                    }
                                    xlsCol += 1;

                                    SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
                                    salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
                                    salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
                                    salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                                    salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
                                    salaryHeadSequence.Sequence = ci;
                                    salaryHeadSequence.XLColIndex = ColGrs + countCTC;
                                    //if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == bplib.clsWebLib.FESTIVAL_BONUS)//BONUS
                                    //{
                                    //    salaryHeadSequence.HeadCategory = bplib.clsWebLib.FESTIVAL_BONUS;
                                    //}

                                    salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();
                                    if (grossFormula.Length == 0)
                                    {
                                        grossFormula += salaryHeadSequence.XLColIndex.ToString();
                                    }
                                    else
                                    {
                                        grossFormula += "," + salaryHeadSequence.XLColIndex.ToString();
                                    }
                                    list.Add(salaryHeadSequence);

                                    _count_earning_ctchead++;
                                }
                            }//IsCTCComponent
                        }//CTC/Gross
                    }//SalaryHead
                }//for
                xlsCol += 1;

                countCTC++;
                _count_earning_ctchead++;
                sheet1.Range[xlsRow, ColGrs + countCTC].Text = "CTC";
                sheet1.Range[xlsRow, ColGrs + countCTC].ColumnWidth = 10;

                SalaryHeadSequence salaryHSCTC = new SalaryHeadSequence();

                salaryHSCTC.SalaryHead = grossFormula;
                salaryHSCTC.SalaryHeadId = "CTC";
                salaryHSCTC.XLColIndex = ColGrs + countCTC;
                list.Add(salaryHSCTC);

                // deduction
                //int countDeduction = countGross;

                int countDeduction = countCTC;

                for (int ci = 0; ci < dtSalaryHead.Rows.Count; ci++)
                {
                    if (dtSalaryHead.Rows[ci]["SalaryHead"].ToString().Trim().Length > 0)
                    {
                        bool _IsNetPayEffect = true;// bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IsNetPayEffect"].ToString());
                        bool _IsValid = false;
                        if (eSalaryStructure.Cash == ess)
                        {
                            if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == "CASH")
                            {
                                _IsValid = true;
                            }//HeadCategory
                        }//Cash
                        else if (eSalaryStructure.Bank == ess)
                        {
                            if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CASH")
                            {
                                _IsValid = true;
                            }//HeadCategory
                        }//Cash
                        else
                        {
                            _IsValid = true;
                        }//Cash

                        if (_IsValid && _IsNetPayEffect)
                        {
                            if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "TOTAL DEDUCTION" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "CTC" && dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() != "GROSS")
                            {//	Total Deduction
                                if (dtSalaryHead.Rows[ci]["HeadType"].ToString().ToUpper() == "D")
                                {
                                    _total_head_count++;
                                    countDeduction++;
                                    oru.SetHeaderTextRotate(ref sheet1, xlsRow, ColGrs + countDeduction, dtSalaryHead.Rows[ci]["SalaryHead"].ToString());
                                    //sheet1.Range[xlsRow, ColGrs + countDeduction].Text = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                                    //sheet1.Range[xlsRow, ColGrs + countDeduction].ColumnWidth = 14;

                                    if (dtSalaryHead.Rows[ci]["Sequence"].ToString() == "99")
                                    {
                                        sheet1.Range[xlsRow, ColGrs + countDeduction].CellStyle.Font.Color = ExcelKnownColors.Red;
                                    }
                                    xlsCol += 1;

                                    SalaryHeadSequence salaryHeadSequence = new SalaryHeadSequence();
                                    salaryHeadSequence.IsInt = bplib.clsWebLib.GetBoolData(dtSalaryHead.Rows[ci]["IntegerInDisb"].ToString());
                                    salaryHeadSequence.DecimalNo = Convert.ToInt32(bplib.clsWebLib.GetNumData(dtSalaryHead.Rows[ci]["DecimalNo"].ToString()));
                                    salaryHeadSequence.SalaryHead = dtSalaryHead.Rows[ci]["SalaryHead"].ToString();
                                    salaryHeadSequence.SalaryHeadId = dtSalaryHead.Rows[ci]["SalaryHeadID"].ToString();
                                    salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();
                                    salaryHeadSequence.Deduction = "Deduction";
                                    //salaryHeadSequence.IsInt = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();
                                    //salaryHeadSequence.DecimalNo = dtSalaryHead.Rows[ci]["HeadCategory"].ToString();

                                    salaryHeadSequence.Sequence = ci;
                                    salaryHeadSequence.XLColIndex = ColGrs + countDeduction;

                                    //for bonus and vpf
                                    salaryHeadSequence.HeadCategory = dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper();
                                    //if (dtSalaryHead.Rows[ci]["HeadCategory"].ToString().ToUpper() == bplib.clsWebLib.PFHEADCATEGORY)//vpf
                                    //{
                                    //    salaryHeadSequence.HeadCategory = bplib.clsWebLib.PFHEADCATEGORY;
                                    //}

                                    if (deductionFormula.Length == 0)
                                    {
                                        deductionFormula += salaryHeadSequence.XLColIndex.ToString();
                                    }
                                    else
                                    {
                                        deductionFormula += "," + salaryHeadSequence.XLColIndex.ToString();
                                    }

                                    list.Add(salaryHeadSequence);

                                    _count_deducting_head++;
                                }
                            }//CTC/Gross
                        }
                    }//SalaryHead
                }//for
                SalaryHeadSequence salaryHSDed = new SalaryHeadSequence();

                salaryHSDed.SalaryHead = deductionFormula;
                salaryHSDed.SalaryHeadId = "Deduction";
                salaryHSDed.XLColIndex = ColGrs + 1 + countDeduction;
                list.Add(salaryHSDed);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public enum eSalaryStructure
        {
            Bank,
            Cash,
            Both
        }
    }//cls

}
public class ParaSalaryReport
{
    /// <summary>
    /// salary structure
    /// </summary>
    public string GroupId { get; set; }
    public string CompanyId { get; set; }
    public string PlantId { get; set; }
    public string UserId { get; set; }
    public System.Web.HttpResponse Response { get; set; }
    public string EmployeeIds { get; set; }//
    public string MinWageEffectiveDate { get; set; }//
    public bool IsCTCbased { get; set; }//

    /// <summary>
    /// salary sheet
    /// </summary>
    public string SalaryProcessId { get; set; }
    public string EmpStatus { get; set; }
    public string sYear { get; set; }
    public string sMonth { get; set; }
    public string PayGroup { get; set; }
}
class ParaDynamicHead
{
    public DataTable dtSalaryHead { get; set; }
    public int _total_head_count { get; set; }
    public IWorksheet sheet1 { get; set; }
    public int xlsRow { get; set; }
    public int xlsCol { get; set; }
    public int ColGrs { get; set; }
    public int _count_earning_head { get; set; }
    public int _count_deducting_head { get; set; }
    public int _count_earning_ctchead { get; set; }
    public List<SalaryHeadSequence> list { get; set; }
}
class SalarySheetReport
{
    public string EmpInfoSystemID { get; set; }
    public string SalaryHeadID { get; set; }
    public string HeadCategory { get; set; }
    public decimal DisbusmentAmount { get; set; } = 0;
    public decimal EntryAmount { get; set; } = 0;
}
class SalarySheetReportUD
{
    public string EmpSystemID { get; set; }
    public string SalaryHeadID { get; set; }
    public string HeadCategory { get; set; }
    public decimal DisbusmentAmount { get; set; } = 0;
    public decimal EntryAmount { get; set; } = 0;
}
class SalarySheetReportStructure //basic and Gross Value Structure and  
{
    public string EmpInfoSystemID { get; set; }
    public string SalaryHeadID { get; set; }
    public decimal DisbusmentAmount { get; set; } = 0;
    public decimal EntryAmount { get; set; } = 0;
}
class SalaryStructureReport
{
    public string EmpInfoSystemID { get; set; }
    public string SalaryHeadID { get; set; }
    public decimal EntryAmount { get; set; } = 0;
}
public class SalaryRegisterSorting : BaseModel
{
    public string Parameter { get; set; }
    public string Sequence { get; set; }
}
class SalaryStructurePaySlip
{
    public string SystemId { get; set; }
    public string SalaryHeadID { get; set; }
    public decimal EntryAmount { get; set; } = 0;
    public bool isDecimal { get; set; }
    public int DecimalNo { get; set; }
}
class SalarySheetPaySlip
{
    //public string SystemId { get; set; }
    public string SalaryHeadID { get; set; }
    //public decimal EntryAmount { get; set; } = 0;
    public decimal DisbusmentAmount { get; set; } = 0;
    public bool IsDecimalInDisb { get; set; }
    public int DecimalNo { get; set; }
}

