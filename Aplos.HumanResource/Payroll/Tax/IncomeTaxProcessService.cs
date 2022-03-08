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

        public void ProcessIncomeTax(string PolicyId, string YearId,string EmpId,string TaxTypeId,string StartDate,string EndDate)
        {
            try
            {
                #region IncomeTaxMaster Data Generation

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataTable GrossEarningDt, DeductionDt, TaxableIncomeDt;
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
                    DeleteExistingData(EmpId, PolicyId, YearId, TaxTypeId);

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
                            if (CalculatedDict.ContainsKey(IncomeTaxId + "/" + SalaryHeadId))
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

                #region Deductions Saving

                DeductionDt = DeductionQuery(PolicyId, EmpId);

                sqlx = @"select * from employeeinvestmentdeduction where 1=2";
                con.OpenDataSetThroughAdapter(sqlx, out dsRef, false, "1");
                if (DeductionDt.Rows.Count > 0)
                {
                    for (int x = 0; x < DeductionDt.Rows.Count; x++)
                    {
                        string ActualValue = DeductionDt.Rows[x][@"ActualValue"].ToString();
                        string UserValue = DeductionDt.Rows[x][@"UserValue"].ToString();
                        string IncomeTaxItemChildId = DeductionDt.Rows[x][@"IncomeTaxItemChildId"].ToString();
                        string IncomeTaxId = DeductionDt.Rows[x][@"IncomeTaxId"].ToString();

                        DataRow drF = dsRef.Tables[0].NewRow();
                        clsGenID genid = new clsGenID();
                        genid.GenID("EmployeeInvestmentDeduction", out string _pk);

                        drF["Id"] = "EID" + _pk;
                        drF["EmployeeIncomeTaxId"] = IncomeTaxId;
                        drF["ActualValue"] = ActualValue;
                        drF["UserValue"] = UserValue;
                        drF["IncomeTaxItemChildId"] = IncomeTaxItemChildId;
                        drF["AddedBy"] = identity.Name;
                        drF["AddedFromIp"] = identity.IPAddress;
                        drF["AddedDate"] = DateTime.Now.ToString();

                        dsRef.Tables[0].Rows.Add(drF);

                    }
                    _info.SaveDataSets(dsRef);
                }

                #endregion

                #region Taxable Income Saving

                TaxableIncomeDt = TaxableIncomeQuery(PolicyId, EmpId, YearId);

                sqlx = @"select * from TaxableIncome where 1=2";
                con.OpenDataSetThroughAdapter(sqlx, out dsRef, false, "1");

                if (TaxableIncomeDt.Rows.Count > 0)
                {
                    for (int x = 0; x < TaxableIncomeDt.Rows.Count; x++)
                    {
                        string IncomeTaxId = TaxableIncomeDt.Rows[x][@"IncomeTaxId"].ToString();
                        string NetEarning = TaxableIncomeDt.Rows[x][@"NetEarning"].ToString();
                        string Investments = TaxableIncomeDt.Rows[x][@"Investments"].ToString();
                        double TaxableIncome = clsStaticInfo.dbl(NetEarning) - clsStaticInfo.dbl(Investments);


                        DataRow drF = dsRef.Tables[0].NewRow();
                        clsGenID genid = new clsGenID();
                        genid.GenID("TaxableIncome", out string _Id);

                        drF["Id"] = "NTI" + _Id;
                        drF["EmployeeIncomeTaxId"] = IncomeTaxId;
                        drF["NetEarning"] = NetEarning;
                        drF["TaxableIncome"] = TaxableIncome;
                        drF["Investments"] = Investments;
                        drF["AddedBy"] = identity.Name;
                        drF["AddedFromIp"] = identity.IPAddress;
                        drF["AddedDate"] = DateTime.Now.ToString();
                        dsRef.Tables[0].Rows.Add(drF);

                    }
                    _info.SaveDataSets(dsRef);
                }

                #endregion

                #region Tax Slab Saving
                DataTable EmpTaxableIncomeDt, TaxSlabRatesDt;

                StringCollection StrDistinctIncomeTaxId = new StringCollection();

                var sqlquery = @"select * from EmployeeNetTax where 1=2";
                con.OpenDataSetThroughAdapter(sqlquery, out dsRef, false, "1");

                EmpTaxableIncomeDt = eis.TaxableGridData(EmpId, PolicyId, YearId);
                TaxSlabRatesDt = GetSlabQuery(PolicyId, EmpId);

                if (TaxSlabRatesDt.Rows.Count > 0)
                {
                    double Income = 0, Amt, TotalAmt = 0;
                    for (int j = 0; j < TaxSlabRatesDt.Rows.Count; j++)
                    {

                        string IncomeTaxId = TaxSlabRatesDt.Rows[j][@"IncomeTaxId"].ToString();
                        double Range = clsStaticInfo.dbl(TaxSlabRatesDt.Rows[j]["Range"].ToString());
                        int TaxPercent = Convert.ToInt32(TaxSlabRatesDt.Rows[j]["TaxRate"].ToString());
                        string SlabId = clsWebLib.RetValidLen(TaxSlabRatesDt.Rows[j]["SlabId"].ToString()).ToString();

                        var IncomeRow = EmpTaxableIncomeDt.Rows
                              .Cast<DataRow>()
                              .Where(x => x["IncomeTaxId"].ToString() == IncomeTaxId).ToList();

                        #region For Distinct Employee Income

                        if (StrDistinctIncomeTaxId.Contains(IncomeTaxId))
                        {

                        }
                        else
                        {
                            StrDistinctIncomeTaxId.Add(IncomeTaxId);
                            Income = 0; Amt = 0; TotalAmt = 0;
                            Income = clsStaticInfo.dbl(IncomeRow[0][@"taxableIncome"].ToString());
                        }

                        #endregion

                        if ((Income - TotalAmt) >= Range)
                        {
                            Amt = Range;
                            TotalAmt += Range;
                        }
                        else
                        {
                            Amt = Income - TotalAmt;
                            TotalAmt += Amt;
                        }
                        if (Amt > 0)
                        {
                            DataRow drF = dsRef.Tables[0].NewRow();
                            clsGenID genid = new clsGenID();
                            genid.GenID("EmployeeNetTax", out string _Id);

                            drF["Id"] = "ENT" + _Id;
                            drF["EmployeeIncomeTaxId"] = IncomeTaxId;
                            drF["SlabId"] = SlabId;
                            drF["DistributedAmt"] = Amt;
                            drF["TaxPercentage"] = TaxPercent;
                            drF["TaxAmt"] = (Amt * TaxPercent) / 100;
                            drF["AddedBy"] = identity.Name;
                            drF["AddedFromIp"] = identity.IPAddress;
                            drF["AddedDate"] = DateTime.Now.ToString();

                            dsRef.Tables[0].Rows.Add(drF);

                        }
                    }
                    _info.SaveDataSets(dsRef);
                }

                #endregion

                #region Tax After Rebate

                #region DataSet Region

                DataTable TaxRebateMaster, EstimatedTaxDt;
                StringCollection IncomeTaxIdCollection = new StringCollection();
                StringCollection SavingChecker = new StringCollection();

                sqlx = @"select em.EmpsystemId,em.Id as IncomeTaxId,em.TaxPolicyHeaderId,
                TaxableIncome,SUM(TaxAmt) as EstimatedTax
                from taxableincome t left join EmployeeIncomeTaxMaster em on
                em.Id=t.EmployeeIncomeTaxId
				left join EmployeeNetTax net on net.EmployeeIncomeTaxId=em.Id
                where Em.empsystemid in (" + EmpId + @") 
				and em.TaxPolicyHeaderId='" + PolicyId + @"'
				GROUP BY em.EmpsystemId,em.Id,em.TaxPolicyHeaderId,TaxableIncome";
                EstimatedTaxDt = _sqlRepository.GetDataTable(sqlx);

                var sqlz = @"select * from TaxAfterRebate where 1=2";
                con.OpenDataSetThroughAdapter(sqlz, out dsRef, false, "1");

                sqly = @"select ei.EmpSystemId,ei.Id as IncomeTaxId,
			   (select max(Maximum) from TaxRebateConfiguration where 
                TaxPolicyId='" + PolicyId + @"')AS FinalMax,
			    t.TaxPolicyId,t.Minimum,t.Maximum,t.IsFix,t.IsPercentage,t.Value
                from TaxRebateConfiguration t left join TaxPolicyHeader th on
                th.Id=t.TaxPolicyId
                left join EmployeeIncomeTaxMaster ei on ei.TaxPolicyHeaderId=th.Id 
                where TaxPolicyId='" + PolicyId + @"'
                and ei.EmpSystemId in (" + EmpId + @")               
				order by ei.EmpSystemId,ei.Id";
                TaxRebateMaster = _sqlRepository.GetDataTable(sqly);

                #endregion

                #region Processing Region

                if (TaxRebateMaster.Rows.Count > 0)
                {
                    double TaxableIncome = 0, EstimatedTax = 0, RebateAmt = 0;
                    for (int j = 0; j < TaxRebateMaster.Rows.Count; j++)
                    {
                        double Minimum = clsStaticInfo.dbl(TaxRebateMaster.Rows[j]["Minimum"].ToString());
                        double Maximum = clsStaticInfo.dbl(TaxRebateMaster.Rows[j]["Maximum"].ToString());
                        double Value = clsStaticInfo.dbl(TaxRebateMaster.Rows[j]["Value"].ToString());
                        string IsFix = clsWebLib.GetBoolData(TaxRebateMaster.Rows[j]["IsFix"]).ToString();
                        string IsPercent = clsWebLib.GetBoolData(TaxRebateMaster.Rows[j]["IsPercentage"]).ToString();
                        string IncomeTaxId = TaxRebateMaster.Rows[j][@"IncomeTaxId"].ToString();
                        double FinalMax = clsStaticInfo.dbl(TaxRebateMaster.Rows[j]["FinalMax"].ToString());

                        var EstimatedTaxRow = EstimatedTaxDt.Rows
                            .Cast<DataRow>()
                            .Where(x => x["IncomeTaxId"].ToString() == IncomeTaxId).ToList();

                        #region For Distinct Employee Income

                        if (IncomeTaxIdCollection.Contains(IncomeTaxId))
                        {

                        }
                        else
                        {
                            IncomeTaxIdCollection.Add(IncomeTaxId);

                            TaxableIncome = 0; EstimatedTax = 0; RebateAmt = 0;
                            TaxableIncome = clsStaticInfo.dbl(EstimatedTaxRow[0][@"taxableIncome"].ToString());
                            EstimatedTax = clsStaticInfo.dbl(EstimatedTaxRow[0][@"EstimatedTax"].ToString());
                        }

                        #endregion

                        if (Minimum <= TaxableIncome && Maximum >= TaxableIncome)
                        {
                            if (IsFix == "True")
                            {
                                RebateAmt = EstimatedTax;
                            }
                            else if (IsPercent == "True")
                            {
                                RebateAmt = (EstimatedTax * Value) / 100;
                            }

                            double NetTax = EstimatedTax - RebateAmt;

                            DataRow drF = dsRef.Tables[0].NewRow();
                            clsGenID genid = new clsGenID();
                            genid.GenID("TaxAfterRebate", out string _Id);
                            drF["Id"] = "TR" + _Id;
                            drF["EmployeeIncomeTaxId"] = IncomeTaxId;
                            drF["EstimatedTax"] = EstimatedTax;
                            drF["TaxRebate"] = RebateAmt;
                            drF["TaxAfterRebate"] = NetTax;
                            drF["AddedBy"] = identity.Name;
                            drF["AddedFromIp"] = identity.IPAddress;
                            drF["AddedDate"] = DateTime.Now.ToString();
                            dsRef.Tables[0].Rows.Add(drF);

                        }
                        else if (TaxableIncome > FinalMax)
                        {
                            if (SavingChecker.Contains(IncomeTaxId))
                            {
                                // Skip
                            }
                            else
                            {
                                SavingChecker.Add(IncomeTaxId);

                                double NetTax = EstimatedTax - RebateAmt;
                                DataRow drF = dsRef.Tables[0].NewRow();
                                clsGenID genid = new clsGenID();
                                genid.GenID("TaxAfterRebate", out string _Id);
                                drF["Id"] = "TR" + _Id;
                                drF["EmployeeIncomeTaxId"] = IncomeTaxId;
                                drF["EstimatedTax"] = EstimatedTax;
                                drF["TaxRebate"] = RebateAmt;
                                drF["TaxAfterRebate"] = NetTax;
                                drF["AddedBy"] = identity.Name;
                                drF["AddedFromIp"] = identity.IPAddress;
                                drF["AddedDate"] = DateTime.Now.ToString();
                                dsRef.Tables[0].Rows.Add(drF);
                            }

                        }
                    }
                    _info.SaveDataSets(dsRef);
                }

                #endregion

                #endregion

                #region Additional Charges

                #region DataSet Region

                StringCollection TaxRebateIdCollection = new StringCollection();
                DataTable AdditionalTaxDt, TaxAfterRebateDt;

                Dictionary<string, List<AdditionalTaxCalculationsList>> AddtnTaxDict =
                  new Dictionary<string, List<AdditionalTaxCalculationsList>>();


                sql = @"select ei.EmpSystemId,ei.Id as IncomeTaxId,ei.TaxYearId,ei.TaxPolicyHeaderId,t.TaxAfterRebate
				from TaxAfterRebate t left join 
				EmployeeIncomeTaxMaster ei on ei.Id=t.EmployeeIncomeTaxId
				where ei.EmpSystemId in(" + EmpId + ") and ei.TaxPolicyHeaderId='" + PolicyId + "'";
                TaxAfterRebateDt = _sqlRepository.GetDataTable(sql);

                sqlx = @"Select ei.EmpSystemId,ei.Id as IncomeTaxId,am.TaxPolicyId,am.UserName,
                am.IsFix,am.IsPercentage,am.Value
                from AdditionalTaxMaster AM LEFT JOIN taxpolicyheader th 
                on th.Id=AM.taxpolicyId
				left join EmployeeIncomeTaxMaster ei on ei.TaxPolicyHeaderId=th.Id
                where am.TaxPolicyId ='" + PolicyId + "' and ei.EmpSystemId in(" + EmpId + ")" +
                "order by ei.EmpSystemId";
                AdditionalTaxDt = _sqlRepository.GetDataTable(sqlx);

                sqlz = @"select * from TaxAfterAdditionalCharges where 1=2";
                con.OpenDataSetThroughAdapter(sqlz, out dsRef, false, "1");

                #endregion

                #region Processing Region

                double AdditionalTaxAmt = 0, TaxAfterRebate = 0;
                if (AdditionalTaxDt.Rows.Count > 0)
                {
                    for (int j = 0; j < AdditionalTaxDt.Rows.Count; j++)
                    {
                        double Value = clsStaticInfo.dbl(AdditionalTaxDt.Rows[j]["Value"].ToString());
                        string IsFix = clsWebLib.GetBoolData(AdditionalTaxDt.Rows[j]["IsFix"]).ToString();
                        string IsPercent = clsWebLib.GetBoolData(AdditionalTaxDt.Rows[j]["IsPercentage"]).ToString();
                        string IncomeTaxId = clsWebLib.RetValidLen(AdditionalTaxDt.Rows[j]["IncomeTaxId"].ToString()).ToString();

                        var TaxAfterRebateRow = TaxAfterRebateDt.Rows
                        .Cast<DataRow>()
                        .Where(x => x["IncomeTaxId"].ToString() == IncomeTaxId).ToList();

                        #region For Distinct Employee Income

                        if (TaxRebateIdCollection.Contains(IncomeTaxId))
                        {

                        }
                        else
                        {
                            TaxRebateIdCollection.Add(IncomeTaxId);
                            TaxAfterRebate = 0; AdditionalTaxAmt = 0;
                            TaxAfterRebate = clsStaticInfo.dbl(TaxAfterRebateRow[0][@"TaxAfterRebate"].ToString());
                        }

                        #endregion

                        if (TaxAfterRebate > 0)
                        {
                            AdditionalTaxAmt = 0;
                            if (IsFix == "True")
                            {
                                AdditionalTaxAmt = AdditionalTaxAmt + Value;
                            }
                            else if (IsPercent == "True")
                            {
                                AdditionalTaxAmt = AdditionalTaxAmt + (TaxAfterRebate * Value) / 100;
                            }
                        }

                        if (AddtnTaxDict.ContainsKey(IncomeTaxId))
                        {
                            AddtnTaxDict[IncomeTaxId].Add(new AdditionalTaxCalculationsList
                            {
                                TaxAfterRebate = TaxAfterRebate,
                                AddtnTax = AdditionalTaxAmt,
                                EmployeeIncomeTaxId = IncomeTaxId
                            });
                        }
                        else
                        {
                            var datax = new List<AdditionalTaxCalculationsList>();
                            datax.Add(new AdditionalTaxCalculationsList
                            {
                                TaxAfterRebate = TaxAfterRebate,
                                AddtnTax = AdditionalTaxAmt,
                                EmployeeIncomeTaxId = IncomeTaxId
                            });
                            AddtnTaxDict.Add(IncomeTaxId, datax);
                        }

                    }

                    foreach (var item in AddtnTaxDict)
                    {
                        List<AdditionalTaxCalculationsList> data = item.Value;
                        if (data == null)
                        {
                            continue;
                        }
                        string IncomeTaxId = data[0].EmployeeIncomeTaxId;
                        double NetAdditionalAmt = 0, RebateTax = 0;
                        NetAdditionalAmt = data.Sum(x => x.AddtnTax);
                        RebateTax = data.Max(x => x.TaxAfterRebate);

                        // Saving Part
                        DataRow drF = dsRef.Tables[0].NewRow();
                        clsGenID genid = new clsGenID();
                        genid.GenID("TaxAfterAdditionalCharges", out string _Id);
                        drF["Id"] = "AC" + _Id;
                        drF["EmployeeIncomeTaxId"] = IncomeTaxId;
                        drF["TaxAfterRebate"] = RebateTax;
                        drF["AdditionalTax"] = NetAdditionalAmt;
                        drF["NetTax"] = RebateTax + NetAdditionalAmt;
                        drF["AddedBy"] = identity.Name;
                        drF["AddedFromIp"] = identity.IPAddress;
                        drF["AddedDate"] = DateTime.Now.ToString();
                        dsRef.Tables[0].Rows.Add(drF);
                    }

                    _info.SaveDataSets(dsRef);

                }

                #endregion

                #endregion

                #region Tax After Surcharge

                #region DataSet region

                DataTable SurchargeDt;
                StringCollection SurchargeIdCollection = new StringCollection();

                sqlx = @"select em.EmpsystemId,em.Id as IncomeTaxId,em.TaxPolicyHeaderId,
                TaxableIncome,NET.NetTax as EstimatedTax
                from taxableincome t left join EmployeeIncomeTaxMaster em on
                em.Id=t.EmployeeIncomeTaxId
				left join TaxAfterAdditionalCharges net on net.EmployeeIncomeTaxId=em.Id
                where Em.empsystemid in ("+EmpId+@") 
				and em.TaxPolicyHeaderId='"+PolicyId+"'";
                EstimatedTaxDt = _sqlRepository.GetDataTable(sqlx);

                sqlz = @"select * from TaxAfterSurcharge where 1=2";
                con.OpenDataSetThroughAdapter(sqlz, out dsRef, false, "1");

                sqly = @"select ei.EmpSystemId,ei.Id as IncomeTaxId,
			    t.TaxPolicyId,t.Minimum,t.Maximum,t.IsFix,t.IsPercentage,t.Value
                from TaxSurChargeConfiguration t left join TaxPolicyHeader th on
                th.Id=t.TaxPolicyId
                left join EmployeeIncomeTaxMaster ei on ei.TaxPolicyHeaderId=th.Id 
                where TaxPolicyId='" + PolicyId + @"'
                and ei.EmpSystemId in (" + EmpId + @")               
				order by ei.EmpSystemId,ei.Id,t.Id";
                SurchargeDt = _sqlRepository.GetDataTable(sqly);

                #endregion

                #region Processing Part

                if (SurchargeDt.Rows.Count > 0)
                {
                    double TaxableIncome = 0, EstimatedTax = 0, SurchargeAmt=0;
                    for (int j = 0; j < SurchargeDt.Rows.Count; j++)
                    {
                        double Minimum = clsStaticInfo.dbl(SurchargeDt.Rows[j]["Minimum"].ToString());
                        double Maximum = clsStaticInfo.dbl(SurchargeDt.Rows[j]["Maximum"].ToString());
                        double Value = clsStaticInfo.dbl(SurchargeDt.Rows[j]["Value"].ToString());
                        string IsFix = clsWebLib.GetBoolData(SurchargeDt.Rows[j]["IsFix"]).ToString();
                        string IsPercent = clsWebLib.GetBoolData(SurchargeDt.Rows[j]["IsPercentage"]).ToString();
                        string IncomeTaxId = SurchargeDt.Rows[j][@"IncomeTaxId"].ToString();

                        var EstimatedSurchargeRow = EstimatedTaxDt.Rows
                            .Cast<DataRow>()
                            .Where(x => x["IncomeTaxId"].ToString() == IncomeTaxId).ToList();

                        #region For Distinct Employee Income

                        if (SurchargeIdCollection.Contains(IncomeTaxId))
                        {

                        }
                        else
                        {
                            SurchargeIdCollection.Add(IncomeTaxId);

                            TaxableIncome = 0; EstimatedTax = 0; SurchargeAmt=0;
                            TaxableIncome = clsStaticInfo.dbl(EstimatedSurchargeRow[0][@"taxableIncome"].ToString());
                            EstimatedTax = clsStaticInfo.dbl(EstimatedSurchargeRow[0][@"EstimatedTax"].ToString());
                        }

                        #endregion
                      
                        if (Minimum <= TaxableIncome && Maximum >= TaxableIncome)
                        {
                            if (IsFix == "True")
                            {
                                SurchargeAmt = Value;
                            }
                            else if (IsPercent == "True")
                            {
                                SurchargeAmt = (EstimatedTax * Value) / 100;
                            }
                        }

                        double NetTax = EstimatedTax + SurchargeAmt;

                        DataRow drF = dsRef.Tables[0].NewRow();
                        clsGenID genid = new clsGenID();
                        genid.GenID("TaxAfterSurcharge", out string _Id);
                        drF["Id"] = "TS" + _Id;
                        drF["EmployeeIncomeTaxId"] = IncomeTaxId;
                        drF["EstimatedTax"] = EstimatedTax;
                        drF["TaxSurcharge"] = SurchargeAmt;
                        drF["NetTax"] = NetTax;
                        drF["AddedBy"] = identity.Name;
                        drF["AddedFromIp"] = identity.IPAddress;
                        drF["AddedDate"] = DateTime.Now.ToString();
                        dsRef.Tables[0].Rows.Add(drF);
                    }
                    _info.SaveDataSets(dsRef);
                }

                #endregion

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
        public DataTable DeductionQuery(string PolicyId,string EmpId)
        {
            try
            {
                string sql = @"select Masterx.Id as IncomeTaxId,itc.Limit as TaxSavingItemLimit,ti.UserName as TaxSavingItem,itc.TaxSavingItemId,
                    it.TaxSavingGroupId,tg.UserName as TaxSavingGroup,tg.MaxLimit as SavingGpLimit,
                    itc.DocumentApplicable,	
                    ActualValue=case when itc.isuserdefined=1 then (select itc.Limit)
					else (select '0') end,
					UserValue=case when itc.isuserdefined=1 then (select itc.Limit)
					else (select '0') end,
                    itc.Id as IncomeTaxItemChildId
                    from IncomeTaxItemChild itc left join IncomeTaxItemMaster it on 
                    it.SystemId=itc.IncomeTaxItemMasterId
                    left join hkp.TaxSavingItem ti on ti.Id=itc.TaxSavingItemId
                    left join hkp.TaxSavingGroup tg on tg.Id=it.TaxSavingGroupId	
					left join (
					select eit.Id,eit.TaxPolicyHeaderId
					from EmployeeIncomeTaxMaster eit 
					left join TaxPolicyHeader th on th.Id=
					eit.TaxPolicyHeaderId 
					where eit.EmpSystemId in("+EmpId+@")
					) AS Masterx On Masterx.TaxPolicyHeaderId=it.TaxPolicyHeaderId
                    where it.TaxPolicyHeaderId='"+PolicyId+@"'
					order by Masterx.Id";               
                    return _sqlRepository.GetDataTable(sql);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public DataTable TaxableIncomeQuery(string PolicyId, string EmpId,string YearId)
        {
            try
            {
                string sql = @"select ei.EmpSystemId,ei.Id as IncomeTaxId,
				SUM(case when 
				(GrossEarning-isnull(ed.ExemptionAmt,'0')) < 0 THEN GrossEarning
				else (GrossEarning-isnull(ed.ExemptionAmt,'0')) end)as NetEarning,Masterx.Investments
                from EmployeeEarningData ed 
                left join employeeincometaxmaster ei on ei.Id=ed.EmployeeIncomeTaxId
				left join (
				select edx.EmployeeIncomeTaxId,SUM(UserValue) as Investments from 
				EmployeeInvestmentDeduction edx left join EmployeeIncomeTaxMaster
				em on em.id=edx.EmployeeIncomeTaxId
				where em.EmpSystemId in("+EmpId+@")
				group by EmployeeIncomeTaxId
				) as Masterx on Masterx.EmployeeIncomeTaxId=ei.Id
                where ei.EmpSystemId in("+EmpId+@") and ei.TaxYearId='"+YearId+@"'
                and ei.TaxPolicyHeaderId='"+PolicyId+@"'
				GROUP BY ei.EmpSystemId,ei.Id,Masterx.Investments";
                return _sqlRepository.GetDataTable(sql);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        public void DeleteExistingData(string EmpId,string PolicyId,string YearId,string TypeId)
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
            var sqlx = @"delete from EmployeeEarningData where EmployeeIncomeTaxId 
            in(" + IncomeTaxId + ")";
           
            sqlx+= Environment.NewLine + @"delete from employeeinvestmentdeduction 
            where 
            EmployeeIncomeTaxId in(" + IncomeTaxId + ")";
           
            sqlx += Environment.NewLine + @"delete from TaxAfterRebate where 
            EmployeeIncomeTaxId in(" + IncomeTaxId + ")";

            sqlx += Environment.NewLine + @"delete from TaxAfterSurcharge where 
            EmployeeIncomeTaxId in(" + IncomeTaxId + ")";

            sqlx += Environment.NewLine + @"delete from TaxAfterAdditionalCharges
            where EmployeeIncomeTaxId in(" + IncomeTaxId + ")";

            sqlx += Environment.NewLine + @"delete from TaxableIncome where 
            EmployeeIncomeTaxId in(" + IncomeTaxId + ")";

            sqlx += Environment.NewLine + @"delete from EmployeeNetTax where 
            EmployeeIncomeTaxId in(" + IncomeTaxId + ")";
            con.executeQuery(sqlx); 
            con.CommitTransaction();
            
        }
        public DataTable GetSlabQuery(string PolicyId,string EmpId)
        {
            try
            {
                string strSQL = @"select si.Id AS SlabId,si.PolicyId,si.Minimum,si.Maximum,
                si.TaxRate,si.DifferenceAmt as Range,Masterx.Id as IncomeTaxId
				from TaxPolicySlabInfo si 
                left join TaxPolicyHeader th on th.Id=si.PolicyId
				left join (
				select eit.Id,eit.TaxPolicyHeaderId from EmployeeIncomeTaxMaster eit left join
				TaxPolicyHeader th on th.Id=eit.TaxPolicyHeaderId
				where eit.EmpSystemId In("+EmpId+@")
				)as masterx on masterx.TaxPolicyHeaderId=th.Id
                where th.Id='"+PolicyId+ @"'
				order by Masterx.Id,SlabId";
                return _sqlRepository.GetDataTable(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        #endregion

    }

    public class AdditionalTaxCalculationsList
    {
        public string EmployeeIncomeTaxId { get; set; }
        public double TaxAfterRebate { get; set; }
        public double AddtnTax { get; set; }

    }
}

