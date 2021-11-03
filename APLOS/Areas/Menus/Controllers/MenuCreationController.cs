using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Model.Menus;
using Library.Service.Employees;
using Library.Service.Menus;
using OTSBD;
using SourceCodeMenuCollection;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace Aplos.Areas.Menus.Controllers
{
    public class MenuCreationController : BaseController
    {
        #region Constructor

        private readonly IMenuService _menuService;
        private readonly IMenuActionService _menuActionService;
        private readonly ISqlRepository _sqlRepository;
        public MenuCreationController(MenuService menuService, IMenuActionService menuActionService, ISqlRepository R)
        {
            _sqlRepository = R;
            _menuService = menuService;
            _menuActionService = menuActionService;


        }

        #endregion Constructor

        public ActionResult Index()
        {
            return View();
        }

        #region GetMenuList

        [Authorize]
        public JsonResult GetMenuList()
        {
            return Json(new SelectList(_menuService.GetMenuList(), "Value", "Text"), JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult GetAllMenuList(GridParameter parameters)
        {
            return Json(_menuService.Query(parameters), JsonRequestBehavior.AllowGet);
        }

        #endregion GetMenuList

        #region -- Operations

        [HttpPost, Authorize]
        public JsonResult GeActionListByMenu(string menuId, List<Dictionary<string, object>> SourceCodeMenuActions)
        {
            List<Dictionary<string, object>> MenuActions = (List<Dictionary<string, object>>)_menuActionService.GeActionListByMenu(menuId);
            if (SourceCodeMenuActions == null)
            {
                for (int i = 0; i < MenuActions.Count; i++)
                {
                    MenuActions[i]["Archive"] = true;
                }
            }
            else
            {

                for (int i = 0; i < MenuActions.Count; i++)
                {
                    var k = SourceCodeMenuActions.Where(p => p["ActionName"].ToString().ToLower() == MenuActions[i]["Action"].ToString().ToLower()).FirstOrDefault();
                    if (k != null)
                    {
                        SourceCodeMenuActions.Remove(k);
                    }
                    else
                    {
                        MenuActions[i]["Archive"] = true;
                    }
                }

                for (int i = 0; i < SourceCodeMenuActions.Count; i++)
                {
                    Dictionary<string, object> data = new Dictionary<string, object>();
                    data["Id"] = null;
                    data["MenuId"] = menuId;
                    data["Action"] = SourceCodeMenuActions[i]["ActionName"];
                    data["UserName"] = SourceCodeMenuActions[i]["ActionName"];
                    data["Description"] = "";
                    data["Active"] = true;
                    data["Archive"] = false;

                    MenuActions.Add(data);
                }
            }
            return Json(MenuActions, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Create(Menu entity, IEnumerable<MenuAction> actionList)
        {
            _menuService.InsertGraph(entity, actionList);
            return Json(new { Message = AplosMessage.Insert });
        }

        [HttpPost]
        public JsonResult Edit(Menu entity, IEnumerable<MenuAction> actionList)
        {
            _menuService.UpdateGraph(entity, actionList);
            return Json(new { Message = AplosMessage.Updated });
        }

        [HttpPost]
        public ActionResult Delete(string id)
        {
            _menuService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        [HttpPost]
        public ActionResult DeleteMenuAction(string id)
        {
            _menuActionService.Delete(id);
            return Json(new { Message = AplosMessage.Deleted });
        }

        #endregion -- Operations

        [HttpGet]
        public JsonResult GeModuleList()
        {

            string sql = "SELECT * FROM [MMS].[Module]";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult GetCompanyGroup(string MenuMasterId)
        {

            string sql = @"SELECT distinct cg.Id AS CompanyGroupId,cg.UserName,CONVERT(BIT, CASE WHEN isnull(m.Id,'')='' THEN 0 ELSE 1 END) AS IsSaved
                              FROM org.CompanyGroup AS cg
                            LEFT OUTER JOIN (       
                            SELECT M.* FROM [MST].[CompanyGroupMenuMaster] M 
                            INNER JOIN mst.MenuMaster AS mm ON mm.Id=m.MenuMasterId
                            where mm.Id='" + MenuMasterId + "') M ON cg.Id=m.CompanyGroupId ";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }
        public enum HierarchyPrefix { MODULE, FRAME, GROUP, SUBGROUP }
        [HttpGet, Authorize]
        public JsonResult GeMenuHierarchyOldWithMenu(string panelname)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                //                string sql = @"SELECT distinct 'FRAME-'+F.id AS id,F.UserName AS MenuText FROM [MMS].[MenuFrame] F
                //inner join mst.MenuMaster AS mm on mm.MenuFrameId=f.id where mm.PanelName='" + panelname + "'";

                string sql = @"SELECT distinct F.Sequence, '" + HierarchyPrefix.MODULE.ToString() + @"-'+F.id AS id,
                                CONCAT(F.UserName,'(',CASE WHEN ISNULL(md.Id,'')='' THEN 'Not Assigned' ELSE 'Assigned' END ,')') AS MenuText FROM [MMS].Module F
                                inner join mst.MenuMaster AS mm on mm.ModuleId=f.id 
                                LEFT JOIN [MMS].[CompanyGroupModule] MD ON md.moduleId=f.Id
                        where mm.PanelName='" + panelname + "'";
                var _dataM = _sqlRepository.GetDataCollection(sql);


                //         sql = @"    Select * from ( SELECT distinct mo.sequence P,f.Sequence C, '" + HierarchyPrefix.FRAME.ToString() + @"-'+F.id AS id,'" + HierarchyPrefix.MODULE.ToString() + @"-'+ MO.Id AS pid , F.UserName AS MenuText FROM [MMS].[MenuFrame] F
                //                     inner join mst.MenuMaster AS mm on mm.MenuFrameId=f.id 
                //                     inner join  [MMS].Module mo on mo.id=mm.moduleid where mm.PanelName='" + panelname + @"'
                //                     UNION ALL
                //                     SELECT distinct M.Sequence P,MG.Sequence C, '" + HierarchyPrefix.GROUP.ToString() + @"-'+mg.id AS id,'" + HierarchyPrefix.FRAME.ToString() + @"-'+m.Id AS pid,mg.UserName AS MenuText FROM mms.MenuFrame M
                //                     CROSS JOIN [MMS].[MenuGroup] MG
                //inner join mst.MenuMaster AS mm on mm.MenuFrameId=m.id and mm.MenuGroupId=mg.id and mm.PanelName='" + panelname + @"'
                //                     UNION ALL
                //                     SELECT M.Sequence P,MG.Sequence C, '" + HierarchyPrefix.SUBGROUP.ToString() + @"-'+mg.id AS id,'" + HierarchyPrefix.GROUP.ToString() + @"-'+m.Id AS pid,mg.UserName AS MenuText FROM mms.[MenuGroup] M
                //                     CROSS JOIN [MMS].[MenuSubGroup] MG
                //inner join mst.MenuMaster AS mm on mm.MenuGroupId=m.id and mm.MenuSubGroupId=mg.id and mm.PanelName='" + panelname + @"'
                //                     UNION ALL
                //                     SELECT F.Sequence P,m.Sequence C, mm.MenuId AS id,'" + HierarchyPrefix.FRAME.ToString() + @"-'+mm.MenuFrameId AS pid,m.UserName AS MenuText
                //                       FROM mst.MenuMaster AS mm 
                //                       INNER JOIN mms.Menu AS m ON m.Id=mm.MenuId
                //                      Inner join MMS.MenuFrame F ON F.Id = mm.MenuFrameId
                //                     WHERE isnull(mm.MenuFrameId,'')<>'' AND isnull(mm.MenuGroupId,'')='' AND ISNULL(mm.MenuSubGroupId,'')='' and mm.PanelName='" + panelname + @"'
                //                     UNION ALL
                //                     SELECT MG.Sequence P,m.Sequence C, mm.MenuId AS id,'" + HierarchyPrefix.GROUP.ToString() + @"-'+mm.MenuGroupId AS pid,m.UserName AS MenuText
                //                       FROM mst.MenuMaster AS mm 
                //                       INNER JOIN mms.Menu AS m ON m.Id=mm.MenuId
                //                    INNER JOIN MMS.MenuGroup as MG ON MG.Id = mm.MenuGroupId
                //                     WHERE isnull(mm.MenuFrameId,'')<>'' AND isnull(mm.MenuGroupId,'')<>'' AND ISNULL(mm.MenuSubGroupId,'')='' and mm.PanelName='" + panelname + @"'
                //                     UNION ALL
                //                     SELECT SMG.Sequence P,m.Sequence C, mm.MenuId AS id,'" + HierarchyPrefix.SUBGROUP.ToString() + @"-'+mm.MenuSubGroupId AS pid,m.UserName AS MenuText
                //                       FROM mst.MenuMaster AS mm 
                //                       INNER JOIN mms.Menu AS m ON m.Id=mm.MenuId
                //                    INNER JOIN MMS.MenuSubGroup as SMG ON SMG.Id = mm.MenuSubGroupId

                //             WHERE isnull(mm.MenuFrameId,'')<>'' AND isnull(mm.MenuGroupId,'')<>'' AND ISNULL(mm.MenuSubGroupId,'')<>'' and mm.PanelName='" + panelname + @"') MEnuH order by p ,c";
                //         
                sql = @" Select * from (SELECT DISTINCT 'CONTAINER' AS NODETYPE,'' AS MenuMasterId,  mo.sequence P,f.Sequence C, CONCAT('MODULE-',mm.ModuleId,'FRAME-',MM.MenuFrameId) AS id, CONCAT('MODULE-',mm.ModuleId) AS pid 
                            ,F.UserName AS MenuText,mm.ModuleId,mm.MenuFrameId,NULL AS MenuGroupId,NULL AS MenuSubGroupId
                             FROM [MMS].[MenuFrame] F
                            inner join mst.MenuMaster AS mm on mm.MenuFrameId=f.id 
                            inner join  [MMS].Module mo on mo.id=mm.moduleid where mm.PanelName='" + panelname + @"'
                            UNION

                            SELECT distinct 'CONTAINER' AS NODETYPE,'' AS MenuMasterId,F.Sequence P,MG.Sequence C, CONCAT('MODULE-',mm.ModuleId,'FRAME-',MM.MenuFrameId,'GROUP-',MM.MenuGroupId) AS id, CONCAT('MODULE-',mm.ModuleId,'FRAME-',MM.MenuFrameId) AS pid 
                            ,MG.UserName AS MenuText,mm.ModuleId,mm.MenuFrameId,mm.MenuGroupId,NULL AS MenuSubGroupId FROM [MMS].[MenuFrame] F
                            inner join mst.MenuMaster AS mm on mm.MenuFrameId=f.id 
                            INNER JOIN [MMS].[MenuGroup] MG ON mg.Id=mm.MenuGroupId
                            inner join  [MMS].Module mo on mo.id=mm.moduleid where mm.PanelName='" + panelname + @"'
                            UNION
                            SELECT distinct 'CONTAINER' AS NODETYPE,'' AS MenuMasterId,MG.Sequence P,MSG.Sequence C, CONCAT('MODULE-',mm.ModuleId,'FRAME-',MM.MenuFrameId,'GROUP-',MM.MenuGroupId,'SUBGROUP-',MM.MenuSubGroupId) AS id, CONCAT('MODULE-',mm.ModuleId,'FRAME-',MM.MenuFrameId,'GROUP-',MM.MenuGroupId) AS pid 
                            ,MSG.UserName AS MenuText, mm.ModuleId,mm.MenuFrameId,mm.MenuGroupId,mm.MenuSubGroupId  FROM [MMS].[MenuFrame] F
                            inner join mst.MenuMaster AS mm on mm.MenuFrameId=f.id 
                            INNER JOIN [MMS].[MenuGroup] MG ON mg.Id=mm.MenuGroupId
                            INNER JOIN [MMS].[MenuSubGroup] MSG ON msg.Id=mm.MenuSubGroupId
                            inner join  [MMS].Module mo on mo.id=mm.moduleid where mm.PanelName='" + panelname + @"'

                            UNION ALL
                            SELECT 'MENU' AS NODETYPE,mm.Id AS MenuMasterId,F.Sequence P,m.Sequence C, mm.MenuId AS id,CONCAT('MODULE-',mm.ModuleId,'FRAME-',MM.MenuFrameId) AS pid,m.UserName AS MenuText
                            ,mm.ModuleId,mm.MenuFrameId,mm.MenuGroupId,mm.MenuSubGroupId FROM mst.MenuMaster AS mm 
                            INNER JOIN mms.Menu AS m ON m.Id=mm.MenuId
                            Inner join MMS.MenuFrame F ON F.Id = mm.MenuFrameId
                            WHERE isnull(mm.MenuFrameId,'')<>'' AND isnull(mm.MenuGroupId,'')='' AND ISNULL(mm.MenuSubGroupId,'')='' and mm.PanelName='" + panelname + @"'
                            UNION ALL
                            SELECT 'MENU' AS NODETYPE,mm.Id AS MenuMasterId,MG.Sequence P,m.Sequence C, mm.MenuId AS id,CONCAT('MODULE-',mm.ModuleId,'FRAME-',MM.MenuFrameId,'GROUP-',MM.MenuGroupId) AS pid,m.UserName AS MenuText
                            ,mm.ModuleId,mm.MenuFrameId,mm.MenuGroupId,mm.MenuSubGroupId FROM mst.MenuMaster AS mm 
                            INNER JOIN mms.Menu AS m ON m.Id=mm.MenuId
                            INNER JOIN MMS.MenuGroup as MG ON MG.Id = mm.MenuGroupId
                            WHERE isnull(mm.MenuFrameId,'')<>'' AND isnull(mm.MenuGroupId,'')<>'' AND ISNULL(mm.MenuSubGroupId,'')='' and mm.PanelName='" + panelname + @"'
                            UNION ALL
                            SELECT 'MENU' AS NODETYPE,mm.Id AS MenuMasterId,SMG.Sequence P,m.Sequence C, mm.MenuId AS id,CONCAT('MODULE-',mm.ModuleId,'FRAME-',MM.MenuFrameId,'GROUP-',MM.MenuGroupId,'SUBGROUP-',MM.MenuSubGroupId) AS pid,m.UserName AS MenuText
                            ,mm.ModuleId,mm.MenuFrameId,mm.MenuGroupId,mm.MenuSubGroupId FROM mst.MenuMaster AS mm 
                            INNER JOIN mms.Menu AS m ON m.Id=mm.MenuId
                            INNER JOIN MMS.MenuSubGroup as SMG ON SMG.Id = mm.MenuSubGroupId
                            WHERE isnull(mm.MenuFrameId,'')<>'' AND isnull(mm.MenuGroupId,'')<>'' AND ISNULL(mm.MenuSubGroupId,'')<>'' and mm.PanelName='" + panelname + @"'
                            ) MEnuH order by p ,c";
                var _dataC = _sqlRepository.GetDataCollection(sql);

                return Json(new { MASTER = _dataM, DATA = _dataC, Error = false, }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpGet]
        public JsonResult GeMenuHierarchy(string panelname)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                //                string sql = @"SELECT distinct 'FRAME-'+F.id AS id,F.UserName AS MenuText FROM [MMS].[MenuFrame] F
                //inner join mst.MenuMaster AS mm on mm.MenuFrameId=f.id where mm.PanelName='" + panelname + "'";

                string sql = @"SELECT distinct F.Sequence, '" + HierarchyPrefix.MODULE.ToString() + @"-'+F.id AS id,
                                CONCAT(F.UserName,'(',CASE WHEN ISNULL(md.Id,'')='' THEN 'Not Assigned' ELSE 'Assigned' END ,')') AS MenuText FROM [MMS].Module F
                                inner join mst.MenuMaster AS mm on mm.ModuleId=f.id 
                                LEFT JOIN [MMS].[CompanyGroupModule] MD ON md.moduleId=f.Id
                        where mm.PanelName='" + panelname + "'";
                var _dataM = _sqlRepository.GetDataCollection(sql);


                //         sql = @"    Select * from ( SELECT distinct mo.sequence P,f.Sequence C, '" + HierarchyPrefix.FRAME.ToString() + @"-'+F.id AS id,'" + HierarchyPrefix.MODULE.ToString() + @"-'+ MO.Id AS pid , F.UserName AS MenuText FROM [MMS].[MenuFrame] F
                //                     inner join mst.MenuMaster AS mm on mm.MenuFrameId=f.id 
                //                     inner join  [MMS].Module mo on mo.id=mm.moduleid where mm.PanelName='" + panelname + @"'
                //                     UNION ALL
                //                     SELECT distinct M.Sequence P,MG.Sequence C, '" + HierarchyPrefix.GROUP.ToString() + @"-'+mg.id AS id,'" + HierarchyPrefix.FRAME.ToString() + @"-'+m.Id AS pid,mg.UserName AS MenuText FROM mms.MenuFrame M
                //                     CROSS JOIN [MMS].[MenuGroup] MG
                //inner join mst.MenuMaster AS mm on mm.MenuFrameId=m.id and mm.MenuGroupId=mg.id and mm.PanelName='" + panelname + @"'
                //                     UNION ALL
                //                     SELECT M.Sequence P,MG.Sequence C, '" + HierarchyPrefix.SUBGROUP.ToString() + @"-'+mg.id AS id,'" + HierarchyPrefix.GROUP.ToString() + @"-'+m.Id AS pid,mg.UserName AS MenuText FROM mms.[MenuGroup] M
                //                     CROSS JOIN [MMS].[MenuSubGroup] MG
                //inner join mst.MenuMaster AS mm on mm.MenuGroupId=m.id and mm.MenuSubGroupId=mg.id and mm.PanelName='" + panelname + @"'
                //                     UNION ALL
                //                     SELECT F.Sequence P,m.Sequence C, mm.MenuId AS id,'" + HierarchyPrefix.FRAME.ToString() + @"-'+mm.MenuFrameId AS pid,m.UserName AS MenuText
                //                       FROM mst.MenuMaster AS mm 
                //                       INNER JOIN mms.Menu AS m ON m.Id=mm.MenuId
                //                      Inner join MMS.MenuFrame F ON F.Id = mm.MenuFrameId
                //                     WHERE isnull(mm.MenuFrameId,'')<>'' AND isnull(mm.MenuGroupId,'')='' AND ISNULL(mm.MenuSubGroupId,'')='' and mm.PanelName='" + panelname + @"'
                //                     UNION ALL
                //                     SELECT MG.Sequence P,m.Sequence C, mm.MenuId AS id,'" + HierarchyPrefix.GROUP.ToString() + @"-'+mm.MenuGroupId AS pid,m.UserName AS MenuText
                //                       FROM mst.MenuMaster AS mm 
                //                       INNER JOIN mms.Menu AS m ON m.Id=mm.MenuId
                //                    INNER JOIN MMS.MenuGroup as MG ON MG.Id = mm.MenuGroupId
                //                     WHERE isnull(mm.MenuFrameId,'')<>'' AND isnull(mm.MenuGroupId,'')<>'' AND ISNULL(mm.MenuSubGroupId,'')='' and mm.PanelName='" + panelname + @"'
                //                     UNION ALL
                //                     SELECT SMG.Sequence P,m.Sequence C, mm.MenuId AS id,'" + HierarchyPrefix.SUBGROUP.ToString() + @"-'+mm.MenuSubGroupId AS pid,m.UserName AS MenuText
                //                       FROM mst.MenuMaster AS mm 
                //                       INNER JOIN mms.Menu AS m ON m.Id=mm.MenuId
                //                    INNER JOIN MMS.MenuSubGroup as SMG ON SMG.Id = mm.MenuSubGroupId

                //             WHERE isnull(mm.MenuFrameId,'')<>'' AND isnull(mm.MenuGroupId,'')<>'' AND ISNULL(mm.MenuSubGroupId,'')<>'' and mm.PanelName='" + panelname + @"') MEnuH order by p ,c";

                sql = @" Select *,
CONVERT(BIT, CASE WHEN ISNULL(c.Id,'')<>'' THEN 1 ELSE 0 END) AS isChecked from (SELECT DISTINCT 'CONTAINER' AS NODETYPE,  mo.sequence P,f.Sequence C, CONCAT('MODULE-',mm.ModuleId,'FRAME-',MM.MenuFrameId) AS id, CONCAT('MODULE-',mm.ModuleId) AS pid 
                            ,F.UserName AS MenuText,mm.ModuleId,mm.MenuFrameId,NULL AS MenuGroupId,NULL AS MenuSubGroupId
                             FROM [MMS].[MenuFrame] F
                            inner join mst.MenuMaster AS mm on mm.MenuFrameId=f.id 
                            inner join  [MMS].Module mo on mo.id=mm.moduleid where mm.PanelName='" + panelname + @"'
                            UNION

                            SELECT distinct 'CONTAINER' AS NODETYPE,F.Sequence P,MG.Sequence C, CONCAT('MODULE-',mm.ModuleId,'FRAME-',MM.MenuFrameId,'GROUP-',MM.MenuGroupId) AS id, CONCAT('MODULE-',mm.ModuleId,'FRAME-',MM.MenuFrameId) AS pid 
                            ,MG.UserName AS MenuText,mm.ModuleId,mm.MenuFrameId,mm.MenuGroupId,NULL AS MenuSubGroupId FROM [MMS].[MenuFrame] F
                            inner join mst.MenuMaster AS mm on mm.MenuFrameId=f.id 
                            INNER JOIN [MMS].[MenuGroup] MG ON mg.Id=mm.MenuGroupId
                            inner join  [MMS].Module mo on mo.id=mm.moduleid where mm.PanelName='" + panelname + @"'
                            UNION
                            SELECT distinct 'CONTAINER' AS NODETYPE,MG.Sequence P,MSG.Sequence C, CONCAT('MODULE-',mm.ModuleId,'FRAME-',MM.MenuFrameId,'GROUP-',MM.MenuGroupId,'SUBGROUP-',MM.MenuSubGroupId) AS id, CONCAT('MODULE-',mm.ModuleId,'FRAME-',MM.MenuFrameId,'GROUP-',MM.MenuGroupId) AS pid 
                            ,MSG.UserName AS MenuText, mm.ModuleId,mm.MenuFrameId,mm.MenuGroupId,mm.MenuSubGroupId  FROM [MMS].[MenuFrame] F
                            inner join mst.MenuMaster AS mm on mm.MenuFrameId=f.id 
                            INNER JOIN [MMS].[MenuGroup] MG ON mg.Id=mm.MenuGroupId
                            INNER JOIN [MMS].[MenuSubGroup] MSG ON msg.Id=mm.MenuSubGroupId
                            inner join  [MMS].Module mo on mo.id=mm.moduleid where mm.PanelName='" + panelname + @"'

                            UNION ALL
                            SELECT 'MENU' AS NODETYPE,F.Sequence P,mm.Sequence C, mm.Id AS id,CONCAT('MODULE-',mm.ModuleId,'FRAME-',MM.MenuFrameId) AS pid,mm.[Description] AS MenuText
                            ,mm.ModuleId,mm.MenuFrameId,mm.MenuGroupId,mm.MenuSubGroupId FROM mst.MenuMaster AS mm 
                            --INNER JOIN mms.Menu AS m ON m.Id=mm.MenuId
                            Inner join MMS.MenuFrame F ON F.Id = mm.MenuFrameId
                            WHERE isnull(mm.MenuFrameId,'')<>'' AND isnull(mm.MenuGroupId,'')='' AND ISNULL(mm.MenuSubGroupId,'')='' and mm.PanelName='" + panelname + @"'
                            UNION ALL
                            SELECT 'MENU' AS NODETYPE,MG.Sequence P,mm.Sequence C, mm.Id AS id,CONCAT('MODULE-',mm.ModuleId,'FRAME-',MM.MenuFrameId,'GROUP-',MM.MenuGroupId) AS pid,mm.[Description] AS MenuText
                            ,mm.ModuleId,mm.MenuFrameId,mm.MenuGroupId,mm.MenuSubGroupId FROM mst.MenuMaster AS mm 
                           -- INNER JOIN mms.Menu AS m ON m.Id=mm.MenuId
                            INNER JOIN MMS.MenuGroup as MG ON MG.Id = mm.MenuGroupId
                            WHERE isnull(mm.MenuFrameId,'')<>'' AND isnull(mm.MenuGroupId,'')<>'' AND ISNULL(mm.MenuSubGroupId,'')='' and mm.PanelName='" + panelname + @"'
                            UNION ALL
                            SELECT 'MENU' AS NODETYPE,SMG.Sequence P,mm.Sequence C, mm.Id AS id,CONCAT('MODULE-',mm.ModuleId,'FRAME-',MM.MenuFrameId,'GROUP-',MM.MenuGroupId,'SUBGROUP-',MM.MenuSubGroupId) AS pid,mm.[Description] AS MenuText
                            ,mm.ModuleId,mm.MenuFrameId,mm.MenuGroupId,mm.MenuSubGroupId FROM mst.MenuMaster AS mm 
                            --INNER JOIN mms.Menu AS m ON m.Id=mm.MenuId
                            INNER JOIN MMS.MenuSubGroup as SMG ON SMG.Id = mm.MenuSubGroupId
                            WHERE isnull(mm.MenuFrameId,'')<>'' AND isnull(mm.MenuGroupId,'')<>'' AND ISNULL(mm.MenuSubGroupId,'')<>'' and mm.PanelName='" + panelname + @"'
                        ) MEnuH 
                     LEFT JOIN mst.CompanyGroupMenuMaster AS c ON c.ModuleId=menuh.ModuleId AND c.MenuMasterId=menuh.id
                     order by p ,c";
                var _dataC = _sqlRepository.GetDataCollection(sql);

                return Json(new { MASTER = _dataM, DATA = _dataC, Error = false, }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {

                return Json(new { Error = true, Message = ex.Message });
            }
        }
        [HttpGet]
        public JsonResult GetMenu(string id, string panelname)
        {

            string sql = @"
                            SELECT Null AS SourceCodeMenu,'' AS MatchingComment, m.Id,MM.Id AS MenuMasterId,m.Sequence,mm.ModuleId, mm.MenuFrameId, mm.MenuGroupId, mm.MenuSubGroupId,mm.IsExternalMenu,mm.Remarks,
                                   mm.PanelName,m.Controller,m.Href, m.UserName [Description],mi.Code,m.[Active],mi.MenuItemGroup
                            FROM [MST].[MenuMaster] MM
                            LEFT OUTER JOIN mms.Menu AS m ON m.Id=mm.MenuId AND MM.PanelName='" + panelname + @"'
                            LEFT OUTER JOIN mms.MenuItem AS mi ON mi.MenuId=m.Id where m.Id='" + id + @"' ORDER BY m.Sequence";



            List<Dictionary<string, object>> databaseMenu = _sqlRepository.GetDataCollection(sql);
            SourceCodeMenuCollection.SourceMenuList _SourceCodeMenuList = new SourceCodeMenuCollection.SourceMenuList();

            #region matching source code menu with created menu
            for (int i = 0; i < databaseMenu.Count; i++)
            {
                var smenu = _SourceCodeMenuList.ControllerList.Where(p => p.JSHref.ToLower() == databaseMenu[i]["Href"].ToString().ToLower()).FirstOrDefault();
                if (smenu != null)
                {
                    if (databaseMenu[i]["Controller"].ToString().ToLower() != smenu.ControllerNameForMenu.ToLower())
                        databaseMenu[i]["MatchingComment"] = "Href matched but controller name missing, suggested controller name :" + smenu.ControllerNameForMenu;
                    databaseMenu[i]["SourceCodeMenu"] = smenu;
                    // _SourceCodeMenuList.ControllerList.Remove(smenu);
                }
                else
                {
                    smenu = _SourceCodeMenuList.ControllerList.Where(p => p.ControllerNameForMenu.ToLower() == databaseMenu[i]["Controller"].ToString().ToLower()).FirstOrDefault();
                    if (smenu != null)
                    {
                        databaseMenu[i]["MatchingComment"] = "Controller name matched but Href missing, suggested Href name :" + smenu.JSHref;
                        databaseMenu[i]["SourceCodeMenu"] = smenu;
                        // _SourceCodeMenuList.ControllerList.Remove(smenu);
                    }
                }

                if (databaseMenu[i]["SourceCodeMenu"] != DBNull.Value)
                {
                    if (databaseMenu[i]["PanelName"].ToString().ToLower() == "application")
                    {

                        SourceMenuControllers sourceMenu = (SourceMenuControllers)databaseMenu[i]["SourceCodeMenu"];
                        if (sourceMenu.ApplicationPanel == false)
                        {
                            databaseMenu[i]["MatchingComment"] += " Menu has been not been registered in application panel";
                        }
                        else
                        {
                            _SourceCodeMenuList.ControllerList.Remove(sourceMenu);
                        }
                    }
                    if (databaseMenu[i]["PanelName"].ToString().ToLower() == "master")
                    {
                        SourceMenuControllers sourceMenu = (SourceMenuControllers)databaseMenu[i]["SourceCodeMenu"];
                        if (sourceMenu.MasterPanel == false)
                        {
                            databaseMenu[i]["MatchingComment"] += " Menu has been not been registered in master panel.";

                        }
                        else
                        {
                            _SourceCodeMenuList.ControllerList.Remove(sourceMenu);
                        }
                    }
                }

            }
            #endregion matching source code menu with created menu

            return Json(databaseMenu, JsonRequestBehavior.AllowGet);

        }

        [HttpPost]
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


                if (string.IsNullOrEmpty(menuMaster.Code) == true)
                    throw new Exception("Enter Code");
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

                    dr["UserCode"] = menuMaster.Code;
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

                    dr["UserCode"] = menuMaster.Code;
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


                string groupids = "''";
                for (int i = 0; i < companygroup.Count; i++)
                {
                    groupids += ",'" + companygroup[i].CompanyGroupId + "'";
                }

                sql = "SELECT * FROM [MST].[CompanyGroupMenuMaster] where CompanyGroupId IN (" + groupids + ")  AND MenuMasterId='" + MenuMasterID + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsCompanyGroup, false, "1");
                //while (dsCompanyGroup.Tables[0].DefaultView.Count > 0)
                //    dsCompanyGroup.Tables[0].DefaultView[0].Delete();

                for (int i = 0; i < companygroup.Count; i++)
                {
                    dsCompanyGroup.Tables[0].DefaultView.RowFilter = "CompanyGroupId='" + companygroup[i].CompanyGroupId + "'";
                    if (companygroup[i].IsSaved == true)
                    {

                        if (dsCompanyGroup.Tables[0].DefaultView.Count == 0)
                        {
                            //if (companygroup[i].IsSaved == false)
                            //    continue;

                            string _id = "";
                            bplib.clsGenID id = new bplib.clsGenID();
                            id.GenID("MENU ASSIGN", out _id);
                            _id = "CGM" + _id;

                            DataRow dr = dsCompanyGroup.Tables[0].NewRow();
                            dr["Id"] = _id;

                            dr["ModuleId"] = menuMaster.ModuleId;
                            dr["MenuMasterId"] = MenuMasterID;

                            dr["CompanyGroupId"] = companygroup[i].CompanyGroupId;


                            dr["Active"] = companygroup[i].IsSaved;
                            dr["Archive"] = false;

                            dr["AddedBy"] = identity.Name;
                            dr["AddedDate"] = System.DateTime.Now.ToString();
                            dr["AddedFromIP"] = identity.IPAddress;
                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedDate"] = System.DateTime.Now.ToString();
                            dr["UpdatedFromIP"] = identity.IPAddress;

                            dsCompanyGroup.Tables[0].Rows.Add(dr);
                        }
                    }
                    else
                    {
                        DataRow dr = dsCompanyGroup.Tables[0].DefaultView[0].Row;
                        if (companygroup[i].IsSaved == false)
                            dr.Delete();
                        //dr.BeginEdit();

                        //dr["ModuleId"] = menuMaster.ModuleId;
                        //dr["MenuMasterId"] = MenuMasterID;

                        //dr["CompanyGroupId"] = companygroup[i].CompanyGroupId;


                        //dr["Active"] = companygroup[i].IsSaved;
                        //dr["Archive"] = false;

                        //dr["UpdatedBy"] = identity.Name;
                        //dr["UpdatedDate"] = System.DateTime.Now.ToString();
                        //dr["UpdatedFromIP"] = identity.IPAddress;

                        //dr.EndEdit();

                    }
                }
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
                info.SaveDataSets(dsMenu, dsMenuItem, dsMenuMaster, dsCompanyGroup, dsMenuAction);

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
        [HttpPost]
        public JsonResult DeleteMenu(string MenuId, string MenuMasterId)
        {
            try
            {
                ConnectionManager.clsConnection connection = new ConnectionManager.clsConnection();
                connection.BeginTransaction();
                connection.executeQuery(@"DELETE FROM SEC.RoleDetail where MenuMasterId = '" + MenuMasterId + "'");
                connection.executeQuery(@"DELETE FROM SEC.UserAccessDetail where MenuMasterId = '" + MenuMasterId + "'");
                connection.executeQuery(@"DELETE FROM MMS.MenuAction where MenuId = '" + MenuId + "'");
                connection.executeQuery(@"DELETE FROM MMS.MenuItem where MenuId = '" + MenuId + "'");
                connection.executeQuery(@"DELETE FROM MMS.MenuDetail where MenuId =  '" + MenuId + "'");
                connection.executeQuery(@"DELETE FROM MST.CompanyGroupMenuMaster where MenuMasterId = '" + MenuMasterId + "'");
                connection.executeQuery(@"DELETE FROM MST.MenuMaster where MenuId = '" + MenuId + "'");
                connection.executeQuery(@"DELETE FROM [MMS].[Menu]  where Id =  '" + MenuId + "'");
                connection.CommitTransaction();

                return Json(new { Message = "Menu Deleted Successfully", Error = false }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {

                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);

            }
        }
        [HttpGet, Authorize]
        public JsonResult GetAutoMenuSequence()
        {
            return Json(GetMenuSequence(), JsonRequestBehavior.AllowGet);
        }
        private double GetMenuSequence()
        {
            DataTable dt = _sqlRepository.GetDataTable("SELECT  isnull(Max(Sequence),0) AS Sequence FROM [MMS].[Menu]");
            if (dt.Rows.Count > 0)
                return clsStaticInfo.dbl(dt.Rows[0]["Sequence"].ToString()) + 1;

            return 1;
        }
        [HttpPost]
        public JsonResult UpdateMenuLocation(Dictionary<string, object> NewHierarchy, Dictionary<string, object> Menu, List<Dictionary<string, object>> Siblings, string PanelName)
        {
            try
            {


                if (NewHierarchy == null)
                    throw new Exception("No data updated");

                if (Menu == null)
                    throw new Exception("No data updated");

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

                string MenuID = Menu["id"].ToString();

                DataSet dsMenuMaster;
                string sql = "SELECT * FROM [MST].[MenuMaster] where Id='" + MenuID + "' and panelName='" + PanelName + "'";
                ConnectionManager.DAL.ConManager objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out dsMenuMaster, false, "1");

                string MenuMasterID = "";
                if (dsMenuMaster.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = dsMenuMaster.Tables[0].Rows[0];


                    dr.BeginEdit();

                    MenuMasterID = dr["id"].ToString();

                    dr["ModuleId"] = DBNull.Value;
                    dr["MenuFrameId"] = DBNull.Value;
                    dr["MenuGroupId"] = DBNull.Value;
                    dr["MenuSubGroupId"] = DBNull.Value;
                    dr["PanelName"] = PanelName;

                    dr["ModuleId"] = NewHierarchy["ModuleId"];
                    dr["MenuFrameId"] = NewHierarchy["MenuFrameId"];
                    dr["MenuGroupId"] = NewHierarchy["MenuGroupId"];
                    dr["MenuSubGroupId"] = NewHierarchy["MenuSubGroupId"];

                    //for (int i = 1; i < NewHierarchy.Count; i++)
                    //{
                    //    foreach (HierarchyPrefix val in Enum.GetValues(typeof(HierarchyPrefix)))
                    //    {
                    //        if (NewHierarchy[i].ToUpper().Contains(val.ToString().ToUpper()))
                    //        {
                    //            if (val == HierarchyPrefix.MODULE)
                    //                dr["ModuleId"] = NewHierarchy[i].Replace(val.ToString() + "-", "");
                    //            if (val == HierarchyPrefix.FRAME)
                    //                dr["MenuFrameId"] = NewHierarchy[i].Replace(val.ToString() + "-", "");
                    //            if (val == HierarchyPrefix.GROUP)
                    //                dr["MenuGroupId"] = NewHierarchy[i].Replace(val.ToString() + "-", "");
                    //            if (val == HierarchyPrefix.SUBGROUP)
                    //                dr["MenuSubGroupId"] = NewHierarchy[i].Replace(val.ToString() + "-", "");
                    //        }
                    //    }
                    //}



                    dr["UpdatedBy"] = identity.Name;
                    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                    dr["UpdatedFromIP"] = identity.IPAddress;

                    dr.EndEdit();
                }


                string ids = "''";
                for (int i = 0; i < Siblings.Count; i++)
                {
                    if (Siblings[i]["Id"].ToString() != MenuID)
                    {
                        ids += ",'" + Siblings[i]["Id"] + "'";
                    }
                }
                sql = "SELECT * FROM [MST].[MenuMaster] where Id IN (" + ids + ") and panelName='" + PanelName + "'";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenDataSetThroughAdapter(sql, out DataSet dsUpdateSequence, false, "1");
                for (int i = 0; i < Siblings.Count; i++)
                {
                    if (Siblings[i]["Id"].ToString() == MenuID)
                    {
                        dsMenuMaster.Tables[0].Rows[0]["Sequence"] = clsStaticInfo.dbl(Siblings[i]["Sequence"].ToString());
                    }
                    else
                    {
                        dsUpdateSequence.Tables[0].DefaultView.RowFilter = "Id='" + Siblings[i]["Id"].ToString() + "'";
                        if (dsUpdateSequence.Tables[0].DefaultView.Count > 0)
                        {
                            dsUpdateSequence.Tables[0].DefaultView[0]["Sequence"] = clsStaticInfo.dbl(Siblings[i]["Sequence"].ToString());
                        }

                    }
                }

                //DataSet dsCompanyGroup;
                //sql = "SELECT * FROM [MST].[CompanyGroupMenuMaster] where MenuMasterId='" + MenuMasterID + "'";
                //objCon = new ConnectionManager.DAL.ConManager("1");
                //objCon.OpenDataSetThroughAdapter(sql, out dsCompanyGroup, false, "1");

                //for (int i = 0; i < dsCompanyGroup.Tables[0].Rows.Count; i++)
                //{

                //    DataRow dr = dsCompanyGroup.Tables[0].Rows[i];

                //    dr.BeginEdit();

                //    dr["ModuleId"] = dsMenuMaster.Tables[0].Rows[0]["ModuleId"];

                //    dr["UpdatedBy"] = identity.Name;
                //    dr["UpdatedDate"] = System.DateTime.Now.ToString();
                //    dr["UpdatedFromIP"] = identity.IPAddress;

                //    dr.EndEdit();


                //}



                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsMenuMaster, dsUpdateSequence);




                return Json(new { Message = "Menu Updated Successfully", Error = false }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Message = ex.Message, Error = true }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        public ActionResult GetMenuDetailList()
        {
            try
            {

                string FirstJoin = "MST.MenuMaster MM";
                string cmdText = @"SELECT Null AS SourceCodeMenu,'' AS MatchingComment,'' AS Area,  MN.Description MenuName,MN.Id Id
                                      , MDL.Id ModuleId,'' MenuItemGroup, mn.Sequence, MNFR.Id MenuFrameId, MG.Id MenuGroupId
                                      , SMG.Id MenuSubGroupId
                                      ,MM.Id MenuMasterId , MM.PanelName,Mn.Description,mm.IsExternalMenu,MM.Remarks,MM.Active ,Mn.Controller,Mn.Href ,MDL.UserName Module
                                      , SMDL.UserName SubModule, SMDL.Id SubModuleId , MNFR.UserName MenuFrame, MG.UserName MenuGroup , SMG.UserName SubMenuGroup
									  ,MI.Code, MM.Remarks,Mn.Href,MN.Controller,MM.PanelName--,ISNULL(MarkForDeletion,0) MarkForDeletion
                                FROM {0}
                                LEFT JOIN MMS.Module MDL ON MDL.Id = MM.ModuleId
                                LEFT JOIN MMS.SubModule SMDL ON MDL.Id = MM.SubModuleId
                                LEFT JOIN MMS.Menu Mn ON Mn.Id = MM.MenuId
                                LEFT JOIN MMS.MenuFrame MNFR ON MNFR.Id = MM.MenuFrameId
                                LEFT JOIN MMS.MenuGroup MG ON MG.Id = MM.MenuGroupId
                                LEFT JOIN MMS.MenuSubGroup SMG ON SMG.Id = MM.MenuSubGroupId
                                LEFT JOIN MMS.MenuItem MI ON MI.Id = MM.MenuItemId
                                ORDER BY MDL.Sequence,SMDL.Sequence,MNFR.Sequence,MG.Sequence,SMG.Sequence, MN.Sequence";

                string sql = string.Format(cmdText, FirstJoin);

                List<Dictionary<string, object>> databaseMenu = _sqlRepository.GetDataCollection(sql);
                SourceCodeMenuCollection.SourceMenuList _SourceCodeMenuList = new SourceCodeMenuCollection.SourceMenuList();


                #region matching source code menu with created menu

                for (int i = 0; i < databaseMenu.Count; i++)
                {
                    if (databaseMenu[i]["Href"].ToString().ToLower() == "buyer-master".ToLower())
                    {

                    }
                    var smenu = _SourceCodeMenuList.ControllerList.Where(p => p.JSHref.ToLower() == databaseMenu[i]["Href"].ToString().ToLower()).FirstOrDefault();
                    if (smenu != null)
                    {
                        if (databaseMenu[i]["Controller"].ToString().ToLower() != smenu.ControllerNameForMenu.ToLower())
                        {
                            databaseMenu[i]["MatchingComment"] = "href matched but Controller name missing, suggested controller name :" + smenu.ControllerNameForMenu;
                        }

                        databaseMenu[i]["SourceCodeMenu"] = smenu;
                        databaseMenu[i]["Area"] = smenu.Area;


                    }
                    else
                    {
                        smenu = _SourceCodeMenuList.ControllerList.Where(p => p.ControllerNameForMenu.ToLower() == databaseMenu[i]["Controller"].ToString().ToLower()).FirstOrDefault();
                        if (smenu != null)
                        {
                            databaseMenu[i]["MatchingComment"] = "Controller matched but invalid Href, suggested Href name :" + smenu.JSHref;
                            databaseMenu[i]["SourceCodeMenu"] = smenu;
                            databaseMenu[i]["Area"] = smenu.Area;
                        }
                        else
                        {

                            databaseMenu[i]["MatchingComment"] += "Controller and Href both are invalid";

                        }
                    }
                }

                StringCollection strCol = new StringCollection();
                for (int i = 0; i < databaseMenu.Count; i++)
                {
                    if (databaseMenu[i]["SourceCodeMenu"] != DBNull.Value)
                    {
                        if (databaseMenu[i]["PanelName"].ToString().ToLower() == "application")
                        {

                            SourceMenuControllers sourceMenu = (SourceMenuControllers)databaseMenu[i]["SourceCodeMenu"];
                            if (sourceMenu.ApplicationPanel == false)
                            {
                                databaseMenu[i]["MatchingComment"] += " Menu has been not been registered in application panel";
                            }

                        }
                        if (databaseMenu[i]["PanelName"].ToString().ToLower() == "master")
                        {
                            SourceMenuControllers sourceMenu = (SourceMenuControllers)databaseMenu[i]["SourceCodeMenu"];
                            if (sourceMenu.MasterPanel == false)
                            {
                                databaseMenu[i]["MatchingComment"] += " Menu has been not been registered in master panel";
                            }

                        }


                        SourceMenuControllers _data = (SourceMenuControllers)databaseMenu[i]["SourceCodeMenu"];
                        databaseMenu[i]["Area"] = _data.Area;

                        _SourceCodeMenuList.ControllerList.Remove(_data);
                    }

                    if (strCol.Contains(databaseMenu[i]["Href"].ToString().ToLower() + "-" + databaseMenu[i]["PanelName"].ToString().ToLower()) == false)
                    {
                        strCol.Add(databaseMenu[i]["Href"].ToString().ToLower() + "-" + databaseMenu[i]["PanelName"].ToString().ToLower());
                    }
                    else
                    {
                        databaseMenu[i]["MatchingComment"] += "[Duplicate Menu]";
                    }
                }
                #endregion matching source code menu with created menu

                #region add unused menus
                FirstJoin = @" (SELECT 1 AS Test) AS TRK
                                left join MST.MenuMaster MM ON 1=2 ";
                sql = string.Format(cmdText, FirstJoin);
                Dictionary<string, object> databaseMenuBlank = _sqlRepository.GetData(sql);
                Dictionary<string, object> _newData = databaseMenuBlank.DeepClone<Dictionary<string, object>>();

                //for application panel
                strCol = new StringCollection();
                foreach (SourceMenuControllers item in _SourceCodeMenuList.ControllerList)
                {
                    if (strCol.Contains(item.JSHref) == false)
                    {
                        strCol.Add(item.JSHref);

                        if (item.ApplicationPanel == true)
                        {
                            _newData = databaseMenuBlank.DeepClone<Dictionary<string, object>>();
                            _newData["SourceCodeMenu"] = item;
                            _newData["MatchingComment"] = "New Item";
                            _newData["Area"] = item.Area;
                            _newData["MenuName"] = null;
                            _newData["Id"] = null;
                            _newData["ModuleId"] = null;
                            _newData["MenuItemGroup"] = null;
                            _newData["Sequence"] = null;
                            _newData["MenuFrameId"] = null;
                            _newData["MenuGroupId"] = null;
                            _newData["MenuSubGroupId"] = null;
                            _newData["MenuMasterId"] = null;
                            _newData["PanelName"] = "Application";
                            _newData["Description"] = item.ControllerNameForMenu;
                            _newData["IsExternalMenu"] = false;
                            _newData["Remarks"] = null;
                            _newData["Active"] = true;
                            _newData["Controller"] = item.ControllerNameForMenu;
                            _newData["Href"] = item.JSHref;
                            _newData["Module"] = "";
                            _newData["SubModule"] = "";
                            _newData["SubModuleId"] = "";
                            _newData["MenuFrame"] = "";
                            _newData["MenuGroup"] = "";
                            _newData["SubMenuGroup"] = "";
                            _newData["Code"] = "";
                            _newData["Remarks"] = "";
                            _newData["MenuGroup"] = "";



                            databaseMenu.Add(_newData);
                        }

                        if (item.MasterPanel == true)
                        {
                            _newData = databaseMenuBlank.DeepClone<Dictionary<string, object>>();
                            _newData["SourceCodeMenu"] = item;
                            _newData["MatchingComment"] = "New Item";
                            _newData["Area"] = item.Area;
                            _newData["MenuName"] = null;
                            _newData["Id"] = null;
                            _newData["ModuleId"] = null;
                            _newData["MenuItemGroup"] = null;
                            _newData["Sequence"] = null;
                            _newData["MenuFrameId"] = null;
                            _newData["MenuGroupId"] = null;
                            _newData["MenuSubGroupId"] = null;
                            _newData["MenuMasterId"] = null;
                            _newData["PanelName"] = "Master";
                            _newData["Description"] = null;
                            _newData["IsExternalMenu"] = false;
                            _newData["Remarks"] = null;
                            _newData["Active"] = true;
                            _newData["Controller"] = item.ControllerNameForMenu;
                            _newData["Href"] = item.JSHref;
                            _newData["Module"] = "";
                            _newData["SubModule"] = "";
                            _newData["SubModuleId"] = "";
                            _newData["MenuFrame"] = "";
                            _newData["MenuGroup"] = "";
                            _newData["SubMenuGroup"] = "";
                            _newData["Code"] = "";
                            _newData["Remarks"] = "";
                            _newData["MenuGroup"] = "";


                            databaseMenu.Add(_newData);
                        }

                    }
                }
                #endregion add unused menus 


                var jsondata = Json(databaseMenu, JsonRequestBehavior.AllowGet);
                jsondata.MaxJsonLength = int.MaxValue;
                return jsondata;
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
    }
    public class menuCompanyGroup
    {

        public string CompanyGroupId { get; set; } = "";
        public string UserName { get; set; } = "";
        public bool IsSaved { get; set; } = false;
    }
    public class menuCombined
    {
        public string Id { get; set; } = "";
        public string ModuleId { get; set; } = "";
        public decimal Sequence { get; set; }
        public string MenuItemGroup { get; set; } = "";
        public string MenuFrameId { get; set; } = "";
        public string MenuGroupId { get; set; } = "";
        public string MenuSubGroupId { get; set; } = "";
        public string PanelName { get; set; } = "";
        public string Description { get; set; } = "";
        public bool IsExternalMenu { get; set; } = false;
        public string Remarks { get; set; } = "";
        public bool Active { get; set; } = true;
        public string Code { get; set; } = "";
        public string UserCode { get; set; } = "";
        public string Controller { get; set; } = "";
        public string Href { get; set; } = "";
    }
}