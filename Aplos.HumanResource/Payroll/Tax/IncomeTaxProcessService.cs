using System;
using System.Collections.Generic;
using Library.Data.Sql;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using System.Collections.Specialized;
using Library.Service.Extension;

namespace Library.HumanResource.Payroll.Tax
{
    public class IncomeTaxProcessService
    {
        #region Constructor 

        ISqlRepository _sqlRepository;
        public IncomeTaxProcessService()
        {
            _sqlRepository = new SqlRepository();
        }
        #endregion

     
        #region Processing Functions

        public IEnumerable<object> getGridData(string PolicyId,string Earning,string PlantId)
        {
            try
            {
                var sql = @"	select dd.* from (	select sdm.EmpInfoSystemID,e.EmployeeName,
                SUM((sd.DefineAmount*12))as StructureEarning,
	            d.UserName as Dept,u.UserName as Unit,s.UserName as Section,ss.UserName as SubSection
				from SalaryInfoDefineMaster sdm join SalaryInfoDefine sd on 
				sd.SalaryID=sdm.SystemID
				join SalaryHead sh on sh.SalaryHeadID=sd.SalaryHeadID
				join TaxEarningMasterChild tem on tem.SalaryHeadId=sh.SalaryHeadID	
				join EmployeeInformation e on e.SystemId=sdm.EmpInfoSystemID
				left join org.Department d on d.Id=e.DepartmentId
				left join org.Unit u on u.Id=e.UnitId
				left join org.Section s on s.Id=e.SectionId
				left join org.SubSection ss on ss.Id=e.SubSectionId
				where sh.HeadType='E' and tem.TaxPolicyHeaderId='"+PolicyId+@"'	
				and e.EmployeeStatus='Active' and e.PlantId='"+PlantId+@"'		
			    group by sdm.EmpInfoSystemID,e.EmployeeName,d.UserName,u.UserName
				,s.UserName,ss.UserName
				)as dd
				where dd.StructureEarning>='"+Earning+@"'
				order by dd.StructureEarning desc";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getPlants()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                
                var sql = @"select Id as Value,UserName as Text from org.Plant
				where CompanyId='"+identity.CompanyId+"'";
                
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public void ProcessIncomeTax(string PolicyId, string YearId, string PlantId, string EmpId,string TaxTypeId,string StartDate,string EndDate)
        {
            try
            {

                #region DataSet Generation Region

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataTable GrossEarningDt;
                DataSet dsRef, dsMaster;
                StringCollection StrDistinctEmployee = new StringCollection();

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                string sql = @"select * from EmployeeIncomeTaxMaster where" +
                    " EmpSystemId In ("+EmpId+") AND TaxPolicyHeaderId='" + PolicyId + "' " +
                    " AND TaxYearId='" + YearId + "'";

                con.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                var sqly = @"Select SystemId as EmployeeId,EmployeeName,
                cast((DATEDIFF(m, DOB, GETDATE())/12) as varchar) + ' Year ' + 
                       cast((DATEDIFF(m, DOB, GETDATE())%12) as varchar) + ' Month' as Age	   
                from EmployeeInformation where SystemId in ("+EmpId+")";

                con.OpenDataSetThroughAdapter(sqly, out DataSet dsEmpMaster, false, "1");

                string sqlx = @"select * from EmployeeIncomeTaxMaster where 1=2";
                con.OpenDataSetThroughAdapter(sqlx, out dsRef, false, "1");

                #endregion

                #region Adding in StringCollection 

                for (int i = 0; i < dsMaster.Tables[0].Rows.Count; i++)
                {
                    string EmployeeId = dsMaster.Tables[0].Rows[i][@"EmpSystemId"].ToString();

                    if (StrDistinctEmployee.Contains(EmployeeId))
                    {
                        continue;
                    }
                    StrDistinctEmployee.Add(EmployeeId);
                }

                #endregion

                #region Saving in IncomeTaxMaster

                for (int i = 0; i < dsEmpMaster.Tables[0].Rows.Count; i++)
                {
                    string EmployeeId = dsEmpMaster.Tables[0].Rows[i][@"EmployeeId"].ToString();
                    string Age= dsEmpMaster.Tables[0].Rows[i][@"Age"].ToString();

                    if (StrDistinctEmployee.Contains(EmployeeId))
                    {
                        continue;
                    }
                    else
                    {
                        DataRow drF = dsRef.Tables[0].NewRow();
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("dbo.EmployeeIncomeTaxMaster", out string _Id);
                       
                        drF["Id"] = "EIT"+_Id;
                        drF["EmpSystemId"] = EmployeeId;
                        drF["TaxPolicyHeaderId"] = PolicyId;
                        drF["TaxTypeId"] = TaxTypeId;
                        drF["TaxYearId"] = YearId;
                        drF["CurrentAge"] = Age;
                        drF["AddedBy"] = identity.Name;
                        drF["AddedFromIp"] = identity.IPAddress;
                        drF["AddedDate"] = DateTime.Now.ToString();
                        dsRef.Tables[0].Rows.Add(drF);
                    }
                    
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsRef);

                #endregion

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion


    }

}

