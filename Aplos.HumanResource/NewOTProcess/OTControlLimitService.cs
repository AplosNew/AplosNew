using Library.Crosscutting.Security;
using Library.Data.Sql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.NewOTProcess
{
   public class OTControlLimitService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public OTControlLimitService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }

        public DataTable GetBudgetDataToUpload()
        {
            try
            {
                string sql = @"SELECT distinct mb.Id BudgetCodeId,mb.Deployment,mb.Code BudgetCode,p.Code PositionCode,DGM.EmployeeCategory,E.UserName Entity,d.UserName Department,s.UserName Section,ss.UserName SubSection,DG.UserName Designation,p.Activity,sd.ShiftDefinationName
        ,ei.EmployeeName ResponsiblePerson,OT.DailyOTLimit,OT.WeeklyOTLimit,OT.WeekOffOTLimit,OT.MonthlyOTLimit,'' Remarks
        ,mb.ROBudgetCode,mb.PRBudgetCode
        ,ag.UserName AttendanceGroup,P.UserDefineGroup2,Direct=CASE WHEN P.IsDirect=1 THEN 'Yes' ELSE 'No' END,ISNULL(ONR.ONRoll,0)OnRoll
        FROM MST.ManpowerBudget AS mb
        LEFT JOIN ORG.Entity E ON E.Id=mb.EntityId
        LEFT JOIN ORG.Position AS p ON P.Id=mb.PositionId
        LEFT JOIN ORG.Department AS d ON d.Id=p.DepartmentId
        LEFT JOIN ORG.Section AS S ON S.Id=p.SectionId
        LEFT JOIN ORG.SubSection AS SS ON SS.Id=p.SubSectionId
        LEFT JOIN HKP.Designation DG ON DG.Id=P.DesignationId 
        LEFT JOIN dbo.ShiftDefination AS sd ON sd.SystemID=mb.ShiftDefinationId
        LEFT JOIN dbo.AttendanceGroup AS ag ON ag.Id=mb.AttendanceGroupId
        LEFT JOIN (SELECT dm.DesignationId,dmc.IsOTEntitled,ec.UserName EmployeeCategory FROM MST.DesignationMaster AS dm
        LEFT JOIN SCS.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId=dm.Id
        LEFT JOIN HKP.EmployeeCategory AS ec ON ec.Id=dm.EmployeeCategoryId                            
        ) DGM ON DGM.DesignationId=DG.Id
        LEFT JOIN dbo.EmployeeInformation AS ei ON ei.SystemId=mb.ResponsiblePerson
       LEFT JOIN (SELECT COUNT(SystemId)OnRoll,BudgetCode FROM dbo.EmployeeInformation GROUP BY BudgetCode) ONR ON ONR.BudgetCode=mb.Id 
       
        LEFT JOIN (
        	SELECT * FROM  [dbo].[OTControlLimitDetail]
                   WHERE OTControlLimitId IN(SELECT TOP (1) ID FROM [dbo].[OTControlLimit] A ORDER BY EffectiveDate DESC)
        ) OT ON OT.BudgetCodeId=MB.Id
       
        WHERE mb.Active=1 AND DGM.IsOTEntitled=1";

                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public void SaveData(Dictionary<string, object> data, List<Dictionary<string, object>> detailList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                DataSet dsMaster, dsDetail;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.OTControlLimit WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";
                string masterId = "";

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "OTControlLimit", out _Id);

                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }

                masterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                con.OpenDataSetThroughAdapter("SELECT * FROM dbo.OTControlLimitDetail WHERE OTControlLimitId ='" + masterId + "'", out dsDetail, false, "1");

                int count = 0;
                foreach (var item in detailList)
                {
                    count++;
                    DataView dv = new DataView(dsDetail.Tables[0]);
                    dv.RowFilter = "Id='" + item["Id"] + "'";

                    if (dv.Count == 0)
                    {
                        item["Id"] = masterId + "-" + count;
                        item["OTControlLimitId"] = masterId;

                        AddNewRow(dsDetail.Tables[0], item);
                    }
                    else
                    {
                        DataRow drmo = dv[0].Row;
                        EditRow(drmo, item);
                    }
                }


                OTSBD.clsStaticInfo obj = new OTSBD.clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsDetail);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void AddNewRow(DataTable dt, Dictionary<string, object> sourceData)
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

            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

            dt.Rows.Add(dr);
        }
        private void EditRow(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

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

            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }


        public IEnumerable<object> GetList()
        {
            string sql = @"SELECT OTL.*,ei.EmployeeName ApproveByName,eiw.EmployeeName ByWhomName FROM [dbo].[OTControlLimit] OTL
LEFT JOIN dbo.EmployeeInformation AS ei ON ei.SystemId = OTL.ApproveBy
LEFT JOIN dbo.EmployeeInformation AS eiw ON eiw.SystemId = OTL.ByWhom";
            return _sqlRepository.GetDataCollection(sql,null);
        }
    }
}
