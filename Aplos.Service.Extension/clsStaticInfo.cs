using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.UI.WebControls;
using System.Data.SqlClient;
using Syncfusion.XlsIO;
using System.Reflection;

namespace Library.Service.Extension
{
    public class clsStaticInfo
    {
        //private string REMOTESERVERNAME = "";
        //private string REMOTEDATABASENAME = "";
        private string REMOTELINKSERVER = "";

        //private string LOCALSERVERNAME = "";
        //private string LOCALDATABASENAME = "";
        //private string USER_ID = "";
        //private string PASSWORD = "";

        //private string AUTH_SERVER_NAME = "";
        //private string AUTH_DATABASE_NAME = "";
        private string AUTH_LINK_SERVER = "";

        //private string strConnString = "";

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
        public static string GetDate(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";

            try
            {
                return Convert.ToDateTime(s).ToString("dd-MMM-yyyy");
            }
            catch (Exception)
            {
                return "";
            }
        }
        public static string GetDateTime(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";

            try
            {
                return Convert.ToDateTime(s).ToString("dd-MMM-yyyy hh:mm:ss tt");
            }
            catch (Exception)
            {
                return "";
            }
        }
        public static string NumberFormat(int Precision = 0, bool percent = false)
        {
            string percentSign = "";
            if (percent)
                percentSign = "%";
            if (Precision > 0)
                return "#,##0." + new string('0', Precision) + percentSign + "_);(#,##0." + new string('0', Precision) + percentSign + ")";

            return "#,##0_);(#,##0)";
        }
        public static void SetDate(IRange Cell, string s)
        {
            if (string.IsNullOrEmpty(s))
                return;

            try
            {
                Cell.DateTime = Convert.ToDateTime(s);
            }
            catch (Exception)
            {
                return;
            }
        }
        public static bool emailValidation(string emailID)
        {
            bool validID = false;
            bool valiDOT = false;
            bool validAT = false;
            bool HasIllegalChar = false;
            int ATposition = 0;
            int ATcount = 0;
            int doubleDOT = 0;
            string y = "";
            //this boolsheet(!!!) function created by booooollllsheet tarek
            //detectign illegal '@' starting and ending of string, string must be >=4
            if (emailID.Length < 4 || emailID.Substring(0, 1) == "@" || emailID.Substring(emailID.Length - 1, 1) == "@")
            {
                validID = false;
                return validID;
            }
            //detecting illegal char set
            for (int ichr = 0; ichr < emailID.Length; ichr++)
            {
                int chr;
                chr = char.ConvertToUtf32(emailID, ichr);
                int[] col = { 126, 96, 33, 35, 36, 37, 94, 38, 32, 40, 41, 42, 43, 44, 45, 93, 125, 91, 61, 123, 39, 34, 59, 58, 62, 60, 47, 63, 92 }; //----->> ~!#$%^&*()/\,{[}]:"?><,./;'
                //validating double DOT Sequentially
                if (chr == 46)
                {
                    doubleDOT++;
                    if (doubleDOT > 1)
                    {
                        validID = false;
                        return validID;
                    }
                }
                else
                {
                    doubleDOT = 0;
                }
                //------------------------------------
                foreach (int g in col)
                {
                    if (chr == g)
                    {
                        HasIllegalChar = true;
                        validID = false;
                        return validID;
                    }
                }
            }

            //detecting '@' position

            for (int i = 0; i < emailID.Length; i++)
            {
                int f = 0;

                f = char.ConvertToUtf32(emailID, i);
                if (i > 0 && (f == 64))
                {
                    validAT = true;
                    ATposition = i;
                    ATcount++;
                }
            }
            if (ATcount == 1)
            {
                y = emailID.Substring(ATposition + 1);
            }
            else
            {
                validID = false;
                return validID;
            }

            //validating domain name after '@'

            for (int j = 0; j < y.Length; j++)
            {
                int f = 0;

                f = char.ConvertToUtf32(y, j);
                if (j > 1 && (f == 46))
                {
                    valiDOT = true;
                }
            }
            //detectign illegal '.' starting and ending of whole string and left+right of '@', string must be >=4
            if (emailID.Substring((emailID.Length - 1), 1) == "." || emailID.Substring(0, 1) == "." || emailID.Substring((ATposition - 1), 1) == "." || emailID.Substring((ATposition + 1), 1) == ".")
            {
                valiDOT = false;
                validID = false;
                return validID;
            }

            if (validAT == true && ATcount == 1 && valiDOT == true && HasIllegalChar == false)
            {
                validID = true;
            }

            return validID;
        }

        public clsStaticInfo()
        {
            //conec
            //LOCALSERVERNAME = ConfigurationManager.AppSettings["SERVER_NAME"];
            //LOCALDATABASENAME = ConfigurationManager.AppSettings["DATABASE_NAME"];

            //USER_ID = ConfigurationManager.AppSettings["USER_ID"];
            //PASSWORD = ConfigurationManager.AppSettings["PASSWORD"];
            //strConnString = "Data Source=" + LOCALSERVERNAME + ";Initial Catalog=" + LOCALDATABASENAME + ";User ID=" + USER_ID + ";Password=" + PASSWORD + "";

            //REMOTESERVERNAME = "[" + ConfigurationManager.AppSettings["REMOTE_SERVER_NAME"] + "]";
            //REMOTEDATABASENAME = ConfigurationManager.AppSettings["REMOTE_DATABASE_NAME"];

            ////REMOTELINKSERVER = REMOTESERVERNAME + "." + REMOTEDATABASENAME + ".dbo.";
            //REMOTELINKSERVER = REMOTEDATABASENAME + ".dbo.";

            //AUTH_SERVER_NAME = ConfigurationManager.AppSettings["AUTH_SERVER_NAME"];
            //AUTH_DATABASE_NAME = ConfigurationManager.AppSettings["AUTH_DATABASE_NAME"];
            //AUTH_LINK_SERVER = AUTH_DATABASE_NAME + ".dbo.";
        }
        public static double dbl(string value)
        {
            return Convert.ToDouble(clsWebLib.GetNumData(value));
        }
        public static double dbl(object value)
        {
            if (value == null)
                value = "";
            return Convert.ToDouble(clsWebLib.GetNumData(value.ToString()));
        }
        public void GetNonPostedJournals(string EntityID, string FiscalYear, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM JournalEntryNonPosted where EntityID='" + EntityID + @"' AND FiscalYear='" + FiscalYear + @"'";
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

        public void GetAllModuleWiseUserInfo(out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM tblModuleWiseUserInfo";

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

        public void IsNumber(string pNumber, string pFieldName, bool IsZeroAllowed, bool IsNegativeAllowed, bool IsDecimalAllowed, int pDecimalPlace)
        {
            if (clsWebLib.IsNumeric(pNumber) == false)
            {
                Exception ex = new Exception("'" + pFieldName + "' should be numeric value");
                throw (ex);
            }
            if (IsZeroAllowed == false && Convert.ToDouble(pNumber) == 0)
            {
                Exception ex = new Exception("'" + pFieldName + "' should not be zero");
                throw (ex);
            }
            if (IsZeroAllowed == false && Math.Round(Convert.ToDouble(pNumber), pDecimalPlace) == 0)
            {
                Exception ex = new Exception("'" + pFieldName + "' should not be nearly zero");
                throw (ex);
            }
            if (IsNegativeAllowed == false && Convert.ToDouble(pNumber) < 0)
            {
                Exception ex = new Exception("'" + pFieldName + "' should not be negative");
                throw (ex);
            }
            if (IsDecimalAllowed == false && Convert.ToDouble(pNumber) < 1 && Convert.ToDouble(pNumber) > 0)
            {
                Exception ex = new Exception("'" + pFieldName + "' should be integer");
                throw (ex);
            }
        }

        public static bool IsIntegratedWithSAP(string GroupID)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsLocal = null;

            try
            {
                bool FLAG = false;

                strSQL = "SELECT * FROM GroupCreation WHERE GroupID='" + GroupID + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsLocal, false, "1");

                if (dsLocal.Tables[0].Rows.Count > 0)
                {
                    if (clsWebLib.GetBoolData(dsLocal.Tables[0].Rows[0]["IsIntegratedWithSAP"].ToString().Trim()) == true)
                    {
                        FLAG = true;
                    }
                }

                return FLAG;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objCon = null;
                dsLocal = null;
            }
        }

        public void GetPlantWiseHRMSSetting(string sGroupID, string sPlantID, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = "SELECT * FROM dbo.PlantWiseHRMSSetting WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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

        public void GetDayType(out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.DayType ORDER BY DayType";
                //strSQL = @"SELECT * FROM dbo.DayType where DayType in ('P','L')  ORDER BY DayType";

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
        public void GetDayTypeManual(out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT * FROM dbo.DayType ORDER BY DayType";
                strSQL = @"SELECT * FROM dbo.DayType where DayType in ('P','L','HDP','A')  ORDER BY DayType";

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
        public void GetDayTypeLeast(out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.DayType ORDER BY DayType";
                //strSQL = @"SELECT * FROM dbo.DayType where DayType in ('P','L')  ORDER BY DayType";

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

        public void GetDayTypeOnlyNormalAttnProce(string sDayType, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.DayType
                            WHERE ISNULL(Category,'') NOT IN ('Holiday','Working Day','Leave','Weekend','Half Day-First Half',
                                                        'Half Day-Second Half')";

                if (sDayType.Trim() != "")
                {
                    strSQL = strSQL + @" AND DayType NOT IN ('" + sDayType + "') ";
                }
                strSQL = strSQL + @" ORDER BY DayType";

                //strSQL = "SELECT * FROM dbo.DayType where DayType in ('P','L')  ORDER BY DayType";

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

        public void GetNormalDayTypeAttnProce(out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM dbo.DayType
                            WHERE ISNULL(DayType,'') IN ('A','L','P','H','W','LV')
                            ORDER BY DayType";

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
        public void getCutOffDate(string PlantId, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @" SELECT [Id]
     
      ,[PlantId]
      ,[ModuleName]
      ,[CutOffDate]
     
  FROM [SCS].[OpeningBalanceCutOffDate] where PlantId='" + PlantId + "' and ModuleName='HR'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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
        public void GetCompanyPlant(string CompanyID, string UserSystemID, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @" SELECT plant.plantid systemid
                                   --, userentityplant.plantid
                                   , plant.Code+'-'+plant.plantname Description
                            FROM   userentitymodule
                                   INNER JOIN userentityplant ON userentitymodule.systemid = userentityplant.userentitymodulesystemid
                                   LEFT OUTER JOIN plant ON userentityplant.plantid = plant.plantid
                                   LEFT OUTER JOIN applicablemodules ON applicablemodules.systemid = userentitymodule.modulesystemid
                            WHERE  userentitymodule.usersystemid = '" + UserSystemID + @"'
                                   AND applicablemodules.systemid = '" + clsWebLib.MODULEID + @"'
                                   AND userentitymodule.companyid = '" + CompanyID + @"' ";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
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

        public void PlantWiseAccess(DropDownList ddl, string CompanyID, string PlantID, string USERSYSTEMID, string SYSTEM_CONFIG_USER_ID)
        {
            DataSet dsLocal = null;
            DataSet dsCompAllPlant = null;
            try
            {
                dsLocal = new DataSet();
                ddl.DataSource = null;
                ddl.DataBind();
                GetCompanyPlant(CompanyID, out dsCompAllPlant);

                if (dsCompAllPlant.Tables[0].Rows.Count > 0)
                {
                    ddl.DataSource = dsCompAllPlant.Tables[0].DefaultView;
                    ddl.DataTextField = "PlantName";
                    ddl.DataValueField = "PlantID";
                    ddl.DataBind();
                }
                //////if (string.IsNullOrEmpty(SYSTEM_CONFIG_USER_ID) == true)
                //////{
                //////GetCompanyPlant(CompanyID, USERSYSTEMID, out dsLocal);
                //////if (dsLocal.Tables[0].Rows.Count > 0)
                //////{
                //////    ddl.DataSource = dsLocal.Tables[0].DefaultView;
                //////    ddl.DataTextField = "Description";
                //////    ddl.DataValueField = "SystemID";
                //////    ddl.DataBind();
                //////}
                //////else
                //////{
                //////    ddl.DataSource = dsCompAllPlant.Tables[0].DefaultView;
                //////    ddl.DataTextField = "PlantName";
                //////    ddl.DataValueField = "PlantID";
                //////    ddl.DataBind();
                //////}

                ////////if (ddl.Items.Count == 1)
                ////////{
                ////////    ddl.SelectedIndex = 0;
                ////////    ddl.Enabled = false;
                ////////}
                ////////else if (ddl.Items.Count > 1)
                ////////{
                ////////    ddl.Items.Insert(0, "");
                ////////    if (string.IsNullOrEmpty(PlantID) == false)
                ////////    {
                ////////        ddl.SelectedValue = PlantID;
                ////////    }
                ////////    else
                ////////    {
                ////////        ddl.SelectedIndex = -1;
                ////////    }
                ////////    ddl.Enabled = true;
                ////////}
                //////}//config user
                //////else
                //////{
                //////    ddl.DataSource = dsCompAllPlant.Tables[0].DefaultView;
                //////    ddl.DataTextField = "PlantName";
                //////    ddl.DataValueField = "PlantID";
                //////    ddl.DataBind();
                //////    //if (ddl.Items.Count == 1)
                //////    //{
                //////    //    ddl.Items.Insert(0, "");
                //////    //    if (string.IsNullOrEmpty(PlantID) == false)
                //////    //    {
                //////    //        ddl.SelectedValue = PlantID;
                //////    //    }
                //////    //    else
                //////    //    {
                //////    //        ddl.SelectedIndex = -1;
                //////    //    }
                //////    //    ddl.Enabled = true;
                //////    //}
                //////}

                if (ddl.Items.Count == 1)
                {
                    ddl.SelectedIndex = 0;
                    ddl.Enabled = false;
                }
                else if (ddl.Items.Count > 1)
                {
                    ddl.Items.Insert(0, "");
                    if (string.IsNullOrEmpty(PlantID) == false)
                    {
                        ddl.SelectedValue = PlantID;
                        ddl.Enabled = false;
                    }
                    else
                    {
                        ddl.SelectedIndex = -1;
                        ddl.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                dsLocal = null;
            }
        }//End Function

        public void PlantWiseAccess(DropDownList ddl, string CompanyID, string PlantID, string USERSYSTEMID, string SYSTEM_CONFIG_USER_ID, bool controle, bool sys, string emp, string com)
        {
            DataSet dsLocal = null;
            DataSet dsCompAllPlant = null;
            try
            {
                dsLocal = new DataSet();
                ddl.DataSource = null;
                ddl.DataBind();
                GetCompanyPlant(CompanyID, controle, sys, emp, com, out dsCompAllPlant);

                if (dsCompAllPlant.Tables[0].Rows.Count > 0)
                {
                    ddl.DataSource = dsCompAllPlant.Tables[0].DefaultView;
                    ddl.DataTextField = "PlantName";
                    ddl.DataValueField = "PlantID";
                    ddl.DataBind();
                }
                //////if (string.IsNullOrEmpty(SYSTEM_CONFIG_USER_ID) == true)
                //////{
                //////GetCompanyPlant(CompanyID, USERSYSTEMID, out dsLocal);
                //////if (dsLocal.Tables[0].Rows.Count > 0)
                //////{
                //////    ddl.DataSource = dsLocal.Tables[0].DefaultView;
                //////    ddl.DataTextField = "Description";
                //////    ddl.DataValueField = "SystemID";
                //////    ddl.DataBind();
                //////}
                //////else
                //////{
                //////    ddl.DataSource = dsCompAllPlant.Tables[0].DefaultView;
                //////    ddl.DataTextField = "PlantName";
                //////    ddl.DataValueField = "PlantID";
                //////    ddl.DataBind();
                //////}

                ////////if (ddl.Items.Count == 1)
                ////////{
                ////////    ddl.SelectedIndex = 0;
                ////////    ddl.Enabled = false;
                ////////}
                ////////else if (ddl.Items.Count > 1)
                ////////{
                ////////    ddl.Items.Insert(0, "");
                ////////    if (string.IsNullOrEmpty(PlantID) == false)
                ////////    {
                ////////        ddl.SelectedValue = PlantID;
                ////////    }
                ////////    else
                ////////    {
                ////////        ddl.SelectedIndex = -1;
                ////////    }
                ////////    ddl.Enabled = true;
                ////////}
                //////}//config user
                //////else
                //////{
                //////    ddl.DataSource = dsCompAllPlant.Tables[0].DefaultView;
                //////    ddl.DataTextField = "PlantName";
                //////    ddl.DataValueField = "PlantID";
                //////    ddl.DataBind();
                //////    //if (ddl.Items.Count == 1)
                //////    //{
                //////    //    ddl.Items.Insert(0, "");
                //////    //    if (string.IsNullOrEmpty(PlantID) == false)
                //////    //    {
                //////    //        ddl.SelectedValue = PlantID;
                //////    //    }
                //////    //    else
                //////    //    {
                //////    //        ddl.SelectedIndex = -1;
                //////    //    }
                //////    //    ddl.Enabled = true;
                //////    //}
                //////}

                if (ddl.Items.Count == 1)
                {
                    ddl.SelectedIndex = 0;
                    ddl.Enabled = false;
                }
                else if (ddl.Items.Count > 1)
                {
                    ddl.Items.Insert(0, "");
                    if (string.IsNullOrEmpty(PlantID) == false)
                    {
                        ddl.SelectedValue = PlantID;
                        ddl.Enabled = false;
                    }
                    else
                    {
                        ddl.SelectedIndex = -1;
                        ddl.Enabled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                dsLocal = null;
            }
        }//End Function

        public void getJobLocationGroupWise(string GroupID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT chkSelect = Convert(bit, 'FALSE'), SystemID, JobLocation FROM JobLocation
                //            WHERE GroupID = '" + GroupID.ToString() + "' ORDER BY JobLocation";

                strSQL = @"SELECT chkSelect = Convert(bit, 'FALSE'), SystemID, JobLocation FROM dbo.JobLocation
                            WHERE GroupID = '" + GroupID.ToString() + "' ORDER BY JobLocation";

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
        }//End of function  OK

        public void getEmpGrpGroupWise(string GroupID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                //strSQL = @"SELECT chkSelectEG = Convert(bit, 'FALSE'), SystemID, EmployeeGroupCode,
                //                  EmployeeGroupName FROM EmployeeGroup

                //            WHERE GroupID ='" + GroupID.ToString() + @"' AND IsActive = 1
                //                    ORDER BY EmployeeGroupCode";

                strSQL = @"SELECT chkSelectEG = Convert(bit, 'FALSE'), Id, Code,
                                  UserName FROM HKP.EmployeeGroup
                            WHERE CompanyGroupId ='" + GroupID.ToString() + @"' AND Active = 1
                                    ORDER BY Code";

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
        }//End of function  OK

        public void IsPositionCodeApplicable(string PlantId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * from PlantWiseHRMSSetting
                            WHERE PlantId ='" + PlantId + @"' AND IsPositionCodeApplicable = 1";
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
        }//End of function  OK

        public void GetEmployeeNotifications(out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT * FROM [dbo].[EmployeeNotifications]
                           WHERE EventSourceTableSystemID IS NULL";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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

        #region Group

        public void GetAllGroup(out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM GroupCreation Order By GroupName";
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

        #endregion Group

        #region Plant

        public void GetPlantPrefix(string PlantID, out string pfx)
        {
            string strSQl;
            DataSet dsRef = null;
            ConnectionManager.DAL.ConManager objCon;
            pfx = string.Empty;
            try
            {
                //strSQl = "SELECT PKPrefixField FROM [ORG].Plant WHERE ID = '" + PlantID + "'";
                strSQl = "SELECT PlantPrefix FROM [dbo].[PlantWiseHRMSSetting] WHERE PlantID = '" + PlantID + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQl, out dsRef, false, false, "", "1");

                if (dsRef.Tables[0].Rows.Count > 0)
                {
                    pfx = dsRef.Tables[0].Rows[0]["PlantPrefix"].ToString();

                    if (pfx.Trim().Length == 0)
                    {
                        throw new Exception("No prefix found for this plant...");
                    }
                }
                else
                {
                    throw new Exception("No prefix found for this plant...");
                }
            }
            catch (Exception ex)
            { throw (ex); }
            finally
            {
                objCon = null;
            }
        }//End Function

        public void GetDataPlant(string PlantID, out DataSet dsRef)
        {
            string strSQl;

            ConnectionManager.DAL.ConManager objCon;

            try
            {
                strSQl = "SELECT * FROM [ORG].Plant WHERE ID = '" + PlantID.ToString() + "' ORDER BY UserName";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQl, out dsRef, false, false, "", "1");
            }
            catch (Exception ex)
            { throw (ex); }
            finally
            {
                objCon = null;
            }
        }//End Function

        public void GetPlantPositionCode(string PlantID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM dbo.PlantWiseHRMSSetting WHERE PlantID = '" + PlantID + "'";
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

        public void GetDataforComboPlant(string sGroupID, string CompanyID, out DataSet dsRef)
        {
            string strSQl;
            ConnectionManager.DAL.ConManager objCon;

            try
            {
                strSQl = @"SELECT ID PlantID, StandardName PlantName
                            FROM [ORG].Plant
                            WHERE CompanyGroupId = '" + sGroupID + @"' AND CompanyId = '" + CompanyID + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQl, out dsRef, false, false, "", "1");
            }
            catch (Exception ex)
            { throw (ex); }
            finally
            {
                objCon = null;
            }
        }//End Function

        public void SearchWorkGroupInfo(string sPlantId, string sCompanyId, out DataSet dsRef)
        {
            string strSQl;
            ConnectionManager.DAL.ConManager objCon;

            try
            {
                strSQl = @"SELECT * FROM [HKP].[WorkGroup] Where PlantId='" + sPlantId + @"'  AND CompanyId='" + sCompanyId + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQl, out dsRef, false, false, "", "1");
            }
            catch (Exception ex)
            { throw (ex); }
            finally
            {
                objCon = null;
            }
        }//End Function

        public void GetBankInfo(string sGroupID, string CompanyID, string strKey, out DataSet dsRef)
        {
            string strSql;
            ConnectionManager.DAL.ConManager objCon;

            try
            {
                strSql = @"select * from (select
                                b.Id BankId,b.UserName BankName,bb.UserName BranchName,bb.Id BranchId
                                from [HKP].[Bank] b
                                left outer join [HKP].[BankBranch] bb on bb.BankId=b.Id) x
                                ";
                if (strKey.Trim() != "")
                {
                    strSql = strSql + " WHERE " + strKey + "";
                }

                strSql = strSql + " Order By BankName,BranchName";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
            }
            catch (Exception ex)
            { throw (ex); }
            finally
            {
                objCon = null;
            }
        }//End Function

        public void GetDataPlantWiseHRMSSetting(string sGroupID, string PlantID, out DataSet dsRef)
        {
            string strSQl;

            ConnectionManager.DAL.ConManager objCon;

            try
            {
                strSQl = @"SELECT * FROM PlantWiseHRMSSetting
                            WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + PlantID.ToString() + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQl, out dsRef, false, false, "", "1");
            }
            catch (Exception ex)
            { throw (ex); }
            finally
            {
                objCon = null;
            }
        }//End Function


        public void GetIncrementHistory(string pEmpSystemId, string pToEffectiveDate, out DataSet dsRef)
        {
            string strSQl;

            ConnectionManager.DAL.ConManager objCon;

            try
            {
                strSQl = @"select * from IncrementHistory
                            WHERE EmpSystemId = '" + pEmpSystemId + @"' AND ToEffectiveDate = '" + pToEffectiveDate.ToString() + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQl, out dsRef, false, false, "", "1");
            }
            catch (Exception ex)
            { throw (ex); }
            finally
            {
                objCon = null;
            }
        }//End Function

        #endregion Plant

        #region Company

        public void SearchCompany(string strKey, string strGroupID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (strKey.Length == 0)
                {
                    strKey = "1=1";
                }

                strSQL = @"SELECT * FROM (
                            SELECT C.CompanyID, C.CompanyName, C.ShortName, C.FullName, C.GroupID, GC.GroupName,
                            C.Type, C.City, C.PostalCode, C.CountryID, CTR.Name AS CountryName, C.MobileNo, C.EmailNo, C.IsActive, C.TIN, C.CODE
                            FROM Company AS C
                            LEFT OUTER JOIN GroupCreation AS GC ON C.GroupID=GC.GroupID
                            LEFT OUTER JOIN Country AS CTR ON C.CountryID=CTR.CountryID
                            WHERE C.GroupID='" + strGroupID + @"'
                            ) AS Comp
                            WHERE " + strKey + @"
                            Order By CompanyName";

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

        public void GetCompanyGroupWise(string GroupID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT C.* FROM Company C
                                INNER JOIN dbo.CompanyAndApplicableModulesAssignment CA ON C.CompanyID = CA.CompanyID
                                                    AND ApplicableModuleID = 'HRMS'
                            Where C.GroupID = '" + GroupID.ToString().Trim() + @"'
                            ORDER BY C.Code";

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

        public void GetCompanyWise(string CompanyID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT C.* FROM Company C
                                INNER JOIN dbo.CompanyAndApplicableModulesAssignment CA ON C.CompanyID = CA.CompanyID
                                                    AND ApplicableModuleID = 'HRMS'
                            Where C.CompanyID = '" + CompanyID.ToString().Trim() + @"'
                            ORDER BY C.Code";

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

        #endregion Company

        #region GENERAL

        public string GetColumnNameForXls(int ColumnNo)
        {
            /*
             * Added By Shohel
             * This Function get the number of the column as (int) and Return the Name of the Column as (string)
             * ColumnNo must be greater or equal 1
             * As for Example:
             *          1. If the ColumnNo is equal 3 then this Function returns "C"
             *          2. If the ColumnNo is equal 27 then this Function returns "AA"
             *          3. If the ColumnNo is equal 53 then this Function returns "BA"
            */

            ColumnNo = ColumnNo - 1;
            if (ColumnNo < 0)
            {
                return "";
            }

            int CharVelue1 = 0, CharVelue2 = 0;
            char ch1, ch2;
            string ColumnName;
            int reminder, div;

            reminder = ColumnNo % 26;
            div = ColumnNo / 26;

            if (div == 0)
            {
                CharVelue1 = 65;
                CharVelue1 = CharVelue1 + reminder;
            }
            if (div > 0)
            {
                CharVelue1 = 65;
                CharVelue2 = 65;
                CharVelue1 = CharVelue1 + div;
                CharVelue2 = CharVelue2 + reminder;
            }

            if (CharVelue2 == 0)
            {
                ch1 = (char)CharVelue1;
                ColumnName = "" + ch1;
            }
            else
            {
                CharVelue1 = CharVelue1 - 1;
                ch1 = (char)CharVelue1;
                ch2 = (char)CharVelue2;
                ColumnName = "" + ch1 + ch2;
            }

            return ColumnName;
        }//End Function

        public void LoadDDLfromDatagrid(ref DataGrid SourceDG, ref DropDownList DestDDL)
        {
            Type tp = null;
            DataTable DT = null;
            DataRow DR = null;

            try
            {
                if (SourceDG.Columns.Count > 0)
                {
                    if (DestDDL.Items.Count > 0)
                    {
                        DestDDL.Items.Clear();
                    }

                    DT = new DataTable();
                    DT.Columns.Add("Value");
                    DT.Columns.Add("Text");
                    for (int ColCount = 0; ColCount < SourceDG.Columns.Count; ColCount++)
                    {
                        tp = SourceDG.Columns[ColCount].GetType();
                        if (tp.Name == "BoundColumn")
                        {
                            if (((BoundColumn)SourceDG.Columns[ColCount]).Visible == false)
                            {
                                continue;
                            }
                            else
                            {
                                DR = DT.NewRow();
                                DR["Value"] = ((BoundColumn)SourceDG.Columns[ColCount]).DataField.ToString();
                                DR["Text"] = ((BoundColumn)SourceDG.Columns[ColCount]).HeaderText.ToString();
                                DT.Rows.Add(DR);
                                //DestDDL.Items.Add(((BoundColumn)SourceDG.Columns[ColCount]).DataField.ToString());
                                //DestDDL.Items[ddlItem].Value=((BoundColumn)SourceDG.Columns[ColCount]).DataField.ToString();
                                //DestDDL.Items[ddlItem].Text = ((BoundColumn)SourceDG.Columns[ColCount]).HeaderText.ToString();
                            }
                        }
                    }
                    DestDDL.DataSource = DT;
                    DestDDL.DataValueField = "Value";
                    DestDDL.DataTextField = "Text";
                    DestDDL.DataBind();
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                tp = null;
                DT = null;
                DR = null;
            }
        }//End Function
        public void ClearLabel(params Label[] lbl)
        {
            try
            {
                foreach (Label b in lbl)
                {
                    b.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
            }
        }//End Function

        public void SaveDataSets(ref DataSet[] dsRef)
        {
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                int i = 0;
                foreach (DataSet value in dsRef)
                {
                    if (dsRef[i] != null)
                    {
                        objCon.SaveDataSetThroughAdapter(ref dsRef[i], true, "1");
                        i = i + 1;
                    }
                    else
                    {
                        i = i + 1;
                    }
                }
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (IsTransactionStarted)
                {
                    objCon.RollBack();
                }
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function


        public void SaveDataSetsAndDelete(string EmpSystemId, string LeaveTransactionId, params DataSet[] dsRef)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                objCon.ExecuteNonQueryWrapper("delete from MaternityBenefitDetail where MaternityBenefitMasterId in (select Id from MaternityBenefitMaster where EmpSystemId='" + EmpSystemId + @"' and LeaveTransactionId='" + LeaveTransactionId + @"')", true, "1");
                objCon.ExecuteNonQueryWrapper(" delete  from MaternityBenefitMaster where EmpSystemId='" + EmpSystemId + @"' and LeaveTransactionId='" + LeaveTransactionId + @"' and IsPaidBefore=0 and IsPaidAfter=0 ", true, "1");
                int i = 0;
                foreach (DataSet value in dsRef)
                {
                    if (dsRef[i] != null)
                    {
                        objCon.SaveDataSetThroughAdapter(ref dsRef[i], true, "1");
                        i = i + 1;
                    }
                    else
                    {
                        i = i + 1;
                    }
                }
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception)

            {
                try
                {
                    if (IsTransactionStarted)
                    {
                        objCon.RollBack();
                    }
                }
                catch (Exception exx)
                {

                    throw (exx);

                }


            }
            finally
            {

                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void SaveDataSets(params DataSet[] dsRef)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                int i = 0;
                foreach (DataSet value in dsRef)
                {
                    if (dsRef[i] != null)
                        if (dsRef[i].Tables.Count > 0)
                            objCon.SaveDataSetThroughAdapter(ref dsRef[i], true, "1");
                    i++;
                }
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                try
                {
                    if (IsTransactionStarted)
                    {
                        objCon.RollBack();
                    }
                    objCon.CloseConnection();
                }
                catch (Exception exp)
                {
                    
                    throw exp;
                }
            }
            finally
            {
                objCon = null;
            }
        }//End Function
        public void SaveDataSetsWithQuery(List<string> ExecutableQueries, params DataSet[] dsRef)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                int i = 0;
                foreach (DataSet value in dsRef)
                {
                    if (dsRef[i] != null)
                        if (dsRef[i].Tables.Count > 0)
                            objCon.SaveDataSetThroughAdapter(ref dsRef[i], true, "1");
                    i++;
                }

                if (ExecutableQueries != null)
                {
                    for (int Q = 0; Q < ExecutableQueries.Count; Q++)
                    {
                        objCon.executeQuery(ExecutableQueries[Q]);
                    }
                }

                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception)
            {
                try
                {
                    if (IsTransactionStarted)
                    {
                        objCon.RollBack();
                    }
                    objCon.CloseConnection();
                }
                catch (Exception exp)
                {
                    throw exp;
                }
            }
            finally
            {
                objCon = null;
            }
        }//End Function

        public void DeleteOD(string odmasterid)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                objCon.ExecuteNonQueryWrapper("", true, "1");
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (IsTransactionStarted)
                {
                    objCon.RollBack();
                }
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function
        public void SaveDataSetsWeekOffChange(string empid, string fromdate, string todate, params DataSet[] dsRef)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                objCon.ExecuteNonQueryWrapper("UPDATE [dbo].[AttdnRawData] SET ProcessedFlag = 0 WHERE LogDownLoadNum IN (" + empid + ") AND Pdate = '" + fromdate + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("UPDATE [dbo].[AttdnRawData] SET ProcessedFlag = 0 WHERE LogDownLoadNum IN (" + empid + ") AND Pdate = '" + todate + "'", true, "1");
                int i = 0;
                foreach (DataSet value in dsRef)
                {
                    if (dsRef[i] != null)
                    {
                        objCon.SaveDataSetThroughAdapter(ref dsRef[i], true, "1");
                        i = i + 1;
                    }
                    else
                    {
                        i = i + 1;
                    }
                }
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (IsTransactionStarted)
                {
                    objCon.RollBack();
                }
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        //public void SaveDataRemote(ref DataSet dsRef)
        //{
        //    SqlCommandBuilder sqlComBulder = null;
        //    SqlDataAdapter sqlDataAdapter = null;
        //    try
        //    {
        //        string strQuery = dsRef.ExtendedProperties["Query"].ToString();
        //        sqlDataAdapter = new SqlDataAdapter(strQuery, strConnString);
        //        sqlComBulder = new SqlCommandBuilder(sqlDataAdapter);
        //        sqlComBulder.GetInsertCommand();
        //        sqlComBulder.GetUpdateCommand();
        //        sqlComBulder.GetDeleteCommand();

        //        sqlDataAdapter.Update(dsRef);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw (ex);
        //    }
        //    finally
        //    {
        //        sqlComBulder = null;
        //        sqlDataAdapter = null;
        //    }
        //}

        public void SaveDataSetsSalaryStructure(string TaxDefineMasterSystemID, params DataSet[] dsRef)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;

                objCon.ExecuteNonQueryWrapper("DELETE FROM dbo.TaxDeductionInfoMonthWise WHERE TaxDefineMasterSystemID = '" + TaxDefineMasterSystemID + "' and isnull(ispaid,0)=0", true, "1");
                int i = 0;
                foreach (DataSet value in dsRef)
                {
                    if (dsRef[i] != null)
                    {
                        objCon.SaveDataSetThroughAdapter(ref dsRef[i], true, "1");
                        i = i + 1;
                    }
                    else
                    {
                        i = i + 1;
                    }
                }
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (IsTransactionStarted)
                {
                    objCon.RollBack();
                }
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void SaveEmployeeDocument(string sql)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;

                objCon.ExecuteNonQueryWrapper(sql, true, "1");

                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (IsTransactionStarted)
                {
                    objCon.RollBack();
                }
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void UpDateShitAndAttnd(string EmpSystemId, string pDate, params DataSet[] dsRef)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;

                objCon.ExecuteNonQueryWrapper("UPDATE AttdnRawData set ProcessedFlag = 0 WHERE LogDownLoadNum = '" + EmpSystemId + "' AND PDate >= '" + pDate + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("DELETE FROM [AttdnProcessData] WHERE EmpSystemID = '" + EmpSystemId + "' AND WorkDate >= '" + pDate + "'", true, "1");
                int i = 0;
                foreach (DataSet value in dsRef)
                {
                    if (dsRef[i] != null)
                    {
                        objCon.SaveDataSetThroughAdapter(ref dsRef[i], true, "1");
                        i = i + 1;
                    }
                    else
                    {
                        i = i + 1;
                    }
                }
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (IsTransactionStarted)
                {
                    objCon.RollBack();
                }
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void UpDateShitAndAttnd_SingleDate(string EmpSystemId, string pDate, params DataSet[] dsRef)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;

                objCon.ExecuteNonQueryWrapper("UPDATE AttdnRawData set ProcessedFlag = 0 WHERE LogDownLoadNum = '" + EmpSystemId + "' AND PDate = '" + pDate + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("DELETE FROM [AttdnProcessData] WHERE EmpSystemID = '" + EmpSystemId + "' AND WorkDate = '" + pDate + "'", true, "1");
                int i = 0;
                foreach (DataSet value in dsRef)
                {
                    if (dsRef[i] != null)
                    {
                        objCon.SaveDataSetThroughAdapter(ref dsRef[i], true, "1");
                        i = i + 1;
                    }
                    else
                    {
                        i = i + 1;
                    }
                }
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (IsTransactionStarted)
                {
                    objCon.RollBack();
                }
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void SaveDataSetsAndDelete(string EmpSystemId, string pDate, string fixedShiftId, params DataSet[] dsRef)
        {
            //throw new Exception("test");
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                objCon.ExecuteNonQueryWrapper("DELETE FROM EmployeeWeekOffByDay WHERE EmpSystemID = '" + EmpSystemId + "' AND EffectiveDate = '" + pDate + "' and FixSystemid<>'" + fixedShiftId + "'", true, "1");
                int i = 0;
                foreach (DataSet value in dsRef)
                {
                    if (dsRef[i] != null)
                    {
                        objCon.SaveDataSetThroughAdapter(ref dsRef[i], true, "1");
                        i = i + 1;
                    }
                    else
                    {
                        i = i + 1;
                    }
                }
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                if (IsTransactionStarted)
                {
                    objCon.RollBack();
                }
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function


        #endregion GENERAL

        #region Common function

        public void SaveData(ref DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.SaveDataSetThroughAdapter(ref dsRef, false, "1");
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

        public void DeleteUnapprovedSalaryStructure(string sEmpSystemID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM SalaryInfoDefine WHERE SalaryID IN (SELECT SystemID FROM SalaryInfoDefineMaster WHERE EmpInfoSystemID = '" + sEmpSystemID + @"' AND IsApproved = 0)", true, "1");
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM SalaryInfoDefineEffectiveDate WHERE SalaryID IN (SELECT SystemID FROM SalaryInfoDefineMaster WHERE EmpInfoSystemID = '" + sEmpSystemID + @"' AND IsApproved = 0)", true, "1");
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM SalaryInfoDefineMaster WHERE EmpInfoSystemID = '" + sEmpSystemID + @"' AND IsApproved = 0", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void DeleteData(string ID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("Delete from Contacts where ContactID='" + ID + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void DeleteSubCategoryData(string ContactID, string ContactTypeID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            string Sql = null;

            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                Sql = "Delete from ContactSubCategory where ContactID='" + ContactID.Trim() + "' and ContactTypeID='" + ContactTypeID.Trim() + "'";
                objCon.ExecuteNonQueryWrapper(Sql, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void DeleteSubCategoryDataRemote(string ContactID, string ContactTypeID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            string Sql = null;

            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                Sql = "Delete from " + REMOTELINKSERVER + "ContactSubCategory where ContactID='" + ContactID.Trim() + "' and ContactTypeID='" + ContactTypeID.Trim() + "'";
                objCon.ExecuteNonQueryWrapper(Sql, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void DeleteContactType(string strContactTypeID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("Delete from ContactTypes where ContactTypeID='" + strContactTypeID.Trim() + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        #endregion Common function

        #region User Creation

        public void OpenAuthKeyList(out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "Select * from " + AUTH_LINK_SERVER + "tblAuthKeyList";
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

        public void OpenAuthSignature(string strFieldName, string strEntryDate, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "Select * from " + AUTH_LINK_SERVER + "Signature where Field ='" + strFieldName.Trim() + "' and Dates = '" + strEntryDate + "'";
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

        public void DeleteModuleWiseUser_AuthKey(string strSystemID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

                objCon.ExecuteNonQueryWrapper("Delete from UserWiseITApplication Where UserSystemID='" + strSystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete from UserAccessToOtherMail Where UserSystemID='" + strSystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete from UserAccessToGroupMail Where UserSystemID='" + strSystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete from UserInfo Where SystemID='" + strSystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper("Delete from " + AUTH_LINK_SERVER + "tblAuthKeyList Where EntryID='" + strSystemID + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                }
                catch (Exception)
                {
                }
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        public void DeleteUserITApplication(string strSystemID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper("Delete from UserWiseITApplication Where SystemID='" + strSystemID + "'", true, "1");

                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function

        #endregion User Creation

        #region Tarek Functions

        public static string GetxlsCol(int intCol)
        {
            //returns excel columns based on column number. tested 1 to 256 column numbers
            try
            {
                if (intCol < 1 || intCol > 256)
                {
                    Exception ex = new Exception("Invalid Column Value");
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

        public static bool isInt(string num)
        {
            /**check whether a value is integer or not returns true if integer,
                                                 * false if floating or string containing alpahnumeric**/
            bool isInt;
            int number;
            try
            {
                isInt = Int32.TryParse(num, out number);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
            return isInt;
        }/**check whether a value is integer or not returns true if integer,

                                                 * false if floating or string containing alpahnumeric**/

        public static string nullrecorder(string str)
        {
            //this function returns an empty string(not a null) from null or empty or '&nbsp;' from the page
            if (str == "&nbsp;")
                str = "";
            if (string.IsNullOrEmpty(str) == true)
                str = "";

            return str;
        }//this function returns an empty string(not a null) from null or empty '&nbsp;' from the page

        public static string nullrecorder(object obj)
        {
            if (obj == null)
                return "";

            string str = obj.ToString();
            //this function returns an empty string(not a null) from null or empty or '&nbsp;' from the page
            if (str == "&nbsp;")
                str = "";
            if (string.IsNullOrEmpty(str) == true)
                str = "";

            return str;
        }

        public static int dateDiff(string firstDate, string lastDate)
        {
            /**return day difference in integer.
              Example 1: firstDate[Less Than]lastDate returns positive value
              Example 2: firstDate>lastDate returns negative value
              Example 3: firstDate=lastDate returns 0 [zero]**/
            int difference = 0;
            try
            {
                firstDate = Convert.ToDateTime(firstDate).ToString("dd-MMM-yyyy");
                lastDate = Convert.ToDateTime(lastDate).ToString("dd-MMM-yyyy");

                if (clsWebLib.IsDateOK(firstDate) == false)
                {
                    Exception ex = new Exception("Invalid [First Date]");
                    throw (ex);
                }
                if (clsWebLib.IsDateOK(lastDate) == false)
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
        }/**return day difference in integer.

              Example 1: firstDate[Less Than]lastDate returns positive value
              Example 2: firstDate>lastDate returns negative value
              Example 3: firstDate=lastDate returns 0 [zero]**/

        //valueFromGrid has two overloads
        public static string valueFromGrid(int rowIndex, string columnName, string textBoxName, ref DataGrid dg)
        {
            /**this function returns cell value from row index and column name instead of getting column index and row index
           * input variables:
           * rowIndex:: integer value, gets the current row index
           * columnName:: string value, gets the column name
           * textBoxName:: if the value returns from an bound text box inside the grid, this variable gets that text box name
           * dg:: data grid which value will be returned**/
            string value = "";
            int columnIndex = 0;
            bool foundColumnName = false;
            try
            {
                foreach (DataGridColumn gdColumn in dg.Columns)
                {
                    if (columnName.ToUpper() == gdColumn.HeaderText.ToUpper())//MATCHING [columnName] PARAMETER WITH BOUND COLUMN NAMES
                    {
                        foundColumnName = true;//GOT THE SELECTED COLUMN'S INDEX
                        columnIndex = dg.Columns.IndexOf(gdColumn);
                        break;
                    }
                }

                if (foundColumnName == true)
                {
                    if (textBoxName != "")
                    {
                        TextBox tbValue = (TextBox)dg.Items[rowIndex].Cells[columnIndex].FindControl(textBoxName);
                        value = nullrecorder(tbValue.Text.Trim());/**GETTING VALUE FROM TEXT BOX
                                                                                    INSIDE THE GRID IF USER PROVIDE THE [textBoxName]**/
                    }
                    else
                    {
                        value = nullrecorder(dg.Items[rowIndex].Cells[columnIndex].Text.Trim());/**IF NO TEXT BOX NAME PROVIDED THEN GETTING VALUE
                                                                                                                    FROM CELL **/
                    }
                }
                else
                {
                    Exception ex = new Exception("No column found. Column Name- [" + columnName + "] in datagrid [" + dg.ID.ToString() + "]");
                    throw (ex);//THROW AN EXCEPTION IF NO COLUMN FOUND WITH PROVIDED COLUMN NAME
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return value;
        } /**this function returns cell value from row index and column name instead of getting column index

           /**this function returns cell value from row index and column name instead of getting column index and row index
           * input variables:
           * rowIndex:: integer value, gets the current row index
           * columnName:: string value, gets the column name
           * textBoxName:: if the value returns from an bound text box inside the grid, this variable gets that text box name
           * dg:: data grid which value will be returned**/

        public static string valueFromGrid(int rowIndex, string fieldOrColumnName, ref DataGrid dg)
        {
            /**this function returns cell value from row index and column name instead of getting column index and row index
           * input variables:
           * rowIndex:: integer value, gets the current row index
           * columnName:: string value, gets the column name
           * dg:: data grid which value will be returned**/
            string value = "";
            int columnIndex = 0;
            bool foundColumnName = false;
            try
            {
                BoundColumn bfName;
                //first search by datafield name
                for (int intCnt = 0; intCnt < dg.Columns.Count; intCnt++)
                {
                    if (dg.Columns[intCnt].GetType().Name == "BoundColumn")
                    {
                        bfName = (BoundColumn)dg.Columns[intCnt];
                        if (fieldOrColumnName.ToUpper() == bfName.DataField.ToUpper())//MATCHING [fieldName] PARAMETER WITH BOUND COLUMN NAMES
                        {
                            foundColumnName = true;//GOT THE SELECTED COLUMN'S INDEX
                            columnIndex = intCnt;
                            break;
                        }
                    }
                }
                if (foundColumnName == false)
                {
                    foreach (DataGridColumn gdColumn in dg.Columns)
                    {
                        if (fieldOrColumnName.ToUpper() == gdColumn.HeaderText.ToUpper())//MATCHING [columnName] PARAMETER WITH BOUND COLUMN NAMES
                        {
                            foundColumnName = true;//GOT THE SELECTED COLUMN'S INDEX
                            columnIndex = dg.Columns.IndexOf(gdColumn);
                            break;
                        }
                    }
                }

                if (foundColumnName == true)
                {
                    value = nullrecorder(dg.Items[rowIndex].Cells[columnIndex].Text.Trim());/**GETTING VALUE
                                                                                                                    FROM CELL **/
                }
                else
                {
                    Exception ex = new Exception("No column found. data field/column Name- [" + fieldOrColumnName + "] in datagrid [" + dg.ID.ToString() + "]");
                    throw (ex);//THROW AN EXCEPTION IF NO COLUMN FOUND WITH PROVIDED COLUMN NAME
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return value;
        } /**this function returns cell value from row index and column name instead of getting column index and row index

           * input variables:
           * rowIndex:: integer value, gets the current row index
           * columnName:: string value, gets the column name
           * dg:: data grid which value will be returned**/

        public static string setValueToGrid(int rowIndex, string fieldOrColumnName, string value, ref DataGrid dg)
        {
            /**this function set cell value from row index and column name instead of getting column index and row index
           * input variables:
           * rowIndex:: integer value, gets the current row index
           * columnName:: string value, gets the column name
           * dg:: data grid which value will be returned**/

            int columnIndex = 0;
            bool foundColumnName = false;
            try
            {
                BoundColumn bfName;
                for (int intCnt = 0; intCnt < dg.Columns.Count; intCnt++)
                {
                    if (dg.Columns[intCnt].GetType().Name == "BoundColumn")
                    {
                        bfName = (BoundColumn)dg.Columns[intCnt];
                        if (fieldOrColumnName.ToUpper() == bfName.DataField.ToUpper())//MATCHING [fieldName] PARAMETER WITH BOUND COLUMN NAMES
                        {
                            foundColumnName = true;//GOT THE SELECTED COLUMN'S INDEX
                            columnIndex = intCnt;
                            break;
                        }
                    }
                }
                if (foundColumnName == false)
                {
                    foreach (DataGridColumn gdColumn in dg.Columns)
                    {
                        if (fieldOrColumnName.ToUpper() == gdColumn.HeaderText.ToUpper())//MATCHING [columnName] PARAMETER WITH BOUND COLUMN NAMES
                        {
                            foundColumnName = true;//GOT THE SELECTED COLUMN'S INDEX
                            columnIndex = dg.Columns.IndexOf(gdColumn);
                            break;
                        }
                    }
                }

                if (foundColumnName == true)
                {
                    dg.Items[rowIndex].Cells[columnIndex].Text = value;
                }
                else
                {
                    Exception ex = new Exception("No column found. data field Name- [" + fieldOrColumnName + "] in datagrid [" + dg.ID.ToString() + "]");
                    throw (ex);//THROW AN EXCEPTION IF NO COLUMN FOUND WITH PROVIDED COLUMN NAME
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return value;
        }

        public static string setValueToGrid(int rowIndex, string fieldOrColumnName, string textBoxName, string value, ref DataGrid dg)
        {
            /**this function set cell value from row index and column name instead of getting column index and row index
           * input variables:
           * rowIndex:: integer value, gets the current row index
           * columnName:: string value, gets the column name
           * dg:: data grid which value will be returned**/

            int columnIndex = 0;
            bool foundColumnName = false;
            TextBox txtbox = null;
            try
            {
                //BoundColumn bfName;
                //for (int intCnt = 0; intCnt < dg.Columns.Count; intCnt++)
                //{
                //    if (dg.Columns[intCnt].GetType().Name == "BoundColumn")
                //    {
                //        bfName = (System.Web.UI.WebControls.BoundColumn)dg.Columns[intCnt];
                //        if (fieldOrColumnName.ToUpper() == bfName.DataField.ToUpper())//MATCHING [fieldName] PARAMETER WITH BOUND COLUMN NAMES
                //        {
                //            txtbox = (TextBox)dg.Items[rowIndex].Cells[intCnt].FindControl(textBoxName);
                //            if (dg.Items[rowIndex].Cells[intCnt].Controls.Contains(txtbox) == true)
                //            {
                //                foundColumnName = true;//GOT THE SELECTED COLUMN'S INDEX
                //                columnIndex = intCnt;
                //                break;
                //            }
                //        }
                //    }

                //}
                if (foundColumnName == false)
                {
                    foreach (DataGridColumn gdColumn in dg.Columns)
                    {
                        if (fieldOrColumnName.ToUpper() == gdColumn.HeaderText.ToUpper())//MATCHING [columnName] PARAMETER WITH BOUND COLUMN NAMES
                        {
                            txtbox = (TextBox)dg.Items[rowIndex].Cells[dg.Columns.IndexOf(gdColumn)].FindControl(textBoxName);
                            if (dg.Items[rowIndex].Cells[dg.Columns.IndexOf(gdColumn)].Controls.Contains(txtbox) == true)
                            {
                                foundColumnName = true;//GOT THE SELECTED COLUMN'S INDEX
                                columnIndex = dg.Columns.IndexOf(gdColumn);
                                break;
                            }
                        }
                    }
                }

                if (foundColumnName == true)
                {
                    txtbox.Text = value;
                }
                else
                {
                    Exception ex = new Exception("No column/field found. data column/field Name- [" + fieldOrColumnName + "] in datagrid [" + dg.ID.ToString() + "]");
                    throw (ex);//THROW AN EXCEPTION IF NO COLUMN FOUND WITH PROVIDED COLUMN NAME
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }

            return value;
        }

        //validation
        public static void numericValidation(ref TextBox txtBox, bool isMandatory, bool isInteger, bool negativeAllowed, string fieldName)
        {
            try
            {
                if (isMandatory == true)
                {
                    if (txtBox.Text.Trim() == "")
                    {
                        Exception ex = new Exception("please insert [" + fieldName + "]");
                        txtBox.Focus();
                        throw (ex);
                    }
                    if (Convert.ToDouble(clsWebLib.GetNumData(txtBox.Text.Trim())) == 0)
                    {
                        Exception ex = new Exception("please insert [" + fieldName + "]");
                        txtBox.Focus();
                        throw (ex);
                    }

                    if (txtBox.Text.Trim() != "")
                    {
                        if (clsWebLib.IsNumeric(txtBox.Text.Trim()) == false)
                        {
                            Exception ex = new Exception("Invalid numeric value");
                            txtBox.Focus();
                            throw (ex);
                        }
                    }
                }

                if (txtBox.Text.Trim() != "")
                {
                    if (clsWebLib.IsNumeric(txtBox.Text.Trim()) == false)
                    {
                        Exception ex = new Exception("Invalid numeric value");
                        txtBox.Focus();
                        throw (ex);
                    }
                    if (isInteger == true)
                    {
                        if (isInt(txtBox.Text.Trim()) == false)
                        {
                            Exception ex = new Exception("Number must be integer");
                            txtBox.Focus();
                            throw (ex);
                        }
                    }
                    if (negativeAllowed == false)
                    {
                        if (Convert.ToDouble(clsWebLib.GetNumData(txtBox.Text.Trim())) < 0)
                        {
                            Exception ex = new Exception("Negative values are not allowed");
                            txtBox.Focus();
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

        public static int getColumnIndex(string columnName, ref DataGrid dg)
        {
            int columnIndex = 0;
            bool foundColumnName = false;
            try
            {
                foreach (DataGridColumn gdColumn in dg.Columns)
                {
                    if (columnName.ToUpper() == gdColumn.HeaderText.ToUpper())//MATCHING [columnName] PARAMETER WITH BOUND COLUMN NAMES
                    {
                        foundColumnName = true;//GOT THE SELECTED COLUMN'S INDEX
                        columnIndex = dg.Columns.IndexOf(gdColumn);
                    }
                }

                if (foundColumnName == true)
                {
                    return columnIndex;
                }
                else
                {
                    Exception ex = new Exception("No column found. Column Name- [" + columnName + "] in datagrid [" + dg.ID.ToString() + "]");
                    throw (ex);//THROW AN EXCEPTION IF NO COLUMN FOUND WITH PROVIDED COLUMN NAME
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
            //return columnIndex;
        }

        #endregion Tarek Functions

        #region

        public void GetAttdnLockData(string strGroupID, string strPlantID, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT * FROM AttdnLock
                                WHERE GroupID = '" + strGroupID + @"' AND PlantID = '" + strPlantID + @"'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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

        public bool GetAttdnLock(string strGroupID, string strPlantID, string strWorkDate)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            DataSet dsRef = null;

            bool blnStatus = false;

            try
            {
                strSql = @"SELECT * FROM dbo.AttdnProcessData
                                WHERE GroupID = '" + strGroupID + @"' AND PlantID = '" + strPlantID + @"'
                                        AND WorkDate = '" + strWorkDate + "' AND ToReprocess = 'No'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");

                if (dsRef.Tables[0].Rows.Count == 0)
                {
                    blnStatus = true;
                }
                else
                {
                    blnStatus = false;
                }
                return blnStatus;
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

        public void GetAttdnLock(string strGroupID, string strPlantID, string strWorkDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT * FROM dbo.AttdnProcessData
                                WHERE GroupID = '" + strGroupID + @"' AND PlantID = '" + strPlantID + @"'
                                        AND WorkDate > '" + strWorkDate + "'";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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

        public void GetAttdnDataForLock(string strGroupID, string strPlantID, string strWorkDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT * FROM dbo.AttdnProcessData
                            WHERE GroupID = '" + strGroupID + @"' AND PlantID = '" + strPlantID + @"'
                                AND WorkDate <= '" + strWorkDate + "' AND IsLock = 0";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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

        public void GetEmpDateWiseShiftAssignForLock(string strGroupID, string strPlantID, string strWorkDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT * FROM dbo.EmpDateWiseShiftAssign
                            WHERE GroupID = '" + strGroupID + @"' AND PlantID = '" + strPlantID + @"'
                                AND WorkDate <= '" + strWorkDate + "' AND AttdnLock = 0";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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

        public void GetFinalOTForLock(string strGroupID, string strPlantID, string strWorkDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT * FROM dbo.FinalOT
                            WHERE GroupID = '" + strGroupID + @"' AND PlantID = '" + strPlantID + @"'
                                AND WorkDate <= '" + strWorkDate + "' AND AttdnLock = 0";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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

        public void GetAttdnDataForUnLock(string strGroupID, string strPlantID, string strWorkDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT * FROM dbo.AttdnProcessData
                            WHERE GroupID = '" + strGroupID + @"' AND PlantID = '" + strPlantID + @"'
                                AND WorkDate >= '" + strWorkDate + "' AND IsLock = 1";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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

        public void GetEmpDateWiseShiftAssignForUnLock(string strGroupID, string strPlantID, string strWorkDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT * FROM dbo.EmpDateWiseShiftAssign
                            WHERE GroupID = '" + strGroupID + @"' AND PlantID = '" + strPlantID + @"'
                                AND WorkDate >= '" + strWorkDate + "' AND AttdnLock = 1";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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

        public void GetFinalOTForUnLock(string strGroupID, string strPlantID, string strWorkDate, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;

            try
            {
                strSql = @"SELECT * FROM dbo.FinalOT
                            WHERE GroupID = '" + strGroupID + @"' AND PlantID = '" + strPlantID + @"'
                                AND WorkDate >= '" + strWorkDate + "' AND AttdnLock = 1";

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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
        public void GetCompanyPlant(string CompanyID, bool isControlAdmin, bool isSysAdmin, string employeeId, string companyId, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var str = "";
                str = !isControlAdmin && !isSysAdmin ? @" and isnull(emp.SystemId,'') in (select systemid from EmployeeInformation e where e.BudgetCode in
                                    (select Id from mst.ManpowerBudget where EntityId in (select entityid from [HKP].[ApprovalConfiguration]
                                            where ProbationRP='" + employeeId + "')))" : @" AND Emp.CompanyId='" + companyId + "'";

                strSQL = @"SELECT PLC.ID PlantID, PLC.Code + '-' + PLC.UserName PlantName FROM [ORG].Plant PLC
                                WHERE CompanyID = '" + CompanyID.ToString() + @"' ORDER BY PLC.Sequence";

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
        public void GetEntityAttnProce(string PlantId, string employeeId, bool isControlAdmin, bool isSysAdmin, out DataSet dsRef)
        {
            string strSQ = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var strSQL = "";
                if (isControlAdmin == true || isSysAdmin == true)
                {

                    strSQL = @"SELECT E.Id as EntityId,e.UserName as EntityName FROM ORG.Entity e where e.plantId=" + PlantId;
                }
                else
                {

                    strSQL = @"SELECT E.Id as EntityId,e.UserName as EntityName FROM ORG.Entity e 
                                    where e.Id in (select EntityId from SEC.UserEntity where UserId =" + employeeId + " and PlantId=" + PlantId + @") 
                                    and e.plantId = " + PlantId;
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
        #endregion

        #region Employee

        public string GetEmployeeSQL()
        {
            string strSQL;
            try
            {
                strSQL = @"SELECT TOP (100) *
                                            FROM (
	                                            SELECT E.SystemID
                                                    ,CONVERT(INT, e.EmployeeCode)EmployeeCode 
		                                            --,E.EmployeeCode
		                                            ,E.BudgetCode
		                                            ,pr.UserName Position
                                                    --,pr.PaymentLink
		                                            ,E.EmployeeName
		                                            ,Dsg.StandardName AS Designation
                                                    ,DsgGiv.UserName AS GivenDesignation
                                                    ,Ent.UserName as Entity
		                                            ,U.StandardName AS Unit
		                                            ,Dv.StandardName AS Division
		                                            ,De.StandardName AS Department
		                                            ,Se.StandardName AS Section
		                                            ,SuS.StandardName SubSection
		                                            ,REPLACE(Convert(VARCHAR(11), E.DOB, 106), ' ', '-') AS DOB
                                                    ,REPLACE(Convert(VARCHAR(11), E.DOJ, 106), ' ', '-') AS DOJ
		                                            ,REPLACE(Convert(VARCHAR(11), E.DOC, 106), ' ', '-') AS DOC
                                                    ,E.DOS
		                                            --,E.FatherName
		                                            --,E.MotherName
		                                            ,E.EmpType
		                                            ,E.EmploymentType EmploymentNature
		                                            ,E.NationalID
		                                            ,E.GenderID GenderName
		                                            ,EC.StandardName EmployeeType
                                                    ,E.GroupID,E.CompanyID,E.PlantId
                                                    ,E.EmployeeStatus
                                                    --,E.UnitId,E.DivisionId,E.DepartmentId,E.SectionId,E.SubSectionId,E.SubdivisionID
													--,E.LineId
                                                    --,E.DesignationGroupId
                                                    --,E.DesignationSystemID
                                                    --,E.PositionID
													--,E.GivenDesignationId
                                                    --,E.LegalDesignationId
	                                            FROM EmployeeInformation AS E
	                                            LEFT OUTER JOIN [MST].[ManpowerBudget] PMB ON pmb.Id = e.BudgetCode
	                                            LEFT OUTER JOIN [ORG].[Position] PR ON PR.Id = PMB.PositionId
                                                LEFT OUTER JOIN [ORG].[Entity] Ent ON PMB.EntityId=Ent.Id
	                                            LEFT OUTER JOIN [HKP].[EmployeeCategory] AS EC ON E.EmployeeCategorySystemID = EC.ID
	                                            LEFT OUTER JOIN [ORG].[Unit] AS U ON U.ID = E.UnitID
	                                            LEFT OUTER JOIN [ORG].Division AS Dv ON Dv.ID = E.DivisionID
	                                            LEFT OUTER JOIN [ORG].Department AS De ON De.ID = E.DepartmentID
	                                            LEFT OUTER JOIN [HKP].Designation AS Dsg ON Dsg.ID = E.DesignationSystemID
                                                LEFT OUTER JOIN [HKP].Designation AS DsgGiv ON DsgGiv.ID = E.GivenDesignationId
	                                            LEFT OUTER JOIN [ORG].Section AS Se ON Se.ID = E.SectionID
	                                            LEFT OUTER JOIN [ORG].SubSection AS SuS ON SuS.ID = E.SubSectionID) A";

                return strSQL;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
            }
        }//End Function

        #endregion

        public void GetCompanyPlant(string CompanyID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT PLC.ID PlantID, PLC.Code + '-' + PLC.UserName PlantName FROM [ORG].Plant PLC
                                WHERE CompanyID = '" + CompanyID.ToString() + @"' ORDER BY PLC.Sequence";

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

        //public void GetCompanyPlant(string CompanyID, bool isControlAdmin, bool isSysAdmin, string employeeId, string companyId, out DataSet dsRef)
        //{
        //    string strSQL;
        //    ConnectionManager.DAL.ConManager objCon;
        //    try
        //    {
        //        var str = "";
        //        str = !isControlAdmin && !isSysAdmin ? @" and isnull(emp.SystemId,'') in (select systemid from EmployeeInformation e where e.BudgetCode in
        //                            (select Id from mst.ManpowerBudget where EntityId in (select entityid from [HKP].[ApprovalConfiguration]
        //                                    where ProbationRP='" + employeeId + "')))" : @" AND Emp.CompanyId='" + companyId + "'";

        //        strSQL = @"SELECT PLC.ID PlantID, PLC.Code + '-' + PLC.UserName PlantName FROM [ORG].Plant PLC
        //                        WHERE CompanyID = '" + CompanyID.ToString() + @"' ORDER BY PLC.Sequence";

        //        objCon = new ConnectionManager.DAL.ConManager("1");
        //        objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
        //    }
        //    catch (Exception ex)
        //    {
        //        throw (ex);
        //    }
        //    finally
        //    {
        //        objCon = null;
        //    }
        //}//End Function

        public void GetMandatoryField(string sPlantID, string strScreenName, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = string.Empty;
            try
            {
                strSql = "SELECT * FROM MandatoryField WHERE (PlantID = '" + sPlantID + @"') AND ScreenName = '" + strScreenName + "' ORDER BY FieldName";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, false, "", "1");
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

        public void Check(TextBox txtbox, string caption)
        {
            try
            {
                if (string.IsNullOrEmpty(txtbox.Text.Trim()))
                {
                    Exception ex = new Exception(caption + " can not be blank...");
                    throw (ex);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Check(DropDownList ddl, string caption)
        {
            try
            {
                if (ddl.SelectedValue.Length == 0)
                {
                    Exception ex = new Exception(caption + " can not be blank...");
                    throw (ex);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Access

        public enum EnumAccess
        {
            CREATE, EDIT, DELETE
        }

        public void CheckAccess(Label create, Label edit, Label delete, EnumAccess ea)
        {
            try
            {
                if (EnumAccess.CREATE == ea)
                {
                    if (create.Text == "NO")
                        throw new Exception("Access Denied for Insert ... !!!");
                }
                else if (EnumAccess.EDIT == ea)
                {
                    if (edit.Text == "NO")
                        throw new Exception("Access Denied for Edit/Update ... !!!");
                }
                else
                {
                    if (delete.Text == "NO")
                        throw new Exception("Access Denied for Delete ... !!!");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //public void xLoadUserAccessInfo(string linkid, Label create, Label edit, Label delete)
        //{
        //    try
        //    {
        //        var menuActionList = new UserAuthService().GetAuthoorizedMenuActionList(linkid.Trim());//menu permission.
        //        if (menuActionList != null)
        //        {
        //            create.Text = menuActionList.Any(r => r.Action.ToUpper() == "CREATE") ? "YES" : "NO";
        //            edit.Text = menuActionList.Any(r => r.Action.ToUpper() == "EDIT") ? "YES" : "NO";
        //            delete.Text = menuActionList.Any(r => r.Action.ToUpper() == "DELETE") ? "YES" : "NO";
        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        throw (ex);
        //    }
        //    finally
        //    {
        //    }
        //}//End Function

        //public void LoadUserAccessInfo(string linkid, bool sa, bool ca, Label create, Label edit, Label delete)
        //{
        //    try
        //    {
        //        if (sa || ca)
        //        {
        //            //to bypass sys admin/ cotrl admin / power || Convert.ToBoolean(Session["pa"])
        //        }
        //        else//non adminn
        //        {
        //            var menuActionList = new UserAuthService().GetAuthoorizedMenuActionList(linkid.Trim());//menu permission.
        //            if (menuActionList != null)
        //            {
        //                create.Text = menuActionList.Any(r => r.Action.ToUpper() == "CREATE") ? "YES" : "NO";
        //                edit.Text = menuActionList.Any(r => r.Action.ToUpper() == "EDIT") ? "YES" : "NO";
        //                delete.Text = menuActionList.Any(r => r.Action.ToUpper() == "DELETE") ? "YES" : "NO";
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw (ex);
        //    }
        //    finally
        //    {
        //    }
        //}//End Function

        #endregion

        #region Tax

  
        #endregion

        #region Common

        /// <summary>
        /// Default UserName and Id
        /// </summary>
        /// <param name="ds"></param>
        /// <param name=""></param>
        public void LoadDDL(DataSet ds, string displayField, string valueField, DropDownList ddl)
        {
            ddl.DataSource = ds;
            ddl.DataTextField = displayField;
            ddl.DataValueField = valueField;
            ddl.DataBind();
            ddl.Items.Insert(0, "ALL");
            ddl.SelectedIndex = -1;
        }

        public void LoadDDLInitBlank(DataSet ds, string displayField, string valueField, DropDownList ddl)
        {
            ddl.DataSource = ds;
            ddl.DataTextField = displayField;
            ddl.DataValueField = valueField;
            ddl.DataBind();
            ddl.Items.Insert(0, " ");
            ddl.SelectedIndex = -1;
        }
        public void LoadDDLNoGroup(DataSet ds, string displayField, string valueField, DropDownList ddl)
        {
            ddl.DataSource = ds;
            ddl.DataTextField = displayField;
            ddl.DataValueField = valueField;
            ddl.DataBind();
            ddl.Items.Insert(0, "No Group");
            ddl.SelectedIndex = -1;
        }

        public void LoadDDL(DataTable dt, string displayField, string valueField, DropDownList ddl)
        {
            ddl.DataSource = dt;
            ddl.DataTextField = displayField;
            ddl.DataValueField = valueField;
            ddl.DataBind();
            ddl.Items.Insert(0, "ALL");
            ddl.SelectedIndex = -1;
            //if (ddl.Items.Count == 2)
            //{
            //    ddl.SelectedIndex = 1;
            //}
            //else
            //{
            //    ddl.SelectedIndex = -1;
            //}
        }

        public void LoadDDL(DataSet ds, DropDownList ddl, string initvalue)
        {
            ddl.DataSource = ds;
            ddl.DataTextField = "UserName";
            ddl.DataValueField = "Id";
            ddl.DataBind();
            ddl.Items.Insert(0, initvalue);
            ddl.SelectedIndex = -1;
            //if (ddl.Items.Count == 2)
            //{
            //    ddl.SelectedIndex = 1;
            //}
            //else
            //{
            //    ddl.SelectedIndex = -1;
            //}
        }

        public void LoadDDL(DataSet ds, DropDownList ddl)
        {
            ddl.DataSource = ds;
            ddl.DataTextField = "UserName";
            ddl.DataValueField = "Id";
            ddl.DataBind();
            ddl.Items.Insert(0, "ALL");
            ddl.SelectedIndex = -1;
            //if (ddl.Items.Count == 2)
            //{
            //    ddl.SelectedIndex = 1;
            //}
            //else
            //{
            //    ddl.SelectedIndex = -1;
            //}
        }

        public void LoadDDL(DataTable dt, DropDownList ddl)
        {
            ddl.DataSource = dt;
            ddl.DataTextField = "UserName";
            ddl.DataValueField = "Id";
            ddl.DataBind();
            ddl.Items.Insert(0, "ALL");
            ddl.SelectedIndex = -1;
            //if (ddl.Items.Count == 2)
            //{
            //    ddl.SelectedIndex = 1;
            //}
            //else
            //{
            //    ddl.SelectedIndex = -1;
            //}
        }

        #endregion

        #region SQL Common

        public string EntityTables()
        {
            return @"                       LEFT JOIN org.Unit U ON E.UnitID = U.Id
                                            LEFT JOIN org.Division Dv ON E.DivisionID = Dv.Id
                                            LEFT JOIN org.SubDivision SubDv ON E.SubdivisionID = SubDv.Id
                                            LEFT JOIN org.Department Dp ON E.DepartmentID = Dp.Id
                                            LEFT JOIN org.Section S ON E.SectionID = S.Id
                                            LEFT JOIN org.SubSection SB ON E.SubSectionID = SB.Id
                                            LEFT JOIN org.Line L ON E.LineID = L.Id
                                            LEFT JOIN hkp.DesignationGroup DG ON E.DesignationGroupID = Dg.Id
                                            LEFT JOIN hkp.Designation D ON E.DesignationSystemID = D.Id
                                            LEFT JOIN hkp.Designation GVD ON E.GivenDesignationId = GVD.Id
                                            LEFT JOIN hkp.LegalDesignation LD ON E.LegalDesignationId = LD.Id
                                            LEFT JOIN  HKP.LegalDesignation LDes ON LDes.Id = E.LegalDesignationId
                                            LEFT JOIN MST.LegalSalaryGradeDesignation LSGD ON LSGD.LegalDesignationId = LDes.Id
                                            LEFT JOIN SCS.LegalSalaryGrade LSalGr ON LSalGr.Id = LSGD.LegalSalaryGradeId
                                            LEFT JOIN
                                            --hkp.EmployeeCategory EC ON E.EmployeeCategorySystemID = EC.Id
                                            (
                                            SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
											LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
											)EC ON EC.DesignationId=E.GivenDesignationId";
        }

        public string EntityColumns()
        {
            return @" 	, DG.UserName DesignationGroup
		                , D.UserName Designation
		                , ISNULL(LD.UserName,GVD.UserName) LDDesignationGD
		                , ISNULL(LocLangLD.Name,LocLangGD.Name) DesignationLocal
		                , L.UserName Line
		                , U.UserName Unit
		                , Dv.UserName Division
                        , SubDv.UserName SubDivision
		                , Dp.UserName Department
		                , S.UserName Section
		                , SB.UserName SubSection
		                , EC.UserName AS EmpCategory
                        , LSalGr.Code GradeCode";
        }

        public string EntityAlias()
        {
            return @"   , DesignationGroup
	                    , Designation
                        , GivenDesignation
		                , EmpCategory
		                , Line
		                , SubSection
		                , Section
		                , Department
		                , Division
		                , Unit ";
        }

        #endregion
    } //end class

    public class IdentityParameter
    {
        public string CompanyGroupId { get; set; }
        public string CompanyId { get; set; }
        public string PlantId { get; set; }
        public string AddedBy { get; set; }
        public DateTime? AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }

    public static class Extensions
    {
        public static List<T> ToList<T>(this DataTable table) where T : new()
        {
            IList<PropertyInfo> properties = typeof(T).GetProperties().ToList();
            List<T> result = new List<T>();

            foreach (var row in table.Rows)
            {
                var item = CreateItemFromRow<T>((DataRow)row, properties);
                result.Add(item);
            }

            return result;
        }

        private static T CreateItemFromRow<T>(DataRow row, IList<PropertyInfo> properties) where T : new()
        {
            T item = new T();
            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(System.DayOfWeek))
                {
                    DayOfWeek day = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), row[property.Name].ToString());
                    property.SetValue(item, day, null);
                }
                else
                {
                    if (row[property.Name] == DBNull.Value)
                        property.SetValue(item, null, null);
                    else
                        property.SetValue(item, row[property.Name], null);
                }
            }
            return item;
        }
    }
} //end namespace