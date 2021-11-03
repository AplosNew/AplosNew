using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Inventory;
using Library.Model.Products;
using Library.Model.Taxations;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using Library.ViewModel.OrderManagements;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;

namespace Library.Service.Inventory
{
	public class GRNAcceptanceMapService :Service<GRNAcceptanceMap>, IGRNAcceptanceMapService
	{

        #region Constructor

        private readonly ISqlRepository _sqlRepository;
        private readonly IRepositoryAsync<GateEntry> _gateEntryRepository;
        private readonly IRepositoryAsync<GRNAcceptanceMap> _grncceptanceMapRepository;
        private readonly IRepositoryAsync<MaterialRequsitionDetails> _materialRequsitionDetailsRepository;
        private readonly IUnitOfWork _unitOfWork;

        public GRNAcceptanceMapService( 
            IRepositoryAsync<GRNAcceptanceMap> grncceptanceMapRepository
            , IPKGeneratorService pkGeneratorService
            , IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            , IRepositoryAsync<MaterialRequsitionDetails> materialRequsitionDetailsRepository
            ) : base(grncceptanceMapRepository, unitOfWork, pkGeneratorService)
        {
            _grncceptanceMapRepository = grncceptanceMapRepository;
            _sqlRepository = sqlRepository;
            _unitOfWork = unitOfWork;
            _materialRequsitionDetailsRepository = materialRequsitionDetailsRepository;
        }

        #endregion Constructor
    }
}
