using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Library.Data.Sql;
using OTSBD;
using bplib;

namespace Library.HumanResource.Employee
{
    public class FactoryVisitorService
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;

        public FactoryVisitorService()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
        }
        public string SaveExpectedVisit(IEnumerable<VisitorModel> DataToSave)
        {
            try
            {
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                if (DataToSave.Count() == 0)
                    return "";
                List<VisitorModel> items = DataToSave.ToList();

                con.OpenDataSetThroughAdapter("select * from dbo.VisitorServiceData where 1=2", out dsMaster, false, "1");

                foreach (VisitorModel item in DataToSave)
                {

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();
                        clsGenID genid = new clsGenID();
                        genid.GenID("VisitorService", out string _Id);
                     
                        dr["Id"] = "VSD"+ _Id;
                        dr["CardNo"] = DBNull.Value;
                        dr["ExpectedDate"] = item.ExpectedDate;
                        dr["ExpectedTime"] = item.ExpectedTime;
                        dr["VisitorCategory"] = item.VisitorCategory;
                        dr["VisitorType"] = item.VisitorType;
                        dr["VisitorName"] = item.VisitorName;
                        dr["ToMeet"] = item.ToMeet;
                        dr["Purpose"] = item.Purpose;
                        dr["Remarks"] = item.Remarks;
                        dr["InDone"] = false;
                        dr["OutDone"] = false;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = DateTime.Now.ToString();
                        dr["AddedFromIP"] = item.AddedFromIP;
                        dsMaster.Tables[0].Rows.Add(dr);
                    }                   

                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);
                string MasterId = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                return MasterId;

            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }

    public class VisitorModel
    {       
        #region Fixed Fields
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedFromIP { get; set; }
        #endregion

        #region Other Fields
        public string Id { get; set; }
        public string CardNo { get; set; }
        public string Purpose { get; set; }
        public string Remarks { get; set; }
        public DateTime ExpectedDate { get; set; }
        public DateTime ExpectedTime { get; set; }
        public DateTime InDate { get; set; }
        public DateTime InTime { get; set; }
        public DateTime OutDate { get; set; }
        public DateTime OutTime { get; set; }
        public string VisitorType { get; set; }
        public string VisitorCategory { get; set; }
        public string VisitorName { get; set; }
        public string MobileNo { get; set; }
        public string ToMeet { get; set; }
        public decimal NoOfPerson { get; set; }
        public string InDone { get; set; }
        public string OutDone { get; set; }

        #endregion
    }

}
  