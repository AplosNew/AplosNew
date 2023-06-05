'use strict';
GLControlController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$routeParams', '$location', '$http', '$filter', '$controller'];
function GLControlController(cboService, commonMessage, $scope, $rootScope, baseService, $routeParams, $location, $http, $filter, $controller) {
    $rootScope.title = 'GL Control';
    $scope.Action = 'Save';
    $scope.ModelList = [];
    $scope.path = 'Accounts/GeneralAccountDeterminate/';
    $scope.getListUrl = $scope.path + 'getlist';
    $scope.getSeqUrl = $scope.path + 'getautosequence';
    $scope.saveUrl = $scope.path + 'CreateGlControl';
    $scope.deleteUrl = $scope.path + 'DeleteGlControl/';
    baseService.init($scope.getListUrl);
    $scope.searchBy = "UserName"; $scope.search = "";
    $scope.searchByList = [{ value: 'Id', name: "Id" }, { value: 'Code', name: "Code" }, { value: 'ShortName', name: "Short Name" }, { value: 'StandardName', name: "Standard Name" }, { value: 'UserName', name: "User Name" }];
    $controller('baseMaterialAndArticleController', { $scope: $scope, $http: $http });

    $scope.tab = 1;
    $scope.setTab = function (newTab) {
        $scope.tab = newTab;
    };
    $scope.isSet = function (tabNum) {
        return $scope.tab === tabNum;
    };

    $scope.product = {
        Id: null,
        CompanyGroupId: null,
        CountryId: null,
        CompanyId: null,
        PositionCode: null,
        PlantId: null,
        EntityId: null,
        ProcurementDays: null,
        ProcurementFrequency: null,
        MaterialType: null,
        QualityStdSet: null,
        CostReductionCategory: null,
        MaterialMasterId: null,
        ArticleId: null,
        ArticleCriticality: null,
        FirstCharacteristicsId: null,
        FirstCharacteristicsValueId: null,
        SecondCharacteristicsId: null,
        SecondCharacteristicsValueId: null,
        ThirdCharacteristicsId: null,
        ThirdCharacteristicsValueId: null,
        MinStockLevel: null,
        MaxStockLevel: null,
        CostingPercentage: null,
        ProcurementPercentage: null,
        QualityApprovalReq: null,
        QualityApprovedBy: null,
        PossitionCodeForApproval: null,
        QualityStdSet: null,
        SupplierQualityReportReq: null,
        RequisitionType: null,
        PriceApproval: null,
        POGroupId: null,
        Imported: null,
        ImportedCurrencyId: null,
        ImportedBaseRate: null,
        ImportedTgtLandedRate: null,
        ImportProcurementLedTimeDays: null,
        ImportedMinimumOrderQty: null,
        ImportedArticleLifeDays: null,
        Local: null,
        LocalCurrencyId: null,
        LocalBaseRate: null,
        LocalTgtLandedRate: null,
        LocalProcurementLedTimeDays: null,
        LocalMinimumOrderQty: null,
        LocalArticleLifeDays: null,
        AutoPoGeneration: null,
        POGenerationCriteria: null,
        PoGenerationDay: null,
        LastProcurementRate: null,
        MinimumProcurementRate: null,
        MaximumProcurementRate: null,
        MaterialMasterName: null,
        ArticleName: null,
        ProcurementsPlanDay: null,
        Remarks: null
    };
    $scope.productNew = Object.assign({}, $scope.product);


    $scope.getData = function () {
        $http({
            method: 'POST',
            url: $scope.path + "GetGlControlList",
            data: { column: $scope.searchBy, value: $scope.search },
            dataType: 'JSON'
        }).then(function successCallback(response) {          
            $scope.ModelList = response.data;
            ClearFields(response.data.Sequence);
            $scope.GetSequence();
        });
    }
    $scope.getData();

    $scope.ModelTemp = {
        Id: null,
        Sequence: 0,
        Code: null,
        ShortName: null,
        StandardName: null,
        UserName: null,
        Description: null,
        Remarks: null,
        Active: true
    };
    $scope.ModelNew = Object.assign({}, $scope.ModelTemp);

    $scope.ModelTempMat = {
        Id: null,
        UserName: null,
        StorageLocationId: null,
        StorageSubLocation: null,
        MaterialTypeId: null,
        MaterialGroupMasterId: null,
        MaterialMasterId: null,
        MaterialMasterArticleId: null,
        AccessType: null,
        NoOfBin: null,
        Remarks: null,
        StorageLevel: null,
    };
    $scope.ModelNewMat = Object.assign({}, $scope.ModelTempMat);


    $scope.GetSequence = function () {
        cboService.getSequence($scope.getSeqUrl, function (data) {
            $scope.ModelTemp.Sequence = data;
            $scope.ModelNew.Sequence = data;
        });
    };
    $scope.GetSequence();

    $scope.Get = function (args) {

        $scope.ModelNew = Object.assign({}, args.data);
        $scope.Action = 'Update';
        if (!$rootScope.isCollapsed) {
            $rootScope.toggle();
        }
    };

    $scope.Save = function () {
        $scope.$broadcast('show-errors-check-validity');
        if ($scope.ModelNewForm.$valid) {
            $http({
                method: 'POST',
                url: $scope.saveUrl,
                data: { 'data': $scope.ModelNew },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();

                }
            }), function errorCallBack(response) {
                ShowResult(response.data.Message, 'failure');
            }

        }
    };

    $scope.Delete = function () {
        if (!baseService.isUndefinedOrNull($scope.ModelNew.Id)) {
            $http({
                method: 'POST',
                url: $scope.deleteUrl + $scope.ModelNew.Id,
                dataType: 'JSON'
            }).then(function successCallback(response) {
                if (response.data.Error === true) {
                    ShowResult(response.data.Message, 'failure');
                }
                else {
                    ShowResult(response.data.Message, 'success');
                    ClearFields(response.data.Sequence);
                    $scope.getData();
                }
                function errorCallBack(response) {
                    ShowResult(response.data.Message, 'failure');
                }
            });
        }
    };

    $scope.Clear = function () {
        ClearFields($scope.GetSequence());
        return true;
    };

    function ClearFields(seq) {
        $scope.Action = 'Save';
        $scope.ModelNew = Object.assign({}, $scope.ModelTemp);
        $scope.ModelNew.Sequence = seq;
    }


    $scope.searchbyMaterialMasterDatalist = [
        {
            'name': 'Material Type',
            'value': 'MaterialTypeName'
        },
        {
            'name': 'Material Group',
            'value': 'MaterialGroupMasterName'
        },
        {
            'name': 'Code',
            'value': 'Code'
        },
        {
            'name': 'Product',
            'value': 'ProductMasterName'
        },
        {
            'name': 'User Name',
            'value': 'UserName'
        },
        {
            'name': 'Standard Name',
            'value': 'StandardName'
        },
        {
            'name': 'Short Name',
            'value': 'ShortName'
        },
        {
            'name': 'IsAsset',
            'value': 'Asset'
        },
        {
            'name': 'Asset Master',
            'value': 'AssetMasterName'
        },
        {
            'name': 'Budget Code',
            'value': 'AssetBudgetCode'
        },
        {
            'name': 'Activity',
            'value': 'ActivityName'
        },
        {
            'name': 'Id',
            'value': 'Id'
        }
    ];

    $scope.columnExcluedList = ['WithSKU', 'Description', 'Active', 'IsInventory', 'IsExpenseOut', 'IsAsset	', 'AssetMasterName', 'AssetType', 'IsRevenue'];
    $scope.popUpList = [];
    $scope.valueData = '';
    $scope.popUpParameters = {
        limit: 10
        , offset: 0
        , order: 'asc'
        , sort: 'UserName'
        , searchBy: "UserName"
        , pageSize: 10
        , total_count: 0
        , search: null
        , serverPagination: true
    };
    $scope.popUp = function () {
        $scope.popUpUrl = $scope.path +'GetMaterialList';
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                    for (var i = 0; i < $scope.popUpDataList.length; i++) {
                        $scope.ModelNewMat.MaterialTypeId = $scope.popUpDataList[i].MaterialTypeId;
                        $scope.ModelNewMat.MaterialGroupMasterId = $scope.popUpDataList[i].MaterialGroupMasterId; 
                        $scope.ModelNewMat.MaterialMasterId = $scope.popUpDataList[i].Id;
                    }
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();
    };

    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };

    $scope.selectDoubleClick = function (data) {
        $scope.model = data;
        $scope.MaterialHSNCodeId = data.HSNCodeId;
        //getAttribute();
        //getArticle();
        $scope.selectIDs(data);
        $scope.closePopUp();
    };

    $scope.selectByButton = function () {
        if (baseService.isUndefinedOrNull($scope.valueData))
            return ShowResult('Please at first select row', 'failure', 'popUpId');
        $scope.selectDoubleClick($scope.valueData);
        $scope.closePopUp();
    };

    // #region ---------------------------------      MATERIAL ALLOCACTION GRID      -----------------------------------//

    $scope.BinHeadList = [];
    $scope.selectIDs = function (data) {
            $http({
                method: 'POST',
                url: $scope.path + "selectIDs",
                data: {
                    'materialType': data.MaterialTypeId,
                    'materialGroup': data.MaterialGroupMasterId,
                    'materialMasterId': data.Id
                },
                dataType: 'JSON'
            }).then(function successCallback(response) {
                $scope.BinHeadList = response.data;
                //$scope.selectBinIDs();
            })
    }
    // #endregion ---------------------------------      MATERIAL ALLOCACTION GRID      -----------------------------------//
  
}