using Library.Core;
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

namespace Library.HumanResource.Payroll.Tax
{
    public class IncomeTaxPolicy
    {
        ISqlRepository _sqlRepository;
        public IncomeTaxPolicy()
        {
            _sqlRepository = new SqlRepository();
        }

        #region Save
        public void Save(TaxPolicyMaster master)
        {
            try
            {
                DataSet dsMaster;
                GetTexPolicyMaster(master.SystemID, out dsMaster);
                _TexMaster(ref dsMaster, master);

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void SaveGeneral(TaxPolicyGeneral GeneralTax)
        {
            try
            {
                DataSet dsMaster;
                GetTexPolicyGeneral(GeneralTax.SystemID, out dsMaster);
                if (GeneralTax.SystemID != null)
                {
                    ValidationToUpdate(GeneralTax.SystemID, GeneralTax.IsExemption);
                }
                _TexGeneral(ref dsMaster, GeneralTax);

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveGeneralFormula(TaxGeneralFormula GeneralFormula, IEnumerable<TaxGeneralFormulaDetail> details)
        {
            try
            {
                DataSet dsFormula;
                DataSet dsFormulaDetail;
                GetTexPolicyGeneralFormula(GeneralFormula.Id, out dsFormula);
                _TexGeneralFormula(ref dsFormula, GeneralFormula);
                GetTexPolicyGeneralFormulaa(GeneralFormula.Id, out dsFormulaDetail);

                DataRow drF;
                while (dsFormulaDetail.Tables[0].DefaultView.Count > 0)
                    dsFormulaDetail.Tables[0].DefaultView[0].Delete();
                string _Id = dsFormula.Tables[0].Rows[0]["Id"].ToString();
                int count = 0;
                if (details != null)
                {

                    foreach (var item in details)
                    {
                        drF = dsFormulaDetail.Tables[0].NewRow();
                        count++;
                        string pk = _Id + "_" + count;
                        drF["Id"] = pk;
                        drF["TaxPolicyGeneralId"] = _Id;
                        drF["Sequence"] = item.Sequence;
                        drF["SalaryHeadID"] = item.SalaryHeadID;
                        drF["Component"] = item.Component;

                        dsFormulaDetail.Tables[0].Rows.Add(drF);
                    }

                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsFormula, dsFormulaDetail);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveProfessionalTax(TaxSlabDefineProfessional ProTax)
        {
            try
            {
                DataSet dsMaster;
                GetTexPro(ProTax.Id, out dsMaster);
                _TexPro(ref dsMaster, ProTax);

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveRebate(List<TaxRebateSlabDefine> Rebate, string MasterID, InvestmentCredits InvestmentCredit)
        {
            try
            {
                DataSet dsMaster;
                DataSet dsUpdateMaster;
                GetTexInc(MasterID, out dsMaster);
                GetTexPolicyMaster(MasterID, out dsUpdateMaster);
                _UpdateMasters(ref dsUpdateMaster, InvestmentCredit, MasterID);
                while (dsMaster.Tables[0].DefaultView.Count > 0)
                {
                    dsMaster.Tables[0].DefaultView[0].Delete();
                }
                //_TexInc(ref dsMaster, Rebate);
                //bplib.clsGenID objGenID = null;

                string idFromDB = "";
                string systemID = "";
                for (int i = 0; i < Rebate.Count; i++)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID = new bplib.clsGenID();
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "TaxRebateSlabDefine", out idFromDB);
                    systemID = "TAXPR_" + idFromDB;
                    dr["SystemID"] = systemID;
                    dr["TaxPolicyMstID"] = MasterID;
                    dr["TaxAbleIncomeLowerForRebate"] = Rebate[i].TaxAbleIncomeLowerForRebate;
                    dr["TaxAbleIncomeUpperForRebate"] = Rebate[i].TaxAbleIncomeUpperForRebate;
                    //dr["SlabDefine"] = Rebate[i].SlabDefine;
                    //dr["InvesmentAmtForRebate"] = Rebate[i].InvesmentAmtForRebate;
                    dr["InvestAmtTaxPercentageRebate"] = Rebate[i].InvestAmtTaxPercentageRebate;
                    dsMaster.Tables[0].Rows.Add(dr);
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsUpdateMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveIncome(TaxSlabDefinee Slab, string masterID)
        {
            try
            {
                DataSet dsUpdateMaster;
                GetTexPolicyMaster(masterID, out dsUpdateMaster);
                _UpdateMaster(ref dsUpdateMaster, Slab, masterID);
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsUpdateMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void SaveTaxRebate(TaxRebate Slab, string masterID)
        {
            try
            {
                DataSet dsUpdateMaster;
                GetTexPolicyMaster(masterID, out dsUpdateMaster);
                _UpdateMasterTaxRebate(ref dsUpdateMaster, Slab, masterID);
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsUpdateMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void valid(MasterData masterData, ref bool IsAvailable, ref string Year, string msg)
        {

            try
            {
                if (masterData != null)
                {
                    if (IsAvailable == false)
                    {
                        IsAvailable = true;
                        Year = masterData.TaxYearID;
                    }
                    else if (IsAvailable == true && Year != masterData.TaxYearID)
                    {
                        IsAvailable = true;
                        Year = masterData.TaxYearID;
                    }
                    else
                    {
                        throw new Exception(msg);
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void SaveTPPW(List<TaxPolicyPlantWise> BP, string plantID)
        {
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                DataSet dsMaster;
                GetMasterData(plantID, out dsMaster);
                var db = dsMaster.Tables[0].ToList<MasterData>();



                DataTable dtBp = null;
                DataSet dsBp = null;
                DataView dvBp = null;
                DataRow drBp = null;
                string BPId = string.Empty;
                string sql = "SELECT * FROM [dbo].[TaxPolicyPlantWise] ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsBp, false, "1");

                bplib.clsGenID objGenID = null;
                objGenID = new bplib.clsGenID();
                objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "Tax_POLICY_P", out BPId);
                int count = 0;

                for (int i = dsBp.Tables[0].Rows.Count - 1; i >= 0; i--)
                {
                    string policyID = dsBp.Tables[0].Rows[i]["TaxPolicyID"].ToString();
                    foreach (var item in BP)
                    {
                        if (item.TaxPolicyID == policyID && item.IsSelectPolicy == false)
                        {
                            DataView dv = new DataView(dsBp.Tables[0]);
                            dv.RowFilter = "Id='" + item.Id + "'";
                            if (dv.Count > 0)
                            {
                                Delete(item.Id);
                            }
                        }
                    }
                }


                objCon.OpenDataSetThroughAdapter(sql, out dsBp, false, "1");


                bool IsForAll = false;
                bool IsForMale = false;
                bool IsForFemale = false;
                string YearId = null;
                string YearIda = null;
                string YearIdaa = null;


                foreach (var item in BP)
                {
                    if (item.IsSelectPolicy == true)
                    {
                        var ForAll = db.Where(r => r.SystemID == item.TaxPolicyID && r.Male == true && r.Female == true && r.TaxYearID == item.TaxPolicyYearID).FirstOrDefault();
                        var ForMale = db.Where(r => r.SystemID == item.TaxPolicyID && r.Male == true && r.Female == false && r.TaxYearID == item.TaxPolicyYearID).FirstOrDefault();
                        var ForFemale = db.Where(r => r.SystemID == item.TaxPolicyID && r.Female && r.Male == false && r.TaxYearID == item.TaxPolicyYearID).FirstOrDefault();

                        valid(ForAll, ref IsForAll, ref YearId, "Only one policy is allowed for 'No Gender Specific'");
                        valid(ForMale, ref IsForMale, ref YearIda, "Only one policy is allowed for 'Male'");
                        valid(ForFemale, ref IsForFemale, ref YearIdaa, "Only one policy is allowed for 'Female'");

                        if (IsForAll && YearId == item.TaxPolicyYearID)
                        {
                            if (IsForMale == true && YearIda == item.TaxPolicyYearID ||  IsForFemale == true && YearIdaa == item.TaxPolicyYearID)
                            {
                                throw new Exception("Already 'GenderSpecific' Policy Is Tagged");
                            }
                        }


                        dvBp = new DataView(dsBp.Tables[0]);
                        //dvBp.Table = ;
                        dvBp.RowFilter = " TaxPolicyID='" + item.TaxPolicyID + "' and plantID='" + item.PlantId + "' ";

                        if (dvBp.Count == 0)
                        {
                            count++;
                            string pk = "TP_PW" + BPId + "_" + count;
                            drBp = dsBp.Tables[0].NewRow();
                            drBp["Id"] = pk;
                            drBp["TaxPolicyId"] = item.TaxPolicyID;
                            drBp["PlantId"] = item.PlantId;

                            drBp["AddedBy"] = identity.Name;
                            drBp["AddedDate"] = DateTime.Now;
                            drBp["AddedFromIP"] = identity.IPAddress;

                            dsBp.Tables[0].Rows.Add(drBp);
                        }

                    }
                }
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsBp);
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Delete(string ID)
        {
            try
            {
                if (string.IsNullOrEmpty(ID))
                {
                    throw new Exception("Select Id first");
                }
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from [dbo].[TaxPolicyPlantWise] where Id ='" + ID + "'");

                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        #region S a v e Tax Policy Master
        void _TexMaster(ref DataSet dsSaveBonusMaster, TaxPolicyMaster ui_master)
        {
            DataView _dvSave = null;
            //_masterpk = string.Empty;
            try
            {
                _dvSave = new DataView(dsSaveBonusMaster.Tables[0]);
                _dvSave.RowFilter = "SystemID ='" + ui_master.SystemID + "'";
                if (_dvSave.Count == 0)
                {
                    DataRow dr = dsSaveBonusMaster.Tables[0].NewRow();
                    _TexMasterCol("ADDNEW", ui_master, ref dr);
                    dsSaveBonusMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = _dvSave[0].Row;
                    dr.BeginEdit();
                    _TexMasterCol("Edit", ui_master, ref dr);
                    dr.EndEdit();
                }
            }


            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void _TexMasterCol(string OPN_FLAG, TaxPolicyMaster ui_master, ref DataRow drLocal)
        {
            bplib.clsGenID objGenID = null;

            string idFromDB = "";
            string systemID = "";

            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    objGenID = new bplib.clsGenID();
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "BOUNS_POLICY_MONTHLY_RETAIN", out idFromDB);
                    systemID = "TAXP-" + idFromDB;
                    ui_master.SystemID = systemID.Trim();

                    drLocal["SystemID"] = bplib.clsWebLib.RetValidLen(ui_master.SystemID);
                    drLocal["TaxPolicyName"] = ui_master.TaxPolicyName;
                    drLocal["Description"] = ui_master.Description;
                    drLocal["TaxTypeId"] = ui_master.TaxTypeId;
                    drLocal["PlantID"] = ui_master.PlantID;
                    drLocal["GroupID"] = ui_master.GroupID;
                    drLocal["TaxYearID"] = ui_master.TaxYearID;
                    drLocal["MinimumTaxableAmount"] = ui_master.MinimumTaxableAmount;
                    //drLocal["GenderID"] = ui_master.GenderID;
                    drLocal["CalculationBasis"] = ui_master.CalculationBasis;
                    drLocal["TaxLimitInvestAll"] = ui_master.TaxLimitInvestAll;
                    drLocal["TaxFixedTaxInvestAll"] = ui_master.TaxFixedTaxInvestAll;
                    drLocal["TaxPercentageInvestAll"] = ui_master.TaxPercentageInvestAll;
                    drLocal["TaxFixedTaxRebate"] = ui_master.TaxFixedTaxRebate;
                    drLocal["TaxPercentageRebate"] = ui_master.TaxPercentageRebate;
                    drLocal["BaseOnIncomeTaxRebate"] = ui_master.BaseOnIncomeTaxRebate;
                    //drLocal["IsGenderSpecific"] = ui_master.IsGenderSpecific;
                    drLocal["IsFixedTaxInvestAll"] = ui_master.IsFixedTaxInvestAll;
                    drLocal["IsPercentageTaxInvestAll"] = ui_master.IsPercentageTaxInvestAll;
                    drLocal["IsBaseOnActEntAmt"] = ui_master.IsBaseOnActEntAmt;
                    drLocal["IsLimitInvestAll"] = ui_master.IsLimitInvestAll;
                    drLocal["IsFixedTaxRebate"] = ui_master.IsFixedTaxRebate;
                    drLocal["IsPercentageTaxRebate"] = ui_master.IsPercentageTaxRebate;
                    drLocal["TaxFixedBonusDefine"] = ui_master.TaxFixedBonusDefine;
                    drLocal["TaxFixedLvEncash"] = ui_master.TaxFixedLvEncash;
                    drLocal["IsFixedTaxBonusDefine"] = ui_master.IsFixedTaxBonusDefine;
                    drLocal["IsTaxAsPerActual"] = ui_master.IsTaxAsPerActual;
                    drLocal["IsTaxAsPerProjection"] = ui_master.IsTaxAsPerProjection;
                    drLocal["IsFixedTaxLvEncash"] = ui_master.IsFixedTaxLvEncash;
                    drLocal["IsTaxAsPerActualLvEncash"] = ui_master.IsTaxAsPerActualLvEncash;
                    drLocal["IsTaxAsPerProjectionLvEncash"] = ui_master.IsTaxAsPerProjectionLvEncash;
                    drLocal["IsCumulativeTaxSlabDefine"] = ui_master.IsCumulativeTaxSlabDefine;
                    drLocal["IsBrakeTaxSlabDefine"] = ui_master.IsBrakeTaxSlabDefine;
                    drLocal["Male"] = ui_master.Male;
                    drLocal["Female"] = ui_master.Female;
                    drLocal["AgeFrom"] = ui_master.AgeFrom;
                    drLocal["AgeTo"] = ui_master.AgeTo;

                    drLocal["AddedBy"] = ui_master.AddedBy;
                    drLocal["DateAdded"] = bplib.clsWebLib.DateData_AppToDB(DateTime.Now.ToShortDateString().ToString(), bplib.clsWebLib.DB_DATE_FORMAT);

                }
                else
                {
                    drLocal["TaxPolicyName"] = ui_master.TaxPolicyName;
                    drLocal["Description"] = ui_master.Description;
                    drLocal["TaxTypeId"] = ui_master.TaxTypeId;
                    drLocal["PlantID"] = ui_master.PlantID;
                    drLocal["GroupID"] = ui_master.GroupID;
                    drLocal["TaxYearID"] = ui_master.TaxYearID;
                    drLocal["MinimumTaxableAmount"] = ui_master.MinimumTaxableAmount;
                    //drLocal["GenderID"] = ui_master.GenderID;
                    drLocal["CalculationBasis"] = ui_master.CalculationBasis;
                    drLocal["TaxLimitInvestAll"] = ui_master.TaxLimitInvestAll;
                    drLocal["TaxFixedTaxInvestAll"] = ui_master.TaxFixedTaxInvestAll;
                    drLocal["TaxPercentageInvestAll"] = ui_master.TaxPercentageInvestAll;
                    drLocal["TaxFixedTaxRebate"] = ui_master.TaxFixedTaxRebate;
                    drLocal["TaxPercentageRebate"] = ui_master.TaxPercentageRebate;
                    drLocal["BaseOnIncomeTaxRebate"] = ui_master.BaseOnIncomeTaxRebate;
                    //drLocal["IsGenderSpecific"] = ui_master.IsGenderSpecific;
                    drLocal["IsFixedTaxInvestAll"] = ui_master.IsFixedTaxInvestAll;
                    drLocal["IsPercentageTaxInvestAll"] = ui_master.IsPercentageTaxInvestAll;
                    drLocal["IsBaseOnActEntAmt"] = ui_master.IsBaseOnActEntAmt;
                    drLocal["IsLimitInvestAll"] = ui_master.IsLimitInvestAll;
                    drLocal["IsFixedTaxRebate"] = ui_master.IsFixedTaxRebate;
                    drLocal["IsPercentageTaxRebate"] = ui_master.IsPercentageTaxRebate;
                    drLocal["TaxFixedBonusDefine"] = ui_master.TaxFixedBonusDefine;
                    drLocal["TaxFixedLvEncash"] = ui_master.TaxFixedLvEncash;
                    drLocal["IsFixedTaxBonusDefine"] = ui_master.IsFixedTaxBonusDefine;
                    drLocal["IsTaxAsPerActual"] = ui_master.IsTaxAsPerActual;
                    drLocal["IsTaxAsPerProjection"] = ui_master.IsTaxAsPerProjection;
                    drLocal["IsFixedTaxLvEncash"] = ui_master.IsFixedTaxLvEncash;
                    drLocal["IsTaxAsPerActualLvEncash"] = ui_master.IsTaxAsPerActualLvEncash;
                    drLocal["IsTaxAsPerProjectionLvEncash"] = ui_master.IsTaxAsPerProjectionLvEncash;
                    drLocal["IsCumulativeTaxSlabDefine"] = ui_master.IsCumulativeTaxSlabDefine;
                    drLocal["IsBrakeTaxSlabDefine"] = ui_master.IsBrakeTaxSlabDefine;
                    drLocal["Male"] = ui_master.Male;
                    drLocal["Female"] = ui_master.Female;
                    drLocal["AgeFrom"] = ui_master.AgeFrom;
                    drLocal["AgeTo"] = ui_master.AgeTo;

                    drLocal["UpdatedBy"] = ui_master.AddedBy;
                    drLocal["DateUpdated"] = bplib.clsWebLib.DateData_AppToDB(DateTime.Now.ToShortDateString().ToString(), bplib.clsWebLib.DB_DATE_FORMAT);
                }

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
        #endregion S a v e Tax Policy Master

        #region S a v e Tax Policy General
        void _TexGeneral(ref DataSet dsSaveBonusMaster, TaxPolicyGeneral ui_master)
        {
            DataView _dvSave = null;
            //_masterpk = string.Empty;
            try
            {
                _dvSave = new DataView(dsSaveBonusMaster.Tables[0]);
                _dvSave.RowFilter = "SystemID ='" + ui_master.SystemID + "'";
                if (_dvSave.Count == 0)
                {
                    DataRow dr = dsSaveBonusMaster.Tables[0].NewRow();
                    _TexGeneralCol("ADDNEW", ui_master, ref dr);
                    dsSaveBonusMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = _dvSave[0].Row;
                    dr.BeginEdit();
                    _TexGeneralCol("Edit", ui_master, ref dr);
                    dr.EndEdit();
                }
            }


            catch (Exception ex)
            {
                throw ex;
            }
        }
        void _TexGeneralFormula(ref DataSet dsSaveBonusMaster, TaxGeneralFormula ui_master)
        {
            DataView _dvSave = null;
            //_masterpk = string.Empty;
            try
            {
                _dvSave = new DataView(dsSaveBonusMaster.Tables[0]);
                _dvSave.RowFilter = "Id ='" + ui_master.Id + "'";
                if (_dvSave.Count == 0)
                {
                    DataRow dr = dsSaveBonusMaster.Tables[0].NewRow();
                    _TexGeneralColFormula("ADDNEW", ui_master, ref dr);
                    dsSaveBonusMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = _dvSave[0].Row;
                    dr.BeginEdit();
                    _TexGeneralColFormula("Edit", ui_master, ref dr);
                    dr.EndEdit();
                }
            }


            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void _TexGeneralCol(string OPN_FLAG, TaxPolicyGeneral ui_master, ref DataRow drLocal)
        {
            bplib.clsGenID objGenID = null;

            string idFromDB = "";
            string systemID = "";

            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    objGenID = new bplib.clsGenID();
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "TaxPolicyGeneralFormula", out idFromDB);
                    systemID = "TAXP-G-" + idFromDB;
                    ui_master.SystemID = systemID.Trim();

                    drLocal["SystemID"] = bplib.clsWebLib.RetValidLen(ui_master.SystemID);
                    drLocal["TaxPolicyMstID"] = ui_master.TaxPolicyMstID;
                    drLocal["SalaryHeadID"] = ui_master.SalaryHeadID;
                    drLocal["IsTaxable"] = ui_master.IsTaxable;
                    drLocal["IsFixedTaxGeneral"] = ui_master.IsFixedTaxGeneral;
                    if (ui_master.IsFixedTaxGeneral)
                    {
                        drLocal["TaxFixedGeneral"] = ui_master.TaxFixedGeneral;
                    }
                    else
                    {
                        drLocal["TaxFixedGeneral"] = DBNull.Value;
                    }
                    if (ui_master.IsPercentageTaxGeneral)
                    {
                        drLocal["TaxPercentageGeneral"] = ui_master.TaxPercentageGeneral;
                    }
                    else
                    {
                        drLocal["TaxPercentageGeneral"] = DBNull.Value;
                    }
                    drLocal["IsPercentageTaxGeneral"] = ui_master.IsPercentageTaxGeneral;

                    drLocal["Sequence"] = ui_master.Sequence;
                    drLocal["IsExemption"] = ui_master.IsExemption;
                    if (ui_master.IsExemption == false)
                    {
                        drLocal["IsLessOrMore"] = ui_master.IsLessOrMore;
                    }
                    else
                    {
                        drLocal["IsLessOrMore"] = DBNull.Value;
                    }

                    drLocal["AddedBy"] = ui_master.AddedBy;
                    drLocal["DateAdded"] = bplib.clsWebLib.DateData_AppToDB(DateTime.Now.ToShortDateString().ToString(), bplib.clsWebLib.DB_DATE_FORMAT);

                }
                else
                {
                    drLocal["SalaryHeadID"] = ui_master.SalaryHeadID;
                    drLocal["IsTaxable"] = ui_master.IsTaxable;
                    drLocal["IsFixedTaxGeneral"] = ui_master.IsFixedTaxGeneral;
                    if (ui_master.IsFixedTaxGeneral)
                    {
                        drLocal["TaxFixedGeneral"] = ui_master.TaxFixedGeneral;
                    }
                    else
                    {
                        drLocal["TaxFixedGeneral"] = DBNull.Value;
                    }
                    if (ui_master.IsPercentageTaxGeneral)
                    {
                        drLocal["TaxPercentageGeneral"] = ui_master.TaxPercentageGeneral;
                    }
                    else
                    {
                        drLocal["TaxPercentageGeneral"] = DBNull.Value;
                    }
                    drLocal["IsPercentageTaxGeneral"] = ui_master.IsPercentageTaxGeneral;
                    drLocal["IsExemption"] = ui_master.IsExemption;
                    drLocal["Sequence"] = ui_master.Sequence;
                    if (ui_master.IsExemption == false)
                    {
                        drLocal["IsLessOrMore"] = ui_master.IsLessOrMore;
                    }
                    else
                    {
                        drLocal["IsLessOrMore"] = DBNull.Value;
                    }

                    drLocal["UpdatedBy"] = ui_master.AddedBy;
                    drLocal["DateUpdated"] = bplib.clsWebLib.DateData_AppToDB(DateTime.Now.ToShortDateString().ToString(), bplib.clsWebLib.DB_DATE_FORMAT);
                }

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
        private void _TexGeneralColFormula(string OPN_FLAG, TaxGeneralFormula ui_master, ref DataRow drLocal)
        {
            bplib.clsGenID objGenID = null;
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string idFromDB = "";
            string systemID = "";

            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    objGenID = new bplib.clsGenID();
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "TaxPolicyGeneralFormula ", out idFromDB);
                    systemID = "TAXP-GF" + idFromDB;
                    ui_master.Id = systemID.Trim();

                    drLocal["Id"] = bplib.clsWebLib.RetValidLen(ui_master.Id);
                    drLocal["TaxPolicyGeneralId"] = ui_master.TaxPolicyGeneralId;
                    drLocal["Formula"] = ui_master.Formula;
                    drLocal["FormulaID"] = ui_master.FormulaID;
                    drLocal["Description"] = ui_master.Description;
                    drLocal["OptionBasedValue"] = ui_master.OptionBasedValue;
                    drLocal["IsOptionBased"] = ui_master.IsOptionBased;

                    drLocal["AddedBy"] = identity.Name;
                    drLocal["AddedDate"] = bplib.clsWebLib.DateData_AppToDB(DateTime.Now.ToShortDateString().ToString(), bplib.clsWebLib.DB_DATE_FORMAT);
                    drLocal["AddedFromIP"] = identity.IPAddress;

                }
                else
                {
                    drLocal["Formula"] = ui_master.Formula;
                    drLocal["FormulaID"] = ui_master.FormulaID;
                    drLocal["Description"] = ui_master.Description;
                    drLocal["OptionBasedValue"] = ui_master.OptionBasedValue;
                    drLocal["IsOptionBased"] = ui_master.IsOptionBased;
                    drLocal["UpdatedBy"] = identity.Name;
                    drLocal["UpdatedFromIP"] = identity.IPAddress;
                    drLocal["UpdatedDate"] = bplib.clsWebLib.DateData_AppToDB(DateTime.Now.ToShortDateString().ToString(), bplib.clsWebLib.DB_DATE_FORMAT);
                }

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
        #endregion S a v e Tax Policy General

        #region S a v e Professional Tax
        void _TexPro(ref DataSet dsSaveBonusMaster, TaxSlabDefineProfessional ui_master)
        {
            DataView _dvSave = null;
            //_masterpk = string.Empty;
            try
            {
                _dvSave = new DataView(dsSaveBonusMaster.Tables[0]);
                _dvSave.RowFilter = "Id ='" + ui_master.Id + "'";
                if (_dvSave.Count == 0)
                {
                    DataRow dr = dsSaveBonusMaster.Tables[0].NewRow();
                    _TexProCol("ADDNEW", ui_master, ref dr);
                    dsSaveBonusMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = _dvSave[0].Row;
                    dr.BeginEdit();
                    _TexProCol("Edit", ui_master, ref dr);
                    dr.EndEdit();
                }
            }


            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void _TexProCol(string OPN_FLAG, TaxSlabDefineProfessional ui_master, ref DataRow drLocal)
        {
            bplib.clsGenID objGenID = null;

            string idFromDB = "";
            string systemID = "";

            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    objGenID = new bplib.clsGenID();
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "BOUNS_POLICY_MONTHLY_RETAIN", out idFromDB);
                    systemID = "TAXP-SP-" + idFromDB;
                    ui_master.Id = systemID.Trim();

                    drLocal["Id"] = bplib.clsWebLib.RetValidLen(ui_master.Id);
                    drLocal["TaxPolicyMasterId"] = ui_master.TaxPolicyMasterId;
                    drLocal["YearlyMinValue"] = ui_master.YearlyMinValue;
                    drLocal["YearlyMaxValue"] = ui_master.YearlyMaxValue;
                    drLocal["MonthlyMinValue"] = ui_master.MonthlyMinValue;
                    drLocal["MonthlyMaxValue"] = ui_master.MonthlyMaxValue;
                    drLocal["YearlyTaxAmount"] = ui_master.YearlyTaxAmount;
                    drLocal["MonthlyTaxAmount"] = ui_master.MonthlyTaxAmount;
                    drLocal["SeqenceNo"] = ui_master.SeqenceNo;
                    drLocal["AdjustingAmount"] = ui_master.AdjustingAmount;
                    if (ui_master.MonthOfAdjustment is null)
                    {
                        drLocal["MonthOfAdjustment"] = DBNull.Value;
                    }
                    else
                    {
                        drLocal["MonthOfAdjustment"] = ui_master.MonthOfAdjustment;
                    }

                    drLocal["AddedBy"] = ui_master.AddedBy;
                    drLocal["DateAdded"] = bplib.clsWebLib.DateData_AppToDB(DateTime.Now.ToShortDateString().ToString(), bplib.clsWebLib.DB_DATE_FORMAT);

                }
                else
                {
                    drLocal["YearlyMinValue"] = ui_master.YearlyMinValue;
                    drLocal["YearlyMaxValue"] = ui_master.YearlyMaxValue;
                    drLocal["MonthlyMinValue"] = ui_master.MonthlyMinValue;
                    drLocal["MonthlyMaxValue"] = ui_master.MonthlyMaxValue;
                    drLocal["YearlyTaxAmount"] = ui_master.YearlyTaxAmount;
                    drLocal["MonthlyTaxAmount"] = ui_master.MonthlyTaxAmount;
                    drLocal["SeqenceNo"] = ui_master.SeqenceNo;
                    drLocal["AdjustingAmount"] = ui_master.AdjustingAmount;
                    if (ui_master.MonthOfAdjustment is null)
                    {
                        drLocal["MonthOfAdjustment"] = DBNull.Value;
                    }
                    else
                    {
                        drLocal["MonthOfAdjustment"] = ui_master.MonthOfAdjustment;
                    }

                    drLocal["UpdatedBy"] = ui_master.AddedBy;
                    drLocal["DateUpdated"] = bplib.clsWebLib.DateData_AppToDB(DateTime.Now.ToShortDateString().ToString(), bplib.clsWebLib.DB_DATE_FORMAT);
                }

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
        #endregion S a v e Professional Tax

        #region S a v e Rebate
        void _TexInc(ref DataSet dsSaveBonusMaster, TaxRebateSlabDefine ui_master)
        {
            DataView _dvSave = null;
            //_masterpk = string.Empty;
            try
            {
                _dvSave = new DataView(dsSaveBonusMaster.Tables[0]);
                _dvSave.RowFilter = "SystemID ='" + ui_master.SystemID + "'";
                if (_dvSave.Count == 0)
                {
                    DataRow dr = dsSaveBonusMaster.Tables[0].NewRow();
                    _TexIncCol("ADDNEW", ui_master, ref dr);
                    dsSaveBonusMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = _dvSave[0].Row;
                    dr.BeginEdit();
                    _TexIncCol("Edit", ui_master, ref dr);
                    dr.EndEdit();
                }
            }


            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void _TexIncCol(string OPN_FLAG, TaxRebateSlabDefine ui_master, ref DataRow drLocal)
        {
            bplib.clsGenID objGenID = null;

            string idFromDB = "";
            string systemID = "";

            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    objGenID = new bplib.clsGenID();
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "TaxRebateSlabDefine", out idFromDB);
                    systemID = "TAXP-R-" + idFromDB;
                    ui_master.SystemID = systemID.Trim();

                    drLocal["SystemID"] = bplib.clsWebLib.RetValidLen(ui_master.SystemID);
                    drLocal["TaxPolicyMstID"] = ui_master.TaxPolicyMstID;
                    drLocal["TaxAbleIncomeLowerForRebate"] = ui_master.TaxAbleIncomeLowerForRebate;
                    drLocal["TaxAbleIncomeUpperForRebate"] = ui_master.TaxAbleIncomeUpperForRebate;
                    //drLocal["SlabDefine"] = ui_master.SlabDefine;
                    //drLocal["InvesmentAmtForRebate"] = ui_master.InvesmentAmtForRebate;
                    drLocal["InvestAmtTaxPercentageRebate"] = ui_master.InvestAmtTaxPercentageRebate;
                }
                else
                {
                    drLocal["TaxAbleIncomeLowerForRebate"] = ui_master.TaxAbleIncomeLowerForRebate;
                    drLocal["TaxAbleIncomeUpperForRebate"] = ui_master.TaxAbleIncomeUpperForRebate;
                    //drLocal["SlabDefine"] = ui_master.SlabDefine;
                    //drLocal["InvesmentAmtForRebate"] = ui_master.InvesmentAmtForRebate;
                    drLocal["InvestAmtTaxPercentageRebate"] = ui_master.InvestAmtTaxPercentageRebate;
                }

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
        #endregion S a v e Rebate

        #region S a v e Income Tax
        void _IncomeTex(ref DataSet dsSaveBonusMaster, TaxSlabDefine ui_master)
        {
            DataView _dvSave = null;
            //_masterpk = string.Empty;
            try
            {
                _dvSave = new DataView(dsSaveBonusMaster.Tables[0]);
                _dvSave.RowFilter = "SystemID ='" + ui_master.SystemID + "'";
                if (_dvSave.Count == 0)
                {
                    DataRow dr = dsSaveBonusMaster.Tables[0].NewRow();
                    _IncomeTexCol("ADDNEW", ui_master, ref dr);
                    dsSaveBonusMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = _dvSave[0].Row;
                    dr.BeginEdit();
                    _IncomeTexCol("Edit", ui_master, ref dr);
                    dr.EndEdit();
                }
            }


            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void _IncomeTexCol(string OPN_FLAG, TaxSlabDefine ui_master, ref DataRow drLocal)
        {
            bplib.clsGenID objGenID = null;

            string idFromDB = "";
            string systemID = "";

            try
            {
                if (OPN_FLAG == "ADDNEW")
                {
                    objGenID = new bplib.clsGenID();
                    objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "Tax_POLICY_Income", out idFromDB);
                    systemID = "TAXP-I-" + idFromDB;
                    ui_master.SystemID = systemID.Trim();

                    drLocal["SystemID"] = bplib.clsWebLib.RetValidLen(ui_master.SystemID);
                    drLocal["TaxPolicyMstID"] = ui_master.TaxPolicyMstID;
                    drLocal["SlabDefine"] = ui_master.SlabDefine;
                    drLocal["TaxAbleIncome"] = ui_master.TaxAbleIncome;
                    drLocal["SlabDefine"] = ui_master.SlabDefine;
                    drLocal["TaxRate"] = ui_master.TaxRate;
                    drLocal["SequenceNo"] = ui_master.SequenceNo;
                    drLocal["AddedBy"] = ui_master.AddedBy;
                    drLocal["DateAdded"] = bplib.clsWebLib.DateData_AppToDB(DateTime.Now.ToShortDateString().ToString(), bplib.clsWebLib.DB_DATE_FORMAT);
                }
                else
                {
                    drLocal["SlabDefine"] = ui_master.SlabDefine;
                    drLocal["TaxAbleIncome"] = ui_master.TaxAbleIncome;
                    drLocal["SlabDefine"] = ui_master.SlabDefine;
                    drLocal["TaxRate"] = ui_master.TaxRate;
                    drLocal["SequenceNo"] = ui_master.SequenceNo;
                    drLocal["UpdatedBy"] = ui_master.AddedBy;
                    drLocal["DateUpdated"] = bplib.clsWebLib.DateData_AppToDB(DateTime.Now.ToShortDateString().ToString(), bplib.clsWebLib.DB_DATE_FORMAT);
                }

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
        #endregion S a v e Income Tax

        #region Update Master From Income Tax
        void _UpdateMaster(ref DataSet dsUpdateMaster, TaxSlabDefinee ui_master, string masterID)
        {
            DataView _dvSave = null;
            //_masterpk = string.Empty;
            try
            {
                _dvSave = new DataView(dsUpdateMaster.Tables[0]);
                _dvSave.RowFilter = "SystemID ='" + masterID + "'";
                if (_dvSave.Count > 0)
                {
                    DataRow dr = _dvSave[0].Row;
                    dr.BeginEdit();
                    _UpdateMasterCol("Edit", ui_master, ref dr);
                    dr.EndEdit();
                }
            }


            catch (Exception ex)
            {
                throw ex;
            }
        }

        void _UpdateMasterTaxRebate(ref DataSet dsUpdateMaster, TaxRebate ui_master, string masterID)
        {
            DataView _dvSave = null;
            //_masterpk = string.Empty;
            try
            {
                _dvSave = new DataView(dsUpdateMaster.Tables[0]);
                _dvSave.RowFilter = "SystemID ='" + masterID + "'";
                if (_dvSave.Count > 0)
                {
                    DataRow dr = _dvSave[0].Row;
                    dr.BeginEdit();
                    _UpdateMasterColTaxRebate("Edit", ui_master, ref dr);
                    dr.EndEdit();
                }
            }


            catch (Exception ex)
            {
                throw ex;
            }
        }

        void _UpdateMasters(ref DataSet dsUpdateMaster, InvestmentCredits ui_master, string masterID)
        {
            DataView _dvSave = null;
            //_masterpk = string.Empty;
            try
            {
                _dvSave = new DataView(dsUpdateMaster.Tables[0]);
                _dvSave.RowFilter = "SystemID ='" + masterID + "'";
                if (_dvSave.Count > 0)
                {
                    DataRow dr = _dvSave[0].Row;
                    dr.BeginEdit();
                    _UpdateMasterCols("Edit", ui_master, ref dr);
                    dr.EndEdit();
                }
            }


            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void _UpdateMasterCol(string OPN_FLAG, TaxSlabDefinee ui_master, ref DataRow drLocal)
        {
            try
            {
                drLocal["IsCumulativeTaxSlabDefine"] = ui_master.Cumulative;
                drLocal["IsBrakeTaxSlabDefine"] = ui_master.BrakeUp;
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

        private void _UpdateMasterColTaxRebate(string OPN_FLAG, TaxRebate ui_master, ref DataRow drLocal)
        {
            try
            {
                if (ui_master.CumulativeOrBrakeUp == "Cumulative")
                {
                    drLocal["IsTaxRebateCumulative"] = true;
                    drLocal["IsTaxRebateBreakUp"] = false;
                }
                else
                {
                    drLocal["IsTaxRebateCumulative"] = false;
                    drLocal["IsTaxRebateBreakUp"] = true;
                }

                if (ui_master.FixedOrPercentage == "Fixed")
                {
                    drLocal["IsTaxRebateFixed"] = true;
                    drLocal["IsTaxRebatePercentage"] = false;
                }
                else
                {
                    drLocal["IsTaxRebateFixed"] = false;
                    drLocal["IsTaxRebatePercentage"] = true;
                }

                if (ui_master.TaxableIncomeOrTax == "Taxable Income")
                {
                    drLocal["IsTaxRebateTaxableIncome"] = true;
                    drLocal["IsTaxRebateTax"] = false;
                }
                else
                {
                    drLocal["IsTaxRebateTaxableIncome"] = false;
                    drLocal["IsTaxRebateTax"] = true;
                }

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

        private void _UpdateMasterCols(string OPN_FLAG, InvestmentCredits ui_master, ref DataRow drLocal)
        {
            try
            {
                drLocal["IsCumulativeInvestmentCredit"] = ui_master.IsCumulativeInvestmentCredit;
                drLocal["IsBrakeInvestmentCredit"] = ui_master.IsBrakeInvestmentCredit;
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

        #region Query


        public IEnumerable<object> GetPlantTaxPolicy(string plantID)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"SELECT IsSelectPolicy = Case WHEN p.TaxPolicyID IS NULL THEN Convert(bit, 'False')
                            ELSE Convert(bit, 'True') END, b.SystemID TaxPolicyID, b.TaxPolicyName,b.Description,b.TaxYearID TaxPolicyYearID,y.TaxYearName,p.Id 
                            FROM TaxPolicyMaster b                            
							LEFT JOIN SCS.TaxYear y on y.Id=b.TaxYearID
							LEFT JOIN TaxPolicyPlantWise p ON p.TaxPolicyID = b.SystemID
                            and p.PlantId = '" + plantID + "'";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function
        public IEnumerable<object> GetPlantWisePolicy()
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"select  p.Id PlantId,p.UserName PlantName, c.Id CompanyId,c.UserName as CompanyName
                         from ORG.Plant p
                         left join [ORG].[Company] c on c.Id = p.CompanyId ";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function
        public IEnumerable<object> GetMaster()
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"select m.* ,t.TaxYearName
                            from TaxPolicyMaster m
                            left join SCS.TaxYear t on t.Id=m.TaxYearID
                            --left join TaxPolicyPlantWise p on p.TaxPolicyId = m.SystemID";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function
        public IEnumerable<object> GetGeneral(string Master)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"select g.*,s.SalaryHead from TaxPolicyGeneral g
                            left join SalaryHead s on s.SalaryHeadID = g.SalaryHeadID
                            where TaxPolicyMstID= '" + Master + "' order by Sequence";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function
        public IEnumerable<object> GetGeneralFormula(string Master)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"select * from TaxPolicyGeneralFormula where TaxPolicyGeneralId ='" + Master + "'";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public void GetMasterData(string plantID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "select SystemID,IsGenderSpecific ,GenderID,TaxYearID,Male,Female from TaxPolicyMaster ";
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

        public void GetTexPolicyMaster(string MasterID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "select * from TaxPolicyMaster WHERE SystemID= '" + MasterID + @"'";
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

        public void GetTexPolicyGeneral(string ID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "select * from TaxPolicyGeneral WHERE SystemID= '" + ID + @"'";
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
        public void GetTexPolicyGeneralFormula(string ID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "select * from TaxPolicyGeneralFormula WHERE Id= '" + ID + @"'";
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
        public void GetTexPolicyGeneralFormulaa(string ID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "select * from FormulaDetail WHERE TaxPolicyGeneralId= '" + ID + @"'";
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
        public void GetTexPro(string ID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "select * from TaxSlabDefineProfessional WHERE Id= '" + ID + @"'";
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
        public void GetTexInc(string ID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "select * from TaxRebateSlabDefine WHERE TaxPolicyMstID= '" + ID + @"'";
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
        public void GetIncomeTax(string ID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "select * from [dbo].[TaxSlabDefine] WHERE SystemID= '" + ID + @"'";
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

        public IEnumerable<object> GetCompTaxYear(string sGroupID)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"SELECT ID TaxYearID, TaxYearName FROM scs.TaxYear
                            WHERE ID IN (SELECT DISTINCT TaxYearID 
						                            FROM scs.CompanyTaxYear 
						                            WHERE --IsClosed=0 and 
                            CompanyGroupId = '" + sGroupID + @"') order by StartDate";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public IEnumerable<object> GetTab(string Doj, string TaxYeadId)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"select * from scs.TaxYear where Id='" + TaxYeadId + "' and '" + Doj + "' between StartDate and EndDate";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function
        public IEnumerable<object> GetValidationForPlant(string TPId)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"select GenderCheck = case when ISNULL(p.TaxPolicyId,'') <> '' then 1 else 0 end
                            from TaxPolicyMaster m
                            left join TaxPolicyPlantWise p on p.TaxPolicyId = m.SystemID
							 where m.SystemID = '" + TPId + "' ";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function
        public IEnumerable<object> GetTaxMonth(string Year)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"select (month(StartDate))MonthOfAdjustments,PeriodName months from SCS.TaxYearPeriod where TaxYearId = '" + Year + "' order by PeriodNo";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public IEnumerable<object> GetTaxType(string sGroupID)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"select Id, Category, Username from [dbo].[TaxType]";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function
        public IEnumerable<object> GetIncomeTaxType(string sGroupID)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"select Id, Category, Username from [dbo].[TaxType] where Category ='Income Tax'";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function 


        public IEnumerable<object> GetIncomeTaxTransactionInv(string TaxYear, string TaxType, string empId, string CompanyID)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"select IsSelect = case when t.IncTaxItmChildId is null  THEN Convert(bit, 'False')ELSE Convert(bit, 'True') END
	                        ,[Value]  = case when t.Value is null then null else t.Value end ,
							c.Id IncTaxItmChildId,tg.MaxLimit,c.Limit TaxSavingItemLimit,c.isFix,c.isPercentage,c.SalaryHeadId
							,tg.UserName TaxGroup
							,ts.UserName TaxSavingItem
                            , Tax = case when c.isTax = 0 then 'Yes' else '' end 
							, Taxable = case when c.isTaxableIncome = 0 then 'Yes' else '' end ,t.Id
						from IncomeTaxItemChild c
						left join IncomeTaxItemTransaction t on c.Id=t.IncTaxItmChildId  and  t.EmpSystemId='" + empId + @"' 
						left join IncomeTaxItemMaster m on m.SystemId = c.IncomeTaxItemMasterId
						left join [HKP].[TaxSavingGroup] tg on tg.Id = m.TaxSavingGroupId
						left join [HKP].[TaxSavingItem] ts on ts.Id = c.TaxSavingItemId
						where c.IsInvestment=1 and c.IsDeduction=0 and c.IsEarning=0
                            and m.TaxYearId='" + TaxYear + "' and m.TaxTypeId='" + TaxType + "' and m.CompanyId='" + CompanyID + "' order by tg.[Sequence], c.[Sequence]";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public IEnumerable<object> GetIncomeTabValue(string TaxYear, string TaxType, string empId, string CompanyID)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"select IsSelect = case when t.IncTaxItmChildId is null  THEN Convert(bit, 'False')ELSE Convert(bit, 'True') END
	                        ,[Value]  = case when t.Value is null then null else t.Value end ,
							c.Id IncTaxItmChildId,tg.MaxLimit,c.Limit TaxSavingItemLimit,c.isFix,c.isPercentage,c.SalaryHeadId
							,tg.UserName TaxGroup
							,ts.UserName TaxSavingItem
                            , Tax = case when c.isTax = 0 then 'Yes' else '' end 
							, Taxable = case when c.isTaxableIncome = 0 then 'Yes' else '' end ,t.Id
	                        ,[Type] = case when c.IsInvestment = 1 then 'Investment' else '' end
						from IncomeTaxItemChild c
						left join IncomeTaxItemTransaction t on c.Id=t.IncTaxItmChildId  and  t.EmpSystemId='" + empId + @"' 
						left join IncomeTaxItemMaster m on m.SystemId = c.IncomeTaxItemMasterId
						left join [HKP].[TaxSavingGroup] tg on tg.Id = m.TaxSavingGroupId
						left join [HKP].[TaxSavingItem] ts on ts.Id = c.TaxSavingItemId
						where c.IsEarning='1' and m.TaxYearId='" + TaxYear + "' and m.TaxTypeId='" + TaxType + "' and m.CompanyId='" + CompanyID + "' order by tg.[Sequence], c.[Sequence]";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public IEnumerable<object> GetTaxableIncomePara(string TaxYear, string TaxType, string empId, string PlantId)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"select f.Id TaxFormulaId,Descriptions= CONCAT('Examption For ',s.SalaryHead ),f.[Description]
                                ,[Value]  = case when I.Value is null then null else I.Value end
                                ,OptionBase = case when f.IsOptionBased = 1 then CONCAT('Option: ',f.OptionBasedValue) else '' end
                                ,IsEnable = case when f.Formula like '%Actual Amount%' then Convert(bit, 'false') else Convert(bit, 'true') end ,I.Id
								,m.TaxPolicyName
                                ,IsSelect = case when I.Id is null  THEN Convert(bit, 'False')ELSE Convert(bit, 'True') END
                                    from EmployeeInformation e
                                    left join TaxPolicyPlantWise tp on tp.PlantId = e.PlantId
                                    left join TaxPolicyMaster m on m.SystemID = tp.TaxPolicyId
                                    left join TaxPolicyGeneral g on g.TaxPolicyMstID = m.SystemID and isnull(m.GenderID,e.GenderID)=e.GenderID
                                    left join TaxPolicyGeneralFormula f on f.TaxPolicyGeneralId=g.SystemID
                                    left join TaxableIncomeparameter I on I.TaxFormulaId = f.Id and I.EmpSystemId = e.SystemId and I.TaxYearId =m.TaxYearID and I.TaxTypeId=m.TaxTypeId
                                    left join SalaryHead s on s.SalaryHeadID = g.SalaryHeadID
                            where e.SystemId='" + empId + "' and e.PlantId='" + PlantId + "' and g.IsExemption=1 and m.TaxYearID='" + TaxYear + "' and m.TaxTypeId='" + TaxType + @"'
                                and (f.Formula like '%Actual Amount%' or f.IsOptionBased =1)";

                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public IEnumerable<object> GetIncomeTaxTransactionDed(string TaxYear, string TaxType, string empId, string CompanyID)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"select tg.Id GroupId,IsSelect = case when t.IncTaxItmChildId is null  THEN Convert(bit, 'False')ELSE Convert(bit, 'True') END
	                        ,[Value]  = case when t.Value is null then null else t.Value end ,
							c.Id IncTaxItmChildId,tg.MaxLimit,c.Limit TaxSavingItemLimit,c.isFix,c.isPercentage,c.SalaryHeadId
							,tg.UserName TaxGroup
							,ts.UserName TaxSavingItem
                            , Tax = case when c.isTax = 0 then 'Yes' else '' end 
							, Taxable = case when c.isTaxableIncome = 0 then 'Yes' else '' end ,t.Id
                            ,[Type] = case when c.IsInvestment = 1 then 'Investment' else '' end
						from IncomeTaxItemChild c
						left join IncomeTaxItemTransaction t on c.Id=t.IncTaxItmChildId  and  t.EmpSystemId='" + empId + @"' 
						left join IncomeTaxItemMaster m on m.SystemId = c.IncomeTaxItemMasterId
						left join [HKP].[TaxSavingGroup] tg on tg.Id = m.TaxSavingGroupId
						left join [HKP].[TaxSavingItem] ts on ts.Id = c.TaxSavingItemId
						where c.IsDeduction='1' and m.TaxYearId='" + TaxYear + "' and m.TaxTypeId='" + TaxType + "' and m.CompanyId='" + CompanyID + "' order by tg.[Sequence], c.[Sequence]";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public IEnumerable<object> GetPro(string MasterID, string YearId)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"SELECT p.*,t.PeriodName months
                            FROM TaxSlabDefineProfessional p
                            left join SCS.TaxYearPeriod t on (month(t.StartDate))=p.MonthOfAdjustment and t.TaxYearId = '" + YearId + @"'
                            WHERE TaxPolicyMasterId = '" + MasterID + @"'
                            order by SeqenceNo";


                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function
        public IEnumerable<object> GetRebate(string sGroupID)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"SELECT *
                            FROM [dbo].[TaxRebateSlabDefine] WHERE TaxPolicyMstID = '" + sGroupID + @"'";


                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function
        public IEnumerable<object> GetIncome(string sGroupID)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"SELECT s.*
                            FROM [dbo].[TaxSlabDefine] s 
                            left join [dbo].[TaxPolicyMaster] m on m.SystemID = s.TaxPolicyMstID
                            WHERE s.TaxPolicyMstID = '" + sGroupID + @"'";


                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public IEnumerable<object> GetTaxRebate(string sGroupID)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"SELECT s.*
                            FROM [dbo].[TaxRebate] s                             
                            WHERE s.TaxPolicyMasterId = '" + sGroupID + @"'";


                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public IEnumerable<object> GetFormulaList(string GeneralFormulaId)
        {
            try
            {
                string strSQL = string.Empty;

                strSQL = @"SELECT D.Sequence,D.SalaryHeadID
                        ,SalaryHead= CASE WHEN ISNULL(SD.SalaryHead,'')<>'' THEN SD.SalaryHead ELSE D.Component END,D.Component,D.NoticePeriodSettingId
                        FROM [dbo].[FormulaDetail] D
                        LEFT JOIN dbo.SalaryHead SD ON SD.SalaryHeadID=D.SalaryHeadID
                            WHERE D.TaxPolicyGeneralId = '" + GeneralFormulaId + @"'";


                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        #endregion

        #region Delete 
        public void DeleteMaster(string ID)
        {
            try
            {
                if (string.IsNullOrEmpty(ID))
                    throw new Exception("Select Id first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from TaxPolicyMaster where SystemID='" + ID + "'");

                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void DeleteGeneral(string ID)
        {
            try
            {
                if (string.IsNullOrEmpty(ID))
                    throw new Exception("Select Id first");

                ConnectionManager.DAL.ConManager xx = new ConnectionManager.DAL.ConManager("1");
                xx.OpenDataSetThroughAdapter("select * from TaxPolicyGeneralFormula where TaxPolicyGeneralId='" + ID + "' ", out DataSet dsMaster, false, "1");
                while (dsMaster.Tables[0].DefaultView.Count > 0)
                {
                    throw new Exception("Delete Examption Formula first..");
                }
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from TaxPolicyGeneral where SystemID ='" + ID + "'");

                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void DeleteGeneralFormula(string ID)
        {
            try
            {
                if (string.IsNullOrEmpty(ID))
                    throw new Exception("Select Id first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from FormulaDetail  where TaxPolicyGeneralId='" + ID + "'");
                con.executeQuery("delete from TaxPolicyGeneralFormula  where Id='" + ID + "'");
                con.CommitTransaction();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void DeleteProfessionalTax(string ID)
        {
            try
            {
                if (string.IsNullOrEmpty(ID))
                    throw new Exception("Select Id first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from TaxSlabDefineProfessional where Id='" + ID + "'");

                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void DeleteIncomeSlab(string ID)
        {
            try
            {
                if (string.IsNullOrEmpty(ID))
                    throw new Exception("Select Id first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from [TaxSlabDefine] where TaxPolicyMstID='" + ID + "'");

                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void DeleteRebate(string ID)
        {
            try
            {
                if (string.IsNullOrEmpty(ID))
                    throw new Exception("Select Id first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from TaxRebateSlabDefine where TaxPolicyMstID='" + ID + "'");

                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void DeleteIncome(string ID)
        {
            try
            {
                if (string.IsNullOrEmpty(ID))
                    throw new Exception("Select Id first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from TaxSlabDefine where SystemID='" + ID + "'");

                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void ValidationToUpdate(string ID, bool IsExamption)
        {
            try
            {
                if (IsExamption == false)
                {
                    DataSet dsGeneralFormula;
                    ConnectionManager.DAL.ConManager objCon;
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    string sql = "SELECT * FROM [dbo].[TaxPolicyGeneralFormula] WHERE TaxPolicyGeneralId='" + ID + "' ";
                    objCon = new ConnectionManager.DAL.ConManager("1");
                    objCon.OpenDataSetThroughAdapter(sql, out dsGeneralFormula, false, "1");
                    while (dsGeneralFormula.Tables[0].DefaultView.Count > 0)
                    {
                        throw new Exception("Delete Examption Details first..");
                    }
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }//End Function

        #endregion

    }
}

public class TaxPolicyMaster
{
    public string SystemID { get; set; }
    public string TaxPolicyName { get; set; }
    public string Description { get; set; }
    public string TaxTypeId { get; set; }
    public string TaxYearID { get; set; }
    public decimal MinimumTaxableAmount { get; set; }
    //public string GenderID { get; set; }
    public string CalculationBasis { get; set; }
    public string PlantID { get; set; }
    public string GroupID { get; set; }
    public int TaxLimitInvestAll { get; set; }
    public decimal TaxFixedTaxInvestAll { get; set; }
    public int TaxPercentageInvestAll { get; set; }
    public decimal TaxFixedTaxRebate { get; set; }
    public int TaxPercentageRebate { get; set; }
    public string BaseOnIncomeTaxRebate { get; set; }
    //public bool IsGenderSpecific { get; set; }
    public bool IsFixedTaxInvestAll { get; set; }
    public bool IsPercentageTaxInvestAll { get; set; }
    public bool IsBaseOnActEntAmt { get; set; }
    public bool IsLimitInvestAll { get; set; }
    public bool IsFixedTaxRebate { get; set; }
    public bool IsPercentageTaxRebate { get; set; }
    //public string TaxAbleIncomeLowerForRebate { get; set; }
    //public string SlabDefine { get; set; }
    //public string InvesmentAmtForRebate { get; set; }
    //public string InvestAmtTaxPercentageRebate { get; set; }
    public int TaxFixedBonusDefine { get; set; }
    public string TaxFixedLvEncash { get; set; }
    public bool IsFixedTaxBonusDefine { get; set; }
    public bool IsTaxAsPerActual { get; set; }
    public bool IsTaxAsPerProjection { get; set; }
    public bool IsFixedTaxLvEncash { get; set; }
    public bool IsTaxAsPerActualLvEncash { get; set; }
    public bool IsTaxAsPerProjectionLvEncash { get; set; }
    public bool IsCumulativeTaxSlabDefine { get; set; }
    public bool IsBrakeTaxSlabDefine { get; set; }
    public bool Male { get; set; }
    public bool Female { get; set; }
    public double AgeFrom { get; set; }
    public double AgeTo { get; set; }
    public string AddedBy { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime DateAdded { get; set; }
    public DateTime DateUpdated { get; set; }
}
public class TaxPolicyGeneral
{
    public string SystemID { get; set; }
    public string TaxPolicyMstID { get; set; }
    public string SalaryHeadID { get; set; }
    public bool IsTaxable { get; set; }
    public bool IsFixedTaxGeneral { get; set; }
    public int TaxFixedGeneral { get; set; }
    public bool IsPercentageTaxGeneral { get; set; }
    public decimal TaxPercentageGeneral { get; set; }
    public bool IsExemption { get; set; }
    public decimal Sequence { get; set; }
    public bool IsExmWhichEverLess { get; set; }
    public bool IsMaxExmpAmt { get; set; }
    public decimal TaxMaxExmpAmt { get; set; }
    public bool IsExmBaseOnActual { get; set; }
    public bool IsExmBaseOnOtherSlrHd { get; set; }
    public string ExmSalaryHeadID { get; set; }
    public string IsLessOrMore { get; set; }
    public decimal PercentageExmAmtOtherSlrHd { get; set; }
    public string AddedBy { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime DateAdded { get; set; }
    public DateTime DateUpdated { get; set; }
}
public class TaxSlabDefineProfessional
{
    public string Id { get; set; }
    public string TaxPolicyMasterId { get; set; }
    public decimal YearlyMinValue { get; set; }
    public decimal YearlyMaxValue { get; set; }
    public decimal MonthlyMinValue { get; set; }
    public decimal MonthlyMaxValue { get; set; }
    public decimal YearlyTaxAmount { get; set; }
    public decimal MonthlyTaxAmount { get; set; }
    public decimal SeqenceNo { get; set; }
    public decimal AdjustingAmount { get; set; }
    public string MonthOfAdjustment { get; set; }
    public string AddedBy { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime DateAdded { get; set; }
    public DateTime DateUpdated { get; set; }
}
public class TaxRebateSlabDefine
{
    public string SystemID { get; set; }
    public string TaxPolicyMstID { get; set; }
    public double TaxAbleIncomeLowerForRebate { get; set; }
    public double TaxAbleIncomeUpperForRebate { get; set; }
    //public string SlabDefine { get; set; }
    //public bool InvesmentAmtForRebate { get; set; }
    public double InvestAmtTaxPercentageRebate { get; set; }
}
public class TaxSlabDefine
{
    public string SystemID { get; set; }
    public string TaxPolicyMstID { get; set; }
    public bool Cumulative { get; set; }
    public bool BrakeUp { get; set; }
    public string SlabDefine { get; set; }
    public string AddedBy { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime DateAdded { get; set; }
    public DateTime DateUpdated { get; set; }
    public int TaxAbleIncome { get; set; }
    public int TaxRate { get; set; }
    public int SequenceNo { get; set; }
}
public class MasterData
{
    public string SystemID { get; set; }
    public bool IsGenderSpecific { get; set; }
    public string GenderId { get; set; }
    public string TaxYearID { get; set; }
    public bool Male { get; set; }
    public bool Female { get; set; }

}
public class TaxPolicyPlantWise : BaseModel
{
    #region Scalar Properties            
    public string Id { get; set; }
    public string TaxPolicyID { get; set; }
    public string TaxPolicyYearID { get; set; }
    public string PlantId { get; set; }
    public bool IsSelectPolicy { get; set; }

    #endregion Scalar Properties

    #region Audit Properties
    [NeverUpdate]
    public string AddedBy { get; set; }
    public string AddedFromIP { get; set; }
    [NeverUpdate]
    public DateTime? AddedDate { get; set; }

    public string UpdatedBy { get; set; }
    public string UpdatedFromIP { get; set; }
    public DateTime? UpdatedDate { get; set; }

    #endregion Audit Properties
}

public class TaxGeneralFormula
{
    public string Id { get; set; }
    public string TaxPolicyGeneralId { get; set; }
    public string Formula { get; set; }
    public string FormulaID { get; set; }
    public string Description { get; set; }
    public string OptionBasedValue { get; set; }
    public bool IsOptionBased { get; set; }
}

public class TaxGeneralFormulaDetail
{
    public string Id { get; set; }
    public decimal Sequence { get; set; }
    public string SalaryHeadID { get; set; }
    public string TaxPolicyGeneralId { get; set; }
    public string Component { get; set; }
}

public class TaxSlabDefinee
{
    public bool Cumulative { get; set; }
    public bool BrakeUp { get; set; }
}

public class InvestmentCredits
{
    public bool IsCumulativeInvestmentCredit { get; set; }
    public bool IsBrakeInvestmentCredit { get; set; }
}

public class TaxRebate
{
    public string CumulativeOrBrakeUp { get; set; }
    public string FixedOrPercentage { get; set; }
    public string TaxableIncomeOrTax { get; set; }
}