#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Setups;
using Library.Security.Core;
using Library.Service.Core;
using Library.Service.Setups;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.Setups.Controllers
{
    public class BusinessProcessController : BaseController
    {
        #region Constructor

        private readonly IBusinessProcessService _businessProcessService;
        private readonly ISqlRepository _sqlRepository;
        public BusinessProcessController(IBusinessProcessService businessProcessService, ISqlRepository R)
        {
            _businessProcessService = businessProcessService;
            _sqlRepository = R;
        }

        #endregion Constructor

        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetList(GridParameter parameters, string companyGroupId)
        {
            return Json(_businessProcessService.Query(parameters, companyGroupId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetBusinessProcessList(string materialMasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_businessProcessService.GetBusinessProcessList(identity.CompanyGroupId, materialMasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(BusinessProcess BusinessProcess)
        {
            _businessProcessService.Insert(BusinessProcess);
            return Json(new { BusinessProcess, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(BusinessProcess BusinessProcess)
        {
            _businessProcessService.Update(BusinessProcess);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public JsonResult Delete(string id)
        {
            _businessProcessService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, Authorize]
        public JsonResult GetDynamicColList(string businessProcessId)
        {
            try
            {
                string sql = @"SELECT * FROM dbo.FabricRollManagementColSetting Where ISNULL(BusinessProcessId,'" + businessProcessId + "')='" + businessProcessId + "'";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult SaveBPSatting(List<Dictionary<string, object>> funds, string BusinessProcessId)
        {
            try
            {

                SaveData(funds, BusinessProcessId);


                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, ex.Message });
            }

        }

        private string GetPK()
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "FabricRollManagementColSetting", out sID);
            return sID;
        }

        private void SaveData(List<Dictionary<string, object>> funds, string BusinessProcessId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster, dsMasterOrder, dsfunds;
            string contId = string.Empty;
            string id = string.Empty;
            try
            {

                #region FUND 

                DataSet dsChild;

                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.FabricRollManagementColSetting where  BusinessProcessId='" + BusinessProcessId + "'", out dsChild, false, "1");
                #region data update

                if (funds != null)
                {
                    foreach (var item in funds)
                    {
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            item["Id"] = GetPK();
                            item["BusinessProcessId"] = BusinessProcessId;

                            AddNewRow(dsChild.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }
                #endregion

                #endregion

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsChild);

            }
            catch (Exception ex)
            {
                throw (ex);
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
            dr["AddedFromIP"] = identity.IPAddress;

            dr["UpdatedBy"] = identity.Name;
            dr["UpdatedDate"] = System.DateTime.Now.ToString();
            dr["UpdatedFromIP"] = identity.IPAddress;

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
            dr["UpdatedFromIP"] = identity.IPAddress;

            dr.EndEdit();
        }

        public JsonResult GetQueryResult()
        {
            string sql = null;
            return new JsonResult
            {
                ContentEncoding = Encoding.UTF8,
                ContentType = "application/json;",
                Data = _sqlRepository.GetDataCollection(sql),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,

            };
        }

        public JsonResult SaveBPTable(string BusinessProcess)
        {
            string schema = "BPDT.";
            string sql = @"CREATE TABLE " + schema + "" + BusinessProcess + " (Id Varchar(30) primary key, Sequence decimal(18,2) NULL, UserName varchar(50) NULL);";
            return Json(new
            {
                ContentEncoding = Encoding.UTF8,
                ContentType = "application/json;",
                Data = _sqlRepository.GetDataCollection(sql),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Message = AplosMessage.Insert
            });
        }


        public JsonResult SaveAlterBPTable(string BusinessProcessId, string BusinessProcess, string columnName, string dataType, string nullable)
        {
            string schema = "BPDT.";
            if (dataType == "varchar30")
            {
                dataType = "varchar(30)";
            }
            if (dataType == "varchar90")
            {
                dataType = "varchar(90)";
            }
            if (dataType == "varchar250")
            {
                dataType = "varchar(250)";
            }
            if (dataType == "DECIMAL")
            {
                dataType = "decimal(18,4)";
            }
            SaveBusinessProcessDataTableColumnCreationData(BusinessProcessId, columnName, dataType, nullable);
            string sql = @"ALTER TABLE " + schema + "" + BusinessProcess + " ADD " + columnName + " " + dataType + " " + nullable + "";

            return Json(new
            {
                ContentEncoding = Encoding.UTF8,
                ContentType = "application/json;",
                Data = _sqlRepository.GetDataCollection(sql),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Message = AplosMessage.Insert
            });

        }

        private double GetSequence(string BusinessProcessId)
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM BusinessProcessDataTableColumnCreation Where BusinessProcessId='" + BusinessProcessId + "'");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        private void SaveBusinessProcessDataTableColumnCreationData(string BusinessProcessId, string columnName, string dataType, string nullable)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            string _Id = "";
            double Sequence = 0;

            bplib.clsGenID genid = new bplib.clsGenID();
            genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "BPDTCC", out _Id);
            Sequence = GetSequence(BusinessProcessId);
            try
            {

                string sql = "SELECT * FROM dbo.BusinessProcessDataTableColumnCreation WHERE Id='" + _Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    dr["Id"] = _Id;
                    dr["BusinessProcessId"] = BusinessProcessId;
                    dr["Sequence"] = Sequence;
                    dr["ColumnName"] = columnName;
                    dr["UseName"] = columnName;
                    dr["DataType"] = dataType;
                    dr["NullAble"] = nullable;

                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = DateTime.Now;
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsMaster.Tables[0].Rows.Add(dr);
                }

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);

            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        public JsonResult DropAlterBPTable(string businessProcessId, string BusinessProcess, string columnName)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;

            strSQL = "DELETE FROM dbo.BusinessProcessDataTableColumnCreation where BusinessProcessId='" + businessProcessId + "' AND ColumnName='" + columnName + "'";
            objCon = new ConnectionManager.DAL.ConManager("1");
            objCon.OpenConnection("1");
            objCon.BeginTransaction();
            objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
            objCon.CommitTransaction();

            string schema = "BPDT.";

            DataTable dt = _sqlRepository.GetDataTable("SELECT  * FROM " + schema + "" + BusinessProcess + " Where " + columnName + " IS NOT NULL");
            if (dt.Rows.Count > 0)
            {
                throw new Exception("This " + columnName + " has value, so column can't drop.");
            }
            string sql = @"ALTER TABLE " + schema + "" + BusinessProcess + " DROP COLUMN " + columnName + "";

            return Json(new
            {
                ContentEncoding = Encoding.UTF8,
                ContentType = "application/json;",
                Data = _sqlRepository.GetDataCollection(sql),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Message = AplosMessage.Deleted
            });
        }

        [HttpGet, Authorize]
        public JsonResult GetBusinessProcessDataTable(string businessProcessId)
        {

            try
            {
                string _sql = @"SELECT * FROM dbo.BusinessProcessDataTableColumnCreation Where BusinessProcessId='" + businessProcessId + "'";
                return Json(_sqlRepository.GetDataCollection(_sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Define Enum
        [Authorize]
        public ActionResult DefineEnum()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetDefineEnumlist()
        {

            try
            {
                string _sql = @"SELECT * FROM dbo.[DefineEnum] ";
                return Json(_sqlRepository.GetDataCollection(_sql), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public enum DefineEnumName
        {
            BulkPacking
            , Detention
            , DirectMaterial
            , DirectProcess
            , FOB
            , FinalPacking
            , IndividualPacking
            , Machine
            , OrderLineItem
            , Operation
            , Profit
            , Production
            , ProductionOrder
            , SalesOrder
            , SalesExpense
            , ValueLoss
            , WorkCenter
            , StandardDuration
            , LoadFactor
            , PlanEffeciency
            , SKU1
            , SKU2
            , SKU1SKU2
            , Advance 
            , PaymentTerm 
            , DeleteEntry 
            , Modify 
        }

        [HttpGet, Authorize]
        public JsonResult GetCboDefineEnumName()
        {
            return Json(EnumService.GetEnumCbo<DefineEnumName>(), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SaveDefineEnum(Dictionary<string, object> datas)
        {
            try
            {
                var data = SaveDefineEnumData(datas);
                return Json(new { Error = false, Data = data, Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });

            }
        }


        public Dictionary<string, object> SaveDefineEnumData(Dictionary<string, object> datas)
        {

            try
            {
                //Master Table - PMSMaster
                string TableName = "[DefineEnum]";
                DataSet dsMaster;

                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + TableName + " where EnumName ='" + datas["EnumName"] + "'", out dsMaster, false, "1");

                string _Id = "";

                #region data Master update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    AddNewRow(dsMaster.Tables[0], datas);
                }
                else
                {
                    EditRow(dsMaster.Tables[0].Rows[0], datas);
                }
                #endregion data update

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);

                return datas;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        public JsonResult DeleteDefineEnum(string id)
        {
            DeleteDefineEnumData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteDefineEnumData(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [dbo].[DefineEnum] WHERE Id='" + Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    objCon.CloseConnection();
                    throw (ex);
                }
                catch (Exception)
                {
                    throw ex;
                }
            }
            finally
            {

                objCon = null;
            }
        }//End of function

        #endregion
    }
}