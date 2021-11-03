using Library.Crosscutting.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.HumanResource.Leave
{
    public class clsLeaveSandwichOnHoliday
    {
        public void ProcessSandwich(CustomIdentity identity, string sFromDate, string sToDate,string CurrDate)
        {
            DataSet dsEmpList = null;
            DataSet dsSave = null;
            DataView dvSave = null;
            bool IsDataOk = false;
            string sPlantID = string.Empty;
            try
            {
                sPlantID = identity.PlantId;
                DateTime dtf = Convert.ToDateTime(sFromDate).AddDays(-7);
                DateTime dtt = Convert.ToDateTime(sToDate).AddDays(7);
                if(dtt>Convert.ToDateTime(CurrDate))
                {
                    dtt = Convert.ToDateTime(CurrDate);
                }

                _GetEmpList(sPlantID, dtf.ToString("dd-MMM-yyyy"), dtt.ToString("dd-MMM-yyyy"),out dsEmpList);

                string _seed = string.Empty;
                bplib.clsGenID objGenID = new bplib.clsGenID();
                objGenID.GenHRID(DateTime.Now.ToShortDateString().ToString(), "HolidaySandwich", out _seed);
                int _count = 0;
                if (dsEmpList.Tables[0].Rows.Count > 0)
                {
                    //call delete
                    _DeleteHolidayAbsentismAssignment(sFromDate, sToDate, identity.PlantId);
                    _GetHolidayAbsentismAssignment(sFromDate, sToDate, out dsSave);

                    for (int i = 0; i < dsEmpList.Tables[0].Rows.Count; i++)
                    {
                        IsDataOk = false;
                        //BL
                        //set IsDataOk
                        string _EmpSystemID = dsEmpList.Tables[0].Rows[i]["EmpSystemID"].ToString();
                        string _WorkDate = dsEmpList.Tables[0].Rows[i]["AttdnProcDate"].ToString();
                        string _Daystatus = dsEmpList.Tables[0].Rows[i]["Category"].ToString();
                        if(_Daystatus.ToUpper()=="HOLIDAY")
                        {
                            if(Convert.ToDateTime(_WorkDate)>=Convert.ToDateTime(sFromDate) && Convert.ToDateTime(_WorkDate) <= Convert.ToDateTime(sToDate))
                            {
                            IsDataOk = true;
                            }
                        }
                        if (IsDataOk)
                        {
                            dvSave = new DataView(dsSave.Tables[0]);
                            dvSave.RowFilter = "EmpSystemID='" + _EmpSystemID + "' AND WorkDate='" + _WorkDate + "' ";
                            if (dvSave.Count == 0)
                            {
                                _count++;
                                DataRow dr = dsSave.Tables[0].NewRow();
                                _AddRow(ref dr, identity, _WorkDate, _EmpSystemID, _seed, _count);
                                dsSave.Tables[0].Rows.Add(dr);
                            }
                            dvSave.RowFilter = null;
                        }//IsDataOk
                    }//for
                    _Save(dsSave);
                }//count
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void _AddRow(ref DataRow dr,CustomIdentity identity,string _WorkDate,string _EmpSystemID,string _seed, int _count)
        {
            try
            {
                dr["Id"] = "S" + _seed + "-" + _count;
                dr["PlantID"] = identity.PlantId;
                dr["EmpSystemID"] = _EmpSystemID;
                dr["WorkDate"] = _WorkDate;
                dr["CompanyGroupId"] = identity.CompanyGroupId;
                dr["AddedBy"] = identity.Name;
                dr["AddedDate"] = System.DateTime.Now.ToString();
                dr["AddedFromIP"] = identity.IPAddress;
                dr["UpdatedBy"] = identity.Name;
                dr["UpdatedDate"] = System.DateTime.Now.ToString();
                dr["UpdatedFromIP"] = identity.IPAddress;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void _GetEmpList(string sPlantID, string sFromDate, string sToDate, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {

                strSQL = @" DECLARE @FromDate DATE = '"+ sFromDate + @"'
                            DECLARE      @ToDate DATE = '" + sToDate + @"'
                            DECLARE		@Plantid varchar(10)
		                            set @Plantid='" + sPlantID + @"'

                     SELECT 0 CheckBoxSelect 
                     , APD.EmpSystemID
                     , FORMAT(APD.WorkDate ,'dd-MMM-yyyy') AttdnProcDate
                     , APD.DayStatus,apd.Category
                     , FORMAT( x.WorkDate  ,'dd-MMM-yyyy') BeforeWorkDate
                     , x.DayStatus BeforeDayStatus
                     , FORMAT(y.WorkDate,'dd-MMM-yyyy') AfterWorkDate
                     , y.DayStatus AfterDayStatus
                     , EI.EmployeeCode
                     , EI.EmployeeName
                     , FORMAT(EI.DOB,'dd-MMM-yyyy') DOB
                     , FORMAT(EI.DOJ,'dd-MMM-yyyy') DOJ
 
                    FROM ( select apd.EmpSystemID, apd.WorkDate, apd.DayStatus,dt.Category
	                       FROM AttdnProcessData AS apd
                           left join daytype dt on dt.daytype=apd.daystatus
	                       WHERE apd.WorkDate BETWEEN @FromDate AND @ToDate
                           AND apd.PlantID=@Plantid
	                       AND isnull(apd.DayStatus,'') IN (
                                                            ---DayType
                                                            SELECT DayType  FROM DayType WHERE Category='Weekend' --AND  1=(SELECT IsSandwichAbsentInWeekend  From  [dbo].[PlantWiseHRMSSetting] where plantid=@Plantid)
                                                            UNION
                                                            SELECT DayType FROM DayType WHERE Category='Holiday' --AND  1=(SELECT IsSandwichAbsentInHoliday From  [dbo].[PlantWiseHRMSSetting] where plantid=@Plantid)

                                                           ) 
                           AND apd.EmpSystemID NOT IN (select EmpSystemID  FROM [SCS].[WeeklyAbsentismAssignment] WHERE EmpSystemID=apd.EmpSystemID AND WorkingDate=apd.WorkDate)
                    ) AS APD
                    LEFT OUTER JOIN AttdnProcessData AS X ON apd.EmpSystemID=x.EmpSystemID AND isnull(x.WorkDate,'')=(SELECT TOP 1 isnull(WorkDate,'') AS PK 
                                                                                                                      FROM AttdnProcessData WHERE WorkDate<apd.WorkDate 
																								                      AND EmpSystemID=APD.EmpSystemID	AND DayStatus NOT IN (
                                                                                                                                                                                ---DayType
                                                                                                                                                                                SELECT DayType  FROM DayType WHERE Category='Weekend' --AND  1=(SELECT IsSandwichAbsentInWeekend  From  [dbo].[PlantWiseHRMSSetting] where plantid=@Plantid)
                                                                                                                                                                                UNION
                                                                                                                                                                                SELECT DayType FROM DayType WHERE Category='Holiday' --AND  1=(SELECT IsSandwichAbsentInHoliday From  [dbo].[PlantWiseHRMSSetting] where plantid=@Plantid)
                                                                                                                                                                             ) 
                                                                                                                      ORDER BY WorkDate  DESC)
 

                    LEFT OUTER JOIN AttdnProcessData AS Y ON apd.EmpSystemID=Y.EmpSystemID AND isnull(Y.WorkDate,'')=(SELECT TOP 1 isnull(WorkDate,'') AS PK 
                                                                                                                      FROM AttdnProcessData WHERE WorkDate>apd.WorkDate 
																								                      AND EmpSystemID=APD.EmpSystemID	AND DayStatus NOT IN (
                                                                                                                                                                                 ---DayType
                                                                                                                                                                                SELECT DayType  FROM DayType WHERE Category='Weekend' --AND  1=(SELECT IsSandwichAbsentInWeekend  From  [dbo].[PlantWiseHRMSSetting] where plantid=@Plantid)
                                                                                                                                                                                UNION
                                                                                                                                                                                SELECT DayType FROM DayType WHERE Category='Holiday' --AND  1=(SELECT IsSandwichAbsentInHoliday From  [dbo].[PlantWiseHRMSSetting] where plantid=@Plantid)
                                                                                                                                                                              ) 
                                                                                                                      ORDER BY WorkDate  ASC)
                                                                                                  
                                                                                                  
                                                                                                  
                     LEFT JOIN dbo.Employeeinformation EI ON EI.SystemId = apd.EmpSystemID
                    WHERE
					
						(ISNULL(x.DayStatus,'')='LV' and isnull(x.IsLWP,0)=1 AND ISNULL(Y.DayStatus,'')='LV' and isnull(y.IsLWP,0)=1)
					or (ISNULL(x.DayStatus,'')='LV' and isnull(x.IsLWP,0)=1 AND ISNULL(Y.DayStatus,'')='A')
					or (ISNULL(x.DayStatus,'')='A' AND ISNULL(Y.DayStatus,'')='LV' and isnull(y.IsLWP,0)=1)
					or (ISNULL(x.DayStatus,'')='A' AND ISNULL(Y.DayStatus,'')='A')
					
                    ORDER BY APD.EmpSystemID,APD.WorkDate  ";

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
        }//End 
        void _GetHolidayAbsentismAssignment(string sFromDate, string sToDate,out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @" SELECT *  FROM [TRN].[HolidayAbsentismAssignment] where WorkDate between '"+sFromDate+@"' and '"+sToDate+@"' ";
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
        }//End 
        void _Save(params DataSet[] dsRef)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();//

                //objCon.ExecuteNonQueryWrapper("DELETE FROM SalaryProcChild WHERE MonthNo = " + intMonthNo + " AND YearNo = " + intYearNo + " AND IsDisbursed = 0 AND (" + strEmp + ")", true, "1");

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
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    throw (ex);
                }
                catch (Exception ex2)
                {
                    throw ex;
                }

            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }

        }//End Function
        void _DeleteHolidayAbsentismAssignment(string sFromDate, string sToDate,string plantid)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            string sql = string.Empty;
            try
            {
                sql = @"delete from [TRN].[HolidayAbsentismAssignment] where WorkDate between '"+sFromDate+"' and '"+sToDate+"' and PlantId='"+plantid+"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(sql, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    throw (ex);
                }
                catch (Exception ex2)
                {
                    throw ex;
                }

            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }

        }//End Function
    }
}
