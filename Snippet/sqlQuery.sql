REPLACE(CONVERT(CHAR(11), SO.DeliveryDate, 106),' ','-')
select count(MenuId) from MMS.MenuAction
GROUP BY MenuId
having count(MenuId) > 4
-- CAST(0 as BIT) AS Archive
SELECT  ABS(CHECKSUM(NEWID()) % 1000000)
-- COALESCE(' , Thana : '+ AD.Thana, '')

IF EXISTS(SELECT 1 FROM(
SELECT A.CheckingColumn,B.CheckingColumn2 FROM
(SELECT Id,ProcessId AS CheckingColumn FROM MST.OperationMachineType) AS A LEFT OUTER JOIN
(SELECT OperationId,MachineTypeId AS CheckingColumn2 FROM MST.OperationMachineType ) AS B ON A.Id=B.OperationId
) AA WHERE CheckingColumn IN ('') AND CheckingColumn2='') SELECT 1 ELSE SELECT 0 RETURN 

SELECT ISNULL((SELECT TOP(1) ISNULL(A.ToCurrencyBankSelling,0) FROM SCS.ExchangeRate AS A WHERE A.FromCurrencyCode='201712' AND A.ToCurrencyCode='20178' 
AND A.FromDate<=CAST('29-May-2018' AS DATE) ORDER BY CAST(FromDate AS DATE) DESC), 0)


SELECT ISNULL(MAX(CAST(RIGHT(Id, 2) AS INT)), 0) Id FROM [TRN].[ProductDefinitionEfficency] WHERE ProductDefinitionId=''


;WITH CTE AS
(
    SELECT MGP.UserName AS MaterialGroupMaster, MM.Id AS MaterialMasterId
			, MM.Code, MM.ShortName, MM.StandardName,MM.UserName AS MaterialMasterName
            , FAM.UserName AS AssetMaster, B.UserName AS BudgetName
    , COUNT(*) OVER (PARTITION BY MP.MaterialMasterId) AS RN
	FROM [MST].[MaterialMaster] AS MM
	LEFT JOIN [MST].[MaterialGroupMaster] AS MGP ON MM.MaterialGroupMasterId = MGP.Id
	LEFT JOIN [MST].[FixedAssetMaster] AS FAM ON MM.AssetMasterId = FAM.Id
	LEFT JOIN MST.BudgetMaster AS BM ON MM.BudgetMasterId=BM.Id
	LEFT JOIN HKP.Budget AS B ON B.Id=BM.BudgetId
	LEFT JOIN MST.MaterialMasterMachineProcess AS MP ON MP.MaterialMasterId=MM.Id
	WHERE MM.CompanyGroupId = '" + companyGroupId + @"' AND MM.Archive = 0 AND MM.Active = 1
	AND MM.Id IN(SELECT MaterialMasterId FROM MST.MaterialMasterBusinessProcess AS A 
                JOIN SCS.BusinessProcess AS B ON A.BusinessProcessId=B.Id
				WHERE B.BusinessProcessName='" + BusinessProcessEnum.MachineDefinition + @"')
	AND MP.ProcessId IN(" + ReturnStringArray(processIds) + @")
) SELECT DISTINCT *, COUNT(*) OVER () AS TotalRows FROM CTE WHERE RN>1

--Find a table from multi DB
SELECT name
FROM   sys.databases
WHERE  CASE
         WHEN state_desc = 'ONLINE' 
              THEN OBJECT_ID(QUOTENAME(name) + '.SCS.RptConfigTemplate', 'U')
       END IS NOT NULL 
-----------------