'use strict';
fixedAssetAUCCapitalizeGRNBassController.$inject = ['cboService', 'commonMessage', '$scope', '$rootScope', 'baseService', '$http'];
function fixedAssetAUCCapitalizeGRNBassController(cboService, commonMessage, $scope, $rootScope, baseService, $http) {
    $rootScope.title = "FixedAsset Capitalize";
    $scope.Action = 'Save';
    $scope.index = -1;
    $scope.dataList = [];
    $scope.path = 'FixedAssets/FixedAssetRegister/';
    $scope.getListUrl = $scope.path + 'GetFixedAssetCapitalizeJournalData';
    $scope.saveUrl = $scope.path + 'InsertGRNFixedAssetCapitalizeJournal';

    baseService.init($scope.getListUrl, null, null, 'DESC', 'Id', 'Id');
    $scope.getData = function (pageno) {
        baseService.pagination(pageno)
            .then(function (result) {
                $scope.dataList = result.Rows;
            }, function () {
                ShowResult(commonMessage.NetworkError, 'failure');
            }).finally(function () {
            });
    };
    $scope.getData();

    cboService.getCboVoucherTypeFixedAssetCapitalizeJournalList(function (result) {
        $scope.voucherTypeList = result;
        if (baseService.arrayLength($scope.voucherTypeList) === 1)
            $scope.modelNew.VoucherTypeId = $scope.voucherTypeList[0].Value;
    });

    $scope.model = {
        Id: null
        , MaterialStorage: null
        , GRNDate: null
        , VoucherTypeId: null
        , EmployeeId:null
        , EmployeeCode:null
        , PartyCode:null
        , PartyName:null
        , InvoicingBy:null
        , InvoicingByAddress:null
        , GateEntryNo:null
        , PaymentTermName:null
        , BaseOnDueDate:null
        , EmployeeName:null
        , PartyAccountGroupName:null
        , DeliveryBy:null
        , DeliveryByAddress:null
        , EntryDate:null
        , CurrencyCode:null
        , MatureDate:null
        , IsNonCreditable: null
        , ToCurrencyRate:0
    };
    $scope.modelNew = Object.assign({}, $scope.model);

    $scope.popUpList = [];
    $scope.valueData = '';
    $scope.searchByList = [
        {
            value: 'Id'
            , name: 'Id No'
        },
        {
            value: 'Voucher No'
            , name: 'VoucherNo'
        },
        {
            value: 'Voucher Date'
            , name: 'VoucherDate'
        }
    ];

    $scope.columnExcluedList = [];
    $scope.popUp = function () {
        $scope.popUpParameters = {
            limit: 10,
            offset: 0,
            order: 'DESC',
            sort: 'Id',
            searchBy: "Id",
            pageSize: 10,
            total_count: 0,
            search: null,
            serverPagination: true
        };
        $scope.popUpUrl = 'FixedAssets/FixedAssetRegister/GetGRNFixedAssetList';
        $scope.popUpTitle = 'Inventory Fixed Asset GRN Data';
        $scope.getPopUpData = function (pageno) {
            baseService.paginationBase($scope.popUpUrl, pageno, $scope.popUpParameters)
                .then(function (result) {
                    $scope.popUpDataList = result.Rows;
                    $scope.popUpParameters.total_count = result.Total;
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure', 'popUpId');
                }).finally(function () {
                });
        };
        angular.element(document.querySelector('#popUpId')).modal('show');
        $scope.getPopUpData();
    };


    $scope.popUpDataList = [];
    $scope.popUp = function () {
        $http({
            method: 'GET',
            url: 'FixedAssets/FixedAssetRegister/GetGRNFixedAssetList'
        }).then(function successCallback(response) {
            $scope.popUpDataList = response.data;
            angular.element(document.querySelector('#popUpId')).modal('show');
        });
    }

    $scope.selectDoubleClick = function () {
        var gridObj = $("#popUpData").data("ejGrid");
        var data = gridObj.getSelectedRecords()[0];
        var voucherTypeId = $scope.modelNew.VoucherTypeId;
        $scope.modelNew = data;
        $scope.modelNew.PostingDate = new Date();
        $scope.modelNew.GRNDate = data.GRNDate;
        $scope.modelNew.VoucherTypeId = voucherTypeId;
        getInventoryMaterialList();
        $scope.closePopUp();
    };

    $scope.closePopUp = function () {
        $scope.valueData = '';
        angular.element(document.querySelector('#popUpId')).modal('hide');
    };

    function getInventoryMaterialList() {
        $http.get('FixedAssets/FixedAssetRegister/GetGRNCapitalizeFixedAssetGL?issueId=' + $scope.modelNew.Id)
            .then(function (response) {
                $scope.inventoryMaterialList = [];
                //for (var i = 0; i < baseService.arrayLength(response.data); i++) {
                //    response.data[i].budgetList = [];
                //    response.data[i].activityList = [];
                //    response.data[i].budgetList.push({ BudgetMasterId: response.data[i].BudgetMasterId, BudgetName: response.data[i].BudgetName });
                //    response.data[i].activityList.push({ ActivityId: response.data[i].ActivityId, ActivityName: response.data[i].ActivityName });
                //}
                $scope.inventoryMaterialList = response.data;
            });
    }

    $scope.Post = function () {
        $http({
            method: 'POST',
            url: $scope.saveUrl,
            data: {
                issueId: $scope.modelNew.Id
                , voucherTypeId: $scope.modelNew.VoucherTypeId
                , ToCurrencyRate: $scope.modelNew.ToCurrencyRate
                , voucherDetailVMList: $scope.inventoryMaterialList
            },
            dataType: 'JSON'
        }).then(function (response) {
            if (response.data.Error === true)
                ShowResult(response.data.Message, 'failure');
            else {
                ShowResult(response.data.Message, 'success');
                $scope.Clear();
                $scope.getData();
            }
        }), function (response) {
            ShowResult(response.data.Message, 'failure');
        };
    };
    $scope.Clear = function () {
        $scope.model = {};
        $scope.modelNew = { PostingDate: new Date() };
        $scope.inventoryMaterialList = [];
        if (baseService.arrayLength($scope.voucherTypeList) === 1)
            $scope.modelNew.VoucherTypeId = $scope.voucherTypeList[0].Value;
    };

    //#region GL, Budget & Activity

    baseService.getCompanyConfiguration(function (result) {
        $scope.companyConfig = result;
    });
    $scope.searchGLByList = [

        {
            'name': 'Account Group',
            'value': 'AccountGroupName'
        },
        {
            'name': 'GL',
            'value': 'GLGeneralInfoName'
        }
    ];
    $scope.glListParameters = {
        limit: 10,
        offset: 0,
        order: 'asc',
        sort: 'GLGeneralInfoCode',
        searchBy: "GLGeneralInfoName",
        pageSize: 10,
        total_count: 0,
        search: null,
        serverPagination: true
    };
    $scope.getGLPopUP = function (index) {
        $scope.index = index;
        $scope.getGLData = function (pageno) {
            baseService.paginationBase('accounts/glitem/GetExpenseGLCOAWise?coaId=' + $scope.companyConfig.COAId, pageno, $scope.glListParameters)
                .then(function (data) {
                    $scope.glList = data.Rows;
                    $scope.glListParameters.total_count = data.Total;
                    angular.element(document.querySelector('#gltListPopUp')).modal('show');
                }, function () {
                    ShowResult(commonMessage.NetworkError, 'failure');
                }).finally(function () {
                });
        };
        $scope.getGLData();
    };

    $scope.setGL = function (data) {
        $scope.inventoryMaterialList[$scope.index].GLGeneralInfoId = data.GLGeneralInfoId;
        $scope.inventoryMaterialList[$scope.index].GLName = data.GLGeneralInfoCode + '-' + data.GLGeneralInfoName;
        getBudgetList($scope.index);
        $scope.closeGltListPopUp();
    };

    $scope.refreshGL = function (index) {
        $scope.inventoryMaterialList[index].GLGeneralInfoId = null;
        $scope.inventoryMaterialList[index].GLName = null;
        $scope.inventoryMaterialList[index].BudgetMasterId = null;
        $scope.inventoryMaterialList[index].ActivityId = null;
        $scope.inventoryMaterialList[index].budgetList = null;
        $scope.inventoryMaterialList[index].activityList = null;
    };

    $scope.downPaymentBudgetList = [];
    function getBudgetList(index) {
        cboService.getBudgetMasterCboByCOAAndGLId($scope.companyConfig.COAId, $scope.inventoryMaterialList[index].GLGeneralInfoId, function (result) {
            $scope.inventoryMaterialList[index].BudgetMasterId = null;
            $scope.inventoryMaterialList[index].budgetList = [];
            $scope.inventoryMaterialList[index].budgetList = result;
        });
    }

    $scope.activityList = [];
    $scope.getActivity = function (index) {
        cboService.getBudgetMasterActivityCbo($scope.inventoryMaterialList[index].BudgetMasterId, function (result) {
            $scope.inventoryMaterialList[index].ActivityId = null;
            $scope.inventoryMaterialList[index].activityList = [];
            $scope.inventoryMaterialList[index].activityList = result;
        });
    };

    $scope.closeGltListPopUp = function () {
        $scope.index = -1;
        angular.element(document.querySelector('#gltListPopUp')).modal('hide');
    };
} 