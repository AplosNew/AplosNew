using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Library.Service.HumanResources.Shift
{
   public class clsTemplateReadShiftAssignment
    {//
        public List<EmployeeShiftUploadTemplateVM> ReadData(string plantid, string path)
        {
            List<EmployeeShiftUploadTemplateVM> data = null;
            //string path = "";
            DataSet dsExcel = null;
            try
            {
                data = new List<EmployeeShiftUploadTemplateVM>();
                //SaveFile(out path);
                ReadFile(path, out dsExcel);
                Validation(dsExcel, plantid);
                data = dsExcel.Tables[0].ToList<EmployeeShiftUploadTemplateVM>();
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
        public void Validation(DataSet dsExcel,string plantid)
        {
            //DataSet dsEmpInfo = null;
            //DataTable dtEmpInfo = null;
            //DataView dvEmpInfo = null;
            try
            {
                //GetEmployeeInfo(plantid,out dsEmpInfo);
                //dtEmpInfo = dsEmpInfo.Tables[0];
                //dvEmpInfo = new DataView();



                if (dsExcel.Tables[0].Rows.Count > 0)
                {
                    //for (int i = 0; i < dsExcel.Tables[0].Rows.Count; i++)
                    //{
                    //    if(string.IsNullOrEmpty(dsExcel.Tables[0].Rows[i]["EmployeeCode"].ToString()))
                    //        {
                    //        DataRow dr = dsExcel.Tables[0].Rows[i];
                    //        dr.BeginEdit();
                    //        dr.Delete();
                    //        dr.EndEdit();
                    //        //dr.AcceptChanges();
                    //    }
                    //}
                    if (false)
                    {
                        for (int i = 0; i < dsExcel.Tables[0].Rows.Count; i++)
                        {
                            string strTempPDate = "";
                            string strTempPTimee = "";
                            string strTempPType = "";
                            //string strTempDefineAmt = "0.0";
                            string _empEmpSystemId = Regex.Replace(dsExcel.Tables[0].Rows[i][0].ToString().Trim(), @"\s", "");
                            //string _empCode = dsExcel.Tables[0].Rows[i]["EmployeeCode"].ToString().Trim();

                            strTempPDate = dsExcel.Tables[0].Rows[i][1].ToString().Trim();
                            strTempPTimee = dsExcel.Tables[0].Rows[i][2].ToString().Trim();
                            strTempPType = dsExcel.Tables[0].Rows[i][3].ToString().Trim().ToUpper();
                            //strTempEntryAmt = dsExcel.Tables[0].Rows[i]["Amount"].ToString().Trim();
                            //strTempEntryAmt = dsExcel.Tables[0].Rows[i]["F2"].ToString().Trim();


                            //DateTime dtPDate;

                            //bool isValidDate = DateTime.TryParseExact(
                            //    strTempPDate,
                            //    "dd-MMM-yyyy",
                            //    CultureInfo.InvariantCulture,
                            //    DateTimeStyles.None,
                            //    out dtPDate);
                            //DateTime dtPTime;

                            //bool isValidDateTime = DateTime.TryParseExact(
                            //    strTempPTimee,
                            //    "dd-MMM-yyyy hh:mm:ss",
                            //    CultureInfo.InvariantCulture,
                            //    DateTimeStyles.None,
                            //    out dtPTime);

                            //if (_empEmpSystemId.Trim().Length > 0 && strTempPType.Trim().Length > 0)
                            //{
                            //    dvEmpInfo.Table = dtEmpInfo;
                            //    dvEmpInfo.RowFilter = "SystemId = " + _empEmpSystemId;
                            //    //if (dvEmpInfo.Count == 1)


                                

                                
                            //}//blank checking

                        }//for

                    }

                }
                else
                {
                    throw new Exception("Please Select File");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public void GetEmployeeInfo(string plantid,out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" SELECT SystemId, EmployeeCode, PlantId, DOJ, DOS  FROM EmployeeInformation WHERE PlantId = '" + plantid + @"'";

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
        }//
    }
}
