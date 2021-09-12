
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace HRService
{
    public delegate void getAccount(string accountID, string COADesc);
    public delegate void closeApplication();
    class clsStdLib
    {
        public clsStdLib()
        {

        }
        public enum mType
        {
            Error,
            Success,
            Information
        }
        public static bool passwordGet = true;
        public static string[] sMonth = new string[] { "<Unselect>", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        public enum NotificationType
        {
            Attendance,

            Salary,
            SalaryDisbursement,
            SalaryApproval,
            SalaryApprovalRollback,

            Promotion,
            PromotionRollback,

            Increment,
            IncrementRollback,

            GeneralAnnouncement,
            Holiday,
            Birthday
        }

        #region date related
        public static readonly string dateFormat = "dd-MMM-yyyy";
        public static readonly string sqliteDateFormat = "yyyy-MM-dd";
        public static readonly string AppToDBdateFormat = "yyyy-MM-dd hh:mm:ss";
        public static bool IsDateOK(string strdate)
        {
            try
            {
                if (strdate.Length != 11)
                {
                    return false;
                }
                if (strdate.Substring(2, 1) != "-" && strdate.Substring(6, 1) != "-")
                {
                    return false;
                }
                System.DateTime myDt = System.Convert.ToDateTime(strdate);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                //
            }
        }// end function
        private static bool DateOkCheck(string strdate)
        {
            try
            {
                System.DateTime myDt = System.Convert.ToDateTime(strdate);
                return true;
            }
            catch (System.Exception ex)
            {
                return false;
            }
            finally
            {
                //
            }
        }// end function
        public static object chk_NullDateData(object dateValue)
        {
            if (DateOkCheck("" + dateValue.ToString()) == false)
            {
                dateValue = "";
            }

            if (("" + dateValue.ToString()) == "")
            {
                System.DateTime dt = new System.DateTime(1901, 1, 1);
                dateValue = (object)dt;
            }
            return (object)dateValue;
        }
        public static System.DateTime AppDateConvert(object dateValue, string input_date_format, string output_date_format)
        {
            string strDate = null;
            dateValue = chk_NullDateData(dateValue);
            strDate = dateValue.ToString();
            if (strDate != "")
            {
                if (input_date_format.Trim() != "")
                {
                    if (output_date_format.Trim() != "")
                    {
                        System.Globalization.DateTimeFormatInfo InputFormat = new System.Globalization.DateTimeFormatInfo();
                        InputFormat.ShortDatePattern = input_date_format;
                        System.DateTime myDt = System.Convert.ToDateTime(strDate, InputFormat);
                        strDate = myDt.ToString(output_date_format);
                    }
                }
            }
            return System.Convert.ToDateTime(strDate);
        }// End of function
        public static Object DateData_AppToDB(object dateValue, string DB_Level_date_format)
        {
            if (string.IsNullOrEmpty((string)dateValue))
                return DBNull.Value;

            string strDate = null;
            strDate = dateValue.ToString();
            if (DB_Level_date_format != "")
            {
                // Collecting the user terminal set format 
                System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
                strDate = AppDateConvert(strDate, USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString(), DB_Level_date_format).ToString();
            }

            string m = System.Convert.ToDateTime(strDate).ToString(AppToDBdateFormat);
            return System.Convert.ToDateTime(strDate).ToString(AppToDBdateFormat);


        }// End of function
        public static System.DateTime DateData_DBToApp(object dateValue)
        {
            string strDate = null;
            strDate = dateValue.ToString();

            System.Globalization.DateTimeFormatInfo myDBDateFormat = new System.Globalization.CultureInfo("en-US", false).DateTimeFormat;
            strDate = DateData_DBToApp(dateValue, myDBDateFormat.ShortDatePattern.ToString()).ToString();
            return System.Convert.ToDateTime(strDate);
        }// End function
        public static System.DateTime DateData_DBToApp(object dateValue, string DB_Level_date_format)
        {
            string strDate = null;
            strDate = dateValue.ToString();
            if (DB_Level_date_format != "")
            {
                // Collecting the user terminal set format 
                System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
                strDate = AppDateConvert(strDate, DB_Level_date_format, USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString()).ToString();
            }
            return System.Convert.ToDateTime(strDate);
        }// End of function
        public static String makeBaseBlank(object dateValue)
        {
            System.DateTime dt;
            dt = System.Convert.ToDateTime(dateValue.ToString());
            if (dt.Year == 1901)
            {
                return "";
            }
            else
            {
                return dateValue.ToString();
            }
        }// End of function
        ///<summary>
        ///return day difference in integer. 
        ///    Example 1: firstDate[Less Than]lastDate returns positive value
        ///    Example 2: firstDate>lastDate returns negative value
        ///    Example 3: firstDate=lastDate returns 0 [zero]**/
        /// </summary>
        public static int dateDiff(string firstDate, string lastDate)
        {

            int difference = 0;
            try
            {
                firstDate = Convert.ToDateTime(firstDate).ToString("dd-MMM-yyyy");
                lastDate = Convert.ToDateTime(lastDate).ToString("dd-MMM-yyyy");

                if (IsDateOK(firstDate) == false)
                {
                    Exception ex = new Exception("Invalid [First Date]");
                    throw (ex);
                }
                if (IsDateOK(lastDate) == false)
                {
                    Exception ex = new Exception("Invalid [Last Date]");
                    throw (ex);
                }
                DateTime dateFirstDate = Convert.ToDateTime(firstDate);
                DateTime dateLastDate = Convert.ToDateTime(lastDate);
                TimeSpan TimeSpan = dateLastDate.Subtract(dateFirstDate);


                difference = TimeSpan.Days;
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return difference;
        }



        public static string getSqliteDate(string standardDate)
        {
            return (Convert.ToDateTime(standardDate).ToString(sqliteDateFormat));
        }
        public static string getStandardDateFromSqliteDate(string SqliteDate)
        {
            if (SqliteDate.Length != 10)
                return "";
            if (SqliteDate.Split('-').Length != 3)
                return "";
            //many things to validate 
            //but i have less time :)
            string month = ValidLength(sMonth[Convert.ToInt32(SqliteDate.Split('-')[1])], 3).ToString();


            return SqliteDate.Split('-')[2] + "-" + month + "-" + SqliteDate.Split('-')[0];
        }
        #endregion date related

        #region numeric
        public static bool IsNumeric(string strNumber)
        {
            Double d;
            System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
            if (strNumber.Length == 0)
            {
                return false;
            }
            return Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d);
        } // End Function
        public static string GetNumericData(string strNumber)
        {
            double d;
            strNumber = strNumber.Replace(",", "");
            System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
            if (strNumber.Trim() == "")
            { return "0"; }
            else if (System.Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
            {
                return strNumber;
            }
            else
            {
                return "0";
            }
        }// end function
        public static string GetNumericDataInDecimalFormat(string strNumber, int precision)
        {
            if (precision < 1)
                return strNumber;

            string s_precision = new String('0', precision);

            double d;
            System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
            if (strNumber.Trim() == "")
            { return "0." + s_precision; }
            else if (System.Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
            {
                return string.Format("{0:0." + s_precision + "}", d);
            }
            else
            {
                return "0." + s_precision;
            }
        }// end function
        public static double dbl(string d)
        {
            return Convert.ToDouble(GetNumericData(d));

        }
        public static int Percentage(int total, double percentage)
        {
            return (int)(total * (percentage / 100));

        }
        //validation
        public static void numericValidation(string value, bool isMandatory, bool isInteger, bool negativeAllowed, string fieldName)
        {

            try
            {



                if (isMandatory == true)
                {
                    if (value.Trim() == "")
                    {
                        Exception ex = new Exception("please insert [" + fieldName + "]");
                        throw (ex);
                    }
                    if (Convert.ToDouble(GetNumericData(value.Trim())) == 0)
                    {
                        Exception ex = new Exception("please insert [" + fieldName + "]");
                        throw (ex);
                    }

                    if (value.Trim() != "")
                    {
                        if (IsNumeric(value.Trim()) == false)
                        {
                            Exception ex = new Exception("Invalid numeric value [" + value + "] for the field [" + fieldName + "]");
                            throw (ex);
                        }
                    }
                }

                if (value.Trim() != "")
                {
                    if (IsNumeric(value.Trim()) == false)
                    {
                        Exception ex = new Exception("Invalid numeric value [" + value + "] for the field [" + fieldName + "]");
                        throw (ex);
                    }
                    if (isInteger == true)
                    {

                        if (isInt(value.Trim()) == false)
                        {
                            Exception ex = new Exception("Number must be integer for the field [" + fieldName + "]");
                            throw (ex);
                        }

                    }
                    if (negativeAllowed == false)
                    {
                        if (Convert.ToDouble(GetNumericData(value.Trim())) < 0)
                        {
                            Exception ex = new Exception("Negative values are not allowed for the field [" + fieldName + "]");
                            throw (ex);
                        }
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


        }

        ///<summary>
        ///check whether a value is integer or not returns true if integer, 
        ///false if floating or string containing alpahnumeric
        ///</summary>
        public static bool isInt(string num)
        {

            bool isInt;
            int number;
            try
            {
                isInt = System.Int32.TryParse(num, out number);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
            return isInt;
        }


        #endregion numeric

        #region string

        public static readonly string excelNegativePOsitiveSign = @"+#,##0.00;-#,##0.00;* ??;@";
        public static readonly string NegativePOsitiveSign = @"+#,##0.00;-#,##0.00;0";
        public static readonly string NumberFormatString = "#,##0.000;(#,##0.000);* ??;@";
        public static readonly string NumberFormatStringFourDecimal = "#,##0.0000;(#,##0.0000);* ??;@";
        public static readonly string NumberFormatStringFiveDecimal = "#,##0.00000;(#,##0.00000);* ??;@";
        public static readonly string NumberFormatStringTwoDecimal = "#,##0.00;(#,##0.00);* ??;@";
        public static readonly string NumberFormatStringTwoDecimalWithZero = "#,##0.00;(#,##0.00)";
        public static readonly string NumberFormatStringInteger = "#,##0;(#,##0);* ??;@";
        public static readonly string NumberFormatStringIntegerWithZero = "#,##0;(#,##0)";
        public static readonly string NumberFormatStringText = "@"; //format cell data as text


        public static object ValidLength(string str)
        {

            string removechar = "";
            if (str.Trim() == "")
            {
                return (object)Convert.DBNull;
            }
            removechar = str.Trim();
            removechar = removechar.Replace("'", " ");

            return (object)removechar.Trim();

        }
        public static object ValidLength(string str, int length)
        {

            string removechar = "";
            if (str.Trim() == "")
            {
                return (object)Convert.DBNull;
            }
            removechar = str.Trim();
            removechar = removechar.Replace("'", " ");


            int strLen = removechar.Length;
            if (strLen > length)
                removechar = removechar.Substring(0, length);

            return (object)removechar.Trim();

        }
        public static string FileNameLegalChar(string fileName)
        {
            string illegalChar = @"~`!@#$%^&*=/\|>,<";
            foreach (char c in illegalChar)
            {
                fileName = fileName.Replace(c.ToString(), " ");
            }

            return fileName;
        }
        private StringCollection getTableColumns(ref DataSet dsLocal)
        {
            StringCollection strcol = new StringCollection();
            for (int COL = 0; COL < dsLocal.Tables[0].Columns.Count; COL++)
            {
                strcol.Add(dsLocal.Tables[0].Columns[COL].ColumnName.ToUpper());
            }

            return strcol;

        }
        public static string emptyString(string str)
        {
            //this function returns an empty string(not a null) from null or empty or '&nbsp;' from the page
            if (str == "&nbsp;")
                str = "";
            if (string.IsNullOrEmpty(str) == true)
                str = "";


            return str;
        }//this function returns an empty string(not a null) from null or empty '&nbsp;' from the page
        #endregion string


        #region others
        public void copyDataset(DataSet source, ref DataSet destination)
        {
            StringCollection strColDestinationColumns = getTableColumns(ref destination);//upper case
            DataRow drLocal = null;
            for (int ROW = 0; ROW < source.Tables[0].Rows.Count; ROW++)
            {
                drLocal = destination.Tables[0].NewRow();
                for (int COL = 0; COL < source.Tables[0].Columns.Count; COL++)
                {
                    if (strColDestinationColumns.Contains(source.Tables[0].Columns[COL].ToString().ToUpper()))
                    {
                        drLocal[source.Tables[0].Columns[COL].ToString()] = ValidLength(source.Tables[0].Rows[ROW][source.Tables[0].Columns[COL].ToString()].ToString());
                    }
                }
                destination.Tables[0].Rows.Add(drLocal);
            }


        }
        public static string GetxlsCol(int intCol)
        {
            //returns excel columns based on column number. tested 1 to 256 column numbers
            try
            {
                if (intCol < 1 || intCol > 256)
                {
                    System.Exception ex = new Exception("Invalid Column Value");
                    throw (ex);
                }
                intCol = intCol - 1;
                int intFirstLetter = ((intCol) / 512) + 64;
                int intSecondLetter = ((intCol % 512) / 26) + 64;
                int intThirdLetter = (intCol % 26) + 65;
                char FirstLetter;
                char SecondLetter;
                if (intFirstLetter > 64)
                    FirstLetter = (char)intFirstLetter;
                else
                    FirstLetter = ' ';

                if (intSecondLetter > 64)
                    SecondLetter = (char)intSecondLetter;
                else
                    SecondLetter = ' ';

                char ThirdLetter = (char)intThirdLetter;
                return string.Concat(FirstLetter, SecondLetter, ThirdLetter).Trim();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }//returns excel columns based on column number. tested 1 to 256 column numbers
        #endregion others


        public static double sum(string columnName, DataTable dtLocal, string criteria)
        {
            double total = 0;
            DataRow[] dr = dtLocal.Select(criteria);
            foreach (DataRow d in dr)
            {
                total += dbl(d[columnName].ToString());
            }


            return total;
        }
    }
}
