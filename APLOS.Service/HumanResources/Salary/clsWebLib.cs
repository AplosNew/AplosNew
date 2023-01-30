using System;
using System.Web.UI.WebControls;
using System.Data;
using System.Security.Cryptography;
using System.IO;
using System.Web.UI;
using Library.Crosscutting.Security;
//using Zen.Barcode;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Configuration;
using System.Threading;
using Library.Service.Helpers;

//using clsGeneral;

//GetBarCodeInCode128
namespace bplib
{
    public class clsWebLib
    {

        //public static readonly string DB_DATE_FORMAT = "MM/dd/yyyy";
        //public static readonly string STD_DATE_FORMAT = "dddd,MMMM dd,yyyy";
        //public static readonly string REPORT_DB_NAME = "BpNewReport.mdb";
        //public static readonly string GROUPID = "G2014-3";
        //public static readonly string GROUPNAME = "EPIC";
        //public static readonly string UserGroupImplementedMsg = "User Group Access Implemented";

        //public static readonly string MODULEID = "LGS";
        //public static readonly string FormName = "OMS Shipment & Procurement" + "&nbsp;&nbsp;";
        //public static readonly string PageTitle = "OMS Shipment & Procurement ";
        //public static readonly string FormName = "Logistics Management System" + "&nbsp;&nbsp;";
        //public static readonly string PageTitle = "Logistics Management System ";

        public static string PI_DECIMAL_POINT = "F2";
        public static string PI_DECIMAL_POINT_4DECIMAL = "F4";
        public static string CI_DECIMAL_POINT = "F2";
        public static string CI_DECIMAL_POINT_4DECIMAL = "F4";
        public static double DIFFERENCE_ALLOWED_FOR_PI_VALUE = 5.00;
        public static double DIFFERENCE_ALLOWED = 0.50;
        //public static readonly string SAPDownLoadFilePath = "PO Download";
        //public static readonly string DB_USER_ID = "bappaather";
        //public static readonly string DB_USER_PASSWORD = "bappaather";
        public static readonly string REPORT_LOCK_PASSWORD = "@popInternational!@#$%^&*";

        //public static readonly string DB_USER_ID = "sa";//DBUSERID
        //public static readonly string DB_USER_PASSWORD = "!!@@";//"123";//DBUSERPWD

        public static readonly string DB_DATE_FORMAT = "MM/dd/yyyy";
        public static readonly string STD_DATE_FORMAT = "dddd,MMMM dd,yyyy";
        public static readonly string REPORT_DB_NAME = "BpNewReport.mdb";

        //public static readonly string GROUPID = "CG20181";
        //public static readonly string GROUPID = CustomIdentity

        //public static readonly string GROUPID = "G2015-1";
        public static readonly string MODULEID = "HRMS";

        public static readonly string MODULE = "HR";

        //public static readonly string GROUPNAME = "PRATIBHA";
        //public static readonly string GROUPNAME = ConfigurationManager.AppSettings["GROUPNAME"];//"12345678";

        public static readonly string FormName = "HRMS" + "&nbsp;&nbsp;";

        //public static readonly string PageTitle = "PRATIBHA(HRMS) ";
        //public static readonly string PageTitle = GROUPNAME + "(" + MODULEID + ") ";

        public static readonly string ModuleName = "HRMS";
        public static readonly string AddNewString = "The  form is in Add Mode. A new data is going to create on press the [create] button below after finish the entry.";
        public static readonly string EditString = "The form is in Edit Mode. An existing data is going to be updated / deleted on press the [save] / [delete] button below..";
        public static readonly string DefaultString = "The form is in Default state. If you want to Add new data, press [Add New] button. If you want to edit any existing data please press [Edit] button.";
        //public static readonly string ReportString = "The form is in Default state. If you want to Add new data, press [Add New] button. If you want to edit any existing data please press [Edit] button.";

        public static readonly string MAXROW = "100"; // ConfigurationManager.AppSettings["MAXROW"];//"12345678";

        //public static readonly string REPORT_SARVER_NAME = "[" + ConfigurationManager.AppSettings["SERVER_NAME"] + "]";//"";
        //public static readonly string REPORT_DATABASE_NAME = ConfigurationManager.AppSettings["DATABASE_NAME"];//"";
        //public static readonly string REPORT_USER_ID = ConfigurationManager.AppSettings["REPORT_USER_ID"];//"sa";
        //public static readonly string REPORT_PASSWORD = ConfigurationManager.AppSettings["REPORT_PASSWORD"];//"";

        //public static readonly string DB_USER_ID = ConfigurationManager.AppSettings["DBUSERID"];//"12345678";
        //public static readonly string DB_USER_PASSWORD = ConfigurationManager.AppSettings["DBUSERPWD"];//"12345678";

        public static readonly string BONUS_BLOCK_Bonus = "Bonus";
        public static readonly string BONUS_BLOCK_YearlyBonus = "Yearly Bonus";
        public static readonly string BONUS_BLOCK_AttendanceBonus = "Attendance Bonus";

        public static readonly int EMP_BASIC_PK_PAD = 5;
        public static readonly int PrOId = 6;
        public static readonly string SUCCESS = "Data Saved Successfully !!!";
        public static readonly string DELETE = "Data Deleted Successfully !!!";
        public static readonly string LEAVETYPE = "'General','Leave Without Pay','Earn','Maternity'";


        public static readonly string EmployeeStatus_Separated = "Separated";//
        public static readonly string EMP_OTHER_STATUS = "'LONG ABSENTEEISM','TBS'";//'LONG ABSENTEEISM','TBS'
        public static readonly string Current_Status_LA = "LONG ABSENTEEISM";//LONG ABSENTEEISM
        public static readonly string Current_Status_TBS = "TBS";//LONG ABSENTEEISM
        public static readonly string PFHEADCATEGORY = "PF VOLUNTARY";//PF VOLUNTARY
        public static readonly string FESTIVAL_BONUS = "FESTIVAL BONUS";//PF VOLUNTARY
        public static readonly string BONUS_OTHER = "Other Bonus";//PF VOLUNTARY
        public static readonly string BONUS_STATUTORY = "Statutory Bonus";//PF VOLUNTARY
        public static readonly string BONUS_EXGRATIA = "Ex-Gratia";//PF VOLUNTARY

        //public static readonly string LEAVETYPE = "'Location Allowance' is not found in the applied Salary Rule for " + lblEmpName.Text + "";

        //public static readonly string AddNewString = "The  form is in Add Mode. A new data is going to create on press the [create] button below after finish the entry.";
        //public static readonly string EditString = "The form is in Edit Mode. An existing data is going to be updated / deleted on press the [save] / [delete] button below..";
        //public static readonly string DefaultString = "The form is in Default state. If you want to Add new data, press [Add New] button. If you want to edit any existing data please press [Edit] button.";

        /// <summary>
        /// REQUISITION_TYPE column is added in Table:Requisition Master(roll + trims)
        /// and to pass a hard-coded value for 'bank file', the below value [BANK_FILE] is stored here.
        /// </summary>
        public static readonly string REQUISITION_TYPE = "BANK_FILE";

        public static readonly string _BANKFILE_TRIMS_ISSUE_EDIT_MESSAGE = "Internally Transferred and Issued";

        #region customized function

        public clsWebLib()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public static string MaxRow()
        {
            return (MAXROW.Length == 0 ? "100" : MAXROW);
        }

        public static string DateValidationMsg(string FieldName)
        {
            return "Please Define " + FieldName + ".... (allowed format is  dd-MMM-yyyy ex: '05-Dec-2002')";
        }

        public static string GetProjectName()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsRef;
            try
            {
                string projectName = "";

                strSQL = @"SELECT * FROM GroupCreation WHERE GroupID='" + identity.CompanyGroupId + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSQL, out dsRef, false, "1");
                if (dsRef.Tables[0].Rows.Count > 0)
                {
                    projectName = dsRef.Tables[0].Rows[0]["ProjectName"].ToString().Trim();
                }

                return projectName;
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

        //public static string CIMSLinkServerString()
        //{
        //    string linkString = "";
        //    if ((string)ConfigurationManager.AppSettings["CIMS_SERVER_NAME"] != "")
        //    {
        //        linkString = ConfigurationManager.AppSettings["CIMS_SERVER_NAME"] + "." + ConfigurationManager.AppSettings["CIMS_DATABASE_NAME"] + "." + "dbo.";
        //    }
        //    else
        //    {
        //        linkString = ConfigurationManager.AppSettings["CIMS_DATABASE_NAME"] + "." + "dbo.";
        //    }

        //    return linkString;
        //}//end of function

        public static string BappsMultiLineStringBuilder(string str)
        {
            int i = 0;
            char[] param = { '\n' };
            char[] lineEnd = { '\r' };
            string ss = "";
            if (str.Trim() == "")
            {
                return ("");
            }
            string[] lines = str.Split(param);
            foreach (string s in lines)
            {
                lines[i++] = s.TrimEnd(lineEnd);
            }
            foreach (string line in lines)
            {
                ss = ss + line + "<br>";
            }

            return (ss);
        }//end of function

        public static object RetValidLen(string str, int How_Long_Should_It_Be)
        {
            string removechar = "";
            if (str.Trim() == "")
            {
                return (object)Convert.DBNull;
            }
            removechar = str.Trim();
            removechar = removechar.Replace("'", " ");
            if ((removechar.Trim()).Length > How_Long_Should_It_Be)
            {
                return (object)(removechar.Substring(1, How_Long_Should_It_Be));
            }
            else
            {
                return (object)removechar.Trim();
            }
        }//end of function
        public static object RetValidLen(object str)
        {
            if (str == null)
                return DBNull.Value;

            string removechar = "";
            if (str.ToString().Trim() == "")
            {
                return (object)Convert.DBNull;
            }
            removechar = str.ToString().Trim();
            removechar = removechar.Replace("'", " ");

            return (object)removechar.Trim();

        }//end of function

        public static object RetValidLen(string str)
        {
            if (string.IsNullOrEmpty(str))
                str = "";
            string removechar = "";
            if (str.Trim() == "")
            {
                return (object)Convert.DBNull;
            }
            removechar = str.Trim();
            removechar = removechar.Replace("'", " ");
            ////if ((removechar.Trim()).Length > How_Long_Should_It_Be)
            ////{
            ////    return (object)(removechar.Substring(1, How_Long_Should_It_Be));
            ////}
            ////else
            ////{
            ////    return (object)removechar.Trim();
            ////}
            return (object)removechar.Trim();
        }//end of function

        public static System.Drawing.Image GetBarCodeInCode128(string BarcodeText, string TextOnTop, string TextOnBottom)
        {
            //Code128BarcodeDraw barCode128 = BarcodeDrawFactory.Code128WithChecksum;
            System.Drawing.Image barcodeImg = null;// barCode128.Draw(BarcodeText.Trim(), 70, 2);

            //System.Drawing.Graphics gf = Graphics.FromImage(barcodeImg);
            //Pen p = new Pen(Color.White, 10);
            //gf.DrawRectangle(p, 0, 0, barcodeImg.Width, 10);//upper Part containing item Name
            //gf.DrawRectangle(p, 0, barcodeImg.Height - 10, barcodeImg.Width, 10);//Lower part containing itemCode
            //Color colorStringColor = System.Drawing.Color.Black;
            ////Set the alignment based on the coordinates
            //StringFormat stringformatWriteTextFormat = new StringFormat();
            //stringformatWriteTextFormat.Alignment = StringAlignment.Center;

            //if (string.IsNullOrEmpty(TextOnTop.Trim()) == false)
            //{
            //    gf.DrawString(TextOnTop.Trim(), new Font("Arial", 8, FontStyle.Bold), new SolidBrush(colorStringColor),
            //        new Point(barcodeImg.Width / 2, 0), stringformatWriteTextFormat);
            //}

            //if (string.IsNullOrEmpty(TextOnBottom.Trim()) == false)
            //{
            //    gf.DrawString(TextOnBottom.Trim(), new Font("Arial", 8, FontStyle.Bold), new SolidBrush(colorStringColor),
            //       new Point(barcodeImg.Width / 2, barcodeImg.Height - 12), stringformatWriteTextFormat);
            //}

            return barcodeImg;
        }//end of function

        public static Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        public static Bitmap ResizeImage(System.Drawing.Image image, int width, int height, GraphicsUnit GraphicsUnit)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, width, height, GraphicsUnit, wrapMode);
                }
            }

            return destImage;
        }

        public static void LoadDataSetFromDataGrid(ref DataGrid dgSource, out DataSet dsDest)
        {
            Type T = null;
            DataRow drLocal = null;
            try
            {
                dsDest = new DataSet();
                dsDest.Tables.Add(new DataTable("dsFromDg"));

                //Adding Column Name To DataSource
                for (int ColCount = 0; ColCount < dgSource.Columns.Count; ColCount++)
                {
                    T = dgSource.Columns[ColCount].GetType();
                    //dsDest.Tables[0].Columns.Add(((BoundColumn)dgSource.Columns[ColCount]).DataField.ToString());
                    if (T.Name == "BoundColumn")
                    {
                        dsDest.Tables[0].Columns.Add(((BoundColumn)dgSource.Columns[ColCount]).DataField.ToString());
                    }
                    else if (T.Name == "TemplateColumn")
                    {
                        dsDest.Tables[0].Columns.Add(((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString());
                    }
                    //
                }

                //Adding Row To DataSource
                for (int rowCount = 0; rowCount < dgSource.Items.Count; rowCount++)
                {
                    drLocal = dsDest.Tables[0].NewRow();

                    for (int ColCount = 0; ColCount < dgSource.Columns.Count; ColCount++)
                    {
                        T = dgSource.Columns[ColCount].GetType();
                        if (T.Name == "BoundColumn")
                        {
                            if ((dgSource.Items[rowCount].Cells[ColCount].Text.ToString().Trim() != "&nbsp;") && (dgSource.Items[rowCount].Cells[ColCount].Text.ToString().Trim() != ""))
                            {
                                drLocal[((BoundColumn)dgSource.Columns[ColCount]).DataField.ToString()] = dgSource.Items[rowCount].Cells[ColCount].Text.ToString().Trim();
                            }
                            else
                            {
                                drLocal[((BoundColumn)dgSource.Columns[ColCount]).DataField.ToString()] = DBNull.Value;
                            }
                        }
                        else if (T.Name == "TemplateColumn")
                        {
                            if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "Balance")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((TextBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("TextBoxPaymentBalance")).Text.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "CheckValue")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = GetBoolData(((CheckBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("CheckBoxCheckValue")).Checked.ToString().Trim());
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "OrderQty")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((TextBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("TextBoxOrderQty")).Text.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "Sort")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((TextBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("TextBoxSort")).Text.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "NewRow")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((TextBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("TextBoxNewRow")).Text.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "ddlUOM")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((DropDownList)dgSource.Items[rowCount].Cells[ColCount].FindControl("ddlUOM")).SelectedValue.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "ddlStorageLocation")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((DropDownList)dgSource.Items[rowCount].Cells[ColCount].FindControl("ddlStorageLocation")).SelectedValue.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "Axis")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((DropDownList)dgSource.Items[rowCount].Cells[ColCount].FindControl("ddlAxis")).SelectedValue.ToString().Trim();
                            }
                            ////else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "Axis")
                            ////{
                            ////    drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((TextBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("TextBoxAxis")).Text.ToString().Trim();
                            ////}
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "Qty")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((TextBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("txtQuantity")).Text.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "IssueQty")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((TextBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("txtIssueQty")).Text.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "OrderQty")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((TextBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("TextBoxOrderQty")).Text.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "CIQty")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((TextBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("TextBoxOrderQty")).Text.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "Amount")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((TextBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("TextBoxAmount")).Text.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "Topic")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((TextBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("TextBoxTopic")).Text.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "Details")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((TextBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("TextBoxDetails")).Text.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "Remarks")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((TextBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("txtRemsrks")).Text.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "ItemIndentQty")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((TextBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("TextBoxItemIndentQty")).Text.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "Rate")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((TextBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("TextBoxRate")).Text.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "Advance")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((TextBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("TextBoxAdvance")).Text.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "AdvanceAdj")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((TextBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("TextBoxAdvanceAdj")).Text.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "IsLock")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = GetBoolData(((CheckBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("CheckBoxLock")).Checked.ToString().Trim());
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "IsChecked")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = GetBoolData(((CheckBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("CheckBoxIsChecked")).Checked.ToString().Trim());
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "IsCheckBoxSelect")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = GetBoolData(((CheckBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("chkSelect")).Checked.ToString().Trim());
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "PrintFlag")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = GetBoolData(((CheckBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("CheckBoxPrintFlag")).Checked.ToString().Trim());
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "IsInclude")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = GetBoolData(((CheckBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("CheckBoxInclude")).Checked.ToString().Trim());
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "IsBudgetLock")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((CheckBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("CheckBoxIsBudgetLock")).Checked.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "IsTransctionLock")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((CheckBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("CheckBoxIsTransctionLock")).Checked.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "IsExchangeRateConfirmed")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((CheckBox)dgSource.Items[rowCount].Cells[ColCount].FindControl("CheckBoxIsExchangeRateConfirmed")).Checked.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "DIM1CharValue")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((Label)dgSource.Items[rowCount].Cells[ColCount].FindControl("lblDim1CharValue")).Text.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "DIM1BuyerLevelSpecification")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((Label)dgSource.Items[rowCount].Cells[ColCount].FindControl("lblDim1BuyerLevelSpecification")).Text.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "DIM1VendorLevelSpecification")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((Label)dgSource.Items[rowCount].Cells[ColCount].FindControl("lblDim1VendorLevelSpecification")).Text.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "DIM2CharValue")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((Label)dgSource.Items[rowCount].Cells[ColCount].FindControl("lblDim2CharValue")).Text.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "DIM2BuyerLevelSpecification")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((Label)dgSource.Items[rowCount].Cells[ColCount].FindControl("lblDim2BuyerLevelSpecification")).Text.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "DIM2VendorLevelSpecification")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((Label)dgSource.Items[rowCount].Cells[ColCount].FindControl("lblDim2VendorLevelSpecification")).Text.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "DIM3CharValue")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((Label)dgSource.Items[rowCount].Cells[ColCount].FindControl("lblDim3CharValue")).Text.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "DIM3BuyerLevelSpecification")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((Label)dgSource.Items[rowCount].Cells[ColCount].FindControl("lblDim3BuyerLevelSpecification")).Text.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "DIM3VendorLevelSpecification")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((Label)dgSource.Items[rowCount].Cells[ColCount].FindControl("lblDim3VendorLevelSpecification")).Text.ToString().Trim();
                            }
                            else if (((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString() == "BOMandSOwiseRMSystemIDSource")
                            {
                                drLocal[((TemplateColumn)dgSource.Columns[ColCount]).FooterText.ToString()] = ((LinkButton)dgSource.Items[rowCount].Cells[ColCount].FindControl("LinkButtonRMSource")).Text.ToString().Trim();
                            }
                            //
                        }
                    }

                    dsDest.Tables[0].Rows.Add(drLocal);
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                T = null;
                drLocal = null;
            }
        }//end function

        public static object RetValidLen(decimal sequence)
        {
            throw new NotImplementedException();
        }

        public static object RetValidLen(bool isDefault)
        {
            throw new NotImplementedException();
        }

        public static bool GetBoolData(string inputData)
        {

            if (string.IsNullOrEmpty(inputData) == true)//null or empty
            {
                return false;
            }
            else if (string.IsNullOrEmpty(inputData.Trim()) == true)//null or empty
            {
                return false;
            }
            else if (string.Compare(inputData.Trim(), "0", true) == 0)
            {
                return false;
            }
            else if (string.Compare(inputData.Trim(), "NO", true) == 0)
            {
                return false;
            }
            else if (string.Compare(inputData.Trim(), "FALSE", true) == 0)
            {
                return false;
            }
            else if (Convert.ToDouble(bplib.clsWebLib.GetNumData(inputData.Trim())) < 0)
                return false;


            return true;
        } // End Function
        public static bool GetBoolData(object Data)
        {
            if (Data == null)
                return false;

            string inputData = Data.ToString();
            if (string.IsNullOrEmpty(inputData) == true)//null or empty
            {
                return false;
            }
            else if (string.IsNullOrEmpty(inputData.Trim()) == true)//null or empty
            {
                return false;
            }
            else if (string.Compare(inputData.Trim(), "0", true) == 0)
            {
                return false;
            }
            else if (string.Compare(inputData.Trim(), "NO", true) == 0)
            {
                return false;
            }
            else if (string.Compare(inputData.Trim(), "FALSE", true) == 0)
            {
                return false;
            }
            else if (Convert.ToDouble(bplib.clsWebLib.GetNumData(inputData.Trim())) < 0)
                return false;


            return true;
        } // End Function

        public static string GetColumnNameForXls(int ColumnNo)
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

        #endregion customized function

        //Info message box

        #region Alert + confirm jscript message builder utill

        //Use of the function:
        //------------------------------------------------
        // for Alert

        //			System.Web.UI.Page this_page_ref=this;
        //			bplib.clsWebLib.BappsAlert(ref this_page_ref, "Error !! occurred in data saving process. Please see the log below for details.","bappskey1");
        //			this_page_ref=null;

        // For confirmation in isPostback False part u have rgoster the button with scrip.
        // To activate the delete conformation we need
        // add the delete button a  new additional attribute.
        // so on cancel it will not allow to post.
        // this following command ha to be added in if not postback
        // part. ---------- Bappa
        //bplib.clsWebLib.bappsConfirm(ref this.Button_delete,"Are you sure to delete this data");

        public static void BappsOpenDocument(ref Page aspxPage, string strFilePath, string strKey)
        {
            string strScript = "<script language=JavaScript>window.open('" + strFilePath + "','',width='500',height='700')</script>";
            if (aspxPage.ClientScript.IsStartupScriptRegistered(strKey) == false)
            {
                aspxPage.ClientScript.RegisterStartupScript(typeof(Page), strKey, strScript);
            }
        }//end of function

        public static void BappsOpenDocumentWide(ref Page aspxPage, string strFilePath, string strKey)
        {
            string strScript = "<script language=JavaScript>window.open('" + strFilePath + "','bapps','width='+screen.width+',height='+screen.height*0.90+',location=no,directories=no,menubar=no,toolbar=no,scrollbars=yes,status=yes,resizable=yes,left=0,top=0')</script>";
            if (aspxPage.ClientScript.IsStartupScriptRegistered(strKey) == false)
            {
                aspxPage.ClientScript.RegisterStartupScript(typeof(Page), strKey, strScript);
            }
        }//end of function

        public static void BappsAlert(ref Page aspxPage, string strMessage, string strKey)
        {
            string strScript = "<script language=JavaScript>alert('" + strMessage + "')</script>";
            if (aspxPage.ClientScript.IsStartupScriptRegistered(strKey) == false)
            {
                aspxPage.ClientScript.RegisterStartupScript(typeof(Page), strKey, strScript);
            }
        }//end of function

        //public static void BappsAlertAjax(ref Page aspxPage, string strMessage, string strKey)
        //{
        //    /*
        //     * Use this Function When Control in Between Update Panel and Trigger as Async or None.
        //     * Do Not Use thin function in PostBackTrigger. use BappsAlert() instead.
        //     * */

        //    string strScript = "alert('" + strMessage + "')";
        //    if (aspxPage.ClientScript.IsStartupScriptRegistered(strKey) == false)
        //    {
        //        ScriptManager.RegisterStartupScript(aspxPage, typeof(Page), strKey, strScript, true);
        //    }
        //}//end of function

        public static void bappsConfirm(ref Button btn, string strMessage)
        {
            btn.Attributes.Add("onclick", "return confirm('" + strMessage + "');");
        }//end of function

        #endregion Alert + confirm jscript message builder utill

        #region App Date / time type data managment

        //sample use
        //txtModificationDate.Text = bplib.clsutilib.DateData_DBToApp(dsLocal.Tables[0].Rows[0]["ModificationDate"].ToString()).ToString("d");
        //drLocal["ModificationDate"] =bplib.clsutilib.DateData_AppToDB(System.DateTime.Now,clsStartUp.DB_DATE_FORMAT);
        private static bool DateOkCheck(string strdate)
        {
            try
            {
                DateTime myDt = Convert.ToDateTime(strdate);
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

        public static object chk_NullDateData(object dateValue)
        {
            if (DateOkCheck("" + dateValue.ToString()) == false)
            {
                dateValue = "";
            }

            if (("" + dateValue.ToString()) == "")
            {
                DateTime dt = new DateTime(1901, 1, 1);
                dateValue = (object)dt;
            }
            return (object)dateValue;
        }

        public static DateTime AppDateConvert(object dateValue, string input_date_format, string output_date_format)
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
                        DateTime myDt = Convert.ToDateTime(strDate, InputFormat);
                        strDate = myDt.ToString(output_date_format);
                    }
                }
            }
            return Convert.ToDateTime(strDate);
        }// End of function

        public static DateTime DateData_AppToDB(object dateValue, string DB_Level_date_format)
        {
            string strDate = null;
            strDate = dateValue.ToString();
            if (DB_Level_date_format != "")
            {
                // Collecting the user terminal set format
                System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
                //By Monir
                strDate = AppDateConvert(strDate, "MM/dd/yyyy", getUserDateFormat()).ToShortDateString();
                //strDate = AppDateConvert(strDate, USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString(), DB_Level_date_format).ToString();
            }
            return Convert.ToDateTime(strDate);
        }// End of function

        public static DateTime DateData_DBToApp(object dateValue)
        {
            string strDate = null;
            strDate = dateValue.ToString();

            System.Globalization.DateTimeFormatInfo myDBDateFormat = new System.Globalization.CultureInfo("en-US", false).DateTimeFormat;
            strDate = DateData_DBToApp(dateValue, myDBDateFormat.ShortDatePattern.ToString()).ToString();
            return Convert.ToDateTime(strDate);
        }// End function

        public static DateTime DateData_DBToApp(object dateValue, string DB_Level_date_format)
        {
            try
            {
                string strDate = null;
                strDate = dateValue.ToString();
                if (DB_Level_date_format != "")
                {
                    // Collecting the user terminal set format
                    System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
                    strDate = AppDateConvert(strDate, DB_Level_date_format, USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString()).ToString();
                }
                return Convert.ToDateTime(strDate);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }// End of function

        public static String makeBaseBlank(object dateValue)
        {
            DateTime dt;
            dt = Convert.ToDateTime(dateValue.ToString());
            if (dt.Year == 1901)
            {
                return "";
            }
            else
            {
                return dateValue.ToString();
            }
        }// End of function

        public static string AppSysTimeFormat(object TimeValue)
        {
            string strTime = null;
            strTime = TimeValue.ToString();
            if (strTime != "")
            {
                System.Globalization.DateTimeFormatInfo AppTimeFormat = new System.Globalization.DateTimeFormatInfo();
                AppTimeFormat.ShortTimePattern = "HH:mm:ss";
                DateTime dt = Convert.ToDateTime(strTime, AppTimeFormat);
                strTime = dt.ToString();
            }
            return (string)strTime;
        } //End function

        public static string getUserDateFormat()
        {
            System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
            return USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString();
        }

        public static string getUserDateSeparator()
        {
            System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
            return USER_TERMINAL_DATE_FORMAT.DateSeparator.ToString();
        }

        #endregion App Date / time type data managment

        public static bool IsTimeOK(string strdate)
        {
            try
            {
                if (strdate.Length != 8)
                {
                    return false;
                }
                if (strdate.Substring(2, 1) != ":" && strdate.Substring(5, 1) != ":")
                {
                    return false;
                }
                DateTime myDt = Convert.ToDateTime(strdate);
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
        }//End Function

        public static string GetMonthName(string monthValue)
        {
            string _month = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(monthValue))
                {
                    throw new Exception("Value can not be blank...");
                }
                string val = GetNumData(monthValue);
                int _monthValue = Convert.ToInt16(val);

                if (_monthValue < 1)
                {
                    throw new Exception("Minimum value can be 1...");
                }
                if (_monthValue > 12)
                {
                    throw new Exception("Maximum value can be 12...");
                }

                switch (_monthValue)
                {
                    case 1:
                        _month = "JAN";
                        break;

                    case 2:
                        _month = "FEB";
                        break;

                    case 3:
                        _month = "MAR";
                        break;

                    case 4:
                        _month = "APR";
                        break;

                    case 5:
                        _month = "MAY";
                        break;

                    case 6:
                        _month = "JUN";
                        break;

                    case 7:
                        _month = "JUL";
                        break;

                    case 8:
                        _month = "AUG";
                        break;

                    case 9:
                        _month = "SEP";
                        break;

                    case 10:
                        _month = "OCT";
                        break;

                    case 11:
                        _month = "NOV";
                        break;

                    default:
                        _month = "DEC";
                        break;
                }
                return _month;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //
            }
        }// end function
        public static string GetMonthNameBangla(string monthValue)
        {
            string _month = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(monthValue))
                {
                    throw new Exception("Value can not be blank...");
                }
                string val = GetNumData(monthValue);
                int _monthValue = Convert.ToInt16(val);

                if (_monthValue < 1)
                {
                    throw new Exception("Minimum value can be 1...");
                }
                if (_monthValue > 12)
                {
                    throw new Exception("Maximum value can be 12...");
                }

                switch (_monthValue)
                {
                    case 1:
                        _month = "জানুয়ারী";
                        break;

                    case 2:
                        _month = "ফেব্রুয়ারী";
                        break;

                    case 3:
                        _month = "মার্চ";
                        break;

                    case 4:
                        _month = "এপ্রিল";
                        break;

                    case 5:
                        _month = "মে";
                        break;

                    case 6:
                        _month = "জুন";
                        break;

                    case 7:
                        _month = "জুলাই";
                        break;

                    case 8:
                        _month = "আগস্ট";
                        break;

                    case 9:
                        _month = "সেপ্টেম্বর";
                        break;

                    case 10:
                        _month = "অক্টোবর";
                        break;

                    case 11:
                        _month = "নভেম্বর";
                        break;

                    default:
                        _month = "ডিসেম্বর";
                        break;
                }
                return _month;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                //
            }
        }// end function

        #region DataType Checking

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
                DateTime myDt = Convert.ToDateTime(strdate);
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

        public static string xGetPath()
        {
            try
            {
                string rf = new AppSettingsReader().GetValue("ROOT_FOLDER", typeof(string)).ToString();
                return rf + "\\";
                //return rf +"/EmpPic\\";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                //
            }
        }// end function

        public static string GetPath(bool IsForRead)
        {
            try
            {
                //           if (IsForRead)
                //           {
                //string rf = new AppSettingsReader().GetValue("POPResources", typeof(string)).ToString();
                //return rf + "\\";
                //           }
                //           else
                //           {
                //               string rf = new AppSettingsReader().GetValue("ROOT_FOLDER", typeof(string)).ToString();
                //               return rf + "\\";
                //           }
                string rf = ResourcesPathReader.GetEmployeeDestinationPicPath();
                return rf;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                //
            }
        }// end function

        public static bool IsInteger(string strNumber)
        {
            int d;
            System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
            if (strNumber.Length == 0)
            {
                return false;
            }
            return int.TryParse(strNumber, System.Globalization.NumberStyles.Integer, n, out d);
        } // End Function

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

        public static string GetNumData(string strNumber)
        {
            double d;
            if (string.IsNullOrEmpty(strNumber))
                strNumber = "0";

            strNumber = strNumber.Replace(",", "");
            //System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
            if (strNumber.Trim() == "")
            { return "0"; }
            else if (Double.TryParse(strNumber, out d) == true)
            {
                return strNumber;
            }
            else
            {
                return "0";
            }
        }// end function

        public static string GetNumDataFourDecimal(string strNumber)
        {
            double d;
            System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
            if (strNumber.Trim() == "")
            { return "0.0000"; }
            else if (Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
            {
                return string.Format("{0:0.0000}", d);
            }
            else
            {
                return "0.0000";
            }
        }// end function

        public static string ChkDBNull(ref String FldValue)
        {
            string s;
            if (Convert.IsDBNull(FldValue) == true)
            {
                s = "";
            }
            else
            {
                s = (string)FldValue;
            }
            return s;
        } // End Function

        #endregion DataType Checking

        #region CheckNumeric_Lenchk

        public static string GetNumDataTwoDecimal(string strNumber)
        {
            double d;
            System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
            if (strNumber.Trim() == "")
            { return "0.00"; }
            else if (Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
            {
                return string.Format("{0:0.00}", d);
            }
            else
            {
                return "0.00";
            }
        }// end function

        public static bool ChkEntryNumeric(string str, char keyP)
        {
            int x = 0;
            int i;
            int KeyAssci;
            KeyAssci = Convert.ToInt32(keyP);
            if (KeyAssci == 8 || KeyAssci == 13)
            {
                return false;
            }
            if (KeyAssci == 46)
            {
                for (i = 1; i <= str.Length; ++i)
                {
                    x = String.CompareOrdinal(str, i, ".", 0, 1);
                    if (x == 0)
                    {
                        return true;
                    }
                }
            }
            if ((KeyAssci < 48 || KeyAssci > 57) && (KeyAssci != 46))
            {
                return true;
            }

            return false;
        }

        public static bool LenChk(String str, int lenStr, char KeyP)
        {
            if (Convert.ToInt32(KeyP) == 8)
            {
                return false;
            }
            if (str.Length >= lenStr)
            {
                return true;
            }
            return false;
        }

        #endregion CheckNumeric_Lenchk

        #region Support function for Ultra

        //Sample use
        //		private void LoadStaticInfo()
        //		{
        //
        //			string[] EntityTypes = {"CutNo","Pattern","Color","Size"};
        //			BS.clsStylePlan objPLan;
        //			try
        //			{
        //				objPLan = new BS.clsStylePlan();
        //				ulCboOrder.DataSource = CreateNewTableForArray("Order by", EntityTypes );
        //
        //				ulCboCritea.DataSource=CreateDataTableForCriteria();
        //			}
        //			catch(System.Exception ex)
        //			{
        //				MessageBox.Show(this, ex.ToString(),"System",MessageBoxButtons.OK, MessageBoxIcon.Error);
        //			}
        //		}//End function

        public static DataTable CreateNewTableForArray(string ColumnName, string[] arr_Value)
        {
            DataTable dtOut;
            try
            {
                DataColumn col;
                dtOut = new DataTable();
                col = new DataColumn();
                col.DataType = typeof(string);
                col.ColumnName = ColumnName;
                dtOut.Columns.Add(col);

                DataRow newrow;
                foreach (string strValue in arr_Value)
                {
                    newrow = dtOut.NewRow();
                    newrow[ColumnName] = strValue;
                    dtOut.Rows.Add(newrow);
                }
                return dtOut;
            }
            catch (Exception ex)
            { throw (ex); }
            finally
            {
                dtOut = null;
            }
        }//End function

        //ultraGridFinPlan.DataSource=BS.clsUltraUIManager.CreateNewVirtualTable( dsLocal.Tables[0],"FileEntryID","FileNo","Patern","CmbOrShade","Size","FoldeNo","QtyBook");

        public static DataTable CreateNewVirtualTable(DataTable dtINPut, params string[] ColumnNames)
        {
            DataTable dtOut;
            try
            {
                DataColumn col;
                dtOut = new DataTable();

                foreach (string colname in ColumnNames)
                {
                    col = new DataColumn();
                    col.DataType = dtINPut.Columns[colname].DataType;
                    col.ColumnName = dtINPut.Columns[colname].ColumnName;
                    col.AllowDBNull = dtINPut.Columns[colname].AllowDBNull;
                    dtOut.Columns.Add(col);
                    col = null;
                }

                DataRow newrow;
                foreach (DataRow row in dtINPut.Rows)
                {
                    newrow = dtOut.NewRow();
                    foreach (string colname in ColumnNames)
                    {
                        newrow[colname] = row[colname];
                    }
                    dtOut.Rows.Add(newrow);
                }

                return dtOut;
            }
            catch (Exception ex)
            { throw (ex); }
            finally
            {
                dtOut = null;
            }
        }//End function

        public static DataTable CreateNewVirtualTable(params string[] ColumnNames)
        {
            DataTable dtOut;
            int i = 0;
            try
            {
                DataColumn col;
                dtOut = new DataTable();

                foreach (string colname in ColumnNames)
                {
                    col = new DataColumn();
                    col.DataType = typeof(String);
                    col.MaxLength = 50;
                    col.ColumnName = colname;
                    col.AllowDBNull = true;
                    dtOut.Columns.Add(col);
                    col = null;
                }
                i = dtOut.Columns.Count;
                return dtOut;
            }
            catch (Exception ex)
            { throw (ex); }
            finally
            {
                dtOut = null;
            }
        }//End function

        #endregion Support function for Ultra

        #region Ultra settings

        #region WebCombo Setting

        //			public static void WebComboSetting(Infragistics.WebUI.WebCombo.WebCombo webCombo)
        //			{
        //
        //				//webCombo.ExpandEffects.Type = Infragistics.WebUI.WebCombo.ExpandEffectType.Fade;
        //				webCombo.ExpandEffects.Type = Infragistics.WebUI.WebCombo.ExpandEffectType.NotSet;
        //				//webCombo.Width = 300;
        //				webCombo.DropDownLayout.DropdownWidth = 300;
        //				webCombo.DropDownLayout.DropdownHeight = 120;
        //				webCombo.DropDownLayout.AllowColSizing=Infragistics.WebUI.UltraWebGrid.AllowSizing.Free;
        //				webCombo.DropDownLayout.ColWidthDefault=50;
        //				webCombo.DropDownLayout.HeaderStyle.BackgroundImage = "./BlueExplorer.gif";
        //
        //				webCombo.Font.Name="Tahoma";
        //				webCombo.Font.Size=System.Web.UI.WebControls.FontUnit.Point(8);
        //
        //				webCombo.Editable=true;
        //				webCombo.Enabled=true;
        //
        //				webCombo.JavaScriptFileName= "InfraSup/ig_common/WebGrid3/ig_webcombo3_1.js";
        //				webCombo.JavaScriptFileNameCommon= "InfraSup/ig_common/Scripts/ig_csom.js";
        //				webCombo.DropImage1="InfraSup/ig_common/WebGrid3/ig_cmboDown1.bmp";
        //				webCombo.DropImage2="InfraSup/ig_common/WebGrid3/ig_cmboDown2.bmp";
        //				webCombo.DropImageXP1="InfraSup/ig_common/WebGrid3/ig_cmboDownXP1.bmp";
        //				webCombo.DropImageXP2="InfraSup/ig_common/WebGrid3/ig_cmboDownXP2.bmp";
        //				webCombo.DropDownLayout.JavaScriptFileName="InfraSup/ig_common/WebGrid3/ig_WebGrid.js";
        //				webCombo.DropDownLayout.ImageUrls.ImageDirectory="InfraSup/ig_common/WebGrid3/";
        //			}

        #endregion WebCombo Setting

        #region WebDate Setting

        //			public static void WebDateChooserSetting(Infragistics.WebUI.WebSchedule.WebDateChooser webDateChooser)
        //			{
        //				//if (!IsPostBack)
        //					webDateChooser.ExpandEffects.Type = Infragistics.WebUI.WebDropDown.ExpandEffectType.Fade;
        //
        //				webDateChooser.AllowNull = false;
        //				webDateChooser.Height= 10;
        //				webDateChooser.Width = 100;
        //
        //				webDateChooser.DropButton.ImageUrl1="InfraSup/ig_common/webschedule1/igsch_xpblueup.gif";
        //				webDateChooser.DropButton.ImageUrl2="InfraSup/ig_common/webschedule1/igsch_xpbluedn.gif";
        //				webDateChooser.CalendarLayout.FooterFormat = "Today: {00:MM/dd/yyyy}";
        //
        //				webDateChooser.CalendarLayout.DayHeaderStyle.BackgroundImage = "./BlueExplorer.gif";
        //				webDateChooser.CalendarLayout.DayHeaderStyle.BorderStyle =System.Web.UI.WebControls.BorderStyle.Inset;
        //				webDateChooser.CalendarLayout.DayHeaderStyle.Font.Bold = true;
        //
        //				webDateChooser.CalendarLayout.CalendarStyle.BorderColor =System.Drawing.Color.Gainsboro;
        //
        //				webDateChooser.CalendarLayout.TitleStyle.BackgroundImage = "./BlueExplorer.gif";
        //				webDateChooser.CalendarLayout.TitleStyle.Font.Bold = true;
        //
        //				webDateChooser.CalendarLayout.ShowYearDropDown = false;
        //				webDateChooser.CalendarLayout.ShowMonthDropDown = false;
        //
        //				webDateChooser.CalendarLayout.NextPrevStyle.BackgroundImage = "./BlueExplorer.gif";
        //				webDateChooser.CalendarLayout.NextMonthImageUrl = "./btnNext.gif";
        //				webDateChooser.CalendarLayout.PrevMonthImageUrl = "./btnPrev.gif";
        //				webDateChooser.CalendarLayout.NextPrevStyle.BorderColor =System.Drawing.Color.Gainsboro;
        //				webDateChooser.CalendarLayout.NextPrevStyle.Font.Bold=true;
        //				webDateChooser.CalendarLayout.NextPrevStyle.Width =30;
        //
        //				webDateChooser.CalendarLayout.OtherMonthDayStyle.ForeColor =System.Drawing.Color.LightGray;
        //
        //				webDateChooser.CalendarLayout.FooterStyle.BackgroundImage = "./BlueExplorer.gif";
        //				webDateChooser.CalendarLayout.FooterStyle.BorderStyle = System.Web.UI.WebControls.BorderStyle.Inset;
        //				webDateChooser.CalendarLayout.FooterStyle.Font.Italic = true;
        //				webDateChooser.CalendarLayout.FooterStyle.Font.Bold = true;
        //				webDateChooser.CalendarLayout.FooterStyle.ForeColor =System.Drawing.Color.Indigo;
        //
        //				webDateChooser.CalendarLayout.WeekendDayStyle.ForeColor =System.Drawing.Color.Red;
        //
        //				webDateChooser.CalendarLayout.TodayDayStyle.Font.Italic = true;
        //				webDateChooser.CalendarLayout.TodayDayStyle.Font.Bold = true;
        //				webDateChooser.CalendarLayout.TodayDayStyle.ForeColor = System.Drawing.Color.Indigo;
        //
        //				webDateChooser.Font.Name="Tahoma";
        //				webDateChooser.Font.Size=System.Web.UI.WebControls.FontUnit.Point(8);
        //
        //				webDateChooser.EditStyle.Font.Name="Tahoma";
        //				webDateChooser.EditStyle.Font.Size=System.Web.UI.WebControls.FontUnit.Point(8);
        //
        //				webDateChooser.CalendarLayout.Calendar.Font.Name="Tahoma";
        //				webDateChooser.CalendarLayout.Calendar.Font.Size=System.Web.UI.WebControls.FontUnit.Point(8);
        //
        //
        //				webDateChooser.JavaScriptFileName="InfraSup/ig_common/webschedule1/ig_webdropdown.js";
        //				webDateChooser.JavaScriptFileNameCommon="InfraSup/ig_common/scripts/ig_csom.js";
        //				webDateChooser.CalendarJavaScriptFileName="InfraSup/ig_common/webschedule1/ig_calendar.js";
        //
        //			}

        #endregion WebDate Setting

        #region Web Grid Setting

        //public static void SetUltraWebGrid(Infragistics.WebUI.UltraWebGrid.UltraWebGrid UltrWebGrid, bool blnAddNew, bool blnUpdate, bool blnDelete)
        //{
        //    if (blnAddNew == true)
        //    {
        //        UltrWebGrid.DisplayLayout.AllowAddNewDefault = Infragistics.WebUI.UltraWebGrid.AllowAddNew.Yes;
        //        UltrWebGrid.DisplayLayout.AddNewBox.Hidden = false;
        //        //UltrWebGrid.DisplayLayout.AddNewBox.
        //    }
        //    if (blnUpdate == true)
        //    {
        //        UltrWebGrid.DisplayLayout.AllowUpdateDefault = Infragistics.WebUI.UltraWebGrid.AllowUpdate.Yes;
        //    }
        //    if (blnDelete == true)
        //    {
        //        UltrWebGrid.DisplayLayout.AllowDeleteDefault = Infragistics.WebUI.UltraWebGrid.AllowDelete.Yes;
        //    }
        //    UltrWebGrid.Font.Name = "Verdana";
        //    UltrWebGrid.Font.Size = 8;
        //    //new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
        //    UltrWebGrid.DisplayLayout.Bands[0].AllowColumnMoving = Infragistics.WebUI.UltraWebGrid.AllowColumnMoving.NotSet;
        //    UltrWebGrid.DisplayLayout.Bands[0].FooterStyle.BackColor = System.Drawing.Color.White;
        //    UltrWebGrid.DisplayLayout.AddNewBox.ButtonStyle.BackColor = System.Drawing.Color.CornflowerBlue;
        //    UltrWebGrid.DisplayLayout.AddNewBox.Style.BackColor = System.Drawing.Color.WhiteSmoke;
        //    UltrWebGrid.DisplayLayout.AddNewBox.ButtonStyle.Cursor = Infragistics.WebUI.Shared.Cursors.Hand;
        //    UltrWebGrid.DisplayLayout.Bands[0].AddButtonCaption = "Add New";
        //    //UltrWebGrid.DisplayLayout.Bands[0].HeaderStyle.BackgroundImage=@".\Picture\GridLooks.jpg";
        //    UltrWebGrid.DisplayLayout.Bands[0].HeaderStyle.BackColor = System.Drawing.Color.RoyalBlue;
        //    UltrWebGrid.DisplayLayout.Bands[0].HeaderStyle.ForeColor = System.Drawing.Color.White;

        //    UltrWebGrid.JavaScriptFileName = "InfraSup/ig_common/WebGrid3/ig_WebGrid.js";
        //    UltrWebGrid.JavaScriptFileNameCommon = "InfraSup/ig_common/Scripts/ig_csom.js";
        //} //end function

        #endregion Web Grid Setting

        #endregion Ultra settings

        public static void Throw(string msg)
        {
            try
            {
                throw new Exception(msg);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string MSG_Salary_Calculate(string SalaryHead, string emp)
        {
            try
            {
                return " <b>" + SalaryHead + "</b> is not found in the applied Salary Rule for <b>" + emp + "</b>,</br> but as per BudgetCode he must have <b>" + SalaryHead + "</b>";
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    public class clsEncryptDecrypt
    {
        #region all function are atatic

        public clsEncryptDecrypt()
        {
            // nothing to do
        }

        private static int ReturnPasswordValue(string strPassword)
        {
            int lngPos = 0;
            int lngCounter = 0;
            string s;
            char c;
            char[] charArray;

            while (lngPos < strPassword.Length)
            {
                s = strPassword.Substring(lngPos, 1).ToString();
                charArray = s.ToCharArray();
                c = charArray[0];

                lngCounter = lngCounter + Convert.ToInt32(c);
                lngPos = lngPos + 1;
            }
            return lngCounter;
        }// end of funtion

        public static string EncryptWord(string strPassword, string strUser)
        {
            int lngPos = 0;
            int lngPass = 0;
            string strEncryptText = "";
            long lngChar = 0;
            string s;
            char c;
            char[] charArray;
            Random rndm;
            try
            {
                rndm = new Random();
                if (strPassword.Trim().Length == 0)
                {
                    return "";
                }
                lngPass = ReturnPasswordValue(strUser);
                if (strUser.Trim().Length == 0)
                {
                    rndm.Next(5);
                }
                else
                {
                    rndm.Next(lngPass);
                }
                lngPos = 0;
                while (lngPos < strPassword.Length)
                {
                    s = strPassword.Substring(lngPos, 1).ToString();
                    charArray = s.ToCharArray();
                    c = charArray[0];

                    //lngChar =System.Convert.ToInt32((int)c)+ System.Convert.ToInt32((System.Math.Round(100.00)+ 1));

                    lngChar = Convert.ToInt32((int)c) + Convert.ToInt32(strPassword.Length);

                    if (lngChar > 255)
                    {
                        lngChar = lngChar - 255;
                    }
                    if (lngChar < 1)
                    {
                        lngChar = lngChar + 255;
                    }
                    strEncryptText = strEncryptText + Convert.ToChar(lngChar);
                    lngPos = lngPos + 1;
                }
                return strEncryptText;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                rndm = null;
            }
        }// end of funcion

        public static string DecryptWord(string strPassword, string strUser)
        {
            int lngPos = 0;
            int lngPass = 0;
            string strEncryptText = "";
            //				string strChar="";
            int lngChar = 0;
            string s;
            char c;
            char[] charArray;

            Random rndm;
            try
            {
                if (strPassword.Trim().Length == 0)
                {
                    return "";
                }
                lngPass = ReturnPasswordValue(strUser);
                rndm = new Random();
                if (strUser.Trim().Length == 0)
                {
                    rndm.Next(5);
                }
                else
                {
                    rndm.Next(lngPass);
                }
                lngPos = 0;

                while (lngPos < strPassword.Length)
                {
                    s = strPassword.Substring(lngPos, 1).ToString();
                    charArray = s.ToCharArray();
                    c = charArray[0];

                    //lngChar =System.Convert.ToInt32((int)c)- System.Convert.ToInt32((System.Math.Round(100.00)+ 1));

                    lngChar = Convert.ToInt32((int)c) - Convert.ToInt32(strPassword.Length);

                    if (lngChar > 255)
                    {
                        lngChar = lngChar - 255;
                    }
                    if (lngChar < 1)
                    {
                        lngChar = lngChar + 255;
                    }
                    strEncryptText = strEncryptText + Convert.ToChar(lngChar);
                    lngPos = lngPos + 1;
                }
                return strEncryptText;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                rndm = null;
            }
        }// end of function

        #endregion all function are atatic
    }

    public class EncDec
    {
        // Encrypt a byte array into a byte array using a key and an IV
        public static byte[] Encrypt(byte[] clearData, byte[] Key, byte[] IV)
        {
            // Create a MemoryStream to accept the encrypted bytes
            MemoryStream ms = new MemoryStream();

            // Create a symmetric algorithm.
            // We are going to use Rijndael because it is strong and
            // available on all platforms.
            // You can use other algorithms, to do so substitute the
            // next line with something like
            //      TripleDES alg = TripleDES.Create();
            Rijndael alg = Rijndael.Create();

            // Now set the key and the IV.
            // We need the IV (Initialization Vector) because
            // the algorithm is operating in its default
            // mode called CBC (Cipher Block Chaining).
            // The IV is XORed with the first block (8 byte)
            // of the data before it is encrypted, and then each
            // encrypted block is XORed with the
            // following block of plaintext.
            // This is done to make encryption more secure.

            // There is also a mode called ECB which does not need an IV,
            // but it is much less secure.
            alg.Key = Key;
            alg.IV = IV;

            // Create a CryptoStream through which we are going to be
            // pumping our data.
            // CryptoStreamMode.Write means that we are going to be
            // writing data to the stream and the output will be written
            // in the MemoryStream we have provided.
            CryptoStream cs = new CryptoStream(ms,
               alg.CreateEncryptor(), CryptoStreamMode.Write);

            // Write the data and make it do the encryption
            cs.Write(clearData, 0, clearData.Length);

            // Close the crypto stream (or do FlushFinalBlock).
            // This will tell it that we have done our encryption and
            // there is no more data coming in,
            // and it is now a good time to apply the padding and
            // finalize the encryption process.
            cs.Close();

            // Now get the encrypted data from the MemoryStream.
            // Some people make a mistake of using GetBuffer() here,
            // which is not the right way.
            byte[] encryptedData = ms.ToArray();

            return encryptedData;
        }

        // Encrypt a string into a string using a password
        //    Uses Encrypt(byte[], byte[], byte[])

        public static string Encrypt(string clearText, string Password)
        {
            Password = Password.ToUpper();
            // First we need to turn the input string into a byte array.
            byte[] clearBytes =
              System.Text.Encoding.Unicode.GetBytes(clearText);

            // Then, we need to turn the password into Key and IV
            // We are using salt to make it harder to guess our key
            // using a dictionary attack -
            // trying to guess a password by enumerating all possible words.
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,
                new byte[] {0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d,
            0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76});

            // Now get the key/IV and do the encryption using the
            // function that accepts byte arrays.
            // Using PasswordDeriveBytes object we are first getting
            // 32 bytes for the Key
            // (the default Rijndael key length is 256bit = 32bytes)
            // and then 16 bytes for the IV.
            // IV should always be the block size, which is by default
            // 16 bytes (128 bit) for Rijndael.
            // If you are using DES/TripleDES/RC2 the block size is
            // 8 bytes and so should be the IV size.
            // You can also read KeySize/BlockSize properties off
            // the algorithm to find out the sizes.
            byte[] encryptedData = Encrypt(clearBytes,
                     pdb.GetBytes(32), pdb.GetBytes(16));

            // Now we need to turn the resulting byte array into a string.
            // A common mistake would be to use an Encoding class for that.
            //It does not work because not all byte values can be
            // represented by characters.
            // We are going to be using Base64 encoding that is designed
            //exactly for what we are trying to do.
            return Convert.ToBase64String(encryptedData);
        }

        // Encrypt bytes into bytes using a password
        //    Uses Encrypt(byte[], byte[], byte[])

        public static byte[] Encrypt(byte[] clearData, string Password)
        {
            // We need to turn the password into Key and IV.
            // We are using salt to make it harder to guess our key
            // using a dictionary attack -
            // trying to guess a password by enumerating all possible words.
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,
                new byte[] {0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d,
            0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76});

            // Now get the key/IV and do the encryption using the function
            // that accepts byte arrays.
            // Using PasswordDeriveBytes object we are first getting
            // 32 bytes for the Key
            // (the default Rijndael key length is 256bit = 32bytes)
            // and then 16 bytes for the IV.
            // IV should always be the block size, which is by default
            // 16 bytes (128 bit) for Rijndael.
            // If you are using DES/TripleDES/RC2 the block size is 8
            // bytes and so should be the IV size.
            // You can also read KeySize/BlockSize properties off the
            // algorithm to find out the sizes.
            return Encrypt(clearData, pdb.GetBytes(32), pdb.GetBytes(16));
        }

        // Encrypt a file into another file using a password
        public static void Encrypt(string fileIn,
                    string fileOut, string Password)
        {
            // First we are going to open the file streams
            FileStream fsIn = new FileStream(fileIn,
                FileMode.Open, FileAccess.Read);
            FileStream fsOut = new FileStream(fileOut,
                FileMode.OpenOrCreate, FileAccess.Write);

            // Then we are going to derive a Key and an IV from the
            // Password and create an algorithm
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,
                new byte[] {0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d,
            0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76});

            Rijndael alg = Rijndael.Create();
            alg.Key = pdb.GetBytes(32);
            alg.IV = pdb.GetBytes(16);

            // Now create a crypto stream through which we are going
            // to be pumping data.
            // Our fileOut is going to be receiving the encrypted bytes.
            CryptoStream cs = new CryptoStream(fsOut,
                alg.CreateEncryptor(), CryptoStreamMode.Write);

            // Now will will initialize a buffer and will be processing
            // the input file in chunks.
            // This is done to avoid reading the whole file (which can
            // be huge) into memory.
            int bufferLen = 4096;
            byte[] buffer = new byte[bufferLen];
            int bytesRead;

            do
            {
                // read a chunk of data from the input file
                bytesRead = fsIn.Read(buffer, 0, bufferLen);

                // encrypt it
                cs.Write(buffer, 0, bytesRead);
            } while (bytesRead != 0);

            // close everything

            // this will also close the unrelying fsOut stream
            cs.Close();
            fsIn.Close();
        }

        // Decrypt a byte array into a byte array using a key and an IV
        public static byte[] Decrypt(byte[] cipherData,
                                    byte[] Key, byte[] IV)
        {
            // Create a MemoryStream that is going to accept the
            // decrypted bytes
            MemoryStream ms = new MemoryStream();

            // Create a symmetric algorithm.
            // We are going to use Rijndael because it is strong and
            // available on all platforms.
            // You can use other algorithms, to do so substitute the next
            // line with something like
            //     TripleDES alg = TripleDES.Create();
            Rijndael alg = Rijndael.Create();

            // Now set the key and the IV.
            // We need the IV (Initialization Vector) because the algorithm
            // is operating in its default
            // mode called CBC (Cipher Block Chaining). The IV is XORed with
            // the first block (8 byte)
            // of the data after it is decrypted, and then each decrypted
            // block is XORed with the previous
            // cipher block. This is done to make encryption more secure.
            // There is also a mode called ECB which does not need an IV,
            // but it is much less secure.
            alg.Key = Key;
            alg.IV = IV;

            // Create a CryptoStream through which we are going to be
            // pumping our data.
            // CryptoStreamMode.Write means that we are going to be
            // writing data to the stream
            // and the output will be written in the MemoryStream
            // we have provided.
            CryptoStream cs = new CryptoStream(ms,
                alg.CreateDecryptor(), CryptoStreamMode.Write);

            // Write the data and make it do the decryption
            cs.Write(cipherData, 0, cipherData.Length);

            // Close the crypto stream (or do FlushFinalBlock).
            // This will tell it that we have done our decryption
            // and there is no more data coming in,
            // and it is now a good time to remove the padding
            // and finalize the decryption process.
            cs.Close();

            // Now get the decrypted data from the MemoryStream.
            // Some people make a mistake of using GetBuffer() here,
            // which is not the right way.
            byte[] decryptedData = ms.ToArray();

            return decryptedData;
        }

        // Decrypt a string into a string using a password
        //    Uses Decrypt(byte[], byte[], byte[])

        public static string Decrypt(string cipherText, string Password)
        {
            Password = Password.ToUpper();
            // First we need to turn the input string into a byte array.
            // We presume that Base64 encoding was used
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            // Then, we need to turn the password into Key and IV
            // We are using salt to make it harder to guess our key
            // using a dictionary attack -
            // trying to guess a password by enumerating all possible words.
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,
                new byte[] {0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65,
            0x64, 0x76, 0x65, 0x64, 0x65, 0x76});

            // Now get the key/IV and do the decryption using
            // the function that accepts byte arrays.
            // Using PasswordDeriveBytes object we are first
            // getting 32 bytes for the Key
            // (the default Rijndael key length is 256bit = 32bytes)
            // and then 16 bytes for the IV.
            // IV should always be the block size, which is by
            // default 16 bytes (128 bit) for Rijndael.
            // If you are using DES/TripleDES/RC2 the block size is
            // 8 bytes and so should be the IV size.
            // You can also read KeySize/BlockSize properties off
            // the algorithm to find out the sizes.
            byte[] decryptedData = Decrypt(cipherBytes,
                pdb.GetBytes(32), pdb.GetBytes(16));

            // Now we need to turn the resulting byte array into a string.
            // A common mistake would be to use an Encoding class for that.
            // It does not work
            // because not all byte values can be represented by characters.
            // We are going to be using Base64 encoding that is
            // designed exactly for what we are trying to do.
            return System.Text.Encoding.Unicode.GetString(decryptedData);
        }

        // Decrypt bytes into bytes using a password
        //    Uses Decrypt(byte[], byte[], byte[])

        public static byte[] Decrypt(byte[] cipherData, string Password)
        {
            // We need to turn the password into Key and IV.
            // We are using salt to make it harder to guess our key
            // using a dictionary attack -
            // trying to guess a password by enumerating all possible words.
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,
                new byte[] {0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d,
            0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76});

            // Now get the key/IV and do the Decryption using the
            //function that accepts byte arrays.
            // Using PasswordDeriveBytes object we are first getting
            // 32 bytes for the Key
            // (the default Rijndael key length is 256bit = 32bytes)
            // and then 16 bytes for the IV.
            // IV should always be the block size, which is by default
            // 16 bytes (128 bit) for Rijndael.
            // If you are using DES/TripleDES/RC2 the block size is
            // 8 bytes and so should be the IV size.

            // You can also read KeySize/BlockSize properties off the
            // algorithm to find out the sizes.
            return Decrypt(cipherData, pdb.GetBytes(32), pdb.GetBytes(16));
        }

        // Decrypt a file into another file using a password
        public static void Decrypt(string fileIn,
                    string fileOut, string Password)
        {
            // First we are going to open the file streams
            FileStream fsIn = new FileStream(fileIn,
                        FileMode.Open, FileAccess.Read);
            FileStream fsOut = new FileStream(fileOut,
                        FileMode.OpenOrCreate, FileAccess.Write);

            // Then we are going to derive a Key and an IV from
            // the Password and create an algorithm
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(Password,
                new byte[] {0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d,
            0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76});
            Rijndael alg = Rijndael.Create();

            alg.Key = pdb.GetBytes(32);
            alg.IV = pdb.GetBytes(16);

            // Now create a crypto stream through which we are going
            // to be pumping data.
            // Our fileOut is going to be receiving the Decrypted bytes.
            CryptoStream cs = new CryptoStream(fsOut,
                alg.CreateDecryptor(), CryptoStreamMode.Write);

            // Now will will initialize a buffer and will be
            // processing the input file in chunks.
            // This is done to avoid reading the whole file (which can be
            // huge) into memory.
            int bufferLen = 4096;
            byte[] buffer = new byte[bufferLen];
            int bytesRead;

            do
            {
                // read a chunk of data from the input file
                bytesRead = fsIn.Read(buffer, 0, bufferLen);

                // Decrypt it
                cs.Write(buffer, 0, bytesRead);
            } while (bytesRead != 0);

            // close everything
            cs.Close(); // this will also close the unrelying fsOut stream
            fsIn.Close();
        }
    }
}