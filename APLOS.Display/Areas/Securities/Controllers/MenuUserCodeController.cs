#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Sql;
using Library.Model.Securites;
using Library.Service.Helpers;
using Library.Service.Organizations;
using Library.Service.Securites;
using Microsoft.ReportingServices.Diagnostics.Internal;
using OTSBD;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using System.Web.Script.Serialization;

#endregion Using

namespace Aplos.Areas.Securities.Controllers
{
    public class MenuUserCodeController : BaseController
    {
        #region Constructor

        SqlRepository _sqlRepository = null;

        public MenuUserCodeController()
        {
            _sqlRepository = new SqlRepository();
        }

        #endregion Constructor


        public ActionResult Aplos()
        {
            return View();
        }


        [HttpGet, AllowAnonymous]
        public ActionResult GetMenuDetailList(string ModuleId, string MenuFrameId)
        {
            try
            {

                var cmdText = @"SELECT [isSelect] = Convert(bit, 'True'), [isToBeSelect] = Convert(bit, 'False'),  MM.Description MenuName,MM.Id Id,
                               CASE WHEN ISNULL(cg.Id,'')<>'' THEN 'YES' ELSE 'NO' END AS IsCompanyGroupAssigned
                               ,CASE WHEN ISNULL(MM.TobeChecked,0)<>0 THEN 'YES' ELSE 'NO' END AS TobeChecked,MM.Specialdecision
                                      , MDL.Id ModuleId,'' MenuItemGroup, mM.Sequence, MNFR.Id MenuFrameId, MG.Id MenuGroupId
                                      , SMG.Id MenuSubGroupId,ISNULL(MM.UserDefineCode,'') UserDefineCode
                                      ,MM.Id MenuMasterId , MM.PanelName,MM.Description,mm.IsExternalMenu,MM.Remarks,MM.Controller,MM.Href ,MDL.UserName Module
                                      , SMDL.UserName SubModule, SMDL.Id SubModuleId , MNFR.UserName MenuFrame, MG.UserName MenuGroup , SMG.UserName SubMenuGroup
									  , MM.Remarks,MM.Href,MM.Controller,MM.PanelName,ISNULL(MarkForDeletion,0) MarkForDeletion ,MM.Image, MM.MenuHelpDocName, MM.MenuHelpDocInternalName
                                FROM MST.MenuMaster MM
                                INNER JOIN mst.CompanyGroupMenuMaster AS cg ON cg.ModuleId=mm.ModuleId AND cg.MenuMasterId=mm.Id
                                LEFT JOIN MMS.Module MDL ON MDL.Id = MM.ModuleId
                                LEFT JOIN MMS.SubModule SMDL ON MDL.Id = MM.SubModuleId
                             
                                LEFT JOIN MMS.MenuFrame MNFR ON MNFR.Id = MM.MenuFrameId
                                LEFT JOIN MMS.MenuGroup MG ON MG.Id = MM.MenuGroupId
                                LEFT JOIN MMS.MenuSubGroup SMG ON SMG.Id = MM.MenuSubGroupId
                                WHERE (1=1)";
                if (!string.IsNullOrEmpty(ModuleId) && ModuleId != "null")
                {
                    cmdText += "AND  MDL.Id = '" + ModuleId + @"'";
                }
                if (!string.IsNullOrEmpty(MenuFrameId) && MenuFrameId != "null")
                {
                    cmdText += "AND  MNFR.Id = '" + MenuFrameId + @"'";
                }
                cmdText += @"ORDER BY convert(bit,CASE WHEN ISNULL(cg.Id,'')<>'' THEN 1 ELSE 0 END) DESC, MDL.Sequence,SMDL.Sequence,MNFR.Sequence,MG.Sequence,SMG.Sequence, MM.Sequence";

                var jsondata = Json(_sqlRepository.GetDataCollection(cmdText), JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        [HttpPost, AllowAnonymous]
        public ActionResult SaveCompanyGroupMenuMaster(List<Dictionary<string, object>> CompanyGroupMenuMaster)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");

                System.Data.DataSet dsCompanyGroupMenuMaster = null;
                string menuIds = "''";
                for (int i = 0; i < CompanyGroupMenuMaster.Count; i++)
                {
                    if(!string.IsNullOrEmpty(CompanyGroupMenuMaster[i]["UserDefineCode"].ToString()))
                    {
                            menuIds += ",'" + CompanyGroupMenuMaster[i]["MenuMasterId"] + "'";

                    }
                }
                string sql = "SELECT * FROM [MST].[MenuMaster] where Id IN (" + menuIds + ") ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsCompanyGroupMenuMaster, false, "1");
                for (int i = 0; i < CompanyGroupMenuMaster.Count; i++)
                {
                    dsCompanyGroupMenuMaster.Tables[0].DefaultView.RowFilter = "Id='" + CompanyGroupMenuMaster[i]["MenuMasterId"] + "'";

                    if (dsCompanyGroupMenuMaster.Tables[0].DefaultView.Count > 0)
                    {
                        DataRow dr = dsCompanyGroupMenuMaster.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();
                        dr["UserDefineCode"] = CompanyGroupMenuMaster[i]["UserDefineCode"];
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit(); 
                    }
                }
                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsCompanyGroupMenuMaster);
                return Json(new { Message = "Menu Added Successfully", Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }
    }
}