#region Using

using Library.Core;
using Library.Crosscutting.Security;
using Library.Data;
using Library.Data.Repositories;
using Library.Data.Sql;
using Library.Data.UnitOfWorks;
using Library.Model.Materials;
using Library.Model.Systems;
using Library.Service.Core;
using Library.Service.Enums;
using Library.Service.Extension;
using Library.Service.Helpers;
using Library.Service.Logs;
using Library.Service.Properties;
using Library.Service.Systems;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;

#endregion Using

namespace Library.Service.Materials
{
    public class ServiceGroupGLService : Service<ServiceGroupGL>, IServiceGroupGLService
    {
        #region Constructor

        private readonly IUnitOfWork _unitOfWork;
        private readonly ISqlRepository _sqlRepository;
        private readonly IServiceGroupPartyAccountGroupGLService _serviceGroupPartyAccountGroupGLService;

        public ServiceGroupGLService(
            IRepositoryAsync<ServiceGroupGL> FixedAssetClassRepository,
            IPKGeneratorService pkGeneratorService,
            IServiceGroupPartyAccountGroupGLService serviceGroupPartyAccountGroupGLService,
            IUnitOfWork unitOfWork
            , ISqlRepository sqlRepository
            ) : base(FixedAssetClassRepository, unitOfWork, pkGeneratorService)
        {
            _unitOfWork = unitOfWork;
            _sqlRepository = sqlRepository;
            _serviceGroupPartyAccountGroupGLService = serviceGroupPartyAccountGroupGLService;
        }

     

        #endregion Constructor

        public void InsertOrUpdate(string masterId, ServiceGroupGL entity)
        {
            try
            {
                if (string.IsNullOrEmpty(entity.Id))
                {
                    entity.Id = GetPK();
                    entity.ServiceGroupId = masterId;
                    InsertGraph(entity);
                }
                else
                {
                    UpdateGraph(entity);
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, entity.AddedBy,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        private string GetPK()
        {
            return GetAutoNumber(nameof(ServiceGroupGL), PKGeneratorEnum.Yearly, null, DateTime.Now);
        }

        private PKGenerator GetMaxNumber()
        {
            return base.GetMaxNumber(nameof(ServiceGroupGL), PKGeneratorEnum.Auto, null, DateTime.Now);
        }

        #region ServiceGroupGL

        public void InsertUpdateServiceGroupDeterminate(IEnumerable<ServiceGroupGL> entities, IEnumerable<ServiceGroupPartyAccountGroupGL> serviceGroupPartyAccountGroupGL)
        {
            var flag = false;
            try
            {
                var pk = GetMaxNumber();
                foreach (var item in entities)
                {
                    if (string.IsNullOrEmpty(item.Id))
                    {
                        item.ModelState = ModelState.Added;
                        pk.MaxNumber++;
                        //log
                        AuditService.Log(item);
                        item.Id = pk.MaxNumber.ToString();
                    }
                    else
                    {
                        item.ModelState = ModelState.Modified;
                        AuditService.Log(item);
                        //log
                    }

                    InsertOrUpdateGraph(item);
                }

                _serviceGroupPartyAccountGroupGLService.InsertOrUpdate(entities, serviceGroupPartyAccountGroupGL);

                _unitOfWork.BeginTransaction();
                flag = true;
                _unitOfWork.SaveChanges();
                flag = false;
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Machine.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public void DeleteGraph(string id)
        {
            var flag = false;
            try
            {
                if (string.IsNullOrEmpty(id))
                    throw new CustomException(string.Format(ResourcesCore.IsNull, "ServiceGroup Id"));

                _unitOfWork.BeginTransaction();
                flag = true;
                ServiceGroupGL entity = Find(id);
                // If section row inactive
                _serviceGroupPartyAccountGroupGLService.DeleteGraph(entity.Id);
                base.DeleteGraph(entity);
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
                throw new CustomException(ex.Message, ex, Logger.ThrowError(GetType().Name,
                    MethodBase.GetCurrentMethod().Name, null, ErrorType.ServiceError,
                    null, ex.Message, ex.GetType().Name, false, ModuleEnum.Organization.ToString()));
            }
            finally
            {
                if (flag)
                    _unitOfWork.Rollback();
            }
        }

        public GridModel GetDataByServiceGroupId(GridParameter parameters, string ServiceGroupId, string coaId)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                if (ServiceGroupId != null)
                {
                    parameters.CmdText = @"SELECT MAD.*
								,GLGI1.AccountCode + ' - ' + GLGI1.UserName AS DownPaymentGLInfo
								,GLGI2.AccountCode + ' - ' + GLGI2.UserName AS ClearingAccountGLInfo
								,GLGI3.AccountCode + ' - ' + GLGI3.UserName AS InventoryGLInfo
								,GLGI4.AccountCode + ' - ' + GLGI4.UserName AS ExpenseGLInfo
                                 FROM [HKP].[ServiceGroupGL] MAD
                                 LEFT OUTER JOIN [HKP].[ServiceGroup] MGM ON MAD.ServiceGroupId = MAD.Id
							  LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI1 ON GLGI1.Id=MAD.DownPaymentGLId
        			          LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI2 ON GLGI2.Id=MAD.ClearingAccountGLId
        			          LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI3 ON GLGI3.Id=MAD.ServiceGLId
        			          LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=MAD.ExpenseGLId
                            WHERE MAD.ServiceGroupId='" + ServiceGroupId + @"' AND MAD.COAId='" + coaId + @"'  ";
                }
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Bank.ToString()));
            }
        }

        public GridModel GetSearchWithCombine(GridParameter parameters, string coaId)
        {
            try
            {
                string coaStr = "where isnull(c.Id,'') =''";
                if (coaId != "null")
                    coaStr = "where isnull(c.Id,'') ='" + coaId + @"'";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT F.Id ,MGM.Id AS ServiceGroupId
                                          ,MGM.UserName AS ServiceGroupName
                                         ,MT.UserName AS ServiceTypeName
										 ,MGM.ServiceTypeId
										 ,F.DownPaymentGLId
										 ,F.ClearingAccountGLId
        		                         ,F.COAId
        		                         ,F.UserName 'COAName'
                                         ,F.DownPaymentGLInfo
                                         ,F.ClearingAccountGLInfo
					                     ,F.DownPaymentBudgetMasterId
					                     ,F.DownPaymentActivityId
					                     ,F.DownPaymentBudgetName
					                     ,F.DownPaymentActivityName
					                     ,F.ClearingAccountBudgetMasterId
					                     ,F.ClearingAccountActivityId
					                     ,F.ClearingAccountBudgetName
					                     ,F.ClearingAccountActivityName
                                         ,F.ServiceGLInfo
                                         ,F.ServiceGLId
                                         ,F.ServiceBudgetMasterId
					                     ,F.ServiceActivityId
					                     ,F.ServiceBudgetName
					                     ,F.ServiceActivityName
                                         ,F.ExpenseGLInfo
                                         ,F.ExpenseGLId
                                         ,F.ExpenseBudgetMasterId
					                     ,F.ExpenseActivityId
					                     ,F.ExpenseBudgetName
					                     ,F.ExpenseActivityName
                                            FROM HKP.ServiceGroup As MGM
                                            LEFT OUTER JOIN HKP.ServiceType As MT ON MT.Id = MGM.ServiceTypeId
                                            LEFT OUTER JOIN (select
        			MAD.Id,MAD.ServiceGroupId,
        			c.Id AS COAId,GLGI1.AccountCode
        			,MAD.DownPaymentGLId,MAD.ClearingAccountGLId
                    ,MAD.ServiceGLId,MAD.ExpenseGLId
        			,C.UserName
					,GLGI1.AccountCode + ' - ' + GLGI1.UserName AS DownPaymentGLInfo
					,GLGI2.AccountCode + ' - ' + GLGI2.UserName AS ClearingAccountGLInfo
					,GLGI3.AccountCode + ' - ' + GLGI3.UserName AS ServiceGLInfo
					,GLGI4.AccountCode + ' - ' + GLGI4.UserName AS ExpenseGLInfo
        			,MAD.DownPaymentBudgetMasterId
                    ,MAD.DownPaymentActivityId
					,DPB.UserName AS DownPaymentBudgetName
					,DPA.UserName AS DownPaymentActivityName
        			,MAD.ClearingAccountBudgetMasterId
                    ,MAD.ClearingAccountActivityId
					,CAB.UserName AS ClearingAccountBudgetName
					,CAA.UserName AS ClearingAccountActivityName
        			,MAD.ServiceBudgetMasterId
                    ,MAD.ServiceActivityId
        			,MAD.ExpenseBudgetMasterId
                    ,MAD.ExpenseActivityId
					,IB.UserName AS ServiceBudgetName
					,IA.UserName AS ServiceActivityName
					,EB.UserName AS ExpenseBudgetName
					,EA.UserName AS ExpenseActivityName
        			from HKP.COA c
        			LEFT OUTER JOIN HKP.ServiceGroupGL AS MAD ON MAD.COAId=c.Id
        			LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI1 ON GLGI1.Id=MAD.DownPaymentGLId
        			LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI2 ON GLGI2.Id=MAD.ClearingAccountGLId
        			LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI3 ON GLGI3.Id=MAD.ServiceGLId
        			LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=MAD.ExpenseGLId

					LEFT OUTER JOIN MST.BudgetMaster AS DPBM ON MAD.DownPaymentBudgetMasterId = DPBM.Id
					LEFT OUTER JOIN HKP.Budget AS DPB ON DPBM.BudgetId = DPB.Id
					LEFT OUTER JOIN HKP.Activity AS DPA ON MAD.DownPaymentActivityId = DPA.Id

					LEFT OUTER JOIN MST.BudgetMaster AS CABM ON MAD.ClearingAccountBudgetMasterId = CABM.Id
					LEFT OUTER JOIN HKP.Budget AS CAB ON CABM.BudgetId = CAB.Id
					LEFT OUTER JOIN HKP.Activity AS CAA ON MAD.ClearingAccountActivityId = CAA.Id

					LEFT OUTER JOIN MST.BudgetMaster AS IBM ON MAD.ServiceBudgetMasterId = IBM.Id
					LEFT OUTER JOIN HKP.Budget AS IB ON IBM.BudgetId = IB.Id
					LEFT OUTER JOIN HKP.Activity AS IA ON MAD.ServiceActivityId = IA.Id

					LEFT OUTER JOIN MST.BudgetMaster AS EBM ON MAD.ExpenseBudgetMasterId = EBM.Id
					LEFT OUTER JOIN HKP.Budget AS EB ON EBM.BudgetId = EB.Id
					LEFT OUTER JOIN HKP.Activity AS EA ON MAD.ExpenseActivityId = EA.Id
											" + coaStr + @"
											)AS F ON F.ServiceGroupId = MGM.Id ";

                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        private string ServiceGroupGlSql()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return @" DECLARE @sql_ nvarchar(max)

                                    select ServiceGroupId,
									ServiceGroupName,
									ServiceTypeName,COAName
									,DownPaymentGLInfo								
									,ClearingAccountGLInfo,DownPaymentBudgetName
									,DownPaymentActivityName,ClearingAccountBudgetName,
									ClearingAccountActivityName,ServiceGLInfo,
									ServiceBudgetName,ServiceActivityName
																
									,ExpenseGLInfo,
									ExpenseBudgetName,	
									ExpenseActivityName
																
									,GL,
							
									PartyAccountGroup +'GL' PartyAccountGroup
                                    INTO #tempOT
                                    from 
                                    (
                                    SELECT A.* FROM
	                                (

                                    SELECT F.Id ,MGM.Id AS ServiceGroupId
                                          ,MGM.UserName AS ServiceGroupName
                                         ,MT.UserName AS ServiceTypeName
										 
        		                         
        		                         ,F.UserName 'COAName'
                                         ,F.DownPaymentGLInfo
                                         ,F.ClearingAccountGLInfo
					                    
					                     ,F.DownPaymentBudgetName
					                     ,F.DownPaymentActivityName
					                     
					                     ,F.ClearingAccountBudgetName
					                     ,F.ClearingAccountActivityName
                                         ,F.ServiceGLInfo
                                        
					                     ,F.ServiceBudgetName
					                     ,F.ServiceActivityName
                                         ,F.ExpenseGLInfo
                                         
                                         
					                     ,F.ExpenseActivityId
					                     ,F.ExpenseBudgetName
					                     ,F.ExpenseActivityName
										 ,MGGL.GL,MGGL.PartyAccountGroup
                                            FROM HKP.ServiceGroup As MGM
                                            LEFT OUTER JOIN HKP.ServiceType As MT ON MT.Id = MGM.ServiceTypeId
                                            LEFT OUTER JOIN (select
        			MAD.Id,MAD.ServiceGroupId,
        			c.Id AS COAId,GLGI1.AccountCode
        			,MAD.DownPaymentGLId,MAD.ClearingAccountGLId
                    ,MAD.ServiceGLId,MAD.ExpenseGLId
        			,C.UserName
					,GLGI1.AccountCode + ' - ' + GLGI1.UserName AS DownPaymentGLInfo
					,GLGI2.AccountCode + ' - ' + GLGI2.UserName AS ClearingAccountGLInfo
					,GLGI3.AccountCode + ' - ' + GLGI3.UserName AS ServiceGLInfo
					,GLGI4.AccountCode + ' - ' + GLGI4.UserName AS ExpenseGLInfo
        			,MAD.DownPaymentBudgetMasterId
                    ,MAD.DownPaymentActivityId
					,DPB.UserName AS DownPaymentBudgetName
					,DPA.UserName AS DownPaymentActivityName
        			,MAD.ClearingAccountBudgetMasterId
                    ,MAD.ClearingAccountActivityId
					,CAB.UserName AS ClearingAccountBudgetName
					,CAA.UserName AS ClearingAccountActivityName
        			,MAD.ServiceBudgetMasterId
                    ,MAD.ServiceActivityId
        			,MAD.ExpenseBudgetMasterId
                    ,MAD.ExpenseActivityId
					,IB.UserName AS ServiceBudgetName
					,IA.UserName AS ServiceActivityName
					,EB.UserName AS ExpenseBudgetName
					,EA.UserName AS ExpenseActivityName
        			from HKP.COA c
        			LEFT OUTER JOIN HKP.ServiceGroupGL AS MAD ON MAD.COAId=c.Id
        			LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI1 ON GLGI1.Id=MAD.DownPaymentGLId
        			LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI2 ON GLGI2.Id=MAD.ClearingAccountGLId
        			LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI3 ON GLGI3.Id=MAD.ServiceGLId
        			LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=MAD.ExpenseGLId
					LEFT OUTER JOIN MST.BudgetMaster AS DPBM ON MAD.DownPaymentBudgetMasterId = DPBM.Id
					LEFT OUTER JOIN HKP.Budget AS DPB ON DPBM.BudgetId = DPB.Id
					LEFT OUTER JOIN HKP.Activity AS DPA ON MAD.DownPaymentActivityId = DPA.Id

					LEFT OUTER JOIN MST.BudgetMaster AS CABM ON MAD.ClearingAccountBudgetMasterId = CABM.Id
					LEFT OUTER JOIN HKP.Budget AS CAB ON CABM.BudgetId = CAB.Id
					LEFT OUTER JOIN HKP.Activity AS CAA ON MAD.ClearingAccountActivityId = CAA.Id

					LEFT OUTER JOIN MST.BudgetMaster AS IBM ON MAD.ServiceBudgetMasterId = IBM.Id
					LEFT OUTER JOIN HKP.Budget AS IB ON IBM.BudgetId = IB.Id
					LEFT OUTER JOIN HKP.Activity AS IA ON MAD.ServiceActivityId = IA.Id

					LEFT OUTER JOIN MST.BudgetMaster AS EBM ON MAD.ExpenseBudgetMasterId = EBM.Id
					LEFT OUTER JOIN HKP.Budget AS EB ON EBM.BudgetId = EB.Id
					LEFT OUTER JOIN HKP.Activity AS EA ON MAD.ExpenseActivityId = EA.Id
											
											)AS F ON F.ServiceGroupId = MGM.Id

					
					LEFT JOIN ( SELECT MGL.Id,MGL.ServiceGroupGLId,GL.UserName GL
					,MGL.GLType,MGL.PartyAccountGroupId,PAG.UserName PartyAccountGroup
					FROM HKP.ServiceGroupPartyAccountGroupGL MGL
					LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=MGL.PartyAccountGroupId
					LEFT JOIN HKP.GLGeneralInfo AS GL ON GL.Id=MGL.GLGeneralInfoId
					
					) MGGL ON MGGL.ServiceGroupGLId=MGM.Id
					) A
					group by			 a.Id,a.ServiceGroupId,
										a.ServiceGroupName,
										a.ServiceTypeName,
										a.COAName,a.DownPaymentGLInfo
										
										,a.ClearingAccountGLInfo
										,a.DownPaymentBudgetName
										,a.DownPaymentActivityName
										,ClearingAccountBudgetName
										,a.ClearingAccountActivityName
										,a.ExpenseActivityId
										,a.ServiceGLInfo
										,a.ServiceBudgetName

										,a.ServiceActivityName
										,a.ExpenseGLInfo
										,a.ExpenseBudgetName
										,a.ExpenseActivityName
										,a.PartyAccountGroup
										,a.GL
										
										
                            ) TT
	                            DECLARE @sql nvarchar(max),
                                    @col nvarchar(max),
                                    @col1 nvarchar(max)

                            SELECT @col = (
                                SELECT DISTINCT ','+QUOTENAME(PartyAccountGroup)	
                                FROM #tempOT 
                                FOR XML PATH ('')
                            )
							

                            SELECT @sql = N'
                            (SELECT *
                            FROM #tempOT
                            PIVOT (
                                MAX([GL]) FOR [PartyAccountGroup] IN ('+STUFF(@col,1,1,'')+')
								 
                            ) as pvt)'
				
                            EXEC sp_executesql @sql
                            drop table #tempOT
                            
                            ";

        }
        private string ServiceGroupBudgetSql()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return @"DECLARE @sql_ nvarchar(max)

                                    select ServiceGroupId
									
																
									,Budget,
							
									PartyAccountGroup +'Budget' PartyAccountGroup
                                    INTO #tempOT
                                    from 
                                    (
                                    SELECT A.* FROM
	                                (

                                    SELECT F.Id ,MGM.Id AS ServiceGroupId
                                          ,MGM.UserName AS ServiceGroupName
                                         ,MT.UserName AS ServiceTypeName
										 
        		                         
        		                         ,F.UserName 'COAName'
                                         ,F.DownPaymentGLInfo
                                         ,F.ClearingAccountGLInfo
					                    
					                     ,F.DownPaymentBudgetName
					                     ,F.DownPaymentActivityName
					                     
					                     ,F.ClearingAccountBudgetName
					                     ,F.ClearingAccountActivityName
                                         ,F.ServiceGLInfo
                                        
					                     ,F.ServiceBudgetName
					                     ,F.ServiceActivityName
                                         ,F.ExpenseGLInfo
                                         
                                         
					                     ,F.ExpenseActivityId
					                     ,F.ExpenseBudgetName
					                     ,F.ExpenseActivityName
										 ,MGGL.Budget,MGGL.PartyAccountGroup
                                            FROM HKP.ServiceGroup As MGM
                                            LEFT OUTER JOIN HKP.ServiceType As MT ON MT.Id = MGM.ServiceTypeId
                                            LEFT OUTER JOIN (select
        			MAD.Id,MAD.ServiceGroupId,
        			c.Id AS COAId,GLGI1.AccountCode
        			,MAD.DownPaymentGLId,MAD.ClearingAccountGLId
                    ,MAD.ServiceGLId,MAD.ExpenseGLId
        			,C.UserName
					,GLGI1.AccountCode + ' - ' + GLGI1.UserName AS DownPaymentGLInfo
					,GLGI2.AccountCode + ' - ' + GLGI2.UserName AS ClearingAccountGLInfo
					,GLGI3.AccountCode + ' - ' + GLGI3.UserName AS ServiceGLInfo
					,GLGI4.AccountCode + ' - ' + GLGI4.UserName AS ExpenseGLInfo
        			,MAD.DownPaymentBudgetMasterId
                    ,MAD.DownPaymentActivityId
					,DPB.UserName AS DownPaymentBudgetName
					,DPA.UserName AS DownPaymentActivityName
        			,MAD.ClearingAccountBudgetMasterId
                    ,MAD.ClearingAccountActivityId
					,CAB.UserName AS ClearingAccountBudgetName
					,CAA.UserName AS ClearingAccountActivityName
        			,MAD.ServiceBudgetMasterId
                    ,MAD.ServiceActivityId
        			,MAD.ExpenseBudgetMasterId
                    ,MAD.ExpenseActivityId
					,IB.UserName AS ServiceBudgetName
					,IA.UserName AS ServiceActivityName
					,EB.UserName AS ExpenseBudgetName
					,EA.UserName AS ExpenseActivityName
        			from HKP.COA c
        			LEFT OUTER JOIN HKP.ServiceGroupGL AS MAD ON MAD.COAId=c.Id
        			LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI1 ON GLGI1.Id=MAD.DownPaymentGLId
        			LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI2 ON GLGI2.Id=MAD.ClearingAccountGLId
        			LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI3 ON GLGI3.Id=MAD.ServiceGLId
        			LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=MAD.ExpenseGLId
					LEFT OUTER JOIN MST.BudgetMaster AS DPBM ON MAD.DownPaymentBudgetMasterId = DPBM.Id
					LEFT OUTER JOIN HKP.Budget AS DPB ON DPBM.BudgetId = DPB.Id
					LEFT OUTER JOIN HKP.Activity AS DPA ON MAD.DownPaymentActivityId = DPA.Id

					LEFT OUTER JOIN MST.BudgetMaster AS CABM ON MAD.ClearingAccountBudgetMasterId = CABM.Id
					LEFT OUTER JOIN HKP.Budget AS CAB ON CABM.BudgetId = CAB.Id
					LEFT OUTER JOIN HKP.Activity AS CAA ON MAD.ClearingAccountActivityId = CAA.Id

					LEFT OUTER JOIN MST.BudgetMaster AS IBM ON MAD.ServiceBudgetMasterId = IBM.Id
					LEFT OUTER JOIN HKP.Budget AS IB ON IBM.BudgetId = IB.Id
					LEFT OUTER JOIN HKP.Activity AS IA ON MAD.ServiceActivityId = IA.Id

					LEFT OUTER JOIN MST.BudgetMaster AS EBM ON MAD.ExpenseBudgetMasterId = EBM.Id
					LEFT OUTER JOIN HKP.Budget AS EB ON EBM.BudgetId = EB.Id
					LEFT OUTER JOIN HKP.Activity AS EA ON MAD.ExpenseActivityId = EA.Id
											
											)AS F ON F.ServiceGroupId = MGM.Id

					
					LEFT JOIN ( SELECT MGL.Id,MGL.ServiceGroupGLId,B.UserName Budget
					,MGL.GLType,MGL.PartyAccountGroupId,PAG.UserName PartyAccountGroup
					FROM HKP.ServiceGroupPartyAccountGroupGL MGL
					LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=MGL.PartyAccountGroupId
					
					LEFT JOIN MST.BudgetMaster AS BM ON MGL.BudgetMasterId = BM.Id
					LEFT JOIN HKP.Budget B ON B.Id=BM.BudgetId
					

					) MGGL ON MGGL.ServiceGroupGLId=MGM.Id
					) A
					group by			 a.Id,a.ServiceGroupId,
										a.ServiceGroupName,
										a.ServiceTypeName,
										a.COAName,a.DownPaymentGLInfo
										
										,a.ClearingAccountGLInfo
										,a.DownPaymentBudgetName
										,a.DownPaymentActivityName
										,ClearingAccountBudgetName
										,a.ClearingAccountActivityName
										,a.ExpenseActivityId
										,a.ServiceGLInfo
										,a.ServiceBudgetName

										,a.ServiceActivityName
										,a.ExpenseGLInfo
										,a.ExpenseBudgetName
										,a.ExpenseActivityName
										,a.PartyAccountGroup
										,a.Budget
										
										
                            ) TT
	                            DECLARE @sql nvarchar(max),
                                    @col nvarchar(max),
                                    @col1 nvarchar(max)

                            SELECT @col = (
                                SELECT DISTINCT ','+QUOTENAME(PartyAccountGroup)	
                                FROM #tempOT 
                                FOR XML PATH ('')
                            )
							

                            SELECT @sql = N'
                            (SELECT *
                            FROM #tempOT
                            PIVOT (
                                MAX([Budget]) FOR [PartyAccountGroup] IN ('+STUFF(@col,1,1,'')+')
								 
                            ) as pvt)'
				
                            EXEC sp_executesql @sql
                            drop table #tempOT
                            
                            
                            ";

        }
        private string ServiceGroupActivitySql()
        {
            var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;

            return @"DECLARE @sql_ nvarchar(max)

                                    select ServiceGroupId
									
																
									,Activity,
							
									PartyAccountGroup +'Activity' PartyAccountGroup
                                    INTO #tempOT
                                    from 
                                    (
                                    SELECT A.* FROM
	                                (

                                    SELECT F.Id ,MGM.Id AS ServiceGroupId
                                          ,MGM.UserName AS ServiceGroupName
                                         ,MT.UserName AS ServiceTypeName
										 
        		                         
        		                         ,F.UserName 'COAName'
                                         ,F.DownPaymentGLInfo
                                         ,F.ClearingAccountGLInfo
					                    
					                     ,F.DownPaymentBudgetName
					                     ,F.DownPaymentActivityName
					                     
					                     ,F.ClearingAccountBudgetName
					                     ,F.ClearingAccountActivityName
                                         ,F.ServiceGLInfo
                                        
					                     ,F.ServiceBudgetName
					                     ,F.ServiceActivityName
                                         ,F.ExpenseGLInfo
                                         
                                         
					                     ,F.ExpenseActivityId
					                     ,F.ExpenseBudgetName
					                     ,F.ExpenseActivityName
										 ,MGGL.Activity,MGGL.PartyAccountGroup
                                            FROM HKP.ServiceGroup As MGM
                                            LEFT OUTER JOIN HKP.ServiceType As MT ON MT.Id = MGM.ServiceTypeId
                                            LEFT OUTER JOIN (select
        			MAD.Id,MAD.ServiceGroupId,
        			c.Id AS COAId,GLGI1.AccountCode
        			,MAD.DownPaymentGLId,MAD.ClearingAccountGLId
                    ,MAD.ServiceGLId,MAD.ExpenseGLId
        			,C.UserName
					,GLGI1.AccountCode + ' - ' + GLGI1.UserName AS DownPaymentGLInfo
					,GLGI2.AccountCode + ' - ' + GLGI2.UserName AS ClearingAccountGLInfo
					,GLGI3.AccountCode + ' - ' + GLGI3.UserName AS ServiceGLInfo
					,GLGI4.AccountCode + ' - ' + GLGI4.UserName AS ExpenseGLInfo
        			,MAD.DownPaymentBudgetMasterId
                    ,MAD.DownPaymentActivityId
					,DPB.UserName AS DownPaymentBudgetName
					,DPA.UserName AS DownPaymentActivityName
        			,MAD.ClearingAccountBudgetMasterId
                    ,MAD.ClearingAccountActivityId
					,CAB.UserName AS ClearingAccountBudgetName
					,CAA.UserName AS ClearingAccountActivityName
        			,MAD.ServiceBudgetMasterId
                    ,MAD.ServiceActivityId
        			,MAD.ExpenseBudgetMasterId
                    ,MAD.ExpenseActivityId
					,IB.UserName AS ServiceBudgetName
					,IA.UserName AS ServiceActivityName
					,EB.UserName AS ExpenseBudgetName
					,EA.UserName AS ExpenseActivityName
        			from HKP.COA c
        			LEFT OUTER JOIN HKP.ServiceGroupGL AS MAD ON MAD.COAId=c.Id
        			LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI1 ON GLGI1.Id=MAD.DownPaymentGLId
        			LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI2 ON GLGI2.Id=MAD.ClearingAccountGLId
        			LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI3 ON GLGI3.Id=MAD.ServiceGLId
        			LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=MAD.ExpenseGLId
					LEFT OUTER JOIN MST.BudgetMaster AS DPBM ON MAD.DownPaymentBudgetMasterId = DPBM.Id
					LEFT OUTER JOIN HKP.Budget AS DPB ON DPBM.BudgetId = DPB.Id
					LEFT OUTER JOIN HKP.Activity AS DPA ON MAD.DownPaymentActivityId = DPA.Id

					LEFT OUTER JOIN MST.BudgetMaster AS CABM ON MAD.ClearingAccountBudgetMasterId = CABM.Id
					LEFT OUTER JOIN HKP.Budget AS CAB ON CABM.BudgetId = CAB.Id
					LEFT OUTER JOIN HKP.Activity AS CAA ON MAD.ClearingAccountActivityId = CAA.Id

					LEFT OUTER JOIN MST.BudgetMaster AS IBM ON MAD.ServiceBudgetMasterId = IBM.Id
					LEFT OUTER JOIN HKP.Budget AS IB ON IBM.BudgetId = IB.Id
					LEFT OUTER JOIN HKP.Activity AS IA ON MAD.ServiceActivityId = IA.Id

					LEFT OUTER JOIN MST.BudgetMaster AS EBM ON MAD.ExpenseBudgetMasterId = EBM.Id
					LEFT OUTER JOIN HKP.Budget AS EB ON EBM.BudgetId = EB.Id
					LEFT OUTER JOIN HKP.Activity AS EA ON MAD.ExpenseActivityId = EA.Id
											
											)AS F ON F.ServiceGroupId = MGM.Id

					
					LEFT JOIN ( SELECT MGL.Id,MGL.ServiceGroupGLId,A.UserName Activity
					,MGL.GLType,MGL.PartyAccountGroupId,PAG.UserName PartyAccountGroup
					FROM HKP.ServiceGroupPartyAccountGroupGL MGL
					LEFT JOIN HKP.PartyAccountGroup PAG ON PAG.Id=MGL.PartyAccountGroupId

					LEFT JOIN HKP.Activity A ON A.Id=MGL.ActivityId

					) MGGL ON MGGL.ServiceGroupGLId=MGM.Id
					) A
					group by			 a.Id,a.ServiceGroupId,
										a.ServiceGroupName,
										a.ServiceTypeName,
										a.COAName,a.DownPaymentGLInfo
										
										,a.ClearingAccountGLInfo
										,a.DownPaymentBudgetName
										,a.DownPaymentActivityName
										,ClearingAccountBudgetName
										,a.ClearingAccountActivityName
										,a.ExpenseActivityId
										,a.ServiceGLInfo
										,a.ServiceBudgetName

										,a.ServiceActivityName
										,a.ExpenseGLInfo
										,a.ExpenseBudgetName
										,a.ExpenseActivityName
										,a.PartyAccountGroup
										,a.Activity
										
										
                            ) TT
	                            DECLARE @sql nvarchar(max),
                                    @col nvarchar(max),
                                    @col1 nvarchar(max)

                            SELECT @col = (
                                SELECT DISTINCT ','+QUOTENAME(PartyAccountGroup)	
                                FROM #tempOT 
                                FOR XML PATH ('')
                            )
							

                            SELECT @sql = N'
                            (SELECT *
                            FROM #tempOT
                            PIVOT (
                                MAX([Activity]) FOR [PartyAccountGroup] IN ('+STUFF(@col,1,1,'')+')
								 
                            ) as pvt)'
				
                            EXEC sp_executesql @sql
                            drop table #tempOT
                            
                            
                            ";

        }


        public void ServiceGroupGlReport()
        {
             try
            { 
               
                ExcelEngine excelEngine = new ExcelEngine();
                //Instantiate the Excel application object
                IApplication application = excelEngine.Excel;

                //Set the default application version
                application.DefaultVersion = ExcelVersion.Excel2013;
                IWorkbook workbook = application.Workbooks.Create(1);
                IWorksheet sheet = workbook.Worksheets[0];

                sheet.Name = "Service Group Report";

                DataTable dtServiceGroupGlReport = _sqlRepository.GetDataTable(ServiceGroupGlSql());
                DataTable dtServiceGroupBudgetReport = _sqlRepository.GetDataTable(ServiceGroupBudgetSql());
                DataTable dtServiceGroupActivityReport = _sqlRepository.GetDataTable(ServiceGroupActivitySql());



                int ROW = 6;
                int COL = 1;


                sheet[ROW, COL].Text = "Sl No.";

                sheet[ROW, COL].ColumnWidth = 3;
                int colSlNo = COL;
                COL++;

               
                sheet[ROW, COL].Text = "Service Group";
                sheet[ROW, COL].ColumnWidth = 15;
                int colServiceGroupName = COL;
                COL++;

                sheet[ROW, COL].Text = "Service Type";
                sheet[ROW, COL].ColumnWidth = 15;
                int colServiceTypeName = COL;
                COL++;
              
               
                sheet[ROW, COL].Text = "Down Payment GL Info";
                sheet[ROW, COL].ColumnWidth = 15;
                int colDownPaymentGLInfo = COL;
                COL++;
                sheet[ROW, COL].Text = "Down Payment Budget";
                sheet[ROW, COL].ColumnWidth = 15;
                int colDownPaymentBudgetName = COL;
                COL++;
                sheet[ROW, COL].Text = "Down Payment Activity";      
                sheet[ROW, COL].ColumnWidth = 15;
                int colDownPaymentActivityName = COL;
                COL++;
                sheet[ROW, COL].Text = "Clearing Account GL Info";
                sheet[ROW, COL].ColumnWidth = 15;
                int colClearingAccountGLInfo = COL;
                COL++;
                sheet[ROW, COL].Text = "Clearing Account Budget";
                sheet[ROW, COL].ColumnWidth = 15;
                int colClearingAccountBudgetName = COL;
                COL++;
                sheet[ROW, COL].Text = "Clearing Account Activity";
                sheet[ROW, COL].ColumnWidth = 15;
                int colClearingAccountActivityName = COL;
                COL++;
                sheet[ROW, COL].Text = "Service GL Info";
                sheet[ROW, COL].ColumnWidth = 15;
                int colServiceGLInfo = COL;
                COL++;
                sheet[ROW, COL].Text = "Service Budget";
                sheet[ROW, COL].ColumnWidth = 15;
                int colServiceBudgetName = COL;
                COL++;
                sheet[ROW, COL].Text = "Service Activity";
                sheet[ROW, COL].ColumnWidth = 15;
                int colServiceActivityName = COL;
                COL++;
                sheet[ROW, COL].Text = "Expense GL Info";
                sheet[ROW, COL].ColumnWidth = 15;
                int colExpenseGLInfo = COL;
                COL++;
                sheet[ROW, COL].Text = "Expense Budget";
                sheet[ROW, COL].ColumnWidth = 15;
                int colExpenseBudgetName = COL;
                COL++;
                sheet[ROW, COL].Text = "Expense Activity";
                sheet[ROW, COL].ColumnWidth = 15;
                int colExpenseActivityName = COL;
                COL++;

                Dictionary<string, int> DicColindex = new Dictionary<string, int>();
                bool ColumnFound = false;
                for (int i = 0; i < dtServiceGroupGlReport.Columns.Count-1; i++)
                {
                    if (dtServiceGroupGlReport.Columns[i].ColumnName.ToString().ToUpper() == "EXPENSEACTIVITYNAME")
                        ColumnFound = true;
                    if (ColumnFound == false)
                        continue;

                    COL++;
                    sheet[ROW, COL].Text = dtServiceGroupGlReport.Columns[i + 1].ColumnName;
                    sheet[ROW, COL].ColumnWidth = 15;
                    DicColindex.Add(dtServiceGroupGlReport.Columns[i + 1].ColumnName, COL);
                    COL++;
                    sheet[ROW, COL].Text = dtServiceGroupGlReport.Columns[i + 1].ColumnName.Replace("GL", "Budget");
                    sheet[ROW, COL].ColumnWidth = 15;
                    DicColindex.Add(dtServiceGroupGlReport.Columns[i + 1].ColumnName.Replace("GL", "Budget"), COL);
                    COL++;
                    sheet[ROW, COL].Text = dtServiceGroupGlReport.Columns[i + 1].ColumnName.Replace("GL","Activity");
                    sheet[ROW, COL].ColumnWidth = 15;
                    DicColindex.Add(dtServiceGroupGlReport.Columns[i + 1].ColumnName.Replace("GL", "Activity"), COL);

                }





                int endCol = COL;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Font.Bold = true;
                sheet.Range[ROW, 1, ROW, endCol].CellStyle.Interior.ColorIndex = ExcelKnownColors.Grey_40_percent;
                sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);
                ROW++;

                int StartRow = ROW; //row 20
                for (int i = 0; i < dtServiceGroupGlReport.Rows.Count; i++)
                {


                    sheet[ROW, colSlNo].Number = (i + 1);

                   
                    sheet[ROW, colServiceGroupName].Text = dtServiceGroupGlReport.Rows[i]["ServiceGroupName"].ToString();
                    sheet[ROW, colServiceTypeName].Text = dtServiceGroupGlReport.Rows[i]["ServiceTypeName"].ToString();
                   
                    sheet[ROW, colDownPaymentGLInfo].Text = dtServiceGroupGlReport.Rows[i]["DownPaymentGLInfo"].ToString();
                    sheet[ROW, colClearingAccountGLInfo].Text = dtServiceGroupGlReport.Rows[i]["ClearingAccountGLInfo"].ToString();
                 
                   
                    sheet[ROW, colDownPaymentBudgetName].Text = dtServiceGroupGlReport.Rows[i]["DownPaymentBudgetName"].ToString();
                    sheet[ROW, colDownPaymentActivityName].Text = dtServiceGroupGlReport.Rows[i]["DownPaymentActivityName"].ToString();
                    
                  
                    sheet[ROW, colClearingAccountBudgetName].Text = dtServiceGroupGlReport.Rows[i]["ClearingAccountBudgetName"].ToString();
                    sheet[ROW, colClearingAccountActivityName].Text = dtServiceGroupGlReport.Rows[i]["ClearingAccountActivityName"].ToString();
                    sheet[ROW, colServiceGLInfo].Text = dtServiceGroupGlReport.Rows[i]["ServiceGLInfo"].ToString();
                  
                    sheet[ROW, colServiceBudgetName].Text = dtServiceGroupGlReport.Rows[i]["ServiceBudgetName"].ToString();
                    sheet[ROW, colServiceActivityName].Text = dtServiceGroupGlReport.Rows[i]["ServiceActivityName"].ToString();
                    sheet[ROW, colExpenseGLInfo].Text = dtServiceGroupGlReport.Rows[i]["ExpenseGLInfo"].ToString();
                  
                    sheet[ROW, colExpenseBudgetName].Text = dtServiceGroupGlReport.Rows[i]["ExpenseBudgetName"].ToString();
                    sheet[ROW, colExpenseActivityName].Text = dtServiceGroupGlReport.Rows[i]["ExpenseActivityName"].ToString();



             
                    foreach (var item in DicColindex)

                    {
                        if (dtServiceGroupGlReport.Columns.Contains(item.Key))
                            sheet[ROW, item.Value].Text = dtServiceGroupGlReport.Rows[i][item.Key].ToString();                        
                    }



                    dtServiceGroupBudgetReport.DefaultView.RowFilter = "ServiceGroupId='" + dtServiceGroupGlReport.Rows[i]["ServiceGroupId"].ToString() + @"'";
                    if (dtServiceGroupBudgetReport.DefaultView.Count > 0)
                    {

                        foreach (var item in DicColindex)
                        {
                            if (dtServiceGroupBudgetReport.Columns.Contains(item.Key))
                                sheet[ROW, item.Value].Text = dtServiceGroupBudgetReport.Rows[i][item.Key].ToString();
                        }

                    }

                    dtServiceGroupActivityReport.DefaultView.RowFilter = "ServiceGroupId='" + dtServiceGroupGlReport.Rows[i]["ServiceGroupId"].ToString() + @"'";

                    if (dtServiceGroupActivityReport.DefaultView.Count > 0)
                    {
                        foreach (var item in DicColindex)
                        {
                            if (dtServiceGroupActivityReport.Columns.Contains(item.Key))
                                sheet[ROW, item.Value].Text = dtServiceGroupActivityReport.Rows[i][item.Key].ToString();
                        }
               
                    
                    }




                    sheet.Range[ROW, 1, ROW, endCol].BorderAround(ExcelLineStyle.Hair);
                    sheet.Range[ROW, 1, ROW, endCol].BorderInside(ExcelLineStyle.Hair);

                    ROW++;

                }


                sheet.IsGridLinesVisible = false;

                sheet.UsedRange.WrapText = true;
                sheet.UsedRange.VerticalAlignment = ExcelVAlign.VAlignTop;
                sheet.Range[StartRow, 1, ROW, endCol].CellStyle.Font.Size = 8f;

                sheet["A" + StartRow.ToString()].FreezePanes();

                sheet.Range[StartRow, colSlNo, ROW, colSlNo].NumberFormat = clsStaticInfo.NumberFormat();
                sheet.Range[StartRow, colSlNo, ROW, colSlNo].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                ReportUtility reportUtility = new ReportUtility();
                // reportUtility.PlantHeader(ref sheet, endCol, "Service Group",identity.PlantId);
                reportUtility.CompanyGroupHeader(ref sheet, endCol, "Service Group", identity.CompanyGroupId);
                reportUtility.PageSetup(ref sheet, 6, ExcelPageOrientation.Landscape);
                sheet[ROW, COL].HorizontalAlignment = ExcelHAlign.HAlignLeft;
                sheet.Range[1, 1, 6, endCol].HorizontalAlignment = ExcelHAlign.HAlignLeft;

                string strFileName = "Service Group.xlsx";
                workbook.SaveAs(strFileName, ExcelSaveType.SaveAsXLS, System.Web.HttpContext.Current.Response, ExcelDownloadType.PromptDialog);
                workbook.Close();
            }
            catch (Exception)
            {

                throw;
            }
        }




        public GridModel GetSearchWithCombineWithAssing(GridParameter parameters, string coaId)
        {
            try
            {
                string coaStr = " ";
                if (coaId != "null")
                    coaStr += "where isnull(c.Id,'') ='" + coaId + @"'";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT   F.Id ,MGM.Id AS ServiceGroupId
                                          ,MGM.UserName AS ServiceGroupName
                                         ,MT.UserName AS ServiceTypeName
										 ,MGM.ServiceTypeId
										 ,F.DownPaymentGLId
										 ,F.ClearingAccountGLId
        		                         ,F.COAId
        		                         ,F.UserName 'COAName'
                                         ,F.DownPaymentGLInfo
                                         ,F.ClearingAccountGLInfo
					                     ,F.DownPaymentBudgetMasterId
					                     ,F.DownPaymentActivityId
					                     ,F.DownPaymentBudgetName
					                     ,F.DownPaymentActivityName
					                     ,F.ClearingAccountBudgetMasterId
					                     ,F.ClearingAccountActivityId
					                     ,F.ClearingAccountBudgetName
					                     ,F.ClearingAccountActivityName
                                         ,F.ServiceGLInfo
                                         ,F.ServiceGLId
                                         ,F.ServiceBudgetMasterId
					                     ,F.ServiceActivityId
					                     ,F.ServiceBudgetName
					                     ,F.ServiceActivityName
                                         ,F.ExpenseGLInfo
                                         ,F.ExpenseGLId
                                         ,F.ExpenseBudgetMasterId
					                     ,F.ExpenseActivityId
					                     ,F.ExpenseBudgetName
					                     ,F.ExpenseActivityName
                                            FROM HKP.ServiceGroup As MGM
LEFT OUTER JOIN HKP.ServiceType As MT ON MT.Id = MGM.ServiceTypeId
                                            LEFT OUTER JOIN (select
        			MAD.Id,MAD.ServiceGroupId,
        			c.Id AS COAId,GLGI1.AccountCode
        			,MAD.DownPaymentGLId,MAD.ClearingAccountGLId
                    ,MAD.ServiceGLId,MAD.ExpenseGLId
        			,C.UserName
					,GLGI1.AccountCode + ' - ' + GLGI1.UserName AS DownPaymentGLInfo
					,GLGI2.AccountCode + ' - ' + GLGI2.UserName AS ClearingAccountGLInfo
					,GLGI3.AccountCode + ' - ' + GLGI3.UserName AS ServiceGLInfo
					,GLGI4.AccountCode + ' - ' + GLGI4.UserName AS ExpenseGLInfo
        			,MAD.DownPaymentBudgetMasterId
                    ,MAD.DownPaymentActivityId
					,DPB.UserName AS DownPaymentBudgetName
					,DPA.UserName AS DownPaymentActivityName
        			,MAD.ClearingAccountBudgetMasterId
                    ,MAD.ClearingAccountActivityId
					,CAB.UserName AS ClearingAccountBudgetName
					,CAA.UserName AS ClearingAccountActivityName
        			,MAD.ServiceBudgetMasterId
                    ,MAD.ServiceActivityId
        			,MAD.ExpenseBudgetMasterId
                    ,MAD.ExpenseActivityId
					,IB.UserName AS ServiceBudgetName
					,IA.UserName AS ServiceActivityName
					,EB.UserName AS ExpenseBudgetName
					,EA.UserName AS ExpenseActivityName
        			from HKP.COA c
        			LEFT OUTER JOIN HKP.ServiceGroupGL AS MAD ON MAD.COAId=c.Id
        			LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI1 ON GLGI1.Id=MAD.DownPaymentGLId
        			LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI2 ON GLGI2.Id=MAD.ClearingAccountGLId
        			LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI3 ON GLGI3.Id=MAD.ServiceGLId
        			LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=MAD.ExpenseGLId
					LEFT OUTER JOIN MST.BudgetMaster AS DPBM ON MAD.DownPaymentBudgetMasterId = DPBM.Id
					LEFT OUTER JOIN HKP.Budget AS DPB ON DPBM.BudgetId = DPB.Id
					LEFT OUTER JOIN HKP.Activity AS DPA ON MAD.DownPaymentActivityId = DPA.Id

					LEFT OUTER JOIN MST.BudgetMaster AS CABM ON MAD.ClearingAccountBudgetMasterId = CABM.Id
					LEFT OUTER JOIN HKP.Budget AS CAB ON CABM.BudgetId = CAB.Id
					LEFT OUTER JOIN HKP.Activity AS CAA ON MAD.ClearingAccountActivityId = CAA.Id

					LEFT OUTER JOIN MST.BudgetMaster AS IBM ON MAD.ServiceBudgetMasterId = IBM.Id
					LEFT OUTER JOIN HKP.Budget AS IB ON IBM.BudgetId = IB.Id
					LEFT OUTER JOIN HKP.Activity AS IA ON MAD.ServiceActivityId = IA.Id

					LEFT OUTER JOIN MST.BudgetMaster AS EBM ON MAD.ExpenseBudgetMasterId = EBM.Id
					LEFT OUTER JOIN HKP.Budget AS EB ON EBM.BudgetId = EB.Id
					LEFT OUTER JOIN HKP.Activity AS EA ON MAD.ExpenseActivityId = CAA.Id
											" + coaStr + @"
											)AS F ON F.ServiceGroupId = MGM.Id
                     WHERE F.DownPaymentGLId <> '' AND F.ClearingAccountGLId <> '' AND F.ServiceGLId <> '' AND F.ExpenseGLId <>''";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public GridModel GetSearchWithCombineWithNotAssing(GridParameter parameters, string coaId)
        {
            try
            {
                string coaStr = " ";
                if (coaId != "null")
                    coaStr += "where isnull(c.Id,'') ='" + coaId + @"'";
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT   F.Id ,MGM.Id AS ServiceGroupId
                                          ,MGM.UserName AS ServiceGroupName
                                         ,MT.UserName AS ServiceTypeName
										 ,MGM.ServiceTypeId
										 ,F.DownPaymentGLId
										 ,F.ClearingAccountGLId
        		                         ,F.COAId
        		                         ,F.UserName 'COAName'
                                         ,F.DownPaymentGLInfo
                                         ,F.ClearingAccountGLInfo
					                     ,F.DownPaymentBudgetMasterId
					                     ,F.DownPaymentActivityId
					                     ,F.DownPaymentBudgetName
					                     ,F.DownPaymentActivityName
					                     ,F.ClearingAccountBudgetMasterId
					                     ,F.ClearingAccountActivityId
					                     ,F.ClearingAccountBudgetName
					                     ,F.ClearingAccountActivityName
                                         ,F.ServiceGLInfo
                                         ,F.ServiceGLId
                                         ,F.ServiceBudgetMasterId
					                     ,F.ServiceActivityId
					                     ,F.ServiceBudgetName
					                     ,F.ServiceActivityName
                                         ,F.ExpenseGLInfo
                                         ,F.ExpenseGLId
                                         ,F.ExpenseBudgetMasterId
					                     ,F.ExpenseActivityId
					                     ,F.ExpenseBudgetName
					                     ,F.ExpenseActivityName
                                            FROM HKP.ServiceGroup As MGM
LEFT OUTER JOIN HKP.ServiceType As MT ON MT.Id = MGM.ServiceTypeId
                                            LEFT OUTER JOIN (select
        			MAD.Id,MAD.ServiceGroupId,
        			c.Id AS COAId,GLGI1.AccountCode
        			,MAD.DownPaymentGLId,MAD.ClearingAccountGLId
                    ,MAD.ServiceGLId,MAD.ExpenseGLId
        			,C.UserName
					,GLGI1.AccountCode + ' - ' + GLGI1.UserName AS DownPaymentGLInfo
					,GLGI2.AccountCode + ' - ' + GLGI2.UserName AS ClearingAccountGLInfo
					,GLGI3.AccountCode + ' - ' + GLGI3.UserName AS ServiceGLInfo
					,GLGI4.AccountCode + ' - ' + GLGI4.UserName AS ExpenseGLInfo
        			,MAD.DownPaymentBudgetMasterId
                    ,MAD.DownPaymentActivityId
					,DPB.UserName AS DownPaymentBudgetName
					,DPA.UserName AS DownPaymentActivityName
        			,MAD.ClearingAccountBudgetMasterId
                    ,MAD.ClearingAccountActivityId
					,CAB.UserName AS ClearingAccountBudgetName
					,CAA.UserName AS ClearingAccountActivityName
        			,MAD.ServiceBudgetMasterId
                    ,MAD.ServiceActivityId
        			,MAD.ExpenseBudgetMasterId
                    ,MAD.ExpenseActivityId
					,IB.UserName AS ServiceBudgetName
					,IA.UserName AS ServiceActivityName
					,EB.UserName AS ExpenseBudgetName
					,EA.UserName AS ExpenseActivityName
        			from HKP.COA c
        			LEFT OUTER JOIN HKP.ServiceGroupGL AS MAD ON MAD.COAId=c.Id
        			LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI1 ON GLGI1.Id=MAD.DownPaymentGLId
        			LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI2 ON GLGI2.Id=MAD.ClearingAccountGLId
        			LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI3 ON GLGI3.Id=MAD.ServiceGLId
        			LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=MAD.ExpenseGLId
					LEFT OUTER JOIN MST.BudgetMaster AS DPBM ON MAD.DownPaymentBudgetMasterId = DPBM.Id
					LEFT OUTER JOIN HKP.Budget AS DPB ON DPBM.BudgetId = DPB.Id
					LEFT OUTER JOIN HKP.Activity AS DPA ON MAD.DownPaymentActivityId = DPA.Id

					LEFT OUTER JOIN MST.BudgetMaster AS CABM ON MAD.ClearingAccountBudgetMasterId = CABM.Id
					LEFT OUTER JOIN HKP.Budget AS CAB ON CABM.BudgetId = CAB.Id
					LEFT OUTER JOIN HKP.Activity AS CAA ON MAD.ClearingAccountActivityId = CAA.Id

					LEFT OUTER JOIN MST.BudgetMaster AS IBM ON MAD.ServiceBudgetMasterId = IBM.Id
					LEFT OUTER JOIN HKP.Budget AS IB ON IBM.BudgetId = IB.Id
					LEFT OUTER JOIN HKP.Activity AS IA ON MAD.ServiceActivityId = IA.Id

					LEFT OUTER JOIN MST.BudgetMaster AS EBM ON MAD.ExpenseBudgetMasterId = EBM.Id
					LEFT OUTER JOIN HKP.Budget AS EB ON EBM.BudgetId = EB.Id
					LEFT OUTER JOIN HKP.Activity AS EA ON MAD.ExpenseActivityId = CAA.Id
											" + coaStr + @"
											)AS F ON F.ServiceGroupId = MGM.Id
                     WHERE ( ISNULL(F.DownPaymentGLId, '') = '' OR ISNULL(F.ClearingAccountGLId, '') = ''  OR ISNULL(F.ServiceGLId , '') = ''  OR ISNULL(F.ExpenseGLId , '') = '') ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        public GridModel GetSearchWithCombineCoa(GridParameter parameters)
        {
            try
            {
                var identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
                parameters.CmdText = @"SELECT   F.Id ,MGM.Id AS ServiceGroupId
                                          ,MGM.UserName AS ServiceGroupName
                                         ,MT.UserName AS ServiceTypeName
										 ,MGM.ServiceTypeId
										 ,F.DownPaymentGLId
										 ,F.ClearingAccountGLId
        		                         ,F.COAId
        		                         ,F.UserName 'COAName'
                                         ,F.DownPaymentGLInfo
                                         ,F.ClearingAccountGLInfo
					                     ,F.DownPaymentBudgetMasterId
					                     ,F.DownPaymentActivityId
					                     ,F.DownPaymentBudgetName
					                     ,F.DownPaymentActivityName
					                     ,F.ClearingAccountBudgetMasterId
					                     ,F.ClearingAccountActivityId
					                     ,F.ClearingAccountBudgetName
					                     ,F.ClearingAccountActivityName
                                         ,F.ServiceGLInfo
                                         ,F.ServiceGLId
                                         ,F.ServiceBudgetMasterId
					                     ,F.ServiceActivityId
					                     ,F.ServiceBudgetName
					                     ,F.ServiceActivityName
                                         ,F.ExpenseGLInfo
                                         ,F.ExpenseGLId
                                         ,F.ExpenseBudgetMasterId
					                     ,F.ExpenseActivityId
					                     ,F.ExpenseBudgetName
					                     ,F.ExpenseActivityName
                                            FROM HKP.ServiceGroup As MGM
                                            LEFT OUTER JOIN HKP.ServiceType As MT ON MT.Id = MGM.ServiceTypeId
                                            LEFT OUTER JOIN (select
        			MAD.Id,MAD.ServiceGroupId,
        			c.Id AS COAId,GLGI1.AccountCode
        			,MAD.DownPaymentGLId,MAD.ClearingAccountGLId
                    ,MAD.ServiceGLId,MAD.ExpenseGLId
        			,C.UserName
					,GLGI1.AccountCode + ' - ' + GLGI1.UserName AS DownPaymentGLInfo
					,GLGI2.AccountCode + ' - ' + GLGI2.UserName AS ClearingAccountGLInfo
					,GLGI3.AccountCode + ' - ' + GLGI3.UserName AS ServiceGLInfo
					,GLGI4.AccountCode + ' - ' + GLGI4.UserName AS ExpenseGLInfo
        			,MAD.DownPaymentBudgetMasterId
                    ,MAD.DownPaymentActivityId
					,DPB.UserName AS DownPaymentBudgetName
					,DPA.UserName AS DownPaymentActivityName
        			,MAD.ClearingAccountBudgetMasterId
                    ,MAD.ClearingAccountActivityId
					,CAB.UserName AS ClearingAccountBudgetName
					,CAA.UserName AS ClearingAccountActivityName
        			,MAD.ServiceBudgetMasterId
                    ,MAD.ServiceActivityId
        			,MAD.ExpenseBudgetMasterId
                    ,MAD.ExpenseActivityId
					,IB.UserName AS ServiceBudgetName
					,IA.UserName AS ServiceActivityName
					,EB.UserName AS ExpenseBudgetName
					,EA.UserName AS ExpenseActivityName
        			from HKP.COA c
        			LEFT OUTER JOIN HKP.ServiceGroupGL AS MAD ON MAD.COAId=c.Id
        			LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI1 ON GLGI1.Id=MAD.DownPaymentGLId
        			LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI2 ON GLGI2.Id=MAD.ClearingAccountGLId
        			LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI3 ON GLGI3.Id=MAD.ServiceGLId
        			LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=MAD.ExpenseGLId
					LEFT OUTER JOIN MST.BudgetMaster AS DPBM ON MAD.DownPaymentBudgetMasterId = DPBM.Id
					LEFT OUTER JOIN HKP.Budget AS DPB ON DPBM.BudgetId = DPB.Id
					LEFT OUTER JOIN HKP.Activity AS DPA ON MAD.DownPaymentActivityId = DPA.Id

					LEFT OUTER JOIN MST.BudgetMaster AS CABM ON MAD.ClearingAccountBudgetMasterId = CABM.Id
					LEFT OUTER JOIN HKP.Budget AS CAB ON CABM.BudgetId = CAB.Id
					LEFT OUTER JOIN HKP.Activity AS CAA ON MAD.ClearingAccountActivityId = CAA.Id

					LEFT OUTER JOIN MST.BudgetMaster AS IBM ON MAD.ServiceBudgetMasterId = IBM.Id
					LEFT OUTER JOIN HKP.Budget AS IB ON IBM.BudgetId = IB.Id
					LEFT OUTER JOIN HKP.Activity AS IA ON MAD.ServiceActivityId = IA.Id

					LEFT OUTER JOIN MST.BudgetMaster AS EBM ON MAD.ExpenseBudgetMasterId = EBM.Id
					LEFT OUTER JOIN HKP.Budget AS EB ON EBM.BudgetId = EB.Id
					LEFT OUTER JOIN HKP.Activity AS EA ON MAD.ExpenseActivityId = EA.Id
											where isnull(c.Id,'') ='2'
											)AS F ON F.ServiceGroupId = MGM.Id ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Material.ToString()));
            }
        }

        #endregion ServiceGroupGL

        public GridModel GetPartyAccountGroup(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT FADVR.Id
                                                ,FADVR.FixedAssetGLId
                                                ,FADVR.GLGeneralInfoId
                                                ,FADVR.AddedBy
                                                ,FADVR.AddedDate
                                                ,FADVR.AddedFromIP
                                                ,FADVR.UpdatedBy
                                                ,FADVR.UpdatedDate
                                                ,FADVR.UpdatedFromIP
                                                ,PAG.Id AS PartyAccountGroupId
                                                ,PAG.UserName
                                                ,PAG.Code
                                                ,VR.UserName AS VendorRecontGLText
                                                ,VR.AccountCode AS VendorReconGLCode
                                                FROM HKP.PartyAccountGroup AS PAG
                                                LEFT OUTER JOIN [HKP].[ServiceGroupPartyAccountGroupGL]AS FADVR ON FADVR.PartyAccountGroupId = PAG.Id
                                                	LEFT OUTER JOIN HKP.GLGeneralInfo AS VR ON FADVR.GLGeneralInfoId = VR.Id
                                                WHERE PAG.AccountType='Vendor' ";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }

        public GridModel GetPartyAccountVD(GridParameter parameters)
        {
            try
            {
                parameters.CmdText = @"SELECT FADV.*
                                    ,GLGI4.AccountCode AS ClearingAccGLCode
                                    ,GLGI4.UserName AS ClearingAccGLText
                                    ,B.UserName AS BudgetName
                                    ,A.UserName AS ActivityName
                                     FROM  [HKP].[ServiceGroupPartyAccountGroupGL] AS FADV
                                     LEFT OUTER JOIN HKP.GLGeneralInfo AS GLGI4 ON GLGI4.Id=FADV.GLGeneralInfoId
				                     LEFT OUTER JOIN MST.BudgetMaster BM ON FADV.BudgetMasterId = BM.Id
                                     LEFT OUTER JOIN HKP.Budget AS B ON BM.BudgetId = B.Id
                                     LEFT OUTER JOIN HKP.Activity AS A ON FADV.ActivityId = A.Id";
                return _sqlRepository.GetGridData(parameters);
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, ex,
                    Logger.ThrowError(GetType().Name, MethodBase.GetCurrentMethod().Name, null,
                    ErrorType.ServiceError, null, ex.Message, ex.GetType().Name, false, ModuleEnum.Party.ToString()));
            }
        }
    }
}