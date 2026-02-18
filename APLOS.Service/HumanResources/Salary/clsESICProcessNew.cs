using bplib;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace OTSBD
{
    public class clsESICProcessNew
    {
        public string sFormulaValue = "";

        public clsESICProcessNew()
        {
            // TODO: Add constructor logic here
        }//End Function
        public void GetESICPolicyMaster(string sESICMstSystemID, string sGroupID, string sPlantID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (sESICMstSystemID != "")
                {
                    strSQL = @"SELECT *
                                FROM ESICPolicyMaster 
                              WHERE ID = '" + sESICMstSystemID + @"'
                                    AND GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'";
                }
                else
                {
                    strSQL = @"SELECT *
                                FROM ESICPolicyMaster
                                WHERE GroupID = '" + sGroupID + @"' AND PlantID = '" + sPlantID + @"'";
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
        public void GetESICPolicyDetails(string sESICMstSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM [dbo].[ESICPolicyDetails] WHERE ESICPolicyMasterID = '" + sESICMstSystemID + @"'";

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
        public void GetESICPolicyLeaveType(string sESICMstSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM ESICPolicyLeaveType
                              WHERE ESICPolicyMasterID = '" + sESICMstSystemID + @"'";

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
        public void GetESICPolicyMonthNo(string sESICMstSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM ESICPolicyMonthNo WHERE ESICPolicyMasterID = '" + sESICMstSystemID + @"'";

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
        public void GetSalaryHead(out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM SalaryHead";

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

        public void GetESICEligibleEmployee(string sEmpSystemID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (sEmpSystemID != "")
                {
                    strSQL = @"SELECT *
                                        FROM ESICEligibleEmployee 
                                      WHERE " + sEmpSystemID + @"";
                }
                else
                {
                    strSQL = @"SELECT *
                                        FROM ESICEligibleEmployee";
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
        public void GetESICMonthlyEmpWiseCalculation(string sEmpSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (sEmpSystemID != "")
                {
                    strSQL = @"SELECT *
                                        FROM ESICMonthlyEmpWiseCalculation 
                                      WHERE ESICEligibleEmpID IN (SELECT ID FROM ESICEligibleEmployee 
                                                                      WHERE " + sEmpSystemID + @")";
                }
                else
                {
                    strSQL = @"SELECT *
                                        FROM ESICMonthlyEmpWiseCalculation";
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

        public void XGetUnTagEmployeeListWithESICPolicyMaster(ESICParaListNew para, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (para.sEmpSystemID == "")
                {
                    strSQL = @"SELECT EEE.ID ESICEligibleEmpID, DM.ESICPolicyMasterID, E.* 
                                FROM [dbo].[EmployeeInformation] E
			                                INNER JOIN [MST].[DesignationMaster] DM ON E.GivenDesignationId = DM.DesignationId
			                                LEFT JOIN (SELECT * FROM [dbo].[ESICEligibleEmployee] WHERE IsActive = 1) EEE ON E.SystemId = EEE.EmpSystemID
			                                LEFT JOIN [dbo].[ESICPolicyMaster] ESICPLMst ON DM.ESICPolicyMasterID = ESICPLMst.ID
                                WHERE E.GroupID = '" + para.GroupID + @"' AND E.PlantId = '" + para.PlantID + @"' AND E.EmployeeStatus = 'Active'
	                                    AND E.SystemID NOT IN (SELECT EmpSystemID FROM [dbo].[ESICEligibleEmployee])";
                }
                else
                {
                    strSQL = @"SELECT EEE.ID ESICEligibleEmpID, DM.ESICPolicyMasterID, E.* 
                                FROM [dbo].[EmployeeInformation] E
			                                INNER JOIN [MST].[DesignationMaster] DM ON E.GivenDesignationId = DM.DesignationId
			                                LEFT JOIN (SELECT * FROM [dbo].[ESICEligibleEmployee] WHERE IsActive = 1) EEE ON E.SystemId = EEE.EmpSystemID
			                                LEFT JOIN [dbo].[ESICPolicyMaster] ESICPLMst ON DM.ESICPolicyMasterID = ESICPLMst.ID
                                WHERE E.GroupID = '" + para.GroupID + @"' AND E.PlantId = '" + para.PlantID + @"' AND E.EmployeeStatus = 'Active'
	                                    AND E.SystemID NOT IN (SELECT EmpSystemID FROM [dbo].[ESICEligibleEmployee]) AND E.SystemId IN (" + para.sEmpSystemID + @")";
                }
                strSQL += @"
                                ORDER BY E.GivenDesignationId, E.SystemId";

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

        public void GetUnTagEmployeeListWithESICPolicyMaster(ESICParaListNew para, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (para.sEmpSystemID == "")
                {
                    strSQL = @"SELECT EEE.ID ESICEligibleEmpID, DM.ESICPolicyMasterID, E.* 
                                FROM [dbo].[EmployeeInformation] E
			                                INNER JOIN   (SELECT DC.LeavePolicyMasterId,DC.ESICPolicyMasterID,DM.DesignationId FROM MST.DesignationMaster DM
                                LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                WHERE DC.PlantId='" + para.PlantID + @"' ) DM ON E.GivenDesignationId = DM.DesignationId
			                                LEFT JOIN (SELECT * FROM [dbo].[ESICEligibleEmployee] WHERE IsActive = 1) EEE ON E.SystemId = EEE.EmpSystemID
			                                LEFT JOIN [dbo].[ESICPolicyMaster] ESICPLMst ON DM.ESICPolicyMasterID = ESICPLMst.ID
                                WHERE E.GroupID = '" + para.GroupID + @"' AND E.PlantId = '" + para.PlantID + @"' AND --E.EmployeeStatus = 'Active'
                                      E.DOJ <= '" + para.ToDate + @"' AND (E.DOS > '" + para.FromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL
                                      OR E.DOS = '' OR E.DOS = '01/01/1901' OR E.EmployeeStatus = 'Active')
	                                  AND E.SystemID NOT IN (SELECT EmpSystemID FROM [dbo].[ESICEligibleEmployee])";
                }
                else
                {
                    strSQL = @"SELECT EEE.ID ESICEligibleEmpID, DM.ESICPolicyMasterID, E.* 
                                FROM [dbo].[EmployeeInformation] E
			                                INNER JOIN (SELECT DC.LeavePolicyMasterId,DC.ESICPolicyMasterID,DM.DesignationId FROM MST.DesignationMaster DM
                                            LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                            WHERE DC.PlantId='" + para.PlantID + @"' ) DM ON E.GivenDesignationId = DM.DesignationId
			                                LEFT JOIN (SELECT * FROM [dbo].[ESICEligibleEmployee] WHERE IsActive = 1) EEE ON E.SystemId = EEE.EmpSystemID
			                                LEFT JOIN [dbo].[ESICPolicyMaster] ESICPLMst ON DM.ESICPolicyMasterID = ESICPLMst.ID
                                WHERE E.GroupID = '" + para.GroupID + @"' AND E.PlantId = '" + para.PlantID + @"' AND --E.EmployeeStatus = 'Active'
                                      E.DOJ <= '" + para.ToDate + @"' AND (E.DOS > '" + para.FromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL
                                      OR E.DOS = '' OR E.DOS = '01/01/1901' OR E.EmployeeStatus = 'Active')
	                                  AND E.SystemID NOT IN (SELECT EmpSystemID FROM [dbo].[ESICEligibleEmployee]) AND E.SystemId IN (" + para.sEmpSystemID + @")";
                }
                strSQL += @"
                                ORDER BY E.GivenDesignationId, E.SystemId";

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
        public void XxGetUnTagEmployeeListWithESICPolicyMaster(ESICParaListNew para, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (para.sEmpSystemID == "")
                {
                    strSQL = @"SELECT EEE.ID ESICEligibleEmpID, DM.ESICPolicyMasterID, E.* 
                                FROM [dbo].[EmployeeInformation] E
			                                INNER JOIN   (SELECT DC.LeavePolicyMasterId,DC.ESICPolicyMasterID,DM.DesignationId FROM MST.DesignationMaster DM
                                LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                WHERE DC.PlantId='" + para.PlantID + @"' ) DM ON E.GivenDesignationId = DM.DesignationId
			                                LEFT JOIN (SELECT * FROM [dbo].[ESICEligibleEmployee] WHERE IsActive = 1) EEE ON E.SystemId = EEE.EmpSystemID
			                                LEFT JOIN [dbo].[ESICPolicyMaster] ESICPLMst ON DM.ESICPolicyMasterID = ESICPLMst.ID
                                WHERE E.GroupID = '" + para.GroupID + @"' AND E.PlantId = '" + para.PlantID + @"' AND --E.EmployeeStatus = 'Active'
                                      E.DOJ <= '" + para.ToDate + @"' AND (E.DOS > '" + para.FromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL
                                      OR E.DOS = '' OR E.DOS = '01/01/1901' OR E.EmployeeStatus = 'Active')
	                                  AND E.SystemID NOT IN (SELECT EmpSystemID FROM [dbo].[ESICEligibleEmployee])";
                }
                else
                {
                    strSQL = @"SELECT EEE.ID ESICEligibleEmpID, DM.ESICPolicyMasterID, E.* 
                                FROM [dbo].[EmployeeInformation] E
			                                INNER JOIN (SELECT DC.LeavePolicyMasterId,DC.ESICPolicyMasterID,DM.DesignationId FROM MST.DesignationMaster DM
                                            LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                            WHERE DC.PlantId='" + para.PlantID + @"' ) DM ON E.GivenDesignationId = DM.DesignationId
			                                LEFT JOIN (SELECT * FROM [dbo].[ESICEligibleEmployee] WHERE IsActive = 1) EEE ON E.SystemId = EEE.EmpSystemID
			                                LEFT JOIN [dbo].[ESICPolicyMaster] ESICPLMst ON DM.ESICPolicyMasterID = ESICPLMst.ID
                                WHERE E.GroupID = '" + para.GroupID + @"' AND E.PlantId = '" + para.PlantID + @"' AND --E.EmployeeStatus = 'Active'
                                      E.DOJ <= '" + para.ToDate + @"' AND (E.DOS > '" + para.FromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL
                                      OR E.DOS = '' OR E.DOS = '01/01/1901' OR E.EmployeeStatus = 'Active')
	                                  AND E.SystemID NOT IN (SELECT EmpSystemID FROM [dbo].[ESICEligibleEmployee]) AND E.SystemId IN (" + para.sEmpSystemID + @")";
                }
                strSQL += @"
                                ORDER BY E.GivenDesignationId, E.SystemId";

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



        public void GetTagEmployeeListWithESICPolicyMaster(ESICParaListNew para, string sESICMstSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (para.sEmpSystemID == "")
                {
                    strSQL = @"SELECT --EEE.ID 
                                 E.SystemId  ESICEligibleEmpID
                                , DM.ESICPolicyMasterID, E.* 
                                FROM [dbo].[EmployeeInformation] E
			                                INNER JOIN (SELECT DC.LeavePolicyMasterId,DC.ESICPolicyMasterID,DM.DesignationId FROM MST.DesignationMaster DM
                                            LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                            WHERE DC.PlantId='" + para.PlantID + @"' ) DM ON E.GivenDesignationId = DM.DesignationId
										    ---INNER JOIN (SELECT * FROM [dbo].[ESICEligibleEmployee] WHERE IsActive = 1) EEE ON E.SystemId = EEE.EmpSystemID
										    --LEFT JOIN [dbo].[SalaryInfoDefineMaster] SLR ON E.SystemId = SLR.EmpInfoSystemID
			                                LEFT JOIN [dbo].[ESICPolicyMaster] ESICPLMst ON DM.ESICPolicyMasterID = ESICPLMst.ID
                                WHERE E.GroupID = '" + para.GroupID + @"' AND E.PlantId = '" + para.PlantID + @"' AND --E.EmployeeStatus = 'Active'
                                      E.DOJ <= '" + para.ToDate + @"' AND (E.DOS > '" + para.FromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL
                                      OR E.DOS = '' OR E.DOS = '01/01/1901' OR E.EmployeeStatus = 'Active')
	                                  AND DM.ESICPolicyMasterID = '" + sESICMstSystemID + @"'";
                }
                else
                {
                    strSQL = @"SELECT --EEE.ID 
                                  E.SystemId  ESICEligibleEmpID
                                , DM.ESICPolicyMasterID, E.* 
                                FROM [dbo].[EmployeeInformation] E
			                                INNER JOIN (SELECT DC.LeavePolicyMasterId,DC.ESICPolicyMasterID,DM.DesignationId FROM MST.DesignationMaster DM
                                            LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                            WHERE DC.PlantId='" + para.PlantID + @"' ) DM ON E.GivenDesignationId = DM.DesignationId
										    ---INNER JOIN (SELECT * FROM [dbo].[ESICEligibleEmployee] WHERE IsActive = 1) EEE ON E.SystemId = EEE.EmpSystemID
										    --LEFT JOIN [dbo].[SalaryInfoDefineMaster] SLR ON E.SystemId = SLR.EmpInfoSystemID
			                                LEFT JOIN [dbo].[ESICPolicyMaster] ESICPLMst ON DM.ESICPolicyMasterID = ESICPLMst.ID
                                WHERE E.GroupID = '" + para.GroupID + @"' AND E.PlantId = '" + para.PlantID + @"' AND --E.EmployeeStatus = 'Active'
                                      E.DOJ <= '" + para.ToDate + @"' AND (E.DOS > '" + para.FromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL
                                      OR E.DOS = '' OR E.DOS = '01/01/1901' OR E.EmployeeStatus = 'Active')
	                                  AND DM.ESICPolicyMasterID = '" + sESICMstSystemID + @"' AND E.SystemId IN (" + para.sEmpSystemID + @")";
                }
                strSQL += @"
                              ORDER BY E.GivenDesignationId, E.SystemId";

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
        public void xGetTagEmployeeListWithESICPolicyMaster(ESICParaListNew para, string sESICMstSystemID, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (para.sEmpSystemID == "")
                {
                    strSQL = @"SELECT EEE.ID ESICEligibleEmpID, DM.ESICPolicyMasterID, E.* 
                                FROM [dbo].[EmployeeInformation] E
			                                INNER JOIN (SELECT DC.LeavePolicyMasterId,DC.ESICPolicyMasterID,DM.DesignationId FROM MST.DesignationMaster DM
                                            LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                            WHERE DC.PlantId='" + para.PlantID + @"' ) DM ON E.GivenDesignationId = DM.DesignationId
										    INNER JOIN (SELECT * FROM [dbo].[ESICEligibleEmployee] WHERE IsActive = 1) EEE ON E.SystemId = EEE.EmpSystemID
										    --LEFT JOIN [dbo].[SalaryInfoDefineMaster] SLR ON E.SystemId = SLR.EmpInfoSystemID
			                                LEFT JOIN [dbo].[ESICPolicyMaster] ESICPLMst ON DM.ESICPolicyMasterID = ESICPLMst.ID
                                WHERE E.GroupID = '" + para.GroupID + @"' AND E.PlantId = '" + para.PlantID + @"' AND --E.EmployeeStatus = 'Active'
                                      E.DOJ <= '" + para.ToDate + @"' AND (E.DOS > '" + para.FromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL
                                      OR E.DOS = '' OR E.DOS = '01/01/1901' OR E.EmployeeStatus = 'Active')
	                                  AND DM.ESICPolicyMasterID = '" + sESICMstSystemID + @"'";
                }
                else
                {
                    strSQL = @"SELECT EEE.ID ESICEligibleEmpID, DM.ESICPolicyMasterID, E.* 
                                FROM [dbo].[EmployeeInformation] E
			                                INNER JOIN (SELECT DC.LeavePolicyMasterId,DC.ESICPolicyMasterID,DM.DesignationId FROM MST.DesignationMaster DM
                                            LEFT JOIN SCS.DesignationMasterConfiguration DC ON DM.Id=DC.DesignationMasterId
                                            WHERE DC.PlantId='" + para.PlantID + @"' ) DM ON E.GivenDesignationId = DM.DesignationId
										    INNER JOIN (SELECT * FROM [dbo].[ESICEligibleEmployee] WHERE IsActive = 1) EEE ON E.SystemId = EEE.EmpSystemID
										    --LEFT JOIN [dbo].[SalaryInfoDefineMaster] SLR ON E.SystemId = SLR.EmpInfoSystemID
			                                LEFT JOIN [dbo].[ESICPolicyMaster] ESICPLMst ON DM.ESICPolicyMasterID = ESICPLMst.ID
                                WHERE E.GroupID = '" + para.GroupID + @"' AND E.PlantId = '" + para.PlantID + @"' AND --E.EmployeeStatus = 'Active'
                                      E.DOJ <= '" + para.ToDate + @"' AND (E.DOS > '" + para.FromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL
                                      OR E.DOS = '' OR E.DOS = '01/01/1901' OR E.EmployeeStatus = 'Active')
	                                  AND DM.ESICPolicyMasterID = '" + sESICMstSystemID + @"' AND E.SystemId IN (" + para.sEmpSystemID + @")";
                }
                strSQL += @"
                              ORDER BY E.GivenDesignationId, E.SystemId";

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
                // strEntryDate = AppDateConvert(strEntryDate, getUserDateFormat(), "MM/dd/yyyy").ToString("MM/dd/yyyy");
                strEntryDate = clsWebLib.AppDateConvert(strEntryDate, "MM/dd/yyyy", clsWebLib.getUserDateFormat()).ToShortDateString();

                strSql = "SELECT * FROM Signature WHERE Field ='" + strFieldName.Trim() + "' AND Dates = '" + strEntryDate + "'";

                SB = new System.Text.StringBuilder(strEntryDate);
                strID = SB.Replace(getUserDateSeparator().ToString(), "").ToString();

                objCoManager = new ConnectionManager.DAL.ConManager("1");
                objCoManager.OpenDataSetThroughAdapter(strSql, out dsLocal, false, false, "", "1");
                dtLocal = dsLocal.Tables[0];
                dvLocal = new DataView();

                dvLocal.Table = dtLocal;
                dvLocal.RowFilter = "Field ='" + strFieldName.Trim() + "'and Dates = '" + strEntryDate + "'";
                if (dvLocal.Count == 0)
                {// Add data
                    //LastNumber = 1 + SrNo;
                    LastNumber = 1;

                    drLocal = dtLocal.NewRow();
                    drLocal["Field"] = RetValidLen(strFieldName, 50);
                    drLocal["Dates"] = strEntryDate.Trim();
                    drLocal["LastNumber"] = LastNumber;
                    dtLocal.Rows.Add(drLocal);
                }
                else if (dvLocal.Count == 1)
                {
                    drLocal = dvLocal[0].Row;

                    LastNumber = Convert.ToDecimal(GetNumData(("" + drLocal["LastNumber"].ToString())));
                    //LastNumber = LastNumber + SrNo;
                    //LastNumber = LastNumber;

                    drLocal.BeginEdit();
                    drLocal["LastNumber"] = LastNumber + 1;
                    drLocal.EndEdit();
                }
                objCoManager.SaveDataSetThroughAdapter(ref dsLocal, false, "1");
                //strID = strID + "-" + ((int)LastNumber - SrNo);
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
        }//End Function
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
        }//End Function
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
        public static string getUserDateFormat()
        {
            System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
            return USER_TERMINAL_DATE_FORMAT.ShortDatePattern.ToString();
        }//End Function
        public static string getUserDateSeparator()
        {
            System.Globalization.DateTimeFormatInfo USER_TERMINAL_DATE_FORMAT = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat;
            return USER_TERMINAL_DATE_FORMAT.DateSeparator.ToString();
        }//End Function
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
        }//End Function
        public static object RetValidLen(string str)
        {

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

        }//End Function
        public static string GetNumData(string strNumber)
        {
            double d;
            strNumber = strNumber.Replace(",", "");
            System.Globalization.NumberFormatInfo n = new System.Globalization.NumberFormatInfo();
            if (strNumber.Trim() == "")
            { return "0"; }
            else if (Double.TryParse(strNumber, System.Globalization.NumberStyles.Float, n, out d) == true)
            {
                return strNumber;
            }
            else
            {
                return "0";
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

        private void ReLoadFormulaWithValue(string sEmpSystemID, ESICParaListNew para, string sFormulaID, bool bEarning, ref DataTable dtValue, ref DataTable dtSlrHd)
        {
            DataSet dsLocal = null;
            DataView dvLocal = null;
            DataView dvSlrHd = null;
            string strTemp = "";

            try
            {
                dsLocal = new DataSet();

                string strFormulaIDTemp = sFormulaID.Trim();
                string sLocalCurrencyID = para.LocalCurrencyID;
                string sForeignCurRate = para.ForeignCurRate;

                sFormulaValue = "";
                strFormulaIDTemp = strFormulaIDTemp.Replace("(", " ( ");
                strFormulaIDTemp = strFormulaIDTemp.Replace(")", " ) ");
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

                        dvLocal.RowFilter = "SalaryHeadID = '" + strTemp.Trim() + "' AND EmpSystemID = '" + sEmpSystemID + "'";
                        if (dvLocal.Count == 1)
                        {
                            if (bEarning == false)
                            {
                                if (dvLocal[0]["EntryCurrencyID"].ToString().Trim() == sLocalCurrencyID.Trim())
                                {
                                    strTemp = dvLocal[0]["EntryAmount"].ToString().Trim();
                                }
                                else
                                {
                                    strTemp = (Convert.ToDecimal(dvLocal[0]["EntryAmount"].ToString().Trim()) * Convert.ToDecimal(sForeignCurRate.Trim())).ToString();
                                }
                            }
                            else
                            {
                                decimal decAmount = Convert.ToDecimal(dvLocal[0]["EarningAmount"].ToString().Trim());

                                if (decAmount == 0)
                                { decAmount = Convert.ToDecimal(dvLocal[0]["EntryAmount"].ToString().Trim()); }

                                if (dvLocal[0]["EarningCurrencyID"].ToString().Trim() == sLocalCurrencyID.Trim())
                                {
                                    strTemp = dvLocal[0]["EarningAmount"].ToString().Trim();
                                }
                                else
                                {
                                    strTemp = (decAmount * Convert.ToDecimal(sForeignCurRate.Trim())).ToString();
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
        public static double Evaluate(string expression)
        {
            // That is some code instruction, is'nt it?
            return (double)new System.Xml.XPath.XPathDocument
            (new StringReader("<r/>")).CreateNavigator().Evaluate
            (string.Format("number({0})", new
            System.Text.RegularExpressions.Regex(@"([\+\-\*])")
            .Replace(expression, " ${1} ")
            .Replace("/", " div ")
            .Replace("%", " mod ")));
        }//End Function 
        public void LoadEmpSlrDefForSlrProcess(ESICParaListNew para, string sEmpInfo, out DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";

            try
            {
                strSql = @"SELECT * FROM 
		                            (
                                     SELECT SD.SystemID AS SlrInfoDefSystemID, SEFD.PlantID, SEFD.EmpInfoSystemID, SEFD.EffectiveDate, SEFD.SalaryRuleMasterSystemID, 
                                            SD.SalaryHeadID, SH.SalaryHead, SH.HeadType, SH.HeadCategory, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate,	
                                            SD.EntryCurrencyID, ECR.Name AS EntryCurrency, SD.EntryAmount, SD.DefineCurrencyID, SD.SalaryID,
                                            DECR.Name AS DefinitionCurrency, SD.DefineAmount, ISNULL(CRC.AccumulateExchangeRate, 0) AccumulateExchangeRate, 
                                            AcltExcDisbSlrHDID = CASE WHEN CRC.AccumulateExchangeRate = 1 THEN CRC.AccumulateExchangeSalaryHeadID
						                                              ELSE SD.SalaryHeadID END,
			                                CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.Name AS DisbusmentCurrency, 
			                                SlrDis.RuleType, SlrDis.FixedMonthDayValue, ISNULL(SlrDis.IsMonthDay, 0) IsMonthDay, ISNULL(SlrDis.IsMonthWorkDay, 0) IsMonthWorkDay,-- SlrDis.IsBankPayment, SlrDis.IsCashPayment,
                                            ISNULL(SlrDis.IsFixedDisbus, 0) IsFixedDisbus, SRDSM.SalaryRuleDayStatusSystemID, SRDSM.IsOverWrite, SRDSM.ShiftType, SRDSM.DayType, SRDSM.LeaveType,
											IsNetPayEffect = CASE WHEN (SlrDis.IsNetPayEffect IS NULL) AND (SRDSM.IsDSPNetPayEffect IS NOT NULL) THEN SRDSM.IsDSPNetPayEffect 
																  ELSE SlrDis.IsNetPayEffect END,
											SlrProc.DisbusmentAmount EarningAmount, SlrProc.DisbusmentCurrencyID EarningCurrencyID, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                  ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo, SRM.CurrencyRuleSystemID  
		                            FROM (
                                          SELECT SystemID, SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, 
	                                             AmtDefinitionCurrencyID, AmtDefinitionRate, AddedBy, DateAdded, UpdatedBy, DateUpdated
                                          FROM SalaryInfoDefine
                                            UNION
                                          (
                                           SELECT SystemID, SalaryID, SalaryHeadID, EntryCurrencyID, EntryAmount, DefineCurrencyID, DefineAmount, 
		                                          AmtDefinitionCurrencyID, AmtDefinitionRate, AddedBy, DateAdded, UpdatedBy, DateUpdated                   
                                           FROM SalaryInfoBack
                                          )
                                         ) SD
										INNER JOIN 
												(
												 SELECT SLM.* FROM 
                                                            (
                                                             SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
		                                                            IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
                                                             FROM SalaryInfoDefineMaster
                                                             UNION 
                                                            (
                                                             SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
		                                                            IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
                                                             FROM SalaryInfoBackMaster
                                                            )
                                                            ) SLM 
	                                                            INNER JOIN
			                                                            (
			                                                             SELECT EmpInfoSystemID, MAX(EffectiveDate) EffectiveDate 
			                                                             FROM 
				                                                             (
				                                                               SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
						                                                              IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
				                                                               FROM SalaryInfoDefineMaster
						                                                           UNION 
				                                                              (
					                                                            SELECT SystemID, EmpInfoSystemID, SalaryIncrementSystemID, SalaryRuleMasterSystemID, GroupID, PlantID, EffectiveDate, 
							                                                           IsApproved, ApprovedBy, DateApproved, AddedBy, DateAdded, UpdatedBy, DateUpdated 
					                                                            FROM SalaryInfoBackMaster
				                                                              )
				                                                             ) A
				                                                            WHERE IsApproved = 1 AND EffectiveDate <= '" + para.ToDate + @"'
				                                                            GROUP BY EmpInfoSystemID
			                                                            ) B ON SLM.EmpInfoSystemID = B.EmpInfoSystemID AND SLM.EffectiveDate = B.EffectiveDate
												) SEFD ON SD.SalaryID = SEFD.SystemID
			                            INNER JOIN EmployeeInformation E ON SEFD.EmpInfoSystemID = E.SystemID
			                            INNER JOIN SalaryHead SH ON SD.SalaryHeadID = SH.SalaryHeadID AND ISNULL(SH.HeadCategory, '') != 'Tax'
			                            INNER JOIN SalaryRuleMaster SRM ON SEFD.SalaryRuleMasterSystemID = SRM.SystemID
			                            LEFT JOIN CurrencyRuleChild CRC ON SRM.CurrencyRuleSystemID = CRC.MstSystemID AND SD.SalaryHeadID = CRC.SalaryHeadID
			                            LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
			                            LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
			                            LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
			                            LEFT JOIN 
					                            (
                                                 SELECT SalaryRuleMasterSystemID, srg.SalaryHeadID, 'Gen' RuleType, sh.PartOfNetPay IsNetPayEffect, FixedMonthDayValue, IsMonthDay, --ISNULL(IsBankPayment, Convert(bit, 'True')) IsBankPayment, ISNULL(IsCashPayment, Convert(bit, 'True')) IsCashPayment,
						                                IsMonthWorkDay, IsFixedDisbus FROM SalaryRuleGeneral srg
						                                LEFT JOIN SalaryHead AS sh ON sh.SalaryHeadID = srg.SalaryHeadID
						                            UNION
					                             (
                                                  SELECT SalaryRuleMasterSystemID, srg.SalaryHeadID, 'Abs' RuleType, sh.PartOfNetPay IsNetPayEffect, FixedMonthDayValue, IsMonthDay, --Convert(bit, 'True') IsBankPayment, Convert(bit, 'True') IsCashPayment, 
						                                 IsMonthWorkDay, IsFixedDisbus FROM SalaryRuleAbsenteeism srg
						                                LEFT JOIN SalaryHead AS sh ON sh.SalaryHeadID = srg.SalaryHeadID
                                                 )
                                                ) SlrDis ON SRM.SystemID = SlrDis.SalaryRuleMasterSystemID AND SD.SalaryHeadID = SlrDis.SalaryHeadID
			                            LEFT JOIN SalaryRuleDayStatusMaster SRDSM ON SRM.SystemID = SRDSM.SalaryRuleMasterSystemID
											                            AND SD.SalaryHeadID = SRDSM.SalaryHeadID
										LEFT JOIN 
										       (
												SELECT * FROM [dbo].[SalaryProcChild]
													WHERE SlrProcMstSystemID IN (
																				 SELECT SystemID FROM [dbo].[SalaryProcMaster]
																				  WHERE MonthNo = MONTH('" + para.ToDate + @"') AND YearNo = YEAR('" + para.ToDate + @"')
																				)
											   ) SlrProc ON E.SystemID = SlrProc.EmpInfoSystemID 
											                            AND SD.SalaryHeadID = SlrProc.SalaryHeadID 
                                        WHERE E.DOJ <= '" + para.ToDate + @"' AND (E.DOS >= '" + para.FromDate + @"' OR E.DOS IS NULL OR E.DOS = NULL 
                                                                               OR E.DOS = '' OR E.DOS = '01/01/1901')
                                              AND SEFD.IsApproved = 1 AND SEFD.EffectiveDate <= '" + para.ToDate + @"'
                                    ) A 
                                  WHERE (" + sEmpInfo + @") ";
                if (para.PlantID != "ALL" & para.PlantID != "")
                {
                    strSql += @" AND PlantID = '" + para.PlantID + @"' ";
                }

                strSql += @" ORDER BY EmpInfoSystemID, HeadType DESC";

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
        public void LoadCurrencyRule(ESICParaListNew para, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @"SELECT CRC.SystemID CurrencyRuleChildSystemID, CRC.MstSystemID CurrencyRuleSystemID, CRC.SalaryHeadID, SD.HeadType, 
                                  CRC.AmtEntryCurrency, ECR.Code AS EntryCrc, CRC.AmtDefinitionCurrency, DECR.Code AS DefinCr,
                                  CRC.AmtDisbusmentCurrency, DICR.Code AS DisbCr, CRC.AccumulateExchangeRate, 
                                  CRC.AccumulateExchangeSalaryHeadID, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                  ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo  
                            FROM CurrencyRuleChild CRC
												INNER JOIN CurrencyRuleMaster CRM ON CRC.MstSystemID = CRM.SystemID
					                            LEFT JOIN SCS.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
			                                    LEFT JOIN SCS.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
			                                    LEFT JOIN SCS.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id 
                                                LEFT JOIN SalaryHead SD ON CRC.SalaryHeadID = SD.SalaryHeadID
                            WHERE CRM.GroupID = '" + para.GroupID + @"' AND CRM.PlantId = '" + para.PlantID + @"'";

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
        public void GeneratorESICEligibleEmployee(ESICParaListNew para)
        {
            #region Variable Dataset
            DataTable dtValue = null;
            DataSet dsESICEligibleEmp = null;
            DataTable dtESICEligibleEmp = null;
            DataRow drESICEligibleEmp = null;
            DataView dvESICEligibleEmp = null;

            DataSet dsESICMntEmpWiseCal = null;
            DataTable dtESICMntEmpWiseCal = null;
            DataRow drESICMntEmpWiseCal = null;
            DataView dvESICMntEmpWiseCal = null;

            DataSet dsSalInfo = null;
            DataSet dsSalHd = null;
            DataTable dtSalHd = null;
            DataView dvSlrHd = null;
            DataSet dsESICPolicyMst = null;
            DataSet dsESICPolicyDtl = null;
            DataSet dsESICMonthNo = null;
            DataSet dsUnTagEmp = null;
            DataSet dsCurRl = null;
            DataTable dtCurRl = null;
            DataView dvCurRl = null;
            //clsSalaryStructureAplos obSS = new global::clsSalaryStructureAplos();

            #endregion Variable Dataset
            #region Declare Variable

            string sESICEligibleEmpID = "";
            string sESICMntEmpCalID = "";
            string sESICMstID = "";
            string sESICDtlID = "";
            string sGroupID = para.GroupID;
            string sPlantID = para.PlantID;
            string sESICElgGentID = "";
            string sESICDedGentID = "";
            string sFormulaID = "";
            string sFormulaResult = "";
            string sEmpInfoSysIDColl = "";
            string sEmpSystemID = "";
            string sEmpSysID = "";
            string sEntCurID = "";
            string sEarnCurID = "";
            string sSlrHD = "";
            string sFormulaDesIDEmp = "";
            string sFormulaDesIDEmpr = "";
            string strTemp = "";
            string sRoundOption = "";
            string sCurrencyRuleSystemID = "";

            int TotSelectEmpForProc = 0;
            int TotProcComp = 0;
            int grdRowMaxCnt = 0;
            int SelectedEmpCnt = 0;
            int EmpCntForLoop = 0;
            int iDecimalNo = 0;

            string sESICContSalaryHeadIDEmp = "";
            string sESICContSalaryHeadIDEmpr = "";

            DateTime dtStartDate;
            DateTime dtEndDate;

            decimal decEntCur = 0;
            decimal decEarnCur = 0;
            decimal decEarningValueRangeFrom = 0;
            decimal decEarningValueRangeTo = 0;
            decimal decFixedValueEmp = 0;
            decimal decFixedValueEmpr = 0;
            decimal decEmpCtbtnAmount = 0;
            decimal decEmprCtbtnAmount = 0;

            bool bMaturity = false;
            bool bIsActive = true;
            bool bIsFixedEmp = false;
            bool bIsFormulaEmp = false;
            bool bIsContributionSlrHDdependOnEarningEmp = false;

            bool bIsFixedEmpr = false;
            bool bIsFormulaEmpr = false;
            bool bIsContributionSlrHDdependOnEarningEmpr = false;
            bool bEarning = false;

            bool bIntegerInDisb = false;
            bool bIsDecimalInDisb = false;
            #endregion Declare Variable

            try
            {
                LoadCurrencyRule(para, out dsCurRl);
                dtCurRl = dsCurRl.Tables[0];
                dvCurRl = new DataView();

                GetESICPolicyMaster("", sGroupID.Trim(), sPlantID.Trim(), out dsESICPolicyMst);

                if (dsESICPolicyMst.Tables[0].Rows.Count > 0)
                {
                    for (int ESICPlCnt = 0; ESICPlCnt < dsESICPolicyMst.Tables[0].Rows.Count; ESICPlCnt++)
                    {
                        sESICMstID = dsESICPolicyMst.Tables[0].Rows[ESICPlCnt]["ID"].ToString().Trim();

                        #region DataSet

                        GetESICPolicyDetails(sESICMstID, out dsESICPolicyDtl);
                        GetESICPolicyMonthNo(sESICMstID, out dsESICMonthNo);

                        GetSalaryHead(out dsSalHd);
                        dtSalHd = dsSalHd.Tables[0];

                        #endregion DataSet

                        #region Tag Employee List

                        GetTagEmployeeListWithESICPolicyMaster(para, sESICMstID.Trim(), out dsUnTagEmp);
                        if (dsUnTagEmp.Tables[0].Rows.Count > 0)
                        {
                            sEmpInfoSysIDColl = "";
                            sEmpSystemID = "";
                            TotSelectEmpForProc = dsUnTagEmp.Tables[0].Rows.Count;
                            TotProcComp = 0;
                            grdRowMaxCnt = 0;
                            SelectedEmpCnt = 0;
                            EmpCntForLoop = 0;

                            while (SelectedEmpCnt < dsUnTagEmp.Tables[0].Rows.Count)
                            {
                                sEmpInfoSysIDColl = "";
                                sEmpSystemID = "";
                                EmpCntForLoop = 0;

                                if ((SelectedEmpCnt + 1) <= dsUnTagEmp.Tables[0].Rows.Count)
                                {
                                    grdRowMaxCnt = dsUnTagEmp.Tables[0].Rows.Count - TotProcComp;
                                }
                                else
                                {
                                    grdRowMaxCnt = 30;
                                }

                                #region Employee System ID Collection

                                for (int iUnTgEmCnt = SelectedEmpCnt; iUnTgEmCnt < grdRowMaxCnt + SelectedEmpCnt; iUnTgEmCnt++)
                                {
                                    if (string.IsNullOrEmpty(sEmpInfoSysIDColl) == true)
                                    {
                                        sEmpInfoSysIDColl = "EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                        sEmpSystemID = "EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                    }
                                    else
                                    {
                                        sEmpInfoSysIDColl += " OR EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                        sEmpSystemID += " OR EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                    }
                                    EmpCntForLoop++;
                                }

                                #endregion Employee System ID Collection

                                if (EmpCntForLoop == grdRowMaxCnt)
                                {
                                    GetESICEligibleEmployee(sEmpSystemID, out dsESICEligibleEmp);
                                    dtESICEligibleEmp = dsESICEligibleEmp.Tables[0];
                                    dvESICEligibleEmp = new DataView();

                                    GetESICMonthlyEmpWiseCalculation(sEmpSystemID, out dsESICMntEmpWiseCal);
                                    dtESICMntEmpWiseCal = dsESICMntEmpWiseCal.Tables[0];
                                    dvESICMntEmpWiseCal = new DataView();

                                    //Get General Salary Amount Head Wise
                                    List<dicSalInfoNew> dicSalInfo = new List<dicSalInfoNew>();
                                    //if (para.dsSalInfo == null)
                                    //{
                                    LoadEmpSlrDefForSlrProcess(para, sEmpInfoSysIDColl, out dsSalInfo);
                                    if (dsSalInfo.Tables[0].Rows.Count > 0)
                                        dicSalInfo = dsSalInfo.Tables[0].ToList<dicSalInfoNew>();
                                    //}
                                    //else
                                    //{
                                    //    if (para.dsSalInfo.Tables[0].Rows.Count > 0)
                                    //        dicSalInfo = para.dsSalInfo.Tables[0].ToList<dicSalInfo>();
                                    //}

                                    sESICElgGentID = "";
                                    sESICDedGentID = "";
                                    sESICElgGentID = "ECE" + sESICElgGentID;

                                    GenRefSrNoID(DateTime.Now.ToShortDateString().ToString(), "ESIC_CALCULATION", dsUnTagEmp.Tables[0].Rows.Count, out sESICDedGentID);
                                    sESICDedGentID = "ECC" + sESICDedGentID;
                                    for (int iUnTgEmCnt = 0; iUnTgEmCnt < dsUnTagEmp.Tables[0].Rows.Count; iUnTgEmCnt++)
                                    {
                                        sESICEligibleEmpID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["ESICEligibleEmpID"].ToString().Trim();
                                        sESICMntEmpCalID = sESICDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();
                                        sEmpSysID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim();

                                        #region Salary Amount Insert Into Virtual Table

                                        dtValue = new DataTable();
                                        dtValue.TableName = "TempTable";
                                        dtValue.Columns.Add("EmpSystemID");
                                        dtValue.Columns.Add("SalaryHeadID");
                                        dtValue.Columns.Add("EntryCurrencyID");
                                        dtValue.Columns.Add("EntryAmount");
                                        dtValue.Columns.Add("EarningCurrencyID");
                                        dtValue.Columns.Add("EarningAmount");
                                        dtValue.Columns.Add("DecimalNo");
                                        dtValue.Columns.Add("IntegerInDisb");
                                        dtValue.Columns.Add("IsDecimalInDisb");
                                        dtValue.Columns.Add("RoundOption");

                                        //if (para.dsSalInfo == null)
                                        //{
                                        var dicSalInfo_Sub = dicSalInfo.FindAll(x => x.EmpInfoSystemID == sEmpSysID);
                                        if (dicSalInfo_Sub.Count > 0)
                                        {
                                            sCurrencyRuleSystemID = dicSalInfo_Sub[0].CurrencyRuleSystemID;
                                            if (para.dsSalInfo == null)
                                            {
                                                for (int i = 0; i < dicSalInfo_Sub.Count; i++)
                                                {
                                                    sSlrHD = dicSalInfo_Sub[i].SalaryHeadID;
                                                    sEntCurID = dicSalInfo_Sub[i].EntryCurrencyID;
                                                    decEntCur = dicSalInfo_Sub[i].EntryAmount;
                                                    sEarnCurID = dicSalInfo_Sub[i].EarningCurrencyID;
                                                    decEarnCur = dicSalInfo_Sub[i].EarningAmount;

                                                    iDecimalNo = dicSalInfo_Sub[i].DecimalNo;
                                                    bIntegerInDisb = dicSalInfo_Sub[i].IntegerInDisb;
                                                    bIsDecimalInDisb = dicSalInfo_Sub[i].IsDecimalInDisb;
                                                    sRoundOption = dicSalInfo_Sub[i].RoundOption;

                                                    #region For SalaryHead Wise Amount In Virtual 2nd Table

                                                    DataRow dtValueRow = dtValue.NewRow();
                                                    dtValueRow["EmpSystemID"] = dicSalInfo_Sub[i].EmpInfoSystemID;
                                                    dtValueRow["SalaryHeadID"] = sSlrHD;
                                                    dtValueRow["EntryCurrencyID"] = sEntCurID;
                                                    dtValueRow["EntryAmount"] = decEntCur;
                                                    dtValueRow["EarningCurrencyID"] = sEarnCurID;
                                                    dtValueRow["EarningAmount"] = decEarnCur;
                                                    dtValueRow["DecimalNo"] = iDecimalNo;
                                                    dtValueRow["IntegerInDisb"] = bIntegerInDisb;
                                                    dtValueRow["IsDecimalInDisb"] = bIsDecimalInDisb;
                                                    dtValueRow["RoundOption"] = sRoundOption;

                                                    dtValue.Rows.Add(dtValueRow);

                                                    #endregion For SalaryHead Wise Amount In Virtual 2nd Table

                                                    if (dicSalInfo_Sub[i].HeadCategory == "ESIC Employee Contribution")
                                                    {
                                                        sESICContSalaryHeadIDEmp = dicSalInfo_Sub[i].SalaryHeadID;
                                                    }
                                                    if (dicSalInfo_Sub[i].HeadCategory == "ESIC Employer Contribution")
                                                    {
                                                        sESICContSalaryHeadIDEmpr = dicSalInfo_Sub[i].SalaryHeadID;
                                                    }
                                                }
                                            }
                                        }
                                        //}
                                        //else
                                        if (para.dsSalInfo != null)
                                        {
                                            dtValue = para.dsSalInfo.Tables[0];
                                            strTemp = "ESIC Employee Contribution";

                                            dvSlrHd = new DataView();
                                            dvSlrHd.Table = dtSalHd;
                                            dvSlrHd.RowFilter = "HeadCategory = '" + strTemp.Trim() + "'";
                                            if (dvSlrHd.Count > 0)
                                            { sESICContSalaryHeadIDEmp = dvSlrHd[0].Row["SalaryHeadID"].ToString().Trim(); }

                                            strTemp = "ESIC Employer Contribution";

                                            dvSlrHd.Table = dtSalHd;
                                            dvSlrHd.RowFilter = "HeadCategory = '" + strTemp.Trim() + "'";
                                            if (dvSlrHd.Count > 0)
                                            { sESICContSalaryHeadIDEmpr = dvSlrHd[0].Row["SalaryHeadID"].ToString().Trim(); }
                                        }

                                        #endregion Salary Amount Insert Into Virtual Table
                                        if (dtValue.Rows.Count > 0)
                                        {
                                            for (int iESICDtl = 0; iESICDtl < dsESICPolicyDtl.Tables[0].Rows.Count; iESICDtl++)
                                            {
                                                #region Clear

                                                sFormulaDesIDEmp = "";
                                                sFormulaDesIDEmpr = "";

                                                decFixedValueEmp = 0;
                                                decFixedValueEmpr = 0;
                                                decEmpCtbtnAmount = 0;
                                                decEmprCtbtnAmount = 0;
                                                decEarningValueRangeFrom = 0;
                                                decEarningValueRangeTo = 0;

                                                dtEndDate = System.DateTime.Now;
                                                bMaturity = false;
                                                bEarning = false;
                                                bIsActive = true;
                                                bIsFixedEmpr = false;
                                                bIsFormulaEmpr = false;
                                                bIsFixedEmp = false;
                                                bIsFormulaEmp = false;
                                                bIsContributionSlrHDdependOnEarningEmp = false;
                                                bIsContributionSlrHDdependOnEarningEmpr = false;

                                                #endregion Clear

                                                #region Select ESICPolicyDetails ID if have multiple column

                                                sFormulaID = dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["FormulaDesIDEarning"].ToString().Trim();
                                                ReLoadFormulaWithValue(sEmpSysID, para, sFormulaID, bEarning, ref dtValue, ref dtSalHd);
                                                sFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();
                                                decEarningValueRangeFrom = Convert.ToDecimal(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["EarningValueRangeFrom"].ToString().Trim());
                                                decEarningValueRangeTo = Convert.ToDecimal(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["EarningValueRangeTo"].ToString().Trim());

                                                if (Convert.ToDecimal(sFormulaResult) > decEarningValueRangeFrom && Convert.ToDecimal(sFormulaResult) < decEarningValueRangeTo)
                                                {
                                                    bMaturity = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsMandatory"].ToString().Trim());
                                                }
                                                else
                                                {
                                                    bMaturity = false;

                                                    //if (para.bStructure == true)
                                                    //{
                                                    //    bIsActive = false;
                                                    //}
                                                    //else
                                                    //{
                                                    //string sCurrentMonthName = System.DateTime.Now.ToString("MMMMMMMMMMMMM").Substring(0, 3);
                                                    string sCurrentMonthName = para.ToDate/*System.DateTime.Now.ToString("dd-MMM-yyyy")*/;
                                                    string sMatDt = "";

                                                    if (dsESICMonthNo.Tables[0].Rows.Count > 0)
                                                    {
                                                        for (int iMnt = 0; iMnt < dsESICMonthNo.Tables[0].Rows.Count; iMnt++)
                                                        {
                                                            sMatDt = "01-" + dsESICMonthNo.Tables[0].Rows[iMnt]["MonthName"].ToString().Substring(0, 3) + "-" + System.DateTime.Now.Year.ToString();
                                                            if (Convert.ToDateTime(sCurrentMonthName).Month == Convert.ToDateTime(sMatDt).Month)
                                                            {
                                                                bIsActive = false;
                                                                //dtEndDate = Convert.ToDateTime(sMatDt);
                                                                dtEndDate = Convert.ToDateTime(para.ToDate);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        bIsActive = false;
                                                    }
                                                    //}
                                                }
                                                sESICDtlID = dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["ID"].ToString().Trim();

                                                sFormulaDesIDEmp = dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["FormulaDesIDEmp"].ToString().Trim();
                                                decFixedValueEmp = Convert.ToDecimal(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["FixedValueEmp"].ToString().Trim());
                                                bIsFixedEmp = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsFixedEmp"].ToString().Trim());
                                                bIsFormulaEmp = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsFormulaEmp"].ToString().Trim());
                                                bIsContributionSlrHDdependOnEarningEmp = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsContributionSlrHDdependOnEarningEmp"].ToString().Trim());

                                                sFormulaDesIDEmpr = dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["FormulaDesIDEmployer"].ToString().Trim();
                                                decFixedValueEmpr = Convert.ToDecimal(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["FixedValueEmployer"].ToString().Trim());
                                                bIsFixedEmpr = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsFixedEmployer"].ToString().Trim());
                                                bIsFormulaEmpr = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsFormulaEmployer"].ToString().Trim());
                                                bIsContributionSlrHDdependOnEarningEmpr = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsContributionSlrHDdependOnEarningEmployer"].ToString().Trim());

                                                #endregion Select ESICPolicyDetails ID if have multiple column

                                                #region Employee Contribution Amount

                                                if (bIsFixedEmp == true)
                                                {
                                                    decEmpCtbtnAmount = decFixedValueEmp;
                                                }
                                                else if (bIsFormulaEmp == true)
                                                {
                                                    bEarning = bIsContributionSlrHDdependOnEarningEmp;
                                                    ReLoadFormulaWithValue(sEmpSysID, para, sFormulaDesIDEmp, bEarning, ref dtValue, ref dtSalHd);
                                                    sFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();

                                                    decEmpCtbtnAmount = Convert.ToDecimal(sFormulaResult);
                                                }

                                                #endregion Employee Contribution Amount

                                                #region Employer Contribution Amount

                                                if (bIsFixedEmpr == true)
                                                {
                                                    decEmprCtbtnAmount = decFixedValueEmpr;
                                                }
                                                else if (bIsFormulaEmpr == true)
                                                {
                                                    bEarning = bIsContributionSlrHDdependOnEarningEmpr;
                                                    ReLoadFormulaWithValue(sEmpSysID, para, sFormulaDesIDEmpr, bEarning, ref dtValue, ref dtSalHd);
                                                    sFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();

                                                    decEmprCtbtnAmount = Convert.ToDecimal(sFormulaResult);
                                                }

                                                //*******kabir*********//


                                                para.IsESICMandatoryNew = bMaturity;
                                                if (bMaturity == false)
                                                {
                                                    para.IsESICOptionalNew = true;
                                                }






                                                #endregion Employer Contribution Amount

                                                #region Data Save IN Table [ESICMonthlyEmpWiseCalculation]

                                                if (bIsActive == true)
                                                {
                                                    dvESICMntEmpWiseCal.Table = dtESICMntEmpWiseCal;
                                                    dvESICMntEmpWiseCal.RowFilter = "ESICEligibleEmpID = '" + sESICEligibleEmpID + "' AND MonthNo = '" + Convert.ToDateTime(para.ToDate).Month + "' AND YearNo = '" + Convert.ToDateTime(para.ToDate).Year + "'";
                                                    if (dvESICMntEmpWiseCal.Count == 0)
                                                    {//Add new block
                                                        drESICMntEmpWiseCal = dtESICMntEmpWiseCal.NewRow();
                                                        UpdateTheDataRowInTableESICMonthlyEmpWiseCalculation("ADDNEW", sESICMntEmpCalID, sESICEligibleEmpID, para.ToDate, decEmpCtbtnAmount, decEmprCtbtnAmount, para.sUser, ref drESICMntEmpWiseCal);
                                                        dtESICMntEmpWiseCal.Rows.Add(drESICMntEmpWiseCal);
                                                    }
                                                    else
                                                    {//edit block
                                                        drESICMntEmpWiseCal = dvESICMntEmpWiseCal[0].Row;
                                                        drESICMntEmpWiseCal.BeginEdit();
                                                        UpdateTheDataRowInTableESICMonthlyEmpWiseCalculation("EDIT", sESICMntEmpCalID, sESICEligibleEmpID, para.ToDate, decEmpCtbtnAmount, decEmprCtbtnAmount, para.sUser, ref drESICMntEmpWiseCal);
                                                        drESICMntEmpWiseCal.EndEdit();
                                                    }
                                                }

                                                #endregion Data Save IN Table [ESICMonthlyEmpWiseCalculation]

                                                #region Data Save IN Table [ESICEligibleEmployee]

                                                dvESICEligibleEmp.Table = dtESICEligibleEmp;
                                                dvESICEligibleEmp.RowFilter = "ID = '" + sESICEligibleEmpID.Trim() + "'";
                                                if (dvESICEligibleEmp.Count == 1)
                                                {//Edit block
                                                    drESICEligibleEmp = dvESICEligibleEmp[0].Row;
                                                    drESICEligibleEmp.BeginEdit();
                                                    UpdateTheDataRowInTableESICEligibleEmp("EDIT", sESICEligibleEmpID.Trim(), sEmpSysID, sESICMstID, sESICDtlID, System.DateTime.Now, dtEndDate, bIsActive, bMaturity, para.sUser, ref drESICEligibleEmp);
                                                    drESICEligibleEmp.EndEdit();
                                                }

                                                #endregion Data Save IN Table [ESICEligibleEmployee]

                                                //if(bIsActive == false & para.bStructure == true)
                                                //{
                                                //    dvESICMntEmpWiseCal.Table = dtESICMntEmpWiseCal;
                                                //    dvESICMntEmpWiseCal.RowFilter = "ESICEligibleEmpID = '" + sESICEligibleEmpID + "'";
                                                //    if (dvESICMntEmpWiseCal.Count > 0)
                                                //    {
                                                //        while (dvESICMntEmpWiseCal.Count > 0)
                                                //        {
                                                //            drESICMntEmpWiseCal = dvESICMntEmpWiseCal[0].Row;
                                                //            drESICMntEmpWiseCal.Delete();
                                                //        }
                                                //    }

                                                //    dvESICEligibleEmp.Table = dtESICEligibleEmp;
                                                //    dvESICEligibleEmp.RowFilter = "ID = '" + sESICEligibleEmpID.Trim() + "'";
                                                //    if (dvESICEligibleEmp.Count > 1)
                                                //    {
                                                //        while (dvESICEligibleEmp.Count > 0)
                                                //        {
                                                //            drESICEligibleEmp = dvESICEligibleEmp[0].Row;
                                                //            drESICEligibleEmp.Delete();
                                                //        }
                                                //    }
                                                //}
                                            }
                                        }
                                    }

                                    ////SaveDataSets(dsESICEligibleEmp, dsESICMntEmpWiseCal);
                                }
                                ////if (SelectedEmpCnt == grdRowMaxCnt)
                                ////{
                                TotProcComp += grdRowMaxCnt;
                                TotSelectEmpForProc -= grdRowMaxCnt;
                                //if (bIsActive == false & para.bStructure == true)
                                //{
                                //    SaveDataSets(dsESICMntEmpWiseCal, dsESICEligibleEmp);
                                //}
                                //else
                                //{
                                //SaveDataSets(dsESICEligibleEmp, dsESICMntEmpWiseCal);  //KABIRE
                                //}
                                ////}
                                if ((dsUnTagEmp.Tables[0].Rows.Count - TotProcComp) < 30)
                                {
                                    SelectedEmpCnt += (dsUnTagEmp.Tables[0].Rows.Count - TotProcComp);

                                    if (SelectedEmpCnt <= 0)
                                    { SelectedEmpCnt = dsUnTagEmp.Tables[0].Rows.Count + 1; }
                                }
                                else
                                {
                                    SelectedEmpCnt += 30;
                                }
                                dsESICEligibleEmp = null;
                                dsESICMntEmpWiseCal = null;
                            }
                            //if (bMaturity == true)
                            //{
                            //SaveDataSets(dsESICEligibleEmp, dsESICMntEmpWiseCal);
                            //}
                        }

                        #endregion Tag Employee List

                        //if (para.ShouldNotProcessUntaggedEmp == false)
                        //{
                        //    #region Untag Employee List

                        //    GetUnTagEmployeeListWithESICPolicyMaster(para, out dsUnTagEmp);
                        //    if (dsUnTagEmp.Tables[0].Rows.Count > 0)
                        //    {
                        //        sEmpInfoSysIDColl = "";
                        //        sEmpSystemID = "";
                        //        TotSelectEmpForProc = dsUnTagEmp.Tables[0].Rows.Count;
                        //        TotProcComp = 0;
                        //        grdRowMaxCnt = 0;
                        //        SelectedEmpCnt = 0;
                        //        EmpCntForLoop = 0;

                        //        while (SelectedEmpCnt < dsUnTagEmp.Tables[0].Rows.Count)
                        //        {
                        //            sEmpInfoSysIDColl = "";
                        //            sEmpSystemID = "";
                        //            EmpCntForLoop = 0;

                        //            if ((SelectedEmpCnt + 1) <= dsUnTagEmp.Tables[0].Rows.Count)
                        //            {
                        //                grdRowMaxCnt = dsUnTagEmp.Tables[0].Rows.Count - TotProcComp;
                        //            }
                        //            else
                        //            {
                        //                grdRowMaxCnt = 30;
                        //            }

                        //            #region Employee System ID Collection

                        //            for (int iUnTgEmCnt = SelectedEmpCnt; iUnTgEmCnt < grdRowMaxCnt + SelectedEmpCnt; iUnTgEmCnt++)
                        //            {
                        //                if (string.IsNullOrEmpty(sEmpInfoSysIDColl) == true)
                        //                {
                        //                    sEmpInfoSysIDColl = "EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                        //                    sEmpSystemID = "EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                        //                }
                        //                else
                        //                {
                        //                    sEmpInfoSysIDColl += " OR EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                        //                    sEmpSystemID += " OR EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                        //                }
                        //                EmpCntForLoop++;
                        //            }

                        //            #endregion Employee System ID Collection

                        //            if (EmpCntForLoop == grdRowMaxCnt)
                        //            {
                        //                GetESICEligibleEmployee(sEmpSystemID, out dsESICEligibleEmp);
                        //                dtESICEligibleEmp = dsESICEligibleEmp.Tables[0];
                        //                dvESICEligibleEmp = new DataView();

                        //                GetESICMonthlyEmpWiseCalculation(sEmpSystemID, out dsESICMntEmpWiseCal);
                        //                dtESICMntEmpWiseCal = dsESICMntEmpWiseCal.Tables[0];
                        //                dvESICMntEmpWiseCal = new DataView();

                        //                //Get General Salary Amount Head Wise
                        //                List<dicSalInfo> dicSalInfo = new List<dicSalInfo>();
                        //                ////LoadEmpSlrDefForSlrProcess(para, sEmpSysIDColl, out dsSalInfo);
                        //                ////if (dsSalInfo.Tables[0].Rows.Count > 0)
                        //                ////    dicSalInfo = dsSalInfo.Tables[0].ToList<dicSalInfo>();
                        //                //if (para.dsSalInfo == null)
                        //                //{
                        //                LoadEmpSlrDefForSlrProcess(para, sEmpInfoSysIDColl, out dsSalInfo);
                        //                if (dsSalInfo.Tables[0].Rows.Count > 0)
                        //                    dicSalInfo = dsSalInfo.Tables[0].ToList<dicSalInfo>();
                        //                //}
                        //                ////else
                        //                ////{
                        //                ////    if (para.dsSalInfo.Tables[0].Rows.Count > 0)
                        //                ////        dicSalInfo = para.dsSalInfo.Tables[0].ToList<dicSalInfo>();
                        //                ////}

                        //                sESICElgGentID = "";
                        //                sESICDedGentID = "";
                        //                GenRefSrNoID(DateTime.Now.ToShortDateString().ToString(), "ESIC_ELIGIBLE", dsUnTagEmp.Tables[0].Rows.Count, out sESICElgGentID);
                        //                sESICElgGentID = "ECE" + sESICElgGentID;

                        //                GenRefSrNoID(DateTime.Now.ToShortDateString().ToString(), "ESIC_CALCULATION", dsUnTagEmp.Tables[0].Rows.Count, out sESICDedGentID);
                        //                sESICDedGentID = "ECC" + sESICDedGentID;
                        //                for (int iUnTgEmCnt = SelectedEmpCnt; iUnTgEmCnt < grdRowMaxCnt + SelectedEmpCnt; iUnTgEmCnt++)
                        //                {
                        //                    sESICEligibleEmpID = sESICElgGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();
                        //                    sESICMntEmpCalID = sESICDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();
                        //                    sEmpSysID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim();

                        //                    //if (sEmpSysID == "2018-10008" || sEmpSysID == "2018-10009" || sEmpSysID == "2018-1001" || sEmpSysID == "2018-10010" || sEmpSysID == "2018-10011" || sEmpSysID == "2018-10012" || sEmpSysID == "2018-10013" || sEmpSysID == "2018-10015" || sEmpSysID == "2018-10019")
                        //                    //{
                        //                    //    string a = "";
                        //                    //}

                        //                    #region Master Table Data Capture [Start Date]

                        //                    dtStartDate = Convert.ToDateTime(dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["DOJ"].ToString().Trim());

                        //                    #endregion Master Table Data Capture 

                        //                    #region Salary Amount Insert Into Virtual Table

                        //                    dtValue = new DataTable();
                        //                    dtValue.TableName = "TempTable";
                        //                    dtValue.Columns.Add("EmpSystemID");
                        //                    dtValue.Columns.Add("SalaryHeadID");
                        //                    dtValue.Columns.Add("EntryCurrencyID");
                        //                    dtValue.Columns.Add("EntryAmount");
                        //                    dtValue.Columns.Add("EarningCurrencyID");
                        //                    dtValue.Columns.Add("EarningAmount");
                        //                    dtValue.Columns.Add("DecimalNo");
                        //                    dtValue.Columns.Add("IntegerInDisb");
                        //                    dtValue.Columns.Add("IsDecimalInDisb");
                        //                    dtValue.Columns.Add("RoundOption");

                        //                    //if (para.dsSalInfo == null)
                        //                    //{
                        //                    var dicSalInfo_Sub = dicSalInfo.FindAll(x => x.EmpInfoSystemID == sEmpSysID);
                        //                    if (dicSalInfo_Sub.Count > 0)
                        //                    {
                        //                        sCurrencyRuleSystemID = dicSalInfo_Sub[0].CurrencyRuleSystemID;
                        //                        if (para.dsSalInfo == null)
                        //                        {
                        //                            for (int i = 0; i < dicSalInfo_Sub.Count; i++)
                        //                            {
                        //                                sSlrHD = dicSalInfo_Sub[i].SalaryHeadID;
                        //                                sEntCurID = dicSalInfo_Sub[i].EntryCurrencyID;
                        //                                decEntCur = dicSalInfo_Sub[i].EntryAmount;
                        //                                sEarnCurID = dicSalInfo_Sub[i].EarningCurrencyID;
                        //                                decEarnCur = dicSalInfo_Sub[i].EarningAmount;

                        //                                iDecimalNo = dicSalInfo_Sub[i].DecimalNo;
                        //                                bIntegerInDisb = dicSalInfo_Sub[i].IntegerInDisb;
                        //                                bIsDecimalInDisb = dicSalInfo_Sub[i].IsDecimalInDisb;
                        //                                sRoundOption = dicSalInfo_Sub[i].RoundOption;

                        //                                #region For SalaryHead Wise Amount In Virtual 2nd Table

                        //                                DataRow dtValueRow = dtValue.NewRow();
                        //                                dtValueRow["EmpSystemID"] = dicSalInfo_Sub[i].EmpInfoSystemID;
                        //                                dtValueRow["SalaryHeadID"] = sSlrHD;
                        //                                dtValueRow["EntryCurrencyID"] = sEntCurID;
                        //                                dtValueRow["EntryAmount"] = decEntCur;
                        //                                dtValueRow["EarningCurrencyID"] = sEarnCurID;
                        //                                dtValueRow["EarningAmount"] = decEarnCur;
                        //                                dtValueRow["DecimalNo"] = iDecimalNo;
                        //                                dtValueRow["IntegerInDisb"] = bIntegerInDisb;
                        //                                dtValueRow["IsDecimalInDisb"] = bIsDecimalInDisb;
                        //                                dtValueRow["RoundOption"] = sRoundOption;

                        //                                dtValue.Rows.Add(dtValueRow);

                        //                                #endregion For SalaryHead Wise Amount In Virtual 2nd Table

                        //                                if (dicSalInfo_Sub[i].HeadCategory == "ESIC Employee Contribution")
                        //                                {
                        //                                    sESICContSalaryHeadIDEmp = dicSalInfo_Sub[i].SalaryHeadID;
                        //                                }
                        //                                if (dicSalInfo_Sub[i].HeadCategory == "ESIC Employer Contribution")
                        //                                {
                        //                                    sESICContSalaryHeadIDEmpr = dicSalInfo_Sub[i].SalaryHeadID;
                        //                                }
                        //                            }
                        //                        }
                        //                    }
                        //                    //}
                        //                    //else
                        //                    if (para.dsSalInfo != null)
                        //                    {
                        //                        dtValue = para.dsSalInfo.Tables[0];
                        //                        strTemp = "ESIC Employee Contribution";

                        //                        dvSlrHd = new DataView();
                        //                        dvSlrHd.Table = dtSalHd;
                        //                        dvSlrHd.RowFilter = "HeadCategory = '" + strTemp.Trim() + "'";
                        //                        if (dvSlrHd.Count > 0)
                        //                        { sESICContSalaryHeadIDEmp = dvSlrHd[0].Row["SalaryHeadID"].ToString().Trim(); }

                        //                        strTemp = "ESIC Employer Contribution";

                        //                        dvSlrHd.Table = dtSalHd;
                        //                        dvSlrHd.RowFilter = "HeadCategory = '" + strTemp.Trim() + "'";
                        //                        if (dvSlrHd.Count > 0)
                        //                        { sESICContSalaryHeadIDEmpr = dvSlrHd[0].Row["SalaryHeadID"].ToString().Trim(); }
                        //                    }

                        //                    #endregion Salary Amount Insert Into Virtual Table
                        //                    if (dtValue.Rows.Count > 0)
                        //                    {
                        //                        for (int iESICDtl = 0; iESICDtl < dsESICPolicyDtl.Tables[0].Rows.Count; iESICDtl++)
                        //                        {
                        //                            #region Clear

                        //                            sFormulaDesIDEmp = "";
                        //                            sFormulaDesIDEmpr = "";

                        //                            decFixedValueEmp = 0;
                        //                            decFixedValueEmpr = 0;
                        //                            decEmpCtbtnAmount = 0;
                        //                            decEmprCtbtnAmount = 0;
                        //                            decEarningValueRangeFrom = 0;
                        //                            decEarningValueRangeTo = 0;

                        //                            dtEndDate = System.DateTime.Now;
                        //                            bMaturity = false;
                        //                            bEarning = false;
                        //                            bIsActive = true;
                        //                            bIsFixedEmpr = false;
                        //                            bIsFormulaEmpr = false;
                        //                            bIsFixedEmp = false;
                        //                            bIsFormulaEmp = false;
                        //                            bIsContributionSlrHDdependOnEarningEmp = false;
                        //                            bIsContributionSlrHDdependOnEarningEmpr = false;

                        //                            #endregion Clear

                        //                            #region Select ESICPolicyDetails ID if have multiple column

                        //                            sFormulaID = dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["FormulaDesIDEarning"].ToString().Trim();
                        //                            ReLoadFormulaWithValue(sEmpSysID, para, sFormulaID, bEarning, ref dtValue, ref dtSalHd);
                        //                            sFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();
                        //                            decEarningValueRangeFrom = Convert.ToDecimal(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["EarningValueRangeFrom"].ToString().Trim());
                        //                            decEarningValueRangeTo = Convert.ToDecimal(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["EarningValueRangeTo"].ToString().Trim());

                        //                            if (Convert.ToDecimal(sFormulaResult) > decEarningValueRangeFrom && Convert.ToDecimal(sFormulaResult) < decEarningValueRangeTo)
                        //                            {
                        //                                bMaturity = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsMandatory"].ToString().Trim());
                        //                            }
                        //                            else
                        //                            {
                        //                                bMaturity = false;
                        //                                //string sCurrentMonthName = System.DateTime.Now.ToString("MMMMMMMMMMMMM").Substring(0, 3);
                        //                                string sCurrentMonthName = System.DateTime.Now.ToString("dd-MMM-yyyy");
                        //                                string sMatDt = "";

                        //                                if (dsESICMonthNo.Tables[0].Rows.Count > 0)
                        //                                {
                        //                                    for (int iMnt = 0; iMnt < dsESICMonthNo.Tables[0].Rows.Count; iMnt++)
                        //                                    {
                        //                                        sMatDt = "01-" + dsESICMonthNo.Tables[0].Rows[iMnt]["MonthName"].ToString().Substring(0, 3) + "-" + System.DateTime.Now.Year.ToString();
                        //                                        if (Convert.ToDateTime(sCurrentMonthName) == Convert.ToDateTime(sMatDt))
                        //                                        {
                        //                                            bIsActive = false;
                        //                                            dtEndDate = Convert.ToDateTime(sMatDt);
                        //                                        }
                        //                                    }
                        //                                }
                        //                                else
                        //                                {
                        //                                    bIsActive = false;
                        //                                }
                        //                            }
                        //                            sESICDtlID = dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["ID"].ToString().Trim();

                        //                            sFormulaDesIDEmp = dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["FormulaDesIDEmp"].ToString().Trim();
                        //                            decFixedValueEmp = Convert.ToDecimal(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["FixedValueEmp"].ToString().Trim());
                        //                            bIsFixedEmp = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsFixedEmp"].ToString().Trim());
                        //                            bIsFormulaEmp = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsFormulaEmp"].ToString().Trim());
                        //                            bIsContributionSlrHDdependOnEarningEmp = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsContributionSlrHDdependOnEarningEmp"].ToString().Trim());

                        //                            sFormulaDesIDEmpr = dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["FormulaDesIDEmployer"].ToString().Trim();
                        //                            decFixedValueEmpr = Convert.ToDecimal(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["FixedValueEmployer"].ToString().Trim());
                        //                            bIsFixedEmpr = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsFixedEmployer"].ToString().Trim());
                        //                            bIsFormulaEmpr = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsFormulaEmployer"].ToString().Trim());
                        //                            bIsContributionSlrHDdependOnEarningEmpr = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsContributionSlrHDdependOnEarningEmployer"].ToString().Trim());

                        //                            #endregion Select ESICPolicyDetails ID if have multiple column
                        //                            if (bMaturity == true)
                        //                            {
                        //                                #region Employee Contribution Amount

                        //                                if (bIsFixedEmp == true)
                        //                                {
                        //                                    decEmpCtbtnAmount = decFixedValueEmp;
                        //                                }
                        //                                else if (bIsFormulaEmp == true)
                        //                                {
                        //                                    bEarning = bIsContributionSlrHDdependOnEarningEmp;
                        //                                    ReLoadFormulaWithValue(sEmpSysID, para, sFormulaDesIDEmp, bEarning, ref dtValue, ref dtSalHd);
                        //                                    sFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();

                        //                                    decEmpCtbtnAmount = Convert.ToDecimal(sFormulaResult);
                        //                                }

                        //                                #endregion Employee Contribution Amount

                        //                                #region Employer Contribution Amount

                        //                                if (bIsFixedEmpr == true)
                        //                                {
                        //                                    decEmprCtbtnAmount = decFixedValueEmpr;
                        //                                }
                        //                                else if (bIsFormulaEmpr == true)
                        //                                {
                        //                                    bEarning = bIsContributionSlrHDdependOnEarningEmpr;
                        //                                    ReLoadFormulaWithValue(sEmpSysID, para, sFormulaDesIDEmpr, bEarning, ref dtValue, ref dtSalHd);
                        //                                    sFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();

                        //                                    decEmprCtbtnAmount = Convert.ToDecimal(sFormulaResult);
                        //                                }

                        //                                #endregion Employer Contribution Amount

                        //                                #region Data Save IN Table [ESICMonthlyEmpWiseCalculation]

                        //                                if (bIsActive == true)
                        //                                {
                        //                                    dvESICMntEmpWiseCal.Table = dtESICMntEmpWiseCal;
                        //                                    dvESICMntEmpWiseCal.RowFilter = "ID = '" + sESICMntEmpCalID.Trim() + "'";
                        //                                    if (dvESICMntEmpWiseCal.Count == 0)
                        //                                    {//Add new block
                        //                                        drESICMntEmpWiseCal = dtESICMntEmpWiseCal.NewRow();
                        //                                        UpdateTheDataRowInTableESICMonthlyEmpWiseCalculation("ADDNEW", sESICMntEmpCalID, sESICEligibleEmpID, para.ToDate, decEmpCtbtnAmount, decEmprCtbtnAmount, para.sUser, ref drESICMntEmpWiseCal);
                        //                                        dtESICMntEmpWiseCal.Rows.Add(drESICMntEmpWiseCal);
                        //                                    }
                        //                                    else
                        //                                    {//edit block
                        //                                        drESICMntEmpWiseCal = dvESICMntEmpWiseCal[0].Row;
                        //                                        drESICMntEmpWiseCal.BeginEdit();
                        //                                        UpdateTheDataRowInTableESICMonthlyEmpWiseCalculation("EDIT", sESICMntEmpCalID, sESICEligibleEmpID, para.ToDate, decEmpCtbtnAmount, decEmprCtbtnAmount, para.sUser, ref drESICMntEmpWiseCal);
                        //                                        drESICMntEmpWiseCal.EndEdit();
                        //                                    }
                        //                                }

                        //                                #endregion Data Save IN Table [ESICMonthlyEmpWiseCalculation]

                        //                                #region Data Save IN Table [ESICEligibleEmployee]

                        //                                dvESICEligibleEmp.Table = dtESICEligibleEmp;
                        //                                dvESICEligibleEmp.RowFilter = "ID = '" + sESICEligibleEmpID.Trim() + "'";
                        //                                if (dvESICEligibleEmp.Count == 0)
                        //                                {//Add new block
                        //                                    drESICEligibleEmp = dtESICEligibleEmp.NewRow();
                        //                                    UpdateTheDataRowInTableESICEligibleEmp("ADDNEW", sESICEligibleEmpID.Trim(), sEmpSysID, sESICMstID, sESICDtlID, dtStartDate, dtEndDate, bIsActive, bMaturity, para.sUser, ref drESICEligibleEmp);
                        //                                    dtESICEligibleEmp.Rows.Add(drESICEligibleEmp);
                        //                                }

                        //                                #endregion Data Save IN Table [ESICEligibleEmployee]

                        //                            }
                        //                        }
                        //                    }
                        //                }
                        //            }
                        //            //if (SelectedEmpCnt == grdRowMaxCnt)
                        //            //{
                        //            TotProcComp += grdRowMaxCnt;
                        //            TotSelectEmpForProc -= grdRowMaxCnt;
                        //            SaveDataSets(dsESICEligibleEmp, dsESICMntEmpWiseCal);
                        //            //}
                        //            if ((dsUnTagEmp.Tables[0].Rows.Count - TotProcComp) < 30)
                        //            {
                        //                SelectedEmpCnt += (dsUnTagEmp.Tables[0].Rows.Count - TotProcComp);

                        //                if (SelectedEmpCnt <= 0)
                        //                { SelectedEmpCnt = dsUnTagEmp.Tables[0].Rows.Count + 1; }
                        //            }
                        //            else
                        //            {
                        //                SelectedEmpCnt += 30;
                        //            }
                        //            dsESICEligibleEmp = null;
                        //            dsESICMntEmpWiseCal = null;
                        //        }
                        //        //if (bMaturity == true)
                        //        //{
                        //        //SaveDataSets(dsESICEligibleEmp, dsESICMntEmpWiseCal);
                        //        //}
                        //    }

                        //    #endregion Untag Employee
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                #region Clear DataSet 

                dsESICEligibleEmp = null;
                dtESICEligibleEmp = null;
                drESICEligibleEmp = null;
                dvESICEligibleEmp = null;

                dsESICMntEmpWiseCal = null;
                dtESICMntEmpWiseCal = null;
                drESICMntEmpWiseCal = null;
                dvESICMntEmpWiseCal = null;

                dsSalInfo = null;
                dsESICPolicyMst = null;
                dsESICPolicyDtl = null;
                dsESICMonthNo = null;
                dsUnTagEmp = null;

                #endregion Clear DataSet 
            }
        }///End Function
        public void GeneratorESICEligibleEmployeeForSalaryStracture(ESICParaListNew para, out DataTable dtESICMntEmpWiseCal)
        {
            #region Variable Dataset
            DataTable dtValue = null;
            //DataSet dsESICEligibleEmp = null;
            //DataTable dtESICEligibleEmp = null;
            //DataRow drESICEligibleEmp = null;
            //DataView dvESICEligibleEmp = null;

            //DataSet dsESICMntEmpWiseCal = null;
            //DataTable dtESICMntEmpWiseCal = null;
            DataRow drESICMntEmpWiseCal = null;
            DataView dvESICMntEmpWiseCal = null;

            dtESICMntEmpWiseCal = new DataTable();
            dtESICMntEmpWiseCal.TableName = "TempTable";
            dtESICMntEmpWiseCal.Columns.Add("ID");
            dtESICMntEmpWiseCal.Columns.Add("ESICEligibleEmpID");
            dtESICMntEmpWiseCal.Columns.Add("MonthNo");
            dtESICMntEmpWiseCal.Columns.Add("YearNo");
            dtESICMntEmpWiseCal.Columns.Add("EmployeeContributionAmount");
            dtESICMntEmpWiseCal.Columns.Add("EmployerContributionAmount");
            dtESICMntEmpWiseCal.Columns.Add("AddedBy");
            dtESICMntEmpWiseCal.Columns.Add("AddedDate");
            dtESICMntEmpWiseCal.Columns.Add("AddedFromIP");
            dtESICMntEmpWiseCal.Columns.Add("UpdatedBy");
            dtESICMntEmpWiseCal.Columns.Add("UpdatedDate");
            dtESICMntEmpWiseCal.Columns.Add("UpdatedFromIP");


            DataSet dsSalInfo = null;
            DataSet dsSalHd = null;
            DataTable dtSalHd = null;
            DataView dvSlrHd = null;
            DataSet dsESICPolicyMst = null;
            DataSet dsESICPolicyDtl = null;
            DataSet dsESICMonthNo = null;
            DataSet dsUnTagEmp = null;
            DataSet dsCurRl = null;
            DataTable dtCurRl = null;
            DataView dvCurRl = null;
            //clsSalaryStructureAplos obSS = new global::clsSalaryStructureAplos();

            #endregion Variable Dataset
            #region Declare Variable

            string sESICEligibleEmpID = "";
            string sESICMntEmpCalID = "";
            string sESICMstID = "";
            string sESICDtlID = "";
            string sGroupID = para.GroupID;
            string sPlantID = para.PlantID;
            string sESICElgGentID = "";
            string sESICDedGentID = "";
            string sFormulaID = "";
            string sFormulaResult = "";
            decimal sFormulaResultInDecimal = 0;
            string sEmpInfoSysIDColl = "";
            string sEmpSystemID = "";
            string sEmpSysID = "";
            string sEntCurID = "";
            string sEarnCurID = "";
            string sSlrHD = "";
            string sFormulaDesIDEmp = "";
            string sFormulaDesIDEmpr = "";
            string strTemp = "";
            string sRoundOption = "";
            string sCurrencyRuleSystemID = "";

            int TotSelectEmpForProc = 0;
            int TotProcComp = 0;
            int grdRowMaxCnt = 0;
            int SelectedEmpCnt = 0;
            int EmpCntForLoop = 0;
            int iDecimalNo = 0;

            string sESICContSalaryHeadIDEmp = "";
            string sESICContSalaryHeadIDEmpr = "";

            DateTime dtStartDate;
            DateTime dtEndDate;

            decimal decEntCur = 0;
            decimal decEarnCur = 0;
            decimal decEarningValueRangeFrom = 0;
            decimal decEarningValueRangeTo = 0;
            decimal decFixedValueEmp = 0;
            decimal decFixedValueEmpr = 0;
            decimal decEmpCtbtnAmount = 0;
            decimal decEmprCtbtnAmount = 0;
            bool IsESICEntitle = false;
            bool bMaturity = false;
            bool bIsActive = true;
            bool bIsFixedEmp = false;
            bool bIsFormulaEmp = false;
            bool bIsContributionSlrHDdependOnEarningEmp = false;

            bool bIsFixedEmpr = false;
            bool bIsFormulaEmpr = false;
            bool bIsContributionSlrHDdependOnEarningEmpr = false;
            bool bEarning = false;

            bool bIntegerInDisb = false;
            bool bIsDecimalInDisb = false;
            #endregion Declare Variable

            try
            {
                LoadCurrencyRule(para, out dsCurRl);
                dtCurRl = dsCurRl.Tables[0];
                dvCurRl = new DataView();

                GetESICPolicyMaster("", sGroupID.Trim(), sPlantID.Trim(), out dsESICPolicyMst);

                if (dsESICPolicyMst.Tables[0].Rows.Count > 0)
                {
                    for (int ESICPlCnt = 0; ESICPlCnt < dsESICPolicyMst.Tables[0].Rows.Count; ESICPlCnt++)
                    {
                        sESICMstID = dsESICPolicyMst.Tables[0].Rows[ESICPlCnt]["ID"].ToString().Trim();

                        #region DataSet

                        GetESICPolicyDetails(sESICMstID, out dsESICPolicyDtl);
                        GetESICPolicyMonthNo(sESICMstID, out dsESICMonthNo);

                        GetSalaryHead(out dsSalHd);
                        dtSalHd = dsSalHd.Tables[0];

                        #endregion DataSet

                        #region Tag Employee List

                        GetTagEmployeeListWithESICPolicyMaster(para, sESICMstID.Trim(), out dsUnTagEmp);
                        if (dsUnTagEmp.Tables[0].Rows.Count > 0)
                        {
                            para.IsESICPolicyDefined = true;

                            sEmpInfoSysIDColl = "";
                            sEmpSystemID = "";
                            TotSelectEmpForProc = dsUnTagEmp.Tables[0].Rows.Count;
                            TotProcComp = 0;
                            grdRowMaxCnt = 0;
                            SelectedEmpCnt = 0;
                            EmpCntForLoop = 0;

                            while (SelectedEmpCnt < dsUnTagEmp.Tables[0].Rows.Count)
                            {
                                sEmpInfoSysIDColl = "";
                                sEmpSystemID = "";
                                EmpCntForLoop = 0;

                                if ((SelectedEmpCnt + 1) <= dsUnTagEmp.Tables[0].Rows.Count)
                                {
                                    grdRowMaxCnt = dsUnTagEmp.Tables[0].Rows.Count - TotProcComp;
                                }
                                else
                                {
                                    grdRowMaxCnt = 30;
                                }

                                #region Employee System ID Collection

                                for (int iUnTgEmCnt = SelectedEmpCnt; iUnTgEmCnt < grdRowMaxCnt + SelectedEmpCnt; iUnTgEmCnt++)
                                {
                                    if (string.IsNullOrEmpty(sEmpInfoSysIDColl) == true)
                                    {
                                        sEmpInfoSysIDColl = "EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                        sEmpSystemID = "EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                    }
                                    else
                                    {
                                        sEmpInfoSysIDColl += " OR EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                        sEmpSystemID += " OR EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                    }
                                    EmpCntForLoop++;
                                }

                                #endregion Employee System ID Collection

                                if (EmpCntForLoop == grdRowMaxCnt)
                                {

                                    dvESICMntEmpWiseCal = new DataView();

                                    //Get General Salary Amount Head Wise
                                    List<dicSalInfoNew> dicSalInfo = new List<dicSalInfoNew>();
                                    //if (para.dsSalInfo == null)
                                    //{
                                    LoadEmpSlrDefForSlrProcess(para, sEmpInfoSysIDColl, out dsSalInfo);
                                    if (dsSalInfo.Tables[0].Rows.Count > 0)
                                        dicSalInfo = dsSalInfo.Tables[0].ToList<dicSalInfoNew>();

                                    sESICElgGentID = "";
                                    sESICDedGentID = "";
                                    sESICElgGentID = "ECE" + sESICElgGentID;

                                    GenRefSrNoID(DateTime.Now.ToShortDateString().ToString(), "ESIC_CALCULATION", dsUnTagEmp.Tables[0].Rows.Count, out sESICDedGentID);
                                    sESICDedGentID = "ECC" + sESICDedGentID;
                                    for (int iUnTgEmCnt = 0; iUnTgEmCnt < dsUnTagEmp.Tables[0].Rows.Count; iUnTgEmCnt++)
                                    {
                                        sESICEligibleEmpID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["ESICEligibleEmpID"].ToString().Trim();
                                        sESICMntEmpCalID = sESICDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();
                                        sEmpSysID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim();

                                        #region Salary Amount Insert Into Virtual Table

                                        dtValue = new DataTable();
                                        dtValue.TableName = "TempTable";
                                        dtValue.Columns.Add("EmpSystemID");
                                        dtValue.Columns.Add("SalaryHeadID");
                                        dtValue.Columns.Add("EntryCurrencyID");
                                        dtValue.Columns.Add("EntryAmount");
                                        dtValue.Columns.Add("EarningCurrencyID");
                                        dtValue.Columns.Add("EarningAmount");
                                        dtValue.Columns.Add("DecimalNo");
                                        dtValue.Columns.Add("IntegerInDisb");
                                        dtValue.Columns.Add("IsDecimalInDisb");
                                        dtValue.Columns.Add("RoundOption");

                                        //if (para.dsSalInfo == null)
                                        //{
                                        var dicSalInfo_Sub = dicSalInfo.FindAll(x => x.EmpInfoSystemID == sEmpSysID);
                                        if (dicSalInfo_Sub.Count > 0)
                                        {
                                            sCurrencyRuleSystemID = dicSalInfo_Sub[0].CurrencyRuleSystemID;
                                            if (para.dsSalInfo == null)
                                            {
                                                for (int i = 0; i < dicSalInfo_Sub.Count; i++)
                                                {
                                                    sSlrHD = dicSalInfo_Sub[i].SalaryHeadID;
                                                    sEntCurID = dicSalInfo_Sub[i].EntryCurrencyID;
                                                    decEntCur = dicSalInfo_Sub[i].EntryAmount;
                                                    sEarnCurID = dicSalInfo_Sub[i].EarningCurrencyID;
                                                    decEarnCur = dicSalInfo_Sub[i].EarningAmount;

                                                    iDecimalNo = dicSalInfo_Sub[i].DecimalNo;
                                                    bIntegerInDisb = dicSalInfo_Sub[i].IntegerInDisb;
                                                    bIsDecimalInDisb = dicSalInfo_Sub[i].IsDecimalInDisb;
                                                    sRoundOption = dicSalInfo_Sub[i].RoundOption;

                                                    #region For SalaryHead Wise Amount In Virtual 2nd Table

                                                    DataRow dtValueRow = dtValue.NewRow();
                                                    dtValueRow["EmpSystemID"] = dicSalInfo_Sub[i].EmpInfoSystemID;
                                                    dtValueRow["SalaryHeadID"] = sSlrHD;
                                                    dtValueRow["EntryCurrencyID"] = sEntCurID;
                                                    dtValueRow["EntryAmount"] = decEntCur;
                                                    dtValueRow["EarningCurrencyID"] = sEarnCurID;
                                                    dtValueRow["EarningAmount"] = decEarnCur;
                                                    dtValueRow["DecimalNo"] = iDecimalNo;
                                                    dtValueRow["IntegerInDisb"] = bIntegerInDisb;
                                                    dtValueRow["IsDecimalInDisb"] = bIsDecimalInDisb;
                                                    dtValueRow["RoundOption"] = sRoundOption;

                                                    dtValue.Rows.Add(dtValueRow);

                                                    #endregion For SalaryHead Wise Amount In Virtual 2nd Table

                                                    if (dicSalInfo_Sub[i].HeadCategory == "ESIC Employee Contribution")
                                                    {
                                                        sESICContSalaryHeadIDEmp = dicSalInfo_Sub[i].SalaryHeadID;
                                                    }
                                                    if (dicSalInfo_Sub[i].HeadCategory == "ESIC Employer Contribution")
                                                    {
                                                        sESICContSalaryHeadIDEmpr = dicSalInfo_Sub[i].SalaryHeadID;
                                                    }
                                                }
                                            }
                                        }
                                        //}
                                        //else
                                        if (para.dsSalInfo != null)
                                        {
                                            dtValue = para.dsSalInfo.Tables[0];
                                            strTemp = "ESIC Employee Contribution";

                                            dvSlrHd = new DataView();
                                            dvSlrHd.Table = dtSalHd;
                                            dvSlrHd.RowFilter = "HeadCategory = '" + strTemp.Trim() + "'";
                                            if (dvSlrHd.Count > 0)
                                            { sESICContSalaryHeadIDEmp = dvSlrHd[0].Row["SalaryHeadID"].ToString().Trim(); }

                                            strTemp = "ESIC Employer Contribution";

                                            dvSlrHd.Table = dtSalHd;
                                            dvSlrHd.RowFilter = "HeadCategory = '" + strTemp.Trim() + "'";
                                            if (dvSlrHd.Count > 0)
                                            { sESICContSalaryHeadIDEmpr = dvSlrHd[0].Row["SalaryHeadID"].ToString().Trim(); }
                                        }

                                        #endregion Salary Amount Insert Into Virtual Table
                                        if (dtValue.Rows.Count > 0)
                                        {
                                            for (int iESICDtl = 0; iESICDtl < dsESICPolicyDtl.Tables[0].Rows.Count; iESICDtl++)
                                            {
                                                #region Clear

                                                sFormulaDesIDEmp = "";
                                                sFormulaDesIDEmpr = "";

                                                decFixedValueEmp = 0;
                                                decFixedValueEmpr = 0;
                                                decEmpCtbtnAmount = 0;
                                                decEmprCtbtnAmount = 0;
                                                decEarningValueRangeFrom = 0;
                                                decEarningValueRangeTo = 0;

                                                dtEndDate = System.DateTime.Now;
                                                bMaturity = false;
                                                bEarning = false;
                                                bIsActive = true;
                                                bIsFixedEmpr = false;
                                                bIsFormulaEmpr = false;
                                                bIsFixedEmp = false;
                                                bIsFormulaEmp = false;
                                                bIsContributionSlrHDdependOnEarningEmp = false;
                                                bIsContributionSlrHDdependOnEarningEmpr = false;

                                                #endregion Clear

                                                #region Select ESICPolicyDetails ID if have multiple column

                                                sFormulaID = dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["FormulaDesIDEarning"].ToString().Trim();
                                                ReLoadFormulaWithValue(sEmpSysID, para, sFormulaID, bEarning, ref dtValue, ref dtSalHd);
                                                try
                                                {
                                                    sFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();
                                                    sFormulaResultInDecimal = Convert.ToDecimal(sFormulaResult);
                                                }
                                                catch (Exception ex)
                                                {
                                                    throw new Exception("ESIC Formula Issue: " + sFormulaValue);
                                                }
                                                decEarningValueRangeFrom = Convert.ToDecimal(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["EarningValueRangeFrom"].ToString().Trim());
                                                decEarningValueRangeTo = Convert.ToDecimal(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["EarningValueRangeTo"].ToString().Trim());



                                                if (sFormulaResultInDecimal > decEarningValueRangeFrom && sFormulaResultInDecimal <= decEarningValueRangeTo)
                                                {
                                                    bMaturity = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsMandatory"].ToString().Trim());
                                                    IsESICEntitle = true;
                                                }
                                                else
                                                {
                                                    bMaturity = false;
                                                    IsESICEntitle = false;
                                                    //if (para.bStructure == true)
                                                    //{
                                                    //    bIsActive = false;
                                                    //}
                                                    //else
                                                    //{
                                                    //string sCurrentMonthName = System.DateTime.Now.ToString("MMMMMMMMMMMMM").Substring(0, 3);
                                                    string sCurrentMonthName = para.ToDate/*System.DateTime.Now.ToString("dd-MMM-yyyy")*/;
                                                    string sMatDt = "";

                                                    if (dsESICMonthNo.Tables[0].Rows.Count > 0)
                                                    {
                                                        for (int iMnt = 0; iMnt < dsESICMonthNo.Tables[0].Rows.Count; iMnt++)
                                                        {
                                                            sMatDt = "01-" + dsESICMonthNo.Tables[0].Rows[iMnt]["MonthName"].ToString().Substring(0, 3) + "-" + System.DateTime.Now.Year.ToString();
                                                            if (Convert.ToDateTime(sCurrentMonthName).Month == Convert.ToDateTime(sMatDt).Month)
                                                            {
                                                                bIsActive = false;
                                                                //dtEndDate = Convert.ToDateTime(sMatDt);
                                                                dtEndDate = Convert.ToDateTime(para.ToDate);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        bIsActive = false;
                                                    }
                                                    //}
                                                }
                                                sESICDtlID = dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["ID"].ToString().Trim();

                                                sFormulaDesIDEmp = dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["FormulaDesIDEmp"].ToString().Trim();
                                                decFixedValueEmp = Convert.ToDecimal(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["FixedValueEmp"].ToString().Trim());
                                                bIsFixedEmp = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsFixedEmp"].ToString().Trim());
                                                bIsFormulaEmp = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsFormulaEmp"].ToString().Trim());
                                                bIsContributionSlrHDdependOnEarningEmp = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsContributionSlrHDdependOnEarningEmp"].ToString().Trim());

                                                sFormulaDesIDEmpr = dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["FormulaDesIDEmployer"].ToString().Trim();
                                                decFixedValueEmpr = Convert.ToDecimal(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["FixedValueEmployer"].ToString().Trim());
                                                bIsFixedEmpr = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsFixedEmployer"].ToString().Trim());
                                                bIsFormulaEmpr = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsFormulaEmployer"].ToString().Trim());
                                                bIsContributionSlrHDdependOnEarningEmpr = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsContributionSlrHDdependOnEarningEmployer"].ToString().Trim());

                                                #endregion Select ESICPolicyDetails ID if have multiple column

                                                #region Employee Contribution Amount

                                                if (bIsFixedEmp == true)
                                                {
                                                    decEmpCtbtnAmount = decFixedValueEmp;
                                                }
                                                else if (bIsFormulaEmp == true)
                                                {
                                                    bEarning = bIsContributionSlrHDdependOnEarningEmp;
                                                    ReLoadFormulaWithValue(sEmpSysID, para, sFormulaDesIDEmp, bEarning, ref dtValue, ref dtSalHd);
                                                    sFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();

                                                    decEmpCtbtnAmount = Convert.ToDecimal(sFormulaResult);
                                                }

                                                #endregion Employee Contribution Amount

                                                #region Employer Contribution Amount

                                                if (bIsFixedEmpr == true)
                                                {
                                                    decEmprCtbtnAmount = decFixedValueEmpr;
                                                }
                                                else if (bIsFormulaEmpr == true)
                                                {
                                                    bEarning = bIsContributionSlrHDdependOnEarningEmpr;
                                                    ReLoadFormulaWithValue(sEmpSysID, para, sFormulaDesIDEmpr, bEarning, ref dtValue, ref dtSalHd);
                                                    sFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();

                                                    decEmprCtbtnAmount = Convert.ToDecimal(sFormulaResult);
                                                }

                                                //*******kabir*********//

                                                //if (para.IsbuttonPFClicked == "YES")
                                                //{
                                                //    //AsPerCbxCalculatePF = _para.IsPFEntitle;
                                                //    if (para.IsESICEntitleNew==true)
                                                //    {
                                                //        bMaturity = para.IsESICEntitleNew;
                                                //    }

                                                //}
                                                //else
                                                //{
                                                //    // old
                                                //    para.IsESICMandatoryNew = bMaturity;
                                                //    if (bMaturity == false)
                                                //    {
                                                //        para.IsESICOptionalNew = true;
                                                //    }
                                                //}

                                                if (para.IsbuttonPFClicked == "YES")
                                                {
                                                    //AsPerCbxCalculatePF = _para.IsPFEntitle;

                                                    if (para.IsESICEntitleNew == true)
                                                    {
                                                        IsESICEntitle = para.IsESICEntitleNew;
                                                    }


                                                }
                                                else
                                                {
                                                    // old
                                                    //if (bIsAllEmpApplocable)
                                                    //{
                                                    //    IsESICEntitle = true;
                                                    //    para.IsESICEntitleNew = true;
                                                    //}
                                                    //IsESICEntitle = true;
                                                    if (IsESICEntitle)
                                                    {
                                                        para.IsESICEntitleNew = true;
                                                    }
                                                   
                                                    if (bMaturity)
                                                    {
                                                        IsESICEntitle = true;
                                                        para.IsESICEntitleNew = true;
                                                    }
                                                    para.IsESICMandatoryNew = bMaturity;
                                                    if (bMaturity == false)
                                                    {
                                                        para.IsESICOptionalNew = true;
                                                    }
                                                }


                                                //para.IsESICMandatoryNew = bMaturity;
                                                //if (bMaturity == false)
                                                //{
                                                //    para.IsESICOptionalNew = true;
                                                //}






                                                #endregion Employer Contribution Amount

                                                #region Data Save IN Table [ESICMonthlyEmpWiseCalculation]
                                                //if (bIsActive == true)
                                                //{
                                                //if (bMaturity == true)
                                                //{
                                                if (IsESICEntitle == true || bMaturity == true)///cal all time 
                                                {
                                                    dvESICMntEmpWiseCal.Table = dtESICMntEmpWiseCal;
                                                    dvESICMntEmpWiseCal.RowFilter = "ESICEligibleEmpID = '" + sESICEligibleEmpID + "' AND MonthNo = '" + Convert.ToDateTime(para.ToDate).Month + "' AND YearNo = '" + Convert.ToDateTime(para.ToDate).Year + "'";
                                                    if (dvESICMntEmpWiseCal.Count == 0)
                                                    {//Add new block
                                                        drESICMntEmpWiseCal = dtESICMntEmpWiseCal.NewRow();
                                                        UpdateTheDataRowInTableESICMonthlyEmpWiseCalculation("ADDNEW", sESICMntEmpCalID, sESICEligibleEmpID, para.ToDate, decEmpCtbtnAmount, decEmprCtbtnAmount, para.sUser, ref drESICMntEmpWiseCal);
                                                        dtESICMntEmpWiseCal.Rows.Add(drESICMntEmpWiseCal);
                                                    }
                                                    else
                                                    {//edit block
                                                        drESICMntEmpWiseCal = dvESICMntEmpWiseCal[0].Row;
                                                        drESICMntEmpWiseCal.BeginEdit();
                                                        UpdateTheDataRowInTableESICMonthlyEmpWiseCalculation("EDIT", sESICMntEmpCalID, sESICEligibleEmpID, para.ToDate, decEmpCtbtnAmount, decEmprCtbtnAmount, para.sUser, ref drESICMntEmpWiseCal);
                                                        drESICMntEmpWiseCal.EndEdit();
                                                    }
                                                }

                                                #endregion Data Save IN Table [ESICMonthlyEmpWiseCalculation]

                                                #region Data Save IN Table [ESICEligibleEmployee]

                                                //dvESICEligibleEmp.Table = dtESICEligibleEmp;
                                                //dvESICEligibleEmp.RowFilter = "ID = '" + sESICEligibleEmpID.Trim() + "'";
                                                //if (dvESICEligibleEmp.Count == 1)
                                                //{//Edit block
                                                //    drESICEligibleEmp = dvESICEligibleEmp[0].Row;
                                                //    drESICEligibleEmp.BeginEdit();
                                                //    UpdateTheDataRowInTableESICEligibleEmp("EDIT", sESICEligibleEmpID.Trim(), sEmpSysID, sESICMstID, sESICDtlID, System.DateTime.Now, dtEndDate, bIsActive, bMaturity, para.sUser, ref drESICEligibleEmp);
                                                //    drESICEligibleEmp.EndEdit();
                                                //}

                                                #endregion Data Save IN Table [ESICEligibleEmployee]


                                            }
                                        }
                                    }

                                    ////SaveDataSets(dsESICEligibleEmp, dsESICMntEmpWiseCal);
                                }
                                ////if (SelectedEmpCnt == grdRowMaxCnt)
                                ////{
                                TotProcComp += grdRowMaxCnt;
                                TotSelectEmpForProc -= grdRowMaxCnt;

                                if ((dsUnTagEmp.Tables[0].Rows.Count - TotProcComp) < 30)
                                {
                                    SelectedEmpCnt += (dsUnTagEmp.Tables[0].Rows.Count - TotProcComp);

                                    if (SelectedEmpCnt <= 0)
                                    { SelectedEmpCnt = dsUnTagEmp.Tables[0].Rows.Count + 1; }
                                }
                                else
                                {
                                    SelectedEmpCnt += 30;
                                }
                                //dsESICEligibleEmp = null;
                                //dsESICMntEmpWiseCal = null;
                            }
                            //if (bMaturity == true)
                            //{
                            //SaveDataSets(dsESICEligibleEmp, dsESICMntEmpWiseCal);
                            //}
                        }


                        #endregion Tag Employee List

                    }
                }
                else
                {
                    para.IsESICPolicyDefined = false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                #region Clear DataSet 

                //dsESICEligibleEmp = null;
                //dtESICEligibleEmp = null;
                //drESICEligibleEmp = null;
                //dvESICEligibleEmp = null;

                //dsESICMntEmpWiseCal = null;
                //dtESICMntEmpWiseCal = null;
                //drESICMntEmpWiseCal = null;
                //dvESICMntEmpWiseCal = null;

                dsSalInfo = null;
                dsESICPolicyMst = null;
                dsESICPolicyDtl = null;
                dsESICMonthNo = null;
                dsUnTagEmp = null;

                #endregion Clear DataSet 
            }
        }///End Function
        public void xGeneratorESICEligibleEmployee(ESICParaListNew para)
        {
            #region Variable Dataset
            DataTable dtValue = null;
            DataSet dsESICEligibleEmp = null;
            DataTable dtESICEligibleEmp = null;
            DataRow drESICEligibleEmp = null;
            DataView dvESICEligibleEmp = null;

            DataSet dsESICMntEmpWiseCal = null;
            DataTable dtESICMntEmpWiseCal = null;
            DataRow drESICMntEmpWiseCal = null;
            DataView dvESICMntEmpWiseCal = null;

            DataSet dsSalInfo = null;
            DataSet dsSalHd = null;
            DataTable dtSalHd = null;
            DataView dvSlrHd = null;
            DataSet dsESICPolicyMst = null;
            DataSet dsESICPolicyDtl = null;
            DataSet dsESICMonthNo = null;
            DataSet dsUnTagEmp = null;
            DataSet dsCurRl = null;
            DataTable dtCurRl = null;
            DataView dvCurRl = null;
            //clsSalaryStructureAplos obSS = new global::clsSalaryStructureAplos();

            #endregion Variable Dataset
            #region Declare Variable

            string sESICEligibleEmpID = "";
            string sESICMntEmpCalID = "";
            string sESICMstID = "";
            string sESICDtlID = "";
            string sGroupID = para.GroupID;
            string sPlantID = para.PlantID;
            string sESICElgGentID = "";
            string sESICDedGentID = "";
            string sFormulaID = "";
            string sFormulaResult = "";
            string sEmpInfoSysIDColl = "";
            string sEmpSystemID = "";
            string sEmpSysID = "";
            string sEntCurID = "";
            string sEarnCurID = "";
            string sSlrHD = "";
            string sFormulaDesIDEmp = "";
            string sFormulaDesIDEmpr = "";
            string strTemp = "";
            string sRoundOption = "";
            string sCurrencyRuleSystemID = "";

            int TotSelectEmpForProc = 0;
            int TotProcComp = 0;
            int grdRowMaxCnt = 0;
            int SelectedEmpCnt = 0;
            int EmpCntForLoop = 0;
            int iDecimalNo = 0;

            string sESICContSalaryHeadIDEmp = "";
            string sESICContSalaryHeadIDEmpr = "";

            DateTime dtStartDate;
            DateTime dtEndDate;

            decimal decEntCur = 0;
            decimal decEarnCur = 0;
            decimal decEarningValueRangeFrom = 0;
            decimal decEarningValueRangeTo = 0;
            decimal decFixedValueEmp = 0;
            decimal decFixedValueEmpr = 0;
            decimal decEmpCtbtnAmount = 0;
            decimal decEmprCtbtnAmount = 0;

            bool bMaturity = false;
            bool bIsActive = true;
            bool bIsFixedEmp = false;
            bool bIsFormulaEmp = false;
            bool bIsContributionSlrHDdependOnEarningEmp = false;

            bool bIsFixedEmpr = false;
            bool bIsFormulaEmpr = false;
            bool bIsContributionSlrHDdependOnEarningEmpr = false;
            bool bEarning = false;

            bool bIntegerInDisb = false;
            bool bIsDecimalInDisb = false;
            #endregion Declare Variable

            try
            {
                LoadCurrencyRule(para, out dsCurRl);
                dtCurRl = dsCurRl.Tables[0];
                dvCurRl = new DataView();

                GetESICPolicyMaster("", sGroupID.Trim(), sPlantID.Trim(), out dsESICPolicyMst);

                if (dsESICPolicyMst.Tables[0].Rows.Count > 0)
                {
                    for (int ESICPlCnt = 0; ESICPlCnt < dsESICPolicyMst.Tables[0].Rows.Count; ESICPlCnt++)
                    {
                        sESICMstID = dsESICPolicyMst.Tables[0].Rows[ESICPlCnt]["ID"].ToString().Trim();

                        #region DataSet

                        GetESICPolicyDetails(sESICMstID, out dsESICPolicyDtl);
                        GetESICPolicyMonthNo(sESICMstID, out dsESICMonthNo);

                        GetSalaryHead(out dsSalHd);
                        dtSalHd = dsSalHd.Tables[0];

                        #endregion DataSet

                        #region Tag Employee List

                        GetTagEmployeeListWithESICPolicyMaster(para, sESICMstID.Trim(), out dsUnTagEmp);
                        if (dsUnTagEmp.Tables[0].Rows.Count > 0)
                        {
                            sEmpInfoSysIDColl = "";
                            sEmpSystemID = "";
                            TotSelectEmpForProc = dsUnTagEmp.Tables[0].Rows.Count;
                            TotProcComp = 0;
                            grdRowMaxCnt = 0;
                            SelectedEmpCnt = 0;
                            EmpCntForLoop = 0;

                            while (SelectedEmpCnt < dsUnTagEmp.Tables[0].Rows.Count)
                            {
                                sEmpInfoSysIDColl = "";
                                sEmpSystemID = "";
                                EmpCntForLoop = 0;

                                if ((SelectedEmpCnt + 1) <= dsUnTagEmp.Tables[0].Rows.Count)
                                {
                                    grdRowMaxCnt = dsUnTagEmp.Tables[0].Rows.Count - TotProcComp;
                                }
                                else
                                {
                                    grdRowMaxCnt = 30;
                                }

                                #region Employee System ID Collection

                                for (int iUnTgEmCnt = SelectedEmpCnt; iUnTgEmCnt < grdRowMaxCnt + SelectedEmpCnt; iUnTgEmCnt++)
                                {
                                    if (string.IsNullOrEmpty(sEmpInfoSysIDColl) == true)
                                    {
                                        sEmpInfoSysIDColl = "EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                        sEmpSystemID = "EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                    }
                                    else
                                    {
                                        sEmpInfoSysIDColl += " OR EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                        sEmpSystemID += " OR EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                    }
                                    EmpCntForLoop++;
                                }

                                #endregion Employee System ID Collection

                                if (EmpCntForLoop == grdRowMaxCnt)
                                {
                                    GetESICEligibleEmployee(sEmpSystemID, out dsESICEligibleEmp);
                                    dtESICEligibleEmp = dsESICEligibleEmp.Tables[0];
                                    dvESICEligibleEmp = new DataView();

                                    GetESICMonthlyEmpWiseCalculation(sEmpSystemID, out dsESICMntEmpWiseCal);
                                    dtESICMntEmpWiseCal = dsESICMntEmpWiseCal.Tables[0];
                                    dvESICMntEmpWiseCal = new DataView();

                                    //Get General Salary Amount Head Wise
                                    List<dicSalInfoNew> dicSalInfo = new List<dicSalInfoNew>();
                                    //if (para.dsSalInfo == null)
                                    //{
                                    LoadEmpSlrDefForSlrProcess(para, sEmpInfoSysIDColl, out dsSalInfo);
                                    if (dsSalInfo.Tables[0].Rows.Count > 0)
                                        dicSalInfo = dsSalInfo.Tables[0].ToList<dicSalInfoNew>();
                                    //}
                                    //else
                                    //{
                                    //    if (para.dsSalInfo.Tables[0].Rows.Count > 0)
                                    //        dicSalInfo = para.dsSalInfo.Tables[0].ToList<dicSalInfo>();
                                    //}

                                    sESICElgGentID = "";
                                    sESICDedGentID = "";
                                    sESICElgGentID = "ECE" + sESICElgGentID;

                                    GenRefSrNoID(DateTime.Now.ToShortDateString().ToString(), "ESIC_CALCULATION", dsUnTagEmp.Tables[0].Rows.Count, out sESICDedGentID);
                                    sESICDedGentID = "ECC" + sESICDedGentID;
                                    for (int iUnTgEmCnt = 0; iUnTgEmCnt < dsUnTagEmp.Tables[0].Rows.Count; iUnTgEmCnt++)
                                    {
                                        sESICEligibleEmpID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["ESICEligibleEmpID"].ToString().Trim();
                                        sESICMntEmpCalID = sESICDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();
                                        sEmpSysID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim();

                                        #region Salary Amount Insert Into Virtual Table

                                        dtValue = new DataTable();
                                        dtValue.TableName = "TempTable";
                                        dtValue.Columns.Add("EmpSystemID");
                                        dtValue.Columns.Add("SalaryHeadID");
                                        dtValue.Columns.Add("EntryCurrencyID");
                                        dtValue.Columns.Add("EntryAmount");
                                        dtValue.Columns.Add("EarningCurrencyID");
                                        dtValue.Columns.Add("EarningAmount");
                                        dtValue.Columns.Add("DecimalNo");
                                        dtValue.Columns.Add("IntegerInDisb");
                                        dtValue.Columns.Add("IsDecimalInDisb");
                                        dtValue.Columns.Add("RoundOption");

                                        //if (para.dsSalInfo == null)
                                        //{
                                        var dicSalInfo_Sub = dicSalInfo.FindAll(x => x.EmpInfoSystemID == sEmpSysID);
                                        if (dicSalInfo_Sub.Count > 0)
                                        {
                                            sCurrencyRuleSystemID = dicSalInfo_Sub[0].CurrencyRuleSystemID;
                                            if (para.dsSalInfo == null)
                                            {
                                                for (int i = 0; i < dicSalInfo_Sub.Count; i++)
                                                {
                                                    sSlrHD = dicSalInfo_Sub[i].SalaryHeadID;
                                                    sEntCurID = dicSalInfo_Sub[i].EntryCurrencyID;
                                                    decEntCur = dicSalInfo_Sub[i].EntryAmount;
                                                    sEarnCurID = dicSalInfo_Sub[i].EarningCurrencyID;
                                                    decEarnCur = dicSalInfo_Sub[i].EarningAmount;

                                                    iDecimalNo = dicSalInfo_Sub[i].DecimalNo;
                                                    bIntegerInDisb = dicSalInfo_Sub[i].IntegerInDisb;
                                                    bIsDecimalInDisb = dicSalInfo_Sub[i].IsDecimalInDisb;
                                                    sRoundOption = dicSalInfo_Sub[i].RoundOption;

                                                    #region For SalaryHead Wise Amount In Virtual 2nd Table

                                                    DataRow dtValueRow = dtValue.NewRow();
                                                    dtValueRow["EmpSystemID"] = dicSalInfo_Sub[i].EmpInfoSystemID;
                                                    dtValueRow["SalaryHeadID"] = sSlrHD;
                                                    dtValueRow["EntryCurrencyID"] = sEntCurID;
                                                    dtValueRow["EntryAmount"] = decEntCur;
                                                    dtValueRow["EarningCurrencyID"] = sEarnCurID;
                                                    dtValueRow["EarningAmount"] = decEarnCur;
                                                    dtValueRow["DecimalNo"] = iDecimalNo;
                                                    dtValueRow["IntegerInDisb"] = bIntegerInDisb;
                                                    dtValueRow["IsDecimalInDisb"] = bIsDecimalInDisb;
                                                    dtValueRow["RoundOption"] = sRoundOption;

                                                    dtValue.Rows.Add(dtValueRow);

                                                    #endregion For SalaryHead Wise Amount In Virtual 2nd Table

                                                    if (dicSalInfo_Sub[i].HeadCategory == "ESIC Employee Contribution")
                                                    {
                                                        sESICContSalaryHeadIDEmp = dicSalInfo_Sub[i].SalaryHeadID;
                                                    }
                                                    if (dicSalInfo_Sub[i].HeadCategory == "ESIC Employer Contribution")
                                                    {
                                                        sESICContSalaryHeadIDEmpr = dicSalInfo_Sub[i].SalaryHeadID;
                                                    }
                                                }
                                            }
                                        }
                                        //}
                                        //else
                                        if (para.dsSalInfo != null)
                                        {
                                            dtValue = para.dsSalInfo.Tables[0];
                                            strTemp = "ESIC Employee Contribution";

                                            dvSlrHd = new DataView();
                                            dvSlrHd.Table = dtSalHd;
                                            dvSlrHd.RowFilter = "HeadCategory = '" + strTemp.Trim() + "'";
                                            if (dvSlrHd.Count > 0)
                                            { sESICContSalaryHeadIDEmp = dvSlrHd[0].Row["SalaryHeadID"].ToString().Trim(); }

                                            strTemp = "ESIC Employer Contribution";

                                            dvSlrHd.Table = dtSalHd;
                                            dvSlrHd.RowFilter = "HeadCategory = '" + strTemp.Trim() + "'";
                                            if (dvSlrHd.Count > 0)
                                            { sESICContSalaryHeadIDEmpr = dvSlrHd[0].Row["SalaryHeadID"].ToString().Trim(); }
                                        }

                                        #endregion Salary Amount Insert Into Virtual Table
                                        if (dtValue.Rows.Count > 0)
                                        {
                                            for (int iESICDtl = 0; iESICDtl < dsESICPolicyDtl.Tables[0].Rows.Count; iESICDtl++)
                                            {
                                                #region Clear

                                                sFormulaDesIDEmp = "";
                                                sFormulaDesIDEmpr = "";

                                                decFixedValueEmp = 0;
                                                decFixedValueEmpr = 0;
                                                decEmpCtbtnAmount = 0;
                                                decEmprCtbtnAmount = 0;
                                                decEarningValueRangeFrom = 0;
                                                decEarningValueRangeTo = 0;

                                                dtEndDate = System.DateTime.Now;
                                                bMaturity = false;
                                                bEarning = false;
                                                bIsActive = true;
                                                bIsFixedEmpr = false;
                                                bIsFormulaEmpr = false;
                                                bIsFixedEmp = false;
                                                bIsFormulaEmp = false;
                                                bIsContributionSlrHDdependOnEarningEmp = false;
                                                bIsContributionSlrHDdependOnEarningEmpr = false;

                                                #endregion Clear

                                                #region Select ESICPolicyDetails ID if have multiple column

                                                sFormulaID = dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["FormulaDesIDEarning"].ToString().Trim();
                                                ReLoadFormulaWithValue(sEmpSysID, para, sFormulaID, bEarning, ref dtValue, ref dtSalHd);
                                                sFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();
                                                decEarningValueRangeFrom = Convert.ToDecimal(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["EarningValueRangeFrom"].ToString().Trim());
                                                decEarningValueRangeTo = Convert.ToDecimal(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["EarningValueRangeTo"].ToString().Trim());

                                                if (Convert.ToDecimal(sFormulaResult) > decEarningValueRangeFrom && Convert.ToDecimal(sFormulaResult) < decEarningValueRangeTo)
                                                {
                                                    bMaturity = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsMandatory"].ToString().Trim());
                                                }
                                                else
                                                {
                                                    bMaturity = false;

                                                    //if (para.bStructure == true)
                                                    //{
                                                    //    bIsActive = false;
                                                    //}
                                                    //else
                                                    //{
                                                    //string sCurrentMonthName = System.DateTime.Now.ToString("MMMMMMMMMMMMM").Substring(0, 3);
                                                    string sCurrentMonthName = para.ToDate/*System.DateTime.Now.ToString("dd-MMM-yyyy")*/;
                                                    string sMatDt = "";

                                                    if (dsESICMonthNo.Tables[0].Rows.Count > 0)
                                                    {
                                                        for (int iMnt = 0; iMnt < dsESICMonthNo.Tables[0].Rows.Count; iMnt++)
                                                        {
                                                            sMatDt = "01-" + dsESICMonthNo.Tables[0].Rows[iMnt]["MonthName"].ToString().Substring(0, 3) + "-" + System.DateTime.Now.Year.ToString();
                                                            if (Convert.ToDateTime(sCurrentMonthName).Month == Convert.ToDateTime(sMatDt).Month)
                                                            {
                                                                bIsActive = false;
                                                                //dtEndDate = Convert.ToDateTime(sMatDt);
                                                                dtEndDate = Convert.ToDateTime(para.ToDate);
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        bIsActive = false;
                                                    }
                                                    //}
                                                }
                                                sESICDtlID = dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["ID"].ToString().Trim();

                                                sFormulaDesIDEmp = dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["FormulaDesIDEmp"].ToString().Trim();
                                                decFixedValueEmp = Convert.ToDecimal(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["FixedValueEmp"].ToString().Trim());
                                                bIsFixedEmp = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsFixedEmp"].ToString().Trim());
                                                bIsFormulaEmp = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsFormulaEmp"].ToString().Trim());
                                                bIsContributionSlrHDdependOnEarningEmp = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsContributionSlrHDdependOnEarningEmp"].ToString().Trim());

                                                sFormulaDesIDEmpr = dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["FormulaDesIDEmployer"].ToString().Trim();
                                                decFixedValueEmpr = Convert.ToDecimal(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["FixedValueEmployer"].ToString().Trim());
                                                bIsFixedEmpr = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsFixedEmployer"].ToString().Trim());
                                                bIsFormulaEmpr = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsFormulaEmployer"].ToString().Trim());
                                                bIsContributionSlrHDdependOnEarningEmpr = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsContributionSlrHDdependOnEarningEmployer"].ToString().Trim());

                                                #endregion Select ESICPolicyDetails ID if have multiple column

                                                #region Employee Contribution Amount

                                                if (bIsFixedEmp == true)
                                                {
                                                    decEmpCtbtnAmount = decFixedValueEmp;
                                                }
                                                else if (bIsFormulaEmp == true)
                                                {
                                                    bEarning = bIsContributionSlrHDdependOnEarningEmp;
                                                    ReLoadFormulaWithValue(sEmpSysID, para, sFormulaDesIDEmp, bEarning, ref dtValue, ref dtSalHd);
                                                    sFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();

                                                    decEmpCtbtnAmount = Convert.ToDecimal(sFormulaResult);
                                                }

                                                #endregion Employee Contribution Amount

                                                #region Employer Contribution Amount

                                                if (bIsFixedEmpr == true)
                                                {
                                                    decEmprCtbtnAmount = decFixedValueEmpr;
                                                }
                                                else if (bIsFormulaEmpr == true)
                                                {
                                                    bEarning = bIsContributionSlrHDdependOnEarningEmpr;
                                                    ReLoadFormulaWithValue(sEmpSysID, para, sFormulaDesIDEmpr, bEarning, ref dtValue, ref dtSalHd);
                                                    sFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();

                                                    decEmprCtbtnAmount = Convert.ToDecimal(sFormulaResult);
                                                }

                                                //*******kabir*********//


                                                para.IsESICMandatoryNew = bMaturity;
                                                if (bMaturity == false)
                                                {
                                                    para.IsESICOptionalNew = true;
                                                }






                                                #endregion Employer Contribution Amount

                                                #region Data Save IN Table [ESICMonthlyEmpWiseCalculation]

                                                if (bIsActive == true)
                                                {
                                                    dvESICMntEmpWiseCal.Table = dtESICMntEmpWiseCal;
                                                    dvESICMntEmpWiseCal.RowFilter = "ESICEligibleEmpID = '" + sESICEligibleEmpID + "' AND MonthNo = '" + Convert.ToDateTime(para.ToDate).Month + "' AND YearNo = '" + Convert.ToDateTime(para.ToDate).Year + "'";
                                                    if (dvESICMntEmpWiseCal.Count == 0)
                                                    {//Add new block
                                                        drESICMntEmpWiseCal = dtESICMntEmpWiseCal.NewRow();
                                                        UpdateTheDataRowInTableESICMonthlyEmpWiseCalculation("ADDNEW", sESICMntEmpCalID, sESICEligibleEmpID, para.ToDate, decEmpCtbtnAmount, decEmprCtbtnAmount, para.sUser, ref drESICMntEmpWiseCal);
                                                        dtESICMntEmpWiseCal.Rows.Add(drESICMntEmpWiseCal);
                                                    }
                                                    else
                                                    {//edit block
                                                        drESICMntEmpWiseCal = dvESICMntEmpWiseCal[0].Row;
                                                        drESICMntEmpWiseCal.BeginEdit();
                                                        UpdateTheDataRowInTableESICMonthlyEmpWiseCalculation("EDIT", sESICMntEmpCalID, sESICEligibleEmpID, para.ToDate, decEmpCtbtnAmount, decEmprCtbtnAmount, para.sUser, ref drESICMntEmpWiseCal);
                                                        drESICMntEmpWiseCal.EndEdit();
                                                    }
                                                }

                                                #endregion Data Save IN Table [ESICMonthlyEmpWiseCalculation]

                                                #region Data Save IN Table [ESICEligibleEmployee]

                                                dvESICEligibleEmp.Table = dtESICEligibleEmp;
                                                dvESICEligibleEmp.RowFilter = "ID = '" + sESICEligibleEmpID.Trim() + "'";
                                                if (dvESICEligibleEmp.Count == 1)
                                                {//Edit block
                                                    drESICEligibleEmp = dvESICEligibleEmp[0].Row;
                                                    drESICEligibleEmp.BeginEdit();
                                                    UpdateTheDataRowInTableESICEligibleEmp("EDIT", sESICEligibleEmpID.Trim(), sEmpSysID, sESICMstID, sESICDtlID, System.DateTime.Now, dtEndDate, bIsActive, bMaturity, para.sUser, ref drESICEligibleEmp);
                                                    drESICEligibleEmp.EndEdit();
                                                }

                                                #endregion Data Save IN Table [ESICEligibleEmployee]

                                                //if(bIsActive == false & para.bStructure == true)
                                                //{
                                                //    dvESICMntEmpWiseCal.Table = dtESICMntEmpWiseCal;
                                                //    dvESICMntEmpWiseCal.RowFilter = "ESICEligibleEmpID = '" + sESICEligibleEmpID + "'";
                                                //    if (dvESICMntEmpWiseCal.Count > 0)
                                                //    {
                                                //        while (dvESICMntEmpWiseCal.Count > 0)
                                                //        {
                                                //            drESICMntEmpWiseCal = dvESICMntEmpWiseCal[0].Row;
                                                //            drESICMntEmpWiseCal.Delete();
                                                //        }
                                                //    }

                                                //    dvESICEligibleEmp.Table = dtESICEligibleEmp;
                                                //    dvESICEligibleEmp.RowFilter = "ID = '" + sESICEligibleEmpID.Trim() + "'";
                                                //    if (dvESICEligibleEmp.Count > 1)
                                                //    {
                                                //        while (dvESICEligibleEmp.Count > 0)
                                                //        {
                                                //            drESICEligibleEmp = dvESICEligibleEmp[0].Row;
                                                //            drESICEligibleEmp.Delete();
                                                //        }
                                                //    }
                                                //}
                                            }
                                        }
                                    }

                                    ////SaveDataSets(dsESICEligibleEmp, dsESICMntEmpWiseCal);
                                }
                                ////if (SelectedEmpCnt == grdRowMaxCnt)
                                ////{
                                TotProcComp += grdRowMaxCnt;
                                TotSelectEmpForProc -= grdRowMaxCnt;
                                //if (bIsActive == false & para.bStructure == true)
                                //{
                                //    SaveDataSets(dsESICMntEmpWiseCal, dsESICEligibleEmp);
                                //}
                                //else
                                //{
                                //SaveDataSets(dsESICEligibleEmp, dsESICMntEmpWiseCal);  //KABIRE
                                //}
                                ////}
                                if ((dsUnTagEmp.Tables[0].Rows.Count - TotProcComp) < 30)
                                {
                                    SelectedEmpCnt += (dsUnTagEmp.Tables[0].Rows.Count - TotProcComp);

                                    if (SelectedEmpCnt <= 0)
                                    { SelectedEmpCnt = dsUnTagEmp.Tables[0].Rows.Count + 1; }
                                }
                                else
                                {
                                    SelectedEmpCnt += 30;
                                }
                                dsESICEligibleEmp = null;
                                dsESICMntEmpWiseCal = null;
                            }
                            //if (bMaturity == true)
                            //{
                            //SaveDataSets(dsESICEligibleEmp, dsESICMntEmpWiseCal);
                            //}
                        }

                        #endregion Tag Employee List

                        if (para.ShouldNotProcessUntaggedEmp == false)
                        {
                            #region Untag Employee List

                            GetUnTagEmployeeListWithESICPolicyMaster(para, out dsUnTagEmp);
                            if (dsUnTagEmp.Tables[0].Rows.Count > 0)
                            {
                                sEmpInfoSysIDColl = "";
                                sEmpSystemID = "";
                                TotSelectEmpForProc = dsUnTagEmp.Tables[0].Rows.Count;
                                TotProcComp = 0;
                                grdRowMaxCnt = 0;
                                SelectedEmpCnt = 0;
                                EmpCntForLoop = 0;

                                while (SelectedEmpCnt < dsUnTagEmp.Tables[0].Rows.Count)
                                {
                                    sEmpInfoSysIDColl = "";
                                    sEmpSystemID = "";
                                    EmpCntForLoop = 0;

                                    if ((SelectedEmpCnt + 1) <= dsUnTagEmp.Tables[0].Rows.Count)
                                    {
                                        grdRowMaxCnt = dsUnTagEmp.Tables[0].Rows.Count - TotProcComp;
                                    }
                                    else
                                    {
                                        grdRowMaxCnt = 30;
                                    }

                                    #region Employee System ID Collection

                                    for (int iUnTgEmCnt = SelectedEmpCnt; iUnTgEmCnt < grdRowMaxCnt + SelectedEmpCnt; iUnTgEmCnt++)
                                    {
                                        if (string.IsNullOrEmpty(sEmpInfoSysIDColl) == true)
                                        {
                                            sEmpInfoSysIDColl = "EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                            sEmpSystemID = "EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                        }
                                        else
                                        {
                                            sEmpInfoSysIDColl += " OR EmpInfoSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                            sEmpSystemID += " OR EmpSystemID = '" + dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim() + "'";
                                        }
                                        EmpCntForLoop++;
                                    }

                                    #endregion Employee System ID Collection

                                    if (EmpCntForLoop == grdRowMaxCnt)
                                    {
                                        GetESICEligibleEmployee(sEmpSystemID, out dsESICEligibleEmp);
                                        dtESICEligibleEmp = dsESICEligibleEmp.Tables[0];
                                        dvESICEligibleEmp = new DataView();

                                        GetESICMonthlyEmpWiseCalculation(sEmpSystemID, out dsESICMntEmpWiseCal);
                                        dtESICMntEmpWiseCal = dsESICMntEmpWiseCal.Tables[0];
                                        dvESICMntEmpWiseCal = new DataView();

                                        //Get General Salary Amount Head Wise
                                        List<dicSalInfoNew> dicSalInfo = new List<dicSalInfoNew>();
                                        ////LoadEmpSlrDefForSlrProcess(para, sEmpSysIDColl, out dsSalInfo);
                                        ////if (dsSalInfo.Tables[0].Rows.Count > 0)
                                        ////    dicSalInfo = dsSalInfo.Tables[0].ToList<dicSalInfo>();
                                        //if (para.dsSalInfo == null)
                                        //{
                                        LoadEmpSlrDefForSlrProcess(para, sEmpInfoSysIDColl, out dsSalInfo);
                                        if (dsSalInfo.Tables[0].Rows.Count > 0)
                                            dicSalInfo = dsSalInfo.Tables[0].ToList<dicSalInfoNew>();
                                        //}
                                        ////else
                                        ////{
                                        ////    if (para.dsSalInfo.Tables[0].Rows.Count > 0)
                                        ////        dicSalInfo = para.dsSalInfo.Tables[0].ToList<dicSalInfo>();
                                        ////}

                                        sESICElgGentID = "";
                                        sESICDedGentID = "";
                                        GenRefSrNoID(DateTime.Now.ToShortDateString().ToString(), "ESIC_ELIGIBLE", dsUnTagEmp.Tables[0].Rows.Count, out sESICElgGentID);
                                        sESICElgGentID = "ECE" + sESICElgGentID;

                                        GenRefSrNoID(DateTime.Now.ToShortDateString().ToString(), "ESIC_CALCULATION", dsUnTagEmp.Tables[0].Rows.Count, out sESICDedGentID);
                                        sESICDedGentID = "ECC" + sESICDedGentID;
                                        for (int iUnTgEmCnt = SelectedEmpCnt; iUnTgEmCnt < grdRowMaxCnt + SelectedEmpCnt; iUnTgEmCnt++)
                                        {
                                            sESICEligibleEmpID = sESICElgGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();
                                            sESICMntEmpCalID = sESICDedGentID.ToString() + "-" + (iUnTgEmCnt + 1).ToString();
                                            sEmpSysID = dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["SystemID"].ToString().Trim();

                                            //if (sEmpSysID == "2018-10008" || sEmpSysID == "2018-10009" || sEmpSysID == "2018-1001" || sEmpSysID == "2018-10010" || sEmpSysID == "2018-10011" || sEmpSysID == "2018-10012" || sEmpSysID == "2018-10013" || sEmpSysID == "2018-10015" || sEmpSysID == "2018-10019")
                                            //{
                                            //    string a = "";
                                            //}

                                            #region Master Table Data Capture [Start Date]

                                            dtStartDate = Convert.ToDateTime(dsUnTagEmp.Tables[0].Rows[iUnTgEmCnt]["DOJ"].ToString().Trim());

                                            #endregion Master Table Data Capture 

                                            #region Salary Amount Insert Into Virtual Table

                                            dtValue = new DataTable();
                                            dtValue.TableName = "TempTable";
                                            dtValue.Columns.Add("EmpSystemID");
                                            dtValue.Columns.Add("SalaryHeadID");
                                            dtValue.Columns.Add("EntryCurrencyID");
                                            dtValue.Columns.Add("EntryAmount");
                                            dtValue.Columns.Add("EarningCurrencyID");
                                            dtValue.Columns.Add("EarningAmount");
                                            dtValue.Columns.Add("DecimalNo");
                                            dtValue.Columns.Add("IntegerInDisb");
                                            dtValue.Columns.Add("IsDecimalInDisb");
                                            dtValue.Columns.Add("RoundOption");

                                            //if (para.dsSalInfo == null)
                                            //{
                                            var dicSalInfo_Sub = dicSalInfo.FindAll(x => x.EmpInfoSystemID == sEmpSysID);
                                            if (dicSalInfo_Sub.Count > 0)
                                            {
                                                sCurrencyRuleSystemID = dicSalInfo_Sub[0].CurrencyRuleSystemID;
                                                if (para.dsSalInfo == null)
                                                {
                                                    for (int i = 0; i < dicSalInfo_Sub.Count; i++)
                                                    {
                                                        sSlrHD = dicSalInfo_Sub[i].SalaryHeadID;
                                                        sEntCurID = dicSalInfo_Sub[i].EntryCurrencyID;
                                                        decEntCur = dicSalInfo_Sub[i].EntryAmount;
                                                        sEarnCurID = dicSalInfo_Sub[i].EarningCurrencyID;
                                                        decEarnCur = dicSalInfo_Sub[i].EarningAmount;

                                                        iDecimalNo = dicSalInfo_Sub[i].DecimalNo;
                                                        bIntegerInDisb = dicSalInfo_Sub[i].IntegerInDisb;
                                                        bIsDecimalInDisb = dicSalInfo_Sub[i].IsDecimalInDisb;
                                                        sRoundOption = dicSalInfo_Sub[i].RoundOption;

                                                        #region For SalaryHead Wise Amount In Virtual 2nd Table

                                                        DataRow dtValueRow = dtValue.NewRow();
                                                        dtValueRow["EmpSystemID"] = dicSalInfo_Sub[i].EmpInfoSystemID;
                                                        dtValueRow["SalaryHeadID"] = sSlrHD;
                                                        dtValueRow["EntryCurrencyID"] = sEntCurID;
                                                        dtValueRow["EntryAmount"] = decEntCur;
                                                        dtValueRow["EarningCurrencyID"] = sEarnCurID;
                                                        dtValueRow["EarningAmount"] = decEarnCur;
                                                        dtValueRow["DecimalNo"] = iDecimalNo;
                                                        dtValueRow["IntegerInDisb"] = bIntegerInDisb;
                                                        dtValueRow["IsDecimalInDisb"] = bIsDecimalInDisb;
                                                        dtValueRow["RoundOption"] = sRoundOption;

                                                        dtValue.Rows.Add(dtValueRow);

                                                        #endregion For SalaryHead Wise Amount In Virtual 2nd Table

                                                        if (dicSalInfo_Sub[i].HeadCategory == "ESIC Employee Contribution")
                                                        {
                                                            sESICContSalaryHeadIDEmp = dicSalInfo_Sub[i].SalaryHeadID;
                                                        }
                                                        if (dicSalInfo_Sub[i].HeadCategory == "ESIC Employer Contribution")
                                                        {
                                                            sESICContSalaryHeadIDEmpr = dicSalInfo_Sub[i].SalaryHeadID;
                                                        }
                                                    }
                                                }
                                            }
                                            //}
                                            //else
                                            if (para.dsSalInfo != null)
                                            {
                                                dtValue = para.dsSalInfo.Tables[0];
                                                strTemp = "ESIC Employee Contribution";

                                                dvSlrHd = new DataView();
                                                dvSlrHd.Table = dtSalHd;
                                                dvSlrHd.RowFilter = "HeadCategory = '" + strTemp.Trim() + "'";
                                                if (dvSlrHd.Count > 0)
                                                { sESICContSalaryHeadIDEmp = dvSlrHd[0].Row["SalaryHeadID"].ToString().Trim(); }

                                                strTemp = "ESIC Employer Contribution";

                                                dvSlrHd.Table = dtSalHd;
                                                dvSlrHd.RowFilter = "HeadCategory = '" + strTemp.Trim() + "'";
                                                if (dvSlrHd.Count > 0)
                                                { sESICContSalaryHeadIDEmpr = dvSlrHd[0].Row["SalaryHeadID"].ToString().Trim(); }
                                            }

                                            #endregion Salary Amount Insert Into Virtual Table
                                            if (dtValue.Rows.Count > 0)
                                            {
                                                for (int iESICDtl = 0; iESICDtl < dsESICPolicyDtl.Tables[0].Rows.Count; iESICDtl++)
                                                {
                                                    #region Clear

                                                    sFormulaDesIDEmp = "";
                                                    sFormulaDesIDEmpr = "";

                                                    decFixedValueEmp = 0;
                                                    decFixedValueEmpr = 0;
                                                    decEmpCtbtnAmount = 0;
                                                    decEmprCtbtnAmount = 0;
                                                    decEarningValueRangeFrom = 0;
                                                    decEarningValueRangeTo = 0;

                                                    dtEndDate = System.DateTime.Now;
                                                    bMaturity = false;
                                                    bEarning = false;
                                                    bIsActive = true;
                                                    bIsFixedEmpr = false;
                                                    bIsFormulaEmpr = false;
                                                    bIsFixedEmp = false;
                                                    bIsFormulaEmp = false;
                                                    bIsContributionSlrHDdependOnEarningEmp = false;
                                                    bIsContributionSlrHDdependOnEarningEmpr = false;

                                                    #endregion Clear

                                                    #region Select ESICPolicyDetails ID if have multiple column

                                                    sFormulaID = dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["FormulaDesIDEarning"].ToString().Trim();
                                                    ReLoadFormulaWithValue(sEmpSysID, para, sFormulaID, bEarning, ref dtValue, ref dtSalHd);
                                                    sFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();
                                                    decEarningValueRangeFrom = Convert.ToDecimal(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["EarningValueRangeFrom"].ToString().Trim());
                                                    decEarningValueRangeTo = Convert.ToDecimal(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["EarningValueRangeTo"].ToString().Trim());

                                                    if (Convert.ToDecimal(sFormulaResult) > decEarningValueRangeFrom && Convert.ToDecimal(sFormulaResult) < decEarningValueRangeTo)
                                                    {
                                                        bMaturity = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsMandatory"].ToString().Trim());
                                                    }
                                                    else
                                                    {
                                                        bMaturity = false;
                                                        //string sCurrentMonthName = System.DateTime.Now.ToString("MMMMMMMMMMMMM").Substring(0, 3);
                                                        string sCurrentMonthName = System.DateTime.Now.ToString("dd-MMM-yyyy");
                                                        string sMatDt = "";

                                                        if (dsESICMonthNo.Tables[0].Rows.Count > 0)
                                                        {
                                                            for (int iMnt = 0; iMnt < dsESICMonthNo.Tables[0].Rows.Count; iMnt++)
                                                            {
                                                                sMatDt = "01-" + dsESICMonthNo.Tables[0].Rows[iMnt]["MonthName"].ToString().Substring(0, 3) + "-" + System.DateTime.Now.Year.ToString();
                                                                if (Convert.ToDateTime(sCurrentMonthName) == Convert.ToDateTime(sMatDt))
                                                                {
                                                                    bIsActive = false;
                                                                    dtEndDate = Convert.ToDateTime(sMatDt);
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            bIsActive = false;
                                                        }
                                                    }
                                                    sESICDtlID = dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["ID"].ToString().Trim();

                                                    sFormulaDesIDEmp = dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["FormulaDesIDEmp"].ToString().Trim();
                                                    decFixedValueEmp = Convert.ToDecimal(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["FixedValueEmp"].ToString().Trim());
                                                    bIsFixedEmp = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsFixedEmp"].ToString().Trim());
                                                    bIsFormulaEmp = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsFormulaEmp"].ToString().Trim());
                                                    bIsContributionSlrHDdependOnEarningEmp = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsContributionSlrHDdependOnEarningEmp"].ToString().Trim());

                                                    sFormulaDesIDEmpr = dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["FormulaDesIDEmployer"].ToString().Trim();
                                                    decFixedValueEmpr = Convert.ToDecimal(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["FixedValueEmployer"].ToString().Trim());
                                                    bIsFixedEmpr = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsFixedEmployer"].ToString().Trim());
                                                    bIsFormulaEmpr = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsFormulaEmployer"].ToString().Trim());
                                                    bIsContributionSlrHDdependOnEarningEmpr = Convert.ToBoolean(dsESICPolicyDtl.Tables[0].Rows[iESICDtl]["IsContributionSlrHDdependOnEarningEmployer"].ToString().Trim());

                                                    #endregion Select ESICPolicyDetails ID if have multiple column
                                                    if (bMaturity == true)
                                                    {
                                                        #region Employee Contribution Amount

                                                        if (bIsFixedEmp == true)
                                                        {
                                                            decEmpCtbtnAmount = decFixedValueEmp;
                                                        }
                                                        else if (bIsFormulaEmp == true)
                                                        {
                                                            bEarning = bIsContributionSlrHDdependOnEarningEmp;
                                                            ReLoadFormulaWithValue(sEmpSysID, para, sFormulaDesIDEmp, bEarning, ref dtValue, ref dtSalHd);
                                                            sFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();

                                                            decEmpCtbtnAmount = Convert.ToDecimal(sFormulaResult);
                                                        }

                                                        #endregion Employee Contribution Amount

                                                        #region Employer Contribution Amount

                                                        if (bIsFixedEmpr == true)
                                                        {
                                                            decEmprCtbtnAmount = decFixedValueEmpr;
                                                        }
                                                        else if (bIsFormulaEmpr == true)
                                                        {
                                                            bEarning = bIsContributionSlrHDdependOnEarningEmpr;
                                                            ReLoadFormulaWithValue(sEmpSysID, para, sFormulaDesIDEmpr, bEarning, ref dtValue, ref dtSalHd);
                                                            sFormulaResult = Evaluate(sFormulaValue.Trim()).ToString();

                                                            decEmprCtbtnAmount = Convert.ToDecimal(sFormulaResult);
                                                        }

                                                        #endregion Employer Contribution Amount

                                                        #region Data Save IN Table [ESICMonthlyEmpWiseCalculation]

                                                        if (bIsActive == true)
                                                        {
                                                            dvESICMntEmpWiseCal.Table = dtESICMntEmpWiseCal;
                                                            dvESICMntEmpWiseCal.RowFilter = "ID = '" + sESICMntEmpCalID.Trim() + "'";
                                                            if (dvESICMntEmpWiseCal.Count == 0)
                                                            {//Add new block
                                                                drESICMntEmpWiseCal = dtESICMntEmpWiseCal.NewRow();
                                                                UpdateTheDataRowInTableESICMonthlyEmpWiseCalculation("ADDNEW", sESICMntEmpCalID, sESICEligibleEmpID, para.ToDate, decEmpCtbtnAmount, decEmprCtbtnAmount, para.sUser, ref drESICMntEmpWiseCal);
                                                                dtESICMntEmpWiseCal.Rows.Add(drESICMntEmpWiseCal);
                                                            }
                                                            else
                                                            {//edit block
                                                                drESICMntEmpWiseCal = dvESICMntEmpWiseCal[0].Row;
                                                                drESICMntEmpWiseCal.BeginEdit();
                                                                UpdateTheDataRowInTableESICMonthlyEmpWiseCalculation("EDIT", sESICMntEmpCalID, sESICEligibleEmpID, para.ToDate, decEmpCtbtnAmount, decEmprCtbtnAmount, para.sUser, ref drESICMntEmpWiseCal);
                                                                drESICMntEmpWiseCal.EndEdit();
                                                            }
                                                        }

                                                        #endregion Data Save IN Table [ESICMonthlyEmpWiseCalculation]

                                                        #region Data Save IN Table [ESICEligibleEmployee]

                                                        dvESICEligibleEmp.Table = dtESICEligibleEmp;
                                                        dvESICEligibleEmp.RowFilter = "ID = '" + sESICEligibleEmpID.Trim() + "'";
                                                        if (dvESICEligibleEmp.Count == 0)
                                                        {//Add new block
                                                            drESICEligibleEmp = dtESICEligibleEmp.NewRow();
                                                            UpdateTheDataRowInTableESICEligibleEmp("ADDNEW", sESICEligibleEmpID.Trim(), sEmpSysID, sESICMstID, sESICDtlID, dtStartDate, dtEndDate, bIsActive, bMaturity, para.sUser, ref drESICEligibleEmp);
                                                            dtESICEligibleEmp.Rows.Add(drESICEligibleEmp);
                                                        }

                                                        #endregion Data Save IN Table [ESICEligibleEmployee]

                                                    }
                                                }
                                            }
                                        }
                                    }
                                    //if (SelectedEmpCnt == grdRowMaxCnt)
                                    //{
                                    TotProcComp += grdRowMaxCnt;
                                    TotSelectEmpForProc -= grdRowMaxCnt;
                                    SaveDataSets(dsESICEligibleEmp, dsESICMntEmpWiseCal);
                                    //}
                                    if ((dsUnTagEmp.Tables[0].Rows.Count - TotProcComp) < 30)
                                    {
                                        SelectedEmpCnt += (dsUnTagEmp.Tables[0].Rows.Count - TotProcComp);

                                        if (SelectedEmpCnt <= 0)
                                        { SelectedEmpCnt = dsUnTagEmp.Tables[0].Rows.Count + 1; }
                                    }
                                    else
                                    {
                                        SelectedEmpCnt += 30;
                                    }
                                    dsESICEligibleEmp = null;
                                    dsESICMntEmpWiseCal = null;
                                }
                                //if (bMaturity == true)
                                //{
                                //SaveDataSets(dsESICEligibleEmp, dsESICMntEmpWiseCal);
                                //}
                            }

                            #endregion Untag Employee
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                #region Clear DataSet 

                dsESICEligibleEmp = null;
                dtESICEligibleEmp = null;
                drESICEligibleEmp = null;
                dvESICEligibleEmp = null;

                dsESICMntEmpWiseCal = null;
                dtESICMntEmpWiseCal = null;
                drESICMntEmpWiseCal = null;
                dvESICMntEmpWiseCal = null;

                dsSalInfo = null;
                dsESICPolicyMst = null;
                dsESICPolicyDtl = null;
                dsESICMonthNo = null;
                dsUnTagEmp = null;

                #endregion Clear DataSet 
            }
        }///End Function
        private void UpdateTheDataRowInTableESICEligibleEmp(string OPN_FLAG, string sESICEligibleEmpID, string sEmpSysID, string sESICMstID, string sESICDtlID, DateTime dtStartDate, DateTime dtEndDate, bool bIsActive, bool bIsMandatory, string sUser, ref DataRow drLocal)
        {
            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    drLocal["ID"] = RetValidLen(sESICEligibleEmpID);
                    drLocal["EmpSystemID"] = RetValidLen(sEmpSysID);
                    drLocal["ESICMstID"] = RetValidLen(sESICMstID);
                    drLocal["ESICDtlID"] = RetValidLen(sESICDtlID);
                    drLocal["StartDate"] = dtStartDate;
                    drLocal["IsMaturity"] = bIsMandatory;

                    drLocal["AddedBy"] = RetValidLen(sUser);
                    drLocal["AddedDate"] = DateTime.Now.ToString();
                    drLocal["AddedFromIP"] = "";
                }

                drLocal["IsActive"] = bIsActive;
                if (bIsActive == false)
                {
                    drLocal["EndDate"] = dtEndDate;
                }
                else
                {
                    drLocal["EndDate"] = DBNull.Value;
                }

                drLocal["UpdatedBy"] = RetValidLen(sUser);
                drLocal["UpdatedDate"] = DateTime.Now.ToString();
                drLocal["UpdatedFromIP"] = "";
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                //
            }
        }//End Function
        private void UpdateTheDataRowInTableESICMonthlyEmpWiseCalculation(string OPN_FLAG, string sESICMntDedID, string sESICEligibleEmpID, string sToDate, decimal decEmpCtbtnAmount, decimal decEmprCtbtnAmount, string sUser, ref DataRow drLocal)
        {
            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    drLocal["ID"] = RetValidLen(sESICMntDedID);

                    drLocal["AddedBy"] = RetValidLen(sUser);
                    drLocal["AddedDate"] = DateTime.Now.ToString();
                    drLocal["AddedFromIP"] = "";
                }

                drLocal["ESICEligibleEmpID"] = RetValidLen(sESICEligibleEmpID);
                drLocal["MonthNo"] = Convert.ToDateTime(sToDate).Month;
                drLocal["YearNo"] = Convert.ToDateTime(sToDate).Year;
                drLocal["EmployeeContributionAmount"] = decEmpCtbtnAmount;
                drLocal["EmployerContributionAmount"] = decEmprCtbtnAmount;

                drLocal["UpdatedBy"] = RetValidLen(sUser);
                drLocal["UpdatedDate"] = DateTime.Now.ToString();
                drLocal["UpdatedFromIP"] = "";
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                //
            }
        }//End Function
    }
    public class ESICParaListNew
    {
        public string GroupID { get; set; }
        public string PlantID { get; set; }
        public string sEmpSystemID { get; set; }
        public string LocalCurrencyID { get; set; }
        public string ForeignCurRate { get; set; }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public string sUser { get; set; }
        public DataSet dsSalInfo { get; set; }
        //public bool bStructure { get; set; } = false;
        public bool ShouldNotProcessUntaggedEmp { get; set; }
        public bool IsESICMandatoryNew { get; set; }
        public bool IsESICOptionalNew { get; set; }
        public bool IsESICEntitleNew { get; set; } = false;
        public string IsbuttonPFClicked { get; set; } = "NO";
        public bool IsESICPolicyDefined { get; set; } = false;
    }
    public class dicESICSalInfoNew
    {
        public string SlrInfoDefSystemID { get; set; } = "";
        public string PlantID { get; set; } = "";
        public string EmpInfoSystemID { get; set; } = "";
        public DateTime? EffectiveDate { get; set; }
        public string SalaryRuleMasterSystemID { get; set; } = "";
        public string SalaryHeadID { get; set; } = "";
        public string SalaryHead { get; set; } = "";
        public string HeadType { get; set; } = "";
        public string HeadCategory { get; set; } = "";
        public string AmtDefinitionCurrencyID { get; set; } = "";
        public decimal AmtDefinitionRate { get; set; } = 0;
        public string EntryCurrencyID { get; set; } = "";
        public string EntryCurrency { get; set; } = "";
        public decimal EntryAmount { get; set; } = 0;
        public string DefineCurrencyID { get; set; } = "";
        public string SalaryID { get; set; } = "";
        public string DefinitionCurrency { get; set; } = "";
        public decimal DefineAmount { get; set; } = 0;
        public bool AccumulateExchangeRate { get; set; } = false;
        public string AcltExcDisbSlrHDID { get; set; } = "";
        public string DisbusmentCurrencyID { get; set; } = "";
        public string DisbusmentCurrency { get; set; } = "";
        public string RuleType { get; set; } = "";
        public decimal FixedMonthDayValue { get; set; } = 0;
        public bool IsMonthDay { get; set; } = false;
        public bool IsMonthWorkDay { get; set; } = false;
        public bool IsFixedDisbus { get; set; } = false;
        public bool IsBankPayment { get; set; } = false;
        public bool IsCashPayment { get; set; } = false;
        public string SalaryRuleDayStatusSystemID { get; set; } = "";
        public bool IsOverWrite { get; set; } = false;
        public string ShiftType { get; set; } = "";
        public string DayType { get; set; } = "";
        public string LeaveType { get; set; } = "";
        public bool IsNetPayEffect { get; set; } = false;
        public string EarningCurrencyID { get; set; } = "";
        public decimal EarningAmount { get; set; } = 0;
    }
    public class dicESICEligibleNew
    {
        public string ID { get; set; } = "";
        public string EmpSystemID { get; set; } = "";
        public string ESICMstID { get; set; } = "";
        public string ESICDtlID { get; set; } = "";
        public DateTime? EndDate { get; set; }
        public DateTime? StartDate { get; set; }
        public bool IsActive { get; set; } = false;
    }
}