using Library.Data;
using Library.Data.Repositories;
using Library.Data.UnitOfWorks;
using Library.Model.Products;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Systems;
using System;
using System.Reflection;

namespace Library.Service.Products
{
    public class ProductMasterAttributeValueService : Service<ProductMasterAttributeValue>, IProductMasterAttributeValueService
    {
        #region Constructor

        public ProductMasterAttributeValueService(
            IRepositoryAsync<ProductMasterAttributeValue> productMasterAttributeValue,
            IPKGeneratorService pkGeneratorService,
            IUnitOfWork unitOfWork) :
            base(productMasterAttributeValue, unitOfWork, pkGeneratorService)
        {
        }

        #endregion Constructor

        public void DeleteGraph(string key)
        {
            try
            {
                var entity = Query(m => m.ProductMasterId == key && !m.Archive).Select();
                if (entity != null)
                {
                    foreach (var item in entity)
                    {
                        base.DeleteGraph(item);
                    }
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