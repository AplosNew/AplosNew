#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Materials;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Materials
{
    public class DefectCodeDetailService : Service<DefectCodeDetail>, IDefectCodeDetailService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public DefectCodeDetailService(
            IRepositoryAsync<DefectCodeDetail> charaterValueRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(charaterValueRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public GridModel Query(GridParameter parameters, string defectCodeId)
        {
            parameters.CmdText = @"SELECT DCD.Id
	                                      ,DCD.DefectCodeId
	                                      ,DCD.Zone
	                                      ,DCD.Point
	                                      ,DCD.Archive
	                                      ,FGZ.UserName AS ZoneName
                                  FROM [" + DbSchema.Masters + @"].[" + DbTable.DefectCodeDetail + @"] AS DCD
                                  LEFT OUTER JOIN HKP.FGZone AS FGZ ON DCD.Zone=FGZ.Id
                                  WHERE DefectCodeId='" + parameters.search + "'";

            return _sqlRepository.GetGridData(parameters);
        }

        public void Insert(IEnumerable<DefectCodeDetail> entity, string materialGridId, string[] deletedItems)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                var pkdetail = GetMaxNumber(nameof(DefectCode), PKGeneratorEnum.Auto, identity.CompanyGroupId, DateTime.Now);
                var Count = 0;
                foreach (var item in entity)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        Count++;
                        item.Id = pkdetail.MaxNumber + "-" + Count;
                        item.ModelState = ModelState.Added;
                    }
                    else
                    {
                        item.ModelState = ModelState.Modified;
                    }
                    InsertOrUpdateGraph(item);
                }
                if (deletedItems != null)
                {
                    foreach (var item in deletedItems)
                    {
                        var del = Find(item);
                        del.Archive = true;
                        del.ModelState = ModelState.Modified;
                        InsertOrUpdateGraph(del);
                    }
                }
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(DefectCodeDetail), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        public override void Update(DefectCodeDetail entity)
        {
            try
            {
                base.Update(entity);
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                entity.AddedBy, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public void DeleteGraph(string defectCodeId)
        {
            try
            {
                var list = base.Query(t => t.DefectCodeId == defectCodeId).Select().AsEnumerable();
                foreach (var item in list)
                {
                    base.DeleteGraph(item);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public void DeleteGraph(string[] deletedItems)
        {
            try
            {
                var list = base.Query(t => deletedItems.Contains(t.Id)).Select().AsEnumerable();
                foreach (var item in list)
                {
                    base.DeleteGraph(item);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }
    }
}