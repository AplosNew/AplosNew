using System;
using System.Data;
using OTSBD;

namespace bplib
{
    public class clsGenID
    {
        public clsGenID()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        #region Gen ID
        public void GenID(string strEntryDate, string strFieldName, out string strID)
        {
            ConnectionManager.DAL.ConManager objCoManager;
            string strSql = "";
            DataSet dsLocal = null;
            DataTable dtLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;
            // System.Text.StringBuilder SB = null;
            decimal LastNumber = 0;

            try
            {
                //by Monir
                strEntryDate = clsWebLib.AppDateConvert(strEntryDate, "MM/dd/yyyy", clsWebLib.getUserDateFormat()).ToShortDateString();
                //strEntryDate = bplib.clsWebLib.AppDateConvert(strEntryDate, bplib.clsWebLib.getUserDateFormat(), "MM/dd/yyyy").ToString("MM/dd/yyyy");
                strSql = "SELECT [Field], [Dates], [LastNumber], Year(Dates) as YearNo FROM Signature WHERE Field ='" + strFieldName.Trim() + "' and Year(Dates) = '" + Convert.ToDateTime(strEntryDate).Year.ToString() + "'";

                //SB = new System.Text.StringBuilder(strEntryDate);
                // strID = SB.Replace(bplib.clsWebLib.getUserDateSeparator().ToString(), "").ToString();

                strID = Convert.ToDateTime(strEntryDate).Year.ToString();

                objCoManager = new ConnectionManager.DAL.ConManager("1");
                objCoManager.OpenDataSetThroughAdapter(strSql, out dsLocal, false, false, "", "1");
                dtLocal = dsLocal.Tables[0];
                dvLocal = new DataView();
                dvLocal.Table = dtLocal;
                dvLocal.RowFilter = "Field ='" + strFieldName.Trim() + "'and YearNo = '" + Convert.ToDateTime(strEntryDate).Year.ToString() + "'";
                if (dvLocal.Count == 0)
                {// Add data
                    drLocal = dtLocal.NewRow();
                    drLocal["Field"] = clsWebLib.RetValidLen(strFieldName, 50);
                    drLocal["Dates"] = strEntryDate.Trim();
                    drLocal["LastNumber"] = 1;
                    LastNumber = 1;
                    dtLocal.Rows.Add(drLocal);
                }
                else if (dvLocal.Count == 1)
                {
                    drLocal = dvLocal[0].Row;

                    LastNumber = Convert.ToDecimal(clsWebLib.GetNumData(("" + drLocal["LastNumber"].ToString())));
                    LastNumber = LastNumber + 1;

                    drLocal.BeginEdit();
                    drLocal["Dates"] = strEntryDate.Trim();
                    drLocal["LastNumber"] = LastNumber;
                    drLocal.EndEdit();
                }
                objCoManager.SaveDataSetThroughAdapter(ref dsLocal, false, "1");
                //strID = strID + "-" + (int)LastNumber;
                strID = strID + (int)LastNumber;

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                dtLocal = null;
                dvLocal = null;
                drLocal = null;
            }
        }
        public void GenIDDaily(string strEntryDate, string strFieldName, out string strID)
        {
            ConnectionManager.DAL.ConManager objCoManager;
            string strSql = "";
            DataSet dsLocal = null;
            DataTable dtLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;
            // System.Text.StringBuilder SB = null;
            double LastNumber = 0;

            try
            {
                //by Monir
                strEntryDate = clsWebLib.AppDateConvert(strEntryDate, "MM/dd/yyyy", clsWebLib.getUserDateFormat()).ToShortDateString();
                //strEntryDate = bplib.clsWebLib.AppDateConvert(strEntryDate, bplib.clsWebLib.getUserDateFormat(), "MM/dd/yyyy").ToString("MM/dd/yyyy");
                strSql = "SELECT [Field], [Dates], [LastNumber], Year(Dates) as YearNo FROM Signature WHERE Field ='" + strFieldName.Trim() + "' and Dates = '" + Convert.ToDateTime(strEntryDate).ToString() + "'";

                //SB = new System.Text.StringBuilder(strEntryDate);
                // strID = SB.Replace(bplib.clsWebLib.getUserDateSeparator().ToString(), "").ToString();

                strID = Convert.ToDateTime(strEntryDate).Year.ToString();

                objCoManager = new ConnectionManager.DAL.ConManager("1");
                objCoManager.OpenDataSetThroughAdapter(strSql, out dsLocal, false, false, "", "1");
                dtLocal = dsLocal.Tables[0];
                dvLocal = new DataView();
                dvLocal.Table = dtLocal;
                //dvLocal.RowFilter = "Field ='" + strFieldName.Trim() + "'and YearNo = '" + Convert.ToDateTime(strEntryDate).Year.ToString() + "'";
                if (dtLocal.Rows.Count == 0)
                {// Add data
                    drLocal = dtLocal.NewRow();
                    drLocal["Field"] = clsWebLib.RetValidLen(strFieldName, 50);
                    drLocal["Dates"] = strEntryDate.Trim();
                    drLocal["LastNumber"] = 1;
                    LastNumber = 1;
                    dtLocal.Rows.Add(drLocal);
                }
                else if (dvLocal.Count == 1)
                {
                    drLocal = dvLocal[0].Row;

                    LastNumber = clsStaticInfo.dbl(drLocal["LastNumber"].ToString());
                    LastNumber = LastNumber + 1;

                    drLocal.BeginEdit();
                    drLocal["Dates"] = strEntryDate.Trim();
                    drLocal["LastNumber"] = LastNumber;
                    drLocal.EndEdit();
                }
                objCoManager.SaveDataSetThroughAdapter(ref dsLocal, false, "1");
                //strID = strID + "-" + (int)LastNumber;
                strID = LastNumber.ToString();

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                dtLocal = null;
                dvLocal = null;
                drLocal = null;
            }
        }

        public void GenID(string strFieldName, out string strID)
        {
            ConnectionManager.DAL.ConManager objCoManager;
            string strSql = "";
            DataSet dsLocal = null;
            DataTable dtLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;
            // System.Text.StringBuilder SB = null;
            decimal LastNumber = 0;

            try
            {
                //by Monir
                //strEntryDate = clsWebLib.AppDateConvert(strEntryDate, "MM/dd/yyyy", clsWebLib.getUserDateFormat()).ToShortDateString();
                //strEntryDate = bplib.clsWebLib.AppDateConvert(strEntryDate, bplib.clsWebLib.getUserDateFormat(), "MM/dd/yyyy").ToString("MM/dd/yyyy");
                strSql = "SELECT [Field], [Dates], [LastNumber], Year(Dates) as YearNo FROM Signature WHERE Field ='" + strFieldName.Trim() + "'";

                //SB = new System.Text.StringBuilder(strEntryDate);
                // strID = SB.Replace(bplib.clsWebLib.getUserDateSeparator().ToString(), "").ToString();


                objCoManager = new ConnectionManager.DAL.ConManager("1");
                objCoManager.OpenDataSetThroughAdapter(strSql, out dsLocal, false, false, "", "1");
                dtLocal = dsLocal.Tables[0];
                dvLocal = new DataView();
                dvLocal.Table = dtLocal;
                dvLocal.RowFilter = "Field ='" + strFieldName.Trim() + "'";
                if (dvLocal.Count == 0)
                {// Add data
                    drLocal = dtLocal.NewRow();
                    drLocal["Field"] = clsWebLib.RetValidLen(strFieldName, 50);
                    drLocal["Dates"] = System.DateTime.Now.ToString();
                    drLocal["LastNumber"] = 1;
                    LastNumber = 1;
                    dtLocal.Rows.Add(drLocal);
                }
                else if (dvLocal.Count == 1)
                {
                    drLocal = dvLocal[0].Row;

                    LastNumber = Convert.ToDecimal(clsWebLib.GetNumData(("" + drLocal["LastNumber"].ToString())));
                    LastNumber = LastNumber + 1;

                    drLocal.BeginEdit();
                    drLocal["Dates"] = System.DateTime.Now.ToString();
                    drLocal["LastNumber"] = LastNumber;
                    drLocal.EndEdit();
                }
                objCoManager.SaveDataSetThroughAdapter(ref dsLocal, false, "1");
                //strID = strID + "-" + (int)LastNumber;
                strID = LastNumber.ToString("F0");

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                dtLocal = null;
                dvLocal = null;
                drLocal = null;
            }
        }
        public void GenEmpCode(string strEntryDate, string strFieldName, out string strID)
        {
            ConnectionManager.DAL.ConManager objCoManager;
            string strSql = "";
            DataSet dsLocal = null;
            DataTable dtLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;
            // System.Text.StringBuilder SB = null;
            decimal LastNumber = 0;

            try
            {
                //by Monir
                strEntryDate = clsWebLib.AppDateConvert(strEntryDate, "MM/dd/yyyy", clsWebLib.getUserDateFormat()).ToShortDateString();
                //strEntryDate = bplib.clsWebLib.AppDateConvert(strEntryDate, bplib.clsWebLib.getUserDateFormat(), "MM/dd/yyyy").ToString("MM/dd/yyyy");
                strSql = "SELECT [Field], [Dates], [LastNumber], Year(Dates) as YearNo FROM Signature WHERE Field ='" + strFieldName.Trim() + "' and Year(Dates) = '" + Convert.ToDateTime(strEntryDate).Year.ToString() + "'";

                //SB = new System.Text.StringBuilder(strEntryDate);
                // strID = SB.Replace(bplib.clsWebLib.getUserDateSeparator().ToString(), "").ToString();

                //strID = Convert.ToDateTime(strEntryDate).Year.ToString();

                objCoManager = new ConnectionManager.DAL.ConManager("1");
                objCoManager.OpenDataSetThroughAdapter(strSql, out dsLocal, false, false, "", "1");
                dtLocal = dsLocal.Tables[0];
                dvLocal = new DataView();
                dvLocal.Table = dtLocal;
                dvLocal.RowFilter = "Field ='" + strFieldName.Trim() + "'and YearNo = '" + Convert.ToDateTime(strEntryDate).Year.ToString() + "'";
                if (dvLocal.Count == 0)
                {// Add data
                    drLocal = dtLocal.NewRow();
                    drLocal["Field"] = clsWebLib.RetValidLen(strFieldName, 50);
                    drLocal["Dates"] = strEntryDate.Trim();
                    drLocal["LastNumber"] = 1;
                    LastNumber = 1;
                    dtLocal.Rows.Add(drLocal);
                }
                else if (dvLocal.Count == 1)
                {
                    drLocal = dvLocal[0].Row;

                    LastNumber = Convert.ToDecimal(clsWebLib.GetNumData(("" + drLocal["LastNumber"].ToString())));
                    LastNumber = LastNumber + 1;

                    drLocal.BeginEdit();
                    drLocal["Dates"] = strEntryDate.Trim();
                    drLocal["LastNumber"] = LastNumber;
                    drLocal.EndEdit();
                }
                objCoManager.SaveDataSetThroughAdapter(ref dsLocal, false, "1");
                strID = /*strID + "-" +*/ (int)LastNumber + "";

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                dtLocal = null;
                dvLocal = null;
                drLocal = null;
            }
        }
        public void GenHRID(string strEntryDate, string strFieldName, out string strID)
        {
            ConnectionManager.DAL.ConManager objCoManager;
            string strSql = "";
            DataSet dsLocal = null;
            DataTable dtLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;
            // System.Text.StringBuilder SB = null;
            decimal LastNumber = 0;

            try
            {
                //by Monir
                strEntryDate = clsWebLib.AppDateConvert(strEntryDate, "MM/dd/yyyy", clsWebLib.getUserDateFormat()).ToShortDateString();
                //strEntryDate = bplib.clsWebLib.AppDateConvert(strEntryDate, bplib.clsWebLib.getUserDateFormat(), "MM/dd/yyyy").ToString("MM/dd/yyyy");
                strSql = "SELECT [Field], [Dates], [LastNumber], Year(Dates) as YearNo FROM Signature WHERE Field ='" + strFieldName.Trim() + "' and Year(Dates) = '" + Convert.ToDateTime(strEntryDate).Year.ToString() + "'";

                //SB = new System.Text.StringBuilder(strEntryDate);
                // strID = SB.Replace(bplib.clsWebLib.getUserDateSeparator().ToString(), "").ToString();

                //strID = Convert.ToDateTime(strEntryDate).Year.ToString();

                objCoManager = new ConnectionManager.DAL.ConManager("1");
                objCoManager.OpenDataSetThroughAdapter(strSql, out dsLocal, false, false, "", "1");
                dtLocal = dsLocal.Tables[0];
                dvLocal = new DataView();
                dvLocal.Table = dtLocal;
                dvLocal.RowFilter = "Field ='" + strFieldName.Trim() + "'and YearNo = '" + Convert.ToDateTime(strEntryDate).Year.ToString() + "'";
                if (dvLocal.Count == 0)
                {// Add data
                    drLocal = dtLocal.NewRow();
                    drLocal["Field"] = clsWebLib.RetValidLen(strFieldName, 50);
                    drLocal["Dates"] = strEntryDate.Trim();
                    drLocal["LastNumber"] = 1;
                    LastNumber = 1;
                    dtLocal.Rows.Add(drLocal);
                }
                else if (dvLocal.Count == 1)
                {
                    drLocal = dvLocal[0].Row;

                    LastNumber = Convert.ToDecimal(clsWebLib.GetNumData(("" + drLocal["LastNumber"].ToString())));
                    LastNumber = LastNumber + 1;

                    drLocal.BeginEdit();
                    drLocal["Dates"] = strEntryDate.Trim();
                    drLocal["LastNumber"] = LastNumber;
                    drLocal.EndEdit();
                }
                objCoManager.SaveDataSetThroughAdapter(ref dsLocal, false, "1");
                strID = System.DateTime.Now.ToString("yyyy").Substring(2, 2) + ((int)LastNumber).ToString() + "";

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                dtLocal = null;
                dvLocal = null;
                drLocal = null;
            }
        }
        public void GenHRID(string strEntryDate, string strFieldName, int Count, out string strID)
        {
            ConnectionManager.DAL.ConManager objCoManager;
            string strSql = "";
            DataSet dsLocal = null;
            DataTable dtLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;
            // System.Text.StringBuilder SB = null;
            decimal LastNumber = 0;
            decimal LastNumberR = 0;

            try
            {
                //by Monir
                strEntryDate = clsWebLib.AppDateConvert(strEntryDate, "MM/dd/yyyy", clsWebLib.getUserDateFormat()).ToShortDateString();
                //strEntryDate = bplib.clsWebLib.AppDateConvert(strEntryDate, bplib.clsWebLib.getUserDateFormat(), "MM/dd/yyyy").ToString("MM/dd/yyyy");
                strSql = "SELECT [Field], [Dates], [LastNumber], Year(Dates) as YearNo FROM Signature WHERE Field ='" + strFieldName.Trim() + "' and Year(Dates) = '" + Convert.ToDateTime(strEntryDate).Year.ToString() + "'";

                //SB = new System.Text.StringBuilder(strEntryDate);
                // strID = SB.Replace(bplib.clsWebLib.getUserDateSeparator().ToString(), "").ToString();

                //strID = Convert.ToDateTime(strEntryDate).Year.ToString();

                objCoManager = new ConnectionManager.DAL.ConManager("1");
                objCoManager.OpenDataSetThroughAdapter(strSql, out dsLocal, false, false, "", "1");
                dtLocal = dsLocal.Tables[0];
                dvLocal = new DataView();
                dvLocal.Table = dtLocal;
                dvLocal.RowFilter = "Field ='" + strFieldName.Trim() + "'and YearNo = '" + Convert.ToDateTime(strEntryDate).Year.ToString() + "'";
                if (dvLocal.Count == 0)
                {// Add data
                    drLocal = dtLocal.NewRow();
                    drLocal["Field"] = clsWebLib.RetValidLen(strFieldName, 50);
                    drLocal["Dates"] = strEntryDate.Trim();
                    drLocal["LastNumber"] = 1;
                    LastNumber = 1;
                    dtLocal.Rows.Add(drLocal);
                }
                else if (dvLocal.Count == 1)
                {
                    drLocal = dvLocal[0].Row;

                    LastNumber = Convert.ToDecimal(clsWebLib.GetNumData(("" + drLocal["LastNumber"].ToString())));
                    LastNumberR = LastNumber + 1;
                    //LastNumber = LastNumber + Count + 1;
                    LastNumber = LastNumber  + 1;

                    drLocal.BeginEdit();
                    drLocal["Dates"] = strEntryDate.Trim();
                    drLocal["LastNumber"] = LastNumber;
                    drLocal.EndEdit();
                }
                objCoManager.SaveDataSetThroughAdapter(ref dsLocal, false, "1");
                //strID = /*strID + "-" +*/ (int)LastNumber + "";
                //strID = (int)LastNumberR + "";
                //strID = System.DateTime.Now.Year.ToString() + ((int)LastNumberR).ToString() + "";
                strID = System.DateTime.Now.ToString("yyyy").Substring(2, 2) + ((int)LastNumber).ToString() + "";
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                dtLocal = null;
                dvLocal = null;
                drLocal = null;
            }
        }
        public void GenIDYearly(string strEntryDate, string strFieldName, out string strID)
        {
            ConnectionManager.DAL.ConManager objCoManager;
            string strSql = "";
            DataSet dsLocal = null;
            DataTable dtLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;
            // System.Text.StringBuilder SB = null;
            decimal LastNumber = 0;

            try
            {
                //by Monir
                strEntryDate = clsWebLib.AppDateConvert(strEntryDate, "MM/dd/yyyy", clsWebLib.getUserDateFormat()).ToShortDateString();
                //strEntryDate = bplib.clsWebLib.AppDateConvert(strEntryDate, bplib.clsWebLib.getUserDateFormat(), "MM/dd/yyyy").ToString("MM/dd/yyyy");
                strSql = "SELECT [Field], [Dates], [LastNumber], Year(Dates) as YearNo FROM Signature WHERE Field ='" + strFieldName.Trim() + "' and Year(Dates) = '" + Convert.ToDateTime(strEntryDate).Year.ToString() + "'";

                //SB = new System.Text.StringBuilder(strEntryDate);
                // strID = SB.Replace(bplib.clsWebLib.getUserDateSeparator().ToString(), "").ToString();

                //strID = Convert.ToDateTime(strEntryDate).Year.ToString();

                objCoManager = new ConnectionManager.DAL.ConManager("1");
                objCoManager.OpenDataSetThroughAdapter(strSql, out dsLocal, false, false, "", "1");
                dtLocal = dsLocal.Tables[0];
                dvLocal = new DataView();
                dvLocal.Table = dtLocal;
                //dvLocal.RowFilter = "Field ='" + strFieldName.Trim() + "'and Year(Dates) = '" + Convert.ToDateTime(strEntryDate).Year.ToString() + "'";
                dvLocal.RowFilter = "Field ='" + strFieldName.Trim() + "'and YearNo = '" + Convert.ToDateTime(strEntryDate).Year + "'";
                if (dvLocal.Count == 0)
                {// Add data
                    drLocal = dtLocal.NewRow();
                    drLocal["Field"] = clsWebLib.RetValidLen(strFieldName, 50);
                    drLocal["Dates"] = strEntryDate.Trim();
                    drLocal["LastNumber"] = 1;
                    LastNumber = 1;
                    dtLocal.Rows.Add(drLocal);
                }
                else if (dvLocal.Count == 1)
                {
                    drLocal = dvLocal[0].Row;

                    LastNumber = Convert.ToDecimal(clsWebLib.GetNumData(("" + drLocal["LastNumber"].ToString())));
                    LastNumber = LastNumber + 1;

                    drLocal.BeginEdit();
                    drLocal["Dates"] = strEntryDate.Trim();
                    drLocal["LastNumber"] = LastNumber;
                    drLocal.EndEdit();
                }
                objCoManager.SaveDataSetThroughAdapter(ref dsLocal, false, "1");
                strID = /*strID + "-" +*/ (int)LastNumber + "";
                strID = DateTime.Now.ToString("yyyy") + "-" + strID;

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                dtLocal = null;
                dvLocal = null;
                drLocal = null;
            }
        }

        public void GenIDByDate(string strEntryDate, string strFieldName, out string strID)  //By Date Wise
        {
            ConnectionManager.DAL.ConManager objCoManager;
            string strSql = "";
            //  int						lngRecCount=0;
            DataSet dsLocal = null;
            DataTable dtLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;

            System.Text.StringBuilder SB = null;
            decimal LastNumber = 0;

            try
            {
                //strEntryDate=bplib.clsWebLib.AppDateConvert(strEntryDate,"MM/dd/yyyy",bplib.clsWebLib.getUserDateFormat()).ToShortDateString();
                strEntryDate = clsWebLib.AppDateConvert(strEntryDate, clsWebLib.getUserDateFormat(), "MM/dd/yyyy").ToString("MM/dd/yyyy");

                strSql = "Select * from Signature where Field ='" + strFieldName.Trim() + "' and Dates = '" + strEntryDate + "'";

                SB = new System.Text.StringBuilder(strEntryDate);
                strID = SB.Replace(clsWebLib.getUserDateSeparator().ToString(), "").ToString();

                objCoManager = new ConnectionManager.DAL.ConManager("1");
                objCoManager.OpenDataSetThroughAdapter(strSql, out dsLocal, false, false, "", "1");
                dtLocal = dsLocal.Tables[0];
                dvLocal = new DataView();
                dvLocal.Table = dtLocal;
                dvLocal.RowFilter = "Field ='" + strFieldName.Trim() + "'and Dates = '" + strEntryDate + "'";
                if (dvLocal.Count == 0)
                {// Add data
                    drLocal = dtLocal.NewRow();
                    drLocal["Field"] = clsWebLib.RetValidLen(strFieldName, 50);
                    drLocal["Dates"] = strEntryDate.Trim();
                    drLocal["LastNumber"] = 1;
                    LastNumber = 1;
                    dtLocal.Rows.Add(drLocal);
                }
                else if (dvLocal.Count == 1)
                {
                    drLocal = dvLocal[0].Row;

                    LastNumber = Convert.ToDecimal(clsWebLib.GetNumData(("" + drLocal["LastNumber"].ToString())));
                    LastNumber = LastNumber + 1;

                    drLocal.BeginEdit();
                    drLocal["LastNumber"] = LastNumber;
                    drLocal.EndEdit();
                }
                objCoManager.SaveDataSetThroughAdapter(ref dsLocal, false, "1");
                strID = strID + "-" + (int)LastNumber;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                dtLocal = null;
                dvLocal = null;
                drLocal = null;
            }
        }
        //public void GenID_AuthKey(string strEntryDate, string strFieldName, out string strID, out string strAuthKey)  //By Date Wise
        //{

        //    ConnectionManager.DAL.ConManager objCoManager;
        //    string strSql = "";
        //    //  int						lngRecCount=0;
        //    DataSet dsLocal = null;
        //    DataTable dtLocal = null;
        //    DataRow drLocal = null;
        //    DataView dvLocal = null;


        //    DataSet dsAuth = null;
        //    DataTable dtAuth = null;
        //    DataRow drAuth = null;
        //    DataView dvAuth = null;


        //    System.Text.StringBuilder SB = null;
        //    decimal LastNumber = 0;

        //    try
        //    {
        //        //strEntryDate=bplib.clsWebLib.AppDateConvert(strEntryDate,"MM/dd/yyyy",bplib.clsWebLib.getUserDateFormat()).ToShortDateString();
        //        strEntryDate = clsWebLib.AppDateConvert(strEntryDate, clsWebLib.getUserDateFormat(), "MM/dd/yyyy").ToString("MM/dd/yyyy");

        //        #region LOCAL SIGNATURE TABLE UPDATE **********************************
        //        strSql = "Select * from Signature where Field ='" + strFieldName.Trim() + "' and Dates = '" + strEntryDate + "'";
        //        SB = new System.Text.StringBuilder(strEntryDate);

        //        strID = SB.Replace(clsWebLib.getUserDateSeparator().ToString(), "").ToString();
        //        strAuthKey = SB.Replace(clsWebLib.getUserDateSeparator().ToString(), "").ToString();

        //        objCoManager = new ConnectionManager.DAL.ConManager("1");
        //        objCoManager.OpenDataSetThroughAdapter(strSql, out dsLocal, false, false, "", "1");
        //        dtLocal = dsLocal.Tables[0];
        //        dvLocal = new DataView();
        //        dvLocal.Table = dtLocal;
        //        dvLocal.RowFilter = "Field ='" + strFieldName.Trim() + "'and Dates = '" + strEntryDate + "'";
        //        if (dvLocal.Count == 0)
        //        {// Add data
        //            drLocal = dtLocal.NewRow();
        //            drLocal["Field"] = clsWebLib.RetValidLen(strFieldName, 50);
        //            drLocal["Dates"] = strEntryDate.Trim();
        //            drLocal["LastNumber"] = 1;
        //            LastNumber = 1;
        //            dtLocal.Rows.Add(drLocal);
        //        }
        //        else if (dvLocal.Count == 1)
        //        {
        //            drLocal = dvLocal[0].Row;

        //            LastNumber = Convert.ToDecimal(clsWebLib.GetNumData(("" + drLocal["LastNumber"].ToString())));
        //            LastNumber = LastNumber + 1;

        //            drLocal.BeginEdit();
        //            drLocal["LastNumber"] = LastNumber;
        //            drLocal.EndEdit();
        //        }
        //        objCoManager.SaveDataSetThroughAdapter(ref dsLocal, false, "1");

        //        #endregion //End LOCAL SIGNATURE TABLE UPDATE

        //        #region REMOTE SIGNATURE TABLE UPDATE =================================
        //        clsStaticInfo objStatic = new clsStaticInfo();
        //        objStatic.OpenAuthSignature(strFieldName.Trim(), strEntryDate, out dsAuth);

        //        dtAuth = dsAuth.Tables[0];
        //        dvAuth = new DataView();
        //        dvAuth.Table = dtAuth;
        //        dvAuth.RowFilter = "Field ='" + strFieldName.Trim() + "'and Dates = '" + strEntryDate + "'";
        //        if (dvAuth.Count == 0)
        //        {// Add data
        //            drAuth = dtAuth.NewRow();
        //            drAuth["Field"] = clsWebLib.RetValidLen(strFieldName, 50);
        //            drAuth["Dates"] = strEntryDate.Trim();
        //            drAuth["LastNumber"] = LastNumber;
        //            dtAuth.Rows.Add(drAuth);
        //        }
        //        else if (dvLocal.Count == 1)
        //        {
        //            drAuth = dvAuth[0].Row;
        //            drAuth.BeginEdit();
        //            drAuth["LastNumber"] = LastNumber;
        //            drAuth.EndEdit();
        //        }
        //        objStatic.SaveDataRemote(ref dsAuth);

        //        #endregion //End REMOTE SIGNATURE TABLE UPDATE


        //        strID = strID + "-" + (int)LastNumber; // New ID
        //        strAuthKey = strAuthKey + LastNumber.ToString("000");//New AuthKey
        //    }
        //    catch (Exception ex)
        //    {
        //        throw (ex);
        //    }
        //    finally
        //    {
        //        dtLocal = null;
        //        dvLocal = null;
        //        drLocal = null;
        //    }
        //}

        public void GenRefSrNoID(string strEntryDate, string strFieldName, int SrNo, out string strID)
        {
            ConnectionManager.DAL.ConManager objCoManager;
            string strSql = "";
            //  int						lngRecCount=0;
            DataSet dsLocal = null;
            DataTable dtLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;

            System.Text.StringBuilder SB = null;
            decimal LastNumber = 0;

            try
            {
                strEntryDate = clsWebLib.AppDateConvert(strEntryDate, clsWebLib.getUserDateFormat(), "dd-MMM-yyyy").ToString("dd-MMM-yyyy");
                //strEntryDate = clsWebLib.AppDateConvert(strEntryDate, clsWebLib.getUserDateFormat(), "MM/dd/yyyy").ToString("MM/dd/yyyy");

                strSql = "SELECT * FROM Signature WHERE Field ='" + strFieldName.Trim() + "' AND Dates = '" + strEntryDate + "'";

                SB = new System.Text.StringBuilder(strEntryDate);
                strID = SB.Replace(clsWebLib.getUserDateSeparator().ToString(), "").ToString();

                objCoManager = new ConnectionManager.DAL.ConManager("1");
                objCoManager.OpenDataSetThroughAdapter(strSql, out dsLocal, false, false, "", "1");
                dtLocal = dsLocal.Tables[0];
                dvLocal = new DataView();

                dvLocal.Table = dtLocal;
                dvLocal.RowFilter = "Field ='" + strFieldName.Trim() + "'and Dates = '" + strEntryDate + "'";
                if (dvLocal.Count == 0)
                {// Add data
                    LastNumber = 1 + SrNo;

                    drLocal = dtLocal.NewRow();
                    drLocal["Field"] = clsWebLib.RetValidLen(strFieldName, 50);
                    drLocal["Dates"] = strEntryDate.Trim();
                    drLocal["LastNumber"] = LastNumber;
                    dtLocal.Rows.Add(drLocal);
                }
                else if (dvLocal.Count == 1)
                {
                    drLocal = dvLocal[0].Row;

                    LastNumber = Convert.ToDecimal(clsWebLib.GetNumData(("" + drLocal["LastNumber"].ToString())));
                    LastNumber = LastNumber + SrNo;

                    drLocal.BeginEdit();
                    drLocal["LastNumber"] = LastNumber;
                    drLocal.EndEdit();
                }
                objCoManager.SaveDataSetThroughAdapter(ref dsLocal, false, "1");
                strID = strID + "-" + ((int)LastNumber - SrNo);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                dtLocal = null;
                dvLocal = null;
                drLocal = null;
            }
        }


        public void GenerateIDYearly(string strEntryDate, string strFieldName, out string strID)
        {
            ConnectionManager.DAL.ConManager objCoManager;

            string strSql = "";
            DataSet dsLocal = null;
            DataTable dtLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;
            // System.Text.StringBuilder SB = null;
            decimal MaxNumber = 0;

            try
            {
                //by Monir
                strEntryDate = clsWebLib.AppDateConvert(strEntryDate, "MM/dd/yyyy", clsWebLib.getUserDateFormat()).ToShortDateString();
                //strEntryDate = bplib.clsWebLib.AppDateConvert(strEntryDate, bplib.clsWebLib.getUserDateFormat(), "MM/dd/yyyy").ToString("MM/dd/yyyy");
                //strSql = "SELECT [Field], [Dates], [LastNumber], Year(Dates) as YearNo FROM Signature WHERE Field ='" + strFieldName.Trim() + "' and Year(Dates) = '" + Convert.ToDateTime(strEntryDate).Year.ToString() + "'";
                string period = Convert.ToDateTime(strEntryDate).Year.ToString();
                strSql = "SELECT Id, Period, FieldName, MaxNumber,UpdatedDate FROM ACS.PKGenerator WHERE FieldName ='" + strFieldName.Trim() + @"'  AND Period='" + period + "'";

                objCoManager = new ConnectionManager.DAL.ConManager("1");
                objCoManager.OpenDataSetThroughAdapter(strSql, out dsLocal, false, false, "", "1");


                dtLocal = dsLocal.Tables[0];
                dvLocal = new DataView(dtLocal);
                //dvLocal.Table = dtLocal;
                dvLocal.RowFilter = "FieldName ='" + strFieldName.Trim() + "' AND Period = '" + period + "'";
                if (dvLocal.Count == 0)
                {// Add data
                    drLocal = dtLocal.NewRow();
                    drLocal["FieldName"] = clsWebLib.RetValidLen(strFieldName, 100);
                    drLocal["Period"] = DateTime.Now.Year;
                    drLocal["MaxNumber"] = 1;
                    drLocal["UpdatedDate"] = DateTime.Now;
                    MaxNumber = 1;
                    dtLocal.Rows.Add(drLocal);
                }
                else if (dvLocal.Count == 1)
                {
                    drLocal = dvLocal[0].Row;

                    MaxNumber = Convert.ToDecimal(clsWebLib.GetNumData(("" + drLocal["MaxNumber"].ToString())));
                    MaxNumber = MaxNumber + 1;

                    drLocal.BeginEdit();
                    drLocal["FieldName"] = clsWebLib.RetValidLen(strFieldName, 100);
                    drLocal["Period"] = DateTime.Now.Year;
                    drLocal["MaxNumber"] = MaxNumber;
                    drLocal["UpdatedDate"] = DateTime.Now;
                    drLocal.EndEdit();
                }

                objCoManager.SaveDataSetThroughAdapter(ref dsLocal, false, "1");
                strID = /*strID + "-" +*/ (int)MaxNumber + "";
                strID = DateTime.Now.ToString("yy") + strID;

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                dtLocal = null;
                dvLocal = null;
                drLocal = null;
            }
        }

        public void GenerateIDAuto(string strFieldName, out string strID)
        {
            ConnectionManager.DAL.ConManager objCoManager;

            string strSql = "";
            DataSet dsLocal = null;
            DataTable dtLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;
            // System.Text.StringBuilder SB = null;
            decimal MaxNumber = 0;

            try
            {
                string period = "Auto";
                strSql = "SELECT Id, Period, FieldName, MaxNumber,UpdatedDate FROM ACS.PKGenerator WHERE FieldName ='" + strFieldName.Trim() + @"'  AND Period='" + period + "'";

                objCoManager = new ConnectionManager.DAL.ConManager("1");
                objCoManager.OpenDataSetThroughAdapter(strSql, out dsLocal, false, false, "", "1");


                dtLocal = dsLocal.Tables[0];
                dvLocal = new DataView(dtLocal);
                //dvLocal.Table = dtLocal;
                dvLocal.RowFilter = "FieldName ='" + strFieldName.Trim() + "' AND Period = '" + period + "'";
                if (dvLocal.Count == 0)
                {// Add data
                    drLocal = dtLocal.NewRow();
                    drLocal["FieldName"] = clsWebLib.RetValidLen(strFieldName, 100);
                    drLocal["Period"] = period;
                    drLocal["MaxNumber"] = 1;
                    drLocal["UpdatedDate"] = DateTime.Now;
                    MaxNumber = 1;
                    dtLocal.Rows.Add(drLocal);
                }
                else if (dvLocal.Count == 1)
                {
                    drLocal = dvLocal[0].Row;

                    MaxNumber = Convert.ToDecimal(clsWebLib.GetNumData(("" + drLocal["MaxNumber"].ToString())));
                    MaxNumber = MaxNumber + 1;

                    drLocal.BeginEdit();
                    drLocal["FieldName"] = clsWebLib.RetValidLen(strFieldName, 100);
                    drLocal["Period"] = period;
                    drLocal["MaxNumber"] = MaxNumber;
                    drLocal["UpdatedDate"] = DateTime.Now;
                    drLocal.EndEdit();
                }

                objCoManager.SaveDataSetThroughAdapter(ref dsLocal, false, "1");
                strID = (int)MaxNumber + "";


            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                dtLocal = null;
                dvLocal = null;
                drLocal = null;
            }
        }
      
        public void GenerateIDMaxYearly(string strEntryDate, string strFieldName, out string strID)
        {
            ConnectionManager.DAL.ConManager objCoManager;

            string strSql = "";
            DataSet dsLocal = null;
            DataTable dtLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;
            // System.Text.StringBuilder SB = null;
            decimal MaxNumber = 0;

            try
            {
                //by Monir
                strEntryDate = clsWebLib.AppDateConvert(strEntryDate, "MM/dd/yyyy", clsWebLib.getUserDateFormat()).ToShortDateString();
                string period = Convert.ToDateTime(strEntryDate).Year.ToString();
                strSql = "SELECT Id, Period, FieldName, MaxNumber,UpdatedDate FROM ACS.PKGenerator WHERE FieldName ='" + strFieldName.Trim() + @"'  AND Period='" + period + "'";

                objCoManager = new ConnectionManager.DAL.ConManager("1");
                objCoManager.OpenDataSetThroughAdapter(strSql, out dsLocal, false, false, "", "1");


                dtLocal = dsLocal.Tables[0];
                dvLocal = new DataView(dtLocal);
                //dvLocal.Table = dtLocal;
                dvLocal.RowFilter = "FieldName ='" + strFieldName.Trim() + "' AND Period = '" + period + "'";
                if (dvLocal.Count == 0)
                {// Add data
                    drLocal = dtLocal.NewRow();
                    drLocal["FieldName"] = clsWebLib.RetValidLen(strFieldName, 100);
                    drLocal["Period"] = DateTime.Now.Year;
                    drLocal["MaxNumber"] = 1;
                    drLocal["UpdatedDate"] = DateTime.Now;
                    MaxNumber = 1;
                    dtLocal.Rows.Add(drLocal);
                }
                else if (dvLocal.Count == 1)
                {
                    drLocal = dvLocal[0].Row;

                    MaxNumber = Convert.ToDecimal(clsWebLib.GetNumData(("" + drLocal["MaxNumber"].ToString())));
                    MaxNumber = MaxNumber + 1;

                    drLocal.BeginEdit();
                    drLocal["FieldName"] = clsWebLib.RetValidLen(strFieldName, 100);
                    drLocal["Period"] = DateTime.Now.Year;
                    drLocal["MaxNumber"] = MaxNumber;
                    drLocal["UpdatedDate"] = DateTime.Now;
                    drLocal.EndEdit();
                }

                objCoManager.SaveDataSetThroughAdapter(ref dsLocal, false, "1");
                strID = (int)MaxNumber + "";

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                dtLocal = null;
                dvLocal = null;
                drLocal = null;
            }
        }

        #endregion

        #region GetLastNumber
        public void GetLastNumber(string strEntryDate, string strFieldName, out string strID)
        {
            ConnectionManager.DAL.ConManager objCoManager;
            string strSql = "";
            //	int						lngRecCount=0;
            DataSet dsLocal = null;
            DataTable dtLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;

            System.Text.StringBuilder SB = null;
            decimal LastNumber = 0;

            try
            {
                strEntryDate = clsWebLib.AppDateConvert(strEntryDate, clsWebLib.getUserDateFormat(), "MM/dd/yyyy").ToString("MM/dd/yyyy");

                strSql = "Select * from Signature where Field ='" + strFieldName.Trim() + "' and Dates = '" + strEntryDate + "'";

                SB = new System.Text.StringBuilder(strEntryDate);
                strID = SB.Replace(clsWebLib.getUserDateSeparator().ToString(), "").ToString();

                objCoManager = new ConnectionManager.DAL.ConManager("1");
                objCoManager.OpenDataSetThroughAdapter(strSql, out dsLocal, false, false, "", "1");
                //------------------------------------------------------
                if ((dsLocal.Tables[0].Rows.Count < 0) || (dsLocal.Tables[0].Rows.Count == 0))
                {
                    LastNumber = 0;
                }
                else
                {
                    LastNumber = Math.Round(Convert.ToDecimal(clsWebLib.GetNumData(dsLocal.Tables[0].Rows[0]["LastNumber"].ToString().Trim())));
                }
                strID = Convert.ToString(LastNumber + 1);
                //------------------------------

                dtLocal = dsLocal.Tables[0];
                dvLocal = new DataView();
                dvLocal.Table = dtLocal;
                dvLocal.RowFilter = "Field ='" + strFieldName.Trim() + "'and Dates = '" + strEntryDate + "'";
                if (dvLocal.Count == 0)
                {// Add data
                    drLocal = dtLocal.NewRow();
                    drLocal["Field"] = clsWebLib.RetValidLen(strFieldName, 50);
                    drLocal["Dates"] = strEntryDate.Trim();
                    drLocal["LastNumber"] = 1;
                    LastNumber = 1;
                    dtLocal.Rows.Add(drLocal);
                }
                else if (dvLocal.Count == 1)
                {
                    drLocal = dvLocal[0].Row;

                    LastNumber = Convert.ToDecimal(clsWebLib.GetNumData(("" + drLocal["LastNumber"].ToString())));
                    LastNumber = LastNumber + 1;

                    drLocal.BeginEdit();
                    drLocal["LastNumber"] = LastNumber;
                    drLocal.EndEdit();
                }
                objCoManager.SaveDataSetThroughAdapter(ref dsLocal, false, "1");
                //strID=strID+"-"+(int)LastNumber;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                dtLocal = null;
                dvLocal = null;
                drLocal = null;
            }
        }
        #endregion

        #region Folder
        public void MakeFolderID(string strEntryDate, string strFieldName, ref string strCode, ref double lastNo, double BlockNo)
        {
            ConnectionManager.DAL.ConManager objCoManager;
            string strSql = "";
            DataSet dsLocal = null;
            DataTable dtLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;

            System.Text.StringBuilder SB = null;
            try
            {
                objCoManager = new ConnectionManager.DAL.ConManager("1");
                lastNo = 0;

                //strEntryDate = bplib.clsWebLib.AppDateConvert(strEntryDate,"MM/dd/yyyy",bplib.clsWebLib.getUserDateFormat()).ToShortDateString();
                strEntryDate = clsWebLib.AppDateConvert(strEntryDate, clsWebLib.getUserDateFormat(), "MM/dd/yyyy").ToString("MM/dd/yyyy");

                strSql = "Select * from Signature where Field ='" + strFieldName.Trim() + "' and Dates = '" + strEntryDate + "'";
                objCoManager.OpenDataSetThroughAdapter(strSql, out dsLocal, false, "1");

                SB = new System.Text.StringBuilder(strEntryDate);
                strCode = SB.Replace(clsWebLib.getUserDateSeparator().ToString(), "").ToString();

                strCode = strCode.Substring(strCode.Length - 2, 2) + strCode.Substring(0, 2) + strCode.Substring(2, 2);
                dtLocal = dsLocal.Tables[0];
                dvLocal = new DataView();
                dvLocal.Table = dtLocal;
                dvLocal.RowFilter = "Field ='" + strFieldName.Trim() + "'and Dates = '" + strEntryDate + "'";
                if (dvLocal.Count == 0)
                {// Add data
                    drLocal = dtLocal.NewRow();
                    drLocal["Field"] = clsWebLib.RetValidLen(strFieldName, 50);
                    drLocal["Dates"] = strEntryDate.Trim();
                    //						drLocal["LastNumber"]=BlockNo + 1;
                    //						lastNo = BlockNo + 1;
                    drLocal["LastNumber"] = BlockNo;
                    lastNo = 1;

                    dtLocal.Rows.Add(drLocal);
                }
                else if (dvLocal.Count == 1)
                {
                    drLocal = dvLocal[0].Row;

                    lastNo = Convert.ToDouble(clsWebLib.GetNumData(("" + drLocal["LastNumber"].ToString())));
                    drLocal.BeginEdit();
                    //drLocal["LastNumber"]=lastNo + BlockNo +1;
                    drLocal["LastNumber"] = lastNo + BlockNo;

                    lastNo = lastNo + 1;
                    drLocal.EndEdit();
                }
                objCoManager.SaveDataSetThroughAdapter(ref dsLocal, false, "1");
                strCode = strCode + (int)lastNo;
            }
            catch (Exception ex)
            { throw (ex); }
            finally
            {
                dsLocal = null;
                dtLocal = null;
                dvLocal = null;
            }

        }
        public void MakeFolderIDMod(string strEntryDate, string strFieldName, ref string strCode, ref double lastNo, double BlockNo)
        {
            ConnectionManager.DAL.ConManager objCoManager;
            string strSql = "";
            DataSet dsLocal = null;
            DataTable dtLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;

            System.Text.StringBuilder SB = null;
            try
            {
                objCoManager = new ConnectionManager.DAL.ConManager("1");
                lastNo = 0;

                //strEntryDate = bplib.clsWebLib.AppDateConvert(strEntryDate,"MM/dd/yyyy",bplib.clsWebLib.getUserDateFormat()).ToShortDateString();
                strEntryDate = clsWebLib.AppDateConvert(strEntryDate, clsWebLib.getUserDateFormat(), "MM/dd/yyyy").ToString("MM/dd/yyyy");

                strSql = "Select [Field],[Dates],[LastNumber],year(Dates) as YearNo from Signature where Field ='" + strFieldName.Trim() + "'  and year(Dates) = '" + Convert.ToDateTime(strEntryDate).Year.ToString() + "'";
                objCoManager.OpenDataSetThroughAdapter(strSql, out dsLocal, false, "1");

                SB = new System.Text.StringBuilder(strEntryDate);
                strCode = SB.Replace(clsWebLib.getUserDateSeparator().ToString(), "").ToString();

                strCode = strCode.Substring(strCode.Length - 2, 2) + strCode.Substring(0, 2) + strCode.Substring(2, 2);
                dtLocal = dsLocal.Tables[0];
                dvLocal = new DataView();
                dvLocal.Table = dtLocal;
                dvLocal.RowFilter = "Field ='" + strFieldName.Trim() + "'and YearNo = '" + Convert.ToDateTime(strEntryDate).Year.ToString() + "'";
                if (dvLocal.Count == 0)
                {// Add data
                    drLocal = dtLocal.NewRow();
                    drLocal["Field"] = clsWebLib.RetValidLen(strFieldName, 50);
                    drLocal["Dates"] = strEntryDate.Trim();
                    //						drLocal["LastNumber"]=BlockNo + 1;
                    //						lastNo = BlockNo + 1;
                    drLocal["LastNumber"] = BlockNo;
                    lastNo = 1;

                    dtLocal.Rows.Add(drLocal);
                }
                else if (dvLocal.Count == 1)
                {
                    drLocal = dvLocal[0].Row;

                    lastNo = Convert.ToDouble(clsWebLib.GetNumData(("" + drLocal["LastNumber"].ToString())));
                    drLocal.BeginEdit();
                    //drLocal["LastNumber"]=lastNo + BlockNo +1;
                    drLocal["LastNumber"] = lastNo + BlockNo;

                    lastNo = lastNo + 1;
                    drLocal.EndEdit();
                }
                objCoManager.SaveDataSetThroughAdapter(ref dsLocal, false, "1");
                strCode = strCode + (int)lastNo;
            }
            catch (Exception ex)
            { throw (ex); }
            finally
            {
                dsLocal = null;
                dtLocal = null;
                dvLocal = null;
            }

        }

        #endregion

        #region dada
        public void GenIDWithLastNumber(string strEntryDate, int IDNo, string strFieldName, out int strID)
        {
            ConnectionManager.DAL.ConManager objCoManager;
            string strSql = "";
            DataSet dsLocal = null;
            DataTable dtLocal = null;
            DataRow drLocal = null;
            DataView dvLocal = null;

            decimal LastNumber = 0;

            try
            {
                strEntryDate = clsWebLib.AppDateConvert(strEntryDate, clsWebLib.getUserDateFormat(), "MM/dd/yyyy").ToString("MM/dd/yyyy");
                strSql = "SELECT [Field], [Dates], [LastNumber], Year(Dates) as YearNo FROM Signature WHERE Field ='" + strFieldName.Trim() + "' and Year(Dates) = '" + Convert.ToDateTime(strEntryDate).Year.ToString() + "'";

                //strID = Convert.ToDateTime(strEntryDate).Year.ToString();

                objCoManager = new ConnectionManager.DAL.ConManager("1");
                objCoManager.OpenDataSetThroughAdapter(strSql, out dsLocal, false, false, "", "1");
                dtLocal = dsLocal.Tables[0];
                dvLocal = new DataView();
                dvLocal.Table = dtLocal;
                dvLocal.RowFilter = "Field ='" + strFieldName.Trim() + "'and YearNo = '" + Convert.ToDateTime(strEntryDate).Year.ToString() + "'";
                if (dvLocal.Count == 0)
                {// Add data
                    drLocal = dtLocal.NewRow();
                    drLocal["Field"] = clsWebLib.RetValidLen(strFieldName, 50);
                    drLocal["Dates"] = strEntryDate.Trim();
                    drLocal["LastNumber"] = IDNo;
                    LastNumber = 1;
                    dtLocal.Rows.Add(drLocal);
                }
                else if (dvLocal.Count == 1)
                {
                    drLocal = dvLocal[0].Row;

                    LastNumber = Convert.ToDecimal(clsWebLib.GetNumData(("" + drLocal["LastNumber"].ToString())));
                    LastNumber = LastNumber + IDNo;

                    drLocal.BeginEdit();
                    drLocal["Dates"] = strEntryDate.Trim();
                    drLocal["LastNumber"] = LastNumber;
                    drLocal.EndEdit();
                }
                objCoManager.SaveDataSetThroughAdapter(ref dsLocal, false, "1");
                //strID = strID + "-" + (int)LastNumber;
                strID = (int)LastNumber;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                dtLocal = null;
                dvLocal = null;
                drLocal = null;
            }
        }
        #endregion
    }
}