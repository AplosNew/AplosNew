namespace Library.Model.Enums
{
    public class DbTable
    {
        #region -- Process --

        public const string Process = "Process";
        public const string CompanyProcess = "CompanyProcess";
        public const string SubProcess = "SubProcess";
        public const string CompanySubProcess = "CompanySubProcess";
        public const string ProcessConfig = "ProcessConfig";
        public const string ProcessCategory = "ProcessCategory";
        public const string CompanyGroupProcessCategory = "CompanyGroupProcessCategory";
        public const string ProcessSubCategory = "ProcessSubCategory";
        public const string ProcessType = "ProcessType";
        public const string EntityProcessTag = "EntityProcessTag";
        public const string Utility = "Utility";
        public const string ProcessCriteria = "ProcessCriteria";
        public const string CompanyGroupProcessCriteria = "CompanyGroupProcessCriteria";
        public const string ProcessSet = "ProcessSet";
        public const string SubProcessSet = "SubProcessSet";
        public const string ProcessSetDetail = "ProcessSetDetail";
        public const string SubProcessSetDetail = "SubProcessSetDetail";

        #endregion -- Process --

        #region -- Product --

        public const string Item = "Item";
        public const string ItemCategory = "ItemCategory";
        public const string ItemSubCategory = "ItemSubCategory";
        public const string Product = "Product";
        public const string ProductCategory = "ProductCategory";
        public const string ProductSubCategory = "ProductSubCategory";
        public const string CompanyGroupWiseProduct = "CompanyGroupWiseProduct";
        public const string CompanyGroupWiseProductCategory = "CompanyGroupWiseProductCategory";
        public const string CompanyGroupWiseProductSubCategory = "CompanyGroupWiseProductSubCategory";
        public const string ProductSubCategoryAttribute = "ProductSubCategoryAttribute";
        public const string ProductMaster = "ProductMaster";
        public const string ProductMasterAttributeValue = "ProductMasterAttributeValue";
        public const string ProductGroup = "ProductGroup";

        #endregion -- Product --

        #region -- Material --

        public const string Characteristics = "Characteristics";
        public const string CharacteristicsValue = "CharacteristicsValue";
        public const string CharacteristicsWisePropertiesUOMFactor = "CharacteristicsWisePropertiesUOMFactor";
        public const string CharacteristicsWisePropertiesMaster = "CharacteristicsWisePropertiesMaster";
        public const string CharacteristicsWisePropertiesDetail = "CharacteristicsWisePropertiesDetail";
        public const string CharacteristicsWisePropertiesUOM = "CharacteristicsWisePropertiesUOM";
        public const string CompanyGroupCharacteristics = "CompanyGroupCharacteristics";
        public const string MaterialGrid = "MaterialGrid";
        public const string MaterialGridCharacteristics = "MaterialGridCharacteristics";
        public const string MaterialType = "MaterialType";

        public const string CompanyGroupWiseMaterialGroup1 = "CompanyGroupWiseMaterialGroup1";
        public const string MaterialGroup1 = "MaterialGroup1";
        public const string CompanyGroupWiseMaterialGroup2 = "CompanyGroupWiseMaterialGroup2";
        public const string MaterialGroup2 = "MaterialGroup2";
        public const string CompanyGroupWiseMaterialGroup3 = "CompanyGroupWiseMaterialGroup3";
        public const string MaterialGroup3 = "MaterialGroup3";
        public const string CompanyGroupWiseMaterialGroup4 = "CompanyGroupWiseMaterialGroup4";
        public const string MaterialGroup4 = "MaterialGroup4";
        public const string CompanyGroupWiseMaterialGroupMaster = "CompanyGroupWiseMaterialGroupMaster";
        public const string MaterialGroupMaster = "MaterialGroupMaster";
        public const string MaterialMasterProcessRouting = "MaterialMasterProcessRouting";

        public const string MaterialAttribute = "MaterialAttribute";
        public const string MaterialAttributeMaster = "MaterialAttributeMaster";
        public const string MaterialAttributeMasterDetail = "MaterialAttributeMasterDetail";
        public const string CompanyGroupMaterialAttribute = "CompanyGroupMaterialAttribute";
        public const string MaterialAttributeValue = "MaterialAttributeValue";
        public const string MaterialMasterAttributeValue = "MaterialMasterAttributeValue";
        public const string DefectCode = "DefectCode";
        public const string DefectCodeDetail = "DefectCodeDetail";

        public const string OurStyle = "OurStyle";
        public const string MaterialMaster = "MaterialMaster";
        public const string MaterialMasterAlternativeUOM = "MaterialMasterAlternativeUOM";
        public const string FGZone = "FGZone";
        public const string FGComponent = "FGComponent";

        #endregion -- Material --

        #region -- IE

        public const string OperationTimeCaptureMaster = "OperationTimeCaptureMaster";
        public const string OperationTimeCaptureDetail = "OperationTimeCaptureDetail";
        public const string OperationVideoUpload = "OperationVideoUpload";
        public const string BulletinMaster = "BulletinMaster";
        public const string BulletinDetail = "BulletinDetail";
        public const string SubsectionStructureMaster = "SubsectionStructureMaster";
        public const string SubsectionStructureDetail = "SubsectionStructureDetail";

        #endregion -- IE

        #region --- Productions

        public const string ProductionOrderDetail = "ProductionOrderDetail";
        public const string ProductionOrder = "ProductionOrder";
        public const string ProductionOrderWorkCenter = "ProductionOrderWorkCenter";
        public const string ProductionOrderSubprocessSet = "ProductionOrderSubprocessSet";
        public const string ProductionOrderProcessCriteria = "ProductionOrderProcessCriteria";
        public const string CustomerPO = "CustomerPO";
        public const string SalesOrderMaster = "SalesOrderMaster";
        public const string SalesOrderMaterialMaster = "SalesOrderMaterialMaster";
        public const string SalesOrderCharacteristicsValue1st = "SalesOrderCharacteristicsValue1st";
        public const string SalesOrderCharacteristicsValue2nd = "SalesOrderCharacteristicsValue2nd";
        public const string SalesOrderCharacteristicsValueSummary = "SalesOrderCharacteristicsValueSummary";
        public const string RecipeRawMaterial = "RecipeRawMaterial";
        public const string RecipeSubprocess = "RecipeSubprocess";
        public const string RecipeMaster = "RecipeMaster";

        public const string ProductionSettings = "ProductionSettings";
        public const string ProcessCapacityUOM = "ProcessCapacityUOM";
        public const string ProductionStatus = "ProductionStatus";
        public const string CompanyGroupProductionStatus = "CompanyGroupProductionStatus";
        public const string DMM = "DMM";
        public const string CompanyGroupDMM = "CompanyGroupDMM";

        #endregion --- Productions

        #region -- Machine--

        public const string Machine = "Machine";
        public const string MachineClass = "MachineClass";
        public const string CompanyGroupMachineClass = "CompanyGroupMachineClass";
        public const string MachineSubClass = "MachineSubClass";
        public const string MachineCategory = "MachineCategory";
        public const string MachineSubCategory = "MachineSubCategory";
        public const string OperationType = "OperationType";
        public const string Operation = "Operation";
        public const string CompanyGroupOperationType = "CompanyGroupOperationType";
        public const string OperationElement = "OperationElement";
        public const string CompanyGroupOperationElement = "CompanyGroupOperationElement";
        public const string MachineMaster = "MachineMaster";
        public const string ThirdPartyOperation = "ThirdPartyOperation";
        public const string CompanyGroupThirdPartyOperation = "CompanyGroupThirdPartyOperation";
        public const string OperationCategory = "OperationCategory";
        public const string CompanyGroupOperationCategory = "CompanyGroupOperationCategory";
        public const string OperationCategorySalary = "OperationCategorySalary";
        public const string OperationActivity = "OperationActivity";
        public const string CompanyGroupOperationActivity = "CompanyGroupOperationActivity";

        #endregion -- Machine--
    }
}