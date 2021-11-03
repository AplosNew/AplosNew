#region Using
using Aplos.Properties;
using System.Web.Mvc;
using Aplos.Controllers;
using Library.Crosscutting.Security;
using System.Threading;
using Library.Data.Sql;
using System.Collections.Generic;
using Library.Service.Systems;
using Library.OrderManagement.Production;
using Library.Model.Productions.ProductionBooking;
using Library.Service.Productions;
using System.Data;
using System;
using Library.Security.Core;
using System.Collections.Specialized;
using System.Linq;
using Library.OrderManagement.Packing;

#endregion

namespace Aplos.Areas.OrderManagements.Controllers
{
    public class PackingConfirmationController : BaseController
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly IProductionSummaryService _ProductionSummaryService;
        clsPacking _clsPacking = new clsPacking();

        public PackingConfirmationController(ISqlRepository R, IPKGeneratorService pkGeneratorService, IProductionSummaryService ProductionSummaryService)
        {
            _sqlRepository = R;
            _pkGeneratorService = pkGeneratorService;
            _ProductionSummaryService = ProductionSummaryService;

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
        public ActionResult GetDataList()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_clsPacking.GetDataList(identity.PlantId), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public JsonResult GetPackingProcessCbo(string entity)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_clsPacking.GetPackingProcessCbo(entity, identity.IsSysAdmin, identity.IsControlAdmin, identity.UserId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public ActionResult GetProductionOrderDataList()
        {
            return Json(_clsPacking.GetProductionOrderDataList(), JsonRequestBehavior.AllowGet);
        }


        [HttpGet, Authorize]
        public ActionResult GetPackDataList(string MasterId)
        {
            return Json(_clsPacking.GetPackDataList(MasterId), JsonRequestBehavior.AllowGet);
        }

        [HttpGet, Authorize]
        public JsonResult GetToProcessCbo(string FromId, string EntityId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            return Json(_clsPacking.GetToProcessCbo(FromId, EntityId, identity.IsSysAdmin, identity.IsControlAdmin, identity.UserId), JsonRequestBehavior.AllowGet);

        }


        [HttpPost, Authorize]
        public JsonResult Create(ProductionSummary ps, PackingConfirmation pc, List<ProductionSummaryDetail> psd, IEnumerable<PackingChild> packingChild)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            OTSBD.IdentityParameter para = new OTSBD.IdentityParameter
            {
                CompanyGroupId = identity.CompanyGroupId,
                CompanyId = identity.CompanyId,
                PlantId = identity.PlantId,
                AddedBy = identity.Name,
                AddedDate = DateTime.Now,
                AddedFromIP = identity.IPAddress,
                UpdatedBy = identity.Name,
                UpdatedDate = DateTime.Now,
                UpdatedFromIP = identity.IPAddress
            };

            var psdNew = psd.GroupBy(a => new { a.Characteristics1ValueId, a.Characteristics2ValueId, a.Characteristics3ValueId })
    .Select(a => new { Qty = a.Sum(b => b.Qty), Characteristics1ValueId = a.Key.Characteristics1ValueId, Characteristics2ValueId = a.Key.Characteristics2ValueId, Characteristics3ValueId = a.Key.Characteristics3ValueId }).ToList();

            psd = new List<ProductionSummaryDetail>();
            foreach (var item in psdNew)
            {
                psd.Add(new ProductionSummaryDetail
                {
                    Characteristics1ValueId = item.Characteristics1ValueId
                ,
                    Characteristics2ValueId = item.Characteristics2ValueId
                ,
                    Characteristics3ValueId = item.Characteristics3ValueId
                ,
                    Qty = item.Qty
                });
            }

            ps.PlantId = identity.PlantId;
            pc.PlantId = identity.PlantId;
            _clsPacking.SavePackingConfirmationData(pc, para, out string masterId);
            ps.PackingConfirmationId = masterId;
            _ProductionSummaryService.SaveInOutMaster(ps, psd, identity.CompanyGroupId);
            _clsPacking.SaveConfirmPackingChildData(packingChild, para, masterId);
            return Json(new { Message = AplosMessage.Success });
        }



        #endregion Operations

    }

}