#region Using

using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Enums;
using Library.Model.Products;
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

namespace Library.Service.Products
{
    public class ProductSubCategoryAttributeService : Service<ProductSubCategoryAttribute>, IProductSubCategoryAttributeService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<ProductSubCategoryAttribute> _charaterRepository;

        public ProductSubCategoryAttributeService(
            IRepositoryAsync<ProductSubCategoryAttribute> charaterRepository,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(charaterRepository, unitOfWork, pkGeneratorService)
        {
            _charaterRepository = charaterRepository;
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor

        public IEnumerable<object> GetSearchData(string productSubCategoryId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                string _sql = $"SELECT MA.UserName AS MaterialAttributeName, PSCA.* FROM {DbSchema.Masters}.[{DbTable.ProductSubCategoryAttribute}] as PSCA " +
                                     $"INNER JOIN {DbSchema.HKP}.[{DbTable.MaterialAttribute}] as MA ON MA.Id = PSCA.MaterialAttributeId " +
                                     $"WHERE PSCA.ProductSubCategoryId='{productSubCategoryId}' AND PSCA.Archive=0 ORDER BY PSCA.ProductSubCategoryId, PSCA.[Sequence]";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public void Insert(IEnumerable<ProductSubCategoryAttribute> entites)
        {
            var flag = false;
            try
            {
                _unitOfWork.BeginTransaction();
                flag = true;
                foreach (var item in entites)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        item.Id = GetPK();
                        InsertGraph(item);
                    }
                    else
                        UpdateGraph(item);
                }
                var productSubCategoryId = entites.First().ProductSubCategoryId;
                var dbList = base.Query(t => t.ProductSubCategoryId == productSubCategoryId).Select().AsEnumerable();
                if (dbList.Count() > 0)
                {
                    if (entites == null)
                    {
                        foreach (var item in dbList)
                        {
                            CheckIndividualDel(item.ProductSubCategoryId, item.MaterialAttributeId);
                            base.DeleteGraph(item);
                        }
                    }
                    else
                    {
                        foreach (var item in dbList)
                        {
                            if (!entites.Any(t => t.Id == item.Id))
                            {
                                CheckIndividualDel(item.ProductSubCategoryId, item.MaterialAttributeId);
                                base.DeleteGraph(item);
                            }
                        }
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (CustomException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
                                null, ErrorType.ServiceError, null,
                                ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private string GetPK()
        {
            return "PSCA" + GetAutoNumber(nameof(ProductSubCategoryAttribute), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Gets all items in this collection. </summary>
        /// <returns>
        /// An enumerator that allows foreach to be used to process all items in this collection.
        /// </returns>
        ///-------------------------------------------------------------------------------------------------
        public override IQueryFluent<ProductSubCategoryAttribute> Query()
        {
            return base.Query(r => !r.Archive);
        }

        //public IEnumerable<object> GetCbo()
        //{
        //    try
        //    {
        //        return from m in base.Query(m => !m.Archive)
        //               select new { Text = m.UserName, Value = m.Id };
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
        //                        null, ErrorType.ServiceError, null,
        //                        ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
        //    }
        //}

        //public IEnumerable<object> GetByProductSubCategoryAttributeIdCbo(string productSubCategoryId)
        //{
        //    try
        //    {
        //        return from m in base.Query(m => !m.Archive && m.ProductSubCategoryId == productSubCategoryId)
        //               select new { Text = m.UserName, Value = m.Id };
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new CustomException(ex.Message, ex,
        //            Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name,
        //                        null, ErrorType.ServiceError, null,
        //                        ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
        //    }
        //}

        public IEnumerable<object> GetAttribute(string productSubCategoryId, string productMasterId)
        {
            try
            {
                string _sql = @"SELECT PSCA.MaterialAttributeId AS MaterialAttributeId
		                                ,MA.UserName AS MaterialAttributeName
		                                ,PSCA.IsFreeField
		                                ,PSCA.IsPreDefinedField
		                                ,PSCA.IsMandatory
		                                ,PMAV.Id
		                                ,PMAV.ProductMasterId
		                                ,MaterialAttributeValueId =
                                                            CASE
                                                              WHEN (ISNULL(PMAV.Id, '') = '' AND MAV.IsDefault = 1) THEN MAV.Id
                                                              ELSE PMAV.MaterialAttributeValueId
                                                            END
		                                ,MaterialAttributeValueFreeText = CASE
										                                WHEN (ISNULL(PMAV.Id, '') = '' AND MAV.IsDefault = 1) THEN MAV.[Description]
										                                ELSE (ISNULL(PMAV.MaterialAttributeValueFreeText, '') + ISNULL(MAV2.[Description], ''))
										                                END
		                                ,'True' AS FlagDisable
                                FROM (SELECT * FROM MST.ProductSubCategoryAttribute WHERE ProductSubCategoryId = '" + productSubCategoryId + @"') AS PSCA
                                LEFT OUTER JOIN HKP.MaterialAttribute AS MA ON PSCA.MaterialAttributeId = MA.Id
                                LEFT OUTER JOIN (SELECT * FROM MST.ProductMasterAttributeValue WHERE Archive = 0 AND ProductMasterId = '" + productMasterId + @"') AS PMAV
				                                ON PMAV.MaterialAttributeId = MA.Id
                                LEFT JOIN (SELECT * FROM HKP.MaterialAttributeValue WHERE Active = 1 AND IsDefault = 1) AS MAV
				                                ON PSCA.MaterialAttributeId = MAV.MaterialAttributeId
                                LEFT JOIN (SELECT * FROM HKP.MaterialAttributeValue WHERE Active = 1) AS MAV2
				                                ON PMAV.MaterialAttributeValueId = MAV2.Id
                                ORDER BY PSCA.MaterialAttributeId, PSCA.[Sequence]";
                return _sqlRepository.GetDataCollection(_sql, null);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void DeleteGraph(string productSubCategoryId)
        {
            var flag = false;
            try
            {
                CheckIdUse(productSubCategoryId);
                _unitOfWork.BeginTransaction();
                flag = true;
                var entity = base.Query(t => t.ProductSubCategoryId == productSubCategoryId).Select().AsEnumerable();
                if (entity != null)
                {
                    foreach (var item in entity)
                    {
                        base.DeleteGraph(item);
                    }
                }
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        private void CheckIdUse(string productSubCategoryId)
        {
            string sql = @"IF EXISTS(SELECT 1 FROM(
                            SELECT ProductSubCategoryId AS CheckingColumn FROM MST.ProductMaster
                            ) A WHERE CheckingColumn='" + productSubCategoryId + "') SELECT 1 ELSE SELECT 0 RETURN ";
            var data = Convert.ToBoolean(_charaterRepository.SqlQuery<int>(sql).Single());
            if (data)
                throw new CustomException("This data already exist in product master, you can't delete....!");
        }

        private void CheckIndividualDel(string productSubCategoryId, string materialAttributeId)
        {
            string sql = @"SELECT MaterialAttribute FROM(
                            (SELECT Id,ProductSubCategoryId FROM MST.ProductMaster) AS A LEFT OUTER JOIN
                            (SELECT ProductMasterId, MaterialAttributeId, UserName AS MaterialAttribute FROM MST.ProductMasterAttributeValue AS PM
                            LEFT OUTER JOIN HKP.MaterialAttribute AS MA ON MA.Id=PM.MaterialAttributeId) AS B ON A.Id=B.ProductMasterId
                            ) WHERE A.ProductSubCategoryId='" + productSubCategoryId + "' AND B.MaterialAttributeId='" + materialAttributeId + @"'";
            var data = _charaterRepository.SqlQuery<string>(sql).FirstOrDefault();
            if (data != null)
                throw new CustomException("This [" + data + "] material attribute already exist in product master, you can't delete....!");
        }
    }
}