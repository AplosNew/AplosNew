#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Setups;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.HumanResource.Controllers
{
    public class EmployeeAdditionDeductionProcessController  : BaseController
    {
        //abcd
        
        //authentication for
        //GetList Create Delete

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        public EmployeeAdditionDeductionProcessController(ISqlRepository R)
        {
            _sqlRepository = R;
        }

        #endregion Constructor


     
        public ActionResult Aplos()
        {
            return View();
        }


        [HttpGet, Authorize]
       public ActionResult RunProcess(string date)
        {
            try
            {

                DataTable dtTrial = new DataTable();
                dtTrial.Columns.Add("Id", typeof(string));
                dtTrial.Columns.Add("SystemId", typeof(string));
                dtTrial.Columns.Add("LegalDesignationId", typeof(string));
                dtTrial.Columns.Add("EmpTypeId", typeof(string));
                dtTrial.Columns.Add("EmpAdditionDeductionHeaderId", typeof(string));
                dtTrial.Columns.Add("AdditionDeductionHeadId", typeof(string));
                dtTrial.Columns.Add("Amount", typeof(float));
                dtTrial.Columns.Add("Month", typeof(int));
                dtTrial.Columns.Add("MonthDay", typeof(int));
                dtTrial.Columns.Add("PlantId", typeof(int));
                dtTrial.Columns.Add("YearNo", typeof(int));
                dtTrial.Columns.Add("Plant", typeof(string));
                dtTrial.Columns.Add("EmpType", typeof(string));
                dtTrial.Columns.Add("LegalDesignation", typeof(string));
                dtTrial.Columns.Add("AdditionDeductionHead", typeof(string));

                //Getting Today's Day and Year
                var dayNo = int.Parse(DateTime.Now.ToString("dd"));
                var monthNo = int.Parse(DateTime.Now.ToString("MM"));
                var yearNo = int.Parse(DateTime.Now.ToString("yyyy"));

                if (date!=null)
                {
                     dayNo = int.Parse(Convert.ToDateTime(date).ToString("dd"));
                     monthNo = int.Parse(Convert.ToDateTime(date).ToString("MM"));
                }

                //Getting all the EmployeeAdditionDeduction Header Ids with every child Details of it Based on Effective Date.
                string getEmployeeAdditionHeaders = @"Select  eh.Id as HeaderId, eh.Type, eh.AdditionDeductionHeadId, eh.Amount ,MONTH('1' + ep.Month +'00') AS [MONTH_NUMBER] , ep.MonthDay,
                                                    ec.PlantId, isnull(ec.DesignationId,'All') as DesignationId , ec.EmpTypeId,eh.isHeadApplicable,eh.HeadValueId , isnull(dm.LegalDesignationId,'All') LegalDesignationId , sh.SalaryHead,
													isnull(ec.EmploymentType,'All') as EmploymentType
                                                    from dbo.EmployeeAdditionDeductionHeader eh
                                                    left join dbo.EmployeeAdditionDeductionPeriod ep on ep.MasterId = eh.Id
                                                    left join dbo.EmployeeAdditionDeductionPlantChild ec on ec.MasterId = eh.Id
                                                    left join mst.DesignationMasterLegalDesignation dm on dm.DesignationMasterId = ec.DesignationId
                                                    left join dbo.SalaryHead sh on sh.SalaryHeadID = eh.AdditionDeductionHeadId
                                                    where EffectiveDate<=GetDate() 
                                                    and ep.Month is not null  and eh.Active = 1
                                                    and MONTH('1' + ep.Month +'00') = " + monthNo+" and MonthDay <= "+dayNo+"";

                DataTable dtEmpAddDedTab = _sqlRepository.GetDataTable(getEmployeeAdditionHeaders);

                var old = @"Select * from dbo.EmployeeAdditionDeductionProcess";
                DataTable dtOldData = _sqlRepository.GetDataTable(old);

                if (dtEmpAddDedTab.Rows.Count>0)
                {
                    for(int i = 0; i<dtEmpAddDedTab.Rows.Count;i++)
                    {

                        string plant = dtEmpAddDedTab.Rows[i]["PlantId"].ToString();
                        string EmpType = dtEmpAddDedTab.Rows[i]["EmpTypeId"].ToString();
                        string LDesg = dtEmpAddDedTab.Rows[i]["LegalDesignationId"].ToString();
                        string EmploymentType = dtEmpAddDedTab.Rows[i]["EmploymentType"].ToString();
                        string EmployTyStr = ""; 
                        string DesgStr = "";
                        if (LDesg == "All")
                        {
                            DesgStr = "1 = 1";
                        }
                        else
                        {
                            DesgStr = "ei.LegalDesignationId='" + LDesg + "'";
                        }

                        if(EmploymentType == "All")
                        {
                            EmployTyStr = "1=1";
                        }
                        else
                        {
                            EmployTyStr = "ei.EmploymentType='" + EmploymentType + "'";
                        }

                        //Getting all the Employees from the EmployeeInformation with the Filters
                        string emps = "";
                        if(dtEmpAddDedTab.Rows[i]["isHeadApplicable"].ToString() == "True")
                        {
                            emps = @"Select ei.SystemId,ei.PlantId, ei.LegalDesignationId, ddm.DesignationMasterId, dm.Id, dm.EmployeeCategoryId, p.Username as Plant, em.Username as EmpType
                                    ,lld.UserName as LegalDesignation , EmpSlr.DefineAmount
									from EmployeeInformation ei
                                    left join mst.DesignationMasterLegalDesignation ddm on ddm.LegalDesignationId = ei.LegalDesignationId
                                    left join mst.DesignationMaster dm on dm.Id = ddm.DesignationMasterId
                                    left join hkp.legalDesignation lld on lld.Id = ei.LegalDesignationId
                                    left join org.Plant p on p.Id = ei.PlantId
                                    left join hkp.EmployeeCategory em on em.Id = dm.EmployeeCategoryId
									left JOIN SalaryInfoDefineMaster MST ON ei.SystemId = MST.EmpInfoSystemID 
									left join  SalaryInfoDefine EmpSlr on EmpSlr.SalaryID = MST.SystemID 
                                    where ei.PlantId = '" + plant + @"' and 
                                    dm.EmployeeCategoryId = '" + EmpType + @"' and
                                    " + DesgStr + @" and "+EmployTyStr+@" and
									EmpSlr.SalaryHeadID = '"+dtEmpAddDedTab.Rows[i]["HeadValueId"].ToString()+ @"' and
                                    ei.EmployeeStatus = 'Active'
									and EmpSlr.DefineAmount is not null and EmpSlr.DefineAmount>0
                                    ";
                        }
                        else
                        {
                             emps = @"Select ei.SystemId,ei.PlantId, ei.LegalDesignationId, ddm.DesignationMasterId, dm.Id, dm.EmployeeCategoryId, p.Username as Plant, em.Username as EmpType
                                    ,lld.UserName as LegalDesignation from EmployeeInformation ei
                                    left join mst.DesignationMasterLegalDesignation ddm on ddm.LegalDesignationId = ei.LegalDesignationId
                                    left join mst.DesignationMaster dm on dm.Id = ddm.DesignationMasterId
                                    left join hkp.legalDesignation lld on lld.Id = ei.LegalDesignationId
                                    left join org.Plant p on p.Id = ei.PlantId
                                    left join hkp.EmployeeCategory em on em.Id = dm.EmployeeCategoryId
                                    where ei.PlantId = '" + plant + @"' and 
                                    dm.EmployeeCategoryId = '" + EmpType + @"' and 
                                    " + DesgStr + @" and " + EmployTyStr + @" and
                                    ei.EmployeeStatus = 'Active' 
                                    ";
                        }
                       
                        DataTable dtActiveEmps = _sqlRepository.GetDataTable(emps);

                        if(dtActiveEmps.Rows.Count>0)
                        {
                            string _Id = "";
                            bplib.clsGenID genid = new bplib.clsGenID();
                            genid.GenID("dbo.EmployeeAdditionDeductionProcess", out _Id);
                            DataRow dr = null;
                            for (int j = 0; j < dtActiveEmps.Rows.Count; j++)
                            {

                                dtOldData.DefaultView.RowFilter = @"EmpSystemId='"+ dtActiveEmps.Rows[j]["SystemId"].ToString() + "'";
                                int count = 0;
                                for(int v = 0; v<dtOldData.DefaultView.Count; v++)
                                {
                                    var ch = dtOldData.DefaultView[v].Row;
                                    if(ch["EmpAdditionDeductionHeaderId"].ToString() == dtEmpAddDedTab.Rows[i]["HeaderId"].ToString() &&
                                        ch["AdditionDeductionHeadId"].ToString() == dtEmpAddDedTab.Rows[i]["AdditionDeductionHeadId"].ToString() 
                                        && int.Parse(ch["Month"].ToString())== int.Parse(dtEmpAddDedTab.Rows[i]["MONTH_NUMBER"].ToString())
                                        && int.Parse(ch["YearNo"].ToString()) == yearNo )
                                    {
                                        count++;
                                    } 
                                    
                                }

                                if(count >0)
                                {
                                    continue;
                                }
                                else
                                {
                                    
                                    
                                    dr = dtTrial.NewRow();
                                    dr["Id"] = "TR"+_Id + i + j;
                                    dr["SystemId"] = dtActiveEmps.Rows[j]["SystemId"].ToString();
                                    dr["LegalDesignationId"] = dtActiveEmps.Rows[j]["LegalDesignationId"].ToString();
                                    dr["LegalDesignation"] = dtActiveEmps.Rows[j]["LegalDesignation"].ToString();
                                    dr["EmpTypeId"] = EmpType;
                                    dr["EmpAdditionDeductionHeaderId"] = dtEmpAddDedTab.Rows[i]["HeaderId"].ToString();
                                    dr["AdditionDeductionHeadId"] = dtEmpAddDedTab.Rows[i]["AdditionDeductionHeadId"].ToString();
                                    dr["AdditionDeductionHead"] = dtEmpAddDedTab.Rows[i]["SalaryHead"].ToString();
                                    dr["Amount"] = clsStaticInfo.dbl(dtEmpAddDedTab.Rows[i]["Amount"].ToString());
                                    dr["Month"] = clsStaticInfo.dbl(dtEmpAddDedTab.Rows[i]["MONTH_NUMBER"].ToString());
                                    dr["MonthDay"] = clsStaticInfo.dbl(dtEmpAddDedTab.Rows[i]["MonthDay"].ToString());
                                    dr["PlantId"] = dtActiveEmps.Rows[j]["PlantId"].ToString();
                                    dr["Plant"] = dtActiveEmps.Rows[j]["Plant"].ToString();
                                    dr["EmpType"] = dtActiveEmps.Rows[j]["EmpType"].ToString();
                                    dr["YearNo"] = yearNo;
                                    dtTrial.Rows.Add(dr);
                                }

                               
                            }
                        }
                        
                    }
                }


                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from dbo.EmployeeAdditionDeductionProcess where 1 = 2 ", out dsMaster, false, "1");

                //For Testing
                DataTable dttrialRun = dsMaster.Tables[0]; 
                //

                DataRow dd = null;
                for(int i =0; i< dtTrial.Rows.Count;i++)
                {
                    dd = dsMaster.Tables[0].NewRow();
                    dd["Id"] = dtTrial.Rows[i]["Id"];
                    dd["EmpSystemId"] = dtTrial.Rows[i]["SystemId"];
                    dd["LegalDesignationId"] = dtTrial.Rows[i]["LegalDesignationId"];
                    dd["EmpTypeId"] = dtTrial.Rows[i]["EmpTypeId"];
                    dd["EmpAdditionDeductionHeaderId"] = dtTrial.Rows[i]["EmpAdditionDeductionHeaderId"];
                    dd["AdditionDeductionHeadId"] = dtTrial.Rows[i]["AdditionDeductionHeadId"];
                    dd["Amount"] = dtTrial.Rows[i]["Amount"];
                    dd["Month"] = dtTrial.Rows[i]["Month"];
                    dd["MonthDay"] = dtTrial.Rows[i]["MonthDay"];
                    dd["PlantId"] = dtTrial.Rows[i]["PlantId"];
                    dd["YearNo"] = dtTrial.Rows[i]["YearNo"];

                    dsMaster.Tables[0].Rows.Add(dd);
                }

                
               

                //Checking with the Master Tables of Tarek Sir

                var mas = @"SELECT am.EmpInfoSystemID,Cast(am.MonthNo as int) as MonthNo,Cast(am.YearNo as int) as YearNo,ac.SalaryHeadID FROM MonthWiseExtraSalaryAmtMaster AS am
                            left join  MonthWiseExtraSalaryAmtChild AS ac on ac.MWESAMasterSystemID = am.SystemID where am.YearNo='"+yearNo+"'";

                DataTable dtMainTable = _sqlRepository.GetDataTable(mas);

                //Getting the structure for the Amount Master Table
                DataSet dsAmtMaster;
                ConnectionManager.DAL.ConManager conn = new ConnectionManager.DAL.ConManager("1");
                conn.OpenDataSetThroughAdapter("select * from dbo.MonthWiseExtraSalaryAmtMaster where 1 = 2 ", out dsAmtMaster, false, "1");

                //Getting the structure for the Amount Child Table
                DataSet dsAmtChild;
                ConnectionManager.DAL.ConManager conn1 = new ConnectionManager.DAL.ConManager("1");
                conn1.OpenDataSetThroughAdapter("select * from dbo.MonthWiseExtraSalaryAmtChild where 1 = 2 ", out dsAmtChild, false, "1");
                
                DataTable dtAmtMaster =new DataTable();
                dtAmtMaster = dtTrial.Copy();
                dtTrial.Rows.Clear();
                

                // To check whether the Entries are present or not in the above two tables.
                if(dtAmtMaster.Rows.Count>0)
                {
                    DataRow dtt = null;
                    for (int i = 0; i < dtAmtMaster.Rows.Count; i++)
                    {

                        dtMainTable.DefaultView.RowFilter = @"EmpInfoSystemID='"+dtAmtMaster.Rows[i]["SystemId"] +@"' and MonthNo="+ dtAmtMaster.Rows[i]["Month"] +@" 
                        and YearNo="+dtAmtMaster.Rows[i]["YearNo"]+@" and SalaryHeadID='"+dtAmtMaster.Rows[i]["AdditionDeductionHeadId"] +@"'";
                        if(dtMainTable.DefaultView.Count>0)
                        {
                            continue;
                        }
                        else
                        {
                            
                            dtt = dtTrial.NewRow();
                            dtt["Id"] = dtAmtMaster.Rows[i]["Id"];
                            dtt["SystemId"] = dtAmtMaster.Rows[i]["SystemId"];
                            dtt["LegalDesignationId"] = dtAmtMaster.Rows[i]["LegalDesignationId"];
                            dtt["EmpTypeId"] = dtAmtMaster.Rows[i]["EmpTypeId"];
                            dtt["EmpAdditionDeductionHeaderId"] = dtAmtMaster.Rows[i]["EmpAdditionDeductionHeaderId"];
                            dtt["AdditionDeductionHeadId"] = dtAmtMaster.Rows[i]["AdditionDeductionHeadId"];
                            dtt["Amount"] = dtAmtMaster.Rows[i]["Amount"];
                            dtt["Month"] = dtAmtMaster.Rows[i]["Month"];
                            dtt["MonthDay"] = dtAmtMaster.Rows[i]["MonthDay"];
                            dtt["PlantId"] = dtAmtMaster.Rows[i]["PlantId"];
                            dtt["YearNo"] = dtAmtMaster.Rows[i]["YearNo"];
                            dtt["LegalDesignation"] = dtAmtMaster.Rows[i]["LegalDesignation"].ToString();
                            dtt["AdditionDeductionHead"] = dtAmtMaster.Rows[i]["AdditionDeductionHead"].ToString();
                            dtt["Plant"] = dtAmtMaster.Rows[i]["Plant"].ToString();
                            dtt["EmpType"] = dtAmtMaster.Rows[i]["EmpType"].ToString();
                            dtTrial.Rows.Add(dtt);

                            DataRow da = null;
                            DataRow dc = null;
                            string masterId = "";
                            dsAmtMaster.Tables[0].DefaultView.RowFilter = @"EmpInfoSystemID='" + dtAmtMaster.Rows[i]["SystemId"] + @"' and MonthNo= " + dtAmtMaster.Rows[i]["Month"] + @"  and YearNo=" + dtAmtMaster.Rows[i]["YearNo"] + @"";
                            if(dsAmtMaster.Tables[0].DefaultView.Count>0)
                            {
                                masterId = dsAmtMaster.Tables[0].DefaultView[0].Row["SystemId"].ToString();
                            }
                            else
                            {
                                da = dsAmtMaster.Tables[0].NewRow();
                                da["SystemId"] = "EADM-" + dtAmtMaster.Rows[i]["YearNo"].ToString().Substring(2, 2) + "-" + i;
                                da["EmpInfoSystemID"] = dtAmtMaster.Rows[i]["SystemId"].ToString();
                                da["PlantID"] = dtAmtMaster.Rows[i]["PlantId"].ToString();
                                da["MonthNo"] = int.Parse(dtAmtMaster.Rows[i]["Month"].ToString());
                                da["YearNo"] = int.Parse(dtAmtMaster.Rows[i]["YearNo"].ToString());
                                da["IsDisbusted"] = false;
                                da["AddedBy"] = "Sayanto";
                                da["DateAdded"] = DateTime.Now;
                                da["UpdatedBy"] = "Sayanto";
                                da["DateUpdated"] = DateTime.Now;
                                dsAmtMaster.Tables[0].Rows.Add(da);
                                masterId = da["SystemId"].ToString();
                            }

                            dc = dsAmtChild.Tables[0].NewRow();
                            dc["SystemId"] = "EADC-" + dtAmtMaster.Rows[i]["YearNo"].ToString().Substring(2, 2) + "-" + i;
                            dc["MWESAMasterSystemID"] = masterId;
                            dc["CurrencyRuleSystemID"] = "CR-202019";
                            dc["SalaryHeadID"] = dtAmtMaster.Rows[i]["AdditionDeductionHeadId"].ToString();
                            dc["EntryCurrencyID"] = 8;
                            dc["EntryAmount"] = clsStaticInfo.dbl(dtAmtMaster.Rows[i]["Amount"].ToString());
                            dc["DefineCurrencyID"] = 8;
                            dc["DefineAmount"] = clsStaticInfo.dbl(dtAmtMaster.Rows[i]["Amount"].ToString());
                            dc["AmtDefinitionCurrencyID"] = 8;
                            dc["AmtDefinitionRate"] = 0;
                            dc["ExtDataUploadApp"] = "Yes";
                            dc["AddedBy"] = "Sayanto";
                            dc["DateAdded"] = DateTime.Now;
                            dc["UpdatedBy"] = "Sayanto";
                            dc["DateUpdated"] = DateTime.Now;
                            dsAmtChild.Tables[0].Rows.Add(dc);
                        }
                    }
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster); // , dsAmtMaster, dsAmtChild


                return Json(Library.Service.Helpers.DataTableExtensions.DataTableToJson(dtTrial), JsonRequestBehavior.AllowGet);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        // The Report Downloads
        [HttpPost, Authorize]
        public ActionResult GetCurrentReport( List<Dictionary<string, object>> data)
        {

            try
            {
                var workbook = GetCurrentSaveData(data);

                var strFileName = DateTime.Now.ToString("yy-MM-dd") + "-" + "CurrentProcessedReport.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private IWorkbook GetCurrentSaveData(List<Dictionary<string,object>> data)
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            workbook.Version = ExcelVersion.Excel2016;

            var sheet = workbook.Worksheets[0];
            sheet.Name = "Current";




            int ROW = 6;
            int endCol = 1;
            int COL = 1;



            #region Grid Headers

            report.SetHeaderText(ref sheet, ROW, COL, "Sr No", 5, ExcelHAlign.HAlignCenter);
            int ColRow = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Id", 13, ExcelHAlign.HAlignCenter);
            int ColPLID = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Id", 13, ExcelHAlign.HAlignCenter);
            int ColMONo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Plant", 13, ExcelHAlign.HAlignCenter);
            int ColPlanQty = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Legal Designation", 13, ExcelHAlign.HAlignCenter);
            int ColItemId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Category", 13, ExcelHAlign.HAlignCenter);
            int ColSoId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Emp Addition Deduction Id", 13, ExcelHAlign.HAlignCenter);
            int ColMaterial = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Salary Head", 13, ExcelHAlign.HAlignCenter);
            int ColArticle = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Amount", 13, ExcelHAlign.HAlignCenter);
            int ColProductCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Month", 15, ExcelHAlign.HAlignCenter);
            int ColPONo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Month Day", 13, ExcelHAlign.HAlignCenter);
            int ColLotNo = COL;
            COL++;            

            report.SetHeaderText(ref sheet, ROW, COL, "Year No", 13, ExcelHAlign.HAlignCenter);
            int ColSoPoNo = COL;
            COL++;


            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;

            for (int i = 0; i < data.Count; i++)
            {
                sheet[ROW, ColRow].Text = (i+1).ToString();
                sheet[ROW, ColPLID].Text = data[i]["Id"].ToString();
                sheet[ROW, ColMONo].Text = data[i]["SystemId"].ToString();
                sheet[ROW, ColPlanQty].Text = data[i]["Plant"].ToString();
                sheet[ROW, ColItemId].Text = data[i]["LegalDesignation"].ToString();
                sheet[ROW, ColSoId].Text = data[i]["EmpType"].ToString();
                sheet[ROW, ColMaterial].Text = data[i]["EmpAdditionDeductionHeaderId"].ToString();
                sheet[ROW, ColArticle].Text = data[i]["AdditionDeductionHead"].ToString();
                sheet[ROW, ColProductCode].Text = data[i]["Amount"].ToString();
                sheet[ROW, ColPONo].Text = data[i]["Month"].ToString();
                sheet[ROW, ColLotNo].Text = data[i]["MonthDay"].ToString();
                sheet[ROW, ColSoPoNo].Text = data[i]["YearNo"].ToString();



                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }

            ROW++;
           

            endRow = ROW - 1;
            endRow = ROW - 1;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref sheet, endCol, "Current Employee Addition/Deduction Report", identity.PlantId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }

        //To get the already saved data

        [HttpPost, Authorize] //HttpGet
        public ActionResult GetSavedReport()
        {

            try
            {
                var workbook = GetSavedSaveData();

                workbook.Version = ExcelVersion.Excel2013;
                var strFileName ="SavedEmployeeAdditionDeduction.xlsx";
                string fullPath = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + strFileName);
                workbook.SaveAs(fullPath);

                return Json(new { FileName = strFileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private IWorkbook GetSavedSaveData() //HttpGet
        {
            var excelEngine = new ExcelEngine();
            var report = new ReportUtility();
            var workbook = report.GetWorkbook(ref excelEngine, 3);
            excelEngine.Excel.DefaultVersion = ExcelVersion.Excel2013;
            var sheet = workbook.Worksheets[0];
            sheet.Name = "Saved";

            var str = @"Select ep.*,p.UserName as Plant, l.UserName as LegalDesignation, sh.SalaryHead,ec.UserName as EmpType
                        from dbo.EmployeeAdditionDeductionProcess ep
                        left join org.plant p on ep.PlantId = p.Id
                        left join hkp.LegalDesignation l on l.Id = ep.LegalDesignationId
                        left join dbo.SalaryHead sh on sh.SalaryHeadID = ep.AdditionDeductionHeadId
                        left join hkp.EmployeeCategory ec on ec.Id = ep.EmpTypeId";

            List<Dictionary<string, object>> data = _sqlRepository.GetDataCollection(str);



            int ROW = 6;
            int endCol = 1;
            int COL = 1;


            #region Grid Headers

            report.SetHeaderText(ref sheet, ROW, COL, "Sr. No", 5, ExcelHAlign.HAlignCenter);
            int ColRow = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Id", 13, ExcelHAlign.HAlignCenter);
            int ColPLID = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Id", 13, ExcelHAlign.HAlignCenter);
            int ColMONo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Plant", 13, ExcelHAlign.HAlignCenter);
            int ColPlanQty = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Legal Designation", 13, ExcelHAlign.HAlignCenter);
            int ColItemId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Employee Category", 13, ExcelHAlign.HAlignCenter);
            int ColSoId = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Emp Addition Deduction Id", 13, ExcelHAlign.HAlignCenter);
            int ColMaterial = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Salary Head", 13, ExcelHAlign.HAlignCenter);
            int ColArticle = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Amount", 13, ExcelHAlign.HAlignCenter);
            int ColProductCode = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Month", 15, ExcelHAlign.HAlignCenter);
            int ColPONo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Month Day", 13, ExcelHAlign.HAlignCenter);
            int ColLotNo = COL;
            COL++;

            report.SetHeaderText(ref sheet, ROW, COL, "Year No", 13, ExcelHAlign.HAlignCenter);
            int ColSoPoNo = COL;
            COL++;

            //Trial
            //Trial for builds
            ROW++;
            endCol = COL;
            #endregion Headers


            var startRow = 0;
            var endRow = 0;
            int RowIndex = ROW;
            startRow = ROW;


            for (int i = 0; i < data.Count; i++)
            {
                sheet[ROW, ColRow].Text = (i + 1).ToString();
                sheet[ROW, ColPLID].Text = data[i]["Id"].ToString();
                sheet[ROW, ColMONo].Text = data[i]["EmpSystemId"].ToString();
                sheet[ROW, ColPlanQty].Text = data[i]["Plant"].ToString();
                sheet[ROW, ColItemId].Text = data[i]["LegalDesignation"].ToString();
                sheet[ROW, ColSoId].Text = data[i]["EmpType"].ToString();
                sheet[ROW, ColMaterial].Text = data[i]["EmpAdditionDeductionHeaderId"].ToString();
                sheet[ROW, ColArticle].Text = data[i]["SalaryHead"].ToString();
                sheet[ROW, ColProductCode].Text = data[i]["Amount"].ToString();
                sheet[ROW, ColPONo].Text = data[i]["Month"].ToString();
                sheet[ROW, ColLotNo].Text = data[i]["MonthDay"].ToString();
                sheet[ROW, ColSoPoNo].Text = data[i]["YearNo"].ToString();



                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);

                ROW++;

            }

            ROW++;


            endRow = ROW - 1;
            endRow = ROW - 1;

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            sheet.UsedRange.WrapText = true;
            sheet.UsedRange.CellStyle.Font.Size = 8;
            ReportUtility reportUtility = new ReportUtility();
            reportUtility.PlantHeader(ref sheet, endCol, "Saved Employee Addition/Deduction Report", identity.PlantId);
            reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
            return workbook;
        }



        private void SetHeaderTextTop(ref IWorksheet sheet, int row, int col, string txt, int width, ExcelHAlign al)
        {
            sheet.Range[row, col].Text = txt;
            sheet.Range[row, col].ColumnWidth = width;
            sheet.Range[row, col].CellStyle.Font.Bold = true;
            sheet.Range[row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;
            sheet.Range[row, col].HorizontalAlignment = al;

        }
        private void SetTextTop(ref IWorksheet sheet, int row, int col, string txt, int width, ExcelHAlign al)
        {

            sheet.Range[row, col - 1, row, col].Text = txt;
            sheet.Range[row, col - 1, row, col].Merge();
            sheet.Range[row, col - 1, row, col].ColumnWidth = width;
            sheet.Range[row, col - 1, row, col].HorizontalAlignment = al;
            sheet.Range[row, col - 1, row, col].VerticalAlignment = ExcelVAlign.VAlignCenter;

        }
    }
}