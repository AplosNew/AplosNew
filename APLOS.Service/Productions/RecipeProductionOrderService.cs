#region Using

using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Productions;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Productions
{
    public class RecipeProductionOrderService : Service<RecipeProductionOrder>, IRecipeProductionOrderService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;

        public RecipeProductionOrderService(
            IRepositoryAsync<RecipeProductionOrder> baseRepository
            ,IPKGeneratorService pkGeneratorService
            ,IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) :
            base(baseRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
        }

        #endregion Constructor
    }
}