using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Service.Extension.HumanResource.Leave
{
    public class clsOffDayListGenerate
    {
        string plantid = string.Empty;
        string toDate = string.Empty;
        string fromDate = string.Empty;
        List<string> list_W_Total = null;
        List<string> list_H_Total = null;
        public clsOffDayListGenerate(string plantid, string fromDate, string toDate, List<string> list_W_Total, List<string> list_H_Total)
        {
            this.plantid = plantid;
            this.toDate = toDate;
            this.fromDate = fromDate;
            this.list_W_Total = list_W_Total;
            this.list_H_Total = list_H_Total;
        }
        public void GenerateList(List<string> listW, List<string> listH)
        {
            try
            {
                //get hr setting for H/W
                DataSet dsHRSetting = null;
                _getHRSettingForHW_Priority(plantid, out dsHRSetting);

                DateTime _fd = Convert.ToDateTime(fromDate);
                DateTime _td = Convert.ToDateTime(toDate);
                while (_fd < _td)
                {
                    if (list_W_Total.Contains(_fd.ToString("dd-MMM-yyyy")) && list_H_Total.Contains(_fd.ToString("dd-MMM-yyyy")))
                    {
                        if (dsHRSetting.Tables[0].Rows.Count > 0)
                        {
                            listH.Add(_fd.ToString("dd-MMM-yyyy"));
                        }
                        else
                        {
                            listW.Add(_fd.ToString("dd-MMM-yyyy"));
                        }
                    }
                    else
                    {
                        //W
                        if (list_W_Total.Contains(_fd.ToString("dd-MMM-yyyy")))
                        {
                            listW.Add(_fd.ToString("dd-MMM-yyyy"));
                        }
                        //H
                        if (list_H_Total.Contains(_fd.ToString("dd-MMM-yyyy")))
                        {
                            listH.Add(_fd.ToString("dd-MMM-yyyy"));
                        }
                    }
                    _fd = _fd.AddDays(1);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        void _getHRSettingForHW_Priority(string plantid, out System.Data.DataSet dsRef)
        {
            string strSQL = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"select systemid from PlantWiseHRMSSetting where IsPriorityOfHolidayOverWeekOff=1 and plantid='" + plantid + @"'";
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
    }
}
