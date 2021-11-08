#region Using
using Aplos.Controllers;
using System.Web.Mvc;
using Library.HumanResource.NewAttendanceProcess;
using System.Data;
using System;

#endregion

namespace Aplos.Areas.Attendances.Controllers
{
    public class SandwichProcessController : BaseController
    {
        #region Constructor

        SandwichProcessService ss = new SandwichProcessService();

        public SandwichProcessController(
            )
        {
        }
        #endregion

        #region -- Pages
        public ActionResult Aplos()
        {
            return View();
        }
        #endregion

        [HttpPost, Authorize]
        public ActionResult GetEmployeeInformation(string month , string year)
        {
            var jsondata = Json(ss.GetEmployeeInformation(month, year), JsonRequestBehavior.AllowGet);
            jsondata.MaxJsonLength = int.MaxValue;
            return jsondata;
        }

        [HttpPost, Authorize]
        public ActionResult RunProcess(string month, string year)
        {
            string CGId = "";
            DataSet GroupList;
            NewAttendanceProcessService repo = new NewAttendanceProcessService();

            repo.GetCompanyGp(out GroupList);
            if (GroupList.Tables[0].Rows.Count > 0)
            {

                for (int k = 0; k < GroupList.Tables[0].Rows.Count; k++)
                {
                    CGId = GroupList.Tables[0].Rows[k][@"CGId"].ToString();

                }
            }

            DataSet PlantList;
            repo.GetPlant(CGId, out PlantList);

            if (PlantList.Tables[0].Rows.Count > 0)
            {

                for (int j = 0; j < PlantList.Tables[0].Rows.Count; j++)
                {
                    string CatchPlant = "";
                    try
                    {
                        var PlantValue = PlantList.Tables[0].Rows[j][@"PlantValue"].ToString();
                        CatchPlant = PlantValue;
                        ss.Process(PlantValue,month,year);
                    }
                    catch (Exception ex)
                    {
                        repo.CommonLogFunction(ex, CatchPlant, "SandwichProcess");
                    }
                }
            }
            return Json(new { Error = false, Message = "Sandwich Process Triggered Successfully..." }, JsonRequestBehavior.AllowGet);

        }
    }
}