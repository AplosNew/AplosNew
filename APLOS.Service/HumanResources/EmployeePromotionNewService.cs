using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Service.Enums;
using Library.Service.Logs;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using Library.Model.Employees;

namespace Library.Service.HumanResources
{
    public class EmployeePromotionNewService
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        public EmployeePromotionNewService(ISqlRepository sqlRepository)
        {
            this._sqlRepository = sqlRepository;
        }
        #endregion Constructor






        public void abc()
        {
            Dictionary<string, DataSet> listdata;
            abc(out listdata);

            DataSet dsFromFucnction = (DataSet)listdata["DS2"];


        }


        public void abc(out Dictionary<string, DataSet> listdata)
        {
            listdata = new Dictionary<string, DataSet>();

            DataSet ds = new DataSet(); listdata.Add("DS1", ds);
            DataSet ds2 = new DataSet(); listdata.Add("DS2", ds2);
        }

        public void LoadEmpSalaryInfoDefineData(string EmpSystemId,
            out IEnumerable<EmpSalaryInfoDefineModelNew> EmpSalaryInfoDefine,
            out IEnumerable<EmpSalaryInfoDefineModelNew> EmpApprovedSalaryInfoDefine,
            //out IEnumerable<object> EmpApprovedSalaryInfoDefine,
            out IEnumerable<SalaryRuleModelNew> ResultSalaryRule,
            out IEnumerable<SalaryRuleModelNew> ResultSelectedSalaryRule,
            out IEnumerable<OpenHeadModelNew> ResultOpenHead,
            out IEnumerable<OpenHeadModelNew> ResultApprovedOpenHead,
            out CustomOutParaNew outPara, out bool IsFreshEntry)
        {
            string NewFormula_Desc = string.Empty;
            string ApprovedFormula_Desc = string.Empty;
            string ResultMinWage = string.Empty;
            string ResultGross = string.Empty;
            string ResultNetCTC = string.Empty;
            bool IsSalaryRuleEditableEmployee = false;
            string ApprovalStatus = string.Empty;
            string ApprovedEffectiveDate = string.Empty;
            string ApprovedNextDueDate = string.Empty;
            string ResultEffectiveDate = string.Empty;
            outPara = new CustomOutParaNew();
            IsFreshEntry = false;

            DataSet dsPayRollGroup = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GetPayRollGroupId(EmpSystemId, out dsPayRollGroup);


            if (identity.IsSysAdmin == false)
            {
                if (dsPayRollGroup.Tables[0].Rows.Count > 0)
                {
                    if (!CheckEditableEmpByPayRollGroup(dsPayRollGroup.Tables[0].Rows[0]["Payrollgroupid"].ToString()))
                    {
                        Exception ex = new Exception("Invalid User for this Payroll Group.");
                        throw (ex);
                    }
                }
                else
                {
                    if (!CheckEditableEmpByPayRollGroup(""))
                    {
                        Exception ex = new Exception("Invalid User for this Payroll Group.");
                        throw (ex);
                    }
                }
            }


            clsSalaryStructureAplosNew ob = new clsSalaryStructureAplosNew();
            DataSet dsbasicInfo = null;

            GetSysIdWiseEmpBasicInfoInformation(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, EmpSystemId, out dsbasicInfo);


            clsSalaryInfoNew obs = null;
            DataSet dsRule = null;
            DataSet dsIsapproved = null;
            DataSet dsSalaryRuleEditableEmployee = null;

            DateTime _EffectiveDate = GetEffectiveDate(EmpSystemId);
            obs = new clsSalaryInfoNew();
            obs.LoadLatestSalaryStructure(identity.CompanyGroupId, identity.PlantId, EmpSystemId, out dsRule);

            var EffectiveDate = _EffectiveDate.ToString("dd-MMM-yyyy");
            CustomParaNew o = new CustomParaNew();
            o.PlantId = identity.PlantId;
            o.EffectiveDate = EffectiveDate;
            o.CompanyId = identity.CompanyId;
            o.CompanyGroupId = identity.CompanyGroupId;
            o.EmployeeId = EmpSystemId;
            o.User = identity.Name;



            DataSet dsIsFreshEntry = null;
            obs.CheckFreshEntry(identity.PlantId, EmpSystemId, out dsIsFreshEntry);




            if (dsbasicInfo.Tables[0].Rows.Count > 0)
            {
                if (string.IsNullOrEmpty(dsbasicInfo.Tables[0].Rows[0]["SalaryRuleMasterSystemID"].ToString().Trim()))
                {
                    DataSet dsSalaryRuleId = GivenDesignationChange(dsbasicInfo.Tables[0].Rows[0]["GivenDesignationId"].ToString().Trim(), identity.CompanyGroupId, identity.PlantId);
                    if (dsSalaryRuleId.Tables[0].Rows.Count > 0)
                    {
                        o.SalaryRuleId = dsSalaryRuleId.Tables[0].Rows[0]["SalaryRuleMasterId"].ToString().Trim();
                        o.IsFreshEntry = true;
                    }
                    else
                    {
                        o.IsFreshEntry = true;
                    }
                }
                else
                {
                    //o.SalaryRuleId = dsbasicInfo.Tables[0].Rows[0]["SalaryRuleMasterSystemID"].ToString().Trim();
                    DataSet dsSalaryRuleId = GivenDesignationChange(dsbasicInfo.Tables[0].Rows[0]["GivenDesignationId"].ToString().Trim(), identity.CompanyGroupId, identity.PlantId);
                    if (dsSalaryRuleId.Tables[0].Rows.Count>0)
                    {
                        o.SalaryRuleId = dsSalaryRuleId.Tables[0].Rows[0]["SalaryRuleMasterId"].ToString().Trim();
                    }
                    else
                    {
                        throw new CustomException("Salary Rule doesn't define in Designation Master Configuration for this Designation.");
                    }

                    if (dsIsFreshEntry.Tables[0].Rows.Count == 0)
                    {
                        o.IsFreshEntry = true;
                    }

                }

            }

            DataSet dsLocalcur = null;
            obs.GetLocalCurrency(identity.CompanyGroupId, identity.PlantId, out dsLocalcur);
            if (dsLocalcur.Tables[0].Rows.Count > 0)
            {
                //_para.cu = "" + dsLocal.Tables[0].Rows[0]["Currency"].ToString().Trim();
                o.LocalCurrencyId = "" + dsLocalcur.Tables[0].Rows[0]["LocalCurrency"].ToString().Trim();
            }

            DataTable dsLocal = null;
            DataTable dtApprovedSalaryDifn = null;
            DataSet dsApprovedOpenHead = null;
            DataSet dsMinWage = null;
            DataSet dsSalaryRule = null;
            DataSet dsSelectedSalaryRule = null;
            DataSet dsOpenHead = null;
            CustomParaAdditionalPolicySetting outCustomParaAdditionalPolicySetting = new CustomParaAdditionalPolicySetting();
            try
            {
                ob.StartProcess(o, out dsLocal, out dtApprovedSalaryDifn, out dsMinWage, out dsSalaryRule, out dsSelectedSalaryRule, out dsOpenHead, out dsApprovedOpenHead, out NewFormula_Desc, out ApprovedFormula_Desc,out outCustomParaAdditionalPolicySetting);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            #region GetEmployeeSalaryRuleEditable

            ob.GetEmployeeSalaryRuleEditable(identity.PlantId, EmpSystemId, out dsSalaryRuleEditableEmployee);
            IsSalaryRuleEditableEmployee = (dsSalaryRuleEditableEmployee.Tables[0].Rows.Count > 0 ? true : false);
            #endregion

            #region Net Gross and Net CTC
            //Net Gross and Net CTC
            DataView dv = new DataView();
            decimal decNetGross = 0;
            decimal decNetCTC = 0;
            ResultGross = string.Empty;
            ResultNetCTC = decNetCTC.ToString("#,##0.00;(#,##0.00)");
            if (dtApprovedSalaryDifn != null && dtApprovedSalaryDifn.Rows.Count > 0)
            {

                dv.Table = dtApprovedSalaryDifn;
                //dv.Sort = "HeadType DESC, SalaryHead";
                dv.Sort = "SequenceNo";
                var dg = dv.ToTable();


                //for (int i = 0; i < dg.Rows.Count; i++)
                //{
                //    if (dg.Rows[i]["HeadType"].ToString() == "Earning")
                //    {
                //        //if (Convert.ToBoolean(bplib.clsWebLib.GetBoolData(dg.Rows[i]["IsGrossComponent"].ToString())) == true)
                //        //{ decNetGross += Convert.ToDecimal(bplib.clsWebLib.GetNumData(dg.Rows[i]["EntryAmount"].ToString().Trim())); }
                //        //if (Convert.ToBoolean(bplib.clsWebLib.GetBoolData(dg.Rows[i]["IsCTCComponent"].ToString())) == true)
                //        //{ decNetCTC += Convert.ToDecimal(bplib.clsWebLib.GetNumData(dg.Rows[i]["EntryAmount"].ToString().Trim())); }
                //    }
                //}
                ResultGross = decNetGross.ToString("#,##0.00;(#,##0.00)");
                ResultNetCTC = decNetCTC.ToString("#,##0.00;(#,##0.00)");
            }





            #endregion Net Gross and Net CTC

            #region open head
            //open head
            ResultOpenHead = ConvertOpenHeadToList(dsOpenHead);

            #endregion open head

            #region all Salary Rule
            //SalaryRule
            ResultSalaryRule = dsSalaryRule.Tables[0].AsEnumerable().Select(
                dataRow => new SalaryRuleModelNew
                {
                    SalaryRuleMasterSystemID = dataRow.Field<string>("SalaryRuleMasterSystemID"),
                    SalaryRuleName = dataRow.Field<string>("SalaryRuleName"),
                    SalaryRuleDescription = dataRow.Field<string>("SalaryRuleDescription")
                }).ToList();
            #endregion Salary Rule

            #region Selected Salary Rule
            //SelectedSalaryRule
            ResultSelectedSalaryRule = dsSelectedSalaryRule.Tables[0].AsEnumerable().Select(
               dataRow => new SalaryRuleModelNew
               {
                   SalaryRuleMasterSystemID = dataRow.Field<string>("SalaryRuleMasterSystemID"),
                   SalaryRuleName = dataRow.Field<string>("SalaryRuleName"),
                   SalaryRuleDescription = dataRow.Field<string>("SalaryRuleDescription")
               }).ToList();
            #endregion Selected Salary Rule


            IsFreshEntry = o.IsFreshEntry;







            //MinWage

            
            if (dsMinWage.Tables[0].Rows.Count > 0)
            {
                ResultMinWage = dsMinWage.Tables[0].Rows[0]["MinWage"].ToString()+"("+ dsMinWage.Tables[0].Rows[0]["Grade"].ToString() + ")";
                if (Convert.ToDecimal(dsMinWage.Tables[0].Rows[0]["MinWage"].ToString()) ==0)
                {
                    //throw new Exception("Minimum wage not found.");
                }
                if (string.IsNullOrEmpty(dsMinWage.Tables[0].Rows[0]["EmployeeLocationId"].ToString()))
                {
                    //throw new Exception("Employee LocationId not found against this Budget Code["+ dsMinWage.Tables[0].Rows[0]["Code"].ToString() + "].");
                }
            }
            else
            {
                //throw new Exception("Minimum wage not found.");
            }
            if (o.IsFreshEntry == false)
            {
                //GetApprovedEffectiveDateAndNextDueDate
                DataSet dsApprovedEffectiveDateAndNextDueDate = GetApprovedEffectiveDateAndNextDueDate(EmpSystemId, identity.PlantId);
                ApprovedEffectiveDate = dsApprovedEffectiveDateAndNextDueDate.Tables[0].Rows[0]["EffectiveDate"].ToString();
                ApprovedNextDueDate = dsApprovedEffectiveDateAndNextDueDate.Tables[0].Rows[0]["NextDueDate"].ToString();

                ResultApprovedOpenHead = ConvertOpenHeadToList(dsApprovedOpenHead);


                
                //ApprovedInfo
                ob.GetApprovedInfo(EmpSystemId, dsOpenHead.Tables[0].Rows[0]["EffectiveDate"].ToString(), out dsIsapproved);

                if (dsIsapproved.Tables[0].Rows.Count > 0)
                {
                    ApprovalStatus = "Approved";
                }
                else
                {
                    ApprovalStatus = "UnApproved";
                }
                if (dtApprovedSalaryDifn != null)
                {
                    EmpApprovedSalaryInfoDefine = ConvertEmpSalaryInfoDefineToList(dtApprovedSalaryDifn);
                }
                else
                {
                    EmpApprovedSalaryInfoDefine = null;
                }
                //EmpApprovedSalaryInfoDefine = GetApprovedSalaryDetails(EmpSystemId, Convert.ToDateTime( dsOpenHead.Tables[0].Rows[0]["EffectiveDate"]).ToString("dd-MMM-yyyy"), "");
                //EmpApprovedSalaryInfoDefine = GetApprovedSalaryDetails(EmpSystemId, Convert.ToDateTime(ApprovedEffectiveDate).ToString("dd-MMM-yyyy"), "");

            }
            else
            {

                //if (dsLocal.TableCleared[])
                //{

                //}

                ApprovalStatus = "UnApproved";
                //ResultMinWage = null;
                ApprovedEffectiveDate = null;
                ApprovedNextDueDate = null;
                EmpApprovedSalaryInfoDefine = null;
                ResultApprovedOpenHead = null;
            }




            if (dsOpenHead.Tables[0].Rows.Count>0)
            {

                if (!string.IsNullOrEmpty(dsOpenHead.Tables[0].Rows[0]["EffectiveDate"].ToString()))
                {
                    ResultEffectiveDate = Convert.ToDateTime(dsOpenHead.Tables[0].Rows[0]["EffectiveDate"]).ToString("dd-MMM-yyyy");

                }
                else
                {

                    ResultEffectiveDate = GetEffectiveDate(EmpSystemId).ToString("dd-MMM-yyyy");
                }
            }
            else
            {
                ResultEffectiveDate = GetEffectiveDate(EmpSystemId).ToString("dd-MMM-yyyy");
            }



            //_employeePromotionService.LoadEmpSalaryInfoDefineData_OnGrid(EmpSystemId, out dsLocal);
            if (dsLocal != null)
            {
                EmpSalaryInfoDefine = ConvertEmpSalaryInfoDefineToList(dsLocal);
            }
            else
            {
                EmpSalaryInfoDefine = null;
            }


            outPara.ResultMinWage = ResultMinWage;
            outPara.ResultGross = ResultGross;
            outPara.ResultNetCTC = ResultNetCTC;
            outPara.IsSalaryRuleEditableEmployee = IsSalaryRuleEditableEmployee;
            outPara.ApprovalStatus = ApprovalStatus;
            outPara.ApprovedEffectiveDate = ApprovedEffectiveDate;
            outPara.ApprovedNextDueDate = ApprovedNextDueDate;
            outPara.ResultEffectiveDate = ResultEffectiveDate;
            outPara.NewFormula_Desc = NewFormula_Desc;
            outPara.ApprovedFormula_Desc = ApprovedFormula_Desc;


            if (outCustomParaAdditionalPolicySetting != null)
            {
                outPara.IsESICEntitle = outCustomParaAdditionalPolicySetting.IsESICEntitle;
                outPara.IsESICMandatory = outCustomParaAdditionalPolicySetting.IsESICMandatory;
                outPara.IsESICPolicyDefined = outCustomParaAdditionalPolicySetting.IsESICPolicyDefined;

                outPara.IsPFEntitle = outCustomParaAdditionalPolicySetting.IsPFEntitle;
                outPara.IsPFMandatory = outCustomParaAdditionalPolicySetting.IsPFMandatory;
                outPara.IsPFPolicyDefined = outCustomParaAdditionalPolicySetting.IsPFPolicyDefined;



                outPara.IsVPFEntitle = outCustomParaAdditionalPolicySetting.IsVoluntaryPFEntitle;
                outPara.VPFPersentage = outCustomParaAdditionalPolicySetting.VPFPescentage;
                outPara.VPFEffectiveDate = outCustomParaAdditionalPolicySetting.VPFEffectiveDate;


                outPara.IsBonusRtnEntitle = outCustomParaAdditionalPolicySetting.IsBonusRtnEntitle;
                outPara.IsBonusRtnMandatory = outCustomParaAdditionalPolicySetting.IsBonusRtnMandatory;
                outPara.IsBonusRtnPolicyDefined = outCustomParaAdditionalPolicySetting.IsBonusRtnPolicyDefined;

                

            }

            //Get Un Approved NextDueDate
            DataSet dsUApprovedEffectiveDateAndNextDueDate = GetUnApprovedEffectiveDateAndNextDueDate(EmpSystemId);
            if (!string.IsNullOrEmpty(dsUApprovedEffectiveDateAndNextDueDate.Tables[0].Rows[0]["NextDueDate"].ToString()))
            {
                outPara.UnApprovedNextDueDate = dsUApprovedEffectiveDateAndNextDueDate.Tables[0].Rows[0]["NextDueDate"].ToString();
            }




            //return Json(new { EmpSalaryInfoDefine = ResultData, ResultMinWage, ResultSalaryRule, ResultSelectedSalaryRule, ResultOpenHead, ResultGross, ResultNetCTC, Message = AplosMessage.Updated }, JsonRequestBehavior.AllowGet);
        }

        private List<OpenHeadModelNew> ConvertOpenHeadToList(DataSet ds)
        {
            List<OpenHeadModelNew> dt = new List<OpenHeadModelNew>();
            if (ds.Tables.Count > 0 && ds.Tables[0].Rows.Count > 0)
            {
                return ds.Tables[0].AsEnumerable().Select(
                    dataRow => new OpenHeadModelNew
                    {
                        SalaryHeadID = dataRow.Field<string>("SalaryHeadID"),
                        SalaryHead = dataRow.Field<string>("SalaryHead"),
                        Description = dataRow.Field<string>("Description"),
                        SalaryRuleDescription = dataRow.Field<string>("SalaryRuleDescription"),
                        HeadType = dataRow.Field<string>("HeadType"),
                        EntryCurrency = dataRow.Field<string>("EntryCurrency"),
                        Amount = dataRow.Field<decimal>("Amount"),
                        HeadCategory = dataRow.Field<string>("HeadCategory"),
                        EffectiveDate = dataRow.Field<DateTime?>("EffectiveDate"),
                        SalaryID = dataRow.Field<string>("SalaryID"),
                        SalaryHdSequence = dataRow.Field<int>("SalaryHdSequence")
                    }).ToList();
            }
            return dt;
        }
        private IEnumerable<EmpSalaryInfoDefineModelNew> ConvertEmpSalaryInfoDefineToList(DataTable dt)
        {
            // dt.AsEnumerable().Select(
            //    dataRow => new EmpSalaryInfoDefineModel
            //    {
            ////        if (string.IsNullOrEmpty(dataRow.Field<bool>("IsSelectSlrHd")))
            ////{

            ////}   ,
            //        IsSelectSlrHd =bplib.clsWebLib.GetBoolData(dt.row),
            //        SlrInfoDefSystemID = dataRow.Field<string>("SlrInfoDefSystemID"),
            //        CurrencyRuleChildSystemID = dataRow.Field<string>("CurrencyRuleChildSystemID"),
            //        SalaryHeadID = dataRow.Field<string>("SalaryHeadID"),
            //        SalaryHead = dataRow.Field<string>("SalaryHead"),
            //        HeadType = dataRow.Field<string>("HeadType"),
            //        FormulaDesID = dataRow.Field<string>("FormulaDesID"),
            //        FixedValue = dataRow.Field<string>("FixedValue"),
            //        IsOpen = dataRow.Field<string>("IsOpen"),
            //        EntryCurrencyID = dataRow.Field<string>("EntryCurrencyID"),
            //        EntryCurrency = dataRow.Field<string>("EntryCurrency"),
            //        DefinitionCurrencyID = dataRow.Field<string>("DefinitionCurrencyID"),
            //        DefinitionCurrency = dataRow.Field<string>("DefinitionCurrency"),
            //        EntryAmount = dataRow.Field<decimal>("EntryAmount").ToString(),
            //        DefineAmount = dataRow.Field<decimal>("DefineAmount").ToString(),
            //        TagAndUnTag = dataRow.Field<string>("TagAndUnTag"),
            //        MonthPeriod = dataRow.Field<string>("MonthPeriod"),
            //        IsNA = dataRow.Field<string>("IsNA"),
            //        HeadCategory = dataRow.Field<string>("HeadCategory"),
            //        SalaryHdSequence = dataRow.Field<string>("SalaryHdSequence"),
            //        SalaryCategory = dataRow.Field<string>("SalaryCategory")
            //    }).ToList();


            List<EmpSalaryInfoDefineModelNew> list = new List<EmpSalaryInfoDefineModelNew>();
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                EmpSalaryInfoDefineModelNew Data = new EmpSalaryInfoDefineModelNew();

                Data.IsSelectSlrHd = bplib.clsWebLib.GetBoolData(dt.Rows[i]["IsSelectSlrHd"].ToString());
                Data.SlrInfoDefSystemID = dt.Rows[i]["SlrInfoDefSystemID"].ToString();
                Data.CurrencyRuleChildSystemID = dt.Rows[i]["CurrencyRuleChildSystemID"].ToString();
                Data.SalaryHeadID = dt.Rows[i]["SalaryHeadID"].ToString();
                Data.SalaryHead = dt.Rows[i]["SalaryHead"].ToString();
                Data.HeadType = dt.Rows[i]["HeadType"].ToString();
                Data.FormulaDesID = dt.Rows[i]["FormulaDesID"].ToString();
                Data.FixedValue = dt.Rows[i]["FixedValue"].ToString();
                Data.IsOpen = bplib.clsWebLib.GetBoolData(dt.Rows[i]["IsOpen"].ToString());
                Data.EntryCurrencyID = dt.Rows[i]["EntryCurrencyID"].ToString();
                Data.EntryCurrency = dt.Rows[i]["EntryCurrency"].ToString();
                Data.DefinitionCurrencyID = dt.Rows[i]["DefinitionCurrencyID"].ToString();
                Data.DefinitionCurrency = dt.Rows[i]["DefinitionCurrency"].ToString();
                Data.EntryAmount = Convert.ToDecimal(bplib.clsWebLib.GetNumData(dt.Rows[i]["EntryAmount"].ToString()));
                Data.DefineAmount = Convert.ToDecimal(bplib.clsWebLib.GetNumData(dt.Rows[i]["DefineAmount"].ToString()));
                //Data.TagAndUnTag = dt.Rows[i]["TagAndUnTag"].ToString();
                Data.MonthPeriod = dt.Rows[i]["MonthPeriod"].ToString();
                Data.IsNA = bplib.clsWebLib.GetBoolData(dt.Rows[i]["IsNA"].ToString());
                Data.HeadCategory = dt.Rows[i]["HeadCategory"].ToString();
                Data.SalaryHdSequence = dt.Rows[i]["SalaryHdSequence"].ToString();
                Data.SequenceNo = Convert.ToInt32(dt.Rows[i]["SequenceNo"].ToString());
                Data.SalaryCategory = dt.Rows[i]["SalaryCategory"].ToString();
                //Data.HeadCategory = dt.Rows[i]["HeadCategory"].ToString();
                Data.IsSlabBased= Convert.ToBoolean(dt.Rows[i]["IsSlabBased"].ToString());
                list.Add(Data);
            }

            return list;
        }
        public IEnumerable<object> GetEmployeeListForSalaryStrcApproval(string companyGroupId, string plantId)
        {
            try
            {
                string sql = @"SELECT [CheckBoxSelect] = Convert(BIT, 'False') , EI.*,Replace(CONVERT(VARCHAR(11), EI.DOB, 106), ' ', '-') DOBs,ISNULL(FORMAT(EI.DOS,'dd-MMM-yyyy'),'')DOSs,Replace(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJs
                                 ,LGD.userName LegalDesignation                                    
                                    ,se.UserName Section
                                    ,Sus.UserName SubSection
								  ,DG.UserName GivenDesignation 
								  ,DG.UserName GivenDesignation, DP.UserName Department, PR.UserName PositionName,E.UserName EntityName,DSG.UserName Designation,PR.DesignationId,PG.StandardName PayRollGroupName,PG.Id PayRollGroupId, ISNULL(IIF(SM.IsApproved=1 , 'Approved', 'Un-approved'),'New') as ApprovedStatus , DeG.UserName DesignationGroupName,IH.IsConfirmation,IH.IncrementType
								  ,PMB.Id,PMB.Code,mbd.TotalNumber BudgetedManPower,OnRollManPwr=ONR.OnRoll-ISNULL(A.TS,0),CAR.CurrentApprovalRequired,BalanceRequired=mbd.TotalNumber-(ONR.OnRoll-ISNULL(A.TS,0))
                                  ,ISNULL(LA.LAEmp,0)LA,ISNULL(TBS.TBSEmp,0)TBS,NetShortage=mbd.TotalNumber-(ONR.OnRoll-ISNULL(A.TS,0))-ISNULL(LA.LAEmp,0)-ISNULL(TBS.TBSEmp,0)
                       FROM dbo.Employeeinformation EI                             
							  LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id							 
                              LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                              LEFT JOIN(Select SUM(TotalNumber)TotalNumber,ManpowerBudgetId from MST.ManpowerBudgetDetail Group BY ManpowerBudgetId) AS mbd ON mbd.ManpowerBudgetId=PMB.Id
                              LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                              LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
							  LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
							  LEFT JOIN HKP.Designation DeG on DeG.Id=EI.GivenDesignationId
				              LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
							  Left join MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
							  Left Join hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
                              LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId
                              LEFT JOIN HKP.DesignationGroup DG ON DG.Id=DM.DesignationGroupId
                            LEFT JOIN HKP.LegalDesignation LGD on LGD.Id=EI.LegalDesignationId	
                            LEFT JOIN ORG.Section AS Se ON Se.Id= PR.SectionID 
                            LEFT JOIN ORG.SubSection AS SuS ON SuS.Id= PR.SubSectionID
                              LEFT JOIN SalaryInfoDefineMaster SM ON SM.EmpInfoSystemID=EI.SystemID AND SM.EmpInfoSystemID=(SELECT TOP 1 EmpInfoSystemID FROM SalaryInfoDefineMaster WHERE EmpInfoSystemID= EI.SystemID  ORDER BY   EffectiveDate DESC) AND isnull(SM.IsApproved,0)=0
							  LEFT JOIN IncrementHistory IH ON IH.EmpSystemID=EI.SystemID AND IH.IsApproved=0  AND IH.EmpSystemID=(SELECT TOP 1 EmpSystemID FROM IncrementHistory WHERE EmpSystemID= EI.SystemID  ORDER BY FromEffectiveDate DESC)  AND sm.SystemID=IH.ToSalaryId
							 
							 LEFT JOIN (SELECT COUNT(SystemId) OnRoll,BudgetCode FROM EmployeeInformation WHERE EmployeeStatus = 'Active' GROUP BY BudgetCode) ONR ON ONR.BudgetCode=EI.BudgetCode
							 LEFT JOIN (SELECT COUNT(BudgetCode) CurrentApprovalRequired,SystemId FROM EmployeeInformation GROUP BY SystemId) CAR ON CAR.SystemId=EI.SystemId
							 LEFT JOIN (SELECT COUNT(BudgetCode) TS,SystemId FROM EmployeeInformation WHERE EmployeeStatus = 'Active' AND ISNULL(EmployeeCurrentStatus,'') IN ('TBS','LONG ABSENTEEISM') GROUP BY SystemId) A ON A.SystemId=EI.SystemId
							 LEFT JOIN (SELECT COUNT(BudgetCode) TBSEmp,SystemId FROM EmployeeInformation WHERE EmployeeStatus = 'Active' AND ISNULL(EmployeeCurrentStatus,'') IN ('TBS') GROUP BY SystemId) TBS ON TBS.SystemId=EI.SystemId
							 LEFT JOIN (SELECT COUNT(BudgetCode) LAEmp,SystemId FROM EmployeeInformation WHERE EmployeeStatus = 'Active' AND ISNULL(EmployeeCurrentStatus,'') IN ('LONG ABSENTEEISM') GROUP BY SystemId) LA ON LA.SystemId=EI.SystemId
							   
                             
                              WHERE EI.PlantId='" + plantId + @"' AND  EI.GroupId='"+ companyGroupId + @"' AND (EI.SystemId IN (SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster where  IsApproved=0) --or  EI.SystemId  IN (SELECT EmpSystemID FROM IncrementHistory Where IsApproved=0) 
                                            AND EI.DOJ<=GETDATE() AND (EI.DOS is null OR EI.DOS BETWEEN FORMAT(DATEADD(mm, DATEDIFF(mm, 0, GETDATE()) - 1, 0),'dd-MMM-yyyy') AND FORMAT(DATEADD (dd, -1, DATEADD(mm, DATEDIFF(mm, 0, GETDATE()) + 1, 0)),'dd-MMM-yyyy')))";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IEnumerable<object> GetEmployeeListForPromotionApproval(string companyGroupId, string plantId)
        {
            try
            {
                string sql = @"SELECT EI.*,Replace(CONVERT(VARCHAR(11), EI.DOB, 106), ' ', '-') DOBs,Replace(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJs
                                 ,LGD.userName LegalDesignation
                                    
                                    ,se.UserName Section
                                    ,Sus.UserName SubSection
								  ,DG.UserName GivenDesignation 
								  ,DG.UserName GivenDesignation, DP.UserName Department, PMB.Code,PR.UserName PositionName,E.UserName EntityName,DSG.UserName Designation,PR.DesignationId,PG.StandardName PayRollGroupName,PG.Id PayRollGroupId, ISNULL(IIF(IH.IsApproved=1 , 'Approved', 'Un-approved'),'New') as ApprovedStatus , DeG.UserName DesignationGroupName,IH.IsConfirmation,IH.IncrementType
                              FROM dbo.Employeeinformation EI                             
							  LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id							 
                              LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                              LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                              LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
							  LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
							  LEFT JOIN HKP.Designation DeG on DeG.Id=EI.GivenDesignationId
				              LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
							  Left join MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
							  Left Join hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
                              LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId
                              LEFT JOIN HKP.DesignationGroup DG ON DG.Id=DM.DesignationGroupId
                              LEFT JOIN HKP.LegalDesignation LGD on LGD.Id=EI.LegalDesignationId	
                              LEFT JOIN ORG.Section AS Se ON Se.Id= PR.SectionID 
                              LEFT JOIN ORG.SubSection AS SuS ON SuS.Id= PR.SubSectionID
                              --LEFT JOIN SalaryInfoDefineMaster SM ON SM.EmpInfoSystemID=EI.SystemID AND SM.EmpInfoSystemID=(SELECT TOP 1 SystemID FROM SalaryInfoDefineMaster WHERE EmpInfoSystemID= EI.SystemID  ORDER BY   EffectiveDate DESC) AND SM.IsApproved=0
							  LEFT JOIN IncrementHistory IH ON IH.EmpSystemID=EI.SystemID   AND IH.IsApproved=0  AND IH.EmpSystemID=(SELECT TOP 1 EmpSystemID FROM IncrementHistory WHERE EmpSystemID= EI.SystemID  ORDER BY FromEffectiveDate DESC)
                              WHERE EI.EmployeeStatus ='Active' AND EI.PlantId='" + plantId + @"' AND  EI.GroupId='" + companyGroupId + @"' 
                              AND  EI.SystemId IN (SELECT EmpSystemID FROM IncrementHistory Where IsApproved=0 AND IncrementType IN ('Promotion','Increment and Promotion','Confirmation with Promotion','Confirmation with Increment and Promotion')) ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IEnumerable<object> GetEmployeeListForSalaryStrcUnApproval(string companyGroupId, string plantId)
        {
            try
            {
                string sql = @"SELECT  [CheckBoxSelect] = Convert(bit, 'False'),  EI.*,Replace(CONVERT(VARCHAR(11), EI.DOB, 106), ' ', '-') DOBs,Replace(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJs                                 
                                  ,LGD.userName LegalDesignation
                                    
                                    ,se.UserName Section
                                    ,Sus.UserName SubSection
								  ,DG.UserName GivenDesignation
                                , DP.UserName Department, PMB.Code,PR.UserName PositionName,E.UserName EntityName
                                  ,DSG.UserName Designation,PR.DesignationId
                                  ,PG.StandardName PayRollGroupName
                                  ,PG.Id PayRollGroupId
                                  ,ISNULL(IIF(SM.IsApproved=1 , 'Approved', 'Un-approved'),'New') as ApprovedStatus 
                                  ,DeG.UserName DesignationGroupName
                                  ,IH.IsConfirmation,IH.IncrementType,SM.SystemID SalaryStructureId
                              FROM dbo.Employeeinformation EI
                              LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id							
							  LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id							 
                              LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                              LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                              LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
							  LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
							  LEFT JOIN HKP.Designation DeG on DeG.Id=EI.GivenDesignationId
				              LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
							  Left join MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
							  Left Join hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
                              LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId
                              LEFT JOIN HKP.DesignationGroup DG ON DG.Id=DM.DesignationGroupId
                            LEFT JOIN HKP.LegalDesignation LGD on LGD.Id=EI.LegalDesignationId	
                            LEFT JOIN ORG.Section AS Se ON Se.Id= PR.SectionID 
                            LEFT JOIN ORG.SubSection AS SuS ON SuS.Id= PR.SubSectionID

                              LEFT JOIN SalaryInfoDefineMaster SM ON SM.EmpInfoSystemID=EI.SystemID AND SM.SystemID=(SELECT TOP 1 SystemID FROM SalaryInfoDefineMaster WHERE EmpInfoSystemID= EI.SystemID  ORDER BY   EffectiveDate DESC) AND SM.IsApproved=1
							  LEFT JOIN IncrementHistory IH ON IH.EmpSystemID=EI.SystemID   AND IH.IsApproved=0  AND IH.EmpSystemID=(SELECT TOP 1 EmpSystemID FROM IncrementHistory WHERE EmpSystemID= EI.SystemID  ORDER BY FromEffectiveDate DESC) AND sm.SystemID=IH.ToSalaryId                               
                              WHERE EI.EmployeeStatus ='Active' AND EI.PlantId='" + plantId + @"' AND  EI.GroupId='" + companyGroupId + @"' 
                              AND  EI.SystemId NOT IN (SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster where  IsApproved=0)   
							         --AND EI.SystemID IN  (SELECT EmpSystemID FROM IncrementHistory Where IsApproved=0)

                              AND ( EI.SystemId  IN (SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster where  IsApproved=1)   
									     or  EI.SystemId  IN (SELECT EmpInfoSystemID FROM SalaryInfoBackMaster)
										 ) ";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        private DateTime GetEffectiveDate(string empid)
        {
            clsSalaryInfoNew obs = null;
            DataSet dsLocal = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


            try
            {
                obs = new clsSalaryInfoNew();
                obs.GetCutOffDate(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, empid, out dsLocal);
                ValidateDate(dsLocal, "CutoffDate");
                DateTime dCutoffDate = Convert.ToDateTime(dsLocal.Tables[0].Rows[0]["CutOffDate"].ToString());
                obs.GetEmployeeDOJ(empid, out dsLocal);
                ValidateDate(dsLocal, "DOJ");
                DateTime dDOJ = Convert.ToDateTime(dsLocal.Tables[0].Rows[0]["DOJ"].ToString());

                if (dCutoffDate > dDOJ)
                {
                    return dCutoffDate;
                }
                else
                {
                    return dDOJ;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void UpdateSalaryStractureForIncrement(EmployeeInformation employeeInformation
            , List<EmployeeEligibleForSalaryHeadEnum> oEmployeeEligibleForSalaryHeadEnum, List<PFEmployeeVoluntaryValueTemp> oPFEmployeeVoluntaryValue
            , EmpSalaryInfoModel SalaryInfo
            , IEnumerable<EmpSalaryInfoDefineModel> EmpSalaryInfoDefineNew
            , IncrementHistoryModel incrementHistory)
        {
            clsSalaryStructureAplosNew obj = null;
            CustomParaNew _para = null;
            DataSet dsLocal = null;
            clsEmployeeLoad objApp = null;
            DataSet dsLastApprovedEffectiveDate = null;

            try
            {
                _para = new CustomParaNew();
                objApp = new clsEmployeeLoad();
                obj = new clsSalaryStructureAplosNew();
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                _para.BudgetCodeId = employeeInformation.BudgetCode;
                _para.CompanyId = employeeInformation.CompanyId;
                _para.CompanyGroupId = identity.CompanyGroupId;
                _para.DOJ = employeeInformation.DOJ.ToString();
                _para.EmployeeId = employeeInformation.SystemId;
                _para.PlantId = employeeInformation.PlantID;
                _para.SalaryRuleId = SalaryInfo.SalaryRuleMasterSystemID;
                _para.EffectiveDate = Convert.ToDateTime(SalaryInfo.EffectiveDate).ToString("dd-MMM-yyyy");
                _para.IsFreshEntry = SalaryInfo.IsFreshEntry;
                //pf
                //_para.IsPFEntitle = PFSettingModel.IsPFEntitle;
                //_para.IsbuttonPFClicked = PFSettingModel.IsbuttonPFClicked;
                ////_para.IsPFNotEntitleGetAllo = PFSettingModel.IsPFNotEntitleGetAllo;
                //_para.PFEffectiveDate = PFSettingModel.PFEffectiveDate;

                obj.LoadSlrRuleInfo(_para);
                _para.ForeignCurRate = "1";
                if (string.IsNullOrEmpty(_para.ForeignCurRate))
                {
                    _para.ForeignCurRate = "0.0";
                }

                if (_para.ForeignCurRate.Length > 20 || bplib.clsWebLib.IsNumeric(_para.ForeignCurRate) == false)
                {
                    Exception ex = new Exception("Invalid / Blank Data not allowed for 'Amount Definition Currency Rate'. \n Please Enter Numeric data Only");
                    throw (ex);
                }

                objApp.LoadEmployeeInfoNew(_para, out dsLocal);
                if (_para.IsFreshEntry == false)
                {
                    obj.GetLastApprovedEffectiveDate(_para.EmployeeId, out dsLastApprovedEffectiveDate);
                    if (Convert.ToDateTime(SalaryInfo.EffectiveDate.ToString()) <= Convert.ToDateTime(dsLastApprovedEffectiveDate.Tables[0].Rows[0]["EffectiveDate"]))
                    {
                        Exception ex = new Exception("Previous or Same Data can not be inserted");
                        throw (ex);
                    }
                }



                if (dsLocal.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < dsLocal.Tables[0].Rows.Count; i++)
                    {
                        obj.GetEmployeeInfo(dsLocal.Tables[0].Rows[i], _para);
                    }
                    _para.SalaryRuleId = SalaryInfo.SalaryRuleMasterSystemID;
                    _para.EffectiveDate = Convert.ToDateTime(SalaryInfo.EffectiveDate).ToString("dd-MMM-yyyy");
                    _para.NextDueDate = SalaryInfo.NextDueDate.ToString();
                    _para.User = identity.Name;
                    _para.AddedFromIP = identity.IPAddress;
                    _para.UpdatedFromIP = identity.IPAddress;
                    _para.SalaryId = SalaryInfo.SalaryID;
                    _para.ApprovalStatus = "Unapproved";
                    DataSet dsEmpSalaryInfoDefineNew = Library.Service.Helpers.DataTableExtensions.ToDataSet<EmpSalaryInfoDefineModel>(EmpSalaryInfoDefineNew);
                    DataSet dsEmployeeEligibleForSalaryHeadEnum = Library.Service.Helpers.DataTableExtensions.ToDataSet<EmployeeEligibleForSalaryHeadEnum>(oEmployeeEligibleForSalaryHeadEnum);
                    DataSet dsPFEmployeeVoluntaryValue = Library.Service.Helpers.DataTableExtensions.ToDataSet<PFEmployeeVoluntaryValueTemp>(oPFEmployeeVoluntaryValue);


                    
                    foreach (var item in EmpSalaryInfoDefineNew)
                    {
                       
                        if (item.EntryAmount<0)
                        {
                            throw new Exception("Negative amount is not allowed for Salary head [" + item.SalaryHead.ToString() + "].");
                        }

                    }



                    obj.SaveData(_para, dsEmpSalaryInfoDefineNew.Tables[0], incrementHistory, dsEmployeeEligibleForSalaryHeadEnum, dsPFEmployeeVoluntaryValue);
                }
                else
                {
                    throw new Exception("No employee found ...");
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                dsLocal = null;
            }

            //obj.LoadEmployeeInfo(_para, out dsLocal);

            //_para.CompanyId = employeeInformation.CompanyId;
            //_para.CompanyGroupId = employeeInformation.CompanyGroupId;
            //_para.User = employeeInformation.User;
            //_para.PlantId = employeeInformation.PlantID;
            //_para.Plant = employeeInformation.Plant;
            //_para.EmployeeId = employeeInformation.EmployeeId;
            //_para.BudgetCodeId = employeeInformation.BudgetCodeId;
            //_para.DesignationGroupId = employeeInformation.DesignationGroupId;
            //_para.EmployeeName = employeeInformation.EmployeeName;
            //_para.EmployeeCode = employeeInformation.EmployeeCode;
            //_para.EffectiveDate = employeeInformation.EffectiveDate;
            //_para.DOJ = employeeInformation.DOJ.ToString();
            //_para.NextDueDate = employeeInformation.NextDueDate;
            //_para.SalaryId = employeeInformation.SalaryId;
            //_para.SalaryFlag = employeeInformation.SalaryFlag;
            //_para.CuttOffDate = employeeInformation.CuttOffDate;
            //_para.SalaryRule = employeeInformation.SalaryRule;
            //_para.FormulaValue = employeeInformation.FormulaValue;
            //_para.SalaryRuleId = employeeInformation.SalaryRuleId;
            //_para.TaxGroupId = employeeInformation.TaxGroupId;
            //_para.EmpTaxGroupPk = employeeInformation.EmpTaxGroupPk;
            //_para.ApprovalStatus = employeeInformation.ApprovalStatus;
            //_para.LocalCurrencyId = employeeInformation.LocalCurrencyId;
            //_para.ForeignCurrencyId = employeeInformation.ForeignCurrencyId;
            //_para.ForeignCurRate = employeeInformation.ForeignCurRate;
            //_para.CurrencyRuleMasterId = employeeInformation.CurrencyRuleMasterId;
            //_para.StartMonth = employeeInformation.StartMonth;
            //_para.EndMonth = employeeInformation.EndMonth;








            //_para.BudgetCodeId = employeeInformation.BudgetCode;
            //_para.CompanyId = employeeInformation.CompanyId;
            //_para.CompanyGroupId = identity.CompanyGroupId;
            //_para.DOJ = employeeInformation.DOJ.ToString();
            //_para.EmployeeId = employeeInformation.SystemId;
            //_para. = employeeInformation.SystemId;









        }
        public void ReCalculateSalaryStracture(IEnumerable<EmpSalaryInfoDefineModelNew> EmpSalaryInfoDefineNew,  out IEnumerable<EmpSalaryInfoDefineModelNew> EmpSalaryInfoDefine)
        {
            clsSalaryStructureAplosNew obj = new clsSalaryStructureAplosNew();
          
            try
            {
              
                


                DataSet dsEmpSalaryInfoDefineNew = Library.Service.Helpers.DataTableExtensions.ToDataSet<EmpSalaryInfoDefineModelNew>(EmpSalaryInfoDefineNew);              
                
                DataView dv= obj.ReCalculateSalryDefine(dsEmpSalaryInfoDefineNew);
                //Net Gross and Net CTC
                //DataView dv = new DataView();
                //dv.Table = obj.ReCalculateSalryDefine(dsEmpSalaryInfoDefineNew); 
                //dv.Sort = "HeadType DESC, SalaryHead";
                dv.Sort = "SequenceNo";
                var dg = dv.ToTable();

                //DataRow[] dataRows = dtResult.Select().OrderBy(u => Convert.ToInt32(u["SequenceNo"])).ToArray();

                


                EmpSalaryInfoDefine = ConvertEmpSalaryInfoDefineToList(dg);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                //dsLocal = null;
            }







            //obj.LoadEmployeeInfo(_para, out dsLocal);

            //_para.CompanyId = employeeInformation.CompanyId;
            //_para.CompanyGroupId = employeeInformation.CompanyGroupId;
            //_para.User = employeeInformation.User;
            //_para.PlantId = employeeInformation.PlantID;
            //_para.Plant = employeeInformation.Plant;
            //_para.EmployeeId = employeeInformation.EmployeeId;
            //_para.BudgetCodeId = employeeInformation.BudgetCodeId;
            //_para.DesignationGroupId = employeeInformation.DesignationGroupId;
            //_para.EmployeeName = employeeInformation.EmployeeName;
            //_para.EmployeeCode = employeeInformation.EmployeeCode;
            //_para.EffectiveDate = employeeInformation.EffectiveDate;
            //_para.DOJ = employeeInformation.DOJ.ToString();
            //_para.NextDueDate = employeeInformation.NextDueDate;
            //_para.SalaryId = employeeInformation.SalaryId;
            //_para.SalaryFlag = employeeInformation.SalaryFlag;
            //_para.CuttOffDate = employeeInformation.CuttOffDate;
            //_para.SalaryRule = employeeInformation.SalaryRule;
            //_para.FormulaValue = employeeInformation.FormulaValue;
            //_para.SalaryRuleId = employeeInformation.SalaryRuleId;
            //_para.TaxGroupId = employeeInformation.TaxGroupId;
            //_para.EmpTaxGroupPk = employeeInformation.EmpTaxGroupPk;
            //_para.ApprovalStatus = employeeInformation.ApprovalStatus;
            //_para.LocalCurrencyId = employeeInformation.LocalCurrencyId;
            //_para.ForeignCurrencyId = employeeInformation.ForeignCurrencyId;
            //_para.ForeignCurRate = employeeInformation.ForeignCurRate;
            //_para.CurrencyRuleMasterId = employeeInformation.CurrencyRuleMasterId;
            //_para.StartMonth = employeeInformation.StartMonth;
            //_para.EndMonth = employeeInformation.EndMonth;








            //_para.BudgetCodeId = employeeInformation.BudgetCode;
            //_para.CompanyId = employeeInformation.CompanyId;
            //_para.CompanyGroupId = identity.CompanyGroupId;
            //_para.DOJ = employeeInformation.DOJ.ToString();
            //_para.EmployeeId = employeeInformation.SystemId;
            //_para. = employeeInformation.SystemId;









        }
        public void CalculateSalary(EmployeeInformation employeeInformation
            , string IsbuttonPFClicked
            , bool IsPFEntitle
            , bool IsVPFEntitle,string VPFPescentage
            , bool IsESICEntitle, bool IsBonusEntitle
            , string salaryRuleMasterSystemID
            , IEnumerable<OpenHeadModelNew> OpenHeadNew,string EffectiveDate
            , out IEnumerable<EmpSalaryInfoDefineModelNew> EmpSalaryInfoDefine
            , out string NewGross
            , out string NewCTC
            , out string _Formula_Desc
            ,out CustomParaAdditionalPolicySetting para)
        {
            para = null;
            DataTable dtResult = null;
            DataSet dsLocal = null;
            DataSet dsOpenHead = Library.Service.Helpers.DataTableExtensions.ToDataSet<OpenHeadModelNew>(OpenHeadNew);
            CustomParaNew _para = new CustomParaNew();
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            _para.PlantId = employeeInformation.PlantID;
            _para.CompanyId = employeeInformation.CompanyId;
            _para.CompanyGroupId = employeeInformation.GroupID;
            _para.EmployeeId = employeeInformation.SystemId;
            _para.SalaryRuleId = salaryRuleMasterSystemID;
            _para.User = identity.Name;
            _para.IsbuttonPFClicked = IsbuttonPFClicked;
            _para.IsPFEntitle = IsPFEntitle;
            _para.IsESICEntitle = IsESICEntitle;
            _para.IsVoluntaryPFEntitle = IsVPFEntitle;
            _para.VPFPescentage = VPFPescentage;
            _para.IsBonusEntitle = IsBonusEntitle;
            _para.EDate = EffectiveDate;



            clsSalaryInfoNew objSal = new clsSalaryInfoNew();


            clsSalaryStructureAplosNew obj = new clsSalaryStructureAplosNew();




            try
            {

                objSal.GetLocalCurrency(identity.CompanyGroupId, identity.PlantId, out dsLocal);
                if (dsLocal.Tables[0].Rows.Count > 0)
                {
                    //_para.cu = "" + dsLocal.Tables[0].Rows[0]["Currency"].ToString().Trim();
                    _para.LocalCurrencyId = "" + dsLocal.Tables[0].Rows[0]["LocalCurrency"].ToString().Trim();
                }

                obj.StartCalculation(_para, dsOpenHead, out dtResult, out _Formula_Desc,out  para);



                //Net Gross and Net CTC
                DataView dv = new DataView();
                dv.Table = dtResult;
                //dv.Sort = "HeadType DESC, SalaryHead";
                dv.Sort = "SequenceNo";
                var dg = dv.ToTable();

                DataRow[] dataRows = dtResult.Select().OrderBy(u =>Convert.ToInt32(u["SequenceNo"])).ToArray();

                //CheckAll(true);
                decimal decNetGross = 0;
                decimal decNetCTC = 0;
                               
                NewGross = decNetGross.ToString("#,##0.00;(#,##0.00)");
                NewCTC = decNetCTC.ToString("#,##0.00;(#,##0.00)");


                //dtResultSalaryDetails = dg;


                EmpSalaryInfoDefine = ConvertEmpSalaryInfoDefineToList(dg);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Employee Load and Search
        public IEnumerable<object> GetSalaryStrcUnApprovedEmployee(string companyGroupId, string plantId)
        {
            try
            {

                string xsql = @"SELECT EI.*,Replace(CONVERT(VARCHAR(11), EI.DOB, 106), ' ', '-') DOBs,Replace(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJs
                                  ,PO.UserName PresThanaName,ParmPO.UserName ParmThanaName,D.UserName PresDistrictName,ParmD.UserName ParmDistrictName
								  ,C.UserName PresCountryName,ParmC.UserName ParmCountryName,ParmP.UserName ParmPostOfficeName, PerP.UserName PresPostOfficeName
                                  ,PerCT.UserName PresCityName,ParCT.UserName ParmCityName,AM.CountryId
								  ,CG.[Image] CompanyGroupLogo, CNT.PhoneLength, COM.IsTINRequiredForSalaryAbove
								  ,CNT.TINCaption, CNT.NIDCaption, CNT.NIDLength, CNT.TINLength, COM.TINRequiredForSalaryAbove
								  ,DG.UserName GivenDesignation, DP.UserName Department, PMB.Code,PR.UserName PositionName,E.UserName EntityName,DSG.UserName Designation,PR.DesignationId,PG.StandardName PayRollGroupName,PG.Id PayRollGroupId, ApprovedStatus=case when SM.IsApproved=1 then 'Approved' when SM.IsApproved=0 then 'Un-approved' when SM.IsApproved is null then 'Not Defined' end  , DeG.UserName DesignationGroupName
                              FROM dbo.Employeeinformation EI
                              LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id
							  LEFT JOIN scs.PoliceStation PO ON EI.PresThanaID=PO.Id
							  LEFT JOIN scs.PoliceStation ParmPO ON EI.ParmThanaID=ParmPO.Id
							  LEFT JOIN SCS.District D ON EI.PresDistrictID = D.Id
							  LEFT JOIN SCS.District ParmD ON EI.ParmDistrictID = ParmD.Id
		                      LEFT JOIN SCS.Country C ON EI.PresCountryID = C.ID
		                      LEFT JOIN SCS.Country ParmC	ON EI.ParmCountryID = ParmC.ID
		                      LEFT JOIN SCS.PostOffice ParmP ON EI.ParmPostOfficeID = ParmP.ID
		                      LEFT JOIN SCS.PostOffice PerP ON EI.PresPostOfficeID = PerP.ID
                              LEFT JOIN SCS.City PerCT ON EI.PresCityID = PerCT.ID
		                      LEFT JOIN SCS.City ParCT ON EI.ParmCityID = ParCT.ID
                              LEFT JOIN SCS.[State] ParmS ON EI.ParmStateId = ParmS.Id
							  LEFT JOIN SCS.[State] PresS ON EI.PresStateId = PresS.Id
							  LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id
							  LEFT JOIN MST.AddressMaster AM ON PL.AddressMasterId=AM.Id
							  LEFT JOIN SCS.Country CNT ON AM.CountryId=CNT.Id
							  LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                              LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                              LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                              LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
							  LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
							  LEFT JOIN HKP.Designation DeG on DeG.Id=EI.GivenDesignationId
				              LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
							  Left join MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
							  Left Join hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
                              LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId
                              LEFT JOIN HKP.DesignationGroup DG ON DG.Id=DM.DesignationGroupId
                              Left Join SalaryInfoDefineMaster SM ON SM.EmpInfoSystemID=EI.SystemID 
                  --            Left Join (
							  
				--      SELECT X.EmpInfoSystemID,MAX(ed) ed
                  --                       ,ISNULL(M.IsApproved,0) IsApproved
                 --                     FROM(
                  --                      SELECT EmpInfoSystemID,MAX(EffectiveDate) ed 
                  --                     FROM SalaryInfoDefineMaster WHERE PlantId='" + plantId + @"'
                  --                    GROUP BY EmpInfoSystemID
                  --                    UNION
                  --                     SELECT EmpInfoSystemID,MAX(EffectiveDate) ed 
                  --                      FROM SalaryInfoBackMaster WHERE PlantId='" + plantId + @"'
                  --                     GROUP BY EmpInfoSystemID
                  --                     ) X
                  --                      LEFT JOIN (SELECT EmpInfoSystemID,EffectiveDate,IsApproved FROM SalaryInfoDefineMaster WHERE PlantId='" + plantId + @"'
                  --                      UNION
                  --                     SELECT EmpInfoSystemID,EffectiveDate,IsApproved FROM SalaryInfoBackMaster WHERE PlantId='" + plantId + @"'
                  --                      ) M ON M.EffectiveDate=X.ed AND M.EmpInfoSystemID=X.EmpInfoSystemID
                  --                      GROUP BY X.EmpInfoSystemID,M.IsApproved 
							           --                               ) SM ON SM.EmpInfoSystemID=EI.SystemID 
                              WHERE EI.EmployeeStatus ='Active' AND EI.PlantId='" + plantId + @"' AND  EI.GroupId='" + companyGroupId + "' AND ( EI.SystemId IN (SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster where  IsApproved=0) or  EI.SystemId Not IN (SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster) ) " +

                             " AND EI.SystemID NOT IN (SELECT EmpInfoSystemID FROM SalaryInfoBackMaster) AND EI.SystemID NOT IN (SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster where  IsApproved=1)";



                string sql = @"SELECT EI.*,Replace(CONVERT(VARCHAR(11), EI.DOB, 106), ' ', '-') DOBs,Replace(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJs                                 
								  ,DG.UserName GivenDesignation, DP.UserName Department, PMB.Code,PR.UserName PositionName,E.UserName EntityName
								  ,DSG.UserName Designation
								  ,PR.DesignationId
								  ,PG.StandardName PayRollGroupName
								  ,PG.Id PayRollGroupId
								  , ApprovedStatus=case when SM.IsApproved=1 then 'Approved' when SM.IsApproved=0 then 'Un-approved' when SM.IsApproved is null then 'Not Defined' end  , DeG.UserName DesignationGroupName
								  ,PFPolicy=case when dmc.PFPolicyMasterID IS NOT NULL THEN 'Yes' ELSE 'No' END
								  ,ESICPolicy=case when dmc.ESICPolicyMasterID IS NOT NULL THEN 'Yes' ELSE 'No' END
								  ,BnsPolicy=case when dmc.BnsPlcMthRetainID IS NOT NULL THEN 'Yes' ELSE 'No' END
								  ,SalaryRule=case when dmc.SalaryRuleMasterId IS NOT NULL THEN 'Yes' ELSE 'No' END
								  ,ld.UserName LegalDesignation
								  ,ec.UserName EmployeeCategory
								  ,LSG.UserName SalaryGrade
                              FROM dbo.Employeeinformation EI                             
                              LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                              LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                              LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
							  LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
							  LEFT JOIN HKP.Designation DeG on DeG.Id=EI.GivenDesignationId
							  LEFT JOIN  HKP.LegalDesignation AS ld ON ld.Id=ei.LegalDesignationId----
							  
				              LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
							  Left join MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
							  Left Join hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
							  
							  LEFT JOIN mst.DesignationMasterLegalDesignation AS dmld ON dmld.LegalDesignationId=ei.LegalDesignationId---
                              LEFT JOIN MST.DesignationMaster DM ON DM.id=dmld.DesignationMasterId----
                              LEFT JOIN hkp.EmployeeCategory AS ec ON ec.Id=dm.EmployeeCategoryId-----
                              
                              left join [MST].[LegalSalaryGradeDesignation] LSGD ON LSGD.LegalDesignationId = EI.LegalDesignationId and lsgd.PlantId=ei.PlantId
                              LEFT JOIN [SCS].[LegalSalaryGrade] LSG ON LSGD.LegalSalaryGradeId = LSG.Id                             
                              
                              
                              LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId = DM.Id AND dmc.PlantId=ei.PlantId                              
                              LEFT JOIN HKP.DesignationGroup DG ON DG.Id=DM.DesignationGroupId                            
                              
                              
                              Left Join SalaryInfoDefineMaster SM ON SM.EmpInfoSystemID=EI.SystemID 
                              WHERE EI.EmployeeStatus ='Active' AND EI.PlantId='" + plantId + @"' AND  EI.GroupId='" + companyGroupId + @"' AND ( EI.SystemId IN (SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster where  IsApproved=0) or  EI.SystemId Not IN (SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster) ) 

                               AND EI.SystemID NOT IN (SELECT EmpInfoSystemID FROM SalaryInfoBackMaster) AND EI.SystemID NOT IN (SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster where  IsApproved=1)";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public GridModel xGetSalaryStrcUnApprovedEmployee(GridParameter parameters, string companyGroupId, string plantId)
        {
            try
            {
                parameters.CmdText = @"SELECT EI.*,PO.UserName PresThanaName,ParmPO.UserName ParmThanaName,D.UserName PresDistrictName,ParmD.UserName ParmDistrictName
								  ,C.UserName PresCountryName,ParmC.UserName ParmCountryName,ParmP.UserName ParmPostOfficeName, PerP.UserName PresPostOfficeName
                                  ,PerCT.UserName PresCityName,ParCT.UserName ParmCityName,AM.CountryId
								  ,CG.[Image] CompanyGroupLogo, CNT.PhoneLength, COM.IsTINRequiredForSalaryAbove
								  ,CNT.TINCaption, CNT.NIDCaption, CNT.NIDLength, CNT.TINLength, COM.TINRequiredForSalaryAbove
								  ,DG.UserName GivenDesignation, DP.UserName Department, PMB.Code,PR.UserName PositionName,E.UserName EntityName,DSG.UserName Designation,PR.DesignationId,PG.StandardName PayRollGroupName,PG.Id PayRollGroupId
                              FROM dbo.Employeeinformation EI
                              LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id
							  LEFT JOIN scs.PoliceStation PO ON EI.PresThanaID=PO.Id
							  LEFT JOIN scs.PoliceStation ParmPO ON EI.ParmThanaID=ParmPO.Id
							  LEFT JOIN SCS.District D ON EI.PresDistrictID = D.Id
							  LEFT JOIN SCS.District ParmD ON EI.ParmDistrictID = ParmD.Id
		                      LEFT JOIN SCS.Country C ON EI.PresCountryID = C.ID
		                      LEFT JOIN SCS.Country ParmC	ON EI.ParmCountryID = ParmC.ID
		                      LEFT JOIN SCS.PostOffice ParmP ON EI.ParmPostOfficeID = ParmP.ID
		                      LEFT JOIN SCS.PostOffice PerP ON EI.PresPostOfficeID = PerP.ID
                              LEFT JOIN SCS.City PerCT ON EI.PresCityID = PerCT.ID
		                      LEFT JOIN SCS.City ParCT ON EI.ParmCityID = ParCT.ID
                              LEFT JOIN SCS.[State] ParmS ON EI.ParmStateId = ParmS.Id
							  LEFT JOIN SCS.[State] PresS ON EI.PresStateId = PresS.Id
							  LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id
							  LEFT JOIN MST.AddressMaster AM ON PL.AddressMasterId=AM.Id
							  LEFT JOIN SCS.Country CNT ON AM.CountryId=CNT.Id
							  LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                              LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                              LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                              LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
							  LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
							  LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
				              LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
							  Left join MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
							  Left Join hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
                              WHERE EI.EmployeeStatus ='Active' AND EI.PlantId='" + plantId + @"' AND  EI.GroupId='" + companyGroupId + "' AND ( EI.SystemId IN (SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster where  IsApproved=0) or  EI.SystemId Not IN (SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster) ) AND EI.SystemId Not IN (SELECT EmpInfoSystemID FROM SalaryInfoBackMaster)";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IEnumerable<object> GetSalaryStrcApprovedEmployee(string companyGroupId, string plantId)
        {
            try
            {
                string sql = @"SELECT EI.*,Replace(CONVERT(VARCHAR(11), EI.DOB, 106), ' ', '-') DOBs,Replace(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJs                                 
								  ,DG.UserName GivenDesignation, DP.UserName Department, PMB.Code,PR.UserName PositionName,E.UserName EntityName
								  ,DSG.UserName Designation
								  ,PR.DesignationId
								  ,PG.StandardName PayRollGroupName
								  ,PG.Id PayRollGroupId
								  , ApprovedStatus=case when SM.IsApproved=1 then 'Approved' when SM.IsApproved=0 then 'Un-approved' when SM.IsApproved is null then 'Not Defined' end  , DeG.UserName DesignationGroupName
								  ,PFPolicy=case when dmc.PFPolicyMasterID IS NOT NULL THEN 'Yes' ELSE 'No' END
								  ,ESICPolicy=case when dmc.ESICPolicyMasterID IS NOT NULL THEN 'Yes' ELSE 'No' END
								  ,BnsPolicy=case when dmc.BnsPlcMthRetainID IS NOT NULL THEN 'Yes' ELSE 'No' END
								  ,SalaryRule=case when dmc.SalaryRuleMasterId IS NOT NULL THEN 'Yes' ELSE 'No' END
								  ,ld.UserName LegalDesignation
								  ,ec.UserName EmployeeCategory
								  ,LSG.UserName SalaryGrade
                              FROM dbo.Employeeinformation EI                             
                              LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                              LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                              LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
							  LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
							  LEFT JOIN HKP.Designation DeG on DeG.Id=EI.GivenDesignationId
							  LEFT JOIN  HKP.LegalDesignation AS ld ON ld.Id=ei.LegalDesignationId----
							  
				              LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
							  Left join MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
							  Left Join hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
							  
							  LEFT JOIN mst.DesignationMasterLegalDesignation AS dmld ON dmld.LegalDesignationId=ei.LegalDesignationId---
                              LEFT JOIN MST.DesignationMaster DM ON DM.id=dmld.DesignationMasterId----
                              LEFT JOIN hkp.EmployeeCategory AS ec ON ec.Id=dm.EmployeeCategoryId-----
                              
                              left join [MST].[LegalSalaryGradeDesignation] LSGD ON LSGD.LegalDesignationId = EI.LegalDesignationId and lsgd.PlantId=ei.PlantId
                              LEFT JOIN [SCS].[LegalSalaryGrade] LSG ON LSGD.LegalSalaryGradeId = LSG.Id                             
                              
                              
                              LEFT JOIN scs.DesignationMasterConfiguration AS dmc ON dmc.DesignationMasterId = DM.Id AND dmc.PlantId=ei.PlantId                              
                              LEFT JOIN HKP.DesignationGroup DG ON DG.Id=DM.DesignationGroupId                            
                              
	                          LEFT JOIN TRN.EmployeeProbationalPeriod AS EP ON EP.Id = (SELECT TOP 1 Id FROM TRN.EmployeeProbationalPeriod WHERE EmployeeId = EI.SystemID	ORDER BY AddedDate DESC	) and EI.SystemId=EP.EmployeeId
							  LEFT JOIN IncrementHistory AS IH ON  IH.SystemID=(SELECT TOP 1 SystemID FROM IncrementHistory WHERE EmpSystemID= EI.SystemID
							  ORDER BY ConfirmationDate DESC) AND  EI.SystemId=IH.EmpSystemID
                              LEFT JOIN SalaryInfoDefineMaster SM ON SM.EmpInfoSystemID=EI.SystemId
                              AND sm.SystemID=(SELECT TOP 1 SystemID FROM SalaryInfoDefineMaster WHERE EmpInfoSystemID= EI.SystemID
							  ORDER BY   EffectiveDate DESC)

                              WHERE EI.EmployeeStatus ='Active' AND EI.PlantId='" + plantId + @"' AND  EI.GroupId='" + companyGroupId + @"' AND EI.SystemId IN (
                              SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster where IsApproved = 1
                              union
                              SELECT EmpInfoSystemID FROM SalaryInfoBackMaster where EmpInfoSystemID IN (SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster where IsApproved = 0)                            
                              union
                              SELECT EmpInfoSystemID FROM SalaryInfoBackMaster where EmpInfoSystemID NOT IN (SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster)
                              )";

         //       string xsql = @"SELECT EI.*,Replace(CONVERT(VARCHAR(11), EI.DOB, 106), ' ', '-') DOBs,Replace(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJs
         //                         ,PO.UserName PresThanaName,ParmPO.UserName ParmThanaName,D.UserName PresDistrictName,ParmD.UserName ParmDistrictName
								 // ,C.UserName PresCountryName,ParmC.UserName ParmCountryName,ParmP.UserName ParmPostOfficeName, PerP.UserName PresPostOfficeName
         //                         ,PerCT.UserName PresCityName,ParCT.UserName ParmCityName,AM.CountryId
								 // ,CG.[Image] CompanyGroupLogo, CNT.PhoneLength, COM.IsTINRequiredForSalaryAbove
								 // ,CNT.TINCaption, CNT.NIDCaption, CNT.NIDLength, CNT.TINLength, COM.TINRequiredForSalaryAbove
								 // ,DG.UserName GivenDesignation, DP.UserName Department, PMB.Code,PR.UserName PositionName,E.UserName EntityName,DSG.UserName Designation,PR.DesignationId,PG.StandardName PayRollGroupName,PG.Id PayRollGroupId
         //                         ,EP.Id,IH.IsConfirmation,IH.ConfirmationCode,
								 // IIF(EP.Id = IH.ConfirmationCode , 1, 0) as IsGross, 
								 // CASE WHEN ISNULL(EP.Id,'')<>'' AND ISNULL(IH.ConfirmationCode,'')='' THEN 1 ELSE 0 END AS IsPending
         //                         ,ISNULL(IIF(SM.IsApproved=1 , 'Approved', 'Un-approved'),'New') as ApprovedStatus, DeG.UserName DesignationGroupName
         //                     FROM dbo.Employeeinformation EI
         //                     LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id
							  //LEFT JOIN scs.PoliceStation PO ON EI.PresThanaID=PO.Id
							  //LEFT JOIN scs.PoliceStation ParmPO ON EI.ParmThanaID=ParmPO.Id
							  //LEFT JOIN SCS.District D ON EI.PresDistrictID = D.Id
							  //LEFT JOIN SCS.District ParmD ON EI.ParmDistrictID = ParmD.Id
		       //               LEFT JOIN SCS.Country C ON EI.PresCountryID = C.ID
		       //               LEFT JOIN SCS.Country ParmC	ON EI.ParmCountryID = ParmC.ID
		       //               LEFT JOIN SCS.PostOffice ParmP ON EI.ParmPostOfficeID = ParmP.ID
		       //               LEFT JOIN SCS.PostOffice PerP ON EI.PresPostOfficeID = PerP.ID
         //                     LEFT JOIN SCS.City PerCT ON EI.PresCityID = PerCT.ID
		       //               LEFT JOIN SCS.City ParCT ON EI.ParmCityID = ParCT.ID
         //                     LEFT JOIN SCS.[State] ParmS ON EI.ParmStateId = ParmS.Id
							  //LEFT JOIN SCS.[State] PresS ON EI.PresStateId = PresS.Id
							  //LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id
							  //LEFT JOIN MST.AddressMaster AM ON PL.AddressMasterId=AM.Id
							  //LEFT JOIN SCS.Country CNT ON AM.CountryId=CNT.Id
							  //LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
         //                     LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
         //                     LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
         //                     LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
							  //LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
							  //LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
				     //         LEFT JOIN ORG.Department DP on DP.Id=EI.DepartmentId
							  //LEFT JOIN MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
							  //LEFT JOIN hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
         //                     LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=EI.GivenDesignationId
         //                     LEFT JOIN HKP.DesignationGroup DeG ON DeG.Id=DM.DesignationGroupId
	        //                  LEFT JOIN TRN.EmployeeProbationalPeriod AS EP ON EP.Id = (SELECT TOP 1 Id FROM TRN.EmployeeProbationalPeriod WHERE EmployeeId = EI.SystemID	ORDER BY AddedDate DESC	) and EI.SystemId=EP.EmployeeId
							  //LEFT JOIN IncrementHistory AS IH ON  IH.SystemID=(SELECT TOP 1 SystemID FROM IncrementHistory WHERE EmpSystemID= EI.SystemID
							  //ORDER BY ConfirmationDate DESC) AND  EI.SystemId=IH.EmpSystemID
         //                     LEFT JOIN SalaryInfoDefineMaster SM ON SM.EmpInfoSystemID=EI.SystemId
         //                     AND sm.SystemID=(SELECT TOP 1 SystemID FROM SalaryInfoDefineMaster WHERE EmpInfoSystemID= EI.SystemID
							  //ORDER BY   EffectiveDate DESC)

         //                     WHERE EI.EmployeeStatus ='Active' AND EI.PlantId='" + plantId + @"' AND  EI.GroupId='" + companyGroupId + @"' AND EI.SystemId IN (
         //                     SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster where IsApproved = 1
         //                     union
         //                     SELECT EmpInfoSystemID FROM SalaryInfoBackMaster where EmpInfoSystemID IN (SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster where IsApproved = 0)                            
         //                     union
         //                     SELECT EmpInfoSystemID FROM SalaryInfoBackMaster where EmpInfoSystemID NOT IN (SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster)
         //                     )";





                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IEnumerable<object> xGetSalaryStrcApprovedEmployee(string companyGroupId, string plantId)
        {
            try
            {
                string sql = @"SELECT EI.*,Replace(CONVERT(VARCHAR(11), EI.DOB, 106), ' ', '-') DOBs,Replace(CONVERT(VARCHAR(11), EI.DOJ, 106), ' ', '-') DOJs
                                  ,PO.UserName PresThanaName,ParmPO.UserName ParmThanaName,D.UserName PresDistrictName,ParmD.UserName ParmDistrictName
								  ,C.UserName PresCountryName,ParmC.UserName ParmCountryName,ParmP.UserName ParmPostOfficeName, PerP.UserName PresPostOfficeName
                                  ,PerCT.UserName PresCityName,ParCT.UserName ParmCityName,AM.CountryId
								  ,CG.[Image] CompanyGroupLogo, CNT.PhoneLength, COM.IsTINRequiredForSalaryAbove
								  ,CNT.TINCaption, CNT.NIDCaption, CNT.NIDLength, CNT.TINLength, COM.TINRequiredForSalaryAbove
								  ,DG.UserName GivenDesignation, DP.UserName Department, PMB.Code,PR.UserName PositionName,E.UserName EntityName,DSG.UserName Designation,PR.DesignationId,PG.StandardName PayRollGroupName,PG.Id PayRollGroupId
                                  ,EP.Id,IH.IsConfirmation,IH.ConfirmationCode,
								  IIF(EP.Id = IH.ConfirmationCode , 1, 0) as IsGross, 
								  CASE WHEN ISNULL(EP.Id,'')<>'' AND ISNULL(IH.ConfirmationCode,'')='' THEN 1 ELSE 2 END AS IsPending
                                  ,ISNULL(IIF(SM.IsApproved=1 , 'Approved', 'Un-approved'),'New') as ApprovedStatus
                              FROM dbo.Employeeinformation EI
                              LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id
							  LEFT JOIN scs.PoliceStation PO ON EI.PresThanaID=PO.Id
							  LEFT JOIN scs.PoliceStation ParmPO ON EI.ParmThanaID=ParmPO.Id
							  LEFT JOIN SCS.District D ON EI.PresDistrictID = D.Id
							  LEFT JOIN SCS.District ParmD ON EI.ParmDistrictID = ParmD.Id
		                      LEFT JOIN SCS.Country C ON EI.PresCountryID = C.ID
		                      LEFT JOIN SCS.Country ParmC	ON EI.ParmCountryID = ParmC.ID
		                      LEFT JOIN SCS.PostOffice ParmP ON EI.ParmPostOfficeID = ParmP.ID
		                      LEFT JOIN SCS.PostOffice PerP ON EI.PresPostOfficeID = PerP.ID
                              LEFT JOIN SCS.City PerCT ON EI.PresCityID = PerCT.ID
		                      LEFT JOIN SCS.City ParCT ON EI.ParmCityID = ParCT.ID
                              LEFT JOIN SCS.[State] ParmS ON EI.ParmStateId = ParmS.Id
							  LEFT JOIN SCS.[State] PresS ON EI.PresStateId = PresS.Id
							  LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id
							  LEFT JOIN MST.AddressMaster AM ON PL.AddressMasterId=AM.Id
							  LEFT JOIN SCS.Country CNT ON AM.CountryId=CNT.Id
							  LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                              LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                              LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                              LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
							  LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
							  LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
				              LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
							  Left join MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
							  Left Join hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
	                          LEFT JOIN TRN.EmployeeProbationalPeriod AS EP ON EP.Id = (SELECT TOP 1 Id FROM TRN.EmployeeProbationalPeriod WHERE EmployeeId = EI.SystemID	ORDER BY AddedDate DESC	) and EI.SystemId=EP.EmployeeId
							  LEFT JOIN IncrementHistory AS IH ON  IH.SystemID=(SELECT TOP 1 SystemID FROM IncrementHistory WHERE EmpSystemID= EI.SystemID
							  ORDER BY ConfirmationDate DESC) AND  EI.SystemId=IH.EmpSystemID
                             --LEFT JOIN SalaryInfoDefineMaster SM ON SM.EmpInfoSystemID=EI.SystemId
 LEFT JOIN (
							  
							           SELECT X.EmpInfoSystemID,MAX(ed) ed
                                          ,ISNULL(M.IsApproved,0) IsApproved
                                          FROM(
                                         SELECT EmpInfoSystemID,MAX(EffectiveDate) ed 
                                         FROM SalaryInfoDefineMaster WHERE PlantId='" + plantId + @"'
                                        GROUP BY EmpInfoSystemID
                                        UNION
                                         SELECT EmpInfoSystemID,MAX(EffectiveDate) ed 
                                         FROM SalaryInfoBackMaster WHERE PlantId='" + plantId + @"'
                                        GROUP BY EmpInfoSystemID
                                        ) X
                                        LEFT JOIN (SELECT EmpInfoSystemID,EffectiveDate,IsApproved FROM SalaryInfoDefineMaster WHERE PlantId='" + plantId + @"'
                                        UNION
                                        SELECT EmpInfoSystemID,EffectiveDate,IsApproved FROM SalaryInfoBackMaster WHERE PlantId='" + plantId + @"'
                                        ) M ON M.EffectiveDate=X.ed AND M.EmpInfoSystemID=X.EmpInfoSystemID
                                        GROUP BY X.EmpInfoSystemID,M.IsApproved 
							                                          ) SM ON SM.EmpInfoSystemID=EI.SystemID 
                              WHERE EI.EmployeeStatus ='Active' AND EI.PlantId='" + plantId + @"' AND  EI.GroupId='" + companyGroupId + "' AND EI.SystemId IN (SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster where  IsApproved=1)";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        public IEnumerable<object> GetSalaryStrcApprovedEmployeeById(string EmpSystemId, string companyGroupId, string plantId)
        {
            try
            {
                string sql = @"SELECT EI.*,PO.UserName PresThanaName,ParmPO.UserName ParmThanaName,D.UserName PresDistrictName,ParmD.UserName ParmDistrictName
								  ,C.UserName PresCountryName,ParmC.UserName ParmCountryName,ParmP.UserName ParmPostOfficeName, PerP.UserName PresPostOfficeName
                                  ,PerCT.UserName PresCityName,ParCT.UserName ParmCityName,AM.CountryId
								  ,CG.[Image] CompanyGroupLogo, CNT.PhoneLength, COM.IsTINRequiredForSalaryAbove
								  ,CNT.TINCaption, CNT.NIDCaption, CNT.NIDLength, CNT.TINLength, COM.TINRequiredForSalaryAbove
								  ,DG.UserName GivenDesignation, DP.UserName Department, PMB.Code,PR.UserName PositionName,E.UserName EntityName,DSG.UserName Designation,PR.DesignationId,PG.StandardName PayRollGroupName,PG.Id PayRollGroupId
                              FROM dbo.Employeeinformation EI
                              LEFT JOIN ORG.CompanyGroup AS CG ON EI.GroupId=CG.Id
							  LEFT JOIN scs.PoliceStation PO ON EI.PresThanaID=PO.Id
							  LEFT JOIN scs.PoliceStation ParmPO ON EI.ParmThanaID=ParmPO.Id
							  LEFT JOIN SCS.District D ON EI.PresDistrictID = D.Id
							  LEFT JOIN SCS.District ParmD ON EI.ParmDistrictID = ParmD.Id
		                      LEFT JOIN SCS.Country C ON EI.PresCountryID = C.ID
		                      LEFT JOIN SCS.Country ParmC	ON EI.ParmCountryID = ParmC.ID
		                      LEFT JOIN SCS.PostOffice ParmP ON EI.ParmPostOfficeID = ParmP.ID
		                      LEFT JOIN SCS.PostOffice PerP ON EI.PresPostOfficeID = PerP.ID
                              LEFT JOIN SCS.City PerCT ON EI.PresCityID = PerCT.ID
		                      LEFT JOIN SCS.City ParCT ON EI.ParmCityID = ParCT.ID
                              LEFT JOIN SCS.[State] ParmS ON EI.ParmStateId = ParmS.Id
							  LEFT JOIN SCS.[State] PresS ON EI.PresStateId = PresS.Id
							  LEFT JOIN ORG.Plant PL ON EI.PlantId = PL.Id
							  LEFT JOIN MST.AddressMaster AM ON PL.AddressMasterId=AM.Id
							  LEFT JOIN SCS.Country CNT ON AM.CountryId=CNT.Id
							  LEFT JOIN ORG.Company COM ON EI.CompanyId=COM.Id
                              LEFT JOIN MST.ManpowerBudget PMB ON EI.BudgetCode=PMB.Id
                              LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                              LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
							  LEFT JOIN HKP.Designation DSG ON PR.DesignationId=DSG.Id
							  LEFT JOIN HKP.Designation DG on DG.Id=EI.GivenDesignationId
				              LEFT JOIN ORG.Department DP on DP.Id=PR.DepartmentId
							  Left join MST.payrollgroupmaster PM on PM.EmployeeId=EI.SystemId
							  Left Join hkp.payrollgroup PG on PG.Id=PM.PayRollGroupId
                              WHERE EI.EmployeeStatus ='Active' AND EI.SystemId='" + EmpSystemId + @"' AND EI.PlantId='" + plantId + @"' AND  EI.GroupId='" + companyGroupId + "' AND EI.SystemId IN (SELECT EmpInfoSystemID FROM SalaryInfoDefineMaster where  IsApproved=1)";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        #endregion




        public bool CheckEditableEmpByPayRollGroup(string empPayRollGroupId)
        {
            bool IsEditableEmp = false;
            DataSet dsPayRollGroupAccess = null;
            GetPayRollGroupAccess(out dsPayRollGroupAccess);
            try
            {

                if (dsPayRollGroupAccess.Tables[0].Rows.Count > 0)
                {
                    DataView dv = new DataView();

                    if (string.IsNullOrEmpty(empPayRollGroupId))
                    {
                        IsEditableEmp = true;
                    }
                    else
                    {
                        dv.Table = dsPayRollGroupAccess.Tables[0];
                        dv.RowFilter = "PayRollGroupId=" + empPayRollGroupId;
                        if (dv.Count > 0)
                        {
                            IsEditableEmp = true;
                        }
                    }
                }
                else
                {
                    //if (string.IsNullOrEmpty(empPayRollGroupId))
                    //{
                    //    IsEditableEmp = true;
                    //}

                    IsEditableEmp = true;
                }

                return IsEditableEmp;
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public void GetPayRollGroupAccess(out System.Data.DataSet dsRef)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"select UserId,PayRollGroupId from [SEC].[UserPayrollGroup] Where UserId='" + identity.UserId + @"'";

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
        public void GetPayRollGroupId(string EmployeeId, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"select Payrollgroupid from [MST].payrollgroupmaster Where EmployeeId='" + EmployeeId + @"'";

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



        void ValidateDate(DataSet dsLocal, string FieldName)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                if (dsLocal.Tables[0].Rows.Count == 0)
                {
                    bplib.clsWebLib.Throw("No " + FieldName + " found for Plant [" + identity.PlantName + "]");
                }

                if (string.IsNullOrEmpty(dsLocal.Tables[0].Rows[0][FieldName].ToString()))
                {
                    bplib.clsWebLib.Throw(" " + FieldName + " can not be blank for Plant [" + identity.PlantName + "] ");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void xLoadEmpSalaryInfoDefineData_OnGrid(out System.Data.DataSet ds)
        {
            ds = new DataSet();
            DataSet dsLocal = null;
            DataSet dsSlrDefChd = null;
            clsSalaryInfoNew objSal = null;
            bool bIsApproved = false;
            string SlrInfoDefMstSystemID = "";
            string SlrInfoDefSystemID = "";
            decimal decNetGross = 0;
            decimal decNetCTC = 0;
            string sRoundOption = "";
            bool bIntegerInDisb = false;
            bool bIsDecimalInDisb = false;
            int iDecimalNo = 0;
            try
            {
                //txtSalaryCalculationFormula.Text = "";
                objSal = new clsSalaryInfoNew();

                #region Table Create

                DataTable dt = new DataTable();
                dt.TableName = "TempTable";
                //dt.Columns.Add("IsSelectSlrHd", typeof(bool));                  //0
                dt.Columns.Add("SlrInfoDefSystemID");                           //1
                dt.Columns.Add("CurrencyRuleChildSystemID");                    //2
                dt.Columns.Add("SalaryHeadID");                                 //3
                dt.Columns.Add("SalaryHead");                                   //4
                dt.Columns.Add("HeadType");                                     //5
                dt.Columns.Add("FormulaDesID");                                 //6
                dt.Columns.Add("FixedValue");                                   //7
                dt.Columns.Add("IsOpen");                                       //8
                dt.Columns.Add("EntryCurrencyID");                              //9
                dt.Columns.Add("EntryCurrency");                                //10
                dt.Columns.Add("DefinitionCurrencyID");                         //11
                dt.Columns.Add("DefinitionCurrency");                           //12
                dt.Columns.Add("EntryAmount");                                  //13
                dt.Columns.Add("DefineAmount");                                 //14
                dt.Columns.Add("TagAndUnTag");                                  //15
                dt.Columns.Add("MonthPeriod");                                  //16
                dt.Columns.Add("IsNA");                                         //17
                dt.Columns.Add("HeadCategory");                                 //18
                dt.Columns.Add("BaseOnNetPay");                                 //19
                dt.Columns.Add("RefAbsentism");                                 //20
                dt.Columns.Add("IsCTCComponent");                               //21
                dt.Columns.Add("IsGrossComponent");                             //22
                dt.Columns.Add("IsGNRBaseOthSlrHD");                            //23
                dt.Columns.Add("GNRBaseOthSlrHDFormula");                       //24
                dt.Columns.Add("GNRApplicableMonthNo");                         //25
                dt.Columns.Add("EarningCurrencyID");                            //26
                dt.Columns.Add("EarningAmount");                                //27
                dt.Columns.Add("SequenceNo");                                   //28
                dt.Columns.Add("IsGNRWhichEverLess", typeof(bool));             //29
                dt.Columns.Add("RoundOption");                                  //30
                dt.Columns.Add("IntegerInDisb", typeof(bool));                  //31
                dt.Columns.Add("IsDecimalInDisb", typeof(bool));                //32
                dt.Columns.Add("DecimalNo");                                    //33
                dt.Columns.Add("SalaryCategory");                               //34
                dt.Columns.Add("SalaryHdSequence");                             //35

                #endregion Table Create


                //objSal.SalaryStructureAPHeadOnGrid(ddlPlant.SelectedValue.ToString().Trim(), lblEmpSystemId.Text.Trim(), lblStartmonth.Text, lblEndmonth.Text, TextEffectiveDate.Text, ddSalaryRule.SelectedValue, out dsLocal);
                if (dsLocal.Tables[0].Rows.Count > 0)
                {
                    //dgEmpSalaryDefine.DataSource = dsLocal.Tables[0];
                    //dgEmpSalaryDefine.DataBind();
                    //dgEmpSalaryDefine.Visible = true;
                    //PanEmpSalaryDefine.Visible = true;

                    //txtSalaryCalculationFormula.Visible = true;

                    for (int i = 0; i < dsLocal.Tables[0].Rows.Count; i++)
                    {
                        if (Convert.ToBoolean(dsLocal.Tables[0].Rows[i]["IsOpen"]) == false)
                        {
                            if (Convert.ToDecimal(dsLocal.Tables[0].Rows[i]["FixedValue"].ToString()) > 0)
                            {
                                //txtSalaryCalculationFormula.Text += "\n" + dsLocal.Tables[0].Rows[i]["SalaryHead"].ToString() + ": " + dsLocal.Tables[0].Rows[i]["FixedValue"].ToString();
                            }
                            else if (dsLocal.Tables[0].Rows[i]["FormulaDes"].ToString() != "")
                            {
                                //txtSalaryCalculationFormula.Text += "\n" + dsLocal.Tables[0].Rows[i]["SalaryHead"].ToString() + ": " + dsLocal.Tables[0].Rows[i]["FormulaDes"].ToString();
                            }
                        }

                        if (bIsApproved == false)
                        {
                            bIsApproved = Convert.ToBoolean(dsLocal.Tables[0].Rows[i]["IsApproved"]);
                        }

                        //lblSlrInfoDefSystemID.Text = dsLocal.Tables[0].Rows[0]["SlrInfoDefSystemID"].ToString();
                        //SlrInfoDefMstSystemID = lblSalaryID.Text;
                        if (SlrInfoDefSystemID == "")
                        {
                            SlrInfoDefSystemID = dsLocal.Tables[0].Rows[i]["SlrInfoDefSystemID"].ToString();
                        }
                        else
                        {
                            SlrInfoDefSystemID += "', '" + dsLocal.Tables[0].Rows[i]["SlrInfoDefSystemID"].ToString();
                        }
                    }


                    #region Data Load In Grid DataSet

                    for (int i = 0; i < dsLocal.Tables[0].Rows.Count; i++)
                    {
                        sRoundOption = dsLocal.Tables[0].Rows[i]["RoundOption"].ToString().Trim();
                        bIntegerInDisb = Convert.ToBoolean(dsLocal.Tables[0].Rows[i]["IntegerInDisb"].ToString().Trim());
                        bIsDecimalInDisb = Convert.ToBoolean(dsLocal.Tables[0].Rows[i]["IsDecimalInDisb"].ToString().Trim());
                        iDecimalNo = Convert.ToInt32(dsLocal.Tables[0].Rows[i]["DecimalNo"].ToString().Trim());

                        DataRow dtRow = dt.NewRow();
                        //dtRow["IsSelectSlrHd"] = Convert.ToBoolean(dsLocal.Tables[0].Rows[i]["IsSelectSlrHd"].ToString().Trim());
                        dtRow["SlrInfoDefSystemID"] = dsLocal.Tables[0].Rows[i]["SlrInfoDefSystemID"].ToString().Trim();
                        dtRow["CurrencyRuleChildSystemID"] = dsLocal.Tables[0].Rows[i]["CurrencyRuleChildSystemID"].ToString().Trim();
                        dtRow["SalaryHeadID"] = dsLocal.Tables[0].Rows[i]["SalaryHeadID"].ToString().Trim();
                        dtRow["SalaryHead"] = dsLocal.Tables[0].Rows[i]["SalaryHead"].ToString().Trim();
                        dtRow["HeadType"] = dsLocal.Tables[0].Rows[i]["HeadType"].ToString().Trim();
                        dtRow["FormulaDesID"] = dsLocal.Tables[0].Rows[i]["FormulaDesID"].ToString().Trim();
                        dtRow["FixedValue"] = dsLocal.Tables[0].Rows[i]["FixedValue"].ToString().Trim();
                        dtRow["IsOpen"] = Convert.ToBoolean(dsLocal.Tables[0].Rows[i]["IsOpen"].ToString().Trim());
                        dtRow["IsNA"] = Convert.ToBoolean(dsLocal.Tables[0].Rows[i]["IsNA"].ToString().Trim());
                        dtRow["EntryCurrencyID"] = dsLocal.Tables[0].Rows[i]["EntryCurrencyID"].ToString().Trim();
                        dtRow["EntryCurrency"] = dsLocal.Tables[0].Rows[i]["EntryCurrency"].ToString().Trim();
                        dtRow["DefinitionCurrencyID"] = dsLocal.Tables[0].Rows[i]["DefinitionCurrencyID"].ToString().Trim();
                        dtRow["DefinitionCurrency"] = dsLocal.Tables[0].Rows[i]["DefinitionCurrency"].ToString().Trim();
                        dtRow["EntryAmount"] = dsLocal.Tables[0].Rows[i]["EntryAmount"].ToString().Trim();
                        dtRow["DefineAmount"] = dsLocal.Tables[0].Rows[i]["DefineAmount"].ToString().Trim();
                        dtRow["TagAndUnTag"] = dsLocal.Tables[0].Rows[i]["TagAndUnTag"].ToString().Trim();
                        dtRow["MonthPeriod"] = dsLocal.Tables[0].Rows[i]["MonthPeriod"].ToString().Trim();
                        dtRow["HeadCategory"] = dsLocal.Tables[0].Rows[i]["HeadCategory"].ToString().Trim();

                        dtRow["BaseOnNetPay"] = dsLocal.Tables[0].Rows[i]["BaseOnNetPay"].ToString().Trim();
                        dtRow["RefAbsentism"] = dsLocal.Tables[0].Rows[i]["RefAbsentism"].ToString().Trim();
                        dtRow["IsCTCComponent"] = dsLocal.Tables[0].Rows[i]["IsCTCComponent"].ToString().Trim();
                        dtRow["IsGrossComponent"] = dsLocal.Tables[0].Rows[i]["IsGrossComponent"].ToString().Trim();
                        dtRow["IsGNRBaseOthSlrHD"] = dsLocal.Tables[0].Rows[i]["IsGNRBaseOthSlrHD"].ToString().Trim();
                        dtRow["GNRBaseOthSlrHDFormula"] = dsLocal.Tables[0].Rows[i]["GNRBaseOthSlrHDFormula"].ToString().Trim();
                        dtRow["GNRApplicableMonthNo"] = dsLocal.Tables[0].Rows[i]["GNRApplicableMonthNo"].ToString().Trim();
                        dtRow["EarningCurrencyID"] = dsLocal.Tables[0].Rows[i]["EarningCurrencyID"].ToString().Trim();
                        dtRow["EarningAmount"] = dsLocal.Tables[0].Rows[i]["EarningAmount"].ToString().Trim();
                        dtRow["SequenceNo"] = dsLocal.Tables[0].Rows[i]["SequenceNo"].ToString().Trim();
                        dtRow["IsGNRWhichEverLess"] = dsLocal.Tables[0].Rows[i]["IsGNRWhichEverLess"].ToString().Trim();

                        dtRow["RoundOption"] = sRoundOption;
                        dtRow["IntegerInDisb"] = bIntegerInDisb;
                        dtRow["IsDecimalInDisb"] = bIsDecimalInDisb;
                        dtRow["DecimalNo"] = iDecimalNo;
                        dtRow["SalaryCategory"] = dsLocal.Tables[0].Rows[i]["SalaryCategory"].ToString().Trim();
                        dtRow["SalaryHdSequence"] = dsLocal.Tables[0].Rows[i]["SalaryHdSequence"].ToString().Trim();
                        dt.Rows.Add(dtRow);
                    }

                    #endregion Data Load In Grid DataSet

                    objSal.SalaryStructureAPHeadOnGridAfterLoadGrid(SlrInfoDefMstSystemID, SlrInfoDefSystemID, out dsSlrDefChd);
                    if (dsSlrDefChd.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < dsSlrDefChd.Tables[0].Rows.Count; i++)
                        {
                            sRoundOption = dsSlrDefChd.Tables[0].Rows[i]["RoundOption"].ToString().Trim();
                            bIntegerInDisb = Convert.ToBoolean(dsSlrDefChd.Tables[0].Rows[i]["IntegerInDisb"].ToString().Trim());
                            bIsDecimalInDisb = Convert.ToBoolean(dsSlrDefChd.Tables[0].Rows[i]["IsDecimalInDisb"].ToString().Trim());
                            iDecimalNo = Convert.ToInt32(dsSlrDefChd.Tables[0].Rows[i]["DecimalNo"].ToString().Trim());

                            DataRow dtRow = dt.NewRow();
                            //dtRow["IsSelectSlrHd"] = Convert.ToBoolean(dsSlrDefChd.Tables[0].Rows[i]["IsSelectSlrHd"].ToString().Trim());
                            dtRow["SlrInfoDefSystemID"] = dsSlrDefChd.Tables[0].Rows[i]["SlrInfoDefSystemID"].ToString().Trim();
                            dtRow["CurrencyRuleChildSystemID"] = dsSlrDefChd.Tables[0].Rows[i]["CurrencyRuleChildSystemID"].ToString().Trim();
                            dtRow["SalaryHeadID"] = dsSlrDefChd.Tables[0].Rows[i]["SalaryHeadID"].ToString().Trim();
                            dtRow["SalaryHead"] = dsSlrDefChd.Tables[0].Rows[i]["SalaryHead"].ToString().Trim();
                            dtRow["HeadType"] = dsSlrDefChd.Tables[0].Rows[i]["HeadType"].ToString().Trim();
                            dtRow["FormulaDesID"] = dsSlrDefChd.Tables[0].Rows[i]["FormulaDesID"].ToString().Trim();
                            dtRow["FixedValue"] = dsSlrDefChd.Tables[0].Rows[i]["FixedValue"].ToString().Trim();
                            dtRow["IsOpen"] = Convert.ToBoolean(dsSlrDefChd.Tables[0].Rows[i]["IsOpen"].ToString().Trim());
                            dtRow["IsNA"] = Convert.ToBoolean(dsSlrDefChd.Tables[0].Rows[i]["IsNA"].ToString().Trim());
                            dtRow["EntryCurrencyID"] = dsSlrDefChd.Tables[0].Rows[i]["EntryCurrencyID"].ToString().Trim();
                            dtRow["EntryCurrency"] = dsSlrDefChd.Tables[0].Rows[i]["EntryCurrency"].ToString().Trim();
                            dtRow["DefinitionCurrencyID"] = dsSlrDefChd.Tables[0].Rows[i]["DefinitionCurrencyID"].ToString().Trim();
                            dtRow["DefinitionCurrency"] = dsSlrDefChd.Tables[0].Rows[i]["DefinitionCurrency"].ToString().Trim();
                            dtRow["EntryAmount"] = dsSlrDefChd.Tables[0].Rows[i]["EntryAmount"].ToString().Trim();
                            dtRow["DefineAmount"] = dsSlrDefChd.Tables[0].Rows[i]["DefineAmount"].ToString().Trim();
                            dtRow["TagAndUnTag"] = dsSlrDefChd.Tables[0].Rows[i]["TagAndUnTag"].ToString().Trim();
                            dtRow["MonthPeriod"] = dsSlrDefChd.Tables[0].Rows[i]["MonthPeriod"].ToString().Trim();
                            dtRow["HeadCategory"] = dsSlrDefChd.Tables[0].Rows[i]["HeadCategory"].ToString().Trim();

                            dtRow["BaseOnNetPay"] = dsSlrDefChd.Tables[0].Rows[i]["BaseOnNetPay"].ToString().Trim();
                            dtRow["RefAbsentism"] = dsSlrDefChd.Tables[0].Rows[i]["RefAbsentism"].ToString().Trim();
                            dtRow["IsCTCComponent"] = dsSlrDefChd.Tables[0].Rows[i]["IsCTCComponent"].ToString().Trim();
                            dtRow["IsGrossComponent"] = dsSlrDefChd.Tables[0].Rows[i]["IsGrossComponent"].ToString().Trim();
                            dtRow["IsGNRBaseOthSlrHD"] = dsSlrDefChd.Tables[0].Rows[i]["IsGNRBaseOthSlrHD"].ToString().Trim();
                            dtRow["GNRBaseOthSlrHDFormula"] = dsSlrDefChd.Tables[0].Rows[i]["GNRBaseOthSlrHDFormula"].ToString().Trim();
                            dtRow["GNRApplicableMonthNo"] = dsSlrDefChd.Tables[0].Rows[i]["GNRApplicableMonthNo"].ToString().Trim();
                            dtRow["EarningCurrencyID"] = dsSlrDefChd.Tables[0].Rows[i]["EarningCurrencyID"].ToString().Trim();
                            dtRow["EarningAmount"] = dsSlrDefChd.Tables[0].Rows[i]["EarningAmount"].ToString().Trim();
                            dtRow["SequenceNo"] = dsSlrDefChd.Tables[0].Rows[i]["SequenceNo"].ToString().Trim();
                            dtRow["IsGNRWhichEverLess"] = false;

                            dtRow["RoundOption"] = sRoundOption;
                            dtRow["IntegerInDisb"] = bIntegerInDisb;
                            dtRow["IsDecimalInDisb"] = bIsDecimalInDisb;
                            dtRow["DecimalNo"] = iDecimalNo;
                            dtRow["SalaryCategory"] = dsSlrDefChd.Tables[0].Rows[i]["SalaryCategory"].ToString().Trim();
                            dtRow["SalaryHdSequence"] = dsSlrDefChd.Tables[0].Rows[i]["SalaryHdSequence"].ToString().Trim();
                            dt.Rows.Add(dtRow);
                        }
                    }

                    ds.Tables.Add(dt);

                    DataView dv = new DataView();
                    dv.Table = dt;
                    //dv.Sort = "HeadType DESC, SalaryHead";
                    dv.Sort = "SalaryHdSequence";
                    var dg = dv.ToTable();
                    //dgEmpSalaryDefine.DataSource = dg;
                    ////dgEmpSalaryDefine.DataSource = dt;
                    //dgEmpSalaryDefine.DataBind();
                    //dgEmpSalaryDefine.Visible = true;
                    //PanEmpSalaryDefine.Visible = true;

                    decNetGross = 0;
                    decNetCTC = 0;

                    for (int i = 0; i < dg.Rows.Count; i++)
                    {
                        if (dg.Rows[i]["HeadType"].ToString() == "Earning")
                        {
                            if (Convert.ToBoolean(dg.Rows[i]["IsGrossComponent"].ToString()) == true)
                            { decNetGross += Convert.ToDecimal(dg.Rows[i]["EntryAmount"].ToString().Trim()); }
                            if (Convert.ToBoolean(dg.Rows[i]["IsCTCComponent"].ToString()) == true)
                            { decNetCTC += Convert.ToDecimal(dg.Rows[i]["EntryAmount"].ToString().Trim()); }
                        }
                    }

                    //if (decNetGross > 0)
                    //{
                    //    lblNetGross.Text = "Net Gross Amount: " + decNetGross.ToString("#,##0.00;(#,##0.00)");
                    //    lblNetGross.Visible = true;
                    //}
                    //else { lblNetGross.Visible = false; }

                    //if (decNetCTC > 0)
                    //{
                    //    lblNetCTC.Text = "Net CTC Amount: " + decNetCTC.ToString("#,##0.00;(#,##0.00)");
                    //    lblNetCTC.Visible = true;
                    //}
                    //else { lblNetCTC.Visible = false; }
                }
                else
                {
                    //dgEmpSalaryDefine.DataSource = null;
                    //dgEmpSalaryDefine.DataBind();
                    //dgEmpSalaryDefine.Visible = false;
                    //PanEmpSalaryDefine.Visible = false;

                    //lblNetGross.Text = "Net Gross Amount:";
                    //lblNetCTC.Text = "Net CTC Amount:";

                    //lblNetGross.Visible = false;
                    //lblNetCTC.Visible = false;
                    //txtSalaryCalculationFormula.Visible = false;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objSal = null;
                dsLocal = null;
            }
        }//End Function

        public void GetSysIdWiseEmpBasicInfoInformation(string sGroupID, string sCompanyID, string sPlantID, string strEmpSysID, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT * FROM
                                        (SELECT E.SystemID, E.EmployeeId, E.EmployeeCode , E.CardNumber, E.Salutation, E.FirstName, E.MiddleName, E.LastName,E.EmployeeCodePreFix,E.EmployeeCodeNumeric,
					                            E.EmployeeName, E.EmployeeNameLocal, E.NickName, E.EmpPicPath, E.EmpType, E.EmploymentType, '' UserGroupSystemID,
					                            '' CtlPrlGroupName, E.GroupID, GC.StandardName GroupName, E.CompanyID, CMP.StandardName CompanyName, E.PlantID, Pt.StandardName PlantName,
					                            REPLACE(Convert(varchar(11), E.DOB, 106),' ','-') AS DOB, REPLACE(Convert(varchar(11), E.DOJ, 106),' ','-') AS DOJ,
					                            E.DOCIsDay, E.DOCDay, E.DOCIsMonth, E.DOCMonth, REPLACE(Convert(varchar(11), E.DOC, 106),' ','-') AS DOC,
					                            REPLACE(Convert(varchar(11), E.DOS, 106),' ','-') AS DOS, REPLACE(Convert(varchar(11), E.ReActiveDate, 106),' ','-') AS ReActiveDate,
					                            E.EmployeeStatus, E.NationalID, E.CitizenID,E.TIN, Citi.StandardName CitizenName, E.FatherName,E.FatherNameLocal, E.MotherName,E.MotherNameLocal,
					                            E.ReligionID, Rg.StandardName ReligionName, E.CivilStatusID, CS.StandardName CivilStatusName, E.BloodGroupID, BG.StandardName BloodGroupName,
					                            E.GenderID, E.GenderID GenderName, E.SpouseName, E.SpouseNationalID, E.SpouseOccupation, E.NoOfChildren, E.PresentAddress1, E.PresentAddress2,
					                            E.ParmanentAddress1, E.ParmanentAddress2, E.PresThanaID, PresT.StandardName PresThanaName, E.ParmThanaID, ParmT.StandardName ParmThanaName,
					                            E.PresPostOfficeID, PresPO.StandardName PresPostOfficeName, E.ParmPostOfficeID, ParmPO.StandardName ParmPostOfficeName,
					                            E.PresZipCode, E.ParmZipCode, E.PresDistrictID, PresD.StandardName PresDistrictName, E.ParmDistrictID,
					                            ParmD.StandardName ParmDistrictName, E.PresCountryID, PresC.StandardName PresCountry, E.ParmCountryID, ParmC.StandardName ParmCountry,
					                            E.TelePhnNo, E.CellPhnNo, E.EmailID, EN.UnitID, U.StandardName UnitName,
					                            PC.DivisionID, Dv.StandardName DivisionName, PC.DepartmentID, De.StandardName DepartmentName, PC.SectionID, Se.StandardName SectionName, PC.SubSectionID,
					                            SuS.StandardName SubSectionName, PMB.LineID, Ln.StandardName LineName, E.BudgetCategoryID, EBC.StandardName BudgetCategoryName, E.EmployeeCategorySystemID,
                                                E.SubSecStrucSystemID, SSSM.Description SubSectionStructureDes, SSSM.Code SubSectionStructureCode,
					                            EC.UserName EmpCategoryName, DM.DesignationGroupID, DM.DesignationID, DG.StandardName DesignationGroupName, Dsg.StandardName DesignationName,
					                            E.LVPolicyMasterSystemID, DGM.LeavePolicyMasterId, LPM.PolicyName LeavePolicyName, E.SalaryRuleMasterSystemID, SRM.SalaryRuleName,
					                            E.BankSystemID, E.BankName, E.BankAccNo, E.RegisterFP, E.RegisterProximate, E.IsSlvDevReg, R.EmployeeStatue ResignStatue,
                                                E.EmployeeGroupSystemID, E.JobLocationID, ISNULL(E.IsConfirmed,0)IsConfirmed, JbLc.JobLocation, SRM.CurrencyRuleSystemID, SID.SlrRulMstSystemID IncrementSlrRulMstSystemID,
                                                REPLACE(Convert(varchar(11), SID.EffectiveDate, 106),' ','-') AS IncrementEffectiveDate,EmrCntPer2CellNo
												,EmrCntPer2Name,EmrCntPer1CellNo2,EmrCntPer1CellNo3,EmrCntPer2CellNo2,EmrCntPer2CellNo3
												,EmrCntPer1CellNo,pmb.Code BudgetCodeName, E.SalaryPercentage
												,EmrCntPer1Name,E.BudgetCode,PC.SubdivisionID,E.IsDirect,PMB.PositionId,PC.UserName PositionName
                                                ,E.PresCityID, E.ParmCityID, E.PresAreaID, E.ParmAreaID
		                                        ,PresCT.UserName PresCity, ParmCT.UserName ParmCity, PresAR.UserName PresArea, ParmAR.UserName ParmaArea
                                                ,E.GivenDesignationId,E.LegalDesignationId,Dsgg.UserName GivenDesignation
                                                , tge.TaxGroupID TaxGrpEmpSystemID,SD.UserName Subdivision
                                                , DGM.DesignationGroupId GivenDesignationGroupId, DGM.UserName GivenDesignationGroup
                                                ,EG.UserName EmployeeGroupName
                                                ,tgr.TaxGroupName, dgSRM.TaxGroupID TaxGroupIDSR
                                                ,REPLACE(Convert(varchar(11),E.BirthdayCelebrationDate, 106),' ','-') AS BirthdayCelebrationDate
                                                ,REPLACE(Convert(varchar(11),E.MarriagedayCelebrationDate, 106),' ','-') AS MarriagedayCelebrationDate
												,E.PresStateId,E.ParmStateId,E.PresentArea,E.ParmanentArea,E.Height,E.Weight,E.IdentificationMark,E.LocalIdentificationMark,
												E.PreviouslyWorkedHere,E.PreviousEmployeeCode,E.PreviousDesignation,E.PreviousSalary,E.PreviousServicePeriod,E.ExitReason,
												E.AnyRelativeWorkedHere,E.RelativeSystemId,E.PresentAddress1Local,E.PresentAddress2Local,E.ParmanentAddress1Local,E.ParmanentAddress2Local
												,E.RelationShip,E.RelativeCellNo,E.ExitReasonLocal,E.SpouseNameLocal,E.PreviousDesignationLocal,E.EmpSignature,E.PaymentMode,LDSg.UserName LegalDesignation
				                            FROM EmployeeInformation AS E
                                                    LEFT OUTER JOIN
                                                                [MST].[ManpowerBudget] pmb on e.BudgetCode=pmb.Id
                                                    LEFT OUTER JOIN
																[ORG].[Position] AS PC ON PMB.PositionID  = PC.ID
                                                    LEFT JOIN ORG.Entity EN ON PMB.EntityId=EN.Id
                                                    LEFT JOIN MST.DesignationMaster DM ON DM.DesignationId=E.GivenDesignationID
                                                    LEFT JOIN HKP.EmployeeCategory EBC ON EBC.Id=DM.EmployeeCategoryId
LEFT OUTER JOIN
																[HKP].Designation AS Dsg ON Dsg.ID = DM.DesignationId
						                            LEFT OUTER JOIN
									                            [ORG].[CompanyGroup] GC ON E.GroupID = GC.ID
						                            LEFT OUTER JOIN
									                            [ORG].[Company] CMP ON E.CompanyID = CMP.ID
						                            LEFT OUTER JOIN
									                            [ORG].Plant Pt ON E.PlantID = Pt.ID
						                            LEFT OUTER JOIN
									                            [SCS].[Country] Citi ON E.CitizenID = Citi.ID
						                            LEFT OUTER JOIN
									                            [SCS].Religion Rg ON E.ReligionID = Rg.ID
						                            LEFT OUTER JOIN
									                            HKP.CivilStatus AS CS ON E.CivilStatusID  = CS.ID
						                            LEFT OUTER JOIN
									                            [HKP].[BloodGroup] AS BG ON E.BloodGroupID  = BG.ID
						                            
						                            LEFT OUTER JOIN
									                            [SCS].[PoliceStation] AS PresT ON E.PresThanaID  = PresT.ID
						                            LEFT OUTER JOIN
									                            [SCS].[PoliceStation] AS ParmT ON E.ParmThanaID  = ParmT.ID
						                            LEFT OUTER JOIN
									                            [SCS].[PostOffice] AS PresPO ON E.PresPostOfficeID  = PresPO.ID
						                            LEFT OUTER JOIN
									                            [SCS].[PostOffice] AS ParmPO ON E.ParmPostOfficeID  = ParmPO.ID
						                            LEFT OUTER JOIN
									                            [SCS].[District] AS PresD ON E.PresDistrictID  = PresD.ID
						                            LEFT OUTER JOIN
									                            [SCS].[District] AS ParmD ON E.ParmDistrictID  = ParmD.ID
						                            LEFT OUTER JOIN
									                            [SCS].Country AS PresC ON E.PresCountryID  = PresC.ID
						                            LEFT OUTER JOIN
									                            [SCS].Country AS ParmC ON E.ParmCountryID  = ParmC.ID
                                                    LEFT OUTER JOIN
                                                                [SCS].[City] PresCT ON E.PresCityID = PresCT.Id
			                            			LEFT OUTER JOIN
                                                                [SCS].[City] ParmCT ON E.ParmCityID = ParmCT.Id
			                                        LEFT OUTER JOIN
                                                                [SCS].[Area] PresAR ON E.PresAreaID = PresAR.Id
			                                        LEFT OUTER JOIN
                                                                [SCS].[Area] ParmAR ON E.ParmAreaID = ParmAR.Id
                                                    LEFT OUTER JOIN	[HKP].LegalDesignation AS LDSg ON LDSg.ID = E.LegalDesignationId
						                            LEFT OUTER JOIN
																--[HKP].[EmployeeCategory] AS EC ON E.EmployeeCategorySystemID = EC.ID
																(
                                                                SELECT ECT.Id, ECT.UserName, DM.DesignationId FROM [HKP].[EmployeeCategory] ECT
																LEFT JOIN MST.DesignationMaster DM ON ECT.Id=DM.EmployeeCategoryId
																)EC ON EC.DesignationId=E.GivenDesignationId
													LEFT OUTER JOIN
																[ORG].[Unit] AS U ON U.ID = EN.UnitID
													LEFT OUTER JOIN
																[ORG].Division AS Dv ON Dv.ID = PC.DivisionID
													LEFT OUTER JOIN
																[ORG].Department AS De ON De.ID = PC.DepartmentID
                                                    LEFT OUTER JOIN
																[HKP].Designation AS Dsgg ON Dsgg.ID = E.GivenDesignationID
													LEFT OUTER JOIN
																[ORG].Section AS Se ON Se.ID = PC.SectionID
													LEFT OUTER JOIN
																[ORG].SubSection AS SuS ON SuS.ID = PC.SubSectionID
									                LEFT OUTER JOIN
									                            [ORG].Line AS Ln ON Ln.ID = PMB.LineID
                                                    LEFT OUTER JOIN
									                            [ORG].SubDivision AS SD ON SD.Id = PC.SubdivisionID
									                LEFT OUTER JOIN
									                            [TRN].[SubsectionStructureMaster] AS SSSM ON SSSM.ID = E.SubSecStrucSystemID
                                                    
						                            LEFT OUTER JOIN
									                            [HKP].[DesignationGroup] DG ON DM.DesignationGroupID = DG.ID
                                                    LEFT OUTER JOIN
									                            (               SELECT DC.SalaryRuleMasterId,dc.PlantId,dm.*,dc.LeavePolicyMasterId
                                                                                FROM MST.DesignationMaster DM
							                                                    LEFT JOIN SCS.DesignationMasterConfiguration DC 
                                                                                            ON DM.Id=DC.DesignationMasterId
                                                    ) DGM ON E.GivenDesignationID = DGM.DesignationId and E.PlantId=DGM.PlantId
						                            LEFT OUTER JOIN
									                            SalaryRuleMaster dgSRM ON DGM.SalaryRuleMasterId = dgSRM.SystemID
                                                    LEFT OUTER JOIN
																[dbo].[TaxGroupTagWithEmployee] tge on tge.EmpInfoSystemID=E.SystemId
						                            LEFT OUTER JOIN
									                            LeavePolicyMaster LPM ON DGM.LeavePolicyMasterId = LPM.SystemID
						                            LEFT OUTER JOIN
									                            SalaryRuleMaster SRM ON E.SalaryRuleMasterSystemID = SRM.SystemID
                                                    LEFT OUTER JOIN
									                            TaxGroup tgr ON SRM.TaxGroupID = tgr.SystemID
                                                    LEFT OUTER JOIN
									                            JobLocation JbLc ON E.JobLocationID = JbLc.SystemID
                                                    LEFT OUTER JOIN
									                           [HKP].[EmployeeGroup]  EG ON E.EmployeeGroupSystemID= EG.Id
						                            LEFT OUTER JOIN
									                            (
                                                                 SELECT TOP (1) * FROM SalaryIncrementInfoDefineMaster WHERE IsApproved = 0
                                                                ) SID ON E.SystemID = SID.EmpInfoSystemID
                                                    LEFT OUTER JOIN
									                            Resign R ON E.SystemID = R.EmpInfoSystemID AND IsEffected = 1
							         WHERE E.GroupID = '" + sGroupID + @"' AND E.CompanyID = '" + sCompanyID + @"'
                                              AND E.PlantID = '" + sPlantID + @"') A
                         WHERE SystemID = '" + strEmpSysID + @"'
                        Order By EmployeeCodePreFix,EmployeeCodeNumeric";

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



        public DataSet GivenDesignationChange(string GivenDesignationId, string CompanyGroupId, string PlantId)
        {
            DataSet dsRef = null;
            ConnectionManager.DAL.ConManager objCon;
            string strSql = @"select dmc.SalaryRuleMasterId from mst.DesignationMaster dm 
            join scs.DesignationMasterConfiguration dmc
            on dm.Id = dmc.DesignationMasterId where dm.DesignationId = " + GivenDesignationId + @" and dmc.CompanyGroupId = '" + CompanyGroupId + @"' and dmc.Plantid = '" + PlantId + @"'";
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
                return dsRef;
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
        public DataSet GetApprovedEffectiveDateAndNextDueDate(string EmpSystemId, string PlantId)
        {
            DataSet dsRef = null;
            ConnectionManager.DAL.ConManager objCon;
            string strSql = @"  select  FORMAT( y.NextDueDate,'dd-MMM-yyyy') NextDueDate, FORMAT( x.EffectiveDate,'dd-MMM-yyyy') EffectiveDate from (	
                                SELECT Max(EffectiveDate) EffectiveDate FROM					
                                (SELECT Max(EffectiveDate) EffectiveDate FROM SalaryInfoDefineMaster  WHERE EmpInfoSystemID =  '" + EmpSystemId + @"' and IsApproved =1 and PlantId='" + PlantId + @"' 
                                union
                                SELECT Max(EffectiveDate) EffectiveDate FROM SalaryInfoBackMaster  WHERE EmpInfoSystemID =  '" + EmpSystemId + @"' and IsApproved =1 and PlantId='" + PlantId + @"' )s

                                ) x
                                left join 
                                ( select EmpSystemId,EffectiveDate,NextDueDate from SalaryIncrementNextDueDate  WHERE EmpSystemId =  '" + EmpSystemId + @"' and PlantId='" + PlantId + @"' 
                                ) y 
                                on x.EffectiveDate=y.EffectiveDate where x.EffectiveDate is not null";

            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
                return dsRef;
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
        public DataSet GetUnApprovedEffectiveDateAndNextDueDate(string EmpSystemId)
        {
            DataSet dsRef = null;
            ConnectionManager.DAL.ConManager objCon;
            string strSql = @"Select REPLACE(CONVERT(VARCHAR(11), Max(NextDueDate), 106),' ','-') NextDueDate  from
                            (
                            select NextDueDate from SalaryIncrementNextDueDate where  empsystemid='" + EmpSystemId + @"' and EffectiveDate=(SELECT Max(EffectiveDate) EffectiveDate FROM SalaryInfoDefineMaster where empinfosystemid='" + EmpSystemId + @"' and IsApproved=0 ))x";

            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
                return dsRef;
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


        public void SalaryRuleChange(string EmpSystemId, string SalaryRuleId, out IEnumerable<OpenHeadModel> ResultOpenHead)
        {
            DataSet dsbasicInfo = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GetSysIdWiseEmpBasicInfoInformation(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, EmpSystemId, out dsbasicInfo);


            clsSalaryInfoNew obs = null;

            DateTime _EffectiveDate = GetEffectiveDate(EmpSystemId);
            obs = new clsSalaryInfoNew();

            var EffectiveDate = _EffectiveDate.ToString("dd-MMM-yyyy");

            DataSet dsOpenHead = null;

            obs.SalaryOpenHeadOnGrid(identity.PlantId, SalaryRuleId, EmpSystemId, EffectiveDate, out dsOpenHead);


            #region open head
            //open head
            ResultOpenHead = dsOpenHead.Tables[0].AsEnumerable().Select(
               dataRow => new OpenHeadModel
               {
                   SalaryHeadID = dataRow.Field<string>("SalaryHeadID"),
                   SalaryHead = dataRow.Field<string>("SalaryHead"),
                   Description = dataRow.Field<string>("Description"),
                   SalaryRuleDescription = dataRow.Field<string>("SalaryRuleDescription"),
                   HeadType = dataRow.Field<string>("HeadType"),
                   EntryCurrency = dataRow.Field<string>("EntryCurrency"),
                   Amount = dataRow.Field<decimal>("Amount"),
                   HeadCategory = dataRow.Field<string>("HeadCategory"),
                   EffectiveDate = dataRow.Field<DateTime?>("EffectiveDate"),
                   SalaryID = dataRow.Field<string>("SalaryID"),
                   SalaryHdSequence = dataRow.Field<int>("SalaryHdSequence")

               }).ToList();

            #endregion open head




            //return Json(new { EmpSalaryInfoDefine = ResultData, ResultMinWage, ResultSalaryRule, ResultSelectedSalaryRule, ResultOpenHead, ResultGross, ResultNetCTC, Message = AplosMessage.Updated }, JsonRequestBehavior.AllowGet);

        }//End Function
        public void GetFomulaDetails(string EmpSystemId, string salaryRuleMasterSystemID, out string _Formula_Desc)
        {
            //DataTable dtResult = null;

            //DataSet dsOpenHead = Library.Service.Helpers.DataTableExtensions.ToDataSet<OpenHeadModel>(OpenHeadNew);
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DateTime _EffectiveDate = GetEffectiveDate(EmpSystemId);
            var EffectiveDate = _EffectiveDate.ToString("dd-MMM-yyyy");

            CustomParaNew _para = new CustomParaNew();
            _para.PlantId = identity.PlantId;
            _para.CompanyId = identity.CompanyId;
            _para.CompanyGroupId = identity.CompanyGroupId;
            _para.EmployeeId = EmpSystemId;
            _para.SalaryRuleId = salaryRuleMasterSystemID;
            _para.EffectiveDate = EffectiveDate;




            clsSalaryStructureAplosNew obj = new clsSalaryStructureAplosNew();




            try
            {
                obj.GetFormulaDetails(_para, out _Formula_Desc);



            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataSet GetSalaryRuleForDefaultSet(string GivenDesignationId, string CompanyGroupId, string PlantId)
        {
            DataSet dsRef = null;
            ConnectionManager.DAL.ConManager objCon;
            string strSql = @"select dmc.SalaryRuleMasterId from mst.DesignationMaster dm 
            join scs.DesignationMasterConfiguration dmc
            on dm.Id = dmc.DesignationMasterId where dm.DesignationId = " + GivenDesignationId + @" and dmc.CompanyGroupId = '" + CompanyGroupId + @"' and dmc.Plantid = '" + PlantId + @"'";
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strSql, out dsRef, false, "1");
                return dsRef;
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
        //========================1===============================

        public void LoadEmpSalaryInfoDefineData_OnGrid(string EmpSystemId, out System.Data.DataSet ds)
        {


            DataSet dsbasicInfo = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GetSysIdWiseEmpBasicInfoInformation(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, EmpSystemId, out dsbasicInfo);
            clsSalaryInfoNew obs = null;
            DataSet dsRule = null;

            DateTime _EffectiveDate = GetEffectiveDate(EmpSystemId);
            var sSlrRuleMstSystemID = dsbasicInfo.Tables[0].Rows[0]["SalaryRuleMasterSystemID"].ToString().Trim();
            obs = new clsSalaryInfoNew();
            obs.LoadLatestSalaryStructure(identity.CompanyGroupId, identity.PlantId, EmpSystemId, out dsRule);

            ds = new DataSet();
            DataSet dsLocal = null;
            DataSet dsSlrDefChd = null;
            clsSalaryInfoNew objSal = null;
            bool bIsApproved = false;
            string SlrInfoDefMstSystemID = "";
            string SlrInfoDefSystemID = "";
            decimal decNetGross = 0;
            decimal decNetCTC = 0;
            string sRoundOption = "";
            bool bIntegerInDisb = false;
            bool bIsDecimalInDisb = false;
            int iDecimalNo = 0;
            try
            {
                //txtSalaryCalculationFormula.Text = "";
                objSal = new clsSalaryInfoNew();

                #region Table Create

                DataTable dt = new DataTable();
                dt.TableName = "TempTable";

                dt.Columns.Add("SlrInfoDefSystemID");                           //1
                dt.Columns.Add("CurrencyRuleChildSystemID");                    //2
                dt.Columns.Add("SalaryHeadID");                                 //3
                dt.Columns.Add("SalaryHead");                                   //4
                dt.Columns.Add("HeadType");                                     //5
                dt.Columns.Add("FormulaDesID");                                 //6
                dt.Columns.Add("FixedValue");                                   //7
                dt.Columns.Add("IsOpen");                                       //8
                dt.Columns.Add("EntryCurrencyID");                              //9
                dt.Columns.Add("EntryCurrency");                                //10
                dt.Columns.Add("DefinitionCurrencyID");                         //11
                dt.Columns.Add("DefinitionCurrency");                           //12
                dt.Columns.Add("EntryAmount");                                  //13
                dt.Columns.Add("DefineAmount");                                 //14
                dt.Columns.Add("TagAndUnTag");                                  //15
                dt.Columns.Add("MonthPeriod");                                  //16
                dt.Columns.Add("IsNA");                                         //17
                dt.Columns.Add("HeadCategory");                                 //18
                dt.Columns.Add("BaseOnNetPay");                                 //19
                dt.Columns.Add("RefAbsentism");                                 //20
                dt.Columns.Add("IsCTCComponent");                               //21
                dt.Columns.Add("IsGrossComponent");                             //22
                dt.Columns.Add("IsGNRBaseOthSlrHD");                            //23
                dt.Columns.Add("GNRBaseOthSlrHDFormula");                       //24
                dt.Columns.Add("GNRApplicableMonthNo");                         //25
                dt.Columns.Add("EarningCurrencyID");                            //26
                dt.Columns.Add("EarningAmount");                                //27
                dt.Columns.Add("SequenceNo");                                   //28
                dt.Columns.Add("IsGNRWhichEverLess", typeof(bool));             //29
                dt.Columns.Add("RoundOption");                                  //30
                dt.Columns.Add("IntegerInDisb", typeof(bool));                  //31
                dt.Columns.Add("IsDecimalInDisb", typeof(bool));                //32
                dt.Columns.Add("DecimalNo");                                    //33
                dt.Columns.Add("SalaryCategory");                               //34
                dt.Columns.Add("SalaryHdSequence");                             //35

                #endregion Table Create

                //if (string.IsNullOrEmpty(this.ddlTaxYear.SelectedValue.Trim()) == false)
                //{
                //objSal.SalaryStructureAPHeadOnGrid(identity.PlantId, EmpSystemId, "", "", TextEffectiveDate.Text, ddSalaryRule.SelectedValue, out dsLocal);
                objSal.SalaryStructureAPHeadOnGrid(identity.PlantId, EmpSystemId, "", "", _EffectiveDate.ToString("dd-MMM-yyyy"), sSlrRuleMstSystemID, out dsLocal);
                if (dsLocal.Tables[0].Rows.Count > 0)
                {
                    //dgEmpSalaryDefine.DataSource = dsLocal.Tables[0];
                    //dgEmpSalaryDefine.DataBind();
                    //dgEmpSalaryDefine.Visible = true;
                    //PanEmpSalaryDefine.Visible = true;

                    /// txtSalaryCalculationFormula.Visible = true;

                    for (int i = 0; i < dsLocal.Tables[0].Rows.Count; i++)
                    {
                        if (Convert.ToBoolean(dsLocal.Tables[0].Rows[i]["IsOpen"]) == false)
                        {
                            if (Convert.ToDecimal(dsLocal.Tables[0].Rows[i]["FixedValue"].ToString()) > 0)
                            {
                                /// txtSalaryCalculationFormula.Text += "\n" + dsLocal.Tables[0].Rows[i]["SalaryHead"].ToString() + ": " + dsLocal.Tables[0].Rows[i]["FixedValue"].ToString();
                            }
                            else if (dsLocal.Tables[0].Rows[i]["FormulaDes"].ToString() != "")
                            {
                                // txtSalaryCalculationFormula.Text += "\n" + dsLocal.Tables[0].Rows[i]["SalaryHead"].ToString() + ": " + dsLocal.Tables[0].Rows[i]["FormulaDes"].ToString();
                            }
                        }

                        if (bIsApproved == false)
                        {
                            bIsApproved = Convert.ToBoolean(dsLocal.Tables[0].Rows[i]["IsApproved"]);
                        }

                        //lblSlrInfoDefSystemID.Text = dsLocal.Tables[0].Rows[0]["SlrInfoDefSystemID"].ToString();
                        //SlrInfoDefMstSystemID = lblSalaryID.Text;
                        if (SlrInfoDefSystemID == "")
                        {
                            SlrInfoDefSystemID = dsLocal.Tables[0].Rows[i]["SlrInfoDefSystemID"].ToString();
                        }
                        else
                        {
                            SlrInfoDefSystemID += "', '" + dsLocal.Tables[0].Rows[i]["SlrInfoDefSystemID"].ToString();
                        }
                    }

                    if (bIsApproved == true)
                    {
                        //this.lblApprovalStatus.ForeColor = Color.DarkBlue;
                        //this.lblApprovalStatus.Text = "Approved";

                        //////this.btnDefine.Enabled = false;
                        //this.Button_Delete.Enabled = false;
                        ////this.btnDefine.Enabled = false;
                        //this.btnPFChkUnChk.Enabled = false;
                        ////this.Button_save.Enabled = false;
                    }
                    else
                    {
                        //this.lblApprovalStatus.ForeColor = Color.Green;
                        //this.lblApprovalStatus.Text = "UnApproved";
                        //this.btnDefine.Enabled = true;
                        //this.Button_save.Enabled = true;
                        //this.btnPFChkUnChk.Enabled = true;
                    }

                    #region Data Load In Grid DataSet

                    for (int i = 0; i < dsLocal.Tables[0].Rows.Count; i++)
                    {
                        sRoundOption = dsLocal.Tables[0].Rows[i]["RoundOption"].ToString().Trim();
                        bIntegerInDisb = Convert.ToBoolean(dsLocal.Tables[0].Rows[i]["IntegerInDisb"].ToString().Trim());
                        bIsDecimalInDisb = Convert.ToBoolean(dsLocal.Tables[0].Rows[i]["IsDecimalInDisb"].ToString().Trim());
                        iDecimalNo = Convert.ToInt32(dsLocal.Tables[0].Rows[i]["DecimalNo"].ToString().Trim());

                        DataRow dtRow = dt.NewRow();
                        //dtRow["IsSelectSlrHd"] = Convert.ToBoolean(dsLocal.Tables[0].Rows[i]["IsSelectSlrHd"].ToString().Trim());
                        dtRow["SlrInfoDefSystemID"] = dsLocal.Tables[0].Rows[i]["SlrInfoDefSystemID"].ToString().Trim();
                        dtRow["CurrencyRuleChildSystemID"] = dsLocal.Tables[0].Rows[i]["CurrencyRuleChildSystemID"].ToString().Trim();
                        dtRow["SalaryHeadID"] = dsLocal.Tables[0].Rows[i]["SalaryHeadID"].ToString().Trim();
                        dtRow["SalaryHead"] = dsLocal.Tables[0].Rows[i]["SalaryHead"].ToString().Trim();
                        dtRow["HeadType"] = dsLocal.Tables[0].Rows[i]["HeadType"].ToString().Trim();
                        dtRow["FormulaDesID"] = dsLocal.Tables[0].Rows[i]["FormulaDesID"].ToString().Trim();
                        dtRow["FixedValue"] = dsLocal.Tables[0].Rows[i]["FixedValue"].ToString().Trim();
                        dtRow["IsOpen"] = Convert.ToBoolean(dsLocal.Tables[0].Rows[i]["IsOpen"].ToString().Trim());
                        dtRow["IsNA"] = Convert.ToBoolean(dsLocal.Tables[0].Rows[i]["IsNA"].ToString().Trim());
                        dtRow["EntryCurrencyID"] = dsLocal.Tables[0].Rows[i]["EntryCurrencyID"].ToString().Trim();
                        dtRow["EntryCurrency"] = dsLocal.Tables[0].Rows[i]["EntryCurrency"].ToString().Trim();
                        dtRow["DefinitionCurrencyID"] = dsLocal.Tables[0].Rows[i]["DefinitionCurrencyID"].ToString().Trim();
                        dtRow["DefinitionCurrency"] = dsLocal.Tables[0].Rows[i]["DefinitionCurrency"].ToString().Trim();
                        dtRow["EntryAmount"] = dsLocal.Tables[0].Rows[i]["EntryAmount"].ToString().Trim();
                        dtRow["DefineAmount"] = dsLocal.Tables[0].Rows[i]["DefineAmount"].ToString().Trim();
                        dtRow["TagAndUnTag"] = dsLocal.Tables[0].Rows[i]["TagAndUnTag"].ToString().Trim();
                        dtRow["MonthPeriod"] = dsLocal.Tables[0].Rows[i]["MonthPeriod"].ToString().Trim();
                        dtRow["HeadCategory"] = dsLocal.Tables[0].Rows[i]["HeadCategory"].ToString().Trim();

                        dtRow["BaseOnNetPay"] = dsLocal.Tables[0].Rows[i]["BaseOnNetPay"].ToString().Trim();
                        dtRow["RefAbsentism"] = dsLocal.Tables[0].Rows[i]["RefAbsentism"].ToString().Trim();
                        dtRow["IsCTCComponent"] = dsLocal.Tables[0].Rows[i]["IsCTCComponent"].ToString().Trim();
                        dtRow["IsGrossComponent"] = dsLocal.Tables[0].Rows[i]["IsGrossComponent"].ToString().Trim();
                        dtRow["IsGNRBaseOthSlrHD"] = dsLocal.Tables[0].Rows[i]["IsGNRBaseOthSlrHD"].ToString().Trim();
                        dtRow["GNRBaseOthSlrHDFormula"] = dsLocal.Tables[0].Rows[i]["GNRBaseOthSlrHDFormula"].ToString().Trim();
                        dtRow["GNRApplicableMonthNo"] = dsLocal.Tables[0].Rows[i]["GNRApplicableMonthNo"].ToString().Trim();
                        dtRow["EarningCurrencyID"] = dsLocal.Tables[0].Rows[i]["EarningCurrencyID"].ToString().Trim();
                        dtRow["EarningAmount"] = dsLocal.Tables[0].Rows[i]["EarningAmount"].ToString().Trim();
                        dtRow["SequenceNo"] = dsLocal.Tables[0].Rows[i]["SequenceNo"].ToString().Trim();
                        dtRow["IsGNRWhichEverLess"] = dsLocal.Tables[0].Rows[i]["IsGNRWhichEverLess"].ToString().Trim();

                        dtRow["RoundOption"] = sRoundOption;
                        dtRow["IntegerInDisb"] = bIntegerInDisb;
                        dtRow["IsDecimalInDisb"] = bIsDecimalInDisb;
                        dtRow["DecimalNo"] = iDecimalNo;
                        dtRow["SalaryCategory"] = dsLocal.Tables[0].Rows[i]["SalaryCategory"].ToString().Trim();
                        dtRow["SalaryHdSequence"] = dsLocal.Tables[0].Rows[i]["SalaryHdSequence"].ToString().Trim();
                        dt.Rows.Add(dtRow);
                    }

                    #endregion Data Load In Grid DataSet

                    objSal.SalaryStructureAPHeadOnGridAfterLoadGrid(SlrInfoDefMstSystemID, SlrInfoDefSystemID, out dsSlrDefChd);
                    if (dsSlrDefChd.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < dsSlrDefChd.Tables[0].Rows.Count; i++)
                        {
                            sRoundOption = dsSlrDefChd.Tables[0].Rows[i]["RoundOption"].ToString().Trim();
                            bIntegerInDisb = Convert.ToBoolean(dsSlrDefChd.Tables[0].Rows[i]["IntegerInDisb"].ToString().Trim());
                            bIsDecimalInDisb = Convert.ToBoolean(dsSlrDefChd.Tables[0].Rows[i]["IsDecimalInDisb"].ToString().Trim());
                            iDecimalNo = Convert.ToInt32(dsSlrDefChd.Tables[0].Rows[i]["DecimalNo"].ToString().Trim());

                            DataRow dtRow = dt.NewRow();
                            //dtRow["IsSelectSlrHd"] = Convert.ToBoolean(dsSlrDefChd.Tables[0].Rows[i]["IsSelectSlrHd"].ToString().Trim());
                            dtRow["SlrInfoDefSystemID"] = dsSlrDefChd.Tables[0].Rows[i]["SlrInfoDefSystemID"].ToString().Trim();
                            dtRow["CurrencyRuleChildSystemID"] = dsSlrDefChd.Tables[0].Rows[i]["CurrencyRuleChildSystemID"].ToString().Trim();
                            dtRow["SalaryHeadID"] = dsSlrDefChd.Tables[0].Rows[i]["SalaryHeadID"].ToString().Trim();
                            dtRow["SalaryHead"] = dsSlrDefChd.Tables[0].Rows[i]["SalaryHead"].ToString().Trim();
                            dtRow["HeadType"] = dsSlrDefChd.Tables[0].Rows[i]["HeadType"].ToString().Trim();
                            dtRow["FormulaDesID"] = dsSlrDefChd.Tables[0].Rows[i]["FormulaDesID"].ToString().Trim();
                            dtRow["FixedValue"] = dsSlrDefChd.Tables[0].Rows[i]["FixedValue"].ToString().Trim();
                            dtRow["IsOpen"] = Convert.ToBoolean(dsSlrDefChd.Tables[0].Rows[i]["IsOpen"].ToString().Trim());
                            dtRow["IsNA"] = Convert.ToBoolean(dsSlrDefChd.Tables[0].Rows[i]["IsNA"].ToString().Trim());
                            dtRow["EntryCurrencyID"] = dsSlrDefChd.Tables[0].Rows[i]["EntryCurrencyID"].ToString().Trim();
                            dtRow["EntryCurrency"] = dsSlrDefChd.Tables[0].Rows[i]["EntryCurrency"].ToString().Trim();
                            dtRow["DefinitionCurrencyID"] = dsSlrDefChd.Tables[0].Rows[i]["DefinitionCurrencyID"].ToString().Trim();
                            dtRow["DefinitionCurrency"] = dsSlrDefChd.Tables[0].Rows[i]["DefinitionCurrency"].ToString().Trim();
                            dtRow["EntryAmount"] = dsSlrDefChd.Tables[0].Rows[i]["EntryAmount"].ToString().Trim();
                            dtRow["DefineAmount"] = dsSlrDefChd.Tables[0].Rows[i]["DefineAmount"].ToString().Trim();
                            dtRow["TagAndUnTag"] = dsSlrDefChd.Tables[0].Rows[i]["TagAndUnTag"].ToString().Trim();
                            dtRow["MonthPeriod"] = dsSlrDefChd.Tables[0].Rows[i]["MonthPeriod"].ToString().Trim();
                            dtRow["HeadCategory"] = dsSlrDefChd.Tables[0].Rows[i]["HeadCategory"].ToString().Trim();

                            dtRow["BaseOnNetPay"] = dsSlrDefChd.Tables[0].Rows[i]["BaseOnNetPay"].ToString().Trim();
                            dtRow["RefAbsentism"] = dsSlrDefChd.Tables[0].Rows[i]["RefAbsentism"].ToString().Trim();
                            dtRow["IsCTCComponent"] = dsSlrDefChd.Tables[0].Rows[i]["IsCTCComponent"].ToString().Trim();
                            dtRow["IsGrossComponent"] = dsSlrDefChd.Tables[0].Rows[i]["IsGrossComponent"].ToString().Trim();
                            dtRow["IsGNRBaseOthSlrHD"] = dsSlrDefChd.Tables[0].Rows[i]["IsGNRBaseOthSlrHD"].ToString().Trim();
                            dtRow["GNRBaseOthSlrHDFormula"] = dsSlrDefChd.Tables[0].Rows[i]["GNRBaseOthSlrHDFormula"].ToString().Trim();
                            dtRow["GNRApplicableMonthNo"] = dsSlrDefChd.Tables[0].Rows[i]["GNRApplicableMonthNo"].ToString().Trim();
                            dtRow["EarningCurrencyID"] = dsSlrDefChd.Tables[0].Rows[i]["EarningCurrencyID"].ToString().Trim();
                            dtRow["EarningAmount"] = dsSlrDefChd.Tables[0].Rows[i]["EarningAmount"].ToString().Trim();
                            dtRow["SequenceNo"] = dsSlrDefChd.Tables[0].Rows[i]["SequenceNo"].ToString().Trim();
                            dtRow["IsGNRWhichEverLess"] = false;

                            dtRow["RoundOption"] = sRoundOption;
                            dtRow["IntegerInDisb"] = bIntegerInDisb;
                            dtRow["IsDecimalInDisb"] = bIsDecimalInDisb;
                            dtRow["DecimalNo"] = iDecimalNo;
                            dtRow["SalaryCategory"] = dsSlrDefChd.Tables[0].Rows[i]["SalaryCategory"].ToString().Trim();
                            dtRow["SalaryHdSequence"] = dsSlrDefChd.Tables[0].Rows[i]["SalaryHdSequence"].ToString().Trim();
                            dt.Rows.Add(dtRow);
                        }
                    }

                    ds.Tables.Add(dt);

                    DataView dv = new DataView();
                    dv.Table = dt;
                    //dv.Sort = "HeadType DESC, SalaryHead";
                    dv.Sort = "SalaryHdSequence";
                    var dg = dv.ToTable();
                    //dgEmpSalaryDefine.DataSource = dg;
                    ////dgEmpSalaryDefine.DataSource = dt;
                    //dgEmpSalaryDefine.DataBind();
                    //dgEmpSalaryDefine.Visible = true;
                    //PanEmpSalaryDefine.Visible = true;

                    decNetGross = 0;
                    decNetCTC = 0;

                    for (int i = 0; i < dg.Rows.Count; i++)
                    {
                        if (dg.Rows[i]["HeadType"].ToString() == "Earning")
                        {
                            if (Convert.ToBoolean(dg.Rows[i]["IsGrossComponent"].ToString()) == true)
                            { decNetGross += Convert.ToDecimal(dg.Rows[i]["EntryAmount"].ToString().Trim()); }
                            if (Convert.ToBoolean(dg.Rows[i]["IsCTCComponent"].ToString()) == true)
                            { decNetCTC += Convert.ToDecimal(dg.Rows[i]["EntryAmount"].ToString().Trim()); }
                        }
                    }

                    if (decNetGross > 0)
                    {
                        //lblNetGross.Text = "Net Gross Amount: " + decNetGross.ToString("#,##0.00;(#,##0.00)");
                        //lblNetGross.Visible = true;
                    }
                    else
                    {
                        //lblNetGross.Visible = false;
                    }

                    if (decNetCTC > 0)
                    {
                        //lblNetCTC.Text = "Net CTC Amount: " + decNetCTC.ToString("#,##0.00;(#,##0.00)");
                        //lblNetCTC.Visible = true;
                    }
                    else
                    {
                        //lblNetCTC.Visible = false;
                    }
                }
                else
                {
                    //dgEmpSalaryDefine.DataSource = null;
                    //dgEmpSalaryDefine.DataBind();
                    //dgEmpSalaryDefine.Visible = false;
                    //PanEmpSalaryDefine.Visible = false;

                    //lblNetGross.Text = "Net Gross Amount:";
                    //lblNetCTC.Text = "Net CTC Amount:";

                    //lblNetGross.Visible = false;
                    //lblNetCTC.Visible = false;
                    //txtSalaryCalculationFormula.Visible = false;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objSal = null;
                dsLocal = null;
            }
        }//End Function


        public IEnumerable<object> GetApprovedSalaryDetails(string EmpSystemId, string EffictiveDate, string SalaryRuleMasterSystemID)
        {
            try
            {
                string sql = @" SELECT * FROM (
                              SELECT --SDM.EmpInfoSystemID,FORMAT(EffectiveDate,'dd-MMM-yyyy') EffectiveDate--,SH.*

                                --SH.HeadCategory
                                1 IsSelectSlrHd
                                ,SIDD.systemid   SlrInfoDefSystemID
                                ,SDM.EmpInfoSystemID
                                ,SDM.SalaryRuleMasterSystemID
                                ,SRM.SalaryRuleName
                                ,SRM.SalaryRuleDescription
                                ,'' CurrencyRuleSystemID---
                                ,'' CurrencyRuleChildSystemID--
                                ,'' TagAndUnTag--
                                ,SIDD.SalaryHeadID
                                ,SH.SalaryHead
                                ,SH.HeadType
                                ,SIDD.EntryCurrencyID
                                ,'' EntryCurrency--
                                ,'' DefinitionCurrencyID--
                                ,'' DefinitionCurrency--
                                ,'' DisbusmentCurrencyID--
                                ,'' DisbusmentCurrency--
                                ,'' FormulaDes--
                                ,'' FormulaDesID--
                                ,'' FixedValue--
                                ,'' IsOpen--
                                ,'' IsNA--
                                ,SIDD.EntryAmount
                                ,SIDD.DefineAmount
                                ,SIDD.SequenceNo
                                ,FORMAT(SDM.EffectiveDate,'dd-MMM-yyyy') EffectiveDate
                                ,'' EndDate--
                                ,'' MonthPeriod--
                                ,SIDD.AmtDefinitionCurrencyID
                                ,SIDD.AmtDefinitionRate
                                ,SDM.IsApproved
                                ,'' HeadCategory--
                                ,'' BaseOnNetPay--
                                ,'' RefAbsentism--
                                ,SH.IsCTCComponent
                                ,SH.IsGrossComponent
                                ,'' IsGNRBaseOthSlrHD--
                                ,'' GNRBaseOthSlrHDFormula--
                                ,'' GNRApplicableMonthNo--
                                ,'' EarningCurrencyID--
                                ,'' EarningAmount--
                                ,'' IsGNRWhichEverLess--
                                ,'' RoundOption--
                                ,'' IntegerInDisb--
                                ,'' IsDecimalInDisb
                                ,'' DecimalNo
                                ,SIDD.SalaryCategory
                                ,SH.Sequence SalaryHdSequence--
                                ,SIDD.SalaryID


								
								
								
                                FROM SalaryInfoDefineMaster SDM
                                LEFT JOIN salaryinfodefine SIDD ON SIDD.SalaryID=SDM.SystemID
                                LEFT JOIN SalaryHead SH on SH.SalaryHeadID=SIDD.SalaryHeadID
                                LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID=SDM.SalaryRuleMasterSystemID
                                Where SDM.EmpInfoSystemID='" + EmpSystemId + @"' and SDM.EffectiveDate='" + EffictiveDate + @"' --and SDM.SalaryRuleMasterSystemID='" + SalaryRuleMasterSystemID + @"'
                                union
                                SELECT        --SH.HeadCategory
                                1 IsSelectSlrHd
                                ,SIDD.systemid   SlrInfoDefSystemID
                                ,SDM.EmpInfoSystemID
                                ,SDM.SalaryRuleMasterSystemID
                                ,SRM.SalaryRuleName
                                ,SRM.SalaryRuleDescription
                                ,'' CurrencyRuleSystemID---
                                ,'' CurrencyRuleChildSystemID--
                                ,'' TagAndUnTag--
                                ,SIDD.SalaryHeadID
                                ,SH.SalaryHead
                                ,SH.HeadType
                                ,SIDD.EntryCurrencyID
                                ,'' EntryCurrency--
                                ,'' DefinitionCurrencyID--
                                ,'' DefinitionCurrency--
                                ,'' DisbusmentCurrencyID--
                                ,'' DisbusmentCurrency--
                                ,'' FormulaDes--
                                ,'' FormulaDesID--
                                ,'' FixedValue--
                                ,'' IsOpen--
                                ,'' IsNA--
                                ,SIDD.EntryAmount
                                ,SIDD.DefineAmount
                                ,SIDD.SequenceNo
                                ,FORMAT(SDM.EffectiveDate,'dd-MMM-yyyy') EffectiveDate
                                ,'' EndDate--
                                ,'' MonthPeriod--
                                ,SIDD.AmtDefinitionCurrencyID
                                ,SIDD.AmtDefinitionRate
                                ,SDM.IsApproved
                                ,'' HeadCategory--
                                ,'' BaseOnNetPay--
                                ,'' RefAbsentism--
                                ,SH.IsCTCComponent
                                ,SH.IsGrossComponent
                                ,'' IsGNRBaseOthSlrHD--
                                ,'' GNRBaseOthSlrHDFormula--
                                ,'' GNRApplicableMonthNo--
                                ,'' EarningCurrencyID--
                                ,'' EarningAmount--
                                ,'' IsGNRWhichEverLess--
                                ,'' RoundOption--
                                ,'' IntegerInDisb--
                                ,'' IsDecimalInDisb
                                ,'' DecimalNo
                                ,SIDD.SalaryCategory
                                ,SH.Sequence SalaryHdSequence--
                                ,SIDD.SalaryID

                                FROM SalaryInfoBackMaster SDM
                                LEFT JOIN salaryinfoback SIDD ON SIDD.SalaryID=SDM.SystemID
                                LEFT JOIN SalaryHead SH on SH.SalaryHeadID=SIDD.SalaryHeadID
                                LEFT JOIN SalaryRuleMaster SRM ON SRM.SystemID=SDM.SalaryRuleMasterSystemID
                                Where SDM.EmpInfoSystemID='" + EmpSystemId + @"' and SDM.EffectiveDate='" + EffictiveDate + @"' --and SDM.SalaryRuleMasterSystemID='" + SalaryRuleMasterSystemID + @"' 
                                ) a";

                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        //===========================******


        public void GetEmpSalaryInfoDefineSummaryData(string CompanyGroupId, string PlantId, string EmpSystemId)
        {

            string TextEffectiveDate = string.Empty;





            clsSalaryInfoNew obs = null;
            DataSet dsLocal = null;
            DataSet ds = null;
            try
            {
                #region Search For


                #region Clear

                ClearEmpBasicInfo();
                //DGNullify(dgSalaryOpen, dgEmpSalaryDefine);

                #endregion

                obs = new clsSalaryInfoNew();

                obs.LoadLatestSalaryStructure(CompanyGroupId, PlantId, EmpSystemId, out dsLocal);
                LoadEmployeeInfo();



                DateTime _EffectiveDate = GetEffectiveDate(EmpSystemId);
                TextEffectiveDate = _EffectiveDate.ToString("dd-MMM-yyyy");


                if (dsLocal.Tables[0].Rows.Count == 1)
                {
                    //lblSalaryID.Text = dsLocal.Tables[0].Rows[0]["SystemID"].ToString().Trim();

                    LoadSalaryStructureInfo();

                }
                else if (dsLocal.Tables[0].Rows.Count > 0)
                {
                    //TextEffectiveDate.Enabled = false;
                    bplib.clsWebLib.Throw("Incremented Salary Structure can't be viewed here... ");
                }
                else//0
                {
                    LoadSalaryStructureInfo();

                }

                //lblSearchFor.Text = "";
                //PanelSearch.Visible = false;
                //PanelFactory.Visible = true;
                //PanelConfirmation.Visible = false;


                //lblSalaryID.Text = string.Empty;
                LoadEmpSlrOpenHdData_OnGrid();
                LoadEmpSalaryInfoDefineData_OnGrid(EmpSystemId, out ds);
                LoadSlrRuleInfo();
                //ddl Salary rule change event end 
                DataSet dsMaxED = null;
                obs.GetPossibleMaxEffectiveDate(EmpSystemId, out dsMaxED);
                if (dsMaxED.Tables[0].Rows.Count > 0)
                {
                    //TextEffectiveDate.Text = Convert.ToDateTime(dsMaxED.Tables[0].Rows[0]["ed"].ToString()).AddDays(1).ToString("dd-MMM-yyyy");
                }
                GetMinWage();
                ///TextEffectiveDate.Enabled = true;
                LoadDynamicData();

                #endregion Search For

                //DataGridSearch.DataSource = null;
                //DataGridSearch.DataBind();

                //Button_LogOff.Visible = true;
                //PanelSearch.Visible = false;
            }
            catch (Exception ex)
            {

            }
            finally
            {
            }
        }
        private void LoadEmployeeInfo()
        {
            DataSet dsLocal = null;
            DataSet dsSetting = null;
            DataSet dsSalaryRule = null;
            DataSet dsEmpImage = null;
            clsEmployeeLoad objApp = null;

            try
            {
                // lblImg.Text = "0";

                objApp = new clsEmployeeLoad();
                objApp.GetSysIdWiseEmpBasicInfoInformation("", "", "", "", out dsLocal);
                if (dsLocal.Tables[0].Rows.Count > 0)
                {
                    //lblEmpSystemId.Text = "" + dsLocal.Tables[0].Rows[0]["SystemID"].ToString();

                    //txtEmpCode.Text = "" + dsLocal.Tables[0].Rows[0]["EmployeeCode"].ToString();
                    //lblEmpName.Text = "" + dsLocal.Tables[0].Rows[0]["EmployeeName"].ToString();
                    //lblEmpDOJ.Text = "" + dsLocal.Tables[0].Rows[0]["DOJ"].ToString();
                    //lblBudgetCode.Text = "" + dsLocal.Tables[0].Rows[0]["BudgetCodeName"].ToString();
                    //lblBudgetCodeId.Text = "" + dsLocal.Tables[0].Rows[0]["BudgetCode"].ToString();
                    //lblDesignationGroupId.Text = "" + dsLocal.Tables[0].Rows[0]["DesignationGroupId"].ToString();
                    //lblGivenDesignation.Text = "" + dsLocal.Tables[0].Rows[0]["GivenDesignation"].ToString();
                    //lblGivenDesignationId.Text = "" + dsLocal.Tables[0].Rows[0]["GivenDesignationId"].ToString();

                    string _EmployeeCategorySystemID = dsLocal.Tables[0].Rows[0]["EmployeeCategorySystemID"].ToString();
                    //SR manual select
                    //lblSalaryRuleSysID.Text = "" + dsLocal.Tables[0].Rows[0]["SalaryRuleMasterSystemID"].ToString();
                    //ddSalaryRule.Enabled = true;
                    //lblGivenDesignationSalaryRule.Text = dsLocal.Tables[0].Rows[0]["SalaryRuleMasterSystemID"].ToString();


                    //lblLegalDesignation.Text = dsLocal.Tables[0].Rows[0]["LegalDesignation"].ToString();
                    //lblEmployeeCategory.Text = dsLocal.Tables[0].Rows[0]["EmpCategoryName"].ToString();
                    //lblPaymentMode.Text = dsLocal.Tables[0].Rows[0]["PaymentMode"].ToString();

                    //get setting
                    objApp.GetSettingPlantWise("", out dsSetting);
                    if (dsSetting.Tables[0].Rows.Count > 0)//Budgetcode wise auto select
                    {
                        //get Salary rule ap BudgetCode
                        //string GivenDesignationId = lblGivenDesignationId.Text;
                        //if (GivenDesignationId.Trim().Length == 0)
                        //{
                        //    throw new Exception("Given Designaiton can not be blank...");
                        //}

                        if (_EmployeeCategorySystemID.Trim().Length == 0)
                        {
                            //throw new Exception("Employee Category can not be blank...");
                        }

                        DataSet dsDGroup = null;
                        //objApp.GetDesignationGroup(GivenDesignationId, out dsDGroup);
                        //if (dsDGroup.Tables[0].Rows.Count == 0)
                        //{
                        //    bplib.clsWebLib.Throw("Designation:[" + lblGivenDesignation + "] is not found");
                        //}
                        //else
                        //{
                        //    if (string.IsNullOrEmpty(dsDGroup.Tables[0].Rows[0]["DesignationGroup"].ToString()))
                        //    {
                        //        bplib.clsWebLib.Throw("'Designation Master' is not defined for Designation:[" + lblGivenDesignation + "]");
                        //    }
                        //}
                        //objApp.GetSalaryRule(GivenDesignationId, ddlPlant.SelectedValue, out dsSalaryRule);
                        //if (dsSalaryRule.Tables[0].Rows.Count > 0)
                        //{
                        //    //Salary Rule not by setting but as per individual entry
                        //    ddSalaryRule.SelectedValue = lblGivenDesignationSalaryRule.Text;
                        //    ddSalaryRule.Enabled = false;
                        //}
                        //else
                        //{
                        //    throw new Exception("No Salary Rule found for Designation Group [" + dsDGroup.Tables[0].Rows[0]["DesignationGroup"].ToString() + "] and Plant [" + ddlPlant.Items[ddlPlant.SelectedIndex].Text + "]");
                        //}
                    }


                    //lblUnit.Text = "" + dsLocal.Tables[0].Rows[0]["UnitName"].ToString();


                    if (string.IsNullOrEmpty(dsLocal.Tables[0].Rows[0]["BankSystemID"].ToString().Trim()) == false)
                    {

                    }

                    //lblDivision.Text = "" + dsLocal.Tables[0].Rows[0]["DivisionName"].ToString();
                    //lblDepartment.Text = "" + dsLocal.Tables[0].Rows[0]["DepartmentName"].ToString();
                    //lblSection.Text = "" + dsLocal.Tables[0].Rows[0]["SectionName"].ToString();
                    //lblSubSection.Text = "" + dsLocal.Tables[0].Rows[0]["SubSectionName"].ToString();
                    //lblBudgetCategory.Text = "" + dsLocal.Tables[0].Rows[0]["BudgetCategoryName"].ToString();
                    //lblEmpCategor.Text = "" + dsLocal.Tables[0].Rows[0]["EmpCategoryName"].ToString();
                    //lblDesignationGroup.Text = "" + dsLocal.Tables[0].Rows[0]["DesignationGroupName"].ToString();
                    //lblDesignation.Text = "" + dsLocal.Tables[0].Rows[0]["DesignationName"].ToString();

                    //lblEmpStatus.Text = "" + dsLocal.Tables[0].Rows[0]["EmployeeStatus"].ToString();
                    //lblEmpResignStatus.Text = "" + dsLocal.Tables[0].Rows[0]["ResignStatue"].ToString();

                    //txtTin.Text = "" + dsLocal.Tables[0].Rows[0]["Tin"].ToString();
                    //lblTaxGrpEmpSystemID.Text = "" + dsLocal.Tables[0].Rows[0]["TaxGrpEmpSystemID"].ToString();
                    //if (dsLocal.Tables[0].Rows[0]["TaxGroupIDSR"].ToString().Trim().Length > 0)
                    //{
                    //    ddlTaxGroup.SelectedValue = dsLocal.Tables[0].Rows[0]["TaxGroupIDSR"].ToString();
                    //}
                    //else
                    //{
                    //    throw new Exception("No 'Tax Group' is assigned for selected salary rule...");
                    //}

                    //ImgEmpPic.ImageUrl = ResourcesPathReader.ShowEmployeePicture() + lblEmpSystemId.Text + ".jpg";


                    //objApp.SaveEmployeeImage(lblEmpSystemId.Text.ToString().Trim(), out dsEmpImage);
                    //if (dsEmpImage.Tables[0].Rows.Count > 0)
                    //{
                    //    if (dsEmpImage.Tables[0].Rows[0]["EmpImage"].ToString() != "")
                    //    {
                    //        ImgEmpPic.ImageUrl = "~/Handler1.ashx?EmpSystemID=" + dsEmpImage.Tables[0].Rows[0]["EmpSystemID"].ToString();

                    //    }
                    //}
                }
                else
                {
                    throw new Exception("No Data found ...");
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                dsLocal = null;
            }
        }//End Function 
        private void LoadDynamicData()
        {
            DataSet dsLocal = null;
            DataSet dsTaxYear = null;
            DataSet ds = null;

            clsTax objTax = null;
            clsSalaryInfoNew objSal = null;

            try
            {
                objTax = new clsTax();
                objSal = new clsSalaryInfoNew();

                //ddSalaryRule.DataSource = dsLocal;
                //ddSalaryRule.DataBind();

                //objSal.GetSalaryRule((string)Session["LOGIN_GROUP_ID"].ToString().Trim(), ddlPlant.SelectedValue.ToString().Trim(), "", out dsLocal);
                //ddSalaryRule.DataSource = dsLocal;
                //ddSalaryRule.DataTextField = "SalaryRuleName";
                //ddSalaryRule.DataValueField = "SystemID";
                //ddSalaryRule.DataBind();
                //ddSalaryRule.Items.Insert(0, "");

                //if (ddSalaryRule.Items.Count == 2)
                //{
                //    ddSalaryRule.SelectedIndex = 1;
                //    //xLoadTaxPolicyInfo();
                //    LoadEmpSlrOpenHdData_OnGrid();
                //    LoadEmpSalaryInfoDefineData_OnGrid(out ds);
                //    LoadSlrRuleInfo();
                //}
                //else
                //{
                //    ddSalaryRule.SelectedIndex = -1;
                //}

                //dsLocal = null;

                //objSal.GetLocalCurrency((string)Session["LOGIN_GROUP_ID"].ToString().Trim(), ddlPlant.SelectedValue.ToString().Trim(), out dsLocal);
                //if (dsLocal.Tables[0].Rows.Count > 0)
                //{
                //    lblLocalCurrency.Text = "" + dsLocal.Tables[0].Rows[0]["Currency"].ToString().Trim();
                //    lblLocalCurrencyID.Text = "" + dsLocal.Tables[0].Rows[0]["LocalCurrency"].ToString().Trim();
                //}

                //dsLocal = null;
                //objTax.GetTaxGrpInfo((string)Session["LOGIN_GROUP_ID"].ToString().Trim(), out dsLocal);
                //ddlTaxGroup.DataSource = dsLocal;
                //ddlTaxGroup.DataTextField = "TaxGroupName";
                //ddlTaxGroup.DataValueField = "SystemID";
                //ddlTaxGroup.DataBind();
                //ddlTaxGroup.Items.Insert(0, "");

                //if (dsLocal.Tables[0].Rows.Count > 0)
                //{
                //    for (int i = 0; i < dsLocal.Tables[0].Rows.Count; i++)
                //    {
                //        if (Convert.ToBoolean(dsLocal.Tables[0].Rows[i]["DefaultGroup"].ToString().Trim()) == true)
                //        {
                //            ddlTaxGroup.SelectedValue = "" + dsLocal.Tables[0].Rows[i]["SystemID"].ToString().Trim();
                //        }
                //    }
                //}

                string strDate = DateTime.Now.ToString("dd-MMM-yyyy");

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                dsLocal = null;
            }
        }//End Function 
        private void LoadEmpSlrOpenHdData_OnGrid()
        {
            DataSet dsLocal = null;
            DataView dvLocal = null;
            DataRow drLocal = null;
            clsSalaryInfoNew objSal = null;

            try
            {
                objSal = new clsSalaryInfoNew();

                //objSal.SalaryOpenHeadOnGridWithPrev(ddlPlant.SelectedValue.ToString().Trim(), lblEmpSystemId.Text.Trim(), lblSalaryID.Text, ddSalaryRule.SelectedValue, out dsLocal);
                //dvLocal = new DataView();
                //dvLocal.Table = dsLocal.Tables[0];
                //if (dsLocal.Tables[0].Rows.Count > 0)
                //{
                //    dvLocal.RowFilter = "HeadCategory = 'Tax'";
                //    if (dvLocal.Count == 1)
                //    {
                //        //this.lblMonthlyTaxPayableAmtEntryLabel.Visible = true;
                //        //this.txtTaxToBePay.Visible = true;
                //        drLocal = dvLocal[0].Row;
                //        drLocal.Delete();
                //    }
                //    else
                //    {
                //        //this.lblMonthlyTaxPayableAmtEntryLabel.Visible = false;
                //        //this.txtTaxToBePay.Visible = false;
                //    }

                //    dgSalaryOpen.DataSource = dsLocal.Tables[0];
                //    dgSalaryOpen.DataBind();
                //    panSalaryOpen.Visible = true;
                //    dgSalaryOpen.Visible = true;

                //    if (dsLocal.Tables[0].Rows[0]["EffectiveDate"].ToString().Trim() != "")
                //    {
                //        TextEffectiveDate.Text = Convert.ToDateTime(dsLocal.Tables[0].Rows[0]["EffectiveDate"].ToString().Trim()).ToString("dd-MMM-yyyy");
                //    }

                //}
                //else
                //{
                //    dgSalaryOpen.DataSource = null;
                //    dgSalaryOpen.DataBind();
                //    panSalaryOpen.Visible = false;
                //    dgSalaryOpen.Visible = false;
                //}
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objSal = null;
                dsLocal = null;
            }
        }//End Function 
        private void LoadSlrRuleInfo()
        {
            DataSet dsLocal = null;
            clsSalaryInfoNew objSal = null;

            //StringCollection strCurrency = new StringCollection();

            string strCurrencyID = "";
            //lblCurrencyRuleSystemID.Text = "";

            try
            {
                objSal = new clsSalaryInfoNew();

                //objSal.LoadSalaryRuleInfo(ddlPlant.SelectedValue.ToString().Trim(), ddSalaryRule.SelectedValue.Trim(), out dsLocal);

                //if (dsLocal.Tables[0].Rows.Count > 0)
                //{
                //    txtSalaryDescription.Text = "" + dsLocal.Tables[0].Rows[0]["SalaryRuleDescription"].ToString();
                //    lblCurrencyRuleSystemID.Text = "" + dsLocal.Tables[0].Rows[0]["CurrencyRuleSystemID"].ToString();

                //    for (int j = 0; j < dsLocal.Tables[0].Rows.Count; j++)
                //    {
                //        if (strCurrency.Contains(dsLocal.Tables[0].Rows[j]["AmtEntryCurrency"].ToString().Trim()) == false)
                //        {
                //            strCurrency.Add(dsLocal.Tables[0].Rows[j]["AmtEntryCurrency"].ToString().Trim());
                //        }
                //        if (strCurrency.Contains(dsLocal.Tables[0].Rows[j]["AmtDefinitionCurrency"].ToString().Trim()) == false)
                //        {
                //            strCurrency.Add(dsLocal.Tables[0].Rows[j]["AmtDefinitionCurrency"].ToString().Trim());
                //        }
                //        if (strCurrency.Contains(dsLocal.Tables[0].Rows[j]["AmtDisbusmentCurrency"].ToString().Trim()) == false)
                //        {
                //            strCurrency.Add(dsLocal.Tables[0].Rows[j]["AmtDisbusmentCurrency"].ToString().Trim());
                //        }
                //    }

                //    for (int c = 0; c < strCurrency.Count; c++)
                //    {
                //        if (lblLocalCurrencyID.Text.Trim() != strCurrency[c].ToString())
                //        {
                //            strCurrencyID = strCurrency[c].ToString();
                //            lblForeignCurrency.Text = strCurrency[c].ToString();
                //        }
                //    }

                dsLocal = null;
                //string sFromDate = "01-" + this.ddlMonthName.Text.Substring(0, 3) + "-" + this.ddlYearNo.Text.Trim();

                //objSal.GetCurrencyInfo((string)Session["LOGIN_GROUP_ID"].ToString().Trim(), ddlPlant.SelectedValue.ToString().Trim(), strCurrencyID, TextEffectiveDate.Text.Trim(), out dsLocal);
                //if (dsLocal.Tables[0].Rows.Count > 0)
                //{
                //    lblForeignCurrencyID.Text = "" + dsLocal.Tables[0].Rows[0]["CurrencyCode"].ToString().Trim();
                //    lblForeignCurrency.Text = "" + dsLocal.Tables[0].Rows[0]["CurrencyDesc"].ToString().Trim();
                //    txtForeignCurRate.Text = "" + dsLocal.Tables[0].Rows[0]["ExchangeRate"].ToString().Trim();
                //}

                //if (strCurrency.Count > 1)
                //{
                //    lblForeignCurrencyTit.Visible = true;
                //    lblForeignCurrency.Visible = true;
                //    lblRateTit.Visible = true;
                //    txtForeignCurRate.Visible = true;
                //}
                //else
                //{
                //    lblForeignCurrencyTit.Visible = false;
                //    lblForeignCurrency.Visible = false;
                //    lblRateTit.Visible = false;
                //    txtForeignCurRate.Visible = false;
                //}
                //}
                //else
                //{
                //    lblForeignCurrencyID.Text = "";
                //    lblForeignCurrency.Text = "";
                //    txtForeignCurRate.Text = "";
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                objSal = null;
                dsLocal = null;
            }
        }//End Function 
        void GetMinWage()
        {
            clsSalaryStructureAplos obj = null;
            DataSet dsLocal = null;
            try
            {
                //obj = new clsSalaryStructureAplos();
                //obj.GetMinWage(ddlPlant.SelectedValue, TextEffectiveDate.Text.Trim(), lblEmpSystemId.Text, out dsLocal);
                //if (dsLocal.Tables[0].Rows.Count > 0)
                //{
                //    lblGrade.Text = dsLocal.Tables[0].Rows[0]["Grade"].ToString();
                //    lblEmployeeLocation.Text = dsLocal.Tables[0].Rows[0]["EmployeeLocation"].ToString();
                //    if (string.IsNullOrEmpty(dsLocal.Tables[0].Rows[0]["MinWage"].ToString()) == false)
                //    {
                //        lblMinWage.Text = dsLocal.Tables[0].Rows[0]["MinWage"].ToString() + " Effective from:(" + dsLocal.Tables[0].Rows[0]["MWEffectiveDate"].ToString() + ")";
                //    }
                //}
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }//End Function


        void LoadSalaryStructureInfo()
        {
            //Session["VERIFICATION_STATE"] = 1;
            //State((int)Session["VERIFICATION_STATE"]);

            //Button_save.Text = "Save";
            //Button_save.Enabled = true;
            //Button_Delete.Enabled = true;
            //Button_Delete.Visible = true;

            //string tx = TextEffectiveDate.Text;
            //LoadDetails();

            //lblSearchFor.Text = "";
            //PanelSearch.Visible = false;
            //PanelFactory.Visible = true;
            //PanelConfirmation.Visible = false;
        }
        private void LoadDetails()
        {
            DataSet ds = null;
            try
            {

                //LoadEmployeeInfo();
                //MONIR
                //xLoadTaxPolicyInfo();


                LoadEmpSlrOpenHdData_OnGrid();//effDate               
                //LoadEmpSalaryInfoDefineData_OnGrid(out ds);
                LoadEmpDefinationCurrency();

                //MONIR
                //xCalculateTaxableIncomeSlrWise();
                //xLoadTaxSlabDefineData_OnGrid();
                //xLoadTaxableIncomeSlrWiseData_OnGrid();
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                //objApp = null;
                //dsLocal = null;
            }
        }//End Function 
        private void LoadEmpDefinationCurrency()
        {
            DataSet dsLocal = null;
            clsSalaryInfoNew objSlrDef = null;

            //lblDefine.Text = "0";

            try
            {
                objSlrDef = new clsSalaryInfoNew();

                //objSlrDef.LoadEmpAmtDefinationCurrency((string)Session["LOGIN_GROUP_ID"].ToString().Trim(), ddlPlant.SelectedValue.ToString().Trim(), lblEmpSystemId.Text.Trim(), out dsLocal);

                //if (dsLocal.Tables[0].Rows.Count > 0)
                //{
                //    lblForeignCurrencyID.Text = "" + dsLocal.Tables[0].Rows[0]["AmtDefinitionCurrencyID"].ToString().Trim();
                //    lblForeignCurrency.Text = "" + dsLocal.Tables[0].Rows[0]["AmtDefinitionCurrency"].ToString().Trim();
                //    txtForeignCurRate.Text = "" + dsLocal.Tables[0].Rows[0]["AmtDefinitionRate"].ToString().Trim();
                //    lblDefine.Text = "1";
                //}
                //else
                //{
                //    lblForeignCurrencyID.Text = lblLocalCurrencyID.Text.Trim();
                //    lblForeignCurrency.Text = lblLocalCurrency.Text.Trim();
                //    txtForeignCurRate.Text = "1";
                //}
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {
                objSlrDef = null;
                dsLocal = null;
            }
        }//End Function 
        private void ClearEmpBasicInfo()
        {
            //lblEmpSystemId.Text = string.Empty;
            //ImgEmpPic.ImageUrl = "Picture" + "\\noimage.jpg";
            //txtEmpCode.Text = string.Empty;
            //lblEmpName.Text = string.Empty;
            //lblEmpDOJ.Text = string.Empty;
            //lblDesignationGroup.Text = string.Empty;
            //lblDesignationGroupId.Text = string.Empty;
            //lblBudgetCodeId.Text = string.Empty;
            //lblBudgetCode.Text = string.Empty;
            //lblUnit.Text = string.Empty;
            //lblDivision.Text = string.Empty;
            //lblSection.Text = string.Empty;
            //lblSubSection.Text = string.Empty;

            //txtTin.Text = "";
            //lblTaxGrpEmpSystemID.Text = "";

            //lblSlrInfoDefSystemID.Text = "";
            //lblDepartment.Text = "";
            //lblDesignation.Text = "";

            //lblUnit.Text = "";
            //lblSalaryID.Text = "";
            //lblSalaryRuleSysID.Text = "";
            //lblDivision.Text = "";
            //lblDepartment.Text = "";
            //lblSection.Text = "";
            //lblSubSection.Text = "";
            //lblBudgetCategory.Text = "";
            //lblEmpCategor.Text = "";
            //lblDesignationGroup.Text = "";
            //lblDesignationGroupId.Text = "";
            //lblDesignation.Text = "";
            //lblEmpStatus.Text = "";
            //lblEmpResignStatus.Text = "";
            //lblGivenDesignation.Text = "";
            LoadDynamicData();

            //lblForeignCurrencyTit.Visible = false;
            //lblForeignCurrency.Visible = false;
            //lblRateTit.Visible = false;
            //txtForeignCurRate.Visible = false;

            //lblNetGross.Text = "Net Gross Amount: ";
            //lblNetCTC.Text = "Net CTC Amount: ";
            //lblNetGross.Visible = false;
            //lblNetCTC.Visible = false;
        }//End Function 

        public IEnumerable<object> Query(string EmpSystemId)
        {
            DataSet dsbasicInfo = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GetSysIdWiseEmpBasicInfoInformation(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, EmpSystemId, out dsbasicInfo);


            clsSalaryInfoNew obs = null;
            DataSet dsRule = null;

            DateTime _EffectiveDate = GetEffectiveDate(EmpSystemId);
            obs = new clsSalaryInfoNew();
            obs.LoadLatestSalaryStructure(identity.CompanyGroupId, identity.PlantId, EmpSystemId, out dsRule);
            var sSlrRuleMstSystemID = dsbasicInfo.Tables[0].Rows[0]["SalaryRuleMasterSystemID"].ToString().Trim();
            var EffectiveDate = _EffectiveDate.ToString("dd-MMM-yyyy");

            try
            {
                string sql = @"SELECT IsSelectSlrHd = Case WHEN SLID.SalaryRuleMasterSystemID IS NULL THEN Convert(bit, 'False')
                                                       ELSE Convert(bit, 'True') END, 
                                  SLID.SystemID AS SlrInfoDefSystemID, SLID.EmpInfoSystemID, A.SalaryRuleMasterSystemID, A.SalaryRuleName,  
                                  A.SalaryRuleDescription, A.CurrencyRuleSystemID, A.CurrencyRuleChildSystemID, A.TagAndUnTag, A.SalaryHeadID,  
                                  A.SalaryHead, HeadType = CASE WHEN A.HeadType = 'D' THEN 'Deduction' 
                                                                WHEN A.HeadType = 'E' THEN 'Earning'  ELSE '' END,
                                  A.EntryCurrencyID, A.EntryCurrency, A.DefinitionCurrencyID, A.DefinitionCurrency, A.DisbusmentCurrencyID, A.DisbusmentCurrency, A.FormulaDes, 
                                  A.FormulaDesID, A.FixedValue, A.IsOpen,A.IsNA, ISNULL(SLID.EntryAmount, 0) EntryAmount, ISNULL(SLID.DefineAmount, 0) DefineAmount, 
                                  --A.SequenceNo, 
                                  SequenceNo = CASE WHEN ISNULL(A.SequenceNo, 0) > 0 THEN ISNULL(A.SequenceNo, 0)
													ELSE SLID.SequenceNo END, 
								  EffectiveDate = REPLACE(CONVERT(VARCHAR(11), SLID.EffectiveDate, 106),' ','-'), 
                                  EndDate = REPLACE(CONVERT(VARCHAR(11), SLID.EndDate, 106),' ','-') , 
								  MonthPeriod = ISNULL((DATEDIFF(m, SLID.EffectiveDate, SLID.EndDate) + 1), 0), SLID.AmtDefinitionCurrencyID, SLID.AmtDefinitionRate,
                                  IsApproved = Case WHEN SLID.IsApproved IS NULL THEN Convert(bit, 'False')  
                                                    ELSE SLID.IsApproved END,HeadCategory,
								  BaseOnNetPay, RefAbsentism, IsCTCComponent, IsGrossComponent, IsGNRBaseOthSlrHD, GNRBaseOthSlrHDFormula, GNRApplicableMonthNo, '' EarningCurrencyID,
								  '0' EarningAmount, IsGNRWhichEverLess, A.RoundOption, ISNULL(A.IntegerInDisb, 0) IntegerInDisb, ISNULL(A.IsDecimalInDisb, 0) IsDecimalInDisb, 
                                  ISNULL(A.DecimalNo, 0) DecimalNo, SLID.SalaryCategory, A.SalaryHdSequence, SLID.SalaryID 
                           FROM (
                                 SELECT SM.SystemID SalaryRuleMasterSystemID, SM.GroupID, SM.PlantID, SM.SalaryRuleName, SM.SalaryRuleDescription, CRC.MstSystemID CurrencyRuleSystemID, 
                                        CRC.SystemID CurrencyRuleChildSystemID, CRC.SalaryHeadID, TagAndUnTag = CASE WHEN Fml.TagAndUnTag = '1' THEN Fml.TagAndUnTag
																												     WHEN Fxd.TagAndUnTag = '1' THEN Fxd.TagAndUnTag
																													 WHEN SG.IsGNRTagAndUnTag = '1' THEN SG.IsGNRTagAndUnTag
																												ELSE Convert(bit, 'False') END,
										SH.SalaryHead, SH.HeadType, CRC.AmtEntryCurrency AS EntryCurrencyID, CRC.RoundOption, ISNULL(CRC.IntegerInDisb, 0) IntegerInDisb, 
                                        ISNULL(CRC.IsDecimalInDisb, 0) IsDecimalInDisb, ISNULL(CRC.DecimalNo, 0) DecimalNo,
                                        --ECR.CurrencyDesc AS EntryCurrency, CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.CurrencyDesc AS DefinitionCurrency,
                                        ECR.[Name] AS EntryCurrency, CRC.AmtDefinitionCurrency AS DefinitionCurrencyID, DECR.[Name] AS DefinitionCurrency,

                                        CRC.AmtDisbusmentCurrency AS DisbusmentCurrencyID, DICR.[Name] AS DisbusmentCurrency, Fml.FormulaDes, Fml.FormulaDesID, 
                                        ISNULL(Fxd.FixedValue,0) FixedValue, ISNULL(SG.IsOpen, 0) IsOpen,isnull(SG.IsNA,0) IsNA,
                                        SequenceNo = CASE WHEN ISNULL(Fml.SequenceNo, 0) > 0 THEN ISNULL(Fml.SequenceNo, 0)
														  WHEN ISNULL(Fxd.SequenceNo, 0) > 0 THEN ISNULL(Fxd.SequenceNo, 0)
														  WHEN ISNULL(SG.SequenceNo, 0) > 0 THEN ISNULL(SG.SequenceNo, 0)
														  ELSE 0 END,
										SH.HeadCategory,
										BaseOnNetPay = CASE WHEN ISNULL(Fml.BaseOnNetPay, '') != '' THEN Fml.BaseOnNetPay
															WHEN ISNULL(Fxd.BaseOnNetPay, '') != '' THEN Fxd.BaseOnNetPay
															WHEN ISNULL(SG.BaseOnNetPay, '') != '' THEN SG.BaseOnNetPay
															ELSE Convert(bit, 'False') END, 
										RefAbsentism = CASE WHEN ISNULL(Fml.RefAbsentism, '') != '' THEN Fml.RefAbsentism
															WHEN ISNULL(Fxd.RefAbsentism, '') != '' THEN Fxd.RefAbsentism
															WHEN ISNULL(SG.RefAbsentism, '') != '' THEN SG.RefAbsentism
															ELSE Convert(bit, 'False') END, 
										SH.IsCTCComponent, SH.IsGrossComponent, 
										IsGNRBaseOthSlrHD = CASE WHEN ISNULL(Fml.IsGNRBaseOthSlrHD, '') != '' THEN Fml.IsGNRBaseOthSlrHD
															WHEN ISNULL(Fxd.IsGNRBaseOthSlrHD, '') != '' THEN Fxd.IsGNRBaseOthSlrHD
															WHEN ISNULL(SG.IsGNRBaseOthSlrHD, '') != '' THEN SG.IsGNRBaseOthSlrHD
															ELSE Convert(bit, 'False') END, 
										GNRBaseOthSlrHDFormula = ISNULL(Fml.GNRBaseOthSlrHDFormula, '') + ISNULL(Fxd.GNRBaseOthSlrHDFormula, '') + ISNULL(SG.GNRBaseOthSlrHDFormula, ''), 
										GNRApplicableMonthNo = ISNULL(Fml.GNRApplicableMonthNo, '') + ISNULL(Fxd.GNRApplicableMonthNo, '') + ISNULL(SG.GNRApplicableMonthNo, ''),
                                        IsGNRWhichEverLess = CASE WHEN ISNULL(Fml.IsGNRWhichEverLess, '') != '' THEN Fml.IsGNRWhichEverLess
															WHEN ISNULL(Fxd.IsGNRWhichEverLess, '') != '' THEN Fxd.IsGNRWhichEverLess
															WHEN ISNULL(SG.IsGNRWhichEverLess, '') != '' THEN SG.IsGNRWhichEverLess
															ELSE Convert(bit, 'False') END, SH.[Sequence] SalaryHdSequence 
                                 FROM SalaryRuleMaster SM
                                        LEFT JOIN CurrencyRuleChild CRC ON SM.CurrencyRuleSystemID = CRC.MstSystemID
                                        LEFT JOIN SalaryHead SH ON CRC.SalaryHeadID = SH.SalaryHeadID
                                        LEFT JOIN scs.Currency ECR ON CRC.AmtEntryCurrency = ECR.Id
										LEFT JOIN scs.Currency DECR ON CRC.AmtDefinitionCurrency = DECR.Id
										LEFT JOIN scs.Currency DICR ON CRC.AmtDisbusmentCurrency = DICR.Id
                                        LEFT JOIN (
													SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsGNRTagAndUnTag TagAndUnTag, FormulaDes, FormulaDesID, SequenceNo,
														   BaseOnNetPay, RefAbsentism, IsGNRBaseOthSlrHD, GNRBaseOthSlrHDFormula, IsGNRWhichEverLess,
														   GNRApplicableMonthNo 
														FROM SalaryRuleGeneral WHERE IsFormula = 1
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsAbsTagAndUnTag TagAndUnTag, FormulaDes, FormulaDesID, SequenceNo,
														   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
														   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo  
														FROM SalaryRuleAbsenteeism WHERE IsFormula = 1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsDSPTagAndUnTag TagAndUnTag, '' FormulaDes, 
                                                            ('( ' + PerSalaryHeadID + ' * ' + Convert(Varchar(10), PerValue) + ' ) / 100') AS FormulaDesID, SequenceNo,
														   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
														   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo   
														FROM SalaryRuleDayStatusMaster  WHERE IsPercemtage = 1)
												  ) Fml ON SM.SystemID = Fml.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fml.SalaryHeadID
                                        LEFT JOIN (
													SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsGNRTagAndUnTag TagAndUnTag, FixedValue, SequenceNo,
														   BaseOnNetPay, RefAbsentism, IsGNRBaseOthSlrHD, GNRBaseOthSlrHDFormula, 
														   IsGNRWhichEverLess, GNRApplicableMonthNo  
														FROM SalaryRuleGeneral WHERE (IsFixed = 1 or IsNA=1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsAbsTagAndUnTag TagAndUnTag, FixedValue, SequenceNo,
														   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
														   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo   
														FROM SalaryRuleAbsenteeism WHERE IsFixed = 1)
                                                    UNION
                                                    (SELECT SalaryRuleMasterSystemID, SalaryHeadID, IsDSPTagAndUnTag TagAndUnTag, FixedValue, SequenceNo,
														   Convert(bit, 'False') BaseOnNetPay, Convert(bit, 'False') RefAbsentism, Convert(bit, 'False') IsGNRBaseOthSlrHD, '' GNRBaseOthSlrHDFormula, 
														   Convert(bit, 'False') IsGNRWhichEverLess, '' GNRApplicableMonthNo   
														FROM SalaryRuleDayStatusMaster  WHERE IsFixed = 1)
												  ) Fxd ON SM.SystemID = Fxd.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = Fxd.SalaryHeadID
                                        LEFT JOIN SalaryRuleGeneral SG ON SM.SystemID = SG.SalaryRuleMasterSystemID AND CRC.SalaryHeadID = SG.SalaryHeadID AND (SG.IsOpen = 1 OR SG.IsNA=1)
                                  ) A
                                        LEFT JOIN (
                                                   SELECT SD.SystemID, SD.SalaryID, SDM.EmpInfoSystemID, 








EffectiveDate = CASE 
			
			WHEN ISNULL(SDED.EffectiveDate, '') = ''
				THEN REPLACE(CONVERT(VARCHAR(11), SDM.EffectiveDate, 106), ' ', '-')
			ELSE REPLACE(CONVERT(VARCHAR(11), SDED.EffectiveDate, 106), ' ', '-')
			END
		,EndDate = REPLACE(CONVERT(VARCHAR(11), SDED.EndDate, 106), ' ', '-'),
														  SDM.SalaryIncrementSystemID, SDM.SalaryRuleMasterSystemID, 
													      SDM.GroupID, SDM.PlantID, SDM.IsApproved, SDM.ApprovedBy, SDM.DateApproved, SD.SalaryHeadID, SD.EntryCurrencyID, SD.EntryAmount, 
													      SD.DefineCurrencyID, SD.DefineAmount, SD.AmtDefinitionCurrencyID, SD.AmtDefinitionRate, SD.SequenceNo, SD.SalaryCategory
												    FROM (SELECT * FROM SalaryInfoDefineMaster WHERE EffectiveDate='" + EffectiveDate + @"' AND EmpInfoSystemID = '" + EmpSystemId + @"') SDM
																	    INNER JOIN SalaryInfoDefine SD ON SDM.SystemID = SD.SalaryID
																		LEFT JOIN SalaryInfoDefineEffectiveDate SDED ON SDM.SystemID = SDED.SalaryID AND SDED.SalaryHeadID = SD.SalaryHeadID
												    WHERE SDM.EmpInfoSystemID = '" + EmpSystemId + @"'
                                                              --AND SDM.EffectiveDate IN (
                                                                                        --SELECT MAX(EffectiveDate) EffectiveDate FROM SalaryInfoDefineMaster
					                                                                    --  WHERE EmpInfoSystemID = '" + EmpSystemId + @"'
                                                                                       -- )
                                                  ) SLID ON A.SalaryRuleMasterSystemID = SLID.SalaryRuleMasterSystemID AND A.SalaryHeadID = SLID.SalaryHeadID 
												AND A.GroupID = SLID.GroupID AND A.PlantID = SLID.PlantID AND SLID.EmpInfoSystemID = '" + EmpSystemId + @"' 				
                    WHERE (A.SequenceNo > 0 OR SLID.SequenceNo > 0) --A.SequenceNo > 0 
                          AND A.SalaryRuleMasterSystemID = '" + sSlrRuleMstSystemID + @"' AND ISNULL(A.HeadCategory, '') != 'Tax'
                          AND A.PlantID = '" + identity.PlantId + @"'
                    ORDER BY A.SequenceNo ASC --, A.HeadType DESC, A.SalaryHead";
                return _sqlRepository.GetDataCollection(sql);

            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        public void xLoadEmpSalaryInfoDefineData(string EmpSystemId)
        {

            DataSet dsbasicInfo = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GetSysIdWiseEmpBasicInfoInformation(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, EmpSystemId, out dsbasicInfo);


            clsSalaryInfoNew obs = null;
            DataSet dsRule = null;

            DateTime _EffectiveDate = GetEffectiveDate(EmpSystemId);
            obs = new clsSalaryInfoNew();
            obs.LoadLatestSalaryStructure(identity.CompanyGroupId, identity.PlantId, EmpSystemId, out dsRule);
            var sSlrRuleMstSystemID = dsbasicInfo.Tables[0].Rows[0]["SalaryRuleMasterSystemID"].ToString().Trim();
            var EffectiveDate = _EffectiveDate.ToString("dd-MMM-yyyy");
            CustomPara o = new CustomPara();
            o.PlantId = identity.PlantId;
            o.EffectiveDate = EffectiveDate;
            o.CompanyId = identity.CompanyId;
            o.CompanyGroupId = identity.CompanyGroupId;
            o.EmployeeId = EmpSystemId;
            o.SalaryRuleId = sSlrRuleMstSystemID;
            clsSalaryStructureAplos ob = new clsSalaryStructureAplos();
            DataTable dsLocal = null;
            DataTable dsLocalApproved = null;
            DataSet dsMinWage = null;
            DataSet dsSalaryRule = null;
            DataSet dsSelectedSalaryRule = null;
            DataSet dsOpenHead = null;
            //ob.StartProcess(o, out dsLocal,out dsLocalApproved, out dsMinWage, out dsSalaryRule, out dsSelectedSalaryRule, out dsOpenHead);
            //Net Gross and Net CTC
            DataView dv = new DataView();
            dv.Table = dsLocal;
            //dv.Sort = "HeadType DESC, SalaryHead";
            dv.Sort = "SalaryHdSequence";
            var dg = dv.ToTable();



            //CheckAll(true);
            decimal decNetGross = 0;
            decimal decNetCTC = 0;

            for (int i = 0; i < dg.Rows.Count; i++)
            {
                if (dg.Rows[i]["HeadType"].ToString() == "Earning")
                {
                    if (Convert.ToBoolean(dg.Rows[i]["IsGrossComponent"].ToString()) == true)
                    { decNetGross += Convert.ToDecimal(dg.Rows[i]["EntryAmount"].ToString().Trim()); }
                    if (Convert.ToBoolean(dg.Rows[i]["IsCTCComponent"].ToString()) == true)
                    { decNetCTC += Convert.ToDecimal(dg.Rows[i]["EntryAmount"].ToString().Trim()); }
                }
            }
            var ResultGross = decNetGross.ToString("#,##0.00;(#,##0.00)");
            var ResultNetCTC = decNetCTC.ToString("#,##0.00;(#,##0.00)");



            //open head
            var ResultOpenHead = dsOpenHead.Tables[0].AsEnumerable().Select(
                dataRow => new
                {
                    SalaryHeadID = dataRow.Field<string>("SalaryHeadID"),
                    SalaryHead = dataRow.Field<string>("SalaryHead"),
                    Description = dataRow.Field<string>("Description"),
                    SalaryRuleDescription = dataRow.Field<string>("SalaryRuleDescription"),
                    HeadType = dataRow.Field<string>("HeadType"),
                    EntryCurrency = dataRow.Field<string>("EntryCurrency"),
                    Amount = dataRow.Field<decimal>("Amount"),
                    HeadCategory = dataRow.Field<string>("HeadCategory"),
                    EffectiveDate = dataRow.Field<DateTime>("EffectiveDate").ToString("dd-MMM-yyyy"),
                    SalaryID = dataRow.Field<string>("SalaryID"),
                    SalaryHdSequence = dataRow.Field<int>("SalaryHdSequence")

                }).ToList();

            //SalaryRule
            var ResultSalaryRule = dsSalaryRule.Tables[0].AsEnumerable().Select(
                dataRow => new
                {
                    SalaryRuleMasterSystemID = dataRow.Field<string>("SalaryRuleMasterSystemID"),
                    SalaryRuleName = dataRow.Field<string>("SalaryRuleName"),
                    SalaryRuleDescription = dataRow.Field<string>("SalaryRuleDescription")
                }).ToList();
            //SelectedSalaryRule
            var ResultSelectedSalaryRule = dsSelectedSalaryRule.Tables[0].AsEnumerable().Select(
                dataRow => new
                {
                    SalaryRuleMasterSystemID = dataRow.Field<string>("SalaryRuleMasterSystemID"),
                    SalaryRuleName = dataRow.Field<string>("SalaryRuleName"),
                    SalaryRuleDescription = dataRow.Field<string>("SalaryRuleDescription")
                }).ToList();

            //MinWage
            var ResultMinWage = dsMinWage.Tables[0].AsEnumerable().Select(
                dataRow => new
                {
                    MinWage = dataRow.Field<decimal>("MinWage")
                }).ToList();

            //_employeePromotionService.LoadEmpSalaryInfoDefineData_OnGrid(EmpSystemId, out dsLocal);
            var ResultData = dsLocal.AsEnumerable().Select(
                dataRow => new
                {
                    IsSelectSlrHd = dataRow.Field<bool>("IsSelectSlrHd"),
                    SlrInfoDefSystemID = dataRow.Field<string>("SlrInfoDefSystemID"),                       //1
                    CurrencyRuleChildSystemID = dataRow.Field<string>("CurrencyRuleChildSystemID"),          //2
                    SalaryHeadID = dataRow.Field<string>("SalaryHeadID"),                                    //3
                    SalaryHead = dataRow.Field<string>("SalaryHead"),                                        //4
                    HeadType = dataRow.Field<string>("HeadType"),                                           //5
                    FormulaDesID = dataRow.Field<string>("FormulaDesID"),                                   //6
                    FixedValue = dataRow.Field<string>("FixedValue"),                                       //7
                    IsOpen = dataRow.Field<string>("IsOpen"),                                              //8
                    EntryCurrencyID = dataRow.Field<string>("EntryCurrencyID"),                            //9
                    EntryCurrency = dataRow.Field<string>("EntryCurrency"),                                //10
                    DefinitionCurrencyID = dataRow.Field<string>("DefinitionCurrencyID"),                  //11
                    DefinitionCurrency = dataRow.Field<string>("DefinitionCurrency"),                      //12
                    EntryAmount = dataRow.Field<string>("EntryAmount"),                                    //13
                    DefineAmount = dataRow.Field<string>("DefineAmount"),                                  //14
                    TagAndUnTag = dataRow.Field<string>("TagAndUnTag"),                                    //15
                    MonthPeriod = dataRow.Field<string>("MonthPeriod"),                                    //16
                    IsNA = dataRow.Field<string>("IsNA"),                                                 //17
                    HeadCategory = dataRow.Field<string>("HeadCategory"),                                 //18


                }).ToList();
            //return Json(new { EmpSalaryInfoDefine = ResultData, ResultMinWage, ResultSalaryRule, ResultSelectedSalaryRule, ResultOpenHead, ResultGross, ResultNetCTC, Message = AplosMessage.Updated }, JsonRequestBehavior.AllowGet);
        }
        public void xLoadEmpSalaryInfoDefineData_OnGrid(string EmpSystemId, out System.Data.DataSet ds)
        {
            clsSalaryInfoNew obs = null;
            DataSet dsRule = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DateTime _EffectiveDate = GetEffectiveDate(EmpSystemId);
            obs = new clsSalaryInfoNew();
            obs.LoadLatestSalaryStructure(identity.CompanyGroupId, identity.PlantId, EmpSystemId, out dsRule);


            ds = new DataSet();
            DataSet dsLocal = null;
            DataSet dsSlrDefChd = null;
            clsSalaryInfoNew objSal = null;
            bool bIsApproved = false;
            string SlrInfoDefMstSystemID = "";
            string SlrInfoDefSystemID = "";
            decimal decNetGross = 0;
            decimal decNetCTC = 0;
            string sRoundOption = "";
            bool bIntegerInDisb = false;
            bool bIsDecimalInDisb = false;
            int iDecimalNo = 0;
            try
            {
                //txtSalaryCalculationFormula.Text = "";
                objSal = new clsSalaryInfoNew();



                objSal.SalaryStructureAPHeadOnGrid(identity.PlantId, EmpSystemId, "", "", _EffectiveDate.ToString("dd-MMM-yyyy"), dsRule.Tables[0].Rows[0]["SystemID"].ToString().Trim(), out dsLocal);


            }
            catch (Exception ex)
            {
                throw (ex);
            }
            finally
            {

            }
        }//End Function

        //promotion
        public IEnumerable<object> GetUnApprovedEmployeeById(string EmpSystemId)
        {
            try
            {
                string sql = @"SELECT * FROM IncrementHistory WHERE EmpSystemID='" + EmpSystemId + @"' 
                                AND IncrementType IN ('Promotion','Increment and Promotion','Confirmation with Promotion','Confirmation with Increment and Promotion') 
                                AND IsApproved=0";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }


        public IEnumerable<object> GetApprovedDataByEmployeeId(string EmpSystemId)
        {
            try
            {
                string sql = @"SELECT * FROM IncrementHistory WHERE EmpSystemID='" + EmpSystemId + @"' 
                                AND IncrementType IN ('Promotion','Increment and Promotion','Confirmation with Promotion','Confirmation with Increment and Promotion') 
                                AND IsApproved=1";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }



        public IEnumerable<object> GetLegalDesignation()
        {
            try
            {
                string sql = @"SELECT id Value,UserName Text FROM hkp.LegalDesignation ";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
        #region PF Setting 
        public IEnumerable<object> GetSettingsByRule(string RuleId, string PlantId)
        {
            try
            {
                string sql = @"SELECT Id, [SalaryHeadEnum]
                              ,[SalaryRuleId]
                              ,[IsEditable], 0 [IsEntitle], 'NO' [IsMandatory],'' Percentage,'' EffectiveDate FROM  [dbo].[SalaryHeadSetting] WHERE SalaryRuleId='" + RuleId + @"' AND PlantId='" + PlantId + @"' AND SalaryHeadEnum NOT IN ('AttendanceBonus','Absenteeism','OT')";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }





        public CustomParaPFSetting PFCheckAndUnCheck(string pEmpSystemid, CustomParaPFSetting PFSettingModel)
        {
            clsSalaryStructureAplos obj = new clsSalaryStructureAplos();
            return obj.PFCheckAndUnCheck(pEmpSystemid, PFSettingModel);
        }
        public CustomParaPFSetting PFTagUnTagEmpList(string pEmpSystemid,string PlantId )
        {
            clsSalaryStructureAplosNew obj = new clsSalaryStructureAplosNew();
            return obj.PFTagUnTagEmpList(pEmpSystemid, PlantId);
        }



        public void PFCheckAndUnCheckDone(string EmpSystemId, bool IsPFEntitle, string PFEffectiveDate, bool bSave, string User)
        {
            try
            {
                clsSalaryStructureAplos obj = new clsSalaryStructureAplos();
                obj.PFCheckAndUnCheckDone(EmpSystemId, IsPFEntitle, PFEffectiveDate, bSave, User);
                //LoadDataSetFromDataGrid(ref dgEmpSalaryDefine, out dsLocal);

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

        #endregion




        #region Salary Structure Approval 

        /// <summary>
        /// ****Salary Structure Approval**** 
        /// </summary>
        public void LoadEmpSalaryInfoDataForapproval(string EmpSystemId,
            out IEnumerable<EmpSalaryInfoDefineModelNew> EmpSalaryInfoDefine,
          out string ResultSelectedSalaryRule,
          out CustomOutPara outPara)
        {
            string NewFormula_Desc = string.Empty;
            string ApprovedFormula_Desc = string.Empty;
            string ResultMinWage = string.Empty;
            string ResultGross = string.Empty;
            string ResultNetCTC = string.Empty;
            bool IsSalaryRuleEditableEmployee = false;
            string ApprovalStatus = string.Empty;
            string ApprovedEffectiveDate = string.Empty;
            string ApprovedNextDueDate = string.Empty;
            string ResultEffectiveDate = string.Empty;
            outPara = new CustomOutPara();


            DataSet dsPayRollGroup = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            GetPayRollGroupId(EmpSystemId, out dsPayRollGroup);


            if (identity.IsSysAdmin == false)
            {
                if (dsPayRollGroup.Tables[0].Rows.Count > 0)
                {
                    if (!CheckEditableEmpByPayRollGroup(dsPayRollGroup.Tables[0].Rows[0]["Payrollgroupid"].ToString()))
                    {
                        Exception ex = new Exception("Invalid User for this Payroll Group.");
                        throw (ex);
                    }
                }
                else
                {
                    if (!CheckEditableEmpByPayRollGroup(""))
                    {
                        Exception ex = new Exception("Invalid User for this Payroll Group.");
                        throw (ex);
                    }
                }
            }


            clsSalaryStructureAplos ob = new clsSalaryStructureAplos();
            DataSet dsbasicInfo = null;

            GetSysIdWiseEmpBasicInfoInformation(identity.CompanyGroupId, identity.CompanyId, identity.PlantId, EmpSystemId, out dsbasicInfo);


            clsSalaryInfoNew obs = null;
            DataSet dsRule = null;
       

            DateTime _EffectiveDate = GetEffectiveDate(EmpSystemId);
            obs = new clsSalaryInfoNew();
            obs.LoadLatestSalaryStructure(identity.CompanyGroupId, identity.PlantId, EmpSystemId, out dsRule);

            var EffectiveDate = _EffectiveDate.ToString("dd-MMM-yyyy");
            CustomPara o = new CustomPara();
            o.PlantId = identity.PlantId;
            o.EffectiveDate = EffectiveDate;
            o.CompanyId = identity.CompanyId;
            o.CompanyGroupId = identity.CompanyGroupId;
            o.EmployeeId = EmpSystemId;



            DataSet dsIsFreshEntry = null;
            obs.CheckFreshEntry(identity.PlantId, EmpSystemId, out dsIsFreshEntry);




            if (dsbasicInfo.Tables[0].Rows.Count > 0)
            {
                if (string.IsNullOrEmpty(dsbasicInfo.Tables[0].Rows[0]["SalaryRuleMasterSystemID"].ToString().Trim()))
                {
                    DataSet dsSalaryRuleId = GivenDesignationChange(dsbasicInfo.Tables[0].Rows[0]["GivenDesignationId"].ToString().Trim(), identity.CompanyGroupId, identity.PlantId);
                    o.SalaryRuleId = dsSalaryRuleId.Tables[0].Rows[0]["SalaryRuleMasterId"].ToString().Trim();
                    o.IsFreshEntry = true;
                }
                else
                {
                    //o.SalaryRuleId = dsbasicInfo.Tables[0].Rows[0]["SalaryRuleMasterSystemID"].ToString().Trim();
                    DataSet dsSalaryRuleId = GivenDesignationChange(dsbasicInfo.Tables[0].Rows[0]["GivenDesignationId"].ToString().Trim(), identity.CompanyGroupId, identity.PlantId);
                    o.SalaryRuleId = dsSalaryRuleId.Tables[0].Rows[0]["SalaryRuleMasterId"].ToString().Trim();

                    if (dsIsFreshEntry.Tables[0].Rows.Count > 0)
                    {
                        o.IsFreshEntry = true;
                    }

                }

            }



         

            DataTable dtApprovedSalaryDifn = null;
          
           
            
            try
            {

                ob.GetunApprovedSalaryDetailsForapproval(o.PlantId, o.EmployeeId, out dtApprovedSalaryDifn);

            }
            catch (Exception ex)
            {
                throw ex;
            }
            
            #region Net Gross and Net CTC
            //Net Gross and Net CTC
            DataView dv = new DataView();
            decimal decNetGross = 0;
            decimal decNetCTC = 0;
            ResultGross = string.Empty;
            ResultNetCTC = decNetCTC.ToString("#,##0.00;(#,##0.00)");
            if (dtApprovedSalaryDifn != null && dtApprovedSalaryDifn.Rows.Count > 0)
            {

                dv.Table = dtApprovedSalaryDifn;
                //dv.Sort = "HeadType DESC, SalaryHead";
                dv.Sort = "SalaryHdSequence";
                var dg = dv.ToTable();


                //for (int i = 0; i < dg.Rows.Count; i++)
                //{
                //    if (dg.Rows[i]["HeadType"].ToString() == "Earning")
                //    {
                //        if (Convert.ToBoolean(bplib.clsWebLib.GetBoolData(dg.Rows[i]["IsGrossComponent"].ToString())) == true)
                //        { decNetGross += Convert.ToDecimal(bplib.clsWebLib.GetNumData(dg.Rows[i]["EntryAmount"].ToString().Trim())); }
                //        if (Convert.ToBoolean(bplib.clsWebLib.GetBoolData(dg.Rows[i]["IsCTCComponent"].ToString())) == true)
                //        { decNetCTC += Convert.ToDecimal(bplib.clsWebLib.GetNumData(dg.Rows[i]["EntryAmount"].ToString().Trim())); }
                //    }
                //}
                ResultGross = decNetGross.ToString("#,##0.00;(#,##0.00)");
                ResultNetCTC = decNetCTC.ToString("#,##0.00;(#,##0.00)");
            }





            #endregion Net Gross and Net CTC



            #region Selected Salary Rule
            //SelectedSalaryRule
            ResultSelectedSalaryRule = dtApprovedSalaryDifn.Rows[0]["SalaryRuleName"].ToString();
            #endregion Selected Salary Rule



           

            if (dtApprovedSalaryDifn != null)
            {
                EmpSalaryInfoDefine = ConvertEmpSalaryInfoDefineToList(dtApprovedSalaryDifn);
            }
            else
            {
                EmpSalaryInfoDefine = null;
            }




           


            outPara.ResultMinWage = ResultMinWage;
            outPara.ResultGross = ResultGross;
            outPara.ResultNetCTC = ResultNetCTC;
            outPara.IsSalaryRuleEditableEmployee = IsSalaryRuleEditableEmployee;
            outPara.ApprovalStatus = ApprovalStatus;
            outPara.ApprovedEffectiveDate = ApprovedEffectiveDate;
            outPara.ApprovedNextDueDate = ApprovedNextDueDate;
            outPara.ResultEffectiveDate = dtApprovedSalaryDifn.Rows[0]["EffectiveDate"].ToString(); 
            outPara.NewFormula_Desc = NewFormula_Desc;
            outPara.ApprovedFormula_Desc = NewFormula_Desc;

            //Get Un Approved NextDueDate
            DataSet dsUApprovedEffectiveDateAndNextDueDate = GetUnApprovedEffectiveDateAndNextDueDate(EmpSystemId);
            if (!string.IsNullOrEmpty(dsUApprovedEffectiveDateAndNextDueDate.Tables[0].Rows[0]["NextDueDate"].ToString()))
            {
                outPara.UnApprovedNextDueDate = dsUApprovedEffectiveDateAndNextDueDate.Tables[0].Rows[0]["NextDueDate"].ToString();
            }




            //return Json(new { EmpSalaryInfoDefine = ResultData, ResultMinWage, ResultSalaryRule, ResultSelectedSalaryRule, ResultOpenHead, ResultGross, ResultNetCTC, Message = AplosMessage.Updated }, JsonRequestBehavior.AllowGet);
        }




        public void SaveSalaryStructureApprovalData(CustomParaNew para)
        {
            DataTable dtApprovedSalaryDifn = null;
            clsSalaryStructureAplosNew ob = new clsSalaryStructureAplosNew();
            ob.GetunApprovedSalaryDetailsForapproval(para.PlantId, para.EmployeeId, out dtApprovedSalaryDifn);
            para.SalaryRuleId = dtApprovedSalaryDifn.Rows[0]["SalaryRuleMasterSystemID"].ToString();
            para.SalaryId = dtApprovedSalaryDifn.Rows[0]["SalaryID"].ToString();
            para.EffectiveDate = dtApprovedSalaryDifn.Rows[0]["EffectiveDate"].ToString();
            para.EmployeeCode = dtApprovedSalaryDifn.Rows[0]["EmployeeCode"].ToString();
            ob.SaveSalaryStructureApprovalData(para);
        }





        public void SaveSalaryStructureUnApprovalData(string EmpSystemId, string SalaryStructureId)
        {
            DataSet dsLockData = null;
            try
            {
                GetSalaryLockData(EmpSystemId, SalaryStructureId, out dsLockData);
                if (dsLockData.Tables[0].Rows.Count>0)
                {
                    throw new Exception("Salary has already been locked upto ["+ dsLockData.Tables[0].Rows[0]["LastLockMonthName"].ToString() + "].");
                }
                SaveSalaryStructureUnApproval(EmpSystemId);
            }
            catch (Exception ex)
            {

                throw ex;
            }
          
        }//End Function

        public void SaveSalaryStructureUnApproval(string empid)
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
                objCon.ExecuteNonQueryWrapper("update SalaryInfoDefineMaster set IsApproved = 0 WHERE EmpInfoSystemID IN (" + empid + ")", true, "1");


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



        public void GetSalaryLockData(string EmpSystemId, string SalaryStructureId, out System.Data.DataSet dsRef)
        {
            ConnectionManager.DAL.ConManager objCon;
            string strSql = "";
            try
            {
                strSql = @"SELECT *, DateName( month , DateAdd( month , MonthNo , -1 ) ) LastLockMonthName FROM SalaryLock WHERE EmpSystemId ='" + EmpSystemId+ @"' AND SalaryStructureId='" + SalaryStructureId + @"' AND IsLocked=1 ORDER BY YearNo DESC,MonthNo DESC ";

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
        #endregion
    }
}