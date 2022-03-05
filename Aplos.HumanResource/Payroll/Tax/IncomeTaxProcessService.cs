using System;
using System.Collections.Generic;
using Library.Data.Sql;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;

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

        public void ProcessIncomeTax(string PolicyId, string YearId, string PlantId, string EmpId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataTable TaxableIncomeDt, EstimatedTaxDt, SurchargeDt;
                DataSet dsRef;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                              
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion


    }

}

