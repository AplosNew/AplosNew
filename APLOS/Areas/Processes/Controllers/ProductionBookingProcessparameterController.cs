#region Using
using Library.Model.OrderManagements;
using Aplos.Properties;
using Library.Service.OrderManagements;
using Library.Core;
using System.Web.Mvc;
using Aplos.Controllers;
using System;
using System.Data;
using Library.Crosscutting.Security;
using System.Threading;
using System.Collections.Generic;
using Library.Data.Sql;
using Library.Security.Core;

#endregion

namespace Aplos.Areas.Processes.Controllers
{
    public class ProductionBookingProcessparameterController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        string TableName = "dbo.ProductionBookingProcessParameter";
        public ProductionBookingProcessparameterController(ISqlRepository R)
        {
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages
       
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence(string masterId)
        {
            return Json(GetSequence(masterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult getautosequenceDetention(string masterId)
        {
            return Json(GetSequenceDetention(masterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetQualityProcessParameterAutoSequence(string QualityProcessId)
        {
            return Json(GetQualityProcessParameterSequence(QualityProcessId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetHeaderItemCbo(string id,string masterId)
        { 
            return Json(_sqlRepository.GetDataCollection("SELECT Id AS Value, UserName AS Text FROM [dbo].[ProductionBookingParameter] WHERE Id<>'" + id + "' AND ProductionBookingProcessParameterId='"+ masterId + "'"), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetHeaderItemDetentionCbo(string id, string masterId)
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id AS Value, UserName AS Text FROM [dbo].[DetentionMasterMachineParameter] WHERE Id<>'" + id + "' AND DetentionMasterId='" + masterId + "'"), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public JsonResult GetQualityProcessParameterHeaderItemCbo(string id, string masterId)
        {
            return Json(_sqlRepository.GetDataCollection("SELECT Id AS Value, UserName AS Text FROM [dbo].[QualityProcessParameter] WHERE QualityProcessId='"+masterId+"' AND  Id<>'" + id + "'"), JsonRequestBehavior.AllowGet);
        }

        private double GetSequence(string masterId)
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  ISNULL(Max(Sequence),0) AS Sequence FROM dbo.ProductionBookingParameter Where ProductionBookingProcessParameterId='"+masterId+"'");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        private double GetSequenceDetention(string masterId)
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  ISNULL(Max(Sequence),0) AS Sequence FROM dbo.DetentionMasterMachineParameter Where DetentionMasterId = '" + masterId + "'");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        private double GetQualityProcessParameterSequence(string QualityProcessId)
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT ISNULL(Max(Sequence),0) AS Sequence FROM dbo.QualityProcessParameter Where QualityProcessId='"+ QualityProcessId + "'");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        [HttpGet, Authorize]
        public ActionResult GetList()
        {

            string sql = @"SELECT N.*,P.UserName Process,uom.Code InputItemUoM,ouom.Code OutputItemUoM
  FROM [dbo].[ProductionBookingProcessParameter] N 
LEFT JOIN HKP.Process AS p ON P.Id=N.ProcessId
LEFT JOIN SCS.UnitOfMeasurement AS uom ON uom.Id=N.InputItemUoMId
LEFT JOIN SCS.UnitOfMeasurement AS ouom ON ouom.Id=N.OutputItemUoMId";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetQualityProcessList(string masterId)
        {

            string sql = @"SELECT N.*,P.UserName Process
  FROM [dbo].[ProductionQualityProcess] N 
LEFT JOIN HKP.QualityProcess AS p ON P.Id=N.ProcessId
Where N.ProductionBookingProcessParameterId='" + masterId+"'";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetProcessParameterList(string masterId)
        {

            string sql = @"SELECT N.* FROM [dbo].[ProductionBookingParameter] N Where ProductionBookingProcessParameterId='"+masterId+"' Order By N.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetProcessDetentionParameterList(string masterId)
        {

            string sql = @"SELECT N.* FROM [dbo].[DetentionMasterMachineParameter] N Where DetentionMasterId='" + masterId + "' Order By N.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetQualityProcessParameterList(string masterId)
        {

            string sql = @"SELECT N.* FROM [dbo].[QualityProcessParameter] N Where QualityProcessId='" + masterId + "' Order By N.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetDetailList(string OrderLineCostingItemId)
        {

            string sql = @"SELECT D.Sequence,D.ProductionBookingParameterHeadId
                            ,SalaryHead= CASE WHEN ISNULL(SD.UserName,'')<>'' THEN SD.UserName ELSE D.Component END,D.Component,D.ProductionBookingParameterId
                            FROM [dbo].[FormulaDetail] D
                            LEFT JOIN dbo.ProductionBookingParameter SD ON SD.Id=D.ProductionBookingParameterHeadId
                            WHERE ProductionBookingParameterId='" + OrderLineCostingItemId + "' Order By D.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetDetentionDetailList(string OrderLineCostingItemId)
        {

            string sql = @"SELECT D.Sequence,D.DetentionMasterMachineParameterHeadId
                            ,SalaryHead= CASE WHEN ISNULL(SD.UserName,'')<>'' THEN SD.UserName ELSE D.Component END,D.Component,D.DetentionMasterMachineParameterId
                            FROM [dbo].[FormulaDetail] D
                            LEFT JOIN dbo.DetentionMasterMachineParameter SD ON SD.Id=D.DetentionMasterMachineParameterHeadId
                            WHERE DetentionMasterMachineParameterId='" + OrderLineCostingItemId + "' Order By D.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetQualityProcessParameterDetailList(string QualityProcessParameterId)
        {

            string sql = @"SELECT D.Sequence,D.QualityProcessParameterHeadId
                            ,SalaryHead= CASE WHEN ISNULL(SD.UserName,'')<>'' THEN SD.UserName ELSE D.Component END,D.Component,D.QualityProcessParameterId
                            FROM [dbo].[FormulaDetail] D
                            LEFT JOIN dbo.QualityProcessParameter SD ON SD.Id=D.QualityProcessParameterId
                            WHERE QualityProcessParameterId='" + QualityProcessParameterId + "' Order By D.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data)
        {
            try
            {
                SaveMasterData(data, out string Id);
                return Json(new { Id, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        private void SaveMasterData(Dictionary<string, object> data, out string Id)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                 Id = "";
                if (data != null)
                {
                    string _Id = "";

                    DataSet dsMaster;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.ProductionBookingProcessParameter WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");


                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ProductionBookingProcessParameter", out _Id);

                        data["Id"] = _Id;
                        AddNewRow(dsMaster.Tables[0], data);
                    }
                    else
                    {
                        data["AddedBy"] = dsMaster.Tables[0].Rows[0]["AddedBy"].ToString();
                        data["AddedDate"] = dsMaster.Tables[0].Rows[0]["AddedDate"].ToString();
                        data["AddedFromIP"] = dsMaster.Tables[0].Rows[0]["AddedFromIP"].ToString();
                        EditRow(dsMaster.Tables[0].Rows[0], data);
                    }

                     Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);


                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost]
        public JsonResult CreateProcessParameter(Dictionary<string, object> data, IEnumerable<ProductionBookingParameterFormulaDetail> details)
        {
            try
            {
                SaveCostingSOTemplateData(data, details);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        private void SaveCostingSOTemplateData(Dictionary<string, object> data, IEnumerable<ProductionBookingParameterFormulaDetail> details)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                if (data != null)
                {
                    string _Id = "";

                    DataSet dsMaster, dsDestination=null;
                    DataRow drF;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                    con.OpenDataSetThroughAdapter("select * from ProductionBookingParameter where UserName='" + data["UserName"] + "'  AND  Id<>'" + data["Id"] + "' AND ProductionBookingProcessParameterId='"+data["ProductionBookingProcessParameterId"] +"'", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                        throw new Exception("UserName already exists!!!");


                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.ProductionBookingParameter WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.FormulaDetail Where ProductionBookingParameterId='" + data["Id"] + "'", out dsDestination, false, "1");

                    if (data["EntryState"].ToString()=="Entry")
                    {
                        data["Formula"] = DBNull.Value;
                        data["FormulaId"] = DBNull.Value;

                       
                        while (dsDestination.Tables[0].DefaultView.Count > 0)
                            dsDestination.Tables[0].DefaultView[0].Delete();
                    }
                   

                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), nameof(ProductionBookingParameter), out _Id);

                        data["Id"] =  _Id;
                        AddNewRow(dsMaster.Tables[0], data);
                    }
                    else
                    {
                        _Id = data["Id"].ToString();
                        EditRow(dsMaster.Tables[0].Rows[0], data);
                    }

                    string Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                    #region NoticePeriodFormulaDetail 

                    if (data["EntryState"].ToString() == "Calculate")
                    {
                        while (dsDestination.Tables[0].DefaultView.Count > 0)
                            dsDestination.Tables[0].DefaultView[0].Delete();
                        int count = 0;
                        if (details != null)
                        {

                            foreach (var item in details)
                            {
                                drF = dsDestination.Tables[0].NewRow();
                                count++;
                                string pk = _Id + "_" + count;
                                drF["Id"] = pk;
                                drF["ProductionBookingParameterId"] = _Id;
                                drF["Sequence"] = item.Sequence;
                                drF["ProductionBookingParameterHeadId"] = item.ProductionBookingParameterHeadId;
                                drF["Component"] = item.Component;

                                dsDestination.Tables[0].Rows.Add(drF);
                            }

                        } 
                    }
                    #endregion NoticePeriodFormulaDetail 

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster, dsDestination);


                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        [HttpPost]
        public JsonResult CreateQualityProcess(Dictionary<string, object> data)
        {
            try
            {
                SaveQualityProcessData(data);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        private void SaveQualityProcessData(Dictionary<string, object> data)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                if (data != null)
                {
                    string _Id = "";

                    DataSet dsMaster;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.ProductionQualityProcess WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");


                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "ProductionQualityProcess", out _Id);

                        data["Id"] = _Id;
                        AddNewRow(dsMaster.Tables[0], data);
                    }
                    else
                    {
                        data["AddedBy"] = dsMaster.Tables[0].Rows[0]["AddedBy"].ToString();
                        data["AddedDate"] = dsMaster.Tables[0].Rows[0]["AddedDate"].ToString();
                        data["AddedFromIP"] = dsMaster.Tables[0].Rows[0]["AddedFromIP"].ToString();
                        EditRow(dsMaster.Tables[0].Rows[0], data);
                    }


                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster);


                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        [HttpPost]
        public JsonResult CreateQualityProcessParameter(Dictionary<string, object> data, IEnumerable<QualityProcessParameterFormulaDetail> details)
        {
            try
            {
                SaveQualityProcessParameterData(data, details);
                return Json(new { Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        private void SaveQualityProcessParameterData(Dictionary<string, object> data, IEnumerable<QualityProcessParameterFormulaDetail> details)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            try
            {
                if (data != null)
                {
                    string _Id = "";

                    DataSet dsMaster, dsDestination;
                    ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");

                    con.OpenDataSetThroughAdapter("select * from QualityProcessParameter where UserName='" + data["UserName"] + "'  AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                    if (dsMaster.Tables[0].Rows.Count > 0)
                        throw new Exception("UserName already exists!!!");


                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.QualityProcessParameter WHERE Id='" + data["Id"] + "'", out dsMaster, false, "1");
                    con.OpenDataSetThroughAdapter("SELECT * FROM dbo.FormulaDetail Where QualityProcessParameterHeadId='" + data["Id"] + "'", out dsDestination, false, "1");


                    if (dsMaster.Tables[0].Rows.Count == 0)
                    {
                        bplib.clsGenID genid = new bplib.clsGenID();
                        genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "QualityProcessParameter", out _Id);

                        data["Id"] = _Id;
                        AddNewRow(dsMaster.Tables[0], data);
                    }
                    else
                    {
                        _Id = data["Id"].ToString();
                        EditRow(dsMaster.Tables[0].Rows[0], data);
                    }

                    string Id = dsMaster.Tables[0].Rows[0]["Id"].ToString();

                    #region NoticePeriodFormulaDetail 
                    DataRow drF;
                    while (dsDestination.Tables[0].DefaultView.Count > 0)
                        dsDestination.Tables[0].DefaultView[0].Delete();

                    int count = 0;
                    if (details != null)
                    {

                        foreach (var item in details)
                        {
                            drF = dsDestination.Tables[0].NewRow();
                            count++;
                            string pk = _Id + "_" + count;
                            drF["Id"] = pk;
                            drF["QualityProcessParameterId"] = _Id;
                            drF["Sequence"] = item.Sequence;
                            drF["QualityProcessParameterHeadId"] = item.QualityProcessParameterHeadId;
                            drF["Component"] = item.Component;

                            dsDestination.Tables[0].Rows.Add(drF);
                        }

                    }
                    #endregion NoticePeriodFormulaDetail 

                    clsStaticInfo obj = new clsStaticInfo();
                    obj.SaveDataSets(dsMaster, dsDestination);


                }
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
            //dr["UpdatedBy"] = identity.Name;
            //dr["UpdatedDate"] = System.DateTime.Now.ToString();
            //dr["UpdatedFromIP"] = identity.IPAddress;

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

        [HttpPost]
        public JsonResult Delete(string id)
        {
            DeleteData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteData(string SystemID)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM dbo.ProductionBookingProcessParameter WHERE Id = '" + SystemID + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

               // objCon.ExecuteNonQueryWrapper("Delete [dbo].FormulaDetail where CostingSOTemplateId= '" + SystemID + "'", true, "1");
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw exx;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function


        [HttpPost, Authorize]
        public JsonResult DeleteProductionBookingParameter(string id)
        {
            DeleteProductionBookingParameterData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteProductionBookingParameterData(string SystemID)
        {
            string strSQL, strFSQL, strFDSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM dbo.ProductionBookingParameter WHERE Id = '" + SystemID + "'";
                strFSQL = "DELETE FROM dbo.FormulaDetail WHERE ProductionBookingParameterHeadId = '" + SystemID + "'";
                strFDSQL = "DELETE FROM dbo.ProductionSummaryParameterValue WHERE ProductionBookingParameterId = '" + SystemID + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();

                objCon.ExecuteNonQueryWrapper(strFSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strFDSQL, true, "1");
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw exx;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function


        [HttpPost, Authorize]
        public JsonResult DeleteQualityProcess(string id)
        {
            DeleteQualityProcessData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteQualityProcessData(string SystemID)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM dbo.ProductionQualityProcess WHERE Id = '" + SystemID + "'";
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
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw exx;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function


        [HttpPost, Authorize]
        public JsonResult DeleteQualityProcessParameter(string id)
        {
            DeleteQualityProcessParameterData(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        public void DeleteQualityProcessParameterData(string SystemID)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM dbo.QualityProcessParameter WHERE Id = '" + SystemID + "'";
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
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw exx;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function


        #endregion


    }

    public class ProductionBookingParameter
    {
        public string Id { get; set; }
        public string FormulaDes { get; set; }
        public string FormulaDesID { get; set; }
        public string AddedBy { get; set; }
        public DateTime AddedDate { get; set; }
        public string AddedFromIP { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string UpdatedFromIP { get; set; }

    }

    public class ProductionBookingParameterFormulaDetail
    {
        public string Id { get; set; }
        public decimal Sequence { get; set; }
        public string Component { get; set; }
        public string ProductionBookingParameterId { get; set; }
        public string ProductionBookingParameterHeadId { get; set; }

    }

    public class QualityProcessParameterFormulaDetail
    {
        public string Id { get; set; }
        public decimal Sequence { get; set; }
        public string Component { get; set; }
        public string QualityProcessParameterId { get; set; }
        public string QualityProcessParameterHeadId { get; set; }

    }
}