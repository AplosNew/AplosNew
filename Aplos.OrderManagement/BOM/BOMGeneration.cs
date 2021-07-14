using Library.Crosscutting.Security;
using Library.Data.Sql;
using OTSBD;
using Syncfusion.XlsIO;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace Library.OrderManagement.BOM
{

    public class BOMGeneration
    {
        //tarek talukder
        SqlRepository _sqlRepository;
        ConnectionManager.clsConnectionManager ConManager;
        CustomIdentity identity;
        DataTable dtFGMapping = new DataTable("FGMAP");
        public BOMGeneration()
        {
            _sqlRepository = new SqlRepository();
            ConManager = new ConnectionManager.clsConnectionManager();
            try
            {
                identity = (CustomIdentity)Thread.CurrentPrincipal.Identity;
            }
            catch (Exception ex)
            {

            }
        }

        private string sqlGetCommonComponents(string MasterOrderItemId)
        {


            string sql = @"
                         SELECT '' AS ParentKey, bd.Id AS ParentId,BD.IsDestinationSpecific,BD.IsPOSpecific,
                       convert(bit,isnull( bd.SalesOrderSpecificMaterial,0)) AS SalesOrderSpecificMaterial,CASE WHEN ISNULL(BD.IsPOSpecific,0)=1 THEN SO.CustomerPOId ELSE NULL END AS CustomerPOId,BAD.DestinationId, bi.Id,so.Id AS SalesOrderId, bd.RMMaterialMasterId, bd.RMArticleId, bd.[Description] AS RMDescription, bd.CustomerSpec AS RMCustomerSpec,
                       bd.VendorSpec AS RMVendorSpec,bd.ProcessId, bd.Consumption,bd.WastagePer,bd.Sequence,
                        bd.UoMId, mm.BaseUOMId,mm.PurchaseOrderUOMId AS POUoMId,bd.VendorId,
                       fc.CharacteristicsValueId AS SO1,sc.CharacteristicsValueId AS SO2,tc.CharacteristicsValueId AS SO3,
                       
                      
                                    CASE WHEN isnull(tc.Id,'')<>'' THEN tc.Qty ELSE 
                                   CASE WHEN ISNULL(sc.Id,'')<>'' THEN sc.Qty ELSE
                                   CASE WHEN ISNULL(fc.Id,'')<>'' THEN fc.Qty ELSE so.Qty END END END AS OrderQty,
                                
                        CEILING((isnull(
                                    CASE WHEN isnull(tc.Id,'')<>'' THEN tc.Qty ELSE 
                                   CASE WHEN ISNULL(sc.Id,'')<>'' THEN sc.Qty ELSE
                                   CASE WHEN ISNULL(fc.Id,'')<>'' THEN fc.Qty ELSE so.Qty END END END,0)
                                   *(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0)))) AS PlanOrderQty,
                        convert(bit,CASE WHEN ISNULL(adc.Id,'')<>'' THEN 1 ELSE 0 END) AS isParent, convert(bit,0) AS isChild,
      	                --Raw material characteristics      
                       bd.FirstCharacteristicsValueId RMC1, bd.SecondCharacteristicsValueId RMC2,bd.ThirdCharacteristicsValueId RMC3,'' AS SKUDesc, BD.IsSKUCommon
         
                 FROM BOMMasterAttachmentWithItem AS BI 
                INNER JOIN BOMAttachmentDetail AS bd ON bi.Id=bd.BOMMasterAttachmentWithItemId
                LEFT JOIN BOMAttachmentDestination BAD ON BAD.BOMAttachmentDetailId=BD.Id
                INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=bi.MasterOrderItemId
                LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=bd.RMMaterialMasterId
               
                INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=moi.Id AND CASE WHEN isnull(BD.IsDestinationSpecific,0)=1 THEN isnull(BAD.DestinationId,'') else isnull(SO.DestinationId,'') END=isnull(SO.DestinationId,'')
				LEFT JOIN trn.FirstCharacteristics AS fc ON fc.SalesOrderId=so.Id
				LEFT JOIN trn.SecondCharacteristics AS sc ON sc.FirstCharacteristicsId=fc.Id AND sc.SalesOrderId=so.Id
				LEFT JOIN trn.ThirdCharacteristics AS tc ON tc.SecondCharacteristicsId=sc.Id AND tc.SalesOrderId=so.Id
                
                
                LEFT JOIN AttachmentDetailConsumption AS adc ON adc.BOMAttachmentDetailId=bd.Id 
                AND adc.Id=(SELECT TOP 1 Id FROM AttachmentDetailConsumption WHERE BOMAttachmentDetailId=bd.Id)

                        WHERE  bd.IsSKUCommon=1 AND bi.MasterOrderItemId='" + MasterOrderItemId + @"'
                        ORDER BY bd.RMArticleId";

            return sql;
        }
        private string sqlGetMappedComponents(string MasterOrderItemId)
        {

            string sql = @"SELECT '' AS ParentKey, bd.Id AS ParentId,BD.IsDestinationSpecific,BD.IsPOSpecific,BAD.DestinationId, bi.BOMMasterId,b.[Description] AS BOMDesc,  bi.Id,bd.id AS BOMAttachmentDetailId,SO.Id AS SalesOrderId, bd.RMMaterialMasterId,mm.UserName AS Material, bd.RMArticleId,mma.StandardName AS Article, bd.[Description] AS RMDescription, bd.CustomerSpec AS RMCustomerSpec,
                    convert(bit,isnull( bd.SalesOrderSpecificMaterial,0)) AS SalesOrderSpecificMaterial,CASE WHEN ISNULL(BD.IsPOSpecific,0)=1 THEN SO.CustomerPOId ELSE NULL END AS CustomerPOId,                                   
                    bd.VendorSpec AS RMVendorSpec,bd.ProcessId, bd.Consumption,bd.WastagePer,
    bd.UoMId, mm.BaseUOMId,mm.PurchaseOrderUOMId AS POUoMId,bd.VendorId,bd.Sequence,
    isnull(bd.ConsumptionSpecificToSKU1,0) ConsumptionSpecificToSKU1,isnull(bd.ConsumptionSpecificToSKU2,0) ConsumptionSpecificToSKU2,isnull(bd.ConsumptionSpecificToSKU3,0) ConsumptionSpecificToSKU3,
      CASE WHEN ISNULL(adc.Id,'')<>'' THEN 1 ELSE 0 END AS isParent,convert(bit,0) AS isChild,
      --determine how many char the material has
      MATC1.CharacteristicsId AS MATC1,  MATC2.CharacteristicsId AS MATC2,  MATC3.CharacteristicsId AS MATC3,
      
      
--fg mapping data from BOM Template
     mp1.FGFirstCharacteristicsValueId AS CHARMAP1,     mp1.FGSecondCharacteristicsValueId AS CHARMAP2,     mp1.FGThirdCharacteristicsValueId AS CHARMAP3,

      	                            --Raw material characteristics      
                                   bd.FirstCharacteristicsValueId RMC1, bd.SecondCharacteristicsValueId RMC2,bd.ThirdCharacteristicsValueId RMC3,
     
		                            --Raw material mapped characteristics      
--CASE WHEN ISNULL(bm1C.RMFirstCharacteristicsValueId,'')<>'' THEN bm1C.RMFirstCharacteristicsValueId ELSE CASE WHEN isnull(bm1.Id,'')<>'' THEN bm1.RMFirstCharacteristicsValueId ELSE CASE WHEN isnull(bm2.id,'')<>''  THEN  bm2.RMFirstCharacteristicsValueId ELSE bm3.RMFirstCharacteristicsValueId END END END SKURMC1,
--CASE WHEN ISNULL(bm2C.RMSecondCharacteristicsValueId,'')<>'' THEN bm2C.RMSecondCharacteristicsValueId ELSE CASE WHEN isnull(bm1.Id,'')<>'' THEN bm1.RMSecondCharacteristicsValueId ELSE CASE WHEN isnull(bm2.id,'')<>''  THEN  bm2.RMSecondCharacteristicsValueId ELSE bm3.RMSecondCharacteristicsValueId END END END SKURMC2,
--CASE WHEN ISNULL(bm3C.RMThirdCharacteristicsValueId,'')<>'' THEN bm3C.RMThirdCharacteristicsValueId ELSE CASE WHEN isnull(bm1.Id,'')<>'' THEN bm1.RMThirdCharacteristicsValueId ELSE CASE WHEN isnull(bm2.id,'')<>''  THEN  bm2.RMThirdCharacteristicsValueId ELSE bm3.RMThirdCharacteristicsValueId END END END SKURMC3,

CASE WHEN ISNULL(bm1C.RMFirstCharacteristicsValueId,'')<>'' THEN bm1C.RMFirstCharacteristicsValueId ELSE CASE WHEN isnull(bm1.RMFirstCharacteristicsValueId,'')<>'' THEN bm1.RMFirstCharacteristicsValueId ELSE CASE WHEN isnull(bm2.RMFirstCharacteristicsValueId,'')<>''  THEN  bm2.RMFirstCharacteristicsValueId ELSE bm3.RMFirstCharacteristicsValueId END END END SKURMC1,
CASE WHEN ISNULL(bm2C.RMSecondCharacteristicsValueId,'')<>'' THEN bm2C.RMSecondCharacteristicsValueId ELSE CASE WHEN isnull(bm1.RMSecondCharacteristicsValueId,'')<>'' THEN bm1.RMSecondCharacteristicsValueId ELSE CASE WHEN isnull(bm2.RMSecondCharacteristicsValueId,'')<>''  THEN  bm2.RMSecondCharacteristicsValueId ELSE bm3.RMSecondCharacteristicsValueId END END END SKURMC2,
CASE WHEN ISNULL(bm3C.RMThirdCharacteristicsValueId,'')<>'' THEN bm3C.RMThirdCharacteristicsValueId ELSE CASE WHEN isnull(bm1.RMThirdCharacteristicsValueId,'')<>'' THEN bm1.RMThirdCharacteristicsValueId ELSE CASE WHEN isnull(bm2.RMThirdCharacteristicsValueId,'')<>''  THEN  bm2.RMThirdCharacteristicsValueId ELSE bm3.RMThirdCharacteristicsValueId END END END SKURMC3,

  -- ISNULL(bm1c.[Description],ISNULL(bm2c.[Description],ISNULL(bm3.[Description],ISNULL(bm1.[Description],isnull(bm2.[Description],bm3.[Description]))))) AS SKUDesc,
     CONCAT(  
    CASE WHEN isnull(bm1c.[Description],'')<>'' THEN ' '+bm1c.[Description] ELSE '' END,
    CASE WHEN isnull(bm2c.[Description],'')<>'' THEN ' '+bm2c.[Description] ELSE '' END,
    CASE WHEN isnull(bm3c.[Description],'')<>'' THEN ' '+bm3c.[Description] ELSE '' END,
    CASE WHEN isnull(bm1.[Description],'')<>'' THEN ' '+bm1.[Description] ELSE '' END,
    CASE WHEN isnull(bm2.[Description],'')<>'' THEN ' '+bm2.[Description] ELSE '' END,
    CASE WHEN isnull(bm3.[Description],'')<>'' THEN ' '+bm3.[Description] ELSE '' END) AS SKUDesc,
       
                                   BD.IsSKUCommon,
                                    bm1.FGFirstCharacteristicsValueId B1, bm2.FGSecondCharacteristicsValueId B2,bm3.FGThirdCharacteristicsValueId B3,
       
                                   --sales order characretistics without mapped with bom template
                                   fc.CharacteristicsValueId SO1,sc.CharacteristicsValueId SO2,tc.CharacteristicsValueId SO3,
       c1.UserName AS SOC1,cv1.UserName AS SOCV1,
       c2.UserName AS SOC2,cv2.UserName AS SOCV2,
       c3.UserName AS SOC3,cv3.UserName AS SOCV3,
      
                                    CASE WHEN isnull(tc.Id,'')<>'' THEN tc.Qty ELSE 
                                   CASE WHEN ISNULL(sc.Id,'')<>'' THEN sc.Qty ELSE
                                   CASE WHEN ISNULL(fc.Id,'')<>'' THEN fc.Qty ELSE so.Qty END END END AS OrderBreakdownMappedQty,
       
                             
                            CEILING((isnull( CASE WHEN isnull(tc.Id,'')<>'' THEN tc.Qty ELSE 
                                   CASE WHEN ISNULL(sc.Id,'')<>'' THEN sc.Qty ELSE
                                   CASE WHEN ISNULL(fc.Id,'')<>'' THEN fc.Qty ELSE so.Qty END END END,0)
                                   *(1+( isnull(moi.ExtraOrderPercentage,0)/100)))*(100/(100-isnull(moi.OrderWastagePercentage,0)))) AS OrderBreakdownMappedPlannedQty
       
                             
     
     
                             FROM BOMMasterAttachmentWithItem AS BI 
                             INNER JOIN BOMMaster AS b ON b.Id=bi.BOMMasterId
                            INNER JOIN BOMAttachmentDetail AS bd ON bi.Id=bd.BOMMasterAttachmentWithItemId
                            LEFT JOIN BOMAttachmentDestination BAD ON BAD.BOMAttachmentDetailId=BD.Id
                            LEFT JOIN BOMAttachmentSKUMapping MP1 ON mp1.BOMAttachmentDetailId=bd.Id AND mp1.Id=(SELECT TOP 1 Id FROM BOMAttachmentSKUMapping WHERE BOMAttachmentDetailId=bd.Id)
                            LEFT JOIN AttachmentDetailConsumption AS adc ON adc.BOMAttachmentDetailId=bd.Id 
                                        AND adc.Id=(SELECT TOP 1 Id FROM AttachmentDetailConsumption WHERE BOMAttachmentDetailId=bd.Id)

							--material characteristics
							LEFT JOIN (SELECT mm.CharacteristicsId,mm.MaterialMasterId,
										 DENSE_RANK() OVER (PARTITION BY mm.MaterialMasterId ORDER BY mm.Sequence) AS Seq
										 FROM mst.MaterialMasterCharacteristics AS mm ) AS MATC1 ON matc1.MaterialMasterId=bd.RMMaterialMasterId AND MATC1.Seq=1


							LEFT JOIN (SELECT mm.CharacteristicsId,mm.MaterialMasterId,
										 DENSE_RANK() OVER (PARTITION BY mm.MaterialMasterId ORDER BY mm.Sequence) AS Seq
										 FROM mst.MaterialMasterCharacteristics AS mm ) AS MATC2 ON matc2.MaterialMasterId=bd.RMMaterialMasterId AND MATC2.Seq=2


							LEFT JOIN (SELECT mm.CharacteristicsId,mm.MaterialMasterId,
										 DENSE_RANK() OVER (PARTITION BY mm.MaterialMasterId ORDER BY mm.Sequence) AS Seq
										 FROM mst.MaterialMasterCharacteristics AS mm ) AS MATC3 ON matc3.MaterialMasterId=bd.RMMaterialMasterId AND MATC3.Seq=3


                            INNER JOIN trn.MasterOrderItem AS moi ON moi.Id=bi.MasterOrderItemId
                            INNER JOIN trn.SalesOrder AS so ON so.MasterOrderItemId=moi.Id AND CASE WHEN isnull(BD.IsDestinationSpecific,0)=1 THEN isnull(BAD.DestinationId,'') else isnull(SO.DestinationId,'') END=isnull(SO.DestinationId,'')
                            
                            LEFT JOIN mst.MaterialMaster AS mm ON mm.Id=bd.RMMaterialMasterId
                            LEFT JOIN mst.MaterialMasterArticle AS mma ON mma.Id=bd.RMArticleId

                            LEFT JOIN trn.FirstCharacteristics AS fc ON fc.SalesOrderId=so.Id
                            LEFT JOIN trn.SecondCharacteristics AS sc ON sc.FirstCharacteristicsId=fc.Id AND sc.SalesOrderId=so.Id
                            LEFT JOIN trn.ThirdCharacteristics AS tc ON tc.SecondCharacteristicsId=sc.Id AND tc.SalesOrderId=so.Id

                       
							LEFT JOIN hkp.CharacteristicsValue AS cv1 ON cv1.Id=fc.CharacteristicsValueId
							LEFT JOIN hkp.Characteristics AS c1 ON c1.Id=cv1.CharacteristicsId
							
							LEFT JOIN hkp.CharacteristicsValue AS cv2 ON cv2.Id=sc.CharacteristicsValueId
							LEFT JOIN hkp.Characteristics AS c2 ON c2.Id=cv2.CharacteristicsId
							
							LEFT JOIN hkp.CharacteristicsValue AS cv3 ON cv3.Id=tc.CharacteristicsValueId
							LEFT JOIN hkp.Characteristics AS c3 ON c3.Id=cv3.CharacteristicsId
							
                       
                            LEFT JOIN BOMAttachmentSKUMapping BM1 ON bm1.BOMAttachmentDetailId=bd.Id  AND bm1.FGFirstCharacteristicsValueId=fc.CharacteristicsValueId
                                        AND bm1.Id=(SELECT TOP 1 Id FROM BOMAttachmentSKUMapping BM1 WHERE bm1.BOMAttachmentDetailId=bd.Id  AND bm1.FGFirstCharacteristicsValueId=fc.CharacteristicsValueId)
							
							LEFT JOIN BOMAttachmentSKUMapping BM2 ON bm2.BOMAttachmentDetailId=bd.Id  AND bm2.FGSecondCharacteristicsValueId=sc.CharacteristicsValueId
                                        AND bm2.Id=(SELECT TOP 1 Id FROM BOMAttachmentSKUMapping BM2 WHERE bm2.BOMAttachmentDetailId=bd.Id  AND bm2.FGSecondCharacteristicsValueId=sc.CharacteristicsValueId)
						
							LEFT JOIN BOMAttachmentSKUMapping BM3 ON bm3.BOMAttachmentDetailId=bd.Id  AND bm3.FGThirdCharacteristicsValueId=tc.CharacteristicsValueId
                                        AND bm3.Id=(SELECT TOP 1 Id FROM BOMAttachmentSKUMapping BM3 WHERE bm3.BOMAttachmentDetailId=bd.Id  AND bm3.FGThirdCharacteristicsValueId=tc.CharacteristicsValueId)
						
							
                            LEFT JOIN BOMAttachmentSKUMapping BM1C ON bm1C.BOMAttachmentDetailId=bd.Id  
                                       AND bm1c.Id=(SELECT TOP 1 Id FROM BOMAttachmentSKUMapping BM11 WHERE bm11.BOMAttachmentDetailId=bd.Id  AND ISNULL(BM11.IsFirstCharacteristicCommon,0)=1)
							
							LEFT JOIN BOMAttachmentSKUMapping BM2c ON bm2c.BOMAttachmentDetailId=bd.Id 
                                       AND bm2c.Id=(SELECT TOP 1 Id FROM BOMAttachmentSKUMapping BM22 WHERE bm22.BOMAttachmentDetailId=bd.Id  AND ISNULL(BM22.IsSecondCharacteristicCommon,0)=1)
						
							LEFT JOIN BOMAttachmentSKUMapping BM3c ON bm3c.BOMAttachmentDetailId=bd.Id
                                       AND bm3c.Id=(SELECT TOP 1 Id FROM BOMAttachmentSKUMapping BM33 WHERE bm33.BOMAttachmentDetailId=bd.Id  AND ISNULL(BM33.IsThirdCharacteristicCommon,0)=1)
                            

                            WHERE isnull(bd.IsSKUCommon,0)=0  AND BI.MasterOrderItemId='" + MasterOrderItemId + @"'

                            
                            and isnull(fc.CharacteristicsValueId,'')=case when isnull(bd.ConsumptionSpecificToSKU1,0)=1 then isnull(bm1.FGFirstCharacteristicsValueId,'') else isnull(fc.CharacteristicsValueId,'') end
							and isnull(sc.CharacteristicsValueId,'')=case when isnull(bd.ConsumptionSpecificToSKU2,0)=1 then isnull(bm1.FGSecondCharacteristicsValueId,'') else isnull(sc.CharacteristicsValueId,'') end
							and isnull(tc.CharacteristicsValueId,'')=case when isnull(bd.ConsumptionSpecificToSKU3,0)=1 then isnull(bm1.FGThirdCharacteristicsValueId,'') else isnull(tc.CharacteristicsValueId,'') end
                           
                            ORDER BY bd.Id,SO.Id";

            return sql;
        }


        private string sqlGetCommonComponentsForSubMaterialCommon(string MasterOrderItemId)
        {

            string sql = @"
                     SELECT " + BOMComparingStringCommonKey() + @" ,Parent.SalesOrderId, bd.RMMaterialMasterId, bd.RMArticleId, bd.[Description],convert(bit,0) isParent,convert(bit,1) isChild,
       bd.CustomerSpec, bd.VendorSpec, bd.Consumption, bd.UoMId, bd.ProcessId,bd.Sequence,bd.WastagePer, mm.BaseUOMId,mm.PurchaseOrderUOMId AS POUoMId,
       PARENT.SO1,PARENT.SO2,PARENT.SO3,
        PARENT.IsDestinationSpecific,PARENT.IsPOSpecific, PARENT.SalesOrderSpecificMaterial,PARENT.CustomerPOId,PARENT.DestinationId,
       bd.VendorId,bd.FirstCharacteristicsValueId RMC1, bd.SecondCharacteristicsValueId RMC2, bd.ThirdCharacteristicsValueId RMC3, '' AS SKUDesc,
       parent.OrderQty*parent.consumption*(1+(parent.WastagePer/100)) AS OrderQty,
       parent.PlanOrderQty*parent.consumption*(1+(parent.WastagePer/100)) AS PlanOrderQty   
        FROM (" + sqlGetCommonComponents(MasterOrderItemId).Replace("ORDER BY bd.RMArticleId", "").ToString() + @") AS PARENT
                        JOIN AttachmentDetailConsumption bd ON bd.BOMAttachmentDetailId = parent.ParentId
                LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=bd.RMMaterialMasterId

            WHERE bd.IsSKUCommon = 1
            order by bd.RMArticleId";

            return sql;
        }
        private string sqlGetCommonComponentsForSubMaterialMapped(string MasterOrderItemId)
        {

            string sql = @"
                     SELECT " + BOMComparingStringCommonKey() + @" ,Parent.SalesOrderId, bd.RMMaterialMasterId, bd.RMArticleId, bd.[Description],convert(bit,0) isParent,convert(bit,1) isChild,
        PARENT.SO1,PARENT.SO2,PARENT.SO3,       mm.BaseUOMId,mm.PurchaseOrderUOMId AS POUoMId, 
 PARENT.IsDestinationSpecific,PARENT.IsPOSpecific, PARENT.SalesOrderSpecificMaterial,PARENT.CustomerPOId,PARENT.DestinationId,
        bd.CustomerSpec, bd.VendorSpec, bd.Consumption, bd.UoMId, bd.ProcessId,bd.Sequence,bd.WastagePer,
       bd.VendorId,M.SubFirstCharacteristicsValueId RMC1, m.SubSecondCharacteristicsValueId RMC2, m.SubThirdCharacteristicsValueId RMC3, m.[Description] AS SKUDesc,
       parent.OrderQty*parent.consumption*(1+(parent.WastagePer/100)) AS OrderQty,
       parent.PlanOrderQty*parent.consumption*(1+(parent.WastagePer/100)) AS PlanOrderQty   
        FROM (" + sqlGetCommonComponents(MasterOrderItemId).Replace("ORDER BY bd.RMArticleId", "").ToString() + @") AS PARENT
                        JOIN AttachmentDetailConsumption bd ON bd.BOMAttachmentDetailId = parent.ParentId
                        JOIN AttachmentDetailConsumptionSKUMapping AS M ON m.AttachmentDetailConsumptionId=bd.Id  
                LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=bd.RMMaterialMasterId

        WHERE bd.IsSKUCommon = 0
            order by bd.RMArticleId";

            return sql;
        }

        private string sqlGetMappedComponentsForSubMaterialCommon(string MasterOrderItemId)
        {

            string sql = @"
                     SELECT " + BOMComparingStringMappedKey() + @" ,bd.RMMaterialMasterId, bd.RMArticleId, bd.[Description],convert(bit,0) isParent,convert(bit,1) isChild,
       bd.CustomerSpec, bd.VendorSpec, bd.Consumption, bd.UoMId, bd.ProcessId,bd.Sequence,bd.WastagePer,  Parent.SalesOrderId,
        PARENT.SO1,PARENT.SO2,PARENT.SO3, mm.BaseUOMId,mm.PurchaseOrderUOMId AS POUoMId,
        PARENT.IsDestinationSpecific,PARENT.IsPOSpecific, PARENT.SalesOrderSpecificMaterial,PARENT.CustomerPOId,PARENT.DestinationId,
       bd.VendorId,bd.FirstCharacteristicsValueId RMC1, bd.SecondCharacteristicsValueId RMC2, bd.ThirdCharacteristicsValueId RMC3, '' AS SKUDesc,
        parent.OrderBreakdownMappedQty*parent.consumption*(1+(parent.WastagePer/100)) AS OrderQty,
        parent.OrderBreakdownMappedPlannedQty*parent.consumption*(1+(parent.WastagePer/100)) AS PlanOrderQty     
        FROM (" + sqlGetMappedComponents(MasterOrderItemId).Replace("ORDER BY bd.Id,SO.Id", "").ToString() + @") AS PARENT
                        JOIN AttachmentDetailConsumption bd ON bd.BOMAttachmentDetailId = parent.ParentId
                LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=bd.RMMaterialMasterId

            WHERE bd.IsSKUCommon = 1
            order by bd.RMArticleId";

            return sql;
        }
        private string sqlGetMappedComponentsForSubMaterialMapped(string MasterOrderItemId)
        {

            string sql = @"
                     SELECT " + BOMComparingStringMappedKey() + @" , bd.RMMaterialMasterId, bd.RMArticleId, bd.[Description],convert(bit,0) isParent,convert(bit,1) isChild,
       bd.CustomerSpec, bd.VendorSpec, bd.Consumption, bd.UoMId, bd.ProcessId,bd.Sequence,bd.WastagePer,  Parent.SalesOrderId,
        PARENT.SO1,PARENT.SO2,PARENT.SO3, bd.VendorId, mm.BaseUOMId,mm.PurchaseOrderUOMId AS POUoMId,
PARENT.IsDestinationSpecific,PARENT.IsPOSpecific, PARENT.SalesOrderSpecificMaterial,PARENT.CustomerPOId,PARENT.DestinationId,
                  ISNULL(M1.SubFirstCharacteristicsValueId, M.SubFirstCharacteristicsValueId) RMC1,
                  ISNULL(M2.SubSecondCharacteristicsValueId,m.SubSecondCharacteristicsValueId) RMC2, 
                  ISNULL(M3.SubThirdCharacteristicsValueId,m.SubThirdCharacteristicsValueId) RMC3, m.[Description] AS SKUDesc,
                  parent.OrderBreakdownMappedQty*parent.consumption*(1+(parent.WastagePer/100)) AS OrderQty,
                  parent.OrderBreakdownMappedPlannedQty*parent.consumption*(1+(parent.WastagePer/100)) AS PlanOrderQty      
        FROM (" + sqlGetMappedComponents(MasterOrderItemId).Replace("ORDER BY bd.Id,SO.Id", "").ToString() + @") AS PARENT
                        JOIN BOMAttachmentDetail AS D ON d.Id=PARENT.ParentId
                        JOIN BOMAttachmentSKUMapping AS bs ON bs.BOMAttachmentDetailId=d.Id
						and isnull(parent.SKURMC1,'')=isnull(bs.RMFirstCharacteristicsValueId,'')
						and isnull(parent.SKURMC2,'')=isnull(bs.RMSecondCharacteristicsValueId,'')
						and isnull(parent.SKURMC3,'')=isnull(bs.RMThirdCharacteristicsValueId,'')
                        
                        JOIN AttachmentDetailConsumption bd ON bd.BOMAttachmentDetailId = parent.ParentId
						left JOIN AttachmentDetailConsumptionSKUMapping AS M ON m.AttachmentDetailConsumptionId=bd.Id 
								AND (isnull(m.RMFirstCharacteristicsValueId,'')=isnull(bs.RMFirstCharacteristicsValueId,'')
                                    OR isnull(m.RMFirstCharacteristicsValueId,'')=isnull(bs.RMSecondCharacteristicsValueId,'')
                                    OR isnull(m.RMFirstCharacteristicsValueId,'')=isnull(bs.RMSecondCharacteristicsValueId,''))
                                AND (isnull(m.RMSecondCharacteristicsValueId,'')=isnull(bs.RMFirstCharacteristicsValueId,'')
                                    OR isnull(m.RMSecondCharacteristicsValueId,'')=isnull(bs.RMSecondCharacteristicsValueId,'')
                                    OR isnull(m.RMSecondCharacteristicsValueId,'')=isnull(bs.RMSecondCharacteristicsValueId,''))
                                AND (isnull(m.RMThirdCharacteristicsValueId,'')=isnull(bs.RMFirstCharacteristicsValueId,'')
                                    OR isnull(m.RMThirdCharacteristicsValueId,'')=isnull(bs.RMSecondCharacteristicsValueId,'')
                                    OR isnull(m.RMThirdCharacteristicsValueId,'')=isnull(bs.RMSecondCharacteristicsValueId,''))
								--AND isnull(m.RMSecondCharacteristicsValueId,'')=isnull(bs.RMSecondCharacteristicsValueId,'')
								--AND isnull(m.RMThirdCharacteristicsValueId,'')=isnull(bs.RMThirdCharacteristicsValueId,'')
								
						LEFT JOIN 	AttachmentDetailConsumptionSKUMapping M1 ON M1.AttachmentDetailConsumptionId=bd.Id 
						AND M1.Id=(SELECT TOP 1 Id FROM AttachmentDetailConsumptionSKUMapping M WHERE m.AttachmentDetailConsumptionId=bd.Id  AND ISNULL(M.IsFirstCharacteristicCommon,0)=1)
								
                 		LEFT JOIN 	AttachmentDetailConsumptionSKUMapping M2 ON M2.AttachmentDetailConsumptionId=bd.Id 
						AND M2.Id=(SELECT TOP 1 Id FROM AttachmentDetailConsumptionSKUMapping M WHERE m.AttachmentDetailConsumptionId=bd.Id  AND ISNULL(M.IsSecondCharacteristicCommon,0)=1)
								
                 		LEFT JOIN 	AttachmentDetailConsumptionSKUMapping M3 ON M3.AttachmentDetailConsumptionId=bd.Id 
						AND M3.Id=(SELECT TOP 1 Id FROM AttachmentDetailConsumptionSKUMapping M WHERE m.AttachmentDetailConsumptionId=bd.Id  AND ISNULL(M.IsThirdCharacteristicCommon,0)=1)
                LEFT OUTER JOIN mst.MaterialMaster AS mm ON mm.Id=bd.RMMaterialMasterId
								
            WHERE bd.IsSKUCommon = 0
            order by bd.RMArticleId";

            return sql;
        }


        private void ConstructCommonComponents(string MasterOrderItemId, DataTable dtBOQ)
        {
            //add all common data to the saving table
            //we may check the combination in the database for edit; right now I am ignoring that part
            try
            {
                DataTable dtCommonData = _sqlRepository.GetDataTable(sqlGetCommonComponents(MasterOrderItemId));

                dtCommonData.Merge(_sqlRepository.GetDataTable(sqlGetCommonComponentsForSubMaterialCommon(MasterOrderItemId)));
                dtCommonData.Merge(_sqlRepository.GetDataTable(sqlGetMappedComponentsForSubMaterialCommon(MasterOrderItemId)));

                dtCommonData.Merge(_sqlRepository.GetDataTable(sqlGetCommonComponentsForSubMaterialMapped(MasterOrderItemId)));
                dtCommonData.Merge(_sqlRepository.GetDataTable(sqlGetMappedComponentsForSubMaterialMapped(MasterOrderItemId)));

                for (int i = 0; i < dtCommonData.Rows.Count; i++)
                {

                    DataRow dr = dtBOQ.NewRow();
                    //this will be grouping and matching data
                    {
                        dr["SalesOrderId"] = dtCommonData.Rows[i]["SalesOrderId"].ToString();
                        dr["DestinationId"] = bplib.clsWebLib.RetValidLen(dtCommonData.Rows[i]["DestinationId"].ToString());
                        dr["MaterialMasterId"] = dtCommonData.Rows[i]["RMMaterialMasterId"].ToString();
                        dr["ArticleId"] = dtCommonData.Rows[i]["RMArticleId"].ToString();
                        dr["ProcessId"] = dtCommonData.Rows[i]["ProcessId"].ToString();
                        dr["UoMId"] = dtCommonData.Rows[i]["UoMId"].ToString();
                        dr["BaseUoMId"] = dtCommonData.Rows[i]["BaseUoMId"].ToString();
                        dr["POUoMId"] = dtCommonData.Rows[i]["POUoMId"].ToString();
                        dr["VendorId"] = dtCommonData.Rows[i]["VendorId"].ToString();
                        dr["RMDescription"] = dtCommonData.Rows[i]["RMDescription"].ToString();
                        dr["RMCustomerSpec"] = dtCommonData.Rows[i]["RMCustomerSpec"].ToString();
                        dr["RMVendorSpec"] = dtCommonData.Rows[i]["RMVendorSpec"].ToString();
                        dr["FirstCharacteristicsValueId"] = dtCommonData.Rows[i]["RMC1"].ToString();
                        dr["SecondCharacteristicsValueId"] = dtCommonData.Rows[i]["RMC2"].ToString();
                        dr["ThirdCharacteristicsValueId"] = dtCommonData.Rows[i]["RMC3"].ToString();
                        dr["SKUDesc"] = dtCommonData.Rows[i]["SKUDesc"].ToString().Trim();

                        dr["ParentKey"] = dtCommonData.Rows[i]["ParentKey"];

                        dr["SO1"] = dtCommonData.Rows[i]["SO1"];
                        dr["SO2"] = dtCommonData.Rows[i]["SO2"];
                        dr["SO3"] = dtCommonData.Rows[i]["SO3"];

                    }

                    dr["isParent"] = dtCommonData.Rows[i]["isParent"];
                    dr["isChild"] = dtCommonData.Rows[i]["isChild"];
                    dr["IsDestinationSpecific"] = dtCommonData.Rows[i]["IsDestinationSpecific"];
                    dr["IsPOSpecific"] = dtCommonData.Rows[i]["IsPOSpecific"];
                    dr["SalesOrderSpecificMaterial"] = dtCommonData.Rows[i]["SalesOrderSpecificMaterial"];
                    dr["CustomerPOId"] = dtCommonData.Rows[i]["CustomerPOId"];


                    dr["Sequence"] = clsStaticInfo.dbl(dtCommonData.Rows[i]["Sequence"].ToString());
                    dr["OrderQty"] = clsStaticInfo.dbl(dtCommonData.Rows[i]["OrderQty"].ToString());
                    dr["PlanOrderQty"] = clsStaticInfo.dbl(dtCommonData.Rows[i]["PlanOrderQty"].ToString());
                    dr["Consumption"] = clsStaticInfo.dbl(dtCommonData.Rows[i]["Consumption"].ToString());
                    dr["WastagePer"] = clsStaticInfo.dbl(dtCommonData.Rows[i]["WastagePer"].ToString());
                    dr["IncompleteMaterial"] = false;

                    dr["BOMQty"] = clsStaticInfo.dbl(dr["OrderQty"].ToString()) * clsStaticInfo.dbl(dr["Consumption"].ToString());
                    if (clsStaticInfo.dbl(dr["WastagePer"].ToString()) > 0)
                        dr["BOMQty"] = clsStaticInfo.dbl(dr["BOMQty"].ToString()) * (1 + (clsStaticInfo.dbl(dr["WastagePer"].ToString()) / 100));


                    dtBOQ.Rows.Add(dr);




                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        private void ConstructMappedComponents(string MasterOrderItemId, DataTable dtBOQ)
        {
            //add all mapped data to the saving table
            try
            {

                DataTable dtCommonData = _sqlRepository.GetDataTable(sqlGetMappedComponents(MasterOrderItemId));
                if (dtCommonData.Rows.Count > 0)
                    validateMappedSKU(dtCommonData);

                dtCommonData.DefaultView.RowFilter = null;
                dtCommonData = dtCommonData.DefaultView.ToTable();
                for (int i = 0; i < dtCommonData.Rows.Count; i++)
                {
                    DataRow dr = dtBOQ.NewRow();

                    //this will be grouping and matching data
                    {
                        dr["SalesOrderId"] = dtCommonData.Rows[i]["SalesOrderId"].ToString();
                        dr["DestinationId"] = bplib.clsWebLib.RetValidLen(dtCommonData.Rows[i]["DestinationId"].ToString());
                        dr["MaterialMasterId"] = dtCommonData.Rows[i]["RMMaterialMasterId"].ToString();
                        dr["ArticleId"] = dtCommonData.Rows[i]["RMArticleId"].ToString();
                        dr["ProcessId"] = dtCommonData.Rows[i]["ProcessId"].ToString();
                        dr["UoMId"] = dtCommonData.Rows[i]["UoMId"].ToString();
                        dr["BaseUoMId"] = dtCommonData.Rows[i]["BaseUoMId"].ToString();
                        dr["POUoMId"] = dtCommonData.Rows[i]["POUoMId"].ToString();
                        dr["VendorId"] = dtCommonData.Rows[i]["VendorId"].ToString();
                        dr["RMDescription"] = dtCommonData.Rows[i]["RMDescription"].ToString();
                        dr["RMCustomerSpec"] = dtCommonData.Rows[i]["RMCustomerSpec"].ToString();
                        dr["RMVendorSpec"] = dtCommonData.Rows[i]["RMVendorSpec"].ToString();
                        dr["FirstCharacteristicsValueId"] = dtCommonData.Rows[i]["SKURMC1"].ToString();
                        dr["SecondCharacteristicsValueId"] = dtCommonData.Rows[i]["SKURMC2"].ToString();
                        dr["ThirdCharacteristicsValueId"] = dtCommonData.Rows[i]["SKURMC3"].ToString();
                        dr["SKUDesc"] = dtCommonData.Rows[i]["SKUDesc"].ToString().Trim();
                        dr["ParentKey"] = dtCommonData.Rows[i]["ParentKey"];

                        dr["SO1"] = dtCommonData.Rows[i]["SO1"];
                        dr["SO2"] = dtCommonData.Rows[i]["SO2"];
                        dr["SO3"] = dtCommonData.Rows[i]["SO3"];

                    }
                    //if (dtCommonData.Rows[i]["BOMAttachmentDetailId"].ToString() == "2068-7")
                    //{

                    //}
                    dr["isParent"] = dtCommonData.Rows[i]["isParent"];
                    dr["isChild"] = dtCommonData.Rows[i]["isChild"];
                    dr["IsDestinationSpecific"] = dtCommonData.Rows[i]["IsDestinationSpecific"];
                    dr["IsPOSpecific"] = dtCommonData.Rows[i]["IsPOSpecific"];

                    dr["SalesOrderSpecificMaterial"] = dtCommonData.Rows[i]["SalesOrderSpecificMaterial"];
                    dr["CustomerPOId"] = dtCommonData.Rows[i]["CustomerPOId"];

                    dr["Sequence"] = clsStaticInfo.dbl(dtCommonData.Rows[i]["Sequence"].ToString());
                    dr["PlanOrderQty"] = clsStaticInfo.dbl(dtCommonData.Rows[i]["OrderBreakdownMappedPlannedQty"].ToString());//later, this will be only mapped qty
                    dr["OrderQty"] = clsStaticInfo.dbl(dtCommonData.Rows[i]["OrderBreakdownMappedQty"].ToString());//later, this will be only mapped qty
                    dr["Consumption"] = clsStaticInfo.dbl(dtCommonData.Rows[i]["Consumption"].ToString());
                    dr["WastagePer"] = clsStaticInfo.dbl(dtCommonData.Rows[i]["WastagePer"].ToString());


                    dr["IncompleteMaterial"] = false;
                    if (dtCommonData.Rows[i]["RMArticleId"].ToString() == "1739")
                    {

                    }
                    if (UsedCharacteristicsCount(dtCommonData.Rows[i]["MATC1"].ToString(), dtCommonData.Rows[i]["MATC2"].ToString(), dtCommonData.Rows[i]["MATC3"].ToString())
                        != UsedCharacteristicsCount(dtCommonData.Rows[i]["SKURMC1"].ToString(), dtCommonData.Rows[i]["SKURMC2"].ToString(), dtCommonData.Rows[i]["SKURMC3"].ToString()))
                    {
                        dr["IncompleteMaterial"] = true;
                    }
                    //if ((dtCommonData.Rows[i]["MATC1"].ToString() != "" && dtCommonData.Rows[i]["SKURMC1"].ToString() == "")
                    //    || (dtCommonData.Rows[i]["MATC2"].ToString() != "" && dtCommonData.Rows[i]["SKURMC2"].ToString() == "")
                    //     || (dtCommonData.Rows[i]["MATC3"].ToString() != "" && dtCommonData.Rows[i]["SKURMC3"].ToString() == "")
                    //    )
                    //{
                    //    dr["IncompleteMaterial"] = true;
                    //}


                    dr["BOMQty"] = clsStaticInfo.dbl(dr["OrderQty"].ToString()) * clsStaticInfo.dbl(dr["Consumption"].ToString());
                    if (clsStaticInfo.dbl(dr["WastagePer"].ToString()) > 0)
                        dr["BOMQty"] = clsStaticInfo.dbl(dr["BOMQty"].ToString()) * (1 + (clsStaticInfo.dbl(dr["WastagePer"].ToString()) / 100));


                    dtBOQ.Rows.Add(dr);




                }


            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        private int UsedCharacteristicsCount(string C1, string C2, string C3)
        {
            int count = 0;
            if (string.IsNullOrEmpty(C1) == false)
                count++;

            if (string.IsNullOrEmpty(C2) == false)
                count++;

            if (string.IsNullOrEmpty(C3) == false)
                count++;

            return count;
        }
        private void GenerateAndCompactBOQ(string MasterOrderItemId, out DataTable dtBOQ, out DataTable dtBOQDetail)
        {
            try
            {
                dtBOQ = _sqlRepository.GetDataTable("select * from BOQ where 1=2");
                dtBOQ.Columns.Add("ParentKey");
                dtBOQ.Columns.Add("SO1");
                dtBOQ.Columns.Add("SO2");
                dtBOQ.Columns.Add("SO3");

                ConstructMappedComponents(MasterOrderItemId, dtBOQ);
                ConstructCommonComponents(MasterOrderItemId, dtBOQ);

                #region FG Mapping Part

                dtBOQ.DefaultView.RowFilter = null;
                dtFGMapping = dtBOQ.DefaultView.ToTable();


                #endregion FG Mapping Part

                dtBOQDetail = GroupBOM(dtBOQ);
                dtBOQ = dtBOQDetail.DefaultView.ToTable();
                for (int i = 0; i < dtBOQ.Rows.Count; i++)
                    if (bplib.clsWebLib.GetBoolData(dtBOQ.Rows[i]["SalesOrderSpecificMaterial"].ToString()) == false)
                        dtBOQ.Rows[i]["SalesOrderId"] = DBNull.Value;


                dtBOQ = GroupBOM(dtBOQ);




            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        private DataTable GroupBOM(DataTable dtBOQ)
        {

            return dtBOQ.AsEnumerable().GroupBy(x => new
            {
                SalesOrderId = x["SalesOrderId"],
                DestinationId = x["DestinationId"],
                MaterialMasterId = x["MaterialMasterId"],
                ArticleId = x["ArticleId"],
                ProcessId = x["ProcessId"],
                UoMId = x["UoMId"],
                BaseUoMId = x["BaseUoMId"],
                POUoMId = x["POUoMId"],
                VendorId = x["VendorId"],
                RMDescription = x["RMDescription"],
                RMCustomerSpec = x["RMCustomerSpec"],
                RMVendorSpec = x["RMVendorSpec"],
                FirstCharacteristicsValueId = x["FirstCharacteristicsValueId"],
                SecondCharacteristicsValueId = x["SecondCharacteristicsValueId"],
                ThirdCharacteristicsValueId = x["ThirdCharacteristicsValueId"],
                SKUDesc = x["SKUDesc"],
                IncompleteMaterial = x["IncompleteMaterial"],
                Sequence = x["Sequence"],
                isParent = bplib.clsWebLib.GetBoolData(x["isParent"]),
                ParentKey = x["ParentKey"],
                isChild = bplib.clsWebLib.GetBoolData(x["isChild"]),
                IsDestinationSpecific = bplib.clsWebLib.GetBoolData(x["IsDestinationSpecific"]),
                IsPOSpecific = bplib.clsWebLib.GetBoolData(x["IsPOSpecific"]),

                SalesOrderSpecificMaterial = bplib.clsWebLib.GetBoolData(x["SalesOrderSpecificMaterial"]),
                CustomerPOId = x["CustomerPOId"]
            })
                                     .Select(x =>
                                     {
                                         DataRow row = dtBOQ.NewRow();
                                         row["SalesOrderId"] = x.Key.SalesOrderId;
                                         row["DestinationId"] = x.Key.DestinationId;
                                         row["MaterialMasterId"] = x.Key.MaterialMasterId;
                                         row["ArticleId"] = x.Key.ArticleId;
                                         row["ProcessId"] = x.Key.ProcessId;
                                         row["UoMId"] = x.Key.UoMId;
                                         row["BaseUoMId"] = x.Key.BaseUoMId;
                                         row["POUoMId"] = x.Key.POUoMId;
                                         row["VendorId"] = x.Key.VendorId;
                                         row["RMDescription"] = x.Key.RMDescription;
                                         row["RMCustomerSpec"] = x.Key.RMCustomerSpec;
                                         row["RMVendorSpec"] = x.Key.RMVendorSpec;
                                         row["FirstCharacteristicsValueId"] = x.Key.FirstCharacteristicsValueId;
                                         row["SecondCharacteristicsValueId"] = x.Key.SecondCharacteristicsValueId;
                                         row["ThirdCharacteristicsValueId"] = x.Key.ThirdCharacteristicsValueId;
                                         row["SKUDesc"] = x.Key.SKUDesc;
                                         row["Sequence"] = x.Key.Sequence;
                                         row["ParentKey"] = x.Key.ParentKey;

                                         row["isParent"] = x.Key.isParent;
                                         row["isChild"] = x.Key.isChild;
                                         row["IsDestinationSpecific"] = x.Key.IsDestinationSpecific;
                                         row["IsPOSpecific"] = x.Key.IsPOSpecific;
                                         row["SalesOrderSpecificMaterial"] = x.Key.SalesOrderSpecificMaterial;
                                         row["CustomerPOId"] = x.Key.CustomerPOId;

                                         row["IncompleteMaterial"] = x.Key.IncompleteMaterial;
                                         row["OrderQty"] = x.Sum(r => (decimal)r["OrderQty"]);
                                         row["PlanOrderQty"] = x.Sum(r => (decimal)r["PlanOrderQty"]);
                                         row["Consumption"] = x.Average(r => (decimal)r["Consumption"]);
                                         row["WastagePer"] = x.Average(r => (decimal)r["WastagePer"]);
                                         row["BOMQty"] = x.Sum(r => (decimal)r["BOMQty"]);
                                         return row;
                                     }


                                     ).CopyToDataTable();



        }

        public StringCollection GetAllBOMReferenceData(string MasterOrderItemId)
        {

            DataTable dt = _sqlRepository.GetDataTable(@" SELECT DISTINCT b.Id FROM BOQ AS b
                                                                  INNER JOIN trn.POBOQMAP AS p ON p.BOQDetailId=b.Id
                                                                  WHERE b.MasterOrderItemId='" + MasterOrderItemId + "'");

            StringCollection strCol = new StringCollection();
            for (int i = 0; i < dt.Rows.Count; i++)
                strCol.Add(dt.Rows[i]["Id"].ToString());

            return strCol;
        }

        public void BOM(string MasterOrderItemId)
        {
            try
            {


                GenerateAndCompactBOQ(MasterOrderItemId, out DataTable dtSourceBOQ, out DataTable dtDetailBOQ);
                if (dtSourceBOQ.Rows.Count == 0)
                    throw new Exception("No order data found to generate BOM");

                ConManager = new ConnectionManager.clsConnectionManager();
                ConManager.getDataSet("select * from BOQDetail where MasterOrderItemId='" + MasterOrderItemId + "'", out DataSet dsBOQDetail);
                dsBOQDetail.Tables[0].Columns.Add("ParentKey");

                //while (dsBOQDetail.Tables[0].DefaultView.Count > 0)
                //    dsBOQDetail.Tables[0].DefaultView[0].Delete();
                Dictionary<string, List<DataRow>> dicKeyWisePKDetail = new Dictionary<string, List<DataRow>>();
                for (int i = 0; i < dsBOQDetail.Tables[0].Rows.Count; i++)
                {
                    string _key = BOMComparingString(dsBOQDetail.Tables[0].Rows[i]);
                    if (dicKeyWisePKDetail.ContainsKey(_key) == false)
                    {
                        List<DataRow> row = new List<DataRow>();
                        row.Add(dsBOQDetail.Tables[0].Rows[i]);
                        dicKeyWisePKDetail.Add(_key, row);
                    }
                    else
                    {
                        List<DataRow> row = dicKeyWisePKDetail[_key];
                        row.Add(dsBOQDetail.Tables[0].Rows[i]);
                    }
                }



                ConManager = new ConnectionManager.clsConnectionManager();
                ConManager.getDataSet("select * from BOQ where MasterOrderItemId='" + MasterOrderItemId + "' order by ParentId", out DataSet dsBOQ);
                dsBOQ.Tables[0].Columns.Add("ParentKey");
                #region exract parentkey based on parentid
                string strParentId = "";
                string strParentKey = "";
                for (int i = 0; i < dsBOQ.Tables[0].Rows.Count; i++)
                {
                    if (dsBOQ.Tables[0].Rows[i]["ParentId"].ToString() == "")
                        continue;

                    //for faster extraction; at least will save some milliseconds :)
                    if (strParentId != dsBOQ.Tables[0].Rows[i]["ParentId"].ToString())
                    {
                        strParentId = dsBOQ.Tables[0].Rows[i]["ParentId"].ToString();
                        dsBOQ.Tables[0].DefaultView.RowFilter = "Id='" + strParentId + "'";
                        if (dsBOQ.Tables[0].DefaultView.Count > 0)
                            strParentKey = BOMComparingString(dsBOQ.Tables[0].DefaultView[0].Row);
                    }

                    dsBOQ.Tables[0].Rows[i]["ParentKey"] = strParentKey;
                }
                dsBOQ.Tables[0].DefaultView.RowFilter = null;
                #endregion exract parentkey based on parentid

                #region Create dictionary for fastest comparison for existing data check
                //create a dictionary for both data source and db to optimize the search
                Dictionary<string, DataRow> dicSourceBOQ = new Dictionary<string, DataRow>();
                for (int i = 0; i < dtSourceBOQ.Rows.Count; i++)
                    if(dicSourceBOQ.ContainsKey(BOMComparingString(dtSourceBOQ.Rows[i]))==false)
                    dicSourceBOQ.Add(BOMComparingString(dtSourceBOQ.Rows[i]), dtSourceBOQ.Rows[i]);


                Dictionary<string, DataRow> dicBOQ = new Dictionary<string, DataRow>();
                for (int i = 0; i < dsBOQ.Tables[0].Rows.Count; i++)
                    if (dicBOQ.ContainsKey(BOMComparingString(dsBOQ.Tables[0].Rows[i])) == false)
                        dicBOQ.Add(BOMComparingString(dsBOQ.Tables[0].Rows[i]), dsBOQ.Tables[0].Rows[i]);


                Dictionary<string, DataRow> dicBOQDetail = new Dictionary<string, DataRow>();
                for (int i = 0; i < dsBOQDetail.Tables[0].Rows.Count; i++)
                    if (dicBOQDetail.ContainsKey(BOMComparingString(dsBOQDetail.Tables[0].Rows[i])) == false)
                        dicBOQDetail.Add(BOMComparingString(dsBOQDetail.Tables[0].Rows[i], true), dsBOQDetail.Tables[0].Rows[i]);

                //later, we will add the PO related to this BOM whether to delete or not
                StringCollection BOMReferenceData = GetAllBOMReferenceData(MasterOrderItemId);

                #endregion Create dictionary for fastest comparison

                string Key = "";
                #region first delete all unnecessary bom items from DB
                for (int i = 0; i < dsBOQ.Tables[0].Rows.Count; i++)
                {
                    try
                    {


                        Key = BOMComparingString(dsBOQ.Tables[0].Rows[i]);
                        if (dicSourceBOQ.ContainsKey(Key) == false)
                        {


                            //if (bplib.clsWebLib.GetBoolData(dsBOQ.Tables[0].Rows[i]["isParent"].ToString()) == true)
                            //    continue;


                            if (BOMReferenceData.Contains(dsBOQ.Tables[0].Rows[i]["Id"].ToString()))
                            {
                                //set all quantity=0 since the item has been used in PO but also removed from BOM template
                                //unfortunately, we cannot delete the item from BOQ because id has been referenced to POBOQ mapping table
                                dsBOQ.Tables[0].Rows[i]["OrderQty"] = 0;
                                dsBOQ.Tables[0].Rows[i]["PlanOrderQty"] = 0;
                                dsBOQ.Tables[0].Rows[i]["BOMQty"] = 0;
                                dsBOQ.Tables[0].Rows[i]["RequiredQty"] = 0;
                                dsBOQ.Tables[0].Rows[i]["BOMQtyBase"] = 0;
                                dsBOQ.Tables[0].Rows[i]["RequiredQtyBase"] = 0;
                                dsBOQ.Tables[0].Rows[i]["RequiredQtyPO"] = 0;
                                continue;
                            }

                            //no PO data found but item has been aprroved; need to clear the bom generated qty
                            if (bplib.clsWebLib.GetBoolData(dsBOQ.Tables[0].Rows[i]["RequiredQtyApproved"].ToString()) == true)
                            {
                                dsBOQ.Tables[0].Rows[i]["BOMQty"] = 0;
                                dsBOQ.Tables[0].Rows[i]["BOMQtyBase"] = 0;
                                continue;
                            }

                            #region Parent data delete with children 
                            string ParentId = dsBOQ.Tables[0].Rows[i]["Id"].ToString();
                            dsBOQ.Tables[0].DefaultView.RowFilter = "ParentId='" + ParentId + "'";
                            if (bplib.clsWebLib.GetBoolData(dsBOQ.Tables[0].Rows[i]["isParent"].ToString()))
                            {
                                //we are going to delete parent, so check any child reference (PO raised or not)
                                bool ChildReferenceFound = false;
                                for (int PP = 0; PP < dsBOQ.Tables[0].DefaultView.Count; PP++)
                                {
                                    if (BOMReferenceData.Contains(dsBOQ.Tables[0].DefaultView[PP]["Id"].ToString()))
                                    {
                                        ChildReferenceFound = true;
                                        break;
                                    }
                                }
                                if (ChildReferenceFound == false)
                                {
                                    dsBOQ.Tables[0].Rows[i].Delete();
                                    dicBOQ.Remove(Key);

                                    if (dicKeyWisePKDetail.ContainsKey(Key))
                                        for (int AA = 0; AA < dicKeyWisePKDetail[Key].Count; AA++)
                                            dicKeyWisePKDetail[Key][AA].Delete();



                                    while (dsBOQ.Tables[0].DefaultView.Count > 0)
                                    {
                                        string ChildKey = BOMComparingString(dsBOQ.Tables[0].DefaultView[0].Row);
                                        dsBOQ.Tables[0].DefaultView[0].Row.Delete();
                                        dicBOQ.Remove(ChildKey);

                                        if (dicKeyWisePKDetail.ContainsKey(ChildKey))
                                            for (int AA = 0; AA < dicKeyWisePKDetail[ChildKey].Count; AA++)
                                                dicKeyWisePKDetail[ChildKey][AA].Delete();
                                    }
                                }
                            }
                            else
                            {
                                dsBOQ.Tables[0].Rows[i].Delete();
                                dicBOQ.Remove(Key);

                                while (dsBOQ.Tables[0].DefaultView.Count > 0)
                                {
                                    string ChildKey = BOMComparingString(dsBOQ.Tables[0].DefaultView[0].Row);
                                    dsBOQ.Tables[0].DefaultView[0].Row.Delete();
                                    dicBOQ.Remove(ChildKey);
                                }
                            }

                            #endregion Parent data delete with children 
                        }
                    }
                    catch (Exception ex)
                    {


                    }
                }

                //for (int i = 0; i < dsBOQ.Tables[0].Rows.Count; i++)
                //    dsBOQ.Tables[0].Rows[i]["ParentKey"] = "";

                //implement code for child data deletion if parent is already deleted

                #endregion first delete all unnecessary bom items from DB
                Dictionary<string, string> dicKeyWisePK = new Dictionary<string, string>();


                Library.General.Conversions.UOMConversion conversion = new General.Conversions.UOMConversion();

                Key = "";
                string _id = GetPK("BOMMasterAttachmentWithItem");
                for (int i = 0; i < dtSourceBOQ.Rows.Count; i++)
                {
                    Key = BOMComparingString(dtSourceBOQ.Rows[i]);

                    if (dicBOQ.ContainsKey(Key) == false)
                    {
                        DataRow dr = dsBOQ.Tables[0].NewRow();

                        CopyRow(dtSourceBOQ.Rows[i], ref dr);
                        dr["Id"] = _id + "-" + (i + 1).ToString();
                        dr["MasterOrderItemId"] = MasterOrderItemId;
                        dr["IncompleteMaterial"] = dtSourceBOQ.Rows[i]["IncompleteMaterial"];
                        dr["ParentKey"] = dtSourceBOQ.Rows[i]["ParentKey"];
                        dr["RequiredQty"] = clsStaticInfo.dbl(dr["BOMQty"].ToString());

                        dr["BOMQtyBase"] = conversion.Convert(dtSourceBOQ.Rows[i]["MaterialMasterId"].ToString(),
                                            dtSourceBOQ.Rows[i]["UoMId"].ToString(),
                                            dtSourceBOQ.Rows[i]["BaseUoMId"].ToString(), clsStaticInfo.dbl(dr["BOMQty"].ToString())).ToString("F4");

                        dr["RequiredQtyBase"] = clsStaticInfo.dbl(dr["BOMQtyBase"].ToString());
                        dr["RequiredQtyPO"] = conversion.Convert(dtSourceBOQ.Rows[i]["MaterialMasterId"].ToString(),
                                            dtSourceBOQ.Rows[i]["UoMId"].ToString(),
                                            dtSourceBOQ.Rows[i]["POUoMId"].ToString(), clsStaticInfo.dbl(dr["BOMQty"].ToString())).ToString("F4");


                        dr["RequiredQtyApproved"] = false;
                        dsBOQ.Tables[0].Rows.Add(dr);

                        dicBOQ.Add(Key, dr);

                        //dicKeyWisePK.Add(Key, dr["Id"].ToString());
                    }
                    else
                    {
                        DataRow dr = dicBOQ[Key];
                        dr.BeginEdit();
                        dr["ParentKey"] = dtSourceBOQ.Rows[i]["ParentKey"];
                        dr["PlanOrderQty"] = clsStaticInfo.dbl(dtSourceBOQ.Rows[i]["PlanOrderQty"].ToString());
                        dr["OrderQty"] = clsStaticInfo.dbl(dtSourceBOQ.Rows[i]["OrderQty"].ToString());
                        dr["Consumption"] = clsStaticInfo.dbl(dtSourceBOQ.Rows[i]["Consumption"].ToString());
                        dr["WastagePer"] = clsStaticInfo.dbl(dtSourceBOQ.Rows[i]["WastagePer"].ToString());
                        dr["BOMQty"] = clsStaticInfo.dbl(dtSourceBOQ.Rows[i]["BOMQty"].ToString());
                        dr["BOMQtyBase"] = conversion.Convert(dtSourceBOQ.Rows[i]["MaterialMasterId"].ToString(),
                                           dtSourceBOQ.Rows[i]["UoMId"].ToString(),
                                           dtSourceBOQ.Rows[i]["BaseUoMId"].ToString(), clsStaticInfo.dbl(dr["BOMQty"].ToString())).ToString("F4");

                        dr["IncompleteMaterial"] = dtSourceBOQ.Rows[i]["IncompleteMaterial"];
                        dr["isParent"] = dtSourceBOQ.Rows[i]["isParent"];
                        dr["isChild"] = dtSourceBOQ.Rows[i]["isChild"];


                        if (bplib.clsWebLib.GetBoolData(dr["RequiredQtyApproved"].ToString()) == false)
                        {
                            dr["BaseUOMId"] = dtSourceBOQ.Rows[i]["BaseUOMId"];
                            dr["POUOMId"] = dtSourceBOQ.Rows[i]["POUOMId"];

                            dr["RequiredQty"] = clsStaticInfo.dbl(dr["BOMQty"].ToString());
                            dr["RequiredQtyBase"] = clsStaticInfo.dbl(dr["BOMQtyBase"].ToString());
                            dr["RequiredQtyPO"] = conversion.Convert(dtSourceBOQ.Rows[i]["MaterialMasterId"].ToString(),
                                                dtSourceBOQ.Rows[i]["UoMId"].ToString(),
                                                dtSourceBOQ.Rows[i]["POUoMId"].ToString(), clsStaticInfo.dbl(dr["BOMQty"].ToString())).ToString("F4");
                        }

                        dr["UpdatedBy"] = identity.Name;
                        dr["UpdatedFromIP"] = identity.IPAddress;
                        dr["UpdatedDate"] = DateTime.Now;
                        dr.EndEdit();

                        //dicKeyWisePK.Add(Key, dr["Id"].ToString());
                    }
                }

                for (int i = 0; i < dtSourceBOQ.Rows.Count; i++)
                {
                    if (bplib.clsWebLib.GetBoolData(dtSourceBOQ.Rows[i]["isChild"].ToString()) == false)
                        continue;


                    Key = dtSourceBOQ.Rows[i]["ParentKey"].ToString();

                    if (dicBOQ.ContainsKey(Key) == true)
                    {
                        DataRow dr = dicBOQ[Key];

                        dsBOQ.Tables[0].DefaultView.RowFilter = "ParentKey='" + Key + "'";
                        for (int k = 0; k < dsBOQ.Tables[0].DefaultView.Count; k++)
                        {
                            dsBOQ.Tables[0].DefaultView[k]["ParentId"] = dr["Id"];
                        }

                    }
                }

                #region BOQDetail



                for (int i = 0; i < dtDetailBOQ.Rows.Count; i++)
                {
                    Key = BOMComparingString(dtDetailBOQ.Rows[i]);

                    if (dicBOQ.ContainsKey(Key) == true)
                    {
                        string keytemp = BOMComparingString(dtDetailBOQ.Rows[i], true);
                        if (dicBOQDetail.ContainsKey(keytemp) == false)
                        {
                            DataRow dr = dsBOQDetail.Tables[0].NewRow();

                            CopyRow(dtDetailBOQ.Rows[i], ref dr);
                            dr["Id"] = _id + "-" + (i + 1).ToString();
                            dr["BOQId"] = dicBOQ[Key]["Id"];
                            dr["MasterOrderItemId"] = MasterOrderItemId;
                            dr["IncompleteMaterial"] = dtDetailBOQ.Rows[i]["IncompleteMaterial"];
                            //dr["ParentKey"] = dtDetailBOQ.Rows[i]["ParentKey"];
                            dr["RequiredQty"] = clsStaticInfo.dbl(dr["BOMQty"].ToString());

                            dr["BOMQtyBase"] = conversion.Convert(dtDetailBOQ.Rows[i]["MaterialMasterId"].ToString(),
                                                dtDetailBOQ.Rows[i]["UoMId"].ToString(),
                                                dtDetailBOQ.Rows[i]["BaseUoMId"].ToString(), clsStaticInfo.dbl(dr["BOMQty"].ToString())).ToString("F4");

                            dr["RequiredQtyBase"] = clsStaticInfo.dbl(dr["BOMQtyBase"].ToString());
                            dr["RequiredQtyPO"] = conversion.Convert(dtDetailBOQ.Rows[i]["MaterialMasterId"].ToString(),
                                                dtDetailBOQ.Rows[i]["UoMId"].ToString(),
                                                dtDetailBOQ.Rows[i]["POUoMId"].ToString(), clsStaticInfo.dbl(dr["BOMQty"].ToString())).ToString("F4");


                            dr["RequiredQtyApproved"] = false;
                            dsBOQDetail.Tables[0].Rows.Add(dr);

                            if (dicBOQDetail.ContainsKey(keytemp) == false)
                                dicBOQDetail.Add(keytemp, dr);
                           
                            if (dicKeyWisePK.ContainsKey(keytemp) == false)
                                dicKeyWisePK.Add(keytemp, dr["Id"].ToString());
                        }
                        else
                        {

                            DataRow dr = dicBOQDetail[keytemp];
                            dr.BeginEdit();
                            dr["MasterOrderItemId"] = MasterOrderItemId;
                            dr["IncompleteMaterial"] = dtDetailBOQ.Rows[i]["IncompleteMaterial"];
                            //dr["ParentKey"] = dtDetailBOQ.Rows[i]["ParentKey"];
                            dr["RequiredQty"] = clsStaticInfo.dbl(dr["BOMQty"].ToString());

                            dr["BOMQtyBase"] = conversion.Convert(dtDetailBOQ.Rows[i]["MaterialMasterId"].ToString(),
                                                dtDetailBOQ.Rows[i]["UoMId"].ToString(),
                                                dtDetailBOQ.Rows[i]["BaseUoMId"].ToString(), clsStaticInfo.dbl(dr["BOMQty"].ToString())).ToString("F4");

                            dr["RequiredQtyBase"] = clsStaticInfo.dbl(dr["BOMQtyBase"].ToString());
                            dr["RequiredQtyPO"] = conversion.Convert(dtDetailBOQ.Rows[i]["MaterialMasterId"].ToString(),
                                                dtDetailBOQ.Rows[i]["UoMId"].ToString(),
                                                dtDetailBOQ.Rows[i]["POUoMId"].ToString(), clsStaticInfo.dbl(dr["BOMQty"].ToString())).ToString("F4");

                            dr["UpdatedBy"] = identity.Name;
                            dr["UpdatedFromIP"] = identity.IPAddress;
                            dr["UpdatedDate"] = DateTime.Now;
                            dr.EndEdit();


                            if (dicBOQDetail.ContainsKey(keytemp) == false)
                                dicBOQDetail.Add(keytemp, dr);

                            if (dicKeyWisePK.ContainsKey(keytemp) == false)
                                dicKeyWisePK.Add(keytemp, dr["Id"].ToString());
                        }
                    }
                    else
                    {


                    }

                }

                //for (int i = 0; i < dtDetailBOQ.Rows.Count; i++)
                //{
                //    if (bplib.clsWebLib.GetBoolData(dtDetailBOQ.Rows[i]["isChild"].ToString()) == false)
                //        continue;


                //    Key = dtDetailBOQ.Rows[i]["ParentKey"].ToString();

                //    if (dicBOQ.ContainsKey(Key) == true)
                //    {
                //        DataRow dr = dicBOQ[Key];

                //        dsBOQ.Tables[0].DefaultView.RowFilter = "ParentKey='" + Key + "'";
                //        for (int k = 0; k < dsBOQ.Tables[0].DefaultView.Count; k++)
                //        {
                //            dsBOQ.Tables[0].DefaultView[k]["ParentId"] = dr["Id"];
                //        }

                //    }
                //}


                #endregion BOQDetail


                #region BOQ FG Mapping
                ConManager = new ConnectionManager.clsConnectionManager();
                ConManager.getDataSet("select * from BOQFGMapping where BOQDetailId IN (select Id from BOQDetail where MasterOrderItemId='" + MasterOrderItemId + "')", out DataSet dsBOQMapping);
                while (dsBOQMapping.Tables[0].DefaultView.Count > 0)
                    dsBOQMapping.Tables[0].DefaultView[0].Delete();

                for (int i = 0; i < dtFGMapping.Rows.Count; i++)
                {
                    Key = BOMComparingString(dtFGMapping.Rows[i], true);

                    if (dicKeyWisePK.ContainsKey(Key) == true)
                    {
                        DataRow dr = dsBOQMapping.Tables[0].NewRow();

                        dr["BOQDetailId"] = dicKeyWisePK[Key];
                        dr["FirstCharacteristicsValueId"] = bplib.clsWebLib.RetValidLen(dtFGMapping.Rows[i]["SO1"].ToString());
                        dr["SecondCharacteristicsValueId"] = bplib.clsWebLib.RetValidLen(dtFGMapping.Rows[i]["SO2"].ToString());
                        dr["ThirdCharacteristicsValueId"] = bplib.clsWebLib.RetValidLen(dtFGMapping.Rows[i]["SO3"].ToString());

                        dr["OrderQty"] = clsStaticInfo.dbl(dtFGMapping.Rows[i]["OrderQty"].ToString());
                        dr["PlanOrderQty"] = clsStaticInfo.dbl(dtFGMapping.Rows[i]["PlanOrderQty"].ToString());

                        dr["BOMQty"] = clsStaticInfo.dbl(dtFGMapping.Rows[i]["BOMQty"].ToString());


                        dsBOQMapping.Tables[0].Rows.Add(dr);


                    }
                    else
                    {


                    }

                }


                #endregion BOQ FG Mapping
                clsStaticInfo info = new clsStaticInfo();
                info.SaveDataSets(dsBOQ, dsBOQDetail, dsBOQMapping);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        //validation part
        private class CheckMappedLevel
        {
            public bool SKU1 { get; set; } = false;
            public bool SKU2 { get; set; } = false;
            public bool SKU3 { get; set; } = false;
            public DataRow Row { get; set; }
        }
        bool BOMError = false;
        private void WriteSheet(DataTable dt, string Message)
        {
            DataRow dr = dt.NewRow();
            dr["Message"] = Message;
            dt.Rows.Add(dr);

            BOMError = true;
        }
        private void validateMappedSKU(DataTable dtMappedData)
        {
            ExcelEngine excelEngine = new ExcelEngine();
            IApplication application = null;
            IWorkbook workbook = null;
            IWorksheet sheet = null;

            try
            {
                application = excelEngine.Excel;
                workbook = application.Workbooks.Create(1);
                workbook.Worksheets[0].Name = "Error Log";
                sheet = workbook.Worksheets[0];

                DataTable dtError = new DataTable("Error");
                dtError.Columns.Add("Message");

                int ROW = 1;
                sheet[ROW, 1].ColumnWidth = 120;
                sheet[ROW, 1].Text = "Errors";
                sheet[ROW, 1].CellStyle.Font.Bold = true;
                sheet[ROW, 1].CellStyle.Interior.ColorIndex = ExcelKnownColors.Dark_yellow;
                ROW++;

                //key=A-B;
                Dictionary<string, CheckMappedLevel> DicLevelOfMapping = new Dictionary<string, CheckMappedLevel>();
                #region construct dictionary to determine an invalid mapping between bom template and order

                string Key = "";
                CheckMappedLevel Data = new CheckMappedLevel();
                for (int i = 0; i < dtMappedData.Rows.Count; i++)
                {

                    if (Key != dtMappedData.Rows[i]["BOMAttachmentDetailId"].ToString() + "-" + dtMappedData.Rows[i]["SalesOrderId"].ToString())
                    {
                        Data = new CheckMappedLevel();
                        Data.Row = dtMappedData.Rows[i];
                        DicLevelOfMapping.Add(dtMappedData.Rows[i]["BOMAttachmentDetailId"].ToString() + "-" + dtMappedData.Rows[i]["SalesOrderId"].ToString(), Data);
                    }

                    if (string.IsNullOrEmpty(dtMappedData.Rows[i]["CHARMAP1"].ToString()) == false)
                        Data.SKU1 = true;

                    if (string.IsNullOrEmpty(dtMappedData.Rows[i]["CHARMAP2"].ToString()) == false)
                        Data.SKU2 = true;

                    if (string.IsNullOrEmpty(dtMappedData.Rows[i]["CHARMAP3"].ToString()) == false)
                        Data.SKU3 = true;

                    Key = dtMappedData.Rows[i]["BOMAttachmentDetailId"].ToString() + "-" + dtMappedData.Rows[i]["SalesOrderId"].ToString();

                }

                foreach (KeyValuePair<string, CheckMappedLevel> entry in DicLevelOfMapping)
                {
                    //check whether SO has breakdown or not, if not throw error

                    if (string.IsNullOrEmpty(entry.Value.Row["SO1"].ToString()) == true
                        && string.IsNullOrEmpty(entry.Value.Row["SO2"].ToString()) == true
                        && string.IsNullOrEmpty(entry.Value.Row["SO3"].ToString()) == true)
                    {
                        WriteSheet(dtError, "Sales Order breakdown missing. Sales Order#" + entry.Value.Row["SalesOrderId"].ToString() + " for BOM material " + entry.Value.Row["Material"].ToString());
                    }
                }
                #endregion construct dictionary to determine an invalid mapping between bom template and order


                DataRow dr;
                for (int i = 0; i < dtMappedData.Rows.Count; i++)
                {
                    dr = dtMappedData.Rows[i];
                    //determine which level to check
                    if (string.IsNullOrEmpty(dr["CHARMAP1"].ToString()) == false)
                    {
                        if (string.IsNullOrEmpty(dr["SO1"].ToString()) == false)
                        {
                            //SO1 has same characteristics breakdown
                            if (string.IsNullOrEmpty(dr["B1"].ToString()) == true)
                            {
                                if (bplib.clsWebLib.GetBoolData(dr["ConsumptionSpecificToSKU1"].ToString()) == false)
                                {
                                    WriteSheet(dtError, dr["SOC1"].ToString() + " mapping missing for value: " + dr["SOCV1"].ToString() + " in BOM Template, Material: " + dr["Material"].ToString());
                                }
                                else
                                {
                                    dr.Delete();
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            //will add later,if color/size breakdown is missing entirely in sales order

                        }
                    }


                    //determine which level to check
                    if (string.IsNullOrEmpty(dr["CHARMAP2"].ToString()) == false)
                    {
                        if (string.IsNullOrEmpty(dr["SO2"].ToString()) == false)
                        {
                            //SO1 has same characteristics breakdown
                            if (string.IsNullOrEmpty(dr["B2"].ToString()) == true)
                            {
                                if (bplib.clsWebLib.GetBoolData(dr["ConsumptionSpecificToSKU2"].ToString()) == false)
                                {
                                    WriteSheet(dtError, dr["SOC2"].ToString() + " mapping missing for value: " + dr["SOCV2"].ToString() + " in BOM Template, Material: " + dr["Material"].ToString());
                                }
                                else
                                {
                                    dr.Delete();
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            //will add later,if color/size breakdown is missing entirely in sales order

                        }
                    }

                    //determine which level to check
                    if (string.IsNullOrEmpty(dr["CHARMAP3"].ToString()) == false)
                    {
                        if (string.IsNullOrEmpty(dr["SO3"].ToString()) == false)
                        {
                            //SO1 has same characteristics breakdown
                            if (string.IsNullOrEmpty(dr["B3"].ToString()) == true)
                            {
                                if (bplib.clsWebLib.GetBoolData(dr["ConsumptionSpecificToSKU3"].ToString()) == false)
                                {
                                    WriteSheet(dtError, dr["SOC3"].ToString() + " mapping missing for value: " + dr["SOCV3"].ToString() + " in BOM Template, Material: " + dr["Material"].ToString());
                                }
                                else
                                {
                                    dr.Delete();
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            //will add later,if color/size breakdown is missing entirely in sales order

                        }
                    }
                }

                dtMappedData.DefaultView.RowFilter = null;
                dtMappedData = dtMappedData.DefaultView.ToTable();

                if (BOMError)
                {
                    dtError = dtError.DefaultView.ToTable(true, "Message");
                    sheet.ImportDataTable(dtError, false, ROW, 1);

                    string strFileName = "BOM Generation Error Log.xlsx";
                    string fullPath = HostingEnvironment.MapPath("~/") + strFileName;
                    workbook.SaveAs(fullPath, ExcelSaveType.SaveAsXLS);
                    workbook.Close();
                    throw new Exception(strFileName);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }


        private void CopyRow(DataRow drSource, ref DataRow drDestination)
        {

            for (int COL = 0; COL < drSource.Table.Columns.Count; COL++)
            {
                try
                {
                    drDestination[drSource.Table.Columns[COL].ColumnName] = bplib.clsWebLib.RetValidLen(drSource[drSource.Table.Columns[COL].ColumnName].ToString());

                }
                catch (Exception ex)
                {
                }
                try
                {
                    drDestination["AddedBy"] = identity.Name;
                    drDestination["AddedDate"] = DateTime.Now;
                    drDestination["AddedFromIP"] = identity.IPAddress;
                    drDestination["UpdatedBy"] = identity.Name;
                    drDestination["UpdatedFromIP"] = identity.IPAddress;
                    drDestination["UpdatedDate"] = DateTime.Now;

                }
                catch (Exception ex)
                {
                }
            }

        }
        private string GetPK(string TableName)
        {
            string sID = string.Empty;
            bplib.clsGenID objGenID = new bplib.clsGenID();
            objGenID.GenerateIDYearly(DateTime.Now.ToShortDateString().ToString(), TableName, out sID);
            return sID;
        }
        private string BOMComparingString(DataRow dr, bool AlwaysCheckForSalesOrder = false)
        {

            string[] columns = {
                    //"ParentId",
                    "ParentKey",
                    "SalesOrderId",
                    "DestinationId",
                    "CustomerPOId",
                    "MaterialMasterId",
                    "ArticleId",
                    "ProcessId",
                    "UoMId",
                    //"VendorId",
                    "RMDescription",
                    "RMCustomerSpec",
                    "RMVendorSpec",
                    "FirstCharacteristicsValueId",
                    "SecondCharacteristicsValueId",
                    "ThirdCharacteristicsValueId",
                    "SKUDesc", };

            string key = "";
            for (int i = 0; i < columns.Length; i++)
            {
                if (AlwaysCheckForSalesOrder == false)
                    if (columns[i].ToUpper() == "SALESORDERID")
                        if (bplib.clsWebLib.GetBoolData(dr["SalesOrderSpecificMaterial"].ToString().Trim()) == false)
                            continue;



                key += dr[columns[i]].ToString().Trim();
            }

            return key;
        }
        private string BOMComparingStringCommonKey()
        {
            string[] columns = {
                    "PARENT.SalesOrderId",
                    "PARENT.DestinationId",
                    "PARENT.CustomerPOId",
                    "PARENT.RMMaterialMasterId",
                    "PARENT.RMArticleId",
                    "PARENT.ProcessId",
                    "PARENT.UoMId",
                    //"PARENT.VendorId",
                    "PARENT.RMDescription",
                    "PARENT.RMCustomerSpec",
                    "PARENT.RMVendorSpec",
                    "PARENT.RMC1",
                    "PARENT.RMC2",
                    "PARENT.RMC3",
                    "PARENT.SKUDesc", };

            string key = "''";
            for (int i = 0; i < columns.Length; i++)
            {
                key += "," + columns[i].ToString().Trim();
            }

            return ("CONCAT(" + key + ") AS ParentKey");
        }
        private string BOMComparingStringMappedKey()
        {
            string[] columns = {
                    "PARENT.SalesOrderId",
                    "PARENT.DestinationId",
                    "PARENT.CustomerPOId",
                    "PARENT.RMMaterialMasterId",
                    "PARENT.RMArticleId",
                    "PARENT.ProcessId",
                    "PARENT.UoMId",
                    //"PARENT.VendorId",
                    "PARENT.RMDescription",
                    "PARENT.RMCustomerSpec",
                    "PARENT.RMVendorSpec",
                    "PARENT.SKURMC1",
                    "PARENT.SKURMC2",
                    "PARENT.SKURMC3",
                    "PARENT.SKUDesc", };

            string key = "''";
            for (int i = 0; i < columns.Length; i++)
            {
                key += "," + columns[i].ToString().Trim();
            }

            return ("CONCAT(" + key + ") AS ParentKey");
        }
    }
}
