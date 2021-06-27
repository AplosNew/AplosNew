using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.IE;
using Library.Service.Core;
using Library.Service.Machines;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Library.Service.IEnumerable
{
    public class SubsectionStructureDetailService : Service<SubsectionStructureDetail>, ISubsectionStructureDetailService
    {
        private readonly string _TableName = DbSchema.Transaction + ".[" + DbTable.SubsectionStructureDetail + "]";
        private readonly string _TN_Zone = DbSchema.HKP + ".[" + DbTable.FGZone + "]";
        private readonly string _TN_Component = DbSchema.HKP + ".[" + DbTable.FGComponent + "]";
        private readonly string _TN_DesignationGroup = DbSchema.HKP + ".[DesignationGroup]";
        private readonly string _TN_Operation = DbSchema.Masters + ".[" + DbTable.Operation + "]";
        private readonly string _TN_MachineType = DbSchema.Masters + ".[MaterialMasterMachineProcess]";//_TN_MachineTypeClass
        private readonly string _Section = DbSchema.Organizations + ".[Section]";
        private readonly string _SubSection = DbSchema.Organizations + ".[SubSection]";
        private readonly string _Department = DbSchema.Organizations + ".[Department]";
        private readonly string _Division = DbSchema.Organizations + ".[Division]";
        private readonly string _Line = DbSchema.Organizations + ".[Line]";

        #region Constructor

        private readonly IRepositoryAsync<SubsectionStructureDetail> _subsectionstructuredetailrepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPKGeneratorService _pkGeneratorService;
        private readonly ISqlRepository _sqlRepository;

        public SubsectionStructureDetailService(
            IRepositoryAsync<SubsectionStructureDetail> subsectionstructuredetailrepository,
            IUnitOfWork unitOfWork,
            IOperationService operationService,
            IPKGeneratorService pkGeneratorService
            , ISqlRepository sqlRepository
            ) :
            base(subsectionstructuredetailrepository, unitOfWork, pkGeneratorService)
        {
            _subsectionstructuredetailrepository = subsectionstructuredetailrepository;
            _unitOfWork = unitOfWork;
            _pkGeneratorService = pkGeneratorService;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public GridModel GetSearchData(GridParameter parameters)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                parameters.CmdText = @"SELECT	m.Id
		                            , m.AllotedManpower

		                            , m.IsLastOperation

		                            FROM TRN.[BulletinDetail] AS m left outer join
		                            " + _TN_Zone + @" z  ON z.Id=m.ZoneId left outer join
		                            " + _TN_Component + @" c  ON c.Id=m.ComponentId left outer join
		                            " + _TN_DesignationGroup + @" dg  ON dg.Id=m.DesignationgroupId  left outer join
		                            " + _TN_Operation + @" op ON op.Id=m.OperationId  left outer join
		                           " + _TN_MachineType + @"  mt ON mt.Id=m.MachineTypeId
		                            WHERE   m.Archive=0
				                            and m.Companygroupid='" + identity.CompanyGroupId + "'";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string GetPK()
        {
            return "SSD" + _pkGeneratorService.GetAutoNumber(nameof(SubsectionStructureDetail), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public SubsectionStructureDetail GetDetail(string PK)
        {
            var _sql = "select * from " + _TableName + " where Id='" + PK + "' and archive=0";
            return _subsectionstructuredetailrepository.SelectQuery(_sql, null).FirstOrDefault();
        }

        public IEnumerable<SubsectionStructureDetail> GetDetailList(string MasterId)
        {
            try
            {
                var _sql = "select * from " + _TableName + " where SubsectionStructureMasterId='" + MasterId + "'";
                return _subsectionstructuredetailrepository.SqlQuery<SubsectionStructureDetail>(_sql).AsEnumerable();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IEnumerable<object> GetList(string MasterId)
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            try
            {
                var _sql = @"select d.Id,dv.UserName Division,sd.UserName Subdivision,de.UserName Department,s.UserName Section,ss.UserName Subsection,l.UserName Line
                                    ,d.DepartmentId,d.DivisionId,d.LineId,d.SubsectionId,d.SectionId,d.Archive,d.SubdivisionId
                                    from " + _TableName + @" d
                                    left outer join  " + _SubSection + @" ss on ss.Id=d.SubsectionId
                                    left outer join  " + _Section + @" s on s.Id=d.SectionId
                                    left outer join  " + _Division + @" dv on dv.Id=d.DivisionId
                                    left outer join  org.subdivision sd on sd.Id=d.subdivisionid
                                    left outer join  " + _Department + @" de on de.Id=d.DepartmentId
                                    left outer join  " + _Line + @" l on l.Id=d.LineId
		                            WHERE   d.Archive=0
                                            and d.SubsectionStructureMasterId= '" + MasterId + "'";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}