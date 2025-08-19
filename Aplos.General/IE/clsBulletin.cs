using Library.Core;
using Library.Crosscutting.Security;
using Library.Data.Sql;
using Library.Service.Helpers;
using Library.Service.Systems;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;


namespace Library.General.IE
{
    public class clsBulletin
    {
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
        public clsBulletin()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();            
        }

        
        public IEnumerable<object> GetBulletinMachineOperation(string bulletinTemplateMasterId)
        {
            try
            {
                string sql = @"SELECT BTD.Id,BTD.BulletinTemplateMasterId,BTD.Sequence,BTD.OperationVariationId,BTD.OperationGroup,BTD.SkillId,BTD.MachineVarientId,BTD.FGZoneId,BTD.FGComponentId
                            ,CONVERT(NUMERIC(10,2),BTD.AdditionalSPT) AdditionalSPT, CONVERT(NUMERIC(10,2),BTD.TotalSPT) TotalSPT, CONVERT(NUMERIC(10,2),BTD.AllotedWorkstation) AllotedWorkstation
                            , CONVERT(NUMERIC(10,2),BTD.AllotedManpower) AllotedManpower, BTD.AttachmentId,BTD.GaugeFolderId,BTD.OperationConsumptionId,BTD.OperationTypeId,CONVERT(NUMERIC(10,2),BTD.Frequency) Frequency
                            ,BTD.Remark,BTD.OperationCategoryId,BTD.QualityLevel,CONVERT(NUMERIC(10,2),BTD.AvgAllotedTime) AvgAllotedTime,CONVERT(NUMERIC(10,0),BTD.OperationTargetPerHr) OperationTargetPerHr
                            ,CONVERT(NUMERIC(10,0),BTD.RequiredManPower) RequiredManPower
                            ,OPP.Operation,OV.Code OperationCode, OV.UserName OperationVariation, FZ.UserName FGZone, FC.UserName FGComponent, A.UserName Attachment,
                             GF.UserName GaugeFolder, OC.UserName OperationConsumption, OT.UserName OperationType, OV.OperationId, MMA.StandardName MachineName
                            ,0 AvgAllotedTime, OperationSPT=BTD.TotalSPT-BTD.AdditionalSPT, MM.UserName MaterialMaster, 0 IsMaxAllottedTime 
                            , SK.UserName AS SkillName,OPP.BasicProcessTime,OPP.AssociateProcessTime,OPP.PersonalAllowance,OPP.MachineAllowance,OPP.Frequency,OPP.SPI OperationSPI,OV.TotalSAM, OV.AdditionalSAMSymbol,OV.SubOperationSAM,OV.AdditionalSAM
							,BTD.SPI,BTD.NoOfStitch,BTD.OperationLength,BTD.StitchCodeId,BTD.FabricWidth,BTD.NeedleDescription,BTD.NeedleMaterialMasterId,MMN.UserName NeedleMaterialMaster, BTD.NeedleArticleId,MMNA.ShortName NeedleArticle
							,BTD.BobbinDescription,BTD.BobbinMaterialMasterId,MMB.UserName BobbinMaterialMaster,BTD.BobbinArticleId,MMBA.ShortName BobbinArticle
							,BTD.LooperDescription,BTD.LooperMaterialMasterId,MML.UserName LooperMaterialMaster,BTD.LooperArticleId,MMLA.ShortName LooperArticle,SC.userName StitchCode
                            ,BTD.SPIConsumption,BTD.NeedleConsumption,BTD.BobbinConsumption,BTD.LooperConsumption,BTD.Consumption,SC.Needle NeedlePer,SC.Bobbin BobbinPer,SC.Looper LooperPer,BTD.WastagePercentage,BTD.ExtraOrderPercentage
                             FROM [MST].[BulletinTemplateDetail] BTD
                             LEFT JOIN [MST].[OperationVariation] OV ON OV.Id=BTD.OperationVariationId
                             LEFT JOIN (SELECT OP.Id,OP.UserName Operation,ISNULL(OP.BasicProcessTime, 0) AS BasicProcessTime, ISNULL(OP.AssociateProcessTime, 0) AS AssociateProcessTime
                                     ,ISNULL(OP.PersonalAllowance, 0) AS PersonalAllowance, ISNULL(OP.MachineAllowance, 0) AS MachineAllowance
                                     ,OP.Frequency, OP.SPI FROM [MST].[Operation] OP) OPP ON OPP.Id =OV.OperationId
                             LEFT JOIN HKP.FGZone FZ ON FZ.Id=BTD.FGZoneId
                             LEFT JOIN HKP.FGComponent FC ON FC.Id=BTD.FGComponentId
                             LEFT JOIN HKP.Attachment A ON A.Id=BTD.AttachmentId
                             LEFT JOIN HKP.GaugeFolder GF ON GF.Id=BTD.GaugeFolderId
                             LEFT JOIN HKP.OperationConsumption OC ON OC.Id=BTD.OperationConsumptionId
                             LEFT JOIN HKP.OperationType OT ON OT.Id=BTD.OperationTypeId
                             LEFT JOIN [MST].[MaterialMasterArticle] MMA ON MMA.Id = BTD.MachineVarientId
                             LEFT JOIN [MST].[MaterialMaster] MM ON MM.Id=MMA.MaterialMasterId
							 LEFT JOIN [HKP].[Skill] AS SK ON BTD.SkillId=Sk.Id
                             LEFT JOIN [HKP].StitchCode AS SC ON BTD.StitchCodeId=SC.Id

                             LEFT JOIN [MST].[MaterialMaster] MMN ON MMN.Id=BTD.NeedleMaterialMasterId
							 LEFT JOIN [MST].[MaterialMasterArticle] MMNA ON MMNA.Id = BTD.NeedleArticleId

							  LEFT JOIN [MST].[MaterialMaster] MMB ON MMB.Id=BTD.BobbinMaterialMasterId
							 LEFT JOIN [MST].[MaterialMasterArticle] MMBA ON MMBA.Id = BTD.BobbinArticleId

							 LEFT JOIN [MST].[MaterialMaster] MML ON MML.Id=BTD.LooperMaterialMasterId
							 LEFT JOIN [MST].[MaterialMasterArticle] MMLA ON MMLA.Id = BTD.LooperArticleId
                             WHERE BTD.BulletinTemplateMasterId='" + bulletinTemplateMasterId + "'  AND MM.Id <>'' ORDER BY BTD.Sequence ";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetMachineChangeInfo(string plantId)
        {
            try
            {
                string sql = @"SELECT IsMachineChangeableinBulletinTemplate FROM SCS.PlantConfig WHERE PlantId='" + plantId + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetThreadMatrixData(string bulletinTemplateMasterId)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"SELECT SUM(A.NeedleConsumption) NeedleConsumption, SUM(A.BobbinConsumption) BobbinConsumption, SUM(A.LooperConsumption) LooperConsumption,A.ArticleId, A.Thread
                                FROM 
                                (
                                SELECT BTD.NeedleArticleId ArticleId, NMA.ShortName Thread,SUM(BTD.NeedleConsumption) NeedleConsumption,0 BobbinConsumption, 0 LooperConsumption 
                                FROM MST.BulletinTemplateDetail BTD 
                                LEFT JOIN MST.MaterialMaster NM ON NM.Id=BTD.NeedleMaterialMasterId
                                JOIN MST.MaterialMasterArticle NMA ON NMA.Id=BTD.NeedleArticleId
                                WHERE BulletinTemplateMasterId='" + bulletinTemplateMasterId + @"' AND ISNULL(BTD.MachineVarientId,'')<>''
                                GROUP BY BTD.NeedleArticleId, NMA.ShortName,BTD.BobbinArticleId
                                UNION ALL

                                select BTD.BobbinArticleId, BMA.ShortName BobbinArticle,0 NeedleConsumption,SUM(BTD.BobbinConsumption) BobbinConsumption, 0 LooperConsumption 
                                from MST.BulletinTemplateDetail BTD 
                                LEFT JOIN MST.MaterialMaster BM ON BM.Id=BTD.BobbinMaterialMasterId
                                JOIN MST.MaterialMasterArticle BMA ON BMA.Id=BTD.BobbinArticleId
                                Where BulletinTemplateMasterId='" + bulletinTemplateMasterId + @"' AND ISNULL(BTD.MachineVarientId,'')<>''
                                GROUP BY BTD.BobbinArticleId, BMA.ShortName
                                UNION ALL
                                select BTD.LooperArticleId, LMA.ShortName LooperArticle,0 NeedleConsumption,0 BobbinConsumption,SUM(BTD.LooperConsumption) LooperConsumption
                                from MST.BulletinTemplateDetail BTD 
                                LEFT JOIN MST.MaterialMaster LM ON LM.Id=BTD.LooperMaterialMasterId
                                JOIN MST.MaterialMasterArticle LMA ON LMA.Id=BTD.LooperArticleId
                                Where BulletinTemplateMasterId='" + bulletinTemplateMasterId + @"' AND ISNULL(BTD.MachineVarientId,'')<>''
                                GROUP BY BTD.LooperArticleId, LMA.ShortName
                                ) AS A 
                                GROUP BY A.ArticleId, A.Thread";
                return _sqlRepository.GetDataCollection(strSQL, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void DeleteMultiBulletinOperation(string id)
        {
            string strSQL;
            ConnectionManager.DAL.ConManager objCon = null;
            try
            {
                strSQL = "DELETE FROM [MST].[BulletinTemplateDetail] WHERE Id " + id + "";
                objCon = new ConnectionManager.DAL.ConManager("1");
                objCon.OpenConnection("1");
                objCon.BeginTransaction();
                objCon.ExecuteNonQueryWrapper(strSQL, true, "1");
                objCon.CommitTransaction();
            }
            catch (Exception ex)
            {
                try
                {
                    objCon.RollBack();
                    throw (ex);
                }
                catch (Exception exx)
                {
                    throw exx;
                }
            }
            finally
            {
                objCon.CloseConnection();
                objCon = null;
            }
        }//End of function

        public DataSet GetOperationDataByCode(string companyGroupId, string Code, string processId, string bulletinTemplateMasterId)
        {
            try
            {
                GridParameter parameters;
                parameters = new GridParameter
                {
                    ExportType = "DATASET",
                    CmdText = @"SELECT CONVERT (bit,0) Active
                           	,OV.Id OperationVariationId
                           	,OV.Code OperationCode
                           	,OV.[Sequence]
                           	,A.Id MachineVarientId
							,MM.UserName MaterialMaster
                           	,A.StandardName Article
							,S.Id SkillId
                           	,S.UserName Skill
                           	,OV.UserName OperationVariation
                           	,OV.SubOperationSAM
                           	,OV.AdditionalSAM
                           	,OV.SPI,OV.VASSAMSOURCE
                           	,ISNULL(OV.VASFINALSAM,OV.TotalSAM) TtalSAM
							,TotalSAM=CASE WHEN ISNULL(OV.VASSAMSOURCE,'')='' THEN OV.TotalSAM ELSE OV.VASFINALSAM END
                           	,OV.Frequency
                            ,OT.Id OperationTypeId
                            ,OV.AdditionalSAMSymbol
                            ,OV.OperationId
                            ,OCT.Id OperationCategoryId
							,OCT.UserName OperationCategory
                            ,SC.Id StitchCodeId ,SC.UserName StitchCode,O.OperationLength
                           FROM [MST].[OperationVariation] OV
                           LEFT JOIN [MST].[MaterialMasterArticle] A ON A.Id = OV.ArticleId
                           LEFT JOIN [HKP].[Skill] S ON S.Id = OV.SkillId
                           LEFT JOIN [MST].[MaterialMaster] MM ON MM.Id=A.MaterialMasterId AND MM.SkillId=S.Id
                           LEFT JOIN [MST].[Operation] O ON O.Id = OV.OperationId
                           LEFT JOIN [HKP].[OperationType] OT ON OT.Id = O.OperationTypeId
                           LEFT JOIN [HKP].[OperationCategory] OCT ON OCT.Id = O.OperationCategoryId
                           LEFT JOIN [HKP].[StitchCode] SC ON SC.Id = A.StitchCodeId
						   INNER JOIN (Select * from [MST].[OperationProcess] WHERE ProcessId='" + processId + @"')OP ON OP.OperationId=OV.OperationId
                           WHERE OV.CompanyGroupId = '" + companyGroupId + @"' AND OV.Code IN (" + Code + @") "
                };


                return _sqlRepository.GetGridData(parameters).Source;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IEnumerable<object> GetProductionBulletinInfo(string Id)
        {
            try
            {
                string sql = @"SELECT PO.Id POId,PS.UserName ProductionStatus, PO.RequiredTimeUnit, Qty,FORMAT(LSD,'dd-MMM-yyyy') LSD 
	,FORMAT(CommitmentDate,'dd-MMM-yyyy') CommitmentDate, PD.Product, PD.ProductCategory,PD.Buyer,PD.Customer 
    ,ISNULL(PD.BuyerOrder,'') BuyerOrder,ISNULL(PD.OwnOrder,'') OwnOrder,ISNULL(PD.BuyerItem,'') BuyerItem,ISNULL(PD.OwnItem,'') OwnItem,PD.Description,PD.PONumber,PD.MaterialMasterId,PD.MaterialMaster,PD.ArticleId,PD.Article
	,SONo=STUFF((select distinct ','+XSO.Id from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                         LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                                                 WHERE po.Id=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
	FROM TRN.ProductionOrder PO 
	LEFT JOIN [HKP].[ProductionStatus] PS ON PS.Id=PO.ProductionStatusId
	LEFT JOIN 
	(select distinct POD.ProductionOrderId,PM.UserName AS Product,pc.UserName AS ProductCategory
	,MM.Id MaterialMasterId,mm.UserName MaterialMaster,MMA.Id ArticleId,ISNULL(mma.StandardName, '') Article
	,Buyer=  REPLACE(REPLACE(
					STUFF((select distinct ','+XB.UserName from 
	                    trn.SalesOrder XSO 
		                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
									,'&amp;','&'), 'amp;', '')	
,Customer= REPLACE(REPLACE(
						STUFF((select distinct ','+XP.UserName from 
		                    trn.SalesOrder XSO 
		                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                    left outer join [HKP].[Party] Xp on XP.Id=XMO.PartyId
			                    where pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
								,'&amp;','&'), 'amp;', '')	
,BuyerOrder = REPLACE(REPLACE(
			STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
											trn.MasterOrder XMOI 	 
								INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
						,'&amp;','&'), 'amp;', '')
,OwnOrder =REPLACE(REPLACE(
			STUFF((select distinct ','+XMOI.OwnReferenceNo from 
											trn.MasterOrder XMOI 	 
								INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
						,'&amp;','&'), 'amp;', '')
,BuyerItem=REPLACE(REPLACE(
			STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
											trn.MasterOrderItem XMOI 	  
								INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
						,'&amp;','&'), 'amp;', '')	                                                
,OwnItem=REPLACE(REPLACE(
		STUFF((select distinct ','+XMOI.OwnReferenceNo from 
											trn.MasterOrderItem XMOI 	  
								INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							where podx.ProductionOrderId=pod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
		,'&amp;','&'), 'amp;', '')	 
,PONumber=REPLACE(REPLACE(
			STUFF((select distinct ','+CPO.PONumber from 
                                    trn.SalesOrder XSO 
                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						            LEFT JOIN [TRN].[CustomerPO] CPO ON CPO.Id = XSO.CustomerPOId
                                    WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
		,'&amp;','&'), 'amp;', '')	
, Description=REPLACE(REPLACE(
			STUFF((select distinct ','+XSO.Description from 
                                    trn.SalesOrder XSO 
                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
						                                        
                                    WHERE pod.ProductionOrderId=Xpod.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
		,'&amp;','&'), 'amp;', '')	
	FROM TRN.SalesOrder SO
	LEFT JOIN  TRN.ProductionOrderDetail POD ON POD.SalesOrderId=SO.Id
	LEFT JOIN TRN.MasterOrderItem MOI on moi.Id=so.MasterOrderItemId
    LEFT JOIN MST.MaterialMaster mm on mm.id=MOI.MaterialMasterId
    LEFT JOIN MST.MaterialMasterArticle mma ON mma.id = moi.ArticleId
	LEFT JOIN TRN.ProductDefinition AS pd ON pd.MaterialMasterId=mm.Id
	LEFT JOIN [MST].[ProductMaster] PM on pm.id=pd.ProductMasterId
    LEFT JOIN [HKP].[ProductCategory] PC on pc.Id=pm.ProductCategoryId
	) PD ON PD.ProductionOrderId=PO.Id
	LEFT JOIN [TRN].[ProductionBulletinTemplate] PB ON PB.ProductionOrderId=PD.ProductionOrderId
	where PB.BulletinTemplateId='" + Id + "'";
                return _sqlRepository.GetDataCollection(sql, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetBulletinTemplateData()
        {
            try
            {
                string sql = @"Select BT.* ,PM.UserName ProductMaster, SG.UserName SizeGroup
						  ,Buyer=REPLACE(REPLACE(
										 STUFF((select distinct ', '+B.UserName FROM 
                                        [MST].[BulletinTemplateBuyerInfo] BTB 
										JOIN HKP.Buyer B ON B.Id=BTB.BuyerId
                                        WHERE BT.Id=BTB.BulletinTemplateId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')				 
			         	,BuyerItemRefNo=REPLACE(REPLACE(
										STUFF((select distinct ', '+BTB.BuyerStyleRefNo FROM 
                                        [MST].[BulletinTemplateBuyerInfo] BTB 
                                        WHERE BT.Id=BTB.BulletinTemplateId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
			        	,OwnStyleRefNo=REPLACE(REPLACE(
										STUFF((select distinct ', '+BTB.OwnStyleRefNo FROM 
                                        [MST].[BulletinTemplateBuyerInfo] BTB 
                                        WHERE BT.Id=BTB.BulletinTemplateId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')										
							
						,P.UserName Process,ISNULL(BTD.TotalSPT,0)TotalSPT,ISNULL(BTD.RequiredManPower,0)RequiredManPower,ISNULL(BTD.AllotedManpower,0)AllotedManpower,ISNULL(BTD.AllotedWorkstation,0)AllotedWorkstation,CEILING(ISNULL(BTD.LineTargetPerHour,0))LineTargetPerHour
                        ,FORMAT(BT.AddedDate,'dd-MMM-yyyy')CreationDate
                         FROM [MST].[BulletinTemplate] BT
                         LEFT JOIN MST.ProductMaster PM ON PM.Id=BT.ProductMasterId
						 left join [MST].[BulletinTemplateMaster] BTP ON BT.Id=BTP.BulletinTemplateId
						 left join (Select BulletinTemplateMasterId, SUM(TotalSPT) TotalSPT,SUM(RequiredManPower) RequiredManPower
						 ,SUM(AllotedManpower) AllotedManpower,SUM(AllotedWorkstation) AllotedWorkstation
						 ,LineTargetPerHour=(((SUM(AllotedManpower) * 60) /NULLIF(SUM(TotalSPT),0)) * (SUM(TotalSPT) /NULLIF(SUM(AllotedManpower),0))/ MAX(NULLIF(AvgAllotedTime,0)))
						 from MST.BulletinTemplateDetail GROUP BY BulletinTemplateMasterId) BTD ON BTD.BulletinTemplateMasterId=BTP.Id
						 left join HKP.Process P ON P.Id=BTP.ProcessId
                         LEFT JOIN HKP.SizeGroup SG ON SG.Id=BT.SizeGroupId";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetBulletinTemplateDatabyId(string id)
        {
            try
            {
                string sql = @"Select BT.* ,PM.UserName ProductMaster, SG.UserName SizeGroup
						  ,Buyer=REPLACE(REPLACE(
										 STUFF((select distinct ', '+B.UserName FROM 
                                        [MST].[BulletinTemplateBuyerInfo] BTB 
										JOIN HKP.Buyer B ON B.Id=BTB.BuyerId
                                        WHERE BT.Id=BTB.BulletinTemplateId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')				 
			         	,BuyerItemRefNo=REPLACE(REPLACE(
										STUFF((select distinct ', '+BTB.BuyerStyleRefNo FROM 
                                        [MST].[BulletinTemplateBuyerInfo] BTB 
                                        WHERE BT.Id=BTB.BulletinTemplateId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')
			        	,OwnStyleRefNo=REPLACE(REPLACE(
										STUFF((select distinct ', '+BTB.OwnStyleRefNo FROM 
                                        [MST].[BulletinTemplateBuyerInfo] BTB 
                                        WHERE BT.Id=BTB.BulletinTemplateId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
										,'&amp;','&'), 'amp;', '')										
							
						,P.UserName Process,ISNULL(BTD.TotalSPT,0)TotalSPT,ISNULL(BTD.RequiredManPower,0)RequiredManPower,ISNULL(BTD.AllotedManpower,0)AllotedManpower,ISNULL(BTD.AllotedWorkstation,0)AllotedWorkstation,CEILING(ISNULL(BTD.LineTargetPerHour,0))LineTargetPerHour
                        ,FORMAT(BT.AddedDate,'dd-MMM-yyyy')CreationDate
                         FROM [MST].[BulletinTemplate] BT
                         LEFT JOIN MST.ProductMaster PM ON PM.Id=BT.ProductMasterId
						 left join [MST].[BulletinTemplateMaster] BTP ON BT.Id=BTP.BulletinTemplateId
						 left join (Select BulletinTemplateMasterId, SUM(TotalSPT) TotalSPT,SUM(RequiredManPower) RequiredManPower
						 ,SUM(AllotedManpower) AllotedManpower,SUM(AllotedWorkstation) AllotedWorkstation
						 ,LineTargetPerHour=(((SUM(AllotedManpower) * 60) /NULLIF(SUM(TotalSPT),0)) * (SUM(TotalSPT) /NULLIF(SUM(AllotedManpower),0))/ MAX(NULLIF(AvgAllotedTime,0)))
						 from MST.BulletinTemplateDetail GROUP BY BulletinTemplateMasterId) BTD ON BTD.BulletinTemplateMasterId=BTP.Id
						 left join HKP.Process P ON P.Id=BTP.ProcessId
                         LEFT JOIN HKP.SizeGroup SG ON SG.Id=BT.SizeGroupId Where BT.Id='"+id+"'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetBulletin2ndTemplateData(string ProductionId)
        {
            try
            {
                string sql = @"SELECT
                                moi.MaterialMasterId,moi.ArticleId,
                                fc.CharacteristicsValueId SO1,sc.CharacteristicsValueId SO2,tc.CharacteristicsValueId SO3,
                                c1.UserName AS SOC1,cv1.UserName AS SOCV1,
                                c2.UserName AS SOC2,cv2.UserName AS SOCV2,
                                c3.UserName AS SOC3,cv3.UserName AS SOCV3,
                                CASE WHEN isnull(tc.Id,'')<>'' THEN tc.Qty ELSE
                                CASE WHEN ISNULL(sc.Id,'')<>'' THEN sc.Qty ELSE
                                CASE WHEN ISNULL(fc.Id,'')<>'' THEN fc.Qty ELSE so.Qty END END END AS OrderBreakdownQty
                                ,BPT=(A.TotalSPT/NULLIF(A.AllotedManpower,0)),A.AddedDate,A.AddedBy,A.RequiredStdTarget,A.TotalSPT,A.AllotedManpower, PlanEfficency=(A.RequiredStdTarget/(NULLIF(A.AllotedManpower,0) * 60 / A.TotalSPT)*100)
                                ,PerManProductivity=A.RequiredStdTarget/NULLIF(A.AllotedManpower,0),[Target]=(A.AllotedManpower * 60 / A.TotalSPT),A.PlannedHoursPerDay,A.MCTotalSPT,NMCTotalSPT,A.MCTotalMP,NMCTotalMP
                                
                                FROM trn.SalesOrder AS so
                                INNER JOIN trn.ProductionOrderDetail AS pod ON pod.SalesOrderId=so.Id
                                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=so.MasterOrderItemId
                                LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=moi.MaterialMasterId
                                LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=moi.ArticleId
                                
                                LEFT JOIN trn.FirstCharacteristics AS fc ON fc.SalesOrderId=so.Id
                                LEFT JOIN trn.SecondCharacteristics AS sc ON sc.FirstCharacteristicsId=fc.Id AND sc.SalesOrderId=so.Id
                                LEFT JOIN trn.ThirdCharacteristics AS tc ON tc.SecondCharacteristicsId=sc.Id AND tc.SalesOrderId=so.Id
                                
                                
                                LEFT JOIN hkp.CharacteristicsValue AS cv1 ON cv1.Id=fc.CharacteristicsValueId
                                LEFT JOIN hkp.Characteristics AS c1 ON c1.Id=cv1.CharacteristicsId
                                
                                LEFT JOIN hkp.CharacteristicsValue AS cv2 ON cv2.Id=sc.CharacteristicsValueId
                                LEFT JOIN hkp.Characteristics AS c2 ON c2.Id=cv2.CharacteristicsId
                                
                                LEFT JOIN hkp.CharacteristicsValue AS cv3 ON cv3.Id=tc.CharacteristicsValueId
                                LEFT JOIN hkp.Characteristics AS c3 ON c3.Id=cv3.CharacteristicsId
                                
                                LEFT JOIN trn.ProductionOrder PO ON PO.Id=POD.ProductionOrderId
                                LEFT JOIN (
                                SELECT PB.ProductionOrderId,PBM.RequiredStdTarget,FORMAT(PB.AddedDate,'dd-MMM-yyyy') AddedDate,PB.AddedBy 
                                ,PBM.PlannedHoursPerDay
                                ,SUM(PBD.TotalSPT) TotalSPT,SUM(PBD.AllotedManpower) AllotedManpower,SUM(MCTotalSPT)MCTotalSPT,SUM(NMCTotalSPT)NMCTotalSPT,SUM(MCTotalMP)MCTotalMP,SUM(NMCTotalMP)NMCTotalMP
                                FROM trn.ProductionBulletinTemplate PB
                                LEFT JOIN trn.ProductionBulletinTemplateMaster PBM ON PBM.ProductionBulletinTemplateId=PB.Id
                                LEFT JOIN(
                                Select 
                                PBD.ProductionBulletinTemplateMasterId,
                                SUM(PBD.TotalSPT) TotalSPT,SUM(PBD.AllotedManpower) AllotedManpower
                                ,MCTotalSPT=CASE WHEN ISNULL(PBD.MachineVarientId,'')<>'' THEN SUM(PBD.TotalSPT) ELSE 0 END
                                ,NMCTotalSPT=CASE WHEN ISNULL(PBD.MachineVarientId,'')='' THEN SUM(PBD.TotalSPT) ELSE 0 END
                                ,MCTotalMP=CASE WHEN ISNULL(PBD.MachineVarientId,'')<>'' THEN SUM(PBD.AllotedManpower) ELSE 0 END
                                ,NMCTotalMP=CASE WHEN ISNULL(PBD.MachineVarientId,'')='' THEN SUM(PBD.AllotedManpower) ELSE 0 END
                                 from  trn.ProductionBulletinTemplateDetail PBD
                                 GROUP BY PBD.ProductionBulletinTemplateMasterId,PBD.MachineVarientId
                                 )PBD ON PBD.ProductionBulletinTemplateMasterId=PBM.Id
                                GROUP BY PBM.ProcessId,PB.ProductionOrderId,PBM.RequiredStdTarget,PB.AddedDate,PB.AddedBy,PBM.PlannedHoursPerDay 
                                ) A ON A.ProductionOrderId=PO.Id
                                WHERE pod.ProductionOrderId='" + ProductionId + "'";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetProductionBulletinTemplateReportDataByProductionBulletinTemplateId(string ProductionOrderId)
        {
            var sql = @"SELECT PBT.Id,PBT.ProductionOrderId,PBT.ByWhom,PM.UserName AS ProductMaster,SG.UserName AS SizeGroup, PBT.BulletinName,PBTD.Sequence, p.UserName As Process,OPV.Code OperationCode,OPV.UserName AS OperationVariation,ISNULL(MM.UserName,'Manual') AS MachineMaster, MMA.StandardName AS MachineVarient, S.UserName AS Skill
                
                ,PBTD.OperationGroup,PBTD.AdditionalSPT,ISNULL(PBTD.TotalSPT,0) as TotalSPT,ISNULL(PBTD.AllotedWorkstation,0) as AllotedWorkstation,ISNULL(PBTD.AllotedManpower,0) as AllotedManpower,PBTD.Frequency
                ,FZ.UserName AS FGZone, fgc.UserName AS FGComponent,isnull(PBTD.AvgAllotedTime,0) AS AvgAllotedTime
                ,OT.UserName AS OperationType, OC.UserName AS OperationConsumption, GF.UserName AS GaugeFolder, OCategory.UserName AS OperationCategory,PBTD.QualityLevel,PBM.PlannedHoursPerDay,PBM.RequiredStdTarget, TotalBT=PBM.PlannedHoursPerDay*PBM.RequiredStdTarget

				,MMA.Code MachineCode,PBTD.OperationTargetPerHr,PBTD.RequiredManPower,PBM.ProcessId 
                
                ,OperationSPT=PBTD.TotalSPT-PBTD.AdditionalSPT,MMA.Id MachineVarientId,ShortName=CASE WHEN MMA.ShortName IS NULL THEN 'Manual' ELSE MMA.ShortName END, Machine=CASE WHEN MMA.ShortName IS NULL THEN 'No' ELSE 'Yes' END
                ,ATH.UserName Attachment,PBTD.Remark,PBT.PicFileName,PBT.AddedBy,FORMAT(PBT.AddedDate,'dd-MMM-yyyy') AddedDate
				 ,BulletinBuyerStyleRefNo=STUFF((select distinct ', '+BTB.BuyerStyleRefNo FROM 
                                        [MST].[BulletinTemplateBuyerInfo] BTB 
                                        WHERE PBT.BulletinTemplateId=BTB.BulletinTemplateId for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
				 ,Buyer=    STUFF((select distinct ','+XB.UserName from 
	                                                    trn.SalesOrder XSO 
		                                                    JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
		                                                    left outer join trn.MasterOrderItem XMOI on Xmoi.Id=Xso.MasterOrderItemId
		                                                    left outer join trn.MasterOrder XMO on Xmo.Id=Xmoi.MasterOrderId
		                                                    left outer join [HKP].Buyer XB on XB.Id=XMO.BuyerId
			                                                where Xpod.ProductionOrderId=PBT.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

				,BuyerOrder =  STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PBT.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
                              ,OwnOrder = STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrder XMOI 	 
								                                INNER JOIN  trn.MasterOrderItem MOI ON MOI.MasterOrderId=XMOI.Id	 
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=moi.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PBT.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

			 ,BuyerItem= STUFF((select distinct ','+XMOI.BuyerReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PBT.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')  

			                                             
           ,OwnItem=STUFF((select distinct ','+XMOI.OwnReferenceNo from 
																			trn.MasterOrderItem XMOI 	  
								                                INNER JOIN trn.SalesOrder AS sox ON sox.MasterOrderItemId=XMOI.Id  
								                                INNER JOIN trn.ProductionOrderDetail AS podx ON podx.SalesOrderId=sox.Id                                                
							                                where podx.ProductionOrderId=PBT.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')
		  ,Description= STUFF((select distinct ','+XSO.Description from 
                                                                 trn.SalesOrder XSO 
                                                                 JOIN trn.ProductionOrderDetail AS Xpod ON Xpod.SalesOrderId=Xso.Id
                                                                 WHERE Xpod.ProductionOrderId=PBT.ProductionOrderId	for xml path(''),TYPE).value('.', 'VARCHAR(MAX)'), 1, 1, '')

                FROM TRN.ProductionBulletinTemplate PBT 
                LEFT JOIN TRN.[ProductionBulletinTemplateMaster] PBM ON PBT.Id = PBM.ProductionBulletinTemplateId
                LEFT JOIN TRN.ProductionBulletinTemplateDetail PBTD ON PBM.Id = PBTD.ProductionBulletinTemplateMasterId
                LEFT JOIN HKP.Process p ON p.Id = PBM.ProcessId 
                LEFT JOIN MST.OperationVariation OPV ON OPV.Id = PBTD.OperationVariationId 
                LEFT JOIN MST.MaterialMasterArticle MMA ON MMA.Id = PBTD.MachineVarientId
                LEFT JOIN HKP.FGZone FZ ON FZ.Id = PBTD.FGZoneId 
                LEFT JOIN HKP.FGComponent FGC ON FGC.Id = PBTD.FGComponentId
                LEFT JOIN HKP.OperationType OT ON OT.Id = PBTD.OperationTypeId
                LEFT JOIN HKP.OperationConsumption OC ON OC.Id = PBTD.OperationConsumptionId
                LEFT JOIN HKP.GaugeFolder GF ON GF.Id = PBTD.GaugeFolderId
                LEFT JOIN HKP.OperationCategory OCategory ON OCategory.Id = PBTD.OperationCategoryId
                LEFT JOIN MST.ProductMaster PM ON PM.Id = PBT.ProductMasterId
                LEFT JOIN HKP.SizeGroup SG ON SG.Id = PBT.SizeGroupId
                LEFT JOIN mst.OperationMaster AS om ON om.Id=PBTD.SkillMasterId 
                LEFT JOIN HKP.Skill S ON S.Id = OM.SkillId
                LEFT JOIN MST.MaterialMaster MM ON MM.Id = MMA.MaterialMasterId
				LEFT JOIN HKP.Attachment ATH ON ATH.Id = PBTD.AttachmentId
				where PBT.ProductionOrderId = '" + ProductionOrderId + "'  order by p.UserName,PBTD.Sequence";

            return _sqlRepository.GetDataTable(sql);
        }

        public DataTable GetThreadConsumptionSummaryData(string bulletinTemplateMasterId)
        {
            try
            {
                string strSQL = string.Empty;
                strSQL = @"SELECT SUM(A.NeedleConsumption) NeedleConsumption, SUM(A.BobbinConsumption) BobbinConsumption, SUM(A.LooperConsumption) LooperConsumption,A.ArticleId, A.Thread
                                FROM 
                                (
                                SELECT BTD.NeedleArticleId ArticleId, NMA.ShortName Thread,SUM(BTD.NeedleConsumption) NeedleConsumption,0 BobbinConsumption, 0 LooperConsumption 
                                FROM MST.BulletinTemplateDetail BTD 
                                LEFT JOIN MST.MaterialMaster NM ON NM.Id=BTD.NeedleMaterialMasterId
                                JOIN MST.MaterialMasterArticle NMA ON NMA.Id=BTD.NeedleArticleId
                                WHERE BulletinTemplateMasterId='" + bulletinTemplateMasterId + @"' AND ISNULL(BTD.MachineVarientId,'')<>''
                                GROUP BY BTD.NeedleArticleId, NMA.ShortName,BTD.BobbinArticleId
                                UNION ALL

                                select BTD.BobbinArticleId, BMA.ShortName BobbinArticle,0 NeedleConsumption,SUM(BTD.BobbinConsumption) BobbinConsumption, 0 LooperConsumption 
                                from MST.BulletinTemplateDetail BTD 
                                LEFT JOIN MST.MaterialMaster BM ON BM.Id=BTD.BobbinMaterialMasterId
                                JOIN MST.MaterialMasterArticle BMA ON BMA.Id=BTD.BobbinArticleId
                                Where BulletinTemplateMasterId='" + bulletinTemplateMasterId + @"' AND ISNULL(BTD.MachineVarientId,'')<>''
                                GROUP BY BTD.BobbinArticleId, BMA.ShortName
                                UNION ALL
                                select BTD.LooperArticleId, LMA.ShortName LooperArticle,0 NeedleConsumption,0 BobbinConsumption,SUM(BTD.LooperConsumption) LooperConsumption
                                from MST.BulletinTemplateDetail BTD 
                                LEFT JOIN MST.MaterialMaster LM ON LM.Id=BTD.LooperMaterialMasterId
                                JOIN MST.MaterialMasterArticle LMA ON LMA.Id=BTD.LooperArticleId
                                Where BulletinTemplateMasterId='" + bulletinTemplateMasterId + @"' AND ISNULL(BTD.MachineVarientId,'')<>''
                                GROUP BY BTD.LooperArticleId, LMA.ShortName
                                ) AS A 
                                GROUP BY A.ArticleId, A.Thread";
                return _sqlRepository.GetDataTable(strSQL);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public DataTable GetThreadConsumptionData(string bulletinTemplateMasterId)
        {
            try
            {
                string sql = @"SELECT OPP.Operation
	                                ,OV.Code OperationCode
	                                ,OV.UserName OperationVariation
	                                ,MMA.StandardName MachineName
	                                ,MM.UserName MaterialMaster
	                                ,SK.UserName AS SkillName
	                                ,BTD.SPI
	                                ,BTD.NoOfStitch
	                                ,BTD.OperationLength
	                                ,BTD.StitchCodeId
	                                ,BTD.FabricWidth
	                                ,BTD.NeedleDescription
	                                ,BTD.NeedleMaterialMasterId
	                                ,MMN.UserName NeedleMaterialMaster
	                                ,BTD.NeedleArticleId
	                                ,MMNA.ShortName NeedleArticle
	                                ,BTD.BobbinDescription
	                                ,BTD.BobbinMaterialMasterId
	                                ,MMB.UserName BobbinMaterialMaster
	                                ,BTD.BobbinArticleId
	                                ,MMBA.ShortName BobbinArticle
	                                ,BTD.LooperDescription
	                                ,BTD.LooperMaterialMasterId
	                                ,MML.UserName LooperMaterialMaster
	                                ,BTD.LooperArticleId
	                                ,MMLA.ShortName LooperArticle
	                                ,SC.userName StitchCode
	                                ,BTD.SPIConsumption
	                                ,BTD.NeedleConsumption
	                                ,BTD.BobbinConsumption
	                                ,BTD.LooperConsumption
	                                ,BTD.Consumption
	                                ,SC.Needle
	                                ,SC.Bobbin
	                                ,SC.Looper
	                                ,BTD.WastagePercentage
	                                ,BTD.ExtraOrderPercentage
	                                ,TotalWastagePercentage = (BTD.WastagePercentage + BTD.ExtraOrderPercentage)
                                FROM [MST].[BulletinTemplateDetail] BTD
                                LEFT JOIN [MST].[OperationVariation] OV ON OV.Id = BTD.OperationVariationId
                                LEFT JOIN (
	                                SELECT OP.Id,OP.UserName Operation,ISNULL(OP.BasicProcessTime, 0) AS BasicProcessTime
		                                ,ISNULL(OP.AssociateProcessTime, 0) AS AssociateProcessTime,ISNULL(OP.PersonalAllowance, 0) AS PersonalAllowance
		                                ,ISNULL(OP.MachineAllowance, 0) AS MachineAllowance,OP.Frequency,OP.SPI
	                                FROM [MST].[Operation] OP
	                                ) OPP ON OPP.Id = OV.OperationId
                                LEFT JOIN HKP.FGZone FZ ON FZ.Id = BTD.FGZoneId
                                LEFT JOIN HKP.FGComponent FC ON FC.Id = BTD.FGComponentId
                                LEFT JOIN HKP.Attachment A ON A.Id = BTD.AttachmentId
                                LEFT JOIN HKP.GaugeFolder GF ON GF.Id = BTD.GaugeFolderId
                                LEFT JOIN HKP.OperationConsumption OC ON OC.Id = BTD.OperationConsumptionId
                                LEFT JOIN HKP.OperationType OT ON OT.Id = BTD.OperationTypeId
                                LEFT JOIN [MST].[MaterialMasterArticle] MMA ON MMA.Id = BTD.MachineVarientId
                                LEFT JOIN [MST].[MaterialMaster] MM ON MM.Id = MMA.MaterialMasterId
                                LEFT JOIN [HKP].[Skill] AS SK ON BTD.SkillId = Sk.Id
                                LEFT JOIN [HKP].StitchCode AS SC ON BTD.StitchCodeId = SC.Id
                                LEFT JOIN [MST].[MaterialMaster] MMN ON MMN.Id = BTD.NeedleMaterialMasterId
                                LEFT JOIN [MST].[MaterialMasterArticle] MMNA ON MMNA.Id = BTD.NeedleArticleId
                                LEFT JOIN [MST].[MaterialMaster] MMB ON MMB.Id = BTD.BobbinMaterialMasterId
                                LEFT JOIN [MST].[MaterialMasterArticle] MMBA ON MMBA.Id = BTD.BobbinArticleId
                                LEFT JOIN [MST].[MaterialMaster] MML ON MML.Id = BTD.LooperMaterialMasterId
                                LEFT JOIN [MST].[MaterialMasterArticle] MMLA ON MMLA.Id = BTD.LooperArticleId
                                WHERE BTD.BulletinTemplateMasterId = '" + bulletinTemplateMasterId + @"'AND MM.Id <> '' ORDER BY BTD.Sequence";
                return _sqlRepository.GetDataTable(sql);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



    }
}


