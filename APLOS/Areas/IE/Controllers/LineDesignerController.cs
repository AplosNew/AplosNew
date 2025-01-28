#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data.Sql;
using Library.Model.IE;
using Library.Model.Setups;
using Library.Service.IE;
using Library.Service.Setups;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.IE.Controllers
{
    public class LineDesignerController : BaseController
    {
        #region Constructor
        private readonly SqlRepository _sqlRepository;
        private readonly ISizeGroupService _sizeGroupService;

        public LineDesignerController(
              ISizeGroupService sizeGroupService
            )
        {
            _sizeGroupService = sizeGroupService;
            _sqlRepository = new SqlRepository();
        }

        #endregion Constructor


        [Authorize]
        public ActionResult Aplos()
        {
            return View();
        }

        [HttpGet, Authorize]
        public JsonResult GetAllData()
        {
            Library.Planning.LineDesign.GenerateLineDiagram _diagram = new Library.Planning.LineDesign.GenerateLineDiagram();
            _diagram.MakeBulletinList("");

            return Json(_diagram.AllShapesForJson, JsonRequestBehavior.AllowGet);
        }

        [Authorize, HttpPost]
        public ActionResult SearchEmployee(string column, string value)
        {
            string strkey = "1=1";
            if (string.IsNullOrEmpty(column) == false)
                strkey = column + " like '%" + value + "%'";

            string sql = @"
                      select top 100 * from (  
                        SELECT distinct Emp.SystemID AS Id,
                        EMP.EmployeeName,EMP.EmployeeCode,EMP.EmpPicPath,
						isnull(D.UserName,'') Designation,
      
                            DEPT.UserName Department,S.UserName Section,
                            PR.SectionId,SS.UserName SubSection
                            ,PL.UserName Plant
                            FROM EmployeeInformation EMP
                            LEFT JOIN MST.ManpowerBudget PMB ON EMP.BudgetCode=PMB.Id
                            LEFT JOIN ORG.Position PR ON PMB.PositionId=PR.Id
                            LEFT JOIN ORG.Entity E ON PMB.EntityId=E.Id
                            LEFT JOIN ORG.Section S ON S.Id=PR.SectionId
                            LEFT JOIN ORG.SubSection SS ON SS.Id=PR.SubSectionId
                            LEFT OUTER JOIN hkp.LegalDesignation AS D ON D.Id=EMP.LegalDesignationId
                            LEFT JOIN ORG.Department DEPT ON PR.DepartmentId=DEPT.Id
                            LEFT JOIN ORG.Plant PL ON PL.Id=EMP.PlantId
                            LEFT JOIN HKP.Designation DEG ON EMP.GivenDesignationId=DEG.Id
   
                        WHERE emp.EmployeeStatus='Active' 
                ) AS TEMP where " + strkey + " Order By Id";





            return Json(_sqlRepository.GetDataCollection(sql, null), JsonRequestBehavior.AllowGet);
        }

    }
}