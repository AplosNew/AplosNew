using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;


public class xclsSalaryUtility
    {
    public void FractionCalculation(string sRoundOption, bool IntegerInDisb, bool IsDecimalInDisb, int DecimalNo, string sValue, out string sResultValue)
    {
        sResultValue = "0";
        try
        {
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
                    if (dvLocal.Count == 1)
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
            //yes
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
    
}

public static class xclsSalaryUtilityDataTable
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

public class xParaEmployeeShiftAssign
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
