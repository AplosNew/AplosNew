using Aplos.Controllers;
using Aplos.Properties;
using ClientDataExchange;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Menus;
using Library.Service.Menus;
using OTSBD;
using Syncfusion.Pdf.Parsing;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web.Mvc;
using WebApi;

namespace Aplos.Areas.Menus.Controllers
{
    public class MenuSyncController : BaseController
    {
        public readonly string GetMenuListAPI = "MenuSync/MenuSync";
        private readonly ISqlRepository _sqlRepository;
        private readonly IMenuService _menuService;
        private readonly IMenuActionService _menuActionService;
        public MenuSyncController(MenuService menuService, IMenuActionService menuActionService, ISqlRepository sqlRepository)
        {
            _sqlRepository = sqlRepository;
            _menuService = menuService;
            _menuActionService = menuActionService;
        }


        public ActionResult Aplos()
        {
            return View("~/Areas/Menus/Views/MenuSync.cshtml");
        }
        [Authorize]
        public ActionResult GeActionListByMenu(string menuId)
        {
            return Json(_menuActionService.GeActionListByMenu(menuId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost, AllowAnonymous]
        public ActionResult Create(Menu entity, IEnumerable<MenuAction> actionList)
        {
            _menuService.InsertGraph(entity, actionList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost, AllowAnonymous]
        public ActionResult Edit(Menu entity, IEnumerable<MenuAction> actionList)
        {
            _menuService.UpdateGraph(entity, actionList);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost, AllowAnonymous]
        public ActionResult Delete(string id)
        {

            _menuService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost, AllowAnonymous]
        public ActionResult DeleteMenuAction(string id)
        {
            ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
            connection.BeginTransaction();
            connection.executeQuery(@"DELETE FROM SEC.RoleDetail where MenuActionId = '" + id + "'");
            connection.executeQuery(@"DELETE FROM SEC.UserAccessDetail where MenuActionId = '" + id + "'");
            connection.executeQuery(@"DELETE FROM MMS.MenuAction where Id = '" + id + "'");
            connection.CommitTransaction();
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpGet, AllowAnonymous]
        public ActionResult Create()
        {
            return View(new MenuAction { Active = true });
        }
        [HttpPost, AllowAnonymous]
        public ActionResult GetMenuListForSync()
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string table_module = "MMS.Module";
                string table_SubModule = "MMS.SubModule";
                string table_Menu = "MMS.Menu";
                string table_MenuAction = "MMS.MenuAction";
                string table_MenuDetail = "MMS.MenuDetail";
                string table_MenuFrame = "MMS.MenuFrame";
                string table_MenuGroup = "MMS.MenuGroup";
                string table_MenuItem = "MMS.MenuItem";
                string table_MenuSubGroup = "MMS.MenuSubGroup";
                string table_MenuMaster = "MST.MenuMaster";
                string table_CompanyGroupMenuMaster = "MST.CompanyGroupMenuMaster";

                #region Module
                List<Dictionary<string, object>> data_module = getDataFromAPI(table_module);
                EditAddDataToDBTable(data_module, table_module);
                #endregion
                #region SubModule
                List<Dictionary<string, object>> data_SubModule = getDataFromAPI(table_SubModule);
                EditAddDataToDBTable(data_SubModule, table_SubModule);
                #endregion
                //#region Menu
                //List<Dictionary<string, object>> data_Menu = getDataFromAPI(table_Menu);
                //EditAddDataToDBTable(data_Menu, table_Menu);
                //#endregion

                //#region MenuDetail
                //List<Dictionary<string, object>> data_MenuDetail = getDataFromAPI(table_MenuDetail);
                //EditAddDataToDBTable(data_MenuDetail, table_MenuDetail);
                //#endregion
                #region MenuFrame
                List<Dictionary<string, object>> data_MenuFrame = getDataFromAPI(table_MenuFrame);
                EditAddDataToDBTable(data_MenuFrame, table_MenuFrame);
                #endregion
                #region MenuGroup
                List<Dictionary<string, object>> data_MenuGroup = getDataFromAPI(table_MenuGroup);
                EditAddDataToDBTable(data_MenuGroup, table_MenuGroup);
                #endregion
                //#region MenuItem
                //List<Dictionary<string, object>> data_MenuItem = getDataFromAPI(table_MenuItem);
                //EditAddDataToDBTable(data_MenuItem, table_MenuItem);
                //#endregion
                #region MenuSubGroup
                List<Dictionary<string, object>> data_MenuSubGroup = getDataFromAPI(table_MenuSubGroup);
                EditAddDataToDBTable(data_MenuSubGroup, table_MenuSubGroup);
                #endregion
                #region MenuMaster
                List<Dictionary<string, object>> data_MenuMaster = getDataFromAPI(table_MenuMaster);

                if (data_MenuMaster.Count == 0 || data_MenuMaster == null)
                    throw new Exception("Connection Terminated, Please try again later.");
                #region Determining Mark for Deletion
                DataSet dsMenuMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + table_MenuMaster + " ", out dsMenuMaster, false, "1");
                StringCollection strCol = new StringCollection();
                if (data_MenuMaster != null && data_MenuMaster.Count >0)
                {
                    for (int i = 0; i < data_MenuMaster.Count; i++)
                    {
                        strCol.Add(data_MenuMaster[i]["Id"].ToString());
                    }

                    for (int i = 0; i < dsMenuMaster.Tables[0].Rows.Count; i++)
                    {
                        try
                        {
                            dsMenuMaster.Tables[0].Rows[i]["MarkForDeletion"] = false;
                            if (strCol.Contains(dsMenuMaster.Tables[0].Rows[i]["Id"].ToString()) == false)
                            {
                                dsMenuMaster.Tables[0].Rows[i]["MarkForDeletion"] = true;
                            }
                            
                        }
                        catch (Exception ex)
                        {

                            throw ex;
                        }
                    }
                    clsStaticInfo conS = new clsStaticInfo();
                    conS.SaveDataSets(dsMenuMaster);
                    #endregion

                    EditAddDataToDBTable(data_MenuMaster, table_MenuMaster);
                    
                }
                #endregion

                #region MenuAction
                List<Dictionary<string, object>> data_MenuAction = getDataFromAPI(table_MenuAction);
                if(data_MenuAction.Count > 0 && data_MenuAction != null)
                {
                    #region Determining Mark for Deletion
                    DataSet dsMenuAction;
                    ConnectionManager.DAL.ConManager conn = new ConnectionManager.DAL.ConManager("1");
                    conn.OpenDataSetThroughAdapter("select * from " + table_MenuAction + " ", out dsMenuAction, false, "1");
                    StringCollection strColMenuAction = new StringCollection();

                    for (int i = 0; i < data_MenuAction.Count; i++)
                    {
                        strColMenuAction.Add(data_MenuAction[i]["Id"].ToString());
                    }

                    string toBeDeletedMenuActionId = "''";

                    for (int i = 0; i < dsMenuAction.Tables[0].Rows.Count; i++)
                    {
                        try
                        {
                            if (strColMenuAction.Contains(dsMenuAction.Tables[0].Rows[i]["Id"].ToString()) == false)
                            {
                                toBeDeletedMenuActionId += ",'" + dsMenuAction.Tables[0].Rows[i]["Id"].ToString() + "'";

                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }

                    ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                    connection.BeginTransaction();
                    connection.executeQuery(@"DELETE FROM SEC.RoleDetail where MenuActionId IN (" + toBeDeletedMenuActionId + ")");
                    connection.executeQuery(@"DELETE FROM SEC.UserAccessDetail where MenuActionId IN (" + toBeDeletedMenuActionId + ")");
                    connection.executeQuery(@"DELETE FROM MMS.MenuAction where Id IN (" + toBeDeletedMenuActionId + ")");
                    connection.CommitTransaction();
                    //DataSet dsRoleDetail, dsUserAccesDetail = null;
                    //ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();

                    //connection.getDataSet("Select * FROM SEC.RoleDetail where MenuActionId IN (" + toBeDeletedMenuActionId + ")", out dsRoleDetail);
                    //connection.getDataSet("Select * FROM SEC.UserAccessDetail where MenuActionId IN (" + toBeDeletedMenuActionId + ")", out dsUserAccesDetail);

                    //while (dsRoleDetail.Tables[0].DefaultView.Count > 0)
                    //{
                    //    dsRoleDetail.Tables[0].DefaultView[0].Delete();
                    //}
                    //while (dsUserAccesDetail.Tables[0].DefaultView.Count > 0)
                    //{
                    //    dsUserAccesDetail.Tables[0].DefaultView[0].Delete();
                    //}

                    //clsStaticInfo conSMA = new clsStaticInfo();
                    //conSMA.SaveDataSets(dsRoleDetail, dsUserAccesDetail, dsMenuAction);
                    #endregion
                    EditAddDataToDBTable(data_MenuAction, table_MenuAction);

                }
                #endregion





                return Json(new { Error = false, Message = "Menu sync successful" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = true, Message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private List<Dictionary<string, object>> getDataFromAPI(string tableName)
        {
            try
            {
                DataTable dtAplAuth = new DataTable();
                string strClntURL = @"SELECT [Id],[URL] ,[AddedBy] ,[AddedDate],[AddedFromIP],[UpdatedBy]  ,[UpdatedDate] ,[UpdatedFromIP]
                                FROM[dbo].[AplosAuthentication]";

                dtAplAuth = _sqlRepository.GetDataTable(strClntURL);

                string clientUrl = dtAplAuth.Rows[0]["URL"].ToString();

                string MainAPI = clientUrl + GetMenuListAPI;

                clsAPIData APIData = new clsAPIData();

                clsWebApi webApi = new clsWebApi(MainAPI + "?tableName=" + tableName);

                List<Dictionary<string, object>> data = webApi.GetMessage<Dictionary<string, object>>("");

                return data;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public void EditAddDataToDBTable(List<Dictionary<string, object>> data, string tableName)
        {
            try
            {
                DataSet dsMaster;
                ConnectionManager.DAL.ConManager con = new ConnectionManager.DAL.ConManager("1");
                con.OpenDataSetThroughAdapter("select * from " + tableName + " ", out dsMaster, false, "1");



                for (int i = 0; i < data.Count; i++)
                {
                    dsMaster.Tables[0].DefaultView.RowFilter = "Id='" + data[i]["Id"].ToString() + "'";
                    string _Id = "";

                    #region data update
                    if (dsMaster.Tables[0].DefaultView.Count == 0)
                    {
                        AddNewRow(dsMaster.Tables[0], data[i]);
                        foreach (var item in data[i].Keys)
                        {
                            if (item.ToUpper() == "UserCode".ToUpper())
                            {
                                dsMaster.Tables[0].Rows[dsMaster.Tables[0].Rows.Count - 1]["Code"] = data[i]["UserCode"];
                            }
                        }
                    }
                    else
                    {
                        DataRow dr = dsMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();

                        foreach (var item in data[i].Keys)
                        {
                            try
                            {
                                if (item.ToUpper() == "UserCode".ToUpper())
                                {
                                    if (dr["Code"].ToString() == dr["UserCode"].ToString())
                                    {
                                        dr["UserCode"] = data[i]["UserCode"];
                                    }
                                    dr["Code"] = data[i]["UserCode"];
                                }
                                else
                                {
                                    if (dr.Table.Columns[item].DataType == typeof(byte[]))
                                        dr[item] = Convert.FromBase64String(data[i][item].ToString());
                                    else
                                        dr[item] = data[i][item];

                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                        dr.EndEdit();



                    }
                    #endregion data update
                }
                clsStaticInfo _info = new clsStaticInfo();
                _info.SaveDataSets(dsMaster);



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
                    if (dr.Table.Columns[item].DataType == typeof(byte[]))
                        dr[item] = Convert.FromBase64String(sourceData[item].ToString());
                    else
                        dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
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
                    if (dr.Table.Columns[item].DataType == typeof(byte[]))
                        dr[item] = Convert.FromBase64String(sourceData[item].ToString());
                    else
                        dr[item] = sourceData[item];
                }
                catch (Exception)
                {
                }
            }
            dr.EndEdit();
        }

        private void EditRowCGMMaster(DataRow dr, Dictionary<string, object> sourceData)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            dr.BeginEdit();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    if (dr[item].ToString().ToUpper() == "COMPANYGROUPID" || dr[item].ToString().ToUpper() == "MENUMASTERID")
                        continue;
                    dr["ModuleId"] = sourceData["ModuleId"];
                }
                catch (Exception)
                {
                }
            }
            dr.EndEdit();
        }
        private void AddNewRowCGMMaster(DataTable dt, Dictionary<string, object> sourceData, string companyGroupId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            DataRow dr = dt.NewRow();

            foreach (var item in sourceData.Keys)
            {
                try
                {
                    dr["ModuleId"] = sourceData["ModuleId"];
                    dr["MenuMasterId"] = sourceData["MenuMasterId"];
                    dr["CompanyGroupId"] = sourceData["CompanyGroupId"];


                    // dr[item] = sourceData[item];
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            dt.Rows.Add(dr);
        }
        [HttpGet, Authorize]
        public JsonResult GetAutoMenuSequence()
        {
            return Json(GetMenuSequence(), JsonRequestBehavior.AllowGet);
        }
        private double GetMenuSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM [MST].[MenuMaster]");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }
        [HttpGet, AllowAnonymous]
        public ActionResult GetMenuDetailList(string ModuleId, string MenuFrameId)
        {
            try
            {

                var cmdText = @"SELECT [isSelect] = Convert(bit, 'True'), [isToBeSelect] = Convert(bit, 'False'),  MM.Description MenuName,MM.Id Id,
                               CASE WHEN ISNULL(cg.Id,'')<>'' THEN 'YES' ELSE 'NO' END AS IsCompanyGroupAssigned
                               , CASE WHEN ISNULL(MM.TobeChecked,0)<>0 THEN 'YES' ELSE 'NO' END AS TobeChecked,MM.Specialdecision
                               , CASE WHEN ISNULL(MarkForDeletion,0)<>0 THEN 'YES' ELSE 'NO' END AS MarkForDeletionC
                                      , MDL.Id ModuleId,'' MenuItemGroup, mM.Sequence, MNFR.Id MenuFrameId, MG.Id MenuGroupId
                                      , SMG.Id MenuSubGroupId
                                      , MM.Id MenuMasterId , MM.PanelName,MM.Description,mm.IsExternalMenu,MM.Remarks,MM.Controller,MM.Href ,MDL.UserName Module
                                      , SMDL.UserName SubModule, SMDL.Id SubModuleId , MNFR.UserName MenuFrame, MG.UserName MenuGroup , SMG.UserName SubMenuGroup
									  , MM.Remarks,MM.Href,MM.Controller,MM.PanelName,ISNULL(MarkForDeletion,0) MarkForDeletion ,MM.Image, MM.MenuHelpDocName, MM.MenuHelpDocInternalName
                                      , CASE WHEN ISNULL(CG.MenuMasterId,'') ='' THEN 'No' ELSE 'Yes' END Active
                                FROM MST.MenuMaster MM
                                LEFT JOIN mst.CompanyGroupMenuMaster AS cg ON cg.ModuleId=mm.ModuleId AND cg.MenuMasterId=mm.Id
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

        [HttpGet, AllowAnonymous]
        public ActionResult GetAplosCoreUrl()
        {
            try
            {
                DataTable dtAplAuth = new DataTable();
                string strClntURL = @"SELECT [Id],[URL] ,[AddedBy] ,[AddedDate],[AddedFromIP],[UpdatedBy]  ,[UpdatedDate] ,[UpdatedFromIP]
                                FROM[dbo].[AplosAuthentication]";

                dtAplAuth = _sqlRepository.GetDataTable(strClntURL);

                string clientUrl = "Aplos central repository url is missing";
                if (dtAplAuth.Rows.Count > 0)
                    clientUrl = dtAplAuth.Rows[0]["URL"].ToString();


                var jsondata = Json(clientUrl, JsonRequestBehavior.AllowGet);
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

                #region SaveCompanyGroupMenuMaster

                DataTable dtCompanyGroup = _sqlRepository.GetDataTable("SELECT * FROM ORG.CompanyGroup");

                string companyGroupId = dtCompanyGroup.Rows[0]["Id"].ToString();

                string sql = "";
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");

                DataSet dsCompanyGroupMenuMaster;
                //if (CompanyGroupMenuMaster == null)
                //    CompanyGroupMenuMaster = new List<Dictionary<string, object>>();

                string menuIds = "''";
                for (int i = 0; i < CompanyGroupMenuMaster.Count; i++)
                {
                    menuIds += ",'" + CompanyGroupMenuMaster[i]["MenuMasterId"] + "'";
                }

                sql = "SELECT * FROM [MST].[CompanyGroupMenuMaster] where MenuMasterId IN (" + menuIds + ") ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsCompanyGroupMenuMaster, false, "1");
                //while (dsCompanyGroup.Tables[0].DefaultView.Count > 0)
                //    dsCompanyGroup.Tables[0].DefaultView[0].Delete();

                for (int i = 0; i < CompanyGroupMenuMaster.Count; i++)
                {
                    dsCompanyGroupMenuMaster.Tables[0].DefaultView.RowFilter = "MenuMasterId='" + CompanyGroupMenuMaster[i]["MenuMasterId"] + "'";
                    if (dsCompanyGroupMenuMaster.Tables[0].DefaultView.Count == 0)
                    {
                        string _id = "";
                        bplib.clsGenID id = new bplib.clsGenID();
                        id.GenID("CGM", out _id);
                        _id = "CGM" + _id;

                        DataRow dr = dsCompanyGroupMenuMaster.Tables[0].NewRow();
                        dr["Id"] = _id;
                        dr["companyGroupId"] = companyGroupId;
                        dr["ModuleId"] = CompanyGroupMenuMaster[i]["ModuleId"];
                        dr["MenuMasterId"] = CompanyGroupMenuMaster[i]["MenuMasterId"];
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dsCompanyGroupMenuMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dsCompanyGroupMenuMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["companyGroupId"] = companyGroupId;
                        dr["ModuleId"] = CompanyGroupMenuMaster[i]["ModuleId"];
                        dr["MenuMasterId"] = CompanyGroupMenuMaster[i]["MenuMasterId"];
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();
                    }
                }
                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsCompanyGroupMenuMaster);
                #endregion
                return Json(new { Message = "Menu Added Successfully", Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }


        [HttpPost, AllowAnonymous]
        public ActionResult UpdateCompanyGroupMenuMasterFromTree(List<Dictionary<string, object>> CompanyGroupMenuMaster, string PanelName)
        {
            try
            {

                if (string.IsNullOrEmpty(PanelName))
                    throw new Exception("Please select panel");
                if (CompanyGroupMenuMaster == null)
                    throw new Exception("Please select at lease one menu item");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                #region SaveCompanyGroupMenuMaster

                DataTable dtCompanyGroup = _sqlRepository.GetDataTable("select * from org.CompanyGroup");

                string companyGroupId = dtCompanyGroup.Rows[0]["Id"].ToString();

                string sql = "";
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");

                DataSet dsCompanyGroupMenuMaster;
                //if (CompanyGroupMenuMaster == null)


                sql = @"SELECT * FROM [MST].[CompanyGroupMenuMaster] where CompanyGroupId='" + companyGroupId + "' " +
                    " and MenuMasterId in (select Id from MST.MenuMaster where PanelName='" + PanelName + "')";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsCompanyGroupMenuMaster, false, "1");


                string menuIds = "''";
                for (int i = 0; i < CompanyGroupMenuMaster.Count; i++)
                {
                    menuIds += ",'" + CompanyGroupMenuMaster[i]["MenuMasterId"] + "'";
                }

                sql = "SELECT Id,ModuleId FROM [MST].[MenuMaster] where Id IN (" + menuIds + ")";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsMenuMaster, false, "1");

                //while (dsCompanyGroupMenuMaster.Tables[0].DefaultView.Count > 0)
                //    dsCompanyGroupMenuMaster.Tables[0].DefaultView[0].Delete();
                for (int i = 0; i < dsCompanyGroupMenuMaster.Tables[0].Rows.Count; i++)
                {
                    var k = CompanyGroupMenuMaster.Where(x => clsStaticInfo.nullrecorder(x["MenuMasterId"]) == dsCompanyGroupMenuMaster.Tables[0].Rows[i]["MenuMasterId"].ToString()).FirstOrDefault();
                    if (k == null)
                        dsCompanyGroupMenuMaster.Tables[0].Rows[i].Delete();
                }

                string _id = "";
                for (int i = 0; i < CompanyGroupMenuMaster.Count; i++)
                {
                    dsMenuMaster.Tables[0].DefaultView.RowFilter = "Id='" + CompanyGroupMenuMaster[i]["MenuMasterId"] + "'";
                    dsCompanyGroupMenuMaster.Tables[0].DefaultView.RowFilter = "MenuMasterId='" + CompanyGroupMenuMaster[i]["MenuMasterId"] + "'";
                    if (dsCompanyGroupMenuMaster.Tables[0].DefaultView.Count == 0)
                    {
                        if (_id == "")
                        {
                            bplib.clsGenID id = new bplib.clsGenID();
                            id.GenID("CGM", out _id);
                            _id = "CGM" + _id;
                        }
                        DataRow dr = dsCompanyGroupMenuMaster.Tables[0].NewRow();
                        dr["Id"] = _id + "-" + (i + 1);
                        dr["companyGroupId"] = companyGroupId;
                        dr["ModuleId"] = dsMenuMaster.Tables[0].DefaultView[0]["ModuleId"];
                        dr["MenuMasterId"] = dsMenuMaster.Tables[0].DefaultView[0]["Id"];
                        dr["AddedBy"] = identity.Name;
                        dr["AddedDate"] = System.DateTime.Now.ToString();
                        dr["AddedFromIP"] = identity.IPAddress;
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dsCompanyGroupMenuMaster.Tables[0].Rows.Add(dr);
                    }
                    else
                    {

                        DataRow dr = dsCompanyGroupMenuMaster.Tables[0].DefaultView[0].Row;
                        dr.BeginEdit();
                        dr["companyGroupId"] = companyGroupId; 
                        dr["ModuleId"] = dsMenuMaster.Tables[0].DefaultView[0]["ModuleId"];
                        dr["MenuMasterId"] = dsMenuMaster.Tables[0].DefaultView[0]["Id"];
                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr.EndEdit();
                    }

                }
                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsCompanyGroupMenuMaster);
                #endregion
                return Json(new { Message = "Menu assigned to company group Successfully", Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }


        #region Menu Edit
        [HttpPost, AllowAnonymous]
        public JsonResult SaveMenu(menuCombined menuMaster, List<menuCompanyGroup> companygroup, List<MenuAction> menuAction)
        {
            try
            {

                if (string.IsNullOrEmpty(menuMaster.PanelName) == true)
                    throw new Exception("Enter PanelName");

                if (string.IsNullOrEmpty(menuMaster.ModuleId) == true)
                    throw new Exception("Enter module");

                if (string.IsNullOrEmpty(menuMaster.MenuFrameId) == true)
                    throw new Exception("Enter Menu Frame");

                if (string.IsNullOrEmpty(menuMaster.Description) == true)
                    throw new Exception("Enter description");

                if (string.IsNullOrEmpty(menuMaster.Href) == true)
                    throw new Exception("Enter Href");


                if (string.IsNullOrEmpty(menuMaster.UserCode) == true)
                    throw new Exception("Enter User Code");
                if (string.IsNullOrEmpty(menuMaster.Controller) == true)
                    throw new Exception("Enter Controller");
                if (string.IsNullOrWhiteSpace(menuMaster.Controller) == true)
                    throw new Exception("Please Remove Space from Controller");
                if (string.IsNullOrWhiteSpace(menuMaster.Href) == true)
                    throw new Exception("Please Remove Space from Href");



                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                DataSet dsMenu, dsMenuItem, dsMenuMaster;

                //first check for href or menu name

                string sql = "SELECT * FROM [MMS].[Menu] WHERE Id<>'" + menuMaster.Id + "' AND (UserName='" + menuMaster.Description + "' OR Href='" + menuMaster.Href + "')";
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMenu, false, "1");
                if (dsMenu.Tables[0].Rows.Count > 0)
                    throw new Exception("Same menu already exists!!!");


                sql = "SELECT * FROM [MMS].[Menu] WHERE id='" + menuMaster.Id + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMenu, false, "1");
                //if (dsMenu.Tables[0].Rows.Count > 0)
                //    throw new Exception("Same menu has already exists in the system");


                string MenuID = "";
                if (dsMenu.Tables[0].Rows.Count == 0)
                {
                    //         IFCodes   
                    bplib.clsGenID id = new bplib.clsGenID();
                    id.GenID("MENU", out MenuID);
                    MenuID = "MNU" + MenuID;

                    DataRow dr = dsMenu.Tables[0].NewRow();

                    dr["Id"] = MenuID;
                    dr["Area"] = "Aplos";
                    dr["Sequence"] = menuMaster.Sequence;
                    dr["UserName"] = menuMaster.Description;
                    dr["Controller"] = menuMaster.Controller;
                    dr["Href"] = menuMaster.Href; ;
                    dr["Description"] = menuMaster.Description;

                    dr["Active"] = true;

                    dsMenu.Tables[0].Rows.Add(dr);
                }
                else
                {

                    DataRow dr = dsMenu.Tables[0].Rows[0];
                    dr.BeginEdit();
                    MenuID = dr["Id"].ToString();
                    dr["UserName"] = menuMaster.Description;
                    dr["Sequence"] = menuMaster.Sequence;
                    dr["Controller"] = menuMaster.Controller;
                    dr["Href"] = menuMaster.Href; ;
                    dr["Description"] = menuMaster.Description;
                    dr["Active"] = true;

                    dr.EndEdit();
                }

                sql = "SELECT * FROM [MMS].[MenuItem] where MenuID='" + MenuID + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMenuItem, false, "1");
                string MenuItemID = "";
                if (dsMenuItem.Tables[0].Rows.Count == 0)
                {

                    bplib.clsGenID id = new bplib.clsGenID();
                    id.GenID("MENU ITEM", out MenuItemID);
                    MenuItemID = "MI" + MenuItemID;

                    DataRow dr = dsMenuItem.Tables[0].NewRow();
                    dr["Id"] = MenuItemID;

                    dr["MenuId"] = MenuID;
                    dr["MenuItemGroup"] = menuMaster.Description;
                    dr["Sequence"] = "0";
                    dr["Code"] = menuMaster.Code;
                    dr["UserName"] = menuMaster.Description;

                    dr["UserCode"] = menuMaster.UserCode;
                    dr["InterfaceNo"] = menuMaster.Code;
                    dr["StandardName"] = menuMaster.Description;
                    dr["MaximumUser"] = "0";
                    dr["MaximumInactiveTime"] = "0";


                    dr["Active"] = true;
                    dr["Archive"] = false;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsMenuItem.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsMenuItem.Tables[0].Rows[0];
                    MenuItemID = dr["Id"].ToString();

                    dr.BeginEdit();
                    dr["MenuId"] = MenuID;
                    dr["MenuItemGroup"] = menuMaster.Description;
                    //dr["Sequence"] = "0";
                    //dr["Code"] = menuMaster.Code;
                    dr["UserName"] = menuMaster.Description;

                    dr["UserCode"] = menuMaster.UserCode;

                    //dr["InterfaceNo"] = menuMaster.Code;
                    //dr["StandardName"] = menuMaster.Description;
                    //dr["MaximumUser"] = "0";
                    //dr["MaximumInactiveTime"] = "0";


                    dr["Active"] = true;
                    dr["Archive"] = false;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }



                sql = "SELECT * FROM [MST].[MenuMaster] where MenuID='" + MenuID + "' and panelName='" + menuMaster.PanelName + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMenuMaster, false, "1");

                string MenuMasterID = "";
                if (dsMenuMaster.Tables[0].Rows.Count == 0)
                {
                    string _id = "";
                    bplib.clsGenID id = new bplib.clsGenID();
                    id.GenID("MENU MASTER", out _id);
                    _id = "MM" + _id;
                    MenuMasterID = _id;
                    DataRow dr = dsMenuMaster.Tables[0].NewRow();
                    dr["Id"] = _id;

                    dr["MenuId"] = MenuID;
                    dr["MenuItemId"] = MenuItemID;

                    dr["ModuleId"] = menuMaster.ModuleId;
                    dr["MenuFrameId"] = menuMaster.MenuFrameId;
                    dr["MenuGroupId"] = menuMaster.MenuGroupId;
                    dr["MenuSubGroupId"] = menuMaster.MenuSubGroupId;
                    dr["PanelName"] = menuMaster.PanelName;
                    dr["Description"] = menuMaster.Description;
                    dr["Remarks"] = menuMaster.Remarks;

                    dr["IsExternalMenu"] = false;

                    dr["Active"] = true;
                    dr["Archive"] = false;
                    dr["AddedBy"] = identity.Name;
                    dr["AddedDate"] = System.DateTime.Now.ToString();
                    dr["AddedFromIP"] = identity.IPAddress;
                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dsMenuMaster.Tables[0].Rows.Add(dr);
                }
                else
                {
                    DataRow dr = dsMenuMaster.Tables[0].Rows[0];


                    dr.BeginEdit();

                    MenuMasterID = dr["id"].ToString();
                    dr["MenuId"] = MenuID;
                    dr["MenuItemId"] = MenuItemID;

                    dr["ModuleId"] = menuMaster.ModuleId;
                    dr["MenuFrameId"] = menuMaster.MenuFrameId;
                    dr["MenuGroupId"] = menuMaster.MenuGroupId;
                    dr["MenuSubGroupId"] = menuMaster.MenuSubGroupId;
                    dr["PanelName"] = menuMaster.PanelName;
                    dr["Description"] = menuMaster.Description;
                    dr["Remarks"] = menuMaster.Remarks;

                    dr["IsExternalMenu"] = false;

                    dr["Active"] = true;
                    dr["Archive"] = false;

                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }


                DataSet dsCompanyGroup;
                if (companygroup == null)
                    companygroup = new List<menuCompanyGroup>();


                //string groupids = "''";
                //for (int i = 0; i < companygroup.Count; i++)
                //{
                //    groupids += ",'" + companygroup[i].CompanyGroupId + "'";
                //}

                //sql = "SELECT * FROM [MST].[CompanyGroupMenuMaster] where CompanyGroupId IN (" + groupids + ") AND ModuleId='" + menuMaster.ModuleId + "' AND MenuMasterId='" + MenuMasterID + "'";
                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(sql, out dsCompanyGroup, false, "1");
                ////while (dsCompanyGroup.Tables[0].DefaultView.Count > 0)
                ////    dsCompanyGroup.Tables[0].DefaultView[0].Delete();

                //for (int i = 0; i < companygroup.Count; i++)
                //{
                //    dsCompanyGroup.Tables[0].DefaultView.RowFilter = "CompanyGroupId='" + companygroup[i].CompanyGroupId + "'";
                //    if (dsCompanyGroup.Tables[0].DefaultView.Count == 0)
                //    {
                //        //if (companygroup[i].IsSaved == false)
                //        //    continue;

                //        string _id = "";
                //        bplib.clsGenID id = new bplib.clsGenID();
                //        id.GenID("MENU ASSIGN", out _id);
                //        _id = "CGM" + _id;

                //        DataRow dr = dsCompanyGroup.Tables[0].NewRow();
                //        dr["Id"] = _id;

                //        dr["ModuleId"] = menuMaster.ModuleId;
                //        dr["MenuMasterId"] = MenuMasterID;

                //        dr["CompanyGroupId"] = companygroup[i].CompanyGroupId;


                //        dr["Active"] = companygroup[i].IsSaved;
                //        dr["Archive"] = false;

                //        dr["AddedBy"] = identity.Name;
                //        dr["AddedDate"] = System.DateTime.Now.ToString();
                //        dr["AddedFromIP"] = identity.IPAddress;
                //        dr["UpdatedBy"] = identity.Name;
                //        dr["UpdatedDate"] = System.DateTime.Now.ToString();
                //        dr["UpdatedFromIP"] = identity.IPAddress;

                //        dsCompanyGroup.Tables[0].Rows.Add(dr);
                //    }
                //    else
                //    {
                //        DataRow dr = dsCompanyGroup.Tables[0].DefaultView[0].Row;
                //        if (companygroup[i].IsSaved == false)
                //            dr.Delete();
                //        //dr.BeginEdit();

                //        //dr["ModuleId"] = menuMaster.ModuleId;
                //        //dr["MenuMasterId"] = MenuMasterID;

                //        //dr["CompanyGroupId"] = companygroup[i].CompanyGroupId;


                //        //dr["Active"] = companygroup[i].IsSaved;
                //        //dr["Archive"] = false;

                //        //dr["UpdatedBy"] = identity.Name;
                //        //dr["UpdatedDate"] = System.DateTime.Now.ToString();
                //        //dr["UpdatedFromIP"] = identity.IPAddress;

                //        //dr.EndEdit();

                //    }
                //}
                #region MenuAction
                DataSet dsMenuAction;
                if (menuAction == null)
                    menuAction = new List<MenuAction>();


                string actionIds = "''";
                for (int i = 0; i < menuAction.Count; i++)
                {
                    actionIds += ",'" + menuAction[i].Id + "'";
                }

                sql = "SELECT * FROM MMS.MenuAction where Id IN (" + actionIds + ") ";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMenuAction, false, "1");
                //while (dsCompanyGroup.Tables[0].DefaultView.Count > 0)
                //    dsCompanyGroup.Tables[0].DefaultView[0].Delete();

                for (int i = 0; i < menuAction.Count; i++)
                {
                    if (string.IsNullOrEmpty(menuAction[i].Action) == true)
                        throw new Exception("Please Fill Up Action OR Delete the Empty Row.");
                    if (string.IsNullOrEmpty(menuAction[i].UserName) == true)
                        throw new Exception("Please Fill Up Display Name.");
                    if (string.IsNullOrWhiteSpace(menuAction[i].Action) == true)
                        throw new Exception("Please Remove Space from Action");

                    dsMenuAction.Tables[0].DefaultView.RowFilter = "Id='" + menuAction[i].Id + "'";
                    if (dsMenuAction.Tables[0].DefaultView.Count == 0)
                    {
                        //if (companygroup[i].IsSaved == false)
                        //    continue;

                        string _id = "";
                        bplib.clsGenID id = new bplib.clsGenID();
                        id.GenID("MENU ACTION", out _id);
                        _id = "MAC" + _id;

                        DataRow dr = dsMenuAction.Tables[0].NewRow();
                        dr["Id"] = _id;

                        dr["MenuId"] = MenuID;
                        dr["Action"] = menuAction[i].Action;

                        dr["UserName"] = menuAction[i].UserName;
                        dr["Description"] = menuAction[i].Description;

                        dsMenuAction.Tables[0].Rows.Add(dr);
                    }
                    else
                    {
                        DataRow dr = dsMenuAction.Tables[0].DefaultView[0].Row;

                        dr.BeginEdit();

                        dr["MenuId"] = MenuID;
                        dr["Action"] = menuAction[i].Action;

                        dr["UserName"] = menuAction[i].UserName;
                        dr["Description"] = menuAction[i].Description;

                        dr.EndEdit();

                    }
                }
                #endregion

                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsMenu, dsMenuItem, dsMenuMaster, dsMenuAction);

                if (string.IsNullOrEmpty(menuMaster.Id))
                    return Json(new { Message = "Menu Created Successfully", Error = false }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { Message = "Menu Updated Successfully", Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }

            return Json(null, JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetMenuInfoDoc(string menuId)
        {
            try
            {
                DataSet dsMenu = null;
                string sql = "SELECT * FROM MMS.MENU WHERE Id='" + menuId + @"'";
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMenu, false, "1");

                //DataTable dtMenu = _sqlRepository.GetDataTable("SELECT * FROM MMS.MENU WHERE Id='" + menuId + @"'");

                //byte[] sPDFDecoded = Convert.FromBase64String(dsMenu.Tables[0].Rows[0]["MenuHelpDoc"].ToString());

                byte[] sPDFDecoded = (byte[])dsMenu.Tables[0].Rows[0]["MenuHelpDoc"];

                string fileName = "";
                fileName = dsMenu.Tables[0].Rows[0]["MenuHelpDocInternalName"].ToString();

                string fullPathPDF = Path.Combine(System.Web.Hosting.HostingEnvironment.MapPath("~/") + fileName);
                System.IO.File.WriteAllBytes(fullPathPDF, sPDFDecoded);
                PdfLoadedDocument loadedDocument = new PdfLoadedDocument(fullPathPDF);
                loadedDocument.Save(fileName, HttpContext.ApplicationInstance.Response, Syncfusion.Pdf.HttpReadType.Save);
                return null;


                //return Json(new { FileName = fileName, Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = false }, JsonRequestBehavior.AllowGet);
            }
        }




        [HttpPost, AllowAnonymous]
        public JsonResult DeleteMenu(string MenuId, string MenuMasterId)
        {
            try
            {
                ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                connection.BeginTransaction();
                connection.executeQuery(@"DELETE FROM UserFavoriteMenu where MenuMasterId = '" + MenuId + "'");
                connection.executeQuery(@"DELETE FROM SEC.RoleDetail where MenuMasterId = '" + MenuMasterId + "'");
                connection.executeQuery(@"DELETE FROM SEC.UserAccessDetail where MenuMasterId = '" + MenuMasterId + "'");
                connection.executeQuery(@"DELETE FROM MMS.MenuAction where MenuMasterId = '" + MenuId + "'");
                connection.executeQuery(@"DELETE FROM MST.CompanyGroupMenuMaster where MenuMasterId = '" + MenuMasterId + "'");
                connection.executeQuery(@"DELETE FROM MST.MenuMaster where Id = '" + MenuId + "'");

                connection.CommitTransaction();

                return Json(new { Message = "Menu Deleted Successfully", Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }

        [HttpPost, AllowAnonymous]
        public JsonResult UncheckMenu( string MenuMasterId)
        {
            try
            {
                ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                connection.BeginTransaction();

                connection.executeQuery(@"DELETE FROM MST.CompanyGroupMenuMaster WHERE MenuMasterId = '" + MenuMasterId + "'");

                connection.CommitTransaction();

                return Json(new { Message = "Menu Deleted Successfully", Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }
        #endregion

    }
}