#region Using

using Aplos.Controllers;
using Aplos.Properties;
using Library.Core;
using Library.Data.Sql;
using Library.Model.IE;
using Library.Model.Setups;
using Library.Planning.LineDesign;
using Library.Service.IE;
using Library.Service.Setups;
using System.Collections.Generic;
using System.Web.Mvc;

#endregion Using

namespace Aplos.Areas.IE.Controllers
{
    public class LineLayoutForProductionBulletinController : BaseController
    {
        #region Constructor
        private readonly SqlRepository _sqlRepository;
        private readonly ISizeGroupService _sizeGroupService;
        clsLineLayoutForProductionBulletin cp = new clsLineLayoutForProductionBulletin();
        public LineLayoutForProductionBulletinController(
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
        public JsonResult GetAllData(string BulletinId)
        {
            Library.Planning.LineDesign.GenerateLineDiagraForLineLayout _diagram = new Library.Planning.LineDesign.GenerateLineDiagraForLineLayout();
            _diagram.MakeBulletinList(BulletinId,GenerateLineDiagraForLineLayout.DrawType.TwoLines);

            return Json(_diagram.AllShapesForJson, JsonRequestBehavior.AllowGet);
        }
        [Authorize, HttpPost]
        public ActionResult GetOperationVariationCard(string OperationVariationId)
        {
            clsDailyTergatLineDesign DT = new clsDailyTergatLineDesign();
            return Json(DT.GetOperationVariationCard(OperationVariationId), JsonRequestBehavior.AllowGet);
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

        [HttpGet, Authorize]
        public JsonResult GetProductionOrderDataList(string entityId)
        {
            return Json(cp.GetProductionOrderData(entityId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetOperationList(string ProductionBulletinMasterId,string ProcessId)
        {
            string sql = "";
            sql = @"SELECT ov.ArticleId,m.MaterialMasterId,mm.UserName AS MachineMaster,M.StandardName MachineName,mm.UserName MaterialMasterDesc
					,OV.Id OperationVariationId, OV.Code,OV.StandardName OperationVariationDesc, ISNULL(OV.Frequency,0)Frequency,ISNULL(OV.SPI,0)SPI
					,O.Id OperationId,O.UserName OperationDesc,ps.UserName AS ProductionSystem,O.IsMachineRequired,
                    ISNULL(O.PersonalAllowance,0)PersonalAllowance,ISNULL(OV.MachineAllowance,0)MachineAllowance
					,ISNULL(OV.AdditionalAllowance,0) AdditionalAllowances,
                    ISNULL(M.RPM,0)RPM,OV.TotalSAM,O.IsMachineRequired,ov.TotalSAM AS TotalSPT
                    ,M.StandardName AS ArticleDesc,M.ShortName AS ArticleShortName
                    FROM [MST].[OperationVariation] OV
                    join trn.ProductionBulletinTemplateDetail pbt on pbt.OperationVariationId = ov.Id
                    JOIN  trn.ProductionBulletinTemplateMaster pbtm ON pbtm.Id = pbt.ProductionBulletinTemplateMasterId AND pbtm.ProcessId='"+ ProcessId + @"'
                    LEFT JOIN [MST].[MaterialMasterArticle] M ON M.Id = OV.ArticleId
                    LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=m.MaterialMasterId
                    LEFT JOIN [MST].[Operation] O ON O.Id = OV.OperationId
                    LEFT JOIN hkp.ProductionSystem AS ps ON ps.Id=o.ProductionSystemId
                     where pbtm.ProductionBulletinTemplateId = '"+ ProductionBulletinMasterId + "' ORDER BY pbt.Sequence";

            return Json(_sqlRepository.GetDataCollection(sql), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetSaveData(string BulletinId)
        {
            return Json(cp.GetDesign(BulletinId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Save(List<Html> Nodes,string Design, string ProductionBulletinTemplateMasterId, string EntityId, string ProductionOrderId,string ProcessId)
        {
            cp.SaveData(Nodes, Design, ProductionBulletinTemplateMasterId, EntityId, ProductionOrderId, ProcessId);
            return Json(new {  Message = AplosMessage.Success }, JsonRequestBehavior.AllowGet);
        }

    }
}