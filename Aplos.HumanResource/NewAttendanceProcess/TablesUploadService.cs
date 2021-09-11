using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.NewAttendanceProcess

{
    public class TablesUploadService
    {

        ISqlRepository _sqlRepository;
        public TablesUploadService()
        {
            _sqlRepository = new SqlRepository();
        }


        public IEnumerable<object> getCurrentList()
        {
            try
            {
                var str = @"Select ROW_NUMBER() OVER(ORDER BY ew.EffectiveDate desc) as Rows,ew.EmpSystemId, ew.WOHeaderId,format(ew.EffectiveDate,'dd-MMM-yyyy') as EffectiveDate
                            , ew.Id, ei.SystemId,ei.EmployeeCode,
                            ei.EmployeeName, wo.UserName as WOName
                            from dbo.EmployeeWeeklyOff ew
                            left join dbo.EmployeeInformation ei on ei.SystemId = ew.EmpSystemId
                            left join dbo.WeekOffHeader wo on wo.Id = ew.WOHeaderId
                            order by CAST(ew.EffectiveDate as Date) desc
                                ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch(Exception e)
            {
                throw e;
            }
        }


        // The Section for Saving And Updating of Data
        public void AddNewRow(DataTable dt, Dictionary<string, object> sourceData, string addedname, string addeddate)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr["AddedBy"] = addedname;
            dr["AddedDate"] = addeddate;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();

            dt.Rows.Add(dr);
        }

    

        //The Apis for the 2nd Page

    

        public void SaveFileList(List<Dictionary<string,object>> data ,string tab)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string addedname = identity.Name;
                string addeddate = System.DateTime.Now.ToString();
                string TableName = "TPI."+tab;
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where 1 = 2", out dsMaster, false, "1");

               
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    int indexa = 0;
                    for (int i = 0; i < data.Count; i++)
                    {
                        Dictionary<string, object> jj = data[i];
                        indexa++;
                        jj["Id"] = i ;

                        AddNewRow(dsMaster.Tables[0], jj, addedname, addeddate);
                    }


                }

                var sqls = @"Delete from "+TableName;

                ConnectionManager.DAL.ConManager objCone = null;
                objCone = new ConnectionManager.DAL.ConManager("1");
                objCone.OpenConnection("1");
                objCone.BeginTransaction();

                objCone.ExecuteNonQueryWrapper(sqls, true, "1");
                objCone.CommitTransaction();

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
            }
            catch(Exception e)
            {
                throw e;
            }
        }
       
        public DataTable getCurrentTableFile(string tab )
        {
            try
            {
                var str = @"Select * from TPI."+tab+" ";

                return _sqlRepository.GetDataTable(str);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

       
    }
}