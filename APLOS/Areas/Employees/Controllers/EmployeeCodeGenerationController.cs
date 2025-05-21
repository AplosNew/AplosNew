#region Using

using Aplos.Controllers;
using Aplos.HumanResource;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Payrolls;
using Library.Service.Enums;
using Library.Service.Payrolls;
using Library.Service.Properties;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading;
using System.Web.Mvc;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class EmployeeCodeGenerationController : BaseController
    {

        #region Constructor
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        EmployeeProfile employeeProfile = new EmployeeProfile();
        public EmployeeCodeGenerationController(IUnitOfWork U, ISqlRepository R)
        {
            _unitOfWork = U;
            _sqlRepository = R;
        }
        #endregion


        #region -- Pages
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region Operation

        [HttpPost]
        public ActionResult GetList(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false && string.IsNullOrEmpty(value) == false)
                strkey = column + " like '%" + value + "%'";

            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select top 100 * from (SELECT * FROM dbo.EmployeeCodeGenGroup) AS TEMP WHERE " + strkey + " order by sequence";
            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }

        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM dbo.EmployeeCodeGenGroup");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }

        [HttpGet, Authorize]
        public JsonResult GetAllEmployeeCodeGenerationPlantData(string masterId)
        {
            return Json(employeeProfile.GetAllEmployeeCodeGenerationPlantData(masterId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Dictionary<string, object> data, List<Dictionary<string, object>> detaildata)
        {
            try
            {
                DataSet dsMaster, dsDetail, dsIdDetail;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from dbo.EmployeeCodeGenGroup where Code='" + data["Code"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same Code already exists!!!");

                con.OpenDataSetThroughAdapter("select * from dbo.EmployeeCodeGenGroup where UserName='" + data["UserName"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                if (dsMaster.Tables[0].Rows.Count > 0)
                    throw new Exception("Same User Name already exists!!!");

                if (data.ContainsKey("Prefix"))
                {
                    var name = data["Prefix"]?.ToString();
                    if (!string.IsNullOrEmpty(name))
                    {
                        con.OpenDataSetThroughAdapter("select * from dbo.EmployeeCodeGenGroup where Prefix='" + data["Prefix"] + "' AND  Id<>'" + data["Id"] + "'", out dsMaster, false, "1");
                        if (dsMaster.Tables[0].Rows.Count > 0)
                            throw new Exception("Same Prefix already exists!!!");
                    }
                }


                con.OpenDataSetThroughAdapter("select * from dbo.EmployeeCodeGenGroup where Id='" + data["Id"] + "'", out dsMaster, false, "1");
                con.OpenDataSetThroughAdapter("select * from dbo.EmployeeCodeGenGroupDetail where EmployeeCodeGenGroupId='" + data["Id"] + "'", out dsDetail, false, "1");

             

                string _Id = "";

                #region EmployeeCodeGenGroup insert update
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    bplib.clsGenID genid = new bplib.clsGenID();
                    genid.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "EmployeeCodeGenGroup", out _Id);

                    data["Id"] = _Id;
                    AddNewRow(dsMaster.Tables[0], data);
                }
                else
                {
                    _Id = data["Id"].ToString();
                    
                    EditRow(dsMaster.Tables[0].Rows[0], data);
                }
                string mId = dsMaster.Tables[0].Rows[0]["Id"].ToString();
                #endregion data update

                int count = 0;
                con.OpenDataSetThroughAdapter("select COUNT(Id)Idcount from dbo.EmployeeCodeGenGroupDetail where EmployeeCodeGenGroupId='" + data["Id"] + "'", out dsIdDetail, false, "1");
                if (dsIdDetail.Tables[0].Rows.Count>0)
                {
                    count =Convert.ToInt32(dsIdDetail.Tables[0].Rows[0]["Idcount"].ToString());
                }

                if (detaildata!=null)
                {
                    foreach (var item in detaildata)
                    {
                       
                        DataView dv = new DataView(dsDetail.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {
                            count++;
                            item["Id"] = mId + count;
                            item["EmployeeCodeGenGroupId"] = mId;

                            AddNewRow(dsDetail.Tables[0], item);
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    } 
                }

                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster, dsDetail);

                return Json(new { Error = false, Data = data, Sequence = GetSequence(), Message = AplosMessage.Updated });

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });

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

        public ActionResult Delete(string id)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new Exception("Select entry first");

                ConnectionManager.clsConnection con = new ConnectionManager.clsConnection();
                con.BeginTransaction();
                con.executeQuery("delete from dbo.[EmployeeCodeGenGroupDetail] where EmployeeCodeGenGroupId='" + id + "'");
                con.executeQuery("delete from dbo.[EmployeeCodeGenGroup] where id='" + id + "'");
                con.CommitTransaction();

                return Json(new { Error = false, Sequence = GetSequence(), Message = AplosMessage.Deleted }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }


        }

        #endregion

    }
}