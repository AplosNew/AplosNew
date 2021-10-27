#region Using
using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using System;
using System.Data;
using OTSBD;
using clsAttendance;
using System.Collections.Generic;

#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class PFPolicyController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IStoppageService _stoppageService;
        public PFPolicyController(
              IStoppageService stoppageService,
              ISqlRepository sqlRepository
            )
        {
            _stoppageService = stoppageService;
            _sqlRepository = sqlRepository;
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations
        [HttpGet, Authorize]
        public ActionResult GetSalaryHeadListeList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select SalaryHeadID as Id,SalaryHead as UserName 
                            from [dbo].[SalaryHead] 
                            ORDER BY HeadType DESC,SalaryHead";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetList(string PlantId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select m.* 
                                ,EligibilityTimeLenghtDay=CASE WHEN EligibilityBaseOn='DAY' THEN EligibilityTimeLenght END
                                ,EligibilityTimeLenghtMonth=CASE WHEN EligibilityBaseOn='MONTH' THEN EligibilityTimeLenght END
                                ,MaturityTimeLenghtMonth=CASE WHEN MaturityBaseOn='MONTH' THEN MaturityTimeLenght END
                                ,MaturityTimeLenghtYear=CASE WHEN MaturityBaseOn='YEAR' THEN MaturityTimeLenght END,p.CompanyGroupId,p.CompanyId
                        From [dbo].[PFPolicyMaster] m
                        LEFT JOIN ORG.Plant p on p.Id=m.PlantID
                        where PlantID = '" + PlantId + "'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult GetDetailsListM(string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select d.ID ,d.PFPolicyMasterID,d.FormulaDesEarning,d.FormulaDesIDEarning,d.SalaryHeadIDEarning,d.EarningValueRangeFrom,d.EarningValueRangeTo,d.IsMandatory
                            ,IsFixedEmp=case when d.IsFixedEmp = 1 then 'FixedValue' else 'Formula' end
							,d.FixedValueEmp,d.IsFormulaEmp,d.IsContributionSlrHDdependOnEarningEmp,d.FormulaDesEmp,d.FormulaDesIDEmp,d.SalaryHeadIDEmp
                            ,d.IsDistributionEmp,d.IsDistributionEmp,IsFixedEmployer = case when d.IsFixedEmployer= 1 then 'FixedValue' else 'Formula' end ,d.FixedValueEmployer,d.IsFormulaEmployer,d.IsContributionSlrHDdependOnEarningEmployer,d.FormulaDesEmployer
                            ,d.FormulaDesIDEmployer,d.SalaryHeadIDEmployer,d.IsDistributionEmployer,d.EmpCntValPer,d.EmployerCntValPer,d.EmpVolunValPer
                            ,d.FormulaDesIDEmployerDis,d.FormulaDesIDEmpDis,d.IsVoluntaryPF,d.IsNotEntGetEmplrAlwn,d.IsIndividualAlwn,d.AlwnSlrHd,d.IsAgeLimit
                            ,d.AgeLimit, d.FixedValueEmp, d.FixedValueEmployer
                            from PFPolicyMaster m
                            left join PFPolicyDetails d on d.PFPolicyMasterID=m.ID
                            where d.PFPolicyMasterID='" + MasterId + @"'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetHeadList(string MasterId)
        {
            string sql = @"select p.*,s.SalaryHead SalaryHeadName from PFPolicySalaryHead p
                                left join SalaryHead s on s.SalaryHeadID=p.SalaryHeadID
                                where PFPolicyMasterID='" + MasterId + @"' Order By Sequence";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetEmloyeeDetails(string Details)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" select p.ID EmployeeID,p.[Value] EmployeeValue,p.SalaryHeadID EmployeeSalaryHeadID
                            ,p.UpperLimit EmployeeUpperLimit,p.ResidualValueSlrHdID EmployeeResidualValueSlrHdID
                            ,s.SalaryHead EmployeeSalaryHead,ss.SalaryHead EmployeeResidualValueSlrHd
                            From PFEmployeeDistribution  p
							left join SalaryHead s on s.SalaryHeadID = p.SalaryHeadID
							left join SalaryHead ss on ss.SalaryHeadID = p.ResidualValueSlrHdID
                            where PFPolicyDetailsID='" + Details + "'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetEmloyeerDetails(string Details)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" select p.ID EmployeerID,p.[Value] EmployerValue,p.SalaryHeadID EmployerSalaryHeadID
                            ,p.UpperLimit EmployerUpperLimit,p.ResidualValueSlrHdID EmployerResidualValueSlrHdID
                            ,s.SalaryHead EmployerSalaryHead,ss.SalaryHead EmployerResidualValueSlrHd
                            From PFEmployerDistribution  p
							left join SalaryHead s on s.SalaryHeadID = p.SalaryHeadID
							left join SalaryHead ss on ss.SalaryHeadID = p.ResidualValueSlrHdID
                            where PFPolicyDetailsID='" + Details + "'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult GetDetailsList(string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select d.ID ,d.PFPolicyMasterID,d.FormulaDesEarning,d.FormulaDesIDEarning,d.SalaryHeadIDEarning,d.EarningValueRangeFrom,d.EarningValueRangeTo,d.IsMandatory
                            ,IsFixedEmp=case when d.IsFixedEmp = 1 then 'FixedValue' else 'Formula' end
							,d.FixedValueEmp,d.IsFormulaEmp,d.IsContributionSlrHDdependOnEarningEmp,d.FormulaDesEmp,d.FormulaDesIDEmp,d.SalaryHeadIDEmp
                            ,d.IsDistributionEmp,d.IsDistributionEmp,IsFixedEmployer = case when d.IsFixedEmployer= 1 then 'FixedValue' else 'Formula' end ,d.FixedValueEmployer,d.IsFormulaEmployer,d.IsContributionSlrHDdependOnEarningEmployer,d.FormulaDesEmployer
                            ,d.FormulaDesIDEmployer,d.SalaryHeadIDEmployer,d.IsDistributionEmployer,d.EmpCntValPer,d.EmployerCntValPer,d.EmpVolunValPer
                            ,d.FormulaDesIDEmployerDis,d.FormulaDesIDEmpDis,d.IsVoluntaryPF,d.IsNotEntGetEmplrAlwn,d.IsIndividualAlwn,d.AlwnSlrHd,d.IsAgeLimit
                            ,d.AgeLimit, d.FixedValueEmp, d.FixedValueEmployer
                             ,eed.ID as EmployeeID   ,eed.Value as EmployeeValue,eed.UpperLimit as EmployeeUpperLimit,eed.SalaryHeadID as EmployeeSalaryHeadID,eed.ResidualValueSlrHdID as EmployeeResidualValueSlrHdID
                        ,erd.ID AS EmployerID ,erd.value as EmployerValue,erd.UpperLimit as EmployerUpperLimit,erd.SalaryHeadID as EmployerSalaryHeadID,erd.ResidualValueSlrHdID as EmployerResidualValueSlrHdID
                            from PFPolicyMaster m
                            left join PFPolicyDetails d on d.PFPolicyMasterID=m.ID
                            left join PFEmployeeDistribution eed on eed.PFPolicyDetailsID=d.ID
                            left join PFEmployerDistribution erd on erd.PFPolicyDetailsID=d.ID
                            where d.PFPolicyMasterID='" + MasterId + @"'";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public ActionResult SaveM(PFPolicyMaster Master, List<PFPolicySalaryHead> PFPolicySalaryHeadList)
        {
            try
            {
                string MasterId = string.Empty;
                MasterId = SaveMaster(Master, PFPolicySalaryHeadList);
                return Json(new { MasterId, Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost, Authorize]
        public string SaveMaster(PFPolicyMaster Master, List<PFPolicySalaryHead> PFPolicySalaryHeadList)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                if (Master.EligibilityBaseOn == "DAY" && Master.EligibilityTimeLenghtDay > 0)
                {
                    Master.EligibilityTimeLenght = Master.EligibilityTimeLenghtDay;
                }
                else if (Master.EligibilityBaseOn == "MONTH" && Master.EligibilityTimeLenghtMonth > 0)
                {
                    Master.EligibilityTimeLenght = Master.EligibilityTimeLenghtMonth;
                }
                else
                {
                    Exception ex = new Exception("Please select Eligibility Lenght....");
                    throw (ex);
                }
                if (Master.MaturityBaseOn == "MONTH" && Master.MaturityTimeLenghtMonth > 0)
                {
                    Master.MaturityTimeLenght = Master.MaturityTimeLenghtMonth;
                }
                else if (Master.MaturityBaseOn == "YEAR" && Master.MaturityTimeLenghtYear > 0)
                {
                    Master.MaturityTimeLenght = Master.MaturityTimeLenghtYear;
                }
                else
                {
                    Exception ex = new Exception("Please select Maturity Lenght....");
                    throw (ex);
                }

                string Id = string.Empty;
                string sql = "SELECT * FROM [dbo].[PFPolicyMaster] WHERE ID='" + Master.ID + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[dbo].[PFPolicyMaster]", out sID);
                    Id = "PFPM" + sID;
                    dr["ID"] = Id;
                    dr["PFPolicyName"] = Master.PFPolicyName;
                    dr["PFPolicyDescription"] = Master.PFPolicyDescription;
                    dr["Eligibility"] = Master.Eligibility;
                    dr["EligibilityBaseOn"] = Master.EligibilityBaseOn;
                    dr["EligibilityTimeLenght"] = Master.EligibilityTimeLenght;
                    dr["MaturityBaseOn"] = Master.MaturityBaseOn;
                    dr["MaturityTimeLenght"] = Master.MaturityTimeLenght;
                    dr["IsAllEmpApplocable"] = Master.IsAllEmpApplocable;
                    dr["GroupID"] = identity.CompanyGroupId;
                    dr["PlantID"] = Master.PlantID;


                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    Id = dr["ID"].ToString();

                    dr["PFPolicyName"] = Master.PFPolicyName;
                    dr["PFPolicyDescription"] = Master.PFPolicyDescription;
                    dr["Eligibility"] = Master.Eligibility;
                    dr["EligibilityBaseOn"] = Master.EligibilityBaseOn;
                    dr["EligibilityTimeLenght"] = Master.EligibilityTimeLenght;
                    dr["MaturityBaseOn"] = Master.MaturityBaseOn;
                    dr["MaturityTimeLenght"] = Master.MaturityTimeLenght;
                    dr["IsAllEmpApplocable"] = Master.IsAllEmpApplocable;
                    dr["GroupID"] = identity.CompanyGroupId;
                    dr["PlantID"] = Master.PlantID;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }

                #region Save PFPolicySalaryHead Part
                DeleteHead(Id);
                DataSet dsHead;
                GetHead(Id, out dsHead);
                _Head(ref dsHead, Id, PFPolicySalaryHeadList);

                #endregion

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster, dsHead);
                return Id;
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void DeleteHead(string sMstID)
        {
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(@"DELETE FROM PFPolicySalaryHead WHERE PFPolicyMasterID = '" + sMstID + "'", true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                objCon.RollBack();
                throw (ex);
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function
        public void GetHead(string sMstID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                if (sMstID != "")
                {
                    strSQL = "SELECT * FROM PFPolicySalaryHead WHERE PFPolicyMasterID = '" + sMstID + "'";
                }
                else
                {
                    strSQL = "SELECT * FROM PFPolicySalaryHead ";
                }

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

        void _Head(ref DataSet dsSaveBonusMonths, string MasterID, List<PFPolicySalaryHead> HeadList)
        {

            DataView dvMSave = null;
            DataTable dtMSave = null;
            DataRow drMSave = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                dtMSave = dsSaveBonusMonths.Tables[0];
                int count = 0;
                foreach (var item in HeadList)
                {
                    dvMSave = new DataView();
                    dvMSave.Table = dtMSave;
                    dvMSave.RowFilter = "PFPolicyMasterId ='" + item.PFPolicyMasterId + "' and SalaryHeadID='" + item.SalaryHeadID + "'";
                    if (dvMSave.Count == 0)
                    {
                        count++;
                        drMSave = dtMSave.NewRow();
                        drMSave["Id"] = MasterID + count;
                        drMSave["PFPolicyMasterId"] = MasterID;
                        drMSave["SalaryHeadID"] = item.SalaryHeadID;
                        drMSave["SalaryHeadID"] = item.SalaryHeadID;
                        drMSave["Sequence"] = count;
                        drMSave["AddedBy"] = identity.Name;
                        drMSave["AddedDate"] = DateTime.Now;
                        drMSave["AddedFromIP"] = identity.IPAddress;
                        drMSave["UpdatedBy"] = identity.Name;
                        drMSave["UpdatedDate"] = System.DateTime.Now.ToString();
                        drMSave["UpdatedFromIP"] = identity.IPAddress;
                        dtMSave.Rows.Add(drMSave);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public ActionResult SaveDetails(PFPolicyDetails Details, string Master, List<PFPolicyEmployeer> Employer, List<PFPolicyEmployee> Employee)
        {
            try
            {
                string DetailsId = string.Empty;
                DetailsId = SaveDetailsMaster(Details, Master);
                SaveEmployeeDetails(Details, DetailsId, Employee);
                SaveEmployerDetails(Details, DetailsId, Employer);
                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost, Authorize]
        public string SaveDetailsMaster(PFPolicyDetails Details, string Master)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                if (Details.IsDistributionEmployer == false)
                {
                    Details.EmployerValue = 0;
                    Details.EmployerValue = 0;
                    Details.EmployerSalaryHeadID = null;
                    Details.EmployerResidualValueSlrHdID = null;
                }
                if (Details.IsDistributionEmp == false)
                {
                    Details.EmployeeValue = 0;
                    Details.EmployeeValue = 0;
                    Details.EmployeeSalaryHeadID = null;
                    Details.EmployeeResidualValueSlrHdID = null;
                }
                if (Details.IsFixedEmp == true)
                {
                    Details.FormulaDesEmp = null;
                    Details.FormulaDesIDEmp = null;
                    Details.SalaryHeadIDEmp = null;
                }
                if (Details.IsFixedEmployer == true)
                {
                    Details.FormulaDesEmployer = null;
                    Details.FormulaDesIDEmployer = null;
                    Details.SalaryHeadIDEmployer = null;
                }
                string DetailsId = string.Empty;
                string sql = "SELECT * FROM [dbo].[PFPolicyDetails] WHERE ID='" + Details.ID + "' and PFPolicyMasterID ='" + Master + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "[dbo].[PFPolicyDetails]", out sID);
                    DetailsId = "PFD" + sID;
                    dr["ID"] = DetailsId;
                    dr["PFPolicyMasterID"] = Master;
                    dr["FormulaDesEarning"] = Details.FormulaDesEarning;
                    dr["FormulaDesIDEarning"] = Details.FormulaDesIDEarning;
                    dr["SalaryHeadIDEarning"] = Details.SalaryHeadIDEarning;
                    dr["EarningValueRangeTo"] = Details.EarningValueRangeTo;
                    dr["IsMandatory"] = Details.IsMandatory;
                    dr["IsFixedEmp"] = Details.IsFixedEmp;
                    dr["FixedValueEmp"] = Details.FixedValueEmp;
                    dr["IsFormulaEmp"] = Details.IsFormulaEmp;
                    dr["IsContributionSlrHDdependOnEarningEmp"] = Details.IsContributionSlrHDdependOnEarningEmp;
                    dr["FormulaDesEmp"] = Details.FormulaDesEmp;
                    dr["FormulaDesIDEmp"] = Details.FormulaDesIDEmp;
                    dr["SalaryHeadIDEmp"] = Details.SalaryHeadIDEmp;
                    dr["IsDistributionEmp"] = Details.IsDistributionEmp;
                    dr["IsFixedEmployer"] = Details.IsFixedEmployer;
                    dr["FixedValueEmployer"] = Details.FixedValueEmployer;
                    dr["IsFormulaEmployer"] = Details.IsFormulaEmployer;
                    dr["IsContributionSlrHDdependOnEarningEmployer"] = Details.IsContributionSlrHDdependOnEarningEmployer;
                    dr["FormulaDesEmployer"] = Details.FormulaDesEmployer;
                    dr["FormulaDesIDEmployer"] = Details.FormulaDesIDEmployer;
                    dr["SalaryHeadIDEmployer"] = Details.SalaryHeadIDEmployer;
                    dr["IsDistributionEmployer"] = Details.IsDistributionEmployer;
                    dr["EmpCntValPer"] = Details.EmpCntValPer;
                    dr["EmployerCntValPer"] = Details.EmployerCntValPer;
                    dr["EmpVolunValPer"] = Details.EmpVolunValPer;
                    dr["FormulaDesIDEmployerDis"] = Details.FormulaDesIDEmployerDis;
                    dr["FormulaDesIDEmpDis"] = Details.FormulaDesIDEmpDis;
                    dr["IsVoluntaryPF"] = Details.IsVoluntaryPF;
                    dr["IsNotEntGetEmplrAlwn"] = Details.IsNotEntGetEmplrAlwn;
                    dr["IsIndividualAlwn"] = Details.IsIndividualAlwn;
                    dr["AlwnSlrHd"] = Details.AlwnSlrHd;
                    dr["IsAgeLimit"] = Details.IsAgeLimit;
                    dr["AgeLimit"] = Details.AgeLimit;
                    //dr["EmpFixedValue"] = Details.EmpFixedValue;
                    //dr["EmployerFixedValue"] = Details.EmployerFixedValue;
                    dr["EarningValueRangeFrom"] = Details.EarningValueRangeFrom;

                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    DetailsId = dr["ID"].ToString();

                    dr["FormulaDesEarning"] = Details.FormulaDesEarning;
                    dr["FormulaDesIDEarning"] = Details.FormulaDesIDEarning;
                    dr["SalaryHeadIDEarning"] = Details.SalaryHeadIDEarning;
                    dr["EarningValueRangeTo"] = Details.EarningValueRangeTo;
                    dr["IsMandatory"] = Details.IsMandatory;
                    dr["IsFixedEmp"] = Details.IsFixedEmp;
                    dr["FixedValueEmp"] = Details.FixedValueEmp;
                    dr["IsFormulaEmp"] = Details.IsFormulaEmp;
                    dr["IsContributionSlrHDdependOnEarningEmp"] = Details.IsContributionSlrHDdependOnEarningEmp;
                    dr["FormulaDesEmp"] = Details.FormulaDesEmp;
                    dr["FormulaDesIDEmp"] = Details.FormulaDesIDEmp;
                    dr["SalaryHeadIDEmp"] = Details.SalaryHeadIDEmp;
                    dr["IsDistributionEmp"] = Details.IsDistributionEmp;
                    dr["IsFixedEmployer"] = Details.IsFixedEmployer;
                    dr["FixedValueEmployer"] = Details.FixedValueEmployer;
                    dr["IsFormulaEmployer"] = Details.IsFormulaEmployer;
                    dr["IsContributionSlrHDdependOnEarningEmployer"] = Details.IsContributionSlrHDdependOnEarningEmployer;
                    dr["FormulaDesEmployer"] = Details.FormulaDesEmployer;
                    dr["FormulaDesIDEmployer"] = Details.FormulaDesIDEmployer;
                    dr["SalaryHeadIDEmployer"] = Details.SalaryHeadIDEmployer;
                    dr["IsDistributionEmployer"] = Details.IsDistributionEmployer;
                    dr["EmpCntValPer"] = Details.EmpCntValPer;
                    dr["EmployerCntValPer"] = Details.EmployerCntValPer;
                    dr["EmpVolunValPer"] = Details.EmpVolunValPer;
                    dr["FormulaDesIDEmployerDis"] = Details.FormulaDesIDEmployerDis;
                    dr["FormulaDesIDEmpDis"] = Details.FormulaDesIDEmpDis;
                    dr["IsVoluntaryPF"] = Details.IsVoluntaryPF;
                    dr["IsNotEntGetEmplrAlwn"] = Details.IsNotEntGetEmplrAlwn;
                    dr["IsIndividualAlwn"] = Details.IsIndividualAlwn;
                    dr["AlwnSlrHd"] = Details.AlwnSlrHd;
                    dr["IsAgeLimit"] = Details.IsAgeLimit;
                    dr["AgeLimit"] = Details.AgeLimit;
                    dr["EarningValueRangeFrom"] = Details.EarningValueRangeFrom;

                    dr.EndEdit();
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                return DetailsId;
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public string SaveEmployeeDetails(PFPolicyDetails Details, string DetailsId, List<PFPolicyEmployee> Employee)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            DataTable dtMaster = null;
            DataRow drMaster = null;
            DataView dvMaster = null;
            try
            {
                string sql = "SELECT * FROM PFEmployeeDistribution WHERE PFPolicyDetailsID='" + DetailsId + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                dtMaster = dsMaster.Tables[0];
                dvMaster = new DataView();
                dvMaster.Table = dtMaster;
                foreach (var item in Employee)
                {
                    dvMaster.RowFilter = "ID = '" + item.EmployeeID + "'";
                    if (dvMaster.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();
                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "PFEmployeeDistribution", out sID);
                        dr["ID"] = "PFE" + sID;
                        dr["PFPolicyDetailsID"] = DetailsId;
                        dr["Value"] = item.EmployeeValue;
                        dr["SalaryHeadID"] = item.EmployeeSalaryHeadID;
                        dr["UpperLimit"] = item.EmployeeUpperLimit;
                        dr["ResidualValueSlrHdID"] = item.EmployeeResidualValueSlrHdID;

                        dsMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        drMaster = dvMaster[0].Row;
                        drMaster.BeginEdit();
                        drMaster["PFPolicyDetailsID"] = DetailsId;
                        drMaster["Value"] = item.EmployeeValue;
                        drMaster["SalaryHeadID"] = item.EmployeeSalaryHeadID;
                        drMaster["UpperLimit"] = item.EmployeeUpperLimit;
                        drMaster["ResidualValueSlrHdID"] = item.EmployeeResidualValueSlrHdID;
                        drMaster.EndEdit();
                    }
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                return DetailsId;
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public string SaveEmployerDetails(PFPolicyDetails Details, string DetailsId, List<PFPolicyEmployeer> Employer)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            DataTable dtMaster = null;
            DataRow drMaster = null;
            DataView dvMaster = null;
            try
            {
                string sql = "SELECT * FROM PFEmployerDistribution WHERE PFPolicyDetailsID='" + DetailsId + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                dtMaster = dsMaster.Tables[0];
                dvMaster = new DataView();
                dvMaster.Table = dtMaster;
                foreach (var item in Employer)
                {
                    dvMaster.RowFilter = "ID = '" + item.EmployeerID + "'";
                    if (dvMaster.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();
                        string sID = string.Empty;
                        bplib.clsGenID objGenID = new bplib.clsGenID();
                        objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "PFEmployerDistribution", out sID);
                        dr["ID"] = "PFR" + sID;
                        dr["PFPolicyDetailsID"] = DetailsId;
                        dr["Value"] = item.EmployerValue;
                        dr["SalaryHeadID"] = item.EmployerSalaryHeadID;
                        dr["UpperLimit"] = item.EmployerUpperLimit;
                        dr["ResidualValueSlrHdID"] = item.EmployerResidualValueSlrHdID;
                        dtMaster.Rows.Add(dr);
                    }
                    else
                    {
                        drMaster = dvMaster[0].Row;
                        drMaster.BeginEdit();
                        drMaster["PFPolicyDetailsID"] = DetailsId;
                        drMaster["Value"] = item.EmployerValue;
                        drMaster["SalaryHeadID"] = item.EmployerSalaryHeadID;
                        drMaster["UpperLimit"] = item.EmployerUpperLimit;
                        drMaster["ResidualValueSlrHdID"] = item.EmployerResidualValueSlrHdID;
                        drMaster.EndEdit();
                    }
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
                return DetailsId;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost, Authorize]
        public ActionResult Delete(string ID)
        {
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            DataSet dsEmployee;
            DataSet dsEmployer;
            try
            {
                string sql1 = "DELETE FROM PFEmployeeDistribution WHERE PFPolicyDetailsID='" + ID + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql1, out dsEmployee, false, "1");

                string sql2 = "DELETE FROM PFEmployerDistribution WHERE PFPolicyDetailsID='" + ID + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql2, out dsEmployer, false, "1");

                string sql = "DELETE FROM [dbo].[PFPolicyDetails] WHERE ID='" + ID + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");

            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult DeleteM(string ID)
        {
            string strMasterSQL;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsExceptionEmployeeList;
            DataSet dsMaster;
            try
            {
                string sql = "SELECT * FROM [dbo].[PFPolicyDetails] WHERE PFPolicyMasterID='" + ID + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    Exception ex = new Exception("Please Delete Details First....");
                    throw (ex);
                }
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                strMasterSQL = "DELETE FROM [dbo].[PFPolicyMaster] WHERE ID='" + ID + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(strMasterSQL, out dsExceptionEmployeeList, false, "1");
            }
            catch (Exception ex)
            {
                throw (ex);
            }
            return Json(new { Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);
        }

        public class PFPolicyMaster : BaseModel
        {
            #region Scalar Properties            
            public string ID { get; set; }
            public string PFPolicyName { get; set; }
            public string PFPolicyDescription { get; set; }
            public string Eligibility { get; set; }
            public string EligibilityBaseOn { get; set; }
            public int EligibilityTimeLenght { get; set; }
            public bool IsAllEmpApplocable { get; set; }
            public string MaturityBaseOn { get; set; }
            public int MaturityTimeLenght { get; set; }
            public string GroupID { get; set; }
            public string PlantID { get; set; }

            public int EligibilityTimeLenghtDay { get; set; }
            public int EligibilityTimeLenghtMonth { get; set; }

            public int MaturityTimeLenghtMonth { get; set; }
            public int MaturityTimeLenghtYear { get; set; }
            #endregion Scalar Properties

            #region Audit Properties
            [NeverUpdate]
            public string AddedBy { get; set; }
            [NeverUpdate]
            public DateTime? AddedDate { get; set; }
            [NeverUpdate]
            public string AddedFromIP { get; set; }

            public string UpdatedBy { get; set; }
            public DateTime? UpdatedDate { get; set; }
            public string UpdatedFromIP { get; set; }

            #endregion Audit Properties
        }

        public class PFPolicySalaryHead
        {
            #region Scalar Properties            
            public string Id { get; set; }
            public string PFPolicyMasterId { get; set; }
            public string SalaryHeadID { get; set; }
            #endregion Scalar Properties

            #region Audit Properties
            [NeverUpdate]
            public string AddedBy { get; set; }
            [NeverUpdate]
            public DateTime? AddedDate { get; set; }
            [NeverUpdate]
            public string AddedFromIP { get; set; }

            public string UpdatedBy { get; set; }
            public DateTime? UpdatedDate { get; set; }
            public string UpdatedFromIP { get; set; }

            #endregion Audit Properties
        }

        public class PFPolicyDetails : BaseModel
        {
            #region Scalar Properties            
            public string ID { get; set; }
            public string DetailsId { get; set; }
            public string PFPolicyMasterID { get; set; }
            public int EarningValueRangeFrom { get; set; }
            public int EarningValueRangeTo { get; set; }
            public bool IsMandatory { get; set; }
            public bool IsFixedEmp { get; set; }
            public decimal FixedValueEmp { get; set; }
            public bool IsFormulaEmp { get; set; }
            public bool IsContributionSlrHDdependOnEarningEmp { get; set; }
            public bool IsDistributionEmp { get; set; }
            public bool IsFixedEmployer { get; set; }
            public decimal FixedValueEmployer { get; set; }
            public bool IsFormulaEmployer { get; set; }
            public bool IsContributionSlrHDdependOnEarningEmployer { get; set; }
            public string FormulaDesEmployer { get; set; }
            public bool IsDistributionEmployer { get; set; }
            public decimal EmpCntValPer { get; set; }
            public decimal EmployerCntValPer { get; set; }
            public decimal EmpVolunValPer { get; set; }
            public bool IsVoluntaryPF { get; set; }
            public bool IsNotEntGetEmplrAlwn { get; set; }
            public bool IsIndividualAlwn { get; set; }
            public string AlwnSlrHd { get; set; }
            public bool IsAgeLimit { get; set; }
            public int AgeLimit { get; set; }

            public string FormulaDesIDEarning { get; set; }
            public string FormulaDesEarning { get; set; }
            public string SalaryHeadIDEarning { get; set; }
            public string FormulaDesEmp { get; set; }
            public string FormulaDesIDEmp { get; set; }
            public string SalaryHeadIDEmp { get; set; }
            public string FormulaDesIDEmployer { get; set; }
            public string SalaryHeadIDEmployer { get; set; }
            public string FormulaDesIDEmployerDis { get; set; }
            public string FormulaDesIDEmpDis { get; set; }

            //public string EmpFixedValue { get; set; }
            //public string EmployerFixedValue { get; set; }

            public string EmployeeID { get; set; }
            public string EmployeePFPolicyDetailsID { get; set; }
            public decimal EmployeeValue { get; set; }
            public string EmployeeSalaryHeadID { get; set; }
            public decimal EmployeeUpperLimit { get; set; }
            public string EmployeeResidualValueSlrHdID { get; set; }

            public string EmployerID { get; set; }
            public string EmployerPFPolicyDetailsID { get; set; }
            public decimal EmployerValue { get; set; }
            public string EmployerSalaryHeadID { get; set; }
            public decimal EmployerUpperLimit { get; set; }
            public string EmployerResidualValueSlrHdID { get; set; }

            #endregion Scalar Properties

        }

        public class PFPolicyEmployeer
        {
            public string EmployeerID { get; set; }
            public decimal EmployerValue { get; set; }
            public string EmployerSalaryHeadID { get; set; }
            public decimal EmployerUpperLimit { get; set; }
            public string EmployerResidualValueSlrHdID { get; set; }
        }
        public class PFPolicyEmployee
        {
            public string EmployeeID { get; set; }
            public decimal EmployeeValue { get; set; }
            public string EmployeeSalaryHeadID { get; set; }
            public decimal EmployeeUpperLimit { get; set; }
            public string EmployeeResidualValueSlrHdID { get; set; }
        }
        #endregion
    }
}