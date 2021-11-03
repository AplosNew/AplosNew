using clsAttendance;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.ViewModel.Organizations;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Threading;

namespace Library.Service.Attendances
{
    public class clsIncrementType
    {
        ISqlRepository _sqlRepository;
        public clsIncrementType(ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
        }

        #region Increment Group ---------------------

        public List<Dictionary<string, object>> GetIncrementTypeInfo()
        {
            try
            {
                var cmdText = @" select UserName,ShortName,StandardName,Id,Description,Remarks,Active,Code,Sequence
                                    ,Actives = case when Active=1 then 'Yes' else 'No' end
                                    from IncrementType";

                return _sqlRepository.GetDataCollection(cmdText);

            }
            catch (Exception)
            {
                throw;
            }
        }//end of function

        public void SaveIncrementTypeMaster(IncrementType master)
        {
            DataSet dsMaster = null;
         
            try
            {
                var code = CheckCode(master.Code, master.Id);
                if (code.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Code already exist.");
                }
                var UserName = CheckUserName(master.UserName, master.Id);
                if (UserName.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("UserName already exist.");
                }

                SaveIncrementTypeMasters( master,out dsMaster);

                clsStaticInfo obj = new clsStaticInfo();
                
                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }
           
        }

        private DataSet CheckCode(string Code, string id)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT Code FROM IncrementType WHERE Code='" + Code + "' and Id <>'" + id + "'"
            };
            return _sqlRepository.GetGridData(parameters).Source;

        }
        private DataSet CheckUserName(string UserName, string id)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @"SELECT UserName FROM IncrementType WHERE UserName='" + UserName + "' and Id <>'" + id + "'"
            };
            return _sqlRepository.GetGridData(parameters).Source;
        }


        void SaveIncrementTypeMasters(IncrementType master,out DataSet dsMaster)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dsMaster = null;
            try
            {
                IncrementTypeMaster(master.Id, out dsMaster);

                DataView dvMaster = new DataView(dsMaster.Tables[0]);
                dvMaster.RowFilter = "Id='" + master.Id + "' ";
                if (dvMaster.Count == 0)
                {
                    #region add
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "IncrementType", out sID);

                    DataRow dr = dsMaster.Tables[0].NewRow();
                    master.Id = "IT" + sID;
                    foreach (PropertyInfo prop in master.GetType().GetProperties())
                    {
                        SetRowValue(ref dr, prop.Name, prop.GetValue(master, null));
                    }
                    dsMaster.Tables[0].Rows.Add(dr);
                    #endregion
                }
                else
                {
                    #region edit

                    DataRow dr = dvMaster[0].Row;
                    dr.BeginEdit();

                    foreach (PropertyInfo prop in master.GetType().GetProperties())
                    {
                        SetRowValue(ref dr, prop.Name, prop.GetValue(master, null));
                    }
                    dr.EndEdit();
                    #endregion
                }
                dvMaster.RowFilter = null;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        void SetRowValue(ref DataRow dr, string Field, object v)
        {
            try
            {
                if (v is null)
                {
                    dr[Field] = DBNull.Value;
                }
                else
                {
                    dr[Field] = v;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void IncrementTypeMaster(string Id, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM  IncrementType where ID='" + Id + @"' ";

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

        void DeleteDepartmentDetails(string DepartmentGroupId, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"delete FROM  DepartmentGroupDetails where DepartmentGroupId='" + DepartmentGroupId + @"' ";

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


        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(Contract), out sID);
            return sID;
        }


        #endregion

        #region Salary Head Payment ------------

        public void SalaryHeadWisePaymentMode(SalaryHeadPaymentPolicy salaryheadpayment)
        {
            DataSet dsMaster = null;

            try
            {
                Savesalaryhead(salaryheadpayment, out dsMaster);

                clsStaticInfo obj = new clsStaticInfo();

                obj.SaveDataSets(dsMaster);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        void Savesalaryhead(SalaryHeadPaymentPolicy salaryheadpayment, out DataSet dsMaster)
        {

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dsMaster = null;
            try
            {
                var SameHead = CheckHead(salaryheadpayment.PaymentMode, salaryheadpayment.SalaryHeadId, salaryheadpayment.Id, salaryheadpayment.PlantId);
                if (SameHead.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Same Salary Head And PaymentMode already exist.");
                }

                Getselecttable(salaryheadpayment.Id, out dsMaster);

                DataView dvMaster = new DataView(dsMaster.Tables[0]);
                dvMaster.RowFilter = "Id='" + salaryheadpayment.Id + "' ";
                if (dvMaster.Count == 0)
                {
                    #region add
                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "SalaryHeadWisePaymentModePolicy", out sID);

                    DataRow dr = dsMaster.Tables[0].NewRow();
                    salaryheadpayment.Id = "PM" + sID;
                    foreach (PropertyInfo prop in salaryheadpayment.GetType().GetProperties())
                    {
                        SetRowValue(ref dr, prop.Name, prop.GetValue(salaryheadpayment, null));
                    }
                    dsMaster.Tables[0].Rows.Add(dr);
                    #endregion
                }
                else
                {
                    #region edit

                    DataRow dr = dvMaster[0].Row;
                    dr.BeginEdit();

                    foreach (PropertyInfo prop in salaryheadpayment.GetType().GetProperties())
                    {
                        SetRowValue(ref dr, prop.Name, prop.GetValue(salaryheadpayment, null));
                    }
                    dr.EndEdit();
                    #endregion
                }
                dvMaster.RowFilter = null;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private DataSet CheckHead(string PaymentMode, string SalaryHeadId,string Id,string PlantId)
        {
            GridParameter parameters;
            parameters = new GridParameter
            {
                ExportType = "DATASET",
                CmdText = @" select shpm.Id
                                    from SalaryHeadWisePaymentModePolicy shpm
                                    left join SalaryHead sh on sh.SalaryHeadID =shpm.SalaryHeadId
                                    where shpm.PaymentMode='"+ PaymentMode + "' and sh.SalaryHeadID='" + SalaryHeadId +"' and shpm.Id<>'"+ Id+"' and shpm.PlantId = '"+ PlantId + "'  "
            };
            return _sqlRepository.GetGridData(parameters).Source;

        }
        void Getselecttable(string Id, out System.Data.DataSet dsRef)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon;
            try
            {
                strSQL = @"SELECT * FROM  SalaryHeadWisePaymentModePolicy where ID='" + Id + @"' ";

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

        #endregion

    }
    public class IncrementType
    {
        public string Id { get; set; }
        public string Sequence { get; set; }
        public string Code { get; set; }
        public string ShortName { get; set; }
        public string StandardName { get; set; }
        public string UserName { get; set; }
        public string Description { get; set; }
        public string Remarks { get; set; }
        public bool Active { get; set; }
       
        public string AddedBy { get; set; }
        [NeverUpdate]
        public DateTime? AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        [NeverUpdate]
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }

    public class SalaryHeadPaymentPolicy
    {
        public string Id { get; set; }
        public string CompanyGroupId { get; set; }
        public string PlantId { get; set; }
        public string SalaryHeadId { get; set; }
        public string PaymentMode { get; set; }
        public string Amount { get; set; }
        

        public string AddedBy { get; set; }
        [NeverUpdate]
        public DateTime? AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        [NeverUpdate]
        public DateTime? UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }
    }
}
