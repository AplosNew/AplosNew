#region Using
using Aplos.Controllers;
using Library.Model.Employees;
using Aplos.Properties;
using Library.Service.Employees;
using Library.Core;
using System.Web.Mvc;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using System;
using System.Data;
using OTSBD;

#endregion

namespace Aplos.Areas.Employees.Controllers
{
    public class StoppageController : BaseController
    {
        #region Constructor
        private readonly ISqlRepository _sqlRepository;
        private readonly IStoppageService _stoppageService;
        public StoppageController(
              IStoppageService stoppageService,
              ISqlRepository sqlRepository
            )
        {
            _stoppageService = stoppageService;
            _sqlRepository = sqlRepository;
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
        public JsonResult GetCbo(string routeId)
        {
            return Json(_stoppageService.GetCbo(routeId).Rows, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetCompany()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"select Id,UserName from [ORG].[Company]";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpGet, Authorize]
        public ActionResult GetCity(string CompanyId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @"Select  C.Id, C.UserName
								 From SCS.City AS C
                            Left Outer Join MST.AddressMaster AS AM ON C.CountryId=AM.CountryId
                            Left Outer Join ORG.Company AS CO ON AM.Id=CO.AddressMasterId
                            Where CO.Id='"+CompanyId+"' order by C.UserName";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            string sql = @" select S.Id,S.Sequence,S.Code,S.ShortName,S.StandardName,S.UserName,S.Active,c.UserName as City,S.CompanyId,s.CityId
								,AC=CASE WHEN S.Active=1 THEN 'Yes' else 'No' end
								from HKP.Stoppage S
							left outer join SCS.City as c on s.CityId = c.Id 
	                    where s.CompanyGroupId='"+identity.CompanyGroupId+@"' and s.CompanyId='"+identity.CompanyId+@"' and S.PlantId='"+identity.PlantId+@"'
                          ORDER BY S.Sequence";
            var data = _sqlRepository.GetDataCollection(sql);
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetAutoSequence()
        {
            return Json(GetSequence(), JsonRequestBehavior.AllowGet);
        }
        private double GetSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM [HKP].[Stoppage]");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }
        [HttpGet, Authorize]
        public JsonResult GetCityByCompanyCbo(string companyId)
        {
            return Json(_stoppageService.GetCityByCompanyCbo(companyId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Save(StoppageModel stoppage)
        {
            try
            {
                SaveStopate(stoppage);
                
                return Json(new { Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }


        public void SaveStopate(StoppageModel stoppage)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            ConnectionManager.DAL.ConManager objCon;
            DataSet dsMaster;
            try
            {
                string sql = "SELECT * FROM HKP.Stoppage WHERE ID='" + stoppage.Id + "' ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMaster, false, "1");
                
                if (dsMaster.Tables[0].Rows.Count == 0)
                {
                    DataRow dr = dsMaster.Tables[0].NewRow();

                    string sID = string.Empty;
                    bplib.clsGenID objGenID = new bplib.clsGenID();
                    objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), "HKP.Stoppage", out sID);                    
                    dr["Id"] = "SP" + sID;
                    dr["CompanyId"] = stoppage.CompanyId;
                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["PlantId"] = identity.PlantId;
                    dr["CityId"] = stoppage.CityId;
                    dr["Sequence"] = stoppage.Sequence;
                    dr["Code"] = stoppage.Code;
                    dr["ShortName"] = stoppage.ShortName;
                    dr["StandardName"] = stoppage.StandardName;
                    dr["UserName"] = stoppage.UserName;
                    dr["Description"] = stoppage.Description;
                    dr["Remarks"] = stoppage.Remarks;
                    dr["Active"] = stoppage.Active;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = DateTime.Now;
                    dr["AddedFromIP"] = identity.IPAddress;
                    dsMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                    dr.BeginEdit();
                    dr["CompanyId"] = stoppage.CompanyId;
                    dr["CompanyGroupId"] = identity.CompanyGroupId;
                    dr["PlantId"] = identity.PlantId;
                    dr["CityId"] = stoppage.CityId;
                    dr["Sequence"] = stoppage.Sequence;
                    dr["Code"] = stoppage.Code;
                    dr["ShortName"] = stoppage.ShortName;
                    dr["StandardName"] = stoppage.StandardName;
                    dr["UserName"] = stoppage.UserName;
                    dr["Description"] = stoppage.Description;
                    dr["Remarks"] = stoppage.Remarks;
                    dr["Active"] = stoppage.Active;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;
                    dr.EndEdit();
                }                
                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsMaster);
            }

            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Delete(string Id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                //ConnectionManager.DAL.ConManager objCon;
                DataSet dsMaster;
                string sqlr = @"select * from mst.RouteStoppage where StoppageId = '" + Id + @"'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sqlr, out dsMaster, false, "1");

                if (dsMaster.Tables[0].Rows.Count > 0)
                {
                    throw new Exception("Already used in Route Schedule!!!");
                }

                strSQL = "DELETE FROM  HKP.Stoppage WHERE Id='" + Id + "'";
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
                    throw ex;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function
        public class StoppageModel : BaseModel
        {
            #region Scalar Properties            
            public string Id { get; set; }
            public string CompanyGroupId { get; set; }
            public string CompanyId { get; set; }            
            public decimal Sequence { get; set; }            
            public string Code { get; set; }           
            public string ShortName { get; set; }           
            public string StandardName { get; set; }           
            public string UserName { get; set; }            
            public string Description { get; set; }
            public string CityId { get; set; }            
            public string Remarks { get; set; }
            public string PlantId { get; set; }
            public bool Active { get; set; }    
            
            #endregion Scalar Properties

            #region Audit Properties
            [NeverUpdate]
            public string AddedBy { get; set; }
            
            [NeverUpdate]
            public DateTime? AddedDate { get; set; }
            [NeverUpdate]
            public string AddedFromIP { get; set; }
            public string UpdatedBy { get; set; }
            public DateTime? UpdatedDate { get; set; }
            public string UpdatedFromIP { get; set; }
            #endregion Audit Properties
        }
        #endregion
    }
}