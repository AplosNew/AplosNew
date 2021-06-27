using Library.Crosscutting.Security;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Library.HumanResource.Payroll.Setting
{
   public class clsBankCashPercentageSettinng
   {
        string TableName = "BankCashPercentageSettinng";
        public void Save(BankCashPercentage bp, BankCashPercentage cp, CustomIdentity identity, out string _bpid,out string _cpid)
        {
            _bpid = string.Empty;
            _cpid = string.Empty;
            try
            {
                DataSet dsBvalida;
                DataSet dsCvalida;
                //valida
                if (string.IsNullOrEmpty(bp.PlantId))
                {
                    throw new Exception("Plant is blank...");
                }

               
                BankValidation(bp.PlantId, "Bank",bp.Id, out dsBvalida);
                if(dsBvalida.Tables[0].Rows.Count>0)
                {
                    throw new Exception("Formula has already been defined for Bank");
                }
                BankValidation(bp.PlantId, "Cash",cp.Id, out dsCvalida);
                if (dsCvalida.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Formula has already been defined for Cash");
                }

                DataSet dsMaster;
                GetBankCashPercentageSettinng(bp.PlantId, out dsMaster);

                if (string.IsNullOrEmpty(bp.FormulaDescription)==false && string.IsNullOrEmpty(cp.FormulaDescription)==false)
                {
                    _save(ref dsMaster, bp, identity, out _bpid);
                    _save(ref dsMaster, cp, identity, out _cpid); 
                }



                if (string.IsNullOrEmpty(bp.FormulaDescription) && string.IsNullOrEmpty(cp.FormulaDescription))
                {
                    DeleteBCP(bp.PlantId);
                }

                    clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {

                throw ex;

            }
        }

        public void DeleteBCP(string plantid)
        {
            bool IsTransactionStarted = false;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                IsTransactionStarted = true;
                objCon.ExecuteNonQueryWrapper(" delete  from BankCashPercentageSettinng where PlantId='" + plantid + @"'  ", true, "1");
                objCon.CommitTransaction();
                IsTransactionStarted = false;
            }
            catch (Exception ex)
            {
                try
                {
                    if (IsTransactionStarted)
                    {
                        objCon.RollBack();
                    }
                }
                catch (Exception exx)
                {
                    throw (ex);

                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End Function
        void _save(ref DataSet dsMaster, BankCashPercentage data, CustomIdentity identity,out string _id)
        {
            _id = "";
            try
            {
                var dvmaster = new DataView(dsMaster.Tables[0]);
                dvmaster.RowFilter = "id ='" + data.Id + "'";

                if (dvmaster.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenID(TableName, out _id);

                    DataRow dr = dsMaster.Tables[0].NewRow();
                    AddNewRow(identity, data, ref dr);
                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    _id = data.Id.ToString();
                    DataRow dr = dvmaster[0].Row;
                    dr.BeginEdit();
                    EditRow(identity, data, ref dr);
                    dr.EndEdit();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        
        private void AddNewRow(CustomIdentity identity, BankCashPercentage obj,ref DataRow dr)
        {          
            dr["HeadLabel"] = obj.HeadLabel;
            dr["FormulaDes"] = obj.FormulaDescription;
            dr["FormulaDesID"] = obj.FormulaIDDescription;
            dr["PlantId"] = obj.PlantId;

            dr["AddedBy"] = identity.Name;
            dr["AddedDate"] = System.DateTime.Now.ToString();
            dr["AddedFromIP"] = identity.IPAddress;
            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;           
        }

        private void EditRow(CustomIdentity identity, BankCashPercentage obj, ref DataRow dr)
        {

            dr["HeadLabel"] = obj.HeadLabel;
            dr["FormulaDes"] = obj.FormulaDescription;
            dr["FormulaDesID"] = obj.FormulaIDDescription;

            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;
           
        }
        public void BankValidation(string plantid,string HeadLabel, int bpid, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM BankCashPercentageSettinng WHERE plantid='"+ plantid + "' and HeadLabel='"+ HeadLabel + "' and  id <> '" + bpid + @"'";
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

        public void GetBankCashPercentageSettinng(string PlantID, out DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = "SELECT * FROM BankCashPercentageSettinng WHERE  PlantID = '" + PlantID + @"'";
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


   }
}

public class BankCashPercentage
{
    public int Id { get; set; }
    public string HeadLabel { get; set; }
    public string FormulaDescription { get; set; }
    public string FormulaIDDescription { get; set; }
    public string PlantId { get; set; }


}
