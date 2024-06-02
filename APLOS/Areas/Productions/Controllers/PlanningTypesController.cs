#region Using
using Aplos.Controllers;
using Library.Model.Productions;
using Aplos.Properties;
using Library.Service.Productions;
using Library.Core;
using System.Web.Mvc;
using Library.Data.Sql;
using Library.Crosscutting.Security;
using System.Threading;
using System;
using System.Collections.Generic;
using Aplos.MaterialManagement.MaterialQuery;
using System.Data;
using Library.Security.Core;

#endregion

namespace Aplos.Areas.Productions.Controllers
{
    public class PlanningTypesController : BaseController
    {
        #region Constructor
        /// <summary>   The PlanningTypesService service. </summary>
        private readonly IPlanningTypesService _planningTypesService;
        private readonly ISqlRepository _sqlRepository;
        public PlanningTypesController(IPlanningTypesService planningTypesService, ISqlRepository R)
        {
            _planningTypesService = planningTypesService;
            _sqlRepository = R;
        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        #region -- Operations

        [Authorize, HttpGet]
        public JsonResult GetCbo()
        {
            return Json(_sqlRepository.GetDataCollection("SELECT PlanningType AS [Value], UserName AS [Text] FROM [dbo].[PlanningTypes]"), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult GetAllEntity(string CompanyId)
        {
            try
            {

                string sql = @"";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (identity.IsSysAdmin)
                {
                    sql = @"SELECT distinct E.Id,E.PlantId,P.UserName AS PlantName,e.Code,e.UserName AS UserName
                        FROM [ORG].[Entity] E
                            LEFT JOIN dbo.EntityConfig ECC ON ECC.EntityId=E.Id
                            LEFT JOIN org.Plant AS p ON p.Id=e.PlantId
                            LEFT JOIN org.Company AS c ON c.Id=e.CompanyId
                            WHERE ECC.IsProductionEntity=1 AND E.[Active]=1 AND e.CompanyId='" + CompanyId + @"'
                        ORDER BY e.Code";

                    return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
                }

                sql = @"SELECT  distinct E2.Id,e2.PlantId,P.UserName AS PlantName,e2.Code,e2.UserName AS UserName  FROM [SEC].[UserEntity] E
                        LEFT JOIN org.Entity AS e2 ON e2.Id=e.EntityId
                        LEFT JOIN dbo.EntityConfig ECC ON ECC.EntityId=E2.Id
                        LEFT JOIN org.Plant AS p ON p.Id=e.PlantId
                        LEFT JOIN org.Company AS c ON c.Id=e.CompanyId
                        WHERE E.UserId='" + identity.UserId + @"' AND ECC.IsProductionEntity=1 AND E2.[Active]=1 ORDER BY E2.Code";
                return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);


                throw new Exception("No entity configurations was found in the system for the current user");
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }

        }

        [HttpGet, Authorize]
        public ActionResult GetList(GridParameter parameters)
        {
            return Json(_planningTypesService.Query(parameters), JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult Create(PlanningTypes planningTypes)
        {
            _planningTypesService.Insert(planningTypes);
            return Json(new { PlanningTypes= planningTypes, Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(PlanningTypes planningTypes)
        {
            _planningTypesService.Update(planningTypes);
            return Json(new {Message = AplosMessage.Updated });
        }

        public ActionResult Delete(string id)
        {
            _planningTypesService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }



        [HttpGet, Authorize]
        public ActionResult GetPlanningTypeProductMasterDataList(string PlanningTypeId)
        {

            string sql = @"SELECT Flag=CAST(CASE WHEN A.Id IS NULL THEN 0 ELSE 1 END AS BIT) ,A.Id,PM.Id ProductMasterId,PM.Sequence,PM.Code,PM.UserName,PM.StandardName,PC.UserName ProductCategory,PSC.UserName ProductSubCategory,P.UserName Product
FROM [MST].ProductMaster PM
LEFT JOIN [HKP].ProductCategory PC ON PC.Id=PM.ProductCategoryId
LEFT JOIN [HKP].ProductSubCategory PSC ON PSC.Id=PM.ProductSubCategoryId
LEFT JOIN [HKP].Product P ON P.Id=PM.ProductId
OUTER APPLY (Select * from dbo.PlanningTypeProductMaster Where ProductMasterId=PM.Id AND PlanningTypeId='" + PlanningTypeId + @"') A
Where PM.Active=1 Order by PM.Sequence";
            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, Authorize]
        public JsonResult CreatePTPMMap(List<Dictionary<string, object>> data, string masterId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ConnectionManager.DAL.ConManager objCon;
                DataSet dsChild, dsChildId;
                MaterialCommonService materialCommonService = new MaterialCommonService(_sqlRepository);
                #region FUND 
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter("SELECT * FROM dbo.PlanningTypeProductMaster where  PlanningTypeId='" + masterId + "'", out dsChild, false, "1");

                objCon.OpenDataSetThroughAdapter("select count(Id) countId from [dbo].[PlanningTypeProductMaster] where PlanningTypeId='" + masterId + "'", out dsChildId, false, "1");

                var count = Convert.ToInt32(dsChildId.Tables[0].Rows[0]["countId"].ToString()); ;
                if (data != null)
                {
                    foreach (var item in data)
                    {
                        DataView dv = new DataView(dsChild.Tables[0]);
                        dv.RowFilter = "Id='" + item["Id"] + "'";

                        if (dv.Count == 0)
                        {

                            count++;
                            item["Id"] = materialCommonService.MakePK(masterId, count, 2);
                            item["PlanningTypeId"] = masterId;

                            AddNewRow(dsChild.Tables[0], item);
                        }
                        else if (dv.Count > 0 && Convert.ToBoolean(item["Flag"]) == false)
                        {
                            DataRow drmo = dv[0].Row;
                            drmo.Delete();
                        }
                        else
                        {
                            DataRow drmo = dv[0].Row;
                            EditRow(drmo, item);
                        }
                    }
                }

                #endregion

                clsStaticInfo obj = new clsStaticInfo();
                obj.SaveDataSets(dsChild);
                return Json(new { Error = false, Message = AplosMessage.Insert });
            }
            catch (Exception ex)
            {

                throw ex;
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
        #endregion
    }
}