using Library.Service.Employees;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Mvc;

namespace SourceCodeMenuCollection
{
    public class SourceMenuControllers
    {
        private string area = "";
        public string Area
        {
            get
            {
                string[] nodes = JStemplateUrl.Split('/');
                foreach (var item in nodes)
                {
                    if (string.IsNullOrEmpty(item))
                        continue;

                    area = item;
                    break;
                }


                return area;

            }
        }
        public string ControllerName { get; set; } = "";
        public string ControllerNameForMenu { get; set; } = "";
        public string ControllerFullName { get; set; } = "";
        public List<SourceMenuAction> Actions { get; set; }
        public string JSHref { get; set; } = "";
        public string JStemplateUrl { get; set; } = "";
        public string JScontroller { get; set; } = "";
        public bool ApplicationPanel { get; set; } = false;
        public bool MasterPanel { get; set; } = false;

    }
    public class SourceMenuAction
    {
        public string ActionName { get; set; }
        public string ActionUserDefinedName { get; set; }
    }
    public class SourceMenuDetail
    {


        public string Href { get; set; }
        public string templateUrl { get; set; }
        public string controller { get; set; }



    }

    public class SourceMenuList
    {
        string AplosAssemblyPath = @"bin\Aplos.dll";
        string ConfigRootDirectory = "Areas";
        string ApplicationPanelPath = @"Scripts\UPanelApp\upanelApp.js";
        string MasterPanelPath = @"Scripts\MPanelApp\mpanelApp.js";

        public SourceMenuList()
        {
            ConfigRootDirectory = HostingEnvironment.MapPath("~/") + "Areas";
            ApplicationPanelPath = HostingEnvironment.MapPath("~/") + @"Scripts\UPanelApp\upanelApp.js";
            MasterPanelPath = HostingEnvironment.MapPath("~/") + @"Scripts\MPanelApp\mpanelApp.js";
            GenerateMenuListFromSourceCode();
        }
        public List<SourceMenuControllers> ControllerList = new List<SourceMenuControllers>();
        List<string> ConfigFileList = new List<string>();
        List<SourceMenuDetail> HrefList = new List<SourceMenuDetail>();

        public void GenerateMenuListFromSourceCode()
        {


            SourceCodeControllers();


            DirectoryInfo directoryInfo = new DirectoryInfo(ConfigRootDirectory);
            SourceCodeHrefs(directoryInfo);

            for (int i = 0; i < ConfigFileList.Count; i++)
            {
                SourceCodeHrefAndController(ConfigFileList[i]);
            }



            //add remaining hrefs to the menu collection
            List<SourceMenuControllers> sourceMenuControllersTemp = new List<SourceMenuControllers>();
            for (int i = 0; i < ControllerList.Count; i++)
            {
                ControllerList[i].ControllerFullName = ControllerList[i].ControllerFullName.Replace(".", "/");
                ControllerList[i].ControllerFullName = ControllerList[i].ControllerFullName.Replace("Aplos/Areas/", "");
                ControllerList[i].ControllerFullName = ControllerList[i].ControllerFullName.Replace("/Controllers", "");
                ControllerList[i].ControllerFullName = ControllerList[i].ControllerFullName.Replace(ControllerList[i].ControllerName, ControllerList[i].ControllerNameForMenu + "/");
            }
            foreach (var item in HrefList)
            {
                if (item.templateUrl.EndsWith("/") == false)
                    item.templateUrl = item.templateUrl + "/";

                if (item.Href.ToLower() == "buyer-master")
                {


                }

                SourceMenuControllers matchedController = ControllerList.Where(p => p.ControllerName.ToLower() == item.controller.ToLower()).FirstOrDefault();
                SourceMenuControllers tempController = new SourceMenuControllers();
                if (matchedController == null)
                {
                    List<SourceMenuControllers> matchedControllers = ControllerList.Where(p => item.templateUrl.ToLower().Contains(p.ControllerFullName.ToLower())).ToList();
                    if (matchedControllers == null || matchedControllers.Count == 0)
                    {

                        //nothing contained
                        tempController = new SourceMenuControllers
                        {
                            ControllerNameForMenu = item.controller.Remove(item.controller.Length - 10),
                            JScontroller = item.controller,
                            JSHref = item.Href,
                            JStemplateUrl = item.templateUrl,

                        };
                    }
                    else if (matchedControllers.Count == 1)
                    {
                        //single contained
                        tempController = new SourceMenuControllers
                        {
                            Actions = matchedControllers[0].Actions,
                            ApplicationPanel = matchedControllers[0].ApplicationPanel,
                            ControllerFullName = matchedControllers[0].ControllerFullName,
                            ControllerName = matchedControllers[0].ControllerName,
                            ControllerNameForMenu = matchedControllers[0].ControllerNameForMenu,
                            JScontroller = item.controller,
                            JSHref = item.Href,
                            JStemplateUrl = item.templateUrl,
                            MasterPanel = matchedControllers[0].MasterPanel,
                        };
                    }
                    else
                    {
                        //multiple contained

                        List<SourceMenuControllers> matchedControllersMultipleToSingle = matchedControllers.Where(p => item.templateUrl.ToLower() == p.ControllerFullName.ToLower()).ToList();
                        if (matchedControllersMultipleToSingle.Count > 0)
                        { //single matched
                            tempController = new SourceMenuControllers
                            {
                                Actions = matchedControllersMultipleToSingle[0].Actions,
                                ApplicationPanel = matchedControllersMultipleToSingle[0].ApplicationPanel,
                                ControllerFullName = matchedControllersMultipleToSingle[0].ControllerFullName,
                                ControllerName = matchedControllersMultipleToSingle[0].ControllerName,
                                ControllerNameForMenu = matchedControllersMultipleToSingle[0].ControllerNameForMenu,
                                JScontroller = item.controller,
                                JSHref = item.Href,
                                JStemplateUrl = item.templateUrl,
                                MasterPanel = matchedControllersMultipleToSingle[0].MasterPanel,
                            };


                        }
                        else
                        {

                            //multiple matched, need to find out the best match

                            //nothing mactched
                            tempController = new SourceMenuControllers
                            {
                                ControllerNameForMenu = item.controller.Remove(item.controller.Length - 10),
                                JScontroller = item.controller,
                                JSHref = item.Href,
                                JStemplateUrl = item.templateUrl,

                            };
                        }



                    }

                }
                else
                {
                    tempController = new SourceMenuControllers
                    {
                        Actions = matchedController.Actions,
                        ApplicationPanel = matchedController.ApplicationPanel,
                        ControllerFullName = matchedController.ControllerFullName,
                        ControllerName = matchedController.ControllerName,
                        ControllerNameForMenu = matchedController.ControllerNameForMenu,
                        JScontroller = item.controller,
                        JSHref = item.Href,
                        JStemplateUrl = item.templateUrl,
                        MasterPanel = matchedController.MasterPanel,
                    };

                }


                sourceMenuControllersTemp.Add(tempController);
            }





            ControllerList = sourceMenuControllersTemp;

            SourceCodeApplicationPanel();
            SourceCodeMasterPanel();

            ControllerList = ControllerList.Where(p => string.IsNullOrEmpty(p.JScontroller) == false && (p.ApplicationPanel == true || p.MasterPanel == true)).ToList();
            ControllerList = ControllerList.OrderBy(rr => rr.Area).ThenBy(rr => rr.ControllerFullName).ToList();

        }

        private void SourceCodeControllers()
        {
            string AssemblyName = "Aplos";
            string NameSpaceFilter = "Aplos.Areas";
            string ClassNameFilter = "Controller";

            Assembly asm;
            try
            {
                asm = Assembly.Load(AssemblyName);
            }
            catch (Exception)
            {
                asm = Assembly.GetExecutingAssembly();
            }
            if (asm == null)
                asm = Assembly.GetExecutingAssembly();


            var classes1 = asm.GetTypes();
            // List<Type> ControllerList = new List<Type>();
            ControllerList = new List<SourceMenuControllers>();

            foreach (var item in classes1)
            {
                if (item != null)
                {
                    if (item.Namespace == null || item.Name == null)
                        continue;

                    if (item.Namespace.StartsWith(NameSpaceFilter) && item.Name.Contains(ClassNameFilter))
                    {
                        //we got the controller list
                        List<SourceMenuAction> MethodName = new List<SourceMenuAction>();
                        SourceMenuControllers _dic = new SourceMenuControllers
                        {
                            ControllerNameForMenu = item.Name.Trim().Remove(item.Name.Trim().Length - 10),
                            ControllerName = item.Name,
                            ControllerFullName = item.FullName,
                            Actions = MethodName
                        };

                        ControllerList.Add(_dic);

                        //Console.WriteLine("--------------------------------------------------------");
                        //Console.WriteLine("Name: {0}, FullName: {1}", item.Name, item.FullName);


                        //need to get the actions without authorization attributer
                        foreach (var method in item.GetMethods())
                        {
                            if (method.ReturnType == typeof(ActionResult) || method.ReturnType == typeof(JsonResult))
                            {
                                if (method.Name.ToUpper() == "RENDERREPORTASPDF" || method.Name.ToUpper() == "RENDERREPORTASEXCEL")
                                    continue;
                                bool Authorized = false;
                                foreach (var Attr in method.GetCustomAttributes())
                                {
                                    if (Attr.ToString().ToUpper().Contains("AUTHORIZE"))
                                    {
                                        Authorized = true;
                                    }
                                    if (Attr.ToString().ToUpper().Contains("ALLOW"))
                                    {
                                        Authorized = true;
                                    }
                                }

                                if (Authorized == false)
                                {

                                    MethodName.Add(new SourceMenuAction { ActionName = method.Name });
                                    //Console.WriteLine(method.Name);
                                }

                            }
                        }

                    }
                }
            }
        }
        private void SourceCodeHrefs(DirectoryInfo ConfigRootDirectory)
        {


            try
            {
                foreach (FileInfo item in ConfigRootDirectory.GetFiles())
                {
                    if (item.Name.ToUpper().Contains("CONFIG.JS"))
                        ConfigFileList.Add(item.FullName);
                }
                foreach (DirectoryInfo item in ConfigRootDirectory.GetDirectories())
                {
                    SourceCodeHrefs(item);
                }
            }
            catch (Exception ex)
            {


            }

        }
        private void SourceCodeHrefAndController(string FilePath)
        {


            try
            {
                string ln = "";
                SourceMenuDetail _menu = new SourceMenuDetail();
                using (StreamReader file = new StreamReader(FilePath))
                {
                    while ((ln = file.ReadLine()) != null)
                    {
                        ln = ln.Trim().Replace(" ", "");

                        if (ln.StartsWith("//"))
                            continue;

                        if (ln.Contains(".when"))
                        {
                            _menu = new SourceMenuDetail();
                            HrefList.Add(_menu);
                            _menu.Href = ReadValue(ln).Replace("/", "");
                        }
                        if (ln.Contains("templateUrl"))
                        {
                            _menu.templateUrl = ReadValue(ln);
                        }
                        if (ln.Contains("controller"))
                        {
                            _menu.controller = ReadValue(ln);
                        }

                    }
                }
            }
            catch (Exception ex)
            {


            }

        }
        private void SourceCodeApplicationPanel()
        {


            try
            {
                string ln = "";
                using (StreamReader file = new StreamReader(ApplicationPanelPath))
                {
                    while ((ln = file.ReadLine()) != null)
                    {
                        ln = ln.Trim().Replace(" ", "");

                        if (ln.StartsWith("//"))
                            continue;


                        if (ln.Contains(".controller"))
                        {
                            string ControllerName = ReadValue(ln).Replace("/", "");
                            var con = ControllerList.Where(p => p.JScontroller.ToString().ToUpper() == ControllerName.ToUpper());
                            foreach (var item in con)
                            {
                                item.ApplicationPanel = true;
                            }
                        }


                    }
                }
            }
            catch (Exception ex)
            {


            }

        }
        private void SourceCodeMasterPanel()
        {


            try
            {
                string ln = "";
                using (StreamReader file = new StreamReader(MasterPanelPath))
                {
                    while ((ln = file.ReadLine()) != null)
                    {
                        ln = ln.Trim().Replace(" ", "");

                        if (ln.StartsWith("//"))
                            continue;

                        if (ln.Contains(".controller"))
                        {
                            string ControllerName = ReadValue(ln).Replace("/", "");
                            var con = ControllerList.Where(p => p.JScontroller.ToString().ToUpper() == ControllerName.ToUpper());
                            foreach (var item in con)
                            {
                                item.MasterPanel = true;
                            }
                        }


                    }
                }
            }
            catch (Exception ex)
            {


            }

        }

        private string ReadValue(string Line)
        {
            Line = Line.Trim().Replace(" ", "");
            MatchCollection col = Regex.Matches(Line, "\\\"(.*?)\\\"");
            if (col.Count == 0)
                col = Regex.Matches(Line, "\\'(.*?)\\'");
            if (col.Count == 1)
            {
                return col[0].Value.Replace("\"", "").Replace("'", "");
            }
            else
            {
                throw new Exception("");
                // return "";
            }

        }
    }

}
