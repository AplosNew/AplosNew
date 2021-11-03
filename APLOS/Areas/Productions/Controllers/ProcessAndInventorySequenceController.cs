#region Using
using Aplos.Controllers;
using Library.Model.Productions;
using Aplos.Properties;
using Library.Service.Productions;
using Library.Core;
using System.Web.Mvc;
using Library.Data.Sql;
using System;
using OTSBD;
using System.Data;
using Library.Crosscutting.Security;
using System.Threading;
using System.Collections.Generic;

#endregion

namespace Aplos.Areas.Productions.Controllers
{
    public class ProcessAndInventorySequenceController : BaseController
    {
        #region Constructor
        /// <summary>   The CostingTypesService service. </summary>
        SqlRepository _sqlRepository = null;
        ConnectionManager.clsConnectionManager ConManager = null;

        public ProcessAndInventorySequenceController()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();

        }
        #endregion

        #region -- Pages
        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

      
        [HttpPost, Authorize]
        public ActionResult GetProcessAndInventorySeq(string plantId)
        {

            try
            {

                string sql = ProcessAndInventorySeqSql(plantId);
                return Json(new { DATA = _sqlRepository.GetDataCollection(sql), Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        [HttpPost, Authorize]
        public ActionResult GetCompanyList()
        {

            try
            {

                string sql = CompanyListSql();
                return Json(new { DATA = _sqlRepository.GetDataCollection(sql), Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        [HttpPost, Authorize]
        public ActionResult Save(List<Dictionary<string,object>> data,string plantId)
        {

            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string savesql = @"select * from ProcessAndInventorySequence where PlantId =" + plantId + @"";
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(savesql, out DataSet dsLocal, false, "1");

                while (dsLocal.Tables[0].DefaultView.Count>0)
                {
                    dsLocal.Tables[0].DefaultView[0].Delete();
                }

                for (int i = 0; i < data.Count; i++)
                {

                    if (bplib.clsWebLib.GetBoolData((data[i]["Active"]))==true)
                    {                               
                        DataRow dr = dsLocal.Tables[0].NewRow();
                        dr["ProcessId"] = data[i]["ProcessId"];
                        dr["SFGInventoryId"] = data[i]["SFGInventoryId"];
                        dr["PlantId"] = plantId;
                        dr["Sequence"] = data[i]["Sequence"].ToString();
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = DateTime.Now;
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr["Active"] = data[i]["Active"];
                        dsLocal.Tables[0].Rows.Add(dr);

                    }
                    else
                    {

                    }
                        
                   

                }

                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsLocal);

                return Json(new { Error = false, Message = "Data Saved successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message });
            }

        }
        private string ProcessAndInventorySeqSql(string plantId)
        {
            
            return @" select * from (select P.Id as ProcessId,null as SFGInventoryId,P.UserName,isnull(s.Sequence, p.Sequence) AS [Sequence], convert(bit,isnull(S.Active,0)) AS Active  from hkp.Process as P 
                                left join ProcessAndInventorySequence S on s.ProcessId=P.Id and s.plantId='" + plantId + @"'
                                where P.Active=1 and IsProductionProcess=1 AND P.Id IN (SELECT ept.ProcessId FROM hkp.EntityProcessTag AS ept
                                INNER JOIN org.Entity AS e ON e.Id=ept.EntityId
                                WHERE e.PlantId='"+plantId+ @"')
                        union ALL 
                        select null as ProcessId, SFG.Id,Sfg.UserName,isnull(S.Sequence, sfg.Sequence) AS Sequence, convert(bit,isnull(S.Active,0)) AS Active from hkp.SFGInventory as SFG
                        left join ProcessAndInventorySequence S on s.SFGInventoryId=sfg.Id  and s.plantId='" + plantId + @"'
                        where Sfg.Active=1 and sfg.id in (
                        select es.SFGInventoryId from mst.EntitySFGInventory as es 
                        inner join org.Entity as en on en.Id=es.EntityId
                        where en.PlantId='" + plantId + @"')
                        ) AS T order by [Sequence]
              ";

        }
        private string CompanyListSql()
        {

            return @" 
                        select C.Id,c.UserName as CompanyName from ORG.Company as C where Active=1
              ";

        }

    }

}