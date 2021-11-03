#region Using

using Library.Core;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Employees;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#endregion Using

namespace Library.Service.Employees
{
    public class SOPAttachmentDetailService : Service<SOPAttachmentDetail>, ISOPAttachmentDetailService
    {
        #region Constructor

        private readonly ISqlRepository _sqlRepository;

        public SOPAttachmentDetailService(
            IRepositoryAsync<SOPAttachmentDetail> SOPAttachmentDetailRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(SOPAttachmentDetailRepository, unitOfWork, pkGeneratorService)
        {
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public void DeleteGraphBySOPItem(string sopItemId)
        {
            var db_List = base.Query(t => t.SOPItemId == sopItemId).Select(t => t.Id).ToList();
            if (null != db_List)
            {
                foreach (var item in db_List)
                {
                    DeleteGraph(item);
                }
            }
        }

        public void InsertGraph(IEnumerable<SOPAttachmentDetail> entities, string sopItemId)
        {
            try
            {
                if (entities != null)
                {
                    string id = CreatePk(sopItemId);
                    var count = id.ToInt();
                    foreach (var item in entities)
                    {
                        item.Id = sopItemId + "-" + count; count++;
                        item.FileId = item.Id;
                        item.SOPItemId = sopItemId;
                        base.InsertGraph(item);
                    }
                }
                //else
                //    throw new CustomException("Please select at least one attachment...........!");
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        private string CreatePk(string sopItemId)
        {
            try
            {
                string id = string.Empty;
                var Db_Pk = base.Query(t => t.SOPItemId == sopItemId).Select(t => t.Id).AsEnumerable();
                if (Db_Pk.Count() != 0)
                {
                    id = Db_Pk.Max(x => Convert.ToInt32(x.Substring(sopItemId.Length + 1)) + 1).ToString();
                }
                else
                {
                    id = "1";
                }
                return id;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public void UpdateGraph(IEnumerable<SOPAttachmentDetail> entities, string sopItemId)
        {
            try
            {
                if (sopItemId != null)
                {
                    string id = CreatePk(sopItemId);
                    var count = id.ToInt();
                    foreach (var item in entities)
                    {
                        item.Id = sopItemId + "-" + count;
                        item.SOPItemId = sopItemId;
                        count++;
                        base.InsertGraph(item);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }

        public GridModel Query(GridParameter parameters, string sopItemId)
        {
            try
            {
                parameters.CmdText = $"SELECT JDD.Id, JDD.FileName AS 'name', JDD.FileName, JDD.FileId, JDD.sopItemId FROM [HKP].[SOPAttachmentDetail] AS JDD" +
                    $" WHERE JDD.sopItemId='{sopItemId}' ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Employees.ToString()));
            }
        }
    }
}