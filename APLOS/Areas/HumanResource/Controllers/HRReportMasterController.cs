using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Security.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace Aplos.Areas.HumanResource.Controllers
{
    public class HRReportMasterController : Controller
    {
        
        public ActionResult Aplos()
        {
            return View();
        }

        private readonly SqlRepository _sqlRepository;
        #region Constructor
        public HRReportMasterController()
        {
            _sqlRepository = new SqlRepository();
        }
        #endregion Constructor


        public IEnumerable<object> GetMaster(string Id)
        {
            try
            {
                var str = @"select * from HKP.HRReportMaster where Id = '" + Id + "' ";
                return _sqlRepository.GetDataCollection(str);
            }
            catch (Exception e)
            {
                throw e;
            }
        }



        public IEnumerable<object> GetList()
        {
            try
            {
                string sql = @"select * from HKP.HRReportMaster";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public ActionResult GetEntity()
        {
            var entityQry = @"select Id Value, UserName Text from org.Entity where Id !=111 ";
            return Json(_sqlRepository.GetDataCollection(entityQry), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetBudgetCode(List<string> entitylist)
        {
            var bgtQuery = "";
            for (int i = 0; i < entitylist.Count; i++)
            {
                bgtQuery = @"select D.UserName Division, DT.UserName Department, S.UserName Section, SS.UserName SubSection, DSG.UserName Designation, A.UserName Activity,SDF.UserName [Shift], P.Code PositionCode
, P.UserName Position ,BGT2.Code BudgetCode from MST.ManpowerBudget BGT
left join MST.ManpowerBudget BGT2 on BGT.Id = BGT2.ROBudgetCode
left join MST.BudgetMasterActivity BMA on BGT.ROBudgetCode = BMA.BudgetMasterId
left join HKP.Activity A on BMA.ActivityId = A.Id
left join dbo.ShiftDefination SDF on BGT.ShiftDefinationId = SDF.SystemID
left join ORG.Position P on BGT.PositionId = P.Id
left join ORG.Division D on P.DivisionId  = D.Id
left join ORG.Department DT on P.DepartmentId = DT.Id
left join ORG.Section S on P.SectionId = S.Id
left join ORG.SubSection SS on P.SubSectionId = SS.Id
left join ORG.Division DSN on P.DivisionId = DSN.Id
left join HKP.Designation DSG ON P.DesignationId = DSG.Id
where BGT.EntityId in ('" + entitylist[i].ToString() + "')";
            }
            return Json(_sqlRepository.GetDataCollection(bgtQuery), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetEmployee()
        {
            try
            {
                var str = @"select ei.SystemId, ei.EmployeeName, ei.EmployeeId , FORMAT(ei.DOJ, 'dd-MMM-yyyy') as DOJ, x.UserName as category,
                            FORMAT(ei.DOB, 'dd-MMM-yyyy') as DOB ,ei.EmployeeCode, DP.UserName as Department ,
                            LDSG.StandardName as Designation, SC.UserName as Section,
                            SBC.UserName as SubSection from dbo.EmployeeInformation ei
                            LEFT JOIN MST.ManpowerBudget MBGT ON MBGT.Id = ei.BudgetCode
                            LEFT JOIN ORG.POSITION POS ON POS.ID = MBGT.POSITIONID
                            left join MST.ManpowerBudgetDetail MBD ON MBD.ManpowerBudgetId = MBGT.ID
                            left join ORG.Entity UN on UN.Id = MBGT.EntityId
                            left join ORG.Department DP on DP.ID = POS.DepartmentId
                            left join ORG.Section SC on SC.Id = POS.SectionId
                            left join ORG.SubSection SBC on SBC.Id = POS.SubSectionId
                            LEFT JOIN HKP.DesignationGroup EDSGG on EDSGG.id=ei.DesignationGroupId
                            LEFT JOIN hkp.Designation LDSG on LDSG.id = POS.DesignationId
                            LEFT JOIN HKP.LegalDesignation GDSG on GDSG.Id=ei.LegalDesignationId
                            left join mst.DesignationMaster dm on dm.DesignationId = LDSG.Id
                            left join hkp.EmployeeCategory x on x.Id=dm.EmployeeCategoryId
                            left join ShiftDefination sd on sd.systemid = mbgt.shiftdefinationid
                            left join SalaryRuleMaster SRM on srm.systemid = ei.salaryrulemastersystemid
                            left join ResidenceGroup RG on RG.Id = ei.ResidenceGroupId
                            left join TransportGroup TG on TG.Id = ei.TransportGroupId          
                            where ei.EmployeeStatus = 'Active'";
                return Json(_sqlRepository.GetDataCollection(str), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Dictionary<string, object> Save(Dictionary<string, object> data, string Employee)
        {
            try
            {
                //Master Table - PMSMaster
                string TableName = "HKP.HRReportMaster";
                DataSet dsMaster;

                /* if (Employee == null)
                 {
                     throw new Exception("Please Select Responsible Person !!");
                 }*/

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id ='" + data["Id"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);

                    data["Id"] = _Id;
                    data["ResponsiblePersonId"] = Employee;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    data["ResponsiblePersonId"] = Employee;
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Delete(string id)
        {
            try
            {
                string TableName = "HKP.HRReportMaster";

                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where id='" + id + "'");
                con.CommitTransaction();

            }
            catch (Exception ex)
            {

                throw ex;

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
            dr["AddedDate"] = DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = DateTime.Now.ToString();
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
            dr["UpdatedDate"] = DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
            dr.EndEdit();
        }

        public double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM HKP.HRReportMaster");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }
    }
}