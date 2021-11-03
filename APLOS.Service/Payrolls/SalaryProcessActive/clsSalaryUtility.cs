using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;


public class clsSalaryUtility
{
    public void FractionCalculation(string sRoundOption, bool IntegerInDisb, bool IsDecimalInDisb, int DecimalNo, string sValue, out string sResultValue)
    {
        sResultValue = "0";
        bool IsNegative = false;
        try
        {
            if (Convert.ToDouble(sValue) < 0)
            {
                IsNegative = true;
                sValue = sValue.Replace("-", "");
            }

            if (IntegerInDisb == true)
            {
                if (sRoundOption.ToUpper().Trim() == "ROUND")
                {
                    sResultValue = Convert.ToInt32(Math.Round(Convert.ToDouble(sValue) + 0.0000001)).ToString();
                }
                else if (sRoundOption.ToUpper().Trim() == "ROUND UP")
                {
                    sResultValue = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(sValue))).ToString();
                }
                else if (sRoundOption.ToUpper().Trim() == "ROUND DOWN")
                {
                    sResultValue = Convert.ToInt32(Math.Floor(Convert.ToDouble(sValue))).ToString();
                }
            }
            else if (IsDecimalInDisb == true)
            {
                if (sRoundOption.ToUpper().Trim() == "ROUND")
                {
                    sResultValue = Math.Round(Convert.ToDouble(sValue), DecimalNo).ToString();
                }
                else if (sRoundOption.ToUpper().Trim() == "ROUND UP")
                {
                    double multiplier = Math.Pow(10, Convert.ToDouble(DecimalNo));
                    sResultValue = (Math.Ceiling(Convert.ToDouble(sValue) * multiplier) / multiplier).ToString();
                }
                else if (sRoundOption.ToUpper().Trim() == "ROUND DOWN")
                {
                    double multiplier = Math.Pow(10, Convert.ToDouble(DecimalNo));
                    sResultValue = (Math.Floor(Convert.ToDouble(sValue) * multiplier) / multiplier).ToString();
                }
            }
            else
            {
                sResultValue = Convert.ToInt32(Math.Round(Convert.ToDouble(sValue))).ToString();
            }

            if (IsNegative)
            {
                sResultValue = "-" + sResultValue;
                IsNegative = false;
            }
        }
        catch (Exception ex)
        {
            throw (ex);
        }
        finally
        {
        }
    }
    public void dtIdList(string strIdCollection, out DataSet dsLocal)
    {
        dsLocal = new DataSet();

        strIdCollection = strIdCollection.Replace("'", "");
        string[] strIdCol = strIdCollection.Split(',');

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
    }//End Function
    public void ReLoadFormulaWithValueSalaryProc(string sEmpSystemID, FunctionPara para, string strFormulaID,
        out string sFormulaValue, bool bEarning, List<SPvalueHeadWise> dtValue, List<SPSalaryHead> dicSlrHd)
    {
        DataSet dsLocal = null;
        //DataView dvLocal = null;
        //DataView dvSlrHd = null;
        string strTemp = "";

        try
        {
            //dtValue
            //List<SPvalueHeadWise> list = new List<SPvalueHeadWise>();
            //list= dtValue.ToList<SPvalueHeadWise>();

            dsLocal = new DataSet();

            string strFormulaIDTemp = strFormulaID.Trim();
            string sLocalCurrencyID = para.lblLocalCurrencyID;
            string sForeignCurRate = para.lblLocalCurRate;

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
                    //dvLocal = new DataView();
                    //dvLocal.Table = dtValue;

                    var dtv = dtValue.FindAll(x => x.SalaryHeadID == strTemp.Trim() && x.EmpSystemID == sEmpSystemID);
                    // dvLocal.RowFilter = "SalaryHeadID = '" + strTemp.Trim() + "' AND EmpSystemID = '" + sEmpSystemID + "'";
                    if (dtv.Count() > 0)
                    {
                        if (bEarning == false)
                        {
                            if (dtv[0].EntryCurrencyID == para.lblLocalCurrencyID.Trim())
                            {
                                strTemp = dtv[0].EntryAmount;
                                strTemp = GetAbsValue(strTemp);
                            }
                            else
                            {
                                strTemp = (Convert.ToDecimal(dtv[0].EntryAmount) * Convert.ToDecimal(para.txtForeignCurRate.Trim())).ToString();
                                strTemp = GetAbsValue(strTemp);
                            }
                        }
                        else
                        {
                            decimal decAmount = Convert.ToDecimal(Convert.ToDecimal(dtv[0].EarningAmount).ToString("0.00"));

                            if (decAmount == 0)
                            { decAmount = Convert.ToDecimal(Convert.ToDecimal(dtv[0].EntryAmount).ToString("0.00")); }

                            if (dtv[0].EarningCurrencyID == para.lblLocalCurrencyID.Trim())
                            {
                                strTemp = Convert.ToDecimal(dtv[0].EarningAmount).ToString("0.00");
                                strTemp = GetAbsValue(strTemp);
                            }
                            else
                            {
                                strTemp = (decAmount * Convert.ToDecimal(para.txtForeignCurRate.Trim())).ToString();
                                strTemp = GetAbsValue(strTemp);
                            }
                        }
                    }
                    else
                    {
                        var dicsh = dicSlrHd.FindAll(x => x.SalaryHeadID == strTemp.Trim());
                        if (dicsh.Count() > 0)
                        {
                            strTemp = "0.00";
                        }
                        // var dvSPChd_dic = dicProcChild.FindAll(x => x.EmpInfoSystemID == dicLocal_Sub[i].EmpInfoSystemID && x.SalaryHeadID == dicLocal_Sub[i].SalaryHeadID && x.SlrProcMstSystemID == para.lblSalaryProcSystemId.Trim());

                        //dicsal
                        //dvSlrHd = new DataView();
                        //dvSlrHd.Table = dtSlrHd;
                        //dvSlrHd.RowFilter = "SalaryHeadID = '" + strTemp.Trim() + "'";
                        //if (dvSlrHd.Count == 1)
                        //{
                        //    strTemp = "0.00";
                        //}
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
    public void ReLoadFormulaWithValueSalaryProc(string sEmpSystemID, FunctionPara para, string strFormulaID, out string sFormulaValue, bool bEarning, ref DataTable dtValue, ref DataTable dtSlrHd)
    {
        DataSet dsLocal = null;
        DataView dvLocal = null;
        DataView dvSlrHd = null;
        string strTemp = "";

        try
        {
            dsLocal = new DataSet();

            string strFormulaIDTemp = strFormulaID.Trim();
            string sLocalCurrencyID = para.lblLocalCurrencyID;
            string sForeignCurRate = para.lblLocalCurRate;

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
                    dvLocal = new DataView();
                    dvLocal.Table = dtValue;

                    dvLocal.RowFilter = "SalaryHeadID = '" + strTemp.Trim() + "' AND EmpSystemID = '" + sEmpSystemID + "'";
                    if (dvLocal.Count == 1)
                    {
                        if (bEarning == false)
                        {
                            if (dvLocal[0]["EntryCurrencyID"].ToString().Trim() == para.lblLocalCurrencyID.Trim())
                            {
                                strTemp = dvLocal[0]["EntryAmount"].ToString().Trim();
                            }
                            else
                            {
                                strTemp = (Convert.ToDecimal(dvLocal[0]["EntryAmount"].ToString().Trim()) * Convert.ToDecimal(para.txtForeignCurRate.Trim())).ToString();
                            }
                        }
                        else
                        {
                            decimal decAmount = Convert.ToDecimal(Convert.ToDecimal(dvLocal[0]["EarningAmount"].ToString().Trim()).ToString("0.00"));

                            if (decAmount == 0)
                            { decAmount = Convert.ToDecimal(Convert.ToDecimal(dvLocal[0]["EntryAmount"].ToString().Trim()).ToString("0.00")); }

                            if (dvLocal[0]["EarningCurrencyID"].ToString().Trim() == para.lblLocalCurrencyID.Trim())
                            {
                                strTemp = Convert.ToDecimal(dvLocal[0]["EarningAmount"].ToString().Trim()).ToString("0.00");
                            }
                            else
                            {
                                strTemp = (decAmount * Convert.ToDecimal(para.txtForeignCurRate.Trim())).ToString();
                            }
                        }
                    }
                    else
                    {
                        dvSlrHd = new DataView();
                        dvSlrHd.Table = dtSlrHd;
                        dvSlrHd.RowFilter = "SalaryHeadID = '" + strTemp.Trim() + "'";
                        if (dvSlrHd.Count == 1)
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
    public void ReLoadFormulaWithValue(string strFormulaID, ref DataTable dtValue, string lblLocalCurrencyID, string txtForeignCurRate, out string lblFormulaValue, ref DataTable dtSlrHd)
    {
        DataSet dsLocal = null;
        DataView dvLocal = null;
        DataView dvSlrHd = null;

        string strTemp = "";

        try
        {
            dsLocal = new DataSet();

            string strFormulaIDTemp = strFormulaID.Trim();

            lblFormulaValue = "";

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
                if (strTemp.Trim() == "+" || strTemp.Trim() == "-" || strTemp.Trim() == "*" || strTemp.Trim() == "/" || strTemp.Trim() == "(" || strTemp.Trim() == ")")
                {
                    strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                }
                else
                {
                    dvLocal = new DataView();
                    dvLocal.Table = dtValue;

                    dvLocal.RowFilter = "SalaryHeadID = '" + strTemp.Trim() + "'";
                    if (dvLocal.Count > 0)
                    {
                        if (dvLocal[0]["EntryCurrencyID"].ToString().Trim() == lblLocalCurrencyID)
                        {
                            strTemp = dvLocal[0]["Amount"].ToString().Trim();
                        }
                        else
                        {
                            strTemp = (Convert.ToDecimal(dvLocal[0]["Amount"].ToString().Trim()) * Convert.ToDecimal(txtForeignCurRate)).ToString();
                        }
                    }
                    else
                    {
                        dvSlrHd = new DataView();
                        dvSlrHd.Table = dtSlrHd;
                        dvSlrHd.RowFilter = "SalaryHeadID = '" + strTemp.Trim() + "'";
                        if (dvSlrHd.Count == 1)
                        {
                            strTemp = "0.00";
                        }
                    }
                }

                lblFormulaValue += strTemp.Trim();
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
    public static double Evaluate(string expression)
    {
        try
        {
            //That is some code instruction, is'nt it?
            return (double)new System.Xml.XPath.XPathDocument
            (new StringReader("<r/>")).CreateNavigator().Evaluate
            (string.Format("number({0})", new
            System.Text.RegularExpressions.Regex(@"([\+\-\*])")
            .Replace(expression, " ${1} ")
            .Replace("/", " div ")
            .Replace("%", " mod ")));
        }
        catch (Exception ex)
        {
            throw ex;
        }
        finally
        {
        }
    }//End Function 
    public void ReLoadFormulaWithValueSalaryProc(string sEmpSystemID, FunctionPara para, string strFormulaID,
       out string sFormulaValue, out string StructureValue, bool bEarning, List<SPvalueHeadWise> dtValue, List<SPSalaryHead> dicSlrHd)
    {
        DataSet dsLocal = null;
        //DataView dvLocal = null;
        //DataView dvSlrHd = null;
        string strTemp = "";
        StructureValue = string.Empty;
        string Structure_temp = string.Empty;

        try
        {
            //dtValue
            //List<SPvalueHeadWise> list = new List<SPvalueHeadWise>();
            //list= dtValue.ToList<SPvalueHeadWise>();

            dsLocal = new DataSet();

            string strFormulaIDTemp = strFormulaID.Trim();
            string sLocalCurrencyID = para.lblLocalCurrencyID;
            string sForeignCurRate = para.lblLocalCurRate;

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
                bool IsForStructure = false;
                strTemp = "";

                strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                if (strTemp.Trim() == "+" || strTemp.Trim() == "-" || strTemp.Trim() == "*" || strTemp.Trim() == "/" || strTemp.Trim() == ">" || strTemp.Trim() == "<" || strTemp.Trim() == "(" || strTemp.Trim() == ")")
                {
                    strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                }
                else
                {
                    //dvLocal = new DataView();
                    //dvLocal.Table = dtValue;

                    var dtv = dtValue.FindAll(x => x.SalaryHeadID == strTemp.Trim() && x.EmpSystemID == sEmpSystemID);
                    // dvLocal.RowFilter = "SalaryHeadID = '" + strTemp.Trim() + "' AND EmpSystemID = '" + sEmpSystemID + "'";
                    if (dtv.Count() > 0)
                    {
                        if (bEarning == false)
                        {
                            if (dtv[0].EntryCurrencyID == para.lblLocalCurrencyID.Trim())
                            {
                                strTemp = dtv[0].EntryAmount;
                                strTemp = GetAbsValue(strTemp);
                            }
                            else
                            {
                                strTemp = (Convert.ToDecimal(dtv[0].EntryAmount) * Convert.ToDecimal(para.txtForeignCurRate.Trim())).ToString();
                                strTemp = GetAbsValue(strTemp);
                            }
                        }
                        else
                        {
                            decimal decAmount = Convert.ToDecimal(Convert.ToDecimal(dtv[0].EarningAmount).ToString("0.00"));
                            var ss = Convert.ToDecimal(Convert.ToDecimal(dtv[0].EntryAmount).ToString("0.00"));
                            IsForStructure = true;

                            if (decAmount == 0)
                            { decAmount = Convert.ToDecimal(Convert.ToDecimal(dtv[0].EntryAmount).ToString("0.00")); }

                            if (dtv[0].EarningCurrencyID == para.lblLocalCurrencyID.Trim())
                            {
                                strTemp = Convert.ToDecimal(dtv[0].EarningAmount).ToString("0.00");
                                strTemp = GetAbsValue(strTemp);

                                Structure_temp = Convert.ToDecimal(ss).ToString("0.00");
                                Structure_temp = GetAbsValue(Structure_temp);
                            }
                            else
                            {
                                strTemp = (decAmount * Convert.ToDecimal(para.txtForeignCurRate.Trim())).ToString();
                                strTemp = GetAbsValue(strTemp);

                                Structure_temp = (ss * Convert.ToDecimal(para.txtForeignCurRate.Trim())).ToString();
                                Structure_temp = GetAbsValue(Structure_temp);
                            }
                        }
                    }
                    else
                    {
                        var dicsh = dicSlrHd.FindAll(x => x.SalaryHeadID == strTemp.Trim());
                        if (dicsh.Count() > 0)
                        {
                            strTemp = "0.00";
                        }
                        // var dvSPChd_dic = dicProcChild.FindAll(x => x.EmpInfoSystemID == dicLocal_Sub[i].EmpInfoSystemID && x.SalaryHeadID == dicLocal_Sub[i].SalaryHeadID && x.SlrProcMstSystemID == para.lblSalaryProcSystemId.Trim());

                        //dicsal
                        //dvSlrHd = new DataView();
                        //dvSlrHd.Table = dtSlrHd;
                        //dvSlrHd.RowFilter = "SalaryHeadID = '" + strTemp.Trim() + "'";
                        //if (dvSlrHd.Count == 1)
                        //{
                        //    strTemp = "0.00";
                        //}
                    }
                }


                sFormulaValue += strTemp.Trim();
                if (IsForStructure)
                {
                    StructureValue += Structure_temp.Trim();
                }
                else
                {
                    StructureValue += strTemp.Trim();
                }
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


    public void ReLoadFormulaWithValueArrearProcNetPay(string sEmpSystemID, FunctionPara para, string strFormulaID,
      out string sFormulaValue, out string StructureValue, bool bEarning, List<SPvalueHeadWise> dtValue, List<SPSalaryHead> dicSlrHd)
    {
        DataSet dsLocal = null;
        //DataView dvLocal = null;
        //DataView dvSlrHd = null;
        string strTemp = "";
        StructureValue = string.Empty;
        string Structure_temp = string.Empty;

        try
        {
            //dtValue
            //List<SPvalueHeadWise> list = new List<SPvalueHeadWise>();
            //list= dtValue.ToList<SPvalueHeadWise>();

            dsLocal = new DataSet();

            string strFormulaIDTemp = strFormulaID.Trim();
            string sLocalCurrencyID = para.lblLocalCurrencyID;
            string sForeignCurRate = para.lblLocalCurRate;

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
                bool IsForStructure = false;
                strTemp = "";

                strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                if (strTemp.Trim() == "+" || strTemp.Trim() == "-" || strTemp.Trim() == "*" || strTemp.Trim() == "/" || strTemp.Trim() == ">" || strTemp.Trim() == "<" || strTemp.Trim() == "(" || strTemp.Trim() == ")")
                {
                    strTemp = dsLocal.Tables[0].Rows[i]["ID"].ToString();
                }
                else
                {
                    //dvLocal = new DataView();
                    //dvLocal.Table = dtValue;

                    var dtv = dtValue.FindAll(x => x.SalaryHeadID == strTemp.Trim() && x.EmpSystemID == sEmpSystemID);
                    // dvLocal.RowFilter = "SalaryHeadID = '" + strTemp.Trim() + "' AND EmpSystemID = '" + sEmpSystemID + "'";
                    if (dtv.Count() > 0)
                    {
                        if (bEarning == false)
                        {
                            if (dtv[0].EntryCurrencyID == para.lblLocalCurrencyID.Trim())
                            {
                                strTemp = dtv[0].EntryAmount;
                                strTemp = GetAbsValue(strTemp);
                            }
                            else
                            {
                                strTemp = (Convert.ToDecimal(dtv[0].EntryAmount) * Convert.ToDecimal(para.txtForeignCurRate.Trim())).ToString();
                                strTemp = GetAbsValue(strTemp);
                            }
                        }
                        else
                        {
                            decimal decAmount = Convert.ToDecimal(Convert.ToDecimal(dtv[0].EarningAmount).ToString("0.00"));
                            var ss = Convert.ToDecimal(Convert.ToDecimal(dtv[0].EntryAmount).ToString("0.00"));
                            IsForStructure = true;

                            if (decAmount == 0)
                            { decAmount = Convert.ToDecimal(Convert.ToDecimal(dtv[0].EntryAmount).ToString("0.00")); }

                            if (dtv[0].EarningCurrencyID == para.lblLocalCurrencyID.Trim())
                            {
                                strTemp = Convert.ToDecimal(dtv[0].EarningAmount).ToString("0.00");
                                strTemp = GetAbsValue(strTemp);

                                Structure_temp = Convert.ToDecimal(ss).ToString("0.00");
                                Structure_temp = GetAbsValue(Structure_temp);
                            }
                            else
                            {
                                strTemp = (decAmount * Convert.ToDecimal(para.txtForeignCurRate.Trim())).ToString();
                                strTemp = GetAbsValue(strTemp);

                                Structure_temp = (ss * Convert.ToDecimal(para.txtForeignCurRate.Trim())).ToString();
                                Structure_temp = GetAbsValue(Structure_temp);
                            }
                        }
                    }
                    else
                    {
                        var dicsh = dicSlrHd.FindAll(x => x.SalaryHeadID == strTemp.Trim());
                        if (dicsh.Count() > 0)
                        {
                            strTemp = "0.00";
                        }
                        // var dvSPChd_dic = dicProcChild.FindAll(x => x.EmpInfoSystemID == dicLocal_Sub[i].EmpInfoSystemID && x.SalaryHeadID == dicLocal_Sub[i].SalaryHeadID && x.SlrProcMstSystemID == para.lblSalaryProcSystemId.Trim());

                        //dicsal
                        //dvSlrHd = new DataView();
                        //dvSlrHd.Table = dtSlrHd;
                        //dvSlrHd.RowFilter = "SalaryHeadID = '" + strTemp.Trim() + "'";
                        //if (dvSlrHd.Count == 1)
                        //{
                        //    strTemp = "0.00";
                        //}
                    }
                }


                sFormulaValue += strTemp.Trim();
                if (IsForStructure)
                {
                    StructureValue += Structure_temp.Trim();
                }
                else
                {
                    StructureValue += strTemp.Trim();
                }
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

}

public static class clsSalaryUtilityDataTable
{
    public static DataTable ToDataTable<T>(this IList<T> data)
    {
        PropertyDescriptorCollection properties =
            TypeDescriptor.GetProperties(typeof(T));
        DataTable table = new DataTable();
        foreach (PropertyDescriptor prop in properties)
            table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
        foreach (T item in data)
        {
            DataRow row = table.NewRow();
            foreach (PropertyDescriptor prop in properties)
                row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
            table.Rows.Add(row);
        }
        return table;
    }
}
public class SPvalueHeadWise
{
    public string EmpSystemID { get; set; }
    public string SalaryHeadID { get; set; }
    public string EntryCurrencyID { get; set; }
    public string EntryAmount { get; set; }
    public string EarningCurrencyID { get; set; }
    public string EarningAmount { get; set; }
}
public class SPSalaryHead
{
    public string SalaryHeadID { get; set; }
    //public string SalaryHead { get; set; }
    //public string Description { get; set; }
    //public string HeadType { get; set; }
    //public string HeadCategory { get; set; } 
}
public class ParaEmployeeShiftAssign
{
    public string SystemID { get; set; }
    public string EmpSystemID { get; set; }
    public string FixSystemID { get; set; }
    public string RosterSystemID { get; set; }
    public bool IsFix { get; set; }
    public bool IsRoster { get; set; }
    public string EffectiveDate { get; set; }
    public string RosterStartShiftID { get; set; }
    public int StartFromDay { get; set; }
    public string AddedBy { get; set; }
    public string DateAdded { get; set; }
    public string UpdatedBy { get; set; }
    public string DateUpdated { get; set; }
}
public class ParamSalary
{
    public bool IsLastSalaryProcessWithFixedHead { get; set; }
    public bool ShouldTaxProcessContinue { get; set; }

    public bool IsLastDayFixed { get; set; }
    public int LastDay { get; set; }
    public bool IsFirstProcess { get; set; }
    public bool IsLastProcess { get; set; }
    public bool IsFullProcess { get; set; }
    public int intYearNo { get; set; }
    public int intMonthNo { get; set; }
    public int DaysInMonth { get; set; }
    public DateTime FirstDayOfMonth { get; set; }
    public DateTime LastDayOfMonth { get; set; }
    //intYearNo intMonthNo DaysInMonth    
}//End Function
