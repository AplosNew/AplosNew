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
    public class DeductionInvestmentMasterService
    {
        ISqlRepository _sqlRepository;
        public DeductionInvestmentMasterService()
        {
            _sqlRepository = new SqlRepository();
        }

        public IEnumerable<object> GetList(string Company)
        {
            try
            {
                string sql = @"Select itm.SystemId, itm.CompanyId , itm.TaxYearId, itm.TaxTypeId , itm.UserCode , c.UserName as Company ,
                                ty.UserName as TaxType , tyr.TaxYearName,tg.Id TaxSavingGroupId,tg.UserName TaxSavingGroup,tg.MaxLimit
                                from dbo.IncomeTaxItemMaster itm
								left join hkp.TaxSavingGroup tg on tg.Id = itm.TaxSavingGroupId
                                left join org.Company c on c.Id = itm.CompanyId
                                left join dbo.TaxType ty on ty.Id = itm.TaxTypeId
                                left join scs.TaxYear tyr on tyr.Id = itm.TaxYearId where itm.CompanyId='" + Company + "' order by tg.[Sequence]";
                return _sqlRepository.GetDataCollection(sql, null);
            }
           catch(Exception e)
            {
                throw e;
            }
        }

        public IEnumerable<object> getSalaryHeads()
        {
            try
            {
                string sql = @"Select SalaryHeadId , SalaryHead from dbo.SalaryHead";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> getTaxSavingGroup()
        {
            try
            {
                string sql = @"Select Id , Username , MaxLimit from hkp.TaxSavingGroup order by [Sequence]";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public IEnumerable<object> getTaxSavingItem()
        {
            try
            {
                string sql = @"Select Id , Username from hkp.TaxSavingItem";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetTaxYear()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var strSQL = @"SELECT ID TaxYearID, TaxYearName FROM scs.TaxYear
                            WHERE ID IN (SELECT DISTINCT TaxYearID 
						                            FROM scs.CompanyTaxYear 
						                            WHERE Active = 1  and CompanyGroupId = '" + identity.CompanyGroupId + @"') order by StartDate";

                return _sqlRepository.GetDataCollection(strSQL);
            }
            catch(Exception e)
            {
                throw e;
            }
        }
        public IEnumerable<object> getChildList(string id)
        {
            try
            {
                string sql = @"Select tc.* , sh.SalaryHead ,ti.UserName as TaxSavingItem from dbo.IncomeTaxItemChild tc
                                left join dbo.SalaryHead sh on tc.SalaryHeadId = sh.SalaryHeadId
                                left join hkp.TaxSavingItem ti on ti.Id = tc.TaxSavingItemId
                                where tc.IncomeTaxItemMasterId = '" + id+ "' order by tc.Sequence";
                return _sqlRepository.GetDataCollection(sql);
            }
            catch(Exception e)
            {
                throw e;
            }
        }

        public Dictionary<string, object> Create (Dictionary<string, object> dataMaster )
        {
            try
            {
                //Master
                string TableName = "dbo.IncomeTaxItemMaster";
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where SystemId='" + dataMaster["SystemId"] + "'", out dsMaster, false, "1");
                DateTime now = DateTime.Today;
                
                string _Id = "";
                #region data update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);


                    //bplib.clsGenID objGenID = new bplib.clsGenID();
                    //objGenID.GenIDYearly(DateTime.Now.ToShortDateString().ToString(), "Tax", out _Id);

                    dataMaster["SystemId"] = now.ToString("yy") + '-'+_Id;
                    AddNewRow(dsMaster.Tables[0], dataMaster);
                }
                else
                {
                    _Id = dataMaster["SystemId"].ToString();
                    EditRow(dsMaster.Tables[0].Rows[0], dataMaster);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);



                return dataMaster;

            }
            catch (Exception ex)
            {

                throw  ex;

            }
        }

        public string CreateChild( Dictionary<string, object> dataChild, string maxLimit)
        {
            try
            {
                //Master
                string TableName = "dbo.IncomeTaxItemChild";
                DataSet dsChild;
                DataSet dsCheck;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                con.OpenDataSetThroughAdapter("select * from " + TableName + " where IncomeTaxItemMasterId = '"+dataChild["IncomeTaxItemMasterId"] +"' and TaxSavingItemId='" + dataChild["TaxSavingItemId"] + "' ", out dsCheck, false, "1");
                if(dataChild["Id"] != null)
                {
                    if (dsCheck.Tables[0].Rows.Count > 0)
                    {
                        if (dsCheck.Tables[0].Rows[0]["Id"].ToString() != dataChild["Id"].ToString())
                        {
                            throw new Exception("Same Tax Saving Group and Tax Saving Item is already Present!");
                        }
                    }
                    
                }
                else
                {
                    if (dsCheck.Tables[0].Rows.Count > 0)
                    {
                        throw new Exception("SameTax Saving Group and Tax Saving Item is already Present!");
                    }
                }
                


                con.OpenDataSetThroughAdapter("select * from " + TableName + " where Id='" + dataChild["Id"] + "'", out dsChild, false, "1");
                
                double limitToRec = OTSBD.clsStaticInfo.dbl(dataChild["Limit"].ToString());
                double mL = OTSBD.clsStaticInfo.dbl(maxLimit.ToString());
                
                string _Id = "";
                #region data update
                if (dsChild.Tables[0].Rows.Count == 0)
                {
                    
                    if ( mL < limitToRec)
                    {
                        throw new Exception("Limit Exceeds");
                    }
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _Id);


                    //bplib.clsGenID objGenID = new bplib.clsGenID();
                    //objGenID.GenIDYearly(DateTime.Now.ToShortDateString().ToString(), "Tax", out _Id);

                    dataChild["Id"] = _Id;
                    AddNewRow(dsChild.Tables[0], dataChild);
                }
                else
                {

                    double kk = mL - limitToRec;
                    if ( mL < limitToRec)
                    {
                        throw new Exception("Limit Exceeds");
                    }
                    _Id = dataChild["Id"].ToString();
                    EditRow(dsChild.Tables[0].Rows[0], dataChild);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsChild);



                return "Success";

            }
            catch (Exception ex)
            {

                return ex.Message;

            }
        }
        public string Delete(string id)
        {
            try
            {

                
                string TableName = "dbo.IncomeTaxItemMaster";
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where systemid='" + id + "'");
                con.CommitTransaction();
                
                return "Success";

            }
            catch (Exception ex)
            {
                
                return ex.Message;

            }
        }

        

        public string DeleteChild(string id)
        {
            try
            {
                string TableName = "dbo.IncomeTaxItemChild";
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");
                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from " + TableName + " where Id='" + id + "'");
                con.CommitTransaction();
                return "Success";

            }
            catch (Exception ex)
            {

                return ex.Message;

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

            dr.EndEdit();
        }
        
    }
}
