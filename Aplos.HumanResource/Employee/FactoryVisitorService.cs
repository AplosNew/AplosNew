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

                con.OpenDataSetThroughAdapter("select * from dbo.ItemScan where Id='" + items[0].Id + "'", out dsMaster, false, "1");

                foreach (VisitorModel item in DataToSave)
                {

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        DataRow dr = dsMaster.Tables[0].NewRow();


                        bplib.clsGenID id = new bplib.clsGenID();
                        id.GenIDYearly(DateTime.Now.ToShortDateString(), "Item Scan", out string NewId);



                        dr["Id"] = NewId;
                        dr["WorkDate"] = item.WorkDate;
                        dr["Time"] = item.Time;
                        dr["ShiftId"] = item.ShiftId;
                        dr["Grade"] = item.Grade;
                        dr["LocMasterId"] = item.LocMasterId;
                        dr["PurposeId"] = item.PurposeId;
                        dr["Remarks"] = item.Remarks;
                        dr["AddedBy"] = item.AddedBy;
                        dr["AddedDate"] = DateTime.Now.ToString();
                        dsMaster.Tables[0].Rows.Add(dr);


                    }
                    else
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        dr["WorkDate"] = item.WorkDate;
                        dr["Time"] = item.Time;
                        dr["ShiftId"] = item.ShiftId;
                        dr["Grade"] = item.Grade;
                        dr["LocMasterId"] = item.LocMasterId;
                        dr["PurposeId"] = item.PurposeId;
                        dr["Remarks"] = item.Remarks;
                        dr["UpdatedBy"] = item.UpdatedBy;
                        dr["UpdatedDate"] = DateTime.Now.ToString();

                        dr.EndEdit();
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
        public string Id { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedFromIP { get; set; }
        public string PurposeId { get; set; }
        public string Remarks { get; set; }
        public DateTime WorkDate { get; set; }
        public DateTime Time { get; set; }
        public string Grade { get; set; }
        public string LocMasterId { get; set; }
        public string ShiftId { get; set; }

    }

}
  