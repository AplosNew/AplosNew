using Library.Data.Sql;
using Library.Service.Payrolls.SalaryProcessActive;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Library.Service.Payrolls.SalaryProcess
{
   public class SalaryProcessDic
    {
        private readonly ISqlRepository _sqlRepository;
        public SalaryProcessDic(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        public List<Dictionary<string, object>> GetSeparatedEmpInfo(string FromDate, string ToDate, string PlantId)
        {
            DataSet ds = null;
            //clsEmployeeLoad objEL = null;
            clsSalaryProcessQuery2 objEL = null;
            string sql = string.Empty;
            try
            {
                objEL = new clsSalaryProcessQuery2();
                objEL.LoadEmpSalaryProcGrid("", PlantId, "", FromDate, ToDate, out ds);
                DataView dv = new DataView(ds.Tables[0]);
                dv.RowFilter = "EmployeeStatus='" + bplib.clsWebLib.EmployeeStatus_Separated + "' and DOSs >= '" + FromDate + "' and DOSs <='" + ToDate+"'  ";
                //dv.RowFilter = "EmployeeStatus='" + bplib.clsWebLib.EmployeeStatus_Separated + "' and DOSs>='" + FromDate + "'   ";
                var dt = dv.ToTable();
                List<Dictionary<string, object>> dic = CustomJsonResultService.DataTableToJson(dt);
                return dic;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<Dictionary<string, object>> GetSeparatedEmpPresentZeroInfo(string FromDate, string ToDate, string PlantId)
        {
            DataSet ds = null;
            //clsEmployeeLoad objEL = null;
            clsSalaryProcessQuery2 objEL = null;
            string sql = string.Empty;
            try
            {
                objEL = new clsSalaryProcessQuery2();
                objEL.SeparatedEmpZeroPresent("", PlantId, "", FromDate, ToDate, out ds);
                DataView dv = new DataView(ds.Tables[0]);
                dv.RowFilter = "EmployeeStatus='" + bplib.clsWebLib.EmployeeStatus_Separated + "' and DOSs >= '" + FromDate + "' and DOSs <='" + ToDate + "'  ";
                //dv.RowFilter = "EmployeeStatus='" + bplib.clsWebLib.EmployeeStatus_Separated + "' and DOSs>='" + FromDate + "'   ";
                var dt = dv.ToTable();
                List<Dictionary<string, object>> dic = CustomJsonResultService.DataTableToJson(dt);
                return dic;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<Dictionary<string, object>> GetSeparatedApprovedEmpInfo(string FromDate, string ToDate, string PlantId)
        {
            DataSet ds = null;
            clsSalaryProcessQuery objEL = null;
            string sql = string.Empty;
            try
            {
                objEL = new clsSalaryProcessQuery();
                objEL.LoadSeparatedApprocedEmp(PlantId, FromDate, ToDate, out ds);
                List<Dictionary<string, object>> dic = CustomJsonResultService.DataTableToJson(ds.Tables[0]);
                return dic;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Dictionary<string, object>> GetMLVEmpInfo(string FromDate, string ToDate, string PlantId)
        {
            DataSet ds = null;
            //clsEmployeeLoad objEL = null;//
            clsSalaryProcessQuery2 objEL = null;//clsSalaryProcessQuery2
            string sql = string.Empty;
            try
            {
                objEL = new clsSalaryProcessQuery2();
                objEL.LoadMLV(PlantId,FromDate, ToDate, out ds);       
                List<Dictionary<string, object>> dic = CustomJsonResultService.DataTableToJson(ds.Tables[0]);
                return dic;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<Dictionary<string, object>> GetMLVProcessedEmpInfo(string FromDate, string ToDate, string PlantId)
        {
            DataSet ds = null;
            //clsEmployeeLoad objEL = null;
            clsSalaryProcessQuery2 objEL = null;
            string sql = string.Empty;
            try
            {
                objEL = new clsSalaryProcessQuery2();
                objEL.LoadMLVProcessed(PlantId, FromDate, ToDate, out ds);
                List<Dictionary<string, object>> dic = CustomJsonResultService.DataTableToJson(ds.Tables[0]);
                return dic;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<Dictionary<string, object>> GettbsmpInfo(string FromDate, string ToDate, string PlantId)
        {
            DataSet ds = null;
            clsEmployeeLoad objEL = null;
            string sql = string.Empty;
            try
            {
                objEL = new clsEmployeeLoad();
                objEL.LoadStatusDifferent("",PlantId, "",FromDate, ToDate, out ds);
                //DataView dv = new DataView(ds.Tables[0]);
                //// dv.RowFilter = "";
                //dv.RowFilter = "EmployeeStatus='" + bplib.clsWebLib.EmployeeStatus_Separated + "' and DOSs>='" + FromDate + "'  ";
                //var dt = dv.ToTable();
                //var dic = dt.ToList<Dictionary<string, object>>();
                List<Dictionary<string, object>> dic = CustomJsonResultService.DataTableToJson(ds.Tables[0]);
                return dic;
                //return Json(_sqlRepository.GetDataCollection(identity.PlantId, FromDate, ToDate, identity.IsSysAdmin, identity.IsControlAdmin, identity.UserId), JsonRequestBehavior.AllowGet);
                //return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
