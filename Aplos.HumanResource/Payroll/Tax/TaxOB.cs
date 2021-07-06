using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Extension;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.Payroll.Tax
{
    public class TaxOB
    {
        ISqlRepository _sqlRepository;
        public TaxOB()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> GetList(string TaxYear, string TaxType, string empid)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"select t.* from dbo.ProfessionalTaxOpeningBalance t
                                left join EmployeeInformation e on e.SystemId = t.EmpSystemId
                                left join HKP.LegalDesignation l on l.Id = e.LegalDesignationId
                                LEFT JOIN ORG.Department Dp ON e.DepartmentID = Dp.Id
                                where TaxYearId='" + TaxYear + @"' and TaxTypeId = '" + TaxType + "' and e.SystemId = '" + empid + "'";
                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }//End Function

        public void SaveMaster(EmpLists EmpList)
        {

            try
            {

                DataSet dsMonth;

                GetTaxOB(EmpList, out dsMonth);

                _TaxOB(ref dsMonth, EmpList);


                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMonth);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetTaxOB(EmpLists EmpList, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            //var _Id = string.Empty;
            try
            {
                if (EmpList.EmpSystemID != "")
                {
                    strSQL = "SELECT * FROM dbo.ProfessionalTaxOpeningBalance WHERE EmpSystemId in (" + EmpList.EmpSystemID + ")";
                }
                else
                {
                    strSQL = "SELECT * FROM dbo.ProfessionalTaxOpeningBalance ";
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

        void _TaxOB(ref DataSet dsSaveBonusMonths, EmpLists List)
        {

            DataView dvMSave = null;
            DataTable dtMSave = null;
            DataRow drMSave = null;
            try
            {
                string seed_detail = string.Empty;
                bplib.clsGenID objGenID = null;
                objGenID = new bplib.clsGenID();
                objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "Tax_OB", out seed_detail);
                dtMSave = dsSaveBonusMonths.Tables[0];
                int count = 0;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                dvMSave = new DataView();
                dvMSave.Table = dtMSave;
                dvMSave.RowFilter = "EmpSystemId ='" + List.EmpSystemID + "' and Id = '" + List.Id + "' ";
                if (dvMSave.Count == 0)
                {
                    count++;
                    string pk = "TOB_" + seed_detail + "_" + count;
                    drMSave = dtMSave.NewRow();
                    drMSave["Id"] = pk;
                    drMSave["EmpSystemId"] = List.EmpSystemID;
                    drMSave["TaxYearId"] = List.TaxYearId;
                    drMSave["TaxTypeId"] = List.TaxTypeId;
                    drMSave["OpeningTaxableIncomeEarned"] = List.OpeningTaxableIncomeEarned;
                    drMSave["OpeningTaxPaid"] = List.OpeningTaxPaid;

                    drMSave["AddedBy"] = identity.Name;
                    drMSave["DateAdded"] = DateTime.Now;
                    dtMSave.Rows.Add(drMSave);
                }
                else
                {
                    drMSave = dvMSave[0].Row;
                    drMSave.BeginEdit();
                    drMSave["TaxYearId"] = List.TaxYearId;
                    drMSave["TaxTypeId"] = List.TaxTypeId;
                    drMSave["OpeningTaxableIncomeEarned"] = List.OpeningTaxableIncomeEarned;
                    drMSave["OpeningTaxPaid"] = List.OpeningTaxPaid;

                    drMSave["UpdatedBy"] = identity.Name;
                    drMSave["DateUpdated"] = DateTime.Now;
                    drMSave.EndEdit();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveInvsmnt(IncomeTaxItemTransaction Investment, List<IncTaxItmChild> ChildList)
        {
            try
            {
                DataSet dsItemTrsn;
                GetTaxItemTransaction(Investment, ChildList, out dsItemTrsn);
                _TaxItem(ref dsItemTrsn, Investment, ChildList);
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsItemTrsn);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SaveTaxableIncomeEx(IncomeTaxItemTransaction Investment, List<TaxableIncomeparameter> ChildList)
        {
            try
            {
                DataSet dsTaxableIncomeEx;
                GetTaxableIncomeEx(Investment, ChildList, out dsTaxableIncomeEx);
                _TaxableIncomeEx(ref dsTaxableIncomeEx, Investment, ChildList);
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsTaxableIncomeEx);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void GetTaxItemTransaction(IncomeTaxItemTransaction EmpList, List<IncTaxItmChild> ChildList, out DataSet dsRef)
        {
            string strSQL = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            //var _Id = string.Empty;
            try
            {
                //foreach (var item in ChildList)
                //{
                //    if (EmpList.EmpSystemID != "")
                //    {
                //        strSQL = "SELECT * FROM dbo.IncomeTaxItemTransaction WHERE EmpSystemId in (" + EmpList.EmpSystemID + ") and Id in ('" + item.Id + "')";
                //    }
                //    else
                //    {
                strSQL = "SELECT * FROM dbo.IncomeTaxItemTransaction WHERE EmpSystemId in (" + EmpList.EmpSystemID + ")";
                //    }
                //}
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

        public void GetTaxableIncomeEx(IncomeTaxItemTransaction EmpList, List<TaxableIncomeparameter> ChildList, out DataSet dsRef)
        {
            string strSQL = string.Empty;
            ConnectionManager.DAL.ConManager objCon;
            //var _Id = string.Empty;
            try
            {
                //foreach (var item in ChildList)
                //{
                if (EmpList.EmpSystemID != "")
                {
                    strSQL = "SELECT * FROM dbo.TaxableIncomeparameter WHERE EmpSystemId in (" + EmpList.EmpSystemID + ")";
                }
                //    else
                //    {
                //        strSQL = "SELECT * FROM dbo.TaxableIncomeparameter ";
                //    }
                //}
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

        void _TaxItem(ref DataSet dsBp, IncomeTaxItemTransaction List, List<IncTaxItmChild> ChildList)
        {

            DataView dvMSave = null;
            DataTable dtMSave = null;
            DataRow drMSave = null;
            ConnectionManager.DAL.ConManager objCon;
            //int count = 0;
            try
            {
                string seed_detail = string.Empty;
                bplib.clsGenID objGenID = null;
                objGenID = new bplib.clsGenID();
                objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "TaxItem", out seed_detail);
                //dtMSave = dsSaveBonusMonths.Tables[0];
                int count = 0;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;


                DataTable dtBp = null;
                //DataSet dsBp = null;
                DataView dvBp = null;
                DataRow drBp = null;
                string BPId = string.Empty;
                string sql = "SELECT * FROM [dbo].[IncomeTaxItemTransaction] WHERE EmpSystemId = '" + List.EmpSystemID + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsBp, false, "1");

                //bplib.clsGenID objGenID = null;
                //objGenID = new bplib.clsGenID();
                //objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "Tax_POLICY_P", out BPId);
                //int count = 0;
                //for (int i = dsBp.Tables[0].Rows.Count - 1; i >= 0; i--)
                //{
                //    string policyID = dsBp.Tables[0].Rows[i]["IncTaxItmChildId"].ToString();
                //    foreach (var item in ChildList)
                //    {
                //        if (item.IncTaxItmChildId == policyID && item.IsSelect == false)
                //        {
                //            DataView dv = new DataView(dsBp.Tables[0]);
                //            dv.RowFilter = "Id='" + item.Id + "'";
                //            if (dv.Count > 0)
                //            {
                //                Delete(item.Id);
                //            }
                //        }
                //    }
                //}
                objCon.OpenDataSetThroughAdapter(sql, out dsBp, false, "1");
                foreach (var item in ChildList)
                {
                    //dvMSave = new DataView(dsBp.Tables[0]);
                    //dvMSave.Table = dtMSave;
                    dsBp.Tables[0].DefaultView.RowFilter = "Id = '" + item.Id + "' ";
                    if (item.IsSelect == true)
                    {
                        if (dsBp.Tables[0].DefaultView.Count == 0)
                        {
                            count++;
                            string pk = "ITT" + seed_detail + "_" + count;
                            drMSave = dsBp.Tables[0].NewRow();
                            drMSave["Id"] = pk;
                            drMSave["EmpSystemId"] = List.EmpSystemID;
                            drMSave["TaxYearId"] = List.TaxYearId;
                            drMSave["TaxTypeId"] = List.TaxTypeId;
                            drMSave["IncTaxItmChildId"] = item.IncTaxItmChildId;
                            drMSave["Value"] = item.Value;

                            drMSave["AddedBy"] = identity.Name;
                            drMSave["AddedDate"] = DateTime.Now;
                            drMSave["AddedFromIP"] = identity.IPAddress;
                            dsBp.Tables[0].Rows.Add(drMSave);
                        }
                        else
                        {
                            drMSave = dsBp.Tables[0].DefaultView[0].Row;
                            drMSave.BeginEdit();
                            drMSave["TaxYearId"] = List.TaxYearId;
                            drMSave["TaxTypeId"] = List.TaxTypeId;
                            drMSave["IncTaxItmChildId"] = item.IncTaxItmChildId;
                            drMSave["Value"] = item.Value;

                            drMSave["UpdatedBy"] = identity.Name;
                            drMSave["UpdatedDate"] = DateTime.Now;
                            drMSave["UpdatedFromIP"] = identity.IPAddress;
                            drMSave.EndEdit();
                        }
                    }
                    else
                    {
                        while (dsBp.Tables[0].DefaultView.Count>0)
                        {
                            dsBp.Tables[0].DefaultView[0].Row.Delete();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void _TaxableIncomeEx(ref DataSet dsSaveBonusMonths, IncomeTaxItemTransaction List, List<TaxableIncomeparameter> ChildList)
        {

            DataView dvMSave = null;
            DataTable dtMSave = null;
            DataRow drMSave = null;
            ConnectionManager.DAL.ConManager objCon;
            //int count = 0;
            try
            {
                string seed_detail = string.Empty;
                bplib.clsGenID objGenID = null;
                objGenID = new bplib.clsGenID();
                objGenID.GenID(DateTime.Now.ToShortDateString().ToString(), "TaxableIncomeparameter", out seed_detail);
                dtMSave = dsSaveBonusMonths.Tables[0];
                int count = 0;
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                foreach (var item in ChildList)
                {
                    dvMSave = new DataView(dsSaveBonusMonths.Tables[0]);
                    dvMSave.RowFilter = "EmpSystemId ='" + List.EmpSystemID + "' and Id = '" + item.Id + "' ";
                    if (dvMSave.Count == 0)
                    {
                        count++;
                        string pk = "ITT" + seed_detail + "_" + count;
                        drMSave = dtMSave.NewRow();
                        drMSave["Id"] = pk;
                        drMSave["EmpSystemId"] = List.EmpSystemID;
                        drMSave["TaxYearId"] = List.TaxYearId;
                        drMSave["TaxTypeId"] = List.TaxTypeId;
                        drMSave["TaxFormulaId"] = item.TaxFormulaId;
                        if (item.Value == null)
                        {
                            drMSave["Value"] = DBNull.Value;
                        }
                        else
                        {
                            drMSave["Value"] = item.Value;
                        }

                        drMSave["AddedBy"] = identity.Name;
                        drMSave["AddedDate"] = DateTime.Now;
                        drMSave["AddedFromIP"] = identity.IPAddress;
                        dtMSave.Rows.Add(drMSave);
                    }
                    else
                    {
                        drMSave = dvMSave[0].Row;
                        drMSave.BeginEdit();
                        drMSave["TaxYearId"] = List.TaxYearId;
                        drMSave["TaxTypeId"] = List.TaxTypeId;
                        drMSave["TaxFormulaId"] = item.TaxFormulaId;
                        if (item.Value == null)
                        {
                            drMSave["Value"] = DBNull.Value;
                        }
                        else
                        {
                            drMSave["Value"] = item.Value;
                        }

                        drMSave["UpdatedBy"] = identity.Name;
                        drMSave["UpdatedDate"] = DateTime.Now;
                        drMSave["UpdatedFromIP"] = identity.IPAddress;
                        drMSave.EndEdit();
                    }
                }
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
                con.executeQuery("delete from [dbo].[IncomeTaxItemTransaction] where Id ='" + ID + "'");

                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public void DeleteTaxableIncomeparameter(string ID)
        {
            try
            {
                if (string.IsNullOrEmpty(ID))
                {
                    throw new Exception("Select Id first");
                }
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from [dbo].[TaxableIncomeparameter] where Id ='" + ID + "'");

                con.CommitTransaction();

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

public class EmployeeList
{
    public string Id { get; set; }
    public string SystemID { get; set; }
    public decimal OpeningTaxableIncomeEarned { get; set; }
    public decimal OpeningTaxPaid { get; set; }
    public string AddedBy { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime DateAdded { get; set; }
    public DateTime DateUpdated { get; set; }
    public string TaxYearId { get; set; }
    public string TaxTypeId { get; set; }
}

public class ProfessionalTaxOB
{
    public string Id { get; set; }
    public string TaxYearId { get; set; }
    public string TaxTypeId { get; set; }
    public string AddedBy { get; set; }
    public string UpdatedBy { get; set; }
    public DateTime DateAdded { get; set; }
    public DateTime DateUpdated { get; set; }
}

public class EmpLists
{
    public string EmpSystemID { get; set; }
    public string Id { get; set; }
    public string TaxYearId { get; set; }
    public string TaxTypeId { get; set; }
    public string OpeningTaxPaid { get; set; }
    public string OpeningTaxableIncomeEarned { get; set; }
}

public class IncomeTaxItemTransaction
{
    public string Id { get; set; }
    public string EmpSystemID { get; set; }
    public string TaxYearId { get; set; }
    public string TaxTypeId { get; set; }
}

public class IncTaxItmChild
{
    public string Id { get; set; }
    public string IncTaxItmChildId { get; set; }
    public double MaxLimit { get; set; }
    public double Value { get; set; }
    public double TaxSavingItemLimit { get; set; }
    public bool IsSelect { get; set; }
    public string GroupId { get; set; }
}
public class TaxableIncomeparameter
{
    public string Id { get; set; }
    public string TaxFormulaId { get; set; }
    public string OptionBase { get; set; }
    public double Value { get; set; }
    public bool IsSelect { get; set; }
}