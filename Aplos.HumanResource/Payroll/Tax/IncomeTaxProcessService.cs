using System;
using System.Collections.Generic;
using Library.Data.Sql;
using Library.Crosscutting.Security;
using System.Threading;
using System.Data;
using System.Collections.Specialized;
using Library.Service.Extension;
using System.Linq;

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
                #region IncomeTaxMaster Data Generation

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataTable GrossEarningDt;
                DataSet dsRef, dsMaster;
                StringCollection StrDistinctEmployee = new StringCollection();

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                string sql = @"select * from EmployeeIncomeTaxMaster where" +
                    " EmpSystemId In (" + EmpId + ") AND TaxPolicyHeaderId='" + PolicyId + "' " +
                    " AND TaxYearId='" + YearId + "'";

                con.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                var sqly = @"Select SystemId as EmployeeId,EmployeeName,
                cast((DATEDIFF(m, DOB, GETDATE())/12) as varchar) + ' Year ' + 
                       cast((DATEDIFF(m, DOB, GETDATE())%12) as varchar) + ' Month' as Age	   
                from EmployeeInformation where SystemId in (" + EmpId + ")";

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
                    string Age = dsEmpMaster.Tables[0].Rows[i][@"Age"].ToString();

                    if (StrDistinctEmployee.Contains(EmployeeId))
                    {
                        continue;
                    }
                    else
                    {
                        DataRow drF = dsRef.Tables[0].NewRow();
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenID("dbo.EmployeeIncomeTaxMaster", out string _Id);

                        drF["Id"] = "EIT" + _Id;
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

                #region Gross Earning Saving
                GrossEarningDt = GrossEarningQuery(EmpId, PolicyId, YearId, StartDate, EndDate, TaxTypeId);

                string sqla = @"select * from EmployeeEarningData where 1=2";
                con.OpenDataSetThroughAdapter(sqla, out dsRef, false, "1");

                if (GrossEarningDt.Rows.Count > 0)
                {
                    DeleteExistingEarningData(EmpId, PolicyId, YearId, TaxTypeId);

                    for (int j = 0; j < GrossEarningDt.Rows.Count; j++)
                    {
                        string EarningId = GrossEarningDt.Rows[j][@"EarningMasterId"].ToString();
                        string IncomeTaxId = GrossEarningDt.Rows[j][@"IncomeTaxId"].ToString();
                        string ActualValue = GrossEarningDt.Rows[j][@"ActualValue"].ToString();
                        string OpeningValue = GrossEarningDt.Rows[j][@"OpeningValue"].ToString();
                        string ArrearValue = GrossEarningDt.Rows[j][@"ArrearValue"].ToString();
                        string StructureValue = GrossEarningDt.Rows[j][@"StructureValue"].ToString();

                        double Gross = Convert.ToDouble(ActualValue) + Convert.ToDouble(OpeningValue) +
                            Convert.ToDouble(ArrearValue) + Convert.ToDouble(StructureValue);


                        DataRow drF = dsRef.Tables[0].NewRow();
                        clsGenID genid = new clsGenID();
                        genid.GenID("EmployeeEarningData", out string _pk);

                        drF["Id"] = "EE" + _pk;
                        drF["EmployeeIncomeTaxId"] = IncomeTaxId;
                        drF["EarningMasterId"] = EarningId;
                        drF["ActualValue"] = ActualValue;
                        drF["OpeningValue"] = OpeningValue;
                        drF["ArrearValue"] = ArrearValue;
                        drF["StructureValue"] = StructureValue;
                        drF["GrossEarning"] = Gross; ;
                        drF["AddedBy"] = identity.Name;
                        drF["AddedFromIp"] = identity.IPAddress;
                        drF["AddedDate"] = DateTime.Now.ToString();
                        dsRef.Tables[0].Rows.Add(drF);

                    }
                    _info.SaveDataSets(dsRef);
                }

                #endregion

                #region NetEarning Saving
             
                Dictionary<string, List<ExemptionCalcualtionModel>> CalculatedDict =
                 new Dictionary<string, List<ExemptionCalcualtionModel>>();

                EmployeeIncomeTaxService eis = new EmployeeIncomeTaxService();
                DataTable NetEarningDt = eis.EarningQuery(EmpId, PolicyId, YearId);
                if (NetEarningDt.Rows.Count > 0)
                {
                    for (int i = 0; i < NetEarningDt.Rows.Count; i++)
                    {
                        string IsLessOrMore = NetEarningDt.Rows[i][@"IsLessOrMore"].ToString();
                        string SalaryHeadId = NetEarningDt.Rows[i][@"SalaryHeadId"].ToString();
                        string ExemptedValue = NetEarningDt.Rows[i][@"ExemptedValue"].ToString();
                        string EarningDataId = NetEarningDt.Rows[i][@"EarningDataId"].ToString();
                        string IncomeTaxId = NetEarningDt.Rows[i][@"IncomeTaxId"].ToString();

                        StringToFormula stf = new StringToFormula();
                        double result = stf.Eval(ExemptedValue);
                        double value = 0;
                        if (result >= 0)
                        {
                            value = result;
                            if (CalculatedDict.ContainsKey(IncomeTaxId+"/"+SalaryHeadId))
                            {
                                CalculatedDict[IncomeTaxId + "/" + SalaryHeadId].Add(new ExemptionCalcualtionModel
                                {
                                    ExemptAmt = value,
                                    LessOrMore = IsLessOrMore,
                                    EarningDataId = EarningDataId,
                                    SalaryHeadId = SalaryHeadId
                                });
                            }
                            else
                            {
                                var data = new List<ExemptionCalcualtionModel>();
                                data.Add(new ExemptionCalcualtionModel
                                {
                                    ExemptAmt = value,
                                    LessOrMore = IsLessOrMore,
                                    EarningDataId = EarningDataId,
                                    SalaryHeadId = SalaryHeadId
                                });
                                CalculatedDict.Add(IncomeTaxId + "/" + SalaryHeadId, data);
                            }

                        }

                    }

                    string strSql = string.Empty;
                    foreach (var item in CalculatedDict)
                    {
                        List<ExemptionCalcualtionModel> data = item.Value;
                        if (data == null)
                        {
                            continue;
                        }

                        double Amt = 0;                    
                        string Parameter = data[0].LessOrMore;
                        string TableId = data[0].EarningDataId;

                        if (Parameter == "Which Ever Is Less")
                        {
                            Amt = data.Min(x => x.ExemptAmt);
                            if (strSql.Length == 0)
                            {
                                strSql = @"UPDATE EmployeeEarningData SET ExemptionAmt='" + Amt + @"'
                                where id='" + TableId + "'";
                            }
                            else
                            {
                                strSql += Environment.NewLine +
                                    @"UPDATE EmployeeEarningData SET ExemptionAmt='" + Amt + @"'
                                where id='" + TableId + "'";
                            }
                        }
                        else if (Parameter == "Which Ever Is More")
                        {
                            Amt = data.Max(x => x.ExemptAmt);
                            if (strSql.Length == 0)
                            {
                                strSql = @"UPDATE EmployeeEarningData SET ExemptionAmt='" + Amt + @"'
                                where id='" + TableId + "'";
                            }
                            else
                            {
                                strSql += Environment.NewLine +
                                       @"UPDATE EmployeeEarningData SET ExemptionAmt='" + Amt + @"'
                                where id='" + TableId + "'";
                            }
                        }
                    }
                    if (strSql.Length > 0)
                    {
                        eis.UpdateStatus(strSql);
                    }
                }  
                
                #endregion
                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public DataTable GrossEarningQuery(string EmpId, string PolicyId, string YearId,string StartDate,string EndDate,string TaxTypeId)
        {
            try
            {
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                var sqlx = @"select COUNT(*) from salaryprocchild procx 
                join SalaryProcMaster slr on 
                slr.SystemID=procx.SlrProcMstSystemID
                where EmpInfoSystemID in ("+EmpId+@") and 
                slr.FromDate>='"+StartDate+"' and slr.ToDate<='"+EndDate+"'";
                con.OpenDataSetThroughAdapter(sqlx, out DataSet dsMaster, false, "1");

                string sql = @"";
                if (dsMaster.Tables[0].Rows.Count > 0)
                {            
                    sql = @"declare @StartDate as DATE ='" + StartDate + @"',
                            @EndDate as DATE='" + EndDate + @"';	

                select dd.EmpInfoSystemID,dd.IncomeTaxId,dd.EarningMasterId,dd.SalaryHeadId,dd.SalaryHead,
                dd.LastCalculatedDate,
				isnull(dd.OpeningValue,'0')OpeningValue,
                dd.ActualValue,dd.ArrearValue,dd.Rem_Months,
				isnull((dd.DefineAmount),'0') as MonthlyStructureValue,
                isnull((dd.DefineAmount*dd.Rem_Months),'0') as StructureValue                
                from (
                select distinct tem.Id as EarningMasterId,tem.SalaryHeadID,
                sh.SalaryHead,
                (SELECT Id from employeeincometaxmaster ei 
                where ei.TaxTypeId='" + TaxTypeId+@"' 
				AND EI.TaxYearId='"+YearId+@"'
				AND EI.TaxPolicyHeaderId='"+PolicyId+@"' AND 
				EmpSystemId=spc.EmpInfoSystemID)as IncomeTaxId,				
				(select top 1 todate from SalaryProcMaster sl join SalaryProcChild sc
			     on sc.SlrProcMstSystemID=sl.SystemID
			     where EmpInfoSystemID=spc.EmpInfoSystemID 
			      and sl.FromDate>=@StartDate and sl.ToDate<=@EndDate
			     order by todate desc)as LastCalculatedDate,
				
				(select ed.OpeningValue from  EmployeeEarningData ed  left join  
				EmployeeIncomeTaxMaster eim on eim.Id=ed.EmployeeIncomeTaxId
				where eim.EmpSystemId=spc.EmpInfoSystemID  and ed.EarningMasterId=tem.Id
				and eim.TaxPolicyHeaderId='" + PolicyId + @"' AND EIM.TaxYearId='" + YearId + @"'
                )as OpeningValue,

				--- Actual Value
				(select sum(procx.DisbusmentAmount) from 
				 salaryprocchild procx
				 join SalaryProcMaster slr on slr.SystemID=procx.SlrProcMstSystemID
				 where EmpInfoSystemID=spc.EmpInfoSystemID and 
				 salaryheadid=spc.SalaryHeadID
				 and slr.FromDate>=@StartDate and slr.ToDate<=@EndDate
				 group by procx.EmpInfoSystemID,procx.SalaryHeadID 
				 ) as ActualValue,
				
				--- Arrear Value
			     ArrearValue=isnull((select sum(procx.Diff) from 
				 ArrearProcChild procx
				 join ArrearProcMaster slr on slr.SystemID=procx.SlrProcMstSystemID
				 where EmpInfoSystemID=spc.EmpInfoSystemID
				 and salaryheadid=apc.SalaryHeadID
				 and slr.FromDate>=@StartDate and slr.ToDate<=@EndDate
				 group by procx.EmpInfoSystemID,procx.SalaryHeadID 
				 ),'0'),spc.EmpInfoSystemID,
			
                -- Months Remaining For Structure Value
				(datediff(MONTH,
				(select top 1 todate from SalaryProcMaster sl join SalaryProcChild sc
			     on sc.SlrProcMstSystemID=sl.SystemID
			     where EmpInfoSystemID=spc.EmpInfoSystemID
			     and sl.FromDate>=@StartDate and sl.ToDate<=@EndDate
			     order by todate desc),@EndDate)) As Rem_Months,
				 Structure.DefineAmount
			
			    from TaxEarningMasterChild tem 
				left join
				salaryprocchild spc  on spc.SalaryHeadId=tem.SalaryHeadId
				join salaryprocmaster sp on spc.SlrProcMstSystemID=sp.SystemID
                join SalaryHead sh on sh.SalaryHeadID=tem.SalaryHeadID
                left join ArrearProcChild apc on apc.SalaryHeadID=tem.SalaryHeadId
                left join ArrearProcMaster apm on apm.SystemID=apc.SlrProcMstSystemID
			  
				left join
				(					
				select sd.DefineAmount,sd.SalaryHeadID,tem.Id,sh.SalaryHead,sdm.EmpInfoSystemID
				from SalaryInfoDefineMaster sdm join SalaryInfoDefine sd on 
				sd.SalaryID=sdm.SystemID
				join SalaryHead sh on sh.SalaryHeadID=sd.SalaryHeadID
				join TaxEarningMasterChild tem on tem.SalaryHeadId=sh.SalaryHeadID	
				 where EmpInfoSystemID IN(" + EmpId + @")  and sh.HeadType='E'
				and tem.TaxPolicyHeaderId='" + PolicyId + @"'			
				) as Structure on Structure.SalaryHeadID=tem.SalaryHeadId
				and tem.Id=Structure.Id 
				and Structure.EmpInfoSystemID=spc.EmpInfoSystemID 

                where spc.EmpInfoSystemID IN(" + EmpId + @")  
				and ((sp.FromDate>=@StartDate and sp.ToDate<=@EndDate)
                or (apm.FromDate>=@StartDate and apm.ToDate<=@EndDate))
                and tem.TaxPolicyHeaderId='" + PolicyId + @"' and sh.HeadType='E'
                group by spc.EmpInfoSystemID,tem.SalaryHeadId,spc.SalaryHeadID,sh.SalaryHead,tem.Id,
				sp.ToDate,Structure.DefineAmount,apc.SalaryHeadID				
				) as dd
				order by dd.EmpInfoSystemID";
                
                }
                else
                {
                    sql = @"select sdm.EmpInfoSystemID,
                    (SELECT Id from employeeincometaxmaster ei where 
                    ei.TaxTypeId='"+TaxTypeId+@"' AND 
                    EI.TaxYearId='"+YearId+@"'
                    AND EI.TaxPolicyHeaderId='"+PolicyId+@"' AND 
                    EmpSystemId=sdm.EmpInfoSystemID)as IncomeTaxId,
                    sd.DefineAmount,(sd.DefineAmount*12)StructureVal,
				sd.SalaryHeadID,tem.Id as EarningMasterId,sh.SalaryHead,(select '0')as OpeningValue,
				(select '0') as ActualValue,(select '0') as ArrearValue,
				(select '0') as ArrearValue
				from SalaryInfoDefineMaster sdm join SalaryInfoDefine sd on 
				sd.SalaryID=sdm.SystemID
				join SalaryHead sh on sh.SalaryHeadID=sd.SalaryHeadID
				join TaxEarningMasterChild tem on tem.SalaryHeadId=sh.SalaryHeadID				
				where EmpInfoSystemID IN("+EmpId+@")  and sh.HeadType='E'
				and tem.TaxPolicyHeaderId='"+PolicyId+"'";
                
                }
                
                return _sqlRepository.GetDataTable(sql);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }   
        public void DeleteExistingEarningData(string EmpId,string PolicyId,string YearId,string TypeId)
        {
            ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");

            DataSet dsDetail;
            var sql = @"select Id from EmployeeIncomeTaxMaster where TaxYearId='"+YearId+@"'
            and TaxPolicyHeaderId='"+PolicyId+"' and TaxTypeId='"+TypeId+"' and EmpSystemId in("+EmpId+")";
            objCon.OpenDataSetThroughAdapter(sql, out dsDetail, false, "1");

            string IncomeTaxId = "''";
            for(int j=0;j<dsDetail.Tables[0].Rows.Count;j++)
            {
                IncomeTaxId += ",'" + dsDetail.Tables[0].Rows[j][@"Id"].ToString() + "'";
            }
            
            ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
            con.BeginTransaction();
            var sqlx = @"delete from EmployeeEarningData where EmployeeIncomeTaxId in(" + IncomeTaxId + ")";
            con.executeQuery(sqlx);
            con.CommitTransaction();
            
        }
        #endregion

    }
}

