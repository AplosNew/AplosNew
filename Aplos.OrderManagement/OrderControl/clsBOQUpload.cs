using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Enums;
using Library.Service.Helpers;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.OrderManagement.OrderControl
{
    public class clsBOQUpload
    {
        ISqlRepository _sqlRepository;
        public clsBOQUpload()
        {
            _sqlRepository = new SqlRepository();
        }

        #region --Sample File--
        public IWorkbook GetSampleFile(string Name,string CompanyId,string plantId)
        {
            #region declare
            //clsReport objRpt = null;
            ReportUtility oru = new ReportUtility();
            //clsStaticInfo objStatic = null;
            //objStatic = new clsStaticInfo();
            string OTConsiderOn = string.Empty;
            string FactoryName = "";
            string CmpName = "";
            DataSet dsFactory = null;
            clsReport objRpt = null;
            DataSet dsCmp = null;
            objRpt = new clsReport();
            string FactoryAddress = string.Empty;
            #endregion
            try
            {
                ReportUtility ru = new ReportUtility();
                
                ExcelEngine excelEngine = null;
                IApplication application = null;
                var workbook = oru.GetWorkbook(ref excelEngine, 1);
                workbook.Version = ExcelVersion.Excel2013;
               
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);

                int xlsRow = 1, xlsCol = 1;
                int endXlsCol = 1;
                var iId = 0;
                var iSequence = 0;
                var iMasterOrderItemId = 0;
                var iMaterialMasterId = 0;
                var iArticleId = 0;
                var iCostingItemId = 0;
                var iUoMId = 0;
                var iDescription = 0;
                var iRemarks = 0;
                var iNetConsumptionPerUnit = 0;
                var iValueLossPercentage = 0;
                var iGrossConsumption = 0;
                var iMaterialCostPerUnit = 0;
                var iProcessId = 0;
                var iResponsiblePersonId = 0;
                var iIsOutSource = 0;
                var iJobWorkType = 0;
                var iEntityIdWithinCompany = 0;
                var iEntityIdWithinGroup = 0;
                var iVendorId = 0;
                var iProcessGroup = 0;

                

                #region --Sample--
                IWorksheet sheet1 = null;
                sheet1 = workbook.Worksheets[0];
                xlsRow = 1;
                
                #region ------------------Column Header------------------
                iId = xlsCol;
                sheet1.Range[xlsRow, iId].Text = "Id";
                sheet1.Range[xlsRow, iId].ColumnWidth = 18;

                xlsCol += 1;
                iSequence = xlsCol;
                sheet1.Range[xlsRow, iSequence].Text = "Sequence";
                sheet1.Range[xlsRow, iSequence].ColumnWidth = 18;

                xlsCol += 1;
                iMasterOrderItemId = xlsCol;
                sheet1.Range[xlsRow, iMasterOrderItemId].Text = "MasterOrderItemId";
                sheet1.Range[xlsRow, iMasterOrderItemId].ColumnWidth = 18;

                xlsCol += 1;
                iMaterialMasterId = xlsCol;
                sheet1.Range[xlsRow, iMaterialMasterId].Text = "MaterialMasterId";
                sheet1.Range[xlsRow, iMaterialMasterId].ColumnWidth = 18;

                xlsCol += 1;
                iArticleId = xlsCol;
                sheet1.Range[xlsRow, iArticleId].Text = "ArticleId";
                sheet1.Range[xlsRow, iArticleId].ColumnWidth = 18;                


                xlsCol += 1;
                iCostingItemId = xlsCol;
                sheet1.Range[xlsRow, iCostingItemId].Text = "CostingItemId";
                sheet1.Range[xlsRow, iCostingItemId].ColumnWidth = 36;

                xlsCol += 1;
                iUoMId = xlsCol;
                sheet1.Range[xlsRow, iUoMId].Text = "UoMId";
                sheet1.Range[xlsRow, iUoMId].ColumnWidth = 20;

                xlsCol += 1;
                iDescription = xlsCol;
                sheet1.Range[xlsRow, iDescription].Text = "Description";
                sheet1.Range[xlsRow, iDescription].ColumnWidth = 20;

                xlsCol += 1;
                iRemarks = xlsCol;
                sheet1.Range[xlsRow, iRemarks].Text = "Remarks";
                sheet1.Range[xlsRow, iRemarks].ColumnWidth = 20;

                xlsCol += 1;
                iNetConsumptionPerUnit = xlsCol;
                sheet1.Range[xlsRow, iNetConsumptionPerUnit].Text = "NetConsumptionPerUnit";
                sheet1.Range[xlsRow, iNetConsumptionPerUnit].ColumnWidth = 20;

                xlsCol += 1;
                iValueLossPercentage = xlsCol;
                sheet1.Range[xlsRow, iValueLossPercentage].Text = "ValueLossPercentage";
                sheet1.Range[xlsRow, iValueLossPercentage].ColumnWidth = 20;

                xlsCol += 1;
                iGrossConsumption = xlsCol;
                sheet1.Range[xlsRow, iGrossConsumption].Text = "GrossConsumption";
                sheet1.Range[xlsRow, iGrossConsumption].ColumnWidth = 20;
                xlsCol += 1;

                iMaterialCostPerUnit = xlsCol;
                sheet1.Range[xlsRow, iMaterialCostPerUnit].Text = "MaterialCostPerUnit";
                sheet1.Range[xlsRow, iMaterialCostPerUnit].ColumnWidth = 20;
                xlsCol += 1;

                iProcessId = xlsCol;
                sheet1.Range[xlsRow, iProcessId].Text = "ProcessId";
                sheet1.Range[xlsRow, iProcessId].ColumnWidth = 20;
                xlsCol += 1;

                iResponsiblePersonId = xlsCol;
                sheet1.Range[xlsRow, iResponsiblePersonId].Text = "ResponsiblePersonId";
                sheet1.Range[xlsRow, iResponsiblePersonId].ColumnWidth = 20;
                xlsCol += 1;

                iIsOutSource = xlsCol;
                sheet1.Range[xlsRow, iIsOutSource].Text = "IsOutSource";
                sheet1.Range[xlsRow, iIsOutSource].ColumnWidth = 20;
                xlsCol += 1;

                iJobWorkType = xlsCol;
                sheet1.Range[xlsRow, iJobWorkType].Text = "JobWorkType";
                sheet1.Range[xlsRow, iJobWorkType].ColumnWidth = 20;
                xlsCol += 1;

                iEntityIdWithinCompany = xlsCol;
                sheet1.Range[xlsRow, iEntityIdWithinCompany].Text = "EntityIdWithinCompany";
                sheet1.Range[xlsRow, iEntityIdWithinCompany].ColumnWidth = 20;
                xlsCol += 1;

                iEntityIdWithinGroup = xlsCol;
                sheet1.Range[xlsRow, iEntityIdWithinGroup].Text = "EntityIdWithinGroup";
                sheet1.Range[xlsRow, iEntityIdWithinGroup].ColumnWidth = 20;
                xlsCol += 1;

                iVendorId = xlsCol;
                sheet1.Range[xlsRow, iVendorId].Text = "VendorId";
                sheet1.Range[xlsRow, iVendorId].ColumnWidth = 20;
                xlsCol += 1;

                iProcessGroup = xlsCol;
                sheet1.Range[xlsRow, iProcessGroup].Text = "ProcessGroup";
                sheet1.Range[xlsRow, iProcessGroup].ColumnWidth = 20;
                

                endXlsCol = xlsCol;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderInside(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].BorderAround(ExcelLineStyle.Hair);
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].WrapText = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Font.Bold = true;
                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].RowHeight = 23;

                sheet1.Range[xlsRow, 1, xlsRow, endXlsCol].CellStyle.Interior.Color = System.Drawing.Color.LightYellow;

                xlsRow++;

                #endregion ------------------Column Header------------------

                #region data in column



                int ROW = 1;
                int endCol = 1;
                int COL = 1;
                var startRow = 0;

                int RowIndex = ROW;
                startRow = ROW;
                ROW++;
                //for (int i = 0; i < data.Rows.Count; i++)
                //{
                //    sheet1[ROW, isl].Text = data.Rows[i]["SystemId"].ToString();
                //    sheet1[ROW, iEmployeeCode].Text = data.Rows[i]["EmployeeCode"].ToString();
                //    sheet1[ROW, iWorkDate].Text = data.Rows[i]["WorkDate"].ToString();
                //    sheet1[ROW, iDayStatus].Text = data.Rows[i]["DayStatus"].ToString();
                //    sheet1[ROW, iShiftId].Text = data.Rows[i]["ShiftId"].ToString();
                //    sheet1[ROW, iShiftName].Text = data.Rows[i]["ShiftName"].ToString();
                //    sheet1[ROW, iShiftInTime].Text = data.Rows[i]["ShiftInTime"].ToString();
                //    sheet1[ROW, iShiftOutTime].Text = data.Rows[i]["ShiftOutTime"].ToString();

                //    //sheet1[ROW, iInTime].Text = "";
                //    sheet1.Range[ROW, iInDate].NumberFormat = "@";
                //    sheet1.Range[ROW, iInDate].Text = "";
                //    sheet1.Range[ROW, iInTime].NumberFormat = "HH:mm";
                //    sheet1.Range[ROW, iOutDate].NumberFormat = "@";
                //    sheet1.Range[ROW, iOutDate].Text = "";
                //    sheet1.Range[ROW, iOutTime].NumberFormat = "HH:mm";
                //    ROW++;
                //}

                #endregion
                
                #region UsedRange Alignment

                sheet1.UsedRange.WrapText = true;
                sheet1.UsedRange.CellStyle.Font.Size = 10;
                sheet1.Range["A1"].CellStyle.Font.Size = 10;
                sheet1.Range["A2"].CellStyle.Font.Size = 10;
                sheet1.UsedRange.IgnoreErrorOptions = ExcelIgnoreError.All;

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
                sheet1.Name = "Sheet1";
                #endregion Page Setup


                #endregion

                return workbook;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

        #region --File Read--

        public List<BOQData> ReadData(string plantid, string path)
        {
            List<BOQData> data = null;
            //string path = "";
            DataSet dsExcel = null;
            try
            {
                data = new List<BOQData>();
                //SaveFile(out path);
                ReadFile(path, out dsExcel);
                Validation(dsExcel, plantid);
                data = dsExcel.Tables[0].ToList<BOQData>();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void ReadFile(string path, out DataSet dsExcel)
        {
            FileInfo docFile;
            dsExcel = null;
            try
            {
                ExcelEngine excelEngine = null;
                IApplication application = null;
                IWorkbook workbook = null;
                excelEngine = new ExcelEngine();
                application = excelEngine.Excel;
                workbook = excelEngine.Excel.Workbooks.Open(path);
                DataTable dt = workbook.Worksheets[0].ExportDataTable(workbook.Worksheets[0].UsedRange, ExcelExportDataTableOptions.ColumnNames);
                dsExcel = new DataSet();
                dsExcel.Tables.Add(dt);
                docFile = new FileInfo(path);
                if (docFile.Exists)
                {
                    //exception += "\r\nTrying to delete";
                    docFile.Delete();
                }
            }
            catch (Exception ex)
            {
                docFile = new FileInfo(path);
                if (docFile.Exists)
                {
                    docFile.Delete();
                }
                throw (ex);
            }
        }
        public void Validation(DataSet dsExcel, string plantid)
        {            
            try
            {
                if (dsExcel.Tables[0].Rows.Count > 0)
                {
                    if (false)
                    {

                    }

                }
                else
                {
                    throw new Exception("Please Select File");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion

        #region --File Save--

        public void SaveMaster(List<BOQData> EmpList)
        {

            try
            {

                DataSet dsMonth;

                GetTaxOB(EmpList, out dsMonth);

                _TaxOB(ref dsMonth, EmpList);


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMonth);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void GetTaxOB(List<BOQData> EmpList, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            var _Id = string.Empty;
            try
            {
                foreach (var item in EmpList)
                {
                    if (item.Id != null)
                    {

                        if (_Id == "")
                        {
                            _Id = "'" + item.Id.Replace(",", "','") + "'";
                        }
                        else
                        {
                            _Id += ",'" + item.Id.Replace(",", "','") + "'";
                        }
                    }
                    //_Id = "'',"'"+item.Id+"'
                }
                if (_Id != "")
                {
                    strSQL = "SELECT * FROM dbo.QuickBOQ WHERE Id in (" + _Id + ")";
                }
                else
                {
                    strSQL = "SELECT * FROM dbo.QuickBOQ ";
                }

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

        void _TaxOB(ref DataSet dsSaveBonusMonths, List<BOQData> List)
        {

            DataView dvMSave = null;
            DataTable dtMSave = null;
            DataRow drMSave = null;
            try
            {
                string seed_detail = string.Empty;
                bplib.clsGenID objGenID = null;
                objGenID = new bplib.clsGenID();
                objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "BOQ", out seed_detail);
                dtMSave = dsSaveBonusMonths.Tables[0];
                int count = 0;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                foreach (var item in List)
                {
                    dvMSave = new DataView();
                    dvMSave.Table = dtMSave;
                    dvMSave.RowFilter = "Id ='" + item.Id + "' ";
                    if (dvMSave.Count == 0)
                    {
                        count++;
                        //string pk = "BOQ_" + seed_detail + "_" + count;
                        drMSave = dtMSave.NewRow();
                        drMSave["Id"] = item.Id;
                        drMSave["Sequence"] = item.Sequence;
                        drMSave["MasterOrderItemId"] = item.MasterOrderItemId;
                        drMSave["MaterialMasterId"] = item.MaterialMasterId;
                        drMSave["ArticleId"] = item.ArticleId;
                        drMSave["UoMId"] = item.UoMId;
                        drMSave["Description"] = item.Description;
                        drMSave["Remarks"] = item.Remarks;
                        drMSave["NetConsumptionPerUnit"] = item.NetConsumptionPerUnit;
                        drMSave["ValueLossPercentage"] = item.ValueLossPercentage;
                        drMSave["GrossConsumption"] = item.GrossConsumption;
                        drMSave["MaterialCostPerUnit"] = item.MaterialCostPerUnit;
                        drMSave["ProcessId"] = item.ProcessId;
                        drMSave["ResponsiblePersonId"] = item.ResponsiblePersonId;
                        drMSave["IsOutSource"] =bplib.clsWebLib.GetBoolData(item.IsOutSource.ToString());
                        drMSave["JobWorkType"] = item.JobWorkType;

                        if (item.JobWorkType== EnumJobWorkTypeList.EntityWithinCompany.ToString())
                        {
                            drMSave["EntityIdWithinCompany"] = item.EntityIdWithinCompany;
                            drMSave["EntityIdWithinGroup"] = DBNull.Value;
                            drMSave["VendorId"] = DBNull.Value;
                        }

                        if (item.JobWorkType == EnumJobWorkTypeList.EntityWithinGroup.ToString())
                        {
                            drMSave["EntityIdWithinGroup"] = item.EntityIdWithinGroup;
                            drMSave["EntityIdWithinCompany"] = DBNull.Value; ;
                            drMSave["VendorId"] = DBNull.Value;
                        }

                        if (item.JobWorkType == EnumJobWorkTypeList.Vendor.ToString())
                        {
                            drMSave["VendorId"] = item.VendorId;
                            drMSave["EntityIdWithinGroup"] = DBNull.Value;
                            drMSave["EntityIdWithinCompany"] = DBNull.Value; 
                        }

                        
                        drMSave["ProcessGroup"] = item.ProcessGroup;
                        drMSave["AddedBy"] = identity.Name;
                        drMSave["AddedDate"] = DateTime.Now;
                        drMSave["AddedFromIP"] = identity.IPAddress;

                        drMSave["UpdatedBy"] = identity.Name;
                        drMSave["UpdatedDate"] = DateTime.Now;
                        drMSave["UpdatedFromIP"] = identity.IPAddress;
                        dtMSave.Rows.Add(drMSave);
                    }
                    //else
                    //{
                    //    drMSave = dvMSave[0].Row;
                    //    drMSave.BeginEdit();
                    //    drMSave["TaxYearId"] = item.TaxYearId;
                    //    drMSave["TaxTypeId"] = item.TaxTypeId;
                    //    drMSave["OpeningTaxableIncomeEarned"] = item.OpeningTaxableIncomeEarned;
                    //    drMSave["OpeningTaxPaid"] = item.OpeningTaxPaid;

                    //    drMSave["UpdatedBy"] = identity.Name;
                    //    drMSave["DateUpdated"] = DateTime.Now;
                    //    drMSave.EndEdit();
                    //}
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion

    }
}


public class BOQData
{
    public string Id { get; set; } = "";
    public string Sequence { get; set; }
    public string MasterOrderItemId { get; set; } = "";
    public string MaterialMasterId { get; set; } = "";
    public string ArticleId { get; set; } = "";
    public string CostingItemId { get; set; } = "";
    public string UoMId { get; set; } = "";
    public string Description { get; set; } = "";
    public string Remarks { get; set; } = "";
    public string NetConsumptionPerUnit { get; set; } 
    public string ValueLossPercentage { get; set; } 
    public string GrossConsumption { get; set; } 
    public string MaterialCostPerUnit { get; set; } 
    public string ProcessId { get; set; } = "";
    public string ResponsiblePersonId { get; set; } = "";
    public string IsOutSource { get; set; }
    public string JobWorkType { get; set; } = "";
    public string EntityIdWithinCompany { get; set; } = "";
    public string EntityIdWithinGroup { get; set; } = "";
    public string VendorId { get; set; } = "";
    public string ProcessGroup { get; set; } = "";     
}